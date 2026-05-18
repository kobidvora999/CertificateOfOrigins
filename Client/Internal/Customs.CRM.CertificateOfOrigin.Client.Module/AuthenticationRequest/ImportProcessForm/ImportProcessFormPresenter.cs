using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Customs.CRM.CertificateOfOrigins.Client.Api;
using Customs.CRM.CertificateOfOrigins.Client.Api.AuthenticationRequest.ImportProcessForm;
using Customs.CRM.CertificateOfOrigins.ExternalCommon.Common;
using Customs.CRM.CertificateOfOrigins.InternalCommon;
using Customs.CRM.CertificateOfOrigins.InternalProxy;
using Customs.CRM.CertificateOfOrigion.Client.External.Api;
using Customs.CRM.Entities.CertificateOfOriginsPartial;
using Customs.CertificateOfOrigins.Entities;
using Customs.CRM.CertificateOfOrigins.InternalCommon.Common;
using Customs.FinanceInfr.Collateral.ExternalCommon.Common;
using Customs.Inf.MMI.Common.Aspects;
using Customs.Inf.MMI.Common.CAL;
using Customs.Inf.MMI.Common.Common;
using Customs.Inf.MMI.Common.Extensions;
using Customs.Inf.MMI.Common.Navigation;
using Customs.Inf.MMI.Common.Toolbox.CustomMessageBox;
using Customs.Inf.MMI.Common.Toolbox.CustomWindowsManager;
using Customs.Inf.MMI.Common.Toolbox.CustomsWindowBases;
using Customs.Inf.MMI.Services.Api.Search;
using Customs.Inf.MMI.Services.Module.ClientManager;
using Customs.Inf.MMI.Services.Module.Search;
using Customs.Infrastructure.DocumentManagement.ExternalCommon;
using Customs.Infrastructure.UserManagement.ExternalProxy;
using Customs.Shared.Entities;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.Base_Entities;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.Communication;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.Extensions;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.Interfaces;
using MalamTeam.Infrastructure.GeneralServices.Environment.Enums;
using Customs.Infrastructure.Entities;
using Customs.KnowledgeStore.CustomsBook.ExternalCommon.Common;
using Customs.Infrastructure.SystemTables.ExternalCommon;
using Customs.CRM.External;
using Customs.CRM.Entities;

namespace Customs.CRM.CertificateOfOrigins.Client.Module.AuthenticationRequest.ImportProcessForm
{
    public class ImportProcessFormPresenter : SearchConsumerBase, IImportProcessFormPresenter, IImportProcessFormExternalPresenter
    {
        private bool _isCloseAfterSave;
        private CertificateOfOriginsImportAuthenticationRequest newImportProcessRequest;
        private CustomWindowBase popupWindow;
        /// <summary>
        /// Gets the internal import process form viewmodel.
        /// </summary>
        /// <value>
        /// The internal import process form viewmodel.
        /// </value>
        internal ImportProcessFormViewmodel InternalImportProcessFormViewmodel
        {
            get
            {
                IViewModel viewModel;
                ViewModels.TryGetValue("ImportProcessFormViewmodel", out viewModel);
                return viewModel as ImportProcessFormViewmodel;
            }
        }

        /// <summary>
        /// Gets the import process form viewmodel.
        /// </summary>
        /// <value>
        /// The import process form viewmodel.
        /// </value>
        public ImportProcessFormViewmodel ImportProcessFormViewmodel
        {
            get { return InternalImportProcessFormViewmodel; }
        }

        #region ViewModel

        protected override void InternalInitViewModels()
        {
            base.InternalInitViewModels();
            var viewModel = new ImportProcessFormViewmodel() { Name = "ImportProcessFormViewmodel" };
            ViewModels.Add("ImportProcessFormViewmodel", viewModel);

            #region Register Commands

            viewModel.Commands.RegisterCommand(OnGetCollateralRequestCommand, ModuleCommands.GetCollateralRequestCommand);
            viewModel.Commands.RegisterCommand(OnCancelImportProcessCommand, ModuleCommands.CancelImportProcessCommand);
            viewModel.Commands.RegisterCommand<CertificateOfOriginsImportAuthenticationRequest>(OnEditImportProcessCommand, ModuleCommands.EditImportProcessCommand);
            viewModel.Commands.RegisterCommand<bool>(OnSaveImportProcessCommand, ModuleCommands.SaveImportProcessCommand);
            viewModel.Commands.RegisterCommand<CustomsItemDTO>(OnSelectCustomItemCommand, ModuleCommands.SelectCustomItemCommand);
            viewModel.Commands.RegisterCommand(OnSelectDocumentChangeCommand, ModuleCommands.SelectDocumentChangeCommand);
            viewModel.Commands.RegisterCommand(OnSelectInvoiceChangeCommand, ModuleCommands.SelectInvoiceChangeCommand);
            viewModel.Commands.RegisterCommand<int>(OnIsExistsAdditionalRequestsForImporterOrVendorCommand, ModuleCommands.IsExistsAdditionalRequestsForImporterOrVendorCommand);
            viewModel.Commands.RegisterCommand(OnPreferenceDocumentTypeIDChangedCommand, ModuleCommands.PreferenceDocumentTypeIDChangedCommand);
            viewModel.Commands.RegisterCommand<int>(OnImporterIDChangedCommand, "ImporterIDChangedCommand");

            #endregion Register Commands
        }


