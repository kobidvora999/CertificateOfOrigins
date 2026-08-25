using System.Reflection;
using CustomsCloud.CRM.CertificateOfOrigins.BL;
using CustomsCloud.CRM.CertificateOfOrigins.BL.Proxies;
using CustomsCloud.CRM.CertificateOfOrigins.DAL;
using CustomsCloud.CRM.CertificateOfOrigins.Model.CertificateOfOriginsDb;
using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore.Interfaces.Http;
using CustomsCloud.InfrastructureCore.Lock;
using CustomsCloud.InfrastructureCore.Lookup;
using CustomsCloud.InfrastructureCore.Parameters;
using CustomsCloud.InfrastructureCore.Utils.Documents;
using CustomsCloud.InfrastructureCore.Utils.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
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

    // TimeStamp is the [Timestamp] concurrency token. An update must round-trip the stored row version or the save
    // fails the concurrency check (BaseBL maps that to RestConflictException) — so the seeded row and the update
    // request share this value. A create sends none.
    private static readonly byte[] SeedRowVersion = [0, 0, 0, 0, 0, 0, 0, 1];

    [Test]
    public async Task NewCertificatePublishedInOneSaveLinksQrDocumentToSavedIdNotZero()
    {
        var request = NewPublishedRequest(id: 0, originalStatusId: 0);

        var captures = await RunSaveAsync(request, seedExistingId: null);

        // The id is assigned by the real save (EF identity), not injected by a fake — so read it back from the
        // certificate the BL re-fetched after saving.
        var savedId = captures.Returned!.Id;

        Assert.Multiple(() =>
        {
            // The bug: the QR document was linked to certificate id 0. The fix links it to the real saved id.
            Assert.That(savedId, Is.Not.Zero, "the save must assign a real certificate id");
            Assert.That(captures.QrDocumentEntityId, Is.EqualTo(savedId),
                "the QR document must be linked to the saved certificate id, not 0");
            Assert.That(captures.QrDocumentEntityId, Is.Not.Zero);

            // The id-linked upload only becomes possible after the save, so at the main upsert the resource path was
            // not yet known. (EntityIdAtMainSave is deliberately NOT asserted to be 0: identity-value timing is
            // provider-specific — SQL Server assigns it on INSERT, the in-memory provider already at Add — so it
            // would test the provider, not this ordering fix.)
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

        var captures = await RunSaveAsync(request, seedExistingId: existingId);

        Assert.Multiple(() =>
        {
            Assert.That(captures.QrDocumentEntityId, Is.EqualTo(existingId));
            Assert.That(captures.QrPathUpdateId, Is.EqualTo(existingId));
            Assert.That(captures.QrPathUpdateValue, Is.EqualTo(UploadedExternalId));

            // CertificateOfOrigin is an ICloudEntity, so BaseBL.UpdateEntity marks CreateDate/CreateUserId
            // not-modified: the round-tripped DTO does not carry them, and without that guard the update would zero
            // them (CreateDate → 0001-01-01, a SqlDateTime overflow in production). This replaced the hand-rolled
            // Entry(...).IsModified = false guards the DAL used to apply.
            Assert.That(captures.PersistedCreateDate, Is.EqualTo(new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Unspecified)),
                "CreateDate must survive the update untouched");
            Assert.That(captures.PersistedCreateUserId, Is.EqualTo(99),
                "CreateUserId must survive the update untouched");
        });
    }

    private static SaveCertificateOfOriginRequestDto NewPublishedRequest(int id, int originalStatusId)
    {
        return new SaveCertificateOfOriginRequestDto
        {
            Id = id,
            TypeId = 1,
            // Title / CustomerId / OrganizationUnitId are NOT NULL columns and are now enforced by
            // SaveCertificateOfOriginRequestValidator. This fixture previously set only the fields the QR
            // assertions read, so it was not in fact a valid save request.
            Title = "COO-1001",
            CustomerId = 777,
            OrganizationUnitId = 1,
            CertificateNumber = "COO-1001",
            CertificateOfOriginStatusId = (int)ECertificateOfOriginStatus.Published,
            OriginalCertificateOfOriginStatusId = originalStatusId,
            RequestReasonCode = (int)ERequestReason.NewCertificate,
            FeedbackRemark = "remark",
            OriginalFeedbackRemark = "remark", // unchanged → no feedback-message side effect
            QrCodePath = null,                 // empty → QR generation is required on publish
            QrImage = null,
            TimeStamp = id == 0 ? null : SeedRowVersion, // an update round-trips the stored row version
            CertificateOfOriginDetails = [],   // no detail rows → no per-field enrichment proxy calls
        };
    }

    // seedExistingId: null → a brand-new certificate (the save assigns the id). Non-null → seed that row first so the
    // update path has something to update.
    private static async Task<Captures> RunSaveAsync(SaveCertificateOfOriginRequestDto request, int? seedExistingId)
    {
        var captures = new Captures();

        // A REAL DbContext (in-memory). The certificate upsert now runs through BaseBL.AddEntity/UpdateEntity +
        // SaveChangesAsync, and BaseBL routes both through ((IBaseDal)DataLayer).DbContext / .SaveChangesAsync — so the
        // fake DAL below hands it this context. That makes the id assignment real: for a new certificate the id is
        // generated by the save, exactly as in production, instead of being injected by the fake.
        var dbOptions = new DbContextOptionsBuilder<CertificateOfOriginsDbContext>()
            .UseInMemoryDatabase($"coo-qr-{Guid.NewGuid()}")
            .AddInterceptors(new UpsertSnapshotInterceptor(captures))
            .Options;
        using var dbContext = new CertificateOfOriginsDbContext(dbOptions);
        if (seedExistingId is int existingId)
        {
            // The update path needs the row to exist. Seeded with audit values the save must PRESERVE — UpdateEntity
            // marks CreateDate/CreateUserId not-modified.
            dbContext.CertificateOfOrigins.Add(new CertificateOfOrigin
            {
                Id = existingId,
                CertificateNumber = request.CertificateNumber,
                Title = request.CertificateNumber,
                TimeStamp = SeedRowVersion,
                CreateDate = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Unspecified),
                CreateUserId = 99,
            });
            await dbContext.SaveChangesAsync();
            dbContext.ChangeTracker.Clear();
            captures.MainSaveCaptured = false; // the seed save must not consume the snapshot
        }

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

                // The parent upsert is no longer a DAL call. BaseBL.AddEntity/UpdateEntity route through these IBaseDal
                // members — Add/Update to track the entity, DbContext for the CreateDate/CreateUserId not-modified
                // guards, SaveChangesAsync to commit — so they are wired to the real in-memory context. Add/Update
                // return EntityEntry<ICloudEntity>, which BaseBL discards, so returning null here is safe.
                case "Add":
                    dbContext.Add(args![0]!);
                    return null;

                case "Update":
                    dbContext.Update(args![0]!);
                    return null;

                case "get_DbContext":
                    return dbContext;

                case "SaveChangesAsync":
                    return dbContext.SaveChangesAsync();

                // The DAL now only STAGES the child rows (no audit columns); the BL commits them via
                // SaveChangesAsync, which is what maps a concurrency conflict to 409 instead of letting it
                // escape as a 500. Three steps because EF must assign the invoice ids before the items bind.
                case "StageCertificateOfOriginDetails":
                case "StageCertificateOfOriginInvoices":
                case "StageCertificateOfOriginInvoiceItems":
                    return Task.CompletedTask;

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

        // C11: the BL obtains proxies/utils lazily via Resolve<T>() from the service provider, so they are registered
        // here instead of being passed to the constructor. commonServices drives the QR generation on the publish path;
        // the rest return defaults (null/false) so their branches no-op for the two save scenarios under test.
        services.AddSingleton(commonServices);
        services.AddSingleton(Fake<ICustomerProxy>());
        services.AddSingleton(Fake<IExportDealFileProxy>()); // GetLeadDocument... → null (default) → LinkLeadDocument returns early
        services.AddSingleton(Fake<IUserProxy>());           // GetUsersByIds → null (default) → org unit resolves to 0
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

        var bl = new CertificateOfOriginsBl(
            serviceProvider,
            Fake<ILookupUtil>(),
            parametersUtil);

        captures.Returned = await bl.SaveCertificateOfOrigin(request);

        // Read the persisted row back so the audit columns can be asserted (they are stamped by the infrastructure,
        // not by this repo's code, since the entity is an ICloudEntity).
        var persisted = await dbContext.CertificateOfOrigins.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == captures.Returned!.Id);
        captures.PersistedCreateDate = persisted?.CreateDate;
        captures.PersistedCreateUserId = persisted?.CreateUserId;

        return captures;
    }

    private sealed class Captures
    {
        public int QrDocumentEntityId = int.MinValue;
        public int QrPathUpdateId = int.MinValue;
        public string? QrPathUpdateValue;
        public bool MainSaveCaptured;
        public DateTime? PersistedCreateDate;
        public int? PersistedCreateUserId;
        public int EntityIdAtMainSave = int.MinValue;
        public byte[]? QrImageAtMainSave;
        public Guid? GuidAtMainSave;
        public string? QrCodePathAtMainSave = "<not-captured>";
        public CertificateOfOriginDto? Returned;
    }

    // Snapshots the certificate exactly as it is being written by the main upsert — the seam the fake DAL used to
    // provide. At SavingChanges the identity id is still unassigned for a new row, which is what lets the test prove
    // the QR document is linked only AFTER the save.
    private sealed class UpsertSnapshotInterceptor(Captures captures) : SaveChangesInterceptor
    {
        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            if (!captures.MainSaveCaptured)
            {
                var entry = eventData.Context?.ChangeTracker.Entries<CertificateOfOrigin>().FirstOrDefault();
                if (entry is not null)
                {
                    captures.MainSaveCaptured = true;
                    captures.EntityIdAtMainSave = entry.Entity.Id;
                    captures.QrImageAtMainSave = entry.Entity.QrImage;
                    captures.GuidAtMainSave = entry.Entity.Guid;
                    captures.QrCodePathAtMainSave = entry.Entity.QrCodePath;
                }
            }

            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }
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
