namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// Projection of the Users microservice user DTO — the fields this service consumes. Used to enrich certificate
// milestone rows with the acting user's display name (the SP returns only the user id). Extra wire fields ignored.
public class UserDto
{
    public int Id { get; set; }
    public string? Name { get; set; }
}
