using CustomsCloud.InfrastructureCore.Utils.Templates;

namespace CertificateOfOrigins.Model.ModelDTOs;

// The payload the Templates microservice renders: it loads "{Name}.docx" + "{Name}.yml", parses Data as JSON and
// merges it by the field paths declared in the YAML. Name is therefore a lookup key, not a label — it must match
// the template registered in the Templates module exactly.
public class PrintTemplateDto
{
    public string Name { get; set; } = string.Empty;

    // The template data, serialized as camelCase JSON (the YAML field paths are camelCase JSONPaths).
    public string Data { get; set; } = string.Empty;

    // Output format (InfrastructureCore.Utils.Templates.Format).
    public Format Format { get; set; } = Format.Pdf;
}
