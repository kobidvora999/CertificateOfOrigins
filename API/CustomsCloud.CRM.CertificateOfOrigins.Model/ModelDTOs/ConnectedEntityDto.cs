namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// Input of the ESB/EAI Convert operation — a generic cross-service entity reference. For a certificate of
// origin the lookup key is EntityIdKey1 (the certificate number); the other fields are part of the shared
// contract but not consumed by this conversion.
public class ConnectedEntityDto
{
    public string? EntityIdKey1 { get; set; }
    public string? EntityIdKey2 { get; set; }
    public string? EntityIdKey3 { get; set; }
    public string? EntityPath { get; set; }
    public int EntityType { get; set; }
    public string? EntityIdExternalReferenceId { get; set; }
}
