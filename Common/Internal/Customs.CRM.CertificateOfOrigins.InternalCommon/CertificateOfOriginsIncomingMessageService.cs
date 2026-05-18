//------------------------------------------------------------------------------
// <inf-auto-generated>
//     *version: 1.0
//     This code was generated from a template.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </inf-auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.ServiceModel;
using MalamTeam.Infrastructure.GeneralServices.EAISchema;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.Exceptions;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.Extensions;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.AOP;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.Communication;
using Customs.CRM.CertificateOfOrigins.InternalCommon;
using Customs.CRM.CertificateOfOrigins.InternalProxy;
using MalamTeam.Infrastructure.GeneralServices.BaseLayer;

namespace Customs.CRM.CertificateOfOrigins.Service
{
	/// <summary>
	/// Generated service layer
	/// </summary>
	public partial class CertificateOfOriginsIncomingMessageService : BaseMessageService<ICertificateOfOriginsIncomingMessageContractCallback>, Customs.CRM.CertificateOfOrigins.InternalCommon.Contract.ICertificateOfOriginsIncomingMessageContract, IInternalCertificateOfOriginsIncomingMessageService, ICertificateOfOriginsIncomingMessageContractSync
	{
		#region Service Instance Members

		private static readonly ESubSystem SubSystem = ESubSystem.CRM;

		/// <summary>
		/// auto generated
		/// </summary>
		public void IsAlive()
		{
			base.IsAlive(SubSystem);
		}

		/// <summary>
		/// auto generated
		/// </summary>
		public bool IsAliveSync()
		{
			return base.IsAliveSync(SubSystem);
		}

		/// <summary>
		/// auto generated
		/// </summary>
		public void WarmUp()
		{
			base.WarmUp(SubSystem);
		}

		/// <summary>
		/// auto generated
		/// </summary>
		public void WarmUpSync()
		{
			base.WarmUpSync(SubSystem);
		}

		private static readonly string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;

		#endregion Customs.CRM.CertificateOfOrigins.InternalCommon.Contract.ICertificateOfOriginsIncomingMessageContract Instance Members

		/// <summary>
		/// GetPC_MSG2280_2281_CertificateOfOriginRequest auto generated wrapper
		/// </summary>
		public void GetPC_MSG2280_2281_CertificateOfOriginRequest(Customs.Inf.CertificateOfOrigins.EAISchema.PC_NG_2280_MSG01_CertificateOfOriginRequestRequest request)
		{
			const string methodName = "Customs.CRM.CertificateOfOrigins.Service.CertificateOfOriginsIncomingMessageService.GetPC_MSG2280_2281_CertificateOfOriginRequest";
			var response = new Customs.Inf.CertificateOfOrigins.EAISchema.PC_NG_2281_MSG02_CertificateOfOriginRequestFeedbackResponse();
			try
			{
				HandleMessageInitialize(response.Content.ResponseContentHeader);
				response = InternalGetPC_MSG2280_2281_CertificateOfOriginRequest(request);
			}
			catch (System.Exception ex)
			{
				HandleMessageException(response, response.Content.ResponseContentHeader, ex, methodName, assemblyName);
			}
			finally
			{
				HandleMessageFinally(request, response);
			}

			try
			{
				try { base.GetServiceCallback(methodName).OnGetPC_MSG2280_2281_CertificateOfOriginRequestComplete(response); }
				catch (System.Exception ex)
				{
					try
					{
						base.GetServiceCallbackMSMQ(methodName).OnGetPC_MSG2280_2281_CertificateOfOriginRequestComplete(response);
					}
					catch (System.Exception exMsmqResponse)
					{
						throw new ApplicationException("Calling MSMQ Response Failed: ", exMsmqResponse);
					}
				}
			}
			catch (System.Exception ex)
			{
				HandleException(ex, methodName, assemblyName, new object[] { request });
			}
		}

