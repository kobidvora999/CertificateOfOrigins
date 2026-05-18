using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Design;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using Customs.CRM.CertificateOfOrigins.BL.ServiceAdapters;
using Customs.CRM.CertificateOfOrigins.InternalCommon;
using Customs.CRM.CertificateOfOrigins.InternalCommon.Common;
using Customs.CRM.Entities;
using Customs.CertificateOfOrigins.Entities;
using Customs.FinanceInfr.Collateral.ExternalCommon.Common;
using Customs.Inf.CommonService.ExternalCommon;
using Customs.Infrastructure.DocumentManagement.ExternalCommon;
using Customs.Infrastructure.DocumentManagement.ExternalProxy;
using Customs.Infrastructure.Entities;
using Customs.Shared.Entities;
using MalamTeam.Infrastructure.GeneralServices.BaseLayer;
using MalamTeam.Infrastructure.GeneralServices.CommonBase;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.Base_Entities;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.Extensions;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.Interfaces;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.ORM;
using MalamTeam.Infrastructure.GeneralServices.Environment.Enums;
using Microsoft.Data.Extensions;
using Microsoft.Practices.Unity;
using Customs.Infrastructure.Tasks.ExternalCommon;
using Customs.Infrastructure.Tasks.ExternalCommon.DTOs;
using MalamTeam.Infrastructure.GeneralServices.Environment.Const;
using Customs.CRM.External;
using Customs.Inf.MMI.Common.Common;
using Customs.Infrastructure.SystemTables.ExternalCommon;
using Customs.StockPileData.Customers.ExternalCommon.Common;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.Communication;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.EntityValidation;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.Exceptions;
using MalamTeam.Infrastructure.GeneralServices.EAISchema;

namespace Customs.CRM.CertificateOfOrigins.BL
{
    /// <summary>
    /// 
    /// </summary>
    public class AuthenticationRequestBL : BaseBL
    {

        #region Base

        public AuthenticationRequestBL(IUnitOfWork uow)
            : base(uow)
        {
            Container.RegisterTypeIfMissing(typeof(ICollateralServiceAdapter), typeof(CollateralServiceAdapter), true);
            Container.RegisterTypeIfMissing(typeof(ITasksServiceAdapter), typeof(TasksServiceAdapter), true);
        }

        #endregion Base

        #region Request

        public List<DocumentDTO> GetEntityDocuments(CertificateOfOriginsImportAuthenticationRequest importAuthenticationRequest)
        {
            var requestedDocuments =
                _uow.Repository.GetQuery<CertificateOfOriginsImportAuthenticationRequest>()
                    .Where(cr => cr.LeadDocumentID == importAuthenticationRequest.LeadDocumentID)
                    .ToList();

            var requestedDocumentIDs = requestedDocuments.Select(rd => rd.DocumentID).ToList();
            
            var documentTypeIDs = Configuration.GetConfig<string>("CertificateOfOriginsDocumentsFilter");
            var split = documentTypeIDs.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            var documentFilter = split.Select(s => Convert.ToInt32(s)).ToList();
          
            var entityDocuments =
                Container.Resolve<IDocumentsExternalProxy>().GetDocumentsByEntitySync(
                    importAuthenticationRequest.LeadDocumentID, EEntityType.ImportDeclaration);
            entityDocuments = entityDocuments.Where(entDoc => !requestedDocumentIDs.Contains(entDoc.ID)).ToList();
            var docs = new List<DocumentDTO>();

            var entityDocumentsFilterdList = entityDocuments.Where(d => documentFilter.Any(f => f == d.TypeID)).ToList();
            if (!requestedDocuments.IsNullOrEmpty())
            {
                entityDocumentsFilterdList =
                    entityDocumentsFilterdList.Where(d => requestedDocuments.All(r => r.DocumentID != d.ID) && d.ID != 0)
                                              .ToList();
            }

            var Ids = entityDocumentsFilterdList.Select(e => e.ID);
            var documentIDsByOtherLeadDocumentID =
                _uow.Repository.GetQuery<CertificateOfOriginsImportAuthenticationRequest>()
                    .Where(cr => Ids.Contains(cr.DocumentID) && cr.LeadDocumentID != importAuthenticationRequest.LeadDocumentID)
                    .Select(cr => cr.DocumentID).ToList();
            if (!documentIDsByOtherLeadDocumentID.IsNullOrEmpty())
            {
                entityDocumentsFilterdList =
                    entityDocumentsFilterdList.Where(d => !documentIDsByOtherLeadDocumentID.Contains(d.ID)).ToList();
            }

            if (entityDocumentsFilterdList.IsNullOrEmpty())
            {
                return new List<DocumentDTO>();
            }
         
            foreach (var entityDocument in entityDocumentsFilterdList)
            {
                var relatedEntities = new List<EntityDocumentDto>();
                entityDocument.EntityDocument.ToList().ForEach(e =>
                    relatedEntities.Add(new EntityDocumentDto {EntityId = e.EntityID, EntityTypeId = e.EntityTypeID}));
                var documentType = SystemTablesUtil.GetCodeById<DocumentType>(entityDocument.TypeID, true).Name;
                var doc = new DocumentDTO
                    {
                        ID = entityDocument.ID,
                        IsIncoming = entityDocument.IsIncoming,
                        CreateDate = entityDocument.CreateDate,
                        Title = entityDocument.Title,
                        TypeName = documentType,
                        IsAccepted = entityDocument.IsAccepted,
                        IsRequired = entityDocument.IsRequired,
                        Notes = entityDocument.ID + " " + entityDocument.Title + " " + documentType,
                        ExternalID = entityDocument.ExternalID,
                        TypeID = entityDocument.TypeID,
                        StringDynamicParams = entityDocument.Notes,
                        OtherRelatedEntities = relatedEntities
                };

                docs.Add(doc);
            }

            return docs;
        }

