namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// Projection of the Customers microservice customer DTO — the fields this service consumes.
// GetCustomersByIds populates the scalar fields (name/external-id enrichment); GetCustomerInformation
// additionally returns Addresses. Extra fields on the wire are ignored on deserialization; expand when needed.
public class CustomerDto
{
    public int Id { get; set; }
    public string? Name { get; set; }
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
