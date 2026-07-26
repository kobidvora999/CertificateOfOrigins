using CustomsCloud.CRM.CertificateOfOrigins.BL.Proxies;
using CustomsCloud.CRM.CertificateOfOrigins.DAL;
using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore.BL;
using CustomsCloud.InfrastructureCore.Lookup;
using Dapper;
using Lookup;
using System.Data;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL;

public class AuthenticationRequestBl(
    IServiceProvider serviceProvider,
    ICustomerProxy customerProxy,
    IVendorProxy vendorProxy,
    ILookupUtil lookupUtil)
    : BaseBL<AuthenticationRequestBl, ICertificateOfOriginsDal>(serviceProvider)
{
    public async Task<List<GetImportAuthenticationRequestResultDto>> GetAuthenticationRequestByFilter(ImportAuthenticationRequestFilterDto filter)
    {
        var parameters = BuildParameterForProcedure(filter);
        var result = await DataLayer.GetImportAuthenticationRequestByFilter(parameters);
        await FillAuthenticationRequestNames(result);
        return result;
    }

    private static DynamicParameters BuildParameterForProcedure(ImportAuthenticationRequestFilterDto filter)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@PrefernceDocumentType", filter.PreferenceDocumentType, DbType.Int32);
        parameters.Add("@GoodsOrigionCountry", filter.GoodsOriginCountry, DbType.Int32);
        parameters.Add("@IssuingCountry", filter.IssuingCountry, DbType.Int32);
        parameters.Add("@ImportCountry", filter.ImportCountry, DbType.Int32);
        parameters.Add("@FromRequestDate", filter.FromRequestDate, DbType.DateTime);
        parameters.Add("@ToRequestDate", filter.ToRequestDate, DbType.DateTime);
        parameters.Add("@CustomsHouseID", filter.CustomsHouseId, DbType.Int32);
        parameters.Add("@RequestReason", filter.RequestReason, DbType.Int32);
        parameters.Add("@leadDocumentID", filter.LeadDocumentId, DbType.Int32);
        parameters.Add("@ImporterID", filter.ImporterId, DbType.Int32);
        parameters.Add("@VendorID", filter.VendorId, DbType.Int32);
        parameters.Add("@DecisionID", filter.DecisionId, DbType.Int32);
        parameters.Add("@CustomerID", filter.CustomerId, DbType.Int32);
        parameters.Add("@DocumentID", filter.DocumentId, DbType.Int32);
        parameters.Add("@InvoiceNumber", filter.InvoiceNumber, DbType.String);
        parameters.Add("@DocumentNumber", filter.DocumentNumber, DbType.String);
        parameters.Add("@AuthenticationFileID", filter.AuthenticationFileId, DbType.Int32);
        parameters.Add("@CreateUserID", filter.CreateUserId, DbType.Int32);
        return parameters;
    }

    private async Task FillAuthenticationRequestNames(List<GetImportAuthenticationRequestResultDto> requests)
    {
        if (requests.Count == 0)
        {
            return;
        }

        // ImporterName — the importer id arrives in CustomerId (SP: R.ImporterID AS CustomerID); Customers proxy.
        var importerIds = requests.Where(r => r.CustomerId.HasValue).Select(r => r.CustomerId!.Value).Distinct().ToList();
        if (importerIds.Count > 0)
        {
            var customers = await customerProxy.GetCustomersByIds(importerIds);
            if (customers != null)
            {
                var customersById = customers.ToDictionary(c => c.Id);
                foreach (var request in requests)
                {
                    if (request.CustomerId.HasValue && customersById.TryGetValue(request.CustomerId.Value, out var importer))
                    {
                        request.ImporterName = importer.Name;
                    }
                }
            }
        }

        // VendorName — Vendors proxy.
        var vendorIds = requests.Where(r => r.VendorId.HasValue).Select(r => r.VendorId!.Value).Distinct().ToList();
        if (vendorIds.Count > 0)
        {
            var vendors = await vendorProxy.GetVendorsByIds(vendorIds);
            if (vendors != null)
            {
                var vendorsById = vendors.ToDictionary(v => v.Id);
                foreach (var request in requests)
                {
                    if (request.VendorId.HasValue && vendorsById.TryGetValue(request.VendorId.Value, out var vendor))
                    {
                        request.VendorName = vendor.Name;
                    }
                }
            }
        }

        // IssuingCountry name — shared Country lookup (raw id in IssuingCountryIdNum).
        await lookupUtil.FillName<Country, GetImportAuthenticationRequestResultDto>(
            requests,
            r => r.IssuingCountryIdNum ?? 0,
            (r, name) => r.IssuingCountryId = name);

        // OrganizationUnit name — shared OrganizationUnit lookup (raw id in OrganizationUnitIdNum).
        await lookupUtil.FillName<OrganizationUnit, GetImportAuthenticationRequestResultDto>(
            requests,
            r => r.OrganizationUnitIdNum ?? 0,
            (r, name) => r.OrganizationUnitId = name);

        // TODO(migration): LeadDocumentTitle stays null — it's a CRP.DealFile document (no lookup type; needs the
        // owning service's proxy, not yet established). The raw LeadDocumentId is returned for a later pass.
    }

    public async Task<List<GetAuthenticationRequestByLeadDocumentResultDto>> GetAuthenticationRequestByLeadDocumentIDs(List<int> leadDocumentIds)
    {
        var parameters = BuildLeadDocumentIdsParameter(leadDocumentIds);
        var result = await DataLayer.GetAuthenticationRequestByLeadDocumentIDs(parameters);
        await FillLeadDocumentRequestNames(result);
        return result;
    }

    private static DynamicParameters BuildLeadDocumentIdsParameter(List<int> leadDocumentIds)
    {
        // Pass the id list as the Shared.IntArray table-valued parameter (@LeadDocumentIDs).
        var table = new DataTable();
        table.Columns.Add("val", typeof(int));
        if (leadDocumentIds != null)
        {
            foreach (var id in leadDocumentIds)
            {
                table.Rows.Add(id);
            }
        }

        var parameters = new DynamicParameters();
        parameters.Add("@LeadDocumentIDs", table.AsTableValuedParameter("Shared.IntArray"));
        return parameters;
    }

    private async Task FillLeadDocumentRequestNames(List<GetAuthenticationRequestByLeadDocumentResultDto> results)
    {
        if (results.Count == 0)
        {
            return;
        }

        // ImportCountryName + OrganizationUnitName via the shared lookups (raw ids returned by the SP).
        await lookupUtil.FillName<Country, GetAuthenticationRequestByLeadDocumentResultDto>(
            results,
            r => r.ImportCountryId ?? 0,
            (r, name) => r.ImportCountryName = name);

        await lookupUtil.FillName<OrganizationUnit, GetAuthenticationRequestByLeadDocumentResultDto>(
            results,
            r => r.OrganizationUnitId ?? 0,
            (r, name) => r.OrganizationUnitName = name);

        // TODO(migration): LeadDocumentTitle stays null — CRP.DealFile document, needs the owning service's proxy.
    }

    public async Task<int?> CheckImporterOfImportAuthentication(int importerId)
    {
        var result = await DataLayer.CheckImporterOfImportAuthentication(importerId);
        return result;
    }

    public async Task<bool> CheckIfExistsAdditionalRequestsForVendor(int vendorId)
    {
        var result = await DataLayer.CheckIfExistsAdditionalRequestsForVendor(vendorId);
        return result;
    }

    // The WCF took the full ImportAuthenticationRequest entity but used only these 4 scalar fields
    // (ImporterID, VendorId, CustomerID, IssuingCountryID). The @DaysForLastDelivery window stays inside the SP
    // (read from the local Infrastructure.Parameters), so it is not a BL/config concern here.
    public async Task<bool> CheckIfExistsAdditionalRequestsForImporter(int importerId, int? vendorId, int? customerId, int countryId)
    {
        var result = await DataLayer.CheckIfExistsAdditionalRequestsForImporter(importerId, vendorId, customerId, countryId);
        return result;
    }

    #region LEGACY_WCF

    // Original WCF (AuthenticationRequestBL.CheckImporterOfImportAuthentication):
    //
    // public int? CheckImporterOfImportAuthentication(int importerId)
    // {
    //     return _uow.Repository.GetQuery<VerificationProhibitedImporters>()
    //         .FirstOrDefault(c => c.CustomerId == importerId)?.ID == null ? importerId : (int?)null;
    // }
    //
    // Returns the importer id when the importer is NOT on the prohibited list; null when it is.
    #endregion
}
