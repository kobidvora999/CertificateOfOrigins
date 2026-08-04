namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// Result of GetAuthenticationRequestFileByID — a single authentication file with its child requests (each enriched
// with document, item lines, decisions, collaterals, submission date, reminder-task flag), the file-status lookup,
// and the current-user handling flag. The legacy returned the full EF entity; this DTO carries the populated fields.
public class GetAuthenticationRequestFileByIdResultDto
{
    public int Id { get; set; }

    public int State { get; set; }

    public DateTimeOffset CreateDate { get; set; }

    public int AuthenticationFileStatusId { get; set; }

    public string? Notes { get; set; }

    public string? PostalAdress { get; set; }

    public int DeliveryMethodId { get; set; }

    public string? EmailAdress { get; set; }

    public int ReminderMethodId { get; set; }

    public int RequestCountryId { get; set; }

    public int UserId { get; set; }

    public string? UserNameIssuingLetter { get; set; }

    public DateTimeOffset? LastDelivery { get; set; }

    public int? ImporterContactingReasonId { get; set; }

    public DateTimeOffset? FirstProvideContactDate { get; set; }

    // Legacy transient (no SP column): the materializer set CustomerID 0 -> -1, and the SP never populates it, so it
    // is always -1.
    public int CustomerId { get; set; } = -1;

    // Full authentication-file-status lookup table.
    public List<AuthenticationFileStatusDto> FileStatuses { get; set; } = [];

    // The file's child requests.
    public List<AuthenticationFileRequestDto> Requests { get; set; } = [];

    // True when the current user (RequestMetadata.UserId) owns an open task on the file (Tasks service).
    public bool IsCurrentUserHandleFile { get; set; }

    // { EEntityType.ImportDeclaration : [all child requests' LeadDocumentIds] } (EEntityType as int key).
    public Dictionary<int, List<int>> EntityTypeAndIdsToSearch { get; set; } = [];
}
