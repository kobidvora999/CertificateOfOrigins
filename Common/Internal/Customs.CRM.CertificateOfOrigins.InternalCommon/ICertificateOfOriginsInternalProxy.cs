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
using MalamTeam.Infrastructure.GeneralServices.CommonBase.Communication;
namespace Customs.CRM.CertificateOfOrigins.InternalProxy
{
	/// <summary>
	/// Generated proxy interface
	/// </summary>
	public partial interface ICertificateOfOriginsInternalProxy
	{
		/// <summary>
		/// GetCertificateOfOriginsByFilter auto generated wrapper
		/// </summary>
		string GetCertificateOfOriginsByFilter(EventHandler<CallbackEventArgs<System.Collections.Generic.List<Customs.CRM.CertificateOfOrigins.InternalCommon.Common.CertificateOfOriginResult>>> clientCallback,Customs.CRM.CertificateOfOrigins.InternalCommon.Common.CertificateOfOriginFilter filter, object stateObject = null);
		/// <summary>
		/// IsCertificateOfOriginByExternalIdExist auto generated wrapper
		/// </summary>
		string IsCertificateOfOriginByExternalIdExist(EventHandler<CallbackEventArgs<Customs.CRM.CertificateOfOrigins.InternalCommon.Common.CertificateOfOriginResult>> clientCallback,System.String certificateOfOriginExternalId, object stateObject = null);
		/// <summary>
		/// GetCertificateOfOriginById auto generated wrapper
		/// </summary>
		string GetCertificateOfOriginById(EventHandler<CallbackEventArgs<Customs.CertificateOfOrigins.Entities.CertificateOfOrigin>> clientCallback,System.Int32 certificateOfOriginId, object stateObject = null);
		/// <summary>
		/// SaveCertificateOfOrigin auto generated wrapper
		/// </summary>
		string SaveCertificateOfOrigin(EventHandler<CallbackEventArgs<Customs.CertificateOfOrigins.Entities.CertificateOfOrigin>> clientCallback,Customs.CertificateOfOrigins.Entities.CertificateOfOrigin certificateOfOrigin, object stateObject = null);
		/// <summary>
		/// SaveImportAuthenticationRequest auto generated wrapper
		/// </summary>
		string SaveImportAuthenticationRequest(EventHandler<CallbackEventArgs<Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationRequest>> clientCallback,Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationRequest importAuthenticationRequest, object stateObject = null);
		/// <summary>
		/// GetAuthenticationRequestByFilter auto generated wrapper
		/// </summary>
		string GetAuthenticationRequestByFilter(EventHandler<CallbackEventArgs<System.Collections.Generic.List<Customs.CertificateOfOrigins.Entities.GetImportAuthenticationRequestResult>>> clientCallback,Customs.CertificateOfOrigins.Entities.ImportAuthenticationRequestFilter filter, object stateObject = null);
		/// <summary>
		/// GetEntityDocuments auto generated wrapper
		/// </summary>
		string GetEntityDocuments(EventHandler<CallbackEventArgs<System.Collections.Generic.List<Customs.Infrastructure.DocumentManagement.ExternalCommon.DocumentDTO>>> clientCallback,Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationRequest importAuthenticationRequest, object stateObject = null);
		/// <summary>
		/// CreateNewAuthenticationFile auto generated wrapper
		/// </summary>
		string CreateNewAuthenticationFile(EventHandler<CallbackEventArgs<Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationFileDetails>> clientCallback,System.Collections.Generic.List<Customs.CertificateOfOrigins.Entities.GetImportAuthenticationRequestResult> importAuthenticationRequests, object stateObject = null);
		/// <summary>
		/// GetAuthenticationRequestFileByID auto generated wrapper
		/// </summary>
		string GetAuthenticationRequestFileByID(EventHandler<CallbackEventArgs<Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationFileDetails>> clientCallback,System.Int32 fileID, object stateObject = null);
		/// <summary>
		/// SaveAuthenticationRequestFile auto generated wrapper
		/// </summary>
		string SaveAuthenticationRequestFile(EventHandler<CallbackEventArgs<Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationFileDetails>> clientCallback,Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationFileDetails authenticationRequestFile, object stateObject = null);
		/// <summary>
		/// GetAuthenticationRequestByID auto generated wrapper
		/// </summary>
		string GetAuthenticationRequestByID(EventHandler<CallbackEventArgs<Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationRequest>> clientCallback,System.Int32 documentId, object stateObject = null);
		/// <summary>
		/// GetAuthenticationRequestByLeadDocumentIDs auto generated wrapper
		/// </summary>
		string GetAuthenticationRequestByLeadDocumentIDs(EventHandler<CallbackEventArgs<System.Collections.Generic.List<Customs.CRM.Entities.CertificateOfOriginsDTO.ImportAuthenticationRequestDTO>>> clientCallback,System.Collections.Generic.List<System.Int32> leadDocumentIDs, object stateObject = null);
		/// <summary>
		/// GetExportDocumentAuthenticationRequestSearch auto generated wrapper
		/// </summary>
		string GetExportDocumentAuthenticationRequestSearch(EventHandler<CallbackEventArgs<System.Collections.Generic.List<Customs.CRM.Entities.Entities.ExportDocumentAuthenticationRequestSearch.ExportDocumentAuthenticationRequestSearchResult>>> clientCallback,Customs.CRM.Entities.Entities.ExportDocumentAuthenticationRequestSearch.ExportDocumentAuthenticationRequestSearchFilter filter, object stateObject = null);
		/// <summary>
		/// GetExportDocumentAuthenticationRequestByID auto generated wrapper
		/// </summary>
		string GetExportDocumentAuthenticationRequestByID(EventHandler<CallbackEventArgs<Customs.CertificateOfOrigins.Entities.ExportDocumentAuthenticationRequest>> clientCallback,System.Int32 id, object stateObject = null);
		/// <summary>
		/// SaveExportDocumentAuthenticationRequest auto generated wrapper
		/// </summary>
		string SaveExportDocumentAuthenticationRequest(EventHandler<CallbackEventArgs<Customs.CertificateOfOrigins.Entities.ExportDocumentAuthenticationRequest>> clientCallback,Customs.CertificateOfOrigins.Entities.ExportDocumentAuthenticationRequest entity, object stateObject = null);
		/// <summary>
		/// GetCustomerInformation auto generated wrapper
		/// </summary>
		string GetCustomerInformation(EventHandler<CallbackEventArgs<Customs.StockPileData.Customers.ExternalCommon.Common.CustomerDTO>> clientCallback,System.Int32 customerID, object stateObject = null);
		/// <summary>
		/// GetCustomerInformationByCountry auto generated wrapper
		/// </summary>
		string GetCustomerInformationByCountry(EventHandler<CallbackEventArgs<Customs.StockPileData.Customers.ExternalCommon.Common.CustomerDTO>> clientCallback,System.Int32 countryID, object stateObject = null);
		/// <summary>
		/// HandleImportAuthenticationRequestDeliveryForImporterSent auto generated wrapper
		/// </summary>
		string HandleImportAuthenticationRequestDeliveryForImporterSent(EventHandler<CallbackEventArgs<Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationRequest>> clientCallback,Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationRequest authenticationRequest, object stateObject = null);
		/// <summary>
		/// HandleImportAuthenticationRequestDeliveryReminderForImporterSent auto generated wrapper
		/// </summary>
		string HandleImportAuthenticationRequestDeliveryReminderForImporterSent(EventHandler<CallbackEventArgs<Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationRequest>> clientCallback,Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationRequest authenticationRequest, object stateObject = null);
		/// <summary>
		/// HandleImportAuthenticationRequestDeliveryAndReminderForVendorSent auto generated wrapper
		/// </summary>
		string HandleImportAuthenticationRequestDeliveryAndReminderForVendorSent(EventHandler<CallbackEventArgs<Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationFileDetails>> clientCallback,Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationFileDetails authenticationFile,System.Boolean isDelivery, object stateObject = null);
		/// <summary>
		/// CheckIfExistsAdditionalRequestsForImporter auto generated wrapper
		/// </summary>
		string CheckIfExistsAdditionalRequestsForImporter(EventHandler<CallbackEventArgs<System.Boolean>> clientCallback,Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationRequest request, object stateObject = null);
		/// <summary>
		/// CheckIfExistsAdditionalRequestsForVendor auto generated wrapper
		/// </summary>
		string CheckIfExistsAdditionalRequestsForVendor(EventHandler<CallbackEventArgs<System.Boolean>> clientCallback,System.Int32 vendorID, object stateObject = null);
		/// <summary>
		/// GetPathsForNavigationToVendor auto generated wrapper
		/// </summary>
		string GetPathsForNavigationToVendor(EventHandler<CallbackEventArgs<Customs.CRM.External.NavigationToVendorView>> clientCallback, object stateObject = null);
		/// <summary>
		/// HandleSendRemindDeliverNotification auto generated wrapper
		/// </summary>
		string HandleSendRemindDeliverNotification(EventHandler<CallbackEventArgs<System.Boolean>> clientCallback,Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationFileDetails file, object stateObject = null);
		/// <summary>
		/// ChangeStatusAfterDeliverySent auto generated wrapper
		/// </summary>
		string ChangeStatusAfterDeliverySent(EventHandler<CallbackEventArgs<Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationFileDetails>> clientCallback,Customs.CertificateOfOrigins.Entities.CertificateOfOriginsImportAuthenticationFileDetails importAuthenticationRequest, object stateObject = null);
		/// <summary>
		/// CheckImporterOfImportAuthentication auto generated wrapper
		/// </summary>
		string CheckImporterOfImportAuthentication(EventHandler<CallbackEventArgs<System.Nullable<System.Int32>>> clientCallback,System.Int32 importerId, object stateObject = null);
		/// <summary>
		/// LoadDataFromExportDeclaration auto generated wrapper
		/// </summary>
		string LoadDataFromExportDeclaration(EventHandler<CallbackEventArgs<System.Boolean>> clientCallback,Customs.CertificateOfOrigins.Entities.CertificateOfOrigin certificateOfOrigin, object stateObject = null);
	}
}
