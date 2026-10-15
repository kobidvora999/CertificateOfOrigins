namespace CertificateOfOrigins.Model.ModelDTOs;

// Projection of a user's letter signature block from the Users microservice. Legacy read it in every letter data
// procedure from Infrastructure.UserMng_UserTemplateDetails (English name + signature image), joined to
// Infrastructure.UserMng_User (email). Those are user-management tables, so here they come over the Users proxy.
// Extra wire fields are ignored.
public class UserTemplateDetailsDto
{
    public int UserId { get; set; }

    // UserMng_UserTemplateDetails.EnglishFirstName / EnglishLastName — the letters print them as
    // CONCAT(EnglishFirstName, ' ', EnglishLastName).
    public string? EnglishFirstName { get; set; }

    public string? EnglishLastName { get; set; }

    // UserMng_UserTemplateDetails.SignatureImageUrl — the scanned signature. Legacy merged this URL straight into
    // the <UserSignature> image field; the template engine loaded the picture from it.
    public string? SignatureImageUrl { get; set; }

    // UserMng_User.Email.
    public string? Email { get; set; }
}
