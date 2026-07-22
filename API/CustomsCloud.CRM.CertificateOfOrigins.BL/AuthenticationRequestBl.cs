using CustomsCloud.CRM.CertificateOfOrigins.DAL;
using CustomsCloud.InfrastructureCore.BL;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL;

public class AuthenticationRequestBl(IServiceProvider serviceProvider)
    : BaseBL<AuthenticationRequestBl, ICertificateOfOriginsDal>(serviceProvider)
{
    public async Task<int?> CheckImporterOfImportAuthentication(int importerId)
    {
        var result = await DataLayer.CheckImporterOfImportAuthentication(importerId);
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
