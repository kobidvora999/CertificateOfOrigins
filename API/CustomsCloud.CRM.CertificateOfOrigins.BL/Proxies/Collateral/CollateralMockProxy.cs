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

    // No-op for local/testing (the real call converts temp collaterals in the Collateral service); the
    // "Collateral.ChangeFail" feature simulates a transport failure.
    public Task ChangeTempCollateralRequest(List<ChangeTempCollateralRequestDto> requests)
    {
        if (mockUtil.HasMockFeature("Collateral.ChangeFail"))
        {
            throw new InvalidOperationException("Mock: Collateral ChangeTempCollateralRequest failed.");
        }

        return Task.CompletedTask;
    }

    // Default = a single dummy collateral-request id; feature "Collateral.Empty" returns none.
    public Task<List<int>?> GetCollateralRequestIdsByRelatedEntity(int entityType, int entityId)
    {
        if (mockUtil.HasMockFeature("Collateral.Empty"))
        {
            return Task.FromResult<List<int>?>([]);
        }

        return Task.FromResult<List<int>?>([(entityId * 10) + 1]); // TODO: dummy data
    }

    // No-op grant for local/testing; the "Collateral.GrantFail" feature simulates a transport failure.
    public Task GrantAllCollateralRequests(List<GrantCollateralRequestDto> requests)
    {
        if (mockUtil.HasMockFeature("Collateral.GrantFail"))
        {
            throw new InvalidOperationException("Mock: Collateral GrantAllCollateralRequests failed.");
        }

        return Task.CompletedTask;
    }
}