        public CertificateOfOriginsImportAuthenticationRequest SaveImportAuthenticationRequest(CertificateOfOriginsImportAuthenticationRequest importAuthenticationRequestResult)
        {
            var tasksAdapter = Container.Resolve<ITasksServiceAdapter>();
            if (!importAuthenticationRequestResult.Collaterals.IsNullOrEmpty())
                importAuthenticationRequestResult.CollateralID = importAuthenticationRequestResult.Collaterals.First().CollateralRequestID;


            if (!importAuthenticationRequestResult.Collaterals.IsNullOrEmpty())
            {
                ChangeTempCollateralRequest(importAuthenticationRequestResult.Collaterals);
            }

            switch (importAuthenticationRequestResult.DecisionID)
            {
                case (int) EAuthenticationRequestDecision.NewAuthenticationRequest:
                    if (!importAuthenticationRequestResult.IsCurrentUserHandleRequest &&
                        !tasksAdapter.IsTaskExistsOnEntity(new VirtualEntity(importAuthenticationRequestResult),
                            (int) ETaskType.SetDecisionBeforeAssociation))
                        RaiseNewRequestEvent(importAuthenticationRequestResult);
                    break;
                case (int) EAuthenticationRequestDecision.AuthenticationRequried:
                    var tasks = GetTaskDetailsForRequestByTaskType(importAuthenticationRequestResult,
                        new List<int> {(int) ETaskType.HandleRejectedAuthenticationRequest});
                    if (!tasks.IsNullOrEmpty())
                    {
                        var args = new EventUtilArguments(
                            EEventType.ImportAuthenticationRequestProcessedWithWasRejected, new VirtualEntity
                            {
                                EntityType = EEntityType.ImportAuthenticationRequest,
                                ID = importAuthenticationRequestResult.ID,
                                Title = importAuthenticationRequestResult.ID.ToString(),
                            });
                        EventUtil.RaiseEvent(args);
                        RaiseNewRequestEvent(importAuthenticationRequestResult);

                    }

                    break;
                default:
                    RaiseSetDecisionBeforeAssociationEvent(importAuthenticationRequestResult);
                    SendDecisionMessage(importAuthenticationRequestResult);
                    break;
            }
       

            if (importAuthenticationRequestResult.VendorId == 0)
            {
                importAuthenticationRequestResult.VendorId = null;
            }

            if (importAuthenticationRequestResult.DecisionID == (int)EAuthenticationRequestDecision.AuthenticationNeedless)
            {
                EventUtil.RaiseEvent(new EventUtilArguments(EEventType.AuthenticationRequestRejected, importAuthenticationRequestResult)
                {
                    TaskArguments = new EventTaskArguments()
                    {
                        TaskAssignmentArguments = new TaskAssignmentArguments()
                        {
                            SingleUserTaskAssignmentFilter = new SingleUserAssignmentFilter()
                            {
                                UserId = importAuthenticationRequestResult.UserResponseID
                            }
                        }
                    }
                });
            }

            _uow.Repository.Save(importAuthenticationRequestResult);
            _uow.CommitAllChanges();

            return importAuthenticationRequestResult;
        }

        public List<GetImportAuthenticationRequestResult> GetAuthenticationRequestByFilter(ImportAuthenticationRequestFilter filter)
        {
            var result = _uow.Repository.ExecuteFunction<GetImportAuthenticationRequestResult>(filter).ToList();
            return result;
        }

        public void GetLeadDocumentProperties()
        {
            //var entityDocuments =
            //    Container.Resolve<IUsersExternalContract>().GetUserSync();
        }

        /// <summary>
        /// Activate from Task (Coordinator sets decision)
        /// Activate from Search View 
        /// </summary>
        /// <param name="documentId"></param>
        /// <returns></returns>
        public CertificateOfOriginsImportAuthenticationRequest GetAuthenticationRequestByID(int documentId)
        {
            var spFilter = new ResultSetFilter<CertificateOfOriginsImportAuthenticationRequest>
            {
                StoredProcedureName = CertificateOfOriginsConsts.GetRequestByIdProcedure,
                MaterializeFunction = MaterializeForGetRequest,
                Parameters =
                                       {
                                           new SqlParameter
                                               {
                                                   ParameterName = "@DocumentId",
                                                   SqlDbType = SqlDbType.Int,
                                                   Value = documentId
                                               }
                                       }
            };
            var importAuthenticationRequest = _uow.Repository.ExecuteResultSetFunction(spFilter);
            if (importAuthenticationRequest != null)
                GetAdditionalInfoForRequest(importAuthenticationRequest);
            importAuthenticationRequest.AdditionalRequestsForSearchInDays =
                Configuration.GetConfig<int>("AdditionalRequestsForSearchInDays");
            ;
           
            importAuthenticationRequest.IsVendorByIssuingCountryID = AuthenticationRequestFileHelper.IsVendor(importAuthenticationRequest.IssuingCountryID);
            return importAuthenticationRequest;
        }

        private void GetAdditionalInfoForRequest(CertificateOfOriginsImportAuthenticationRequest importAuthenticationRequest)
        {
            var decisions = _uow.Repository.GetQuery<CertificateOfOriginsDecision>().ToList();
            importAuthenticationRequest.Decisions = decisions;

            var collateralList = GetCollateralsForAuthenticationRequest(importAuthenticationRequest);
            if (!collateralList.IsNullOrEmpty())
            {
                importAuthenticationRequest.Collaterals = new ObservableCollection<CollateralRequestDTO>();
                collateralList.ForEach(c => importAuthenticationRequest.Collaterals.Add(c));
            }

            var tasks = GetTaskDetailsForRequestByTaskType(importAuthenticationRequest, new List<int> { (int)ETaskType.SetDecisionBeforeAssociation, (int)ETaskType.SendReminderForImporter, (int)ETaskType.HandleRejectedAuthenticationRequest });
            
            importAuthenticationRequest.IsCurrentUserHandleRequest = tasks.Any(t=>t.UserID == UserUtil.Current.ID);
            importAuthenticationRequest.IsCurrentUserHasOpenTask = tasks.Any(t => t.UserID == UserUtil.Current.ID && t.IsTaskInProgress);
            importAuthenticationRequest.EntityTypeAndIDsToSearch = new Dictionary<EEntityType, List<int>>();
            importAuthenticationRequest.EntityTypeAndIDsToSearch.Add(EEntityType.ImportDeclaration,new List<int> {importAuthenticationRequest.LeadDocumentID});
        }
        private List<IsTaskExistResultDTO> GetTaskDetailsForRequestByTaskType(CertificateOfOriginsImportAuthenticationRequest request, List<int> taskTypeIds)
        {
            var tasksAdapter = Container.Resolve<ITasksServiceAdapter>();

            var filter = new IsTaskExistFilter
            {
                EntityID = request.DocumentID,
                EntityTypeID = (int)EEntityType.ImportAuthenticationRequest,
                TaskTypeIDs = taskTypeIds,
            };

            var result = tasksAdapter.IsTaskExist(filter);
            return result;
        }

        private List<LatestUserHandlingUserForTaskTypeResult> GetLatestUserHandleRequestByTaskType(CertificateOfOriginsImportAuthenticationRequest request, List<int> taskTypeIds)
        {
            var tasksAdapter = Container.Resolve<ITasksServiceAdapter>();

            var filter = new LatestUserHandlingUserForTaskTypeFilter
            {
                EntityID = request.DocumentID,
                EntityType = EEntityType.ImportAuthenticationRequest,
                TaskTypeIDs = taskTypeIds,
            };

            var result = tasksAdapter.GetLatestUserHandlingUserForTaskType(filter);
            return result;
        }

        #region ReminderForImporterScheduler
        public List<ReminderForImporterSchedulerDTO> GetImportAuthenticationRequestsForReminderForImporterScheduler()
        {
            var result = _uow.Repository.ExecuteFunction<ReminderForImporterSchedulerDTO>(CertificateOfOriginsConsts.GetImportAuthenticationRequestsForReminderForImporterScheduler, new List<SqlParameter>()).ToList();
            return result;
        }

        public void RaiseEventForReminderForImporterScheduler(ReminderForImporterSchedulerDTO dto)
        {
            var args = new EventUtilArguments(EEventType.NewReminderForImporterCreated, new VirtualEntity
            {
                EntityType = EEntityType.ImportAuthenticationRequest,
                ID = dto.DocumentID,
                Title = dto.DocumentID.ToString(),
                OrganizationUnitID = dto.OrganizationUnitID,
            })
            {
                TaskArguments = new EventTaskArguments { OpenIdenticalTaskExistsBehaviour = EOpenIdenticalTaskExistsBehaviour.CloseOld },
                RelatedEntities = new List<IEntity> { new VirtualEntity(dto.AuthenticationFileID, (int)EEntityType.AuthenticationRequestFile) },
                OrganizationUnitTypeID = (int)EOrganizationUnitType.ClaliMakor,
            };

            EventUtil.RaiseEvent(args); //open task type SendReminderForImporter

        }

