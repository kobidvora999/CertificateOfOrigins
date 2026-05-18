using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Customs.Inf.MMI.Common.CAL;
using Customs.Inf.MMI.Common.Navigation;
using Customs.Inf.MMI.Services.Api.NavigationManager;
using MalamTeam.Infrastructure.GeneralServices.Environment.Enums;

namespace Customs.CRM.CertificateOfOrigins.Client.Api.AuthenticationRequest.AuthenticationRequestFile
{
    /// <summary>
    /// IAuthenticationRequestFilePresenter
    /// </summary>
    [NavigationMapping(EEntityType.AuthenticationRequestFile, NavigationMappingType.New | NavigationMappingType.Edit)]
    public interface IAuthenticationRequestFilePresenter : IPresenter
    {

    }
}
