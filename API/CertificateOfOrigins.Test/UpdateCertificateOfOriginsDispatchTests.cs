using System.Reflection;
using CertificateOfOrigins.BL;
using CertificateOfOrigins.BL.Proxies;
using CertificateOfOrigins.DAL;
using CertificateOfOrigins.Model.CertificateOfOriginsDb;
using CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore.Interfaces.Http;
using CustomsCloud.InfrastructureCore.Lock;
using CustomsCloud.InfrastructureCore.Lookup;
using CustomsCloud.InfrastructureCore.Parameters;
using CustomsCloud.InfrastructureCore.Utils.Documents;
using CustomsCloud.InfrastructureCore.Utils.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace CertificateOfOrigins.Test;

// Faithfulness coverage for UpdateCetrificateOfOrigins — the 5-way EEventType dispatcher migrated from the legacy
// InternalUpdateCetrificateOfOrigins switch (CertificateOfOriginsExternalServicePartial.cs:104). Each test pins one
// branch's observable side effects against the legacy behaviour, so a regression on any branch fails here.
[TestFixture]
public class UpdateCertificateOfOriginsDispatchTests
{
    private const string QueryUrlTemplate = "https://verify.example/{0}";

    // --- Branch 5: CancellationRequestCommited (legacy ExportDeclarationCancellationRequestCommited, 889-914) ---
    // Every linked certificate is set to Cancelled with the canceled-declaration reason, and the close-open-task event
    // is raised per certificate.
    [Test]
    public async Task CancellationCancelsEachCertificateWithReasonAndRaisesCloseTaskEvent()
    {
        var certs = new[]
        {
            Cert(id: 11, status: (int)ECertificateOfOriginStatus.Published),
            Cert(id: 22, status: (int)ECertificateOfOriginStatus.Published),
        };
        var request = Request((int)EEventType.CancellationRequestCommited, [11, 22]);

        var cap = await RunDispatchAsync(request, certs);

        Assert.Multiple(() =>
        {
            Assert.That(cap.Cancelled, Has.Count.EqualTo(2), "both certificates are cancelled");
            Assert.That(cap.Cancelled.Select(c => c.Id), Is.EquivalentTo(new[] { 11, 22 }));
            Assert.That(cap.Cancelled.Select(c => c.Reason),
                Is.All.EqualTo(CertificateOfOriginsConsts.CanceledDeclarationReason),
                "the reject reason is the canceled-declaration text");
            Assert.That(cap.RaisedEventTypes.Count(e => e == (int)EEventType.ExportDeclarationConnectToCertificateOfOriginCanceled),
                Is.EqualTo(2), "the close-open-task event fires once per certificate");
            // The cancellation branch never reconciles.
            Assert.That(cap.Reconciliations, Is.Empty);
        });
    }

    // --- Branch 4: ExportDeclarationAmendmentRequestCompleted (legacy ExportDeclarationAmendmentSuccess, 446-467) ---
    // Only certificates with an EMPTY declaration number are backfilled; one that already carries a declaration is left
    // untouched.
    [Test]
    public async Task AmendmentBackfillsOnlyCertificatesWithEmptyDeclaration()
    {
        var certs = new[]
        {
            // Empty declaration → backfilled. Non-Received so the follow-up reconciliation is gate-skipped (keeps the
            // test focused on the backfill selection).
            Cert(id: 31, status: (int)ECertificateOfOriginStatus.Cancelled, exportDeclarationNumber: null),
            // Already linked → NOT backfilled.
            Cert(id: 32, status: (int)ECertificateOfOriginStatus.Cancelled, exportDeclarationNumber: "EXISTING-DEC"),
        };
        var request = Request((int)EEventType.ExportDeclarationAmendmentRequestCompleted, [31, 32], exportDeclarationNum: "NEW-DEC");

        var cap = await RunDispatchAsync(request, certs);

        Assert.Multiple(() =>
        {
            Assert.That(cap.DeclarationLinkIds, Is.EqualTo(new[] { 31 }),
                "only the empty-declaration certificate is backfilled");
            Assert.That(cap.DeclarationLinkIds, Does.Not.Contain(32));
        });
    }

