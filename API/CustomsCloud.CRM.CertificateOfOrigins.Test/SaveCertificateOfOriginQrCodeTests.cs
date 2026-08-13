using System.Reflection;
using CustomsCloud.CRM.CertificateOfOrigins.BL;
using CustomsCloud.CRM.CertificateOfOrigins.BL.Proxies;
using CustomsCloud.CRM.CertificateOfOrigins.DAL;
using CustomsCloud.CRM.CertificateOfOrigins.Model.CertificateOfOriginsDb;
using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore.Interfaces.Http;
using CustomsCloud.InfrastructureCore.Lookup;
using CustomsCloud.InfrastructureCore.Parameters;
using CustomsCloud.InfrastructureCore.Utils.Documents;
using CustomsCloud.InfrastructureCore.Utils.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace CustomsCloud.CRM.CertificateOfOrigins.Test;

// Regression coverage for the QR-document ordering bug in SaveCertificateOfOrigin (#33): the QR image is uploaded as a
// document linked to the certificate id, but on publish the generation used to run BEFORE the save assigned that id — so
// a brand-new certificate published in one save (request.Id == 0, empty QrCodePath) linked the QR document to id 0.
// The fix generates the QR (stamping QrImage + Guid so they persist with the main upsert) before the save, then uploads
// the document AFTER the save — once entity.Id is the real certificate id — and persists QrCodePath in a follow-up write.
[TestFixture]
public class SaveCertificateOfOriginQrCodeTests
{
    private const string QueryUrlTemplate = "https://verify.example/{0}";
    private const string UploadedExternalId = "documents/qr/resource-path";

    [Test]
    public async Task NewCertificatePublishedInOneSaveLinksQrDocumentToSavedIdNotZero()
    {
        const int savedId = 4242;
        var request = NewPublishedRequest(id: 0, originalStatusId: 0);

        var captures = await RunSaveAsync(request, savedIdReturnedByDal: savedId);

        Assert.Multiple(() =>
        {
            // The bug: the QR document was linked to certificate id 0. The fix links it to the real saved id.
            Assert.That(captures.QrDocumentEntityId, Is.EqualTo(savedId),
                "the QR document must be linked to the saved certificate id, not 0");
            Assert.That(captures.QrDocumentEntityId, Is.Not.Zero);

            // The id-linked upload only became possible once the save assigned the id — so at the main upsert the entity
            // id was still 0 (new instance) and the resource path was not yet known.
            Assert.That(captures.EntityIdAtMainSave, Is.Zero,
                "the entity id is still 0 during the main upsert of a new certificate");
            Assert.That(captures.QrCodePathAtMainSave, Is.Null.Or.Empty,
                "QrCodePath is only resolved by the post-save upload, so it must be empty at the main upsert");

            // QrImage + Guid are stamped before the save so they persist with the main upsert (matching legacy, which
            // committed them together with the certificate).
            Assert.That(captures.QrImageAtMainSave, Is.Not.Null.And.Not.Empty,
                "the QR image bytes must persist with the main upsert");
            Assert.That(captures.GuidAtMainSave, Is.Not.Null.And.Not.EqualTo(Guid.Empty),
                "the certificate Guid must be stamped and persisted with the main upsert");

            // QrCodePath is persisted by the follow-up write, keyed by the saved id, with the upload's ExternalId.
            Assert.That(captures.QrPathUpdateId, Is.EqualTo(savedId),
                "QrCodePath must be persisted for the saved certificate id");
            Assert.That(captures.QrPathUpdateValue, Is.EqualTo(UploadedExternalId),
                "the persisted QrCodePath must be the uploaded document's ExternalId");
        });
    }

    [Test]
    public async Task UpdateExistingPublishedLinksQrDocumentToExistingId()
    {
        const int existingId = 55;
        // An update (Id != 0) publishing with an empty QrCodePath still generates + uploads the QR; the document must be
        // linked to the existing id and QrCodePath persisted (the fix keeps the update path working too).
        var request = NewPublishedRequest(id: existingId, originalStatusId: (int)ECertificateOfOriginStatus.Published);

        var captures = await RunSaveAsync(request, savedIdReturnedByDal: existingId);

        Assert.Multiple(() =>
        {
            Assert.That(captures.QrDocumentEntityId, Is.EqualTo(existingId));
            Assert.That(captures.QrPathUpdateId, Is.EqualTo(existingId));
            Assert.That(captures.QrPathUpdateValue, Is.EqualTo(UploadedExternalId));
        });
    }

    private static SaveCertificateOfOriginRequestDto NewPublishedRequest(int id, int originalStatusId)
    {
        return new SaveCertificateOfOriginRequestDto
        {
            Id = id,
            TypeId = 1,
            CertificateNumber = "COO-1001",
            CertificateOfOriginStatusId = (int)ECertificateOfOriginStatus.Published,
            OriginalCertificateOfOriginStatusId = originalStatusId,
            RequestReasonCode = (int)ERequestReason.NewCertificate,
            FeedbackRemark = "remark",
            OriginalFeedbackRemark = "remark", // unchanged → no feedback-message side effect
            QrCodePath = null,                 // empty → QR generation is required on publish
            QrImage = null,
            CertificateOfOriginDetails = [],   // no detail rows → no per-field enrichment proxy calls
        };
    }

