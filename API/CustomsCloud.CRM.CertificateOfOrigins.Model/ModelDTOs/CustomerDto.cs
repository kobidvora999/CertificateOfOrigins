namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// Minimal projection of the Customers microservice customer DTO — only the fields this service consumes
// (name/external-id enrichment). Extra fields on the wire are ignored on deserialization; expand when a
// later method (e.g. GetCustomerInformation) needs more.
public class CustomerDto
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? ExternalIdNum { get; set; }
    public bool IsActive { get; set; }
}
