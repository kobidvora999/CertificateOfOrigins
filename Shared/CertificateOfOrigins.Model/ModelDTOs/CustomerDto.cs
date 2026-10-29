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

    // The English name, as the Customers service's CustomerDto carries it — the export-verification letters print the
    // exporter's English personal name, or its English corporation name when that is blank.
    public string? EnglishFirstName { get; set; }
    public string? EnglishLastName { get; set; }
    public string? EnglishCorporationName { get; set; }

    // The customer's activities (Customers service CustomerActivityDto). A foreign customs department is a customer with
    // activity type 40.
    public List<CustomerActivityDto>? CustomerActivity { get; set; }

    // The customer's addresses as the Customers service's CustomerDto carries them (CustomerAddress, with the English
    // fields) — not the Addresses projection above, which has only AddressPurpose / AddressSingleLine. The letters
    // address the foreign customs department from these.
    public List<CustomerAddressDto>? CustomerAddress { get; set; }
}

// A customer address entry — consumed by GetCustomerInformation. The caller (SPA) picks the address whose
// AddressPurpose is Authentication (else the first) and uses AddressSingleLine as the customs-house address.
public class CustomerAddressDto
{
    public int AddressPurpose { get; set; }
    public string? AddressSingleLine { get; set; }

    // The English address fields, as the Customers service's CustomerAddressDto carries them — the export-verification
    // letters address the foreign customs department in English.
    public string? Title { get; set; }
    public string? EnglishMainAddressLine { get; set; }
    public string? EnglishCityName { get; set; }
    public string? EnglishPostalCode { get; set; }
    public int? EnglishCountryId { get; set; }
}

// A customer activity entry (Customers service CustomerActivityDto).
public class CustomerActivityDto
{
    public int? CustomerActivityTypeId { get; set; }
}