		/// <summary>
		/// GetPC_MSG2280_2281_CertificateOfOriginRequestSync auto generated wrapper
		/// </summary>
		public Customs.Inf.CertificateOfOrigins.EAISchema.PC_NG_2281_MSG02_CertificateOfOriginRequestFeedbackResponse GetPC_MSG2280_2281_CertificateOfOriginRequestSync(Customs.Inf.CertificateOfOrigins.EAISchema.PC_NG_2280_MSG01_CertificateOfOriginRequestRequest request)
		{
			var response = new Customs.Inf.CertificateOfOrigins.EAISchema.PC_NG_2281_MSG02_CertificateOfOriginRequestFeedbackResponse();
			try
			{
				HandleMessageInitialize(response.Content.ResponseContentHeader);
				response = InternalGetPC_MSG2280_2281_CertificateOfOriginRequest(request);
			}
			catch (System.Exception ex)
			{
				const string methodName = "Customs.CRM.CertificateOfOrigins.Service.GetPC_MSG2280_2281_CertificateOfOriginRequest";
				HandleMessageException(response, response.Content.ResponseContentHeader, ex, methodName, assemblyName);
			}
			finally
			{
				HandleMessageFinally(request, response);
			}

			return response;
		}

		/// <summary>
		/// GetCertificateRequestByGuid auto generated wrapper
		/// </summary>
		public void GetCertificateRequestByGuid(Customs.Inf.SystemTables.EAISchema.WebInternalServiceInRequest request)
		{
			const string methodName = "Customs.CRM.CertificateOfOrigins.Service.CertificateOfOriginsIncomingMessageService.GetCertificateRequestByGuid";
			var response = new Customs.Inf.SystemTables.EAISchema.WebInternalServiceOutResponse();
			try
			{
				HandleMessageInitialize(response.Content.ResponseContentHeader);
				response = InternalGetCertificateRequestByGuid(request);
			}
			catch (System.Exception ex)
			{
				HandleMessageException(response, response.Content.ResponseContentHeader, ex, methodName, assemblyName);
			}
			finally
			{
				HandleMessageFinally(request, response);
			}

			try
			{
				try { base.GetServiceCallback(methodName).OnGetCertificateRequestByGuidComplete(response); }
				catch (System.Exception ex)
				{
					try
					{
						base.GetServiceCallbackMSMQ(methodName).OnGetCertificateRequestByGuidComplete(response);
					}
					catch (System.Exception exMsmqResponse)
					{
						throw new ApplicationException("Calling MSMQ Response Failed: ", exMsmqResponse);
					}
				}
			}
			catch (System.Exception ex)
			{
				HandleException(ex, methodName, assemblyName, new object[] { request });
			}
		}

		/// <summary>
		/// GetCertificateRequestByGuidSync auto generated wrapper
		/// </summary>
		public Customs.Inf.SystemTables.EAISchema.WebInternalServiceOutResponse GetCertificateRequestByGuidSync(Customs.Inf.SystemTables.EAISchema.WebInternalServiceInRequest request)
		{
			var response = new Customs.Inf.SystemTables.EAISchema.WebInternalServiceOutResponse();
			try
			{
				HandleMessageInitialize(response.Content.ResponseContentHeader);
				response = InternalGetCertificateRequestByGuid(request);
			}
			catch (System.Exception ex)
			{
				const string methodName = "Customs.CRM.CertificateOfOrigins.Service.GetCertificateRequestByGuid";
				HandleMessageException(response, response.Content.ResponseContentHeader, ex, methodName, assemblyName);
			}
			finally
			{
				HandleMessageFinally(request, response);
			}

			return response;
		}

		 // ----------------------------------------------------------------------------------------------------------------------------------------------------

		#region Messages Mock Functions

