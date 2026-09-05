using CertificateOfOrigins.Model.ModelDTOs;
using System.Text.Json;

namespace CertificateOfOrigins.Test.SchemaContract;

// C14: the community domain is Schema First. The domain itself was a developer decision (the legacy name
// GetPC_MSG2280_2281_CertificateOfOriginRequest carries neither WEB nor FRM, so the classification rule did not
// settle it), which makes the committed contract the only written record of the shape - and these tests the thing
// that keeps it honest.
[TestFixture]
public class CommunityCertificateOfOriginRequestContractTests
{
    private const string ContractFile = "community-certificateoforiginrequest.openapi.json";

    private JsonDocument _contract = null!;

    [OneTimeSetUp]
    public void LoadContract() => _contract = OpenApiContract.Load(ContractFile);

    [OneTimeTearDown]
    public void DisposeContract() => _contract.Dispose();

    [Test]
    public void ContractDeclaresTheAgentRequestPath()
    {
        var paths = _contract.RootElement.GetProperty("paths");
        Assert.That(paths.TryGetProperty("/community/CertificateOfOrigins/Request", out var path), Is.True,
            "the contract must describe the agent request path under the community/ domain prefix");
        Assert.That(path.TryGetProperty("post", out _), Is.True, "the agent transmits a message, so the operation is a POST");
    }

    // The legacy service was one-way over MSMQ; the migrated endpoint answers synchronously. That is the single
    // most consequential difference for an agent integrating against it, so it is asserted rather than assumed.
    [Test]
    public void RequestBodyAndResponseAreTheMessageAndItsFeedback()
    {
        var operation = _contract.RootElement
            .GetProperty("paths").GetProperty("/community/CertificateOfOrigins/Request").GetProperty("post");

        var requestSchema = operation.GetProperty("requestBody").GetProperty("content")
            .GetProperty("application/json").GetProperty("schema").GetProperty("$ref").GetString();
        var responseSchema = operation.GetProperty("responses").GetProperty("200").GetProperty("content")
            .GetProperty("application/json").GetProperty("schema").GetProperty("$ref").GetString();

        Assert.Multiple(() =>
        {
            Assert.That(requestSchema, Is.EqualTo("#/components/schemas/CertificateOfOriginRequestMessageDto"));
            Assert.That(responseSchema, Is.EqualTo("#/components/schemas/CertificateOfOriginRequestFeedbackResponseDto"),
                "the feedback comes back on the same call - the migrated endpoint is synchronous, unlike the legacy MSMQ callback");
        });
    }

    // Field and cross-field failures are reported IN BAND, as a 200 carrying exceptions. Agents branch on that
    // array rather than on the status code, so a 422 appearing here would break every one of them.
    [Test]
    public void ValidationFailuresRideInBandOnThe200NotA422()
    {
        var responses = _contract.RootElement
            .GetProperty("paths").GetProperty("/community/CertificateOfOrigins/Request")
            .GetProperty("post").GetProperty("responses");

        Assert.Multiple(() =>
        {
            Assert.That(responses.TryGetProperty("422", out _), Is.False,
                "validation failures are not a 422 - they come back as exceptions on the 200");
            Assert.That(
                OpenApiContract.SchemaProperties(_contract, "CertificateOfOriginRequestFeedbackResponseDto"),
                Does.Contain("exceptions"),
                "the feedback response must carry the in-band exceptions array");
        });
    }

    [TestCase("CertificateOfOriginRequestMessageDto", typeof(CertificateOfOriginRequestMessageDto))]
    [TestCase("CertificateOfOriginAgentRequestDto", typeof(CertificateOfOriginAgentRequestDto))]
    [TestCase("CertificateOfOriginMessageDto", typeof(CertificateOfOriginMessageDto))]
    [TestCase("CertificateOfOriginMessageInvoiceDetailDto", typeof(CertificateOfOriginMessageInvoiceDetailDto))]
    [TestCase("CertificateOfOriginMessageItemDetailDto", typeof(CertificateOfOriginMessageItemDetailDto))]
    [TestCase("NonManipulationCertificateMessageDto", typeof(NonManipulationCertificateMessageDto))]
    [TestCase("CertificateOfOriginRequestFeedbackResponseDto", typeof(CertificateOfOriginRequestFeedbackResponseDto))]
    [TestCase("CertificateOfOriginRequestFeedbackDto", typeof(CertificateOfOriginRequestFeedbackDto))]
    [TestCase("CertificateOfOriginExceptionDto", typeof(CertificateOfOriginExceptionDto))]
    [TestCase("CertificateOfOriginMessageAttachmentDto", typeof(CertificateOfOriginMessageAttachmentDto))]
    public void SchemaAndDtoAgreeInBothDirections(string schemaName, Type dtoType)
        => OpenApiContract.AssertMatches(_contract, schemaName, dtoType);

    // The collection property that round 9 caught being silently dropped: the DTO is plural (invoiceDetails) while
    // fourteen fixtures sent the singular, so no certificate ever got an invoice row. Pinning the name here means
    // the contract - not a coverage run three rounds later - is what catches the next such rename.
    [Test]
    public void InvoiceDetailsCollectionKeepsItsPluralName()
    {
        Assert.That(OpenApiContract.SchemaProperties(_contract, "CertificateOfOriginMessageDto"),
            Does.Contain("invoiceDetails"),
            "the collection is plural on the wire; a singular name silently binds to nothing");
    }
}
