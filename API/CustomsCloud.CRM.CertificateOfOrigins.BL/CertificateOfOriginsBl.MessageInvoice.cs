using CustomsCloud.CRM.CertificateOfOrigins.Model.CertificateOfOriginsDb;
using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL;

// GetPC_MSG2280_2281 message field-validation engine (create branch) — stage 4b: the invoice / item pre-check and
// conversion (legacy ValidateCertificateOfOriginRequestInvoiceDetail + CheckAndConvertInvoiceDetails +
// ConvertMessageInvoiceDetailToInvoiceDetail + ConvertMessageItemDetailToItemDetail). Validates the invoice/item shape,
// resolves the currency / packing / measurement / customs-item / origin-criterion codes via SystemTables proxies, and
// builds the invoice + item entity rows the save persists.
public partial class CertificateOfOriginsBl
{
    // Legacy ValidateCertificateOfOriginRequestInvoiceDetail: at least one invoice, each with at least one item, every
    // item carrying MarksAndNumbers. The legacy short-circuits (throws) on the structural failures; here they are
    // accumulated in-band like the rest of the engine (developer decision — consistent in-band error contract).
    private static void ValidateInvoiceShape(CertificateOfOriginMessageDto certificate, MessageValidationContext context)
    {
        if (certificate.InvoiceDetails.Count == 0)
        {
            context.Exceptions.Add(BuildMessageException(EMessageCode.MandatoryValue, nameof(certificate.InvoiceDetails)));
            return;
        }

        foreach (var invoice in certificate.InvoiceDetails)
        {
            if (invoice.ItemDetails.Count == 0)
            {
                context.Exceptions.Add(BuildMessageException(EMessageCode.MandatoryValue, "ItemDetails"));
            }
            else if (invoice.ItemDetails.Exists(item => string.IsNullOrEmpty(item.MarksAndNumbers)))
            {
                context.Exceptions.Add(BuildMessageException(EMessageCode.MandatoryValue, "MarksAndNumbers"));
            }
        }
    }

    // Legacy CheckAndConvertInvoiceDetails: NonManipulation has no invoices; otherwise convert every message invoice to
    // its entity, validating + resolving each along the way.
    private async Task<List<CertificateOfOriginInvoiceDetail>> ConvertInvoiceDetails(CertificateOfOriginRequestMessageDto request, MessageValidationContext context)
    {
        var certificate = request.CertificateOfOrigin;
        var agentRequest = request.AgentRequest;
        var result = new List<CertificateOfOriginInvoiceDetail>();

        if (agentRequest.CertificateOfOriginTypeCode == (int)ECertificateOfOriginType.NonManipulation || certificate is null)
        {
            return result;
        }

        var isCustomsItemMandatory = agentRequest.IsCustomsItemMandatory;
        var isCriterionMandatory = agentRequest.IsCertificateTypeCodeMandatory;
        var isGetStatus = agentRequest.RequestReasonCode == (int)ERequestReason.GetRequestStatus;

        foreach (var messageInvoice in certificate.InvoiceDetails)
        {
            var invoice = await ConvertMessageInvoiceToEntity(messageInvoice, agentRequest.CertificateOfOriginTypeCode, isCriterionMandatory, isCustomsItemMandatory, isGetStatus, context);
            result.Add(invoice);
        }

        return result;
    }

    private async Task<CertificateOfOriginInvoiceDetail> ConvertMessageInvoiceToEntity(CertificateOfOriginMessageInvoiceDetailDto messageInvoice, int certificateTypeId, bool isCriterionMandatory, bool? isCustomsItemMandatory, bool isGetStatus, MessageValidationContext context)
    {
        var items = new List<CertificateOfOriginItemDetail>();
        foreach (var messageItem in messageInvoice.ItemDetails)
        {
            var item = await ConvertMessageItemToEntity(messageItem, certificateTypeId, isCriterionMandatory, isCustomsItemMandatory, context);
            if (!isGetStatus)
            {
                CheckContainerIsoCode(item, context);
            }

            items.Add(item);
        }

        // Legacy: an invoice date older than 5 years is floor-clamped (silently corrected, not rejected).
        var invoiceDate = messageInvoice.InvoiceDate < DateTime.Today.AddYears(-5) ? DateTime.Today.AddYears(-5) : messageInvoice.InvoiceDate;

        return new CertificateOfOriginInvoiceDetail
        {
            InvoiceDate = invoiceDate,
            InvoiceNumber = messageInvoice.InvoiceNum,
            InvoiceAmount = messageInvoice.InvoiceSum ?? 0,
            IsToPrint = messageInvoice.IsInvoicesForPrint,
            CurrencyTypeId = await ResolveCurrencyTypeId(messageInvoice.CurrencyType, context),
            InvoiceGoodsDescription = CheckDescriptionLength(messageInvoice.DescriptionOfInvoice, context) ?? string.Empty,
            CertificateOfOriginItemDetail = items,
        };
    }

