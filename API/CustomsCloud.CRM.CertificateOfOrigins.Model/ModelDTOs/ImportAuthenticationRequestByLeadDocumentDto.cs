namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// Mirrors the legacy WCF ImportAuthenticationRequestDTO (Customs.CRM.Entities.CertificateOfOriginsDTO) —
// the flat result of GetAuthenticationRequestByLeadDocumentIDs.
public class ImportAuthenticationRequestByLeadDocumentDto
{
    public int LeadDocumentId { get; set; }
    public int DocumentId { get; set; }
    public int? AuthenticationFileId { get; set; }
    public int PreferenceDocumentTypeId { get; set; }
    public string? PreferenceDocumentTypeName { get; set; }
    public DateTimeOffset CreateDate { get; set; }
    public int? AuthenticationFileStatusId { get; set; }
    public string? AuthenticationFileStatusName { get; set; }
    public int DecisionId { get; set; }
    public string? DecisionName { get; set; }
    public int ImportCountryId { get; set; }
    public string? ImportCountryName { get; set; }
    public int OrganizationUnitId { get; set; }
    public string? OrganizationUnitName { get; set; }
    public int? CollateralId { get; set; }
    public bool IsCollateralExists { get; set; }

    // legacy: never populated by the SP — preserved for contract parity
    public int? ImporterId { get; set; }
    public DateTimeOffset? LastDeliveryForImporter { get; set; }
    public string? LeadDocumentTitle { get; set; }
}
