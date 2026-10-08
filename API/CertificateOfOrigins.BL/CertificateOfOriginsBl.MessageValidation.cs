using CertificateOfOrigins.BL.Proxies;
using CertificateOfOrigins.BL.Resolver;
using CertificateOfOrigins.Model.CertificateOfOriginsDb;
using CertificateOfOrigins.Model.ModelDTOs;
using System.Globalization;

namespace CertificateOfOrigins.BL;

// GetPC_MSG2280_2281 incoming-message field-validation engine (create branch). Faithful port of the legacy reflective
// validator (GetCertificateDetailsFromMessageAndCheckFields → ValidateAndCreateCertificateOfOriginDetails →
// ValidateMessageField / CheckSpecificField). Ported as an async BL processing method (developer decision), NOT
// FluentValidation, because each field both validates AND resolves side-values (exporter / destination / org-unit) and
// builds the certificate detail rows — validation, async resolution and construction are interleaved in the legacy.
//
// The legacy mapped a reflected message-DTO field name → CertificateDetailsTypeCodeEnum.Enumeration → its ID. In .NET
// 10 that mapping is the identity ECertificateDetailsType (the enum values ARE the CertificateDetailsTypeCodeEnum ids,
// verified against the DB: ID 1 = ExporterId, …), so the reflected-name/DB-catalogue join is replaced by a static
// per-property → ECertificateDetailsType map — same result, no reflection, no second DB list.
public partial class CertificateOfOriginsBl
{
    // One message field pulled for validation: its detail type, raw string value, and the per-certificate-type
    // constraint (Mandatory/Optional/Condition) from DetailsPerCertificate. This is the .NET 10 stand-in for the legacy
    // transient CertificateOfOriginDetails-with-ConstraintTypeEnumId used only during validation.
    private sealed class MessageField
    {
        public MessageField(ECertificateDetailsType detailType, string? value, int constraintTypeEnumId)
        {
            DetailType = detailType;
            Value = value;
            ConstraintTypeEnumId = constraintTypeEnumId;
        }

        public ECertificateDetailsType DetailType { get; }

        public string? Value { get; set; }

        public string? DisplayedValue { get; set; }

        public int ConstraintTypeEnumId { get; }
    }

    // Accumulates the resolved side-values + validation exceptions + built detail rows across the whole message, so the
    // caller (create branch) can map them onto the certificate. The .NET 10 replacement for the legacy ambient
    // _exporterID / _destinationCountryId / _organizationUnit / _requestExceptions instance fields (no shared mutable
    // state on the BL — one context per request).
    private sealed class MessageValidationContext
    {
        public List<CertificateOfOriginExceptionDto> Exceptions { get; } = [];

        public List<CertificateOfOriginDetails> Details { get; } = [];

        public int? ExporterId { get; set; }

        public int? DestinationCountryId { get; set; }

        public int? OrganizationUnitId { get; set; }

        // Legacy _certificateToUpdateId — the id of the existing certificate a CertificateUpdate targets; becomes both
        // the update target and (in the map) CertificateIDToCancel.
        public int? CertificateToUpdateId { get; set; }

        // The id of the certificate a CertificateReplacement cancels (resolved from certificateIdToCancel) → CertificateIDToCancel.
        public int? CertificateIdToCancel { get; set; }

        // Per-request memoization of the export-declaration details, keyed by "{leadDocumentId}|{exportDeclarationNumber}".
        // A single message hits GetExportDeclarationDetailsForCertificateOfOrigion up to 3× on the same declaration (the
        // amendment guard + the per-reason CheckExportDeclarationNumber use the identical (null, number) key); the cache
        // collapses those to one call. The declaration does not change within a request.
        public Dictionary<string, ExportDeclarationDetailsDto?> DeclarationDetailsCache { get; } = [];
    }

    // Stage 5: the create/update branch. Legacy GetPC_MSG2280_2281_CertificateOfOriginRequestInner default case —
    // validate the message (per-field + cross-field, resolving the exporter / destination / org-unit side-values), and
    // if it is valid map it onto SaveCertificateOfOriginRequestDto and save; then the post-save declaration-submitted
    // reconciliation. Validation errors are accumulated onto the single requestExceptions channel (in-band, not thrown —
    // faithful) and the method returns the saved certificate (null when validation failed, so no save happened).
    private async Task<CertificateOfOrigin?> ProcessCreateCertificateBranch(CertificateOfOriginRequestMessageDto request, MessageValidationContext context, List<CertificateOfOriginExceptionDto> requestExceptions)
    {
        var agentRequest = request.AgentRequest;

        // The validation engine accumulates into context.Exceptions during the pass; they are merged into the single
        // requestExceptions channel at the gate below. The context (with its declaration-details cache) is created once
        // per request by the dispatcher and shared with the amendment guard + reconciliation.
        var invoices = await ValidateMessageBody(request, context, requestExceptions);
        if (invoices == null)
        {
            return null;
        }

        // Per-reason resolution (legacy CheckRequestReasonAndGetSavedCertificate): resolve the existing certificate the
        // reason targets + its reason-specific validations, and record the update/cancel side-values. Runs as part of the
        // accumulation, before the exception gate — faithful to the legacy ordering.
        await ResolveCertificateForReason(agentRequest, context);

        // Legacy: if any validation exception accumulated, the request is rejected — surface them in-band, no save.
        // requestExceptions may already carry a pre-branch error (the amendment-linkage guard), which also blocks the save.
        requestExceptions.AddRange(context.Exceptions);
        if (requestExceptions.Count > 0)
        {
            return null;
        }

        // The certificate number: the supplied id, or a freshly-generated one (legacy ConvertMessageToCertificateOfOrigin
        // → GetCertificateNumber when certificateId is empty).
        var certificateNumber = await ResolveCertificateNumber(agentRequest.CertificateId, agentRequest.RequestReasonCode);

        // Map the validated message + resolved side-values onto the save request (incl. the invoice/item graph) and persist.
        var saveRequest = BuildSaveRequestFromMessage(request, context, certificateNumber, invoices, RequestMetadata.MessageSenderId ?? 0);
        var saved = await SaveCertificateOfOriginCore(saveRequest, context.CertificateToUpdateId);

        // Legacy: post-save, if the linked declaration is submitted/released, reconcile the certificate against it
        // (CheckCertificateOfOriginOnDeclarationSubmited) — only for a real certificate (not EmptyCertificate) that is
        // not NonManipulation. The reconciliation's declaration-mismatch exceptions are returned in-band.
        if (agentRequest.RequestReasonCode != (int)ERequestReason.EmptyCertificate
            && agentRequest.CertificateOfOriginTypeCode != (int)ECertificateOfOriginType.NonManipulation)
        {
            var reconciliationExceptions = await ReconcileWithSubmittedDeclaration(saved, context);
            requestExceptions.AddRange(reconciliationExceptions);
        }

        var certificateEntity = await DataLayer.GetLatestCertificateByNumberForFeedback(saved.CertificateNumber ?? string.Empty);
        return certificateEntity;
    }

