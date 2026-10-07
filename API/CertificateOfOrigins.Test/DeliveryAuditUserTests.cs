using System.Reflection;
using CertificateOfOrigins.BL;
using CertificateOfOrigins.DAL;
using CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore.Interfaces.Http;
using CustomsCloud.InfrastructureCore.Lookup;
using CustomsCloud.InfrastructureCore.Parameters;
using CustomsCloud.InfrastructureCore.Utils.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace CertificateOfOrigins.Test;

// Parity finding G F3: legacy saved the file / child requests / request through the infrastructure repository, which stamps
// the update user. The migration's set-based delivery writes set UpdateDate but never UpdateUserId, so after a delivery the
// row kept the previous editor's id. Every other writer in the DAL takes the user id; these two now do as well. The set-based
// SQL itself cannot run on the in-memory provider, so the test pins what the BL hands to the DAL: the current user.
[TestFixture]
public class DeliveryAuditUserTests
{
    private const int CurrentUserId = 7;

    [Test]
    public async Task VendorDeliveryStampsTheCurrentUserOnTheFileWrite()
    {
        var calls = new List<string>();

        await Bl(calls).HandleImportAuthenticationRequestDeliveryAndReminderForVendorSent(new HandleDeliveryAndReminderForVendorSentRequestDto
        {
            Id = 11,
            AuthenticationFileStatusId = (int)EAuthenticationFileStatus.AuthenticationRequestReminderWasSend,
            DeliveryMethodId = 1,
            IsDelivery = true,
        });

        Assert.That(calls, Is.EqualTo(new[] { $"UpdateFileAfterDelivery(file=11,user={CurrentUserId})" }));
    }

    [Test]
    public async Task ImporterDeliveryStampsTheCurrentUserOnTheRequestAndTheFileWrites()
    {
        var calls = new List<string>();

        await Bl(calls).HandleImportAuthenticationRequestDeliveryForImporterSent(new HandleDeliveryOrReminderForImporterSentRequestDto
        {
            DocumentId = 21,
            AuthenticationFileId = 11,
            OrganizationUnitId = 1,
            AuthenticationFileStatusId = (int)EAuthenticationFileStatus.AuthenticationRequestReminderWasSend,
            DeliveryMethodId = 1,
        });

        Assert.That(calls, Is.EqualTo(new[]
        {
            $"UpdateRequestDecisionAfterDelivery(document=21,user={CurrentUserId})",
            $"UpdateFileAfterDelivery(file=11,user={CurrentUserId})",
        }));
    }

    // Parity finding G-F7: legacy echoed back the entity it received with what the server changed in it, and the legacy
    // client kept the importer-reminder result as the selected request without re-reading it. The result carries every
    // changed field, with the very stamps that were written.
    [Test]
    public async Task VendorDeliveryReturnsTheDatesItWrote()
    {
        var clocks = new List<DateTime>();

        var result = await Bl([], clocks).HandleImportAuthenticationRequestDeliveryAndReminderForVendorSent(new HandleDeliveryAndReminderForVendorSentRequestDto
        {
            Id = 11,
            AuthenticationFileStatusId = (int)EAuthenticationFileStatus.AuthenticationRequestReminderWasSend,
            DeliveryMethodId = 1,
            IsDelivery = true,
        });

        Assert.Multiple(() =>
        {
            Assert.That(result.LastDelivery, Is.EqualTo(clocks.Single().Date));
            Assert.That(result.UpdateDate, Is.EqualTo(clocks.Single().Date));
        });
    }