        #endregion ReminderForImporterScheduler

        public CertificateOfOriginsImportAuthenticationRequest HandleImportAuthenticationRequestDeliveryForImporterSent(CertificateOfOriginsImportAuthenticationRequest authenticationRequest)
        {
            return HandleReminderOrDeliveryRequestSentToImporter(authenticationRequest,
                    EEventType.NewDeliveryForImporterSent,
                    EAuthenticationRequestDecision.LetterForImporterWasSent);
        }

        public CertificateOfOriginsImportAuthenticationRequest HandleImportAuthenticationRequestDeliveryReminderForImporterSent(CertificateOfOriginsImportAuthenticationRequest authenticationRequest)
        {
            return HandleReminderOrDeliveryRequestSentToImporter(authenticationRequest,
                    EEventType.NewDeliveryReminderForImporterSent,
                    EAuthenticationRequestDecision.ReminderForImporterWasSent);

        }

        public CertificateOfOriginsImportAuthenticationFileDetails HandleImportAuthenticationRequestDeliveryAndReminderForVendorSent(CertificateOfOriginsImportAuthenticationFileDetails authenticationFile, bool isDelivery)
        {
            if(!isDelivery) // If it's a reminder
                authenticationFile.AuthenticationFileStatusID = (int) EAuthenticationFileStatus.AuthenticationRequestReminderWasSend;
            UpdateFileAfterDelivery(authenticationFile);

            return authenticationFile;
        }
        public bool CheckIfExistsAdditionalRequestsForImporter(CertificateOfOriginsImportAuthenticationRequest request)
        {
            var sqlParameters = new List<SqlParameter>
            {
                new SqlParameter
                {
                    ParameterName = "@ImporterID",
                    SqlDbType = SqlDbType.Int,
                    Value = request.ImporterID
                },
                new SqlParameter
                {
                    ParameterName = "@VendorID",
                    SqlDbType = SqlDbType.Int,
                    Value = request.VendorId
                },
                new SqlParameter
                {
                    ParameterName = "@CustomerID",
                    SqlDbType = SqlDbType.Int,
                    Value = request.CustomerID
                },
                new SqlParameter
                {
                    ParameterName = "@CountryID",
                    SqlDbType = SqlDbType.Int,
                    Value = request.IssuingCountryID
                }
            };
           var result = _uow.Repository.ExecuteDataSet(CertificateOfOriginsConsts.CheckIfExistsAdditionalRequestsForImporter, sqlParameters).Tables[0].Rows[0][0];
            return (bool) result;
        }


        public bool CheckIfExistsAdditionalRequestsForVendor(int vendorId)
        {
            var sqlParameters = new List<SqlParameter>
            {
                new SqlParameter
                {
                    ParameterName = "@VendorID",
                    SqlDbType = SqlDbType.Int,
                    Value = vendorId
                }
            };
            var result = _uow.Repository.ExecuteDataSet(CertificateOfOriginsConsts.CheckIfExistsAdditionalRequestsForVendor, sqlParameters).Tables[0].Rows[0][0];
            return (bool)result;
        }

        public NavigationToVendorView GetPathsForNavigationToVendor()
        {
            var pathID = CertificateOfOriginsConsts.NavigationToVendorPathID;
            var navigationPaths= new List<NavigationPath>();

            navigationPaths = _uow.Repository.GetQuery<NavigationPath>().Where(p => p.PathID == pathID).ToList();

            var viewPaths = new ObservableCollection<NavigationToVendorPath>();
            foreach (var navigationPath in navigationPaths)
            {
                viewPaths.Add(new NavigationToVendorPath
                {
                    ID = navigationPath.PathRouteID,
                    PathID = navigationPath.PathID,
                    PageNameID = navigationPath.PageNameID,
                    ParentPathRouteID = navigationPath.ParentPathRouteID,
                    ViewID = navigationPath.ViewID,
                    Name = navigationPath.PageNameID.HasValue ? navigationPath.PageName : navigationPath.ViewName
                });
            }

            return new NavigationToVendorView
            {
                PathID = pathID,
                ViewPaths = viewPaths
            };
        }

        #region private

        private void RaiseNewRequestEvent(CertificateOfOriginsImportAuthenticationRequest importAuthenticationRequestResult)
        {
            var args = new EventUtilArguments(EEventType.NewAuthenticationRequest, new VirtualEntity
            {
                EntityType = EEntityType.ImportAuthenticationRequest,
                ID = importAuthenticationRequestResult.DocumentID,
                Title = importAuthenticationRequestResult.DocumentID.ToString(),
                OrganizationUnitID = importAuthenticationRequestResult.OrganizationUnitID
        });
            args.AdditionalInfo = importAuthenticationRequestResult.DocumentID.ToString();

            if (importAuthenticationRequestResult.AuthenticationFileID != null)
                args.RelatedEntities = new List<IEntity> { new VirtualEntity { ID = importAuthenticationRequestResult.AuthenticationFileID ?? 0, EntityType = EEntityType.AuthenticationRequestFile } };


            EventUtil.RaiseEvent(args); // open task 406 SetDecisionBeforeAssociation
        }

        private void RaiseSetDecisionBeforeAssociationEvent(CertificateOfOriginsImportAuthenticationRequest importAuthenticationRequestResult)
        {
            var args = new EventUtilArguments(EEventType.NewDecisionBeforeAssociation, new VirtualEntity
            {
                EntityType = EEntityType.ImportAuthenticationRequest,
                ID = importAuthenticationRequestResult.DocumentID,
                Title = importAuthenticationRequestResult.DocumentID.ToString(),
            });
            args.AdditionalInfo = importAuthenticationRequestResult.DocumentID.ToString();
           
            EventUtil.RaiseEvent(args); //close task type SetDecisionBeforeAssociation
        }

        private void ChangeTempCollateralRequest(ObservableCollection<CollateralRequestDTO> collaterals)
        {
            var collateralAdapter = Container.Resolve<ICollateralServiceAdapter>();
            var changeTempCollateralRequestDTO =
                collaterals.Select(collateral => new ChangeTempCollateralRequestDTO
                {
                    CollateralRequestID = collateral.CollateralRequestID,
                    RelatedEntityID = collateral.RelatedEntity.ID,
                    EntityExternalID = collateral.RelatedEntity.ID.ToString()
                }).ToList();
            collateralAdapter.ChangeTempCollateralRequest(changeTempCollateralRequestDTO);
        }

