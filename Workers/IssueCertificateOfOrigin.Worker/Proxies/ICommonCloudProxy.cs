using IssueCertificateOfOrigin.Worker.Dto;
using System;
using System.Collections.Generic;
using System.Text;

namespace IssueCertificateOfOrigin.Worker.Proxies
{
    public interface ICommonCloudProxy
    {
        Task SendOutgoingMessage(OutgoingMessageRequest request);
    }
}
