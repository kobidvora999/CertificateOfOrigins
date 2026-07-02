using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;
using System.Diagnostics.CodeAnalysis;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL.Proxies;

[ExcludeFromCodeCoverage]
public class DocumentsMockProxy : IDocumentsProxy
{
    // Returns hardcoded dummy data — used while the real Documents service endpoint is unavailable.
    public Task<List<DocumentDto>?> GetDocumentsByEntity(int entityId, int entityTypeId)
    {
        var result = new List<DocumentDto>
        {
            new()
            {
                Id = 1,                       // TODO: dummy data
                TypeId = 1,                   // TODO: dummy data
                TypeName = "Test Document Type",
                Title = "Test Document",
                CreateDate = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
                IsRequired = true,
                IsAccepted = false,
                OtherRelatedEntities = new List<EntityDocumentDto>
                {
                    new() { EntityId = entityId, EntityTypeId = entityTypeId },
                },
            },
        };
        return Task.FromResult<List<DocumentDto>?>(result);
    }
}
