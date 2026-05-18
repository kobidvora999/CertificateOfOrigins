using System.Collections.Generic;
using Customs.CertificateOfOrigins.Entities;
using Customs.CRM.CertificateOfOrigins.BL.ServiceAdapters;
using Customs.CRM.Entities;
using Customs.Inf.CommonService.ExternalCommon;
using Customs.Inf.CommonService.ExternalCommon.TemplateDTOs;
using Customs.Infrastructure.DocumentManagement.ExternalCommon;
using Customs.Infrastructure.DocumentManagement.ExternalCommon.Enums;
using Customs.Infrastructure.Entities;
using Customs.Infrastructure.Tasks.ExternalCommon;
using Customs.KnowledgeStore.CustomsBook.ExternalCommon.Common;
using Customs.StockPileData.Customers.ExternalCommon.Common;
using MalamTeam.Infrastructure.GeneralServices.BaseLayer;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.Base_Entities;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.Extensions;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.ORM;
using MalamTeam.Infrastructure.GeneralServices.Environment.Const;
using MalamTeam.Infrastructure.GeneralServices.Environment.Enums;
using Microsoft.Practices.Unity;

namespace Customs.CRM.CertificateOfOrigins.BL
{
    public class ServicesAdapter : BaseLayer
    {
        public const int IsraelID = 376;

        public ServicesAdapter()
        {
            Container.RegisterType<ICustomerServiceAdapter, CustomerServiceAdapter>();
            Container.RegisterType<ICustomsBookServicesAdapter, CustomsBookServicesAdapter>();
            Container.RegisterType<IUserServiceAdapter, UserServiceAdapter>();
            Container.RegisterType<IDealFileServiceAdapter, DealFileServiceAdapter>();
            Container.RegisterType<ICommonServicesServiceAdapter, CommonServicesServiceAdapter>();
            Container.RegisterType<IDocumentServiceAdapter, DocumentServiceAdapter>();
            Container.RegisterType<ICollateralServiceAdapter, CollateralServiceAdapter>();
            Container.RegisterType<ISendMessagesServiceAdapter, SendMessagesServiceAdapter>();
            Container.RegisterType<IDeliveryAdapter, DeliveryAdapter>();
            Container.RegisterType<ITasksServiceAdapter, TasksServiceAdapter>();
        }

        public int? GetCustomerIDByExternalID(string customerExternalId)
        {
            var identificationParam = new CustomerIdentificationFilter
            {
                ExternalId = customerExternalId
            };

            var customerAdapter = Container.Resolve<ICustomerServiceAdapter>();
            var customer = customerAdapter.GetCustomerDTOByCustomerIdentification(identificationParam);
            if (customer == null) return null;

            return customer.ID;
        }

        public bool IsOrganzationUnitCustomHouse(int organizationUnitID)
        {
            var userAdapter = Container.Resolve<IUserServiceAdapter>();
            var isCustomsHouse = userAdapter.IsOrganzationUnitCustomHouse(organizationUnitID);

            return isCustomsHouse;
        }

        public int? GetCustomsItemIdByFullClassification(string fullClassification, ECustomsBookType bookType)
        {
            var customsBookAdapter = Container.Resolve<ICustomsBookServicesAdapter>();
            var customsItem = customsBookAdapter.GetCustomsItemIdByFullClassificationSync(fullClassification, bookType);

            return customsItem;
        }

        public bool IsTradeAgreementForCountry(int certificateTypeId, int countryId, bool isCountryGroup)
        {
            if (!isCountryGroup && countryId == IsraelID) return true;

            using (var uow = Container.Resolve<IUnitOfWork>(CRMConsts.CertificateOfOriginsUnitOfWork))
            {
                var util = new CertificateOfOriginsUtil(uow);
                var customsBookAdapter = Container.Resolve<ICustomsBookServicesAdapter>();
                var isTradeAgreement = false;

                var tradeAgreementsIds = util.GetTradeAgreementsForCertificateType(certificateTypeId);
                if (tradeAgreementsIds.IsNullOrEmpty()) return false;

                foreach (var tradeAgreementId in tradeAgreementsIds)
                {
                    isTradeAgreement = customsBookAdapter.IsTradeAgreementForCountry(countryId, tradeAgreementId, isCountryGroup);
                    if (isTradeAgreement) return true;
                }

                return isTradeAgreement;              
            }                                               
        }

        

    
        public int? GetExportDeclarationAssessor(string exportDeclarationNum)
        {
            var dealFileAdapter = Container.Resolve<IDealFileServiceAdapter>();
            var assesorId = dealFileAdapter.GetExportDeclarationAssessor(exportDeclarationNum);

            return assesorId;
        }

