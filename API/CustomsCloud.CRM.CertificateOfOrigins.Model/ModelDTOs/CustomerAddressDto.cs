namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

public class CustomerAddressDto
{
    public string? EmailAddress { get; set; }
    public string? FixedLineTelephoneAddress { get; set; }
    public string? MobilePhoneAddress { get; set; }
    public string? FaxAddress { get; set; }
    public string? Website { get; set; }
    public string? AddresTitle { get; set; }
    public string? AddressSingleLine { get; set; }
    public int? AddressPurprose { get; set; }
    public int AddressType { get; set; }
    public int AddressId { get; set; }
    public int? CityId { get; set; }
    public List<int> AuthorizedSignerPermitIds { get; set; } = new();
    public string? StreetName { get; set; }
    public int? StreetCode { get; set; }
    public int? HouseNumber { get; set; }
    public string? Entrance { get; set; }
    public int? Apartment { get; set; }
    public int? PoBox { get; set; }
    public int? LocalPostalCode { get; set; }
    public string? EnglishPostalCode { get; set; }
    public bool IsPalestinianAddress { get; set; }
    public int AddressContactStateId { get; set; }
}
