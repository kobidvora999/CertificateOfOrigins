namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// Result row of dbo.ExportDocumentAuthenticationRequestSearch (export-document authentication-request search).
// Property names match the SP output column aliases (Dapper case-insensitive mapping).
// Cross-service name columns are NULL from the SP and enriched in the BL:
//   CountryName (from CountryId via ILookupUtil<Country>),
//   ForeignCustomsHouseName (from CustomerId via ICustomerProxy),
//   RequestIssuerName (from ExporterCustomerId via ICustomerProxy).
// DocumentTypeName / RequestStatusName / ExportDeclarationTitle come from local joins in the SP.
public class GetExportDocumentAuthenticationRequestSearchResultDto
{
    public int RequestId { get; set; }
    public string? CountryName { get; set; }             // enriched via Country lookup
    public int? CountryId { get; set; }                  // raw country id
    public string? ForeignCustomsHouseName { get; set; } // enriched via Customers proxy
    public int? CustomerId { get; set; }                 // foreign-customs-house customer id
    public string? DocumentTypeName { get; set; }        // local enum join
    public string? ExportDeclarationTitle { get; set; }  // local link-table (OUTER APPLY)
    public string? RequestStatusName { get; set; }       // local enum join
    public string? RequestIssuerName { get; set; }       // enriched via Customers proxy
    public int? ExporterCustomerId { get; set; }         // raw exporter customer id (drives RequestIssuerName)
    public int? ExportLeadDocumentId { get; set; }
}
