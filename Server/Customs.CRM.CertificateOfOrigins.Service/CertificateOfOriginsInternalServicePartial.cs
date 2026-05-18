using Customs.CertificateOfOrigins.Entities;
using Customs.CRM.CertificateOfOrigins.BL;
using Customs.CRM.CertificateOfOrigins.InternalCommon.Common;
using Customs.CRM.Entities;
using Customs.CRM.Entities.CertificateOfOriginsDTO;
using Customs.CRM.Entities.Entities.ExportDocumentAuthenticationRequestSearch;
using Customs.CRM.External;
using Customs.Infrastructure.DocumentManagement.ExternalCommon;
using Customs.StockPileData.Customers.ExternalCommon.Common;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.ORM;
using MalamTeam.Infrastructure.GeneralServices.Environment.Const;
using Microsoft.Practices.Unity;
using System.Collections.Generic;

namespace Customs.CRM.CertificateOfOrigins.Service
{


    public partial class CertificateOfOriginsInternalService
    {
        private CertificateOfOriginsIncomingMessageService _incomingMsgService = new CertificateOfOriginsIncomingMessageService();
        private readonly ServicesAdapter _servicesAdapter = new ServicesAdapter();

        public List<CertificateOfOriginResult> InternalGetCertificateOfOriginsByFilter(CertificateOfOriginFilter filter)
        {
            using (var uow = Container.Resolve<IUnitOfWork>(CRMConsts.CertificateOfOriginsUnitOfWork))
            {
                var bl = new CertificateOfOriginsBL(uow);
                return bl.GetCertificateOfOriginsByFilter(filter);
            }
        }

        public CertificateOfOriginResult InternalIsCertificateOfOriginByExternalIdExist(string certificateOfOriginExternalId)
        {
            using (var uow = Container.Resolve<IUnitOfWork>(CRMConsts.CertificateOfOriginsUnitOfWork))
            {
                var bl = new CertificateOfOriginsBL(uow);
                return bl.IsCertificateOfOriginByExternalIdExist(certificateOfOriginExternalId);
            }
        }

        public CertificateOfOrigin InternalGetCertificateOfOriginById(int certificateOfOriginId)
        {
            using (var uow = Container.Resolve<IUnitOfWork>(CRMConsts.CertificateOfOriginsUnitOfWork))
            {
                var bl = new CertificateOfOriginsBL(uow);
                return bl.GetCertificateOfOriginById(certificateOfOriginId);
            }
        }

        public CertificateOfOrigin InternalSaveCertificateOfOrigin(CertificateOfOrigin certificateOfOrigin)
        {
            bool isStatusChanged;
            bool isRemarksChanged;
            CertificateOfOrigin certificate;
            using (var uow = Container.Resolve<IUnitOfWork>(CRMConsts.CertificateOfOriginsUnitOfWork))
            {
                var bl = new CertificateOfOriginsBL(uow);
                certificate = bl.SaveCertificateOfOrigin(certificateOfOrigin, null, null, out isStatusChanged, out isRemarksChanged);

                if (isStatusChanged && certificate.CertificateOfOriginStatusID == (int)ECertificateOfOriginStatus.PendingRelease)
                {
                    bl.CheckCertificateOfOriginOnDeclarationReleased(certificate);
                }

                if ((isStatusChanged && certificateOfOrigin.CertificateOfOriginStatusID != (int)ECertificateOfOriginStatus.Published)
                      || isRemarksChanged)
                {
                    bl.SendRequestFeedback(certificateOfOrigin, null);
                }

                if (isStatusChanged && certificateOfOrigin.CertificateOfOriginStatusID == (int)ECertificateOfOriginStatus.Published)
                {
                    bl.CreateAttacmentsAndSendFeedBackMessage(certificate);
                    if (certificate.RequestReasonCode == (int)ERequestReason.CertificateReplacement)
                        bl.HandleCertificateReplacement(certificate);
                }
            }

            return certificate;
        }

        public CertificateOfOriginsImportAuthenticationRequest InternalSaveImportAuthenticationRequest(CertificateOfOriginsImportAuthenticationRequest importAuthenticationRequestResult)
        {
            using (var uow = Container.Resolve<IUnitOfWork>(CRMConsts.CertificateOfOriginsUnitOfWork))
            {
                var bl = new AuthenticationRequestBL(uow);
                var importAuthenticationRequest = bl.SaveImportAuthenticationRequest(importAuthenticationRequestResult);
                return importAuthenticationRequest;
            }
        }

        public List<GetImportAuthenticationRequestResult> InternalGetAuthenticationRequestByFilter(ImportAuthenticationRequestFilter filter)
        {
            using (var uow = Container.Resolve<IUnitOfWork>(CRMConsts.CertificateOfOriginsUnitOfWork))
            {
                var bl = new AuthenticationRequestBL(uow);
                return bl.GetAuthenticationRequestByFilter(filter);
            }
        }

