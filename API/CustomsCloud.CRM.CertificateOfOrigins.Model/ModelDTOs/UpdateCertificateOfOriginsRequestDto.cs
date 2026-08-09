namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

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

    public List<int> CertificateOfOriginsIds { get; set; } = [];

    public int? ExporterCustomerId { get; set; }

    public int? DestinationCountryId { get; set; }

    public int OrganizationUnitId { get; set; }

    public List<ExportInvoiceInfoDto> ExportInvoiceInfoList { get; set; } = [];
}

// One invoice from the export declaration (DealFile) + its goods items — matched against the certificate's invoices.
public class ExportInvoiceInfoDto
{
    public string? ExternalIdNum { get; set; }

    public List<ExportGoodsItemInfoDto> ExportGoodsItemInfoList { get; set; } = [];
}

// One goods item within a declaration invoice.
public class ExportGoodsItemInfoDto
{
    public int CertificateOfOriginId { get; set; }

    public int CustomsItemId { get; set; }

    public int OriginCountryId { get; set; }
}
