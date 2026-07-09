namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// Template-generation request data: Name = template name in the Templates module,
// Data = serialized JSON payload for the template, Format = output format (e.g. "pdf"/"docx").
public class PrintTemplateDto
{
    public string Name { get; set; } = string.Empty;

    public string Data { get; set; } = string.Empty;

    public string Format { get; set; } = string.Empty;
}
