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

namespace Customs.CRM.CertificateOfOrigins.InternalProxy
{
	/// <summary>
	/// Generated proxy instance
	/// </summary>
	public partial class CertificateOfOriginsInternalProxyInstance : BaseProxy2<Customs.CRM.CertificateOfOrigins.InternalCommon.Contract.ICertificateOfOriginsInternalContract>, ICertificateOfOriginsInternalProxy
	{

		#region Constructor & Dispose

		/// <summary>
		/// Initialize the base class. You can specify custom timeout parameter by using existing overload to base class
		/// </summary>
		public CertificateOfOriginsInternalProxyInstance(IMMIFacade currentGuiDispatcher)
		: base("CertificateOfOriginsInternalEndpoint", "CRM", "CertificateOfOriginsInternalService", currentGuiDispatcher)
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
		/// <see cref="CertificateOfOriginsInternalProxyInstance"> is reclaimed by garbage collection.
		/// </summary>
		~CertificateOfOriginsInternalProxyInstance()
		{
			Dispose();
		}

	#endregion Constructor & Dispose

		#region GetCertificateOfOriginsByFilter Operation

		/// <summary>
		/// GetCertificateOfOriginsByFilter auto generated wrapper
		/// </summary>
		public string GetCertificateOfOriginsByFilter(EventHandler<CallbackEventArgs<System.Collections.Generic.List<Customs.CRM.CertificateOfOrigins.InternalCommon.Common.CertificateOfOriginResult>>> clientCallback,Customs.CRM.CertificateOfOrigins.InternalCommon.Common.CertificateOfOriginFilter filter, object stateObject = null)
		{
			const string methodName = "Customs.CRM.CertificateOfOrigins.Service.GetCertificateOfOriginsByFilter";
			var data = new InfMethodData<System.Collections.Generic.List<Customs.CRM.CertificateOfOrigins.InternalCommon.Common.CertificateOfOriginResult>> {CallbackDelegate = clientCallback, StateObject = stateObject, MethodName = methodName, Parameters = new object[] { filter}};
			StartTask(data, GetCertificateOfOriginsByFilter);
			return data.CallId.ToString();
		}

