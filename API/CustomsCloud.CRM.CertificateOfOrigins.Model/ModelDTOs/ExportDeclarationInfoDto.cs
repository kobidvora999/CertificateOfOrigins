namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// Export-declaration info for the "declaration released" reconciliation (SaveCertificateOfOrigin →
// CheckCertificateOfOriginOnDeclarationReleased). Mirrors the legacy ExportDeclarationInfoDTO — the fields the
// release check reads (state) and the values fed into the certificate update.
// TODO(migration): ExportInvoiceInfoList (invoice reconciliation for UpdateCetrificateOfOrigins, #34) is deferred.
public class ExportDeclarationInfoDto
{
    public int LeadDocumentId { get; set; }

    // ELeadDocumentState numeric value — Released / Paroled / Closed drive the auto-publish.
    public int LeadDocumentState { get; set; }

    public int? DestinationCountryId { get; set; }

    public int? ExporterCustomerId { get; set; }

    public int? OrganizationUnitId { get; set; }
}
