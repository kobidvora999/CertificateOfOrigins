namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// Output of the ESB/EAI Convert operation — a generic cross-service "entity link". The legacy VirtualEntity has
// ~20 fields but Convert populated only these four for a certificate of origin (the rest stayed default), so this
// is a purpose-built projection. EntityType carries the numeric EEntityType value (CertificateOfOrigin = 12319).
public class VirtualEntityDto
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public int EntityType { get; set; }
    public int CustomerId { get; set; }
}
