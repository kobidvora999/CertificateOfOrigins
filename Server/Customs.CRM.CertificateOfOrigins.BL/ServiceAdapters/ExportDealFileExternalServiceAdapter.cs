using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Customs.CRP.DealFile.ExternalProxy;
using Customs.CRP.ExportDealFile.ExternalProxy;
using Customs.CRP.DealFile.Declaration.ExternalCommon;
using Customs.CRP.External.DealFile.Entities;
using MalamTeam.Infrastructure.GeneralServices.BaseLayer;

namespace Customs.CRM.CertificateOfOrigins.BL.ServiceAdapters
{
    public class ExportDealFileExternalServiceAdapter : BaseServiceAdapter<IExportDealFileExternalProxy>, IExportDealFileExternalServiceAdapter
    {
        public ExportDeclarationInfoDTO GetExportDeclarationInfoForPC(int leadDocumentID)
        {
            return ExternalProxy.GetExportDeclarationInfoForPC(leadDocumentID);
        }

        public ExportDeclarationDetailsDTO GetExportDeclarationDetailsForCertificateOfOrigion(int? leadDocumentID, string leadDocumentTitle)
        {
           return ExternalProxy.GetExportDeclarationDetailsForCertificateOfOrigion(leadDocumentID, leadDocumentTitle);
        }

        public bool ChangeCertificateOfOriginIDForLeadDocument(int leadDocumentId, int oldCertificateOfOriginID, int newCertificateOfOriginID)
        {
            return ExternalProxy.ChangeCertificateOfOriginIDForLeadDocument(leadDocumentId, oldCertificateOfOriginID, newCertificateOfOriginID );
        }

        public LeadDocumentByCertificateOfOriginDTO GetLeadDocumentByCertificateOfOriginId(int certificateOfOriginId)
        {
            return ExternalProxy.GetLeadDocumentByCertificateOfOriginId(certificateOfOriginId);
        }

        public List<DetailsForExportAssociatedGoodsItemsDTO> GetDetailsForExportAssociatedGoodsItemsByLeadDocumentId(int leadDocumentID)
        {
            return ExternalProxy.GetDetailsForExportAssociatedGoodsItemsByLeadDocumentId(leadDocumentID);
        }

        public LeadDocumentByCertificateOfOriginDTO GetLeadDocumentByOldCertificateOfOriginIdAndUpdateToNewCertificateOfOriginId(int OldCertificateOfOriginId, int NewCertificateOfOriginId)
        {
            return ExternalProxy.GetLeadDocumentByOldCertificateOfOriginIdAndUpdateToNewCertificateOfOriginId(OldCertificateOfOriginId, NewCertificateOfOriginId);
        }
    }
}
