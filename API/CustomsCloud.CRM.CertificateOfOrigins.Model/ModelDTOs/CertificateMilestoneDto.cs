namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// Result set 7 of dbo.GetCertificateOfOriginByID — the certificate's status milestones (created/approved/rejected).
// The SP returns the acting user's id only (UserId = approve-user on status 8, else update-user); the display name
// (UserName) is enriched in the BL via IUserProxy (the cross-service Infrastructure.UserMng_User JOIN was removed).
public class CertificateMilestoneDto
{
    public DateTime CreateDate { get; set; }
    public string? ActionName { get; set; }
    public int? UserId { get; set; }
    public string? UserName { get; set; }
    public string? RejectReason { get; set; }
    public int VersionNumber { get; set; }
}
