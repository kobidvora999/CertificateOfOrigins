using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Customs.CRM.Entities;
using Customs.CertificateOfOrigins.Entities;
using Customs.Inf.Delivery.ExternalCommon;
using Customs.Inf.Delivery.ExternalProxy;
using MalamTeam.Infrastructure.GeneralServices.BaseLayer;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.Base_Entities;
using MalamTeam.Infrastructure.GeneralServices.Environment.Enums;

namespace Customs.CRM.CertificateOfOrigins.BL.ServiceAdapters
{
    public class DeliveryAdapter : BaseServiceAdapter<IDeliveryExternalProxy>, IDeliveryAdapter
    {
        public bool SendDeliverySync(CertificateOfOriginsImportAuthenticationFileDetails request, ETemplate template)
        {
            var args = new SendDeliveryArgs
            {
                TemplateTypeId = (int)template,
                RelatedEntity = new VirtualEntity(request),
                FreeText = string.Empty
            };

            var deliveryDestination = new DeliveryDestination
            {
                CustomerAddressPurposeId = (int)EAddressPurpose.General,
                CustomerId = 140845
            };

            args.DeliveryDestinations.Add(deliveryDestination);
            return ExternalProxy.SendDeliverySync(args);
        }

        public string SendDeliveryOneWay(CertificateOfOriginsImportAuthenticationFileDetails request, ETemplate template)
        {
            var args = new SendDeliveryArgs
            {
                TemplateTypeId = (int)template,
                RelatedEntity = new VirtualEntity(request),
                FreeText = string.Empty
            };

            var deliveryDestination = new DeliveryDestination
            {
                CustomerAddressPurposeId = (int)EAddressPurpose.Pre_Ruling,
                CustomerId = request.CustomerID
            };

            args.DeliveryDestinations.Add(deliveryDestination);
            return ExternalProxy.SendDeliveryOneWay(args);
        }
    }
}
