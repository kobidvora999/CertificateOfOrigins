using System.Text.Json.Serialization;

namespace CertificateOfOrigins.Model.ModelDTOs;

// The data contract of the importer alert letter (2244, ImportAuthenticationRequestImporterLetter, "מכתב התראה ליבואן"),
// one per authentication request. Legacy: Infrastructure.usp_Template_INNER_CROSS_ImportAuthenticationRequestImporterLetter.
//
// Property names are the legacy column aliases = the document's tags; the template YAML merges by their camelCase.
public class ImporterAlertLetterResult
{
    // IAR.DocumentID — the request's id.
    public int ImportAuthenticationRequestID { get; set; }

    // IAR.DocumentNumber.
    public string? DocumentTitle { get; set; }

    public int? AuthenticationFileID { get; set; }

    // IAR.InvoiceNumber.
    public string? InvoiceNumberForImportAuthenticationRequest { get; set; }

    // The importer's name (Customers_Customer.Title), from the Customers service.
    [JsonIgnore]
    public int? ImporterId { get; set; }

    public string? ImporterName { get; set; }

    // The lead document's title (CRP.DealFile_LeadDocument.Title).
    // TODO(blocking): no source — the import deal file belongs to another service and this one has no proxy for it (the
    // same gap the authentication-request reads already carry). Renders empty until that proxy exists.
    [JsonIgnore]
    public int LeadDocumentId { get; set; }

    public string? LeadDocumentTitleForImportAuthenticationRequest { get; set; }

    // UserMng_User.Title of the user producing the letter (@CreateUserID).
    public string? UserTitle { get; set; }

    // An IMAGE field — legacy took the signature of the user who last updated the REQUEST (IAR.UpdateUserID), not of the
    // user producing the letter.
    [JsonIgnore]
    public int UpdateUserId { get; set; }

    public string? UserSignature { get; set; }

    // The footer. Legacy fixed the signing unit in the procedure (OrganizationUnitID_ForSignature 99, type 106) and the
    // generic merger filled the unit's contact details (TemplateManagementBL.GetSignatureDetails) and the customs website
    // (the CustomsWebsite configuration).
    // TODO(blocking): the unit's address / phone / fax came from system tables (OrganizationUnitPhoneNumberLOV,
    // CommunicationDetailsCustomsHouseOrganizationUnitType) that have no source in this service. They render empty.
    [JsonPropertyName("customsHouseAddress_ForSignature")] // the tag's own underscore; CA1707 keeps it off the property name
    public string? CustomsHouseAddressForSignature { get; set; }

    [JsonPropertyName("customsHousePhone_ForSignature")] // the tag's own underscore; CA1707 keeps it off the property name
    public string? CustomsHousePhoneForSignature { get; set; }

    [JsonPropertyName("customsHouseFax_ForSignature")] // the tag's own underscore; CA1707 keeps it off the property name
    public string? CustomsHouseFaxForSignature { get; set; }

    [JsonPropertyName("customsWebsite_ForSignature")] // the tag's own underscore; CA1707 keeps it off the property name
    public string? CustomsWebsiteForSignature { get; set; }
}
