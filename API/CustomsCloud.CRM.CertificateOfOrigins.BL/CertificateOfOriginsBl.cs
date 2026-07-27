using CustomsCloud.CRM.CertificateOfOrigins.BL.Proxies;
using CustomsCloud.CRM.CertificateOfOrigins.DAL;
using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore.BL;
using CustomsCloud.InfrastructureCore.BL.Exceptions;
using Dapper;
using System.Data;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL;

public class CertificateOfOriginsBl(IServiceProvider serviceProvider, ICustomerProxy customerProxy, IExportDealFileProxy exportDealFileProxy, IUserProxy userProxy)
    : BaseBL<CertificateOfOriginsBl, ICertificateOfOriginsDal>(serviceProvider)
{
    public async Task<CertificateOfOriginDto> GetCertificateOfOriginById(int certificateOfOriginId)
    {
        // Single certificate with its full graph (7 result sets). Missing id → 404 (the legacy returned null,
        // which callers treated as not-found). Milestone user display-names are enriched here — the SP returns only
        // the acting user id (the cross-service Infrastructure.UserMng_User JOIN was removed).
        var certificate = await DataLayer.GetCertificateOfOriginById(certificateOfOriginId)
            ?? throw new RestNotFoundException();
        await FillMilestoneUserNames(certificate);
        return certificate;
    }

    private async Task FillMilestoneUserNames(CertificateOfOriginDto certificate)
    {
        var userIds = certificate.Milestones
            .Where(m => m.UserId.HasValue)
            .Select(m => m.UserId!.Value)
            .Distinct()
            .ToList();
        if (userIds.Count == 0)
        {
            return;
        }

        var users = await userProxy.GetUsersByIds(userIds);
        if (users == null)
        {
            return;
        }

        var usersById = users.ToDictionary(u => u.Id);
        foreach (var milestone in certificate.Milestones)
        {
            if (milestone.UserId.HasValue && usersById.TryGetValue(milestone.UserId.Value, out var user))
            {
                milestone.UserName = user.Name;
            }
        }
    }

    public async Task<VirtualEntityDto> Convert(ConnectedEntityDto connectedEntity)
    {
        // ESB/EAI Convert: resolve the connected-entity key (the certificate number) into a generic entity link.
        // Reuses the #7 filter search; a missing certificate owns the 404 contract (legacy threw not-exist).
        var filter = new CertificateOfOriginFilterDto { CertificateNumber = connectedEntity.EntityIdKey1 };
        var certificate = (await GetCertificateOfOriginsByFilter(filter)).FirstOrDefault()
            ?? throw new RestNotFoundException();

        var result = new VirtualEntityDto
        {
            Id = certificate.Id,
            Title = certificate.Name,
            EntityType = 12319, // EEntityType.CertificateOfOrigin (MalamTeam.Infrastructure.GeneralServices.Environment.Enums.EEntityType)
            CustomerId = certificate.CustomesAgentId,
        };
        return result;
    }

    public async Task<bool> LoadDataFromExportDeclaration(LoadDataFromExportDeclarationRequestDto request)
    {
        // Guard: without a lead-document id or an export-declaration number there is nothing to look up.
        if (request.LeadDocumentId is null && string.IsNullOrEmpty(request.ExportDeclarationNumber))
        {
            return false;
        }

        var details = await exportDealFileProxy.GetExportDeclarationDetailsForCertificateOfOrigion(
            request.LeadDocumentId, request.ExportDeclarationNumber);

        // The legacy set IsDeclarationReleased/IsCargoExitedOfCustomsRegulation back on the entity (by-ref) and
        // returned this computed flag; over REST only the flag is returned (developer decision 2026-07-27). It is
        // true only when the cargo has exited customs regulation and the request is not a retrospective certificate.
        var isCargoExited = details?.IsCargoExitedOfCustomsRegulation ?? false;
        return isCargoExited && request.RequestReasonCode != (int)ERequestReason.RetrospectiveCertificate;
    }

    public async Task<int> GetCertificateOfOriginID(string certificateNumber)
    {
        // route-style alternate key → not-found owns the 404 contract (RestNotFoundException)
        var result = await DataLayer.GetCertificateOfOriginIdByNumber(certificateNumber)
            ?? throw new RestNotFoundException();
        return result;
    }

    #region LEGACY_WCF

    // Original WCF (CertificateOfOriginsExternalService.InternalGetCertificateOfOriginID):
    //
    // public int? InternalGetCertificateOfOriginID(string certificateNumber)
    // {
    //     using (var uow = Container.Resolve<IUnitOfWork>(CRMConsts.CertificateOfOriginsUnitOfWork))
    //     {
    //         var certificateOfOrigin = uow.Repository.GetQuery<CertificateOfOrigin>()
    //             .OrderByDescending(c => c.CreateDate)
    //             .FirstOrDefault(c => c.CertificateNumber == certificateNumber);
    //         return certificateOfOrigin?.ID;
    //     }
    // }
    #endregion

    public async Task<List<GoodsItemCerificateDto>> GetGoodsItemCerificateDTO(List<GoodsItemCerificateDto> goodsItemCerificateDTOs)
    {
        foreach (var item in goodsItemCerificateDTOs)
        {
            if (item.CertificateNumber != null)
            {
                item.CertificateOfOriginId = await DataLayer.GetCertificateOfOriginIdByNumber(item.CertificateNumber);
            }
        }

        return goodsItemCerificateDTOs;
    }

    public async Task<CertificateOfOriginResultDto?> IsCertificateOfOriginByExternalIdExist(string certificateOfOriginExternalId)
    {
        var filter = new CertificateOfOriginFilterDto { CertificateNumber = certificateOfOriginExternalId };
        var certificates = await GetCertificateOfOriginsByFilter(filter);
        var result = certificates.FirstOrDefault();
        return result;
    }

    public async Task<List<CertificateOfOriginResultDto>> GetCertificateOfOriginsByFilter(CertificateOfOriginFilterDto filter)
    {
        var parameters = BuildParameterForProcedure(filter);
        var result = await DataLayer.GetCertificateOfOriginsByFilter(parameters);
        await FillCustomersInformation(result);
        return result;
    }

    private static DynamicParameters BuildParameterForProcedure(CertificateOfOriginFilterDto filter)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@CertificateNumber", filter.CertificateNumber, DbType.String);
        parameters.Add("@CertificateOfOriginStatusID", filter.CertificateOfOriginStatusId, DbType.Int32);
        parameters.Add("@CertificateOfOriginTypeID", filter.CertificateOfOriginTypeId, DbType.Int32);
        parameters.Add("@CustomsAgentID", filter.CustomsAgentId, DbType.Int32);
        parameters.Add("@CustomsHouseID", filter.CustomsHouseId, DbType.Int32);
        parameters.Add("@DestinationCountry", filter.DestinationCountry, DbType.Int32);
        parameters.Add("@ExportDeclarationID", filter.ExportDeclarationId, DbType.Int32);
        parameters.Add("@ExportDeclarationNum", filter.ExportDeclarationNum, DbType.String);
        parameters.Add("@ExporterCustomerID", filter.ExporterCustomerId, DbType.Int32);
        parameters.Add("@FromIssuingDate", filter.FromIssuingDate, DbType.DateTime);
        parameters.Add("@ToIssuingDate", filter.ToIssuingDate, DbType.DateTime);
        parameters.Add("@FromRequestDate", filter.FromRequestDate, DbType.DateTime);
        parameters.Add("@ToRequestDate", filter.ToRequestDate, DbType.DateTime);
        parameters.Add("@RequestReasonID", filter.RequestReasonId, DbType.Int32);
        parameters.Add("@VersionNumber", filter.VersionNumber, DbType.Int32);
        parameters.Add("@IsLastVersion", filter.IsLastVersion, DbType.Boolean);
        return parameters;
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