    [Test]
    public async Task ImporterReminderReturnsEveryFieldItChangedOnTheRequestAndTheFile()
    {
        var clocks = new List<DateTime>();

        var result = await Bl([], clocks).HandleImportAuthenticationRequestDeliveryReminderForImporterSent(new HandleDeliveryOrReminderForImporterSentRequestDto
        {
            DocumentId = 21,
            AuthenticationFileId = 11,
            OrganizationUnitId = 1,
            AuthenticationFileStatusId = (int)EAuthenticationFileStatus.AuthenticationRequestReminderWasSend,
            DeliveryMethodId = 1,
        });

        Assert.That(clocks.Distinct().Count(), Is.EqualTo(1), "one clock for the request and the file writes");
        var now = clocks[0];
        Assert.Multiple(() =>
        {
            Assert.That(result.DecisionId, Is.EqualTo((int)EAuthenticationRequestDecision.ReminderForImporterWasSent));
            Assert.That(result.LastDeliveryForImporter, Is.EqualTo(now.Date));
            Assert.That(result.UpdateDate, Is.EqualTo(now), "the file's loop over its requests overwrote it with the full timestamp");
            Assert.That(result.FileLastDelivery, Is.EqualTo(now.Date));
            Assert.That(result.FileUpdateDate, Is.EqualTo(now.Date));
        });
    }

    [Test]
    public async Task ImporterDeliveryWithoutAFileReturnsNoFileDates()
    {
        var clocks = new List<DateTime>();

        var result = await Bl([], clocks).HandleImportAuthenticationRequestDeliveryForImporterSent(new HandleDeliveryOrReminderForImporterSentRequestDto
        {
            DocumentId = 21,
            AuthenticationFileId = null,
            OrganizationUnitId = 1,
            AuthenticationFileStatusId = (int)EAuthenticationFileStatus.AuthenticationRequestReminderWasSend,
            DeliveryMethodId = 1,
        });

        Assert.Multiple(() =>
        {
            Assert.That(result.UpdateDate, Is.EqualTo(clocks.Single().Date));
            Assert.That(result.FileLastDelivery, Is.Null);
            Assert.That(result.FileUpdateDate, Is.Null);
        });
    }

    private static AuthenticationRequestBl Bl(List<string> calls, List<DateTime>? clocks = null)
    {
        var dataLayer = Fake<ICertificateOfOriginsDal>((method, args) =>
        {
            switch (method.Name)
            {
                case "UpdateFileAfterDelivery":
                    calls.Add($"UpdateFileAfterDelivery(file={args![0]},user={args[3]})");
                    clocks?.Add((DateTime)args[4]!);
                    return Task.FromResult(true);
                case "UpdateRequestDecisionAfterDelivery":
                    calls.Add($"UpdateRequestDecisionAfterDelivery(document={args![0]},user={args[2]})");
                    clocks?.Add((DateTime)args[3]!);
                    return Task.FromResult(true);
                default:
                    return null;
            }
        });

        var builderType = typeof(IEventUtil).GetMethod(nameof(IEventUtil.CreatBuilder))!.ReturnType;
        var services = new ServiceCollection();
        services.AddSingleton<ILogger<AuthenticationRequestBl>>(NullLogger<AuthenticationRequestBl>.Instance);
        services.AddSingleton(dataLayer);
        services.AddSingleton(Fake<IRequestMetadata>((method, _) => method.Name == "get_UserId" ? (int?)CurrentUserId : null));
        services.AddSingleton(Fake<IEventUtil>((method, _) => method.Name == nameof(IEventUtil.CreatBuilder) ? FakeChainingBuilder(builderType) : null));

        return new AuthenticationRequestBl(services.BuildServiceProvider(), Fake<IParametersUtil>(), Fake<ILookupUtil>());
    }

    // --- Minimal dependency-free interface faking over System.Reflection.DispatchProxy (no mocking package). ---

    private static T Fake<T>(Func<MethodInfo, object?[]?, object?>? handler = null) where T : class
    {
        var proxy = DispatchProxy.Create<T, InterfaceFake>();
        ((InterfaceFake)(object)proxy).Handler = (method, args) => handler?.Invoke(method, args) ?? DefaultReturn(method);
        return proxy;
    }

    // The event builder's type is known only at runtime; every fluent call returns the same proxy.
    private static object FakeChainingBuilder(Type builderType)
    {
        var proxy = typeof(DispatchProxy).GetMethods()
            .First(m => m.Name == nameof(DispatchProxy.Create) && m.GetGenericArguments().Length == 2 && m.GetParameters().Length == 0)
            .MakeGenericMethod(builderType, typeof(InterfaceFake))
            .Invoke(null, null)!;
        ((InterfaceFake)proxy).Handler = (method, _) => method.ReturnType == builderType ? proxy : DefaultReturn(method);
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