    // Legacy GetPC_MSG2280_2281_CertificateOfOriginRequestInner ran this for every reason except CertificateCancellation —
    // the create reasons AND GetRequestStatus: the certificate type, the mandatory body, the per-field and cross-field
    // validation and the invoice conversion (GetCertificateDetailsFromMessageAndCheckFields). Field errors accumulate into
    // context.Exceptions; a hard stop (missing body, missing/unknown type) is added to requestExceptions and returns null.
    // Otherwise returns the converted invoices (empty for NonManipulation / no body).
    private async Task<List<CertificateOfOriginInvoiceDetail>?> ValidateMessageBody(CertificateOfOriginRequestMessageDto request, MessageValidationContext context, List<CertificateOfOriginExceptionDto> requestExceptions)
    {
        var agentRequest = request.AgentRequest;
        var certificateTypeId = agentRequest.CertificateOfOriginTypeCode;

        // Legacy GetCertificateDetailsFromMessageAndCheckFields: NonManipulation validates a DIFFERENT body
        // (request.NonManipulationCertificate) than the standard CertificateOfOrigin body. A null body is a
        // MandatoryValue error EXCEPT for EmptyCertificate and GetRequestStatus, which carry no body
        // (CertificateCancellation is the legacy's third exemption but never runs this validation).
        var isNonManipulation = certificateTypeId == (int)ECertificateOfOriginType.NonManipulation;
        var certificate = request.CertificateOfOrigin;
        var nonManipulation = request.NonManipulationCertificate;

        var bodyMissing = isNonManipulation ? nonManipulation is null : certificate is null;
        if (bodyMissing && agentRequest.RequestReasonCode is not ((int)ERequestReason.EmptyCertificate or (int)ERequestReason.GetRequestStatus))
        {
            var missingBodyName = isNonManipulation ? nameof(request.NonManipulationCertificate) : nameof(request.CertificateOfOrigin);
            requestExceptions.Add(BuildMessageException(EMessageCode.MandatoryValue, missingBodyName));
            return null;
        }

        // Legacy CheckCertificateOfOriginTypeCodeEnum (CertificateOfOriginsIncomingMessageServicePartial.cs:1360-1382):
        // the certificate type must be present (id != 0) and exist in the type table. Legacy threw immediately; here we
        // accumulate the error and return early — which both surfaces it AND reproduces the legacy hard-stop. Without
        // this, an unknown/zero type makes GetDetailsPerCertificate return an empty catalogue, silently skipping the
        // entire field-validation engine for a malformed request.
        if (certificateTypeId == 0)
        {
            requestExceptions.Add(BuildMessageException(EMessageCode.MandatoryValue, nameof(agentRequest.CertificateOfOriginTypeCode)));
            return null;
        }

        // The certificate type's mandatory flags (legacy GetCertificateTypeCode): criterion / customs-item / zipcode.
        var typeCode = await DataLayer.GetCertificateTypeCode(certificateTypeId);
        if (typeCode == null)
        {
            requestExceptions.Add(BuildMessageException(EMessageCode.CertificateTypeNotExist));
            return null;
        }

        agentRequest.IsCertificateTypeCodeMandatory = typeCode.IsCriterionMandatory;
        agentRequest.IsCustomsItemMandatory = typeCode.IsCustomsItemMandatory;
        agentRequest.IsZipcodeMandatory = typeCode.IsZipcodeMandatory;

        // The certificate type's field catalogue (which fields are relevant + their constraint).
        var perCertificate = await DataLayer.GetDetailsPerCertificate(certificateTypeId);

        var invoices = new List<CertificateOfOriginInvoiceDetail>();

        if (isNonManipulation)
        {
            // Legacy HandleNonManipulationCertificateType: validate + build the 15 NonManipulation-body fields via the
            // same field loop (reflecting a different body), plus the optional CustomsHouse detail when a CertificateOfOrigin
            // object rides along. NonManipulation has no invoices (legacy CheckAndConvertInvoiceDetails returns early).
            if (nonManipulation is not null)
            {
                await ValidateAndBuildNonManipulationDetails(certificateTypeId, nonManipulation, perCertificate, context);
                AddCustomsHouseDetail(certificate, context);
                if (context.Details.Count > 0)
                {
                    await CheckMessageCrossFields(certificate, nonManipulation, agentRequest, context.Details, agentRequest.IsZipcodeMandatory, context);
                }
            }
        }
        else if (certificate is not null)
        {
            // Pre-resolve the destination country id (drives the EUR1 place-of-manufacture exemption), mirroring the
            // legacy early GetIdByCode<Country> in GetCertificateDetailsFromMessageAndCheckFields.
            int? destinationCountryId = null;
            if (!string.IsNullOrWhiteSpace(certificate.DestinationCountry))
            {
                destinationCountryId = (await lookupUtil.Search<Lookup.Country>(c => c.CountryAlphaCode2 == certificate.DestinationCountry && c.State == CertificateOfOriginsConsts.ActiveState)).FirstOrDefault()?.Id;
            }

            // Invoice/item shape pre-check + validation-and-conversion (stage 4b), mirroring the legacy
            // ValidateCertificateOfOriginRequestInvoiceDetail + CheckAndConvertInvoiceDetails.
            ValidateInvoiceShape(certificate, context);
            invoices = await ConvertInvoiceDetails(request, context);

            // Per-field validation + detail construction (stages 2+3), then the cross-field pass (stage 4a).
            await ValidateAndBuildCertificateDetails(certificateTypeId, certificate, perCertificate, destinationCountryId, context);
            if (context.Details.Count > 0)
            {
                await CheckMessageCrossFields(certificate, nonManipulation, agentRequest, context.Details, agentRequest.IsZipcodeMandatory, context);
            }
        }

        return invoices;
    }

