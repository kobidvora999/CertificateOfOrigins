using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore;
using CustomsCloud.InfrastructureCore.Proxy.Rest;
using System.Diagnostics.CodeAnalysis;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL.Proxies;

[ExcludeFromCodeCoverage]
public class ExportDealFileProxy(IRestProxy restProxy)
    : BaseMicroServiceProxyAdapter(restProxy, CustomsMicroServices.ExportDealFile), IExportDealFileProxy
{
    public async Task<ExportDeclarationDetailsDto?> GetExportDeclarationDetailsForCertificateOfOrigin(int? leadDocumentId, string? exportDeclarationNumber)
    {
        var req = CreateRequestBuilder()
            .UseGetMethod()
            .WithResource("External/GetExportDeclarationDetailsForCertificateOfOrigin") // TODO(blocking): confirm endpoint name — no ExportDealFile microservice exists yet
            .AddQueryStringParameter("leadDocumentId", leadDocumentId)
            .AddQueryStringParameter("exportDeclarationNumber", exportDeclarationNumber);
        return (await ExecuteAsync<ExportDeclarationDetailsDto>(req)).Data;
    }
}
