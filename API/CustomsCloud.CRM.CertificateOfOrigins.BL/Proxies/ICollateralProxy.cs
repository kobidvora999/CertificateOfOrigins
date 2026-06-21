using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL.Proxies;

public interface ICollateralProxy
{
    Task<List<CollateralRequestDto>?> GetCollateralRequest(int entityType, int entityId, int? collateralRequestId);
}
