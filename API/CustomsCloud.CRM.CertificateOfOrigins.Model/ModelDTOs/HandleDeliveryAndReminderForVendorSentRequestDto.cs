namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// Request for HandleImportAuthenticationRequestDeliveryAndReminderForVendorSent. Faithful to the WCF (developer
// decision 2026-07-29): the status machine runs on the CLIENT-supplied current status + delivery method (no DB
// fetch). IsDelivery false = a reminder was sent. Flattened from the full file entity to the fields the machine
// reads plus the file Id.
public class HandleDeliveryAndReminderForVendorSentRequestDto
{
    public int Id { get; set; }

    public int AuthenticationFileStatusId { get; set; }

    public int DeliveryMethodId { get; set; }

    public bool IsDelivery { get; set; }
}
