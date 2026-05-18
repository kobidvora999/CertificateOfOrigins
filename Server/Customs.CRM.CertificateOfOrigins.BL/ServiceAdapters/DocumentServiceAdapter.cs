using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Customs.Infrastructure.DocumentManagement.ExternalCommon;
using Customs.Infrastructure.DocumentManagement.ExternalCommon.Contracts;
using Customs.Infrastructure.DocumentManagement.ExternalCommon.Enums;
using Customs.Infrastructure.DocumentManagement.ExternalProxy;
using Customs.Infrastructure.Entities;
using MalamTeam.Infrastructure.GeneralServices.BaseLayer;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.Base_Entities;
using MalamTeam.Infrastructure.GeneralServices.EAISchema;
using MalamTeam.Infrastructure.GeneralServices.Environment.Enums;

namespace Customs.CRM.CertificateOfOrigins.BL.ServiceAdapters
{
    /// <summary>
    /// DocumentServiceAdapter
    /// </summary>
    public class DocumentServiceAdapter : BaseServiceAdapter<IDocumentsExternalProxy>, IDocumentServiceAdapter
    {
        /// <summary>
        /// UploadDocumentAndSave
        /// </summary>
        /// <param name="document"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public DocumentDTO UploadDocumentAndSave(DocumentDTO document, byte[] content)
        {
            var documentResult = ExternalProxy.UploadDocumentAndSave(document, content);
            return documentResult;
        }

        /// <summary>
        /// SendMessageToAgent
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="eAgentTalkBackType"></param>
        /// <param name="msgString"></param>
        /// <returns></returns>
        public string SendMessageToAgent(VirtualEntity entity, EAgentTalkBackType eAgentTalkBackType, string msgString)
        {
            return ExternalProxy.SendMessageToAgent(new MessageToAgentDTO(entity, eAgentTalkBackType, msgString,false) {IsExport = true});
        }

        /// <summary>
        /// AttachDocumentsToEntity
        /// </summary>
        /// <param name="documentsToEntityDTO"></param>
        public void AttachDocumentsToEntity(List<DocumentsToEntityDTO> documentsToEntityDTO)
        {
            ExternalProxy.AttachDocumentsToEntity(documentsToEntityDTO);
        }

        public void DeleteDocument(List<int> docIds, VirtualEntity ve)
        {
            ExternalProxy.DeleteDocument(docIds, ve);
        }

        public List<Document> GetDocumentsByEntitySync(int entityId, EEntityType eEntityType)
        {
            return ExternalProxy.GetDocumentsByEntitySync(entityId, eEntityType);
        }
    }
}
