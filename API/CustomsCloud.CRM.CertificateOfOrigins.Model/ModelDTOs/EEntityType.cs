namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// Curated subset of the platform EEntityType (MalamTeam...Environment.Enums.EEntityType) — only the entity-type
// ids this service actually uses. Values are the source of truth from the platform enum (not invented). Add a new
// member here (with its real numeric value) instead of scattering EntityType literals across the code.
public enum EEntityType
{
    ImportDeclaration = 1055,

    CertificateOfOrigin = 12319,

    AuthenticationRequestFile = 12385,
}
