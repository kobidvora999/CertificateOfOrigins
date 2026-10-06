namespace CertificateOfOrigins.Model.ModelDTOs;

// CR 194221 — the data contract of the South-Korea origin-verification letter
// (ExportAuthenticationSendDeliveryForExporterSouthKoreaTemplate). One row per import authentication file.
//
// The tag names are NOT invented: this letter is structurally the same document as the other import-verification
// letters (the EU one, 2251, is its closest sibling), so it reuses that family's established tag vocabulary. Keeping
// them identical is what will make the remaining letters in the family cheap to add.
//
// Property names are the camelCase JSON paths the template YAML merges by.
public class SouthKoreaOriginVerificationLetterResult
{
    // <ImportAuthenticationRequestDate> — stamped at render time.
    public DateTime ImportAuthenticationRequestDate { get; set; }

    // <FileNumber> — the authentication file's id.
    public int FileNumber { get; set; }

    // <ExternalCustomDepartmentAddress> — the foreign customs department the letter is addressed to.
    public string? ExternalCustomDepartmentAddress { get; set; }

    // <ExternalCustomDepartmentCountry> — that department's country. The procedure returns the id; the name is
    // filled in the BL from the shared Country lookup, since a service-owned procedure never joins another
    // service's tables.
    public int? RequestCountryId { get; set; }

    public string? ExternalCustomDepartmentCountry { get; set; }

    // <MovementCertificate> — comma-separated DocumentNumbers of the child requests whose preference-document type
    // is תעודת תנועה (PreferenceDocumentTypeID 4).
    public string? MovementCertificate { get; set; }

    // <InvoiceNumberForImportAuthenticationRequest> — the same for הצהרת חשבונית (PreferenceDocumentTypeID 5).
    public string? InvoiceNumberForImportAuthenticationRequest { get; set; }

    // <FreeText> — the free-text paragraph above the signature ("Dynamic comment" in the sibling templates).
    // ⚠️ TODO(confirm): mapped to the file's Notes column, which is the only free-text field on the entity. Confirm
    // that is the field the letter is meant to print.
    public string? FreeText { get; set; }

    // <EmployeeFullName> / <EmployeeEmailAddress> — the signing customs employee.
    // 🛑 TODO(blocking): no confirmed source. Candidates are the file's UserID, its UpdateUserID, or the acting user
    // on the request — they differ in practice. Once chosen, fill from the Users service like other cross-service
    // names. Until then both tags render empty.
    public string? EmployeeFullName { get; set; }

    public string? EmployeeEmailAddress { get; set; }

    // <UserSignature> — an IMAGE field: the signing employee's scanned signature, not a string.
    // 🛑 TODO(blocking): nothing in this service stores or serves a user signature image, so the tag renders empty.
    // Needs a product/platform decision on where signatures live.
    public byte[]? UserSignature { get; set; }
}
