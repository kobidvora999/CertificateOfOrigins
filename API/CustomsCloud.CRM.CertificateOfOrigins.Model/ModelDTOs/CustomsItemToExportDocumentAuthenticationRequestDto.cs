namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

public class CustomsItemToExportDocumentAuthenticationRequestDto
{
    public int Id { get; set; }
    public int ExportDocumentAuthenticationRequestId { get; set; }
    public int CustomsItemId { get; set; }

    // Transient display fields (legacy partial members, not persisted)
    public string? CustomsItemTitle { get; set; }
    public string? MeasurementUnitName { get; set; }
}
