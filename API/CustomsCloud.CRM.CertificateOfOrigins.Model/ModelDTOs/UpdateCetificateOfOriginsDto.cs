namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// Legacy: Customs.CRM.CertificateOfOrigins.ExternalCommon.Common.UpdateCetificateOfOriginsDTO.
// The legacy "Cetificate" spelling is preserved in the class name (contract recognizability);
// property names follow .NET 10 conventions (Id, no DTO suffix). Legacy "DestinationCoutryID" typo fixed.
public class UpdateCetificateOfOriginsDto
{
    public EEventType EventType { get; set; }

    public int? LeadDocumentId { get; set; }

    public string? ExportDeclarationNum { get; set; }

    public List<int> CertificateOfOriginsIds { get; set; } = [];

    public int? ExporterCustomerId { get; set; }

    public int? DestinationCountryId { get; set; }

    public int OrganizationUnitId { get; set; }

    public List<ExportInvoiceInfoDto>? ExportInvoiceInfoList { get; set; }
}
