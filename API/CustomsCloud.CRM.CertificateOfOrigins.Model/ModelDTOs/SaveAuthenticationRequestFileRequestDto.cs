namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// Request for SaveAuthenticationRequestFile — the authentication file as edited by the SPA plus its child requests
// (the legacy passed the full CertificateOfOriginsImportAuthenticationFileDetails entity with its
// CertificateOfOriginsImportAuthenticationRequest collection). Carries the editable file scalars, the status
// change-detection snapshot (OriginalAuthenticationFileStatusId), and each child's decision + its change-detection
// snapshot (OriginalRequestDecisionId). OrganizationUnitId is a transient (non-column) value used only for the file
// events. The DAL applies the file + child edits set-based; a full re-read is returned.
public class SaveAuthenticationRequestFileRequestDto
{
    public int Id { get; set; }

    public int AuthenticationFileStatusId { get; set; }

    // The status as loaded — used to detect a file-status change (the .NET 10 stateless replacement for the legacy
    // WPF-only AuthenticationFileStatusIDPrev; the save re-read keeps it fresh for the next save).
    public int OriginalAuthenticationFileStatusId { get; set; }

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

    // Transient (the file entity has no OrganizationUnit column) — supplied by the client for the file-level events.
    public int OrganizationUnitId { get; set; }

    public List<SaveAuthenticationRequestFileChildDto> Requests { get; set; } = [];
}

// One child import authentication request within the file save — only the fields this method reads or writes.
public class SaveAuthenticationRequestFileChildDto
{
    public int DocumentId { get; set; }

    public int? AuthenticationFileId { get; set; }

    public int? DecisionId { get; set; }

    // The decision as loaded — used to detect which requests changed (drives the per-request events/message).
    public int? OriginalRequestDecisionId { get; set; }

    public DateTimeOffset DocumentIssuingDate { get; set; }

    public int OrganizationUnitId { get; set; }

    public int UserId { get; set; }

    public int UserResponseId { get; set; }
}
