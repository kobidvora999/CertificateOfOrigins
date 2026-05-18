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
using Customs.CRM.CertificateOfOrigins.InternalProxy;
using MalamTeam.Infrastructure.GeneralServices.BaseLayer;

namespace Customs.CRM.CertificateOfOrigins.Service
{
	/// <summary>
	/// Generated service layer
	/// </summary>
	public partial class CertificateOfOriginsInternalService : BaseService2, Customs.CRM.CertificateOfOrigins.InternalCommon.Contract.ICertificateOfOriginsInternalContract, IInternalCertificateOfOriginsInternalService
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

		#endregion Customs.CRM.CertificateOfOrigins.InternalCommon.Contract.ICertificateOfOriginsInternalContract Instance Members

		/// <summary>
		/// GetCertificateOfOriginsByFilter auto generated wrapper
		/// </summary>
		public System.Collections.Generic.List<Customs.CRM.CertificateOfOrigins.InternalCommon.Common.CertificateOfOriginResult> GetCertificateOfOriginsByFilter(Customs.CRM.CertificateOfOrigins.InternalCommon.Common.CertificateOfOriginFilter filter)
		{
			const string methodName = "Customs.CRM.CertificateOfOrigins.Service.CertificateOfOriginsInternalService.GetCertificateOfOriginsByFilter";
			try
			{
				InitializeService(methodName, filter);
				System.Collections.Generic.List<Customs.CRM.CertificateOfOrigins.InternalCommon.Common.CertificateOfOriginResult> result;
				result = InternalGetCertificateOfOriginsByFilter(filter);
				return result;
			}
			catch (System.Exception ex)
			{
				var parameters = new object[] {filter};
				throw HandleException(ex, assemblyName, methodName, parameters);
			}
			finally
			{
				HandleFinally();
			}
		}

		/// <summary>
		/// IsCertificateOfOriginByExternalIdExist auto generated wrapper
		/// </summary>
		public Customs.CRM.CertificateOfOrigins.InternalCommon.Common.CertificateOfOriginResult IsCertificateOfOriginByExternalIdExist(System.String certificateOfOriginExternalId)
		{
			const string methodName = "Customs.CRM.CertificateOfOrigins.Service.CertificateOfOriginsInternalService.IsCertificateOfOriginByExternalIdExist";
			try
			{
				InitializeService(methodName, certificateOfOriginExternalId);
				Customs.CRM.CertificateOfOrigins.InternalCommon.Common.CertificateOfOriginResult result;
				result = InternalIsCertificateOfOriginByExternalIdExist(certificateOfOriginExternalId);
				return result;
			}
			catch (System.Exception ex)
			{
				var parameters = new object[] {certificateOfOriginExternalId};
				throw HandleException(ex, assemblyName, methodName, parameters);
			}
			finally
			{
				HandleFinally();
			}
		}

		/// <summary>
		/// GetCertificateOfOriginById auto generated wrapper
		/// </summary>
		public Customs.CertificateOfOrigins.Entities.CertificateOfOrigin GetCertificateOfOriginById(System.Int32 certificateOfOriginId)
		{
			const string methodName = "Customs.CRM.CertificateOfOrigins.Service.CertificateOfOriginsInternalService.GetCertificateOfOriginById";
			try
			{
				InitializeService(methodName, certificateOfOriginId);
				Customs.CertificateOfOrigins.Entities.CertificateOfOrigin result;
				result = InternalGetCertificateOfOriginById(certificateOfOriginId);
				return result;
			}
			catch (System.Exception ex)
			{
				var parameters = new object[] {certificateOfOriginId};
				throw HandleException(ex, assemblyName, methodName, parameters);
			}
			finally
			{
				HandleFinally();
			}
		}

		/// <summary>
		/// SaveCertificateOfOrigin auto generated wrapper
		/// </summary>
		public Customs.CertificateOfOrigins.Entities.CertificateOfOrigin SaveCertificateOfOrigin(Customs.CertificateOfOrigins.Entities.CertificateOfOrigin certificateOfOrigin)
		{
			const string methodName = "Customs.CRM.CertificateOfOrigins.Service.CertificateOfOriginsInternalService.SaveCertificateOfOrigin";
			try
			{
				InitializeService(methodName, certificateOfOrigin);
				Customs.CertificateOfOrigins.Entities.CertificateOfOrigin result;
				result = InternalSaveCertificateOfOrigin(certificateOfOrigin);
				return result;
			}
			catch (System.Exception ex)
			{
				var parameters = new object[] {certificateOfOrigin};
				throw HandleException(ex, assemblyName, methodName, parameters);
			}
			finally
			{
				HandleFinally();
			}
		}

		/// <summary>
		/// SaveImportAuthenticationRequest auto generated wrapper
		/// </summary>
		public Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationRequest SaveImportAuthenticationRequest(Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationRequest importAuthenticationRequest)
		{
			const string methodName = "Customs.CRM.CertificateOfOrigins.Service.CertificateOfOriginsInternalService.SaveImportAuthenticationRequest";
			try
			{
				InitializeService(methodName, importAuthenticationRequest);
				Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationRequest result;
				result = InternalSaveImportAuthenticationRequest(importAuthenticationRequest);
				return result;
			}
			catch (System.Exception ex)
			{
				var parameters = new object[] {importAuthenticationRequest};
				throw HandleException(ex, assemblyName, methodName, parameters);
			}
			finally
			{
				HandleFinally();
			}
		}

		/// <summary>
		/// GetAuthenticationRequestByFilter auto generated wrapper
		/// </summary>
		public System.Collections.Generic.List<Customs.CertificateOfOrigins.Entities.GetImportAuthenticationRequestResult> GetAuthenticationRequestByFilter(Customs.CertificateOfOrigins.Entities.ImportAuthenticationRequestFilter filter)
		{
			const string methodName = "Customs.CRM.CertificateOfOrigins.Service.CertificateOfOriginsInternalService.GetAuthenticationRequestByFilter";
			try
			{
				InitializeService(methodName, filter);
				System.Collections.Generic.List<Customs.CertificateOfOrigins.Entities.GetImportAuthenticationRequestResult> result;
				result = InternalGetAuthenticationRequestByFilter(filter);
				return result;
			}
			catch (System.Exception ex)
			{
				var parameters = new object[] {filter};
				throw HandleException(ex, assemblyName, methodName, parameters);
			}
			finally
			{
				HandleFinally();
			}
		}