        #endregion ViewModel
        #region Commands Operations
        private void OnImporterIDChangedCommand(int value)
        {
            ResolveInstance<ICertificateOfOriginsInternalProxy>().CheckImporterOfImportAuthentication(OnCheckImporterOfImportAuthenticationCompleted, value);

        }

        [CommandMethod(true, true)]
        private void OnPreferenceDocumentTypeIDChangedCommand()
        {
            var entity = ImportProcessFormViewmodel.CurrentEntity;
            if (entity.PreferenceDocumentTypeID != (int)ECertificateOfOriginsPrefernceDocumentType.AaccountStatment
                && entity.PreferenceDocumentTypeID != (int)ECertificateOfOriginsPrefernceDocumentType.InvoiceStatement)
            {
                ImportProcessFormViewmodel.CurrentEntity.DocumentIssuingDate = DateTime.Now;
                entity.InvoiceNumber = null;
            }
            else
            {
                if (ImportProcessFormViewmodel.InvoiceDetails != null)
                {
                    ImportProcessFormViewmodel.CurrentEntity.DocumentIssuingDate = ImportProcessFormViewmodel.InvoiceDetails.First().IssuingDate.HasValue ?
                          ImportProcessFormViewmodel.InvoiceDetails.First().IssuingDate.Value : DateTime.Now;
                    entity.DocumentNumber = null;
                    entity.InvoiceNumber = ImportProcessFormViewmodel.InvoiceDetails.First().Invoicenumber;
                }
            }
        }
        [CommandMethod]
        private void OnEditImportProcessCommand(CertificateOfOriginsImportAuthenticationRequest entity)
        {
            ImportProcessFormViewmodel.ViewState = ViewStates.Edit;
            ImportProcessFormViewmodel.IsEditButtonEnabled = false;
            AddVersionImportProcessRequest();
        }

        private void OnEditImportProcessCommandException(CertificateOfOriginsImportAuthenticationRequest entity,
                                                         Exception exc)
        {
        }

        [CommandMethod]
        private void OnCancelImportProcessCommand()
        {
            ResetVersion();
            ImportProcessFormViewmodel.ViewState = ViewStates.ReadOnly;
            ImportProcessFormViewmodel.IsEditButtonEnabled = true;
        }

        private void OnCancelImportProcessCommandException(Exception exc)
        {
        }

        [CommandMethod]
        private void OnSaveImportProcessCommand(bool isClose)
        {
            CheckForCollateral(ImportProcessFormViewmodel.CurrentEntity);

            _isCloseAfterSave = isClose;
            ImportProcessFormViewmodel.ProgressBarVisibility = Visibility.Visible;
            ImportProcessFormViewmodel.SaveDocumentsCommand?.Execute(null);
            ResolveInstance<ICertificateOfOriginsInternalProxy>().SaveImportAuthenticationRequest(OnSaveImportProcessCompleted, ImportProcessFormViewmodel.CurrentEntity);
        }

        private void CheckForCollateral(CertificateOfOriginsImportAuthenticationRequest currentEntity)
        {
            if (currentEntity.Collaterals.IsNullOrEmpty() || ImportProcessFormViewmodel.CurrentDecision == null) return;

            if (ImportProcessFormViewmodel.CurrentDecision.ID == (int)EAuthenticationRequestDecision.AuthenticationNeedless)
                MsgBox.ShowMessage(CertificateOfOriginsConsts.CannotSetDecisionAuthenticationNeedless);
        }

        private void OnSaveImportProcessCommandException(bool isClose, Exception exc)
        {
        }

        [CommandMethod]
        private void OnGetCollateralRequestCommand()
        {

        }

        private void OnGetCollateralRequestCommandException(Exception exc)
        {
        }

        #region OnSelectCustomItemCommand

        [CommandMethod]
        private void OnSelectCustomItemCommand(CustomsItemDTO customItem)
        {
            if (ImportProcessFormViewmodel.SelectedCertificateOfOriginsItemDetails != null)
            {
                ImportProcessFormViewmodel.SelectedCertificateOfOriginsItemDetails.GoodsDescription = customItem.GoodsDescription;
                ImportProcessFormViewmodel.SelectedCertificateOfOriginsItemDetails.MeasurementUnitId = customItem.MeasurementUnitID;
                ImportProcessFormViewmodel.IsMeasurementTypeReadOnly = customItem.MeasurementUnitID != null;

                if (ImportProcessFormViewmodel.SelectedCertificateOfOriginsItemDetails.MeasurementUnitId == null)
                {
                    ImportProcessFormViewmodel.SelectedCertificateOfOriginsItemDetails.IsMeasurementUnitIdSelctable = true;
                }
            }
        }

