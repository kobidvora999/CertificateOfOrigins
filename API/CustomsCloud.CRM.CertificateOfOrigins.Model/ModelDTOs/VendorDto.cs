namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// Minimal projection of the Vendors microservice vendor DTO — only the fields this service consumes
// (name enrichment). Extra fields on the wire are ignored on deserialization; expand when a later method
// needs more.
public class VendorDto
{
    public int Id { get; set; }
    public string? Name { get; set; }
}
