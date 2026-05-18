using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Customs.Inf.MMI.Services.Api.Search;
using MalamTeam.Infrastructure.GeneralServices.Environment.Enums;

namespace Customs.CRM.CertificateOfOrigins.Client.Api.AuthenticationRequest.AuthenticationRequestSearch
{
    /// <summary>
    /// IAuthenticationRequestSearchPresenter
    /// </summary>
    [NavigationSearchMapping(EEntityType.AuthenticationRequestFile, ERegexAndMaskType.RegexNumericUpTo10Digits)]
    [NavigationSearchMapping(EEntityType.ImportAuthenticationRequest, ERegexAndMaskType.RegexNumericUpTo10Digits ,NavigationItemVisibility = Visibility.Collapsed)]

    public interface IAuthenticationRequestSearchPresenter : ISearchable
    {

    }
}
