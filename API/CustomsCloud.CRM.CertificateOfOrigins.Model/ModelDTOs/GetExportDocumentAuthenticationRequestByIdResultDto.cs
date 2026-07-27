namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// Result of GetExportDocumentAuthenticationRequestByID — the export-document authentication request plus its
// three child collections (customs items, lead documents, manufacturing areas), mirroring the legacy entity +
// LoadProperty hydration. OriginalStatusId snapshots the status for the later optimistic dirty-check on Save.
// ExportDeclarationIds replaces the legacy EntityTypeAndIDsToSearch dictionary (which only drove the old WPF
// document-attach picker): the lead-document ids the client can attach documents to.
public class GetExportDocumentAuthenticationRequestByIdResultDto
{
    // NOTE (temporary): 6 fields (State, CreateDate, CreateUserId, UpdateDate, UpdateUserId, OrganizationUnitId)
    // are omitted because the platform MaxCountExceededInterceptor errors at >=30 result columns and the entity
    // has 35. Restore them once a CertificateOfOrigins hash is added to InfrastructureCore's InterceptorList and
    // the DAL switches to .Include(...) + .ExcludeInterceptor("<hash>").
    public int Id { get; set; }
    public int TypeId { get; set; }
    public string Title { get; set; } = null!;
    public byte[]? TimeStamp { get; set; }
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

    public List<int> ExportDeclarationIds { get; set; } = [];
    public List<ExportDocumentAuthenticationRequestCustomsItemDto> CustomsItems { get; set; } = [];
    public List<ExportDocumentAuthenticationRequestLeadDocumentDto> LeadDocuments { get; set; } = [];
    public List<ExportAuthenticationRequestManufacturingAreaDto> ManufacturingAreas { get; set; } = [];
}

public class ExportDocumentAuthenticationRequestCustomsItemDto
{
    public int Id { get; set; }
    public int ExportDocumentAuthenticationRequestId { get; set; }
    public int CustomsItemId { get; set; }
}

public class ExportDocumentAuthenticationRequestLeadDocumentDto
{
    public int Id { get; set; }
    public int ExportRequestId { get; set; }
    public int? LeadDocumentId { get; set; }
    public string LeadDocumentTitle { get; set; } = null!;
}

public class ExportAuthenticationRequestManufacturingAreaDto
{
    public int Id { get; set; }
    public int ExportAuthenticationRequestId { get; set; }
    public string? ManufacturingArea { get; set; }
    public string? ManufacturingZipcode { get; set; }
}
