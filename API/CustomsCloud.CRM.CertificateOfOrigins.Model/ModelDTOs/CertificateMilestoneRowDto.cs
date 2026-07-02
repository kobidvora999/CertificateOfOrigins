namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// Internal projection row for the milestones query — UserName is resolved in the BL via the Users proxy
// because Infrastructure.UserMng_User is not replicated to this service's DB.
public class CertificateMilestoneRowDto
{
    public int VersionNumber { get; set; }
    public string? ActionName { get; set; }
    public DateTimeOffset CreateDate { get; set; }
    public string? RejectReason { get; set; }
    public int? UserId { get; set; }
}
