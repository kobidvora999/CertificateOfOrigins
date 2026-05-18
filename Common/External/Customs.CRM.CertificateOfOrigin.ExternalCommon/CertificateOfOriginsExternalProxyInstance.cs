//------------------------------------------------------------------------------
// <inf-auto-generated>
//     *version: 10.0
//     This code was generated from a template.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </inf-auto-generated>
//------------------------------------------------------------------------------

using System;
using System.ServiceModel;
using System.Transactions;
using MalamTeam.Infrastructure.GeneralServices.BaseLayer;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.Communication;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.Exceptions;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.BaseClasses;

namespace Customs.CRM.CertificateOfOrigins.ExternalProxy
{
	/// <summary>
	/// Generated proxy instance
	/// </summary>
	public partial class CertificateOfOriginsExternalProxyInstance : BaseProxy2<Customs.CRM.CertificateOfOrigins.ExternalCommon.Contract.ICertificateOfOriginsExternalContract>, ICertificateOfOriginsExternalProxy
	{

		#region Constructor & Dispose

		/// <summary>
		/// Initialize the base class. You can specify custom timeout parameter by using existing overload to base class
		/// </summary>
		public CertificateOfOriginsExternalProxyInstance(IMMIFacade currentGuiDispatcher)
		: base("CertificateOfOriginsExternalEndpoint", "CRM", "CertificateOfOriginsExternalService", currentGuiDispatcher)
		{

		}

		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		protected override void Dispose(bool disposing)
		{
			if (!disposing) return;
			GC.SuppressFinalize(this);
			base.Dispose(true);
		}

		/// <summary>
		/// Releases unmanaged resources and performs other cleanup operations before the
		/// <see cref="CertificateOfOriginsExternalProxyInstance"> is reclaimed by garbage collection.
		/// </summary>
		~CertificateOfOriginsExternalProxyInstance()
		{
			Dispose();
		}

	#endregion Constructor & Dispose

		#region TempSync Sync Operation

		/// <summary>
		/// TempSync auto generated wrapper
		/// </summary>
		public System.Boolean TempSync(System.Int32 i)
		{
			var cacheKey = base.GetCacheKey("CertificateOfOriginsExternalProxyInstance_TempSync", i);
			if (Cache.Contains(cacheKey))
			{
				var cacheResult = Cache.GetData(cacheKey);
				{
					try
					{
						return (System.Boolean) cacheResult;
					}
					catch (Exception)
					{
						System.Diagnostics.Trace.TraceWarning("Cache item was modified and can't be casting");
					}
				}
			}

			const string methodName = "Customs.CRM.CertificateOfOrigins.Service.TempSync";
			var data = new InfMethodData<System.Boolean>{MethodName = methodName, Parameters = new object[] { i}};
			try
			{
				InitializeProxy(data);
				var infChannel = base.SubscribeMethod(data);
				using (infChannel.Scope)
				{
					var result = infChannel.Channel.TempSync(i);
					Cache.AddFromProxyInstance(cacheKey, result);
					return result;
				}
			}
			catch (FaultException<InfExceptionData> ex)
			{
				var exf = HandleFaultException(ex);
				throw exf;
			}
			catch (Exception ex)
			{
				var exf = HandleGeneralException(ex);
				throw exf;
			}
			finally
			{
				HandleFinally();
			}
		}

		#endregion TempSync Sync Operation

		#region Convert Sync Operation

		/// <summary>
		/// Convert auto generated wrapper
		/// </summary>
		public MalamTeam.Infrastructure.GeneralServices.CommonBase.Base_Entities.VirtualEntity Convert(MalamTeam.Infrastructure.GeneralServices.EAISchema.ConnectedEntity connectedEntity)
		{
			const string methodName = "Customs.CRM.CertificateOfOrigins.Service.Convert";
			var data = new InfMethodData<MalamTeam.Infrastructure.GeneralServices.CommonBase.Base_Entities.VirtualEntity>{MethodName = methodName, Parameters = new object[] { connectedEntity}};
			try
			{
				InitializeProxy(data);
				var infChannel = base.SubscribeMethod(data);
				using (infChannel.Scope)
				{
					var result = infChannel.Channel.Convert(connectedEntity);
					return result;
				}
			}
			catch (FaultException<InfExceptionData> ex)
			{
				var exf = HandleFaultException(ex);
				throw exf;
			}
			catch (Exception ex)
			{
				var exf = HandleGeneralException(ex);
				throw exf;
			}
			finally
			{
				HandleFinally();
			}
		}

		#endregion Convert Sync Operation

		#region HandleAuthenticationRequestDeliverySent Sync Operation

		/// <summary>
		/// HandleAuthenticationRequestDeliverySent auto generated wrapper
		/// </summary>
		public System.Boolean HandleAuthenticationRequestDeliverySent(MalamTeam.Infrastructure.GeneralServices.CommonBase.RaiseEventArgs raiseEventArgs)
		{
			const string methodName = "Customs.CRM.CertificateOfOrigins.Service.HandleAuthenticationRequestDeliverySent";
			var data = new InfMethodData<System.Boolean>{MethodName = methodName, Parameters = new object[] { raiseEventArgs}};
			try
			{
				InitializeProxy(data);
				var infChannel = base.SubscribeMethod(data);
				using (infChannel.Scope)
				{
					var result = infChannel.Channel.HandleAuthenticationRequestDeliverySent(raiseEventArgs);
					return result;
				}
			}
			catch (FaultException<InfExceptionData> ex)
			{
				var exf = HandleFaultException(ex);
				throw exf;
			}
			catch (Exception ex)
			{
				var exf = HandleGeneralException(ex);
				throw exf;
			}
			finally
			{
				HandleFinally();
			}
		}

