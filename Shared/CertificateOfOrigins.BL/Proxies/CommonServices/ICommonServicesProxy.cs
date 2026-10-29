using CertificateOfOrigins.Model.ModelDTOs;

namespace CertificateOfOrigins.BL.Proxies;

// Legacy ICommonServicesServiceAdapter — the Common microservice's document-generation surface (QR code + SSRS
// reports). Both live on the same service, so they share one proxy.
public interface ICommonServicesProxy
{
    // Legacy: CreateQRCode(url) (SaveCertificateOfOrigin, on publish) — renders the certificate's query URL into a
    // QR-code image (bytes), later uploaded as a document.
    Task<byte[]?> CreateQrCode(string url);

    // Legacy: GenerateReportByParams(reportID, reportParameters, fileFormat), called by the adapter's
    // GanerateReportAndConvertToTemplateResult (SaveCertificateOfOrigin, on publish) — renders an SSRS report and
    // returns the file bytes. This is the reports service, not the template-generation service.
    Task<byte[]?> GenerateReportByParams(GenerateReportByParamsRequestDto request);
}
