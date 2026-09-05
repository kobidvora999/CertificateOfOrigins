using CertificateOfOrigins.Model.ModelDTOs;
using System.Text.Json;

namespace CertificateOfOrigins.Test.SchemaContract;

// C14: the web domain is Schema First, so .spec/OpenApi/web-certificaterequest.openapi.json is the source of
// truth and these tests are what stop the code from drifting away from it.
[TestFixture]
public class WebCertificateRequestContractTests
{
    private const string ContractFile = "web-certificaterequest.openapi.json";

    private JsonDocument _contract = null!;

    [OneTimeSetUp]
    public void LoadContract() => _contract = OpenApiContract.Load(ContractFile);

    [OneTimeTearDown]
    public void DisposeContract() => _contract.Dispose();

    [Test]
    public void ContractDeclaresThePortalVerificationPath()
    {
        var paths = _contract.RootElement.GetProperty("paths");
        Assert.That(paths.TryGetProperty("/web/CertificateOfOrigins/RequestByGuid", out var path), Is.True,
            "the contract must describe the portal verification path under the web/ domain prefix");
        Assert.That(path.TryGetProperty("get", out _), Is.True, "the portal reads by guid, so the operation is a GET");
    }

    // The in-band error contract is the whole reason the portal can rely on this endpoint: nothing matched still
    // answers 200, carrying exceptionDescription. A 404 here would be a breaking change for the portal.
    [Test]
    public void ContractAnswers200WhenNothingMatchedRatherThan404()
    {
        var responses = _contract.RootElement
            .GetProperty("paths").GetProperty("/web/CertificateOfOrigins/RequestByGuid")
            .GetProperty("get").GetProperty("responses");

        Assert.Multiple(() =>
        {
            Assert.That(responses.TryGetProperty("200", out _), Is.True, "200 must be declared");
            Assert.That(responses.TryGetProperty("404", out _), Is.False,
                "a 404 must NOT be declared - an unknown guid returns 200 with exceptionDescription set");
        });
    }

    [TestCase("CertificateOfOriginsRequestDto", typeof(CertificateOfOriginsRequestDto))]
    [TestCase("CertificateOfOriginsResponseDto", typeof(CertificateOfOriginsResponseDto))]
    [TestCase("FieldDataDto", typeof(FieldDataDto))]
    [TestCase("CertificateOfOriginWebInvoiceDetailDto", typeof(CertificateOfOriginWebInvoiceDetailDto))]
    [TestCase("CertificateOfOriginWebItemDetailDto", typeof(CertificateOfOriginWebItemDetailDto))]
    public void SchemaAndDtoAgreeInBothDirections(string schemaName, Type dtoType)
        => OpenApiContract.AssertMatches(_contract, schemaName, dtoType);

    // The query parameters are the request shape for a GET, so they are checked against the request DTO too -
    // otherwise a renamed query field would slip through with the schema block still green.
    [Test]
    public void QueryParametersMatchTheRequestDto()
    {
        var parameters = _contract.RootElement
            .GetProperty("paths").GetProperty("/web/CertificateOfOrigins/RequestByGuid")
            .GetProperty("get").GetProperty("parameters")
            .EnumerateArray().Select(p => p.GetProperty("name").GetString()!)
            .OrderBy(n => n, StringComparer.Ordinal).ToList();

        var dto = OpenApiContract.ClrProperties(typeof(CertificateOfOriginsRequestDto));
        Assert.That(parameters, Is.EqualTo(dto),
            "the declared query parameters must be exactly the properties of CertificateOfOriginsRequestDto");
    }
}
