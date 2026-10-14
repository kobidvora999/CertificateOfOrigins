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

    // The signature block: <EmployeeFullName>, <EmployeeEmailAddress>, <UserSignature>. It is filled in the BL from
    // the Users service, following the sibling letters' legacy (usp_Template_INNER_CROSS_CertificateOfOrigin, the
    // 2251-2259 branch): the signer is the user producing the letter (@CreateUserID → RequestMetadata.UserId), and the
    // block comes from UserMng_UserTemplateDetails INNER JOIN UserMng_User. So a user with no template details gets
    // the whole block empty, the email included.
    //
    // CONCAT(EnglishFirstName, ' ', EnglishLastName).
    public string? EmployeeFullName { get; set; }

    // UserMng_User.Email.
    public string? EmployeeEmailAddress { get; set; }

    // An IMAGE field. Legacy merged UserTemplateDetails.SignatureImageUrl (a URL string) into it as-is.
    // TODO(confirm): that the Templates module's Image field type accepts a URL (and not only base64 content).
    public string? UserSignature { get; set; }
}
