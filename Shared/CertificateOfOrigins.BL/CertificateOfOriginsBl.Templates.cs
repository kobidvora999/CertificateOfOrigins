using CertificateOfOrigins.BL.Proxies;
using CertificateOfOrigins.DAL;
using CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore.BL.Exceptions;
using CustomsCloud.InfrastructureCore.Utils.Templates;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CertificateOfOrigins.BL;

// CR 194221 — rendering documents through the Templates microservice.
//
// The flow is the one the shared template-print pattern prescribes: the caller names a template and the entity it is
// rendered for; this service fetches that template's data itself (dbo.GetTemplateData), serializes it as camelCase
// JSON, and hands it to Templates, which loads "{Name}.docx" + "{Name}.yml" and merges by the YAML field paths.
//
// Adding a template = one case in GetTemplateMeta + a Result DTO + a branch in dbo.GetTemplateData (+ its enrichment
// below when it prints another service's data). GetTemplateData and GenerateTemplate are template-agnostic.
public partial class CertificateOfOriginsBl
{
    // The template YAML field paths are camelCase JSONPaths ($.fileNo, $.letterDate, ...). Null fields are left out, so a
    // typed field (value type DateTime / int) sees a missing token and renders empty instead of failing on a JSON null.
    private static readonly JsonSerializerOptions TemplateJsonOptions =
        new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };

    // The rendered document. Missing entity / unregistered template → 404 (GetTemplateData throws).
    public async Task<Stream> GenerateTemplate(int templateId, int entityId, string? additionalInfo, int? createUserId = null, string? freeText = null)
    {
        var templateUtil = Resolve<ITemplateUtil>();
        var printTemplate = await GetTemplateData(templateId, entityId, additionalInfo, createUserId, freeText);

        var templateRequest = templateUtil.CreateRequestBuilder()
            .WithName(printTemplate.Name)
            .WithJsonData(printTemplate.Data)
            .WithFormat(printTemplate.Format)
            .Build();

        var template = await templateUtil.GenerateTemplate(templateRequest);

        // Copy to a seekable stream the controller can hand to FileStreamResult (the proxy stream is forward-only).
        var memoryStream = new MemoryStream();
        await template.CopyToAsync(memoryStream);
        memoryStream.Position = 0;
        return memoryStream;
    }

    // The Templates payload for a template + entity: the registered template name, its data as JSON, and the output
    // format. Only GenerateTemplate calls it today (there is deliberately no endpoint of its own — no consumer asked
    // for the un-rendered data); it stays public per the shared template-print pattern so a future caller needs no
    // signature change.
    //
    // freeText is the user's free text for a template that allows it — legacy TemplateArgument.FreeText, which the
    // template merger put in <FreeText> (MergerOfTemplateArguments.AddFreeText).
    public async Task<PrintTemplateDto> GetTemplateData(int templateId, int entityId, string? additionalInfo, int? createUserId = null, string? freeText = null)
    {
        var (dataType, name, format, isFreeTextAllowed) = GetTemplateMeta(templateId);

        // Legacy Object Maker passed the producing user as @CreateUserID (the same user the letters sign as). A caller
        // without a request user (a worker, a queue consumer) passes it explicitly; otherwise it is the request user.
        var userId = createUserId ?? RequestMetadata.UserId ?? 0;

        // T is only known at run time (it comes from the template id), so the generic DAL method is invoked by
        // reflection — the same approach the shared pattern uses.
        var method = typeof(ICertificateOfOriginsDal)
            .GetMethod(nameof(ICertificateOfOriginsDal.GetTemplateData))!
            .MakeGenericMethod(dataType);
        var task = (Task)method.Invoke(DataLayer, [templateId, entityId, userId, additionalInfo])!;
        await task.ConfigureAwait(false);
        var data = ((dynamic)task).Result as object
            ?? throw new RestNotFoundException();

        await EnrichTemplateData(templateId, data, userId);

        if (isFreeTextAllowed)
        {
            SetFreeText(data, freeText);
        }

        var json = JsonSerializer.Serialize(data, dataType, TemplateJsonOptions);
        return new PrintTemplateDto { Name = name, Data = json, Format = format };
    }

    // Cross-service data. The procedure returns ids only — a service-owned procedure never joins another service's
    // tables — so names, addresses and the signature block are resolved here, the same split the search reads use.
    private async Task EnrichTemplateData(int templateId, object data, int createUserId)
    {
        switch (data)
        {
            case SouthKoreaOriginVerificationLetterResult letter:
                await EnrichSouthKoreaLetter(letter, createUserId);
                break;
            case ImportVerificationLetterResult letter:
                await EnrichImportVerificationLetter(letter, createUserId);
                break;
            case ExportVerificationLetterResult letter:
                await EnrichExportVerificationLetter(templateId, letter, createUserId);
                break;
            case ImporterAlertLetterResult letter:
                await EnrichImporterAlertLetter(letter, createUserId);
                break;
        }
    }

    private async Task EnrichSouthKoreaLetter(SouthKoreaOriginVerificationLetterResult letter, int createUserId)
    {
        if (letter.RequestCountryId is > 0)
        {
            var country = await lookupUtil.Get<Lookup.Country>(letter.RequestCountryId.Value);

            // The letter is in English and addressed abroad, so it wants the English country name.
            letter.ExternalCustomDepartmentCountry = country?.EnglishName ?? country?.Name;
        }

        await FillSignatureBlock(letter, createUserId);
    }

    // Legacy signed the import-verification letters as the user producing them (@CreateUserID). UserTemplateDetails
    // was INNER JOINed to the user, so without that row the whole block stayed empty, the email included.
    private async Task FillSignatureBlock(SouthKoreaOriginVerificationLetterResult letter, int createUserId)
    {
        var details = await Resolve<IUserProxy>().GetUserTemplateDetails(createUserId);
        if (details is null)
        {
            return;
        }

        letter.EmployeeFullName = LegacyEnglishFullName(details);
        letter.EmployeeEmailAddress = details.Email;
        letter.UserSignature = details.SignatureImageUrl;
    }

    // 2251-2259 (usp_Template_INNER_CROSS_CertificateOfOrigin, CR126557): the supplier block of the file's vendor and the
    // signature block of the user producing the letter.
    private async Task EnrichImportVerificationLetter(ImportVerificationLetterResult letter, int createUserId)
    {
        if (letter.VendorId is int vendorId)
        {
            var vendor = (await Resolve<IVendorProxy>().GetVendorsByIds([vendorId]))?.FirstOrDefault();
            var address = vendor?.VendorAddress?.FirstOrDefault();

            // Legacy INNER JOINed the vendor to its address (#VendorDetails), so a vendor without one left the whole
            // supplier block empty, the name included.
            if (address is not null)
            {
                letter.SupplierTitle = vendor!.Title;
                letter.SupplierAddress = LegacyEnglishAddress(address.EnglishMainAddressLine, address.EnglishCityName, address.EnglishPostalCode);
                letter.SupplierCountry = await EnglishCountryName(address.EnglishCountryId);
            }
        }

        var department = await GetForeignCustomsDepartmentAddress(letter.CustomerId, Resolve<ICustomerProxy>());
        if (department is not null)
        {
            letter.ExternalCustomDepartmentTitle = department.Title;

            // This procedure composed the address differently from the vendor's: CONCAT(line, '\n' + city, ', ' + postal),
            // where a NULL part drops its separator with it, and without the NVARCHAR(255) cast.
            letter.ExternalCustomDepartmentAddress = department.EnglishMainAddressLine
                + (department.EnglishCityName is null ? null : "\n" + department.EnglishCityName)
                + (department.EnglishPostalCode is null ? null : ", " + department.EnglishPostalCode);
            letter.ExternalCustomDepartmentCountry = await EnglishCountryName(department.EnglishCountryId);
        }

        var details = await Resolve<IUserProxy>().GetUserTemplateDetails(createUserId);
        if (details is not null)
        {
            letter.EmployeeFullName = LegacyEnglishFullName(details);
            letter.EmployeeEmailAddress = details.Email;
            letter.UserSignature = details.SignatureImageUrl;
        }
    }

    // 2289-2295 (usp_Template_INNER_CROSS_ExportDocumentAuthenticationRequestLetter): the country, the foreign customs
    // department, the exporter and the signature block.
    private async Task EnrichExportVerificationLetter(int templateId, ExportVerificationLetterResult letter, int createUserId)
    {
        if (letter.CountryId is int countryId)
        {
            var country = await lookupUtil.Get<Lookup.Country>(countryId);
            letter.ExportCountryName = country?.Name;
            letter.CountryEnglishName = country?.EnglishName;
        }

        var customerProxy = Resolve<ICustomerProxy>();
        var department = await GetForeignCustomsDepartmentAddress(letter.CustomerId, customerProxy);
        if (department is not null)
        {
            letter.ExternalCustomDepartmentTitle = department.Title;
            letter.ExternalCustomDepartmentAddress = LegacyEnglishAddress(department.EnglishMainAddressLine, department.EnglishCityName, department.EnglishPostalCode);
            letter.ExternalCustomDepartmentCountry = await EnglishCountryName(department.EnglishCountryId);
        }

        if (letter.ExporterCustomerId is int exporterId)
        {
            var exporter = (await customerProxy.GetCustomersByIds([exporterId]))?.FirstOrDefault();
            if (exporter is not null)
            {
                letter.ExporterCustomer = exporter.Title;

                // IIF(CONCAT(EnglishFirstName, ' ', EnglishLastName) = ' ', EnglishCorporationName, <that name>): a blank
                // personal name means a corporation. T-SQL '=' ignores trailing blanks, so any all-blank name matches.
                var personalName = Truncate($"{exporter.EnglishFirstName} {exporter.EnglishLastName}", LegacyNameLength);
                letter.ExporterName = personalName.TrimEnd(' ').Length == 0 ? exporter.EnglishCorporationName : personalName;
            }
        }

        await FillExportSignatureBlock(templateId, letter, createUserId);
    }

    // #CustomerDetails (both legacy procedures): the customer, provided it is a foreign customs department (activity 40),
    // addressed by its first address (CROSS APPLY TOP 1). Null when it is not one or has no address — the export procedure
    // INNER JOINed that to the letter row (no row at all), the import one LEFT JOINed it (an empty block); here the letter
    // row comes from this service's procedure, so in both only the department block stays empty.
    private static async Task<CustomerAddressDto?> GetForeignCustomsDepartmentAddress(int? customerId, ICustomerProxy customerProxy)
    {
        if (customerId is not int id)
        {
            return null;
        }

        var department = await customerProxy.GetCustomerInformation(id);
        var isForeignCustomsDepartment = department?.CustomerActivity?
            .Any(a => a.CustomerActivityTypeId == CertificateOfOriginsConsts.ForeignCustomsDepartmentActivityTypeId) == true;
        var result = isForeignCustomsDepartment ? department!.CustomerAddress?.FirstOrDefault() : null;
        return result;
    }

    // The two legacy branches differ only here. 2290 / 2293 / 2294 are signed by a fixed user, with the phone on the user
    // row; every other letter by the user producing it, with the user's customs phone (UserMng_ContactDetailsFromCustoms,
    // communication type 1), which was INNER JOINed — a user without one got no block at all. Either way the template
    // details were INNER JOINed too, and the phone printed ' ' when the block was missing.
    private async Task FillExportSignatureBlock(int templateId, ExportVerificationLetterResult letter, int createUserId)
    {
        var isFixedSigner = templateId is
            (int)ECertificateOfOriginsTemplate.ExportAuthenticationSendDeliveryForExporterMercosurTemplate
            or (int)ECertificateOfOriginsTemplate.ExportAuthenticationSendDeliveryForExporterEuropeTemplate
            or ExportAuthenticationSendReminderForExporterTemplateId;
        var signerId = isFixedSigner ? CertificateOfOriginsConsts.ExportExporterLettersSignerUserId : createUserId;

        var details = await Resolve<IUserProxy>().GetUserTemplateDetails(signerId);
        if (details is not null && (isFixedSigner || details.CustomsPhoneNumber is not null))
        {
            letter.CustomsEmployeeName = details.Title;
            letter.EmployeeFullName = LegacyEnglishFullName(details);
            letter.UserSignature = details.SignatureImageUrl;
            letter.UserSignature1 = details.SignatureImageUrl;
            letter.EmployeeEmailAddress = details.Email;
            letter.PhoneNumber = isFixedSigner ? details.PhoneNumber : details.CustomsPhoneNumber;
        }

        // ISNULL(UD.PhoneNumber, ' ').
        letter.PhoneNumber ??= " ";
    }

    // 2244 (usp_Template_INNER_CROSS_ImportAuthenticationRequestImporterLetter): the importer's name, the producing user's
    // title, the signature of the user who last updated the request, and the footer.
    private async Task EnrichImporterAlertLetter(ImporterAlertLetterResult letter, int createUserId)
    {
        if (letter.ImporterId is int importerId)
        {
            var importer = (await Resolve<ICustomerProxy>().GetCustomersByIds([importerId]))?.FirstOrDefault();
            letter.ImporterName = importer?.Title;
        }

        var userProxy = Resolve<IUserProxy>();

        // U.Title, INNER JOINed on @CreateUserID — UsersByIds returns it as Name.
        var producer = (await userProxy.GetUsersByIds([createUserId]))?.FirstOrDefault();
        letter.UserTitle = producer?.Name;

        // US.SignatureImageUrl, LEFT JOINed on IAR.UpdateUserID.
        var updater = await userProxy.GetUserTemplateDetails(letter.UpdateUserId);
        letter.UserSignature = updater?.SignatureImageUrl;

        // The merger's AddSignatureDetails always added the customs website (seeded by API_20261015190000).
        letter.CustomsWebsiteForSignature = await parametersUtil.Get<string>(CertificateOfOriginsConsts.CustomsWebsiteParameter);
    }

    // Legacy put the user's free text in <FreeText>, a single blank when it was empty (AddFreeText).
    private static void SetFreeText(object data, string? freeText)
    {
        if (data is IFreeTextTemplateData letter)
        {
            letter.FreeText = string.IsNullOrEmpty(freeText) ? " " : freeText;
        }
    }

    // CAST(CONCAT(EnglishFirstName, ' ', EnglishLastName) AS NVARCHAR(255)); CONCAT treats NULL as an empty string.
    private static string LegacyEnglishFullName(UserTemplateDetailsDto details)
    {
        var result = Truncate($"{details.EnglishFirstName} {details.EnglishLastName}", LegacyNameLength);
        return result;
    }

    // CAST(CONCAT(EnglishMainAddressLine, ' <newline>', EnglishCityName, ',', EnglishPostalCode) AS NVARCHAR(255)).
    private static string LegacyEnglishAddress(string? mainAddressLine, string? cityName, string? postalCode)
    {
        var result = Truncate($"{mainAddressLine} \n{cityName},{postalCode}", LegacyNameLength);
        return result;
    }

    private async Task<string?> EnglishCountryName(int? countryId)
    {
        if (countryId is not int id)
        {
            return null;
        }

        var country = await lookupUtil.Get<Lookup.Country>(id);
        return country?.EnglishName;
    }

    private static string Truncate(string value, int maxLength)
    {
        var result = value.Length > maxLength ? value[..maxLength] : value;
        return result;
    }

    // The legacy procedures cast the composed names and addresses to NVARCHAR(255).
    private const int LegacyNameLength = 255;

    // 2294 (the export alert letter) is signed like 2290 / 2293 but is not rendered yet — no .docx was supplied.
    private const int ExportAuthenticationSendReminderForExporterTemplateId = 2294;

    // Template id → (data contract, the name registered in the Templates module, output format, whether the template
    // takes the user's free text — legacy General_enum_Template.IsFreeTextAllowed).
    //
    // The name is a lookup key, not a label: Templates loads "{Name}.docx" + "{Name}.yml" by it, so a mismatch fails
    // at render time with "template not found". For the legacy letters it is the legacy Enumeration.
    //
    // TODO(internal): the .docx + .yml pairs in Templates/ are to be uploaded to the MinIO "templates" bucket in the
    // internal environment (INTERNAL_INTEGRATION.md). Until then the render answers "template not found".
    private static (Type DataType, string Name, Format Format, bool IsFreeTextAllowed) GetTemplateMeta(int templateId)
    {
        return (ECertificateOfOriginsTemplate)templateId switch
        {
            ECertificateOfOriginsTemplate.SouthKoreaOriginVerificationLetter => (
                typeof(SouthKoreaOriginVerificationLetterResult), "ExportAuthenticationSendDeliveryForExporterSouthKoreaTemplate", Format.Pdf, false),

            ECertificateOfOriginsTemplate.RequesrForVerificationEurope => (
                typeof(ImportVerificationLetterResult), "RequesrForVerificationEurope", Format.Pdf, true),
            ECertificateOfOriginsTemplate.RequesrForVerificationJordan => (
                typeof(ImportVerificationLetterResult), "RequesrForVerificationJordan", Format.Pdf, true),
            ECertificateOfOriginsTemplate.RequesrForVerificationMexico => (
                typeof(ImportVerificationLetterResult), "RequesrForVerificationMexico", Format.Pdf, true),
            ECertificateOfOriginsTemplate.RequesrForVerificationMercosur => (
                typeof(ImportVerificationLetterResult), "RequesrForVerificationMercosur", Format.Pdf, true),
            ECertificateOfOriginsTemplate.RequesrForVerificationCanada => (
                typeof(ImportVerificationLetterResult), "RequesrForVerificationCanada", Format.Pdf, true),
            ECertificateOfOriginsTemplate.RequesrForVerificationTurkey => (
                typeof(ImportVerificationLetterResult), "RequesrForVerificationTurkey", Format.Pdf, true),
            ECertificateOfOriginsTemplate.RequesrForVerificationGeneral => (
                typeof(ImportVerificationLetterResult), "RequesrForVerificationGeneral", Format.Pdf, false),
            ECertificateOfOriginsTemplate.RequesrForVerificationUsaTemplateNew => (
                typeof(ImportVerificationLetterResult), "RequesrForVerificationUSATemplate_New", Format.Pdf, true),
            ECertificateOfOriginsTemplate.RequesrForVerificationUsaTemplate => (
                typeof(ImportVerificationLetterResult), "RequesrForVerificationUSATemplate", Format.Pdf, true),

            ECertificateOfOriginsTemplate.ImportAuthenticationRequestImporterLetter => (
                typeof(ImporterAlertLetterResult), "ImportAuthenticationRequestImporterLetter", Format.Pdf, false),

            ECertificateOfOriginsTemplate.ExportAuthenticationRequestEurope => (
                typeof(ExportVerificationLetterResult), "ExportAuthenticationRequestEurope", Format.Pdf, true),
            ECertificateOfOriginsTemplate.ExportAuthenticationSendDeliveryForExporterMercosurTemplate => (
                typeof(ExportVerificationLetterResult), "ExportAuthenticationSendDeliveryForExporterMercosurTemplate", Format.Pdf, false),
            ECertificateOfOriginsTemplate.ExportAuthenticationSendDeliveryForExporterTurkeyJordanTemplate => (
                typeof(ExportVerificationLetterResult), "ExportAuthenticationSendDeliveryForExporterTurkeyJordanTemplate", Format.Pdf, false),
            ECertificateOfOriginsTemplate.ExportAuthenticationSendDeliveryForExporterEuropeTemplate => (
                typeof(ExportVerificationLetterResult), "ExportAuthenticationSendDeliveryForExporterEuropeTemplate", Format.Pdf, false),
            ECertificateOfOriginsTemplate.ExportAuthenticationRequestMercosur => (
                typeof(ExportVerificationLetterResult), "ExportAuthenticationRequestMercosur", Format.Pdf, true),

            // An id this service does not render is a bad request, not a missing resource — and keeping the two
            // distinct is what lets a test tell "this template is not registered" apart from "that entity has no
            // data", which a shared 404 cannot.
            _ => throw new RestValidationException(nameof(templateId), $"Unsupported templateId {templateId}."),
        };
    }
}