    private static async Task<Captures> RunSaveAsync(SaveCertificateOfOriginRequestDto request, int savedIdReturnedByDal)
    {
        var captures = new Captures();

        // The QR document builder: record the entity id the certificate is linked to, and return itself for chaining.
        var documentBuilder = default(IDocumentBuilder);
        documentBuilder = Fake<IDocumentBuilder>((method, args) =>
        {
            if (method.Name == "WithEntityId")
            {
                captures.QrDocumentEntityId = (int)args![0]!;
            }

            if (method.Name == "Build")
            {
                return Fake<IDocument>();
            }

            return method.ReturnType == typeof(IDocumentBuilder) ? documentBuilder : null;
        });

        var documentUtil = Fake<IDocumentUtil>((method, args) => method.Name switch
        {
            "CreateDocumentBuilder" => documentBuilder,
            "GetInvalidFilenameChars" => Array.Empty<char>(),
            "UploadDocument" => Task.FromResult<IDocumentResponse>(new FakeDocumentResponse { ExternalId = UploadedExternalId }),
            _ => null,
        });

        var dataLayer = Fake<ICertificateOfOriginsDal>((method, args) =>
        {
            switch (method.Name)
            {
                case "GetLatestCertificateByNumber":
                    return Task.FromResult<CertificateOfOrigin?>(null); // no previous version to supersede

                case "SaveCertificateOfOrigin":
                    var entity = (CertificateOfOrigin)args![0]!;
                    // Snapshot the persisted-with-the-upsert state at the moment of the save (before the id is assigned
                    // from the return value and before the post-save upload stamps QrCodePath).
                    captures.EntityIdAtMainSave = entity.Id;
                    captures.QrImageAtMainSave = entity.QrImage;
                    captures.GuidAtMainSave = entity.Guid;
                    captures.QrCodePathAtMainSave = entity.QrCodePath;
                    return Task.FromResult(savedIdReturnedByDal);

                case "UpdateCertificateQrCodePath":
                    captures.QrPathUpdateId = (int)args![0]!;
                    captures.QrPathUpdateValue = (string?)args![1];
                    return Task.CompletedTask;

                case "GetCertificateOfOriginById":
                    return Task.FromResult<CertificateOfOriginDto?>(new CertificateOfOriginDto { Id = (int)args![0]! });

                default:
                    return null;
            }
        });

        var requestMetadata = Fake<IRequestMetadata>((method, _) => method.Name == "get_UserId" ? (int?)7 : null);

        var parametersUtil = Fake<IParametersUtil>((method, _) =>
        {
            if (method.Name == "Get")
            {
                var resultType = method.ReturnType.GetGenericArguments()[0]; // Task<T> → T
                var value = resultType == typeof(string)
                    ? (object?)QueryUrlTemplate
                    : (resultType.IsValueType ? Activator.CreateInstance(resultType) : null);
                return TaskFromResult(resultType, value);
            }

            return null;
        });

        var commonServices = Fake<ICommonServicesProxy>((method, _) =>
            method.Name == "CreateQrCode" ? Task.FromResult<byte[]?>([1, 2, 3, 4]) : null);

        var services = new ServiceCollection();
        services.AddSingleton<ILogger<CertificateOfOriginsBl>>(NullLogger<CertificateOfOriginsBl>.Instance);
        services.AddSingleton(dataLayer);
        services.AddSingleton(requestMetadata);
        services.AddSingleton(documentUtil);
        services.AddSingleton(Fake<IEventUtil>()); // resolved at the top of the method; not exercised on this path
        var serviceProvider = services.BuildServiceProvider();

        var bl = new CertificateOfOriginsBl(
            serviceProvider,
            Fake<ICustomerProxy>(),
            Fake<IExportDealFileProxy>(),   // GetLeadDocument... → null (default) → LinkLeadDocument returns early
            Fake<IUserProxy>(),             // GetUsersByIds → null (default) → org unit resolves to 0
            Fake<IDataDictionaryFieldProxy>(),
            Fake<ICurrencyTypeProxy>(),
            Fake<IDocumentsProxy>(),
            Fake<ICustomsBookProxy>(),
            commonServices,
            Fake<IOrganizationUnitProxy>(),
            Fake<IMessageManagementProxy>(),
            Fake<ICountryGroupProxy>(),
            Fake<ITasksProxy>(),
            Fake<ILookupUtil>(),
            parametersUtil);

        captures.Returned = await bl.SaveCertificateOfOrigin(request);
        return captures;
    }

    private sealed class Captures
    {
        public int QrDocumentEntityId = int.MinValue;
        public int QrPathUpdateId = int.MinValue;
        public string? QrPathUpdateValue;
        public int EntityIdAtMainSave = int.MinValue;
        public byte[]? QrImageAtMainSave;
        public Guid? GuidAtMainSave;
        public string? QrCodePathAtMainSave = "<not-captured>";
        public CertificateOfOriginDto? Returned;
    }

    private sealed class FakeDocumentResponse : IDocumentResponse
    {
        public string ExternalId { get; init; } = string.Empty;
        public string FileResource { get; init; } = string.Empty;
        public int Id { get; init; }
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
            return TaskFromResult(inner, inner.IsValueType ? Activator.CreateInstance(inner) : null);
        }

        return returnType.IsValueType ? Activator.CreateInstance(returnType) : null;
    }

    private static object TaskFromResult(Type resultType, object? value)
    {
        return typeof(Task).GetMethod(nameof(Task.FromResult))!
            .MakeGenericMethod(resultType)
            .Invoke(null, [value])!;
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
