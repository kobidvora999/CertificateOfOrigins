using CustomsCloud.InfrastructureCore.Utils.Templates;

namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// A ready-to-render template request produced by the generic GetTemplateData: the template name (the {Name}.docx /
// {Name}.yml the Templates module loads), the merged data as a camelCase JSON string, and the output format.
public class PrintTemplateDto
{
    public string Name { get; set; } = string.Empty;

    // The data-contract serialized as camelCase JSON (null fields omitted) — merged by the Templates module.
    public string Data { get; set; } = string.Empty;

    public Format Format { get; set; }
}
