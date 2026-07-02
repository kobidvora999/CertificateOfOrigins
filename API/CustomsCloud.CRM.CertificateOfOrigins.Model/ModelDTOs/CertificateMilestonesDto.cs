namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

public class CertificateMilestonesDto
{
    public DateTimeOffset CreateDate { get; set; }
    public string? ActionName { get; set; }
    public string? UserName { get; set; }
    public string? RejectReason { get; set; }
    public int VersionNumber { get; set; }
}