    // Legacy ConvertMessageToCertificateOfOrigin: the certificate number is the supplied certificateId, or — only for the
    // reasons that have a generating case in the legacy switch — a freshly generated one ("IL" + the 10-digit sequence
    // numerator) when none was supplied. Every other reason keeps the number as sent.
    private async Task<string?> ResolveCertificateNumber(string? certificateId, int requestReasonCode)
    {
        if (!string.IsNullOrEmpty(certificateId) || !NumberGeneratingReasons.Contains(requestReasonCode))
        {
            return certificateId;
        }

        var numerator = await DataLayer.GetNextCertificateOfOriginNumber();
        return CertificateOfOriginsConsts.CertificateNumberPrefixIl + numerator.ToString(CertificateOfOriginsConsts.CertificateNumberFormat10Digit, CultureInfo.InvariantCulture);
    }

    private static readonly HashSet<int> NumberGeneratingReasons =
    [
        (int)ERequestReason.NewCertificate,
        (int)ERequestReason.RetrospectiveCertificate,
        (int)ERequestReason.EmptyCertificate,
        (int)ERequestReason.CertificateReplacement,
        (int)ERequestReason.ImportCertificateReplacement,
        (int)ERequestReason.Draft,
    ];

    // Map the validated incoming message + resolved side-values onto SaveCertificateOfOriginRequestDto (legacy
    // ConvertMessageToCertificateOfOrigin) — including the per-reason cancel/replace ids and the invoice/item graph.
    // agentId is the message sender (legacy request.CustomerID of the EAI envelope). The agent never sends its own id in the
    // message body: the platform sets it from the MessageSenderId header, read here from RequestMetadata.MessageSenderId.
    private static SaveCertificateOfOriginRequestDto BuildSaveRequestFromMessage(CertificateOfOriginRequestMessageDto request, MessageValidationContext context, string? certificateNumber, List<CertificateOfOriginInvoiceDetail> invoices, int agentId)
    {
        var agentRequest = request.AgentRequest;

        // Legacy: CustomerID = (NonManipulation || no certificate body) ? agentId : _exporterID — driven by certificate
        // TYPE, not by whether the exporter resolved. For a non-NonManipulation certificate the exporter id is used
        // unconditionally (its default 0 if never resolved), never the agent id.
        var isNonManipulationOrNoBody = agentRequest.CertificateOfOriginTypeCode == (int)ECertificateOfOriginType.NonManipulation
            || request.CertificateOfOrigin is null;
        var customerId = isNonManipulationOrNoBody ? agentId : context.ExporterId ?? 0;

        // Legacy ConvertMessageToCertificateOfOrigin per-reason assignments:
        //   CertificateUpdate           → CertificateIDToCancel = _certificateToUpdateId (the resolved existing cert)
        //   CertificateReplacement      → CertificateIDToCancel = the cancelled cert's id
        //   ImportCertificateReplacement→ CertificateToReplaceInImport = certificateIdToCancel (the raw external number)
        var reason = agentRequest.RequestReasonCode;
        int? certificateIdToCancel = reason switch
        {
            (int)ERequestReason.CertificateUpdate => context.CertificateToUpdateId,
            (int)ERequestReason.CertificateReplacement => context.CertificateIdToCancel,
            _ => null,
        };
        var certificateToReplaceInImport = reason == (int)ERequestReason.ImportCertificateReplacement
            ? agentRequest.CertificateIdToCancel
            : null;

        return new SaveCertificateOfOriginRequestDto
        {
            TypeId = agentRequest.CertificateOfOriginTypeCode,
            Title = certificateNumber,
            CertificateNumber = certificateNumber,
            CustomerId = customerId,
            CreateCustomerId = agentId,
            UpdateCustomerId = agentId,
            OrganizationUnitId = context.OrganizationUnitId ?? 0,
            DestinationCountry = context.DestinationCountryId,
            CertificateOfOriginStatusId = (int)ECertificateOfOriginStatus.Received,
            RequestReasonCode = reason,
            ReplacementReason = agentRequest.ReplacementReason,
            InternalApplication = agentRequest.InternalApplication,
            ExportDeclarationNumber = agentRequest.ExportDeclarationNum,
            CertificateIdToCancel = certificateIdToCancel,
            CertificateToReplaceInImport = certificateToReplaceInImport,
            IsAttachedList = request.CertificateOfOrigin?.IsAttachedList ?? false,
            InSufficentworkingInd = request.CertificateOfOrigin?.InSufficentworkingInd ?? false,
            InsufficentWorkingText = request.CertificateOfOrigin?.InsufficentWorkingText,
            CertificateOfOriginDetails = context.Details
                .Select(d => new CertificateOfOriginDetailDto
                {
                    CertificateDetailsTypeCodeId = d.CertificateDetailsTypeCodeId,
                    Value = d.Value,
                    DisplayedValue = d.DisplayedValue,
                })
                .ToList(),
            CertificateOfOriginInvoiceDetails = invoices,
        };
    }

