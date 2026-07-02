using CustomsCloud.CRM.CertificateOfOrigins.BL.Proxies;
using CustomsCloud.CRM.CertificateOfOrigins.DAL;
using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore.BL;
using Dapper;
using System.Data;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL;

public class CertificateOfOriginBl(
    IServiceProvider serviceProvider,
    ICustomerProxy customerProxy)
    : BaseBL<CertificateOfOriginBl, ICertificateOfOriginDal>(serviceProvider)
{
    #region LEGACY_WCF
    // public List<CertificateOfOriginResult> GetCertificateOfOriginsByFilter(CertificateOfOriginFilter filter)
    // {
    //     var sqlParameters = new List<SqlParameter> { ... 16 parameters, one per filter field, each ?? DBNull.Value ... };
    //     var result = _uow.Repository.ExecuteFunction<CertificateOfOriginResult>(
    //         CertificateOfOriginsConsts.GetCertificateOfOriginsByFilterSP, sqlParameters);
    //     return (List<CertificateOfOriginResult>)result;
    // }
    // The legacy SP [CRM].[usp_CertificateOfOrigins_GetCertificateOfOriginsByFilter] assembled a dynamic
    // WHERE over CRM.CertificateOfOrigins_CertificateOfOrigin (one optional predicate per non-null filter
    // field), joined Shared.rStockPileData_Customers_Customer for exporter/agent titles and the local
    // status-code enum table for the status name, TOP(ufn_GetMaxRows()) ordered by CreateDate DESC.
    // Migrated to LINQ in the DAL; customer titles are enriched via the Customers microservice because
    // Shared.rStockPileData_Customers_Customer is not replicated to this service's DB.
    #endregion
    public async Task<List<CertificateOfOriginResultDto>> GetCertificateOfOriginsByFilter(CertificateOfOriginFilterDto filter)
    {
        var result = await DataLayer.GetCertificateOfOriginsByFilter(filter);
        await FillCustomersInformation(result);
        return result;
    }

    #region LEGACY_WCF

    // public CertificateOfOriginResult IsCertificateOfOriginByExternalIdExist(string certificateOfOriginExternalId)
    // {
    //     var filter = new CertificateOfOriginFilter { certificateNumber = certificateOfOriginExternalId };
    //     var result = _uow.Repository.ExecuteFunction<CertificateOfOriginResult>(filter).FirstOrDefault();
    //     return result;
    // }
    #endregion
    public async Task<CertificateOfOriginResultDto?> IsCertificateOfOriginByExternalIdExist(string certificateOfOriginExternalId)
    {
        var filter = new CertificateOfOriginFilterDto { CertificateNumber = certificateOfOriginExternalId };
        var certificates = await GetCertificateOfOriginsByFilter(filter);
        var result = certificates.FirstOrDefault();
        return result;
    }

    private async Task FillCustomersInformation(List<CertificateOfOriginResultDto> certificates)
    {
        if (certificates.Count == 0)
        {
            return;
        }

        var customerIds = certificates.Select(c => c.ExporterId)
            .Concat(certificates.Select(c => c.CustomesAgentId))
            .Distinct()
            .ToList();
        var customers = await customerProxy.GetCustomersByIds(customerIds);
        if (customers == null)
        {
            return;
        }

        var customersById = customers.ToDictionary(c => c.Id);
        foreach (var certificate in certificates)
        {
            if (customersById.TryGetValue(certificate.ExporterId, out var exporter))
            {
                certificate.ExporterTitle = exporter.Name;
                certificate.ExporterExternalIdNum = exporter.ExternalIdNum;
            }

            if (customersById.TryGetValue(certificate.CustomesAgentId, out var agent))
            {
                certificate.CustomesAgentTitle = agent.Name;
                certificate.CustomesAgentExternalIdNum = agent.ExternalIdNum;
            }
        }
    }
}
