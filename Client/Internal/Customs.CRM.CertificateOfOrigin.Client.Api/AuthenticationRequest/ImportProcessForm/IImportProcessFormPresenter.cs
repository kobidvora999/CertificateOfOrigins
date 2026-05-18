using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Customs.CertificateOfOrigins.Entities;
using Customs.Inf.MMI.Common.CAL;
using Customs.Inf.MMI.Common.Navigation;
using Customs.Inf.MMI.Services.Api.NavigationManager;
using Customs.Inf.MMI.Services.Api.Search;
using MalamTeam.Infrastructure.GeneralServices.Environment.Enums;

namespace Customs.CRM.CertificateOfOrigins.Client.Api.AuthenticationRequest.ImportProcessForm
{
    /// <summary>
    /// IImportProcessFormPresenter
    /// </summary>
    [NavigationMapping(EEntityType.ImportAuthenticationRequest, NavigationMappingType.None | NavigationMappingType.Edit)]    
    public interface IImportProcessFormPresenter : ISearchable
    {
        void ShowImportProcessPopUp(CertificateOfOriginsImportAuthenticationRequest request);
    }


}