        public List<DocumentDTO> InternalGetEntityDocuments(CertificateOfOriginsImportAuthenticationRequest importAuthenticationRequest)
        {
            using (var uow = Container.Resolve<IUnitOfWork>(CRMConsts.CertificateOfOriginsUnitOfWork))
            {
                var bl = new AuthenticationRequestBL(uow);
                return bl.GetEntityDocuments(importAuthenticationRequest);
            }
        }

        public CertificateOfOriginsImportAuthenticationFileDetails InternalCreateNewAuthenticationFile(List<GetImportAuthenticationRequestResult> importAuthenticationRequests)
        {
            using (var uow = Container.Resolve<IUnitOfWork>(CRMConsts.CertificateOfOriginsUnitOfWork))
            {
                var bl = new AuthenticationRequestBL(uow);
                return bl.CreateNewAuthenticationFile(importAuthenticationRequests);
            }
        }

        public CertificateOfOriginsImportAuthenticationFileDetails InternalGetAuthenticationRequestFileByID(int fileID)
        {
            using (var uow = Container.Resolve<IUnitOfWork>(CRMConsts.CertificateOfOriginsUnitOfWork))
            {
                var bl = new AuthenticationRequestBL(uow);
                return bl.GetAuthenticationRequestFileByID(fileID);
            }
        }

        public CertificateOfOriginsImportAuthenticationFileDetails InternalSaveAuthenticationRequestFile(CertificateOfOriginsImportAuthenticationFileDetails authenticationRequestFile)
        {
            using (var uow = Container.Resolve<IUnitOfWork>(CRMConsts.CertificateOfOriginsUnitOfWork))
            {
                var bl = new AuthenticationRequestBL(uow);
                var file = bl.SaveAuthenticationRequestFile(authenticationRequestFile);
                uow.CommitAllChanges();
                return file;
            }
        }

        public CertificateOfOriginsImportAuthenticationRequest InternalGetAuthenticationRequestByID(int documentId)
        {
            using (var uow = Container.Resolve<IUnitOfWork>(CRMConsts.CertificateOfOriginsUnitOfWork))
            {
                var bl = new AuthenticationRequestBL(uow);
                var file = bl.GetAuthenticationRequestByID(documentId);
                return file;
            }
        }

        public List<ExportDocumentAuthenticationRequestSearchResult> InternalGetExportDocumentAuthenticationRequestSearch(ExportDocumentAuthenticationRequestSearchFilter filter)
        {
            using (var uow = Container.Resolve<IUnitOfWork>(CRMConsts.CertificateOfOriginsUnitOfWork))
            {
                var bl = new ExportDocumentAuthenticationRequestBL(uow);
                return bl.GetExportDocumentAuthenticationRequestSearch(filter);
            }
        }

        public ExportDocumentAuthenticationRequest InternalGetExportDocumentAuthenticationRequestByID(int id)
        {
            using (var uow = Container.Resolve<IUnitOfWork>(CRMConsts.CertificateOfOriginsUnitOfWork))
            {
                var bl = new ExportDocumentAuthenticationRequestBL(uow);
                return bl.GetExportDocumentAuthenticationRequestByID(id);
            }
        }

        public ExportDocumentAuthenticationRequest InternalSaveExportDocumentAuthenticationRequest(
            ExportDocumentAuthenticationRequest entity)
        {
            using (var uow = Container.Resolve<IUnitOfWork>(CRMConsts.CertificateOfOriginsUnitOfWork))
            {
                var bl = new ExportDocumentAuthenticationRequestBL(uow);
                return bl.SaveExportDocumentAuthenticationRequest(entity);
            }
        }

        public CustomerDTO InternalGetCustomerInformation(int customerID)
        {
            using (var uow = Container.Resolve<IUnitOfWork>(CRMConsts.CertificateOfOriginsUnitOfWork))
            {
                var bl = new ExportDocumentAuthenticationRequestBL(uow);
                return bl.GetCustomerInformation(customerID);
            }

        }

        public CustomerDTO InternalGetCustomerInformationByCountry(int countryID)
        {
            using (var uow = Container.Resolve<IUnitOfWork>(CRMConsts.CertificateOfOriginsUnitOfWork))
            {
                var bl = new ExportDocumentAuthenticationRequestBL(uow);
                return bl.GetCustomerInformationByCountry(countryID);
            }
        }