        private CertificateOfOriginsImportAuthenticationRequest MaterializeForGetRequest(DbDataReader reader)
        {
            var importAuthenticationRequest =
                reader.Materialize<CertificateOfOriginsImportAuthenticationRequest>().FirstOrDefault();
            if (importAuthenticationRequest == null) return null;
            importAuthenticationRequest.AcceptChanges();


            reader.NextResult();
            var importAuthenticationRequestItemDetails = reader.Materialize<CertificateOfOriginsItemDetails>().ToList();
            importAuthenticationRequestItemDetails.ForEach(ia => ia.AcceptChanges());
            importAuthenticationRequest.CertificateOfOriginsItemDetails.AddRange(importAuthenticationRequestItemDetails);

            reader.NextResult();
            var importAuthenticationRequestDocument = reader.Materialize<Document>().FirstOrDefault();
            importAuthenticationRequestDocument.AcceptChanges();
            importAuthenticationRequest.Document = importAuthenticationRequestDocument;

            if (importAuthenticationRequest.Document != null)
                importAuthenticationRequest.Document.FileUrl =
                    SystemTablesUtil.GetCodeById<DocumentType>(importAuthenticationRequest.Document.TypeID, true).Name;
            return importAuthenticationRequest;
        }

        private CertificateOfOriginsImportAuthenticationRequest HandleReminderOrDeliveryRequestSentToImporter(CertificateOfOriginsImportAuthenticationRequest authenticationRequest, EEventType eventType, EAuthenticationRequestDecision decision)
        {
            var args = new EventUtilArguments(eventType, new VirtualEntity
            {
                EntityType = EEntityType.ImportAuthenticationRequest,
                ID = authenticationRequest.DocumentID,
                Title = authenticationRequest.DocumentID.ToString(),
                OrganizationUnitID = authenticationRequest.OrganizationUnitID
            });
            if (authenticationRequest.AuthenticationFileID != null)
                args.RelatedEntities = new List<IEntity> { new VirtualEntity { ID = authenticationRequest.AuthenticationFileID ?? 0, EntityType = EEntityType.AuthenticationRequestFile } };

            authenticationRequest.DecisionID = (int)decision;
            authenticationRequest.LastDeliveryForImporter = DateTime.Today;
            authenticationRequest.UpdateDate = DateTime.Today;
            
            UpdateFileAfterDelivery(authenticationRequest.CertificateOfOriginsImportAuthenticationFileDetails);

            _uow.Repository.Save(authenticationRequest);
            _uow.CommitAllChanges();

            EventUtil.RaiseEvent(args);


            return authenticationRequest;
        }

        #endregion private

        #endregion Request

        #region File

        public CertificateOfOriginsImportAuthenticationFileDetails CreateNewAuthenticationFile(List<GetImportAuthenticationRequestResult> importAuthenticationRequests)
        {
            if (importAuthenticationRequests.IsNullOrEmpty()) return null;

            var ids = importAuthenticationRequests.Select(a => a.DocumentID).ToList();
            var rquests = _uow.Repository.GetQuery<CertificateOfOriginsImportAuthenticationRequest>().ToList();
            var requestList = rquests.Where(a => ids.Contains(a.ID)).ToList();
            var req = requestList.FirstOrDefault(r => r.AuthenticationFileID != null);
            if (req != null)
            {
                var file1 = _uow.Repository.GetQuery<CertificateOfOriginsImportAuthenticationFileDetails>()
                    .First(cooiad => cooiad.ID == req.AuthenticationFileID);
                
                throw new InfException(EMessages.FileExistForRequest,new List<MalamValidationParameter>()
                {
                    new MalamValidationParameter() {Value = req.DocumentID},
                    new MalamValidationParameter() {Value = file1.ID}
                });
            }
            
            var file = CreateNewAuthenticationFile(importAuthenticationRequests.First());

            file.CustomerIDList = new List<int>();
            var customerIDsList = importAuthenticationRequests.Where(r => r.CustomerID.HasValue).Select(c=>c.CustomerID.Value).ToList();
            if (!customerIDsList.IsNullOrEmpty())
            {
                file.CustomerIDList.AddRange(customerIDsList);
            }
            foreach (var request in importAuthenticationRequests)
            {
                RaiseEventNewDecisionBeforeAssociation(request);
            }
            file.OrganizationUnitID = importAuthenticationRequests.First().OrganizationUnitIDNum.Value;
            _uow.Repository.Save(file);
            _uow.CommitAllChanges();

            UpdateFileId(importAuthenticationRequests, file.ID);
            _uow.CommitAllChanges();
          
          var args = new EventUtilArguments(EEventType.NewAuthenticationRequestFile, new VirtualEntity
            {
                EntityType = EEntityType.AuthenticationRequestFile,
                ID = file.ID,
                Title = file.ID.ToString(),
                OrganizationUnitID = file.OrganizationUnitID
            });
            args.AdditionalInfo = file.ID.ToString();

            EventUtil.RaiseEvent(args); //open task type HandleAuthenticationRequestFile

            return file;
            
        }
        private void RaiseEventNewDecisionBeforeAssociation(GetImportAuthenticationRequestResult request)
        {
            var args = new EventUtilArguments(EEventType.NewDecisionBeforeAssociation, new VirtualEntity
            {
                EntityType = EEntityType.ImportAuthenticationRequest,
                ID = (int) request.DocumentID,
                Title = request.DocumentID.ToString(),
            });
            args.AdditionalInfo = request.DocumentID.ToString();

            EventUtil.RaiseEvent(args); //close task type SetDecisionBeforeAssociation
        }

        private CertificateOfOriginsImportAuthenticationFileDetails CreateNewAuthenticationFile(GetImportAuthenticationRequestResult importAuthenticationRequest)
        {
            var importAuthenticationFileDetails = new CertificateOfOriginsImportAuthenticationFileDetails
            {
                State = 1,
                AuthenticationFileStatusID = (int)EAuthenticationFileStatus.WaitingForSendingLetter,
                RequestCountryID = importAuthenticationRequest.IssuingCountryIDNum ?? 0,
                UserID = base.UserUtil.Current.ID,
                PostalAdress = "gg",
                DeliveryMethodID = 1,
                EmailAdress = importAuthenticationRequest.ResponseNameEmail,
                ReminderMethodID = 1,
                UserNameIssuingLetter = "ss",
                CreateDate = DateTime.Now,
                UpdateDate = DateTime.Now,
                UpdateUserID = base.UserUtil.Current.ID,
                CreateUserID = base.UserUtil.Current.ID,
                CustomerID = importAuthenticationRequest.CustomerID ?? 1
            };
            
            return importAuthenticationFileDetails;
        }

        public CertificateOfOriginsImportAuthenticationFileDetails GetAuthenticationRequestFileByID(int fileId)
        {
            var spFilter = new ResultSetFilter<CertificateOfOriginsImportAuthenticationFileDetails>
                               {
                                   StoredProcedureName = CertificateOfOriginsConsts.GetFileAndRequestProcedure,
                                   MaterializeFunction = MaterializeForGetFile,
                                   Parameters =
                                       {
                                           new SqlParameter
                                               {
                                                   ParameterName = "@FileID",
                                                   SqlDbType = SqlDbType.Int,
                                                   Value = fileId
                                               }
                                       }
                               };
            var importAuthenticationFile = _uow.Repository.ExecuteResultSetFunction(spFilter);

            if (importAuthenticationFile != null)
                GetAdditionalInfoForFile(importAuthenticationFile);

            return importAuthenticationFile;
        }

