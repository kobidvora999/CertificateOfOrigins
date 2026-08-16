using CustomsCloud.CRM.CertificateOfOrigins.Model.CertificateOfOriginsDb;
using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;
using System.Globalization;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL;

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
    }

    // Stage 5: the create/update branch. Legacy GetPC_MSG2280_2281_CertificateOfOriginRequestInner default case —
    // validate the message (per-field + cross-field, resolving the exporter / destination / org-unit side-values), and
    // if it is valid map it onto SaveCertificateOfOriginRequestDto and save; then the post-save declaration-submitted
    // reconciliation. Validation errors are accumulated onto the single requestExceptions channel (in-band, not thrown —
    // faithful) and the method returns the saved certificate (null when validation failed, so no save happened).
    private async Task<CertificateOfOrigin?> ProcessCreateCertificateBranch(CertificateOfOriginRequestMessageDto request, List<CertificateOfOriginExceptionDto> requestExceptions)
    {
        var agentRequest = request.AgentRequest;
        var certificateTypeId = agentRequest.CertificateOfOriginTypeCode;

        // The validation engine accumulates into context.Exceptions during the pass; they are merged into the single
        // requestExceptions channel at the gate below.
        var context = new MessageValidationContext();

        // NonManipulation carries its certificate body on a different object; the field engine only maps the standard
        // CertificateOfOrigin body. TODO(blocking): the NonManipulation field mapping.
        var certificate = request.CertificateOfOrigin;
        if (certificate is null)
        {
            requestExceptions.Add(BuildMessageException(EMessageCode.MandatoryValue, nameof(request.CertificateOfOrigin)));
            return null;
        }

        // The certificate type's mandatory flags (legacy GetCertificateTypeCode): criterion / customs-item / zipcode.
        var typeCode = await DataLayer.GetCertificateTypeCode(certificateTypeId);
        agentRequest.IsCertificateTypeCodeMandatory = typeCode?.IsCriterionMandatory ?? false;
        agentRequest.IsCustomsItemMandatory = typeCode?.IsCustomsItemMandatory;
        agentRequest.IsZipcodeMandatory = typeCode?.IsZipcodeMandatory ?? false;

        // The certificate type's field catalogue (which fields are relevant + their constraint).
        var perCertificate = await DataLayer.GetDetailsPerCertificate(certificateTypeId);

        // Pre-resolve the destination country id (drives the EUR1 place-of-manufacture exemption), mirroring the legacy
        // early GetIdByCode<Country> in GetCertificateDetailsFromMessageAndCheckFields.
        int? destinationCountryId = null;
        if (!string.IsNullOrWhiteSpace(certificate.DestinationCountry))
        {
            var destinationCountry = await countryProxy.GetCountriesByAlphaCodes([certificate.DestinationCountry]);
            destinationCountryId = destinationCountry?.FirstOrDefault()?.Id;
        }

        // Invoice/item shape pre-check + validation-and-conversion (stage 4b), mirroring the legacy
        // ValidateCertificateOfOriginRequestInvoiceDetail + CheckAndConvertInvoiceDetails (runs before the field loop in
        // the legacy). The converted invoices are validated here; persisting them is a follow-up (see below).
        ValidateInvoiceShape(certificate, context);
        var invoices = await ConvertInvoiceDetails(request, context);

        // Per-field validation + detail construction (stages 2+3), then the cross-field pass (stage 4a).
        await ValidateAndBuildCertificateDetails(certificateTypeId, certificate, perCertificate, destinationCountryId, context);
        if (context.Details.Count > 0)
        {
            await CheckMessageCrossFields(certificate, request.NonManipulationCertificate, agentRequest, context.Details, agentRequest.IsZipcodeMandatory, context);
        }

        // Legacy: if any validation exception accumulated, the request is rejected — surface them in-band, no save.
        if (context.Exceptions.Count > 0)
        {
            requestExceptions.AddRange(context.Exceptions);
            return null;
        }

        // TODO(blocking): the per-reason resolution the legacy runs in CheckRequestReasonAndGetSavedCertificate before
        // the save — fetch the existing certificate the reason targets and run its reason-specific validations
        // (CertificateUpdate: agent/type/status match + set _certificateToUpdateId; CertificateReplacement: cancel-id
        // present + status + set CertificateIDToCancel from the cancelled cert; ImportCertificateReplacement: cancel-id
        // present + CertificateToReplaceInImport; NewCertificate/Draft/Retrospective: not-published/cancelled guard; and
        // the DeclarationMatch/DeclarationMismatch guard). BuildSaveRequestFromMessage therefore cannot yet set
        // CertificateIdToCancel / CertificateToReplaceInImport, so supersession/replacement linking does not occur for
        // messages — delivered with this unit.

        // The validated invoices are built and ready; persisting them requires SaveCertificateOfOrigin's DAL to accept
        // the invoice/item graph (its signature currently takes only the certificate + detail rows).
        // TODO(blocking): extend SaveCertificateOfOrigin (BL + DAL) to persist the invoice/item collection.
        _ = invoices;

        // The certificate number: the supplied id, or a freshly-generated one (legacy ConvertMessageToCertificateOfOrigin
        // → GetCertificateNumber when certificateId is empty).
        var certificateNumber = await ResolveCertificateNumber(agentRequest.CertificateId);

        // Map the validated message + resolved side-values onto the save request and persist.
        var saveRequest = BuildSaveRequestFromMessage(request, context, certificateNumber);
        var saved = await SaveCertificateOfOrigin(saveRequest);

        // TODO(blocking): the post-save CheckCertificateOfOriginOnDeclarationSubmited reconciliation (via
        // UpdateCertificateOfOrigins) — its declaration-mismatch exceptions will be added to requestExceptions here once
        // wired (it reconciles the declaration goods items).
        var certificateEntity = await DataLayer.GetLatestCertificateByNumberForFeedback(saved.CertificateNumber ?? string.Empty);
        return certificateEntity;
    }

    // Legacy ConvertMessageToCertificateOfOrigin: the certificate number is the supplied certificateId, or a freshly
    // generated one ("IL" + the 10-digit sequence numerator) when none was supplied.
    private async Task<string> ResolveCertificateNumber(string? certificateId)
    {
        if (!string.IsNullOrEmpty(certificateId))
        {
            return certificateId;
        }

        var numerator = await DataLayer.GetNextCertificateOfOriginNumber();
        return CertificateOfOriginsConsts.CertificateNumberPrefixIl + numerator.ToString(CertificateOfOriginsConsts.CertificateNumberFormat10Digit, CultureInfo.InvariantCulture);
    }

    // Map the validated incoming message + resolved side-values onto SaveCertificateOfOriginRequestDto (legacy
    // ConvertMessageToCertificateOfOrigin). TODO(blocking): the invoice/item collection (stage 4b) is not mapped.
    // The resolved detail rows and certificate number are carried across.
    private static SaveCertificateOfOriginRequestDto BuildSaveRequestFromMessage(CertificateOfOriginRequestMessageDto request, MessageValidationContext context, string certificateNumber)
    {
        var agentRequest = request.AgentRequest;

        // Legacy: CustomerID = (NonManipulation || no certificate body) ? agentId : _exporterID — driven by certificate
        // TYPE, not by whether the exporter resolved. For a non-NonManipulation certificate the exporter id is used
        // unconditionally (its default 0 if never resolved), never the agent id.
        var isNonManipulationOrNoBody = agentRequest.CertificateOfOriginTypeCode == (int)ECertificateOfOriginType.NonManipulation
            || request.CertificateOfOrigin is null;
        var customerId = isNonManipulationOrNoBody ? request.CustomerId : context.ExporterId ?? 0;

        return new SaveCertificateOfOriginRequestDto
        {
            TypeId = agentRequest.CertificateOfOriginTypeCode,
            Title = certificateNumber,
            CertificateNumber = certificateNumber,
            CustomerId = customerId,
            CreateCustomerId = request.CustomerId,
            UpdateCustomerId = request.CustomerId,
            OrganizationUnitId = context.OrganizationUnitId ?? 0,
            DestinationCountry = context.DestinationCountryId,
            CertificateOfOriginStatusId = (int)ECertificateOfOriginStatus.Received,
            RequestReasonCode = agentRequest.RequestReasonCode,
            ReplacementReason = agentRequest.ReplacementReason,
            InternalApplication = agentRequest.InternalApplication,
            ExportDeclarationNumber = agentRequest.ExportDeclarationNum,
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
        };
    }

    // Stage-2 reflective loop equivalent: for each certificate-body field that (a) is a tracked detail type and (b) is
    // declared in DetailsPerCertificate for this certificate type, build a MessageField, run its per-field validation
    // (CheckSpecificField — stage 3), and add the resulting detail row. A blank value errors only when Mandatory.
    private async Task ValidateAndBuildCertificateDetails(int certificateTypeId, CertificateOfOriginMessageDto certificate, List<DetailsPerCertificate> perCertificate, int? destinationCountryId, MessageValidationContext context)
    {
        // Which detail types this certificate type declares → its constraint (legacy: details.FirstOrDefault by type).
        var constraintByType = perCertificate.ToDictionary(d => d.CertificateDetailsTypeCodeId, d => d.ConstraintTypeEnumId);

        foreach (var (detailType, rawValue) in EnumerateCertificateFields(certificate))
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
    private static string ToStringValue(DateTime value)
    {
        return value.ToString("o", CultureInfo.InvariantCulture);
    }

    // Stage 3: the per-field-type validation + code→id resolution (legacy CheckSpecificField's switch on
    // ECertificateDetailsType). A non-blank field is dispatched to its validator, which both validates AND rewrites
    // field.Value/DisplayedValue (external code → internal id / display name) and, for the exporter / customs-house
    // fields, records the resolved side-value on the context. Faithful to the legacy switch (cases + validators).
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
                // Legacy CheckImportDate on the field only parses/formats; the real constraint is cross-field (stage 4).
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
                await CheckIfInternationalSiteExist(field);
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

    // Legacy CheckIfCountryInSystemAndIsrael: country alpha-2 resolves + must be Israel (per-detail-type message);
    // rewrites the value to the country id and the display to its English name.
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

        var isInTrade = await customsBookProxy.IsTradeAgreementForCountry(certificateTypeId, country.Id, false);
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
        // (GetIdByCode<CountryGroup>(PropID, id) → TheValueInFieldNotExistsInSystem on a miss); on failure the legacy
        // returns 0 and skips the trade-agreement check.
        if (!int.TryParse(field.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var countryGroupId)
            || !await countryGroupProxy.CountryGroupExists(countryGroupId))
        {
            context.Exceptions.Add(BuildMessageException(EMessageCode.TheValueInFieldNotExistsInSystem, field.DetailType));
            return;
        }

        var isInTrade = await customsBookProxy.IsTradeAgreementForCountry(certificateTypeId, countryGroupId, true);
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
        // Country-group id→name has no ILookupUtil type (SystemTables); display left as the id (rollout TODO).
        field.Value = countryGroupId.ToString(CultureInfo.InvariantCulture);
    }

    // Legacy CheckDeclarationDate: within [-5 days, today].
    private static void CheckDeclarationDate(MessageField field, MessageValidationContext context)
    {
        if (!DateTime.TryParse(field.Value, out var date))
        {
            return;
        }

        if (date > DateTime.Today || date < DateTime.Today.AddDays(-5))
        {
            context.Exceptions.Add(BuildMessageException(EMessageCode.ExporterDecDateIllegal));
        }
        else
        {
            field.DisplayedValue = date.ToShortDateString();
        }
    }

    // Legacy CheckExportDate: not more than 3 months in the future.
    private static void CheckExportDate(MessageField field, MessageValidationContext context)
    {
        if (!DateTime.TryParse(field.Value, out var date))
        {
            return;
        }

        if (date > DateTime.Today.AddMonths(3))
        {
            context.Exceptions.Add(BuildMessageException(EMessageCode.ExportDateIllegal));
        }
        else
        {
            field.DisplayedValue = date.ToShortDateString();
        }
    }

    // Legacy CheckExpectedExitDate: within [today, +3 months].
    private static void CheckExpectedExitDate(MessageField field, MessageValidationContext context)
    {
        if (!DateTime.TryParse(field.Value, out var date))
        {
            return;
        }

        if (date < DateTime.Today || date > DateTime.Today.AddMonths(3))
        {
            context.Exceptions.Add(BuildMessageException(EMessageCode.ExitDateIllegal));
        }
        else
        {
            field.DisplayedValue = date.ToShortDateString();
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

        var city = await lookupUtil.Get<Lookup.City>(cityId);
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

    // Legacy CheckIfInternationalSiteExist: the port/shipment value is a locode that resolves to an international site;
    // rewrites the value to the site's locode and the display to its English name. No error if unresolved (legacy).
    private async Task CheckIfInternationalSiteExist(MessageField field)
    {
        var sites = await internationalSiteProxy.GetInternationalSitesByLocodes([field.Value ?? string.Empty]);
        var site = sites?.FirstOrDefault();
        if (site is not null)
        {
            field.Value = site.Locode;
            field.DisplayedValue = site.EnglishName;
        }
    }

    // ── Shared resolution helpers ──

    // Legacy GetCountryId + GetCodeById<Country>: resolve an alpha-2 code to a country; missing → country-not-in-table.
    private async Task<CountryByCodeDto?> ResolveCountry(string? alphaCode, MessageValidationContext context)
    {
        var countries = await countryProxy.GetCountriesByAlphaCodes([alphaCode ?? string.Empty]);
        var country = countries?.FirstOrDefault();
        if (country is null)
        {
            context.Exceptions.Add(BuildMessageException(EMessageCode.ExportCountryDoesNotExistInTheCountryTable, alphaCode));
        }

        return country;
    }

    // Legacy tail of the country validators: rewrite the value to the country id and the display to its English name.
    private static void ApplyCountryResolution(MessageField field, CountryByCodeDto country)
    {
        field.Value = country.Id.ToString(CultureInfo.InvariantCulture);
        field.DisplayedValue = country.EnglishName;
    }

    // Legacy IsCountryIsrael: compare against the CountryIsrael config parameter.
    private async Task<bool> IsCountryIsrael(int countryId)
    {
        var israelId = await parametersUtil.Get<int>("CountryIsrael");
        return countryId == israelId;
    }
}
