namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// Result row of dbo.GetAuthenticationRequestByLeadDocumentID — import-authentication requests for a set of
// lead-document ids (passed as a Shared.IntArray TVP). Property names match the SP output column aliases.
// Cross-service name columns are NULL from the SP and enriched in the BL:
//   ImportCountryName (from ImportCountryId via ILookupUtil<Country>),
//   OrganizationUnitName (from OrganizationUnitId via ILookupUtil<OrganizationUnit>).
// LeadDocumentTitle stays NULL (CRP.DealFile document — no proxy; raw LeadDocumentId returned).
// PreferenceDocumentTypeName / AuthenticationFileStatusName / DecisionName come from local enum joins.
public class GetAuthenticationRequestByLeadDocumentResultDto
{
    public int LeadDocumentId { get; set; }
    public string? LeadDocumentTitle { get; set; }          // NULL (raw id in LeadDocumentId)
    public int DocumentId { get; set; }
    public int? AuthenticationFileId { get; set; }
    public int? PreferenceDocumentTypeId { get; set; }
    public string? PreferenceDocumentTypeName { get; set; } // local enum join
    public DateTime CreateDate { get; set; }
    public int? AuthenticationFileStatusId { get; set; }
    public string? AuthenticationFileStatusName { get; set; } // local enum join
    public int? DecisionId { get; set; }
    public string? DecisionName { get; set; }               // local enum join
    public int? ImportCountryId { get; set; }               // raw import-country id
    public string? ImportCountryName { get; set; }          // enriched via Country lookup
    public int? OrganizationUnitId { get; set; }            // raw organization-unit id
    public string? OrganizationUnitName { get; set; }       // enriched via OrganizationUnit lookup
    public int? CollateralId { get; set; }
    public bool IsCollateralExists { get; set; }
}
