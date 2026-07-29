namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// Request for CloseReminderTask (WCF contract HandleSendRemindDeliverNotification). The legacy took the full
// CertificateOfOriginsImportAuthenticationFileDetails but used only Id + OrganizationUnitId to raise the
// CloseTaskReminderNotice3Months event, so it is flattened to those two scalars here.
public class CloseReminderTaskRequestDto
{
    public int Id { get; set; }

    public int OrganizationUnitId { get; set; }
}
