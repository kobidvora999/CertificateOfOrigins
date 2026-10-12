using System.Reflection;
using CertificateOfOrigins.BL;
using CertificateOfOrigins.BL.Proxies;
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

// A file-status change in SaveAuthenticationRequestFile (legacy ManageImportAuthenticationFileStatus), against legacy:
// - FN-01: cancelling a file detached its children first, so the rejection loop that followed ran over an empty collection
//   and opened no "handle rejected request" task.
// - FN-02: a file with no child requests has unit 0; legacy raised its events with no unit, the .NET 10 builder throws on 0.
[TestFixture]
public class FileStatusChangeParityTests
{
    [Test]
    public async Task CancellingAFileOpensNoRejectionTaskForItsRejectedChildren()
    {
        var raised = await ChangeStatus(
            (int)EAuthenticationFileStatus.CancelledFile,
            organizationUnitId: 7,
            [new SaveAuthenticationRequestFileChildDto { DocumentId = 1, OrganizationUnitId = 7, DecisionId = (int)EAuthenticationRequestDecision.Rejection }]);

        Assert.That(raised.EventTypes, Does.Not.Contain((int)EEventType.AuthenticationRequestRejected));
    }

    // The control: the same rejected child on a status that keeps the children does open the task.
    [Test]
    public async Task AStatusThatKeepsTheChildrenOpensTheRejectionTask()
    {
        var raised = await ChangeStatus(
            (int)EAuthenticationFileStatus.RightAuthenticationAnswer,
            organizationUnitId: 7,
            [new SaveAuthenticationRequestFileChildDto { DocumentId = 1, OrganizationUnitId = 7, DecisionId = (int)EAuthenticationRequestDecision.Rejection }]);

        Assert.That(raised.EventTypes, Does.Contain((int)EEventType.AuthenticationRequestRejected));
    }

    [Test]
    public async Task AFileWithNoRequestsAndNoUnitStillRaisesItsEventsWithoutAUnit()
    {
        var raised = await ChangeStatus((int)EAuthenticationFileStatus.RightAuthenticationAnswer, organizationUnitId: 0, []);

        Assert.Multiple(() =>
        {
            Assert.That(raised.EventTypes, Is.Not.Empty, "the events are raised");
            Assert.That(raised.Units, Is.Empty, "no organization unit is set when there is none");
        });
    }

    private static async Task<(List<int> EventTypes, List<int> Units)> ChangeStatus(int statusId, int organizationUnitId, List<SaveAuthenticationRequestFileChildDto> children)
    {
        var eventTypes = new List<int>();
        var units = new List<int>();
        var builderType = typeof(IEventUtil).GetMethod(nameof(IEventUtil.CreatBuilder))!.ReturnType;
        var eventUtil = Fake<IEventUtil>((method, _) => method.Name switch
        {
            nameof(IEventUtil.CreatBuilder) => FakeBuilder(builderType, eventTypes, units),
            nameof(IEventUtil.RaiseEvent) => new ValueTask(),
            _ => null,
        });

        var services = new ServiceCollection();
        services.AddSingleton<ILogger<AuthenticationRequestBl>>(NullLogger<AuthenticationRequestBl>.Instance);
        services.AddSingleton(eventUtil);
        services.AddSingleton(Fake<ICertificateOfOriginsDal>());
        services.AddSingleton(Fake<IRequestMetadata>());
        services.AddSingleton(Fake<ICollateralProxy>());
        services.AddSingleton(Fake<IMessageManagementProxy>());
        services.AddSingleton(Fake<ITasksProxy>());
        var bl = new AuthenticationRequestBl(services.BuildServiceProvider(), Fake<IParametersUtil>(), Fake<ILookupUtil>());

        var request = new SaveAuthenticationRequestFileRequestDto
        {
            Id = 11,
            AuthenticationFileStatusId = statusId,
            OriginalAuthenticationFileStatusId = (int)EAuthenticationFileStatus.ClarificationRequired,
            OrganizationUnitId = organizationUnitId,
            Requests = children,
        };

        await (Task)typeof(AuthenticationRequestBl)
            .GetMethod("ManageFileStatus", BindingFlags.NonPublic | BindingFlags.Instance)!
            .Invoke(bl, [request])!;
        return (eventTypes, units);
    }

    private static T Fake<T>(Func<MethodInfo, object?[]?, object?>? handler = null) where T : class
    {
        var proxy = DispatchProxy.Create<T, InterfaceFake>();
        ((InterfaceFake)(object)proxy).Handler = (method, args) => handler?.Invoke(method, args) ?? DefaultReturn(method);
        return proxy;
    }

    // A chaining builder: records WithEventType / WithOrganizationUnitId and throws on a 0 unit, as the .NET 10 builder does.
    private static object FakeBuilder(Type builderType, List<int> eventTypes, List<int> units)
    {
        var proxy = typeof(DispatchProxy).GetMethods()
            .First(m => m.Name == nameof(DispatchProxy.Create) && m.GetGenericArguments().Length == 2 && m.GetParameters().Length == 0)
            .MakeGenericMethod(builderType, typeof(InterfaceFake))
            .Invoke(null, null)!;
        ((InterfaceFake)proxy).Handler = (method, args) =>
        {
            if (method.Name == "WithEventType")
            {
                eventTypes.Add(Convert.ToInt32(args![0]));
            }

            if (method.Name == "WithOrganizationUnitId")
            {
                var unit = Convert.ToInt32(args![0]);
                if (unit == 0)
                {
                    throw new InvalidDataException("organization unit 0");
                }

                units.Add(unit);
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
            var value = inner.IsValueType ? Activator.CreateInstance(inner) : (inner.IsGenericType && inner.GetGenericTypeDefinition() == typeof(List<>) ? Activator.CreateInstance(inner) : null);
            return typeof(Task).GetMethod(nameof(Task.FromResult))!.MakeGenericMethod(inner).Invoke(null, [value])!;
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
