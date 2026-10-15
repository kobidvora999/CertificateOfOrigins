using CommonServicesExternal;
using Customs.Infrastructure.DocumentManagement.ExternalCommon;
using CustomsCloud.InfrastructureCore.Proxy;
using malam.com.customs;
using System.Collections.Generic;
using VirtualEntity = malam.com.customs.VirtualEntity;

namespace IssueCertificateOfOrigin.Worker.Proxies
{
    public sealed class DocumentsExternalProxy(ICustomsWcfProxy<IDocumentsExternalContract> proxy) : IDocumentsExternalProxy
    {
        public async Task<Document[]?> GetDocumentsByEntitySync(int entityId, EEntityType entityType)
        {
            var response = await proxy.InvokeAsync(c => c.GetDocumentsByEntitySyncAsync(entityId, entityType));
            return response;
        }

        public async System.Threading.Tasks.Task DeleteDocument(int[] docIds, VirtualEntity ve)
        {
            await proxy.InvokeAsync(c => c.DeleteDocumentAsync(docIds, ve));
        }

        public async Task<DocumentDTO?> UploadDocumentAndSave(DocumentDTO document, byte[] content)
        {
            var response = await proxy.InvokeAsync(c => c.UploadDocumentAndSaveAsync(document, content));
            return response;
        }
    }
}
