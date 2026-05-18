using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Customs.CRM.CertificateOfOrigins.ExternalCommon.Common;
using Customs.Inf.MMI.Common.CAL;

namespace Customs.CRM.CertificateOfOrigion.Client.External.Api
{
    /// <summary>
    /// IImportProcessFormExternalPresenter
    /// </summary>
    public interface IImportProcessFormExternalPresenter : IPresenter
    {
        /// <summary>
        /// Called when OnShowAuthenticationRequest
        /// </summary>
        /// <param name="authenticationRequestDTO">The authentication request dto.</param>
        void OnShowAuthenticationRequest(AuthenticationRequestDTO authenticationRequestDTO);
    }
}
