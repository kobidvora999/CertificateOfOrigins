using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Customs.Inf.CommonService.ExternalCommon;
using Customs.Inf.CommonService.ExternalProxy;
using MalamTeam.Infrastructure.GeneralServices.BaseLayer;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.AOP;
using Microsoft.Practices.Unity;

namespace Customs.CRM.CertificateOfOrigins.BL.ServiceAdapters
{
    public class SendMessagesServiceAdapter : BaseServiceAdapter<IMessageManagementExternalProxy>, ISendMessagesServiceAdapter
    {
        /// <summary>
        /// Sends the message.
        /// </summary>
        /// <param name="sendMessageDTO">The send message DTO.</param>
        public void SendMessage(SendMessageDTO sendMessageDTO)
        {
            //var sendMessageExternalProxy = Container.Resolve<IMessageManagementExternalProxy>();
            //sendMessageExternalProxy.SendMessage(sendMessageDTO);
            ExternalProxy.SendMessage(sendMessageDTO);
        }
    }
}