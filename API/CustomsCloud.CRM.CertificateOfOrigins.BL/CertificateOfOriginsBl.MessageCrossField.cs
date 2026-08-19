using CustomsCloud.CRM.CertificateOfOrigins.BL.Proxies;
using CustomsCloud.CRM.CertificateOfOrigins.Model.CertificateOfOriginsDb;
using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;
using System.Globalization;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL;

// GetPC_MSG2280_2281 message field-validation engine (create branch) — stage 4a: the cross-field rules (legacy
// CheckFields → CheckConditionalFields + CheckFieldForSpecificCertificate), run after the per-field pass and only when
// the message produced details. Faithful port; the CustomsHouse rule also resolves the org-unit side-value the save
// consumes. The invoice/item conversion (stage 4b) is a separate unit — it needs the invoice/item entities + the save
// chain extended and two more SystemTables lookups.
public partial class CertificateOfOriginsBl
{
    // Legacy CheckFields: the two cross-field passes. Runs only for the create/update reasons (the caller gates it on a
    // non-empty details list, i.e. not GetRequestStatus/CertificateCancellation).
    private async Task CheckMessageCrossFields(CertificateOfOriginMessageDto? certificate, NonManipulationCertificateMessageDto? nonManipulation, CertificateOfOriginAgentRequestDto agentRequest, List<CertificateOfOriginDetails> details, bool isZipcodeMandatory, MessageValidationContext context)
    {
        await CheckConditionalFields(certificate, nonManipulation, agentRequest, details, isZipcodeMandatory, context);
        await CheckFieldForSpecificCertificate(certificate, nonManipulation, agentRequest, details, context);
    }

    // Legacy CheckConditionalFields: rules that fire only when a given field was present in this certificate type. The
    // standard CertificateOfOrigin body may be absent for a NonManipulation request, so every rule that reads it is
    // reachable only when its detail exists (which implies a standard body) — mirrored by the certificate-not-null guards
    // (legacy dereferenced request.Content.CertificateOfOrigin only inside those same conditions / the != null guard).
    private async Task CheckConditionalFields(CertificateOfOriginMessageDto? certificate, NonManipulationCertificateMessageDto? nonManipulation, CertificateOfOriginAgentRequestDto agentRequest, List<CertificateOfOriginDetails> details, bool isZipcodeMandatory, MessageValidationContext context)
    {
        var hasImportDate = HasDetail(details, ECertificateDetailsType.ImportDate);
        var hasIsCumulation = HasDetail(details, ECertificateDetailsType.IsCumulation);
        var hasConsigneeCountry = HasDetail(details, ECertificateDetailsType.ConsigneeCountry);
        var hasOriginCountry = HasDetail(details, ECertificateDetailsType.OriginCountry);
        var hasCustomsHouse = HasDetail(details, ECertificateDetailsType.CustomsHouse);

        if (hasImportDate)
        {
            CheckImportDate(agentRequest, nonManipulation, context);
        }

        if (hasIsCumulation && certificate is not null)
        {
            CheckCumulationCountry(certificate, context);
        }

        if (hasConsigneeCountry && certificate is not null)
        {
            CheckConsigneeCountry(certificate, context);
        }

        if (hasOriginCountry && certificate is not null)
        {
            await CheckPlaceOfManufactureAndZipcode(certificate, isZipcodeMandatory, context);
        }

        if (hasCustomsHouse)
        {
            // Legacy passed the CustomsHouse detail object (it rewrites the row's Value/DisplayedValue in place). The
            // detail already carries the raw external site number as its Value at this point.
            var customsHouseDetail = details.Find(d => d.CertificateDetailsTypeCodeId == (int)ECertificateDetailsType.CustomsHouse);
            if (customsHouseDetail is not null)
            {
                await CheckIfSiteExistAndCustomsHouse(customsHouseDetail, context);
            }
        }

        // Legacy (request.Content.CertificateOfOrigin != null && …): InSufficentworkingInd=true requires InsufficentWorkingText.
        if (certificate is not null && certificate.InSufficentworkingInd == true && string.IsNullOrEmpty(certificate.InsufficentWorkingText))
        {
            context.Exceptions.Add(BuildMessageException(EMessageCode.FieldMandatoryWhenTheAnotherField, "InsufficentWorkingText", "InSufficentworkingInd", "true"));
        }
    }