    // --- Branch 2/3: ExportDeclarationReleased / AssemblySharedReleaseAccepted (legacy DeclarationReleased, 310-354) ---
    // A PendingRelease certificate becomes Published: the QR Guid + image are persisted (regression guard — the release
    // path has no main upsert, so they need their own write, else the QR query URL's Guid is orphaned).
    [Test]
    public async Task ReleasedPendingReleaseCertificatePersistsQrGuidAndImage()
    {
        var certs = new[]
        {
            Cert(id: 41, status: (int)ECertificateOfOriginStatus.PendingRelease, exportDeclarationNumber: null, qrCodePath: null),
        };
        var request = Request((int)EEventType.ExportDeclarationReleased, [41], exportDeclarationNum: "REL-DEC");

        var cap = await RunDispatchAsync(request, certs);

        Assert.Multiple(() =>
        {
            Assert.That(cap.QrWrites.Select(q => q.Id), Does.Contain(41), "the QR Guid + image must be persisted on publish");
            var qr = cap.QrWrites.First(q => q.Id == 41);
            Assert.That(qr.Guid, Is.Not.Null.And.Not.EqualTo(Guid.Empty), "the QR Guid must be persisted");
            Assert.That(qr.Image, Is.Not.Null.And.Not.Empty, "the QR image bytes must be persisted");
            // Published status is persisted for the certificate.
            Assert.That(cap.Reconciliations.Any(r => r.Id == 41 && r.StatusId == (int)ECertificateOfOriginStatus.Published),
                Is.True, "the certificate is persisted as Published");
        });
    }

    // --- Branch 2/3 regression guard: the reconciliation gets a STRIPPED request (no invoice info) on release, so a
    // reconciled certificate is forced to Rejected — NOT run through real invoice matching (Finding #1). ---
    [Test]
    public async Task ReleasedReconciliationReceivesStrippedRequestForcingRejected()
    {
        var certs = new[]
        {
            // Received + a reconcilable reason/type + empty declaration → backfilled AND passes the reconciliation gate.
            Cert(id: 51, status: (int)ECertificateOfOriginStatus.Received,
                reason: (int)ERequestReason.NewCertificate, typeId: 1, exportDeclarationNumber: null),
        };
        // The incoming event DOES carry invoice info; the release path must STRIP it before reconciling. If the strip
        // regresses (the full request is reused), the reconciliation would run real matching on faked-empty data and
        // reach DeclarationMatch instead — so asserting Rejected here catches that regression.
        var request = Request((int)EEventType.ExportDeclarationReleased, [51], exportDeclarationNum: "REL-DEC");
        request.ExportInvoiceInfoList = [new ExportInvoiceInfoDto { ExternalIdNum = "INV-1" }];

        var cap = await RunDispatchAsync(request, certs);

        Assert.That(cap.Reconciliations.Any(r => r.Id == 51 && r.StatusId == (int)ECertificateOfOriginStatus.Rejected),
            Is.True, "with the stripped (invoice-less) request the reconciliation forces Rejected");
    }

    // ------------------------------------------------------------------------------------------------------------------

    private static CertificateOfOrigin Cert(
        int id,
        int status,
        int reason = (int)ERequestReason.NewCertificate,
        int typeId = 1,
        string? exportDeclarationNumber = "DEC",
        string? qrCodePath = "documents/existing")
    {
        return new CertificateOfOrigin
        {
            Id = id,
            TypeId = typeId,
            CertificateNumber = $"COO-{id}",
            CertificateOfOriginStatusId = status,
            RequestReasonCode = reason,
            ExportDeclarationNumber = exportDeclarationNumber,
            LeadDocumentId = null,
            OrganizationUnitId = 0,
            QrCodePath = qrCodePath,
            QrImage = null,
        };
    }

