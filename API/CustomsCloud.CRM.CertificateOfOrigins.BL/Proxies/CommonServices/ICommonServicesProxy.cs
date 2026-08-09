using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL.Proxies;

// Legacy ICommonServicesServiceAdapter — the Common microservice's document-generation surface (QR code + certificate
// templates). Both live on the same service, so they share one proxy.
public interface ICommonServicesProxy
{
    // Legacy: CreateQRCode(url) (SaveCertificateOfOrigin, on publish) — renders the certificate's query URL into a
    // QR-code image (bytes), later uploaded as a document.
    Task<byte[]?> CreateQrCode(string url);

    // Legacy: CreateTemplateSync(certificateId, templateId, additionalInfo) / GanerateReportAndConvertToTemplateResult
    // (SaveCertificateOfOrigin, on publish) — renders the certificate document for the given template/report id.
    Task<TemplateResultDto?> GenerateTemplate(int templateId, int certificateOfOriginId, string additionalInfo);
}