        private void OnSelectCustomItemCommandException(CustomsItemDTO customItem, Exception exc)
        {
        }

        #endregion

        private void OnSelectDocumentChangeCommand()
        {
            if (ImportProcessFormViewmodel.DocumentID > 0)
            {
                ImportProcessFormViewmodel.CurrentEntity.DocumentID = ImportProcessFormViewmodel.DocumentID;
                ImportProcessFormViewmodel.SelectedDocument = new DocumentDTO();
                ImportProcessFormViewmodel.SelectedDocument = ImportProcessFormViewmodel.AttachmentDocuments.FirstOrDefault(d => d.ID == ImportProcessFormViewmodel.DocumentID);
                if (ImportProcessFormViewmodel.SelectedDocument != null)
                {
                    ImportProcessFormViewmodel.CurrentEntity.DocumentNumber =
                        ImportProcessFormViewmodel.SelectedDocument.Title;
                    ImportProcessFormViewmodel.SelectedDocumentType = (EDocumentType)ImportProcessFormViewmodel.SelectedDocument.TypeID;
                    InitInvoiceBySelectedDocument(ImportProcessFormViewmodel.SelectedDocument);
                }
            }
        }

        private void OnSelectInvoiceChangeCommand()
        {
            if (ImportProcessFormViewmodel.CurrentEntity.InvoiceID > 0)
            {
                var invoice = ImportProcessFormViewmodel.InvoiceDetails.FirstOrDefault(i =>
                    i.ID == ImportProcessFormViewmodel.CurrentEntity.InvoiceID);
                if (invoice == null) return;
                if (invoice.VendorID.HasValue)
                    ImportProcessFormViewmodel.CurrentEntity.VendorId = invoice.VendorID.Value;
                ImportProcessFormViewmodel.CurrentEntity.InvoiceNumber = invoice.Invoicenumber;
                ImportProcessFormViewmodel.CurrentEntity.InvoiceGoodsItemTaxDifference = invoice.GoodsItemTaxDifferences;
                if (ImportProcessFormViewmodel.CurrentEntity.PreferenceDocumentTypeID == (int)ECertificateOfOriginsPrefernceDocumentType.AaccountStatment
               || ImportProcessFormViewmodel.CurrentEntity.PreferenceDocumentTypeID == (int)ECertificateOfOriginsPrefernceDocumentType.InvoiceStatement
               || ImportProcessFormViewmodel.CurrentEntity.PreferenceDocumentTypeID == 0)
                {
                    ImportProcessFormViewmodel.CurrentEntity.DocumentIssuingDate = invoice.IssuingDate.HasValue ?
                       invoice.IssuingDate.Value : DateTime.Now;
                }
            }
            else
            {
                ImportProcessFormViewmodel.CurrentEntity.InvoiceGoodsItemTaxDifference = null;
            }
        }

        private void OnIsExistsAdditionalRequestsForImporterOrVendorCommand(int value)
        {
            IsExistsAdditionalRequestsForImporterOrVendor(value);
        }

     

        #endregion Commands Operations

        public override string IsEnabledInVersionConfigParamName => "IsCertificateOfOriginsVisible";

        #region OpenAuthenticationRequest
        public void OnShowAuthenticationRequest(AuthenticationRequestDTO authenticationRequestDTO)
        {
            var mainWin = Application.Current.MainWindow;
            var title = string.Format("{0} ",
                                      Inf.MMI.Common.Extensions.LocStr.GetLocalString(LocalStrings.Authentication));
            ImportProcessFormViewmodel.ImportProcessFormViewmodelClosingCommand = new CustomDelegateCommand(OnImportProcessFormViewmodelClosingCommand, ModuleCommands.ImportProcessFormViewmodelClosingCommand);
            popupWindow = WindowsManager.InitWindow(this, title, mainWin, ResizeMode.CanResize,
                                ImportProcessFormViewmodel.WindowLoadedCommand,
                ImportProcessFormViewmodel.ImportProcessFormViewmodelClosingCommand);

            popupWindow.SizeToContent = SizeToContent.Manual;
            popupWindow.Width = mainWin.ActualWidth * 0.8;
            popupWindow.Height = mainWin.ActualHeight * 0.8;
            popupWindow.HorizontalAlignment = HorizontalAlignment.Right;
            popupWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            popupWindow.MinimizeButtonState = WindowButtonState.Normal;
            popupWindow.MaximizeButtonState = WindowButtonState.Normal;
            popupWindow.StatusBarVisibility = Visibility.Visible;
            popupWindow.CanResize = true;
            ImportProcessFormViewmodel.AuthenticationRequestPopUpWindow = popupWindow;
            ImportProcessFormViewmodel.DecisionFilterIds = new List<int> { (int)EAuthenticationRequestDecision.NewAuthenticationRequest };
            InitInvoiceDetails(authenticationRequestDTO);
            newImportProcessRequest = InitNewImportProcess(authenticationRequestDTO);
            ResolveInstance<ICertificateOfOriginsInternalProxy>().GetEntityDocuments(OnGetEntityDocumentsCompleted, newImportProcessRequest);
            ResolveInstance<ICertificateOfOriginsInternalProxy>().GetPathsForNavigationToVendor(OnGetPathsForNavigationToVendorCompleted, newImportProcessRequest);
            ImportProcessFormViewmodel.AuthenticationRequestDTO = authenticationRequestDTO;
            ImportProcessFormViewmodel.ProgressBarVisibility = Visibility.Collapsed;
        }
        #endregion OpenAuthenticationRequest

