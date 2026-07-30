namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// Result of the importer delivery/reminder flow — the request's new decision plus the parent file's status +
// delivery method after the status machine advanced them (the legacy WCF echoed back the mutated request entity).
public class HandleDeliveryOrReminderForImporterSentResultDto
{
    public int DocumentId { get; set; }

    public int DecisionId { get; set; }

    public int AuthenticationFileStatusId { get; set; }

    public int DeliveryMethodId { get; set; }
}
