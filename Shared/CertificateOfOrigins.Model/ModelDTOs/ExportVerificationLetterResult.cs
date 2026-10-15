using System.Text.Json.Serialization;

namespace CertificateOfOrigins.Model.ModelDTOs;

// The data contract of the legacy export-verification letters (2289-2295), one per export authentication request.
// Legacy: Infrastructure.usp_Template_INNER_CROSS_ExportDocumentAuthenticationRequestLetter. The letters share that procedure,
// so they share this contract; each letter's YAML picks the tags its document carries.
//
// Property names are the legacy column aliases = the documents' tags; the template YAML merges by their camelCase.
public class ExportVerificationLetterResult : IFreeTextTemplateData
{
    // CONVERT(VARCHAR(12), GETDATE(), 107) — "Oct 15, 2026".
    public string? ExportAuthenticationRequestDate { get; set; }

    public string? ReferenceNumber { get; set; }

    public int ExportRequestID { get; set; }

    // CONVERT(VARCHAR(12), ..., 101) — "10/15/2026".
    public string? DocumentDate { get; set; }

    public string? InvoiceDate { get; set; }

    public string? MainDocumentTitle { get; set; }

    public string? InvoiceNumbers { get; set; }

    public string? CustomsHouseAddress { get; set; }

    // Legacy 'True'/'False' — the conditional paragraphs ("the certificates ...", "the invoices ...") the letters drop when
    // the request has no main document / no invoices.
    public bool IsDocument { get; set; }

    public bool IsInvoice { get; set; }

    // The request's country: Hebrew name and English name, from the shared Country lookup.
    [JsonIgnore]
    public int? CountryId { get; set; }

    public string? ExportCountryName { get; set; }

    public string? CountryEnglishName { get; set; }

    // The foreign customs department the letter is addressed to (the request's CustomerID), from the Customers service.
    [JsonIgnore]
    public int? CustomerId { get; set; }

    public string? ExternalCustomDepartmentTitle { get; set; }

    public string? ExternalCustomDepartmentAddress { get; set; }

    public string? ExternalCustomDepartmentCountry { get; set; }

    // The exporter (ExporterCustomerID), from the Customers service.
    [JsonIgnore]
    public int? ExporterCustomerId { get; set; }

    // Customers_Customer.Title.
    public string? ExporterCustomer { get; set; }

    // The English personal name, or the English corporation name when the personal name is blank.
    public string? ExporterName { get; set; }

    // The caller's free text (legacy TemplateArgument.FreeText, merged when the template allows free text).
    public string? FreeText { get; set; }

    // The signature block, from the Users service. Which user signs depends on the letter (see CertificateOfOriginsBl).
    public string? CustomsEmployeeName { get; set; }

    public string? EmployeeFullName { get; set; }

    public string? EmployeeEmailAddress { get; set; }

    // ISNULL(phone, ' ') in legacy — a blank, not an empty value, when the user has none.
    public string? PhoneNumber { get; set; }

    // IMAGE fields: UserMng_UserTemplateDetails.SignatureImageUrl, merged as-is. The European exporter letter (2293)
    // prints the signature twice, hence the second tag.
    public string? UserSignature { get; set; }

    public string? UserSignature1 { get; set; }
}
