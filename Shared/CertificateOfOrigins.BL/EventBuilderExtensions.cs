using CustomsCloud.InfrastructureCore.Utils.Events;

namespace CertificateOfOrigins.BL;

internal static class EventBuilderExtensions
{
    // Legacy EventUtil.RaiseEvent accepted an entity with OrganizationUnitID 0 and raised the event without a unit. The .NET 10
    // builder throws on WithOrganizationUnitId(0), which turned such a save into a 500 before anything was persisted (parity
    // findings FN-02, GN-03, GN-07). The unit is applied only when there is one, so the event is still raised, as in legacy.
    public static IEventRequestBuilder WithOrganizationUnitIdWhenKnown(this IEventRequestBuilder builder, int? organizationUnitId)
    {
        var result = organizationUnitId is > 0 ? builder.WithOrganizationUnitId(organizationUnitId.Value) : builder;
        return result;
    }
}