        //public TemplateResult CreateTemplateSync(int certificateOfOriginId, int templateTypeId)
        //{
        //    var commonServicesServiceAdapter = Container.Resolve<ICommonServicesServiceAdapter>();
        //    var templateResult = commonServicesServiceAdapter.CreateTemplateSync(certificateOfOriginId, templateTypeId);

        //    return templateResult;
        //}

        public TemplateResult CreateTemplateSync(int certificateOfOriginId, int templateTypeId, string additionalInfo)
        {
            var commonServicesServiceAdapter = Container.Resolve<ICommonServicesServiceAdapter>();
            var templateResult = commonServicesServiceAdapter.CreatePDFTemplateSync(certificateOfOriginId, templateTypeId, additionalInfo);

            return templateResult;
        }

        public DocumentDTO UploadDocumentAndSave(DocumentDTO document, byte[] content)
        {
            var documentServiceAdapter = Container.Resolve<IDocumentServiceAdapter>();
            var documentResult = documentServiceAdapter.UploadDocumentAndSave(document, content);

            return documentResult;
        }

        public void DeleteDocument(List<int> docIds, VirtualEntity ve)
        {
            var documentServiceAdapter = Container.Resolve<IDocumentServiceAdapter>();
            documentServiceAdapter.DeleteDocument(docIds, ve);
        }

        public List<Document> GetDocumentsByEntitySync(int entityId, EEntityType eEntityType)
        {
            var documentServiceAdapter = Container.Resolve<IDocumentServiceAdapter>();
            return documentServiceAdapter.GetDocumentsByEntitySync(entityId, eEntityType);
        }

        public string SendMessageToAgent(CertificateOfOrigin certificate, string msgString, EAgentTalkBackType agentTalkBackType)
        {
            var entity = new VirtualEntity(certificate.ID, (int) EEntityType.CertificateOfOrigin)
                             {
                                 CustomerID = certificate.CreateCustomerID,
                                 Title = certificate.Title
                             };

            var documentServiceAdapter = Container.Resolve<IDocumentServiceAdapter>();
            var strResult = documentServiceAdapter.SendMessageToAgent(entity, agentTalkBackType, msgString);

            return strResult;
        }

        public  void SendMessage(SendMessageDTO sendMessageDTO)
        {
            var sendMessageServiceAdapter = Container.Resolve<ISendMessagesServiceAdapter>();
            sendMessageServiceAdapter.SendMessage(sendMessageDTO);
        }

        public void SendDeliverySync(CertificateOfOriginsImportAuthenticationFileDetails request, ETemplate template)
        {
            var deliveryAdapter = Container.Resolve<IDeliveryAdapter>();
            deliveryAdapter.SendDeliverySync(request, template);
        }


        public List<CustomsItemDTO> GetCustomsItemsByIdsSync(List<CustomsItemsIdsCacheFilter> customsItemsIdsCacheFilter)
        {
            var customsBookServicesAdapter = Container.Resolve<ICustomsBookServicesAdapter>();
           return customsBookServicesAdapter.GetCustomsItemsByIdsSync(customsItemsIdsCacheFilter);
        }

        public int? GetLatestUserHandlingEntityTasksWithTaskUnification(LatestUserHandlingEntityTasksFilter filter)
        {
            var tasksServiceAdapter = Container.Resolve<ITasksServiceAdapter>();
            var latestUserHandlingEntityTasksResult = tasksServiceAdapter.GetLatestUserHandlingEntityTasksWithTaskUnification(filter);
            return latestUserHandlingEntityTasksResult.UserID;
        }

    }
}
