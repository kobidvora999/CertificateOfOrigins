using System.Reflection;
using CertificateOfOrigins.BL;
using CertificateOfOrigins.BL.Proxies;
using CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore.Interfaces.Http;
using CustomsCloud.InfrastructureCore.Lookup;
using CustomsCloud.InfrastructureCore.Parameters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace CertificateOfOrigins.Test;

// The legacy preference-document letters: the cross-service blocks their legacy data procedures joined in, which the BL now
// fills — each case pins a legacy rule (which user signs, what an INNER JOIN dropped, how a name was composed).
[TestFixture]
public class TemplateLetterEnrichmentTests
{
    private const int CurrentUserId = 7;

    // --- Import verification (2251-2259) ---

    [Test]
    public async Task ImportLetterPrintsTheVendorsEnglishAddressAndTheProducingUsersSignature()
    {
        var calls = new Calls();
        var letter = new ImportVerificationLetterResult { VendorId = 777 };
        calls.Vendor = new VendorDto
        {
            Id = 777,
            Title = "ACME",
            VendorAddress = [new VendorAddressDto { EnglishMainAddressLine = "1 Main St", EnglishCityName = "Austin", EnglishPostalCode = "73301" }],
        };
        calls.Details = id => new UserTemplateDetailsDto { UserId = id, EnglishFirstName = "Dana", EnglishLastName = "Levi", SignatureImageUrl = "sig", Email = "d@x" };

        await Enrich(ECertificateOfOriginsTemplate.RequesrForVerificationMexico, letter, calls);

        Assert.Multiple(() =>
        {
            Assert.That(letter.SupplierTitle, Is.EqualTo("ACME"));
            Assert.That(letter.SupplierAddress, Is.EqualTo("1 Main St \nAustin,73301"));
            Assert.That(calls.DetailsAskedFor, Is.EqualTo(new[] { CurrentUserId }));
            Assert.That(letter.EmployeeFullName, Is.EqualTo("Dana Levi"));
            Assert.That(letter.UserSignature, Is.EqualTo("sig"));
            Assert.That(letter.EmployeesSignature, Is.Null, "legacy never returned a column by that name");
        });
    }

    [Test]
    public async Task AVendorWithoutAnAddressLeavesTheWholeSupplierBlockEmpty()
    {
        var calls = new Calls { Vendor = new VendorDto { Id = 777, Title = "ACME", VendorAddress = [] } };
        var letter = new ImportVerificationLetterResult { VendorId = 777 };

        await Enrich(ECertificateOfOriginsTemplate.RequesrForVerificationCanada, letter, calls);

        Assert.That(letter.SupplierTitle, Is.Null);
    }

    [Test]
    public async Task ImportLetterAddressesTheRequestCustomerWithItsOwnAddressFormat()
    {
        var calls = new Calls
        {
            Customer = new CustomerDto
            {
                Id = 555,
                CustomerAddress = [new CustomerAddressDto { Title = "Douane", EnglishMainAddressLine = "1 Quai", EnglishCityName = "Paris", EnglishPostalCode = null }],
                CustomerActivity = [new CustomerActivityDto { CustomerActivityTypeId = 40 }],
            },
        };
        var letter = new ImportVerificationLetterResult { CustomerId = 555 };

        await Enrich(ECertificateOfOriginsTemplate.RequesrForVerificationEurope, letter, calls);

        Assert.Multiple(() =>
        {
            Assert.That(letter.ExternalCustomDepartmentTitle, Is.EqualTo("Douane"));
            // CONCAT(line, '\n' + city, ', ' + postal): a NULL postal code drops its ", " with it.
            Assert.That(letter.ExternalCustomDepartmentAddress, Is.EqualTo("1 Quai\nParis"));
        });
    }

    // --- Export verification (2289-2295) ---

    [Test]
    public async Task TheExporterLettersAreSignedByTheFixedUserWithThePhoneOnTheUserRow()
    {
        var calls = new Calls
        {
            Details = id => new UserTemplateDetailsDto { UserId = id, Title = "שרה", SignatureImageUrl = "sig", PhoneNumber = "03-1", CustomsPhoneNumber = null },
        };
        var letter = new ExportVerificationLetterResult();

        await Enrich(ECertificateOfOriginsTemplate.ExportAuthenticationSendDeliveryForExporterEuropeTemplate, letter, calls);

        Assert.Multiple(() =>
        {
            Assert.That(calls.DetailsAskedFor, Is.EqualTo(new[] { 69591766 }));
            Assert.That(letter.CustomsEmployeeName, Is.EqualTo("שרה"));
            Assert.That(letter.PhoneNumber, Is.EqualTo("03-1"));
            Assert.That(letter.UserSignature1, Is.EqualTo("sig"), "2293 prints the signature twice");
        });
    }

