using Customs.CertificateOfOrigins.Entities;
using Customs.CRM.CertificateOfOrigins.BL.ServiceAdapters;
using Customs.CRM.CertificateOfOrigins.ExternalCommon.Common;
using Customs.CRM.CertificateOfOrigins.InternalCommon;
using Customs.CRM.CertificateOfOrigins.InternalCommon.Common;
using Customs.CRM.Entities;
using Customs.CRM.Entities.CertificateOfOriginsDTO;
using Customs.CRM.Entities.CertificateOfOriginsPartial;
using Customs.CRP.External.DealFile.Entities;
using Customs.Inf.CertificateOfOrigins.EAISchema;
using Customs.Inf.CommonService.ExternalCommon.TemplateDTOs;
using Customs.Infrastructure.DocumentManagement.ExternalCommon;
using Customs.Infrastructure.DocumentManagement.ExternalCommon.Enums;
using Customs.Infrastructure.DocumentManagement.RepositoryProxy;
using Customs.Infrastructure.Queue;
using Customs.Infrastructure.SystemTables.ExternalCommon;
using Customs.Infrastructure.Tasks.ExternalCommon;
using Customs.Infrastructure.Utils.WcfInvoker;
using Customs.KnowledgeStore.CustomsBook.ExternalCommon.Common;
using Customs.Shared.Entities;
using MalamTeam.Infrastructure.GeneralServices.BaseLayer;
using MalamTeam.Infrastructure.GeneralServices.CommonBase;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.Base_Entities;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.EntityValidation;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.Exceptions;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.Extensions;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.Interfaces;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.ORM;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.Utils;
using MalamTeam.Infrastructure.GeneralServices.EAISchema;
using MalamTeam.Infrastructure.GeneralServices.Environment.Const;
using MalamTeam.Infrastructure.GeneralServices.Environment.Enums;
using Microsoft.Data.Extensions;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading;
using Exception = MalamTeam.Infrastructure.GeneralServices.EAISchema.Exception;

namespace Customs.CRM.CertificateOfOrigins.BL
{
#if TRACE
#endif

    public class CertificateOfOriginsBL : BaseBL
    {
        private readonly IStringUtil _stringUtil = UnityContainerManager.DefaultContainer.Resolve<IStringUtil>();
        ServicesAdapter servicesAdapter = new ServicesAdapter();
        private InfException _requestExceptions;

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="CertificateOfOriginsBL"/> class.
        /// </summary>
        /// <param name="uow">The uow.</param>
        public CertificateOfOriginsBL(IUnitOfWork uow)
            : base(uow)
        {
            Container.RegisterTypeIfMissing(typeof(ICommonServicesServiceAdapter), typeof(CommonServicesServiceAdapter), true);
            Container.RegisterTypeIfMissing(typeof(IDocumentServiceAdapter), typeof(DocumentServiceAdapter), true);
            Container.RegisterTypeIfMissing(typeof(ICustomerServiceAdapter), typeof(CustomerServiceAdapter), true);
            Container.RegisterTypeIfMissing(typeof(IExportDealFileExternalServiceAdapter), typeof(ExportDealFileExternalServiceAdapter), true);
            Container.RegisterTypeIfMissing(typeof(ICustomsBookServicesAdapter), typeof(CustomsBookServicesAdapter), true);
            //Container.RegisterTypeIfMissing(typeof(IDocumentRepositoryUtil), typeof(DocumentRepositoryUtil), true);
            _requestExceptions = new InfException(EMessages.RequestDataNotValid, null, new List<InfException>());
        }

        #endregion

        public List<CertificateOfOriginResult> GetCertificateOfOriginsByFilter(InternalCommon.Common.CertificateOfOriginFilter filter)
        {
            #region sqlParameters

            var sqlParameters = new List<SqlParameter>
            {
                new SqlParameter
                {
                    ParameterName = "@certificateNumber",
                    SqlDbType = SqlDbType.NVarChar,
                    Value = filter.certificateNumber ?? (object)DBNull.Value,
                },
                new SqlParameter
                {
                    ParameterName = "@certificateOfOriginStatusID",
                    SqlDbType = SqlDbType.Int,
                    Value = filter.certificateOfOriginStatusID ?? (object)DBNull.Value,
                },
                new SqlParameter
                {
                    ParameterName = "@certificateOfOriginTypeID",
                    SqlDbType = SqlDbType.Int,
                    Value = filter.certificateOfOriginTypeID ?? (object)DBNull.Value,
                },
                new SqlParameter
                {
                    ParameterName = "@customsAgentID",
                    SqlDbType = SqlDbType.Int,
                    Value = filter.customsAgentID ?? (object)DBNull.Value,
                },
                new SqlParameter
                {
                    ParameterName = "@customsHouseID",
                    SqlDbType = SqlDbType.Int,
                    Value = filter.customsHouseID ?? (object)DBNull.Value,
                },
                new SqlParameter
                {
                    ParameterName = "@destinationCountry",
                    SqlDbType = SqlDbType.Int,
                    Value = filter.destinationCountry ?? (object)DBNull.Value,
                },
                new SqlParameter
                {
                    ParameterName = "@exportDeclarationID",
                    SqlDbType = SqlDbType.Int,
                    Value = filter.exportDeclarationID ?? (object)DBNull.Value,
                },
                new SqlParameter
                {
                    ParameterName = "@exportDeclarationNum",
                    SqlDbType = SqlDbType.NVarChar,
                    Value = filter.exportDeclarationNum ?? (object)DBNull.Value,
                },
                new SqlParameter
                {
                    ParameterName = "@exporterCustomerID",
                    SqlDbType = SqlDbType.Int,
                    Value = filter.exporterCustomerID ?? (object)DBNull.Value,
                },
                new SqlParameter
                {
                    ParameterName = "@fromIssuingDate",
                    SqlDbType = SqlDbType.DateTime,
                    Value = filter.fromIssuingDate ?? (object)DBNull.Value,
                },
                new SqlParameter
                {
                    ParameterName = "@toIssuingDate",
                    SqlDbType = SqlDbType.DateTime,
                    Value = filter.toIssuingDate ?? (object)DBNull.Value,
                },
                new SqlParameter
                {
                    ParameterName = "@fromRequestDate",
                    SqlDbType = SqlDbType.DateTime,
                    Value = filter.fromRequestDate ?? (object)DBNull.Value,
                },
                new SqlParameter
                {
                    ParameterName = "@toRequestDate",
                    SqlDbType = SqlDbType.DateTime,
                    Value = filter.toRequestDate ?? (object)DBNull.Value,
                },
                new SqlParameter
                {
                    ParameterName = "@requestReasonID",
                    SqlDbType = SqlDbType.Int,
                    Value = filter.requestReasonID ?? (object)DBNull.Value,
                },
                new SqlParameter
                {
                    ParameterName = "@versionNumber",
                    SqlDbType = SqlDbType.Int,
                    Value = filter.versionNumber ?? (object)DBNull.Value,
                },
                new SqlParameter
                {
                    ParameterName = "@isLastVersion",
                    SqlDbType = SqlDbType.Bit,
                    Value = filter.isLastVersion ?? (object)DBNull.Value,
                }
            };

            #endregion

            var result = _uow.Repository.ExecuteFunction<CertificateOfOriginResult>(CertificateOfOriginsConsts.GetCertificateOfOriginsByFilterSP, sqlParameters);
            return (List<CertificateOfOriginResult>)result;
        }

        public CertificateOfOriginResult IsCertificateOfOriginByExternalIdExist(string certificateOfOriginExternalId)
        {
            var filter = new CertificateOfOriginFilter { certificateNumber = certificateOfOriginExternalId };
            var result = _uow.Repository.ExecuteFunction<CertificateOfOriginResult>(filter).FirstOrDefault();
            return result;
        }


        #region GetCertificateOfOriginById

        public CertificateOfOrigin GetCertificateOfOriginById(int certificateOfOriginId, bool isFromMessage = false)
        {
            var CertificateOfOriginResult = GetCertificateOfOriginByIdSP(certificateOfOriginId);
            return CertificateOfOriginResult;
        }

        public CertificateOfOrigin GetCertificateOfOriginByIdSP(int certificateOfOriginId)
        {
            var spFilter = new ResultSetFilter<CertificateOfOrigin>
            {
                StoredProcedureName = CertificateOfOriginsConsts.GetCertificateOfOriginByIDSP,
                MaterializeFunction = MaterializeForCertificateOfOrigin,
                Parameters =
                            {
                                new SqlParameter
                                    {
                                        ParameterName = "@CertificateOfOriginID",
                                        SqlDbType = SqlDbType.Int,
                                        Value = certificateOfOriginId
                                    }
                            }
            };
            var certificateOfOrigin = _uow.Repository.ExecuteResultSetFunction(spFilter);
            return certificateOfOrigin;
        }


        private CertificateOfOrigin MaterializeForCertificateOfOrigin(DbDataReader reader)
        {
            var certificateOfOrigin = reader.Materialize<CertificateOfOrigin>().FirstOrDefault();
            if (certificateOfOrigin == null) return null;

            certificateOfOrigin.StakeholdersIDs = new List<int>()
            {
                certificateOfOrigin.CustomerID,      //יצואן
                certificateOfOrigin.CreateCustomerID // סוכן המכס
            };

            certificateOfOrigin.AcceptChanges();

            reader.NextResult();
            var certificateOfOriginVsDeclarationError = reader.Materialize<CertificateOfOriginVsDeclarationError>().ToList();
            certificateOfOriginVsDeclarationError.ForEach(de => de.AcceptChanges());

            reader.NextResult();
            var detailsTypeCodeEnums = reader.Materialize<CertificateDetailsTypeCodeEnum>().ToList();
            detailsTypeCodeEnums.ForEach(dtc => dtc.AcceptChanges());

            reader.NextResult();
            var certificateOfOriginDetails = reader.Materialize<CertificateOfOriginDetails>().ToList();
            certificateOfOriginDetails.ForEach(cd => cd.AcceptChanges());
            reader.NextResult();

            var certificateOfOriginInvoices = reader.Materialize<CertificateOfOriginInvoiceDetail>().ToList();
            reader.NextResult();

            var certificateOfOriginItemDetails = reader.Materialize<CertificateOfOriginItemDetail>().ToList();
            reader.NextResult();

            var certificateMilestonesDTO = reader.Materialize<CertificateMilestonesDTO>().ToList();
            certificateOfOrigin.Milestones = new List<CertificateMilestonesDTO>();
            certificateOfOrigin.Milestones.AddRange(certificateMilestonesDTO);

            certificateOfOrigin.CertificateOfOriginVsDeclarationError.AddRange(certificateOfOriginVsDeclarationError);
            foreach (var certificateOfOriginDetail in certificateOfOriginDetails)
            {
                certificateOfOriginDetail.CertificateDetailsTypeCode = detailsTypeCodeEnums.FirstOrDefault(d => d.ID == certificateOfOriginDetail.CertificateDetailsTypeCodeID);
                certificateOfOriginDetail.CertificateDetailsTypeCode.AcceptChanges();
            }
            certificateOfOrigin.CertificateOfOriginDetails.AddRange(certificateOfOriginDetails);
            if (!certificateOfOriginInvoices.IsNullOrEmpty())
            {
                foreach (var invoice in certificateOfOriginInvoices)
                {
                    if (!certificateOfOriginItemDetails.IsNullOrEmpty())
                    {
                        var thisItemDetails = certificateOfOriginItemDetails.Where(i => i.CertificateOfOriginInvoiceDetailID == invoice.ID).ToList();
                        if (!thisItemDetails.IsNullOrEmpty())
                        {
                            invoice.CertificateOfOriginItemDetail.AddRange(thisItemDetails);
                            thisItemDetails.ForEach(d => d.AcceptChanges());
                        }
                    }
                    certificateOfOrigin.CertificateOfOriginInvoiceDetail.Add(invoice);
                    invoice.AcceptChanges();
                }
            }
            return certificateOfOrigin;
        }

