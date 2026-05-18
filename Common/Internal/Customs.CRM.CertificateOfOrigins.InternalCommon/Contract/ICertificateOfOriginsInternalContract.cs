using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using Customs.CRM.CertificateOfOrigins.InternalCommon.Common;
using Customs.CRM.Entities.Entities.ExportDocumentAuthenticationRequestSearch;
using Customs.CertificateOfOrigins.Entities;
using Customs.Infrastructure.DocumentManagement.ExternalCommon;
using Customs.StockPileData.Customers.ExternalCommon.Common;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.Communication;
using Customs.CRM.Entities.CertificateOfOriginsDTO;
using Customs.CRM.External;

namespace Customs.CRM.CertificateOfOrigins.InternalCommon.Contract
{
    /// <summary>
    /// ICertificateOfOriginsInternalContract
    /// </summary>
    [ServiceContract]
    [InfContractExtension(ESubSystem.CRM)]
    public interface ICertificateOfOriginsInternalContract : IServiceContract
    {
        /// <summary>
        /// Gets the certificate of origins by filter.
        /// </summary>
        /// <param name="filter">The filter.</param>
        /// <returns></returns>
		[OperationContract]
		[FaultContract(typeof(InfExceptionData))]
        List<CertificateOfOriginResult> GetCertificateOfOriginsByFilter(CertificateOfOriginFilter filter);


        /// <summary>
        /// Determines whether [is certificate of origin by external identifier exist] [the specified certificate of origin external identifier].
        /// </summary>
        /// <param name="certificateOfOriginExternalId">The certificate of origin external identifier.</param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(InfExceptionData))]
        CertificateOfOriginResult IsCertificateOfOriginByExternalIdExist(string certificateOfOriginExternalId);

        /// <summary>
        /// Gets the certificate of origin by identifier.
        /// </summary>
        /// <param name="certificateOfOriginId">The certificate of origin identifier.</param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(InfExceptionData))]
        CertificateOfOrigin GetCertificateOfOriginById(int certificateOfOriginId);

        /// <summary>
        /// Saves the certificate of origin.
        /// </summary>
        /// <param name="certificateOfOrigin">The certificate of origin.</param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(InfExceptionData))]
        CertificateOfOrigin SaveCertificateOfOrigin(CertificateOfOrigin certificateOfOrigin);


        /// <summary>
        /// Saves the import authentication request.
        /// </summary>
        /// <param name="importAuthenticationRequest">The import authentication request.</param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(InfExceptionData))]
        CertificateOfOriginsImportAuthenticationRequest SaveImportAuthenticationRequest(CertificateOfOriginsImportAuthenticationRequest importAuthenticationRequest);

        /// <summary>
        /// Gets the authentication request by filter.
        /// </summary>
        /// <param name="filter">The filter.</param>
        /// <returns></returns>
        [OperationContract]
		[FaultContract(typeof(InfExceptionData))]
        List<GetImportAuthenticationRequestResult> GetAuthenticationRequestByFilter(ImportAuthenticationRequestFilter filter);

        /// <summary>
        /// Gets the entity documents.
        /// </summary>
        /// <param name="importAuthenticationRequest">The import authentication request.</param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(InfExceptionData))]
        List<DocumentDTO> GetEntityDocuments(CertificateOfOriginsImportAuthenticationRequest importAuthenticationRequest);

        /// <summary>
        /// Creates the new authentication file.
        /// </summary>
        /// <param name="importAuthenticationRequests">The import authentication requests.</param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(InfExceptionData))]
        CertificateOfOriginsImportAuthenticationFileDetails CreateNewAuthenticationFile(List<GetImportAuthenticationRequestResult> importAuthenticationRequests);

        /// <summary>
        /// Gets the authentication request by identifier.
        /// </summary>
        /// <param name="fileID">The file identifier.</param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(InfExceptionData))]
        CertificateOfOriginsImportAuthenticationFileDetails GetAuthenticationRequestFileByID(int fileID);
       
        /// <summary>
        /// SaveAuthenticationRequestFile
        /// </summary>
        /// <param name="authenticationRequestFile"></param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(InfExceptionData))]
        CertificateOfOriginsImportAuthenticationFileDetails SaveAuthenticationRequestFile(CertificateOfOriginsImportAuthenticationFileDetails authenticationRequestFile);
   
