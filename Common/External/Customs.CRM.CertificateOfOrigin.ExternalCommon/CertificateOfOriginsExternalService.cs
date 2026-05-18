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
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.ServiceModel;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.Exceptions;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.Extensions;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.AOP;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.Communication;
using Customs.CRM.CertificateOfOrigins.ExternalProxy;
using MalamTeam.Infrastructure.GeneralServices.BaseLayer;

namespace Customs.CRM.CertificateOfOrigins.Service
{
	/// <summary>
	/// Generated service layer
	/// </summary>
	public partial class CertificateOfOriginsExternalService : BaseService2, Customs.CRM.CertificateOfOrigins.ExternalCommon.Contract.ICertificateOfOriginsExternalContract, IInternalCertificateOfOriginsExternalService
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

		#endregion Customs.CRM.CertificateOfOrigins.ExternalCommon.Contract.ICertificateOfOriginsExternalContract Instance Members

		/// <summary>
		/// TempSync auto generated wrapper
		/// </summary>
		public System.Boolean TempSync(System.Int32 i)
		{
			const string methodName = "Customs.CRM.CertificateOfOrigins.Service.CertificateOfOriginsExternalService.TempSync";
			try
			{
				InitializeService(methodName, i);
				System.Boolean result;
				result = InternalTempSync(i);
				return result;
			}
			catch (System.Exception ex)
			{
				var parameters = new object[] {i};
				throw HandleException(ex, assemblyName, methodName, parameters);
			}
			finally
			{
				HandleFinally();
			}
		}

		/// <summary>
		/// Convert auto generated wrapper
		/// </summary>
		public MalamTeam.Infrastructure.GeneralServices.CommonBase.Base_Entities.VirtualEntity Convert(MalamTeam.Infrastructure.GeneralServices.EAISchema.ConnectedEntity connectedEntity)
		{
			const string methodName = "Customs.CRM.CertificateOfOrigins.Service.CertificateOfOriginsExternalService.Convert";
			try
			{
				InitializeService(methodName, connectedEntity);
				MalamTeam.Infrastructure.GeneralServices.CommonBase.Base_Entities.VirtualEntity result;
				result = InternalConvert(connectedEntity);
				return result;
			}
			catch (System.Exception ex)
			{
				var parameters = new object[] {connectedEntity};
				throw HandleException(ex, assemblyName, methodName, parameters);
			}
			finally
			{
				HandleFinally();
			}
		}

		/// <summary>
		/// HandleAuthenticationRequestDeliverySent auto generated wrapper
		/// </summary>
		public System.Boolean HandleAuthenticationRequestDeliverySent(MalamTeam.Infrastructure.GeneralServices.CommonBase.RaiseEventArgs raiseEventArgs)
		{
			const string methodName = "Customs.CRM.CertificateOfOrigins.Service.CertificateOfOriginsExternalService.HandleAuthenticationRequestDeliverySent";
			try
			{
				InitializeService(methodName, raiseEventArgs);
				System.Boolean result;
				result = InternalHandleAuthenticationRequestDeliverySent(raiseEventArgs);
				return result;
			}
			catch (System.Exception ex)
			{
				var parameters = new object[] {raiseEventArgs};
				throw HandleException(ex, assemblyName, methodName, parameters);
			}
			finally
			{
				HandleFinally();
			}
		}

		/// <summary>
		/// UpdateCetrificateOfOrigins auto generated wrapper
		/// </summary>
		public void UpdateCetrificateOfOrigins(Customs.CRM.CertificateOfOrigins.ExternalCommon.Common.UpdateCetificateOfOriginsDTO updateCetificateOfOriginsDTO)
		{
			const string methodName = "Customs.CRM.CertificateOfOrigins.Service.CertificateOfOriginsExternalService.UpdateCetrificateOfOrigins";
			try
			{
				InitializeService(methodName, updateCetificateOfOriginsDTO);
				InternalUpdateCetrificateOfOrigins(updateCetificateOfOriginsDTO);
			}
			catch (System.Exception ex)
			{
				var parameters = new object[] {updateCetificateOfOriginsDTO};
				base.HandleException(ex, assemblyName, methodName, parameters, true);
			}
			finally
			{
				HandleFinally();
			}
		}

