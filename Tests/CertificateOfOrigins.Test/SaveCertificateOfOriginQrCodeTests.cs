using System.Reflection;
using CertificateOfOrigins.BL;
using CertificateOfOrigins.BL.Lookups;
using CertificateOfOrigins.BL.Proxies;
using CertificateOfOrigins.DAL;
using CertificateOfOrigins.Model.CertificateOfOriginsDb;
using CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore.BL.Exceptions;
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

namespace CertificateOfOrigins.Test;

// The QR of a published certificate, as legacy CreateQRCodeIfNeededAndUpload: generated and uploaded BEFORE the commit,
// as a file attached to no entity (UploadFile(..., -1, ...)), with QrImage, Guid and QrCodePath persisted together with the
// certificate (parity findings AN-01, AN-02, A-20). Linked to the certificate, the publish's replace-the-documents step
// deleted the QR document it had just created.
[TestFixture]
public class SaveCertificateOfOriginQrCodeTests
{
    private const string QueryUrlTemplate = "https://verify.example/{0}";
    private const string UploadedExternalId = "external-42";
    private const string UploadedFileResource = "documents/qr/IL0000000001.jpg";
    private const string FileServerUrl = "http://files.example:91/documents/qr/IL0000000001.jpg";

    // TimeStamp is the [Timestamp] concurrency token. An update must round-trip the stored row version or the save
    // fails the concurrency check (BaseBL maps that to RestConflictException) — so the seeded row and the update
    // request share this value. A create sends none.
    private static readonly byte[] SeedRowVersion = [0, 0, 0, 0, 0, 0, 0, 1];

    [Test]
    public async Task NewCertificatePublishedInOneSaveUploadsQrUnlinkedAndPersistsPathForSavedId()
    {
        var request = NewPublishedRequest(0, 0);

        var captures = await RunSaveAsync(request, null);

        // The id is assigned by the real save (EF identity), not injected by a fake — so read it back from the
        // certificate the BL re-fetched after saving.
        var savedId = captures.Returned!.Id;

        Assert.Multiple(() =>
        {
            Assert.That(savedId, Is.Not.Zero, "the save must assign a real certificate id");

            // Legacy uploaded the QR as a bare file attached to no entity (UploadFile(..., -1, ...)); linked to the
            // certificate, the publish's replace-the-documents step deleted it (parity finding AN-02).
            Assert.That(captures.QrDocumentEntityId, Is.EqualTo(int.MinValue),
                "the QR document must not be linked to the certificate");

            // QrImage, Guid and QrCodePath all persist with the main upsert, as legacy committed them together with the
            // certificate. QrCodePath is the uploaded file's URL, which the print templates read as the QR image source
            // (legacy GetDocumentFile, parity finding A-20).
            Assert.That(captures.QrImageAtMainSave, Is.Not.Null.And.Not.Empty,
                "the QR image bytes must persist with the main upsert");
            Assert.That(captures.GuidAtMainSave, Is.Not.Null.And.Not.EqualTo(Guid.Empty),
                "the certificate Guid must be stamped and persisted with the main upsert");
            Assert.That(captures.QrCodePathAtMainSave, Is.EqualTo(FileServerUrl),
                "QrCodePath must be the file-server URL of the uploaded file, persisted with the main upsert");
            Assert.That(captures.QrPathUpdateId, Is.EqualTo(int.MinValue), "no separate QrCodePath write after the commit");
        });
    }

