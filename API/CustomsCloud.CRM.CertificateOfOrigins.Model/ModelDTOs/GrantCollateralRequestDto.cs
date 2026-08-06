namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// Legacy GrantCollateralRequestDTO — instructs the Collateral microservice to grant all collateral requests on an
// entity (SaveAuthenticationRequestFile, on an Approval decision). Only the fields this service populates are modelled.
public class GrantCollateralRequestDto
{
    public int EntityId { get; set; }

    public int EntityTypeId { get; set; }
}