		/// <summary>
		/// GetCertificateOfOriginID auto generated wrapper
		/// </summary>
		public System.Nullable<System.Int32> GetCertificateOfOriginID(System.String certificateNumber)
		{
			const string methodName = "Customs.CRM.CertificateOfOrigins.Service.CertificateOfOriginsExternalService.GetCertificateOfOriginID";
			try
			{
				InitializeService(methodName, certificateNumber);
				System.Nullable<System.Int32> result;
				result = InternalGetCertificateOfOriginID(certificateNumber);
				return result;
			}
			catch (System.Exception ex)
			{
				var parameters = new object[] {certificateNumber};
				throw HandleException(ex, assemblyName, methodName, parameters);
			}
			finally
			{
				HandleFinally();
			}
		}

		/// <summary>
		/// GetGoodsItemCerificateDTO auto generated wrapper
		/// </summary>
		public System.Collections.Generic.List<Customs.CRM.CertificateOfOrigins.ExternalCommon.Common.GoodsItemCerificateDTO> GetGoodsItemCerificateDTO(System.Collections.Generic.List<Customs.CRM.CertificateOfOrigins.ExternalCommon.Common.GoodsItemCerificateDTO> goodsItemCerificateDTOs)
		{
			const string methodName = "Customs.CRM.CertificateOfOrigins.Service.CertificateOfOriginsExternalService.GetGoodsItemCerificateDTO";
			try
			{
				InitializeService(methodName, goodsItemCerificateDTOs);
				System.Collections.Generic.List<Customs.CRM.CertificateOfOrigins.ExternalCommon.Common.GoodsItemCerificateDTO> result;
				result = InternalGetGoodsItemCerificateDTO(goodsItemCerificateDTOs);
				return result;
			}
			catch (System.Exception ex)
			{
				var parameters = new object[] {goodsItemCerificateDTOs};
				throw HandleException(ex, assemblyName, methodName, parameters);
			}
			finally
			{
				HandleFinally();
			}
		}

