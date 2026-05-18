using System;
using System.Collections.Generic;
using System.Linq;
using Customs.CertificateOfOrigins.Entities;
using Customs.CRM.CertificateOfOrigins.BL;
using Customs.CRM.CertificateOfOrigins.ExternalCommon.Common;
using Customs.CRM.CertificateOfOrigins.InternalCommon;
using Customs.CRM.CertificateOfOrigins.InternalCommon.Common;
using Customs.Inf.CommonService.ExternalCommon.TemplateDTOs;
using MalamTeam.Infrastructure.GeneralServices.CommonBase;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.Base_Entities;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.EntityValidation;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.Exceptions;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.Extensions;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.ORM;
using MalamTeam.Infrastructure.GeneralServices.EAISchema;
using MalamTeam.Infrastructure.GeneralServices.Environment.Const;
using MalamTeam.Infrastructure.GeneralServices.Environment.Enums;
using Microsoft.Practices.Unity;

namespace Customs.CRM.CertificateOfOrigins.Service
{



    public partial class CertificateOfOriginsExternalService
    {
        public bool InternalTempSync(int i)
        {
            throw new NotImplementedException();
        }

        public VirtualEntity InternalConvert(ConnectedEntity connectedEntity)
        {
            using (var uow = Container.Resolve<IUnitOfWork>(CRMConsts.CertificateOfOriginsUnitOfWork))
            {
                var bl = new CertificateOfOriginsBL(uow);
                var filter = new CertificateOfOriginFilter
                {
                    certificateNumber = connectedEntity.entityIdKey1
                };

                var cerificateOfOrigin = bl.GetCertificateOfOriginsByFilter(filter).FirstOrDefault();

                if (cerificateOfOrigin == null)
                {
                    throw new InfException(EMessages.ConversionOfEntityFaildEntityNotExist, new List<MalamValidationParameter>
                    {
                        new MalamValidationParameter
                        {
                            Value = "תעודת מקור", //TODO: const
                        },
                        new MalamValidationParameter
                        {
                            Value = connectedEntity.entityIdKey1
                        }
                    });
                }

                var virtualEntity = new VirtualEntity
                {
                    ID = cerificateOfOrigin.ID,
                    Title = cerificateOfOrigin.Name,
                    EntityType = EEntityType.CertificateOfOrigin,
                    CustomerID = cerificateOfOrigin.CustomesAgentID
                };
                return (virtualEntity);
            }
        }

        public bool InternalHandleAuthenticationRequestDeliverySent(RaiseEventArgs raiseEventArgs)
        {
            using (var uow = Container.Resolve<IUnitOfWork>(CRMConsts.CertificateOfOriginsUnitOfWork))
            {
                if (raiseEventArgs.RelatedEntities.IsNullOrEmpty()) return false;
                var authenticationRequestFileVirtualEntity = raiseEventArgs.RelatedEntities.SingleOrDefault(
                        e =>
                        e.EntityType == EEntityType.AuthenticationRequestFile ||
                        e.TypeID == (int)EEntityType.AuthenticationRequestFile);
                if (authenticationRequestFileVirtualEntity == null) return false;


                //uow.Repository.GetQuery<CertificateOfOriginsImportAuthenticationFileDetails>().FirstOrDefault(
                //    r => r.ID == authenticationRequestFileVirtualEntity.ID);

                var bl = new AuthenticationRequestBL(uow);
                var authenticationRequestFile = bl.GetAuthenticationRequestFileByID(authenticationRequestFileVirtualEntity.ID);

                if (authenticationRequestFile == null) return false;

                //bl.UpdateFileAfterDelivery(authenticationRequestFile);
            }
            return true;
        }

        public void InternalUpdateCetrificateOfOrigins(UpdateCetificateOfOriginsDTO updateCetificateOfOriginsDTO)
        {            
            if (!Configuration.GetConfig<bool>(CertificateOfOriginsConsts.IsExportDeclarationActive))
                return;
            using (var uow = Container.Resolve<IUnitOfWork>(CRMConsts.CertificateOfOriginsUnitOfWork))
            {
                var bl = new CertificateOfOriginsBL(uow);
                
                switch (updateCetificateOfOriginsDTO.EventType)
                {
                    case EEventType.ExportDeclarationSubmissionSucceeded:
                        bl.UpdateCertrificateOfOrigins(updateCetificateOfOriginsDTO,null,true);
                        break;
                    case EEventType.ExportDeclarationReleased:
                        bl.DeclarationReleased(updateCetificateOfOriginsDTO,null,true);
                        break;
                    case EEventType.AssemblySharedReleaseAccepted:
                        bl.DeclarationReleased(updateCetificateOfOriginsDTO, null, true);
                        break;
                    case EEventType.ExportDeclarationAmendmentRequestCompleted:
                        bl.ExportDeclarationAmendmentSuccess(updateCetificateOfOriginsDTO,true);
                        break;
                    case EEventType.CancellationRequestCommited:
                        bl.ExportDeclarationCancellationRequestCommited(updateCetificateOfOriginsDTO);
                        break;
                }
            }
        }

        public int? InternalGetCertificateOfOriginID(string certificateNumber)
        {
            using (var uow = Container.Resolve<IUnitOfWork>(CRMConsts.CertificateOfOriginsUnitOfWork))
            {
                var certificateOfOrigin = uow.Repository.GetQuery<CertificateOfOrigin>().OrderByDescending(c => c.CreateDate).FirstOrDefault(c => c.CertificateNumber == certificateNumber);
                return certificateOfOrigin?.ID;
            }
        }

        public List<GoodsItemCerificateDTO> InternalGetGoodsItemCerificateDTO(List<GoodsItemCerificateDTO> goodsItemCerificateDTOs)
        {
            using (var uow = Container.Resolve<IUnitOfWork>(CRMConsts.CertificateOfOriginsUnitOfWork))
            {
                foreach (var item in goodsItemCerificateDTOs)
                {
                    var certificateOfOrigin = uow.Repository.GetQuery<CertificateOfOrigin>().OrderByDescending(c => c.CreateDate).FirstOrDefault(c => c.CertificateNumber == item.CertificateNumber);
                    item.certificateOfOriginID = certificateOfOrigin?.ID;                    
                }

                return goodsItemCerificateDTOs;
            }
        }

        public bool InternalSaveCertificateOfOriginAttachments(SaveCertificateAttachmentsArgsDTO saveCertificateAttachmentsArgsDTO)
        {
            using (var uow = Container.Resolve<IUnitOfWork>(CRMConsts.CertificateOfOriginsUnitOfWork))
            {
                var bl = new CertificateOfOriginsBL(uow);
                return bl.SaveCertificateOfOriginAttachments(saveCertificateAttachmentsArgsDTO);
            }
        }
    }
}