        private void GetAdditionalInfoForFile(CertificateOfOriginsImportAuthenticationFileDetails file)
        {
            var decisions = _uow.Repository.GetQuery<CertificateOfOriginsDecision>().ToList();
            var fileStatuses = _uow.Repository.GetQuery<CertificateOfOriginsAuthenticationFileStatus>().ToList();

            file.FileStatuses = fileStatuses;

            foreach (var request in file.CertificateOfOriginsImportAuthenticationRequest)
            {
                request.Decisions = decisions;

                var collateralList = GetCollateralsForAuthenticationRequest(request);
                if (!collateralList.IsNullOrEmpty())
                {
                    request.Collaterals = new ObservableCollection<CollateralRequestDTO>();
                    collateralList.ForEach(c => request.Collaterals.Add(c));
                }
            }

            var tasks = GetTaskDetailsForFileByTaskType(file, new List<int> { (int)ETaskType.ReminderNotice6Months, (int)ETaskType.ReminderNotice10Months, (int)ETaskType.HandleAuthenticationRequestFile, (int)ETaskType.SendReminderForImporter });
            file.IsCurrentUserHandleFile = tasks.Any(t => t.UserID == UserUtil.Current.ID);


        }

        private List<IsTaskExistResultDTO> GetTaskDetailsForFileByTaskType(CertificateOfOriginsImportAuthenticationFileDetails file, List<int> taskTypeIds)
        {
            var tasksAdapter = Container.Resolve<ITasksServiceAdapter>();

            var filter = new IsTaskExistFilter
            {
                EntityID = file.ID,
                EntityTypeID = (int)EEntityType.AuthenticationRequestFile,
                TaskTypeIDs = taskTypeIds,
            };

            var result = tasksAdapter.IsTaskExist(filter);
            return result;
        }

        public CertificateOfOriginsImportAuthenticationFileDetails SaveAuthenticationRequestFile(CertificateOfOriginsImportAuthenticationFileDetails authenticationRequestFile)
        {
            UpdateAndSaveImportAuthenticationRequest(authenticationRequestFile);

            ManageImportAuthenticationRequestStatus(authenticationRequestFile);
            ManageImportAuthenticationFileStatus(authenticationRequestFile);

            _uow.Repository.Save(authenticationRequestFile);
            _uow.CommitAllChanges();

            return GetAuthenticationRequestFileByID(authenticationRequestFile.ID);

        }
    
        public void UpdateFileAfterDelivery(CertificateOfOriginsImportAuthenticationFileDetails file)
        {
            if (file.AuthenticationFileStatusID == (int)EAuthenticationFileStatus.WaitingForSendingLetter)
            {
                file.AuthenticationFileStatusID = (int)EAuthenticationFileStatus.AuthenticationRequestWasSend;
                file.DeliveryMethodID = (int)EDeliveryMethod.PostedMailing;
                //file.LastDelivery = DateTime.Today;

                //האם צריך ארוע בנוסף לארוע שדיוור מעלה?
            }
            else if (file.AuthenticationFileStatusID == (int)EAuthenticationFileStatus.AuthenticationRequestWasSend)
            {
                if (file.DeliveryMethodID == (int)EDeliveryMethod.PostedMailing || file.DeliveryMethodID == (int)EDeliveryMethod.SentByEmailRequest)
                {
                    file.DeliveryMethodID = (int)EDeliveryMethod.FirstRemindSent;
                }
                else if (file.DeliveryMethodID == (int)EDeliveryMethod.FirstRemindSent)
                {
                    file.DeliveryMethodID = (int)EDeliveryMethod.SecondRemindSent;
                }
                //האם צריך ארוע בנוסף לארוע שדיוור מעלה?
            }
            else if (file.AuthenticationFileStatusID == (int)EAuthenticationFileStatus.AuthenticationRequestReminderWasSend)
            {
                if (file.DeliveryMethodID == (int)EDeliveryMethod.FirstRemindSent)
                {
                    file.DeliveryMethodID = (int)EDeliveryMethod.SecondRemindSent;
                }
            }

            file.LastDelivery = DateTime.Today;
            file.UpdateDate = DateTime.Today;

            foreach (var authenticationRequest in file.CertificateOfOriginsImportAuthenticationRequest)
            {
                authenticationRequest.UpdateDate = DateTime.Now;
            }

            _uow.Repository.Save(file);
            _uow.CommitAllChanges();
        }

        public int RaiseSchedulerEvents(
            List<AuthenticationRequestsForSchedulerDTO> authenticationRequestsForSchedulerDTO)
        {
            var tasksAdapter = Container.Resolve<ITasksServiceAdapter>();

            var count = 0;

            foreach (var dto in authenticationRequestsForSchedulerDTO)
            {
                switch (dto.DeliveryMethodID)
                {
                    case (int)EDeliveryMethod.PostedMailing:
                    case (int)EDeliveryMethod.SentByEmailRequest:
                        {
                            if (dto.IsImport && dto.SendThreeMonthsReminder)
                            {
                                count += OpenTask(dto, EEventType.ReminderNotice3Months, ETaskType.VendorReminderNotice3Months,tasksAdapter);
                            }
                            else
                            {
                                count += OpenTask(dto, 
                                                  dto.IsImport?
                                                      dto.IsVendor? EEventType.ImporterReminderNotice6Months : EEventType.ReminderNotice6Months
                                                  :EEventType.ExportReminderNotice6Months,
                                                  dto.IsImport? 
                                                      dto.IsVendor?ETaskType.SendImporterMsgFromVendorReference: ETaskType.ReminderNotice6Months
                                                  : ETaskType.ExportReminderNotice6Months,
                                                  tasksAdapter);
                            }

                            break;
                        }
                    case (int)EDeliveryMethod.FirstRemindSent:
                        count += OpenTask(dto, 
                                         dto.IsImport ?
                                             dto.IsVendor?EEventType.FinalDecisionInTheFile : EEventType.ReminderNotice10Months
                                         :EEventType.ExportReminderNotice10Months, 
                                         dto.IsImport ?
                                             dto.IsVendor? ETaskType.FinalDecisionInCase : ETaskType.ReminderNotice10Months
                                         : ETaskType.ExportReminderNotice10Months, 
                                         tasksAdapter);
                        break;
                }
            }
            return count;
        }

        int OpenTask(AuthenticationRequestsForSchedulerDTO dto, EEventType eEventType, ETaskType eTaskType,ITasksServiceAdapter tasksAdapter)
        {
            EEntityType entityType = dto.IsImport
                ? EEntityType.AuthenticationRequestFile
                : EEntityType.ExportDocumentAuthenticationRequest;

            var filter = new IsTaskExistFilter
            {
                EntityID = dto.ID,
                EntityTypeID = (int) entityType,
                TaskTypeIDs = new List<int>
                {
                    (int) eTaskType
                },
            };

            var result = tasksAdapter.IsTaskExist(filter);
            if (!result.IsNullOrEmpty())
                return 0;
            

            var args = new EventUtilArguments(eEventType, new VirtualEntity
            {
                EntityType = entityType,
                ID = dto.ID,
                Title = dto.ID.ToString(),
                OrganizationUnitID = dto.OrganizationUnitID
            })
            {
                TaskArguments = new EventTaskArguments
                {
                    OpenIdenticalTaskExistsBehaviour = EOpenIdenticalTaskExistsBehaviour.CloseOld
                }
            };
            EventUtil.RaiseEvent(args);
            return 1;
        }
        #region private

