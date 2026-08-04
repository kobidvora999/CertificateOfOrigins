namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// An item line of an import authentication request (CRM.CertificateOfOrigins_ItemDetails) — SP result-set #2 of
// GetAuthenticationRequestByID.
public class AuthenticationRequestItemDetailDto
{
    public int Id { get; set; }

    public int? ImportAuthenticationRequestId { get; set; }

    public int CustomItemId { get; set; }
}
