using System.Reflection;
using CertificateOfOrigins.BL;
using CertificateOfOrigins.BL.Proxies;
using CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore.Interfaces.Http;
using CustomsCloud.InfrastructureCore.Lookup;
using CustomsCloud.InfrastructureCore.Parameters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace CertificateOfOrigins.Test;

// The South-Korea letter's signature block follows its sibling letters' legacy (usp_Template_INNER_CROSS_CertificateOfOrigin,
// 2251-2259): the signer is the user producing the letter (@CreateUserID), and the block comes from UserTemplateDetails
// INNER JOIN User. So a user with no template details gets the whole block empty, the email included.
[TestFixture]
public class TemplateSignatureBlockTests
{
    private const int CurrentUserId = 7;

    [Test]
    public async Task TheBlockIsTheCurrentUsersTemplateDetails()
    {
        var askedFor = new List<int>();
        var letter = new SouthKoreaOriginVerificationLetterResult();

        await Enrich(letter, askedFor, new UserTemplateDetailsDto
        {
            UserId = CurrentUserId,
            EnglishFirstName = "Dana",
            EnglishLastName = "Levi",
            SignatureImageUrl = "signatures/7.png",
            Email = "dana@example.invalid",
        });

        Assert.Multiple(() =>
        {
            Assert.That(askedFor, Is.EqualTo(new[] { CurrentUserId }));
            Assert.That(letter.EmployeeFullName, Is.EqualTo("Dana Levi"));
            Assert.That(letter.EmployeeEmailAddress, Is.EqualTo("dana@example.invalid"));
            Assert.That(letter.UserSignature, Is.EqualTo("signatures/7.png"));
        });
    }

    [Test]
    public async Task AMissingNamePartLeavesOnlyTheSeparatorLikeTsqlConcat()
    {
        var letter = new SouthKoreaOriginVerificationLetterResult();

        await Enrich(letter, [], new UserTemplateDetailsDto { UserId = CurrentUserId, EnglishFirstName = "Dana" });

        Assert.That(letter.EmployeeFullName, Is.EqualTo("Dana "));
    }

    [Test]
    public async Task AUserWithoutTemplateDetailsGetsAnEmptyBlock()
    {
        var letter = new SouthKoreaOriginVerificationLetterResult();

        await Enrich(letter, [], null);

        Assert.Multiple(() =>
        {
            Assert.That(letter.EmployeeFullName, Is.Null);
            Assert.That(letter.EmployeeEmailAddress, Is.Null);
            Assert.That(letter.UserSignature, Is.Null);
        });
    }

    private static async Task Enrich(SouthKoreaOriginVerificationLetterResult letter, List<int> askedFor, UserTemplateDetailsDto? details)
    {
        var userProxy = Fake<IUserProxy>((method, args) =>
        {
            if (method.Name != nameof(IUserProxy.GetUserTemplateDetails))
            {
                return null;
            }

            askedFor.Add((int)args![0]!);
            return Task.FromResult(details);
        });

        var services = new ServiceCollection();
        services.AddSingleton<ILogger<CertificateOfOriginsBl>>(NullLogger<CertificateOfOriginsBl>.Instance);
        services.AddSingleton(userProxy);
        services.AddSingleton(Fake<IRequestMetadata>((method, _) => method.Name == "get_UserId" ? (int?)CurrentUserId : null));
        var bl = new CertificateOfOriginsBl(services.BuildServiceProvider(), Fake<ILookupUtil>(), Fake<IParametersUtil>());

        var enrich = typeof(CertificateOfOriginsBl).GetMethod("EnrichTemplateData", BindingFlags.NonPublic | BindingFlags.Instance)!;
        // GetTemplateData resolves the signer (explicit createUserId, else the request user) and hands it down.
        await (Task)enrich.Invoke(bl, [letter, CurrentUserId])!;
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
