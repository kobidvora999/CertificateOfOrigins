using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Customs.CertificateOfOrigins.Entities;
using Customs.CRM.CertificateOfOrigins.BL;
using Customs.CRM.CertificateOfOrigins.Service;
using Customs.CRM.Entities;
using Customs.Inf.CertificateOfOrigins.EAISchema;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.ORM;
using MalamTeam.Infrastructure.GeneralServices.Environment.Const;
using MalamTeam.Infrastructure.GeneralServices.Environment.Enums;
using MalamTeam.Infrastructure.GeneralServices.Stubs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Practices.Unity;
using Customs.Inf.WebMultiDotNetSupport.CertificateOfOrigins;

namespace Customs.CRM.CertificateOfOrigins.Test
{
    [TestClass]
    public class CertificateOfOriginsIncomingMessageServiceTest : BaseTest
    {

        [TestMethod]
        public void TestWebResults()
        {
            using (var uow = Container.Resolve<IUnitOfWork>(CRMConsts.CertificateOfOriginsUnitOfWork))
            {
                var bl = new CertificateOfOriginWebBL(uow);
                bl.GetCertificateDetailForWeb(new CertificateOfOriginsRequest
                {
                    CertificateOfOriginGuid = "1551A9AF-F6F8-44C3-A0FC-BDE91221928F"
                });
            }

        }


        //private const int CertServerSenderID = 3641;
        //private const string AttachmentFolder = "R:\\Customs Projects Code\\Integration\\EAI Request Samples";
        //private readonly CertificateOfOriginsInternalService _internalService = new CertificateOfOriginsInternalService();
        //private readonly CertificateOfOriginsIncomingMessageService _incomingMessageService = new CertificateOfOriginsIncomingMessageService();
        //private readonly TestUtil _testUtil = new TestUtil();


        //#region InternalGetPC_MSG2280_2281_CertificateOfOriginRequest

        //#region InternalGetPC_MSG2280_2281_CertificateOfOriginRequest_EURMED

        //[TestMethod]
        //public void CertificateOfOrigin_InternalGetPC_MSG2280_2281_CertificateOfOriginRequest_EURMED_Success()
        //{
        //    const string searchFileName = "R:\\Customs Projects Code\\Integration\\EAI Request Samples\\CertificateOfOrigin\\ErumedRequest.xml";
        //    var newCertificate = _testUtil.CreateNewCertificateOfOrigin(searchFileName);

        //    var certificate = _internalService.InternalGetCertificateOfOriginsByFilter(new CertificateOfOriginFilter { certificateNumber = newCertificate.CertificateNumber });
        //    Assert.IsNotNull(certificate);

        //    _testUtil.DeleteCertificateByExternalId(newCertificate.CertificateNumber);
        //}

        //[TestMethod]
        //public void CertificateOfOrigin_InternalGetPC_MSG2280_2281_CertificateOfOriginRequest_EURMED_Exception()
        //{
        //    try
        //    {
        //        const string searchFileName = "R:\\Customs Projects Code\\Integration\\EAI Request Samples\\CertificateOfOrigin\\ErumedRequestErrors.xml";

        //        var message = new PC_NG_2280_MSG01_CertificateOfOriginRequestRequest
        //        {
        //            CertServerSenderID = CertServerSenderID,
        //            CertServerSenderTypeID = (int)EEntityType.Customer,
        //            Content = ConvertGoldenToMessage<PC_NG_2280_MSG01_CertificateOfOriginRequest>(searchFileName, AttachmentFolder)
        //        };

        //        _incomingMessageService.InternalGetPC_MSG2280_2281_CertificateOfOriginRequest(message);
        //    }
        //    catch (Exception ex)
        //    {
        //        Assert.IsNull(null);
        //    }
        //}

        //#endregion InternalGetPC_MSG2280_2281_CertificateOfOriginRequest_EURMED

        //#region InternalGetPC_MSG2280_2281_CertificateOfOriginRequest_EUR1

        //[TestMethod]
        //public void CertificateOfOrigin_InternalGetPC_MSG2280_2281_CertificateOfOriginRequest_EUR1_Success()
        //{
        //    const string searchFileName = "R:\\Customs Projects Code\\Integration\\EAI Request Samples\\CertificateOfOrigin\\EUR1Request.xml";
        //    var newCertificate = _testUtil.CreateNewCertificateOfOrigin(searchFileName);