		/// <summary>
		/// GetEntityDocuments auto generated wrapper
		/// </summary>
		public System.Collections.Generic.List<Customs.Infrastructure.DocumentManagement.ExternalCommon.DocumentDTO> GetEntityDocuments(Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationRequest importAuthenticationRequest)
		{
			const string methodName = "Customs.CRM.CertificateOfOrigins.Service.CertificateOfOriginsInternalService.GetEntityDocuments";
			try
			{
				InitializeService(methodName, importAuthenticationRequest);
				System.Collections.Generic.List<Customs.Infrastructure.DocumentManagement.ExternalCommon.DocumentDTO> result;
				result = InternalGetEntityDocuments(importAuthenticationRequest);
				return result;
			}
			catch (System.Exception ex)
			{
				var parameters = new object[] {importAuthenticationRequest};
				throw HandleException(ex, assemblyName, methodName, parameters);
			}
			finally
			{
				HandleFinally();
			}
		}

		/// <summary>
		/// CreateNewAuthenticationFile auto generated wrapper
		/// </summary>
		public Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationFileDetails CreateNewAuthenticationFile(System.Collections.Generic.List<Customs.CertificateOfOrigins.Entities.GetImportAuthenticationRequestResult> importAuthenticationRequests)
		{
			const string methodName = "Customs.CRM.CertificateOfOrigins.Service.CertificateOfOriginsInternalService.CreateNewAuthenticationFile";
			try
			{
				InitializeService(methodName, importAuthenticationRequests);
				Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationFileDetails result;
				result = InternalCreateNewAuthenticationFile(importAuthenticationRequests);
				return result;
			}
			catch (System.Exception ex)
			{
				var parameters = new object[] {importAuthenticationRequests};
				throw HandleException(ex, assemblyName, methodName, parameters);
			}
			finally
			{
				HandleFinally();
			}
		}

		/// <summary>
		/// GetAuthenticationRequestFileByID auto generated wrapper
		/// </summary>
		public Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationFileDetails GetAuthenticationRequestFileByID(System.Int32 fileID)
		{
			const string methodName = "Customs.CRM.CertificateOfOrigins.Service.CertificateOfOriginsInternalService.GetAuthenticationRequestFileByID";
			try
			{
				InitializeService(methodName, fileID);
				Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationFileDetails result;
				result = InternalGetAuthenticationRequestFileByID(fileID);
				return result;
			}
			catch (System.Exception ex)
			{
				var parameters = new object[] {fileID};
				throw HandleException(ex, assemblyName, methodName, parameters);
			}
			finally
			{
				HandleFinally();
			}
		}

		/// <summary>
		/// SaveAuthenticationRequestFile auto generated wrapper
		/// </summary>
		public Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationFileDetails SaveAuthenticationRequestFile(Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationFileDetails authenticationRequestFile)
		{
			const string methodName = "Customs.CRM.CertificateOfOrigins.Service.CertificateOfOriginsInternalService.SaveAuthenticationRequestFile";
			try
			{
				InitializeService(methodName, authenticationRequestFile);
				Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationFileDetails result;
				result = InternalSaveAuthenticationRequestFile(authenticationRequestFile);
				return result;
			}
			catch (System.Exception ex)
			{
				var parameters = new object[] {authenticationRequestFile};
				throw HandleException(ex, assemblyName, methodName, parameters);
			}
			finally
			{
				HandleFinally();
			}
		}

		/// <summary>
		/// GetAuthenticationRequestByID auto generated wrapper
		/// </summary>
		public Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationRequest GetAuthenticationRequestByID(System.Int32 documentId)
		{
			const string methodName = "Customs.CRM.CertificateOfOrigins.Service.CertificateOfOriginsInternalService.GetAuthenticationRequestByID";
			try
			{
				InitializeService(methodName, documentId);
				Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationRequest result;
				result = InternalGetAuthenticationRequestByID(documentId);
				return result;
			}
			catch (System.Exception ex)
			{
				var parameters = new object[] {documentId};
				throw HandleException(ex, assemblyName, methodName, parameters);
			}
			finally
			{
				HandleFinally();
			}
		}

		/// <summary>
		/// GetAuthenticationRequestByLeadDocumentIDs auto generated wrapper
		/// </summary>
		public System.Collections.Generic.List<Customs.CRM.Entities.CertificateOfOriginsDTO.ImportAuthenticationRequestDTO> GetAuthenticationRequestByLeadDocumentIDs(System.Collections.Generic.List<System.Int32> leadDocumentIDs)
		{
			const string methodName = "Customs.CRM.CertificateOfOrigins.Service.CertificateOfOriginsInternalService.GetAuthenticationRequestByLeadDocumentIDs";
			try
			{
				InitializeService(methodName, leadDocumentIDs);
				System.Collections.Generic.List<Customs.CRM.Entities.CertificateOfOriginsDTO.ImportAuthenticationRequestDTO> result;
				result = InternalGetAuthenticationRequestByLeadDocumentIDs(leadDocumentIDs);
				return result;
			}
			catch (System.Exception ex)
			{
				var parameters = new object[] {leadDocumentIDs};
				throw HandleException(ex, assemblyName, methodName, parameters);
			}
			finally
			{
				HandleFinally();
			}
		}

		/// <summary>
		/// GetExportDocumentAuthenticationRequestSearch auto generated wrapper
		/// </summary>
		public System.Collections.Generic.List<Customs.CRM.Entities.Entities.ExportDocumentAuthenticationRequestSearch.ExportDocumentAuthenticationRequestSearchResult> GetExportDocumentAuthenticationRequestSearch(Customs.CRM.Entities.Entities.ExportDocumentAuthenticationRequestSearch.ExportDocumentAuthenticationRequestSearchFilter filter)
		{
			const string methodName = "Customs.CRM.CertificateOfOrigins.Service.CertificateOfOriginsInternalService.GetExportDocumentAuthenticationRequestSearch";
			try
			{
				InitializeService(methodName, filter);
				System.Collections.Generic.List<Customs.CRM.Entities.Entities.ExportDocumentAuthenticationRequestSearch.ExportDocumentAuthenticationRequestSearchResult> result;
				result = InternalGetExportDocumentAuthenticationRequestSearch(filter);
				return result;
			}
			catch (System.Exception ex)
			{
				var parameters = new object[] {filter};
				throw HandleException(ex, assemblyName, methodName, parameters);
			}
			finally
			{
				HandleFinally();
			}
		}

		/// <summary>
		/// GetExportDocumentAuthenticationRequestByID auto generated wrapper
		/// </summary>
		public Customs.CertificateOfOrigins.Entities.ExportDocumentAuthenticationRequest GetExportDocumentAuthenticationRequestByID(System.Int32 id)
		{
			const string methodName = "Customs.CRM.CertificateOfOrigins.Service.CertificateOfOriginsInternalService.GetExportDocumentAuthenticationRequestByID";
			try
			{
				InitializeService(methodName, id);
				Customs.CertificateOfOrigins.Entities.ExportDocumentAuthenticationRequest result;
				result = InternalGetExportDocumentAuthenticationRequestByID(id);
				return result;
			}
			catch (System.Exception ex)
			{
				var parameters = new object[] {id};
				throw HandleException(ex, assemblyName, methodName, parameters);
			}
			finally
			{
				HandleFinally();
			}
		}