    // Legacy CheckImportDate: NonManipulation only — import date must be ≤ export date and ≥ export date − 1 year.
    private static void CheckImportDate(CertificateOfOriginAgentRequestDto agentRequest, NonManipulationCertificateMessageDto? nonManipulation, MessageValidationContext context)
    {
        if (agentRequest.CertificateOfOriginTypeCode != (int)ECertificateOfOriginType.NonManipulation || nonManipulation is null)
        {
            return;
        }

        var importDate = nonManipulation.ImportDate;
        var exportDate = nonManipulation.ExportDate;
        if (importDate > exportDate || importDate < exportDate.AddYears(-1))
        {
            context.Exceptions.Add(BuildMessageException(EMessageCode.ImportDatetIllegal));
        }
    }

    // Legacy CheckConsigneeCountry: consignee name + address present → consignee country required.
    private static void CheckConsigneeCountry(CertificateOfOriginMessageDto certificate, MessageValidationContext context)
    {
        if (!string.IsNullOrWhiteSpace(certificate.ConsigneeName)
            && !string.IsNullOrWhiteSpace(certificate.ConsigneeAddress)
            && string.IsNullOrWhiteSpace(certificate.ConsigneeCountry))
        {
            context.Exceptions.Add(BuildMessageException(EMessageCode.ImporterCountryRequired));
        }
    }

    // Legacy CheckCumulationCountry: when IsCumulation, exactly one of cumulation country / cumulation group is required.
    private static void CheckCumulationCountry(CertificateOfOriginMessageDto certificate, MessageValidationContext context)
    {
        if (certificate.IsCumulation != true)
        {
            return;
        }

        var hasCountry = !string.IsNullOrWhiteSpace(certificate.CumulationCountry);
        var hasGroup = certificate.CumulationGroupOfCountries.HasValue;
        if (!hasCountry && !hasGroup)
        {
            context.Exceptions.Add(BuildMessageException(EMessageCode.CumulationCountryRequired));
        }
        else if (hasCountry && hasGroup)
        {
            context.Exceptions.Add(BuildMessageException(EMessageCode.CountryAndCountryGroup));
        }
    }

    // Legacy CheckPlaceOfManufactureAndZipcode: for an Israel origin country, place-of-manufacture + zip are mandatory
    // (unless the destination is in the exempt list) and the zip must be at least 7 characters.
    private async Task CheckPlaceOfManufactureAndZipcode(CertificateOfOriginMessageDto certificate, bool isZipcodeMandatory, MessageValidationContext context)
    {
        var countryProxy = Resolve<ICountryProxy>();
        if (string.IsNullOrWhiteSpace(certificate.OriginCountry))
        {
            return;
        }

        var originCountry = await countryProxy.GetCountriesByAlphaCodes([certificate.OriginCountry]);
        var originCountryId = originCountry?.FirstOrDefault()?.Id ?? 0;

        var destinationCountryId = 0;
        if (!string.IsNullOrWhiteSpace(certificate.DestinationCountry))
        {
            var destinationCountry = await countryProxy.GetCountriesByAlphaCodes([certificate.DestinationCountry]);
            destinationCountryId = destinationCountry?.FirstOrDefault()?.Id ?? 0;
        }

        var exemptCsv = await parametersUtil.Get<string>("CountriesExemptedFromSendingThePlaceOfManufacture") ?? string.Empty;
        var exemptCountries = exemptCsv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (await IsCountryIsrael(originCountryId)
            && (!certificate.PlaceOfManufacture.HasValue || !certificate.ZipCodeOfManufacture.HasValue)
            && isZipcodeMandatory
            && !exemptCountries.Contains(destinationCountryId.ToString(CultureInfo.InvariantCulture)))
        {
            context.Exceptions.Add(BuildMessageException(EMessageCode.PlaceOfManufactureAndZipcodeRequired));
        }

        if (certificate.ZipCodeOfManufacture.HasValue && certificate.ZipCodeOfManufacture.Value.ToString(CultureInfo.InvariantCulture).Length < 7)
        {
            context.Exceptions.Add(BuildMessageException(EMessageCode.TheNumOfCharactersInTheZipcodeIsLessThan7));
        }
    }