		/// <summary>
		/// GetCertificateOfOriginsByFilter auto generated wrapper
		/// </summary>
		private System.Collections.Generic.List<Customs.CRM.CertificateOfOrigins.InternalCommon.Common.CertificateOfOriginResult> GetCertificateOfOriginsByFilter(object parameters)
		{
			var data = (InfMethodData<System.Collections.Generic.List<Customs.CRM.CertificateOfOrigins.InternalCommon.Common.CertificateOfOriginResult>>) parameters;
			var filter = (Customs.CRM.CertificateOfOrigins.InternalCommon.Common.CertificateOfOriginFilter)data.Parameters[0];
			try
			{
				var infChannel = base.SubscribeMethod(data);
				using (infChannel.Scope)
				{
					var result = infChannel.Channel.GetCertificateOfOriginsByFilter(filter);
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
		}

		#endregion GetCertificateOfOriginsByFilter Operation

		#region IsCertificateOfOriginByExternalIdExist Operation

		/// <summary>
		/// IsCertificateOfOriginByExternalIdExist auto generated wrapper
		/// </summary>
		public string IsCertificateOfOriginByExternalIdExist(EventHandler<CallbackEventArgs<Customs.CRM.CertificateOfOrigins.InternalCommon.Common.CertificateOfOriginResult>> clientCallback,System.String certificateOfOriginExternalId, object stateObject = null)
		{
			const string methodName = "Customs.CRM.CertificateOfOrigins.Service.IsCertificateOfOriginByExternalIdExist";
			var data = new InfMethodData<Customs.CRM.CertificateOfOrigins.InternalCommon.Common.CertificateOfOriginResult> {CallbackDelegate = clientCallback, StateObject = stateObject, MethodName = methodName, Parameters = new object[] { certificateOfOriginExternalId}};
			StartTask(data, IsCertificateOfOriginByExternalIdExist);
			return data.CallId.ToString();
		}

		/// <summary>
		/// IsCertificateOfOriginByExternalIdExist auto generated wrapper
		/// </summary>
		private Customs.CRM.CertificateOfOrigins.InternalCommon.Common.CertificateOfOriginResult IsCertificateOfOriginByExternalIdExist(object parameters)
		{
			var data = (InfMethodData<Customs.CRM.CertificateOfOrigins.InternalCommon.Common.CertificateOfOriginResult>) parameters;
			var certificateOfOriginExternalId = (System.String)data.Parameters[0];
			try
			{
				var infChannel = base.SubscribeMethod(data);
				using (infChannel.Scope)
				{
					var result = infChannel.Channel.IsCertificateOfOriginByExternalIdExist(certificateOfOriginExternalId);
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
		}

		#endregion IsCertificateOfOriginByExternalIdExist Operation

		#region GetCertificateOfOriginById Operation

		/// <summary>
		/// GetCertificateOfOriginById auto generated wrapper
		/// </summary>
		public string GetCertificateOfOriginById(EventHandler<CallbackEventArgs<Customs.CertificateOfOrigins.Entities.CertificateOfOrigin>> clientCallback,System.Int32 certificateOfOriginId, object stateObject = null)
		{
			const string methodName = "Customs.CRM.CertificateOfOrigins.Service.GetCertificateOfOriginById";
			var data = new InfMethodData<Customs.CertificateOfOrigins.Entities.CertificateOfOrigin> {CallbackDelegate = clientCallback, StateObject = stateObject, MethodName = methodName, Parameters = new object[] { certificateOfOriginId}};
			StartTask(data, GetCertificateOfOriginById);
			return data.CallId.ToString();
		}

		/// <summary>
		/// GetCertificateOfOriginById auto generated wrapper
		/// </summary>
		private Customs.CertificateOfOrigins.Entities.CertificateOfOrigin GetCertificateOfOriginById(object parameters)
		{
			var data = (InfMethodData<Customs.CertificateOfOrigins.Entities.CertificateOfOrigin>) parameters;
			var certificateOfOriginId = (System.Int32)data.Parameters[0];
			try
			{
				var infChannel = base.SubscribeMethod(data);
				using (infChannel.Scope)
				{
					var result = infChannel.Channel.GetCertificateOfOriginById(certificateOfOriginId);
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
		}

		#endregion GetCertificateOfOriginById Operation

		#region SaveCertificateOfOrigin Operation

		/// <summary>
		/// SaveCertificateOfOrigin auto generated wrapper
		/// </summary>
		public string SaveCertificateOfOrigin(EventHandler<CallbackEventArgs<Customs.CertificateOfOrigins.Entities.CertificateOfOrigin>> clientCallback,Customs.CertificateOfOrigins.Entities.CertificateOfOrigin certificateOfOrigin, object stateObject = null)
		{
			const string methodName = "Customs.CRM.CertificateOfOrigins.Service.SaveCertificateOfOrigin";
			var data = new InfMethodData<Customs.CertificateOfOrigins.Entities.CertificateOfOrigin> {CallbackDelegate = clientCallback, StateObject = stateObject, MethodName = methodName, Parameters = new object[] { certificateOfOrigin}};
			StartTask(data, SaveCertificateOfOrigin);
			return data.CallId.ToString();
		}

		/// <summary>
		/// SaveCertificateOfOrigin auto generated wrapper
		/// </summary>
		private Customs.CertificateOfOrigins.Entities.CertificateOfOrigin SaveCertificateOfOrigin(object parameters)
		{
			var data = (InfMethodData<Customs.CertificateOfOrigins.Entities.CertificateOfOrigin>) parameters;
			var certificateOfOrigin = (Customs.CertificateOfOrigins.Entities.CertificateOfOrigin)data.Parameters[0];
			try
			{
				var infChannel = base.SubscribeMethod(data);
				using (infChannel.Scope)
				{
					var result = infChannel.Channel.SaveCertificateOfOrigin(certificateOfOrigin);
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
		}

		#endregion SaveCertificateOfOrigin Operation

		#region SaveImportAuthenticationRequest Operation

		/// <summary>
		/// SaveImportAuthenticationRequest auto generated wrapper
		/// </summary>
		public string SaveImportAuthenticationRequest(EventHandler<CallbackEventArgs<Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationRequest>> clientCallback,Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationRequest importAuthenticationRequest, object stateObject = null)
		{
			const string methodName = "Customs.CRM.CertificateOfOrigins.Service.SaveImportAuthenticationRequest";
			var data = new InfMethodData<Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationRequest> {CallbackDelegate = clientCallback, StateObject = stateObject, MethodName = methodName, Parameters = new object[] { importAuthenticationRequest}};
			StartTask(data, SaveImportAuthenticationRequest);
			return data.CallId.ToString();
		}

		/// <summary>
		/// SaveImportAuthenticationRequest auto generated wrapper
		/// </summary>
		private Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationRequest SaveImportAuthenticationRequest(object parameters)
		{
			var data = (InfMethodData<Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationRequest>) parameters;
			var importAuthenticationRequest = (Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationRequest)data.Parameters[0];
			try
			{
				var infChannel = base.SubscribeMethod(data);
				using (infChannel.Scope)
				{
					var result = infChannel.Channel.SaveImportAuthenticationRequest(importAuthenticationRequest);
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
		}

		#endregion SaveImportAuthenticationRequest Operation

		#region GetAuthenticationRequestByFilter Operation

		/// <summary>
		/// GetAuthenticationRequestByFilter auto generated wrapper
		/// </summary>
		public string GetAuthenticationRequestByFilter(EventHandler<CallbackEventArgs<System.Collections.Generic.List<Customs.CertificateOfOrigins.Entities.GetImportAuthenticationRequestResult>>> clientCallback,Customs.CertificateOfOrigins.Entities.ImportAuthenticationRequestFilter filter, object stateObject = null)
		{
			const string methodName = "Customs.CRM.CertificateOfOrigins.Service.GetAuthenticationRequestByFilter";
			var data = new InfMethodData<System.Collections.Generic.List<Customs.CertificateOfOrigins.Entities.GetImportAuthenticationRequestResult>> {CallbackDelegate = clientCallback, StateObject = stateObject, MethodName = methodName, Parameters = new object[] { filter}};
			StartTask(data, GetAuthenticationRequestByFilter);
			return data.CallId.ToString();
		}

		/// <summary>
		/// GetAuthenticationRequestByFilter auto generated wrapper
		/// </summary>
		private System.Collections.Generic.List<Customs.CertificateOfOrigins.Entities.GetImportAuthenticationRequestResult> GetAuthenticationRequestByFilter(object parameters)
		{
			var data = (InfMethodData<System.Collections.Generic.List<Customs.CertificateOfOrigins.Entities.GetImportAuthenticationRequestResult>>) parameters;
			var filter = (Customs.CertificateOfOrigins.Entities.ImportAuthenticationRequestFilter)data.Parameters[0];
			try
			{
				var infChannel = base.SubscribeMethod(data);
				using (infChannel.Scope)
				{
					var result = infChannel.Channel.GetAuthenticationRequestByFilter(filter);
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
		}

		#endregion GetAuthenticationRequestByFilter Operation

		#region GetEntityDocuments Operation

		/// <summary>
		/// GetEntityDocuments auto generated wrapper
		/// </summary>
		public string GetEntityDocuments(EventHandler<CallbackEventArgs<System.Collections.Generic.List<Customs.Infrastructure.DocumentManagement.ExternalCommon.DocumentDTO>>> clientCallback,Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationRequest importAuthenticationRequest, object stateObject = null)
		{
			const string methodName = "Customs.CRM.CertificateOfOrigins.Service.GetEntityDocuments";
			var data = new InfMethodData<System.Collections.Generic.List<Customs.Infrastructure.DocumentManagement.ExternalCommon.DocumentDTO>> {CallbackDelegate = clientCallback, StateObject = stateObject, MethodName = methodName, Parameters = new object[] { importAuthenticationRequest}};
			StartTask(data, GetEntityDocuments);
			return data.CallId.ToString();
		}

		/// <summary>
		/// GetEntityDocuments auto generated wrapper
		/// </summary>
		private System.Collections.Generic.List<Customs.Infrastructure.DocumentManagement.ExternalCommon.DocumentDTO> GetEntityDocuments(object parameters)
		{
			var data = (InfMethodData<System.Collections.Generic.List<Customs.Infrastructure.DocumentManagement.ExternalCommon.DocumentDTO>>) parameters;
			var importAuthenticationRequest = (Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationRequest)data.Parameters[0];
			try
			{
				var infChannel = base.SubscribeMethod(data);
				using (infChannel.Scope)
				{
					var result = infChannel.Channel.GetEntityDocuments(importAuthenticationRequest);
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
		}

		#endregion GetEntityDocuments Operation

		#region CreateNewAuthenticationFile Operation

		/// <summary>
		/// CreateNewAuthenticationFile auto generated wrapper
		/// </summary>
		public string CreateNewAuthenticationFile(EventHandler<CallbackEventArgs<Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationFileDetails>> clientCallback,System.Collections.Generic.List<Customs.CertificateOfOrigins.Entities.GetImportAuthenticationRequestResult> importAuthenticationRequests, object stateObject = null)
		{
			const string methodName = "Customs.CRM.CertificateOfOrigins.Service.CreateNewAuthenticationFile";
			var data = new InfMethodData<Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationFileDetails> {CallbackDelegate = clientCallback, StateObject = stateObject, MethodName = methodName, Parameters = new object[] { importAuthenticationRequests}};
			StartTask(data, CreateNewAuthenticationFile);
			return data.CallId.ToString();
		}

		/// <summary>
		/// CreateNewAuthenticationFile auto generated wrapper
		/// </summary>
		private Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationFileDetails CreateNewAuthenticationFile(object parameters)
		{
			var data = (InfMethodData<Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationFileDetails>) parameters;
			var importAuthenticationRequests = (System.Collections.Generic.List<Customs.CertificateOfOrigins.Entities.GetImportAuthenticationRequestResult>)data.Parameters[0];
			try
			{
				var infChannel = base.SubscribeMethod(data);
				using (infChannel.Scope)
				{
					var result = infChannel.Channel.CreateNewAuthenticationFile(importAuthenticationRequests);
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
		}

		#endregion CreateNewAuthenticationFile Operation

		#region GetAuthenticationRequestFileByID Operation

		/// <summary>
		/// GetAuthenticationRequestFileByID auto generated wrapper
		/// </summary>
		public string GetAuthenticationRequestFileByID(EventHandler<CallbackEventArgs<Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationFileDetails>> clientCallback,System.Int32 fileID, object stateObject = null)
		{
			const string methodName = "Customs.CRM.CertificateOfOrigins.Service.GetAuthenticationRequestFileByID";
			var data = new InfMethodData<Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationFileDetails> {CallbackDelegate = clientCallback, StateObject = stateObject, MethodName = methodName, Parameters = new object[] { fileID}};
			StartTask(data, GetAuthenticationRequestFileByID);
			return data.CallId.ToString();
		}

		/// <summary>
		/// GetAuthenticationRequestFileByID auto generated wrapper
		/// </summary>
		private Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationFileDetails GetAuthenticationRequestFileByID(object parameters)
		{
			var data = (InfMethodData<Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationFileDetails>) parameters;
			var fileID = (System.Int32)data.Parameters[0];
			try
			{
				var infChannel = base.SubscribeMethod(data);
				using (infChannel.Scope)
				{
					var result = infChannel.Channel.GetAuthenticationRequestFileByID(fileID);
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
		}

		#endregion GetAuthenticationRequestFileByID Operation

		#region SaveAuthenticationRequestFile Operation

		/// <summary>
		/// SaveAuthenticationRequestFile auto generated wrapper
		/// </summary>
		public string SaveAuthenticationRequestFile(EventHandler<CallbackEventArgs<Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationFileDetails>> clientCallback,Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationFileDetails authenticationRequestFile, object stateObject = null)
		{
			const string methodName = "Customs.CRM.CertificateOfOrigins.Service.SaveAuthenticationRequestFile";
			var data = new InfMethodData<Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationFileDetails> {CallbackDelegate = clientCallback, StateObject = stateObject, MethodName = methodName, Parameters = new object[] { authenticationRequestFile}};
			StartTask(data, SaveAuthenticationRequestFile);
			return data.CallId.ToString();
		}

		/// <summary>
		/// SaveAuthenticationRequestFile auto generated wrapper
		/// </summary>
		private Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationFileDetails SaveAuthenticationRequestFile(object parameters)
		{
			var data = (InfMethodData<Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationFileDetails>) parameters;
			var authenticationRequestFile = (Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationFileDetails)data.Parameters[0];
			try
			{
				var infChannel = base.SubscribeMethod(data);
				using (infChannel.Scope)
				{
					var result = infChannel.Channel.SaveAuthenticationRequestFile(authenticationRequestFile);
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
		}

		#endregion SaveAuthenticationRequestFile Operation

		#region GetAuthenticationRequestByID Operation

		/// <summary>
		/// GetAuthenticationRequestByID auto generated wrapper
		/// </summary>
		public string GetAuthenticationRequestByID(EventHandler<CallbackEventArgs<Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationRequest>> clientCallback,System.Int32 documentId, object stateObject = null)
		{
			const string methodName = "Customs.CRM.CertificateOfOrigins.Service.GetAuthenticationRequestByID";
			var data = new InfMethodData<Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationRequest> {CallbackDelegate = clientCallback, StateObject = stateObject, MethodName = methodName, Parameters = new object[] { documentId}};
			StartTask(data, GetAuthenticationRequestByID);
			return data.CallId.ToString();
		}

		/// <summary>
		/// GetAuthenticationRequestByID auto generated wrapper
		/// </summary>
		private Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationRequest GetAuthenticationRequestByID(object parameters)
		{
			var data = (InfMethodData<Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationRequest>) parameters;
			var documentId = (System.Int32)data.Parameters[0];
			try
			{
				var infChannel = base.SubscribeMethod(data);
				using (infChannel.Scope)
				{
					var result = infChannel.Channel.GetAuthenticationRequestByID(documentId);
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
		}

		#endregion GetAuthenticationRequestByID Operation

		#region GetAuthenticationRequestByLeadDocumentIDs Operation

		/// <summary>
		/// GetAuthenticationRequestByLeadDocumentIDs auto generated wrapper
		/// </summary>
		public string GetAuthenticationRequestByLeadDocumentIDs(EventHandler<CallbackEventArgs<System.Collections.Generic.List<Customs.CRM.Entities.CertificateOfOriginsDTO.ImportAuthenticationRequestDTO>>> clientCallback,System.Collections.Generic.List<System.Int32> leadDocumentIDs, object stateObject = null)
		{
			const string methodName = "Customs.CRM.CertificateOfOrigins.Service.GetAuthenticationRequestByLeadDocumentIDs";
			var data = new InfMethodData<System.Collections.Generic.List<Customs.CRM.Entities.CertificateOfOriginsDTO.ImportAuthenticationRequestDTO>> {CallbackDelegate = clientCallback, StateObject = stateObject, MethodName = methodName, Parameters = new object[] { leadDocumentIDs}};
			StartTask(data, GetAuthenticationRequestByLeadDocumentIDs);
			return data.CallId.ToString();
		}

		/// <summary>
		/// GetAuthenticationRequestByLeadDocumentIDs auto generated wrapper
		/// </summary>
		private System.Collections.Generic.List<Customs.CRM.Entities.CertificateOfOriginsDTO.ImportAuthenticationRequestDTO> GetAuthenticationRequestByLeadDocumentIDs(object parameters)
		{
			var data = (InfMethodData<System.Collections.Generic.List<Customs.CRM.Entities.CertificateOfOriginsDTO.ImportAuthenticationRequestDTO>>) parameters;
			var leadDocumentIDs = (System.Collections.Generic.List<System.Int32>)data.Parameters[0];
			try
			{
				var infChannel = base.SubscribeMethod(data);
				using (infChannel.Scope)
				{
					var result = infChannel.Channel.GetAuthenticationRequestByLeadDocumentIDs(leadDocumentIDs);
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
		}

		#endregion GetAuthenticationRequestByLeadDocumentIDs Operation

		#region GetExportDocumentAuthenticationRequestSearch Operation

		/// <summary>
		/// GetExportDocumentAuthenticationRequestSearch auto generated wrapper
		/// </summary>
		public string GetExportDocumentAuthenticationRequestSearch(EventHandler<CallbackEventArgs<System.Collections.Generic.List<Customs.CRM.Entities.Entities.ExportDocumentAuthenticationRequestSearch.ExportDocumentAuthenticationRequestSearchResult>>> clientCallback,Customs.CRM.Entities.Entities.ExportDocumentAuthenticationRequestSearch.ExportDocumentAuthenticationRequestSearchFilter filter, object stateObject = null)
		{
			const string methodName = "Customs.CRM.CertificateOfOrigins.Service.GetExportDocumentAuthenticationRequestSearch";
			var data = new InfMethodData<System.Collections.Generic.List<Customs.CRM.Entities.Entities.ExportDocumentAuthenticationRequestSearch.ExportDocumentAuthenticationRequestSearchResult>> {CallbackDelegate = clientCallback, StateObject = stateObject, MethodName = methodName, Parameters = new object[] { filter}};
			StartTask(data, GetExportDocumentAuthenticationRequestSearch);
			return data.CallId.ToString();
		}

		/// <summary>
		/// GetExportDocumentAuthenticationRequestSearch auto generated wrapper
		/// </summary>
		private System.Collections.Generic.List<Customs.CRM.Entities.Entities.ExportDocumentAuthenticationRequestSearch.ExportDocumentAuthenticationRequestSearchResult> GetExportDocumentAuthenticationRequestSearch(object parameters)
		{
			var data = (InfMethodData<System.Collections.Generic.List<Customs.CRM.Entities.Entities.ExportDocumentAuthenticationRequestSearch.ExportDocumentAuthenticationRequestSearchResult>>) parameters;
			var filter = (Customs.CRM.Entities.Entities.ExportDocumentAuthenticationRequestSearch.ExportDocumentAuthenticationRequestSearchFilter)data.Parameters[0];
			try
			{
				var infChannel = base.SubscribeMethod(data);
				using (infChannel.Scope)
				{
					var result = infChannel.Channel.GetExportDocumentAuthenticationRequestSearch(filter);
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
		}

		#endregion GetExportDocumentAuthenticationRequestSearch Operation

		#region GetExportDocumentAuthenticationRequestByID Operation

		/// <summary>
		/// GetExportDocumentAuthenticationRequestByID auto generated wrapper
		/// </summary>
		public string GetExportDocumentAuthenticationRequestByID(EventHandler<CallbackEventArgs<Customs.CertificateOfOrigins.Entities.ExportDocumentAuthenticationRequest>> clientCallback,System.Int32 id, object stateObject = null)
		{
			const string methodName = "Customs.CRM.CertificateOfOrigins.Service.GetExportDocumentAuthenticationRequestByID";
			var data = new InfMethodData<Customs.CertificateOfOrigins.Entities.ExportDocumentAuthenticationRequest> {CallbackDelegate = clientCallback, StateObject = stateObject, MethodName = methodName, Parameters = new object[] { id}};
			StartTask(data, GetExportDocumentAuthenticationRequestByID);
			return data.CallId.ToString();
		}

		/// <summary>
		/// GetExportDocumentAuthenticationRequestByID auto generated wrapper
		/// </summary>
		private Customs.CertificateOfOrigins.Entities.ExportDocumentAuthenticationRequest GetExportDocumentAuthenticationRequestByID(object parameters)
		{
			var data = (InfMethodData<Customs.CertificateOfOrigins.Entities.ExportDocumentAuthenticationRequest>) parameters;
			var id = (System.Int32)data.Parameters[0];
			try
			{
				var infChannel = base.SubscribeMethod(data);
				using (infChannel.Scope)
				{
					var result = infChannel.Channel.GetExportDocumentAuthenticationRequestByID(id);
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
		}

		#endregion GetExportDocumentAuthenticationRequestByID Operation

		#region SaveExportDocumentAuthenticationRequest Operation

		/// <summary>
		/// SaveExportDocumentAuthenticationRequest auto generated wrapper
		/// </summary>
		public string SaveExportDocumentAuthenticationRequest(EventHandler<CallbackEventArgs<Customs.CertificateOfOrigins.Entities.ExportDocumentAuthenticationRequest>> clientCallback,Customs.CertificateOfOrigins.Entities.ExportDocumentAuthenticationRequest entity, object stateObject = null)
		{
			const string methodName = "Customs.CRM.CertificateOfOrigins.Service.SaveExportDocumentAuthenticationRequest";
			var data = new InfMethodData<Customs.CertificateOfOrigins.Entities.ExportDocumentAuthenticationRequest> {CallbackDelegate = clientCallback, StateObject = stateObject, MethodName = methodName, Parameters = new object[] { entity}};
			StartTask(data, SaveExportDocumentAuthenticationRequest);
			return data.CallId.ToString();
		}

		/// <summary>
		/// SaveExportDocumentAuthenticationRequest auto generated wrapper
		/// </summary>
		private Customs.CertificateOfOrigins.Entities.ExportDocumentAuthenticationRequest SaveExportDocumentAuthenticationRequest(object parameters)
		{
			var data = (InfMethodData<Customs.CertificateOfOrigins.Entities.ExportDocumentAuthenticationRequest>) parameters;
			var entity = (Customs.CertificateOfOrigins.Entities.ExportDocumentAuthenticationRequest)data.Parameters[0];
			try
			{
				var infChannel = base.SubscribeMethod(data);
				using (infChannel.Scope)
				{
					var result = infChannel.Channel.SaveExportDocumentAuthenticationRequest(entity);
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
		}

		#endregion SaveExportDocumentAuthenticationRequest Operation

		#region GetCustomerInformation Operation

		/// <summary>
		/// GetCustomerInformation auto generated wrapper
		/// </summary>
		public string GetCustomerInformation(EventHandler<CallbackEventArgs<Customs.StockPileData.Customers.ExternalCommon.Common.CustomerDTO>> clientCallback,System.Int32 customerID, object stateObject = null)
		{
			const string methodName = "Customs.CRM.CertificateOfOrigins.Service.GetCustomerInformation";
			var data = new InfMethodData<Customs.StockPileData.Customers.ExternalCommon.Common.CustomerDTO> {CallbackDelegate = clientCallback, StateObject = stateObject, MethodName = methodName, Parameters = new object[] { customerID}};
			StartTask(data, GetCustomerInformation);
			return data.CallId.ToString();
		}

		/// <summary>
		/// GetCustomerInformation auto generated wrapper
		/// </summary>
		private Customs.StockPileData.Customers.ExternalCommon.Common.CustomerDTO GetCustomerInformation(object parameters)
		{
			var data = (InfMethodData<Customs.StockPileData.Customers.ExternalCommon.Common.CustomerDTO>) parameters;
			var customerID = (System.Int32)data.Parameters[0];
			try
			{
				var infChannel = base.SubscribeMethod(data);
				using (infChannel.Scope)
				{
					var result = infChannel.Channel.GetCustomerInformation(customerID);
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
		}

		#endregion GetCustomerInformation Operation

		#region GetCustomerInformationByCountry Operation

		/// <summary>
		/// GetCustomerInformationByCountry auto generated wrapper
		/// </summary>
		public string GetCustomerInformationByCountry(EventHandler<CallbackEventArgs<Customs.StockPileData.Customers.ExternalCommon.Common.CustomerDTO>> clientCallback,System.Int32 countryID, object stateObject = null)
		{
			const string methodName = "Customs.CRM.CertificateOfOrigins.Service.GetCustomerInformationByCountry";
			var data = new InfMethodData<Customs.StockPileData.Customers.ExternalCommon.Common.CustomerDTO> {CallbackDelegate = clientCallback, StateObject = stateObject, MethodName = methodName, Parameters = new object[] { countryID}};
			StartTask(data, GetCustomerInformationByCountry);
			return data.CallId.ToString();
		}

		/// <summary>
		/// GetCustomerInformationByCountry auto generated wrapper
		/// </summary>
		private Customs.StockPileData.Customers.ExternalCommon.Common.CustomerDTO GetCustomerInformationByCountry(object parameters)
		{
			var data = (InfMethodData<Customs.StockPileData.Customers.ExternalCommon.Common.CustomerDTO>) parameters;
			var countryID = (System.Int32)data.Parameters[0];
			try
			{
				var infChannel = base.SubscribeMethod(data);
				using (infChannel.Scope)
				{
					var result = infChannel.Channel.GetCustomerInformationByCountry(countryID);
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
		}

		#endregion GetCustomerInformationByCountry Operation

		#region HandleImportAuthenticationRequestDeliveryForImporterSent Operation

		/// <summary>
		/// HandleImportAuthenticationRequestDeliveryForImporterSent auto generated wrapper
		/// </summary>
		public string HandleImportAuthenticationRequestDeliveryForImporterSent(EventHandler<CallbackEventArgs<Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationRequest>> clientCallback,Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationRequest authenticationRequest, object stateObject = null)
		{
			const string methodName = "Customs.CRM.CertificateOfOrigins.Service.HandleImportAuthenticationRequestDeliveryForImporterSent";
			var data = new InfMethodData<Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationRequest> {CallbackDelegate = clientCallback, StateObject = stateObject, MethodName = methodName, Parameters = new object[] { authenticationRequest}};
			StartTask(data, HandleImportAuthenticationRequestDeliveryForImporterSent);
			return data.CallId.ToString();
		}

		/// <summary>
		/// HandleImportAuthenticationRequestDeliveryForImporterSent auto generated wrapper
		/// </summary>
		private Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationRequest HandleImportAuthenticationRequestDeliveryForImporterSent(object parameters)
		{
			var data = (InfMethodData<Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationRequest>) parameters;
			var authenticationRequest = (Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationRequest)data.Parameters[0];
			try
			{
				var infChannel = base.SubscribeMethod(data);
				using (infChannel.Scope)
				{
					var result = infChannel.Channel.HandleImportAuthenticationRequestDeliveryForImporterSent(authenticationRequest);
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
		}

		#endregion HandleImportAuthenticationRequestDeliveryForImporterSent Operation

		#region HandleImportAuthenticationRequestDeliveryReminderForImporterSent Operation

		/// <summary>
		/// HandleImportAuthenticationRequestDeliveryReminderForImporterSent auto generated wrapper
		/// </summary>
		public string HandleImportAuthenticationRequestDeliveryReminderForImporterSent(EventHandler<CallbackEventArgs<Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationRequest>> clientCallback,Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationRequest authenticationRequest, object stateObject = null)
		{
			const string methodName = "Customs.CRM.CertificateOfOrigins.Service.HandleImportAuthenticationRequestDeliveryReminderForImporterSent";
			var data = new InfMethodData<Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationRequest> {CallbackDelegate = clientCallback, StateObject = stateObject, MethodName = methodName, Parameters = new object[] { authenticationRequest}};
			StartTask(data, HandleImportAuthenticationRequestDeliveryReminderForImporterSent);
			return data.CallId.ToString();
		}

		/// <summary>
		/// HandleImportAuthenticationRequestDeliveryReminderForImporterSent auto generated wrapper
		/// </summary>
		private Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationRequest HandleImportAuthenticationRequestDeliveryReminderForImporterSent(object parameters)
		{
			var data = (InfMethodData<Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationRequest>) parameters;
			var authenticationRequest = (Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationRequest)data.Parameters[0];
			try
			{
				var infChannel = base.SubscribeMethod(data);
				using (infChannel.Scope)
				{
					var result = infChannel.Channel.HandleImportAuthenticationRequestDeliveryReminderForImporterSent(authenticationRequest);
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
		}

		#endregion HandleImportAuthenticationRequestDeliveryReminderForImporterSent Operation

		#region HandleImportAuthenticationRequestDeliveryAndReminderForVendorSent Operation

		/// <summary>
		/// HandleImportAuthenticationRequestDeliveryAndReminderForVendorSent auto generated wrapper
		/// </summary>
		public string HandleImportAuthenticationRequestDeliveryAndReminderForVendorSent(EventHandler<CallbackEventArgs<Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationFileDetails>> clientCallback,Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationFileDetails authenticationFile,System.Boolean isDelivery, object stateObject = null)
		{
			const string methodName = "Customs.CRM.CertificateOfOrigins.Service.HandleImportAuthenticationRequestDeliveryAndReminderForVendorSent";
			var data = new InfMethodData<Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationFileDetails> {CallbackDelegate = clientCallback, StateObject = stateObject, MethodName = methodName, Parameters = new object[] { authenticationFile,isDelivery}};
			StartTask(data, HandleImportAuthenticationRequestDeliveryAndReminderForVendorSent);
			return data.CallId.ToString();
		}

		/// <summary>
		/// HandleImportAuthenticationRequestDeliveryAndReminderForVendorSent auto generated wrapper
		/// </summary>
		private Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationFileDetails HandleImportAuthenticationRequestDeliveryAndReminderForVendorSent(object parameters)
		{
			var data = (InfMethodData<Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationFileDetails>) parameters;
			var authenticationFile = (Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationFileDetails)data.Parameters[0];
			var isDelivery = (System.Boolean)data.Parameters[1];
			try
			{
				var infChannel = base.SubscribeMethod(data);
				using (infChannel.Scope)
				{
					var result = infChannel.Channel.HandleImportAuthenticationRequestDeliveryAndReminderForVendorSent(authenticationFile,isDelivery);
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
		}

		#endregion HandleImportAuthenticationRequestDeliveryAndReminderForVendorSent Operation

		#region CheckIfExistsAdditionalRequestsForImporter Operation

		/// <summary>
		/// CheckIfExistsAdditionalRequestsForImporter auto generated wrapper
		/// </summary>
		public string CheckIfExistsAdditionalRequestsForImporter(EventHandler<CallbackEventArgs<System.Boolean>> clientCallback,Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationRequest request, object stateObject = null)
		{
			const string methodName = "Customs.CRM.CertificateOfOrigins.Service.CheckIfExistsAdditionalRequestsForImporter";
			var data = new InfMethodData<System.Boolean> {CallbackDelegate = clientCallback, StateObject = stateObject, MethodName = methodName, Parameters = new object[] { request}};
			StartTask(data, CheckIfExistsAdditionalRequestsForImporter);
			return data.CallId.ToString();
		}

		/// <summary>
		/// CheckIfExistsAdditionalRequestsForImporter auto generated wrapper
		/// </summary>
		private System.Boolean CheckIfExistsAdditionalRequestsForImporter(object parameters)
		{
			var data = (InfMethodData<System.Boolean>) parameters;
			var request = (Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationRequest)data.Parameters[0];
			try
			{
				var infChannel = base.SubscribeMethod(data);
				using (infChannel.Scope)
				{
					var result = infChannel.Channel.CheckIfExistsAdditionalRequestsForImporter(request);
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
		}

		#endregion CheckIfExistsAdditionalRequestsForImporter Operation

		#region CheckIfExistsAdditionalRequestsForVendor Operation

		/// <summary>
		/// CheckIfExistsAdditionalRequestsForVendor auto generated wrapper
		/// </summary>
		public string CheckIfExistsAdditionalRequestsForVendor(EventHandler<CallbackEventArgs<System.Boolean>> clientCallback,System.Int32 vendorID, object stateObject = null)
		{
			const string methodName = "Customs.CRM.CertificateOfOrigins.Service.CheckIfExistsAdditionalRequestsForVendor";
			var data = new InfMethodData<System.Boolean> {CallbackDelegate = clientCallback, StateObject = stateObject, MethodName = methodName, Parameters = new object[] { vendorID}};
			StartTask(data, CheckIfExistsAdditionalRequestsForVendor);
			return data.CallId.ToString();
		}

		/// <summary>
		/// CheckIfExistsAdditionalRequestsForVendor auto generated wrapper
		/// </summary>
		private System.Boolean CheckIfExistsAdditionalRequestsForVendor(object parameters)
		{
			var data = (InfMethodData<System.Boolean>) parameters;
			var vendorID = (System.Int32)data.Parameters[0];
			try
			{
				var infChannel = base.SubscribeMethod(data);
				using (infChannel.Scope)
				{
					var result = infChannel.Channel.CheckIfExistsAdditionalRequestsForVendor(vendorID);
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
		}

		#endregion CheckIfExistsAdditionalRequestsForVendor Operation

		#region GetPathsForNavigationToVendor Operation

		/// <summary>
		/// GetPathsForNavigationToVendor auto generated wrapper
		/// </summary>
		public string GetPathsForNavigationToVendor(EventHandler<CallbackEventArgs<Customs.CRM.External.NavigationToVendorView>> clientCallback, object stateObject = null)
		{
			const string methodName = "Customs.CRM.CertificateOfOrigins.Service.GetPathsForNavigationToVendor";
			var data = new InfMethodData<Customs.CRM.External.NavigationToVendorView> {CallbackDelegate = clientCallback, StateObject = stateObject, MethodName = methodName, Parameters = new object[] { }};
			StartTask(data, GetPathsForNavigationToVendor);
			return data.CallId.ToString();
		}

		/// <summary>
		/// GetPathsForNavigationToVendor auto generated wrapper
		/// </summary>
		private Customs.CRM.External.NavigationToVendorView GetPathsForNavigationToVendor(object parameters)
		{
			var data = (InfMethodData<Customs.CRM.External.NavigationToVendorView>) parameters;
			try
			{
				var infChannel = base.SubscribeMethod(data);
				using (infChannel.Scope)
				{
					var result = infChannel.Channel.GetPathsForNavigationToVendor();
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
		}

		#endregion GetPathsForNavigationToVendor Operation

		#region HandleSendRemindDeliverNotification Operation

		/// <summary>
		/// HandleSendRemindDeliverNotification auto generated wrapper
		/// </summary>
		public string HandleSendRemindDeliverNotification(EventHandler<CallbackEventArgs<System.Boolean>> clientCallback,Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationFileDetails file, object stateObject = null)
		{
			const string methodName = "Customs.CRM.CertificateOfOrigins.Service.HandleSendRemindDeliverNotification";
			var data = new InfMethodData<System.Boolean> {CallbackDelegate = clientCallback, StateObject = stateObject, MethodName = methodName, Parameters = new object[] { file}};
			StartTask(data, HandleSendRemindDeliverNotification);
			return data.CallId.ToString();
		}

		/// <summary>
		/// HandleSendRemindDeliverNotification auto generated wrapper
		/// </summary>
		private System.Boolean HandleSendRemindDeliverNotification(object parameters)
		{
			var data = (InfMethodData<System.Boolean>) parameters;
			var file = (Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationFileDetails)data.Parameters[0];
			try
			{
				var infChannel = base.SubscribeMethod(data);
				using (infChannel.Scope)
				{
					var result = infChannel.Channel.HandleSendRemindDeliverNotification(file);
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
		}

		#endregion HandleSendRemindDeliverNotification Operation

		#region ChangeStatusAfterDeliverySent Operation

		/// <summary>
		/// ChangeStatusAfterDeliverySent auto generated wrapper
		/// </summary>
		public string ChangeStatusAfterDeliverySent(EventHandler<CallbackEventArgs<Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationFileDetails>> clientCallback,Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationFileDetails importAuthenticationRequest, object stateObject = null)
		{
			const string methodName = "Customs.CRM.CertificateOfOrigins.Service.ChangeStatusAfterDeliverySent";
			var data = new InfMethodData<Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationFileDetails> {CallbackDelegate = clientCallback, StateObject = stateObject, MethodName = methodName, Parameters = new object[] { importAuthenticationRequest}};
			StartTask(data, ChangeStatusAfterDeliverySent);
			return data.CallId.ToString();
		}

		/// <summary>
		/// ChangeStatusAfterDeliverySent auto generated wrapper
		/// </summary>
		private Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationFileDetails ChangeStatusAfterDeliverySent(object parameters)
		{
			var data = (InfMethodData<Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationFileDetails>) parameters;
			var importAuthenticationRequest = (Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationFileDetails)data.Parameters[0];
			try
			{
				var infChannel = base.SubscribeMethod(data);
				using (infChannel.Scope)
				{
					var result = infChannel.Channel.ChangeStatusAfterDeliverySent(importAuthenticationRequest);
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
		}

		#endregion ChangeStatusAfterDeliverySent Operation

		#region CheckImporterOfImportAuthentication Operation

		/// <summary>
		/// CheckImporterOfImportAuthentication auto generated wrapper
		/// </summary>
		public string CheckImporterOfImportAuthentication(EventHandler<CallbackEventArgs<System.Nullable<System.Int32>>> clientCallback,System.Int32 importerId, object stateObject = null)
		{
			const string methodName = "Customs.CRM.CertificateOfOrigins.Service.CheckImporterOfImportAuthentication";
			var data = new InfMethodData<System.Nullable<System.Int32>> {CallbackDelegate = clientCallback, StateObject = stateObject, MethodName = methodName, Parameters = new object[] { importerId}};
			StartTask(data, CheckImporterOfImportAuthentication);
			return data.CallId.ToString();
		}

		/// <summary>
		/// CheckImporterOfImportAuthentication auto generated wrapper
		/// </summary>
		private System.Nullable<System.Int32> CheckImporterOfImportAuthentication(object parameters)
		{
			var data = (InfMethodData<System.Nullable<System.Int32>>) parameters;
			var importerId = (System.Int32)data.Parameters[0];
			try
			{
				var infChannel = base.SubscribeMethod(data);
				using (infChannel.Scope)
				{
					var result = infChannel.Channel.CheckImporterOfImportAuthentication(importerId);
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
		}

		#endregion CheckImporterOfImportAuthentication Operation

		#region LoadDataFromExportDeclaration Operation

		/// <summary>
		/// LoadDataFromExportDeclaration auto generated wrapper
		/// </summary>
		public string LoadDataFromExportDeclaration(EventHandler<CallbackEventArgs<System.Boolean>> clientCallback,Customs.CertificateOfOrigins.Entities.CertificateOfOrigin certificateOfOrigin, object stateObject = null)
		{
			const string methodName = "Customs.CRM.CertificateOfOrigins.Service.LoadDataFromExportDeclaration";
			var data = new InfMethodData<System.Boolean> {CallbackDelegate = clientCallback, StateObject = stateObject, MethodName = methodName, Parameters = new object[] { certificateOfOrigin}};
			StartTask(data, LoadDataFromExportDeclaration);
			return data.CallId.ToString();
		}

		/// <summary>
		/// LoadDataFromExportDeclaration auto generated wrapper
		/// </summary>
		private System.Boolean LoadDataFromExportDeclaration(object parameters)
		{
			var data = (InfMethodData<System.Boolean>) parameters;
			var certificateOfOrigin = (Customs.CertificateOfOrigins.Entities.CertificateOfOrigin)data.Parameters[0];
			try
			{
				var infChannel = base.SubscribeMethod(data);
				using (infChannel.Scope)
				{
					var result = infChannel.Channel.LoadDataFromExportDeclaration(certificateOfOrigin);
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
		}

		#endregion LoadDataFromExportDeclaration Operation
	}
}
