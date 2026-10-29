using System.Reflection;
using CertificateOfOrigins.BL;
using CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore.Lookup;
using CustomsCloud.InfrastructureCore.Parameters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace CertificateOfOrigins.Test;

// Legacy resolved message codes with SystemTablesUtil.GetIdByCode, which reads active rows only (State == 1).
// ILookupUtil.Search does not filter on State, so a lookup that replaced such a read passes the state itself: an inactive
// currency or packing code is "not in the system", as it was for legacy.
[TestFixture]
public class LookupActiveRowsTests
{
    private static readonly List<BL.Lookups.CurrencyType> Currencies =
    [
        new() { Id = 1, CurrencyCode = "USD", State = 1 },
        new() { Id = 2, CurrencyCode = "DEM", State = 0 },
    ];

    private static readonly List<BL.Lookups.PackingType> PackingTypes =
    [
        new() { Id = 10, CommonCode = "BX", State = 1 },
        new() { Id = 11, CommonCode = "OLD", State = 0 },
    ];

    [TestCase("USD", 1)]
    [TestCase("DEM", null)]
    public async Task ACurrencyCodeResolvesOnlyWhenItIsActive(string code, int? expected)
    {
        var (result, exceptions) = await Resolve("ResolveCurrencyTypeId", code);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(expected));
            Assert.That(exceptions, Has.Count.EqualTo(expected is null ? 1 : 0), "an inactive code is reported as not existing in the system");
        });
    }

    [TestCase("BX", 10)]
    [TestCase("OLD", null)]
    public async Task APackingCodeResolvesOnlyWhenItIsActive(string code, int? expected)
    {
        var (result, exceptions) = await Resolve("ResolvePackingTypeId", code);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(expected));
            Assert.That(exceptions, Has.Count.EqualTo(expected is null ? 1 : 0));
        });
    }

    private static async Task<(int? Result, List<CertificateOfOriginExceptionDto> Exceptions)> Resolve(string methodName, string code)
    {
        var lookupUtil = Fake<ILookupUtil>((method, args) =>
        {
            if (method.Name != nameof(ILookupUtil.Search))
            {
                return null;
            }

            var type = method.GetGenericArguments()[0];
            if (type == typeof(BL.Lookups.CurrencyType))
            {
                return Task.FromResult(Currencies.Where((Func<BL.Lookups.CurrencyType, bool>)args![0]!));
            }

            return Task.FromResult(PackingTypes.Where((Func<BL.Lookups.PackingType, bool>)args![0]!));
        });

        var services = new ServiceCollection();
        services.AddSingleton<ILogger<CertificateOfOriginsBl>>(NullLogger<CertificateOfOriginsBl>.Instance);
        var bl = new CertificateOfOriginsBl(services.BuildServiceProvider(), lookupUtil, Fake<IParametersUtil>());

        var contextType = typeof(CertificateOfOriginsBl).GetNestedType("MessageValidationContext", BindingFlags.NonPublic)!;
        var context = Activator.CreateInstance(contextType)!;
        var method = typeof(CertificateOfOriginsBl).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance)!;
        var result = await (Task<int?>)method.Invoke(bl, [code, context])!;
        var exceptions = (List<CertificateOfOriginExceptionDto>)contextType.GetProperty("Exceptions")!.GetValue(context)!;
        return (result, exceptions);
    }

    // --- Minimal dependency-free interface faking over System.Reflection.DispatchProxy (no mocking package). ---

    private static T Fake<T>(Func<MethodInfo, object?[]?, object?>? handler = null) where T : class
    {
        var proxy = DispatchProxy.Create<T, InterfaceFake>();
        ((InterfaceFake)(object)proxy).Handler = (method, args) => handler?.Invoke(method, args);
        return proxy;
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
