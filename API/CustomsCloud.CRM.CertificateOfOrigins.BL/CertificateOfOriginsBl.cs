using CustomsCloud.CRM.CertificateOfOrigins.DAL;
using CustomsCloud.InfrastructureCore.BL;
using CustomsCloud.InfrastructureCore.BL.Exceptions;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL;

public class CertificateOfOriginsBl(IServiceProvider serviceProvider)
    : BaseBL<CertificateOfOriginsBl, ICertificateOfOriginsDal>(serviceProvider)
{
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
}
