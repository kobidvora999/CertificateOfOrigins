namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// Request for ChangeStatusAfterDeliverySent. The legacy WCF took the full
// CertificateOfOriginsImportAuthenticationFileDetails but used only Id + OrganizationUnitId to raise the
// CloseAllTaskForImportAuthenticationRequestFile event, so it is flattened to those two scalars here.
public class ChangeStatusAfterDeliverySentRequestDto
{
    public int Id { get; set; }

    public int OrganizationUnitId { get; set; }
}
