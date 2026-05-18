using System.ServiceModel;
using Customs.Inf.CertificateOfOrigins.EAISchema;
using Customs.Inf.SystemTables.EAISchema;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.Communication;
using MalamTeam.Infrastructure.GeneralServices.EAISchema;

namespace Customs.CRM.CertificateOfOrigins.InternalCommon.Contract
{

    /// <summary>
    ///  the service contract
    /// </summary>
    [ServiceContract]
    [XmlSerializerFormat]
    [InfContractExtension(ESubSystem.CRM)]
    public interface ICertificateOfOriginsIncomingMessageContract : IServiceContract
    {

        /// <summary>
        /// Gets the P c_ MS G2280_2281_ certificate of origin request.
        /// </summary>
        /// <param name="request">The request.</param>
        [InfOperationExtension(typeof(PC_NG_2281_MSG02_CertificateOfOriginRequestFeedbackResponse))]
        [OperationContract(IsOneWay = true)]
        void GetPC_MSG2280_2281_CertificateOfOriginRequest(PC_NG_2280_MSG01_CertificateOfOriginRequestRequest request);


        [InfOperationExtension(typeof(WebInternalServiceOutResponse))]
        [OperationContract(IsOneWay = true)]
        void GetCertificateRequestByGuid(WebInternalServiceInRequest request);
    }
}
