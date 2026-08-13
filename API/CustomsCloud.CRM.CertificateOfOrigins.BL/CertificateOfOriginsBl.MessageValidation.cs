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
                // Legacy EMessages.MandatoryValue.
                context.Exceptions.Add(BuildFieldException($"שדה חובה חסר: {detailType}"));
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

    private static string? ToStringValue(DateTime value)
    {
        return value == default ? null : value.ToString("o", CultureInfo.InvariantCulture);
    }

    // Legacy: new InfException(EMessages.X, parameters, null). The EMessages code + localized/English text pipeline
    // (SystemTablesUtil.GetUIMessageWithEnglishAndLevel → ValidationMessages/resx) is still blocked repo-wide
    // (BaseValidationMessages — see Program.cs). TODO(migration): source ExceptionType (the EMessages code) + the
    // English text from ValidationMessages when it lands; the Hebrew text here comes from the UIMessage table.
    private static CertificateOfOriginExceptionDto BuildFieldException(string description)
    {
        return new CertificateOfOriginExceptionDto
        {
            ExceptionLevel = (int)EExceptionLevel.Error,
            ExceptionDescription = description,
            ExceptionType = 0,
        };
    }

    // Stage 3 (next unit): the per-field-type validators + code→id resolvers (CheckSpecificField's ~30-case switch —
    // country / trade-agreement / group / site / date / bool fields, resolving _exporterID / org-unit / etc.). Stubbed
    // here so the stage-2 loop compiles; a non-blank field currently passes through unvalidated until stage 3 lands.
    // TODO(blocking): implement the per-detail-type validation + resolution (see CertificateOfOriginsBl.MessageValidation
    // stage 3). Marked static only because the stub has no body yet; stage 3 makes it an instance method (uses proxies).
#pragma warning disable CA1822
    private Task CheckSpecificField(MessageField field, int certificateTypeId, int? destinationCountryId, MessageValidationContext context)
    {
        return Task.CompletedTask;
    }
#pragma warning restore CA1822
}
