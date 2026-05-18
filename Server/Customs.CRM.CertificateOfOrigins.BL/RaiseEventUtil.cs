using System.Collections.Generic;
using System.Linq;
using Customs.CertificateOfOrigins.Entities;
using Customs.CRM.Entities;
using MalamTeam.Infrastructure.GeneralServices.BaseLayer;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.Interfaces;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.Extensions;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.ORM;
using MalamTeam.Infrastructure.GeneralServices.Environment.Const;
using Microsoft.Practices.Unity;
using MalamTeam.Infrastructure.GeneralServices.CommonBase;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.Base_Entities;
using MalamTeam.Infrastructure.GeneralServices.Environment.Enums;
using Customs.CRM.CertificateOfOrigins.InternalCommon;
using Customs.CRM.CertificateOfOrigins.BL.ServiceAdapters;
using Customs.CRM.CertificateOfOrigins.ExternalCommon.Common;

namespace Customs.CRM.CertificateOfOrigins.BL
{
    public class RaiseEventUtil : BaseLayer
    {

        /// <summary>
        /// Gets the event util.
        /// </summary>
        /// <value>The event util.</value>
        private IEventUtil EventUtil
        {
            get { return Container.Resolve<IEventUtil>(); }
        }        
        //private bool IsRelevantToCurrentVersion
        //{
        //    get { return Configuration.GetConfig<int>(CertificateOfOriginsConstants.CustomsVersionNumber) == 2; }
        //}
        private ISystemTablesUtilBase SystemTablesUtil { get { return Container.Resolve<ISystemTablesUtilBase>(); } }

        public void RaiseCertificateOfOriginEvents(CertificateOfOrigin request, ERequestReason? requestReason, int? certificateToUpdateId)
        {
            var statusId = request.CertificateOfOriginStatusID;

            switch (statusId)
            {
                case (int)ECertificateOfOriginStatus.Received:
                    RaiseEventForStatusReceived(request, certificateToUpdateId);      
                    break;
                case (int) ECertificateOfOriginStatus.Rejected:
                    RaiseEventCertificateOfOriginUserDeniedCertificate(request);
                    break;
                case (int) ECertificateOfOriginStatus.Cancelled:
                    RaiseEventForStatusCancelled(request, requestReason);   
                    break;                
                case (int) ECertificateOfOriginStatus.PendingRelease:
                    RaiseEventCertificateOfOriginUserApprovedCertificate(request);
                    break;
                case (int) ECertificateOfOriginStatus.Published:
                    //RaiseEventCertificateOfOriginCertificateIssued(request);
                    break;                                
                case (int)ECertificateOfOriginStatus.DeclarationMatch:
                    RaiseEventCertificateOfOriginCertificateMatchDeclaration(request);                                        
                    RaiseNewCertificateOfOriginCreatedEvent(request); //פתח משימה למעריך PC_Tsk01_NewCertificateOfOriginCheck                       
                    break;                
                case (int)ECertificateOfOriginStatus.DeclarationMismatch:
                    RaiseEventCertificateOfOriginCertificateDeclarationMismatch(request);                                       
                    RaiseAssessorCertificateOfOriginDecision(request); // פתח משימה למעריך PC_Tsk02_AssessorCertificateOfOriginDecision,
                    break;
            }            
        }

        private void RaiseEventForStatusReceived(CertificateOfOrigin request, int? certificateToUpdateId)
        {
            RaiseEventCertificateOfOriginApplicationReceived(request);

            if (request.RequestReasonCode == (int)ERequestReason.CertificateUpdate)
            {
                RaiseEventCertificateOfOriginApplicationCorrected(request, certificateToUpdateId);
            }
            RaiseNewCertificateOfOriginCreatedEvent(request); //פתח משימה PC_Tsk01_NewCertificateOfOriginCheck
        }