    // Legacy CheckIfSiteExistAndCustomsHouse + CheckIfCustomsHouse: the CustomsHouse external site number resolves to a
    // site → org unit that must be a customs house. On success the detail row is REWRITTEN to the internal org-unit id
    // (Value) + English name (DisplayedValue) — the read path re-parses the CustomsHouse Value as the org-unit id — and
    // the org-unit side-value the save consumes is recorded.
    private async Task CheckIfSiteExistAndCustomsHouse(CertificateOfOriginDetails customsHouseDetail, MessageValidationContext context)
    {
        var siteProxy = Resolve<ISiteProxy>();
        var organizationUnitProxy = Resolve<IOrganizationUnitProxy>();
        var customsHouseExternalNumber = customsHouseDetail.Value;
        if (string.IsNullOrWhiteSpace(customsHouseExternalNumber))
        {
            context.Exceptions.Add(BuildMessageException(EMessageCode.MandatoryNullValue, "CustomsHouse"));
            return;
        }

        var sites = await siteProxy.GetSitesByExternalNumbers([customsHouseExternalNumber]);
        var organizationUnitId = sites?.FirstOrDefault()?.OrganizationUnitId;
        if (!organizationUnitId.HasValue)
        {
            return;
        }

        var isCustomsHouse = await organizationUnitProxy.IsOrganizationUnitCustomsHouse(organizationUnitId.Value);
        if (!isCustomsHouse)
        {
            context.Exceptions.Add(BuildMessageException(EMessageCode.CustomsHouseNotInTable));
        }
        else
        {
            // Legacy CheckIfCustomsHouse: detail.Value = orgUnit id, detail.DisplayedValue = orgUnit English name.
            var organizationUnit = await lookupUtil.Get<Lookup.OrganizationUnit>(organizationUnitId.Value);
            customsHouseDetail.Value = organizationUnitId.Value.ToString(CultureInfo.InvariantCulture);
            customsHouseDetail.DisplayedValue = organizationUnit?.EnglishName;
            context.OrganizationUnitId = organizationUnitId.Value;
        }
    }

    // Legacy CheckFieldForSpecificCertificate: per-certificate-type cross-field rules.
    private async Task CheckFieldForSpecificCertificate(CertificateOfOriginMessageDto? certificate, NonManipulationCertificateMessageDto? nonManipulation, CertificateOfOriginAgentRequestDto agentRequest, List<CertificateOfOriginDetails> details, MessageValidationContext context)
    {
        switch (agentRequest.CertificateOfOriginTypeCode)
        {
            case (int)ECertificateOfOriginType.EURMED:
            case (int)ECertificateOfOriginType.EUR1:
                // Only reached for EUR types, whose gate guarantees a standard body.
                if (certificate is not null)
                {
                    await CheckCountriesForEurCertificates(certificate, agentRequest, details, context);
                }

                break;

            case (int)ECertificateOfOriginType.NonManipulation:
                CheckManifestNumber(nonManipulation, agentRequest, context);
                break;

            default:
                if (certificate is not null)
                {
                    await CheckDestinationCountry(certificate, agentRequest, details, context);
                }

                break;
        }
    }

    // Legacy CheckCountriesForEurCertificates: EUR1/EURMED require exactly one of (country / country-group) for the
    // second-agreement, origin, and destination, then the destination trade-agreement check.
    private async Task CheckCountriesForEurCertificates(CertificateOfOriginMessageDto certificate, CertificateOfOriginAgentRequestDto agentRequest, List<CertificateOfOriginDetails> details, MessageValidationContext context)
    {
        CheckCountryXorGroup(!string.IsNullOrWhiteSpace(certificate.TradeAgreementCountry2), certificate.TradeAgreementGroupOfCountries.HasValue, EMessageCode.SecondCountryRequired, context);
        CheckCountryXorGroup(!string.IsNullOrWhiteSpace(certificate.OriginCountry), certificate.OriginGroupOfCountries.HasValue, EMessageCode.OriginCountryRequired, context);
        CheckCountryXorGroup(!string.IsNullOrWhiteSpace(certificate.DestinationCountry), certificate.DestinationGroupOfCountries.HasValue, EMessageCode.DestinationCountryRequired, context);

        if (!string.IsNullOrWhiteSpace(certificate.DestinationCountry))
        {
            await CheckIfDestinationCountryInAgreement(certificate.DestinationCountry, agentRequest.CertificateOfOriginTypeCode, details, context);
        }
    }