        private void UpdateFileId(List<GetImportAuthenticationRequestResult> importAuthenticationRequests, int fileID)
        {
            IEnumerable<int?> idsList = importAuthenticationRequests.Select(i => i.DocumentID);
            var dataTable = GetRequestIDsSqlParameters(idsList);
            var sqlParameters = new List<SqlParameter>
                                    {
                                        new SqlParameter
                                            {
                                                ParameterName = "@ImportAuthenticationRequestsID",
                                                SqlDbType = SqlDbType.Structured,
                                                Value = dataTable,
                                                TypeName = CertificateOfOriginsConsts.DbType
                                            },
                                        new SqlParameter
                                            {
                                                ParameterName = "@AuthenticationFileID",
                                                SqlDbType = SqlDbType.Int,
                                                Value = fileID,
                                            },
                                    };
            _uow.Repository.ExecuteFunction(CertificateOfOriginsConsts.UpdateRequestsProcedure, sqlParameters);
        }

        private static object GetRequestIDsSqlParameters(IEnumerable<int?> importAuthenticationRequestIDs)
        {
            if (importAuthenticationRequestIDs.IsNullOrEmpty()) return null;

            #region Data Table definition and initialization

            var dataTable = new DataTable(CertificateOfOriginsConsts.DbType);
            dataTable.Columns.Add(new DataColumn(CertificateOfOriginsConsts.RowName, typeof (int)));

            foreach (var importAuthenticationRequestID in importAuthenticationRequestIDs)
            {
                var row = dataTable.NewRow();
                row[CertificateOfOriginsConsts.RowName] = GetParameterValue(importAuthenticationRequestID);
                dataTable.Rows.Add(row);
            }

            #endregion Data Table definition and initialization

            return dataTable;
        }

        private static object GetParameterValue(object value)
        {
            return value ?? DBNull.Value;
        }

        private CertificateOfOriginsImportAuthenticationFileDetails MaterializeForGetFile(DbDataReader reader)
        {
            var importAuthenticationFile = reader.Materialize<CertificateOfOriginsImportAuthenticationFileDetails>().FirstOrDefault();
            if (importAuthenticationFile == null) return null;
                        
            importAuthenticationFile.CustomerIDList = new List<int>();
            var vendorIDList = new List<int>();
            reader.NextResult();

            var importAuthenticationRequest =  reader.Materialize<CertificateOfOriginsImportAuthenticationRequest>().ToList();
            importAuthenticationRequest.ForEach(ia => ia.AcceptChanges());
            importAuthenticationFile.CertificateOfOriginsImportAuthenticationRequest.AddRange(importAuthenticationRequest);
            importAuthenticationFile.EntityTypeAndIDsToSearch = new Dictionary<EEntityType, List<int>>();
            importAuthenticationFile.EntityTypeAndIDsToSearch.Add(EEntityType.ImportDeclaration,importAuthenticationFile.CertificateOfOriginsImportAuthenticationRequest.Select(x=>x.LeadDocumentID).ToList());
            reader.NextResult();

            var importAuthenticationRequestDocument = reader.Materialize<Document>().ToList();
            importAuthenticationRequestDocument.ForEach(ia => ia.AcceptChanges());
            foreach (var request in importAuthenticationRequest)
            {
                request.Document = importAuthenticationRequestDocument.FirstOrDefault(x => x.ID == request.DocumentID);
                if (request.VendorId.HasValue)
                    vendorIDList.Add(request.VendorId.Value);
            }

            reader.NextResult();
            var importAuthenticationRequestItemDetails = reader.Materialize<CertificateOfOriginsItemDetails>().ToList();
            importAuthenticationRequestItemDetails.ForEach(ia => ia.AcceptChanges());
            foreach (var request in importAuthenticationRequest)
            {
                request.CertificateOfOriginsItemDetails.AddRange(importAuthenticationRequestItemDetails.Where(i => i.ImportAuthenticationRequestID == request.DocumentID));
                request.EntityTypeAndIDsToSearch = new Dictionary<EEntityType, List<int>>();
                request.EntityTypeAndIDsToSearch.Add(EEntityType.ImportDeclaration, new List<int> { request.LeadDocumentID });
            }

            if (importAuthenticationFile.CustomerID == 0)
                importAuthenticationFile.CustomerID = -1;

            importAuthenticationFile.AcceptChanges();
            return importAuthenticationFile;
        }

        private void UpdateAndSaveImportAuthenticationRequest(CertificateOfOriginsImportAuthenticationFileDetails authenticationRequestFile)
        {
            foreach (var request in authenticationRequestFile.CertificateOfOriginsImportAuthenticationRequest)
            {
                request.IsOldIndication = request.DocumentIssuingDate <= DateTime.Now.AddYears(-3);
                _uow.Repository.Save(request);
            }
        }

        private void CheckStatus(CertificateOfOriginsImportAuthenticationFileDetails authenticationRequestFile)
        {
            var collateralAdapter = Container.Resolve<ICollateralServiceAdapter>();
            var collateralList = GetCollateralListForFile(authenticationRequestFile);

            switch (authenticationRequestFile.AuthenticationFileStatusID)
            {
                case (int) EAuthenticationFileStatus.ReceivedPartialAnswerInFile:
                case (int) EAuthenticationFileStatus.ReceivedAnswerInFile:
                    break;
                case (int) EAuthenticationFileStatus.RightAuthenticationAnswer:
                    if (!collateralList.IsNullOrEmpty())
                    {
                        collateralAdapter.GrantAllCollateralRequests(new List<GrantCollateralRequestDTO>
                        {
                            new GrantCollateralRequestDTO
                            {
                                EntityID = authenticationRequestFile.ID,
                                EntityTypeID = (int)EEntityType.AuthenticationRequestFile
                            }
                        });
                    }

                    break;
                case (int) EAuthenticationFileStatus.ClarificationRequired:
                    //והפקת מכתב הבהרה לבית המכס/יצואן שיישמר בטאב 

                    break;
                case (int) EAuthenticationFileStatus.WrongAuthenticationAnswer:
                    //והפקת משימה (TBD) על גביית מיסים נדרשת בעקבות הפסילה לממונה תפ"ג.

                    if (!collateralList.IsNullOrEmpty())
                    {
                        foreach (var collateral in collateralList)
                        {
                            collateralAdapter.DebitCreditCollateralRequest(new DebitCreditFilter
                            {
                                CollateralRequestID = collateral
                            });
                        }
                    }
                                        
                    break;
            }

            if (authenticationRequestFile.OriginalAuthenticationFileStatusID == authenticationRequestFile.AuthenticationFileStatusID) return;
            
            RaiseEventForFile(authenticationRequestFile);
            RaiseStatusMessage(authenticationRequestFile);

        }