        #region Load Operations

        /// <summary>
        /// Loads the data.
        /// </summary>
        /// <param name="navObj">The navigation object.</param>
        /// <param name="arguments">The arguments.</param>
        public override void LoadData(INavigationObject navObj, params object[] arguments)
        {
            base.LoadData(navObj, arguments);

            if (navObj == null) return;

            OnDataTransferCallback(new DataTransferCallbackEventArgs(null, DataTransferCallbackType.StopNavigationWithoutDisposePresenter));

            if (!navObj.EntityInstance.IsNewInstance())
            {
                ResolveInstance<ICertificateOfOriginsInternalProxy>().GetAuthenticationRequestByID(OnGetAuthenticationRequestByIDCompleted, navObj.EntityInstance.ID);
            }
        }

        private void AddVersionImportProcessRequest()
        {
            ImportProcessFormViewmodel.AddVersion(CertificateOfOriginsConstants.CurrentEntity,
                                                  ImportProcessFormViewmodel.CurrentEntity);
        }

        private CertificateOfOriginsImportAuthenticationRequest InitNewImportProcess(AuthenticationRequestDTO authenticationRequestDTO)
        {
            var currentUser = ResolveInstance<IUsersExternalUtil>().CurrentWithFullDetails.UserDTO;
            var originCountryID = authenticationRequestDTO.InvoiceDetailDTOList.IsNullOrEmpty()
                ? 0
                : authenticationRequestDTO.InvoiceDetailDTOList.First(i => !i.GoodsItemDetailDTOList.IsNullOrEmpty())
                    .GoodsItemDetailDTOList.First().OriginCountryID;

            var importCountryID = authenticationRequestDTO.InvoiceDetailDTOList.IsNullOrEmpty()
                ? 0
                : authenticationRequestDTO.InvoiceDetailDTOList.First().InvoiceCountryID;

            var newImportProcessRequest = new CertificateOfOriginsImportAuthenticationRequest
            {
                LeadDocumentSubmissionDate = authenticationRequestDTO.LeadDocumentSubmitDate,
                LeadDocumentID = authenticationRequestDTO.LeadDocumentID,
                DocumentIssuingDate = DateTime.Now,
                OriginCountryID = (int)originCountryID,
                IssuingCountryID = originCountryID ?? 0,
                VendorId = ImportProcessFormViewmodel.VendorID ?? 0,
                DocumentID = ImportProcessFormViewmodel.DocumentID,
                InvoiceNumber = ImportProcessFormViewmodel.Invoicenumber,
                UserResponseID = authenticationRequestDTO.AuthenticationRequestInitiatorUserID, // TODO: save the maarich from CR 126551
                UserID = authenticationRequestDTO.AuthenticationRequestInitiatorUserID,
                OrganizationUnitID = authenticationRequestDTO.OrganizationUnitID,
                OrganizationUnitTypeID = currentUser.FormalOrganizationUnitTypeID,
                ResponseNameEmail = currentUser.Email,
                ResponsePhoneNum = currentUser.PhoneNumber,
                ImporterID = authenticationRequestDTO.ImporterID,
                RequestCircumstancesID = 1,
                DecisionID = 1,
                AuthenticationRequestDate = DateTime.Now,
                ImportCountryID = importCountryID ?? 0,
                IsNewInstance = true,
                InvoiceGoodsItemTaxDifference = ImportProcessFormViewmodel.InvoiceGoodsItemTaxDifferences,
                AllInvoiceGoodsItemTaxDifference = ImportProcessFormViewmodel.AllInvoicesGoodsItemTaxDifferences,
            };
            ImportProcessFormViewmodel.ViewState = ViewStates.Edit;

            return newImportProcessRequest;
        }