    private static UpdateCertificateOfOriginsRequestDto Request(int eventType, List<int> ids, string? exportDeclarationNum = "DEC")
    {
        return new UpdateCertificateOfOriginsRequestDto
        {
            EventType = eventType,
            CertificateOfOriginsIds = ids,
            ExportDeclarationNum = exportDeclarationNum,
            LeadDocumentId = 900,
            ExportInvoiceInfoList = [],
        };
    }

    private static async Task<Captures> RunDispatchAsync(UpdateCertificateOfOriginsRequestDto request, IReadOnlyList<CertificateOfOrigin> seededCerts)
    {
        var cap = new Captures();
        var byId = seededCerts.ToDictionary(c => c.Id);

        // A real in-memory context so BaseBL.SaveChangesAsync has a context to route through; the dispatcher branches
        // persist via the set-based DAL methods below (all faked/captured), not the EF upsert path.
        var dbOptions = new DbContextOptionsBuilder<CertificateOfOriginsDbContext>()
            .UseInMemoryDatabase($"coo-dispatch-{Guid.NewGuid()}")
            .Options;
        using var dbContext = new CertificateOfOriginsDbContext(dbOptions);

        var dataLayer = Fake<ICertificateOfOriginsDal>((method, args) =>
        {
            switch (method.Name)
            {
                case "GetCertificatesByIds":
                    var ids = (List<int>)args![0]!;
                    return Task.FromResult(ids.Where(byId.ContainsKey).Select(i => byId[i]).ToList());

                case "GetCertificateDetailsByCertificateIds":
                    return Task.FromResult(new List<CertificateOfOriginDetails>());

                case "GetCertificateInvoiceDetailsByCertificateIds":
                    return Task.FromResult(new List<CertificateReconcileInvoiceDto>());

                case "CancelCertificateFromMessage":
                    cap.Cancelled.Add(((int)args![0]!, (string)args[1]!));
                    return Task.CompletedTask;

                case "UpdateCertificateDeclarationLink":
                    cap.DeclarationLinkIds.Add((int)args![0]!);
                    return Task.CompletedTask;

                case "UpdateCertificateReconciliation":
                    cap.Reconciliations.Add(((int)args![0]!, (int)args[1]!));
                    return Task.CompletedTask;

                case "UpdateCertificateQrCode":
                    cap.QrWrites.Add(((int)args![0]!, (Guid?)args[1], (byte[]?)args[2]));
                    return Task.CompletedTask;

                case "get_DbContext":
                    return dbContext;

                case "SaveChangesAsync":
                    return Task.FromResult(0);

                default:
                    return null; // UpdateCertificatePublishingState / UpdateCertificateQrCodePath / AddCertificate... → Task
            }
        });

        var requestMetadata = Fake<IRequestMetadata>((method, _) => method.Name == "get_UserId" ? (int?)7 : null);

        var parametersUtil = Fake<IParametersUtil>((method, _) =>
        {
            if (method.Name == "Get")
            {
                var resultType = method.ReturnType.GetGenericArguments()[0];
                var value = resultType == typeof(string)
                    ? (object?)QueryUrlTemplate
                    : (resultType.IsValueType ? Activator.CreateInstance(resultType) : null); // bool → false
                return TaskFromResult(resultType, value);
            }

            return null;
        });

        var commonServices = Fake<ICommonServicesProxy>((method, _) =>
            method.Name == "CreateQrCode" ? Task.FromResult<byte[]?>([1, 2, 3, 4]) : null); // GenerateTemplate → null (default)

        var documentBuilder = default(IDocumentBuilder);
        documentBuilder = Fake<IDocumentBuilder>((method, _) =>
            method.Name == "Build" ? Fake<IDocument>() : (method.ReturnType == typeof(IDocumentBuilder) ? documentBuilder : null));
        var documentUtil = Fake<IDocumentUtil>((method, _) => method.Name switch
        {
            "CreateDocumentBuilder" => documentBuilder,
            "GetInvalidFilenameChars" => Array.Empty<char>(),
            "UploadDocument" => Task.FromResult<IDocumentResponse>(new FakeDocumentResponse()),
            _ => null,
        });

        // A chaining fake for the event builder, created over its interface type by reflection (no hard-coded type
        // name); WithEventType captures the raised event type id so a branch's events can be asserted.
        var builderType = typeof(IEventUtil).GetMethod(nameof(IEventUtil.CreatBuilder))!.ReturnType;
        var eventUtil = Fake<IEventUtil>((method, _) => method.Name switch
        {
            nameof(IEventUtil.CreatBuilder) => FakeBuilder(builderType, et => cap.RaisedEventTypes.Add(et)),
            nameof(IEventUtil.RaiseEvent) => new ValueTask(),
            _ => null,
        });

        var services = new ServiceCollection();
        services.AddSingleton<ILogger<CertificateOfOriginsBl>>(NullLogger<CertificateOfOriginsBl>.Instance);
        services.AddSingleton(dataLayer);
        services.AddSingleton(requestMetadata);
        services.AddSingleton(documentUtil);
        services.AddSingleton(eventUtil);
        services.AddSingleton(commonServices);
        services.AddSingleton(Fake<ICustomerProxy>());
        services.AddSingleton(Fake<IExportDealFileProxy>());
        services.AddSingleton(Fake<IUserProxy>());
        services.AddSingleton(Fake<IDataDictionaryFieldProxy>());
        services.AddSingleton(Fake<ICurrencyTypeProxy>());
        services.AddSingleton(Fake<IDocumentsProxy>());
        services.AddSingleton(Fake<ICustomsBookProxy>());
        services.AddSingleton(Fake<IOrganizationUnitProxy>());
        services.AddSingleton(Fake<IMessageManagementProxy>());
        services.AddSingleton(Fake<ICountryGroupProxy>());
        services.AddSingleton(Fake<ITasksProxy>());
        services.AddSingleton(Fake<ILockUtil>());
        services.AddSingleton(Fake<ICountryProxy>());
        services.AddSingleton(Fake<ISiteProxy>());
        services.AddSingleton(Fake<IInternationalSiteProxy>());
        services.AddSingleton(Fake<IPackingTypeProxy>());
        services.AddSingleton(Fake<IMeasurementUnitProxy>());
        var serviceProvider = services.BuildServiceProvider();

        var bl = new CertificateOfOriginsBl(serviceProvider, Fake<ILookupUtil>(), parametersUtil);

        await bl.UpdateCertificateOfOrigins(request);
        return cap;
    }

