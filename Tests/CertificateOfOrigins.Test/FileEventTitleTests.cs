using System.Reflection;
using CertificateOfOrigins.BL;
using CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore.Lookup;
using CustomsCloud.InfrastructureCore.Parameters;
using CustomsCloud.InfrastructureCore.Utils.Events;
using Microsoft.Extensions.DependencyInjection;

namespace CertificateOfOrigins.Test;

// Parity finding F-06: the legacy file entity's Title was computed ("  אימות מסמך מקור (יבוא) מספר פניה " + ID). The events the
// legacy raised with the file ENTITY itself (new VirtualEntity(file) / new EventUtilArguments(type, file)) carried that label as
// their Title; the migration sent the bare id for HandleImportAuthenticationRequest and for the vendor-reminder / final-decision
// file events (CloseTaskReminderNotice3Months already carried the label). The events the legacy built by hand keep the bare id.
[TestFixture]
public class FileEventTitleTests
{
    private const string Label = "  אימות מסמך מקור (יבוא) מספר פניה 11";

    [TestCase(EAuthenticationFileStatus.RightAuthenticationAnswer, EEventType.HandleImportAuthenticationRequest)]
    [TestCase(EAuthenticationFileStatus.AuthenticationRequestReminderWasSend, EEventType.UpdateFileStatusVendorReminderNotice)]
    [TestCase(EAuthenticationFileStatus.ReceivedAnswerInFile, EEventType.UpdateFileStatusFinalDecisionInCase)]
    public async Task TheEventsRaisedWithTheFileEntityCarryTheFilesLabel(EAuthenticationFileStatus status, EEventType expectedEvent)
    {
        var raised = new List<(int EventType, string? Title)>();
        var bl = Bl(raised);

        await (Task)typeof(AuthenticationRequestBl)
            .GetMethod("CheckStatusAndOpenTask", BindingFlags.NonPublic | BindingFlags.Instance)!
            .Invoke(bl, [EventUtil(raised), new SaveAuthenticationRequestFileRequestDto { Id = 11, AuthenticationFileStatusId = (int)status }, 7])!;

        Assert.That(raised.Where(e => e.EventType == (int)expectedEvent).Select(e => e.Title), Is.EqualTo(new[] { Label }));
    }

    [Test]
    public async Task CloseReminderTaskCarriesTheSameLabel()
    {
        var raised = new List<(int EventType, string? Title)>();

        await Bl(raised).CloseReminderTask(new CloseReminderTaskRequestDto { Id = 11, OrganizationUnitId = 1 });

        Assert.That(raised.Single().Title, Is.EqualTo(Label));
    }

    private static AuthenticationRequestBl Bl(List<(int EventType, string? Title)> raised)
    {
        var services = new ServiceCollection();
        services.AddSingleton<Microsoft.Extensions.Logging.ILogger<AuthenticationRequestBl>>(Microsoft.Extensions.Logging.Abstractions.NullLogger<AuthenticationRequestBl>.Instance);
        services.AddSingleton(EventUtil(raised));
        return new AuthenticationRequestBl(services.BuildServiceProvider(), Fake<IParametersUtil>(), Fake<ILookupUtil>());
    }

    private static IEventUtil EventUtil(List<(int EventType, string? Title)> raised)
    {
        var builderType = typeof(IEventUtil).GetMethod(nameof(IEventUtil.CreatBuilder))!.ReturnType;
        return Fake<IEventUtil>((method, _) => method.Name switch
        {
            nameof(IEventUtil.CreatBuilder) => FakeBuilder(builderType, raised),
            nameof(IEventUtil.RaiseEvent) => new ValueTask(),
            _ => null,
        });
    }

    // --- Minimal dependency-free interface faking over System.Reflection.DispatchProxy (no mocking package). ---

    private static T Fake<T>(Func<MethodInfo, object?[]?, object?>? handler = null) where T : class
    {
        var proxy = DispatchProxy.Create<T, InterfaceFake>();
        ((InterfaceFake)(object)proxy).Handler = (method, args) => handler?.Invoke(method, args) ?? DefaultReturn(method);
        return proxy;
    }

    // Records one (event type, title) entry per builder: the event type opens it, the title completes it.
    private static object FakeBuilder(Type builderType, List<(int EventType, string? Title)> raised)
    {
        var proxy = typeof(DispatchProxy).GetMethods()
            .First(m => m.Name == nameof(DispatchProxy.Create) && m.GetGenericArguments().Length == 2 && m.GetParameters().Length == 0)
            .MakeGenericMethod(builderType, typeof(InterfaceFake))
            .Invoke(null, null)!;
        var index = -1;
        ((InterfaceFake)proxy).Handler = (method, args) =>
        {
            if (method.Name == "WithEventType")
            {
                raised.Add((Convert.ToInt32(args![0]), null));
                index = raised.Count - 1;
            }

            if (method.Name == "WithTitle" && index >= 0)
            {
                raised[index] = (raised[index].EventType, (string?)args![0]);
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
