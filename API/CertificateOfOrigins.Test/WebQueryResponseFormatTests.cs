using System.Globalization;
using System.Reflection;
using System.Text.Json;
using CertificateOfOrigins.BL;
using CertificateOfOrigins.DAL;
using CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore.Lookup;
using CustomsCloud.InfrastructureCore.Parameters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace CertificateOfOrigins.Test;

// Parity findings D F2 and D F3 for GetCertificateRequestByGuid (the portal's web query).
//
// F2: the legacy serializer wrote every DateTime as a date-only string, "yyyy-MM-dd" (JSONSerializeHelper); the migration
//     let the platform serializer emit an ISO date-time, so the portal started to receive a time part it never had.
// F3: legacy parsed the stored DateOfDeclaration under the he-IL host culture (day first). The container culture is month
//     first, so a day above 12 failed to parse (the field vanished) and a day <= 12 flipped day and month.
[TestFixture]
public class WebQueryResponseFormatTests
{
    private static readonly DateTime IssuingDate = new(2026, 3, 1, 14, 22, 5, DateTimeKind.Unspecified);

    [Test]
    public async Task DatesInTheWebResponseAreDateOnlyOnTheWire()
    {
        var response = await Query(new CertificateOfOriginWebQueryDto
        {
            Id = 7,
            TypeId = (int)ECertificateOfOriginType.EUR1,
            CertificateNumber = "COO-1001",
            IssuingDate = IssuingDate,
            CertificateOfOriginInvoiceDetail =
            [
                new CertificateOfOriginInvoiceDetailDto { InvoiceDate = IssuingDate, InvoiceNumber = "INV-1", IsToPrint = true },
            ],
        });

        using var json = JsonDocument.Parse(JsonSerializer.Serialize(response, new JsonSerializerOptions(JsonSerializerDefaults.Web)));
        var root = json.RootElement;
        var issuingDate = root.GetProperty("certificateOfOriginDetails").EnumerateArray()
            .Single(field => field.GetProperty("label").GetString() == CertificateOfOriginsConsts.IssuingDateLabel)
            .GetProperty("value").GetString();
        var invoiceDate = root.GetProperty("certificateOfOriginInvoiceDetails")[0].GetProperty("invoiceDate").GetString();

        Assert.Multiple(() =>
        {
            Assert.That(issuingDate, Is.EqualTo("2026-03-01"));
            Assert.That(invoiceDate, Is.EqualTo("2026-03-01"));
        });
    }

    // 25/09/2025 only parses day-first; 05/09/2025 is 5 September (he-IL), not 9 May. The tests run under en-US (the
    // container is not he-IL), so they do not depend on the culture of the machine that runs them.
    [SetCulture("en-US")]
    [TestCase("25/09/2025", "25 September 2025")]
    [TestCase("05/09/2025", "05 September 2025")]
    public async Task DateOfDeclarationIsParsedDayFirstAsTheLegacyHostDid(string stored, string expected)
    {
        var response = await Query(new CertificateOfOriginWebQueryDto
        {
            Id = 7,
            TypeId = (int)ECertificateOfOriginType.EUR1,
            IssuingDate = IssuingDate,
            CertificateOfOriginDetails =
            [
                new CertificateOfOriginWebDetailDto
                {
                    CertificateDetailsTypeCodeId = (int)ECertificateDetailsType.DateOfDeclaration,
                    Value = stored,
                    CertificateDetailsTypeCode = new CertificateDetailsTypeCodeDto { EnglishName = "Date of declaration" },
                },
            ],
        });

        var field = response.CertificateOfOriginDetails.SingleOrDefault(f => f.Label == "Date of declaration");

        Assert.That(field?.Value, Is.EqualTo(expected));
    }

    // Values the new code writes itself are ISO round-trip strings, which parse the same under any culture.
    [Test]
    [SetCulture("en-US")]
    public async Task DateOfDeclarationWrittenByTheNewCodeIsStillParsed()
    {
        var response = await Query(new CertificateOfOriginWebQueryDto
        {
            Id = 7,
            TypeId = (int)ECertificateOfOriginType.EUR1,
            CertificateOfOriginDetails =
            [
                new CertificateOfOriginWebDetailDto
                {
                    CertificateDetailsTypeCodeId = (int)ECertificateDetailsType.DateOfDeclaration,
                    Value = new DateTime(2025, 9, 25).ToString("o", CultureInfo.InvariantCulture),
                    CertificateDetailsTypeCode = new CertificateDetailsTypeCodeDto { EnglishName = "Date of declaration" },
                },
            ],
        });

        Assert.That(response.CertificateOfOriginDetails.Single(f => f.Label == "Date of declaration").Value, Is.EqualTo("25 September 2025"));
    }

    private static async Task<CertificateOfOriginsResponseDto> Query(CertificateOfOriginWebQueryDto certificate)
    {
        var dataLayer = Fake<ICertificateOfOriginsDal>((method, _) =>
            method.Name == "GetCertificateOfOriginDataForWebQuery" ? Task.FromResult<CertificateOfOriginWebQueryDto?>(certificate) : null);

        // A lookup Search yields an empty set (no currencies / field labels are needed by these scenarios).
        var lookupUtil = Fake<ILookupUtil>((method, _) => method.Name == "Search" ? EmptyTask(method.ReturnType) : null);
        var parametersUtil = Fake<IParametersUtil>((method, _) =>
        {
            if (method.Name != "Get")
            {
                return null;
            }

            var resultType = method.ReturnType.GetGenericArguments()[0];
            return TaskFromResult(resultType, resultType == typeof(string) ? "https://verify.example/{0}" : null);
        });

        var services = new ServiceCollection();
        services.AddSingleton<ILogger<CertificateOfOriginsBl>>(NullLogger<CertificateOfOriginsBl>.Instance);
        services.AddSingleton(dataLayer);
        services.AddSingleton(Fake<CertificateOfOrigins.BL.Proxies.IDocumentsProxy>());
        var bl = new CertificateOfOriginsBl(services.BuildServiceProvider(), lookupUtil, parametersUtil);

        var response = await bl.GetCertificateRequestByGuid(new CertificateOfOriginsRequestDto
        {
            CertificateOfOriginGuid = Guid.NewGuid().ToString(),
        });
        return response;
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

    // Task<IEnumerable<T>> / Task<IReadOnlyList<T>> / Task<List<T>> of nothing.
    private static object EmptyTask(Type taskType)
    {
        var resultType = taskType.GetGenericArguments()[0];
        var itemType = resultType.GetGenericArguments()[0];
        object empty = resultType.IsAssignableFrom(itemType.MakeArrayType())
            ? Array.CreateInstance(itemType, 0)
            : Activator.CreateInstance(typeof(List<>).MakeGenericType(itemType))!;
        return TaskFromResult(resultType, empty);
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
