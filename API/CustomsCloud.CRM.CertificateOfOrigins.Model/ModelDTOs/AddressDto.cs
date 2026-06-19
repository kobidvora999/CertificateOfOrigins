namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

public class AddressDto
{
    public string? Address { get; set; }
    public string? AddressForEnvelope { get; set; }
    public List<CommunicationAddressesDto> CommunicationAddresses { get; set; } = new();
}
