namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

public class ImportAuthenticationFileDetailsDto
{
    // Result set 1 — file header (CRM.CertificateOfOrigins_ImportAuthenticationFileDetails)
    public int Id { get; set; }
    public int State { get; set; }
    public DateTimeOffset CreateDate { get; set; }
    public int CreateUserId { get; set; }
    public DateTimeOffset UpdateDate { get; set; }
    public int UpdateUserId { get; set; }
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

    // Computed (materializer / BL)
    public List<int> CustomerIdList { get; set; } = new();
    public int CustomerId { get; set; }
    public int OrganizationUnitId { get; set; }

    // Key = EEntityType value (e.g. 1055 = ImportDeclaration)
    public Dictionary<int, List<int>> EntityTypeAndIdsToSearch { get; set; } = new();
    public bool IsCurrentUserHandleFile { get; set; }

    // Result set 2/3/4 — child requests (with their documents + item details), enriched in BL
    public List<ImportAuthenticationRequestDto> Requests { get; set; } = new();

    // Enrichment (BL) — lookup of all file statuses
    public List<CertificateOfOriginsAuthenticationFileStatusDto> FileStatuses { get; set; } = new();
}
