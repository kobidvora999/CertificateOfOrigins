namespace CertificateOfOrigins.Model.ModelDTOs;

// Legacy ICommonServicesExternalProxy.GenerateReportByParams(reportID, reportParameters, fileFormat) — the Common
// service's SSRS report rendering. Not the template-generation service: the certificate is an SSRS report.
public class GenerateReportByParamsRequestDto
{
    // The SSRS report id (the certificate type's ReportId, 7000-7007).
    public int ReportId { get; set; }

    // The report parameters by name (legacy List<KeyValuePair<string, string>>: EntityID, AdditionalInfo).
    public List<KeyValuePair<string, string>> ReportParameters { get; set; } = [];

    // The output format SSRS renders to (legacy passed "PDF").
    public string FileFormat { get; set; } = string.Empty;
}
