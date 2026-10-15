using System.Reflection;
using CertificateOfOrigins.BL;
using CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore.Utils.Events;

namespace CertificateOfOrigins.Test;

// Parity finding F-07: the legacy raised the AuthenticationRequestRejected event of an AuthenticationNeedless decision with the
// request ENTITY itself, so the event carried the entity's organization unit and customer. The migration sent only the id and the
// title. The unit is sent when the caller supplies one (the builder takes only a real unit); the customer only when there is one.
[TestFixture]
public class AuthenticationNeedlessRejectionTests
{
    [Test]
    public async Task TheRejectionEventCarriesTheRequestsOrganizationUnitAndCustomer()
    {
        var calls = await Raise(new SaveImportAuthenticationRequestRequestDto { DocumentId = 104, OrganizationUnitId = 42, CustomerId = 777, UserResponseId = 5 });

        Assert.Multiple(() =>
        {
            Assert.That(calls, Does.Contain("WithTitle=104"));
            Assert.That(calls, Does.Contain("WithOrganizationUnitId=42"));
            Assert.That(calls, Does.Contain("WithCustomerId=777"));
        });
    }

    [Test]
    public async Task WithoutAUnitOrACustomerNeitherIsSent()
    {
        var calls = await Raise(new SaveImportAuthenticationRequestRequestDto { DocumentId = 104, OrganizationUnitId = 0, CustomerId = null, UserResponseId = 5 });

        Assert.Multiple(() =>
        {
            Assert.That(calls, Does.Contain("WithTitle=104"));
            Assert.That(calls.Any(c => c.StartsWith("WithOrganizationUnitId")), Is.False);
            Assert.That(calls.Any(c => c.StartsWith("WithCustomerId")), Is.False);
        });
    }

    // RaiseAuthenticationNeedlessRejection is the private static step that builds the event; it is invoked directly so the assertion is
    // on the event alone, not on the whole save.
    private static async Task<List<string>> Raise(SaveImportAuthenticationRequestRequestDto request)
    {
        var calls = new List<string>();
        var builderType = typeof(IEventUtil).GetMethod(nameof(IEventUtil.CreatBuilder))!.ReturnType;
        var eventUtil = Fake<IEventUtil>((method, _) => method.Name switch
        {
            nameof(IEventUtil.CreatBuilder) => FakeBuilder(builderType, calls),
            nameof(IEventUtil.RaiseEvent) => new ValueTask(),
            _ => null,
        });

        await (Task)typeof(AuthenticationRequestBl)
            .GetMethod("RaiseAuthenticationNeedlessRejection", BindingFlags.NonPublic | BindingFlags.Static)!
            .Invoke(null, [eventUtil, request])!;
        return calls;
    }

    // --- Minimal dependency-free interface faking over System.Reflection.DispatchProxy (no mocking package). ---

    private static T Fake<T>(Func<MethodInfo, object?[]?, object?> handler) where T : class
    {
        var proxy = DispatchProxy.Create<T, InterfaceFake>();
        ((InterfaceFake)(object)proxy).Handler = (method, args) => handler(method, args) ?? DefaultReturn(method);
        return proxy;
    }

    private static object FakeBuilder(Type builderType, List<string> calls)
    {
        var proxy = typeof(DispatchProxy).GetMethods()
            .First(m => m.Name == nameof(DispatchProxy.Create) && m.GetGenericArguments().Length == 2 && m.GetParameters().Length == 0)
            .MakeGenericMethod(builderType, typeof(InterfaceFake))
            .Invoke(null, null)!;
        ((InterfaceFake)proxy).Handler = (method, args) =>
        {
            if (method.Name is "WithTitle" or "WithOrganizationUnitId" or "WithCustomerId")
            {
                calls.Add($"{method.Name}={args![0]}");
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