    // Stage-2 reflective loop equivalent: for each certificate-body field that (a) is a tracked detail type and (b) is
    // declared in DetailsPerCertificate for this certificate type, build a MessageField, run its per-field validation
    // (CheckSpecificField — stage 3), and add the resulting detail row. A blank value errors only when Mandatory.
    private async Task ValidateAndBuildCertificateDetails(int certificateTypeId, CertificateOfOriginMessageDto certificate, List<DetailsPerCertificate> perCertificate, int? destinationCountryId, MessageValidationContext context)
    {
        await BuildDetailsFromFields(certificateTypeId, EnumerateCertificateFields(certificate), perCertificate, destinationCountryId, context);
    }

    // NonManipulation variant (legacy HandleNonManipulationCertificateType → ValidateAndCreateCertificateOfOriginDetails
    // over the NonManipulation body). Same field loop, different body; there is no place-of-manufacture field on a
    // NonManipulation certificate, so the destination-country exemption argument is null.
    private async Task ValidateAndBuildNonManipulationDetails(int certificateTypeId, NonManipulationCertificateMessageDto nonManipulation, List<DetailsPerCertificate> perCertificate, MessageValidationContext context)
    {
        await BuildDetailsFromFields(certificateTypeId, EnumerateNonManipulationFields(nonManipulation), perCertificate, null, context);
    }

    // Legacy GetCustomsHouseDetails: when a standard CertificateOfOrigin body accompanies the NonManipulation body, its
    // CustomsHouse value is carried across as one additional detail. Added UNCONDITIONALLY — no mandatory/format check
    // (CustomsHouse lives only on the standard body, so the NonManipulation field loop never produces it).
    private static void AddCustomsHouseDetail(CertificateOfOriginMessageDto? certificate, MessageValidationContext context)
    {
        if (certificate is null)
        {
            return;
        }

        var value = certificate.CustomsHouse ?? string.Empty;
        context.Details.Add(new CertificateOfOriginDetails
        {
            CertificateDetailsTypeCodeId = (int)ECertificateDetailsType.CustomsHouse,
            Value = value,
            DisplayedValue = value.Length > 0 ? value : null,
        });
    }

    // The shared per-field validation + detail-construction loop (legacy ValidateAndCreateCertificateOfOriginDetails),
    // driven by whichever body's (detail type, raw value) sequence is supplied. A field not declared in
    // DetailsPerCertificate for this type is skipped; a blank value errors only when Mandatory.
    private async Task BuildDetailsFromFields(int certificateTypeId, IEnumerable<(ECertificateDetailsType DetailType, string? Value)> fields, List<DetailsPerCertificate> perCertificate, int? destinationCountryId, MessageValidationContext context)
    {
        // Which detail types this certificate type declares → its constraint (legacy: details.FirstOrDefault by type).
        var constraintByType = perCertificate.ToDictionary(d => d.CertificateDetailsTypeCodeId, d => d.ConstraintTypeEnumId);

        foreach (var (detailType, rawValue) in fields)
        {
            if (!constraintByType.TryGetValue((int)detailType, out var constraintTypeEnumId))
            {
                // Not applicable to this certificate type → silently skipped (legacy: detail == null → continue).
                continue;
            }

            var value = rawValue ?? string.Empty;
            var field = new MessageField(detailType, value, constraintTypeEnumId)
            {
                DisplayedValue = value.Length > 0 ? value : null,
            };

            if (!string.IsNullOrWhiteSpace(field.Value))
            {
                await CheckSpecificField(field, certificateTypeId, destinationCountryId, context);
            }
            else if (constraintTypeEnumId == (int)EConstraintType.Mandatory)
            {
                context.Exceptions.Add(BuildMessageException(EMessageCode.MandatoryValue, detailType));
            }

            context.Details.Add(new CertificateOfOriginDetails
            {
                CertificateDetailsTypeCodeId = (int)detailType,
                Value = field.Value,
                DisplayedValue = field.DisplayedValue,
            });
        }
    }

    // The message certificate body → (detail type, raw string value) pairs. Replaces the legacy reflection over the
    // message DTO's fields joined to CertificateDetailsTypeCodeEnum.Enumeration. camelCase JSON values are rendered to
    // the string form the legacy stored (id string / date / "True"/"False"). Only the fields that map to a tracked
    // ECertificateDetailsType are listed; whether each is actually validated is gated by DetailsPerCertificate above.
    private static IEnumerable<(ECertificateDetailsType DetailType, string? Value)> EnumerateCertificateFields(CertificateOfOriginMessageDto c)
    {
        yield return (ECertificateDetailsType.ExporterId, c.ExporterId);
        yield return (ECertificateDetailsType.ExporterName, c.ExporterName);
        yield return (ECertificateDetailsType.ExporterAddress, c.ExporterAddress);
        yield return (ECertificateDetailsType.ExporterCountry, c.ExporterCountry);
        yield return (ECertificateDetailsType.TradeAgreementCountry1, c.TradeAgreementCountry1);
        yield return (ECertificateDetailsType.TradeAgreementCountry2, c.TradeAgreementCountry2);
        yield return (ECertificateDetailsType.TradeAgreementGroupOfCountries, ToStringValue(c.TradeAgreementGroupOfCountries));
        yield return (ECertificateDetailsType.ConsigneeName, c.ConsigneeName);
        yield return (ECertificateDetailsType.ConsigneeAddress, c.ConsigneeAddress);
        yield return (ECertificateDetailsType.ConsigneeCountry, c.ConsigneeCountry);
        yield return (ECertificateDetailsType.ConsigneeRemarks, c.ConsigneeRemarks);
        yield return (ECertificateDetailsType.IsConsigneeForPrint, ToStringValue(c.IsConsigneeForPrint));
        yield return (ECertificateDetailsType.OriginCountry, c.OriginCountry);
        yield return (ECertificateDetailsType.OriginGroupOfCountries, ToStringValue(c.OriginGroupOfCountries));
        yield return (ECertificateDetailsType.DestinationCountry, c.DestinationCountry);
        yield return (ECertificateDetailsType.DestinationGroupOfCountries, ToStringValue(c.DestinationGroupOfCountries));
        yield return (ECertificateDetailsType.Transport, c.Transport);
        yield return (ECertificateDetailsType.PortOfShipment, c.PortOfShipment);
        yield return (ECertificateDetailsType.IsCumulation, ToStringValue(c.IsCumulation));
        yield return (ECertificateDetailsType.CumulationCountry, c.CumulationCountry);
        yield return (ECertificateDetailsType.CumulationGroupOfCountries, ToStringValue(c.CumulationGroupOfCountries));
        yield return (ECertificateDetailsType.PlaceOfManufacture, ToStringValue(c.PlaceOfManufacture));
        yield return (ECertificateDetailsType.ZipCodeOfManufacture, ToStringValue(c.ZipCodeOfManufacture));
        yield return (ECertificateDetailsType.Observations, c.Observations);
        yield return (ECertificateDetailsType.IsExportDecForPrint, ToStringValue(c.IsExportDecForPrint));
        yield return (ECertificateDetailsType.CustomsHouse, c.CustomsHouse);
        yield return (ECertificateDetailsType.IssuingCountry, c.IssuingCountry);
        yield return (ECertificateDetailsType.CityOfDeclaration, ToStringValue(c.CityOfDeclaration));
        yield return (ECertificateDetailsType.CountryOfDeclaration, c.CountryOfDeclaration);
        yield return (ECertificateDetailsType.DateOfDeclaration, ToStringValue(c.DateOfDeclaration));
        yield return (ECertificateDetailsType.IsDeclaredByManufacturer, ToStringValue(c.IsDeclaredByManufacturer));
        yield return (ECertificateDetailsType.IsDeclaredByExporter, ToStringValue(c.IsDeclaredByExporter));
    }

