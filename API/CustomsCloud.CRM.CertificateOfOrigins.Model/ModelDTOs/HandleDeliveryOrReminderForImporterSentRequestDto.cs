namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// Request for the importer delivery/reminder flow (HandleImportAuthenticationRequestDeliveryForImporterSent /
// ...DeliveryReminderForImporterSent). Faithful to the WCF (developer decision 2026-07-29): the file status machine
// runs on the CLIENT-supplied current file status + delivery method (no DB fetch). Flattened from the full request
// entity to the fields the flow reads. DocumentId is the request's key.
public class HandleDeliveryOrReminderForImporterSentRequestDto
{
    public int DocumentId { get; set; }

    public int OrganizationUnitId { get; set; }

    public int? AuthenticationFileId { get; set; }

    // The parent file's current status + delivery method — the status machine advances these (client-supplied).
    public int AuthenticationFileStatusId { get; set; }

    public int DeliveryMethodId { get; set; }
}
