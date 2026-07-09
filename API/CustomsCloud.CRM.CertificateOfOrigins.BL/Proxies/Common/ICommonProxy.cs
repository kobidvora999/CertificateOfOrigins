using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL.Proxies;

public interface ICommonProxy
{
    /// <summary>
    /// Generates a QR-code image (byte array) for the given URL.
    /// </summary>
    Task<byte[]?> CreateQRCode(string url);

    /// <summary>
    /// Sends a talkback message to the customs agent for an entity. Returns the service result string.
    /// </summary>
    Task<string?> SendMessageToAgent(MessageToAgentDto message);
}
