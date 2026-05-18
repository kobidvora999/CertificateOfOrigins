//------------------------------------------------------------------------------
// <inf-auto-generated>
//     *version: 1.0
//     This code was generated from a template.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </inf-auto-generated>
//------------------------------------------------------------------------------

using System.ServiceModel;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.Communication;
namespace Customs.CRM.CertificateOfOrigins.InternalCommon
{
	/// <summary>
	/// Callback contract for ICertificateOfOriginsIncomingMessageContract
	/// </summary>
	[XmlSerializerFormat]
	[ServiceContract]
	public interface ICertificateOfOriginsIncomingMessageContractCallback : IServiceCallbackContract
	{

		/// <summary>
		/// GetPC_MSG2280_2281_CertificateOfOriginRequest auto generated wrapper
		/// </summary>
		[OperationContract(IsOneWay = true)]
		void OnGetPC_MSG2280_2281_CertificateOfOriginRequestComplete(Customs.Inf.CertificateOfOrigins.EAISchema.PC_NG_2281_MSG02_CertificateOfOriginRequestFeedbackResponse result);

		/// <summary>
		/// GetCertificateRequestByGuid auto generated wrapper
		/// </summary>
		[OperationContract(IsOneWay = true)]
		void OnGetCertificateRequestByGuidComplete(Customs.Inf.SystemTables.EAISchema.WebInternalServiceOutResponse result);
	}

	/// <summary>
	/// auto generated
	/// </summary>
	[XmlSerializerFormat]
	[ServiceContract]
	public interface ICertificateOfOriginsIncomingMessageContractSync : ISyncServiceContract
	{
		/// <summary>
		/// GetPC_MSG2280_2281_CertificateOfOriginRequest auto generated wrapper
		/// </summary>
		[FaultContract(typeof(InfExceptionData))]
		[OperationContract]
		Customs.Inf.CertificateOfOrigins.EAISchema.PC_NG_2281_MSG02_CertificateOfOriginRequestFeedbackResponse GetPC_MSG2280_2281_CertificateOfOriginRequestSync(Customs.Inf.CertificateOfOrigins.EAISchema.PC_NG_2280_MSG01_CertificateOfOriginRequestRequest request);
		/// <summary>
		/// GetCertificateRequestByGuid auto generated wrapper
		/// </summary>
		[FaultContract(typeof(InfExceptionData))]
		[OperationContract]
		Customs.Inf.SystemTables.EAISchema.WebInternalServiceOutResponse GetCertificateRequestByGuidSync(Customs.Inf.SystemTables.EAISchema.WebInternalServiceInRequest request);
	}
}
