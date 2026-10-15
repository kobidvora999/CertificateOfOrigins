namespace CertificateOfOrigins.Model.ModelDTOs;

// Result of the importer delivery/reminder flow. The legacy WCF echoed back the request entity it received, with what the
// server changed in it and in its parent file. The legacy client used it as the selected request without re-reading it
// (the reminder flow), so this carries every changed field (parity finding G-F7):
// - the request: DecisionId, LastDeliveryForImporter, UpdateDate;
// - the parent file: status, delivery method, LastDelivery, UpdateDate (the two dates are null when the request has no file).
public class HandleDeliveryOrReminderForImporterSentResultDto
{
    public int DocumentId { get; set; }

    public int DecisionId { get; set; }

    public DateTime LastDeliveryForImporter { get; set; }

    public DateTime UpdateDate { get; set; }

    public int AuthenticationFileStatusId { get; set; }

    public int DeliveryMethodId { get; set; }

    public DateTime? FileLastDelivery { get; set; }

    public DateTime? FileUpdateDate { get; set; }
}
