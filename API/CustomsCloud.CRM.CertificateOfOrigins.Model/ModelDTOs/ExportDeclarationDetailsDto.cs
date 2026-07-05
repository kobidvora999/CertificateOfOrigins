namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

public class ExportDeclarationDetailsDto
{
    public int LeadDocumentId { get; set; }
    public bool IsDeclarationReleased { get; set; }
    public bool IsCargoExitedOfCustomsRegulation { get; set; }
    public bool IsDeclarationInAmendmentProcess { get; set; }
    public int LeadDocumentStateId { get; set; }
}
