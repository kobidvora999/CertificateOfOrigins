namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// A decision lookup record (CRM.CertificateOfOrigins_enum_Decision) returned with the authentication request.
public class CertificateOfOriginsDecisionDto
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public int State { get; set; }

    public string? Description { get; set; }

    public string? EnglishName { get; set; }

    public string? Enumeration { get; set; }

    public DateTimeOffset? StartDate { get; set; }
}