        private void InitInvoiceDetails(AuthenticationRequestDTO authenticationRequestDto)
        {
            ImportProcessFormViewmodel.InvoiceDetails = authenticationRequestDto.InvoiceDetailDTOList;
            if (ImportProcessFormViewmodel.InvoiceDetails.IsNullOrEmpty()) return;
            var firstInvoice = ImportProcessFormViewmodel.InvoiceDetails.First();
            if (ImportProcessFormViewmodel.InvoiceDetails.Count == 1
                || ImportProcessFormViewmodel.InvoiceDetails.All(i => i.VendorID == firstInvoice.VendorID))
            {
                ImportProcessFormViewmodel.InvoiceID = firstInvoice.ID;
                ImportProcessFormViewmodel.VendorID = firstInvoice.VendorID;
                ImportProcessFormViewmodel.Invoicenumber = firstInvoice.Invoicenumber;
                ImportProcessFormViewmodel.InvoiceGoodsItemTaxDifferences = firstInvoice.GoodsItemTaxDifferences;
                ImportProcessFormViewmodel.AllInvoicesGoodsItemTaxDifferences = ImportProcessFormViewmodel.InvoiceDetails.Sum(i=>i.GoodsItemTaxDifferences??0);
            }
        }

        #endregion Load Operations

        #region Callback Operations

        private void OnCheckImporterOfImportAuthenticationCompleted(object sender, CallbackEventArgs<int?> e)
        {
            switch (e.Status)
            {
                case CallbackStatus.Success:
                    var importerId = e.Result;
                    if (importerId == null)
                    {
                        MsgBox.ShowError(EMessages.ItIsNotPossibleToOpenCertificateOfOriginForThisImporterPleaseContactTheDirectorOfTheVerificationDepartment);
                        ImportProcessFormViewmodel.CurrentEntity.ImporterID = null;
                    }
                    else
                    {
                        IsExistsAdditionalRequestsForImporterOrVendor(importerId);
                    }
                    break;
            }

        }

        private void OnCheckImporterOfImportAuthenticationCompletedException(object sender,CallbackEventArgs<int?> e,Exception exc)
        {
            MsgBox.ShowError(EMessages.ItIsNotPossibleToOpenCertificateOfOriginForThisImporterPleaseContactTheDirectorOfTheVerificationDepartment);
            ImportProcessFormViewmodel.CurrentEntity.ImporterID = null;
            
        }

        [CallbackMethod]
        private void OnSaveImportProcessCompleted(object sender,
                                                  CallbackEventArgs<CertificateOfOriginsImportAuthenticationRequest> e)
        {
            switch (e.Status)
            {
                case CallbackStatus.Success:
                    var certificateOfOrigin = e.Result;
                    ImportProcessFormViewmodel.CurrentEntity = certificateOfOrigin;
                    if (ImportProcessFormViewmodel.CurrentEntity.ImporterID != null)
                        ImportProcessFormViewmodel.EntityForCollateral.CustomerID = (int)ImportProcessFormViewmodel.CurrentEntity.ImporterID;
                    ImportProcessFormViewmodel.ViewState = ViewStates.ReadOnly;
                    ImportProcessFormViewmodel.IsEditModeEnabled = false;
                    if (_isCloseAfterSave)
                    {
                        ClientEnvironment.Instance.ClientServices.NavigationEngine.CloseMapping(this);
                        ImportProcessFormViewmodel.AuthenticationRequestPopUpWindow.Close();
                    }

                    break;
            }
            ImportProcessFormViewmodel.IsEditButtonEnabled = true;
        }

        private void OnSaveImportProcessCompletedFinally(object sender,
                                                         CallbackEventArgs
                                                             <CertificateOfOriginsImportAuthenticationRequest> e)
        {
            if (_isCloseAfterSave) return;
            ImportProcessFormViewmodel.IsEditModeEnabled = false;
            ImportProcessFormViewmodel.ProgressBarVisibility = Visibility.Collapsed;
        }

        private void OnSaveImportProcessCompletedException(object sender,
                                                           CallbackEventArgs
                                                               <CertificateOfOriginsImportAuthenticationRequest> e,
                                                           Exception exc)
        {
            ImportProcessFormViewmodel.ProgressBarVisibility = Visibility.Collapsed;
        }