        //    var certificate = _internalService.InternalGetCertificateOfOriginsByFilter(new CertificateOfOriginFilter { certificateNumber = newCertificate.CertificateNumber });
        //    Assert.IsNotNull(certificate);

        //    _testUtil.DeleteCertificateByExternalId(newCertificate.CertificateNumber);
        //}

        //[TestMethod]
        //public void CertificateOfOrigin_InternalGetPC_MSG2280_2281_CertificateOfOriginRequest_EUR1_Exception()
        //{
        //    try
        //    {
        //        const string searchFileName = "R:\\Customs Projects Code\\Integration\\EAI Request Samples\\CertificateOfOrigin\\EUR1RequestErrors.xml";

        //        var message = new PC_NG_2280_MSG01_CertificateOfOriginRequestRequest
        //        {
        //            CertServerSenderID = CertServerSenderID,
        //            CertServerSenderTypeID = (int)EEntityType.Customer,
        //            Content = ConvertGoldenToMessage<PC_NG_2280_MSG01_CertificateOfOriginRequest>(searchFileName, AttachmentFolder)
        //        };

        //        _incomingMessageService.InternalGetPC_MSG2280_2281_CertificateOfOriginRequest(message);
        //    }
        //    catch (Exception ex)
        //    {
        //        Assert.IsNull(null);
        //    }
        //}

        //#endregion InternalGetPC_MSG2280_2281_CertificateOfOriginRequest_EUR1

        //#region InternalGetPC_MSG2280_2281_CertificateOfOriginRequest_MERCUSOR_Success

        //[TestMethod]
        //public void CertificateOfOrigin_InternalGetPC_MSG2280_2281_CertificateOfOriginRequest_MERCUSOR_Success()
        //{
        //    const string searchFileName = "R:\\Customs Projects Code\\Integration\\EAI Request Samples\\CertificateOfOrigin\\MercusorRequest.xml";
        //    var newCertificate = _testUtil.CreateNewCertificateOfOrigin(searchFileName);

        //    var certificate = _internalService.InternalGetCertificateOfOriginsByFilter(new CertificateOfOriginFilter { certificateNumber = newCertificate.CertificateNumber });
        //    Assert.IsNotNull(certificate);

        //    _testUtil.DeleteCertificateByExternalId(newCertificate.CertificateNumber);
        //}

        //[TestMethod]
        //public void CertificateOfOrigin_InternalGetPC_MSG2280_2281_CertificateOfOriginRequest_MERCUSOR_Exception()
        //{
        //    try
        //    {
        //        const string searchFileName = "R:\\Customs Projects Code\\Integration\\EAI Request Samples\\CertificateOfOrigin\\MercusorRequestErrors.xml";

        //        var message = new PC_NG_2280_MSG01_CertificateOfOriginRequestRequest
        //        {
        //            CertServerSenderID = CertServerSenderID,
        //            CertServerSenderTypeID = (int)EEntityType.Customer,
        //            Content = ConvertGoldenToMessage<PC_NG_2280_MSG01_CertificateOfOriginRequest>(searchFileName, AttachmentFolder)
        //        };

        //        _incomingMessageService.InternalGetPC_MSG2280_2281_CertificateOfOriginRequest(message);
        //    }
        //    catch (Exception ex)
        //    {
        //        Assert.IsNull(null);
        //    }
        //}

        //#endregion InternalGetPC_MSG2280_2281_CertificateOfOriginRequest_MERCUSOR_Success

        ////no trade agreement for colombia in DB
        //#region InternalGetPC_MSG2280_2281_CertificateOfOriginRequest_COLUMBIA_Success

        ////[TestMethod]
        ////public void CertificateOfOrigin_InternalGetPC_MSG2280_2281_CertificateOfOriginRequest_COLUMBIA_Success()
        ////{
        ////    const string searchFileName = "R:\\Customs Projects Code\\Integration\\EAI Request Samples\\CertificateOfOrigin\\ColumbiaRequest.xml";
        ////    var newCertificate = _testUtil.CreateNewCertificateOfOrigin(searchFileName);

