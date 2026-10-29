namespace CertificateOfOrigins.Model.ModelDTOs;

// Minimal projection of the Vendors microservice vendor DTO — only the fields this service consumes (name enrichment).
// Title is the vendor's display name: legacy read StockPileData.Vendors_Vendor.Title (usp_..._GetImportAuthenticationRequestByFilter:
// `vv.Title VendorName`). The Vendors service contract is not published yet (route TODO(blocking)); the field name follows the
// legacy column, not a guess.
public class VendorDto
{
    public int Id { get; set; }
    public string? Title { get; set; }

    // The vendor's addresses (StockPileData.Vendors_VendorAddress) — the import-verification letters print the supplier's
    // English address and country from them. Named after the legacy table/columns until the Vendors contract is published.
    public List<VendorAddressDto>? VendorAddress { get; set; }
}

// A vendor address entry (legacy Vendors_VendorAddress columns).
public class VendorAddressDto
{
    public string? EnglishMainAddressLine { get; set; }
    public string? EnglishCityName { get; set; }
    public string? EnglishPostalCode { get; set; }

    // Vendors_VendorAddress.English_CountryID.
    public int? EnglishCountryId { get; set; }
}
