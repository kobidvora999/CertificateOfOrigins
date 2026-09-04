using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore;
using CustomsCloud.InfrastructureCore.Proxy.Rest;
using System.Diagnostics.CodeAnalysis;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL.Proxies;

[ExcludeFromCodeCoverage]
public class ExportDealFileProxy(IHttpProxy httpProxy)
    : BaseCustomsProxy(httpProxy, CustomsMicroServices.ExportDealFile), IExportDealFileProxy
{
    public async Task<ExportDeclarationDetailsDto?> GetExportDeclarationDetailsForCertificateOfOrigion(int? leadDocumentId, string? exportDeclarationNumber)
    {
        // TODO(blocking): the ExportDealFile microservice is not yet stood up — confirm the endpoint name/route
        // and switch this proxy to the real service (until then the mock is enabled via x-mock-mode).
        var req = CreateRequestBuilder()
            .UseGetMethod()
            .WithResource("api/ExportDealFile/ExportDeclarationDetailsForCertificateOfOrigin");
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

    // Legacy: CRP.DealFile_LeadDocumentSubmissionData.SubmitDate (LEFT JOIN by LeadDocumentID) — the lead document's
    // submission date from the DealFile service.
    public async Task<DateTimeOffset?> GetLeadDocumentSubmissionDate(int leadDocumentId)
    {
        var req = CreateRequestBuilder()
            .UseGetMethod()
            .WithResource($"api/ExportDealFile/LeadDocumentSubmissionDate/{leadDocumentId}"); // TODO(blocking): confirm endpoint name/route with the DealFile microservice
        var response = await ExecuteAsync(req);
        return await response.GetResult<DateTimeOffset?>();
    }

    public async Task<LeadDocumentByCertificateOfOriginDto?> GetLeadDocumentByOldCertificateOfOriginIdAndUpdateToNewCertificateOfOriginId(int oldCertificateOfOriginId, int newCertificateOfOriginId)
    {
        var req = CreateRequestBuilder()
            .UsePostMethod()
            .WithResource($"api/ExportDealFile/LeadDocumentByCertificateOfOrigin/{oldCertificateOfOriginId}/{newCertificateOfOriginId}"); // TODO(blocking): confirm endpoint name/route with the DealFile microservice
        var response = await ExecuteAsync(req);
        return await response.GetResult<LeadDocumentByCertificateOfOriginDto>();
    }

    public async Task<LeadDocumentByCertificateOfOriginDto?> GetLeadDocumentByCertificateOfOriginId(int certificateOfOriginId)
    {
        var req = CreateRequestBuilder()
            .UseGetMethod()
            .WithResource($"api/ExportDealFile/LeadDocumentByCertificateOfOriginId/{certificateOfOriginId}"); // TODO(blocking): confirm endpoint name/route with the DealFile microservice
        var response = await ExecuteAsync(req);
        return await response.GetResult<LeadDocumentByCertificateOfOriginDto>();
    }

    public async Task ChangeCertificateOfOriginIdForLeadDocument(int leadDocumentId, int oldCertificateOfOriginId, int newCertificateOfOriginId)
    {
        var req = CreateRequestBuilder()
            .UsePostMethod()
            .WithResource($"api/ExportDealFile/ChangeCertificateOfOriginForLeadDocument/{leadDocumentId}/{oldCertificateOfOriginId}/{newCertificateOfOriginId}"); // TODO(blocking): confirm endpoint name/route with the DealFile microservice
        await ExecuteAsync(req);
    }

    public async Task<ExportDeclarationInfoDto?> GetExportDeclarationInfoForPc(int leadDocumentId)
    {
        var req = CreateRequestBuilder()
            .UseGetMethod()
            .WithResource($"api/ExportDealFile/ExportDeclarationInfo/{leadDocumentId}"); // TODO(blocking): confirm endpoint name/route with the DealFile microservice
        var response = await ExecuteAsync(req);
        return await response.GetResult<ExportDeclarationInfoDto>();
    }

    public async Task<List<ExportAssociatedGoodsItemDto>?> GetDetailsForExportAssociatedGoodsItemsByLeadDocumentId(int leadDocumentId)
    {
        var req = CreateRequestBuilder()
            .UseGetMethod()
            .WithResource($"api/ExportDealFile/DetailsForExportAssociatedGoodsItems/{leadDocumentId}"); // TODO(blocking): confirm endpoint name/route with the DealFile microservice
        var response = await ExecuteAsync(req);
        return await response.GetResult<List<ExportAssociatedGoodsItemDto>>();
    }
}
