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

// Legacy RaiseEventCertificateOfOriginUserApprovedCertificate (RaiseEventUtil.cs:165-196): a certificate saved to
// PendingRelease whose export declaration is already Released / Paroled / Closed is published at once through
// DeclarationReleased. The publish happens inside the status events, so the certificate gets no PendingRelease feedback,
// and the save's own Published branch does not run a second time (legacy's duplicate HandleCertificateReplacement).
[TestFixture]
public class SaveCertificateOfOriginAutoPublishTests
{
    private const int CertificateId = 31;
    private const int ReplacedCertificateId = 55;
    private const int LeadDocumentId = 900;
    private static readonly byte[] SeedRowVersion = [0, 0, 0, 0, 0, 0, 0, 1];

    [TestCase((int)ELeadDocumentState.Released)]
    [TestCase((int)ELeadDocumentState.Paroled)]
    [TestCase((int)ELeadDocumentState.Closed)]
    public async Task PendingReleaseWithAReleasedDeclarationIsPublishedAtOnce(int leadDocumentState)
    {
        var captures = await RunApproveAsync(leadDocumentState, declarationFound: true, ERequestReason.NewCertificate);

        Assert.Multiple(() =>
        {
            Assert.That(captures.ReconciliationStatuses, Is.EqualTo(new[] { (int)ECertificateOfOriginStatus.Published }),
                "DeclarationReleased persists the Published status");
            Assert.That(captures.EventTypes, Is.EqualTo(new[]
            {
                (int)EEventType.CertificateOfOriginUserApprovedCertificate,
                (int)EEventType.CertificateOfOriginCertificateIssued,
            }), "the approve event first, then one certificate-issued event — the publish is not repeated");
            Assert.That(captures.FeedbackMessages, Is.Zero, "an auto-published certificate gets no PendingRelease feedback, as legacy");
            Assert.That(captures.InfoLeadDocumentIds, Is.EqualTo(new[] { LeadDocumentId }),
                "the declaration info is read for the lead document the declaration lookup returned");
        });
    }

    [Test]
    public async Task PendingReleaseWithADeclarationNotYetReleasedStaysPendingRelease()
    {
        var captures = await RunApproveAsync((int)ELeadDocumentState.Submited, declarationFound: true, ERequestReason.NewCertificate);

        Assert.Multiple(() =>
        {
            Assert.That(captures.ReconciliationStatuses, Is.Empty, "no publish");
            Assert.That(captures.EventTypes, Is.EqualTo(new[] { (int)EEventType.CertificateOfOriginUserApprovedCertificate }));
            Assert.That(captures.FeedbackMessages, Is.EqualTo(1), "the PendingRelease feedback is sent");
        });
    }

    [Test]
    public async Task PendingReleaseWithNoDeclarationFoundStaysPendingRelease()
    {
        var captures = await RunApproveAsync((int)ELeadDocumentState.Released, declarationFound: false, ERequestReason.NewCertificate);

        Assert.Multiple(() =>
        {
            Assert.That(captures.ReconciliationStatuses, Is.Empty, "no publish");
            Assert.That(captures.InfoLeadDocumentIds, Is.Empty, "no declaration → its info is not read");
            Assert.That(captures.FeedbackMessages, Is.EqualTo(1), "the PendingRelease feedback is sent");
        });
    }

    [Test]
    public async Task AutoPublishedReplacementCancelsTheReplacedCertificateOnce()
    {
        var captures = await RunApproveAsync((int)ELeadDocumentState.Released, declarationFound: true, ERequestReason.CertificateReplacement);

        Assert.Multiple(() =>
        {
            Assert.That(captures.CancelledIds, Is.EqualTo(new[] { ReplacedCertificateId }), "the replaced certificate is cancelled once");
            Assert.That(captures.CancelReasons, Is.EqualTo(new[] { "תעודת מקור הוחלפה בתעודת מקור COO-2001" }), "legacy EMessages.CertificateReplaced");
            Assert.That(captures.FeedbackRemarks, Is.EqualTo(new[] { "סוכן המכס מתבקש לעדכן את הצהרת היצוא 260146, בעקבות החלפת תעודת מקור COO-OLD" }),
                "legacy EMessages.UpdateExportDeclaration on the replacing certificate");
            Assert.That(captures.EventTypes.Count(type => type == (int)EEventType.CertificateOfOriginCertificateReplaced), Is.EqualTo(1),
                "legacy raised CertificateReplaced twice here (inside DeclarationReleased and again in the save's Published branch)");
        });
    }

