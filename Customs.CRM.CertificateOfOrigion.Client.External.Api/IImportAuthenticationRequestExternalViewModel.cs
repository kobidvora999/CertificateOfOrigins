using Customs.Inf.MMI.Common.CAL;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.Base_Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Customs.CRM.CertificateOfOrigion.Client.External.Api
{
    public interface IImportAuthenticationRequestExternalViewModel : IViewModel
    {
        IEntity CurrentEntity { get; set; }
        Visibility ProgressBarVisibility { get; set; }
        List<int> LeadDocumentIds { get; set; }

        ICustomDelegateCommand DealFileImportAuthenticationRequestRefreshCommand { get; set; }
        ICustomDelegateCommand GetAuthenticationRequestByLeadDocumentIDCommand { get; set; }
    }
}