        ////    var certificate = _internalService.InternalGetCertificateOfOriginsByFilter(new CertificateOfOriginFilter { certificateNumber = newCertificate.CertificateNumber });
        ////    Assert.IsNotNull(certificate);

        ////    _testUtil.DeleteCertificateByExternalId(newCertificate.CertificateNumber);
        ////}

        ////[TestMethod]
        ////public void CertificateOfOrigin_InternalGetPC_MSG2280_2281_CertificateOfOriginRequest_COLUMBIA_Exception()
        ////{
        ////    try
        ////    {
        ////        const string searchFileName = "R:\\Customs Projects Code\\Integration\\EAI Request Samples\\CertificateOfOrigin\\ColumbiaRequestErrors.xml";

        ////        var message = new PC_NG_2280_MSG01_CertificateOfOriginRequestRequest
        ////        {
        ////            CertServerSenderID = CertServerSenderID,
        ////            CertServerSenderTypeID = (int)EEntityType.Customer,
        ////            Content = ConvertGoldenToMessage<PC_NG_2280_MSG01_CertificateOfOriginRequest>(searchFileName, AttachmentFolder)
        ////        };

        ////        _incomingMessageService.InternalGetPC_MSG2280_2281_CertificateOfOriginRequest(message);
        ////    }
        ////    catch (Exception ex)
        ////    {
        ////        Assert.IsNull(null);
        ////    }
        ////}

        //#endregion InternalGetPC_MSG2280_2281_CertificateOfOriginRequest_COLUMBIA_Success

        //#region InternalGetPC_MSG2280_2281_CertificateOfOriginRequest_NonManipulationRequest_Success

        //[TestMethod]
        //public void CertificateOfOrigin_InternalGetPC_MSG2280_2281_CertificateOfOriginRequest_NonManipulationRequest_Success()
        //{
        //    const string searchFileName = "R:\\Customs Projects Code\\Integration\\EAI Request Samples\\CertificateOfOrigin\\NonManipulationRequest.xml";
        //    var newCertificate = _testUtil.CreateNewCertificateOfOrigin(searchFileName);

        //    var certificate = _internalService.InternalGetCertificateOfOriginsByFilter(new CertificateOfOriginFilter { certificateNumber = newCertificate.CertificateNumber });
        //    Assert.IsNotNull(certificate);

        //    _testUtil.DeleteCertificateByExternalId(newCertificate.CertificateNumber);
        //}

        //[TestMethod]
        //public void CertificateOfOrigin_InternalGetPC_MSG2280_2281_CertificateOfOriginRequest_NonManipulationRequest_Exception()
        //{
        //    try
        //    {
        //        const string searchFileName = "R:\\Customs Projects Code\\Integration\\EAI Request Samples\\CertificateOfOrigin\\NonManipulationRequestErrors.xml";

        //        var message = new PC_NG_2280_MSG01_CertificateOfOriginRequestRequest
        //        {
        //            CertServerSenderID = CertServerSenderID,
        //            CertServerSenderTypeID = (int)EEntityType.Customer,
        //            Content = ConvertGoldenToMessage<PC_NG_2280_MSG01_CertificateOfOriginRequest>(searchFileName, AttachmentFolder)
        //        };

        //        _incomingMessageService.InternalGetPC_MSG2280_2281_CertificateOfOriginRequest(message);
        //    }
        //    catch (Exception ex)
        //    {
        //        Assert.IsNull(null);
        //    }
        //}

        //#endregion InternalGetPC_MSG2280_2281_CertificateOfOriginRequest_NonManipulationRequest_Success

        //#region InternalGetPC_MSG2280_2281_CertificateOfOriginRequest_RetrospectiveCertificate

        //[TestMethod]
        //public void CertificateOfOrigin_InternalGetPC_MSG2280_2281_CertificateOfOriginRequest_RetrospectiveCertificate_Success()
        //{
        //    const string searchFileName = "R:\\Customs Projects Code\\Integration\\EAI Request Samples\\CertificateOfOrigin\\RetrospectiveCertificate.xml";
        //    var newCertificate = _testUtil.CreateNewCertificateOfOrigin(searchFileName);

