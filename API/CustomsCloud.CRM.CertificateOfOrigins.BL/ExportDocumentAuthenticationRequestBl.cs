using CustomsCloud.CRM.CertificateOfOrigins.BL.Proxies;
using CustomsCloud.CRM.CertificateOfOrigins.DAL;
using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore.BL;
using CustomsCloud.InfrastructureCore.BL.Exceptions;
using CustomsCloud.InfrastructureCore.Lookup;
using Dapper;
using Lookup;
using System.Data;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL;

public class ExportDocumentAuthenticationRequestBl(
    IServiceProvider serviceProvider,
    ICustomerProxy customerProxy,
    ILookupUtil lookupUtil)
    : BaseBL<ExportDocumentAuthenticationRequestBl, ICertificateOfOriginsDal>(serviceProvider)
{
    public async Task<CustomerDto> GetCustomerInformation(int customerId)
    {
        // Single-customer lookup against the Customers service by id; the legacy threw on a missing customer,
        // so a not-found id owns the 404 contract. Address selection was client-side (SPA), not in the BL.
        var customer = await customerProxy.GetCustomerInformation(customerId)
            ?? throw new RestNotFoundException();
        return customer;
    }

    public async Task<List<GetExportDocumentAuthenticationRequestSearchResultDto>> GetExportDocumentAuthenticationRequestSearch(ExportDocumentAuthenticationRequestSearchFilterDto filter)
    {
        var parameters = BuildParameterForProcedure(filter);
        var result = await DataLayer.GetExportDocumentAuthenticationRequestSearch(parameters);
        await FillExportRequestNames(result);
        return result;
    }

    private static DynamicParameters BuildParameterForProcedure(ExportDocumentAuthenticationRequestSearchFilterDto filter)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@CountryID", filter.CountryId, DbType.Int32);
        parameters.Add("@DocumentTypeID", filter.DocumentTypeId, DbType.Int32);
        parameters.Add("@RequestID", filter.RequestId, DbType.Int32);
        parameters.Add("@ForeignCustomsHouseID", filter.ForeignCustomsHouseId, DbType.Int32);
        parameters.Add("@ExportDeclarationID", filter.ExportDeclarationId, DbType.Int32);
        parameters.Add("@RequestOpenDateFrom", filter.RequestOpenDateFrom, DbType.DateTime);
        parameters.Add("@RequestOpenDateTo", filter.RequestOpenDateTo, DbType.DateTime);
        parameters.Add("@ExportAuthenticationDocumentID", filter.ExportAuthenticationDocumentId, DbType.Int32);
        parameters.Add("@InvoiceIDNum", filter.InvoiceIdNum, DbType.String);
        parameters.Add("@MainDocumentTitle", filter.MainDocumentTitle, DbType.String);
        parameters.Add("@ExporterCustomerID", filter.ExporterCustomerId, DbType.Int32);
        parameters.Add("@ExportAuthenticationRequestStatusID", filter.ExportAuthenticationRequestStatusId, DbType.Int32);
        parameters.Add("@CreateUserID", filter.CreateUserId, DbType.Int32);
        return parameters;
    }

    private async Task FillExportRequestNames(List<GetExportDocumentAuthenticationRequestSearchResultDto> results)
    {
        if (results.Count == 0)
        {
            return;
        }

        // ForeignCustomsHouseName (from CustomerId) + RequestIssuerName (from ExporterCustomerId) — both Customers proxy.
        var customerIds = results.Where(r => r.CustomerId.HasValue).Select(r => r.CustomerId!.Value)
            .Concat(results.Where(r => r.ExporterCustomerId.HasValue).Select(r => r.ExporterCustomerId!.Value))
            .Distinct()
            .ToList();
        if (customerIds.Count > 0)
        {
            var customers = await customerProxy.GetCustomersByIds(customerIds);
            if (customers != null)
            {
                var customersById = customers.ToDictionary(c => c.Id);
                foreach (var result in results)
                {
                    if (result.CustomerId.HasValue && customersById.TryGetValue(result.CustomerId.Value, out var foreignCustomsHouse))
                    {
                        result.ForeignCustomsHouseName = foreignCustomsHouse.Name;
                    }

                    if (result.ExporterCustomerId.HasValue && customersById.TryGetValue(result.ExporterCustomerId.Value, out var issuer))
                    {
                        result.RequestIssuerName = issuer.Name;
                    }
                }
            }
        }

        // CountryName via the shared Country lookup (raw id in CountryId).
        await lookupUtil.FillName<Country, GetExportDocumentAuthenticationRequestSearchResultDto>(
            results,
            r => r.CountryId ?? 0,
            (r, name) => r.CountryName = name);
    }
}
