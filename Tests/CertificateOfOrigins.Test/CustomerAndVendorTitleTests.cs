using System.Text.Json;
using CertificateOfOrigins.Model.ModelDTOs;

namespace CertificateOfOrigins.Test;

// Parity findings H-8 and A-24: legacy showed customers, vendors and users by the Title column of Customers_Customer /
// Vendors_Vendor / UserMng_User, and the
// cloud Customers service's CustomerDto carries Title with no Name. The proxy DTOs had a guessed "name" field, which the
// service would leave null, so every customer and vendor name would have come back empty.
[TestFixture]
public class CustomerAndVendorTitleTests
{
    private static readonly JsonSerializerOptions Web = new(JsonSerializerDefaults.Web);

    [Test]
    public void ACustomerReadsItsTitle()
    {
        var customer = JsonSerializer.Deserialize<CustomerDto>("""{"id":5,"title":"Exporter Ltd"}""", Web)!;

        Assert.That(customer.Title, Is.EqualTo("Exporter Ltd"));
    }

    [Test]
    public void AVendorReadsItsTitle()
    {
        var vendor = JsonSerializer.Deserialize<VendorDto>("""{"id":7,"title":"Vendor GmbH"}""", Web)!;

        Assert.That(vendor.Title, Is.EqualTo("Vendor GmbH"));
    }

    // A-24: legacy named the milestone user by UserMng_User.Title (aliased UserName in the result).
    [Test]
    public void AUserReadsItsTitle()
    {
        var user = JsonSerializer.Deserialize<UserDto>("""{"id":9,"title":"Dana Cohen"}""", Web)!;

        Assert.That(user.Title, Is.EqualTo("Dana Cohen"));
    }
}