		/// <summary>
		/// Handles the message mock just for some server logic.
		/// </summary>
		/// <param name="request">The request.</param>
		private static Customs.Inf.CertificateOfOrigins.EAISchema.PC_NG_2281_MSG02_CertificateOfOriginRequestFeedbackResponse HandleMessageMock_GetPC_MSG2280_2281_CertificateOfOriginRequest(Customs.Inf.CertificateOfOrigins.EAISchema.PC_NG_2280_MSG01_CertificateOfOriginRequestRequest request)
		{
			var response = new Customs.Inf.CertificateOfOrigins.EAISchema.PC_NG_2281_MSG02_CertificateOfOriginRequestFeedbackResponse();
			if (request.MessageExternalId.IsNullOrEmpty())
			{
				throw new ApplicationException("MessageExternalId must not be null");
			}
			response.Content.ResponseContentHeader = new ResponseContentHeader();
			response.Content.ResponseContentHeader.ApplicationID = System.Environment.TickCount;

			if (request.MessageExternalId.Length == 1)
			{
				throw new InfException(MalamTeam.Infrastructure.GeneralServices.Environment.Enums.EMessages.ValueNull);
			}
			if (request.MessageExternalId.Length == 2)
			{
				var lstEx = new List<InfException>();
				lstEx.Add(new InfException(MalamTeam.Infrastructure.GeneralServices.Environment.Enums.EMessages.ValueCannotBeMoreThan));
				lstEx.Add(new InfException(MalamTeam.Infrastructure.GeneralServices.Environment.Enums.EMessages.ValueMustBeMoreThan));
				var infExceptionWithInnerinf = new InfException(MalamTeam.Infrastructure.GeneralServices.Environment.Enums.EMessages.ValueNull, null, lstEx);
				throw infExceptionWithInnerinf;
			}
			response.Content.ResponseContentHeader.TransmitionDateTime = DateTime.Now;
			return response;
		}


		/// <summary>
		/// Handles the message mock just for some server logic.
		/// </summary>
		/// <param name="request">The request.</param>
		private static Customs.Inf.SystemTables.EAISchema.WebInternalServiceOutResponse HandleMessageMock_GetCertificateRequestByGuid(Customs.Inf.SystemTables.EAISchema.WebInternalServiceInRequest request)
		{
			var response = new Customs.Inf.SystemTables.EAISchema.WebInternalServiceOutResponse();
			if (request.MessageExternalId.IsNullOrEmpty())
			{
				throw new ApplicationException("MessageExternalId must not be null");
			}
			response.Content.ResponseContentHeader = new ResponseContentHeader();
			response.Content.ResponseContentHeader.ApplicationID = System.Environment.TickCount;

			if (request.MessageExternalId.Length == 1)
			{
				throw new InfException(MalamTeam.Infrastructure.GeneralServices.Environment.Enums.EMessages.ValueNull);
			}
			if (request.MessageExternalId.Length == 2)
			{
				var lstEx = new List<InfException>();
				lstEx.Add(new InfException(MalamTeam.Infrastructure.GeneralServices.Environment.Enums.EMessages.ValueCannotBeMoreThan));
				lstEx.Add(new InfException(MalamTeam.Infrastructure.GeneralServices.Environment.Enums.EMessages.ValueMustBeMoreThan));
				var infExceptionWithInnerinf = new InfException(MalamTeam.Infrastructure.GeneralServices.Environment.Enums.EMessages.ValueNull, null, lstEx);
				throw infExceptionWithInnerinf;
			}
			response.Content.ResponseContentHeader.TransmitionDateTime = DateTime.Now;
			return response;
		}

		#endregion
	}

	interface IInternalCertificateOfOriginsIncomingMessageService
	{
		/// <summary>
		/// GetPC_MSG2280_2281_CertificateOfOriginRequest auto generated wrapper
		/// </summary>
		Customs.Inf.CertificateOfOrigins.EAISchema.PC_NG_2281_MSG02_CertificateOfOriginRequestFeedbackResponse InternalGetPC_MSG2280_2281_CertificateOfOriginRequest(Customs.Inf.CertificateOfOrigins.EAISchema.PC_NG_2280_MSG01_CertificateOfOriginRequestRequest request);
		/// <summary>
		/// GetCertificateRequestByGuid auto generated wrapper
		/// </summary>
		Customs.Inf.SystemTables.EAISchema.WebInternalServiceOutResponse InternalGetCertificateRequestByGuid(Customs.Inf.SystemTables.EAISchema.WebInternalServiceInRequest request);
	}
}
