namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// Projection of the ExportDealFile microservice's export-declaration details — the fields this service
// consumes when deciding whether a certificate-of-origin request may proceed. Mirrors the legacy
// ExportDeclarationDetailsDTO. Extra fields on the wire are ignored on deserialization; expand when needed.
public class ExportDeclarationDetailsDto
{
    public int LeadDocumentId { get; set; }
    public bool IsDeclarationReleased { get; set; }
    public bool IsCargoExitedOfCustomsRegulation { get; set; }
    public bool IsDeclarationInAmendmentProcess { get; set; }
    public int LeadDocumentStateId { get; set; }
}
