namespace CertificateOfOrigins.Model.ModelDTOs;

// Projection of the Users microservice user DTO (UserResult) — the fields this service consumes. Used to enrich certificate
// milestone rows and the importer alert letter with the user's display name (the SP returns only the user id). Extra wire
// fields ignored.
public class UserDto
{
    public int Id { get; set; }

    // The user's display name — legacy Infrastructure.UserMng_User.Title (usp_CertificateOfOrigins_GetCertificateOfOriginByID:
    // `IIF(status = 8, UA.Title, U.Title) as UserName`). UsersByIds returns it as Name (UserResult: Id, UserName, Name, ... —
    // confirmed by the developer, 15.10.2026).
    public string? Name { get; set; }

    // The user's organization unit — used as the current user's OrganizationUnitID when saving certificate
    // attachments. TODO(blocking): confirm the field name exposed by the Users microservice (User/UsersByIds).
    public int OrganizationUnit { get; set; }
}