        public List<ImportAuthenticationRequestDTO> InternalGetAuthenticationRequestByLeadDocumentIDs(List<int> leadDocumentIDs)
        {
            using (var uow = Container.Resolve<IUnitOfWork>(CRMConsts.CertificateOfOriginsUnitOfWork))
            {
                var bl = new CertificateOfOriginsBL(uow);
                return bl.GetAuthenticationRequestByLeadDocumentIDs(leadDocumentIDs);
            }
        }

        public CertificateOfOriginsImportAuthenticationRequest InternalHandleImportAuthenticationRequestDeliveryForImporterSent(CertificateOfOriginsImportAuthenticationRequest authenticationRequest)
        {
            using (var uow = Container.Resolve<IUnitOfWork>(CRMConsts.CertificateOfOriginsUnitOfWork))
            {
                var bl = new AuthenticationRequestBL(uow);
                return bl.HandleImportAuthenticationRequestDeliveryForImporterSent(authenticationRequest);
            }
        }

        public CertificateOfOriginsImportAuthenticationRequest InternalHandleImportAuthenticationRequestDeliveryReminderForImporterSent(CertificateOfOriginsImportAuthenticationRequest authenticationRequest)
        {
            using (var uow = Container.Resolve<IUnitOfWork>(CRMConsts.CertificateOfOriginsUnitOfWork))
            {
                var bl = new AuthenticationRequestBL(uow);
                return bl.HandleImportAuthenticationRequestDeliveryReminderForImporterSent(authenticationRequest);

            }
        }

        public CertificateOfOriginsImportAuthenticationFileDetails InternalHandleImportAuthenticationRequestDeliveryAndReminderForVendorSent(CertificateOfOriginsImportAuthenticationFileDetails authenticationFile, bool isDelivery)
        {
            using (var uow = Container.Resolve<IUnitOfWork>(CRMConsts.CertificateOfOriginsUnitOfWork))
            {
                var bl = new AuthenticationRequestBL(uow);
                return bl.HandleImportAuthenticationRequestDeliveryAndReminderForVendorSent(authenticationFile, isDelivery);
            }
        }

        public bool InternalCheckIfExistsAdditionalRequestsForImporter(CertificateOfOriginsImportAuthenticationRequest request)
        {
            using (var uow = Container.Resolve<IUnitOfWork>(CRMConsts.CertificateOfOriginsUnitOfWork))
            {
                var bl = new AuthenticationRequestBL(uow);
                return bl.CheckIfExistsAdditionalRequestsForImporter(request);
            }
        }

        public bool InternalCheckIfExistsAdditionalRequestsForVendor(int vendorID)
        {
            using (var uow = Container.Resolve<IUnitOfWork>(CRMConsts.CertificateOfOriginsUnitOfWork))
            {
                var bl = new AuthenticationRequestBL(uow);
                return bl.CheckIfExistsAdditionalRequestsForVendor(vendorID);
            }
        }

        public NavigationToVendorView InternalGetPathsForNavigationToVendor()
        {
            using (var uow = Container.Resolve<IUnitOfWork>(InfrastructureConsts.InfrastructureORMMapping))
            {
                var bl = new AuthenticationRequestBL(uow);
                return bl.GetPathsForNavigationToVendor();
            }
        }

        public bool InternalHandleSendRemindDeliverNotification(CertificateOfOriginsImportAuthenticationFileDetails file)
        {
            using (var uow = Container.Resolve<IUnitOfWork>(InfrastructureConsts.InfrastructureORMMapping))
            {
                var bl = new AuthenticationRequestBL(uow);
                return bl.CloseReminderTask(file);
            }
        }

        public CertificateOfOriginsImportAuthenticationFileDetails InternalChangeStatusAfterDeliverySent(
            CertificateOfOriginsImportAuthenticationFileDetails importAuthenticationRequest)
        {
            using (var uow = Container.Resolve<IUnitOfWork>(InfrastructureConsts.InfrastructureORMMapping))
            {
                var bl = new AuthenticationRequestBL(uow);
                return bl.ChangeStatusAfterDeliverySent(importAuthenticationRequest);
            }
        }
        public int? InternalCheckImporterOfImportAuthentication(int importerId)
        {
            using (var uow = Container.Resolve<IUnitOfWork>(CRMConsts.CertificateOfOriginsUnitOfWork))
            {
                var bl = new AuthenticationRequestBL(uow);
                return bl.CheckImporterOfImportAuthentication(importerId);
            }
        }

        public bool InternalLoadDataFromExportDeclaration(CertificateOfOrigin certificateOfOrigin)
        {
            using (var uow = Container.Resolve<IUnitOfWork>(CRMConsts.CertificateOfOriginsUnitOfWork))
            {
                var bl = new CertificateOfOriginsBL(uow);
                return bl.LoadDataFromExportDeclaration(certificateOfOrigin);
            }
        }
    }
}
