using CustomsCloud.CRM.CertificateOfOrigins.BL.Proxies;
using CustomsCloud.CRM.CertificateOfOrigins.DAL;
using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore.BL;
using CustomsCloud.InfrastructureCore.Lookup;
using CustomsCloud.InfrastructureCore.Lookup.Entities;
using Dapper;
using System.Data;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL;

public class CertificateOfOriginBl(
    IServiceProvider serviceProvider,
    ICustomerProxy customerProxy,
    IUserProxy userProxy,
    ILookupUtil lookupUtil)
    : BaseBL<CertificateOfOriginBl, ICertificateOfOriginDal>(serviceProvider)
{
    #region LEGACY_WCF

    // public List<ImportAuthenticationRequestDTO> GetAuthenticationRequestByLeadDocumentIDs(List<int> leadDocumentIDs)
    // {
    //     TVP Shared.IntArray → ExecuteFunction<ImportAuthenticationRequestDTO>(
    //         [CRM].[usp_CertificateOfOrigins_GetAuthenticationRequestByLeadDocumentID], @LeadDocumentIDs);
    //     — SP joined local tables (requests, file details, PrefernceDocumentType/Decision/FileStatus enums)
    //       plus Shared.General_c_Country + Infrastructure.UserMng_OrganizationUnit (names) and
    //       CRP.DealFile_LeadDocument (LeadDocumentTitle). Converted to LINQ per module guidelines;
    //       country/org-unit names enriched via ILookupUtil; ImporterID/LastDeliveryForImporter were never
    //       populated by the legacy SP (parity preserved).
    // }
    #endregion
    public async Task<List<ImportAuthenticationRequestByLeadDocumentDto>> GetAuthenticationRequestByLeadDocumentIDs(List<int> leadDocumentIDs)
    {
        if (leadDocumentIDs == null || leadDocumentIDs.Count == 0)
        {
            return new List<ImportAuthenticationRequestByLeadDocumentDto>();
        }

        var result = await DataLayer.GetAuthenticationRequestsByLeadDocumentIds(leadDocumentIDs);
        if (result.Count == 0)
        {
            return result;
        }

        var countries = await lookupUtil.All<Country>();
        var organizationUnits = await lookupUtil.All<OrganizationUnit>();
        foreach (var request in result)
        {
            request.ImportCountryName = countries?.FirstOrDefault(c => c.Id == request.ImportCountryId)?.Name;
            request.OrganizationUnitName = organizationUnits?.FirstOrDefault(o => o.Id == request.OrganizationUnitId)?.Name;

            // TODO (blocking): LeadDocumentTitle came from CRP.DealFile_LeadDocument — no DealFile microservice;
            // the data source must be decided with the team.
            request.LeadDocumentTitle = null;
        }

        return result;
    }

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

    #region LEGACY_WCF

    // public CertificateOfOrigin GetCertificateOfOriginById(int certificateOfOriginId, bool isFromMessage = false)
    //     => GetCertificateOfOriginByIdSP(certificateOfOriginId);
    // GetCertificateOfOriginByIdSP executed [CRM].[usp_CertificateOfOrigins_GetCertificateOfOriginByID]
    // (7 result sets: certificate, vs-declaration errors, full details-type-code enum table, details,
    // invoices, item details, milestones) and assembled the graph in MaterializeForCertificateOfOrigin,
    // then set StakeholdersIDs = [CustomerID, CreateCustomerID].
    // The milestones result set joined Infrastructure.UserMng_User for user names — that table is not
    // replicated to this service's DB, so user names are enriched via the Users microservice instead.
    #endregion
    public async Task<CertificateOfOriginDto?> GetCertificateOfOriginById(int certificateOfOriginId)
    {
        var certificate = await DataLayer.GetCertificateOfOriginById(certificateOfOriginId);
        if (certificate == null)
        {
            return null;
        }

        certificate.StakeholdersIds = new List<int>
        {
            certificate.CustomerId,       // יצואן
            certificate.CreateCustomerId  // סוכן המכס
        };
        certificate.Milestones = await GetCertificateMilestones(certificate.Title);
        return certificate;
    }

    private async Task<List<CertificateMilestonesDto>> GetCertificateMilestones(string? certificateTitle)
    {
        var rows = await DataLayer.GetCertificateMilestoneRows(certificateTitle);
        var userIds = rows.Where(r => r.UserId.HasValue).Select(r => r.UserId!.Value).Distinct().ToList();
        var users = userIds.Count > 0 ? await userProxy.GetUsersByIds(userIds) : null;
        var result = rows.Select(r => new CertificateMilestonesDto
        {
            VersionNumber = r.VersionNumber,
            ActionName = r.ActionName,
            CreateDate = r.CreateDate,
            RejectReason = r.RejectReason,
            UserName = users?.FirstOrDefault(u => u.Id == r.UserId)?.Title
        }).ToList();
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