		#endregion HandleAuthenticationRequestDeliverySent Sync Operation

		#region UpdateCetrificateOfOrigins Operation

		/// <summary>
		/// UpdateCetrificateOfOrigins auto generated wrapper
		/// </summary>
		public string UpdateCetrificateOfOrigins(Customs.CRM.CertificateOfOrigins.ExternalCommon.Common.UpdateCetificateOfOriginsDTO updateCetificateOfOriginsDTO)
		{
			const string methodName = "Customs.CRM.CertificateOfOrigins.Service.UpdateCetrificateOfOrigins";
			var data = new BaseInfMethodData {IsOneWay=true, MethodName = methodName, Parameters = new object[] { updateCetificateOfOriginsDTO}};
			try
			{
				InitializeProxy(data);
				var infChannel = base.SubscribeMethod(data);
				using (infChannel.Scope)
				{
					infChannel.Channel.UpdateCetrificateOfOrigins(updateCetificateOfOriginsDTO);
				}
			}
			catch (FaultException<InfExceptionData> ex)
			{
				var exf = HandleFaultException(ex);
				throw exf;
			}
			catch (Exception ex)
			{
				var exf = HandleGeneralException(ex);
				throw exf;
			}
			finally
			{
				HandleFinally();
			}
			return data.CallId.ToString();
		}

		#endregion

		#region GetCertificateOfOriginID Sync Operation

		/// <summary>
		/// GetCertificateOfOriginID auto generated wrapper
		/// </summary>
		public System.Nullable<System.Int32> GetCertificateOfOriginID(System.String certificateNumber)
		{
			const string methodName = "Customs.CRM.CertificateOfOrigins.Service.GetCertificateOfOriginID";
			var data = new InfMethodData<System.Nullable<System.Int32>>{MethodName = methodName, Parameters = new object[] { certificateNumber}};
			try
			{
				InitializeProxy(data);
				var infChannel = base.SubscribeMethod(data);
				using (infChannel.Scope)
				{
					var result = infChannel.Channel.GetCertificateOfOriginID(certificateNumber);
					return result;
				}
			}
			catch (FaultException<InfExceptionData> ex)
			{
				var exf = HandleFaultException(ex);
				throw exf;
			}
			catch (Exception ex)
			{
				var exf = HandleGeneralException(ex);
				throw exf;
			}
			finally
			{
				HandleFinally();
			}
		}

		#endregion GetCertificateOfOriginID Sync Operation

		#region GetGoodsItemCerificateDTO Sync Operation

		/// <summary>
		/// GetGoodsItemCerificateDTO auto generated wrapper
		/// </summary>
		public System.Collections.Generic.List<Customs.CRM.CertificateOfOrigins.ExternalCommon.Common.GoodsItemCerificateDTO> GetGoodsItemCerificateDTO(System.Collections.Generic.List<Customs.CRM.CertificateOfOrigins.ExternalCommon.Common.GoodsItemCerificateDTO> goodsItemCerificateDTOs)
		{
			const string methodName = "Customs.CRM.CertificateOfOrigins.Service.GetGoodsItemCerificateDTO";
			var data = new InfMethodData<System.Collections.Generic.List<Customs.CRM.CertificateOfOrigins.ExternalCommon.Common.GoodsItemCerificateDTO>>{MethodName = methodName, Parameters = new object[] { goodsItemCerificateDTOs}};
			try
			{
				InitializeProxy(data);
				var infChannel = base.SubscribeMethod(data);
				using (infChannel.Scope)
				{
					var result = infChannel.Channel.GetGoodsItemCerificateDTO(goodsItemCerificateDTOs);
					return result;
				}
			}
			catch (FaultException<InfExceptionData> ex)
			{
				var exf = HandleFaultException(ex);
				throw exf;
			}
			catch (Exception ex)
			{
				var exf = HandleGeneralException(ex);
				throw exf;
			}
			finally
			{
				HandleFinally();
			}
		}

		#endregion GetGoodsItemCerificateDTO Sync Operation

		#region SaveCertificateOfOriginAttachments Sync Operation

		/// <summary>
		/// SaveCertificateOfOriginAttachments auto generated wrapper
		/// </summary>
		public System.Boolean SaveCertificateOfOriginAttachments(Customs.CRM.CertificateOfOrigins.ExternalCommon.Common.SaveCertificateAttachmentsArgsDTO saveCertificateAttachmentsArgsDTO)
		{
			const string methodName = "Customs.CRM.CertificateOfOrigins.Service.SaveCertificateOfOriginAttachments";
			var data = new InfMethodData<System.Boolean>{MethodName = methodName, Parameters = new object[] { saveCertificateAttachmentsArgsDTO}};
			try
			{
				InitializeProxy(data);
				var infChannel = base.SubscribeMethod(data);
				using (infChannel.Scope)
				{
					var result = infChannel.Channel.SaveCertificateOfOriginAttachments(saveCertificateAttachmentsArgsDTO);
					return result;
				}
			}
			catch (FaultException<InfExceptionData> ex)
			{
				var exf = HandleFaultException(ex);
				throw exf;
			}
			catch (Exception ex)
			{
				var exf = HandleGeneralException(ex);
				throw exf;
			}
			finally
			{
				HandleFinally();
			}
		}

		#endregion SaveCertificateOfOriginAttachments Sync Operation
	}
}
