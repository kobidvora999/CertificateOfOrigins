using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore.Proxy.Rest;
using System.Diagnostics.CodeAnalysis;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL.Proxies;

[ExcludeFromCodeCoverage]
public class CollateralMockProxy(IProxyMockUtil mockUtil) : ICollateralProxy, IMockProxy
{
    // Default = a single realistic dummy collateral; feature "Collateral.Empty" returns none.
    public Task<List<CollateralRequestDto>?> GetCollateralRequest(int entityType, int entityId)
    {
        if (mockUtil.HasMockFeature("Collateral.Empty"))
        {
            return Task.FromResult<List<CollateralRequestDto>?>([]);
        }

        var result = new List<CollateralRequestDto>
        {
            new()
            {
                CollateralRequestId = (entityId * 10) + 1,       // TODO: dummy data
                CustomerId = 279366,                             // TODO: dummy data
                CollateralType = 1,                              // TODO: dummy data
                AmountToGrant = 1000m,                           // TODO: dummy data
                RelatedEntityType = entityType,
                CollateralRequestStatus = 1,                     // TODO: dummy data
                CollateralRequestStatusName = "Mock collateral", // TODO: dummy data
            },
        };
        return Task.FromResult<List<CollateralRequestDto>?>(result);
    }
}
