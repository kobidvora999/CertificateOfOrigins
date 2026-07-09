using CustomsCloud.CRM.CertificateOfOrigins.BL.Proxies;
using CustomsCloud.CRM.CertificateOfOrigins.DAL;
using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore;
using CustomsCloud.InfrastructureCore.BL;
using CustomsCloud.InfrastructureCore.BL.Exceptions;
using CustomsCloud.InfrastructureCore.Lookup;
using CustomsCloud.InfrastructureCore.Lookup.Entities;
using Dapper;
using System.Data;

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
    private static DynamicParameters BuildParameterForProcedure(ExportDocumentAuthenticationRequestSearchFilterDto filter)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@CountryID", filter.CountryId, DbType.Int32);
        parameters.Add("@DocumentTypeID", filter.DocumentTypeId, DbType.Int32);
        parameters.Add("@RequestID", filter.RequestId, DbType.Int32);
        parameters.Add("@ForeignCustomsHouseID", filter.ForeignCustomsHouseCustomerId, DbType.Int32);
        parameters.Add("@ExportDeclarationID", filter.ExportDeclarationId, DbType.Int32);
        parameters.Add("@RequestOpenDateFrom", filter.RequestOpenDateFrom, DbType.DateTime);
        parameters.Add("@RequestOpenDateTo", filter.RequestOpenDateTo, DbType.DateTime);
        parameters.Add("@ExportAuthenticationDocumentID", filter.ExportAuthenticationDocumentId, DbType.Int32);
        parameters.Add("@InvoiceIDNum", filter.InvoiceIdNum, DbType.String);
        parameters.Add("@MainDocumentTitle", filter.MainDocumentTitle, DbType.String);
        parameters.Add("@ExporterCustomerID", filter.ExporterId, DbType.Int32);
        parameters.Add("@ExportAuthenticationRequestStatusID", filter.ExportAuthenticationRequestStatusId, DbType.Int32);
        parameters.Add("@CreateUserID", filter.CreateUserId, DbType.Int32);
        return parameters;
    }

    public async Task<List<ExportDocumentAuthenticationRequestSearchResultDto>> GetExportDocumentAuthenticationRequestSearch(ExportDocumentAuthenticationRequestSearchFilterDto filter)
    {
        var parameters = BuildParameterForProcedure(filter);
        var result = await DataLayer.GetExportDocumentAuthenticationRequestSearch(parameters);
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

    #region LEGACY_WCF

    // public ExportDocumentAuthenticationRequest GetExportDocumentAuthenticationRequestByID(int id)
    // {
    //     var result = GetQuery<ExportDocumentAuthenticationRequest>().Single(edar => edar.ID == id); // threw when missing
    //     result.OriginalStatusID = result.StatusID ?? 0;
    //     LoadProperty ×3: CustomsItemToExportDocumentAuthenticationRequest,
    //                      ExportDocumentAuthenticationRequestLeadDocument,
    //                      ExportAuthenticationRequestManufacturingArea;
    //     result.EntityTypeAndIDsToSearch[EEntityType.ExportDeclaration] =
    //         leadDocuments.Where(LeadDocumentID != null).Select(LeadDocumentID);
    //     Migrated: route-style endpoint — not-found throws RestNotFoundException → 404 (C1).
    // }
    #endregion
    public async Task<ExportDocumentAuthenticationRequestDto?> GetExportDocumentAuthenticationRequestByID(int id)
    {
        var result = await DataLayer.GetExportDocumentAuthenticationRequestById(id);
        if (result == null)
        {
            throw new RestNotFoundException(); // route-style endpoint → 404 (C1)
        }

        result.OriginalStatusId = result.StatusId ?? 0;
        result.EntityTypeAndIdsToSearch = new Dictionary<int, List<int>>
        {
            [12414] = result.ExportDocumentAuthenticationRequestLeadDocument // EntityType.ExportDeclaration = 12414
                .Where(l => l.LeadDocumentId.HasValue)
                .Select(l => l.LeadDocumentId!.Value)
                .ToList()
        };
        return result;
    }

    #region LEGACY_WCF

    // public CustomerDTO GetCustomerInformation(int customerId)
    // {
    //     var customerDto = customerAdapter.GetCustomerDTOByCustomerIdentification(
    //         new CustomerIdentificationFilter { ExternalId = null, CustomerId = customerId });
    //     if (customerDto == null) { throw new InfException(EMessages.InvalidIdentificationNumber /*2345*/); }
    //     return customerDto;
    // }
    #endregion
    public async Task<CustomerDto> GetCustomerInformation(int customerId)
    {
        var customer = await customerProxy.GetCustomerByIdentification(customerId);
        if (customer == null)
        {
            // legacy: EMessages.InvalidIdentificationNumber (2345) — "הלקוח לא קיים במערכת" → 404 (C1)
            throw new RestNotFoundException();
        }

        return customer;
    }

    #region LEGACY_WCF

    // public CustomerDTO GetCustomerInformationByCountry(int countryId)
    // {
    //     var customers = customerAdapter.GetCustomeDTOByCountryID(countryId, (int)ECustomerActivityType.Foreign_customs_house);
    //     if (customers.IsNullOrEmpty()) { throw new InfException(EMessages.NoCustomHouseForThisCountry /*13717*/); }
    //     return customers.FirstOrDefault();
    // }
    #endregion
    public async Task<CustomerDto> GetCustomerInformationByCountry(int countryId)
    {
        var customers = await customerProxy.GetCustomersByCountryId(countryId, 40); // ECustomerActivityType.Foreign_customs_house = 40
        if (customers == null || customers.Count == 0)
        {
            // legacy: EMessages.NoCustomHouseForThisCountry (13717) — "לא הוגדר בית מכס למדינה זו. יש להגדיר כתובת מתאימה" → 404 (C1)
            throw new RestNotFoundException();
        }

        var result = customers.First();
        return result;
    }
}
