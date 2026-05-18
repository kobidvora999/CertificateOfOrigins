using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Customs.Infrastructure.DocumentManagement.ExternalCommon;
using Customs.Infrastructure.DocumentManagement.ExternalCommon.Enums;
using Customs.Infrastructure.Entities;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.Base_Entities;
using MalamTeam.Infrastructure.GeneralServices.EAISchema;
using MalamTeam.Infrastructure.GeneralServices.Environment.Enums;

namespace Customs.CRM.CertificateOfOrigins.BL.ServiceAdapters
{
    /// <summary>
    /// IDocumentServiceAdapter
    /// </summary>
    public interface IDocumentServiceAdapter
    {
        /// <summary>
        /// UploadDocumentAndSave
        /// </summary>
        /// <param name="document"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        DocumentDTO UploadDocumentAndSave(DocumentDTO document, Byte[] content);

        /// <summary>
        /// SendMessageToAgent
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="eAgentTalkBackType"></param>
        /// <param name="msgString"></param>
        /// <returns></returns>
        string SendMessageToAgent(VirtualEntity entity, EAgentTalkBackType eAgentTalkBackType, string msgString);

        /// <summary>
        /// Attaches Document To Entity
        /// </summary>
        /// <param name="documentsToEntityDTO">documentsToEntityDTO</param>
        void AttachDocumentsToEntity(List<DocumentsToEntityDTO> documentsToEntityDTO);

        void DeleteDocument(List<int> docIds, VirtualEntity ve);

        List<Document> GetDocumentsByEntitySync(int entityId, EEntityType eEntityType);
    }
}
