using System.Text.Json.Serialization;

namespace CertificateOfOrigins.Model.ModelDTOs;

// The data contract of the legacy import-verification letters (2251-2259, "מכתב אימות ..."), one per authentication file.
// Legacy: Infrastructure.usp_Template_INNER_CROSS_CertificateOfOrigin, the CR126557 branch. The letters share that branch, so
// they share this contract; each letter's YAML picks the tags its document carries.
//
// Property names are the legacy column aliases = the documents' tags; the template YAML merges by their camelCase.
public class ImportVerificationLetterResult : IFreeTextTemplateData
{
    // CONVERT(VARCHAR(12), GETDATE(), 107) — "Oct 15, 2026". Kept as the formatted string legacy merged.
    public string? ImportAuthenticationRequestDate { get; set; }

    // @EntityID — the authentication file's id.
    public int FileNumber { get; set; }

    // The file's documents of one vendor group, comma-separated (see dbo.GetTemplateData).
    public string? MovementCertificate { get; set; }

    public string? InvoiceNumberForImportAuthenticationRequest { get; set; }

    // CONVERT(VARCHAR(12), DATEADD(MONTH, 3, GETDATE()), 107) — the reply deadline, three months from today.
    public string? ReplyDate { get; set; }

    // The vendor of that group. The procedure returns the id; the supplier block is filled in the BL from the Vendors
    // service (legacy joined StockPileData.Vendors_Vendor + Vendors_VendorAddress, another service's tables).
    [JsonIgnore]
    public int? VendorId { get; set; }

    public string? SupplierTitle { get; set; }

    public string? SupplierAddress { get; set; }

    public string? SupplierCountry { get; set; }

    // The reminder letter's "first letter" date: the file's LastDelivery, CONVERT(VARCHAR(12), ..., 107).
    public string? FirstLetterDate { get; set; }

    // The foreign customs department the letter is addressed to — the request customer (activity 40), from the Customers
    // service (legacy #CustomerDetails).
    [JsonIgnore]
    public int? CustomerId { get; set; }

    public string? ExternalCustomDepartmentTitle { get; set; }

    public string? ExternalCustomDepartmentAddress { get; set; }

    public string? ExternalCustomDepartmentCountry { get; set; }

    // The caller's free text (legacy TemplateArgument.FreeText, merged when the template allows free text).
    public string? FreeText { get; set; }

    // The signature block of the user producing the letter (@CreateUserID), filled in the BL from the Users service.
    public string? EmployeeFullName { get; set; }

    public string? EmployeeEmailAddress { get; set; }

    // An IMAGE field: UserMng_UserTemplateDetails.SignatureImageUrl, merged as-is.
    public string? UserSignature { get; set; }

    // The USA letters (2258, 2259) carry their signature picture under this tag. Legacy never returned a column by that
    // name, so it rendered empty there; it stays empty here (parity).
    public string? EmployeesSignature { get; set; }
}
