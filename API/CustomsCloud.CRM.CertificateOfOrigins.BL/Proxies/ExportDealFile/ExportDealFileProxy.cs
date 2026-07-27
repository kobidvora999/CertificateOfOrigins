using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore;
using CustomsCloud.InfrastructureCore.Proxy.Rest;
using System.Diagnostics.CodeAnalysis;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL.Proxies;

[ExcludeFromCodeCoverage]
public class ExportDealFileProxy(IRestProxy restProxy)
    : BaseMicroServiceProxyAdapter(restProxy, CustomsMicroServices.ExportDealFile), IExportDealFileProxy
{
    public async Task<ExportDeclarationDetailsDto?> GetExportDeclarationDetailsForCertificateOfOrigion(int? leadDocumentId, string? exportDeclarationNumber)
    {
        // TODO(blocking): the ExportDealFile microservice is not yet stood up — confirm the endpoint name/route
        // and switch this proxy to the real service (until then the mock is selected via x-mock-proxy).
        var req = CreateRequestBuilder()
            .UseGetMethod()
            .WithResource("ExportDealFile/ExportDeclarationDetailsForCertificateOfOrigin");
        if (leadDocumentId.HasValue)
        {
            req = req.AddQueryStringParameter("leadDocumentId", leadDocumentId.Value.ToString());
        }

        if (!string.IsNullOrEmpty(exportDeclarationNumber))
        {
            req = req.AddQueryStringParameter("exportDeclarationNumber", exportDeclarationNumber);
        }

        var response = await ExecuteAsync(req);
        return await response.GetResult<ExportDeclarationDetailsDto>();
    }
}
