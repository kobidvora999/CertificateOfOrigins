using CommonServicesExternal;
using Customs.Infrastructure.DocumentManagement.ExternalCommon;
using CustomsCloud.InfrastructureCore.Proxy;
using malam.com.customs;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;
using VirtualEntity = malam.com.customs.VirtualEntity;

namespace IssueCertificateOfOrigin.Worker.Proxies
{
    public interface IDocumentsExternalProxy
    {
        Task<Document[]?> GetDocumentsByEntitySync(int entityId, EEntityType entityType);
        Task DeleteDocument(int[] docIds, VirtualEntity ve);
        Task<DocumentDTO?> UploadDocumentAndSave(DocumentDTO document, byte[] content);
    }
}
