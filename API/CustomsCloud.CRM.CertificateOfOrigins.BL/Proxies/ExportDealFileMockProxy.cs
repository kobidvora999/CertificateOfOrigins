using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;
using System.Diagnostics.CodeAnalysis;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL.Proxies;

[ExcludeFromCodeCoverage]
public class ExportDealFileMockProxy : IExportDealFileProxy
{
    // Returns hardcoded dummy data — used while the ExportDealFile (export declarations) service is unavailable.
    public Task<ExportDeclarationDetailsDto?> GetExportDeclarationDetailsForCertificateOfOrigin(int? leadDocumentId, string? exportDeclarationNumber)
    {
        return Task.FromResult<ExportDeclarationDetailsDto?>(new ExportDeclarationDetailsDto
        {
            LeadDocumentId = leadDocumentId ?? 0,
            IsDeclarationReleased = true,
            IsCargoExitedOfCustomsRegulation = true,
            IsDeclarationInAmendmentProcess = false,
            LeadDocumentStateId = 1
        });
    }
}