        private void RaiseEventForStatusCancelled(CertificateOfOrigin request, ERequestReason? requestReason)
        {
            if (requestReason.HasValue)
            {
                switch (requestReason.Value)
                {
                    case ERequestReason.CertificateUpdate:
                        RaiseEventCertificateOfOriginApplicationCorrected(request, null);
                        break;
                    case ERequestReason.CertificateReplacement:
                        RaiseEventCertificateOfOriginCertificateReplaced(request);
                        break;
                }
            }
            else
            {
                RaiseEventUserCancelledCertificate(request);
            }

                    
        }

        private void RaiseNewCertificateOfOriginCreatedEvent(CertificateOfOrigin request)
        {
            //CR 156814
            if (request.RequestReasonCode == (int) ERequestReason.GetRequestStatus || request.RequestReasonCode == (int) ERequestReason.CertificateCancellation ||
                request.RequestReasonCode == (int)ERequestReason.EmptyCertificate || request.RequestReasonCode ==  (int)ERequestReason.Draft)
                return;
            EventUtilArguments arg;
            //if (request.RequestReasonCode == (int)ERequestReason.EmptyCertificate ||
            //    request.RequestReasonCode == (int)ERequestReason.Draft)
            //{
            //    arg = new EventUtilArguments(EEventType.NewCertificateOfOriginCreatedWithoutTask, request);
            //}
            //else
            //{
                if (Configuration.GetConfig<bool>(CertificateOfOriginsConsts.IsExportDeclarationActive))
                    return;
               
                using (var uow = Container.Resolve<IUnitOfWork>(CRMConsts.CertificateOfOriginsUnitOfWork))
                {
                    var certificateOfOriginsBL = new CertificateOfOriginsBL(uow);
                    arg = certificateOfOriginsBL.RaiseTaskNewCertificateOfOriginCheck(request, EEventType.CertificateOfOriginNewCertificateOfOriginCreated);
                }
            //}
            EventUtil.RaiseEvent(arg);
        }
      
        private void RaiseAssessorCertificateOfOriginDecision(CertificateOfOrigin request)
        {            
            // פתח משימה למעריך PC_Tsk02_AssessorCertificateOfOriginDecision
            //TODO: כולל הודעות שגיאה שהתקבלו מן החוק העסקי PC_BR02_CertificateOfOriginVSDeclarationCheck
            //temp -  todo
        }

        private void RaiseEventUserCancelledCertificate(CertificateOfOrigin request)
        {
            var args = new EventUtilArguments(EEventType.CertificateOfOriginUserCancelledCertificate, new VirtualEntity(request));
            args.AdditionalInfo = request.RejectCancelReason;

            EventUtil.RaiseEvent(args);
        }

        private void RaiseEventCertificateOfOriginCertificateIssued(CertificateOfOrigin request)
        {
            var args = new EventUtilArguments(EEventType.CertificateOfOriginCertificateIssued, new VirtualEntity(request));
            EventUtil.RaiseEvent(args);
        }

        private void RaiseEventCertificateOfOriginUserDeniedCertificate(CertificateOfOrigin request)
        {
            var args = new EventUtilArguments(EEventType.CertificateOfOriginUserDeniedCertificate, new VirtualEntity(request));
            args.AdditionalInfo = request.RejectCancelReason;

            EventUtil.RaiseEvent(args);
        }

        private void RaiseEventCertificateOfOriginCertificateReplaced(CertificateOfOrigin request)
        {
            var args = new EventUtilArguments(EEventType.CertificateOfOriginCertificateReplaced, new VirtualEntity(request));
            //args.AdditionalInfo = TODO: New Certificate Number

            EventUtil.RaiseEvent(args);
        }

