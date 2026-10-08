namespace CertificateOfOrigins.Model.ModelDTOs;

// Minimal projection of the Vendors microservice vendor DTO — only the fields this service consumes (name enrichment).
// Title is the vendor's display name: legacy read StockPileData.Vendors_Vendor.Title (usp_..._GetImportAuthenticationRequestByFilter:
// `vv.Title VendorName`). The Vendors service contract is not published yet (route TODO(blocking)); the field name follows the
// legacy column, not a guess.
public class VendorDto
{
    public int Id { get; set; }
    public string? Title { get; set; }
}