    // The NonManipulation body → (detail type, raw value) pairs, in the legacy field-declaration order (EAI schema
    // order 0-14 → detail type ids 34-48). The .NET 10 DTO corrected two legacy field spellings — ExportBillOfLadingNum
    // and TransitCountry — so those map explicitly to their (misspelled) detail types ExportBillOFLadingNum(39) and
    // TransirCountry(40). The three dates are non-nullable, so they always render → always flow to the date validators.
    private static IEnumerable<(ECertificateDetailsType DetailType, string? Value)> EnumerateNonManipulationFields(NonManipulationCertificateMessageDto c)
    {
        yield return (ECertificateDetailsType.ExportDate, ToStringValue(c.ExportDate));
        yield return (ECertificateDetailsType.ExportCountry, c.ExportCountry);
        yield return (ECertificateDetailsType.ImportBillOfLadingNum, c.ImportBillOfLadingNum);
        yield return (ECertificateDetailsType.ExportPort, c.ExportPort);
        yield return (ECertificateDetailsType.ImportDate, ToStringValue(c.ImportDate));
        yield return (ECertificateDetailsType.ExportBillOFLadingNum, c.ExportBillOfLadingNum);
        yield return (ECertificateDetailsType.TransirCountry, c.TransitCountry);
        yield return (ECertificateDetailsType.PortOfEntrance, c.PortOfEntrance);
        yield return (ECertificateDetailsType.ExpectedExitDate, ToStringValue(c.ExpectedExitDate));
        yield return (ECertificateDetailsType.ExitPort, c.ExitPort);
        yield return (ECertificateDetailsType.GoodsDescription, c.GoodsDescription);
        yield return (ECertificateDetailsType.DeclaringCompany, c.DeclaringCompany);
        yield return (ECertificateDetailsType.DeclaringPerson, c.DeclaringPerson);
        yield return (ECertificateDetailsType.DeclaringPosition, c.DeclaringPosition);
        yield return (ECertificateDetailsType.ManifestNum, c.ManifestNum);
    }

    private static string? ToStringValue(int? value)
    {
        return value?.ToString(CultureInfo.InvariantCulture);
    }

    private static string? ToStringValue(bool? value)
    {
        // Legacy reflected a nullable bool to its ToString() ("True"/"False"); null → not supplied (empty).
        return value?.ToString();
    }

    // The legacy message-DTO date fields are NON-nullable DateTime, so reflection always rendered a value — even when
    // the client omitted the date (default 0001-01-01) — which therefore always flowed into the per-field date validator
    // (CheckDeclarationDate/CheckExportDate/…), never the "mandatory blank" branch. Render unconditionally to preserve
    // that: a missing/default date is rejected by the date-range check, not silently allowed as an optional blank.
    // Decided (parity C-F18, 2026-10-08): the stored Value is ISO ("o"), not legacy's host-culture string. Every reader of a date
    // detail's Value parses it with DateTime.TryParse (this service, the legacy client and server), which reads ISO under any
    // culture; the print SPs read DisplayedValue, which keeps the legacy dd/MM/yyyy (LegacyHostCulture).
    private static string ToStringValue(DateTime value)
    {
        return value.ToString("o", CultureInfo.InvariantCulture);
    }