        private void RaiseEventCertificateOfOriginUserApprovedCertificate(CertificateOfOrigin request)
        {
            var args = new EventUtilArguments(EEventType.CertificateOfOriginUserApprovedCertificate, new VirtualEntity(request));
            EventUtil.RaiseEvent(args);
            if (!Configuration.GetConfig<bool>(CertificateOfOriginsConsts.IsExportDeclarationActive))
                return;

            var exportDealFileExternalServiceAdapter = Container.Resolve<IExportDealFileExternalServiceAdapter>();
            var exportDeclarationDetailsDTO = exportDealFileExternalServiceAdapter.GetExportDeclarationDetailsForCertificateOfOrigion(request.LeadDocumentID, request.ExportDeclarationNumber);
            if (exportDeclarationDetailsDTO != null)
            {
                var exportDeclarationInfoDTO = exportDealFileExternalServiceAdapter.GetExportDeclarationInfoForPC(exportDeclarationDetailsDTO.LeadDocumentID);
                if (exportDeclarationInfoDTO != null &&( exportDeclarationInfoDTO.LeadDocumentState == (int)ELeadDocumentState.Released|| exportDeclarationInfoDTO.LeadDocumentState == (int)ELeadDocumentState.Paroled || exportDeclarationInfoDTO.LeadDocumentState == (int)ELeadDocumentState.Closed))
                {
                    UpdateCetificateOfOriginsDTO updateCetificateOfOriginsDTO = new UpdateCetificateOfOriginsDTO()
                    {
                        CertificateOfOriginsIDs = new List<int>() { request.ID },
                        DestinationCoutryID = exportDeclarationInfoDTO.DestinationCoutryID,
                        ExporterCustomerID = exportDeclarationInfoDTO.ExporterCustomerID,
                        ExportDeclarationNum = request.ExportDeclarationNumber,
                        LeadDocumentID = exportDeclarationInfoDTO.LeadDocumentID,
                        ExportInvoiceInfoDTOList = exportDeclarationInfoDTO.ExportInvoiceInfoDTOList
                    };
                   
                    using (var uow = Container.Resolve<IUnitOfWork>(CRMConsts.CertificateOfOriginsUnitOfWork))
                    {
                        var certificateOfOriginsBL = new CertificateOfOriginsBL(uow);
                        certificateOfOriginsBL.DeclarationReleased(updateCetificateOfOriginsDTO,request,false);
                    }
                }
            }
        }

        private void RaiseEventCertificateOfOriginApplicationReceived(CertificateOfOrigin request)
        {
            var args = new EventUtilArguments(EEventType.CertificateOfOriginApplicationReceived, new VirtualEntity(request));
            EventUtil.RaiseEvent(args);
        }

        private void RaiseEventCertificateOfOriginApplicationCorrected(CertificateOfOrigin request, int? certificateToUpdateId)
        {
            var args = new EventUtilArguments(EEventType.CertificateOfOriginApplicationCorrected,
                                              new VirtualEntity(request));

            if (certificateToUpdateId.HasValue)
            {
                 args.RelatedEntities = new  List<IEntity>
                                            {
                                                new VirtualEntity(certificateToUpdateId.Value, (int) EEntityType.CertificateOfOrigin)
                                            };
            }
            
            EventUtil.RaiseEvent(args);
        }

        private void RaiseEventCertificateOfOriginCertificateMatchDeclaration(CertificateOfOrigin request)
        {
            EEventType eventType;
            //CR 156814
            if (request.RequestReasonCode == (int) ERequestReason.EmptyCertificate ||
                request.RequestReasonCode == (int) ERequestReason.Draft ||
                request.RequestReasonCode == (int) ERequestReason.GetRequestStatus ||
                request.RequestReasonCode == (int) ERequestReason.CertificateCancellation)
            {

                eventType = EEventType.CertificateMatchDeclarationWithoutTask;
            }
            else
            {
                eventType = EEventType.CertificateOfOriginCertificateMatchDeclaration;
            }
            
            var args = new EventUtilArguments(eventType, new VirtualEntity(request));
            ((VirtualEntity)args.MainRelatedEntity).IOrganizationUnitTypeOrganizationUnitTypeID = (int)EOrganizationUnitType.Export;
            EventUtil.RaiseEvent(args);
        }

        private void RaiseEventCertificateOfOriginCertificateDeclarationMismatch(CertificateOfOrigin request)
        {
            var args = new EventUtilArguments(EEventType.CertificateOfOriginCertificateDeclarationMismatch, new VirtualEntity(request));
            ((VirtualEntity)args.MainRelatedEntity).IOrganizationUnitTypeOrganizationUnitTypeID = (int)EOrganizationUnitType.Export;
            EventUtil.RaiseEvent(args);
        }
    }
}
