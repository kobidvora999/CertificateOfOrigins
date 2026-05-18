using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Customs.CRP.External.DealFile.Entities;

namespace Customs.CRM.CertificateOfOrigins.BL.ServiceAdapters
{
    public interface IExportDealFileExternalServiceAdapter
    {
        ExportDeclarationInfoDTO GetExportDeclarationInfoForPC(int leadDocumentID);
        ExportDeclarationDetailsDTO GetExportDeclarationDetailsForCertificateOfOrigion(int? leadDocumentID, string leadDocumentTitle);
        bool ChangeCertificateOfOriginIDForLeadDocument(int leadDocumentId, int oldCertificateOfOriginID, int newCertificateOfOriginID);
        LeadDocumentByCertificateOfOriginDTO GetLeadDocumentByCertificateOfOriginId(int certificateOfOriginId);
        List<DetailsForExportAssociatedGoodsItemsDTO> GetDetailsForExportAssociatedGoodsItemsByLeadDocumentId(int leadDocumentID);
        LeadDocumentByCertificateOfOriginDTO GetLeadDocumentByOldCertificateOfOriginIdAndUpdateToNewCertificateOfOriginId(int OldCertificateOfOriginId, int NewCertificateOfOriginId);
    }
}
