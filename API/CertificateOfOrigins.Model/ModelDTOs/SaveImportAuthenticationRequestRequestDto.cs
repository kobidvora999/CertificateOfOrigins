namespace CertificateOfOrigins.Model.ModelDTOs;

// Request for SaveImportAuthenticationRequest — the import authentication request as edited by the SPA (the legacy
// passed the full CertificateOfOriginsImportAuthenticationRequest entity). Carries the round-trip editable fields
// (mirroring GetAuthenticationRequestByIdResultDto) plus the messaging users and the collaterals. The DAL applies
// these over the existing row (Fetch & Merge), preserving the structural/audit columns not carried here.
public class SaveImportAuthenticationRequestRequestDto
{
    public int DocumentId { get; set; }

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

    // The creating user + the responder — used to address the central-decision message (legacy UserID / UserResponseID).
    public int UserId { get; set; }

    public int UserResponseId { get; set; }

    // --- Coordinator-edited columns (restored 2026-09-07, CHECK 2) ---
    // The legacy save was a full self-tracking-entity write, so every column the coordinator's screen touches was
    // persisted. The migration replaced it with an explicit ExecuteUpdateAsync set-list and these were left out,
    // which meant the values were accepted by the API and silently discarded.
    //
    // DecisionCircumstences is the load-bearing one: the legacy entity validation makes it MANDATORY whenever the
    // decision changes (CertificateOfOriginsImportAuthenticationRequest.ValidateField), so a coordinator who
    // changed a decision and typed the required justification lost exactly that justification — the audit reason
    // for the decision, gone.
    public string? DecisionCircumstences { get; set; }

    public string? CirumstanceDetails { get; set; }   // legacy spelling, kept — it is the column name

    // Nullable on purpose, unlike the column: RequestCircumstancesID is NOT NULL with an FK to
    // enum_Circumstances, so "omitted" has to be distinguishable from 0 — writing 0 would break the FK. Null here
    // means "leave the stored value alone" (see the merge in SaveImportAuthenticationRequest).
    public int? RequestCircumstancesId { get; set; }

    // Also NOT NULL in the DB despite the nullable CLR type. Null means "leave as-is", not "clear".
    public string? Remarks { get; set; }

    public string? ResponsePhoneNum { get; set; }

    public string? DocumentNumber { get; set; }

    public decimal? InvoiceGoodsItemTaxDifference { get; set; }

    public decimal? AllInvoiceGoodsItemTaxDifference { get; set; }

    // Precomputed on load (GetAuthenticationRequestByID): whether the current user already handles the request — gates
    // the NewAuthenticationRequest event in the decision switch.
    public bool IsCurrentUserHandleRequest { get; set; }

    // Collaterals attached to the request (Collateral microservice) — the first supplies CollateralId; all are pushed
    // to permanent via ChangeTempCollateralRequest. Not persisted as a local child collection.
    public List<CollateralRequestDto> Collaterals { get; set; } = [];
}
