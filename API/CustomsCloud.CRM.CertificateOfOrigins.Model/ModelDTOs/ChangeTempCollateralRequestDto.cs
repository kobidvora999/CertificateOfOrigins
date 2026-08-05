namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// Legacy ChangeTempCollateralRequestDTO — converts a temporary collateral request into a permanent one bound to the
// saved entity (Collateral microservice). Only the fields SaveImportAuthenticationRequest populates are modelled.
public class ChangeTempCollateralRequestDto
{
    public int CollateralRequestId { get; set; }

    public int RelatedEntityId { get; set; }

    public string? EntityExternalId { get; set; }
}
