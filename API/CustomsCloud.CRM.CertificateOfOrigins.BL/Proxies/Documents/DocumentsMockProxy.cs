using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore.Proxy.Rest;
using System.Diagnostics.CodeAnalysis;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL.Proxies;

[ExcludeFromCodeCoverage]
public class DocumentsMockProxy(IProxyMockUtil mockUtil) : IDocumentsProxy, IMockProxy
{
    // Default = a couple of realistic dummy documents for the entity; feature "Documents.Empty" returns none.
    public Task<List<DocumentDto>?> GetDocumentsByEntity(int entityId, int entityTypeId)
    {
        if (mockUtil.HasMockFeature("Documents.Empty"))
        {
            return Task.FromResult<List<DocumentDto>?>([]);
        }

        var result = new List<DocumentDto>
        {
            new()
            {
                Id = (entityId * 10) + 1,               // TODO: dummy data
                TypeId = 124,                            // TODO: dummy data (a value from CertificateOfOriginsDocumentsFilter)
                IsIncoming = true,
                CreateDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                Title = "Mock document A",              // TODO: dummy data
                IsAccepted = true,
                IsRequired = false,
                ExternalId = "EXT-" + ((entityId * 10) + 1),
                StringDynamicParams = "mock notes A",  // TODO: dummy data
                OtherRelatedEntities = [new EntityDocumentDto { EntityId = entityId, EntityTypeId = entityTypeId }],
            },
            new()
            {
                Id = (entityId * 10) + 2,               // TODO: dummy data
                TypeId = 184,                            // TODO: dummy data
                IsIncoming = false,
                CreateDate = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc),
                Title = "Mock document B",              // TODO: dummy data
                IsAccepted = false,
                IsRequired = true,
                ExternalId = "EXT-" + ((entityId * 10) + 2),
                StringDynamicParams = "mock notes B",  // TODO: dummy data
                OtherRelatedEntities = [],
            },
        };
        return Task.FromResult<List<DocumentDto>?>(result);
    }
}
