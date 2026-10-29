namespace CertificateOfOrigins.Model.ModelDTOs;

// Result of HandleImportAuthenticationRequestDeliveryAndReminderForVendorSent. The legacy WCF echoed back the file entity it
// received, with what the server changed in it: the status and delivery method (status machine), LastDelivery and UpdateDate
// (UpdateFileAfterDelivery). This carries every one of those fields, so a caller that keeps the result instead of re-reading
// the file is not left with stale values (parity finding G-F7). FirstProvideContactDate is not here: legacy set it on the
// client, before the call.
public class HandleDeliveryAndReminderForVendorSentResultDto
{
    public int Id { get; set; }

    public int AuthenticationFileStatusId { get; set; }

    public int DeliveryMethodId { get; set; }

    public DateTime LastDelivery { get; set; }

    public DateTime UpdateDate { get; set; }
}