        //    var certificate = _internalService.InternalGetCertificateOfOriginsByFilter(new CertificateOfOriginFilter { certificateNumber = newCertificate.CertificateNumber });
        //    Assert.IsNotNull(certificate);

        //    _testUtil.DeleteCertificateByExternalId(newCertificate.CertificateNumber);
        //}

        //#endregion InternalGetPC_MSG2280_2281_CertificateOfOriginRequest_RetrospectiveCertificate

        //#region InternalGetPC_MSG2280_2281_CertificateOfOriginRequest_CertificateUpdate

        //[TestMethod]
        //public void CertificateOfOrigin_InternalGetPC_MSG2280_2281_CertificateOfOriginRequest_CertificateUpdate_Success()
        //{
        //    const string searchFileNameUpdateCertificate = "R:\\Customs Projects Code\\Integration\\EAI Request Samples\\CertificateOfOrigin\\ErumedRequest.xml";
        //    var certificateToUpdate = _testUtil.CreateNewCertificateOfOrigin(searchFileNameUpdateCertificate);
        //    Assert.IsNotNull(certificateToUpdate);

        //    const string searchFileName = "R:\\Customs Projects Code\\Integration\\EAI Request Samples\\CertificateOfOrigin\\CertificateUpdate.xml";
        //    var message = new PC_NG_2280_MSG01_CertificateOfOriginRequestRequest
        //    {
        //        CertServerSenderID = CertServerSenderID,
        //        CertServerSenderTypeID = (int)EEntityType.Customer,
        //        Content = ConvertGoldenToMessage<PC_NG_2280_MSG01_CertificateOfOriginRequest>(searchFileName, AttachmentFolder)
        //    };

        //    _testUtil.UpdateDates(message);
        //    message.Content.AgentRequest.certificateID = certificateToUpdate.CertificateNumber;

        //    var response = _incomingMessageService.InternalGetPC_MSG2280_2281_CertificateOfOriginRequest(message);
        //    var certificateNumber = response.Content.CertificateOfOriginRequestFeedback.certificateID;
        //    var certificate = _internalService.InternalGetCertificateOfOriginsByFilter(new CertificateOfOriginFilter
        //                                                                                   {
        //                                                                                       certificateNumber = certificateNumber
        //                                                                                   });
        //    Assert.IsNotNull(certificate);

        //    _testUtil.DeleteCertificateByExternalId(certificateToUpdate.CertificateNumber);
        //    _testUtil.DeleteCertificateByExternalId(certificateNumber);
        //}

        //#endregion InternalGetPC_MSG2280_2281_CertificateOfOriginRequest_CertificateUpdate

        //#region InternalGetPC_MSG2280_2281_CertificateOfOriginRequest_CertificateReplacement

        //[TestMethod]
        //public void CertificateOfOrigin_InternalGetPC_MSG2280_2281_CertificateOfOriginRequest_CertificateReplacement_Success()
        //{
        //    var certificateIdToCancel = CreateCertificateToCancelAndGetExternalId();

        //    const string searchFileName = "R:\\Customs Projects Code\\Integration\\EAI Request Samples\\CertificateOfOrigin\\CertificateReplacement.xml";

        //    var message = new PC_NG_2280_MSG01_CertificateOfOriginRequestRequest
        //    {
        //        CertServerSenderID = CertServerSenderID,
        //        CertServerSenderTypeID = (int)EEntityType.Customer,
        //        Content = ConvertGoldenToMessage<PC_NG_2280_MSG01_CertificateOfOriginRequest>(searchFileName, AttachmentFolder)
        //    };

        //    _testUtil.UpdateDates(message);

        //    message.Content.AgentRequest.certificateIdToCancel = certificateIdToCancel;
        //    message.Content.AgentRequest.replacementReason = "test";

        //    var response = _incomingMessageService.InternalGetPC_MSG2280_2281_CertificateOfOriginRequest(message);
        //    var certificateNumber = response.Content.CertificateOfOriginRequestFeedback.certificateID;

        //    var certificate = _testUtil.GetCertificateByExternalId(certificateNumber);
        //    Assert.IsNotNull(certificate);

        //    _testUtil.DeleteCertificateByExternalId(certificateNumber);
        //    _testUtil.DeleteCertificateByExternalId(certificateIdToCancel);
        //}