        private void ManageImportAuthenticationRequestStatus(CertificateOfOriginsImportAuthenticationFileDetails authenticationRequestFile)
        {
            var collateralAdapter = Container.Resolve<ICollateralServiceAdapter>();
            foreach (var request in authenticationRequestFile.CertificateOfOriginsImportAuthenticationRequest)
            {
                if (request.DecisionID == request.OriginalRequestDecisionID) continue;

                var collateralList = GetCollateralListForAuthenticationRequest(request);

                if (request.DecisionID != (int)EAuthenticationRequestDecision.DemandAnotherClarification)
                {
                    var args = new EventUtilArguments(EEventType.CloseAllTaskForImportAuthenticationRequest, new VirtualEntity
                    {
                        EntityType = EEntityType.ImportAuthenticationRequest,
                        ID = request.DocumentID,
                        Title = request.DocumentID.ToString(),
                        OrganizationUnitID = request.OrganizationUnitID
                    });
                    if (request.AuthenticationFileID != null)
                        args.RelatedEntities = new List<IEntity> { new VirtualEntity { ID = request.AuthenticationFileID ?? 0, EntityType = EEntityType.AuthenticationRequestFile } };

                    EventUtil.RaiseEvent(args);
                    // close all open tasks on request {404} (event not shown at log)
                }

                SendDecisionMessage(request);
                if(request.DecisionID == (int) EAuthenticationRequestDecision.Approval && !collateralList.IsNullOrEmpty())
                { 
                    collateralAdapter.GrantAllCollateralRequests(new List<GrantCollateralRequestDTO>
                    {
                        new GrantCollateralRequestDTO
                        {
                            EntityID = authenticationRequestFile.ID,
                            EntityTypeID = (int)EEntityType.ImportAuthenticationRequest
                        }
                    });
                }
               

                RaiseEventForAuthenticationRequest(request);
            }


        }


        private void ManageImportAuthenticationFileStatus(CertificateOfOriginsImportAuthenticationFileDetails authenticationRequestFile)
        {
            //if (authenticationRequestFile.AuthenticationFileStatusID == authenticationRequestFile.OriginalAuthenticationFileStatusID)
            //    return;
            var isAuthenticationFileChanged = authenticationRequestFile.AuthenticationFileStatusID !=
                                              authenticationRequestFile.OriginalAuthenticationFileStatusID;
            if (isAuthenticationFileChanged)
            {
                CheckStatusAndOpenTask(authenticationRequestFile);
                
                if (authenticationRequestFile.AuthenticationFileStatusID !=
                    (int) EAuthenticationFileStatus.ClarificationRequired)
                {
                    var args = new EventUtilArguments(EEventType.CloseAllTaskForImportAuthenticationRequestFile,
                        new VirtualEntity
                        {
                            EntityType = EEntityType.AuthenticationRequestFile,
                            ID = authenticationRequestFile.ID,
                            Title = authenticationRequestFile.ID.ToString(),
                            OrganizationUnitID = authenticationRequestFile.OrganizationUnitID
                        });
                    EventUtil.RaiseEvent(args);
                } // close all open tasks on file {339,340,408} (event not shown at log)

                foreach (var request in authenticationRequestFile.CertificateOfOriginsImportAuthenticationRequest.Where(r => r.DecisionID == (int)EAuthenticationRequestDecision.Rejection).ToList())
                {
                    var args = new EventUtilArguments(EEventType.AuthenticationRequestRejected, new VirtualEntity
                    {
                        EntityType = EEntityType.ImportAuthenticationRequest,
                        ID = request.DocumentID,
                        Title = request.DocumentID.ToString(),
                        OrganizationUnitID = request.OrganizationUnitID
                    });
                    args.RelatedEntities = new List<IEntity> { new VirtualEntity { ID = authenticationRequestFile.ID, EntityType = EEntityType.AuthenticationRequestFile } };
                    args.TaskArguments = new EventTaskArguments { OpenIdenticalTaskExistsBehaviour = EOpenIdenticalTaskExistsBehaviour.CloseOld };

                    EventUtil.RaiseEvent(args); // open task HandleRejectedAuthenticationRequest
                }
            }

            if (authenticationRequestFile.OriginalAuthenticationFileStatusID == authenticationRequestFile.AuthenticationFileStatusID) return;

            RaiseEventForFile(authenticationRequestFile);
            RaiseStatusMessage(authenticationRequestFile);

        }

        private void CheckStatusAndOpenTask(CertificateOfOriginsImportAuthenticationFileDetails file)
        {
            var AuthenticationFileStatusList = new List<int>
            {
                (int) EAuthenticationFileStatus.ReceivedAnswerInFile,
                (int) EAuthenticationFileStatus.RightAuthenticationAnswer,
                (int) EAuthenticationFileStatus.ClarificationRequired,
                (int) EAuthenticationFileStatus.WrongAuthenticationAnswer
            };

            if (AuthenticationFileStatusList.Contains(file.AuthenticationFileStatusID) ||
                file.CertificateOfOriginsImportAuthenticationRequest.Any(r=>r.DecisionID == (int)EAuthenticationRequestDecision.Partly))
            {
                //HandleImportAuthenticationRequest
                var args = new EventUtilArguments(EEventType.HandleImportAuthenticationRequest, new VirtualEntity(file))
                {
                    TaskArguments = new EventTaskArguments
                    {
                        OpenIdenticalTaskExistsBehaviour = EOpenIdenticalTaskExistsBehaviour.CloseOld
                    }
                };
                EventUtil.RaiseEvent(args);
            }
            if (file.AuthenticationFileStatusID == (int)EAuthenticationFileStatus.CancelledFile)
            {
                for (int i = file.CertificateOfOriginsImportAuthenticationRequest.Count -1; i > -1; i--)
                {
                    var request = file.CertificateOfOriginsImportAuthenticationRequest[i];
                    request.AuthenticationFileID = null;
                    _uow.Repository.Save(request);
                }
                _uow.CommitAllChanges();
            }

            if (file.AuthenticationFileStatusID == (int) EAuthenticationFileStatus.AuthenticationRequestReminderWasSend && file.AuthenticationFileStatusID != file.AuthenticationFileStatusIDPrev)
            {
                EventUtil.RaiseEvent(new EventUtilArguments(EEventType.UpdateFileStatusVendorReminderNotice, file));
            }
            if (file.AuthenticationFileStatusID == (int)EAuthenticationFileStatus.ReceivedAnswerInFile && file.AuthenticationFileStatusID != file.AuthenticationFileStatusIDPrev)
            {
                EventUtil.RaiseEvent(new EventUtilArguments(EEventType.UpdateFileStatusFinalDecisionInCase, file));
            }
        }