		/// <summary>
		/// SaveExportDocumentAuthenticationRequest auto generated wrapper
		/// </summary>
		public Customs.CertificateOfOrigins.Entities.ExportDocumentAuthenticationRequest SaveExportDocumentAuthenticationRequest(Customs.CertificateOfOrigins.Entities.ExportDocumentAuthenticationRequest entity)
		{
			const string methodName = "Customs.CRM.CertificateOfOrigins.Service.CertificateOfOriginsInternalService.SaveExportDocumentAuthenticationRequest";
			try
			{
				InitializeService(methodName, entity);
				Customs.CertificateOfOrigins.Entities.ExportDocumentAuthenticationRequest result;
				result = InternalSaveExportDocumentAuthenticationRequest(entity);
				return result;
			}
			catch (System.Exception ex)
			{
				var parameters = new object[] {entity};
				throw HandleException(ex, assemblyName, methodName, parameters);
			}
			finally
			{
				HandleFinally();
			}
		}

		/// <summary>
		/// GetCustomerInformation auto generated wrapper
		/// </summary>
		public Customs.StockPileData.Customers.ExternalCommon.Common.CustomerDTO GetCustomerInformation(System.Int32 customerID)
		{
			const string methodName = "Customs.CRM.CertificateOfOrigins.Service.CertificateOfOriginsInternalService.GetCustomerInformation";
			try
			{
				InitializeService(methodName, customerID);
				Customs.StockPileData.Customers.ExternalCommon.Common.CustomerDTO result;
				result = InternalGetCustomerInformation(customerID);
				return result;
			}
			catch (System.Exception ex)
			{
				var parameters = new object[] {customerID};
				throw HandleException(ex, assemblyName, methodName, parameters);
			}
			finally
			{
				HandleFinally();
			}
		}

		/// <summary>
		/// GetCustomerInformationByCountry auto generated wrapper
		/// </summary>
		public Customs.StockPileData.Customers.ExternalCommon.Common.CustomerDTO GetCustomerInformationByCountry(System.Int32 countryID)
		{
			const string methodName = "Customs.CRM.CertificateOfOrigins.Service.CertificateOfOriginsInternalService.GetCustomerInformationByCountry";
			try
			{
				InitializeService(methodName, countryID);
				Customs.StockPileData.Customers.ExternalCommon.Common.CustomerDTO result;
				result = InternalGetCustomerInformationByCountry(countryID);
				return result;
			}
			catch (System.Exception ex)
			{
				var parameters = new object[] {countryID};
				throw HandleException(ex, assemblyName, methodName, parameters);
			}
			finally
			{
				HandleFinally();
			}
		}

		/// <summary>
		/// HandleImportAuthenticationRequestDeliveryForImporterSent auto generated wrapper
		/// </summary>
		public Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationRequest HandleImportAuthenticationRequestDeliveryForImporterSent(Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationRequest authenticationRequest)
		{
			const string methodName = "Customs.CRM.CertificateOfOrigins.Service.CertificateOfOriginsInternalService.HandleImportAuthenticationRequestDeliveryForImporterSent";
			try
			{
				InitializeService(methodName, authenticationRequest);
				Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationRequest result;
				result = InternalHandleImportAuthenticationRequestDeliveryForImporterSent(authenticationRequest);
				return result;
			}
			catch (System.Exception ex)
			{
				var parameters = new object[] {authenticationRequest};
				throw HandleException(ex, assemblyName, methodName, parameters);
			}
			finally
			{
				HandleFinally();
			}
		}

		/// <summary>
		/// HandleImportAuthenticationRequestDeliveryReminderForImporterSent auto generated wrapper
		/// </summary>
		public Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationRequest HandleImportAuthenticationRequestDeliveryReminderForImporterSent(Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationRequest authenticationRequest)
		{
			const string methodName = "Customs.CRM.CertificateOfOrigins.Service.CertificateOfOriginsInternalService.HandleImportAuthenticationRequestDeliveryReminderForImporterSent";
			try
			{
				InitializeService(methodName, authenticationRequest);
				Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationRequest result;
				result = InternalHandleImportAuthenticationRequestDeliveryReminderForImporterSent(authenticationRequest);
				return result;
			}
			catch (System.Exception ex)
			{
				var parameters = new object[] {authenticationRequest};
				throw HandleException(ex, assemblyName, methodName, parameters);
			}
			finally
			{
				HandleFinally();
			}
		}

		/// <summary>
		/// HandleImportAuthenticationRequestDeliveryAndReminderForVendorSent auto generated wrapper
		/// </summary>
		public Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationFileDetails HandleImportAuthenticationRequestDeliveryAndReminderForVendorSent(Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationFileDetails authenticationFile,System.Boolean isDelivery)
		{
			const string methodName = "Customs.CRM.CertificateOfOrigins.Service.CertificateOfOriginsInternalService.HandleImportAuthenticationRequestDeliveryAndReminderForVendorSent";
			try
			{
				InitializeService(methodName, authenticationFile,isDelivery);
				Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationFileDetails result;
				result = InternalHandleImportAuthenticationRequestDeliveryAndReminderForVendorSent(authenticationFile,isDelivery);
				return result;
			}
			catch (System.Exception ex)
			{
				var parameters = new object[] {authenticationFile,isDelivery};
				throw HandleException(ex, assemblyName, methodName, parameters);
			}
			finally
			{
				HandleFinally();
			}
		}

		/// <summary>
		/// CheckIfExistsAdditionalRequestsForImporter auto generated wrapper
		/// </summary>
		public System.Boolean CheckIfExistsAdditionalRequestsForImporter(Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationRequest request)
		{
			const string methodName = "Customs.CRM.CertificateOfOrigins.Service.CertificateOfOriginsInternalService.CheckIfExistsAdditionalRequestsForImporter";
			try
			{
				InitializeService(methodName, request);
				System.Boolean result;
				result = InternalCheckIfExistsAdditionalRequestsForImporter(request);
				return result;
			}
			catch (System.Exception ex)
			{
				var parameters = new object[] {request};
				throw HandleException(ex, assemblyName, methodName, parameters);
			}
			finally
			{
				HandleFinally();
			}
		}

		/// <summary>
		/// CheckIfExistsAdditionalRequestsForVendor auto generated wrapper
		/// </summary>
		public System.Boolean CheckIfExistsAdditionalRequestsForVendor(System.Int32 vendorID)
		{
			const string methodName = "Customs.CRM.CertificateOfOrigins.Service.CertificateOfOriginsInternalService.CheckIfExistsAdditionalRequestsForVendor";
			try
			{
				InitializeService(methodName, vendorID);
				System.Boolean result;
				result = InternalCheckIfExistsAdditionalRequestsForVendor(vendorID);
				return result;
			}
			catch (System.Exception ex)
			{
				var parameters = new object[] {vendorID};
				throw HandleException(ex, assemblyName, methodName, parameters);
			}
			finally
			{
				HandleFinally();
			}
		}

