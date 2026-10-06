namespace CertificateOfOrigins.Model.ModelDTOs;

// An authentication-file status lookup record (CRM.CertificateOfOrigins_enum_AuthenticationFileStatus) returned
// with the file (file.FileStatuses).
public class AuthenticationFileStatusDto
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public int State { get; set; }

    public string? Description { get; set; }

    public string? EnglishName { get; set; }

    public string? Enumeration { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public bool IsAutomatic { get; set; }
}