        private void RaiseEventForFile(CertificateOfOriginsImportAuthenticationFileDetails authenticationRequestFile)
        {
            
                var args = new EventUtilArguments(EEventType.AuthenticationRequestFileStatusUpdate, new VirtualEntity
            {
                EntityType = EEntityType.AuthenticationRequestFile,
                ID = authenticationRequestFile.ID,
                Title = authenticationRequestFile.ID.ToString(),
            });
            args.AdditionalInfo = string.Format("עודכן הסטאטוס ל{0} על ידי {1} בתאריך {2} ",
                                                SystemTablesUtil
                                                    .GetCodeById<CertificateOfOriginsAuthenticationFileStatus>(
                                                        authenticationRequestFile.AuthenticationFileStatusID, true).Name,
                                                base.UserUtil.Current.DisplayName, DateTime.Today.ToShortDateString());
            EventUtil.RaiseEvent(args);
        }
        private void RaiseEventForAuthenticationRequest(CertificateOfOriginsImportAuthenticationRequest request)
        {
            var args = new EventUtilArguments(EEventType.AuthenticationRequestDecisionUpdate, new VirtualEntity
            {
                EntityType = EEntityType.ImportAuthenticationRequest,
                ID = request.DocumentID,
                Title = request.DocumentID.ToString(),
            });
            if (request.AuthenticationFileID != null)
                args.RelatedEntities = new List<IEntity> { new VirtualEntity { ID = request.AuthenticationFileID ?? 0, EntityType = EEntityType.AuthenticationRequestFile } };

            if(request.DecisionID != null)
                args.AdditionalInfo = string.Format("עודכן הסטאטוס ל{0} על ידי {1} בתאריך {2} ",
                                                SystemTablesUtil.GetCodeById<CertificateOfOriginsDecision>(request.DecisionID ?? 0, true).Name,
                                                base.UserUtil.Current.DisplayName,
                                                DateTime.Today.ToShortDateString());
            EventUtil.RaiseEvent(args);
        }

        private List<int> GetCollateralListForFile(CertificateOfOriginsImportAuthenticationFileDetails authenticationRequestFile)
        {
            var collateralAdapter = Container.Resolve<ICollateralServiceAdapter>();
            List<int> collateralList = new List<int>();

            foreach (var request in authenticationRequestFile.CertificateOfOriginsImportAuthenticationRequest)
            {
                var collateraRelatedEntityDTO = new RelatedEntityDTO
                    {
                        RelatedEntityID = request.DocumentID,
                        RelatedEntityType = EEntityType.ImportAuthenticationRequest
                    };
                var res = collateralAdapter.GetCollateralRequestIDsByRelatedEntityDTO(collateraRelatedEntityDTO);
                if(!res.IsNullOrEmpty())
                    collateralList.AddRange(res);
            }
            return collateralList;
        }

        private List<int> GetCollateralListForAuthenticationRequest(CertificateOfOriginsImportAuthenticationRequest request)
        {
            var collateralAdapter = Container.Resolve<ICollateralServiceAdapter>();

            var collateraRelatedEntityDTO = new RelatedEntityDTO
            {
                RelatedEntityID = request.DocumentID,
                RelatedEntityType = EEntityType.ImportAuthenticationRequest
            };
            var res = collateralAdapter.GetCollateralRequestIDsByRelatedEntityDTO(collateraRelatedEntityDTO);

            return res;
        }

        private List<CollateralRequestDTO> GetCollateralsForAuthenticationRequest(CertificateOfOriginsImportAuthenticationRequest request)
        {
            var collateralAdapter = Container.Resolve<ICollateralServiceAdapter>();

            var res = collateralAdapter.GetCollateralRequest(EEntityType.ImportAuthenticationRequest, request.DocumentID, null);

            return res;
        }

        private void RaiseStatusMessage(CertificateOfOriginsImportAuthenticationFileDetails file)
        {
            var listDestinations = new List<MessageDestinationDTO>();
            listDestinations.Add(new MessageDestinationDTO { UserID = file.UserID });

            var sendMessageDTO = new SendMessageDTO(file, EMessageTypes.ImportRequestDecision)
            {
                MessageParameters = new List<string>
                {
                    file.ID.ToString(),
                    SystemTablesUtil.GetCodeById<CertificateOfOriginsAuthenticationFileStatus>(file.AuthenticationFileStatusID, true).Name
                },
                MultipleMessageDestinations = listDestinations
            };

            var servicesAdapter = new ServicesAdapter();
            servicesAdapter.SendMessage(sendMessageDTO);
        }

        private void SendDecisionMessage(CertificateOfOriginsImportAuthenticationRequest request)
        {
            // on ImportAuthenticationRequest creation the UserID == UserResponseID
            var listDestinations = new List<MessageDestinationDTO>();

            var userIDs = new List<int> { request.UserResponseID };

            if (request.UserID != request.UserResponseID)
                userIDs.Add(request.UserID);

            userIDs.ForEach(user => listDestinations.Add(new MessageDestinationDTO { UserID = user }));

            SendMessageDTO sendMessageDTO = new SendMessageDTO(request, EMessageTypes.ImportRequestCentralDecision);

            switch (request.DecisionID)
            {
                case (int)EAuthenticationRequestDecision.AuthenticationRequried:
                case (int)EAuthenticationRequestDecision.AuthenticationNeedless:
                case (int)EAuthenticationRequestDecision.Approval:
                case (int)EAuthenticationRequestDecision.Partly:
                case (int)EAuthenticationRequestDecision.DemandAnotherClarification:
                    sendMessageDTO.MessageParameters = new List<string>
                    {
                        SystemTablesUtil.GetCodeById<CertificateOfOriginsDecision>((int)request.DecisionID, true).Name,
                        request.DocumentID.ToString()

                    };
                    
                    break;
                case (int)EAuthenticationRequestDecision.Rejection:
                    sendMessageDTO.MessageTypeID = EMessageTypes.ImportRequestRejection;
                    sendMessageDTO.MessageParameters = new List<string>
                    {
                        request.DocumentID.ToString(),
                        request.AuthenticationFileID.ToString()
                    };
                    break;
            }

            
            if (listDestinations.Count() > 1)
            {
                sendMessageDTO.IsGroupMessage = true;
                sendMessageDTO.MultipleMessageDestinations = listDestinations;
            }
            else
            {
                sendMessageDTO.UserIDToSendMessage = request.UserResponseID;
            }
            
            var servicesAdapter = new ServicesAdapter();
            servicesAdapter.SendMessage(sendMessageDTO);
        }

   #endregion private

        #endregion File

        public bool CloseReminderTask(CertificateOfOriginsImportAuthenticationFileDetails file)
        {

            var args = new EventUtilArguments(EEventType.CloseTaskReminderNotice3Months, new VirtualEntity(file));
            
                args.RelatedEntities = new List<IEntity> { new VirtualEntity { ID = file.ID, EntityType = EEntityType.AuthenticationRequestFile } };

            EventUtil.RaiseEvent(args);
            return true;
        }

        public CertificateOfOriginsImportAuthenticationFileDetails ChangeStatusAfterDeliverySent(CertificateOfOriginsImportAuthenticationFileDetails authenticationRequestFile)
        {
            var args = new EventUtilArguments(EEventType.CloseAllTaskForImportAuthenticationRequestFile,
                new VirtualEntity
                {
                    EntityType = EEntityType.AuthenticationRequestFile,
                    ID = authenticationRequestFile.ID,
                    Title = authenticationRequestFile.ID.ToString(),
                    OrganizationUnitID = authenticationRequestFile.OrganizationUnitID
                });
            EventUtil.RaiseEvent(args);
            return authenticationRequestFile;
        }

        public int? CheckImporterOfImportAuthentication(int importerId)
        {
            return _uow.Repository.GetQuery<VerificationProhibitedImporters>().FirstOrDefault(c => c.CustomerId == importerId)?.ID == null? importerId: (int?) null;

        }
    }
}
