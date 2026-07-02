using CustomsCloud.CRM.CertificateOfOrigins.BL.Proxies;
using CustomsCloud.CRM.CertificateOfOrigins.DAL;
using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore;
using CustomsCloud.InfrastructureCore.BL;
using CustomsCloud.InfrastructureCore.BL.Exceptions;
using CustomsCloud.InfrastructureCore.Lookup;
using CustomsCloud.InfrastructureCore.Lookup.Entities;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL;

public class ExportDocumentAuthenticationRequestBl(
    IServiceProvider serviceProvider,
    ICustomerProxy customerProxy,
    ILookupUtil lookupUtil)
    : BaseBL<ExportDocumentAuthenticationRequestBl, ICertificateOfOriginDal>(serviceProvider)
{
    #region LEGACY_WCF

    // public List<ExportDocumentAuthenticationRequestSearchResult> GetExportDocumentAuthenticationRequestSearch(filter)
    // {
    //     ExecuteResultSetFunction([CRM].[usp_CertificateOfOrigins_CROSS_ExportDocumentAuthenticationRequestSearch],
    //         filter.GetSQLParams()); // dynamic-SQL search over CRM.CertificateOfOrigins_ExportDocumentAuthenticationRequest
    //     — joins: Country (name), Customers ×2 (foreign customs house + exporter names),
    //       PrefernceDocumentType/ExportAuthenticationRequestStatus enums (names), OUTER APPLY first lead-doc title.
    //     Converted to LINQ per module guidelines; country name via ILookupUtil, customer names via Customers proxy.
    // }
    #endregion
    public async Task<List<ExportDocumentAuthenticationRequestSearchResultDto>> GetExportDocumentAuthenticationRequestSearch(ExportDocumentAuthenticationRequestSearchFilterDto filter)
    {
        var result = await DataLayer.GetExportDocumentAuthenticationRequestSearch(filter);
        if (result.Count == 0)
        {
            return result;
        }

        var countries = await lookupUtil.All<Country>();

        var customerIds = result.Select(r => r.CustomerId)
            .Concat(result.Where(r => r.ExporterCustomerId.HasValue).Select(r => r.ExporterCustomerId!.Value))
            .Distinct()
            .ToList();
        var customers = customerIds.Count > 0 ? await customerProxy.GetCustomersByIds(customerIds) : null;

        foreach (var request in result)
        {
            request.CountryName = countries?.FirstOrDefault(c => c.Id == request.CountryId)?.Name;
            request.ForeignCustomsHouseName = customers?.FirstOrDefault(c => c.Id == request.CustomerId)?.Name;
            request.RequestIssuerName = customers?.FirstOrDefault(c => c.Id == request.ExporterCustomerId)?.Name;
        }

        return result;
    }
}
