namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// A child import authentication request of an authentication file (GetAuthenticationRequestFileByID, SP result-set
// #2). Carries the request scalars plus its enriched document, item lines, decision lookup, collaterals, the lead-
// document submission date, and the SendReminderForImporter task-existence flag (legacy per-row OUTER APPLY).
public class AuthenticationFileRequestDto
{
    public int DocumentId { get; set; }

    public DateTimeOffset CreateDate { get; set; }

    public int? AuthenticationFileId { get; set; }

    public DateTimeOffset AuthenticationRequestDate { get; set; }

    public int? DecisionId { get; set; }

    public int LeadDocumentId { get; set; }

    public DateTimeOffset DocumentIssuingDate { get; set; }

    public int ImportCountryId { get; set; }

    public int IssuingCountryId { get; set; }

    public int OriginCountryId { get; set; }

    public int PreferenceDocumentTypeId { get; set; }

    public string? ResponseNameEmail { get; set; }

    public int OrganizationUnitId { get; set; }

    public int? VendorId { get; set; }

    public int? CustomerId { get; set; }

    public int? ImporterId { get; set; }

    public DateTimeOffset? LastDeliveryForImporter { get; set; }

    public string? InvoiceNumber { get; set; }

    // The lead document (Documents service), enriched with TypeName (DocumentType lookup).
    public DocumentDto? Document { get; set; }

    // Item lines (SP result-set #4).
    public List<AuthenticationRequestItemDetailDto> ItemDetails { get; set; } = [];

    // Full decision lookup table (legacy assigned the same list to every request).
    public List<CertificateOfOriginsDecisionDto> Decisions { get; set; } = [];

    // Collaterals for this request (Collateral service).
    public List<CollateralRequestDto> Collaterals { get; set; } = [];

    // Lead-document submission date (DealFile service; legacy CRP.DealFile_LeadDocumentSubmissionData JOIN).
    public DateTimeOffset? LeadDocumentSubmissionDate { get; set; }

    // True when an open SendReminderForImporter (404) task exists for this request (legacy OUTER APPLY on Tasks_Task).
    public bool IsSendReminderForImporterTaskExists { get; set; }

    // { EEntityType.ImportDeclaration : [LeadDocumentId] } (EEntityType as int key).
    public Dictionary<int, List<int>> EntityTypeAndIdsToSearch { get; set; } = [];
}
