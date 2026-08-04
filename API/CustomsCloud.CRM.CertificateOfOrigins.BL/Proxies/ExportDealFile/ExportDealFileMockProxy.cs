using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore.Proxy.Rest;
using System.Diagnostics.CodeAnalysis;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL.Proxies;

// The real ExportDealFile service is not yet stood up, so this mock is the practical default for local/test
// runs (selected via x-mock-proxy). Default = a released declaration whose cargo has exited customs regulation
// (so LoadDataFromExportDeclaration can return true); feature "ExportDealFile.NotFound" flips to no details.
[ExcludeFromCodeCoverage]
public class ExportDealFileMockProxy(IProxyMockUtil mockUtil) : IExportDealFileProxy, IMockProxy
{
    public Task<ExportDeclarationDetailsDto?> GetExportDeclarationDetailsForCertificateOfOrigion(int? leadDocumentId, string? exportDeclarationNumber)
    {
        if (mockUtil.HasMockFeature("ExportDealFile.NotFound"))
        {
            return Task.FromResult<ExportDeclarationDetailsDto?>(null);
        }

        var result = new ExportDeclarationDetailsDto
        {
            LeadDocumentId = leadDocumentId ?? 0,          // TODO: dummy data
            IsDeclarationReleased = true,                  // TODO: dummy data
            IsCargoExitedOfCustomsRegulation = true,       // TODO: dummy data
            IsDeclarationInAmendmentProcess = false,
            LeadDocumentStateId = 0,                       // TODO: dummy data
        };
        return Task.FromResult<ExportDeclarationDetailsDto?>(result);
    }

    // Default = a fixed submission date; feature "DealFile.NoSubmissionDate" returns none.
    public Task<DateTimeOffset?> GetLeadDocumentSubmissionDate(int leadDocumentId)
    {
        if (mockUtil.HasMockFeature("DealFile.NoSubmissionDate"))
        {
            return Task.FromResult<DateTimeOffset?>(null);
        }

        return Task.FromResult<DateTimeOffset?>(new DateTimeOffset(2026, 1, 15, 0, 0, 0, TimeSpan.Zero)); // TODO: dummy data
    }
}