    [Test]
    public async Task UpdateExistingPublishedUploadsQrUnlinkedAndPersistsPathForExistingId()
    {
        const int existingId = 55;
        // An update (Id != 0) publishing with an empty QrCodePath still generates + uploads the QR, and QrCodePath is
        // persisted with the upsert.
        var request = NewPublishedRequest(existingId, (int)ECertificateOfOriginStatus.Published);

        var captures = await RunSaveAsync(request, existingId);

        Assert.Multiple(() =>
        {
            Assert.That(captures.QrDocumentEntityId, Is.EqualTo(int.MinValue), "the QR document must not be linked to the certificate");
            Assert.That(captures.QrCodePathAtMainSave, Is.EqualTo(FileServerUrl));
            Assert.That(captures.QrPathUpdateId, Is.EqualTo(int.MinValue), "no separate QrCodePath write after the commit");

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

    // C F-09 regression (also exercises this harness with a detail row): legacy CheckIfCountryGroupIsInTradeAgreement
    // wrote the group's English name as the detail's DisplayedValue. The migration left the numeric id there, so the id
    // was persisted and printed. A group id that is not in the lookup keeps the raw value as its display.
    [TestCase((int)ECertificateDetailsType.TradeAgreementGroupOfCountries)]
    [TestCase((int)ECertificateDetailsType.OriginGroupOfCountries)]
    [TestCase((int)ECertificateDetailsType.DestinationGroupOfCountries)]
    [TestCase((int)ECertificateDetailsType.CumulationGroupOfCountries)]
    public async Task ExistingCertificateCountryGroupDetailIsDisplayedByTheGroupEnglishName(int detailType)
    {
        const int existingId = 55;
        var request = NewPublishedRequest(existingId, (int)ECertificateOfOriginStatus.Published);
        request.CertificateOfOriginDetails =
        [
            new CertificateOfOriginDetailDto { CertificateDetailsTypeCodeId = detailType, Value = "5", DisplayedValue = "5" },
            new CertificateOfOriginDetailDto { CertificateDetailsTypeCodeId = detailType, Value = "999", DisplayedValue = "999" },
        ];
        var lookups = Fake<ILookupUtil>((method, args) =>
        {
            if (method.Name != "Get" || method.ReturnType.GetGenericArguments()[0] != typeof(CountryGroup))
            {
                return null;
            }

            CountryGroup? group = (int)args![0]! == 5 ? new CountryGroup { Id = 5, EnglishName = "Europe" } : null;
            return TaskFromResult(typeof(CountryGroup), group);
        });

        var captures = await RunSaveAsync(request, existingId, lookups);

        Assert.Multiple(() =>
        {
            Assert.That(captures.StagedDetails!.Select(d => d.DisplayedValue), Is.EqualTo(new[] { "Europe", "999" }));
            Assert.That(captures.StagedDetails!.Select(d => d.Value), Is.EqualTo(new[] { "5", "999" }), "the stored value stays the group id");
        });
    }

    // A-21 / AN-03: the existing-certificate display follows legacy CheckSpecificField case by case. The three site-id port
    // types show the international site's English name (CheckIfSiteExist); the Israel-only country types, an out-of-range
    // declaration date and a type outside the switch keep the client-sent display (legacy never touched them).
    [Test]
    public async Task ExistingCertificateDisplayFollowsTheLegacyCaseForEachDetailType()
    {
        const int existingId = 55;
        var request = NewPublishedRequest(existingId, (int)ECertificateOfOriginStatus.Published);
        request.CertificateOfOriginDetails =
        [
            new CertificateOfOriginDetailDto { CertificateDetailsTypeCodeId = (int)ECertificateDetailsType.ExportPort, Value = "1", DisplayedValue = "1" },
            new CertificateOfOriginDetailDto { CertificateDetailsTypeCodeId = (int)ECertificateDetailsType.PortOfEntrance, Value = "1", DisplayedValue = "1" },
            new CertificateOfOriginDetailDto { CertificateDetailsTypeCodeId = (int)ECertificateDetailsType.ExitPort, Value = "1", DisplayedValue = "1" },
            // PortOfShipment of an existing certificate is the stored site id (legacy GetIdByCode returned the parsed value).
            new CertificateOfOriginDetailDto { CertificateDetailsTypeCodeId = (int)ECertificateDetailsType.PortOfShipment, Value = "1", DisplayedValue = "1" },
            new CertificateOfOriginDetailDto { CertificateDetailsTypeCodeId = (int)ECertificateDetailsType.PortOfShipment, Value = "ILHFA", DisplayedValue = "as sent" },
            // ExportCountry: legacy CheckExportCountry set the country's English name.
            new CertificateOfOriginDetailDto { CertificateDetailsTypeCodeId = (int)ECertificateDetailsType.ExportCountry, Value = "376", DisplayedValue = "stale" },
            new CertificateOfOriginDetailDto { CertificateDetailsTypeCodeId = (int)ECertificateDetailsType.ExporterCountry, Value = "376", DisplayedValue = "Israel (as sent)" },
            new CertificateOfOriginDetailDto { CertificateDetailsTypeCodeId = (int)ECertificateDetailsType.DateOfDeclaration, Value = "01/01/2020", DisplayedValue = "as sent" },
            new CertificateOfOriginDetailDto { CertificateDetailsTypeCodeId = (int)ECertificateDetailsType.ExporterName, Value = "Acme", DisplayedValue = "old" },
        ];
        var lookups = Fake<ILookupUtil>((method, args) =>
        {
            if (method.Name == "Get" && method.ReturnType.GetGenericArguments()[0] == typeof(InternationalSite))
            {
                var site = (int)args![0]! == 1 ? new InternationalSite { Id = 1, EnglishName = "Haifa" } : null;
                return TaskFromResult(typeof(InternationalSite), site);
            }

            if (method.Name == "Get" && method.ReturnType.GetGenericArguments()[0] == typeof(Lookup.Country))
            {
                var country = (int)args![0]! == 376 ? new Lookup.Country(376, "ישראל", 1, null, "Israel", "IL", "ISR", true, 376) : null;
                return TaskFromResult(typeof(Lookup.Country), country);
            }

            return null;
        });

        var captures = await RunSaveAsync(request, existingId, lookups);

        Assert.That(captures.StagedDetails!.Select(d => d.DisplayedValue), Is.EqualTo(new[]
        {
            "Haifa", "Haifa", "Haifa",
            "Haifa", "as sent",
            "Israel",
            "Israel (as sent)",
            "as sent",
            "Acme",
        }));
    }

    // AN-04: a new certificate converts its DestinationCountry alpha-2 / PortOfShipment locode to an id. Legacy
    // SystemTablesUtil.GetIdByCode threw TheValueInFieldNotExistsInSystem for a code it did not find, and the save failed
    // with nothing written; the new code answers 400 before the first write (AN-01), never keeping the raw code.
    [TestCase((int)ECertificateDetailsType.DestinationCountry, "XX")]
    [TestCase((int)ECertificateDetailsType.PortOfShipment, "XXXXX")]
    public void NewCertificateWithAnUnknownCodeIsRejectedBeforeAnyWrite(int detailType, string unknownCode)
    {
        var request = NewPublishedRequest(0, 0);
        request.CertificateOfOriginDetails =
        [
            new CertificateOfOriginDetailDto { CertificateDetailsTypeCodeId = detailType, Value = unknownCode, DisplayedValue = unknownCode },
        ];
        // Every code lookup finds nothing.
        var lookups = Fake<ILookupUtil>((method, _) =>
        {
            if (method.Name != "Search")
            {
                return null;
            }

            var rowType = method.ReturnType.GetGenericArguments()[0].GetGenericArguments()[0];
            var empty = Array.CreateInstance(rowType, 0);
            return TaskFromResult(method.ReturnType.GetGenericArguments()[0], empty);
        });

        Assert.ThrowsAsync<RestValidationException>(() => RunSaveAsync(request, null, lookups));
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
            QrCodePath = null,                 // empty → QR generation is required on publish
            QrImage = null,
            TimeStamp = id == 0 ? null : SeedRowVersion, // an update round-trips the stored row version
            CertificateOfOriginDetails = [],   // no detail rows → no per-field enrichment proxy calls
        };
    }

    // seedExistingId: null → a brand-new certificate (the save assigns the id). Non-null → seed that row first so the
    // update path has something to update.
    private static async Task<Captures> RunSaveAsync(SaveCertificateOfOriginRequestDto request, int? seedExistingId, ILookupUtil? lookupUtil = null)
    {
        var captures = new Captures();

        // A REAL DbContext (in-memory). The certificate upsert now runs through BaseBL.AddEntity/UpdateEntity +
        // SaveChangesAsync, and BaseBL routes both through ((IBaseDal)DataLayer).DbContext / .SaveChangesAsync — so the
        // fake DAL below hands it this context. That makes the id assignment real: for a new certificate the id is
        // generated by the save, exactly as in production, instead of being injected by the fake.
        var dbOptions = new DbContextOptionsBuilder<CertificateOfOriginsDbContext>()
            .UseInMemoryDatabase($"coo-qr-{Guid.NewGuid()}")
            // The save runs in a transaction (AN-01); the in-memory store has none, so the warning must not throw.
            .ConfigureWarnings(warnings => warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
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
                // NewPublishedRequest always sets CertificateNumber, so the seeded row copies it as-is.
                CertificateNumber = request.CertificateNumber!,
                Title = request.CertificateNumber!,
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
            "UploadDocument" => Task.FromResult<IDocumentResponse>(new FakeDocumentResponse { ExternalId = UploadedExternalId, FileResource = UploadedFileResource }),
            "GetFileUrl" when args is [string resource] => new Uri("http://files.example:91/" + resource),
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
                    captures.StagedDetails = (List<CertificateOfOriginDetails>)args![1]!;
                    return Task.CompletedTask;

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
        services.AddSingleton(Fake<IDocumentsProxy>());
        services.AddSingleton(Fake<ICustomsBookProxy>());
        services.AddSingleton(Fake<IOrganizationUnitProxy>());
        services.AddSingleton(Fake<IMessageManagementProxy>());
        services.AddSingleton(Fake<ITasksProxy>());
        services.AddSingleton(Fake<ILockUtil>());
        var serviceProvider = services.BuildServiceProvider();

        var bl = new CertificateOfOriginsBl(
            serviceProvider,
            lookupUtil ?? Fake<ILookupUtil>(),
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
        public List<CertificateOfOriginDetails>? StagedDetails;
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
