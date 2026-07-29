namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// Result of HandleImportAuthenticationRequestDeliveryAndReminderForVendorSent — the file's status + delivery method
// after the status machine advanced them (the legacy WCF echoed back the mutated file entity).
public class HandleDeliveryAndReminderForVendorSentResultDto
{
    public int Id { get; set; }

    public int AuthenticationFileStatusId { get; set; }

    public int DeliveryMethodId { get; set; }
}
