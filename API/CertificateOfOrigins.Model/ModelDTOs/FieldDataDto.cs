namespace CertificateOfOrigins.Model.ModelDTOs;

// A single label/value line of the web-query response (legacy FieldDataDTO). Value is loosely typed (string or
// the issuing date as a "yyyy-MM-dd" string) as in the legacy contract, so it serializes as-is.
public class FieldDataDto
{
    public string? Label { get; set; }

    public object? Value { get; set; }
}