    // Stage 3: the per-field-type validation + code→id resolution (legacy CheckSpecificField's switch on
    // ECertificateDetailsType). A non-blank field is dispatched to its validator, which both validates and rewrites
    // the field value and display value (external code becomes internal id or display name) and, for the exporter and
    // customs-house fields, records the resolved side-value on the context. Faithful to the legacy switch.
    private async Task CheckSpecificField(MessageField field, int certificateTypeId, int? destinationCountryId, MessageValidationContext context)
    {
        switch (field.DetailType)
        {
            case ECertificateDetailsType.ExporterId:
                await CheckIfExporterExist(field, context);
                if (context.ExporterId is not null and not 0)
                {
                    field.Value = context.ExporterId.Value.ToString(CultureInfo.InvariantCulture);
                }

                break;

            case ECertificateDetailsType.ExporterCountry:
            case ECertificateDetailsType.CountryOfDeclaration:
            case ECertificateDetailsType.IssuingCountry:
            case ECertificateDetailsType.TransirCountry:
                await CheckIfCountryInSystemAndIsrael(field, context);
                break;

            case ECertificateDetailsType.TradeAgreementCountry1:
                await CheckAgreementFirstCountry(field, certificateTypeId, context);
                break;

            case ECertificateDetailsType.TradeAgreementCountry2:
            case ECertificateDetailsType.ConsigneeCountry:
            case ECertificateDetailsType.OriginCountry:
            case ECertificateDetailsType.CumulationCountry:
                await CheckIfCountryIsInTradeAgreement(field, certificateTypeId, context);
                break;

            case ECertificateDetailsType.TradeAgreementGroupOfCountries:
            case ECertificateDetailsType.OriginGroupOfCountries:
            case ECertificateDetailsType.DestinationGroupOfCountries:
            case ECertificateDetailsType.CumulationGroupOfCountries:
                await CheckIfCountryGroupIsInTradeAgreement(field, certificateTypeId, context);
                break;

            case ECertificateDetailsType.DateOfDeclaration:
                CheckDeclarationDate(field, context);
                break;

            case ECertificateDetailsType.IsDeclaredByExporter:
                CheckIfDeclaredByExporter(field, context);
                ConvertBoolFieldToYesNo(field);
                break;

            case ECertificateDetailsType.ExportDate:
                CheckExportDate(field, context);
                break;

            case ECertificateDetailsType.CityOfDeclaration:
                await CheckCityOfDeclaration(field, context);
                break;

            case ECertificateDetailsType.PlaceOfManufacture:
                await CheckCityOfDeclaration(field, context);
                await CheckIfExemptPlaceOfManufacture(field, certificateTypeId, destinationCountryId);
                break;

            case ECertificateDetailsType.ExpectedExitDate:
                CheckExpectedExitDate(field, context);
                break;

            case ECertificateDetailsType.ImportDate:
                // Legacy CheckImportDate(detail): reformat the display to a short date (parsing an unparseable value
                // yields default(DateTime), exactly as the legacy). The range constraint itself is cross-field (stage 4).
                field.DisplayedValue = LegacyHostCulture.ToShortDate(DateTime.TryParse(field.Value, CultureInfo.InvariantCulture, out var importDate) ? importDate : default);
                break;

            case ECertificateDetailsType.IsConsigneeForPrint:
            case ECertificateDetailsType.IsCumulation:
            case ECertificateDetailsType.IsExportDecForPrint:
            case ECertificateDetailsType.IsDeclaredByManufacturer:
                ConvertBoolFieldToYesNo(field);
                break;

            case ECertificateDetailsType.ExportCountry:
                await CheckExportCountry(field, certificateTypeId, context);
                break;

            case ECertificateDetailsType.PortOfEntrance:
            case ECertificateDetailsType.ExitPort:
            case ECertificateDetailsType.ExportPort:
            case ECertificateDetailsType.PortOfShipment:
                await CheckIfInternationalSiteExist(field, context);
                break;

            default:
                // Pass-through fields (name/address/remarks/text) — no validation, kept as-is (legacy commented-out cases).
                break;
        }
    }

    // ── Per-field validators (faithful ports of the legacy Check* helpers) ──

    // Legacy CheckIfExporterExist: resolve the exporter external id to a customer id; missing → CustomerNotInCustomers.
    private async Task CheckIfExporterExist(MessageField field, MessageValidationContext context)
    {
        var customerProxy = Resolve<ICustomerProxy>();
        var exporterId = await customerProxy.GetCustomerIdByExternalId(field.Value ?? string.Empty);
        if (exporterId is null)
        {
            context.Exceptions.Add(BuildMessageException(EMessageCode.CustomerNotInCustomers, field.Value));
        }
        else
        {
            context.ExporterId = exporterId.Value;
        }
    }

    // Legacy CheckIfCountryInSystemAndIsrael: the country alpha-2 code resolves and must be Israel (per-detail-type
    // message). Rewrites the value to the country id and the display to its English name.
    private async Task CheckIfCountryInSystemAndIsrael(MessageField field, MessageValidationContext context)
    {
        var country = await ResolveCountry(field.Value, context);
        if (country is null)
        {
            return;
        }

        if (!await IsCountryIsrael(country.Id))
        {
            var code = field.DetailType switch
            {
                ECertificateDetailsType.ExporterCountry => EMessageCode.IllegalExporterCountry,
                ECertificateDetailsType.IssuingCountry => EMessageCode.IssuingCountryIllegal,
                ECertificateDetailsType.CountryOfDeclaration => EMessageCode.ExporterDecCountryIllegal,
                ECertificateDetailsType.TransirCountry => EMessageCode.TransirCountryIllegal,
                _ => EMessageCode.IllegalExporterCountry,
            };
            context.Exceptions.Add(BuildMessageException(code));
        }

        ApplyCountryResolution(field, country);
    }

    // Legacy CheckExportCountry: for NonManipulation the export country must be non-Israel; for others it must be Israel.
    private async Task CheckExportCountry(MessageField field, int certificateTypeId, MessageValidationContext context)
    {
        var country = await ResolveCountry(field.Value, context);
        if (country is null)
        {
            return;
        }

        var isIsrael = await IsCountryIsrael(country.Id);
        if (!isIsrael && certificateTypeId != (int)ECertificateOfOriginType.NonManipulation)
        {
            context.Exceptions.Add(BuildMessageException(EMessageCode.IllegalExporterCountry));
        }
        else if (isIsrael && certificateTypeId == (int)ECertificateOfOriginType.NonManipulation)
        {
            context.Exceptions.Add(BuildMessageException(EMessageCode.ExportCounrtyIllegal));
        }

        ApplyCountryResolution(field, country);
    }

