using CertificateOfOrigins.DAL;
using CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore.BL.Exceptions;
using CustomsCloud.InfrastructureCore.Utils.Templates;
using System.Text.Json;

namespace CertificateOfOrigins.BL;

// CR 194221 — rendering documents through the Templates microservice.
//
// The flow is the one the shared template-print pattern prescribes: the caller names a template and the entity it is
// rendered for; this service fetches that template's data itself (dbo.GetTemplateData), serializes it as camelCase
// JSON, and hands it to Templates, which loads "{Name}.docx" + "{Name}.yml" and merges by the YAML field paths.
//
// Adding a template = one case in GetTemplateMeta + a Result DTO + a branch in dbo.GetTemplateData. Nothing else here
// changes — GetTemplateData and GenerateTemplate are template-agnostic.
public partial class CertificateOfOriginsBl
{
    // The template YAML field paths are camelCase JSONPaths ($.fileNo, $.letterDate, ...).
    private static readonly JsonSerializerOptions TemplateJsonOptions =
        new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    // The rendered document. Missing entity / unregistered template → 404 (GetTemplateData throws).
    public async Task<Stream> GenerateTemplate(int templateId, int entityId)
    {
        var templateUtil = Resolve<ITemplateUtil>();
        var printTemplate = await GetTemplateData(templateId, entityId);

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
    public async Task<PrintTemplateDto> GetTemplateData(int templateId, int entityId)
    {
        var (dataType, name, format) = GetTemplateMeta(templateId);

        // T is only known at run time (it comes from the template id), so the generic DAL method is invoked by
        // reflection — the same approach the shared pattern uses.
        var method = typeof(ICertificateOfOriginsDal)
            .GetMethod(nameof(ICertificateOfOriginsDal.GetTemplateData))!
            .MakeGenericMethod(dataType);
        var task = (Task)method.Invoke(DataLayer, [templateId, entityId])!;
        await task.ConfigureAwait(false);
        var data = ((dynamic)task).Result as object
            ?? throw new RestNotFoundException();

        await EnrichTemplateData(data);

        var json = JsonSerializer.Serialize(data, dataType, TemplateJsonOptions);
        return new PrintTemplateDto { Name = name, Data = json, Format = format };
    }

    // Cross-service names. The procedure returns ids only — a service-owned procedure never joins another service's
    // tables — so the names are resolved here, the same split the search reads use.
    private async Task EnrichTemplateData(object data)
    {
        if (data is SouthKoreaOriginVerificationLetterResult letter && letter.RequestCountryId is > 0)
        {
            var country = await lookupUtil.Get<Lookup.Country>(letter.RequestCountryId.Value);

            // The letter is in English and addressed abroad, so it wants the English country name.
            letter.ExternalCustomDepartmentCountry = country?.EnglishName ?? country?.Name;
        }
    }

    // Template id → (data contract, the name registered in the Templates module, output format).
    //
    // The name is a lookup key, not a label: Templates loads "{Name}.docx" + "{Name}.yml" by it, so a mismatch fails
    // at render time with "template not found".
    //
    // TODO(internal): the South-Korea letter's .docx + .yml do not exist yet (new in CR 194221, no legacy source). To
    // be produced and uploaded to the MinIO "templates" bucket in the internal environment (INTERNAL_INTEGRATION.md).
    // Until then the render answers "template not found".
    private static (Type DataType, string Name, Format Format) GetTemplateMeta(int templateId)
    {
        return templateId switch
        {
            (int)ECertificateOfOriginsTemplate.SouthKoreaOriginVerificationLetter => (
                typeof(SouthKoreaOriginVerificationLetterResult),
                "ExportAuthenticationSendDeliveryForExporterSouthKoreaTemplate",
                Format.Pdf),

            // An id this service does not render is a bad request, not a missing resource — and keeping the two
            // distinct is what lets a test tell "this template is not registered" apart from "that entity has no
            // data", which a shared 404 cannot.
            _ => throw new RestValidationException($"Unsupported templateId {templateId}."),
        };
    }
}