        //private string CreateCertificateToCancelAndGetExternalId()
        //{
        //    const string searchFileName = "R:\\Customs Projects Code\\Integration\\EAI Request Samples\\CertificateOfOrigin\\ErumedRequest.xml";

        //    var message = new PC_NG_2280_MSG01_CertificateOfOriginRequestRequest
        //    {
        //        CertServerSenderID = CertServerSenderID,
        //        CertServerSenderTypeID = (int)EEntityType.Customer,
        //        Content = ConvertGoldenToMessage<PC_NG_2280_MSG01_CertificateOfOriginRequest>(searchFileName, AttachmentFolder)
        //    };            

        //    _testUtil.UpdateDates(message);

        //    var response = _incomingMessageService.InternalGetPC_MSG2280_2281_CertificateOfOriginRequest(message);
        //    var certificateNumber = response.Content.CertificateOfOriginRequestFeedback.certificateID;

        //    var certificateToCancel = _testUtil.GetCertificateByExternalId(certificateNumber);
        //    ObjectWithChangeTrackerExtensions.StartTracking(certificateToCancel);
        //    certificateToCancel.CertificateOfOriginStatusID = (int) ECertificateOfOriginStatus.Published;
        //    certificateToCancel.AcceptChanges();
        //    _internalService.InternalSaveCertificateOfOrigin(certificateToCancel);

        //    return certificateNumber;
        //}

        //#endregion InternalGetPC_MSG2280_2281_CertificateOfOriginRequest_CertificateReplacement

        //#region InternalGetPC_MSG2280_2281_CertificateOfOriginRequest_ImportCertificateReplacement

        //[TestMethod]
        //public void CertificateOfOrigin_InternalGetPC_MSG2280_2281_CertificateOfOriginRequest_ImportCertificateReplacement_Success()
        //{
        //    const string searchFileName = "R:\\Customs Projects Code\\Integration\\EAI Request Samples\\CertificateOfOrigin\\ImportCertificateReplacement.xml";
        //    var newCertificate = _testUtil.CreateNewCertificateOfOrigin(searchFileName);

        //    var certificate = _internalService.InternalGetCertificateOfOriginsByFilter(new CertificateOfOriginFilter { certificateNumber = newCertificate.CertificateNumber });
        //    Assert.IsNotNull(certificate);

        //    _testUtil.DeleteCertificateByExternalId(newCertificate.CertificateNumber);
        //}

        //#endregion InternalGetPC_MSG2280_2281_CertificateOfOriginRequest_ImportCertificateReplacement

        //#endregion InternalGetPC_MSG2280_2281_CertificateOfOriginRequest        


        //#region Templates with 1 Page

        //#region InternalGetPC_MSG2280_2281_CertificateOfOriginRequest_EURMED_1Page

        //[TestMethod]
        //public void CertificateOfOrigin_InternalGetPC_MSG2280_2281_CertificateOfOriginRequest_EURMED_1Page_Success()
        //{
        //    const string searchFileName = "R:\\Customs Projects Code\\Integration\\EAI Request Samples\\CertificateOfOrigin\\ErumedRequest.xml";
        //    var newCertificate = _testUtil.CreateNewCertificateOfOrigin(searchFileName);

        //    newCertificate.CertificateOfOriginStatusID = (int)ECertificateOfOriginStatus.Published;
        //    newCertificate.AcceptChanges();
        //    _internalService.InternalSaveCertificateOfOrigin(newCertificate);

        //    var certificate = _internalService.InternalGetCertificateOfOriginsByFilter(new CertificateOfOriginFilter { certificateNumber = newCertificate.CertificateNumber });
        //    Assert.IsNotNull(certificate);

        //    _testUtil.DeleteCertificateByExternalId(newCertificate.CertificateNumber);
        //}

        //#endregion InternalGetPC_MSG2280_2281_CertificateOfOriginRequest_EURMED_1Page

        //#region InternalGetPC_MSG2280_2281_CertificateOfOriginRequest_EUR1_1Page