    [Test]
    public async Task TheOtherExportLettersNeedTheProducingUsersCustomsPhoneOrPrintNoBlock()
    {
        var calls = new Calls
        {
            Details = id => new UserTemplateDetailsDto { UserId = id, Title = "דנה", SignatureImageUrl = "sig", PhoneNumber = "03-1", CustomsPhoneNumber = null },
        };
        var letter = new ExportVerificationLetterResult();

        await Enrich(ECertificateOfOriginsTemplate.ExportAuthenticationRequestEurope, letter, calls);

        Assert.Multiple(() =>
        {
            Assert.That(calls.DetailsAskedFor, Is.EqualTo(new[] { CurrentUserId }));
            Assert.That(letter.CustomsEmployeeName, Is.Null);
            Assert.That(letter.UserSignature, Is.Null);
            Assert.That(letter.PhoneNumber, Is.EqualTo(" "), "ISNULL(UD.PhoneNumber, ' ')");
        });
    }

    [Test]
    public async Task AnExporterWithoutAnEnglishPersonalNameIsNamedByItsCorporationName()
    {
        var calls = new Calls { Customer = new CustomerDto { Id = 888, Title = "יצואן", EnglishCorporationName = "Exporter Ltd" } };
        var letter = new ExportVerificationLetterResult { ExporterCustomerId = 888 };

        await Enrich(ECertificateOfOriginsTemplate.ExportAuthenticationRequestMercosur, letter, calls);

        Assert.Multiple(() =>
        {
            Assert.That(letter.ExporterCustomer, Is.EqualTo("יצואן"));
            Assert.That(letter.ExporterName, Is.EqualTo("Exporter Ltd"));
        });
    }

    [Test]
    public async Task AnAllBlankPersonalNameCountsAsBlankLikeTsqlEquality()
    {
        // CONCAT(' ', ' ', NULL) = '  ', and T-SQL '=' ignores trailing blanks, so legacy compared it equal to ' '.
        var calls = new Calls { Customer = new CustomerDto { Id = 888, EnglishFirstName = " ", EnglishCorporationName = "Exporter Ltd" } };
        var letter = new ExportVerificationLetterResult { ExporterCustomerId = 888 };

        await Enrich(ECertificateOfOriginsTemplate.ExportAuthenticationRequestMercosur, letter, calls);

        Assert.That(letter.ExporterName, Is.EqualTo("Exporter Ltd"));
    }

    [Test]
    public async Task TheImporterLetterTakesTheProducingUsersNameFromTheUsersServiceName()
    {
        var calls = new Calls { Users = [new UserDto { Id = CurrentUserId, Name = "דנה לוי" }] };
        var letter = new ImporterAlertLetterResult { UpdateUserId = 42 };

        await Enrich(ECertificateOfOriginsTemplate.ImportAuthenticationRequestImporterLetter, letter, calls);

        Assert.That(letter.UserTitle, Is.EqualTo("דנה לוי"));
    }

    [Test]
    public async Task OnlyAForeignCustomsDepartmentIsAddressed()
    {
        var address = new CustomerAddressDto { Title = "Customs", EnglishMainAddressLine = "1 Port Rd", EnglishCityName = "Rome", EnglishPostalCode = "1" };
        var notADepartment = new Calls { Customer = new CustomerDto { Id = 5, CustomerAddress = [address], CustomerActivity = [new CustomerActivityDto { CustomerActivityTypeId = 1 }] } };
        var department = new Calls { Customer = new CustomerDto { Id = 5, CustomerAddress = [address], CustomerActivity = [new CustomerActivityDto { CustomerActivityTypeId = 40 }] } };
        var first = new ExportVerificationLetterResult { CustomerId = 5 };
        var second = new ExportVerificationLetterResult { CustomerId = 5 };

        await Enrich(ECertificateOfOriginsTemplate.ExportAuthenticationRequestEurope, first, notADepartment);
        await Enrich(ECertificateOfOriginsTemplate.ExportAuthenticationRequestEurope, second, department);

        Assert.Multiple(() =>
        {
            Assert.That(first.ExternalCustomDepartmentTitle, Is.Null);
            Assert.That(second.ExternalCustomDepartmentTitle, Is.EqualTo("Customs"));
            Assert.That(second.ExternalCustomDepartmentAddress, Is.EqualTo("1 Port Rd \nRome,1"));
        });
    }

    // --- The importer alert letter (2244) ---