		/// <summary>
		/// GetPathsForNavigationToVendor auto generated wrapper
		/// </summary>
		public Customs.CRM.External.NavigationToVendorView GetPathsForNavigationToVendor()
		{
			const string methodName = "Customs.CRM.CertificateOfOrigins.Service.CertificateOfOriginsInternalService.GetPathsForNavigationToVendor";
			try
			{
				InitializeService(methodName);
				Customs.CRM.External.NavigationToVendorView result;
				result = InternalGetPathsForNavigationToVendor();
				return result;
			}
			catch (System.Exception ex)
			{
				var parameters = new object[] {};
				throw HandleException(ex, assemblyName, methodName, parameters);
			}
			finally
			{
				HandleFinally();
			}
		}

		/// <summary>
		/// HandleSendRemindDeliverNotification auto generated wrapper
		/// </summary>
		public System.Boolean HandleSendRemindDeliverNotification(Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationFileDetails file)
		{
			const string methodName = "Customs.CRM.CertificateOfOrigins.Service.CertificateOfOriginsInternalService.HandleSendRemindDeliverNotification";
			try
			{
				InitializeService(methodName, file);
				System.Boolean result;
				result = InternalHandleSendRemindDeliverNotification(file);
				return result;
			}
			catch (System.Exception ex)
			{
				var parameters = new object[] {file};
				throw HandleException(ex, assemblyName, methodName, parameters);
			}
			finally
			{
				HandleFinally();
			}
		}

		/// <summary>
		/// ChangeStatusAfterDeliverySent auto generated wrapper
		/// </summary>
		public Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationFileDetails ChangeStatusAfterDeliverySent(Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationFileDetails importAuthenticationRequest)
		{
			const string methodName = "Customs.CRM.CertificateOfOrigins.Service.CertificateOfOriginsInternalService.ChangeStatusAfterDeliverySent";
			try
			{
				InitializeService(methodName, importAuthenticationRequest);
				Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationFileDetails result;
				result = InternalChangeStatusAfterDeliverySent(importAuthenticationRequest);
				return result;
			}
			catch (System.Exception ex)
			{
				var parameters = new object[] {importAuthenticationRequest};
				throw HandleException(ex, assemblyName, methodName, parameters);
			}
			finally
			{
				HandleFinally();
			}
		}

		/// <summary>
		/// CheckImporterOfImportAuthentication auto generated wrapper
		/// </summary>
		public System.Nullable<System.Int32> CheckImporterOfImportAuthentication(System.Int32 importerId)
		{
			const string methodName = "Customs.CRM.CertificateOfOrigins.Service.CertificateOfOriginsInternalService.CheckImporterOfImportAuthentication";
			try
			{
				InitializeService(methodName, importerId);
				System.Nullable<System.Int32> result;
				result = InternalCheckImporterOfImportAuthentication(importerId);
				return result;
			}
			catch (System.Exception ex)
			{
				var parameters = new object[] {importerId};
				throw HandleException(ex, assemblyName, methodName, parameters);
			}
			finally
			{
				HandleFinally();
			}
		}

