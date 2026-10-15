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

    // UserMng_User.Title — the user's Hebrew display name (the export letters' <CustomsEmployeeName>).
    public string? Title { get; set; }

    // UserMng_User.PhoneNumber — the phone the export letters print for their fixed signer.
    public string? PhoneNumber { get; set; }

    // The user's customs phone: Infrastructure.UserMng_ContactDetailsFromCustoms.ContactDetails with CommunicationTypeID 1 —
    // the phone the other export letters print for the user producing them. Legacy INNER JOINed it, so a user without
    // one got no signature block at all on those letters.
    public string? CustomsPhoneNumber { get; set; }
}
