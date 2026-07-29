using CustomsCloud.CRM.CertificateOfOrigins.BL.Proxies;
using CustomsCloud.CRM.CertificateOfOrigins.DAL;
using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore.BL;
using CustomsCloud.InfrastructureCore.BL.Exceptions;
using CustomsCloud.InfrastructureCore.Parameters;
using Dapper;
using System.Data;
using System.Globalization;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL;

public class CertificateOfOriginsBl(IServiceProvider serviceProvider, ICustomerProxy customerProxy, IExportDealFileProxy exportDealFileProxy, IUserProxy userProxy, IDataDictionaryFieldProxy dataDictionaryFieldProxy, ICurrencyTypeProxy currencyTypeProxy, IParametersUtil parametersUtil)
    : BaseBL<CertificateOfOriginsBl, ICertificateOfOriginsDal>(serviceProvider)
{
    public async Task<CertificateOfOriginDto> GetCertificateOfOriginById(int certificateOfOriginId)
    {
        // Single certificate with its full graph (7 result sets). Missing id → 404 (the legacy returned null,
        // which callers treated as not-found). Milestone user display-names are enriched here — the SP returns only
        // the acting user id (the cross-service Infrastructure.UserMng_User JOIN was removed).
        var certificate = await DataLayer.GetCertificateOfOriginById(certificateOfOriginId)
            ?? throw new RestNotFoundException();
        await FillMilestoneUserNames(certificate);
        return certificate;
    }

    private async Task FillMilestoneUserNames(CertificateOfOriginDto certificate)
    {
        var userIds = certificate.Milestones
            .Where(m => m.UserId.HasValue)
            .Select(m => m.UserId!.Value)
            .Distinct()
            .ToList();
        if (userIds.Count == 0)
        {
            return;
        }

        var users = await userProxy.GetUsersByIds(userIds);
        if (users == null)
        {
            return;
        }

        var usersById = users.ToDictionary(u => u.Id);
        foreach (var milestone in certificate.Milestones)
        {
            if (milestone.UserId.HasValue && usersById.TryGetValue(milestone.UserId.Value, out var user))
            {
                milestone.UserName = user.Name;
            }
        }
    }

    #region GetCertificateRequestByGuid (Incoming / public-portal web query)

    // Legacy field labels came from SystemTablesUtil.GetCodeById<DataDictionaryField>(fieldId), where fieldId was
    // read via reflection off the entity's [FieldID] attributes. The target DTO carries no such attributes, so the
    // verified attribute values are used as constants (source of truth: the EF4 entity — 2026-07-28).
    private const int CertificateIdToCancelFieldId = 20306;     // [FieldID] on CertificateIDToCancel
    private const int RequestReasonCodeFieldId = 20310;         // [FieldID] on RequestReasonCode
    private const int ExportDeclarationNumberFieldId = 20661;   // [FieldID] on ExportDeclarationNumber

    // Legacy CertificateOfOriginsConsts (source of truth — not invented).
    private const string IssuingDateLabel = "Issuing Date";
    private const string InvalidGuid = "Invalid Guid";
    private const string NoMatchingCertificate = "No Matching Certificate";

    // Incoming/portal WCF: GetCertificateRequestByGuid (GetPC_Web_9096_CertificateRequest) → CertificateOfOriginWebBL
    // .GetCertificateDetailForWeb. Certificate verification for the public portal, located by guid or by
    // number + issuing-date. The legacy in-band error contract is preserved: an invalid guid or no matching
    // certificate is returned as a populated ExceptionDescription (HTTP 200), not thrown as a 404 — the external
    // portal is unaffected.
    public async Task<CertificateOfOriginsResponseDto> GetCertificateRequestByGuid(CertificateOfOriginsRequestDto request)
    {
        if (request.CertificateOfOriginGuid != null && !Guid.TryParse(request.CertificateOfOriginGuid, out _))
        {
            return new CertificateOfOriginsResponseDto { ExceptionDescription = InvalidGuid };
        }

        var parameters = new DynamicParameters();
        parameters.Add("@Guid", request.CertificateOfOriginGuid != null ? Guid.Parse(request.CertificateOfOriginGuid) : (Guid?)null, DbType.Guid);
        parameters.Add("@CertificateOfOriginNumber", request.CertificateOfOriginNumber, DbType.String);
        parameters.Add("@IssuingDate", request.IssuingDate?.Date, DbType.Date);

        var certificate = await DataLayer.GetCertificateOfOriginDataForWebQuery(parameters);
        if (certificate == null)
        {
            return new CertificateOfOriginsResponseDto { ExceptionDescription = NoMatchingCertificate };
        }

        var response = await ConstructWebResponse(certificate);
        return response;
    }

    private async Task<CertificateOfOriginsResponseDto> ConstructWebResponse(CertificateOfOriginWebQueryDto certificate)
    {
        var response = new CertificateOfOriginsResponseDto
        {
            // TODO(blocking): DocumentID was resolved in the SP from Infrastructure.Docs_* (Documents service,
            // cross-schema, not owned by this module) — the SP now returns NULL. Resolve via the Documents service.
            DocumentId = certificate.DocumentId ?? 0,
            CertificateNumber = certificate.CertificateNumber,
            QueryUrl = await GetQueryUrl(certificate.Guid),
            CertificateOfOriginDetails = await GetCertificateOfOriginDetails(certificate),
            CertificateOfOriginInvoiceDetails = await GetCertificateOfOriginInvoiceDetails(certificate),
        };
        return response;
    }

    private async Task<List<CertificateOfOriginWebInvoiceDetailDto>> GetCertificateOfOriginInvoiceDetails(CertificateOfOriginWebQueryDto certificateOfOrigin)
    {
        // Keep only the invoices that pass the (legacy) print filter, then resolve their currency codes in one batch.
        var printableInvoices = certificateOfOrigin.CertificateOfOriginInvoiceDetail
            .Where(invoice => IsInvoiceIncluded(certificateOfOrigin, invoice))
            .ToList();

        var currencyCodesById = await GetCurrencyCodes(
            printableInvoices.Where(i => i.CurrencyTypeId.HasValue).Select(i => i.CurrencyTypeId!.Value).ToList());

        var invoiceDetails = new List<CertificateOfOriginWebInvoiceDetailDto>();
        foreach (var invoice in printableInvoices)
        {
            var invoiceDetail = new CertificateOfOriginWebInvoiceDetailDto
            {
                InvoiceNumber = invoice.IsToPrint ? invoice.InvoiceNumber : string.Empty,
                InvoiceAmount = invoice.InvoiceAmount,
                InvoiceDate = invoice.InvoiceDate,
                InvoiceGoodsDescription = invoice.InvoiceGoodsDescription,
            };

            // Legacy: SystemTablesUtil.GetCodeById<CurrencyType>(id).CurrencyCode — resolved via ICurrencyTypeProxy
            // against the SystemTables microservice (no ILookupUtil type exists for CurrencyType).
            if (invoice.CurrencyTypeId.HasValue)
            {
                invoiceDetail.CurrencyCode = currencyCodesById.GetValueOrDefault(invoice.CurrencyTypeId.Value);
            }

            // LEGACY quirk (preserved, developer-confirmed 2026-07-28): the item-detail list was always
            // initialized and never populated — it is always empty.
            invoiceDetail.CertificateOfOriginItemDetails = [];

            invoiceDetails.Add(invoiceDetail);
        }

        return invoiceDetails;
    }

    private static bool IsInvoiceIncluded(CertificateOfOriginWebQueryDto certificateOfOrigin, CertificateOfOriginInvoiceDetailDto invoice)
    {
        // LEGACY operator-precedence quirk, preserved bug-for-bug (developer-confirmed 2026-07-28): because the
        // AND binds tighter than the OR in the original, the IsToPrint flag gates ONLY the MERCOSUR branch;
        // IsrCol, EURMED and EUR1 are always included regardless of IsToPrint. The inner parentheses below make
        // that original precedence explicit WITHOUT changing behavior (required by analyzer rule SA1408).
        return (certificateOfOrigin.TypeId == (int)ECertificateOfOriginType.IsrCol ||
                (certificateOfOrigin.TypeId == (int)ECertificateOfOriginType.MERCOSUR &&
                 invoice.IsToPrint)) ||
               (certificateOfOrigin.TypeId == (int)ECertificateOfOriginType.EURMED ||
                certificateOfOrigin.TypeId == (int)ECertificateOfOriginType.EUR1);
    }

    private async Task<Dictionary<int, string?>> GetCurrencyCodes(List<int> currencyTypeIds)
    {
        if (currencyTypeIds.Count == 0)
        {
            return [];
        }

        var currencies = await currencyTypeProxy.GetCurrencyTypesByIds(currencyTypeIds.Distinct().ToList());
        if (currencies == null)
        {
            return [];
        }

        return currencies
            .GroupBy(c => c.Id)
            .ToDictionary(g => g.Key, g => g.First().CurrencyCode);
    }

    private async Task<List<FieldDataDto>> GetCertificateOfOriginDetails(CertificateOfOriginWebQueryDto certificate)
    {
        var fieldDataDtos = await BuildHeaderFields(certificate);

        foreach (var detail in certificate.CertificateOfOriginDetails)
        {
            var field = MapDetailField(certificate, detail);
            if (field != null)
            {
                fieldDataDtos.Add(field);
            }
        }

        return fieldDataDtos;
    }

    private async Task<List<FieldDataDto>> BuildHeaderFields(CertificateOfOriginWebQueryDto certificate)
    {
        var fieldDataDtos = new List<FieldDataDto>();

        var isRetrospective = certificate.RequestReasonCode == (int)ERequestReason.RetrospectiveCertificate;
        var isExportDecForPrint = !string.IsNullOrWhiteSpace(certificate.ExportDeclarationNumber) &&
            certificate.CertificateOfOriginDetails.Any(cood =>
                cood.CertificateDetailsTypeCodeId == (int)ECertificateDetailsType.IsExportDecForPrint &&
                bool.TryParse(cood.Value, out _));

        // Resolve the needed field labels once (was per-field SystemTablesUtil.GetCodeById<DataDictionaryField>).
        var labelFieldIds = new List<int>();
        if (isRetrospective)
        {
            labelFieldIds.Add(RequestReasonCodeFieldId);
        }

        if (certificate.CertificateIdToCancel.HasValue)
        {
            labelFieldIds.Add(CertificateIdToCancelFieldId);
        }

        if (isExportDecForPrint)
        {
            labelFieldIds.Add(ExportDeclarationNumberFieldId);
        }

        var labelsByFieldId = await GetFieldLabels(labelFieldIds);

        if (isRetrospective)
        {
            fieldDataDtos.Add(new FieldDataDto { Label = labelsByFieldId.GetValueOrDefault(RequestReasonCodeFieldId), Value = "Issued Retrospectively" });
        }

        if (certificate.CertificateIdToCancel.HasValue)
        {
            fieldDataDtos.Add(new FieldDataDto
            {
                Label = labelsByFieldId.GetValueOrDefault(CertificateIdToCancelFieldId),
                Value = $"Replacing certificate {certificate.CertificateIdToCancel.Value}"
            });
        }

        fieldDataDtos.Add(new FieldDataDto { Label = IssuingDateLabel, Value = certificate.IssuingDate });

        if (isExportDecForPrint)
        {
            fieldDataDtos.Add(new FieldDataDto { Label = labelsByFieldId.GetValueOrDefault(ExportDeclarationNumberFieldId), Value = certificate.ExportDeclarationNumber });
        }

        return fieldDataDtos;
    }

    private static FieldDataDto? MapDetailField(CertificateOfOriginWebQueryDto certificate, CertificateOfOriginWebDetailDto detail)
    {
        switch (detail.CertificateDetailsTypeCodeId)
        {
            case (int)ECertificateDetailsType.PlaceOfManufacture:
            case (int)ECertificateDetailsType.ZipCodeOfManufacture:
            case (int)ECertificateDetailsType.Observations:
            case (int)ECertificateDetailsType.IsDeclaredByManufacturer:
            case (int)ECertificateDetailsType.IsDeclaredByExporter:
            case (int)ECertificateDetailsType.ExporterAddress:
            case (int)ECertificateDetailsType.ExporterName:
            case (int)ECertificateDetailsType.CountryOfDeclaration:
            case (int)ECertificateDetailsType.DestinationCountry:
            case (int)ECertificateDetailsType.ExporterCountry:
            case (int)ECertificateDetailsType.OriginCountry:
            case (int)ECertificateDetailsType.CustomsHouse:
            case (int)ECertificateDetailsType.ExporterId:
            case (int)ECertificateDetailsType.TradeAgreementCountry1:
            case (int)ECertificateDetailsType.TradeAgreementCountry2:
            case (int)ECertificateDetailsType.TradeAgreementGroupOfCountries:
            case (int)ECertificateDetailsType.ConsigneeRemarks:
            case (int)ECertificateDetailsType.OriginGroupOfCountries:
            case (int)ECertificateDetailsType.DestinationGroupOfCountries:
            case (int)ECertificateDetailsType.Transport:
            case (int)ECertificateDetailsType.IssuingCountry:
            case (int)ECertificateDetailsType.CityOfDeclaration:
            case (int)ECertificateDetailsType.PortOfShipment:
                return PrintOutField(detail);
            case (int)ECertificateDetailsType.DateOfDeclaration:
                return MapDateOfDeclarationField(detail);
            case (int)ECertificateDetailsType.ConsigneeName:
            case (int)ECertificateDetailsType.ConsigneeAddress:
            case (int)ECertificateDetailsType.ConsigneeCountry:
                return MapConsigneeField(certificate, detail);
            default:
                return null;
        }
    }

    private static FieldDataDto? MapDateOfDeclarationField(CertificateOfOriginWebDetailDto detail)
    {
        if (!string.IsNullOrWhiteSpace(detail.Value) &&
            DateTime.TryParse(detail.Value, CultureInfo.CurrentCulture, DateTimeStyles.None, out var dateOfDeclaration))
        {
            return new FieldDataDto
            {
                Label = detail.CertificateDetailsTypeCode?.EnglishName,
                Value = dateOfDeclaration.ToString("dd MMMM yyyy", new CultureInfo("en-US"))
            };
        }

        return null;
    }

    private static FieldDataDto? MapConsigneeField(CertificateOfOriginWebQueryDto certificate, CertificateOfOriginWebDetailDto detail)
    {
        var isConsigneeForPrint = certificate.CertificateOfOriginDetails.Any(cood =>
            cood.CertificateDetailsTypeCodeId == (int)ECertificateDetailsType.IsConsigneeForPrint &&
            bool.TryParse(cood.Value, out _));
        if (!isConsigneeForPrint)
        {
            return null;
        }

        if (certificate.TypeId == (int)ECertificateOfOriginType.EUR1 ||
            certificate.TypeId == (int)ECertificateOfOriginType.EURMED)
        {
            // LEGACY quirk (preserved, developer-confirmed 2026-07-28): result set 5 never returns an IsToPrint
            // column, so this is always false — Consignee fields for EUR1/EURMED are never printed.
            return detail.CertificateOfOriginWebPrintOut?.CertificateDetailsTypeIsToPrint == true
                ? PrintOutField(detail)
                : null;
        }

        return PrintOutField(detail);
    }

    private static FieldDataDto PrintOutField(CertificateOfOriginWebDetailDto detail)
    {
        return new FieldDataDto
        {
            Label = detail.CertificateOfOriginWebPrintOut?.CertificateDetailsTypeEnglishName,
            Value = detail.CertificateOfOriginWebPrintOut?.CertificateDetailsTypeValue
        };
    }

    private async Task<Dictionary<int, string?>> GetFieldLabels(List<int> fieldIds)
    {
        if (fieldIds.Count == 0)
        {
            return [];
        }

        var fields = await dataDictionaryFieldProxy.GetDataDictionaryFieldsByIds(fieldIds.Distinct().ToList());
        if (fields == null)
        {
            return [];
        }

        return fields
            .GroupBy(f => f.Id)
            .ToDictionary(g => g.Key, g => g.First().EnglishName);
    }

    private async Task<string> GetQueryUrl(Guid? guid)
    {
        if (!guid.HasValue)
        {
            return string.Empty;
        }

        // CertificateOfOriginQueryURL — a format string with {0} for the guid (was Configuration.GetConfig in the
        // legacy). Seeded in the local Infrastructure.Parameters table (Active, Level 1).
        var url = await parametersUtil.Get<string>("CertificateOfOriginQueryURL");
        return string.Format(url, guid.Value.ToString());
    }

    #endregion

    public async Task<VirtualEntityDto> Convert(ConnectedEntityDto connectedEntity)
    {
        // ESB/EAI Convert: resolve the connected-entity key (the certificate number) into a generic entity link.
        // Reuses the #7 filter search; a missing certificate owns the 404 contract (legacy threw not-exist).
        var filter = new CertificateOfOriginFilterDto { CertificateNumber = connectedEntity.EntityIdKey1 };
        var certificate = (await GetCertificateOfOriginsByFilter(filter)).FirstOrDefault()
            ?? throw new RestNotFoundException();

        var result = new VirtualEntityDto
        {
            Id = certificate.Id,
            Title = certificate.Name,
            EntityType = (int)EEntityType.CertificateOfOrigin,
            CustomerId = certificate.CustomesAgentId,
        };
        return result;
    }

    public async Task<bool> LoadDataFromExportDeclaration(LoadDataFromExportDeclarationRequestDto request)
    {
        // Guard: without a lead-document id or an export-declaration number there is nothing to look up.
        if (request.LeadDocumentId is null && string.IsNullOrEmpty(request.ExportDeclarationNumber))
        {
            return false;
        }

        var details = await exportDealFileProxy.GetExportDeclarationDetailsForCertificateOfOrigion(
            request.LeadDocumentId, request.ExportDeclarationNumber);

        // The legacy set IsDeclarationReleased/IsCargoExitedOfCustomsRegulation back on the entity (by-ref) and
        // returned this computed flag; over REST only the flag is returned (developer decision 2026-07-27). It is
        // true only when the cargo has exited customs regulation and the request is not a retrospective certificate.
        var isCargoExited = details?.IsCargoExitedOfCustomsRegulation ?? false;
        return isCargoExited && request.RequestReasonCode != (int)ERequestReason.RetrospectiveCertificate;
    }

    public async Task<int> GetCertificateOfOriginID(string certificateNumber)
    {
        // route-style alternate key → not-found owns the 404 contract (RestNotFoundException)
        var result = await DataLayer.GetCertificateOfOriginIdByNumber(certificateNumber)
            ?? throw new RestNotFoundException();
        return result;
    }

    #region LEGACY_WCF
#pragma warning disable S125 // migration convention: preserve the original WCF body as a reference comment

    // Original WCF (CertificateOfOriginsExternalService.InternalGetCertificateOfOriginID):
    //
    // public int? InternalGetCertificateOfOriginID(string certificateNumber)
    // {
    //     using (var uow = Container.Resolve<IUnitOfWork>(CRMConsts.CertificateOfOriginsUnitOfWork))
    //     {
    //         var certificateOfOrigin = uow.Repository.GetQuery<CertificateOfOrigin>()
    //             .OrderByDescending(c => c.CreateDate)
    //             .FirstOrDefault(c => c.CertificateNumber == certificateNumber);
    //         return certificateOfOrigin?.ID;
    //     }
    // }
#pragma warning restore S125
    #endregion

    public async Task<List<GoodsItemCerificateDto>> GetGoodsItemCerificateDTO(List<GoodsItemCerificateDto> goodsItemCerificateDTOs)
    {
        foreach (var item in goodsItemCerificateDTOs)
        {
            if (item.CertificateNumber != null)
            {
                item.CertificateOfOriginId = await DataLayer.GetCertificateOfOriginIdByNumber(item.CertificateNumber);
            }
        }

        return goodsItemCerificateDTOs;
    }

    public async Task<CertificateOfOriginResultDto?> IsCertificateOfOriginByExternalIdExist(string certificateOfOriginExternalId)
    {
        var filter = new CertificateOfOriginFilterDto { CertificateNumber = certificateOfOriginExternalId };
        var certificates = await GetCertificateOfOriginsByFilter(filter);
        var result = certificates.FirstOrDefault();
        return result;
    }

    public async Task<List<CertificateOfOriginResultDto>> GetCertificateOfOriginsByFilter(CertificateOfOriginFilterDto filter)
    {
        var parameters = BuildParameterForProcedure(filter);
        var result = await DataLayer.GetCertificateOfOriginsByFilter(parameters);
        await FillCustomersInformation(result);
        return result;
    }

    private static DynamicParameters BuildParameterForProcedure(CertificateOfOriginFilterDto filter)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@CertificateNumber", filter.CertificateNumber, DbType.String);
        parameters.Add("@CertificateOfOriginStatusID", filter.CertificateOfOriginStatusId, DbType.Int32);
        parameters.Add("@CertificateOfOriginTypeID", filter.CertificateOfOriginTypeId, DbType.Int32);
        parameters.Add("@CustomsAgentID", filter.CustomsAgentId, DbType.Int32);
        parameters.Add("@CustomsHouseID", filter.CustomsHouseId, DbType.Int32);
        parameters.Add("@DestinationCountry", filter.DestinationCountry, DbType.Int32);
        parameters.Add("@ExportDeclarationID", filter.ExportDeclarationId, DbType.Int32);
        parameters.Add("@ExportDeclarationNum", filter.ExportDeclarationNum, DbType.String);
        parameters.Add("@ExporterCustomerID", filter.ExporterCustomerId, DbType.Int32);
        parameters.Add("@FromIssuingDate", filter.FromIssuingDate, DbType.DateTime);
        parameters.Add("@ToIssuingDate", filter.ToIssuingDate, DbType.DateTime);
        parameters.Add("@FromRequestDate", filter.FromRequestDate, DbType.DateTime);
        parameters.Add("@ToRequestDate", filter.ToRequestDate, DbType.DateTime);
        parameters.Add("@RequestReasonID", filter.RequestReasonId, DbType.Int32);
        parameters.Add("@VersionNumber", filter.VersionNumber, DbType.Int32);
        parameters.Add("@IsLastVersion", filter.IsLastVersion, DbType.Boolean);
        return parameters;
    }

    private async Task FillCustomersInformation(List<CertificateOfOriginResultDto> certificates)
    {
        if (certificates.Count == 0)
        {
            return;
        }

        var customerIds = certificates.Select(c => c.ExporterId)
            .Concat(certificates.Select(c => c.CustomesAgentId))
            .Distinct()
            .ToList();
        var customers = await customerProxy.GetCustomersByIds(customerIds);
        if (customers == null)
        {
            return;
        }

        var customersById = customers.ToDictionary(c => c.Id);
        foreach (var certificate in certificates)
        {
            if (customersById.TryGetValue(certificate.ExporterId, out var exporter))
            {
                certificate.ExporterTitle = exporter.Name;
                certificate.ExporterExternalIdNum = exporter.ExternalIdNum;
            }

            if (customersById.TryGetValue(certificate.CustomesAgentId, out var agent))
            {
                certificate.CustomesAgentTitle = agent.Name;
                certificate.CustomesAgentExternalIdNum = agent.ExternalIdNum;
            }
        }
    }
}
