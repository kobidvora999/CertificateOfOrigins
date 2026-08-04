using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL.Proxies;

public interface ICollateralProxy
{
    // Legacy: ICollateralExternalProxy.GetCollateralRequest(EEntityType?, entityId, null). The collaterals attached
    // to an entity, from the Collateral microservice.
    Task<List<CollateralRequestDto>?> GetCollateralRequest(int entityType, int entityId);
}
