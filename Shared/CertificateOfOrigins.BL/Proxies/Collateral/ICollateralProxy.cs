using CertificateOfOrigins.Model.ModelDTOs;

namespace CertificateOfOrigins.BL.Proxies;

public interface ICollateralProxy
{
    // Legacy: ICollateralExternalProxy.GetCollateralRequest(EEntityType?, entityId, null). The collaterals attached
    // to an entity, from the Collateral microservice.
    Task<List<CollateralRequestDto>?> GetCollateralRequest(int entityType, int entityId);

    // Legacy: ICollateralServiceAdapter.ChangeTempCollateralRequest(list) — converts temporary collateral requests to
    // permanent ones bound to the saved entity (SaveImportAuthenticationRequest).
    Task ChangeTempCollateralRequest(List<ChangeTempCollateralRequestDto> requests);

    // Legacy: ICollateralServiceAdapter.GetCollateralRequestIDsByRelatedEntityDTO(...) — the collateral-request ids on
    // an entity (SaveAuthenticationRequestFile, to decide whether to grant).
    Task<List<int>?> GetCollateralRequestIdsByRelatedEntity(int entityType, int entityId);

    // Legacy: ICollateralServiceAdapter.GrantAllCollateralRequests(list) — grants all collateral requests on the
    // entity (SaveAuthenticationRequestFile, on an Approval decision).
    Task GrantAllCollateralRequests(List<GrantCollateralRequestDto> requests);

    // Legacy: ICollateralServiceAdapter.DebitCreditCollateralRequest(DebitCreditFilter) — the OTHER half of the
    // pair above. Grant releases the guarantee when the authentication answer is accepted; this collects it when
    // the answer is rejected (WrongAuthenticationAnswer). Legacy calls it once per collateral on the file.
    Task DebitCreditCollateralRequest(DebitCreditCollateralRequestDto request);
}
