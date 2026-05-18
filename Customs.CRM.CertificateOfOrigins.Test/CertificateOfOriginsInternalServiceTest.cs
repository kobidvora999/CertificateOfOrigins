using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Customs.CertificateOfOrigins.Entities;
using Customs.CRM.CertificateOfOrigins.BL;
using Customs.CRM.CertificateOfOrigins.Service;
using Customs.CRM.Entities;
using Customs.Inf.CertificateOfOrigins.EAISchema;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.Extensions;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.ORM;
using MalamTeam.Infrastructure.GeneralServices.Environment.Const;
using MalamTeam.Infrastructure.GeneralServices.Environment.Enums;
using MalamTeam.Infrastructure.GeneralServices.Stubs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Practices.Unity;
using Customs.CRM.CertificateOfOrigins.InternalCommon.Common;

namespace Customs.CRM.CertificateOfOrigins.Test
{
    [TestClass]
    public class CertificateOfOriginsInternalServiceTest : BaseTest
    {
        private const int CertServerSenderID = 3641;
        private const string AttachmentFolder = "R:\\Customs Projects Code\\Integration\\EAI Request Samples";
        private const string fileName = "R:\\Customs Projects Code\\Integration\\EAI Request Samples\\CertificateOfOrigin\\ErumedRequest.xml";
        private readonly CertificateOfOriginsInternalService _service = new CertificateOfOriginsInternalService();
        private readonly CertificateOfOriginsIncomingMessageService _incomingMessageService = new CertificateOfOriginsIncomingMessageService();
        private readonly TestUtil _testUtil = new TestUtil();


        #region InternalGetCertificateOfOriginsByFilter

        [TestMethod]
        public void CertificateOfOrigin_InternalGetCertificateOfOriginsByFilter_Success()
        {
            var certificate = _testUtil.CreateNewCertificateOfOrigin(fileName);

            var filter = new CertificateOfOriginFilter
                             {
                                 certificateNumber = certificate.CertificateNumber,
                                 certificateOfOriginStatusID = certificate.CertificateOfOriginStatusID
                             };

            var result = _service.InternalGetCertificateOfOriginsByFilter(filter);
            Assert.IsTrue(result.Count > 0);

            _testUtil.DeleteCertificateByExternalId(certificate.CertificateNumber);
        }

        [TestMethod]
        public void CertificateOfOrigin_InternalGetCertificateOfOriginsByFilter_Failure()
        {
            var filter = new CertificateOfOriginFilter
            {
                certificateNumber = "-11"
            };

            var result = _service.InternalGetCertificateOfOriginsByFilter(filter);

            Assert.AreEqual(result.Count, 0);
        }

        #endregion InternalGetCertificateOfOriginsByFilter

        #region InternalIsCertificateOfOriginByIdExist

        [TestMethod]
        public void CertificateOfOrigin_InternalIsCertificateOfOriginByIdExist_Success()
        {
            var certificate = _testUtil.CreateNewCertificateOfOrigin(fileName);

            var result = _service.InternalIsCertificateOfOriginByExternalIdExist(certificate.CertificateNumber);
            Assert.IsNotNull(result);

            _testUtil.DeleteCertificateByExternalId(certificate.CertificateNumber);
        }

        [TestMethod]
        public void CertificateOfOrigin_InternalIsCertificateOfOriginByIdExist_Failure()
        {
            var result = _service.InternalIsCertificateOfOriginByExternalIdExist("-11");
            Assert.IsNull(result);
        }

        #endregion InternalIsCertificateOfOriginByIdExist

        #region InternalGetCertificateOfOriginById

        [TestMethod]
        public void CertificateOfOrigin_InternalGetCertificateOfOriginById_Success()
        {
            var certificate = _testUtil.CreateNewCertificateOfOrigin(fileName);

            var result = _service.InternalGetCertificateOfOriginById(certificate.ID);
            Assert.IsNotNull(result);

            _testUtil.DeleteCertificateByExternalId(certificate.CertificateNumber);
        }

        [TestMethod]
        public void CertificateOfOrigin_InternalGetCertificateOfOriginById_Failure()
        {
            var result = _service.InternalGetCertificateOfOriginById(-1);
            Assert.IsNull(result);
        }

        #endregion InternalGetCertificateOfOriginById

        #region InternalSaveCertificateOfOrigin

        [TestMethod]
        public void CertificateOfOrigin_InternalSaveCertificateOfOrigin_Success()
        {
            var certificate = _testUtil.CreateNewCertificateOfOrigin(fileName);
            certificate.RequestReasonCode = (int) ERequestReason.CertificateUpdate;

            var result = _service.InternalSaveCertificateOfOrigin(certificate);
            Assert.AreEqual(result.RequestReasonCode, (int)ERequestReason.CertificateUpdate);

            _testUtil.DeleteCertificateByExternalId(certificate.CertificateNumber);
        }

        #endregion InternalSaveCertificateOfOrigin      


        #region Create Certificate If Not Exist for Templates Tests

        [TestMethod]
        public void CommonService_CertificateOfOrigin_CreateCertificateIfNotExist_Success()
        {
            var certificateOfOriginTestUtil = new TestUtil();
            var internalService = new CertificateOfOriginsInternalService();
            var certificate = internalService.GetCertificateOfOriginsByFilter(new CertificateOfOriginFilter());

            if (!certificate.IsNullOrEmpty()) return;

            const string searchFileName = "R:\\Customs Projects Code\\Integration\\EAI Request Samples\\CertificateOfOrigin\\ErumedRequest.xml";
            var newCertificate = certificateOfOriginTestUtil.CreateNewCertificateOfOrigin(searchFileName);
            Assert.IsNotNull(newCertificate);
        }

        #endregion Create Certificate If Not Exist for Templates Tests


        [TestMethod]
        public void InternalGetAuthenticationRequestByFilter_Failure()
        {
            var a = new GetImportAuthenticationRequestResult
                        {
                            DocumentID = 111,
                        };


              using (var uow = Container.Resolve<IUnitOfWork>(CRMConsts.CertificateOfOriginsUnitOfWork))
              {
                  var filter = new ImportAuthenticationRequestFilter
                              {
                                  FromRequestDate = DateTime.Now.AddDays(-3),
                                  ToRequestDate = DateTime.Now

                              };

                  var result = uow.Repository.ExecuteFunction<GetImportAuthenticationRequestResult>(filter).ToList();

              }
        }

        [TestMethod]
        public void HandleImportAuthenticationRequestDeliveryForImporterSent()
        {
            var service = new CertificateOfOriginsInternalService();
            var request = service.GetAuthenticationRequestByID(631386874);

            service.HandleImportAuthenticationRequestDeliveryReminderForImporterSent(request);
        }
    }
}
