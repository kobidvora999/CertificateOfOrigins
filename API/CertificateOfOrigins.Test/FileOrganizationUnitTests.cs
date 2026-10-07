using System.Reflection;
using CertificateOfOrigins.BL;
using CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore.Lookup;
using CustomsCloud.InfrastructureCore.Parameters;
using CustomsCloud.InfrastructureCore.Utils.Events;
using Microsoft.Extensions.DependencyInjection;

namespace CertificateOfOrigins.Test;

// Parity finding F-05: the legacy file entity's OrganizationUnitID getter returned the FIRST child request's unit whenever the
// file had requests, so the file-level events always carried the child's unit. The migration used the client-sent file value,
// which the by-id read never returns, so a caller that did not derive it sent 0 and the file's tasks opened with no unit.
[TestFixture]
public class FileOrganizationUnitTests
{
    [Test]
    public async Task TheFileEventsCarryTheFirstChildsUnitNotTheClientSentFileValue()
    {
        var units = await OrganizationUnitsOfTheFileEvents(
            fileUnit: 0,
            children: [new SaveAuthenticationRequestFileChildDto { DocumentId = 1, OrganizationUnitId = 42 }, new SaveAuthenticationRequestFileChildDto { DocumentId = 2, OrganizationUnitId = 99 }]);

        Assert.That(units, Is.Not.Empty.And.All.EqualTo(42), "the first child's unit, as the legacy getter returned");
    }

    [Test]
    public async Task AChildWithoutAUnitLeavesTheFilesOwnValue()
    {
        var units = await OrganizationUnitsOfTheFileEvents(
            fileUnit: 7,
            children: [new SaveAuthenticationRequestFileChildDto { DocumentId = 1, OrganizationUnitId = 0 }]);

        Assert.That(units, Is.Not.Empty.And.All.EqualTo(7));
    }

    [Test]
    public async Task AFileWithoutRequestsUsesItsOwnValue()
    {
        var units = await OrganizationUnitsOfTheFileEvents(fileUnit: 7, children: []);

        Assert.That(units, Is.Not.Empty.And.All.EqualTo(7));
    }

    private static IServiceProvider Services()
    {
        var services = new ServiceCollection();
        services.AddSingleton<Microsoft.Extensions.Logging.ILogger<AuthenticationRequestBl>>(Microsoft.Extensions.Logging.Abstractions.NullLogger<AuthenticationRequestBl>.Instance);
        return services.BuildServiceProvider();
    }

    // CheckStatusAndOpenTask is the private step that raises the file-level events (here the handle-request task, for a status that
    // opens one); it is invoked directly so the assertion is on the event's organization unit alone.
    private static async Task<List<int>> OrganizationUnitsOfTheFileEvents(int fileUnit, List<SaveAuthenticationRequestFileChildDto> children)
    {
        var units = new List<int>();
        var builderType = typeof(IEventUtil).GetMethod(nameof(IEventUtil.CreatBuilder))!.ReturnType;
        var eventUtil = Fake<IEventUtil>((method, _) => method.Name switch
        {
            nameof(IEventUtil.CreatBuilder) => FakeBuilder(builderType, units),
            nameof(IEventUtil.RaiseEvent) => new ValueTask(),
            _ => null,
        });

        var bl = new AuthenticationRequestBl(Services(), Fake<IParametersUtil>(), Fake<ILookupUtil>());
        var request = new SaveAuthenticationRequestFileRequestDto
        {
            Id = 11,
            AuthenticationFileStatusId = (int)EAuthenticationFileStatus.RightAuthenticationAnswer,
            OrganizationUnitId = fileUnit,
            Requests = children,
        };

        await (Task)typeof(AuthenticationRequestBl)
            .GetMethod("CheckStatusAndOpenTask", BindingFlags.NonPublic | BindingFlags.Instance)!
            .Invoke(bl, [eventUtil, request, 7])!;
        return units;
    }

    // --- Minimal dependency-free interface faking over System.Reflection.DispatchProxy (no mocking package). ---

    private static T Fake<T>(Func<MethodInfo, object?[]?, object?>? handler = null) where T : class
    {
        var proxy = DispatchProxy.Create<T, InterfaceFake>();
        ((InterfaceFake)(object)proxy).Handler = (method, args) => handler?.Invoke(method, args) ?? DefaultReturn(method);
        return proxy;
    }

    private static object FakeBuilder(Type builderType, List<int> units)
    {
        var proxy = typeof(DispatchProxy).GetMethods()
            .First(m => m.Name == nameof(DispatchProxy.Create) && m.GetGenericArguments().Length == 2 && m.GetParameters().Length == 0)
            .MakeGenericMethod(builderType, typeof(InterfaceFake))
            .Invoke(null, null)!;
        ((InterfaceFake)proxy).Handler = (method, args) =>
        {
            if (method.Name == "WithOrganizationUnitId")
            {
                units.Add(Convert.ToInt32(args![0]));
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
