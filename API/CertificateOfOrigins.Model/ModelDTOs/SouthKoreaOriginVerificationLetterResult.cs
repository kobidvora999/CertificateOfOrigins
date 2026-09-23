namespace CertificateOfOrigins.Model.ModelDTOs;

// CR 194221 — the data contract of the South-Korea origin-verification letter
// (ExportAuthenticationSendDeliveryForExporterSouthKoreaTemplate). One row per authentication file.
//
// The letter is fixed English prose apart from these four values; everything else — the Korea Customs Service
// address, the reference to Article 3.29 of the Israel–Korea FTA, and the signatory — is part of the .docx.
//
// The property names are the camelCase JSON paths the template YAML merges by ($.fileNo, $.letterDate, ...).
public class SouthKoreaOriginVerificationLetterResult
{
    // "File No.:" — the authentication file's id.
    public int FileNo { get; set; }

    // "Date:" — stamped by the procedure at render time.
    public DateTime LetterDate { get; set; }

    // "Movement certificate(s) No:" — the DocumentNumber of every child request whose preference-document type is
    // תעודת תנועה (PreferenceDocumentTypeID 4), comma-separated.
    public string? MovementCertificates { get; set; }

    // "Invoice declaration(s):" — the same for הצהרת חשבונית (PreferenceDocumentTypeID 5).
    public string? InvoiceDeclarations { get; set; }
}
