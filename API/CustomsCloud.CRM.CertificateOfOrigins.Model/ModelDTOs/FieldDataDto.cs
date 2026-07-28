namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// A single label/value line of the web-query response (legacy FieldDataDTO). Value is loosely typed (string or
// a DateTime for the issuing date) exactly as the legacy contract, so it serializes as-is.
public class FieldDataDto
{
    public string? Label { get; set; }

    public object? Value { get; set; }
}
