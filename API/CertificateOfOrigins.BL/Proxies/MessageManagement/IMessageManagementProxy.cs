using CertificateOfOrigins.Model.ModelDTOs;

namespace CertificateOfOrigins.BL.Proxies;

public interface IMessageManagementProxy
{
    // Legacy: IMessageManagementExternalProxy.SendMessage(SendMessageDTO) (via SendMessagesServiceAdapter). Sends a
    // message to the Message-Management microservice (part of the Common service).
    Task SendMessage(SendMessageDto message);
}