        [CallbackMethod]
        private void OnGetEntityDocumentsCompleted(object sender, CallbackEventArgs<List<DocumentDTO>> e)
        {
            switch (e.Status)
            {
                case CallbackStatus.Success:
                    var documents = e.Result;
                    if (documents.IsNullOrEmpty())
                    {
                        var listofSupportedDocuments = GetEnumsStr(string.Empty);
                        if (listofSupportedDocuments == string.Empty)
                        {
                            MsgBox.ShowMessage(EMessages.ThereIsNoRelevantDocumentsToPerformDocumentVerificationOperationVersion2);
                        }
                        else
                        {
                            MsgBox.ShowMessage(EMessages.ThereIsNoRelevantDocumentsToPerformDocumentVerificationOperation, "", CustomMessageBoxStandardIcon.Information, null, null, args: listofSupportedDocuments);
                        }
                    }
                    else
                    {
                        // sets the current entity
                        ImportProcessFormViewmodel.CurrentEntity = newImportProcessRequest;
                        ImportProcessFormViewmodel.CurrentEntity.InvoiceID = ImportProcessFormViewmodel.InvoiceID;

                        ImportProcessFormViewmodel.AttachmentDocuments = documents;
                        if (ImportProcessFormViewmodel.AttachmentDocuments.Count == 1)
                        {
                            ImportProcessFormViewmodel.DocumentID =
                                ImportProcessFormViewmodel.AttachmentDocuments.First().ID;
                            ImportProcessFormViewmodel.CurrentEntity.DocumentNumber =
                                ImportProcessFormViewmodel.AttachmentDocuments.First().Title;
                            ImportProcessFormViewmodel.CurrentEntity.DocumentID =
                                ImportProcessFormViewmodel.AttachmentDocuments.First().ID;
                            ImportProcessFormViewmodel.SelectedDocument = new DocumentDTO();
                            ImportProcessFormViewmodel.SelectedDocument = ImportProcessFormViewmodel.AttachmentDocuments.FirstOrDefault(d => d.ID == ImportProcessFormViewmodel.DocumentID);
                            if (ImportProcessFormViewmodel.SelectedDocument != null)
                            {
                                InitInvoiceBySelectedDocument(ImportProcessFormViewmodel.SelectedDocument);
                            }
                        }

                        ImportProcessFormViewmodel.EntityForCollateral = new VirtualEntity(newImportProcessRequest);
                        if (ImportProcessFormViewmodel.CurrentEntity.ImporterID != null)
                            ImportProcessFormViewmodel.EntityForCollateral.CustomerID = (int)ImportProcessFormViewmodel.CurrentEntity.ImporterID;
                        newImportProcessRequest.IsChildrenValidationEnabled = true;
                        AddVersionImportProcessRequest();
                        ImportProcessFormViewmodel.IsImportProcessEditable = true;
                        ImportProcessFormViewmodel.CurrentEntity.Collaterals = new ObservableCollection<CollateralRequestDTO>();
                        ImportProcessFormViewmodel.AuthenticationRequestPopUpWindow.Show();
                        CertificateOfOriginsRaisePropertyChanged();
                    }
                    break;
            }
        }

        private void InitInvoiceBySelectedDocument(DocumentDTO selectedDocument)
        {
            if (ImportProcessFormViewmodel.InvoiceDetails != null)
            {
                var invoiceIdList = ImportProcessFormViewmodel.InvoiceDetails.Select(i => i.ID).ToList();
                var entityInvoice = selectedDocument.OtherRelatedEntities.FirstOrDefault(e =>
                    e.EntityTypeId == (int)EEntityType.Invoice && invoiceIdList.Contains(e.EntityId));
                if (entityInvoice != null)
                {
                    var invoice = ImportProcessFormViewmodel.InvoiceDetails.FirstOrDefault(i => i.ID == entityInvoice.EntityId);
                    ImportProcessFormViewmodel.CurrentEntity.InvoiceID = invoice.ID;
                    if (invoice.VendorID.HasValue)
                    {
                        ImportProcessFormViewmodel.CurrentEntity.VendorId = (int)invoice.VendorID;
                    }
                    ImportProcessFormViewmodel.CurrentEntity.InvoiceNumber = invoice.Invoicenumber;
                    ImportProcessFormViewmodel.CurrentEntity.InvoiceGoodsItemTaxDifference = invoice.GoodsItemTaxDifferences;

                }
            }
        }

        private string GetEnumsStr(string listOfSupportedDocuments)
        {
            var documentsFilter = CALFacade.Instance.GetConfigurationValue<string>("CertificateOfOriginsDocumentsFilter");
            if (documentsFilter != string.Empty)
            {
                var documentsStr = documentsFilter.Split(',');
                var documentsInts = new List<int>();
                foreach (var doc in documentsStr)
                {
                    int documentNumber;
                    var isParsed = int.TryParse(doc, out documentNumber);
                    if (isParsed)
                    {
                        documentsInts.Add(documentNumber);
                    }
                }
                for (var i = 0; i < documentsInts.Count(); i++)
                {
                    var currentDocumentInt = documentsInts[i];
                    var systemTablesUtil = CALFacade.Instance.ClientUnityContainer.Resolve(typeof(ISystemTablesUtilBase), null) as ISystemTablesUtilBase;
                    if (systemTablesUtil != null)
                    {
                        var currentDoc = systemTablesUtil.GetCodeById<DocumentType>(currentDocumentInt);

                        if (currentDoc != null)
                        {
                            listOfSupportedDocuments += Environment.NewLine + currentDoc.Name;
                        }
                    }
                }
            }
            return listOfSupportedDocuments;
        }

        private void OnGetEntityDocumentsCompletedFinally(object sender, CallbackEventArgs<List<DocumentDTO>> e)
        {
            ImportProcessFormViewmodel.ProgressBarVisibility = Visibility.Collapsed;
        }
        private void OnGetEntityDocumentsCompletedException(object sender, CallbackEventArgs<List<DocumentDTO>> e, Exception exc) { }


