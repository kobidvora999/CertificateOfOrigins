using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Customs.CertificateOfOrigins.Entities;

namespace Customs.CRM.CertificateOfOrigins.BL.ServiceAdapters
{
    public interface IDealFileServiceAdapter
    {
        CertificateOfOrigin CertificateOfOriginVsDeclarationCheck(CertificateOfOrigin certificateOfOrigin);

        int? GetExportDeclarationAssessor(string exportDeclarationNum);
    }
}