		/// <summary>
		/// LoadDataFromExportDeclaration auto generated wrapper
		/// </summary>
		public System.Boolean LoadDataFromExportDeclaration(Customs.CertificateOfOrigins.Entities.CertificateOfOrigin certificateOfOrigin)
		{
			const string methodName = "Customs.CRM.CertificateOfOrigins.Service.CertificateOfOriginsInternalService.LoadDataFromExportDeclaration";
			try
			{
				InitializeService(methodName, certificateOfOrigin);
				System.Boolean result;
				result = InternalLoadDataFromExportDeclaration(certificateOfOrigin);
				return result;
			}
			catch (System.Exception ex)
			{
				var parameters = new object[] {certificateOfOrigin};
				throw HandleException(ex, assemblyName, methodName, parameters);
			}
			finally
			{
				HandleFinally();
			}
		}
	}

	interface IInternalCertificateOfOriginsInternalService
	{
		/// <summary>
		/// GetCertificateOfOriginsByFilter auto generated wrapper
		/// </summary>
		System.Collections.Generic.List<Customs.CRM.CertificateOfOrigins.InternalCommon.Common.CertificateOfOriginResult> InternalGetCertificateOfOriginsByFilter(Customs.CRM.CertificateOfOrigins.InternalCommon.Common.CertificateOfOriginFilter filter);
		/// <summary>
		/// IsCertificateOfOriginByExternalIdExist auto generated wrapper
		/// </summary>
		Customs.CRM.CertificateOfOrigins.InternalCommon.Common.CertificateOfOriginResult InternalIsCertificateOfOriginByExternalIdExist(System.String certificateOfOriginExternalId);
		/// <summary>
		/// GetCertificateOfOriginById auto generated wrapper
		/// </summary>
		Customs.CertificateOfOrigins.Entities.CertificateOfOrigin InternalGetCertificateOfOriginById(System.Int32 certificateOfOriginId);
		/// <summary>
		/// SaveCertificateOfOrigin auto generated wrapper
		/// </summary>
		Customs.CertificateOfOrigins.Entities.CertificateOfOrigin InternalSaveCertificateOfOrigin(Customs.CertificateOfOrigins.Entities.CertificateOfOrigin certificateOfOrigin);
		/// <summary>
		/// SaveImportAuthenticationRequest auto generated wrapper
		/// </summary>
		Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationRequest InternalSaveImportAuthenticationRequest(Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationRequest importAuthenticationRequest);
		/// <summary>
		/// GetAuthenticationRequestByFilter auto generated wrapper
		/// </summary>
		System.Collections.Generic.List<Customs.CertificateOfOrigins.Entities.GetImportAuthenticationRequestResult> InternalGetAuthenticationRequestByFilter(Customs.CertificateOfOrigins.Entities.ImportAuthenticationRequestFilter filter);
		/// <summary>
		/// GetEntityDocuments auto generated wrapper
		/// </summary>
		System.Collections.Generic.List<Customs.Infrastructure.DocumentManagement.ExternalCommon.DocumentDTO> InternalGetEntityDocuments(Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationRequest importAuthenticationRequest);
		/// <summary>
		/// CreateNewAuthenticationFile auto generated wrapper
		/// </summary>
		Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationFileDetails InternalCreateNewAuthenticationFile(System.Collections.Generic.List<Customs.CertificateOfOrigins.Entities.GetImportAuthenticationRequestResult> importAuthenticationRequests);
		/// <summary>
		/// GetAuthenticationRequestFileByID auto generated wrapper
		/// </summary>
		Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationFileDetails InternalGetAuthenticationRequestFileByID(System.Int32 fileID);
		/// <summary>
		/// SaveAuthenticationRequestFile auto generated wrapper
		/// </summary>
		Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationFileDetails InternalSaveAuthenticationRequestFile(Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationFileDetails authenticationRequestFile);
		/// <summary>
		/// GetAuthenticationRequestByID auto generated wrapper
		/// </summary>
		Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationRequest InternalGetAuthenticationRequestByID(System.Int32 documentId);
		/// <summary>
		/// GetAuthenticationRequestByLeadDocumentIDs auto generated wrapper
		/// </summary>
		System.Collections.Generic.List<Customs.CRM.Entities.CertificateOfOriginsDTO.ImportAuthenticationRequestDTO> InternalGetAuthenticationRequestByLeadDocumentIDs(System.Collections.Generic.List<System.Int32> leadDocumentIDs);
		/// <summary>
		/// GetExportDocumentAuthenticationRequestSearch auto generated wrapper
		/// </summary>
		System.Collections.Generic.List<Customs.CRM.Entities.Entities.ExportDocumentAuthenticationRequestSearch.ExportDocumentAuthenticationRequestSearchResult> InternalGetExportDocumentAuthenticationRequestSearch(Customs.CRM.Entities.Entities.ExportDocumentAuthenticationRequestSearch.ExportDocumentAuthenticationRequestSearchFilter filter);
		/// <summary>
		/// GetExportDocumentAuthenticationRequestByID auto generated wrapper
		/// </summary>
		Customs.CertificateOfOrigins.Entities.ExportDocumentAuthenticationRequest InternalGetExportDocumentAuthenticationRequestByID(System.Int32 id);
		/// <summary>
		/// SaveExportDocumentAuthenticationRequest auto generated wrapper
		/// </summary>
		Customs.CertificateOfOrigins.Entities.ExportDocumentAuthenticationRequest InternalSaveExportDocumentAuthenticationRequest(Customs.CertificateOfOrigins.Entities.ExportDocumentAuthenticationRequest entity);
		/// <summary>
		/// GetCustomerInformation auto generated wrapper
		/// </summary>
		Customs.StockPileData.Customers.ExternalCommon.Common.CustomerDTO InternalGetCustomerInformation(System.Int32 customerID);
		/// <summary>
		/// GetCustomerInformationByCountry auto generated wrapper
		/// </summary>
		Customs.StockPileData.Customers.ExternalCommon.Common.CustomerDTO InternalGetCustomerInformationByCountry(System.Int32 countryID);
		/// <summary>
		/// HandleImportAuthenticationRequestDeliveryForImporterSent auto generated wrapper
		/// </summary>
		Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationRequest InternalHandleImportAuthenticationRequestDeliveryForImporterSent(Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationRequest authenticationRequest);
		/// <summary>
		/// HandleImportAuthenticationRequestDeliveryReminderForImporterSent auto generated wrapper
		/// </summary>
		Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationRequest InternalHandleImportAuthenticationRequestDeliveryReminderForImporterSent(Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationRequest authenticationRequest);
		/// <summary>
		/// HandleImportAuthenticationRequestDeliveryAndReminderForVendorSent auto generated wrapper
		/// </summary>
		Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationFileDetails InternalHandleImportAuthenticationRequestDeliveryAndReminderForVendorSent(Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationFileDetails authenticationFile,System.Boolean isDelivery);
		/// <summary>
		/// CheckIfExistsAdditionalRequestsForImporter auto generated wrapper
		/// </summary>
		System.Boolean InternalCheckIfExistsAdditionalRequestsForImporter(Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationRequest request);
		/// <summary>
		/// CheckIfExistsAdditionalRequestsForVendor auto generated wrapper
		/// </summary>
		System.Boolean InternalCheckIfExistsAdditionalRequestsForVendor(System.Int32 vendorID);
		/// <summary>
		/// GetPathsForNavigationToVendor auto generated wrapper
		/// </summary>
		Customs.CRM.External.NavigationToVendorView InternalGetPathsForNavigationToVendor();
		/// <summary>
		/// HandleSendRemindDeliverNotification auto generated wrapper
		/// </summary>
		System.Boolean InternalHandleSendRemindDeliverNotification(Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationFileDetails file);
		/// <summary>
		/// ChangeStatusAfterDeliverySent auto generated wrapper
		/// </summary>
		Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationFileDetails InternalChangeStatusAfterDeliverySent(Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationFileDetails importAuthenticationRequest);
		/// <summary>
		/// CheckImporterOfImportAuthentication auto generated wrapper
		/// </summary>
		System.Nullable<System.Int32> InternalCheckImporterOfImportAuthentication(System.Int32 importerId);
		/// <summary>
		/// LoadDataFromExportDeclaration auto generated wrapper
		/// </summary>
		System.Boolean InternalLoadDataFromExportDeclaration(Customs.CertificateOfOrigins.Entities.CertificateOfOrigin certificateOfOrigin);
	}

	/// <summary>
	/// Direct proxy instance (refers directly to service)
	/// </summary>
	public partial class CertificateOfOriginsInternalDirectProxyInstance : ICertificateOfOriginsInternalProxy
	{
		/// <summary>
		/// Ctor
		/// </summary>
		/// <summary>
		/// GetCertificateOfOriginsByFilter auto generated wrapper
		/// </summary>
		public string GetCertificateOfOriginsByFilter(EventHandler<CallbackEventArgs<System.Collections.Generic.List<Customs.CRM.CertificateOfOrigins.InternalCommon.Common.CertificateOfOriginResult>>> clientCallback,Customs.CRM.CertificateOfOrigins.InternalCommon.Common.CertificateOfOriginFilter filter, object stateObject = null)
		{
			var service = new CertificateOfOriginsInternalService();
			var result = service.InternalGetCertificateOfOriginsByFilter(filter);
			var callbackArgs = new CallbackEventArgs<System.Collections.Generic.List<Customs.CRM.CertificateOfOrigins.InternalCommon.Common.CertificateOfOriginResult>>(string.Empty, string.Empty,stateObject, new object[] { filter });
			callbackArgs.SetResult(result);
			clientCallback.Invoke(this, callbackArgs);
			return string.Empty;
		}
		/// <summary>
		/// IsCertificateOfOriginByExternalIdExist auto generated wrapper
		/// </summary>
		public string IsCertificateOfOriginByExternalIdExist(EventHandler<CallbackEventArgs<Customs.CRM.CertificateOfOrigins.InternalCommon.Common.CertificateOfOriginResult>> clientCallback,System.String certificateOfOriginExternalId, object stateObject = null)
		{
			var service = new CertificateOfOriginsInternalService();
			var result = service.InternalIsCertificateOfOriginByExternalIdExist(certificateOfOriginExternalId);
			var callbackArgs = new CallbackEventArgs<Customs.CRM.CertificateOfOrigins.InternalCommon.Common.CertificateOfOriginResult>(string.Empty, string.Empty,stateObject, new object[] { certificateOfOriginExternalId });
			callbackArgs.SetResult(result);
			clientCallback.Invoke(this, callbackArgs);
			return string.Empty;
		}
		/// <summary>
		/// GetCertificateOfOriginById auto generated wrapper
		/// </summary>
		public string GetCertificateOfOriginById(EventHandler<CallbackEventArgs<Customs.CertificateOfOrigins.Entities.CertificateOfOrigin>> clientCallback,System.Int32 certificateOfOriginId, object stateObject = null)
		{
			var service = new CertificateOfOriginsInternalService();
			var result = service.InternalGetCertificateOfOriginById(certificateOfOriginId);
			var callbackArgs = new CallbackEventArgs<Customs.CertificateOfOrigins.Entities.CertificateOfOrigin>(string.Empty, string.Empty,stateObject, new object[] { certificateOfOriginId });
			callbackArgs.SetResult(result);
			clientCallback.Invoke(this, callbackArgs);
			return string.Empty;
		}
		/// <summary>
		/// SaveCertificateOfOrigin auto generated wrapper
		/// </summary>
		public string SaveCertificateOfOrigin(EventHandler<CallbackEventArgs<Customs.CertificateOfOrigins.Entities.CertificateOfOrigin>> clientCallback,Customs.CertificateOfOrigins.Entities.CertificateOfOrigin certificateOfOrigin, object stateObject = null)
		{
			var service = new CertificateOfOriginsInternalService();
			var result = service.InternalSaveCertificateOfOrigin(certificateOfOrigin);
			var callbackArgs = new CallbackEventArgs<Customs.CertificateOfOrigins.Entities.CertificateOfOrigin>(string.Empty, string.Empty,stateObject, new object[] { certificateOfOrigin });
			callbackArgs.SetResult(result);
			clientCallback.Invoke(this, callbackArgs);
			return string.Empty;
		}
		/// <summary>
		/// SaveImportAuthenticationRequest auto generated wrapper
		/// </summary>
		public string SaveImportAuthenticationRequest(EventHandler<CallbackEventArgs<Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationRequest>> clientCallback,Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationRequest importAuthenticationRequest, object stateObject = null)
		{
			var service = new CertificateOfOriginsInternalService();
			var result = service.InternalSaveImportAuthenticationRequest(importAuthenticationRequest);
			var callbackArgs = new CallbackEventArgs<Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationRequest>(string.Empty, string.Empty,stateObject, new object[] { importAuthenticationRequest });
			callbackArgs.SetResult(result);
			clientCallback.Invoke(this, callbackArgs);
			return string.Empty;
		}
		/// <summary>
		/// GetAuthenticationRequestByFilter auto generated wrapper
		/// </summary>
		public string GetAuthenticationRequestByFilter(EventHandler<CallbackEventArgs<System.Collections.Generic.List<Customs.CertificateOfOrigins.Entities.GetImportAuthenticationRequestResult>>> clientCallback,Customs.CertificateOfOrigins.Entities.ImportAuthenticationRequestFilter filter, object stateObject = null)
		{
			var service = new CertificateOfOriginsInternalService();
			var result = service.InternalGetAuthenticationRequestByFilter(filter);
			var callbackArgs = new CallbackEventArgs<System.Collections.Generic.List<Customs.CertificateOfOrigins.Entities.GetImportAuthenticationRequestResult>>(string.Empty, string.Empty,stateObject, new object[] { filter });
			callbackArgs.SetResult(result);
			clientCallback.Invoke(this, callbackArgs);
			return string.Empty;
		}
		/// <summary>
		/// GetEntityDocuments auto generated wrapper
		/// </summary>
		public string GetEntityDocuments(EventHandler<CallbackEventArgs<System.Collections.Generic.List<Customs.Infrastructure.DocumentManagement.ExternalCommon.DocumentDTO>>> clientCallback,Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationRequest importAuthenticationRequest, object stateObject = null)
		{
			var service = new CertificateOfOriginsInternalService();
			var result = service.InternalGetEntityDocuments(importAuthenticationRequest);
			var callbackArgs = new CallbackEventArgs<System.Collections.Generic.List<Customs.Infrastructure.DocumentManagement.ExternalCommon.DocumentDTO>>(string.Empty, string.Empty,stateObject, new object[] { importAuthenticationRequest });
			callbackArgs.SetResult(result);
			clientCallback.Invoke(this, callbackArgs);
			return string.Empty;
		}
		/// <summary>
		/// CreateNewAuthenticationFile auto generated wrapper
		/// </summary>
		public string CreateNewAuthenticationFile(EventHandler<CallbackEventArgs<Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationFileDetails>> clientCallback,System.Collections.Generic.List<Customs.CertificateOfOrigins.Entities.GetImportAuthenticationRequestResult> importAuthenticationRequests, object stateObject = null)
		{
			var service = new CertificateOfOriginsInternalService();
			var result = service.InternalCreateNewAuthenticationFile(importAuthenticationRequests);
			var callbackArgs = new CallbackEventArgs<Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationFileDetails>(string.Empty, string.Empty,stateObject, new object[] { importAuthenticationRequests });
			callbackArgs.SetResult(result);
			clientCallback.Invoke(this, callbackArgs);
			return string.Empty;
		}
		/// <summary>
		/// GetAuthenticationRequestFileByID auto generated wrapper
		/// </summary>
		public string GetAuthenticationRequestFileByID(EventHandler<CallbackEventArgs<Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationFileDetails>> clientCallback,System.Int32 fileID, object stateObject = null)
		{
			var service = new CertificateOfOriginsInternalService();
			var result = service.InternalGetAuthenticationRequestFileByID(fileID);
			var callbackArgs = new CallbackEventArgs<Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationFileDetails>(string.Empty, string.Empty,stateObject, new object[] { fileID });
			callbackArgs.SetResult(result);
			clientCallback.Invoke(this, callbackArgs);
			return string.Empty;
		}
		/// <summary>
		/// SaveAuthenticationRequestFile auto generated wrapper
		/// </summary>
		public string SaveAuthenticationRequestFile(EventHandler<CallbackEventArgs<Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationFileDetails>> clientCallback,Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationFileDetails authenticationRequestFile, object stateObject = null)
		{
			var service = new CertificateOfOriginsInternalService();
			var result = service.InternalSaveAuthenticationRequestFile(authenticationRequestFile);
			var callbackArgs = new CallbackEventArgs<Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationFileDetails>(string.Empty, string.Empty,stateObject, new object[] { authenticationRequestFile });
			callbackArgs.SetResult(result);
			clientCallback.Invoke(this, callbackArgs);
			return string.Empty;
		}
		/// <summary>
		/// GetAuthenticationRequestByID auto generated wrapper
		/// </summary>
		public string GetAuthenticationRequestByID(EventHandler<CallbackEventArgs<Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationRequest>> clientCallback,System.Int32 documentId, object stateObject = null)
		{
			var service = new CertificateOfOriginsInternalService();
			var result = service.InternalGetAuthenticationRequestByID(documentId);
			var callbackArgs = new CallbackEventArgs<Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationRequest>(string.Empty, string.Empty,stateObject, new object[] { documentId });
			callbackArgs.SetResult(result);
			clientCallback.Invoke(this, callbackArgs);
			return string.Empty;
		}
		/// <summary>
		/// GetAuthenticationRequestByLeadDocumentIDs auto generated wrapper
		/// </summary>
		public string GetAuthenticationRequestByLeadDocumentIDs(EventHandler<CallbackEventArgs<System.Collections.Generic.List<Customs.CRM.Entities.CertificateOfOriginsDTO.ImportAuthenticationRequestDTO>>> clientCallback,System.Collections.Generic.List<System.Int32> leadDocumentIDs, object stateObject = null)
		{
			var service = new CertificateOfOriginsInternalService();
			var result = service.InternalGetAuthenticationRequestByLeadDocumentIDs(leadDocumentIDs);
			var callbackArgs = new CallbackEventArgs<System.Collections.Generic.List<Customs.CRM.Entities.CertificateOfOriginsDTO.ImportAuthenticationRequestDTO>>(string.Empty, string.Empty,stateObject, new object[] { leadDocumentIDs });
			callbackArgs.SetResult(result);
			clientCallback.Invoke(this, callbackArgs);
			return string.Empty;
		}
		/// <summary>
		/// GetExportDocumentAuthenticationRequestSearch auto generated wrapper
		/// </summary>
		public string GetExportDocumentAuthenticationRequestSearch(EventHandler<CallbackEventArgs<System.Collections.Generic.List<Customs.CRM.Entities.Entities.ExportDocumentAuthenticationRequestSearch.ExportDocumentAuthenticationRequestSearchResult>>> clientCallback,Customs.CRM.Entities.Entities.ExportDocumentAuthenticationRequestSearch.ExportDocumentAuthenticationRequestSearchFilter filter, object stateObject = null)
		{
			var service = new CertificateOfOriginsInternalService();
			var result = service.InternalGetExportDocumentAuthenticationRequestSearch(filter);
			var callbackArgs = new CallbackEventArgs<System.Collections.Generic.List<Customs.CRM.Entities.Entities.ExportDocumentAuthenticationRequestSearch.ExportDocumentAuthenticationRequestSearchResult>>(string.Empty, string.Empty,stateObject, new object[] { filter });
			callbackArgs.SetResult(result);
			clientCallback.Invoke(this, callbackArgs);
			return string.Empty;
		}
		/// <summary>
		/// GetExportDocumentAuthenticationRequestByID auto generated wrapper
		/// </summary>
		public string GetExportDocumentAuthenticationRequestByID(EventHandler<CallbackEventArgs<Customs.CertificateOfOrigins.Entities.ExportDocumentAuthenticationRequest>> clientCallback,System.Int32 id, object stateObject = null)
		{
			var service = new CertificateOfOriginsInternalService();
			var result = service.InternalGetExportDocumentAuthenticationRequestByID(id);
			var callbackArgs = new CallbackEventArgs<Customs.CertificateOfOrigins.Entities.ExportDocumentAuthenticationRequest>(string.Empty, string.Empty,stateObject, new object[] { id });
			callbackArgs.SetResult(result);
			clientCallback.Invoke(this, callbackArgs);
			return string.Empty;
		}
		/// <summary>
		/// SaveExportDocumentAuthenticationRequest auto generated wrapper
		/// </summary>
		public string SaveExportDocumentAuthenticationRequest(EventHandler<CallbackEventArgs<Customs.CertificateOfOrigins.Entities.ExportDocumentAuthenticationRequest>> clientCallback,Customs.CertificateOfOrigins.Entities.ExportDocumentAuthenticationRequest entity, object stateObject = null)
		{
			var service = new CertificateOfOriginsInternalService();
			var result = service.InternalSaveExportDocumentAuthenticationRequest(entity);
			var callbackArgs = new CallbackEventArgs<Customs.CertificateOfOrigins.Entities.ExportDocumentAuthenticationRequest>(string.Empty, string.Empty,stateObject, new object[] { entity });
			callbackArgs.SetResult(result);
			clientCallback.Invoke(this, callbackArgs);
			return string.Empty;
		}
		/// <summary>
		/// GetCustomerInformation auto generated wrapper
		/// </summary>
		public string GetCustomerInformation(EventHandler<CallbackEventArgs<Customs.StockPileData.Customers.ExternalCommon.Common.CustomerDTO>> clientCallback,System.Int32 customerID, object stateObject = null)
		{
			var service = new CertificateOfOriginsInternalService();
			var result = service.InternalGetCustomerInformation(customerID);
			var callbackArgs = new CallbackEventArgs<Customs.StockPileData.Customers.ExternalCommon.Common.CustomerDTO>(string.Empty, string.Empty,stateObject, new object[] { customerID });
			callbackArgs.SetResult(result);
			clientCallback.Invoke(this, callbackArgs);
			return string.Empty;
		}
		/// <summary>
		/// GetCustomerInformationByCountry auto generated wrapper
		/// </summary>
		public string GetCustomerInformationByCountry(EventHandler<CallbackEventArgs<Customs.StockPileData.Customers.ExternalCommon.Common.CustomerDTO>> clientCallback,System.Int32 countryID, object stateObject = null)
		{
			var service = new CertificateOfOriginsInternalService();
			var result = service.InternalGetCustomerInformationByCountry(countryID);
			var callbackArgs = new CallbackEventArgs<Customs.StockPileData.Customers.ExternalCommon.Common.CustomerDTO>(string.Empty, string.Empty,stateObject, new object[] { countryID });
			callbackArgs.SetResult(result);
			clientCallback.Invoke(this, callbackArgs);
			return string.Empty;
		}
		/// <summary>
		/// HandleImportAuthenticationRequestDeliveryForImporterSent auto generated wrapper
		/// </summary>
		public string HandleImportAuthenticationRequestDeliveryForImporterSent(EventHandler<CallbackEventArgs<Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationRequest>> clientCallback,Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationRequest authenticationRequest, object stateObject = null)
		{
			var service = new CertificateOfOriginsInternalService();
			var result = service.InternalHandleImportAuthenticationRequestDeliveryForImporterSent(authenticationRequest);
			var callbackArgs = new CallbackEventArgs<Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationRequest>(string.Empty, string.Empty,stateObject, new object[] { authenticationRequest });
			callbackArgs.SetResult(result);
			clientCallback.Invoke(this, callbackArgs);
			return string.Empty;
		}
		/// <summary>
		/// HandleImportAuthenticationRequestDeliveryReminderForImporterSent auto generated wrapper
		/// </summary>
		public string HandleImportAuthenticationRequestDeliveryReminderForImporterSent(EventHandler<CallbackEventArgs<Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationRequest>> clientCallback,Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationRequest authenticationRequest, object stateObject = null)
		{
			var service = new CertificateOfOriginsInternalService();
			var result = service.InternalHandleImportAuthenticationRequestDeliveryReminderForImporterSent(authenticationRequest);
			var callbackArgs = new CallbackEventArgs<Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationRequest>(string.Empty, string.Empty,stateObject, new object[] { authenticationRequest });
			callbackArgs.SetResult(result);
			clientCallback.Invoke(this, callbackArgs);
			return string.Empty;
		}
		/// <summary>
		/// HandleImportAuthenticationRequestDeliveryAndReminderForVendorSent auto generated wrapper
		/// </summary>
		public string HandleImportAuthenticationRequestDeliveryAndReminderForVendorSent(EventHandler<CallbackEventArgs<Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationFileDetails>> clientCallback,Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationFileDetails authenticationFile,System.Boolean isDelivery, object stateObject = null)
		{
			var service = new CertificateOfOriginsInternalService();
			var result = service.InternalHandleImportAuthenticationRequestDeliveryAndReminderForVendorSent(authenticationFile,isDelivery);
			var callbackArgs = new CallbackEventArgs<Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationFileDetails>(string.Empty, string.Empty,stateObject, new object[] { authenticationFile,isDelivery });
			callbackArgs.SetResult(result);
			clientCallback.Invoke(this, callbackArgs);
			return string.Empty;
		}
		/// <summary>
		/// CheckIfExistsAdditionalRequestsForImporter auto generated wrapper
		/// </summary>
		public string CheckIfExistsAdditionalRequestsForImporter(EventHandler<CallbackEventArgs<System.Boolean>> clientCallback,Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationRequest request, object stateObject = null)
		{
			var service = new CertificateOfOriginsInternalService();
			var result = service.InternalCheckIfExistsAdditionalRequestsForImporter(request);
			var callbackArgs = new CallbackEventArgs<System.Boolean>(string.Empty, string.Empty,stateObject, new object[] { request });
			callbackArgs.SetResult(result);
			clientCallback.Invoke(this, callbackArgs);
			return string.Empty;
		}
		/// <summary>
		/// CheckIfExistsAdditionalRequestsForVendor auto generated wrapper
		/// </summary>
		public string CheckIfExistsAdditionalRequestsForVendor(EventHandler<CallbackEventArgs<System.Boolean>> clientCallback,System.Int32 vendorID, object stateObject = null)
		{
			var service = new CertificateOfOriginsInternalService();
			var result = service.InternalCheckIfExistsAdditionalRequestsForVendor(vendorID);
			var callbackArgs = new CallbackEventArgs<System.Boolean>(string.Empty, string.Empty,stateObject, new object[] { vendorID });
			callbackArgs.SetResult(result);
			clientCallback.Invoke(this, callbackArgs);
			return string.Empty;
		}
		/// <summary>
		/// GetPathsForNavigationToVendor auto generated wrapper
		/// </summary>
		public string GetPathsForNavigationToVendor(EventHandler<CallbackEventArgs<Customs.CRM.External.NavigationToVendorView>> clientCallback, object stateObject = null)
		{
			var service = new CertificateOfOriginsInternalService();
			var result = service.InternalGetPathsForNavigationToVendor();
			var callbackArgs = new CallbackEventArgs<Customs.CRM.External.NavigationToVendorView>(string.Empty, string.Empty,stateObject, new object[] {  });
			callbackArgs.SetResult(result);
			clientCallback.Invoke(this, callbackArgs);
			return string.Empty;
		}
		/// <summary>
		/// HandleSendRemindDeliverNotification auto generated wrapper
		/// </summary>
		public string HandleSendRemindDeliverNotification(EventHandler<CallbackEventArgs<System.Boolean>> clientCallback,Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationFileDetails file, object stateObject = null)
		{
			var service = new CertificateOfOriginsInternalService();
			var result = service.InternalHandleSendRemindDeliverNotification(file);
			var callbackArgs = new CallbackEventArgs<System.Boolean>(string.Empty, string.Empty,stateObject, new object[] { file });
			callbackArgs.SetResult(result);
			clientCallback.Invoke(this, callbackArgs);
			return string.Empty;
		}
		/// <summary>
		/// ChangeStatusAfterDeliverySent auto generated wrapper
		/// </summary>
		public string ChangeStatusAfterDeliverySent(EventHandler<CallbackEventArgs<Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationFileDetails>> clientCallback,Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationFileDetails importAuthenticationRequest, object stateObject = null)
		{
			var service = new CertificateOfOriginsInternalService();
			var result = service.InternalChangeStatusAfterDeliverySent(importAuthenticationRequest);
			var callbackArgs = new CallbackEventArgs<Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationFileDetails>(string.Empty, string.Empty,stateObject, new object[] { importAuthenticationRequest });
			callbackArgs.SetResult(result);
			clientCallback.Invoke(this, callbackArgs);
			return string.Empty;
		}
		/// <summary>
		/// CheckImporterOfImportAuthentication auto generated wrapper
		/// </summary>
		public string CheckImporterOfImportAuthentication(EventHandler<CallbackEventArgs<System.Nullable<System.Int32>>> clientCallback,System.Int32 importerId, object stateObject = null)
		{
			var service = new CertificateOfOriginsInternalService();
			var result = service.InternalCheckImporterOfImportAuthentication(importerId);
			var callbackArgs = new CallbackEventArgs<System.Nullable<System.Int32>>(string.Empty, string.Empty,stateObject, new object[] { importerId });
			callbackArgs.SetResult(result);
			clientCallback.Invoke(this, callbackArgs);
			return string.Empty;
		}
		/// <summary>
		/// LoadDataFromExportDeclaration auto generated wrapper
		/// </summary>
		public string LoadDataFromExportDeclaration(EventHandler<CallbackEventArgs<System.Boolean>> clientCallback,Customs.CertificateOfOrigins.Entities.CertificateOfOrigin certificateOfOrigin, object stateObject = null)
		{
			var service = new CertificateOfOriginsInternalService();
			var result = service.InternalLoadDataFromExportDeclaration(certificateOfOrigin);
			var callbackArgs = new CallbackEventArgs<System.Boolean>(string.Empty, string.Empty,stateObject, new object[] { certificateOfOrigin });
			callbackArgs.SetResult(result);
			clientCallback.Invoke(this, callbackArgs);
			return string.Empty;
		}
	}
}
