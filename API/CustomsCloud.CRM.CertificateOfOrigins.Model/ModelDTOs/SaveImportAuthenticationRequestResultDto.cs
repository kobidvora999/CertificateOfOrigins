namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// Authoritative result of SaveImportAuthenticationRequest — the saved request's persisted scalar state, re-read from
// the DB after the save (via the existing GetImportAuthenticationRequestById projection — ~21 columns, kept under the
// 30-column read interceptor). No proxy enrichment: the SPA already holds the enrichment collections (decisions,
// item lines, document, task flags) from the initial load, so this returns just the authoritative saved row cheaply.
public class SaveImportAuthenticationRequestResultDto
{
    public int DocumentId { get; set; }

    public DateTimeOffset CreateDate { get; set; }

    public int? AuthenticationFileId { get; set; }

    public DateTimeOffset AuthenticationRequestDate { get; set; }

    public int? CollateralId { get; set; }

    public int? DecisionId { get; set; }

    public int LeadDocumentId { get; set; }

    public DateTimeOffset DocumentIssuingDate { get; set; }

    public int ImportCountryId { get; set; }

    public int IssuingCountryId { get; set; }

    public int Number { get; set; }

    public int OriginCountryId { get; set; }

    public int PreferenceDocumentTypeId { get; set; }

    public string? ResponseNameEmail { get; set; }

    public int OrganizationUnitId { get; set; }

    public int? VendorId { get; set; }

    public string? VendorName { get; set; }

    public int? CustomerId { get; set; }

    public int? ImporterId { get; set; }

    public DateTimeOffset? LastDeliveryForImporter { get; set; }

    public string? InvoiceNumber { get; set; }
}
