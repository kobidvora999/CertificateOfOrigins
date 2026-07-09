using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;
using System.Diagnostics.CodeAnalysis;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL.Proxies;

[ExcludeFromCodeCoverage]
public class CollateralMockProxy : ICollateralProxy
{
    // Returns hardcoded dummy data — used while the real Collaterals service endpoint is unavailable.
    public Task<List<CollateralRequestDto>?> GetCollateralRequest(int entityType, int entityId, int? collateralRequestId)
    {
        return Task.FromResult<List<CollateralRequestDto>?>(new List<CollateralRequestDto>());
    }
}