		/// <summary>
		/// SaveCertificateOfOriginAttachments auto generated wrapper
		/// </summary>
		public System.Boolean SaveCertificateOfOriginAttachments(Customs.CRM.CertificateOfOrigins.ExternalCommon.Common.SaveCertificateAttachmentsArgsDTO saveCertificateAttachmentsArgsDTO)
		{
			const string methodName = "Customs.CRM.CertificateOfOrigins.Service.CertificateOfOriginsExternalService.SaveCertificateOfOriginAttachments";
			try
			{
				InitializeService(methodName, saveCertificateAttachmentsArgsDTO);
				System.Boolean result;
				result = InternalSaveCertificateOfOriginAttachments(saveCertificateAttachmentsArgsDTO);
				return result;
			}
			catch (System.Exception ex)
			{
				var parameters = new object[] {saveCertificateAttachmentsArgsDTO};
				throw HandleException(ex, assemblyName, methodName, parameters);
			}
			finally
			{
				HandleFinally();
			}
		}
	}

	interface IInternalCertificateOfOriginsExternalService
	{
		/// <summary>
		/// TempSync auto generated wrapper
		/// </summary>
		System.Boolean InternalTempSync(System.Int32 i);
		/// <summary>
		/// Convert auto generated wrapper
		/// </summary>
		MalamTeam.Infrastructure.GeneralServices.CommonBase.Base_Entities.VirtualEntity InternalConvert(MalamTeam.Infrastructure.GeneralServices.EAISchema.ConnectedEntity connectedEntity);
		/// <summary>
		/// HandleAuthenticationRequestDeliverySent auto generated wrapper
		/// </summary>
		System.Boolean InternalHandleAuthenticationRequestDeliverySent(MalamTeam.Infrastructure.GeneralServices.CommonBase.RaiseEventArgs raiseEventArgs);
		/// <summary>
		/// UpdateCetrificateOfOrigins auto generated wrapper
		/// </summary>
		void InternalUpdateCetrificateOfOrigins(Customs.CRM.CertificateOfOrigins.ExternalCommon.Common.UpdateCetificateOfOriginsDTO updateCetificateOfOriginsDTO);
		/// <summary>
		/// GetCertificateOfOriginID auto generated wrapper
		/// </summary>
		System.Nullable<System.Int32> InternalGetCertificateOfOriginID(System.String certificateNumber);
		/// <summary>
		/// GetGoodsItemCerificateDTO auto generated wrapper
		/// </summary>
		System.Collections.Generic.List<Customs.CRM.CertificateOfOrigins.ExternalCommon.Common.GoodsItemCerificateDTO> InternalGetGoodsItemCerificateDTO(System.Collections.Generic.List<Customs.CRM.CertificateOfOrigins.ExternalCommon.Common.GoodsItemCerificateDTO> goodsItemCerificateDTOs);
		/// <summary>
		/// SaveCertificateOfOriginAttachments auto generated wrapper
		/// </summary>
		System.Boolean InternalSaveCertificateOfOriginAttachments(Customs.CRM.CertificateOfOrigins.ExternalCommon.Common.SaveCertificateAttachmentsArgsDTO saveCertificateAttachmentsArgsDTO);
	}

	/// <summary>
	/// Direct proxy instance (refers directly to service)
	/// </summary>
	public partial class CertificateOfOriginsExternalDirectProxyInstance : ICertificateOfOriginsExternalProxy
	{
		/// <summary>
		/// Ctor
		/// </summary>
		/// <summary>
		/// TempSync auto generated wrapper
		/// </summary>
		public System.Boolean TempSync(System.Int32 i)
		{
			var service = new CertificateOfOriginsExternalService();
			return service.InternalTempSync(i);
		}
		/// <summary>
		/// Convert auto generated wrapper
		/// </summary>
		public MalamTeam.Infrastructure.GeneralServices.CommonBase.Base_Entities.VirtualEntity Convert(MalamTeam.Infrastructure.GeneralServices.EAISchema.ConnectedEntity connectedEntity)
		{
			var service = new CertificateOfOriginsExternalService();
			return service.InternalConvert(connectedEntity);
		}
		/// <summary>
		/// HandleAuthenticationRequestDeliverySent auto generated wrapper
		/// </summary>
		public System.Boolean HandleAuthenticationRequestDeliverySent(MalamTeam.Infrastructure.GeneralServices.CommonBase.RaiseEventArgs raiseEventArgs)
		{
			var service = new CertificateOfOriginsExternalService();
			return service.InternalHandleAuthenticationRequestDeliverySent(raiseEventArgs);
		}
		/// <summary>
		/// UpdateCetrificateOfOrigins auto generated wrapper
		/// </summary>
		public string UpdateCetrificateOfOrigins(Customs.CRM.CertificateOfOrigins.ExternalCommon.Common.UpdateCetificateOfOriginsDTO updateCetificateOfOriginsDTO)
		{
			var service = new CertificateOfOriginsExternalService();
			service.InternalUpdateCetrificateOfOrigins(updateCetificateOfOriginsDTO);
			return string.Empty;
		}
		/// <summary>
		/// GetCertificateOfOriginID auto generated wrapper
		/// </summary>
		public System.Nullable<System.Int32> GetCertificateOfOriginID(System.String certificateNumber)
		{
			var service = new CertificateOfOriginsExternalService();
			return service.InternalGetCertificateOfOriginID(certificateNumber);
		}
		/// <summary>
		/// GetGoodsItemCerificateDTO auto generated wrapper
		/// </summary>
		public System.Collections.Generic.List<Customs.CRM.CertificateOfOrigins.ExternalCommon.Common.GoodsItemCerificateDTO> GetGoodsItemCerificateDTO(System.Collections.Generic.List<Customs.CRM.CertificateOfOrigins.ExternalCommon.Common.GoodsItemCerificateDTO> goodsItemCerificateDTOs)
		{
			var service = new CertificateOfOriginsExternalService();
			return service.InternalGetGoodsItemCerificateDTO(goodsItemCerificateDTOs);
		}
		/// <summary>
		/// SaveCertificateOfOriginAttachments auto generated wrapper
		/// </summary>
		public System.Boolean SaveCertificateOfOriginAttachments(Customs.CRM.CertificateOfOrigins.ExternalCommon.Common.SaveCertificateAttachmentsArgsDTO saveCertificateAttachmentsArgsDTO)
		{
			var service = new CertificateOfOriginsExternalService();
			return service.InternalSaveCertificateOfOriginAttachments(saveCertificateAttachmentsArgsDTO);
		}
	}
}
