namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

public class ExportDocumentAuthenticationRequestDto
{
    public int Id { get; set; }
    public int TypeId { get; set; }
    public string? Title { get; set; }
    public int State { get; set; }
    public DateTimeOffset CreateDate { get; set; }
    public int CreateUserId { get; set; }
    public DateTimeOffset UpdateDate { get; set; }
    public int UpdateUserId { get; set; }
    public int OrganizationUnitId { get; set; }
    public int CustomerId { get; set; }
    public int AuthenticationDocumentTypeId { get; set; }
    public int? ExporterCustomerId { get; set; }
    public int? StatusId { get; set; }
    public int? CountryId { get; set; }
    public string? CustomsHouseAddress { get; set; }
    public int? VendorId { get; set; }
    public DateTimeOffset? AuthenticationRequestArrivalDate { get; set; }
    public string? AuthenticationRequestedByName { get; set; }
    public string? AuthenticationRequestedByEmail { get; set; }
    public string? AuthenticationRequestedByPhone { get; set; }
    public string? AuthenticationRequestNotes { get; set; }
    public int? ExportLeadDocumentId { get; set; }
    public int? DocumentId { get; set; }
    public string? MainDocumentTitle { get; set; }
    public DateTimeOffset? LastDeliveryDate { get; set; }
    public int? DeliveryMethodId { get; set; }
    public string? InvoiceNumbers { get; set; }
    public string? DetailedDecision { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? CommentForCustomsHouseLetter { get; set; }
    public int? TotalDocuments { get; set; }
    public int? TotalInvoices { get; set; }
    public DateTimeOffset? DocumentDate { get; set; }
    public DateTimeOffset? InvoiceDate { get; set; }

    // Client change-tracking baseline (legacy transient partial member — set on Get, compared on Save)
    public int OriginalStatusId { get; set; }

    // Key = EEntityType value (e.g. 12414 = ExportDeclaration)
    public Dictionary<int, List<int>> EntityTypeAndIdsToSearch { get; set; } = new();

    // Used by Save to attach documents (legacy transient partial member)
    public List<int> ListOfAdditionalDocumentsIds { get; set; } = new();

    // Child collections
    public List<CustomsItemToExportDocumentAuthenticationRequestDto> CustomsItemToExportDocumentAuthenticationRequest { get; set; } = new();
    public List<ExportDocumentAuthenticationRequestLeadDocumentDto> ExportDocumentAuthenticationRequestLeadDocument { get; set; } = new();
    public List<ExportAuthenticationRequestManufacturingAreaDto> ExportAuthenticationRequestManufacturingArea { get; set; } = new();
}