        [CallbackMethod]
        private void OnGetAuthenticationRequestByIDCompleted(object sender, CallbackEventArgs<CertificateOfOriginsImportAuthenticationRequest> e)
        {


            switch (e.Status)
            {
                case CallbackStatus.Success:

                    var request = e.Result;
                    if (request == null)
                        return;
                    ShowImportProcessPopUp(request);

                    break;
            }


        }

        public void ShowImportProcessPopUp(CertificateOfOriginsImportAuthenticationRequest request)
        {
            if (!request.AuthenticationFileID.HasValue)
            {
                var userutil = ResolveInstance<IUsersExternalUtil>().CurrentWithFullDetails;
                var originEmployee = userutil.AllowedOrganizationUnitTypes.Contains((int)EOrganizationUnitType.ClaliMakor);
                var isCentralimportverificationrequests = userutil.SpecializationIDs.Contains((int)ESpecialization.Centralimportverificationrequests);
                if (isCentralimportverificationrequests || originEmployee || request.IsCurrentUserHasOpenTask)
                    ImportProcessFormViewmodel.IsEditButtonEnabled = true;
            }
            //Get Paths For Navigation To Vendor
            ResolveInstance<ICertificateOfOriginsInternalProxy>().GetPathsForNavigationToVendor(OnGetPathsForNavigationToVendorCompleted);

            var docDto = GetDocumentDto(request.Document);
            var docDtoList = new List<DocumentDTO>();
            docDtoList.Add(docDto);

            ImportProcessFormViewmodel.CurrentEntity = request;
            if (!ImportProcessFormViewmodel.CurrentEntity.IsNewInstance)
            {
                ImportProcessFormViewmodel.DocumentIDEditViewState = ViewStates.ReadOnly;
            }
            ImportProcessFormViewmodel.SelectedDocument = docDto;
            ImportProcessFormViewmodel.SelectedDocumentType = (EDocumentType)docDto.TypeID;
            ImportProcessFormViewmodel.AttachmentDocuments = docDtoList;
            ImportProcessFormViewmodel.DocumentID = docDto.ID;

            ImportProcessFormViewmodel.ViewState = ViewStates.ReadOnly;
            ImportProcessFormViewmodel.IsEnabledInvoiceID = false;
            ImportProcessFormViewmodel.ProgressBarVisibility = Visibility.Collapsed;

            ImportProcessFormViewmodel.EntityForCollateral = new VirtualEntity(request);
            if (request.ImporterID != null)
                ImportProcessFormViewmodel.EntityForCollateral.CustomerID = (int)request.ImporterID;

            ImportProcessFormViewmodel.DecisionForCoordinator = request.Decisions.Where(d => d.IsForCoordinator).ToList();
            ImportProcessFormViewmodel.DecisionFilterIds = ImportProcessFormViewmodel.DecisionForCoordinator.Select(s => s.ID).ToList();

            if (request.DecisionID != null)
            {
                ImportProcessFormViewmodel.CurrentDecision = request.Decisions.FirstOrDefault(d => d.ID == request.DecisionID);
                ImportProcessFormViewmodel.DecisionFilterIds.Add((int)request.DecisionID);
            }

            if (request.DocumentIssuingDate < DateTime.Now.AddYears(-3))
            {
                request.IsOldIndication = true;
            }

            var window = WindowsManager.InitWindow(this, CertificateOfOriginsConsts.ImportProcessRequestTitle);
            window.MaximizeButtonState = WindowButtonState.Normal;
            window.MinimizeButtonState = WindowButtonState.Normal;
            window.SizeToContent = SizeToContent.Manual;
            window.CanResize = true;
            window.WindowSize = WindowSize.Large;

            ImportProcessFormViewmodel.AuthenticationRequestPopUpWindow = window;
            //window.ShowDialog();
            ImportProcessFormViewmodel.AuthenticationRequestPopUpWindow.Show();
            CertificateOfOriginsRaisePropertyChanged();
        }

        private void CertificateOfOriginsRaisePropertyChanged()
        {
            ImportProcessFormViewmodel.CurrentEntity.RaisePropertyChanged(CertificateOfOriginsImportAuthenticationRequest.PropInvoiceGoodsItemTaxDifference);
            ImportProcessFormViewmodel.CurrentEntity.RaisePropertyChanged(CertificateOfOriginsImportAuthenticationRequest.PropAllInvoiceGoodsItemTaxDifference);
        }

        private Customs.Infrastructure.DocumentManagement.ExternalCommon.DocumentDTO GetDocumentDto(Document doc)
        {
            var docDto = new DocumentDTO();
            docDto.ID = doc.ID;
            docDto.TypeID = doc.TypeID;
            docDto.TypeName = doc.FileUrl;
            docDto.Title = doc.Title;
            docDto.CreateDate = doc.CreateDate;
            docDto.StringDynamicParams = doc.Notes;
            docDto.ExternalID = doc.ExternalID;
            docDto.Notes = doc.ID + " " + doc.Title;

            return docDto;
        }

