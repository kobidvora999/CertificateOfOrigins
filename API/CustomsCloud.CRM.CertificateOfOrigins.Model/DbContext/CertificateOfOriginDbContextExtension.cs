using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;
using Dapper;
using Microsoft.EntityFrameworkCore;
using CustomsCloud.InfrastructureCore.DAL;
using System.Data;

namespace CustomsCloud.CRM.CertificateOfOrigins.DAL;

public partial class CertificateOfOriginDbContext
{
    public async Task<IEnumerable<CertificateOfOriginResultDto>> GetCertificateOfOriginsByFilterAsync(CertificateOfOriginFilterDto filter)
        => await Database.GetDbConnection().QueryAsync<CertificateOfOriginResultDto>(
            "[CRM].[usp_CertificateOfOrigins_GetCertificateOfOriginsByFilter]",
            new
            {
                certificateNumber = filter.CertificateNumber,
                certificateOfOriginStatusID = filter.CertificateOfOriginStatusId,
                certificateOfOriginTypeID = filter.CertificateOfOriginTypeId,
                customsAgentID = filter.CustomsAgentId,
                customsHouseID = filter.CustomsHouseId,
                destinationCountry = filter.DestinationCountry,
                exportDeclarationID = filter.ExportDeclarationId,
                exportDeclarationNum = filter.ExportDeclarationNum,
                exporterCustomerID = filter.ExporterCustomerId,
                fromIssuingDate = filter.FromIssuingDate,
                toIssuingDate = filter.ToIssuingDate,
                fromRequestDate = filter.FromRequestDate,
                toRequestDate = filter.ToRequestDate,
                requestReasonID = filter.RequestReasonId,
                versionNumber = filter.VersionNumber,
                isLastVersion = filter.IsLastVersion,
            },
            commandType: CommandType.StoredProcedure);
}

public partial class CertificateOfOriginDbReadOnlyContext : CertificateOfOriginDbContext, IReadOnlyContext
{
    public CertificateOfOriginDbReadOnlyContext(DbContextOptions<CertificateOfOriginDbContext> options)
        : base(options)
    {
    }
}
