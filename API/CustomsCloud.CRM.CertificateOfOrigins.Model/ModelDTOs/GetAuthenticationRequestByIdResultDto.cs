namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// Result of GetAuthenticationRequestByID — a single import authentication request with its item lines, the decision
// lookup, collaterals (Collateral service), current-user task flags, and computed fields. The legacy returned the
// full EF entity; this DTO carries the fields that method populated.
public class GetAuthenticationRequestByIdResultDto
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

    // SP result-set #2: item lines.
    public List<AuthenticationRequestItemDetailDto> ItemDetails { get; set; } = [];

    // Full decision lookup table (legacy loaded all decisions).
    public List<CertificateOfOriginsDecisionDto> Decisions { get; set; } = [];

    // Collaterals for this request (Collateral microservice).
    public List<CollateralRequestDto> Collaterals { get; set; } = [];

    // The lead document (SP result-set #3, Infrastructure.Docs_Document) — enriched via IDocumentsProxy.GetDocumentById;
    // TypeName carries the legacy Document.FileUrl (DocumentType name). The proxy route is a rollout TODO(blocking).
    public DocumentDto? Document { get; set; }

    // The lead-document submission date (legacy CRP.DealFile_LeadDocumentSubmissionData JOIN) — enriched via
    // IExportDealFileProxy.GetLeadDocumentSubmissionDate. The proxy route is a rollout TODO(blocking).
    public DateTimeOffset? LeadDocumentSubmissionDate { get; set; }

    // Current-user task flags (from the Tasks microservice, compared to RequestMetadata.UserId).
    public bool IsCurrentUserHandleRequest { get; set; }

    public bool IsCurrentUserHasOpenTask { get; set; }

    // Related entity ids to search: { EEntityType.ImportDeclaration : [LeadDocumentId] } (EEntityType as int key).
    public Dictionary<int, List<int>> EntityTypeAndIdsToSearch { get; set; } = [];

    // Config: additional-requests search window in days (parametersUtil "AdditionalRequestsForSearchInDays").
    public int AdditionalRequestsForSearchInDays { get; set; }

    // True when the issuing country is configured as a supplier-delivery (vendor) country.
    public bool IsVendorByIssuingCountryId { get; set; }
}