        private void OnGetAuthenticationRequestByIDCompletedException(object sender, CallbackEventArgs<CertificateOfOriginsImportAuthenticationRequest> e, Exception exc)
        {
        }
        private void OnGetAuthenticationRequestByIDCompletedFinally(object sender, CallbackEventArgs<CertificateOfOriginsImportAuthenticationRequest> e)
        {
            ImportProcessFormViewmodel.ProgressBarVisibility = Visibility.Collapsed;
        }

        [CallbackMethod(true, true)]
        private void OnCheckIfExistsAdditionalRequestsForImporterCompleted(object sender, CallbackEventArgs<bool> e)
        {
            switch (e.Status)
            {
                case CallbackStatus.Success:
                    ImportProcessFormViewmodel.CurrentEntity.IsExistsAdditionalRequestsForImporter = e.Result;
                    break;
            }
        }

        [CallbackMethod(true, true)]
        private void OnCheckIfExistsAdditionalRequestsForVendorCompleted(object sender, CallbackEventArgs<bool> e)
        {
            switch (e.Status)
            {
                case CallbackStatus.Success:
                    ImportProcessFormViewmodel.CurrentEntity.IsExistsAdditionalRequestsForVendor = e.Result;
                    break;
            }
        }

        [CallbackMethod(true, true)]
        private void OnGetPathsForNavigationToVendorCompleted(object sender, CallbackEventArgs<NavigationToVendorView> e)
        {
            switch (e.Status)
            {
                case CallbackStatus.Success:

                    if (newImportProcessRequest != null)
                        ImportProcessFormViewmodel.CurrentEntity = newImportProcessRequest;

                    ImportProcessFormViewmodel.CurrentEntity.NavigationToVendorView = e.Result;
                    break;
            }
        }
        #endregion

        #region PrivateMethod

        private void ResetVersion()
        {
            ImportProcessFormViewmodel.ResetValue(CertificateOfOriginsConstants.CurrentEntity);
            ImportProcessFormViewmodel.RaiseCurrentEntityPropertyChanged(CertificateOfOriginsConstants.CurrentEntity);
        }

        private void OnImportProcessFormViewmodelClosingCommand()
        {
            ImportProcessFormViewmodel.AuthenticationRequestPopUpWindow.Close();
        }

        private void IsExistsAdditionalRequestsForImporterOrVendor(int? importerId)
        {
            var request = ImportProcessFormViewmodel.CurrentEntity;

            var isVendor = AuthenticationRequestFileHelper.IsVendor(request.IssuingCountryID);
            if (request.ImporterID.HasValue && request.IssuingCountryID > 0 &&
                ((isVendor && request.VendorId > 0) || (!isVendor && request.CustomerID.HasValue)))
            {
                ResolveInstance<ICertificateOfOriginsInternalProxy>().CheckIfExistsAdditionalRequestsForImporter(OnCheckIfExistsAdditionalRequestsForImporterCompleted, request);
            }

            if (request.VendorId.HasValue && importerId != 0 && request.VendorId.Value == importerId)
            {
                ResolveInstance<ICertificateOfOriginsInternalProxy>().CheckIfExistsAdditionalRequestsForVendor(OnCheckIfExistsAdditionalRequestsForVendorCompleted, request.VendorId.Value);
            }
        }
        #endregion PrivateMethod


        public override string OnSearchEntityCommandExecuted(IEntity entity, ISearchConsumerItem selectedItem)
        {
            var searchedEntityTypeID = (int)selectedItem.Mapping.EntityType;
            return ResolveInstance<ICertificateOfOriginsInternalProxy>().GetAuthenticationRequestByID(OnSearchEntityCommandCompleted, entity.ID);

        }

        public override string OnSearchEntityCommandExecuted(string searchQuery, ISearchConsumerItem selectedItem)
        {
            var searchedEntityTypeID = ArgumentValidator.AssertNotNullAndOfType(searchQuery, "searchQuery", (object o, out int r) => int.TryParse(searchQuery, out r), true);
            return ResolveInstance<ICertificateOfOriginsInternalProxy>().GetAuthenticationRequestByID(OnSearchEntityCommandCompleted, Convert.ToInt32(searchQuery));
        }
        /// <summary>
        /// Called when [get entity completed].
        /// </summary>
        [CallbackMethod(true, true)]
        private void OnSearchEntityCommandCompleted(object sender, CallbackEventArgs<CertificateOfOriginsImportAuthenticationRequest> e)
        {
            if (e.Result != null)
            {
                e.Result.ID = e.Result.DocumentID;
                e.Result.Title = e.Result.DocumentID.ToString();
                OnSearchEntityCallback(new SearchEntityCallbackEventArgs(e, e.Result));
            }
            else
                OnSearchEntityCallback(new SearchEntityCallbackEventArgs(e, null));
        }

    }
}
