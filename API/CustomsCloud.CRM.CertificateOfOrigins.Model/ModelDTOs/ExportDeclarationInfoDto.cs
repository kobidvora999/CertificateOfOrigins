namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// Export-declaration info for the declaration-submitted/released reconciliation (GetPC_MSG2280_2281
// CheckCertificateOfOriginOnDeclarationSubmited). Mirrors the legacy ExportDeclarationInfoDTO — the state gate + the
// values fed into UpdateCertificateOfOrigins, including the declaration's invoice/goods-item list to reconcile against.
public class ExportDeclarationInfoDto
{
    public int LeadDocumentId { get; set; }

    // ELeadDocumentState numeric value — Submited / Released / Paroled / Closed drive the reconciliation.
    public int LeadDocumentState { get; set; }

    public int? DestinationCountryId { get; set; }

    public int? ExporterCustomerId { get; set; }

    public int? OrganizationUnitId { get; set; }

    // The declaration's invoices + goods items, reconciled against the certificate's invoices (legacy
    // ExportDeclarationInfoDTO.ExportInvoiceInfoDTOList → UpdateCertificateOfOrigins.ExportInvoiceInfoList).
    public List<ExportInvoiceInfoDto> ExportInvoiceInfoList { get; set; } = [];
}