        //[TestMethod]
        //public void CertificateOfOrigin_InternalGetPC_MSG2280_2281_CertificateOfOriginRequest_EUR1_1Page_Success()
        //{
        //    const string searchFileName = "R:\\Customs Projects Code\\Integration\\EAI Request Samples\\CertificateOfOrigin\\EUR1Request.xml";
        //    var newCertificate = _testUtil.CreateNewCertificateOfOrigin(searchFileName);

        //    newCertificate.CertificateOfOriginStatusID = (int)ECertificateOfOriginStatus.Published;
        //    newCertificate.AcceptChanges();
        //    _internalService.InternalSaveCertificateOfOrigin(newCertificate);

        //    var certificate = _internalService.InternalGetCertificateOfOriginsByFilter(new CertificateOfOriginFilter { certificateNumber = newCertificate.CertificateNumber });
        //    Assert.IsNotNull(certificate);

        //    _testUtil.DeleteCertificateByExternalId(newCertificate.CertificateNumber);
        //}

        //#endregion InternalGetPC_MSG2280_2281_CertificateOfOriginRequest_EUR1_1Page

        //#region InternalGetPC_MSG2280_2281_CertificateOfOriginRequest_MERCUSOR_1Page

        //[TestMethod]
        //public void CertificateOfOrigin_InternalGetPC_MSG2280_2281_CertificateOfOriginRequest_MERCUSOR_1Page_Success()
        //{
        //    const string searchFileName = "R:\\Customs Projects Code\\Integration\\EAI Request Samples\\CertificateOfOrigin\\MercusorRequest.xml";
        //    var newCertificate = _testUtil.CreateNewCertificateOfOrigin(searchFileName);

        //    newCertificate.CertificateOfOriginStatusID = (int)ECertificateOfOriginStatus.Published;
        //    newCertificate.AcceptChanges();
        //    _internalService.InternalSaveCertificateOfOrigin(newCertificate);

        //    var certificate = _internalService.InternalGetCertificateOfOriginsByFilter(new CertificateOfOriginFilter { certificateNumber = newCertificate.CertificateNumber });
        //    Assert.IsNotNull(certificate);

        //    _testUtil.DeleteCertificateByExternalId(newCertificate.CertificateNumber);
        //}

        //#endregion InternalGetPC_MSG2280_2281_CertificateOfOriginRequest_MERCUSOR_1Page

        ////no trade agreement for colombia in DB
        //#region InternalGetPC_MSG2280_2281_CertificateOfOriginRequest_COLUMBIA_1Page

        ////[TestMethod]
        ////public void CertificateOfOrigin_InternalGetPC_MSG2280_2281_CertificateOfOriginRequest_COLUMBIA_1Page_Success()
        ////{
        ////    const string searchFileName = "R:\\Customs Projects Code\\Integration\\EAI Request Samples\\CertificateOfOrigin\\ColumbiaRequest.xml";
        ////    var newCertificate = _testUtil.CreateNewCertificateOfOrigin(searchFileName);

        ////    newCertificate.CertificateOfOriginStatusID = (int)ECertificateOfOriginStatus.Published;
        ////    newCertificate.AcceptChanges();
        ////    _internalService.InternalSaveCertificateOfOrigin(newCertificate);

        ////    var certificate = _internalService.InternalGetCertificateOfOriginsByFilter(new CertificateOfOriginFilter { certificateNumber = newCertificate.CertificateNumber });
        ////    Assert.IsNotNull(certificate);

        ////    _testUtil.DeleteCertificateByExternalId(newCertificate.CertificateNumber);
        ////}

        //#endregion InternalGetPC_MSG2280_2281_CertificateOfOriginRequest_COLUMBIA_1Page

        //#region InternalGetPC_MSG2280_2281_CertificateOfOriginRequest_NonManipulationRequest_1Page

        //[TestMethod]
        //public void CertificateOfOrigin_InternalGetPC_MSG2280_2281_CertificateOfOriginRequest_NonManipulationRequest_1Page_Success()
        //{
        //    const string searchFileName = "R:\\Customs Projects Code\\Integration\\EAI Request Samples\\CertificateOfOrigin\\NonManipulationRequest.xml";
        //    var newCertificate = _testUtil.CreateNewCertificateOfOrigin(searchFileName);

