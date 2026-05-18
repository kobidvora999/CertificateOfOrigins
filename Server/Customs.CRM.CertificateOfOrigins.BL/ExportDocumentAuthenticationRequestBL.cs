using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Customs.CRM.CertificateOfOrigins.BL.ServiceAdapters;
using Customs.CRM.CertificateOfOrigins.InternalCommon;
using Customs.CRM.CertificateOfOrigins.InternalCommon.Common;
using Customs.CRM.Entities;
using Customs.CRM.Entities.Entities.ExportDocumentAuthenticationRequestSearch;
using Customs.CertificateOfOrigins.Entities;
using Customs.Inf.CommonService.ExternalCommon;
using Customs.Infrastructure.DocumentManagement.ExternalCommon;
using Customs.StockPileData.Customers.ExternalCommon.Common;
using MalamTeam.Infrastructure.GeneralServices.BaseLayer;
using MalamTeam.Infrastructure.GeneralServices.CommonBase;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.Base_Entities;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.Exceptions;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.Extensions;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.Interfaces;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.ORM;
using MalamTeam.Infrastructure.GeneralServices.Environment.Enums;
using Microsoft.Data.Extensions;
using Microsoft.Practices.Unity;


namespace Customs.CRM.CertificateOfOrigins.BL
{
    public class ExportDocumentAuthenticationRequestBL : BaseBL
    {
        public ExportDocumentAuthenticationRequestBL(IUnitOfWork uow) : base(uow)
        {
            Container.RegisterType<IDocumentServiceAdapter, DocumentServiceAdapter>();
        }

        public List<ExportDocumentAuthenticationRequestSearchResult> GetExportDocumentAuthenticationRequestSearch(ExportDocumentAuthenticationRequestSearchFilter filter)
        {
            var resultSetFilter = new ResultSetFilter<List<ExportDocumentAuthenticationRequestSearchResult>>
            {
                StoredProcedureName =
                                              CertificateOfOriginsConsts
                                              .ExportDocumentAuthenticationRequestSearchProcName,
                MaterializeFunction =
                                              MaterializeExportDocumentAuthenticationRequestSearchResult,
            };
            resultSetFilter.Parameters.AddRange(filter.GetSQLParams());
            var result = _uow.Repository.ExecuteResultSetFunction(resultSetFilter);
            return result;
        }

        private List<ExportDocumentAuthenticationRequestSearchResult> MaterializeExportDocumentAuthenticationRequestSearchResult(DbDataReader reader)
        {
            return reader.Materialize<ExportDocumentAuthenticationRequestSearchResult>().ToList();
        }

        public ExportDocumentAuthenticationRequest SaveExportDocumentAuthenticationRequest(ExportDocumentAuthenticationRequest entity)
        {
            if (entity.IsNewInstance())
            {
                _uow.Repository.Save(entity);
                _uow.CommitAllChanges();
            }
            if (entity.StatusID != entity.OriginalStatusID)
            {
                CheckStatus(entity);
            }
            _uow.Repository.Save(entity);
            _uow.CommitAllChanges();
            entity.OriginalStatusID = entity.StatusID.Value;

            if (entity.ListOfAdditionalDocumentsIDs != null && entity.ListOfAdditionalDocumentsIDs.Count > 0)
            {
                var docsAdapter = Container.Resolve<IDocumentServiceAdapter>();
                docsAdapter.AttachDocumentsToEntity(new List<DocumentsToEntityDTO>{new DocumentsToEntityDTO
                                                                                       {
                                                                                           DocumentIds = entity.ListOfAdditionalDocumentsIDs,
                                                                                           Entity = new VirtualEntity(entity)
                                                                                       }});
            }

            return entity;
        }

        public ExportDocumentAuthenticationRequest GetExportDocumentAuthenticationRequestByID(int id)
        {
            var result = _uow.Repository.GetQuery<ExportDocumentAuthenticationRequest>().Single(edar => edar.ID == id);
            result.OriginalStatusID = result.StatusID.HasValue ? result.StatusID.Value : 0;
            ArgumentValidator.AssertNotNull(result, "ExportDocumentAuthenticationRequest not found by id: " + id);
            _uow.Repository.LoadProperty(result, ExportDocumentAuthenticationRequest.NavPropCustomsItemToExportDocumentAuthenticationRequest);
            _uow.Repository.LoadProperty(result, ExportDocumentAuthenticationRequest.NavPropExportDocumentAuthenticationRequestLeadDocument);
            _uow.Repository.LoadProperty(result, ExportDocumentAuthenticationRequest.NavPropExportAuthenticationRequestManufacturingArea);

            result.EntityTypeAndIDsToSearch = new Dictionary<EEntityType, List<int>>();
            result.EntityTypeAndIDsToSearch.Add(EEntityType.ExportDeclaration, result.ExportDocumentAuthenticationRequestLeadDocument.Where(x => x.LeadDocumentID.HasValue).Select(x => x.LeadDocumentID.Value).ToList());
            return result;
        }