    // The worker approves an existing certificate: DeclarationMatch (6) → PendingRelease (7), so the status side effects run.
    private static async Task<Captures> RunApproveAsync(int leadDocumentState, bool declarationFound, ERequestReason reason)
    {
        var captures = new Captures();

        var dbOptions = new DbContextOptionsBuilder<CertificateOfOriginsDbContext>()
            .UseInMemoryDatabase($"coo-autopublish-{Guid.NewGuid()}")
            .ConfigureWarnings(warnings => warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        using var dbContext = new CertificateOfOriginsDbContext(dbOptions);
        dbContext.CertificateOfOrigins.Add(new CertificateOfOrigin
        {
            Id = CertificateId,
            CertificateNumber = "COO-2001",
            Title = "COO-2001",
            TimeStamp = SeedRowVersion,
            CreateDate = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Unspecified),
            CreateUserId = 99,
        });
        await dbContext.SaveChangesAsync();
        dbContext.ChangeTracker.Clear();

        var dataLayer = Fake<ICertificateOfOriginsDal>((method, args) =>
        {
            switch (method.Name)
            {
                case "GetLatestCertificateByNumber":
                    return Task.FromResult<CertificateOfOrigin?>(null);
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
                case "StageCertificateOfOriginDetails":
                case "StageCertificateOfOriginInvoices":
                case "StageCertificateOfOriginInvoiceItems":
                    return Task.CompletedTask;

                // DeclarationReleased reloads the certificate it publishes; HandleCertificateReplacement loads the replaced one.
                case "GetCertificatesByIds":
                    var ids = (List<int>)args![0]!;
                    var found = ids.Contains(ReplacedCertificateId)
                        ? [new CertificateOfOrigin { Id = ReplacedCertificateId, CertificateNumber = "COO-OLD", Title = "COO-OLD", OrganizationUnitId = 1 }]
                        : dbContext.CertificateOfOrigins.AsNoTracking().Where(c => ids.Contains(c.Id)).ToList();
                    return Task.FromResult(found);
                case "UpdateCertificateReconciliation":
                    captures.ReconciliationStatuses.Add((int)args![1]!);
                    return Task.CompletedTask;
                case "CancelCertificate":
                    captures.CancelledIds.Add((int)args![0]!);
                    captures.CancelReasons.Add((string)args[1]!);
                    return Task.CompletedTask;
                case "UpdateCertificateFeedbackRemark":
                    captures.FeedbackRemarks.Add((string?)args![1]);
                    return Task.CompletedTask;
                case "GetCertificateOfOriginById":
                    return Task.FromResult<CertificateOfOriginDto?>(new CertificateOfOriginDto { Id = (int)args![0]! });
                default:
                    return null;
            }
        });

        var exportDealFile = Fake<IExportDealFileProxy>((method, args) =>
        {
            switch (method.Name)
            {
                case nameof(IExportDealFileProxy.GetExportDeclarationDetailsForCertificateOfOrigion):
                    return Task.FromResult(declarationFound ? new ExportDeclarationDetailsDto { LeadDocumentId = LeadDocumentId } : null);
                case nameof(IExportDealFileProxy.GetExportDeclarationInfoForPc):
                    captures.InfoLeadDocumentIds.Add((int)args![0]!);
                    return Task.FromResult<ExportDeclarationInfoDto?>(new ExportDeclarationInfoDto { LeadDocumentId = LeadDocumentId, LeadDocumentState = leadDocumentState });
                default:
                    return null;
            }
        });

        var builderType = typeof(IEventUtil).GetMethod(nameof(IEventUtil.CreatBuilder))!.ReturnType;
        var eventUtil = Fake<IEventUtil>((method, _) => method.Name switch
        {
            nameof(IEventUtil.CreatBuilder) => FakeBuilder(builderType, captures),
            nameof(IEventUtil.RaiseEvent) => new ValueTask(),
            _ => null,
        });

        var messageManagement = Fake<IMessageManagementProxy>((method, _) =>
        {
            if (method.Name == nameof(IMessageManagementProxy.SendMessage))
            {
                captures.FeedbackMessages++;
            }

            return null;
        });

        var services = new ServiceCollection();
        services.AddSingleton<ILogger<CertificateOfOriginsBl>>(NullLogger<CertificateOfOriginsBl>.Instance);
        services.AddSingleton(dataLayer);
        services.AddSingleton(Fake<IRequestMetadata>((method, _) => method.Name == "get_UserId" ? (int?)7 : null));
        services.AddSingleton(Fake<IDocumentUtil>());
        services.AddSingleton(eventUtil);
        services.AddSingleton(Fake<ICommonServicesProxy>());
        services.AddSingleton(Fake<ICustomerProxy>());
        services.AddSingleton(exportDealFile);
        services.AddSingleton(Fake<IUserProxy>());
        services.AddSingleton(Fake<IDocumentsProxy>());
        services.AddSingleton(Fake<ICustomsBookProxy>());
        services.AddSingleton(Fake<IOrganizationUnitProxy>());
        services.AddSingleton(messageManagement);
        services.AddSingleton(Fake<ITasksProxy>());
        services.AddSingleton(Fake<ILockUtil>());
        var serviceProvider = services.BuildServiceProvider();

        var bl = new CertificateOfOriginsBl(serviceProvider, Fake<ILookupUtil>(), Fake<IParametersUtil>());

        await bl.SaveCertificateOfOrigin(new SaveCertificateOfOriginRequestDto
        {
            Id = CertificateId,
            TypeId = 1,
            Title = "COO-2001",
            CustomerId = 777,
            OrganizationUnitId = 1,
            CertificateNumber = "COO-2001",
            CertificateOfOriginStatusId = (int)ECertificateOfOriginStatus.PendingRelease,
            OriginalCertificateOfOriginStatusId = (int)ECertificateOfOriginStatus.DeclarationMatch,
            RequestReasonCode = (int)reason,
            CertificateIdToCancel = reason == ERequestReason.CertificateReplacement ? ReplacedCertificateId : null,
            ExportDeclarationNumber = "260146",
            TimeStamp = SeedRowVersion,
            CertificateOfOriginDetails = [],
        });

        return captures;
    }

