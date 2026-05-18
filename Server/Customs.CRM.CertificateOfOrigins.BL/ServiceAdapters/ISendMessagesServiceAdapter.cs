using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Customs.Inf.CommonService.ExternalCommon;

namespace Customs.CRM.CertificateOfOrigins.BL.ServiceAdapters
{
    public interface ISendMessagesServiceAdapter
    {

        /// <summary>
        /// Sends the message.
        /// </summary>
        /// <param name="sendMessageDTO">The send message DTO.</param>
        void SendMessage(SendMessageDTO sendMessageDTO);
    }
}
