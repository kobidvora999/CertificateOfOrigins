namespace CertificateOfOrigins.Model.ModelDTOs;

// Request for UpdateCertificateOfOrigins — the export-declaration → certificate reconciliation (legacy
// UpdateCetificateOfOriginsDTO, delivered by the DealFile "declaration submission succeeded" event). Carries the
// certificate ids to reconcile, the declaration key (lead document + number), the expected exporter / destination,
// and the declaration's invoice/goods-item detail used to validate the certificate against the declaration.
public class UpdateCertificateOfOriginsRequestDto
{
    // EEventType numeric value (the DealFile event that triggered this — reconcile is ExportDeclarationSubmissionSucceeded).
    public int EventType { get; set; }

    public int LeadDocumentId { get; set; }

    public string? ExportDeclarationNum { get; set; }

    // Nullable on purpose: DealFile may send an explicit null, which legacy treated as empty (IsNullOrEmpty). A
    // non-nullable list would get the [ApiController] implicit [Required] and answer 400 before the BL runs.
    public List<int>? CertificateOfOriginsIds { get; set; } = [];

    public int? ExporterCustomerId { get; set; }

    public int? DestinationCountryId { get; set; }

    public int OrganizationUnitId { get; set; }

    // Nullable for the same reason: an explicit null is "no invoices" (legacy: Rejected), not a 400.
    public List<ExportInvoiceInfoDto>? ExportInvoiceInfoList { get; set; } = [];
}

// One invoice from the export declaration (DealFile) + its goods items — matched against the certificate's invoices.
public class ExportInvoiceInfoDto
{
    public string? ExternalIdNum { get; set; }

    // Nullable for the same reason: an explicit null is an invoice without goods items.
    public List<ExportGoodsItemInfoDto>? ExportGoodsItemInfoList { get; set; } = [];
}

// One goods item within a declaration invoice.
public class ExportGoodsItemInfoDto
{
    public int CertificateOfOriginId { get; set; }

    public int CustomsItemId { get; set; }

    public int OriginCountryId { get; set; }
}