        //    newCertificate.CertificateOfOriginStatusID = (int)ECertificateOfOriginStatus.Published;
        //    newCertificate.AcceptChanges();
        //    _internalService.InternalSaveCertificateOfOrigin(newCertificate);

        //    var certificate = _internalService.InternalGetCertificateOfOriginsByFilter(new CertificateOfOriginFilter { certificateNumber = newCertificate.CertificateNumber });
        //    Assert.IsNotNull(certificate);

        //    _testUtil.DeleteCertificateByExternalId(newCertificate.CertificateNumber);
        //}

        //#endregion InternalGetPC_MSG2280_2281_CertificateOfOriginRequest_NonManipulationRequest_1Page

        //#endregion Templates with 1 Page


        //#region Templates with 2 Pages

        //#region InternalGetPC_MSG2280_2281_CertificateOfOriginRequest_EURMED2

        //[TestMethod]
        //public void CertificateOfOrigin_InternalGetPC_MSG2280_2281_CertificateOfOriginRequest_EURMED2_Success()
        //{
        //    const string searchFileName = "R:\\Customs Projects Code\\Integration\\EAI Request Samples\\CertificateOfOrigin\\ErumedRequest.xml";
        //    var newCertificate = _testUtil.CreateNewCertificateOfOrigin(searchFileName);

        //    using (var uow = Container.Resolve<IUnitOfWork>(CRMConsts.CertificateOfOriginsUnitOfWork))
        //    {
        //        var bl = new CertificateOfOriginsBL(uow);

        //        var invoice = _testUtil.CreateInvoiceFor2Pages(newCertificate.TypeID);
        //        newCertificate.CertificateOfOriginInvoiceDetail.Add(invoice);

        //        newCertificate.AcceptChanges();
        //        bool isStatusChanged;
        //        bool isRemarksChanged;
        //        bl.SaveCertificateOfOrigin(newCertificate, null, null, out isStatusChanged, out isRemarksChanged);

        //    }

        //    newCertificate.CertificateOfOriginStatusID = (int)ECertificateOfOriginStatus.Published;
        //    newCertificate.AcceptChanges();
        //    _internalService.InternalSaveCertificateOfOrigin(newCertificate);

        //    var certificate = _internalService.InternalGetCertificateOfOriginsByFilter(new CertificateOfOriginFilter { certificateNumber = newCertificate.CertificateNumber });
        //    Assert.IsNotNull(certificate);

        //    _testUtil.DeleteCertificateByExternalId(newCertificate.CertificateNumber);
        //}

        //#endregion InternalGetPC_MSG2280_2281_CertificateOfOriginRequest_EURMED2

        //#region InternalGetPC_MSG2280_2281_CertificateOfOriginRequest_EUR1_2

        //[TestMethod]
        //public void CertificateOfOrigin_InternalGetPC_MSG2280_2281_CertificateOfOriginRequest_EUR1_2_Success()
        //{
        //    const string searchFileName = "R:\\Customs Projects Code\\Integration\\EAI Request Samples\\CertificateOfOrigin\\EUR1Request.xml";
        //    var newCertificate = _testUtil.CreateNewCertificateOfOrigin(searchFileName);

        //    using (var uow = Container.Resolve<IUnitOfWork>(CRMConsts.CertificateOfOriginsUnitOfWork))
        //    {
        //        var bl = new CertificateOfOriginsBL(uow);

        //        var invoice = _testUtil.CreateInvoiceFor2Pages(newCertificate.TypeID);
        //        newCertificate.CertificateOfOriginInvoiceDetail.Add(invoice);

        //        newCertificate.AcceptChanges();
        //        bool isStatusChanged;
        //        bool isRemarksChanged;
        //        bl.SaveCertificateOfOrigin(newCertificate, null, null, out isStatusChanged, out isRemarksChanged);

        //    }

        //    newCertificate.CertificateOfOriginStatusID = (int)ECertificateOfOriginStatus.Published;
        //    newCertificate.AcceptChanges();
        //    _internalService.InternalSaveCertificateOfOrigin(newCertificate);

        //    var certificate = _internalService.InternalGetCertificateOfOriginsByFilter(new CertificateOfOriginFilter { certificateNumber = newCertificate.CertificateNumber });
        //    Assert.IsNotNull(certificate);