    private sealed class Captures
    {
        public List<(int Id, string Reason)> Cancelled { get; } = [];
        public List<int> DeclarationLinkIds { get; } = [];
        public List<(int Id, int StatusId)> Reconciliations { get; } = [];
        public List<(int Id, Guid? Guid, byte[]? Image)> QrWrites { get; } = [];
        public List<int> RaisedEventTypes { get; } = [];
    }

    private sealed class FakeDocumentResponse : IDocumentResponse
    {
        public string ExternalId { get; init; } = "documents/qr/resource-path";
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

    // Builds a chaining proxy for a builder interface known only at runtime (the event builder type). Any method that
    // returns the builder type returns the same proxy (fluent chaining); Build() and the rest return their defaults.
    private static object FakeBuilder(Type builderType, Action<int> onEventType)
    {
        var createMethod = typeof(DispatchProxy).GetMethods()
            .First(m => m.Name == nameof(DispatchProxy.Create) && m.GetGenericArguments().Length == 2 && m.GetParameters().Length == 0);
        var create = createMethod.MakeGenericMethod(builderType, typeof(InterfaceFake));
        var proxy = create.Invoke(null, null)!;
        ((InterfaceFake)proxy).Handler = (method, args) =>
        {
            if (method.Name == "WithEventType" && args is { Length: > 0 } && args[0] is int eventType)
            {
                onEventType(eventType);
            }

            return method.ReturnType == builderType ? proxy : DefaultReturn(method);
        };
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

        if (returnType == typeof(ValueTask))
        {
            return new ValueTask();
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