        /// <summary>
        /// Gets the authentication request by identifier.
        /// </summary>
        /// <param name="fileID">The file identifier.</param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(InfExceptionData))]
        CertificateOfOriginsImportAuthenticationRequest GetAuthenticationRequestByID(int documentId);

        /// <summary>
        /// Gets the authentication request by identifier.
        /// </summary>
        /// <param name="fileID">The file identifier.</param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(InfExceptionData))]
        List<ImportAuthenticationRequestDTO> GetAuthenticationRequestByLeadDocumentIDs(List<int> leadDocumentIDs);

        /// <summary>
        /// GetExportDocumentAuthenticationRequestSearch
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(InfExceptionData))]
        List<ExportDocumentAuthenticationRequestSearchResult> GetExportDocumentAuthenticationRequestSearch(
            ExportDocumentAuthenticationRequestSearchFilter filter);

        /// <summary>
        /// GetGetExportDocumentAuthenticationRequestByID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof (InfExceptionData))]
        ExportDocumentAuthenticationRequest GetExportDocumentAuthenticationRequestByID(int id);

        /// <summary>
        /// SaveExportDocumentAuthenticationRequest
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof (InfExceptionData))]
        ExportDocumentAuthenticationRequest SaveExportDocumentAuthenticationRequest(
            ExportDocumentAuthenticationRequest entity);
        /// <summary>
        /// Gets the customer information.
        /// </summary>
        /// <param name="customerID">The customer identifier.</param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof (InfExceptionData))]
        CustomerDTO GetCustomerInformation(int customerID);

        /// <summary>
        /// Gets the customer information By Country.
        /// </summary>
        /// <param name="customerID">The Country identifier.</param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(InfExceptionData))]
        CustomerDTO GetCustomerInformationByCountry(int countryID);

        /// <summary>
        /// HandleImportAuthenticationRequestDeliveryForImporterSent
        /// </summary>
        /// <param name="importAuthenticationRequesID">The importAuthenticationReques identifier.</param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(InfExceptionData))]
        CertificateOfOriginsImportAuthenticationRequest HandleImportAuthenticationRequestDeliveryForImporterSent(CertificateOfOriginsImportAuthenticationRequest authenticationRequest);

        /// <summary>
        /// HandleImportAuthenticationRequestDeliveryForImporterSent
        /// </summary>
        /// <param name="importAuthenticationRequesID">The importAuthenticationReques identifier.</param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(InfExceptionData))]
        CertificateOfOriginsImportAuthenticationRequest HandleImportAuthenticationRequestDeliveryReminderForImporterSent(CertificateOfOriginsImportAuthenticationRequest authenticationRequest);

        [OperationContract]
        [FaultContract(typeof(InfExceptionData))]
        CertificateOfOriginsImportAuthenticationFileDetails HandleImportAuthenticationRequestDeliveryAndReminderForVendorSent(CertificateOfOriginsImportAuthenticationFileDetails authenticationFile, bool isDelivery);

        [OperationContract]
        [FaultContract(typeof(InfExceptionData))]
        bool CheckIfExistsAdditionalRequestsForImporter(CertificateOfOriginsImportAuthenticationRequest request);

        [OperationContract]
        [FaultContract(typeof(InfExceptionData))]
        bool CheckIfExistsAdditionalRequestsForVendor(int vendorID);

        [OperationContract]
        [FaultContract(typeof(InfExceptionData))]
        NavigationToVendorView GetPathsForNavigationToVendor();

        [OperationContract]
        [FaultContract(typeof(InfExceptionData))]
        bool HandleSendRemindDeliverNotification(CertificateOfOriginsImportAuthenticationFileDetails file);

        [OperationContract]
        [FaultContract(typeof(InfExceptionData))]
        CertificateOfOriginsImportAuthenticationFileDetails ChangeStatusAfterDeliverySent(CertificateOfOriginsImportAuthenticationFileDetails importAuthenticationRequest);

        [OperationContract]
        [FaultContract(typeof(InfExceptionData))]
        int? CheckImporterOfImportAuthentication(int importerId);

        [OperationContract]
        [FaultContract(typeof(InfExceptionData))]
        bool LoadDataFromExportDeclaration(CertificateOfOrigin certificateOfOrigin);
    }
}
