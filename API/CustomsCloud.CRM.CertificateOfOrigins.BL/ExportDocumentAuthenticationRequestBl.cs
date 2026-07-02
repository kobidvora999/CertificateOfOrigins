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
    //     The migrated version returns null when not found (consistent with the repo's Get-by-id precedent).
    // }
    #endregion
    public async Task<ExportDocumentAuthenticationRequestDto?> GetExportDocumentAuthenticationRequestByID(int id)
    {
        var result = await DataLayer.GetExportDocumentAuthenticationRequestById(id);
        if (result == null)
        {
            return null;
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
            // legacy: EMessages.InvalidIdentificationNumber (2345) — "הלקוח לא קיים במערכת"
            throw new RestValidationException(nameof(customerId), "הלקוח לא קיים במערכת", "404");
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
            // legacy: EMessages.NoCustomHouseForThisCountry (13717) — "לא הוגדר בית מכס למדינה זו. יש להגדיר כתובת מתאימה"
            throw new RestValidationException(nameof(countryId), "לא הוגדר בית מכס למדינה זו. יש להגדיר כתובת מתאימה", "404");
        }

        var result = customers.First();
        return result;
    }
}
