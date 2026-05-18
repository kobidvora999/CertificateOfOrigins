using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Customs.CertificateOfOrigins.Entities;
using Customs.CRP.DealFile.ExternalProxy;
using MalamTeam.Infrastructure.GeneralServices.BaseLayer;

namespace Customs.CRM.CertificateOfOrigins.BL.ServiceAdapters
{
    public class DealFileServiceAdapter : BaseServiceAdapter<IDealFileExternalProxy>, IDealFileServiceAdapter
    {   
        public CertificateOfOrigin CertificateOfOriginVsDeclarationCheck(CertificateOfOrigin certificateOfOrigin)
        {
            //TODO: GetExportDeclarationInfoForPC
            return new CertificateOfOrigin();
        }

        public int? GetExportDeclarationAssessor(string exportDeclarationNum)
        {
            //TODO
            return null;
        }
    }
}
