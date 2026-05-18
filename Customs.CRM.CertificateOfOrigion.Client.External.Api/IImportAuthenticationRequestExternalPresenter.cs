using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Customs.CRM.CertificateOfOrigins.ExternalCommon.Common;
using Customs.Inf.MMI.Common.CAL;

namespace Customs.CRM.CertificateOfOrigion.Client.External.Api
{
    public interface IImportAuthenticationRequestExternalPresenter : IPresenter
    {
        
        void GetAuthenticationRequestByLeadDocumentID(List<int> leadDocumentIDs);

        IImportAuthenticationRequestExternalViewModel DealFileAuthenticationRequestViewModelEditViewModel { get; }
    }
}