    private sealed class Captures
    {
        public List<int> ReconciliationStatuses { get; } = [];
        public List<int> CancelledIds { get; } = [];
        public List<string> CancelReasons { get; } = [];
        public List<string?> FeedbackRemarks { get; } = [];
        public List<int> EventTypes { get; } = [];
        public List<int> InfoLeadDocumentIds { get; } = [];
        public int FeedbackMessages { get; set; }
    }

    // --- Minimal dependency-free interface faking over System.Reflection.DispatchProxy (no mocking package). ---

    private static T Fake<T>(Func<MethodInfo, object?[]?, object?>? handler = null) where T : class
    {
        var proxy = DispatchProxy.Create<T, InterfaceFake>();
        ((InterfaceFake)(object)proxy).Handler = (method, args) => handler?.Invoke(method, args) ?? DefaultReturn(method);
        return proxy;
    }

    // A chaining proxy for the event builder (its type is known only at runtime); records the event types raised.
    private static object FakeBuilder(Type builderType, Captures captures)
    {
        var proxy = typeof(DispatchProxy).GetMethods()
            .First(m => m.Name == nameof(DispatchProxy.Create) && m.GetGenericArguments().Length == 2 && m.GetParameters().Length == 0)
            .MakeGenericMethod(builderType, typeof(InterfaceFake))
            .Invoke(null, null)!;
        ((InterfaceFake)proxy).Handler = (method, args) =>
        {
            if (method.Name == "WithEventType")
            {
                captures.EventTypes.Add((int)args![0]!);
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