        #region private

        private void CheckStatus(ExportDocumentAuthenticationRequest authenticationRequestFile)
        {

            switch (authenticationRequestFile.StatusID)
            {
                //פתיחת משימה:NewExportAuthenticationRequest - במעבר לסטטוס- מוכן לטיפול מקצועי 
                case (int)EExportAuthenticationRequestStatus.ReadyForProfessionalTreatment:
                    RaiseEvent(authenticationRequestFile, EEventType.ExportNewAuthenticationRequest, authenticationRequestFile.ID.ToString());
                    RaiseStatusMessage(authenticationRequestFile);
                    break;
                //פתיחת משימה:HandleExportAuthenticationRequestAfterClosing- במעבר לסטטוס סגור
                case (int)EExportAuthenticationRequestStatus.ClosedValid:
                case (int)EExportAuthenticationRequestStatus.ClosedNotValid:
                case (int)EExportAuthenticationRequestStatus.ClosedSemiValid:
                    RaiseEvent(authenticationRequestFile, EEventType.ExportAuthenticationRequestAfterClosing, null);
                    break;
                case (int)EExportAuthenticationRequestStatus.Cancelled:
                case (int)EExportAuthenticationRequestStatus.WaitingForExporter:
                    RaiseEvent(authenticationRequestFile, EEventType.ChangeFileStatus, null);
                    break;
                default:
                    RaiseEvent(authenticationRequestFile, null, null);
                    RaiseStatusMessage(authenticationRequestFile);
                    break;
            }


        }

        private void RaiseEvent(ExportDocumentAuthenticationRequest authenticationRequestFile, EEventType? eventType, string additionalInfo)
        {
            var args = new EventUtilArguments(EEventType.ExportAuthenticationRequestFileStatusUpdate, new VirtualEntity
            {
                EntityType = EEntityType.ExportDocumentAuthenticationRequest,
                ID = authenticationRequestFile.ID,
                Title = authenticationRequestFile.ID.ToString(),
            });
            args.AdditionalInfo = string.Format("עודכן הסטאטוס ל{0} על ידי {1} בתאריך {2} ", SystemTablesUtil.GetCodeById<ExportAuthenticationRequestStatus>(authenticationRequestFile.StatusID.GetValueOrDefault(), true).Name, base.UserUtil.Current.DisplayName, DateTime.Today.ToShortDateString());
            EventUtil.RaiseEvent(args);

            if (eventType.HasValue)
            {
                args = new EventUtilArguments(eventType.Value, new VirtualEntity
                {
                    EntityType = EEntityType.ExportDocumentAuthenticationRequest,
                    ID = authenticationRequestFile.ID,
                    Title = authenticationRequestFile.ID.ToString(),
                });
                args.AdditionalInfo = additionalInfo;
                EventUtil.RaiseEvent(args);
            }
        }

        private void RaiseStatusMessage(ExportDocumentAuthenticationRequest file)
        {
            var sendMessageDTO = new SendMessageDTO(file, EMessageTypes.ImportRequestDecision)
            {
                MessageParameters = new List<string>
                             {
                                 file.ID.ToString(),
                                 SystemTablesUtil.GetCodeById<ExportAuthenticationRequestStatus>( file.StatusID.GetValueOrDefault(), true).Name
                             },

                MultipleMessageDestinations = new List<MessageDestinationDTO> { new MessageDestinationDTO { UserID = UserUtil.Current.ID } }
            };

            var servicesAdapter = new ServicesAdapter();
            servicesAdapter.SendMessage(sendMessageDTO);

        }

        #endregion private

        public CustomerDTO GetCustomerInformation(int customerId)
        {
            var customerAdapter = Container.Resolve<ICustomerServiceAdapter>();
            var customerIdentificationFilter = new CustomerIdentificationFilter { ExternalId = null, CustomerId = customerId };

            var customerDto = customerAdapter.GetCustomerDTOByCustomerIdentification(customerIdentificationFilter);
            if (customerDto == null)
            {
                throw new InfException(EMessages.InvalidIdentificationNumber);
            }
            return customerDto;
        }

        public CustomerDTO GetCustomerInformationByCountry(int countryId)
        {
            var customerAdapter = Container.Resolve<ICustomerServiceAdapter>();
            var customers =
                customerAdapter.GetCustomeDTOByCountryID(countryId, (int)ECustomerActivityType.Foreign_customs_house);
            if (customers.IsNullOrEmpty())
            {
                throw new InfException(EMessages.NoCustomHouseForThisCountry);
            }

            return customers.FirstOrDefault();
        }
    }
}