        public bool LoadDataFromExportDeclaration(CertificateOfOrigin certificateOfOrigin)
        {
            var exportDealFileExternalServiceAdapter = Container.Resolve<IExportDealFileExternalServiceAdapter>();

            if (certificateOfOrigin.LeadDocumentID != null || !certificateOfOrigin.ExportDeclarationNumber.IsNullOrEmpty())
            {
                var exportDeclarationDetailsDto = exportDealFileExternalServiceAdapter.GetExportDeclarationDetailsForCertificateOfOrigion(certificateOfOrigin.LeadDocumentID, certificateOfOrigin.ExportDeclarationNumber);

                certificateOfOrigin.IsDeclarationReleased = exportDeclarationDetailsDto?.IsDeclarationReleased;
                certificateOfOrigin.IsCargoExitedOfCustomsRegulation = exportDeclarationDetailsDto?.IsCargoExitedOfCustomsRegulation;
                if (certificateOfOrigin.IsCargoExitedOfCustomsRegulation.HasValue &&
                    certificateOfOrigin.IsCargoExitedOfCustomsRegulation.Value &&
                    certificateOfOrigin.RequestReasonCode != (int)ERequestReason.RetrospectiveCertificate)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }

        public void DeclarationReleased(UpdateCetificateOfOriginsDTO updateCetificateOfOriginsDTO, CertificateOfOrigin certificateOfOrigin, bool isFromDealFile)
        {
            var certificateOfOrigins = new List<CertificateOfOrigin>();
            if (isFromDealFile && certificateOfOrigins.IsNullOrEmpty())
                certificateOfOrigins = _uow.Repository.GetQuery<CertificateOfOrigin>().Where(coo => updateCetificateOfOriginsDTO.CertificateOfOriginsIDs.Contains(coo.ID)).ToList();
            else
                certificateOfOrigins.Add(certificateOfOrigin);
            var cetificateOfOriginsDTO = new UpdateCetificateOfOriginsDTO()
            {
                ExportDeclarationNum = updateCetificateOfOriginsDTO.ExportDeclarationNum,
                LeadDocumentID = updateCetificateOfOriginsDTO.LeadDocumentID,
                CertificateOfOriginsIDs = new List<int>()
            };
            foreach (var item in certificateOfOrigins)
            {
                if (string.IsNullOrEmpty(item.ExportDeclarationNumber))
                {
                    item.ExportDeclarationNumber = updateCetificateOfOriginsDTO.ExportDeclarationNum;
                    item.LeadDocumentID = updateCetificateOfOriginsDTO.LeadDocumentID;
                    cetificateOfOriginsDTO.CertificateOfOriginsIDs.Add(item.ID);
                }
                if (!item.LeadDocumentID.HasValue && item.ExportDeclarationNumber == updateCetificateOfOriginsDTO.ExportDeclarationNum)
                {
                    item.LeadDocumentID = updateCetificateOfOriginsDTO.LeadDocumentID;
                }
                if (item.CertificateOfOriginStatusID == (int)ECertificateOfOriginStatus.PendingRelease)
                {
                    item.CertificateOfOriginStatusID = (int)ECertificateOfOriginStatus.Published;
                    CreateQRCodeIfNeededAndUpload(item);
                    CreateAttacmentsAndSendFeedBackMessage(item);
                }
                if (item.RequestReasonCode == (int)ERequestReason.CertificateReplacement)
                {
                    HandleCertificateReplacement(item);
                }

                _uow.Repository.Save(item);
                _uow.CommitAllChanges();
            }

            if (!cetificateOfOriginsDTO.CertificateOfOriginsIDs.IsNullOrEmpty())
            {
                UpdateCertrificateOfOrigins(cetificateOfOriginsDTO, certificateOfOrigin, isFromDealFile);
            }
        }

        public void HandleCertificateReplacement(CertificateOfOrigin certificate)
        {
            if (certificate.CertificateIDToCancel != null)
            {
                var certificateToCancel = _uow.Repository.GetQuery<CertificateOfOrigin>().FirstOrDefault(coo => coo.ID == certificate.CertificateIDToCancel);
                if (certificateToCancel == null) return;
                certificate.FeedbackRemark = string.Format(SystemTablesUtil.GetUIMessage((int)EMessages.UpdateExportDeclaration), certificate.ExportDeclarationNumber, certificateToCancel.CertificateNumber);
                servicesAdapter.SendMessageToAgent(certificate, string.Format(SystemTablesUtil.GetUIMessage((int)EMessages.UpdateExportDecForReplacement), certificateToCancel.ExportDeclarationNumber, certificateToCancel.CertificateNumber, certificate.CertificateNumber), EAgentTalkBackType.CertificateOfOriginReplacement);
                certificateToCancel.CertificateOfOriginStatusID = (int)ECertificateOfOriginStatus.Cancelled;
                certificateToCancel.RejectCancelReason = string.Format(SystemTablesUtil.GetUIMessage((int)EMessages.CertificateReplaced), certificate.CertificateNumber);
                var args = new EventUtilArguments(EEventType.CertificateOfOriginCertificateReplaced, new VirtualEntity(certificateToCancel))
                {
                    AdditionalInfo = certificate.CertificateNumber
                };
                EventUtil.RaiseEvent(args);
                _uow.Repository.Save(certificateToCancel);
            }

            _uow.Repository.Save(certificate);
            _uow.CommitAllChanges();

        }

        public void CreateAttacmentsAndSendFeedBackMessage(CertificateOfOrigin certificate)
        {
            if (certificate.IsCreateAttachments) return;

            certificate.IssuingDate = DateTime.Now;
            if (Configuration.GetConfig<bool>("IssueCertificateOfOriginByWorker"))
            {
                certificate.IsInPublishingProcess = true;
            }
            certificate.AcceptChanges();

            _uow.Repository.Save(certificate);
            _uow.CommitAllChanges();
            var args = new EventUtilArguments(EEventType.CertificateOfOriginCertificateIssued, certificate);
            EventUtil.RaiseEvent(args);

            Thread.Sleep(1000);

            var templates = PrintCertificateOfOriginAndSaveAttachments(certificate, string.Empty);

            if (!templates.IsNullOrEmpty())
            {
                var index = 0;
                var attachments = new Attachment[templates.Count];

                foreach (var template in templates)
                {
                    var attachment = new Attachment
                    {
                        DocumentTypeID = template.DocumentTypeID,
                        content = template.Content,
                        fileName = template.FileName
                    };

                    attachments[index] = attachment;
                    index++;
                }

                certificate.IsCreateAttachments = true;
                SendRequestFeedback(certificate, attachments);
            }
        }

        public void SendRequestFeedback(CertificateOfOrigin certificateOfOrigin, Attachment[] attachments)
        {
            if (certificateOfOrigin.IsMessageSent) return;

            using (var uow = Container.Resolve<IUnitOfWork>(CRMConsts.CertificateOfOriginsUnitOfWork))
            {
                var util = new CertificateOfOriginsUtil(uow);
                var message = new PC_NG_2281_MSG02_CertificateOfOriginRequestFeedback
                {
                    CertificateOfOriginRequestFeedback = util.CreateCertificateOfOriginRequestFeedback(certificateOfOrigin),
                    Attachment = attachments
                };

                OutgoingMessageProxy.Send(message, new OutgoingDetails
                {
                    ID = certificateOfOrigin.CreateCustomerID,
                    EntityType = EEntityType.Customer,
                    RelatedEntities = new List<IEntity> { certificateOfOrigin }
                });
                if (certificateOfOrigin.CertificateOfOriginStatusID == (int)ECertificateOfOriginStatus.Published)
                    certificateOfOrigin.IsMessageSent = true;
            }
        }

        public void ExportDeclarationAmendmentSuccess(UpdateCetificateOfOriginsDTO updateCetificateOfOriginsDTO, bool isFromDealFile)
        {
            var certificateOfOrigins = _uow.Repository.GetQuery<CertificateOfOrigin>().Where(coo => updateCetificateOfOriginsDTO.CertificateOfOriginsIDs.Contains(coo.ID)).ToList();
            updateCetificateOfOriginsDTO.CertificateOfOriginsIDs = new List<int>();

            foreach (var item in certificateOfOrigins)
            {

                if (string.IsNullOrEmpty(item.ExportDeclarationNumber))
                {
                    item.LeadDocumentID = updateCetificateOfOriginsDTO.LeadDocumentID;
                    item.ExportDeclarationNumber = updateCetificateOfOriginsDTO.ExportDeclarationNum;
                    updateCetificateOfOriginsDTO.CertificateOfOriginsIDs.Add(item.ID);
                    _uow.Repository.Save(item);
                    _uow.CommitAllChanges();
                }
            }
            if (!updateCetificateOfOriginsDTO.CertificateOfOriginsIDs.IsNullOrEmpty())
            {
                UpdateCertrificateOfOrigins(updateCetificateOfOriginsDTO, null, isFromDealFile);
            }
        }
        public List<Exception> UpdateCertrificateOfOrigins(UpdateCetificateOfOriginsDTO updateCetificateOfOriginsDTO, CertificateOfOrigin certificateOfOrigin, bool isFromDealFile)
        {
            List<Exception> listOfExceptions = new List<Exception>();
            if (updateCetificateOfOriginsDTO.CertificateOfOriginsIDs.IsNullOrEmpty()) return null;
            var certificateOfOrigins = new List<CertificateOfOrigin>();
            if (isFromDealFile)
            {
                certificateOfOrigins = _uow.Repository.GetQuery<CertificateOfOrigin>(CertificateOfOrigin.NavPropCertificateOfOriginDetails, CertificateOfOrigin.NavPropCertificateOfOriginInvoiceDetail).Where(a => updateCetificateOfOriginsDTO.CertificateOfOriginsIDs.Contains(a.ID)).ToList();

                foreach (CertificateOfOrigin certificateOfOriginFromDealFile in certificateOfOrigins)
                {
                    var invoiceDetails = certificateOfOriginFromDealFile.CertificateOfOriginInvoiceDetail;
                    if (!invoiceDetails.IsNullOrEmpty())
                    {
                        foreach (var invoiceDetail in invoiceDetails)
                        {
                            _uow.Repository.LoadProperty(invoiceDetail, CertificateOfOriginInvoiceDetail.NavPropCertificateOfOriginItemDetail);
                        }
                    }
                }
            }
            else certificateOfOrigins.Add(certificateOfOrigin);
            foreach (var item in certificateOfOrigins)
            {
                if (string.IsNullOrEmpty(item.ExportDeclarationNumber))
                {
                    item.ExportDeclarationNumber = updateCetificateOfOriginsDTO.ExportDeclarationNum;
                    item.LeadDocumentID = updateCetificateOfOriginsDTO.LeadDocumentID;
                }
                if (!item.LeadDocumentID.HasValue && item.ExportDeclarationNumber == updateCetificateOfOriginsDTO.ExportDeclarationNum)
                {
                    item.LeadDocumentID = updateCetificateOfOriginsDTO.LeadDocumentID;
                }
                if (item.CertificateOfOriginStatusID == (int)ECertificateOfOriginStatus.Received && item.RequestReasonCode.NotIn((int)ERequestReason.EmptyCertificate, (int)ERequestReason.GetRequestStatus, (int)ERequestReason.CertificateCancellation) && item.TypeID != (int)ECertificateOfOriginType.NonManipulation)
                {
                    bool hasErrors = false;
                    bool hasWarnings = false;
                    string additionalInfo = string.Empty;
                    bool isLinkedToImportDeclaration = false;

                    if (updateCetificateOfOriginsDTO.ExportInvoiceInfoDTOList.IsNullOrEmpty())
                        hasErrors = true;
                    else
                    {
                        ValidateExportDeclarationInfoForPCIsMatch(item, updateCetificateOfOriginsDTO, out isLinkedToImportDeclaration);
                        hasErrors = _requestExceptions.Exceptions.Exists(e => !e.ExceptionLevel.HasValue || e.ExceptionLevel == (int)EExceptionLevel.Error);
                        hasWarnings = _requestExceptions.Exceptions.Exists(e => e.ExceptionLevel == (int)EExceptionLevel.Warning);
                    }

                    if (!hasErrors && !hasWarnings)
                    {
                        if (item.RequestReasonCode != (int)ERequestReason.Draft)
                        {
                            item.CertificateOfOriginStatusID = (int)ECertificateOfOriginStatus.DeclarationMatch;
                        }
                        if (item.RequestReasonCode != (int)ERequestReason.EmptyCertificate && item.RequestReasonCode != (int)ERequestReason.Draft
                            && item.RequestReasonCode != (int)ERequestReason.GetRequestStatus && item.RequestReasonCode != (int)ERequestReason.CertificateCancellation)
                        {
                            var args = RaiseTaskNewCertificateOfOriginCheck(item, EEventType.CertificateOfOriginCertificateMatchDeclaration);
                            EventUtil.RaiseEvent(args);
                        }
                    }
                    else
                    {
                        string englishAdditionalInfo = "";
                        var exceptions = _requestExceptions.Exceptions.ToList();

                        foreach (var exception in exceptions)
                        {
                            var exeptionInfo = String.Empty;
                            var message = SystemTablesUtil.GetUIMessageWithEnglishAndLevel((int)exception.UserMessage);
                            if (exception.Parameters != null)
                            {
                                var parameters = exception.Parameters.Select(p => p.Value).ToList();
                                exeptionInfo = String.Format(message.Item1, parameters.ToArray());
                            }
                            else
                            {
                                exeptionInfo = message.Item1;
                                englishAdditionalInfo += message.Item2;
                            }
                            if (additionalInfo.Length + exeptionInfo.Length + CertificateOfOriginsConsts.LengthOfTaskStart < CertificateOfOriginsConsts.MaximumNumberOfCharactersOfTheField)
                                additionalInfo += exeptionInfo + " , ";

                            item.CertificateOfOriginVsDeclarationError.Add(new CertificateOfOriginVsDeclarationError()
                            {
                                ErrorText = exeptionInfo
                            });
                            listOfExceptions.Add(new Exception()
                            {
                                ExceptionLevel = message.Item3,
                                EnglishDescription = message.Item2,
                                ExeptionDescription = exeptionInfo,
                                ExeptionType = (int)exception.UserMessage
                            });
                        }
                        if (item.RequestReasonCode != (int)ERequestReason.Draft)
                        {
                            if (hasErrors)
                            {
                                item.CertificateOfOriginStatusID = (int)ECertificateOfOriginStatus.Rejected;
                                item.RejectCancelReason = CertificateOfOriginsConsts.ThereIsNoMatchBetweenTheCertificateDataAndTheDeclaration;
                                var args = new EventUtilArguments(EEventType.CertificateOfOriginCertificateDeclarationMismatch, new VirtualEntity(item)
                                {
                                    IOrganizationUnitTypeOrganizationUnitTypeID = (int)EOrganizationUnitType.Export,
                                    OrganizationUnitID = item.OrganizationUnitID
                                });

                                args.EventType = EEventType.CertificateOfOriginCertificateDeclarationMismatch;
                                args.AdditionalInfo = additionalInfo;
                                EventUtil.RaiseEvent(args);
                            }
                            else
                            {
                                if (isLinkedToImportDeclaration)
                                {
                                    var eventArgs = new EventUtilArguments(EEventType.OpenTaskHandlingTheReplacementOfAnImportCertificate, new VirtualEntity
                                    {
                                        EntityType = EEntityType.CertificateOfOrigin,
                                        ID = item.ID,
                                        Title = EEventType.OpenTaskHandlingTheReplacementOfAnImportCertificate.ToString(),
                                        OrganizationUnitID = item.OrganizationUnitID,
                                    });

                                    EventUtil.RaiseEvent(eventArgs);
                                }

                                item.CertificateOfOriginStatusID = (int)ECertificateOfOriginStatus.DeclarationMismatch;

                                var args = new EventUtilArguments(EEventType.CertificateOfOriginCertificateDeclarationHasWarnings, new VirtualEntity(item)
                                {
                                    IOrganizationUnitTypeOrganizationUnitTypeID = (int)EOrganizationUnitType.Export,
                                    OrganizationUnitID = item.OrganizationUnitID
                                })
                                {
                                    EventType = EEventType.CertificateOfOriginCertificateDeclarationHasWarnings,
                                    AdditionalInfo = additionalInfo
                                };
                                int? assesorId = null;
                                if (item.LeadDocumentID.HasValue)
                                {
                                    LatestUserHandlingEntityTasksFilter filter = new LatestUserHandlingEntityTasksFilter()
                                    {
                                        EntityID = (int)item.LeadDocumentID,
                                        EntityType = EEntityType.ExportLeadDocument,
                                        OrganizationUnitTypeID = (int)EOrganizationUnitType.Export,
                                        OrganizationUnitID = updateCetificateOfOriginsDTO.OrganizationUnitID
                                    };
                                    assesorId = servicesAdapter.GetLatestUserHandlingEntityTasksWithTaskUnification(filter); //servicesAdapter.GetExportDeclarationAssessor(item.ExportDeclarationNumber);
                                }
                                if (assesorId.HasValue)
                                {
                                    args.TaskArguments = new EventTaskArguments
                                    {
                                        TaskAssignmentArguments = new TaskAssignmentArguments()
                                        {
                                            SingleUserTaskAssignmentFilter = new SingleUserAssignmentFilter
                                            {
                                                ProfessionId = (int)EProfession.Marech,
                                                OrganizationUnitTypeId = (int)EOrganizationUnitType.Export,
                                                OrganizationUnitId = updateCetificateOfOriginsDTO.OrganizationUnitID,
                                                UserId = assesorId
                                            }
                                        }
                                    };
                                }

                                EventUtil.RaiseEvent(args);
                            }
                        }
                    }
                    PrintCertificateOfOriginAndSaveAttachments(item, CertificateOfOriginsConsts.IsDraft);
                }

                _uow.Repository.Save(item);
                _uow.CommitAllChanges();
            }
            return listOfExceptions;
        }

        public ExportDeclarationInfoDTO CheckDeclarationStatus(CertificateOfOrigin certificateOfOrigin, bool isNeedToReturn)
        {
            var exportDealFileExternalServiceAdapter = Container.Resolve<IExportDealFileExternalServiceAdapter>();
            var exportDeclarationDetailsDTO = certificateOfOrigin.ExportDeclarationDetailsDTO != null ? certificateOfOrigin.ExportDeclarationDetailsDTO
                : (certificateOfOrigin.LeadDocumentID != null || certificateOfOrigin.ExportDeclarationNumber != null) ? exportDealFileExternalServiceAdapter.GetExportDeclarationDetailsForCertificateOfOrigion(certificateOfOrigin.LeadDocumentID, certificateOfOrigin.ExportDeclarationNumber)
                : null;
            if (exportDeclarationDetailsDTO != null)
            {
                var prevCertificate = _uow.Repository.GetQuery<CertificateOfOrigin>().Where(coo => coo.Title == certificateOfOrigin.Title).OrderByDescending(c => c.ID).Skip(1).FirstOrDefault();

                if (certificateOfOrigin.CertificateOfOriginIdOfReplacement != default(int) || prevCertificate != null)
                {
                    exportDealFileExternalServiceAdapter.ChangeCertificateOfOriginIDForLeadDocument(exportDeclarationDetailsDTO.LeadDocumentID, certificateOfOrigin.CertificateOfOriginIdOfReplacement != default(int) ? certificateOfOrigin.CertificateOfOriginIdOfReplacement : prevCertificate.ID, certificateOfOrigin.ID);
                }
                if (isNeedToReturn)
                {
                    return exportDealFileExternalServiceAdapter.GetExportDeclarationInfoForPC(exportDeclarationDetailsDTO.LeadDocumentID);
                }
                return null;
            }
            return null;
        }

        public void CheckCertificateOfOriginOnDeclarationReleased(CertificateOfOrigin certificateOfOrigin)
        {
            if (Configuration.GetConfig<bool>(CertificateOfOriginsConsts.IsExportDeclarationActive))
            {
                var exportDeclarationInfoDTO = CheckDeclarationStatus(certificateOfOrigin, true);
                if (exportDeclarationInfoDTO != null &&
                    (exportDeclarationInfoDTO.LeadDocumentState == (int)ELeadDocumentState.Released ||
                      exportDeclarationInfoDTO.LeadDocumentState == (int)ELeadDocumentState.Paroled ||
                      exportDeclarationInfoDTO.LeadDocumentState == (int)ELeadDocumentState.Closed))
                {
                    UpdateCetificateOfOriginsDTO updateCetificateOfOriginsDTO = new UpdateCetificateOfOriginsDTO()
                    {
                        CertificateOfOriginsIDs = new List<int>() { certificateOfOrigin.ID },
                        DestinationCoutryID = exportDeclarationInfoDTO.DestinationCoutryID,
                        ExporterCustomerID = exportDeclarationInfoDTO.ExporterCustomerID,
                        ExportDeclarationNum = certificateOfOrigin.ExportDeclarationNumber,
                        LeadDocumentID = exportDeclarationInfoDTO.LeadDocumentID,
                        ExportInvoiceInfoDTOList = exportDeclarationInfoDTO.ExportInvoiceInfoDTOList,
                        OrganizationUnitID = exportDeclarationInfoDTO.OrganizationUnitID
                    };
                    DeclarationReleased(updateCetificateOfOriginsDTO, certificateOfOrigin, false);
                }
            }
        }
        public List<Exception> CheckCertificateOfOriginOnDeclarationSubmited(CertificateOfOrigin certificateOfOrigin)
        {
            var exportDeclarationInfoDTO = CheckDeclarationStatus(certificateOfOrigin, true);

            if (exportDeclarationInfoDTO != null &&
               (exportDeclarationInfoDTO.LeadDocumentState == (int)ELeadDocumentState.Submited ||
                exportDeclarationInfoDTO.LeadDocumentState == (int)ELeadDocumentState.Released ||
                exportDeclarationInfoDTO.LeadDocumentState == (int)ELeadDocumentState.Paroled ||
                exportDeclarationInfoDTO.LeadDocumentState == (int)ELeadDocumentState.Closed))
            {
                UpdateCetificateOfOriginsDTO updateCetificateOfOriginsDTO = new UpdateCetificateOfOriginsDTO()
                {
                    CertificateOfOriginsIDs = new List<int>() { certificateOfOrigin.ID },
                    DestinationCoutryID = exportDeclarationInfoDTO.DestinationCoutryID,
                    ExporterCustomerID = exportDeclarationInfoDTO.ExporterCustomerID,
                    ExportDeclarationNum = certificateOfOrigin.ExportDeclarationNumber,
                    LeadDocumentID = exportDeclarationInfoDTO.LeadDocumentID,
                    ExportInvoiceInfoDTOList = exportDeclarationInfoDTO.ExportInvoiceInfoDTOList,
                    OrganizationUnitID = exportDeclarationInfoDTO.OrganizationUnitID
                };

                return UpdateCertrificateOfOrigins(updateCetificateOfOriginsDTO, certificateOfOrigin, false);
            }
            return null;
        }
        private void ValidateExportDeclarationInfoForPCIsMatch(CertificateOfOrigin item, UpdateCetificateOfOriginsDTO updateCetificateOfOriginsDTO, out bool isLinkedToImportDeclaration)
        {
            var exceptions = new List<InfException>();
            var DestinationGroupOfCountries = item.CertificateOfOriginDetails.FirstOrDefault(c => c.CertificateDetailsTypeCodeID == (int)ECertificateDetailsType.DestinationGroupOfCountries)?.Value;
            var destinationCountry = item.CertificateOfOriginDetails.FirstOrDefault(c => c.CertificateDetailsTypeCodeID == (int)ECertificateDetailsType.DestinationCountry)?.Value;
            var exporterId = item.CertificateOfOriginDetails.FirstOrDefault(c => c.CertificateDetailsTypeCodeID == (int)ECertificateDetailsType.ExporterId)?.Value;
            var originCountry = item.CertificateOfOriginDetails.FirstOrDefault(c => c.CertificateDetailsTypeCodeID == (int)ECertificateDetailsType.OriginCountry)?.Value;
            var OriginGroupOfCountries = item.CertificateOfOriginDetails.FirstOrDefault(c => c.CertificateDetailsTypeCodeID == (int)ECertificateDetailsType.OriginGroupOfCountries)?.Value;
            isLinkedToImportDeclaration = false;

            if (updateCetificateOfOriginsDTO.DestinationCoutryID.HasValue)
            {

                if (!destinationCountry.IsNullOrEmpty() && int.TryParse(destinationCountry, out int destinationCountryId) == true && destinationCountryId != updateCetificateOfOriginsDTO.DestinationCoutryID)
                {
                    {
                        exceptions.Add(new InfException(EMessages.DestinationCountryIsNotMAtchTofDestinationCountryInExportdeclaration));
                    }
                }
            }
            if (!string.IsNullOrWhiteSpace(DestinationGroupOfCountries))
            {
                CountryCountryGroup groupOfDestinationCountry = new CountryCountryGroup();
                if (!DestinationGroupOfCountries.IsNullOrEmpty() && int.TryParse(DestinationGroupOfCountries, out int destinationCoutryID) == true)
                {
                    var systemTableUtil = UnityContainerManager.DefaultContainer.Resolve<ISystemTablesUtil>();
                    var predicate = string.Format("(CountryID == {0} && CountryGroupID == {1})", updateCetificateOfOriginsDTO.DestinationCoutryID, destinationCoutryID);
                    groupOfDestinationCountry = systemTableUtil.GetTablesSync<CountryCountryGroup>(new SystemTablesFilter() { IgnoreState = true, Predicate = predicate }).FirstOrDefault();
                }
                if (groupOfDestinationCountry == null || groupOfDestinationCountry.IsNewInstance())
                    exceptions.Add(new InfException(EMessages.DiscrepancyBetweenTheCountriesInTheAgreementVersusTheCountryOfTheBuyer));

            }

            if (!item.ExportDeclarationNumber.IsNullOrEmpty() && item.ExportDeclarationNumber != updateCetificateOfOriginsDTO.ExportDeclarationNum)
            {
                var parameters = new List<MalamValidationParameter> { new MalamValidationParameter { Value = updateCetificateOfOriginsDTO.ExportDeclarationNum } };
                exceptions.Add(new InfException(EMessages.ExportDeclarationNotInSystemForWarningMessage, parameters));
            }
            if (!exporterId.IsNullOrEmpty() && int.TryParse(exporterId, out int exporter) == true && exporter != updateCetificateOfOriginsDTO.ExporterCustomerID)
                exceptions.Add(new InfException(EMessages.ExporterNumberIsNotMAtchToExporterNumberInExportdeclaration));
            if (item.RequestReasonCode == (int)ERequestReason.ImportCertificateReplacement)
            {
                var exportDealFileExternalServiceAdapter = Container.Resolve<IExportDealFileExternalServiceAdapter>();

                var detailsForExportAssociatedGoodsItemsDTO = exportDealFileExternalServiceAdapter.GetDetailsForExportAssociatedGoodsItemsByLeadDocumentId(updateCetificateOfOriginsDTO.LeadDocumentID);
                if (detailsForExportAssociatedGoodsItemsDTO.IsNullOrEmpty() || !detailsForExportAssociatedGoodsItemsDTO.Exists(d => servicesAdapter.IsTradeAgreementForCountry(item.TypeID, (int)d.AssociatedOriginCountryID, false)))
                {
                    exceptions.Add(new InfException(EMessages.ThereIsNoMerchandiseInTheDeclarationThatIsLinkedToTheImportDeclarationWhoseCountryOfOriginIsInTheTradeAgreement, (int)EExceptionLevel.Warning));
                    isLinkedToImportDeclaration = true;
                }
            }
            if (!item.CertificateOfOriginInvoiceDetail.IsNullOrEmpty() && !updateCetificateOfOriginsDTO.ExportInvoiceInfoDTOList.IsNullOrEmpty())
            {

                var util = new CertificateOfOriginsUtil(_uow);
                var certificateTypeCode = util.GetCertificateTypeCode(item.TypeID);
                var customsItemsIdsCacheFilter = new List<CustomsItemsIdsCacheFilter>();
                var customsItemIds = item.CertificateOfOriginInvoiceDetail.Where(i => !i.CertificateOfOriginItemDetail.IsNullOrEmpty())
                    .SelectMany(g => g.CertificateOfOriginItemDetail)
                    .Where(ci => ci.CustomsItemID.HasValue).Select(cc => cc.CustomsItemID).Distinct().ToList();
                customsItemIds.ForEach(c => customsItemsIdsCacheFilter.Add(new CustomsItemsIdsCacheFilter() { CustomsItemID = c, Date = item.CreateDate }));

                customsItemIds = updateCetificateOfOriginsDTO.ExportInvoiceInfoDTOList.Where(i => !i.ExportGoodsItemInfoDTOList.IsNullOrEmpty())
                    .SelectMany(g => g.ExportGoodsItemInfoDTOList)
                    .Select(cc => (int?)cc.CustomsItemID).Distinct().ToList();
                customsItemIds.ForEach(c => customsItemsIdsCacheFilter.Add(new CustomsItemsIdsCacheFilter() { CustomsItemID = c, Date = item.CreateDate }));

                ServicesAdapter _servicesAdapter = new ServicesAdapter();

                var customsItemsByIds = _servicesAdapter.GetCustomsItemsByIdsSync(customsItemsIdsCacheFilter);
                customsItemsByIds.ForEach(ci => ci.FullClassificationby6digits = ci.FullClassification.Length >= 6 ? ci.FullClassification.Substring(0, 6) : null);
                foreach (var invoice in item.CertificateOfOriginInvoiceDetail)
                {
                    ExportInvoiceInfoDTO invoiceFromDealFile = null;
                    invoiceFromDealFile = updateCetificateOfOriginsDTO.ExportInvoiceInfoDTOList.Where(i => i.ExternalIDNum == invoice.InvoiceNumber).FirstOrDefault();
                    if (invoiceFromDealFile == null)
                    {
                        var parameters = new List<MalamValidationParameter> { new MalamValidationParameter { Value = invoice.InvoiceNumber } };
                        exceptions.Add(new InfException(EMessages.ExportInvoiceIsNotMAtchToExportInvoiceInExportdeclaration, (int)EExceptionLevel.Warning, parameters));
                        PassGroupdExceptionListToEntity(exceptions);
                        return;
                    }

                    if (!invoice.CertificateOfOriginItemDetail.IsNullOrEmpty() && !invoiceFromDealFile.ExportGoodsItemInfoDTOList.IsNullOrEmpty())
                    {
                        var customsItemPerInvoice = invoiceFromDealFile.ExportGoodsItemInfoDTOList.Where(g => g.CertificateOfOriginID == item.ID).Select(g => g.CustomsItemID).ToList();

                        foreach (var goodsItem in invoice.CertificateOfOriginItemDetail)
                        {
                            if (!originCountry.IsNullOrEmpty() &&
                            int.TryParse(originCountry, out int originCountryId) == true &&
                            !invoiceFromDealFile.ExportGoodsItemInfoDTOList.Any(g =>
                                g.OriginCountryID == originCountryId))
                            {
                                exceptions.Add(new InfException(EMessages
                                    .OriginCountryIsNotMAtchToOriginCountryInExportdeclaration));
                            }

                            if (!string.IsNullOrWhiteSpace(OriginGroupOfCountries))
                            {
                                CountryCountryGroup groupOforiginCountry = null;
                                if (int.TryParse(OriginGroupOfCountries, out int CountryGroupID) == true)
                                {
                                    var systemTableUtil =
                                        UnityContainerManager.DefaultContainer.Resolve<ISystemTablesUtil>();
                                    groupOforiginCountry = systemTableUtil
                                        .GetTablesSync<CountryCountryGroup>(new SystemTablesFilter()
                                        { IgnoreState = true }).FirstOrDefault(c =>
                                        invoiceFromDealFile.ExportGoodsItemInfoDTOList.Any(g =>
                                            g.OriginCountryID == c.CountryID) &&
                                        c.CountryGroupID == CountryGroupID);
                                }
                                if (groupOforiginCountry == null)
                                    exceptions.Add(new InfException(EMessages
                                        .OriginCountryIsNotMAtchToOriginCountryInExportdeclaration));
                            }

                            if (!invoiceFromDealFile.ExportGoodsItemInfoDTOList.Any(g =>
                                g.CertificateOfOriginID == item.ID))
                                exceptions.Add(new InfException(EMessages
                                    .CertificateNumberISNotMatchToCerrtificateNumberInExportDealFile));

                            var customsItemBy6Digits = customsItemsByIds.Where(ci => ci.ID == goodsItem.CustomsItemID).FirstOrDefault()?.FullClassificationby6digits;

                            if (certificateTypeCode != null && certificateTypeCode.IsCustomsItemMandatory.GetValueOrDefault() && !customsItemBy6Digits.IsNullOrEmpty() && !customsItemsByIds.Any(ci => !customsItemPerInvoice.IsNullOrEmpty() && customsItemPerInvoice.Contains(ci.ID) && !ci.FullClassificationby6digits.IsNullOrEmpty() && ci.FullClassificationby6digits == customsItemBy6Digits))
                            {
                                var parameters = new List<MalamValidationParameter> { new MalamValidationParameter { Value = customsItemBy6Digits } };
                                exceptions.Add(new InfException(EMessages.CustomsItemIsNotMAtchToCustomsItemInExportdeclaration, parameters));
                            }
                        }
                    }
                }
                foreach (var invoiceFromDealFile in updateCetificateOfOriginsDTO.ExportInvoiceInfoDTOList)
                {
                    var goodItemInDealfileInvoiceOfTheCertificate = invoiceFromDealFile.ExportGoodsItemInfoDTOList.Where(gi => gi.CertificateOfOriginID == item.ID);
                    var goodsItemOfTheInvoiceInCertificate = item.CertificateOfOriginInvoiceDetail.FirstOrDefault(i => i.InvoiceNumber == invoiceFromDealFile.ExternalIDNum)?.CertificateOfOriginItemDetail?.Select(gi => gi.CustomsItemID);

                    foreach (var GoodsItemFromDealfile in goodItemInDealfileInvoiceOfTheCertificate)
                    {
                        var customsItemBy6Digits = customsItemsByIds.Where(ci => ci.ID == GoodsItemFromDealfile.CustomsItemID).FirstOrDefault()?.FullClassificationby6digits;
                        if (certificateTypeCode != null && certificateTypeCode.IsCustomsItemMandatory.GetValueOrDefault() && !customsItemBy6Digits.IsNullOrEmpty() &&
                            !customsItemsByIds.Any(ci => !goodsItemOfTheInvoiceInCertificate.IsNullOrEmpty() && goodsItemOfTheInvoiceInCertificate.Contains(ci.ID) && !ci.FullClassificationby6digits.IsNullOrEmpty() && ci.FullClassificationby6digits == customsItemBy6Digits))
                        {
                            var customsItemsOfDeclarationByIds = _servicesAdapter.GetCustomsItemsByIdsSync(new List<CustomsItemsIdsCacheFilter> { new CustomsItemsIdsCacheFilter() { CustomsItemID = GoodsItemFromDealfile.CustomsItemID, Date = DateTime.Now } });
                            var parameters = new List<MalamValidationParameter> { new MalamValidationParameter { Value = customsItemsOfDeclarationByIds.FirstOrDefault()?.FullClassification?.Substring(0, 6) } };
                            exceptions.Add(new InfException(EMessages.CustomsItemInDeclarationIsNotMAtchToCustomsItemsInCertificate, parameters));
                        }
                    }
                }
                if (!exceptions.IsNullOrEmpty())
                {
                    PassGroupdExceptionListToEntity(exceptions);
                }
            }
        }

        public void PassGroupdExceptionListToEntity(List<InfException> exceptions)
        {
            var groupdExceptionList = exceptions.GroupBy(gb => gb.UserMessage);
            foreach (var groupdException in groupdExceptionList)
            {
                if (groupdException.FirstOrDefault() != null && !_requestExceptions.Exceptions.Contains(groupdException.First()))
                    _requestExceptions.Exceptions.Add(groupdException.First());
            }

        }

        public void ExportDeclarationCancellationRequestCommited(UpdateCetificateOfOriginsDTO updateCetificateOfOriginsDTO)
        {
            var certificateOfOrigins = _uow.Repository.GetQuery<CertificateOfOrigin>().Where(coo => updateCetificateOfOriginsDTO.CertificateOfOriginsIDs.Contains(coo.ID)).ToList();
            var cetificateOfOriginsDTO = new UpdateCetificateOfOriginsDTO()
            {
                ExportDeclarationNum = updateCetificateOfOriginsDTO.ExportDeclarationNum,
                LeadDocumentID = updateCetificateOfOriginsDTO.LeadDocumentID
            };
            foreach (var item in certificateOfOrigins)
            {
                item.CertificateOfOriginStatusID = (int)ECertificateOfOriginStatus.Cancelled;
                item.RejectCancelReason =
                    _stringUtil.GetString(EServerTerms.CanceledDeclaration);
                _uow.Repository.Save(item);
                _uow.CommitAllChanges();

                //2. send MessageToAgent

                servicesAdapter.SendMessageToAgent(item, string.Format(SystemTablesUtil.GetUIMessage((int)EMessages.MessageToAgentCertificateOfOrigin), item.CertificateNumber, item.ExportDeclarationNumber), EAgentTalkBackType.CertificateOfOriginCancellation);
                //3. close open task
                var args = new EventUtilArguments(EEventType.ExportDeclarationConnectToCertificateOfOriginCanceled, item);

                EventUtil.RaiseEvent(args);

            }
        }

        #endregion GetCertificateOfOriginById


        #region SaveCertificateOfOrigin

        /// <summary>
        /// SaveCertificateOfOrigin
        /// </summary>
        /// <param name="certificateOfOrigin">certificate Of Origin to save</param>
        /// <param name="requestReason">request Reason for cancelled certificate (update or replace) for events</param>
        /// <param name="certificateToUpdateId">id of certificate To Update for event related entity</param>
        /// <param name="isStatusChanged"></param>
        /// <param name="isRemarksChanged"></param>
        /// <returns></returns>
        public CertificateOfOrigin SaveCertificateOfOrigin(CertificateOfOrigin certificateOfOrigin, ERequestReason? requestReason, int? certificateToUpdateId,
                                                            out bool isStatusChanged, out bool isRemarksChanged)
        {
            var certificateOfOrigincertificateID = certificateOfOrigin.CertificateNumber;
            var certificateOfOriginStatusOriginalValue = certificateOfOrigin.ChangeTracker.OriginalValues.Where(k => k.Key == CertificateOfOrigin.PropCertificateOfOriginStatusID).Select(v => v.Value).FirstOrDefault();
            var certificateOfOriginRemarksOriginalValue = certificateOfOrigin.ChangeTracker.OriginalValues.Where(k => k.Key == CertificateOfOrigin.PropFeedbackRemark).Select(v => v.Value).FirstOrDefault();
            CertificateOfOrigin certificateToCancel;
            var exportDealFileExternalServiceAdapter = Container.Resolve<IExportDealFileExternalServiceAdapter>();
            if (certificateOfOrigin.IsNewInstance())
            {
                if (certificateOfOrigin.CertificateOfOriginStatusID != (int)ECertificateOfOriginStatus.Cancelled)
                {
                    certificateToCancel = _uow.Repository.GetQuery<CertificateOfOrigin>().Where(coo => coo.CertificateNumber == certificateOfOrigincertificateID)
                   .OrderByDescending(a => a.ID).FirstOrDefault();
                    if (certificateToCancel != null)
                    {
                        if (certificateToCancel.CertificateOfOriginStatusID != (int)ECertificateOfOriginStatus.Cancelled)
                        {
                            certificateOfOrigin.CertificateOfOriginIdOfReplacement = certificateToCancel.ID;
                        }
                        certificateToCancel.CertificateOfOriginStatusID = (int)ECertificateOfOriginStatus.Cancelled;
                        var certificateUpdateStr = Environment.NewLine + _stringUtil.GetString(EServerTerms.CertificateUpdateRecived);
                        certificateToCancel.RejectCancelReason += certificateUpdateStr;
                        var args = new EventUtilArguments(EEventType.CertificateOfOriginApplicationCorrected, certificateOfOrigin);
                        EventUtil.RaiseEvent(args);
                        var args1 = new EventUtilArguments(EEventType.CertificateOfOriginUserCancelledCertificate, certificateToCancel)
                        {
                            AdditionalInfo = certificateToCancel.RejectCancelReason,

                        };
                        EventUtil.RaiseEvent(args1);
                        certificateOfOrigin.VersionNumber = certificateToCancel.VersionNumber + 1;
                        certificateToCancel.IsLastVersion = false;
                    }
                    else
                    {
                        certificateOfOrigin.VersionNumber = 1;
                    }
                    certificateOfOrigin.IsLastVersion = true;
                }
            }
            if (certificateOfOrigin.RequestReasonCode != (int)ERequestReason.CertificateCancellation)
            {

                ValidateCertificateOfOrigin(certificateOfOrigin);

                CreateQRCodeIfNeededAndUpload(certificateOfOrigin);
                if (!certificateOfOrigin.IsNewInstance())
                {
                    foreach (var item in certificateOfOrigin.CertificateOfOriginDetails)
                    {
                        CheckSpecificField(item, certificateOfOrigin.TypeID, item.CertificateDetailsTypeCode.EnglishName);
                    }
                }
                else
                {
                    foreach (var item in certificateOfOrigin.CertificateOfOriginDetails)
                    {
                        switch (item.CertificateDetailsTypeCodeID)
                        {
                            case (int)ECertificateDetailsType.DestinationCountry:
                                if (!string.IsNullOrEmpty(item.Value))
                                    item.Value = SystemTablesUtil.GetIdByCode<Country>(Country.PropCountryAlphaCode_2, item.Value).ToString();
                                break;

                            case (int)ECertificateDetailsType.PortOfShipment:
                                if (!string.IsNullOrEmpty(item.Value))
                                    item.Value = SystemTablesUtil.GetIdByCode<InternationalSite>(InternationalSite.PropLocode, item.Value).ToString();
                                break;
                        }
                    }
                }
            }
            if (certificateOfOrigin.CertificateOfOriginStatusID.In((int)ECertificateOfOriginStatus.PendingRelease, (int)ECertificateOfOriginStatus.Published))
            {
                certificateOfOrigin.ApproveUserID = UserUtil.Current.ID;
            }

            SaveCertificateOfOriginInOrm(certificateOfOrigin);
            _uow.CommitAllChanges();

            var leadDocumentByCertificateOfOriginDTO = exportDealFileExternalServiceAdapter.GetLeadDocumentByOldCertificateOfOriginIdAndUpdateToNewCertificateOfOriginId(certificateOfOrigin.CertificateOfOriginIdOfReplacement != default(int) ? certificateOfOrigin.CertificateOfOriginIdOfReplacement : certificateOfOrigin.ID, certificateOfOrigin.ID);
            if (leadDocumentByCertificateOfOriginDTO != null)
            {
                if (leadDocumentByCertificateOfOriginDTO.LeadDocumentTitle != certificateOfOrigin.ExportDeclarationNumber)
                {
                    _requestExceptions.Exceptions.Add(new InfException(EMessages.CertificateNumberISNotMatchToCerrtificateNumberInExportDealFile));
                }
                if (certificateOfOrigin.ExportDeclarationNumber.IsNullOrEmpty())
                {
                    certificateOfOrigin.LeadDocumentID = leadDocumentByCertificateOfOriginDTO.LeadDocumentID;
                    certificateOfOrigin.ExportDeclarationNumber = leadDocumentByCertificateOfOriginDTO.LeadDocumentTitle;
                }
            }
            CheckDeclarationStatus(certificateOfOrigin, false);

            isStatusChanged = CheckIfStatusChangedAndHandleChanges(certificateOfOrigin, certificateOfOriginStatusOriginalValue, requestReason, certificateToUpdateId);
            isRemarksChanged = IsFeedbackRemarksChanged(certificateOfOrigin, certificateOfOriginRemarksOriginalValue);

            return certificateOfOrigin;
        }

        private void CreateQRCodeIfNeededAndUpload(CertificateOfOrigin certificateOfOrigin)
        {
            if (!string.IsNullOrWhiteSpace(certificateOfOrigin.QRCodePath) ||
                certificateOfOrigin.CertificateOfOriginStatusID != (int)ECertificateOfOriginStatus.Published) return;

            var url = Configuration.GetConfig<string>("CertificateOfOriginQueryURL");
            certificateOfOrigin.GUID = Guid.NewGuid();
            url = string.Format(url, certificateOfOrigin.GUID);
            var commonServicesServiceAdapter = Container.Resolve<ICommonServicesServiceAdapter>();
            var qrCodeByteArray = commonServicesServiceAdapter.CreateQRCode(url);

            certificateOfOrigin.QrImage = qrCodeByteArray;

            var docUploader = new DocumentRepositoryUtil();
            using (var stream = new MemoryStream(qrCodeByteArray))
            {
                var fileName = certificateOfOrigin.CertificateNumber + ".jpg";
                var urlPath = docUploader.UploadFile(fileName, (int)EDocumentType.Other, -1, stream);
                certificateOfOrigin.QRCodePath = docUploader.GetDocumentFile(urlPath);
            }
        }

        private bool CheckIfStatusChangedAndHandleChanges(CertificateOfOrigin certificateOfOrigin, object statusOriginalValue, ERequestReason? requestReason, int? certificateToUpdateId)
        {
            var isStatusChanged = false;

            var certificateOfOriginStatusIDOriginalValue = 0;
            if (statusOriginalValue != null)
            {
                int.TryParse(statusOriginalValue.ToString(), out certificateOfOriginStatusIDOriginalValue);

                isStatusChanged = certificateOfOrigin.CertificateOfOriginStatusID != certificateOfOriginStatusIDOriginalValue;
                if (isStatusChanged)
                {
                    var raiseEventUtil = new RaiseEventUtil();
                    raiseEventUtil.RaiseCertificateOfOriginEvents(certificateOfOrigin, requestReason, certificateToUpdateId);
                }
            }
            else
            {
                if (certificateOfOrigin.CertificateOfOriginStatusID == (int)ECertificateOfOriginStatus.Received)
                {
                    isStatusChanged = true;

                    var raiseEventUtil = new RaiseEventUtil();
                    raiseEventUtil.RaiseCertificateOfOriginEvents(certificateOfOrigin, requestReason, certificateToUpdateId);
                }
            }

            //for replaced certificate cancelled event
            if (requestReason != null && requestReason == ERequestReason.CertificateReplacement)
            {
                var raiseEventUtil = new RaiseEventUtil();
                raiseEventUtil.RaiseCertificateOfOriginEvents(certificateOfOrigin, requestReason, certificateToUpdateId);
            }

            return isStatusChanged;
        }

        private bool IsFeedbackRemarksChanged(CertificateOfOrigin certificateOfOrigin, object certificateOfOriginRemarksOriginalValue)
        {
            var isRemarksChanged = false;

            if (certificateOfOriginRemarksOriginalValue != null)
            {
                isRemarksChanged = !certificateOfOrigin.FeedbackRemark.Equals((string)certificateOfOriginRemarksOriginalValue);
            }

            return isRemarksChanged;
        }

        private void SaveCertificateOfOriginInOrm(CertificateOfOrigin certificateOfOrigin)
        {
            if (_uow.Repository != null)
                _uow.Repository.Save(certificateOfOrigin);
        }

        private void ValidateCertificateOfOrigin(CertificateOfOrigin certificateOfOrigin)
        {
            var unValidException = new InfException(EMessages.CertificateOfOriginNotValid, null, new List<InfException>());
            var certificateOfOriginValidationResult = certificateOfOrigin.Validate();

            if (certificateOfOriginValidationResult != null)
            {
                unValidException.Exceptions.AddRange(ConvertValidationErrorsToInfException(certificateOfOriginValidationResult));
            }

            if (!unValidException.Exceptions.IsNullOrEmpty()) throw unValidException;
        }

        #endregion SaveCertificateOfOrigin


        #region PrintCertificateOfOriginAndSaveAttachments

        public List<TemplateResult> PrintCertificateOfOriginAndSaveAttachments(CertificateOfOrigin certificate, string additionalInfo)
        {
            ////var IsProduceTemplateOfCertificateOfOriginWithSSRS = Configuration.GetConfig<bool>("IsProduceTemplateOfCertificateOfOriginWithSSRS");
            var commonService = Container.Resolve<ICommonServicesServiceAdapter>();
            TemplateResult template = null;
            TemplateResult eurPage2Template = null;
            TemplateResult templateForView = null;
            var serviceAdapter = new ServicesAdapter();
            var templateId = 0;
            var isTwoPages = certificate.IsAttachedList;

            var util = new CertificateOfOriginsUtil(_uow);
            var certificateTypeCode = util.GetCertificateTypeCode(certificate.TypeID);
            int? reportId = null;
            if (certificateTypeCode != null)
            {
                reportId = certificateTypeCode.ReportId;
            }

            if (reportId.HasValue)
            {
                if (Configuration.GetConfig<bool>("IssueCertificateOfOriginByWorker") && certificate.IsInPublishingProcess)
                {
                    var certificateType = SystemTablesUtil.GetCodeById<CertificateOfOriginTypeCodeEnum>(certificate.TypeID);

                    SendCertificateToIssueQueue(new IssueCertificateDto() { ReportId = reportId.Value, CertificateOfOriginId = certificate.ID, CertificateNumber = certificate.CertificateNumber, CertificateOfOriginStatusId = certificate.CertificateOfOriginStatusID, CertificateTypeID = certificate.TypeID, CertificateTypeName = certificateType != null ? certificateType.Name : string.Empty, RequestReasonCode = certificate.RequestReasonCode, IsInPublishingProcess = certificate.IsInPublishingProcess, CreateCustomerID = certificate.CreateCustomerID, RejectCancelReason = certificate.RejectCancelReason, InternalApplication = certificate.InternalApplication, FeedbackRemark = certificate.FeedbackRemark, IssuingDate = certificate.IssuingDate, IsDeclarationReleased = certificate.IsDeclarationReleased, GUID = certificate.GUID, OrganizationUnitID = certificate.OrganizationUnitID });
                }
                else
                {
                    template = commonService.GanerateReportAndConvertToTemplateResult(reportId.Value, certificate.ID, additionalInfo);
                }
            }
            else
            {
                switch (certificate.TypeID)
                {
                    case (int)ECertificateOfOriginType.EURMED:
                        if (isTwoPages)
                        {
                            template = serviceAdapter.CreateTemplateSync(certificate.ID, CertificateOfOriginsConstants.EurmedPage2TemplateId, additionalInfo);
                        }
                        else
                        {
                            template = serviceAdapter.CreateTemplateSync(certificate.ID, CertificateOfOriginsConstants.EurmedTemplateId, additionalInfo);
                        }
                        break;
                    case (int)ECertificateOfOriginType.EUR1:
                        if (isTwoPages)
                        {
                            template = serviceAdapter.CreateTemplateSync(certificate.ID, CertificateOfOriginsConstants.Eur1Page2TemplateId, additionalInfo);
                        }
                        else
                        {
                            template = serviceAdapter.CreateTemplateSync(certificate.ID, CertificateOfOriginsConstants.Eur1TemplateId, additionalInfo);
                        }
                        break;
                    case (int)ECertificateOfOriginType.MERCOSUR:
                        templateId = isTwoPages ? CertificateOfOriginsConstants.Mercosur2PagesTemplateId
                                                : CertificateOfOriginsConstants.MercosurTemplateId;
                        template = serviceAdapter.CreateTemplateSync(certificate.ID, templateId, additionalInfo);
                        break;
                    case (int)ECertificateOfOriginType.IsrCol:
                        templateId = isTwoPages ? CertificateOfOriginsConstants.ColIsr2PagesTemplateId
                                                : CertificateOfOriginsConstants.ColIsrTemplateId;
                        template = serviceAdapter.CreateTemplateSync(certificate.ID, templateId, additionalInfo);
                        break;
                    case (int)ECertificateOfOriginType.NonManipulation:
                        template = serviceAdapter.CreateTemplateSync(certificate.ID, CertificateOfOriginsConstants.NonManipulationTemplateId, additionalInfo);
                        break;
                    case (int)ECertificateOfOriginType.Panama:
                        templateId = isTwoPages ? CertificateOfOriginsConstants.Panama2PagesTemplateId
                            : CertificateOfOriginsConstants.PanamaTemplateId;
                        template = serviceAdapter.CreateTemplateSync(certificate.ID, templateId, additionalInfo);
                        break;
                    case (int)ECertificateOfOriginType.SouthKorea:
                        templateId = isTwoPages ? CertificateOfOriginsConstants.SouthKoreaTemplateId2PagesTemplateId
                            : CertificateOfOriginsConstants.SouthKoreaTemplateId;
                        template = serviceAdapter.CreateTemplateSync(certificate.ID, templateId, additionalInfo);
                        break;
                    case (int)ECertificateOfOriginType.UnitedArabEmirates:
                        templateId = isTwoPages ? CertificateOfOriginsConstants.UnitedArabEmiratesTemplateId2PagesTemplateId
                            : CertificateOfOriginsConstants.UnitedArabEmiratesTemplateId;
                        template = serviceAdapter.CreateTemplateSync(certificate.ID, templateId, additionalInfo);
                        break;
                }
            }

            if (template == null) return null;

            var templateResults = new List<TemplateResult>();
            templateResults.Add(template);

            if (eurPage2Template != null) //page 2 in EUR certificates
            {
                templateResults.Add(eurPage2Template);
            }

            if (templateForView != null) //templateForView 2 in EUR certificates
            {
                templateResults.Add(templateForView);
            }

            var saveCertificateAttachmentsArgs = new SaveCertificateAttachmentsArgsDTO()
            {
                CertificatesTemplates = templateResults,
                CertificateID = certificate.ID,
                CertificateNumber = certificate.CertificateNumber,
                CertificateTypeID = certificate.TypeID,
                CertificateRequestReasonCode = certificate.RequestReasonCode,
                AdditionalInfo = additionalInfo
            };
            SaveCertificateOfOriginAttachmentsSync(saveCertificateAttachmentsArgs);

            return templateResults;
        }

        private void SendCertificateToIssueQueue(IssueCertificateDto issueCertificateDto)
        {
            string exchangeName = "IssueCertificateOfOrigin";

            try
            {
                var util = QueueUtilFactory.GetQueueUtil();
                var message = util.CreateQueueMessageBuilder()
                        .SendToExchange(exchangeName)
                        .AddCloudEventMessage(issueCertificateDto)
                        .Build();

                util.SendMessageAsync(message);

            }
            catch (System.Exception exception)
            {
                Logger.WriteError("SendCertificateToIssueQueue", "IssueCertificateOfOrigin", $"{issueCertificateDto.CertificateOfOriginId} exception: {exception}", (int)EModule.CertificateOfOrigins, DateTime.Now);
            }
        }

        public void SaveCertificateOfOriginAttachmentsSync(SaveCertificateAttachmentsArgsDTO saveCertificateAttachmentsArgs)
        {
            var util = WcfInvokerUtilFactory.GetWcfInvokerUtil();
            var message = util.CreateWcfInvokerRequestBuilder()
                .WithActionName("SaveCertificateAttachmentsAction")
                .WithRequest(saveCertificateAttachmentsArgs)
                .Build();

            util.Invoke(message);
        }

        public bool SaveCertificateOfOriginAttachments(SaveCertificateAttachmentsArgsDTO saveCertificateAttachmentsArgsDTO)
        {
            var certificateType = SystemTablesUtil.GetCodeById<CertificateOfOriginTypeCodeEnum>(saveCertificateAttachmentsArgsDTO.CertificateTypeID);
            var serviceAdapter = new ServicesAdapter();

            foreach (var certificateTemplate in saveCertificateAttachmentsArgsDTO.CertificatesTemplates)
            {
                var document = new DocumentDTO()
                {
                    Title = certificateType.Name + " - " + ((saveCertificateAttachmentsArgsDTO.CertificateRequestReasonCode == (int)ERequestReason.Draft || saveCertificateAttachmentsArgsDTO.AdditionalInfo == CertificateOfOriginsConsts.IsDraft) ? CertificateOfOriginsConsts.Draft : CertificateOfOriginsConsts.Final),
                    FileName = string.Format(CertificateOfOriginsConstants.CertificateName, certificateType.Name, saveCertificateAttachmentsArgsDTO.CertificateNumber),
                    TypeID = certificateTemplate.DocumentTypeID,
                    OrganizationUnitID = UserUtil.Current.OrganizationUnitID,
                    EntityId = saveCertificateAttachmentsArgsDTO.CertificateID,
                    EntityTypeId = CertificateOfOriginsConstants.CertificateOfOriginEntityTypeId
                };
                if (document.TypeID == (int)EDocumentType.ExportCertificateOfOrigin)
                {
                    document.DocumentAdditionalFieldValues = new List<DocumentAdditionalFieldValueDTO>()
                    {
                        new DocumentAdditionalFieldValueDTO()
                        {
                            Value = saveCertificateAttachmentsArgsDTO.CertificateNumber,
                            DocumentAdditionaFieldID =
                                CertificateOfOriginsConsts.DocumentAdditionaFieldIDForCertificateNumber
                        }
                    };
                }
                var documentsOfCertificates = serviceAdapter.GetDocumentsByEntitySync(saveCertificateAttachmentsArgsDTO.CertificateID, EEntityType.CertificateOfOrigin);
                var documentEntity = new VirtualEntity()
                {
                    ID = saveCertificateAttachmentsArgsDTO.CertificateID,
                    EntityType = EEntityType.CertificateOfOrigin
                };
                if (!documentsOfCertificates.IsNullOrEmpty())
                {
                    serviceAdapter.DeleteDocument(documentsOfCertificates.Select(c => c.ID).ToList(), documentEntity);
                }
                serviceAdapter.UploadDocumentAndSave(document, certificateTemplate.Content);
            }
            return true;
        }

        #endregion PrintCertificateOfOriginAndSaveAttachments


        #region CancelCetificate

        public void CancelReplacedCertificate(string newCertificateNumber, string certificateToCancelId)
        {
            var certificatesToSave = CancelCertificate(newCertificateNumber, certificateToCancelId, false);
            if (certificatesToSave.IsNullOrEmpty()) return;

            foreach (var certificate in certificatesToSave)
            {
                certificate.AcceptChanges();
                bool isStatusChanged;
                bool isRemarksChanged;
                SaveCertificateOfOrigin(certificate, ERequestReason.CertificateReplacement, null, out isStatusChanged, out isRemarksChanged);
            }
        }

        public void CancelUpdatedCertificates(string certificateToUpdateId, int newCerificateID)
        {
            var certificateToCancel = GetCertificatesToCancel(certificateToUpdateId).Where(v => v.ID != newCerificateID).FirstOrDefault();

            if (certificateToCancel == null) return;


            certificateToCancel.AcceptChanges();
            //bool isStatusChanged;
            //bool isRemarksChanged;
            //SaveCertificateOfOrigin(certificateToCancel, ERequestReason.CertificateUpdate, null, out isStatusChanged, out isRemarksChanged);
        }

        private IEnumerable<CertificateOfOrigin> CancelCertificate(string newCertificateNumber, string certificateId, bool isUpdate)
        {
            var certificates = GetCertificatesToCancel(certificateId);
            var _certificatesToSave = new List<CertificateOfOrigin>();
            foreach (var certificate in certificates)
            {
                certificate.CertificateOfOriginStatusID = (int)ECertificateOfOriginStatus.Cancelled;

                if (isUpdate)
                {
                    var certificateUpdateStr = Environment.NewLine + _stringUtil.GetString(EServerTerms.CertificateUpdateRecived);
                    certificate.RejectCancelReason += certificateUpdateStr;
                }
                else
                {
                    var message = SystemTablesUtil.GetUIMessage((int)EMessages.CertificateReplaced);
                    var replacementReason = string.Format(message, newCertificateNumber);
                    certificate.RejectCancelReason = replacementReason;
                }

                _certificatesToSave.Add(certificate);
            }

            return _certificatesToSave;
        }

        private List<CertificateOfOrigin> GetCertificatesToCancel(string externalId)
        {
            var certificatesResult = GetCertificateOfOriginsByFilter(new CertificateOfOriginFilter { certificateNumber = externalId });
            return certificatesResult.Select(certificate => GetCertificateOfOriginById(certificate.ID)).ToList();
        }

        #endregion CancelCetificate



        public List<ImportAuthenticationRequestDTO> GetAuthenticationRequestByLeadDocumentIDs(List<int> leadDocumentIDs)
        {
            var dataTable = GetLeadDocumentIDsSqlParameters(leadDocumentIDs);

            List<ImportAuthenticationRequestDTO> res = new List<ImportAuthenticationRequestDTO>();
            var parameters = new List<SqlParameter>
            {
                new SqlParameter
                {
                    ParameterName = CertificateOfOriginsConsts.LeadDocumentIDs,
                    SqlDbType = SqlDbType.Structured,
                    Value = dataTable,
                    TypeName = CertificateOfOriginsConsts.DbType
                }
            };

            res = _uow.Repository.ExecuteFunction<ImportAuthenticationRequestDTO>(CertificateOfOriginsConsts.GetAuthenticationRequestByLeadDocumentID, parameters).ToList();

            return res;
        }

        private static object GetLeadDocumentIDsSqlParameters(IEnumerable<int> leadDocumentIDs)
        {
            if (leadDocumentIDs.IsNullOrEmpty()) return null;

            #region Data Table definition and initialization

            var dataTable = new DataTable(CertificateOfOriginsConsts.DbType);
            dataTable.Columns.Add(new DataColumn(CertificateOfOriginsConsts.RowName, typeof(int)));

            foreach (var leadDocumentId in leadDocumentIDs)
            {
                var row = dataTable.NewRow();
                row[CertificateOfOriginsConsts.RowName] = GetParameterValue(leadDocumentId);
                dataTable.Rows.Add(row);
            }

            #endregion Data Table definition and initialization

            return dataTable;
        }

        /// <summary>
        /// Gets the parameter value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static object GetParameterValue(object value)
        {
            return value ?? DBNull.Value;
        }



        public string GetCertificateNumber()
        {
            var result = _uow.Repository.ExecuteFunction<GetCertificateOfOriginNumberResult>(new CertificateOfOriginNumberFilter()).ToList().FirstOrDefault();
            if (result == null) return string.Empty;
            var numerator = result.Column1;
            return CertificateOfOriginsConsts.PrefixIL + numerator.ToString(CertificateOfOriginsConsts.SuffixFormat10Digit);
        }





        public void CheckSpecificField(CertificateOfOriginDetails detail, int certificateTypeId, string fieldName)
        {
            var detailTypeId = detail.CertificateDetailsTypeCodeID;

            switch (detailTypeId)
            {
                case (int)ECertificateDetailsType.ExporterId:
                    var exporterId = CheckIfExporterExist(detail.Value, fieldName);
                    if (exporterId.GetValueOrDefault() != 0)
                        detail.Value = exporterId.ToString();
                    break;
                case (int)ECertificateDetailsType.ExporterCountry:
                case (int)ECertificateDetailsType.CountryOfDeclaration:
                case (int)ECertificateDetailsType.IssuingCountry:
                case (int)ECertificateDetailsType.TransirCountry:
                    CheckIfCountryInSystemAndIsrael(detail, fieldName);
                    break;
                case (int)ECertificateDetailsType.TradeAgreementCountry1:
                    CheckAgreementFirstCountry(detail, certificateTypeId, fieldName);
                    break;
                case (int)ECertificateDetailsType.TradeAgreementCountry2:
                case (int)ECertificateDetailsType.ConsigneeCountry:
                case (int)ECertificateDetailsType.OriginCountry:
                case (int)ECertificateDetailsType.CumulationCountry:
                case (int)ECertificateDetailsType.DestinationCountry:
                    CheckIfCountryIsInTradeAgreement(detail, fieldName, certificateTypeId);
                    break;
                case (int)ECertificateDetailsType.TradeAgreementGroupOfCountries:
                case (int)ECertificateDetailsType.OriginGroupOfCountries:
                case (int)ECertificateDetailsType.DestinationGroupOfCountries:
                case (int)ECertificateDetailsType.CumulationGroupOfCountries:
                    CheckIfCountryGroupIsInTradeAgreement(detail, fieldName, certificateTypeId);
                    break;
                case (int)ECertificateDetailsType.DateOfDeclaration:
                    CheckDeclarationDate(detail);
                    break;
                case (int)ECertificateDetailsType.IsDeclaredByExporter:
                    CheckIfDeclaredByExporter(detail);
                    ConvertBoolFieldToYesNo(detail);
                    break;
                case (int)ECertificateDetailsType.ExportDate:
                    CheckExportDate(detail);
                    break;
                case (int)ECertificateDetailsType.CityOfDeclaration:
                case (int)ECertificateDetailsType.PlaceOfManufacture:
                    CheckCityOfDeclaration(detail, fieldName);
                    break;
                case (int)ECertificateDetailsType.ExpectedExitDate:
                    CheckExpectedExitDate(detail);
                    break;
                case (int)ECertificateDetailsType.ImportDate:
                    CheckImportDate(detail);
                    break;
                case (int)ECertificateDetailsType.IsConsigneeForPrint:
                case (int)ECertificateDetailsType.IsCumulation:
                case (int)ECertificateDetailsType.IsExportDecForPrint:
                case (int)ECertificateDetailsType.IsDeclaredByManufacturer:
                    ConvertBoolFieldToYesNo(detail);
                    break;
                case (int)ECertificateDetailsType.PortOfShipment:
                    CheckIfInternationalSiteExist(detail);
                    break;
                case (int)ECertificateDetailsType.ExportCountry:
                    CheckExportCountry(detail, fieldName, certificateTypeId);
                    break;
                case (int)ECertificateDetailsType.ExportPort:
                case (int)ECertificateDetailsType.PortOfEntrance:
                case (int)ECertificateDetailsType.ExitPort:
                    CheckIfSiteExist(detail);
                    break;
                case (int)ECertificateDetailsType.ExporterName:
                case (int)ECertificateDetailsType.ExporterAddress:
                case (int)ECertificateDetailsType.ConsigneeName:
                case (int)ECertificateDetailsType.ConsigneeAddress:
                case (int)ECertificateDetailsType.ConsigneeRemarks:
                case (int)ECertificateDetailsType.Transport:
                case (int)ECertificateDetailsType.ZipCodeOfManufacture:
                case (int)ECertificateDetailsType.Observations:
                case (int)ECertificateDetailsType.Feedback:
                case (int)ECertificateDetailsType.ImportBillOfLadingNum:
                case (int)ECertificateDetailsType.GoodsDescription:
                case (int)ECertificateDetailsType.DeclaringCompany:
                case (int)ECertificateDetailsType.DeclaringPerson:
                case (int)ECertificateDetailsType.DeclaringPosition:
                case (int)ECertificateDetailsType.ManifestNum:
                case (int)ECertificateDetailsType.ExportBillOFLadingNum:
                    detail.DisplayedValue = detail.Value;
                    break;
                case (int)ECertificateDetailsType.CustomsHouse:
                    CheckIfSiteExistAndCustomsHouse(detail);
                    break;

            }
        }
        private int? CheckIfExporterExist(string customerExternalId, string fieldName)
        {
            var exporterID = servicesAdapter.GetCustomerIDByExternalID(customerExternalId);

            if (exporterID == null)
            {
                var parameters = new List<MalamValidationParameter> { new MalamValidationParameter { FieldName = fieldName } };
                _requestExceptions.Exceptions.Add(new InfException(EMessages.CustomerNotInCustomers, parameters, null));
            }
            return exporterID;
        }


        private void CheckIfCountryInSystemAndIsrael(CertificateOfOriginDetails detail, string fieldName)
        {
            var country = detail.Value;
            var detailTypeId = detail.CertificateDetailsTypeCodeID;

            int countryId;
            if (!int.TryParse(detail.Value, out countryId)) return;

            if (!IsCountryIsrael(countryId))
            {
                switch (detailTypeId)
                {
                    case (int)ECertificateDetailsType.ExporterCountry:
                        _requestExceptions.Exceptions.Add(
                            new InfException(EMessages.IllegalExporterCountry, null, null));
                        break;
                    case (int)ECertificateDetailsType.IssuingCountry:
                        _requestExceptions.Exceptions.Add(new InfException(EMessages.IssuingCountryIllegal, null,
                            null));
                        break;
                    case (int)ECertificateDetailsType.CountryOfDeclaration:
                        _requestExceptions.Exceptions.Add(new InfException(EMessages.ExporterDecCountryIllegal, null,
                            null));
                        break;
                    case (int)ECertificateDetailsType.ExportCountry:
                        _requestExceptions.Exceptions.Add(new InfException(EMessages.ExportCounrtyIllegal, null, null));
                        break;
                    case (int)ECertificateDetailsType.TransirCountry:
                        _requestExceptions.Exceptions.Add(new InfException(EMessages.TransirCountryIllegal, null,
                            null));
                        break;
                }
            }
        }

        private int GetCountryId(string countryCode, string fieldName)
        {
            var countryId = 0;

            try
            {
                countryId = SystemTablesUtil.GetIdByCode<Country>(Country.PropCountryAlphaCode_2, countryCode);
            }
            catch (InfException ex)
            {
                if (ex.UserMessage == EMessages.TheValueInFieldNotExistsInSystem)
                {
                    var parameters = new List<MalamValidationParameter> { new MalamValidationParameter { FieldName = fieldName } };
                    _requestExceptions.Exceptions.Add(new InfException(EMessages.TheValueInFieldNotExistsInSystem, parameters, null));
                }
            }

            return countryId;
        }

        private bool IsCountryIsrael(int countryId)
        {
            var isIsrael = true;
            var countryIsraelId = Configuration.GetConfig<int>(CertificateOfOriginsConstants.CountryIsrael);

            if (countryId != countryIsraelId)
            {
                isIsrael = false;
            }

            return isIsrael;
        }
        private void CheckAgreementFirstCountry(CertificateOfOriginDetails detail, int? certificateOfOriginTypeCodeId, string fieldName)
        {
            var country = detail.Value;

            if (certificateOfOriginTypeCodeId == null) return;

            if (certificateOfOriginTypeCodeId == (int)ECertificateOfOriginType.EUR1
                || certificateOfOriginTypeCodeId == (int)ECertificateOfOriginType.EURMED)
            {
                int countryId;
                if (!int.TryParse(country, out countryId)) return;

                IsCountryIsrael(countryId);
            }

            CheckIfCountryIsInTradeAgreement(detail, fieldName, (int)certificateOfOriginTypeCodeId);
        }
        private void CheckIfCountryIsInTradeAgreement(CertificateOfOriginDetails detail, string fieldName, int certificateTypeId)
        {
            var country = detail.Value;
            var detailTypeId = detail.CertificateDetailsTypeCodeID;
            int countryId;
            if (!int.TryParse(detail.Value, out countryId))
                return;

            #region TradeAgreementService

            var isInTrade = servicesAdapter.IsTradeAgreementForCountry(certificateTypeId, countryId, false);
            if (!isInTrade)
            {
                switch (detailTypeId)
                {
                    case (int)ECertificateDetailsType.TradeAgreementCountry2:
                        _requestExceptions.Exceptions.Add(new InfException(EMessages.SecondCountryNotInAgreement, null, null));
                        break;
                    case (int)ECertificateDetailsType.ConsigneeCountry:
                        _requestExceptions.Exceptions.Add(new InfException(EMessages.ImporterCountryNotInAgreement, null, null));
                        break;
                    case (int)ECertificateDetailsType.OriginCountry:
                        _requestExceptions.Exceptions.Add(new InfException(EMessages.OriginCountryNotInAgreement, null, null));
                        break;
                    case (int)ECertificateDetailsType.DestinationCountry:
                        _requestExceptions.Exceptions.Add(new InfException(EMessages.DestinationCountryNotInAgreement, null, null));
                        break;
                    case (int)ECertificateDetailsType.CumulationCountry:
                        _requestExceptions.Exceptions.Add(new InfException(EMessages.CumulationCountryNotInAgreement, null, null));
                        break;
                }
            }

            #endregion TradeAgreementService

            var countryCode = SystemTablesUtil.GetCodeById<Country>(countryId);
            detail.DisplayedValue = countryCode.EnglishName;
            detail.Value = countryCode.ID.ToString();
        }
        private void CheckIfCountryGroupIsInTradeAgreement(CertificateOfOriginDetails detail, string fieldName, int certificateTypeId)
        {
            var countryGroup = detail.Value;
            var detailTypeId = detail.CertificateDetailsTypeCodeID;
            int countryId;
            if (!int.TryParse(detail.Value, out countryId))
                return;


            #region TradeAgreementService

            var isInTrade = servicesAdapter.IsTradeAgreementForCountry(certificateTypeId, countryId, true);
            if (!isInTrade)
            {
                switch (detailTypeId)
                {
                    case (int)ECertificateDetailsType.TradeAgreementGroupOfCountries:
                        _requestExceptions.Exceptions.Add(new InfException(EMessages.GroupOfCountriesNotInAgreement, null, null));
                        break;
                    case (int)ECertificateDetailsType.OriginGroupOfCountries:
                        _requestExceptions.Exceptions.Add(new InfException(EMessages.OriginGroupOfCountriesNotInAgreement, null, null));
                        break;
                    case (int)ECertificateDetailsType.DestinationGroupOfCountries:
                        _requestExceptions.Exceptions.Add(new InfException(EMessages.DestinationGroupOfCountriesNotInAgreement, null, null));
                        break;
                    case (int)ECertificateDetailsType.CumulationGroupOfCountries:
                        _requestExceptions.Exceptions.Add(new InfException(EMessages.CumulationGroupOfCountriesNotInAgreement, null, null));
                        break;
                }
            }

            var countryGroupCode = SystemTablesUtil.GetCodeById<CountryGroup>(countryId);
            detail.DisplayedValue = countryGroupCode.EnglishName;
            detail.Value = countryGroupCode.ID.ToString();

            #endregion TradeAgreementService
        }

        private int GetCountryGroupId(string countryGroup, string fieldName)
        {
            var groupId = 0;

            try
            {
                int countryGroupId;
                var isParseSucceed = int.TryParse(countryGroup, out countryGroupId);

                if (isParseSucceed)
                {
                    groupId = SystemTablesUtil.GetIdByCode<CountryGroup>(CountryGroup.PropID, countryGroupId);
                }
                else
                {
                    var parameters = new List<MalamValidationParameter> { new MalamValidationParameter { FieldName = fieldName } };
                    _requestExceptions.Exceptions.Add(new InfException(EMessages.TheValueInFieldNotExistsInSystem, parameters, null));
                }
            }
            catch (InfException ex)
            {
                var parameters = new List<MalamValidationParameter> { new MalamValidationParameter { FieldName = fieldName } };
                _requestExceptions.Exceptions.Add(new InfException(EMessages.TheValueInFieldNotExistsInSystem, parameters, null));
            }

            return groupId;
        }
        private void CheckDeclarationDate(CertificateOfOriginDetails detail)
        {
            var strDate = detail.Value;
            DateTime date;
            DateTime.TryParse(strDate, out date);

            if (date > DateTime.Today || date < DateTime.Today.AddDays(-5))
            {
                _requestExceptions.Exceptions.Add(new InfException(EMessages.ExporterDecDateIllegal, null, null));
            }
            else
            {
                detail.DisplayedValue = date.ToShortDateString();
            }
        }
        private void CheckIfDeclaredByExporter(CertificateOfOriginDetails detail)
        {
            bool isDeclaredByExporter;
            bool.TryParse(detail.Value, out isDeclaredByExporter);

            if (!isDeclaredByExporter)
            {
                _requestExceptions.Exceptions.Add(new InfException(EMessages.DeclaringExporter, null, null));
            }
        }
        private void ConvertBoolFieldToYesNo(CertificateOfOriginDetails detail)
        {
            var strBoolField = detail.Value;
            bool boolField;
            bool.TryParse(strBoolField, out boolField);

            detail.DisplayedValue = boolField ? "Yes" : "No";
        }
        private void CheckExportDate(CertificateOfOriginDetails detail)
        {
            var strDate = detail.Value;
            DateTime date;
            DateTime.TryParse(strDate, out date);

            if (date < DateTime.Today || date > DateTime.Today.AddMonths(3))
            {
                _requestExceptions.Exceptions.Add(new InfException(EMessages.ExportDateIllegal, null, null));
            }
            else
            {
                detail.DisplayedValue = date.ToShortDateString();
            }
        }
        private void CheckCityOfDeclaration(CertificateOfOriginDetails detail, string fieldName)
        {
            var city = detail.Value;
            int cityId;
            var isParseSucceed = int.TryParse(city, out cityId);

            if (isParseSucceed)
            {
                var cityCode = SystemTablesUtil.GetCodeById<City>(cityId);

                if (cityCode == null)
                {
                    var parameters = new List<MalamValidationParameter> { new MalamValidationParameter { FieldName = fieldName } };
                    _requestExceptions.Exceptions.Add(new InfException(EMessages.TheValueInFieldNotExistsInSystem, parameters, null));
                }
                else
                {
                    detail.DisplayedValue = cityCode.EnglishName;
                    detail.Value = cityCode.ID.ToString();
                }
            }
            else
            {
                var parameters = new List<MalamValidationParameter> { new MalamValidationParameter { FieldName = fieldName } };
                _requestExceptions.Exceptions.Add(new InfException(EMessages.TheValueInFieldNotExistsInSystem, parameters, null));
            }
        }
        private void CheckExpectedExitDate(CertificateOfOriginDetails detail)
        {
            var strDate = detail.Value;
            DateTime date;
            DateTime.TryParse(strDate, out date);

            if (date < DateTime.Today || date > DateTime.Today.AddMonths(3))
            {
                _requestExceptions.Exceptions.Add(new InfException(EMessages.ExitDateIllegal, null, null));
            }
            else
            {
                detail.DisplayedValue = date.ToShortDateString();
            }
        }
        private void CheckImportDate(CertificateOfOriginDetails detail)
        {
            var strDate = detail.Value;
            DateTime date;
            DateTime.TryParse(strDate, out date);
            detail.DisplayedValue = date.ToShortDateString();
        }
        private void CheckIfSiteExist(CertificateOfOriginDetails detail)
        {
            int siteExteralId;
            if (!int.TryParse(detail.Value, out siteExteralId))
                return;
            var siteId = siteExteralId;


            var siteCode = SystemTablesUtil.GetCodeById<InternationalSite>(siteId);
            detail.Value = siteCode.ID.ToString();
            detail.DisplayedValue = siteCode.EnglishName;

        }
        private void CheckExportCountry(CertificateOfOriginDetails detail, string fieldName, int certificateTypeID)
        {
            var country = detail.Value;
            int countryId;
            if (!int.TryParse(country, out countryId)) return;
            var isCountryIsrael = IsCountryIsrael((countryId));
            if (!isCountryIsrael && certificateTypeID != (int)ECertificateOfOriginType.NonManipulation)
            {
                _requestExceptions.Exceptions.Add(new InfException(EMessages.IllegalExporterCountry, null, null));
            }
            else if (isCountryIsrael && certificateTypeID == (int)ECertificateOfOriginType.NonManipulation)
            {
                _requestExceptions.Exceptions.Add(new InfException(EMessages.ExportCounrtyIllegal, null, null));
            }

            var countryCode = SystemTablesUtil.GetCodeById<Country>(countryId);
            detail.Value = countryCode.ID.ToString();
            detail.DisplayedValue = countryCode.EnglishName;
        }
        private int? GetIdByCode<T>(string externalPropName, string code) where T : class, ILov, new()
        {
            int? typeId = null;

            try
            {
                typeId = SystemTablesUtil.GetIdByCode<T>(externalPropName, code);
            }
            catch (InfException ex)
            {
                _requestExceptions.Exceptions.Add(ex);
            }

            int x;
            if (!int.TryParse(code, out x))
                return null;
            return x;
        }
        private void CheckIfInternationalSiteExist(CertificateOfOriginDetails detail)
        {
            if (detail.Value.IsNullOrEmpty()) return;

            var siteExteralId = detail.Value;
            var siteId = GetIdByCode<InternationalSite>(InternationalSite.PropLocode, siteExteralId);

            if (siteId.HasValue)
            {
                var siteCode = SystemTablesUtil.GetCodeById<InternationalSite>(siteId.Value);
                detail.DisplayedValue = siteCode.EnglishName;
            }
        }
        private void CheckIfSiteExistAndCustomsHouse(CertificateOfOriginDetails detail)
        {
            var siteExteralId = detail.Value;
            var siteId = GetIdByCode<SiteLookup>(SiteLookup.PropExternalSiteNumberForMessages, siteExteralId);

            if (siteId.HasValue)
            {
                var organizationUnitID = SystemTablesUtil.GetCodeById<SiteLookup>(siteId.Value)?.OrganizationUnitID;
                if (organizationUnitID.HasValue)
                {
                    CheckIfCustomsHouse(detail, organizationUnitID.Value);
                }
            }
        }
        private void CheckIfCustomsHouse(CertificateOfOriginDetails detail, int orgUnitId)
        {
            var isCustomsHouse = servicesAdapter.IsOrganzationUnitCustomHouse(orgUnitId);
            if (!isCustomsHouse)
            {
                _requestExceptions.Exceptions.Add(new InfException(EMessages.CustomsHouseNotInTable, null, null));
            }
            else
            {
                var orgUnitCode = SystemTablesUtil.GetCodeById<OrganizationUnitLOV>(orgUnitId);
                detail.Value = orgUnitCode.ID.ToString();
                detail.DisplayedValue = orgUnitCode.EnglishName;
            }
        }
        public EventUtilArguments RaiseTaskNewCertificateOfOriginCheck(CertificateOfOrigin certificateOfOrigin, EEventType eventType)
        {
            var servicesAdapter = new ServicesAdapter();
            EventUtilArguments arg;
            //פתח משימה PC_Tsk01_NewCertificateOfOriginCheck
            arg = new EventUtilArguments(eventType, new VirtualEntity(certificateOfOrigin)
            {
                IOrganizationUnitTypeOrganizationUnitTypeID = (int)EOrganizationUnitType.Export,
                OrganizationUnitID = certificateOfOrigin.OrganizationUnitID
            });

            int? assesorId = null;
            if (certificateOfOrigin.LeadDocumentID.HasValue)
            {
                LatestUserHandlingEntityTasksFilter filter = new LatestUserHandlingEntityTasksFilter()
                {
                    EntityID = (int)certificateOfOrigin.LeadDocumentID,
                    EntityType = EEntityType.ExportLeadDocument,
                    OrganizationUnitTypeID = (int)EOrganizationUnitType.Export,
                    OrganizationUnitID = certificateOfOrigin.OrganizationUnitID


                };
                assesorId = servicesAdapter.GetLatestUserHandlingEntityTasksWithTaskUnification(filter); //servicesAdapter.GetExportDeclarationAssessor(item.ExportDeclarationNumber);
            }
            if (assesorId.HasValue)
            {
                arg.TaskArguments = new EventTaskArguments
                {
                    PreferredUserID = assesorId,
                };
            }
            var certificateNumbers = GetCertificateOfOriginByDeclaration(certificateOfOrigin)
                .Where(cn => !cn.Equals(certificateOfOrigin.CertificateNumber)).ToList();

            if (!certificateNumbers.IsNullOrEmpty())
            {
                var message =
                    SystemTablesUtil.GetUIMessage((int)EMessages.AdditionalCertificatesToExportDeclaration);
                var messageParameter = string.Empty;
                switch (certificateNumbers.Count)
                {
                    case 1:
                        messageParameter = certificateNumbers.First();
                        break;
                    case 2:
                        messageParameter = certificateNumbers.First() + ", ";
                        messageParameter += certificateNumbers.Last();
                        break;
                    default:
                        if (certificateNumbers.Count > 2)
                        {
                            messageParameter = ". לדוגמה:" + certificateNumbers.First() + ", ";
                            messageParameter += certificateNumbers.Last();
                        }

                        break;
                }

                arg.AdditionalInfo = string.Format(message, messageParameter);
            }
            return arg;
        }
        public List<string> GetCertificateOfOriginByDeclaration(CertificateOfOrigin certificateOfOrigin)
        {
            var exportDealFileExternalServiceAdapter = Container.Resolve<IExportDealFileExternalServiceAdapter>();

            var exportDeclarationDetailsDTO = certificateOfOrigin.ExportDeclarationDetailsDTO != null ? certificateOfOrigin.ExportDeclarationDetailsDTO
             : (certificateOfOrigin.LeadDocumentID != null || certificateOfOrigin.ExportDeclarationNumber != null) ? exportDealFileExternalServiceAdapter.GetExportDeclarationDetailsForCertificateOfOrigion(certificateOfOrigin.LeadDocumentID, certificateOfOrigin.ExportDeclarationNumber)
             : null;

            if (exportDeclarationDetailsDTO == null) return null;
            var exportDeclarationInfoDTO = exportDealFileExternalServiceAdapter.GetExportDeclarationInfoForPC(exportDeclarationDetailsDTO.LeadDocumentID);

            if (exportDeclarationInfoDTO == null) return null;
            var certificatesIds = exportDeclarationInfoDTO.ExportInvoiceInfoDTOList.SelectMany(i => i.ExportGoodsItemInfoDTOList).Select(gi => gi.CertificateOfOriginID).ToList();

            var certificateNumbers =
                _uow.Repository.GetQuery<CertificateOfOrigin>().Where(
                    c => certificatesIds.Contains(c.ID) &&
                    c.CertificateOfOriginStatusID != (int)ECertificateOfOriginStatus.Cancelled &&
                    c.CertificateOfOriginStatusID != (int)ECertificateOfOriginStatus.Rejected &&
                    c.CertificateOfOriginStatusID != (int)ECertificateOfOriginStatus.Error).Select(c => c.CertificateNumber).ToList();
            return certificateNumbers;
        }
        public void CancelCertificateOfOriginFromMessage(CertificateOfOrigin certificateToCancel)
        {
            if (certificateToCancel != null)
            {
                certificateToCancel.CertificateOfOriginStatusID = (int)ECertificateOfOriginStatus.Cancelled;
                certificateToCancel.RejectCancelReason = SystemTablesUtil.GetUIMessage((int)EMessages.CertificateOfOriginCancelFromMessage);
                var args = new EventUtilArguments(EEventType.CertificateOfOriginUserCancelledCertificate, certificateToCancel)
                {
                    AdditionalInfo = certificateToCancel.RejectCancelReason,

                };
                _uow.Repository.Save(certificateToCancel);
                _uow.CommitAllChanges();
                EventUtil.RaiseEvent(args);
            }
        }

        public CertificateOfOrigin GetCertificateOfOriginByExternalId(string externalId)
        {
            if (string.IsNullOrWhiteSpace(externalId)) return null;
            CertificateOfOrigin certificate = null;
            var certificateResult = GetCertificateOfOriginsByFilter(new CertificateOfOriginFilter { certificateNumber = externalId }).FirstOrDefault();
            if (certificateResult != null)
            {
                certificate = GetCertificateOfOriginById(certificateResult.ID, isFromMessage: true);
            }
            return certificate;
        }


    }

}