    // Legacy CheckAgreementFirstCountry: for EUR1/EURMED the first agreement country must be Israel, then the standard
    // trade-agreement check runs.
    private async Task CheckAgreementFirstCountry(MessageField field, int certificateTypeId, MessageValidationContext context)
    {
        if (certificateTypeId is (int)ECertificateOfOriginType.EUR1 or (int)ECertificateOfOriginType.EURMED)
        {
            var country = await ResolveCountry(field.Value, context);
            if (country is null)
            {
                return;
            }

            if (!await IsCountryIsrael(country.Id))
            {
                context.Exceptions.Add(BuildMessageException(EMessageCode.IllegalFirstCountryInAgreement));
            }
        }

        await CheckIfCountryIsInTradeAgreement(field, certificateTypeId, context);
    }

    // Legacy CheckIfCountryIsInTradeAgreement: country resolves + is part of the trade agreement for this certificate type.
    private async Task CheckIfCountryIsInTradeAgreement(MessageField field, int certificateTypeId, MessageValidationContext context)
    {
        var country = await ResolveCountry(field.Value, context);
        if (country is null)
        {
            return;
        }

        var isInTrade = await IsTradeAgreementForCountry(certificateTypeId, country.Id, false);
        if (!isInTrade)
        {
            var code = field.DetailType switch
            {
                ECertificateDetailsType.TradeAgreementCountry2 => EMessageCode.SecondCountryNotInAgreement,
                ECertificateDetailsType.ConsigneeCountry => EMessageCode.ImporterCountryNotInAgreement,
                ECertificateDetailsType.OriginCountry => EMessageCode.OriginCountryNotInAgreement,
                ECertificateDetailsType.CumulationCountry => EMessageCode.CumulationCountryNotInAgreement,
                _ => EMessageCode.OriginCountryNotInAgreement,
            };
            context.Exceptions.Add(BuildMessageException(code));
        }

        ApplyCountryResolution(field, country);
    }

    // Legacy CheckIfCountryGroupIsInTradeAgreement: the (numeric) country-group id is part of the trade agreement.
    private async Task CheckIfCountryGroupIsInTradeAgreement(MessageField field, int certificateTypeId, MessageValidationContext context)
    {
        // Legacy GetCountryGroupId: the value must parse AND the group id must exist in the CountryGroup table
        // (GetIdByCode<CountryGroup>(PropID, id) = Lookup.CountryGroup by id → TheValueInFieldNotExistsInSystem on a miss); on
        // failure the legacy returns 0 and skips the trade-agreement check.
        Lookups.CountryGroup? countryGroup = null;
        if (!int.TryParse(field.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var countryGroupId)
            || (countryGroup = await lookupUtil.Get<Lookups.CountryGroup>(countryGroupId, CertificateOfOriginsConsts.ActiveState)) is null)
        {
            context.Exceptions.Add(BuildMessageException(EMessageCode.TheValueInFieldNotExistsInSystem, field.DetailType));
            return;
        }

        var isInTrade = await IsTradeAgreementForCountry(certificateTypeId, countryGroupId, true);
        if (!isInTrade)
        {
            var code = field.DetailType switch
            {
                ECertificateDetailsType.TradeAgreementGroupOfCountries => EMessageCode.GroupOfCountriesNotInAgreement,
                ECertificateDetailsType.OriginGroupOfCountries => EMessageCode.OriginGroupOfCountriesNotInAgreement,
                ECertificateDetailsType.DestinationGroupOfCountries => EMessageCode.DestinationGroupOfCountriesNotInAgreement,
                ECertificateDetailsType.CumulationGroupOfCountries => EMessageCode.CumulationGroupOfCountriesNotInAgreement,
                _ => EMessageCode.GroupOfCountriesNotInAgreement,
            };
            context.Exceptions.Add(BuildMessageException(code));
        }

        // Legacy rewrote Value to the resolved group id (already numeric here) and the display to its English name.
        field.Value = countryGroup.Id.ToString(CultureInfo.InvariantCulture);
        field.DisplayedValue = countryGroup.EnglishName;
    }

    // Legacy CheckDeclarationDate: within [-5 days, today].
    private static void CheckDeclarationDate(MessageField field, MessageValidationContext context)
    {
        if (!DateTime.TryParse(field.Value, CultureInfo.InvariantCulture, out var date))
        {
            return;
        }

        if (date > DateTime.Today || date < DateTime.Today.AddDays(-5))
        {
            context.Exceptions.Add(BuildMessageException(EMessageCode.ExporterDecDateIllegal));
        }
        else
        {
            field.DisplayedValue = LegacyHostCulture.ToShortDate(date);
        }
    }

    // Legacy CheckExportDate: not more than 3 months in the future.
    private static void CheckExportDate(MessageField field, MessageValidationContext context)
    {
        if (!DateTime.TryParse(field.Value, CultureInfo.InvariantCulture, out var date))
        {
            return;
        }

        if (date > DateTime.Today.AddMonths(3))
        {
            context.Exceptions.Add(BuildMessageException(EMessageCode.ExportDateIllegal));
        }
        else
        {
            field.DisplayedValue = LegacyHostCulture.ToShortDate(date);
        }
    }

    // Legacy CheckExpectedExitDate: within [today, +3 months].
    private static void CheckExpectedExitDate(MessageField field, MessageValidationContext context)
    {
        if (!DateTime.TryParse(field.Value, CultureInfo.InvariantCulture, out var date))
        {
            return;
        }

        if (date < DateTime.Today || date > DateTime.Today.AddMonths(3))
        {
            context.Exceptions.Add(BuildMessageException(EMessageCode.ExitDateIllegal));
        }
        else
        {
            field.DisplayedValue = LegacyHostCulture.ToShortDate(date);
        }
    }

