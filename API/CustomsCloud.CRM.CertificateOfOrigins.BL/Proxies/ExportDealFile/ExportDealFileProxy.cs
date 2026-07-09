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
            .WithResource("ExportDealFile/ExportDeclarationDetailsForCertificateOfOrigin") // TODO(blocking): confirm endpoint name — no ExportDealFile microservice exists yet
            .AddQueryStringParameter("leadDocumentId", leadDocumentId)
            .AddQueryStringParameter("exportDeclarationNumber", exportDeclarationNumber);
        return (await ExecuteAsync<ExportDeclarationDetailsDto>(req)).Data;
    }

    public async Task<List<DetailsForExportAssociatedGoodsItemsDto>?> GetDetailsForExportAssociatedGoodsItemsByLeadDocumentId(int? leadDocumentId)
    {
        var req = CreateRequestBuilder()
            .UseGetMethod()
            .WithResource("ExportDealFile/DetailsForExportAssociatedGoodsItemsByLeadDocumentId") // TODO(blocking): confirm endpoint name — no ExportDealFile microservice exists yet
            .AddQueryStringParameter("leadDocumentId", leadDocumentId);
        return (await ExecuteAsync<List<DetailsForExportAssociatedGoodsItemsDto>>(req)).Data;
    }

    public async Task<ExportDeclarationInfoDto?> GetExportDeclarationInfoForPC(int? leadDocumentId)
    {
        var req = CreateRequestBuilder()
            .UseGetMethod()
            .WithResource("ExportDealFile/ExportDeclarationInfoForPC") // TODO(blocking): confirm endpoint name — no ExportDealFile microservice exists yet
            .AddQueryStringParameter("leadDocumentId", leadDocumentId);
        return (await ExecuteAsync<ExportDeclarationInfoDto>(req)).Data;
    }
}
