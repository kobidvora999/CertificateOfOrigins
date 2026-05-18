using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Customs.CRM.Entities;
using Customs.CertificateOfOrigins.Entities;
using Customs.Inf.Delivery.ExternalProxy;
using MalamTeam.Infrastructure.GeneralServices.BaseLayer;
using MalamTeam.Infrastructure.GeneralServices.Environment.Enums;

namespace Customs.CRM.CertificateOfOrigins.BL.ServiceAdapters
{
    public interface IDeliveryAdapter 
    {
        /// <summary>
        /// Sends the delivery sync.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="template">The template.</param>
        /// <returns></returns>
        bool SendDeliverySync(CertificateOfOriginsImportAuthenticationFileDetails request, ETemplate template);

        /// <summary>
        /// Sends the delivery one way.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="template">The template.</param>
        /// <returns></returns>
        string SendDeliveryOneWay(CertificateOfOriginsImportAuthenticationFileDetails request, ETemplate template);
    }
}