    // Legacy: for each (country, group) pair — missing both → required; both present → CountryAndCountryGroup.
    private static void CheckCountryXorGroup(bool hasCountry, bool hasGroup, EMessageCode requiredCode, MessageValidationContext context)
    {
        if (!hasCountry && !hasGroup)
        {
            context.Exceptions.Add(BuildMessageException(requiredCode));
        }
        else if (hasCountry && hasGroup)
        {
            context.Exceptions.Add(BuildMessageException(EMessageCode.CountryAndCountryGroup));
        }
    }

    // Legacy CheckDestinationCountry: non-EUR / non-NonManipulation certs require a destination country + its agreement.
    private async Task CheckDestinationCountry(CertificateOfOriginMessageDto certificate, CertificateOfOriginAgentRequestDto agentRequest, List<CertificateOfOriginDetails> details, MessageValidationContext context)
    {
        if (string.IsNullOrWhiteSpace(certificate.DestinationCountry))
        {
            context.Exceptions.Add(BuildMessageException(EMessageCode.DestinationCountryRequired));
        }
        else
        {
            await CheckIfDestinationCountryInAgreement(certificate.DestinationCountry, agentRequest.CertificateOfOriginTypeCode, details, context);
        }
    }

    // Legacy CheckIfDestinationCountryInAgreement: resolve the destination country (side-value the save consumes as
    // CertificateOfOrigin.DestinationCountry) and verify it is in the trade agreement; on success set the display name
    // on the destination detail.
    private async Task CheckIfDestinationCountryInAgreement(string destinationCountry, int certificateTypeId, List<CertificateOfOriginDetails> details, MessageValidationContext context)
    {
        var customsBookProxy = Resolve<ICustomsBookProxy>();
        var country = await ResolveCountry(destinationCountry, context);
        if (country is null)
        {
            return;
        }

        context.DestinationCountryId = country.Id;
        var isInTrade = await customsBookProxy.IsTradeAgreementForCountry(certificateTypeId, country.Id, false);
        if (!isInTrade)
        {
            context.Exceptions.Add(BuildMessageException(EMessageCode.DestinationCountryNotInAgreement));
        }
        else
        {
            var destinationDetail = details.FirstOrDefault(d => d.CertificateDetailsTypeCodeId == (int)ECertificateDetailsType.DestinationCountry);
            if (destinationDetail is not null)
            {
                destinationDetail.DisplayedValue = country.EnglishName;
            }
        }
    }

    // Legacy CheckManifestNumber: NonManipulation requires a manifest number or an export declaration number, and always
    // requires the export declaration number.
    private static void CheckManifestNumber(NonManipulationCertificateMessageDto? nonManipulation, CertificateOfOriginAgentRequestDto agentRequest, MessageValidationContext context)
    {
        var manifestNum = nonManipulation?.ManifestNum;
        var exportDeclarationNum = agentRequest.ExportDeclarationNum;

        if (string.IsNullOrWhiteSpace(manifestNum) && string.IsNullOrWhiteSpace(exportDeclarationNum))
        {
            context.Exceptions.Add(BuildMessageException(EMessageCode.ManifestIdMissing));
        }

        if (string.IsNullOrWhiteSpace(exportDeclarationNum))
        {
            context.Exceptions.Add(BuildMessageException(EMessageCode.ExportDeclarationMissing));
        }
    }

    private static bool HasDetail(List<CertificateOfOriginDetails> details, ECertificateDetailsType detailType)
    {
        return details.Exists(d => d.CertificateDetailsTypeCodeId == (int)detailType);
    }
}
