using System.Text.Json.Serialization;

namespace CertificateOfOrigins.Model.ModelDTOs;

// A single label/value line of the web-query response (legacy FieldDataDTO). Value is loosely typed (string or
// the issuing date as a "yyyy-MM-dd" string) as in the legacy contract, so it serializes as-is.
// Legacy wire names (parity D-F1).
public class FieldDataDto
{
    [JsonPropertyName("Label")]
    public string? Label { get; set; }

    [JsonPropertyName("Value")]
    public object? Value { get; set; }
}
