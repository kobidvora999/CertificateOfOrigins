using System.Reflection;
using CertificateOfOrigins.BL;
using CertificateOfOrigins.Model.CertificateOfOriginsDb;
using CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore.Utils.Events;

namespace CertificateOfOrigins.Test;

// A-02 regression: legacy RaiseEventUtil raised the declaration-match event per request reason (CR 156814 — the reasons
// that need no assessor task raise CertificateMatchDeclarationWithoutTask) and set the Export organization-unit type on
// both the match and the mismatch events. The migration had collapsed them into a plain status → event table.
[TestFixture]
public class DeclarationStatusEventsTests
{
    [TestCase((int)ERequestReason.EmptyCertificate)]
    [TestCase((int)ERequestReason.Draft)]
    [TestCase((int)ERequestReason.GetRequestStatus)]
    [TestCase((int)ERequestReason.CertificateCancellation)]
    public async Task DeclarationMatchForNoTaskReasonRaisesWithoutTaskEvent(int reason)
    {
        var raised = await RaiseStatusEvents((int)ECertificateOfOriginStatus.DeclarationMatch, reason);

        Assert.Multiple(() =>
        {
            Assert.That(raised.EventTypes, Is.EqualTo(new[] { (int)EEventType.CertificateMatchDeclarationWithoutTask }));
            Assert.That(raised.OrganizationUnitTypes, Is.EqualTo(new[] { CertificateOfOriginsConsts.ExportOrganizationUnitType }));
        });
    }

    [Test]
    public async Task DeclarationMatchForOtherReasonRaisesTheTaskEvent()
    {
        var raised = await RaiseStatusEvents((int)ECertificateOfOriginStatus.DeclarationMatch, (int)ERequestReason.NewCertificate);

        Assert.Multiple(() =>
        {
            Assert.That(raised.EventTypes, Is.EqualTo(new[] { (int)EEventType.CertificateOfOriginCertificateMatchDeclaration }));
            Assert.That(raised.OrganizationUnitTypes, Is.EqualTo(new[] { CertificateOfOriginsConsts.ExportOrganizationUnitType }));
        });
    }

    [Test]
    public async Task DeclarationMismatchCarriesTheExportOrganizationUnitType()
    {
        var raised = await RaiseStatusEvents((int)ECertificateOfOriginStatus.DeclarationMismatch, (int)ERequestReason.NewCertificate);

        Assert.Multiple(() =>
        {
            Assert.That(raised.EventTypes, Is.EqualTo(new[] { (int)EEventType.CertificateOfOriginCertificateDeclarationMismatch }));
            Assert.That(raised.OrganizationUnitTypes, Is.EqualTo(new[] { CertificateOfOriginsConsts.ExportOrganizationUnitType }));
        });
    }

    [Test]
    public async Task OtherStatusEventsLeaveTheOrganizationUnitTypeUnset()
    {
        var raised = await RaiseStatusEvents((int)ECertificateOfOriginStatus.PendingRelease, (int)ERequestReason.NewCertificate);

        Assert.Multiple(() =>
        {
            Assert.That(raised.EventTypes, Is.EqualTo(new[] { (int)EEventType.CertificateOfOriginUserApprovedCertificate }));
            Assert.That(raised.OrganizationUnitTypes, Is.Empty);
        });
    }

    // RaiseStatusEvents is the private static status-event step of SaveCertificateOfOrigin; it is invoked directly so the
    // assertion is on the event alone, not on the whole save.
    private static async Task<Raised> RaiseStatusEvents(int statusId, int reason)
    {
        var raised = new Raised();
        var builderType = typeof(IEventUtil).GetMethod(nameof(IEventUtil.CreatBuilder))!.ReturnType;
        var eventUtil = Fake<IEventUtil>((method, _) => method.Name switch
        {
            nameof(IEventUtil.CreatBuilder) => FakeBuilder(builderType, raised),
            nameof(IEventUtil.RaiseEvent) => new ValueTask(),
            _ => null,
        });

        await (Task)typeof(CertificateOfOriginsBl)
            .GetMethod("RaiseStatusEvents", BindingFlags.NonPublic | BindingFlags.Static)!
            .Invoke(null, [new CertificateOfOrigin { Id = 5, OrganizationUnitId = 1, CertificateOfOriginStatusId = statusId, RequestReasonCode = reason }, eventUtil])!;
        return raised;
    }

    private sealed class Raised
    {
        public List<int> EventTypes { get; } = [];
        public List<int> OrganizationUnitTypes { get; } = [];
    }

    // --- Minimal dependency-free interface faking over System.Reflection.DispatchProxy (no mocking package). ---

    private static T Fake<T>(Func<MethodInfo, object?[]?, object?> handler) where T : class
    {
        var proxy = DispatchProxy.Create<T, InterfaceFake>();
        ((InterfaceFake)(object)proxy).Handler = (method, args) => handler(method, args) ?? DefaultReturn(method);
        return proxy;
    }

    // A chaining proxy for the event builder (its type is known only at runtime); records the event type and the
    // organization-unit type the BL sets on it.
    private static object FakeBuilder(Type builderType, Raised raised)
    {
        var proxy = typeof(DispatchProxy).GetMethods()
            .First(m => m.Name == nameof(DispatchProxy.Create) && m.GetGenericArguments().Length == 2 && m.GetParameters().Length == 0)
            .MakeGenericMethod(builderType, typeof(InterfaceFake))
            .Invoke(null, null)!;
        ((InterfaceFake)proxy).Handler = (method, args) =>
        {
            if (method.Name == "WithEventType")
            {
                raised.EventTypes.Add((int)args![0]!);
            }

            if (method.Name == "WithOrganizationUnitTypeId")
            {
                raised.OrganizationUnitTypes.Add(Convert.ToInt32(args![0]));
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
