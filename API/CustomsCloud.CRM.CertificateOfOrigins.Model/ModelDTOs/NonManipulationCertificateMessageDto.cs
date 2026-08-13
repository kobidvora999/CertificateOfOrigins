namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// The NonManipulation-certificate body of the incoming message (PC_NG_2280_MSG01 NonManipulationCertificate) — the
// import/export shipment leg fields for a non-manipulation certificate.
public class NonManipulationCertificateMessageDto
{
    public DateTime ExportDate { get; set; }

    public string? ExportCountry { get; set; }

    public string? ImportBillOfLadingNum { get; set; }

    public string? ExportPort { get; set; }

    public DateTime ImportDate { get; set; }

    public string? ExportBillOfLadingNum { get; set; }

    public string? TransitCountry { get; set; }

    public string? PortOfEntrance { get; set; }

    public DateTime ExpectedExitDate { get; set; }

    public string? ExitPort { get; set; }

    public string? GoodsDescription { get; set; }

    public string? DeclaringCompany { get; set; }

    public string? DeclaringPerson { get; set; }

    public string? DeclaringPosition { get; set; }

    public string? ManifestNum { get; set; }
}
