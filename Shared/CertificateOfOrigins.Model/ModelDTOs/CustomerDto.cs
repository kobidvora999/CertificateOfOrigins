namespace CertificateOfOrigins.Model.ModelDTOs;

// Projection of the Customers microservice customer DTO — the fields this service consumes. Title is the customer's display
// name: legacy read StockPileData.Customers_Customer.Title in every place this service shows a customer (E.Title ExporterTitle,
// C.Title CustomesAgentTitle, Customer.Title ImporterName, CC.Title ForeignCustomsHouseName / RequestIssuerName), and the cloud
// Customers service's own CustomerDto carries Title, with no Name (parity H-8).
// GetCustomersByIds populates the scalar fields (name/external-id enrichment); GetCustomerInformation
// additionally returns Addresses. Extra fields on the wire are ignored on deserialization; expand when needed.
public class CustomerDto
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? ExternalIdNum { get; set; }
    public bool IsActive { get; set; }
    public List<CustomerAddressDto>? Addresses { get; set; }
}

// A customer address entry — consumed by GetCustomerInformation. The caller (SPA) picks the address whose
// AddressPurpose is Authentication (else the first) and uses AddressSingleLine as the customs-house address.
public class CustomerAddressDto
{
    public int AddressPurpose { get; set; }
    public string? AddressSingleLine { get; set; }
}
