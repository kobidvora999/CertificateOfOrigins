namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// Result row of dbo.GetImportAuthenticationRequestByFilter (import-authentication-request search).
// Property names match the SP output column aliases (Dapper case-insensitive mapping).
// Name columns whose data lives in other services are NULL from the SP and enriched in the BL:
//   IssuingCountryId (from IssuingCountryIdNum via ILookupUtil<Country>),
//   OrganizationUnitId (from OrganizationUnitIdNum via ILookupUtil<OrganizationUnit>),
//   VendorName (Vendors proxy), ImporterName (Customers proxy — the importer id arrives in CustomerId).
// LeadDocumentTitle stays NULL (CRP.DealFile document — needs the owning service's proxy); the raw
// LeadDocumentId is returned so a later migration can resolve it.
public class GetImportAuthenticationRequestResultDto
{
    public int? DocumentId { get; set; }
    public string? IssuingCountryId { get; set; }        // country name — enriched via lookup
    public string? OrganizationUnitId { get; set; }      // org-unit name — enriched via lookup (raw id in OrganizationUnitIdNum)
    public string? PreferenceDocumentTypeId { get; set; } // enum name — local JOIN (P.Name)
    public int? AuthenticationFileId { get; set; }
    public string? LeadDocumentTitle { get; set; }       // NULL (raw id in LeadDocumentId)
    public DateTime CreateDate { get; set; }
    public string? VendorName { get; set; }              // enriched via Vendors proxy
    public int? IssuingCountryIdNum { get; set; }        // raw issuing-country id
    public int? OrganizationUnitIdNum { get; set; }      // raw organization-unit id
    public string? ResponseNameEmail { get; set; }
    public int? LeadDocumentId { get; set; }
    public int? CustomerId { get; set; }                 // = R.ImporterID; the importer id (drives ImporterName)
    public int? VendorId { get; set; }
    public int? DecisionId { get; set; }
    public string? ImporterName { get; set; }            // enriched via Customers proxy
    public int? AuthenticationFileStatusId { get; set; }
}
