namespace CertificateOfOrigins.Model.ModelDTOs;

// Result of GetAuthenticationRequestFileByID — a single authentication file with its child requests (each enriched
// with document, item lines, decisions, collaterals, submission date, reminder-task flag), the file-status lookup,
// and the current-user handling flag. The legacy returned the full EF entity; this DTO carries the populated fields.
public class GetAuthenticationRequestFileByIdResultDto
{
    public int Id { get; set; }

    public int State { get; set; }

    public DateTime CreateDate { get; set; }

    public int AuthenticationFileStatusId { get; set; }

    public string? Notes { get; set; }

    public string? PostalAdress { get; set; }

    public int DeliveryMethodId { get; set; }

    public string? EmailAdress { get; set; }

    public int ReminderMethodId { get; set; }

    public int RequestCountryId { get; set; }

    public int UserId { get; set; }

    public string? UserNameIssuingLetter { get; set; }

    public DateTime? LastDelivery { get; set; }

    public int? ImporterContactingReasonId { get; set; }

    public DateTime? FirstProvideContactDate { get; set; }

    // Legacy transient (no SP column): the materializer set CustomerID 0 -> -1, and the SP never populates it, so it
    // is always -1.
    public int CustomerId { get; set; } = -1;

    // Full authentication-file-status lookup table.
    public List<AuthenticationFileStatusDto> FileStatuses { get; set; } = [];

    // The file's child requests.
    public List<AuthenticationFileRequestDto> Requests { get; set; } = [];

    // Legacy CertificateOfOriginsImportAuthenticationFileDetailsPartial.OrganizationUnitID, a [DataMember] whose getter
    // returns the first child request's unit (0 for a file with none). The client got it on every read and sent it back on
    // the file calls (SaveFile, ChangeStatusAfterDeliverySent, CloseReminderTask), so the file's events carried that unit
    // (parity findings FN-02, GN-03).
    public int OrganizationUnitId { get; set; }

    // True when the current user (RequestMetadata.UserId) owns an open task on the file (Tasks service).
    public bool IsCurrentUserHandleFile { get; set; }

    // { EEntityType.ImportDeclaration : [all child requests' LeadDocumentIds] } (EEntityType as int key).
    public Dictionary<int, List<int>> EntityTypeAndIdsToSearch { get; set; } = [];
}