    private async Task<CertificateOfOriginItemDetail> ConvertMessageItemToEntity(CertificateOfOriginMessageItemDetailDto messageItem, int certificateTypeId, bool isCriterionMandatory, bool? isCustomsItemMandatory, MessageValidationContext context)
    {
        return new CertificateOfOriginItemDetail
        {
            GrossWeight = messageItem.Weight,
            ItemGoodsDescription = messageItem.ItemDescription ?? string.Empty,
            FullClassification = messageItem.ItemId ?? string.Empty,
            MarksAndNumbers = messageItem.MarksAndNumbers ?? string.Empty,
            Quantity = messageItem.PackageQuantity ?? 0,
            PackingTypeId = await ResolvePackingTypeId(messageItem.PackageType, context),
            MeasurementUnitId = await ResolveMeasurementUnitId(messageItem.MeasureType, context) ?? 0,
            RowNum = messageItem.ItemSerial ?? 0,
            CustomsItemId = await CheckAndGetCustomsItem(messageItem.ItemId, isCustomsItemMandatory, context),
            OriginCriterionId = await CheckAndGetOriginCriterion(isCriterionMandatory, messageItem.OriginCriterion, certificateTypeId, context),
            ContainerIsoCode = messageItem.ContainerIsoCode,
        };
    }

    // Legacy CheckContainerISOCode: a container packing type requires the ISO code.
    private static void CheckContainerIsoCode(CertificateOfOriginItemDetail item, MessageValidationContext context)
    {
        if (item.PackingTypeId == CertificateOfOriginsConsts.PackingTypeContainer && item.ContainerIsoCode == null)
        {
            context.Exceptions.Add(BuildMessageException(EMessageCode.RequiredContainerIsoCodeFieldIfItIsAContainer));
        }
    }

    // Legacy CheckValidityField: the invoice goods-description is capped at 255 chars.
    private static string? CheckDescriptionLength(string? description, MessageValidationContext context)
    {
        if (description is not null && description.Length > CertificateOfOriginsConsts.InvoiceDescriptionMaxLength)
        {
            context.Exceptions.Add(BuildMessageException(EMessageCode.TheDescriptionLengthCannotExceed255Characters));
        }

        return description;
    }

    // Legacy CheckIfCustomsItemAndGetItem: resolve the full classification to a customs-item id; a present classification
    // must exist and be at least 6 digits, an absent one is an error only when customs items are mandatory for the type.
    private async Task<int?> CheckAndGetCustomsItem(string? fullClassification, bool? isCustomsItemMandatory, MessageValidationContext context)
    {
        if (string.IsNullOrEmpty(fullClassification))
        {
            if (isCustomsItemMandatory == true)
            {
                context.Exceptions.Add(BuildMessageException(EMessageCode.CustomsItemRequired));
            }

            return null;
        }

        var customsItemId = await customsBookProxy.GetCustomsItemIdByFullClassification(fullClassification);
        if (!customsItemId.HasValue)
        {
            context.Exceptions.Add(BuildMessageException(EMessageCode.ItemNumberNotFound));
        }

        if (fullClassification.Length < 6)
        {
            context.Exceptions.Add(BuildMessageException(EMessageCode.ACustomsItemMustContainAtLeast6Digits));
        }

        return customsItemId;
    }

    // Legacy CheckOriginCriterionAndGetId: only checked when the certificate type mandates a criterion; the code must
    // resolve within the certificate type.
    private async Task<int?> CheckAndGetOriginCriterion(bool isCriterionMandatory, string? originCriterionCode, int certificateTypeId, MessageValidationContext context)
    {
        if (!isCriterionMandatory)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(originCriterionCode))
        {
            context.Exceptions.Add(BuildMessageException(EMessageCode.OriginCriteriaMissing));
            return null;
        }

        var originCriterion = await DataLayer.GetOriginCriterion(originCriterionCode, certificateTypeId);
        if (originCriterion is null)
        {
            context.Exceptions.Add(BuildMessageException(EMessageCode.OriginCriteriaIllegal));
            return null;
        }

        return originCriterion.Id;
    }

    // Legacy GetIdByCode<CurrencyType>(PropCurrencyCode, code): an unresolvable code accumulates a validation exception
    // (the legacy GetIdByCode throws → the caller adds it to _requestExceptions), not a silent null.
    private async Task<int?> ResolveCurrencyTypeId(string? currencyCode, MessageValidationContext context)
    {
        if (string.IsNullOrEmpty(currencyCode))
        {
            return null;
        }

        var currencies = await currencyTypeProxy.GetCurrencyTypesByCodes([currencyCode]);
        var id = currencies?.FirstOrDefault()?.Id;
        if (id is null)
        {
            context.Exceptions.Add(BuildMessageException(EMessageCode.TheValueInFieldNotExistsInSystem, "CurrencyType"));
        }

        return id;
    }

    // Legacy GetIdByCode<PackingType>(PropCommonCode, code): unresolvable code → validation exception.
    private async Task<int?> ResolvePackingTypeId(string? code, MessageValidationContext context)
    {
        if (string.IsNullOrEmpty(code))
        {
            return null;
        }

        var packingTypes = await packingTypeProxy.GetPackingTypesByCodes([code]);
        var id = packingTypes?.FirstOrDefault()?.Id;
        if (id is null)
        {
            context.Exceptions.Add(BuildMessageException(EMessageCode.TheValueInFieldNotExistsInSystem, "PackageType"));
        }

        return id;
    }

    // Legacy GetIdByCode<MeasurementUnit>(PropExternalIDNum, code): unresolvable code → validation exception.
    private async Task<int?> ResolveMeasurementUnitId(string? code, MessageValidationContext context)
    {
        if (string.IsNullOrEmpty(code))
        {
            return null;
        }

        var units = await measurementUnitProxy.GetMeasurementUnitsByCodes([code]);
        var id = units?.FirstOrDefault()?.Id;
        if (id is null)
        {
            context.Exceptions.Add(BuildMessageException(EMessageCode.TheValueInFieldNotExistsInSystem, "MeasureType"));
        }

        return id;
    }
}
