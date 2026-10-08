namespace CertificateOfOrigins.Model.ModelDTOs;

// Projection of the Users microservice user DTO — the fields this service consumes. Used to enrich certificate
// milestone rows with the acting user's display name (the SP returns only the user id). Extra wire fields ignored.
public class UserDto
{
    public int Id { get; set; }

    // The user's display name. Legacy read Infrastructure.UserMng_User.Title
    // (usp_CertificateOfOrigins_GetCertificateOfOriginByID: `IIF(status = 8, UA.Title, U.Title) as UserName`); the milestone
    // keeps the UserName alias. TODO(blocking): confirm the field name exposed by the Users microservice (User/UsersByIds) -
    // named after the legacy column until that contract is published (parity A-24).
    public string? Title { get; set; }

    // The user's organization unit — used as the current user's OrganizationUnitID when saving certificate
    // attachments. TODO(blocking): confirm the field name exposed by the Users microservice (User/UsersByIds).
    public int OrganizationUnit { get; set; }
}
