namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

public class ImportAuthenticationRequestDto
{
    // Result set 1 — header (CRM.CertificateOfOrigins_ImportAuthenticationRequest)
    public int DocumentId { get; set; }
    public DateTimeOffset CreateDate { get; set; }
    public int CreateUserId { get; set; }
    public DateTimeOffset UpdateDate { get; set; }
    public int UpdateUserId { get; set; }
    public int? AuthenticationFileId { get; set; }
    public DateTimeOffset AuthenticationRequestDate { get; set; }
    public string? CirumstanceDetails { get; set; }
    public int? CollateralId { get; set; }
    public string? DecisionCircumstences { get; set; }
    public int? DecisionId { get; set; }
    public int LeadDocumentId { get; set; }
    public DateTimeOffset DocumentIssuingDate { get; set; }
    public int ImportCountryId { get; set; }
    public int IssuingCountryId { get; set; }
    public int ItemDetailId { get; set; }
    public int Number { get; set; }
    public bool IsOldIndication { get; set; }
    public int OriginCountryId { get; set; }
    public int PreferenceDocumentTypeId { get; set; }
    public string? Remarks { get; set; }
    public int RequestCircumstancesId { get; set; }
    public int UserResponseId { get; set; }
    public string? ResponseNameEmail { get; set; }
    public string? ResponsePhoneNum { get; set; }
    public int OrganizationUnitId { get; set; }
    public int UserId { get; set; }
    public int? VendorId { get; set; }
    public string? VendorName { get; set; }
    public int? OrganizationUnitTypeId { get; set; }
    public string? DocumentNumber { get; set; }
    public int? CustomerId { get; set; }
    public int? ImporterId { get; set; }
    public string? InvoiceNumber { get; set; }
    public decimal? InvoiceGoodsItemTaxDifference { get; set; }
    public decimal? AllInvoiceGoodsItemTaxDifference { get; set; }
    public DateTimeOffset? LeadDocumentSubmissionDate { get; set; }
    public bool IsSendReminderForImporterTaskExists { get; set; }

    // Result set 2 — item details
    public List<CertificateOfOriginsItemDetailDto> ItemDetails { get; set; } = new();

    // Result set 3 — document (FileUrl enriched in BL via DocumentType lookup)
    public DocumentDto? Document { get; set; }

    // Enrichment (BL)
    public List<CertificateOfOriginsDecisionDto> Decisions { get; set; } = new();
    public List<CollateralRequestDto> Collaterals { get; set; } = new();
    public bool IsCurrentUserHandleRequest { get; set; }
    public bool IsCurrentUserHasOpenTask { get; set; }

    // Key = EEntityType value (e.g. 1055 = ImportDeclaration)
    public Dictionary<int, List<int>> EntityTypeAndIdsToSearch { get; set; } = new();
    public int AdditionalRequestsForSearchInDays { get; set; }
    public bool IsVendorByIssuingCountryId { get; set; }
}
