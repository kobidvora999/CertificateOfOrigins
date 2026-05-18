using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Customs.CertificateOfOrigins.Entities;
using Customs.CRM.CertificateOfOrigins.InternalCommon.Common;
using Customs.CRM.CertificateOfOrigins.Service;
using Customs.CRM.Entities;
using Customs.CRM.Entities.CertificateOfOriginsPartial;
using Customs.Inf.CertificateOfOrigins.EAISchema;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.ORM;
using MalamTeam.Infrastructure.GeneralServices.Environment.Const;
using MalamTeam.Infrastructure.GeneralServices.Environment.Enums;
using MalamTeam.Infrastructure.GeneralServices.Stubs;
using Microsoft.Practices.Unity;

namespace Customs.CRM.CertificateOfOrigins.Test
{
    public class TestUtil : BaseTest
    {
        private const int CertServerSenderID = 3641;
        private const string AttachmentFolder = "R:\\Customs Projects Code\\Integration\\EAI Request Samples";
        private readonly CertificateOfOriginsInternalService _service = new CertificateOfOriginsInternalService();
        private readonly CertificateOfOriginsIncomingMessageService _incomingMessageService = new CertificateOfOriginsIncomingMessageService();

        public CertificateOfOrigin CreateNewCertificateOfOrigin(string fileName)
        {
            CertificateOfOrigin certificate = null;

            try
            {
                var searchFileName = fileName;

                var message = new PC_NG_2280_MSG01_CertificateOfOriginRequestRequest
                {
                    CertServerSenderID = CertServerSenderID,
                    CertServerSenderTypeID = (int)EEntityType.Customer,
                    Content = ConvertGoldenToMessage<PC_NG_2280_MSG01_CertificateOfOriginRequest>(searchFileName, AttachmentFolder)
                };

                UpdateDates(message);
                                
                var response = _incomingMessageService.InternalGetPC_MSG2280_2281_CertificateOfOriginRequest(message);
                var certificateNumber = response.Content.CertificateOfOriginRequestFeedback.certificateID;

                certificate = GetCertificateByExternalId(certificateNumber);
            }
            catch (Exception ex){}

            return certificate;
        }

        public void DeleteCertificateByExternalId(string externalId)
        {
            var certificateToDelete = GetCertificateByExternalId(externalId);

            if (certificateToDelete != null)
            {
                DeleteCertificateWithChildren(certificateToDelete);
            }
        }

        public CertificateOfOrigin GetCertificateByExternalId(string externalId)
        {
            if (string.IsNullOrWhiteSpace(externalId)) return null;

            CertificateOfOrigin certificate = null;
            var certificateResult = _service.InternalGetCertificateOfOriginsByFilter(new CertificateOfOriginFilter { certificateNumber = externalId }).FirstOrDefault();

            if (certificateResult != null)
            {
                certificate = _service.InternalGetCertificateOfOriginById(certificateResult.ID);
            }

            return certificate;
        }

        private void DeleteCertificateWithChildren(CertificateOfOrigin certificate)
        {
            while (certificate.CertificateOfOriginInvoiceDetail.Count > 0)
            {
                var itemForDelete = certificate.CertificateOfOriginInvoiceDetail.First();
                DeleteInvoiceWithChildren(itemForDelete);
            }

            while (certificate.CertificateOfOriginDetails.Count > 0)
            {
                var detail = certificate.CertificateOfOriginDetails.First();
                DeleteCertificateDetail(detail);
            }

            while (certificate.CertificateOfOriginVsDeclarationError.Count > 0)
            {
                var error = certificate.CertificateOfOriginVsDeclarationError.First();
                DeleteCertificateError(error);
            }

            using (var uow = Container.Resolve<IUnitOfWork>(CRMConsts.CertificateOfOriginsUnitOfWork))
            {
                uow.Repository.Delete(certificate);
                uow.CommitAllChanges();
            }
        }

        private void DeleteInvoiceWithChildren(CertificateOfOriginInvoiceDetail invoiceDetail)
        {
            using (var uow = Container.Resolve<IUnitOfWork>(CRMConsts.CertificateOfOriginsUnitOfWork))
            {
                while (invoiceDetail.CertificateOfOriginItemDetail.Count > 0)
                {
                    var itemDetail = invoiceDetail.CertificateOfOriginItemDetail.First();
                    uow.Repository.Delete(itemDetail);
                    uow.CommitAllChanges();
                }

                uow.Repository.Delete(invoiceDetail);
                uow.CommitAllChanges();
            }
        }

        private void DeleteCertificateDetail(CertificateOfOriginDetails detail)
        {
            using (var uow = Container.Resolve<IUnitOfWork>(CRMConsts.CertificateOfOriginsUnitOfWork))
            {
                uow.Repository.Delete(detail);
                uow.CommitAllChanges();
            }
        }

        private void DeleteCertificateError(CertificateOfOriginVsDeclarationError error)
        {
            using (var uow = Container.Resolve<IUnitOfWork>(CRMConsts.CertificateOfOriginsUnitOfWork))
            {
                uow.Repository.Delete(error);
                uow.CommitAllChanges();
            }
        }

        public void UpdateDates(PC_NG_2280_MSG01_CertificateOfOriginRequestRequest message)
        {
            message.Content.CertificateOfOrigin.DateOfDeclaration = DateTime.Today;
            message.Content.NonManipulationCertificate.ImportDate = DateTime.Today;
            message.Content.NonManipulationCertificate.ExportDate = DateTime.Today;
            message.Content.NonManipulationCertificate.ExpectedExitDate = DateTime.Today;
        }

        public CertificateOfOriginInvoiceDetail CreateInvoiceFor2Pages(int certificateTypeId)
        {
            var invoice = CreateInvoice();
            var counter = CertificateOfOriginsConstants.NumberOfInvoicesInPage + 3;

            while (counter > 0)
            {
                invoice.CertificateOfOriginItemDetail.Add(CreateInvoiceItem(certificateTypeId));
                counter--;
            }

            return invoice;
        }

        public CertificateOfOriginInvoiceDetail CreateInvoice()
        {
            return new CertificateOfOriginInvoiceDetail
                       {
                           InvoiceAmount = 1,
                           InvoiceGoodsDescription = "test",
                           InvoiceDate = DateTime.Now,
                           InvoiceNumber = "2",
                           CurrencyTypeID = 1,
                           IsToPrint = true,                           
                           CertificateOfOriginItemDetail = new Customs.CertificateOfOrigins.Entities.TrackableCollection<CertificateOfOriginItemDetail>()
                       };
        }

        public CertificateOfOriginItemDetail CreateInvoiceItem(int certificateTypeId)
        {
            var item = new CertificateOfOriginItemDetail
                           {
                               CustomsItemID = 5799,
                               FullClassification = "010100",
                               GrossWeight = 1,
                               ItemGoodsDescription = "test test test test",
                               MarksAndNumbers = "test",
                               MeasurementUnitID = 1,
                               OriginCriterionID = 1,
                               PackingTypeID = 1,
                               Quantity = 1,
                               RowNum = 1
                           };

            switch (certificateTypeId)
            {
                case (int) ECertificateOfOriginType.IsrCol:
                    item.OriginCriterionID = 4;
                    break;
                case (int)ECertificateOfOriginType.MERCOSUR:
                    item.OriginCriterionID = 1;
                    break;
            }
            
            return item;
        }
    }
}