        //    _testUtil.DeleteCertificateByExternalId(newCertificate.CertificateNumber);
        //}

        //#endregion InternalGetPC_MSG2280_2281_CertificateOfOriginRequest_EUR1_2

        //#region InternalGetPC_MSG2280_2281_CertificateOfOriginRequest_MERCUSOR2_Success

        //[TestMethod]
        //public void CertificateOfOrigin_InternalGetPC_MSG2280_2281_CertificateOfOriginRequest_MERCUSOR2_Success()
        //{
        //    const string searchFileName = "R:\\Customs Projects Code\\Integration\\EAI Request Samples\\CertificateOfOrigin\\MercusorRequest.xml";
        //    var newCertificate = _testUtil.CreateNewCertificateOfOrigin(searchFileName);

        //    using (var uow = Container.Resolve<IUnitOfWork>(CRMConsts.CertificateOfOriginsUnitOfWork))
        //    {
        //        var bl = new CertificateOfOriginsBL(uow);

        //        var invoice = _testUtil.CreateInvoiceFor2Pages(newCertificate.TypeID);
        //        newCertificate.CertificateOfOriginInvoiceDetail.Add(invoice);

        //        newCertificate.AcceptChanges();
        //        bool isStatusChanged;
        //        bool isRemarksChanged;
        //        bl.SaveCertificateOfOrigin(newCertificate, null, null, out isStatusChanged, out isRemarksChanged);

        //    }

        //    newCertificate.CertificateOfOriginStatusID = (int)ECertificateOfOriginStatus.Published;
        //    newCertificate.AcceptChanges();
        //    _internalService.InternalSaveCertificateOfOrigin(newCertificate);

        //    var certificate = _internalService.InternalGetCertificateOfOriginsByFilter(new CertificateOfOriginFilter { certificateNumber = newCertificate.CertificateNumber });
        //    Assert.IsNotNull(certificate);

        //    _testUtil.DeleteCertificateByExternalId(newCertificate.CertificateNumber);
        //}

        //#endregion InternalGetPC_MSG2280_2281_CertificateOfOriginRequest_MERCUSOR2_Success

        ////no trade agreement for colombia in DB
        //#region InternalGetPC_MSG2280_2281_CertificateOfOriginRequest_COLUMBIA2_Success

        ////[TestMethod]
        ////public void CertificateOfOrigin_InternalGetPC_MSG2280_2281_CertificateOfOriginRequest_COLUMBIA2_Success()
        ////{
        ////    const string searchFileName = "R:\\Customs Projects Code\\Integration\\EAI Request Samples\\CertificateOfOrigin\\ColumbiaRequest.xml";
        ////    var newCertificate = _testUtil.CreateNewCertificateOfOrigin(searchFileName);

        ////    using (var uow = Container.Resolve<IUnitOfWork>(CRMConsts.CertificateOfOriginsUnitOfWork))
        ////    {
        ////        var bl = new CertificateOfOriginsBL(uow);

        ////        var invoice = _testUtil.CreateInvoiceFor2Pages(newCertificate.TypeID);
        ////        newCertificate.CertificateOfOriginInvoiceDetail.Add(invoice);

        ////        newCertificate.AcceptChanges();
        ////        bool isStatusChanged;
        ////        bool isRemarksChanged;
        ////        bl.SaveCertificateOfOrigin(newCertificate, null, null, out isStatusChanged, out isRemarksChanged);

        ////    }

        ////    newCertificate.CertificateOfOriginStatusID = (int)ECertificateOfOriginStatus.Published;
        ////    newCertificate.AcceptChanges();
        ////    _internalService.InternalSaveCertificateOfOrigin(newCertificate);

        ////    var certificate = _internalService.InternalGetCertificateOfOriginsByFilter(new CertificateOfOriginFilter { certificateNumber = newCertificate.CertificateNumber });
        ////    Assert.IsNotNull(certificate);

        ////    _testUtil.DeleteCertificateByExternalId(newCertificate.CertificateNumber);
        ////}

        //#endregion InternalGetPC_MSG2280_2281_CertificateOfOriginRequest_COLUMBIA2_Success

        //#endregion Templates with 2 Pages
    }
}
