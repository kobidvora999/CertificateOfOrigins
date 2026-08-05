namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// Payload for SaveExportDocumentAuthenticationRequest — the full export-document authentication request graph the
// client edits and posts back (round-trips the GetExportDocumentAuthenticationRequestByID result). Id == 0 means a
// new request (insert); otherwise update. OriginalStatusId is the status snapshot from the load, used to detect a
// status transition (fires the events/message). ListOfAdditionalDocumentsIds drives the post-save document-attach.
// TimeStamp round-trips the row-version for optimistic concurrency on update. Child collections are saved
// replace-all (developer decision 2026-08-05).
public class SaveExportDocumentAuthenticationRequestRequestDto
{
    public int Id { get; set; }

    public int TypeId { get; set; }

    public string Title { get; set; } = null!;

    public int State { get; set; }

    public byte[]? TimeStamp { get; set; }

    public int OrganizationUnitId { get; set; }

    public int CustomerId { get; set; }

    public int AuthenticationDocumentTypeId { get; set; }

    public int? ExporterCustomerId { get; set; }

    public int? StatusId { get; set; }

    public int OriginalStatusId { get; set; }

    public int? CountryId { get; set; }

    public string? CustomsHouseAddress { get; set; }

    public int? VendorId { get; set; }

    public DateTime? AuthenticationRequestArrivalDate { get; set; }

    public string? AuthenticationRequestedByName { get; set; }

    public string? AuthenticationRequestedByEmail { get; set; }

    public string? AuthenticationRequestedByPhone { get; set; }

    public string AuthenticationRequestNotes { get; set; } = null!;

    public int? ExportLeadDocumentId { get; set; }

    public int? DocumentId { get; set; }

    public string? MainDocumentTitle { get; set; }

    public DateTime? LastDeliveryDate { get; set; }

    public int? DeliveryMethodId { get; set; }

    public string? InvoiceNumbers { get; set; }

    public string? DetailedDecision { get; set; }

    public string? ReferenceNumber { get; set; }

    public string? CommentForCustomsHouseLetter { get; set; }

    public int? TotalDocuments { get; set; }

    public int? TotalInvoices { get; set; }

    public DateTime? DocumentDate { get; set; }

    public DateTime? InvoiceDate { get; set; }

    // Documents to attach to the request after saving (legacy entity.ListOfAdditionalDocumentsIDs).
    public List<int> ListOfAdditionalDocumentsIds { get; set; } = [];

    public List<ExportDocumentAuthenticationRequestCustomsItemDto> CustomsItems { get; set; } = [];

    public List<ExportDocumentAuthenticationRequestLeadDocumentDto> LeadDocuments { get; set; } = [];

    public List<ExportAuthenticationRequestManufacturingAreaDto> ManufacturingAreas { get; set; } = [];
}