    [Test]
    public async Task TheImporterLetterIsSignedWithTheSignatureOfTheUserWhoLastUpdatedTheRequest()
    {
        var calls = new Calls
        {
            Details = id => new UserTemplateDetailsDto { UserId = id, SignatureImageUrl = "sig-" + id },
            Customer = new CustomerDto { Id = 9, Title = "יבואן" },
        };
        var letter = new ImporterAlertLetterResult { ImporterId = 9, UpdateUserId = 42 };

        await Enrich(ECertificateOfOriginsTemplate.ImportAuthenticationRequestImporterLetter, letter, calls);

        Assert.Multiple(() =>
        {
            Assert.That(calls.DetailsAskedFor, Is.EqualTo(new[] { 42 }));
            Assert.That(letter.UserSignature, Is.EqualTo("sig-42"));
            Assert.That(letter.ImporterName, Is.EqualTo("יבואן"));
        });
    }

    // --- Free text ---

    [Test]
    public void AnEmptyFreeTextIsASingleBlankLikeTheLegacyMerger()
    {
        var setFreeText = typeof(CertificateOfOriginsBl).GetMethod("SetFreeText", BindingFlags.NonPublic | BindingFlags.Static)!;
        var empty = new ImportVerificationLetterResult();
        var given = new ExportVerificationLetterResult();

        setFreeText.Invoke(null, [empty, null]);
        setFreeText.Invoke(null, [given, "please reply"]);

        Assert.Multiple(() =>
        {
            Assert.That(empty.FreeText, Is.EqualTo(" "));
            Assert.That(given.FreeText, Is.EqualTo("please reply"));
        });
    }

    // --- Harness ---

    private sealed class Calls
    {
        public VendorDto? Vendor { get; set; }
        public CustomerDto? Customer { get; set; }
        public List<UserDto>? Users { get; set; }
        public Func<int, UserTemplateDetailsDto?> Details { get; set; } = _ => null;
        public List<int> DetailsAskedFor { get; } = [];
    }

    private static async Task Enrich(ECertificateOfOriginsTemplate template, object letter, Calls calls)
    {
        var services = new ServiceCollection();
        services.AddSingleton<ILogger<CertificateOfOriginsBl>>(NullLogger<CertificateOfOriginsBl>.Instance);
        services.AddSingleton(Fake<IRequestMetadata>((method, _) => method.Name == "get_UserId" ? (int?)CurrentUserId : null));
        services.AddSingleton(Fake<IVendorProxy>((method, _) =>
            method.Name == nameof(IVendorProxy.GetVendorsByIds) ? Task.FromResult<List<VendorDto>?>(calls.Vendor is null ? null : [calls.Vendor]) : null));
        services.AddSingleton(Fake<ICustomerProxy>((method, _) => method.Name switch
        {
            nameof(ICustomerProxy.GetCustomersByIds) => Task.FromResult<List<CustomerDto>?>(calls.Customer is null ? null : [calls.Customer]),
            nameof(ICustomerProxy.GetCustomerInformation) => Task.FromResult(calls.Customer),
            _ => null,
        }));
        services.AddSingleton(Fake<IUserProxy>((method, args) =>
        {
            if (method.Name == nameof(IUserProxy.GetUsersByIds))
            {
                return Task.FromResult(calls.Users);
            }

            if (method.Name != nameof(IUserProxy.GetUserTemplateDetails))
            {
                return null;
            }

            var userId = (int)args![0]!;
            calls.DetailsAskedFor.Add(userId);
            return Task.FromResult(calls.Details(userId));
        }));
        var bl = new CertificateOfOriginsBl(services.BuildServiceProvider(), Fake<ILookupUtil>(), Fake<IParametersUtil>());

        var enrich = typeof(CertificateOfOriginsBl).GetMethod("EnrichTemplateData", BindingFlags.NonPublic | BindingFlags.Instance)!;
        await (Task)enrich.Invoke(bl, [(int)template, letter, CurrentUserId])!;
    }

    // --- Minimal dependency-free interface faking over System.Reflection.DispatchProxy (no mocking package). ---

    private static T Fake<T>(Func<MethodInfo, object?[]?, object?>? handler = null) where T : class
    {
        var proxy = DispatchProxy.Create<T, InterfaceFake>();
        ((InterfaceFake)(object)proxy).Handler = (method, args) => handler?.Invoke(method, args) ?? DefaultReturn(method);
        return proxy;
    }

    private static object? DefaultReturn(MethodInfo method)
    {
        var returnType = method.ReturnType;
        if (returnType == typeof(void))
        {
            return null;
        }

        if (returnType == typeof(Task))
        {
            return Task.CompletedTask;
        }

        if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
        {
            var inner = returnType.GetGenericArguments()[0];
            return typeof(Task).GetMethod(nameof(Task.FromResult))!
                .MakeGenericMethod(inner)
                .Invoke(null, [inner.IsValueType ? Activator.CreateInstance(inner) : null])!;
        }

        return returnType.IsValueType ? Activator.CreateInstance(returnType) : null;
    }

    public class InterfaceFake : DispatchProxy
    {
        public Func<MethodInfo, object?[]?, object?> Handler { get; set; } = (_, _) => null;

        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
        {
            return Handler(targetMethod!, args);
        }
    }
}