    // Legacy CheckIfDeclaredByExporter: the flag must be true.
    private static void CheckIfDeclaredByExporter(MessageField field, MessageValidationContext context)
    {
        bool.TryParse(field.Value, out var isDeclaredByExporter);
        if (!isDeclaredByExporter)
        {
            context.Exceptions.Add(BuildMessageException(EMessageCode.DeclaringExporter));
        }
    }

    // Legacy ConvertBoolFieldToYesNo: bool value → "Yes"/"No" display.
    private static void ConvertBoolFieldToYesNo(MessageField field)
    {
        bool.TryParse(field.Value, out var boolField);
        field.DisplayedValue = boolField ? "Yes" : "No";
    }

    // Legacy CheckCityOfDeclaration: the value is a city id that must resolve in the City lookup.
    private async Task CheckCityOfDeclaration(MessageField field, MessageValidationContext context)
    {
        if (string.IsNullOrWhiteSpace(field.Value))
        {
            context.Exceptions.Add(BuildMessageException(EMessageCode.MandatoryNullValue, field.DetailType));
            return;
        }

        if (!int.TryParse(field.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var cityId))
        {
            context.Exceptions.Add(BuildMessageException(EMessageCode.CityOfDeclarationDoesNotExistInTheCitiesTable, field.Value));
            return;
        }

        var city = await lookupUtil.Get<Lookup.City>(cityId, CertificateOfOriginsConsts.ActiveState);
        if (city is null)
        {
            context.Exceptions.Add(BuildMessageException(EMessageCode.CityOfDeclarationDoesNotExistInTheCitiesTable, cityId));
        }
        else
        {
            field.DisplayedValue = city.EnglishName;
            field.Value = city.Id.ToString(CultureInfo.InvariantCulture);
        }
    }

    // Legacy CheckIfExemptPlaceOfManufacture: for EUR1, if the destination country is in the config-driven exempt list,
    // clear the place-of-manufacture value.
    private async Task CheckIfExemptPlaceOfManufacture(MessageField field, int certificateTypeId, int? destinationCountryId)
    {
        if (certificateTypeId != (int)ECertificateOfOriginType.EUR1)
        {
            return;
        }

        var exemptCsv = await parametersUtil.Get<string>("CountriesExemptedFromSendingThePlaceOfManufacture") ?? string.Empty;
        var exemptCountries = exemptCsv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (destinationCountryId.HasValue && exemptCountries.Contains(destinationCountryId.Value.ToString(CultureInfo.InvariantCulture)))
        {
            field.Value = string.Empty;
            field.DisplayedValue = string.Empty;
        }
    }

    // Legacy CheckIfInternationalSiteExist: the port or shipment value is a locode that resolves to an international
    // site (SystemTablesUtil.GetIdByCode<InternationalSite>(PropLocode) + GetCodeById). Rewrites the value to the site's
    // locode and the display to its English name. An unknown locode is TheValueInFieldNotExistsInSystem: legacy
    // GetIdByCode caught the lookup's miss exception and added it to the request exceptions.
    private async Task CheckIfInternationalSiteExist(MessageField field, MessageValidationContext context)
    {
        var site = (await lookupUtil.Search<Lookups.InternationalSite>(s => s.Locode == field.Value && s.State == CertificateOfOriginsConsts.ActiveState)).FirstOrDefault();
        if (site is null)
        {
            context.Exceptions.Add(BuildMessageException(EMessageCode.TheValueInFieldNotExistsInSystem, field.DetailType));
            return;
        }

        field.Value = site.Locode;
        field.DisplayedValue = site.EnglishName;
    }

    // ── Shared resolution helpers ──

    // Legacy GetCountryId + GetCodeById<Country>: resolve an alpha-2 code to a country (SystemTablesUtil.GetIdByCode<Country>(
    // PropCountryAlphaCode_2) = Lookup.Country by CountryAlphaCode2); missing → country-not-in-table.
    private async Task<Lookup.Country?> ResolveCountry(string? alphaCode, MessageValidationContext context)
    {
        var country = (await lookupUtil.Search<Lookup.Country>(c => c.CountryAlphaCode2 == alphaCode && c.State == CertificateOfOriginsConsts.ActiveState)).FirstOrDefault();
        if (country is null)
        {
            context.Exceptions.Add(BuildMessageException(EMessageCode.ExportCountryDoesNotExistInTheCountryTable, alphaCode));
        }

        return country;
    }

    // Legacy tail of the country validators: rewrite the value to the country id and the display to its English name.
    private static void ApplyCountryResolution(MessageField field, Lookup.Country country)
    {
        field.Value = country.Id.ToString(CultureInfo.InvariantCulture);
        field.DisplayedValue = country.EnglishName;
    }

    // Legacy ServicesAdapter.IsTradeAgreementForCountry: Israel (as a country) always passes; otherwise the certificate type
    // must have trade agreements (our CertificateOfOriginTypeByTradeAgreement table) and CustomsBook must place the country
    // or group in one of them.
    private async Task<bool> IsTradeAgreementForCountry(int certificateTypeId, int countryId, bool isCountryGroup)
    {
        if (!isCountryGroup && countryId == CertificateOfOriginsConsts.IsraelCountryId)
        {
            return true;
        }

        var (found, tradeAgreements) = await Resolve<CertificateTypeTradeAgreementsResolver>().TryFindAsync(certificateTypeId);
        if (!found || tradeAgreements is null || tradeAgreements.TradeAgreementIds.Count == 0)
        {
            return false;
        }

        var customsBookProxy = Resolve<ICustomsBookProxy>();
        foreach (var tradeAgreementId in tradeAgreements.TradeAgreementIds)
        {
            if (await customsBookProxy.IsTradeAgreementForCountry(countryId, tradeAgreementId, isCountryGroup))
            {
                return true;
            }
        }

        return false;
    }

    // Legacy IsCountryIsrael: compare against the CountryIsrael config parameter.
    private async Task<bool> IsCountryIsrael(int countryId)
    {
        var israelId = await parametersUtil.Get<int>("CountryIsrael");
        return countryId == israelId;
    }
}
