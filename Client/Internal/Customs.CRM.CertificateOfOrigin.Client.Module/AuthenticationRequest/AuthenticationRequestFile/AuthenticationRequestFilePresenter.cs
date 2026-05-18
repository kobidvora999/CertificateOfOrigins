using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Windows;
using Customs.CRM.CertificateOfOrigins.Client.Api;
using Customs.CRM.CertificateOfOrigins.Client.Api.AuthenticationRequest.AuthenticationRequestFile;
using Customs.CRM.CertificateOfOrigins.InternalProxy;
using Customs.CRM.Entities;
using Customs.CRM.Entities.CertificateOfOriginsPartial;
using Customs.CertificateOfOrigins.Entities;
using Customs.CRM.External;
using Customs.CRM.CertificateOfOrigins.InternalCommon.Common;
using Customs.CRM.CertificateOfOrigins.Client.Module.AuthenticationRequest.ImportProcessForm;
using Customs.CRM.CertificateOfOrigins.InternalCommon;
using Customs.Inf.Delivery.Client.External.Api.DistributionToCustomer;
using Customs.Inf.MMI.Common.Aspects;
using Customs.Inf.MMI.Common.CAL;
using Customs.Inf.MMI.Common.Extensions;
using Customs.Inf.MMI.Common.Navigation;
using Customs.Inf.MMI.Services.Module.ClientManager;
using Customs.KnowledgeStore.CustomsBook.ExternalCommon.Common;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.Communication;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.Extensions;
using MalamTeam.Infrastructure.GeneralServices.Environment.Enums;
using Customs.Inf.Delivery.ExternalCommon.Payload;
using Microsoft.Practices.Prism.Events;
using Customs.Inf.MMI.Services.Api.NavigationManager;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.Base_Entities;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.Exceptions;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.EntityValidation;
using Customs.Inf.MMI.Common.Toolbox.CustomEventArgs;
using Customs.Infrastructure.SystemTables.ExternalCommon;
using Customs.Infrastructure.UserManagement.ExternalProxy;
using Customs.StockPileData.Customers.ExternalCommon.Common;
using Customs.StockPileData.Customers.ExternalProxy;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.Interfaces;
using Customs.Inf.MMI.Common.Toolbox.CustomMessageBox;
using Customs.Infrastructure.Entities;

namespace Customs.CRM.CertificateOfOrigins.Client.Module.AuthenticationRequest.AuthenticationRequestFile
{
    public class AuthenticationRequestFilePresenter : PresenterBase, IAuthenticationRequestFilePresenter
    {
        private bool _isCloseAfterSave;
        private bool _isCloseTaskSendReminderForImporter;

        /// <summary>
        /// Gets the internal import process form viewmodel.
        /// </summary>
        /// <value>
        /// The internal import process form viewmodel.
        /// </value>
        internal AuthenticationRequestFileViewModel InternalAuthenticationRequestFileViewModel
        {
            get
            {
                IViewModel viewModel;
                ViewModels.TryGetValue("AuthenticationRequestFileViewModel", out viewModel);
                return viewModel as AuthenticationRequestFileViewModel;
            }
        }

        /// <summary>
        /// Gets the import process form viewmodel.
        /// </summary>
        /// <value>
        /// The import process form viewmodel.
        /// </value>
        public AuthenticationRequestFileViewModel AuthenticationRequestFileViewModel
        {
            get { return InternalAuthenticationRequestFileViewModel; }
        }

        #region ViewModel

        protected override void InternalInitViewModels()
        {
            base.InternalInitViewModels();
            var viewModel = new AuthenticationRequestFileViewModel { Name = "AuthenticationRequestFileViewModel" };
            #region Register Commands
            ViewModels.Add("AuthenticationRequestFileViewModel", viewModel);
            viewModel.Commands.RegisterCommand(OnCancelAuthenticationRequestFileCommand, ModuleCommands.CancelAuthenticationRequestFileCommand);
            viewModel.Commands.RegisterCommand<CertificateOfOriginsImportAuthenticationFileDetails>(OnEditAuthenticationRequestFileCommand, ModuleCommands.EditAuthenticationRequestFileCommand);
            viewModel.Commands.RegisterCommand(OnRefreshAuthenticationRequestFileCommand, ModuleCommands.RefreshAuthenticationRequestFileCommand);
            viewModel.Commands.RegisterCommand<bool>(OnSaveAuthenticationRequestFileCommand, ModuleCommands.SaveAuthenticationRequestFileCommand);
            viewModel.Commands.RegisterCommand<CertificateOfOriginsImportAuthenticationFileDetails>(OnRequestDeliverNotificationCommand, ModuleCommands.RequestDeliverNotificationCommand);
            viewModel.Commands.RegisterCommand<CertificateOfOriginsImportAuthenticationFileDetails>(OnRemindDeliverNotificationCommand, ModuleCommands.RemindDeliverNotificationCommand);
            viewModel.Commands.RegisterCommand<CustomsItemDTO>(OnSelectCustomItemCommand, ModuleCommands.SelectCustomItemCommand);
            viewModel.Commands.RegisterCommand(OnPreferenceDocumentTypeIDChangedCommand, ModuleCommands.PreferenceDocumentTypeIDChangedCommand);

            viewModel.Commands.RegisterCommand<CertificateOfOriginsImportAuthenticationFileDetails>(OnSendDeliveryForImporter, ModuleCommands.SendDeliveryForImporter);
            viewModel.Commands.RegisterCommand<CertificateOfOriginsImportAuthenticationFileDetails>(OnSendReminderForImporter, ModuleCommands.SendReminderForImporter);
            viewModel.Commands.RegisterCommand(OnIssuingCountryIDChangedCommand, "IssuingCountryIDChangedCommand");


            viewModel.NavigateToImportAuthenticationRequestFromUserTaskCommand = new CustomDelegateCommand<RoutedNavigationEventArgs>(OnNavigateToImportAuthenticationRequestFromUserTaskCommand, ModuleCommands.NavigateToImportAuthenticationRequestFromUserTaskCommand);
            viewModel.RequestSelection = new CustomDelegateCommand<CustomDataGridEventArgs>(OnRequestSelection, ModuleCommands.RequestSelection);

            #endregion Register Commands

            CALFacade.Instance.ClientEventAggregator.Subscribe<SendDistributionToCustomerPayload>(OnDistributionSent, ThreadOption.UIThread, true, IsHandleDeliveryForImporter);
            viewModel.UserPermissionProfile = ResolveInstance<IUsersExternalUtil>().CurrentWithFullDetails;
        }

        [CommandMethod(true, true)]
        private void OnPreferenceDocumentTypeIDChangedCommand()
        {
            var entity = AuthenticationRequestFileViewModel.SelectedRequest;
            if (entity == null)
                entity = AuthenticationRequestFileViewModel.CurrentEntity
                    .CertificateOfOriginsImportAuthenticationRequest.FirstOrDefault();
            if (entity != null)
            {
                if (entity.PreferenceDocumentTypeID != (int) ECertificateOfOriginsPrefernceDocumentType.AaccountStatment
                    && entity.PreferenceDocumentTypeID !=
                    (int) ECertificateOfOriginsPrefernceDocumentType.InvoiceStatement)
                {
                    entity.InvoiceNumber = "";
                }
                else
                {
                    entity.DocumentNumber = "";
                }
            }
        }

        [CommandMethod(true, true)]
        private void OnIssuingCountryIDChangedCommand()
        {
            if (AuthenticationRequestFileViewModel.SelectedRequest != null)
                AuthenticationRequestFileViewModel.SelectedRequest.IsVendorByIssuingCountryID =
                    AuthenticationRequestFileHelper.IsVendor(AuthenticationRequestFileViewModel.SelectedRequest.IssuingCountryID);

        }

        
        #endregion ViewModel

        #region Commands Operations

        //מעבר בין בקשות אימות
        [CommandMethod]
        private void OnRequestSelection(CustomDataGridEventArgs args)
        {
            if (args != null && args.SelectedItem != null
                && (args.SelectedItem as CertificateOfOriginsImportAuthenticationRequest) != null
                && !(args.SelectedItem as CertificateOfOriginsImportAuthenticationRequest).IsNewInstance())
            {
                AuthenticationRequestFileViewModel.SelectedRequest = (args.SelectedItem as CertificateOfOriginsImportAuthenticationRequest);
                OnIssuingCountryIDChangedCommand();
                AuthenticationRequestFileViewModel.SelectedRequest.RaisePropertyChanged("VendorId");
                 AuthenticationRequestFileViewModel.SelectedRequest.RaisePropertyChanged("CustomerID");
            }
        }
        private void OnRequestSelectionException(CustomDataGridEventArgs args, Exception exc)
        {
            AuthenticationRequestFileViewModel.ProgressBarVisibility = Visibility.Collapsed;
        }

        [CommandMethod]
        public void OnNavigateToImportAuthenticationRequestFromUserTaskCommand(RoutedNavigationEventArgs arg)
        {
            if (arg != null
                && arg.NavigationObject != null
                && arg.NavigationObject.NavigationParams != null
                && arg.NavigationObject.NavigationParams.Any())
            {
                var navigation = arg.NavigationObject.NavigationParams.First();
                var commandParameter = navigation.CommandParameter as List<VirtualEntity>;
                if (commandParameter != null)
                {
                    foreach (var cmdParams in commandParameter)
                    {
                        if (cmdParams.EntityType != EEntityType.ImportAuthenticationRequest) continue;

                        var request = AuthenticationRequestFileViewModel.CurrentEntity.CertificateOfOriginsImportAuthenticationRequest.FirstOrDefault(c => c.ID == cmdParams.ID);
                        if (request == null) continue;

                        if (AuthenticationRequestFileViewModel.SelectedRequest != null && AuthenticationRequestFileViewModel.SelectedRequest.ID != request.ID && AuthenticationRequestFileViewModel.ViewState != ViewStates.ReadOnly)
                        {
                            throw new InfException(EMessages.ViewAlreadyOpenedOnOtherRequest, new List<MalamValidationParameter> { new MalamValidationParameter { Value = AuthenticationRequestFileViewModel.SelectedRequest.ID } });
                        }

                        AuthenticationRequestFileViewModel.SelectedRequest = request;
                        AuthenticationRequestFileViewModel.IsEditModeEnabled = true;

                        return;
                    }
                }
            }
        }
        private void OnNavigateToImportAuthenticationRequestFromUserTaskCommandException(RoutedNavigationEventArgs arg, Exception exc) { }

        [CommandMethod]
        private void OnCancelAuthenticationRequestFileCommand()
        {
            ResetVersion();
            AuthenticationRequestFileViewModel.ViewState = ViewStates.ReadOnly;
            AuthenticationRequestFileViewModel.IsEditModeEnabled = false;
            AuthenticationRequestFileViewModel.IsEditEnable = true;
        }
        private void OnCancelAuthenticationRequestFileCommandException(Exception exc) { }

        [CommandMethod]
        private void OnSaveAuthenticationRequestFileCommand(bool isClose)
        {
            _isCloseAfterSave = isClose;
            AuthenticationRequestFileViewModel.ProgressBarVisibility = Visibility.Visible;

            if (AuthenticationRequestFileViewModel.SaveDocumentsCommand != null &&
                AuthenticationRequestFileViewModel.SaveDocumentsCommand.CanExecute(null))
            {
                AuthenticationRequestFileViewModel.SaveDocumentsCommand.Execute(null);
            }
            SaveAuthenticationRequestFile();
            AuthenticationRequestFileViewModel.IsEditEnable = true;
        }
        private void OnSaveAuthenticationRequestFileCommandException(bool isClose, Exception exc) { }


        [CommandMethod]
        private void OnRefreshAuthenticationRequestFileCommand()
        {
            AuthenticationRequestFileViewModel.ProgressBarVisibility = Visibility.Visible;
            ResolveInstance<ICertificateOfOriginsInternalProxy>().GetAuthenticationRequestFileByID(OnGetAuthenticationRequestFileByIDCompleted, AuthenticationRequestFileViewModel.CurrentEntity.ID);
        }



        private void OnRefreshAuthenticationRequestFileCommandException(Exception exc) { }

        [CommandMethod]
        private void OnEditAuthenticationRequestFileCommand(CertificateOfOriginsImportAuthenticationFileDetails obj)
        {
            AuthenticationRequestFileViewModel.ViewState = ViewStates.Edit;
            AuthenticationRequestFileViewModel.IsEditModeEnabled = true;
            AuthenticationRequestFileViewModel.IsEditEnable = false;
            AddVersion();
        }
        private void OnEditAuthenticationRequestFileCommandException(CertificateOfOriginsImportAuthenticationFileDetails obj, Exception exc) { }

        [CommandMethod]
        private void OnRequestDeliverNotificationCommand(CertificateOfOriginsImportAuthenticationFileDetails file)
        {
            if (CheckBeforeSendNotificationAndReminder())
            {
                //Sends notification to the CustomsHouse or to the Vendor
               if(AuthenticationRequestFileViewModel.CurrentEntity.FirstProvideContactDate == null)
                   AuthenticationRequestFileViewModel.CurrentEntity.FirstProvideContactDate = DateTime.Today;
                AuthenticationRequestFileViewModel.CurrentEntity.IsDelivery = true;
                AuthenticationRequestFileViewModel.CurrentEntity.IsVendorOrCustomsHouse = true;
                SendReminderOrNotificationToCustomer(file.CustomerIDList,
                                                        ETemplate.ImportRequestForVerificationEUR,
                                                        AuthenticationRequestFileViewModel.CurrentEntity);
                ResolveInstance<ICertificateOfOriginsInternalProxy>().ChangeStatusAfterDeliverySent(OnChangeStatusAfterDeliverySent, AuthenticationRequestFileViewModel.CurrentEntity);
            }
            else
            {
                MsgBox.ShowMessage(ResolveInstance<IStringUtil>().GetString(LocalStrings.CustomsHouseWarning));
            }
        }
        private void OnRequestDeliverNotificationCommandException(CertificateOfOriginsImportAuthenticationFileDetails file, Exception exc) { }

        [CommandMethod]
        private void OnRemindDeliverNotificationCommand(CertificateOfOriginsImportAuthenticationFileDetails file)
        {
            if (CheckBeforeSendNotificationAndReminder())
            {
                //Sends Reminder to the CustomsHouse or to the Vendor
                AuthenticationRequestFileViewModel.CurrentEntity.IsDelivery = false;
                AuthenticationRequestFileViewModel.CurrentEntity.IsVendorOrCustomsHouse = true;
                SendReminderOrNotificationToCustomer(file.CustomerIDList,
                                                        ETemplate.ImportRequestForVerificationEURreminder,
                                                        AuthenticationRequestFileViewModel.CurrentEntity);
                ResolveInstance<ICertificateOfOriginsInternalProxy>().HandleSendRemindDeliverNotification(OnHandleSendRemindDeliverNotificationCompleted, file); ;
                ResolveInstance<ICertificateOfOriginsInternalProxy>().ChangeStatusAfterDeliverySent(OnChangeStatusAfterDeliverySent, AuthenticationRequestFileViewModel.CurrentEntity);
            }
            else
            {
                MsgBox.ShowMessage(ResolveInstance<IStringUtil>().GetString(LocalStrings.CustomsHouseWarning));
            }
        }

        private void OnHandleSendRemindDeliverNotificationCompleted(object sender, CallbackEventArgs<bool> e)
        {
            
        }

        private void OnRemindDeliverNotificationCommandException(CertificateOfOriginsImportAuthenticationFileDetails file, Exception exc) { }

        private void OnSendDeliveryForImporter(CertificateOfOriginsImportAuthenticationFileDetails file)
        {
            AuthenticationRequestFileViewModel.CurrentEntity.IsVendorOrCustomsHouse = false;
            SendReminderOrNotificationToCustomer(new List<int> { AuthenticationRequestFileViewModel.SelectedRequest.ImporterID ?? 0 },
                                                    ETemplate.ImportAuthenticationRequestImporterLetter,
                                                    AuthenticationRequestFileViewModel.SelectedRequest,
                                                    true,
                                                    false);
            ResolveInstance<ICertificateOfOriginsInternalProxy>().ChangeStatusAfterDeliverySent(OnChangeStatusAfterDeliverySent, AuthenticationRequestFileViewModel.CurrentEntity);
        }
        
        private void OnChangeStatusAfterDeliverySent(object sender, CallbackEventArgs<CertificateOfOriginsImportAuthenticationFileDetails> e)
        {
            
        }

        private void OnSendDeliveryForImporterException(CertificateOfOriginsImportAuthenticationFileDetails file, Exception exc) { }

        private void OnSendReminderForImporter(CertificateOfOriginsImportAuthenticationFileDetails file)
        {
                AuthenticationRequestFileViewModel.CurrentEntity.IsVendorOrCustomsHouse = false;
                SendReminderOrNotificationToCustomer(new List<int> { AuthenticationRequestFileViewModel.SelectedRequest.ImporterID ?? 0 },
                                                        ETemplate.ImportAuthenticationRequestImporterLetter,
                                                        AuthenticationRequestFileViewModel.SelectedRequest,
                                                        true,
                                                        true);
                ResolveInstance<ICertificateOfOriginsInternalProxy>().ChangeStatusAfterDeliverySent(OnChangeStatusAfterDeliverySent, AuthenticationRequestFileViewModel.CurrentEntity);
        }

        private void OnSendReminderForImporterException(CertificateOfOriginsImportAuthenticationFileDetails file, Exception exc) { }

        #region Distribution To Importer

        private bool IsHandleDeliveryForImporter(SendDistributionToCustomerPayload payload)
        {
            var isImportAuthenticationRequest = payload.EntityID == AuthenticationRequestFileViewModel.SelectedRequest.ID
                    && payload.EntityTypeID == EEntityType.ImportAuthenticationRequest;

            var isAuthenticationRequestFile = payload.EntityID == AuthenticationRequestFileViewModel.CurrentEntity.ID
                    && payload.EntityTypeID == EEntityType.AuthenticationRequestFile;

            return isImportAuthenticationRequest || isAuthenticationRequestFile;

        }

        private void OnDistributionSent(SendDistributionToCustomerPayload payload)
        {
            AuthenticationRequestFileViewModel.FileStatusFilterIds = new ObservableCollection<int>();
            AuthenticationRequestFileViewModel.CurrentEntity.FileStatuses = new List<CertificateOfOriginsAuthenticationFileStatus>();

            if (!AuthenticationRequestFileViewModel.CurrentEntity.IsVendorOrCustomsHouse && _isCloseTaskSendReminderForImporter)
            {
                ResolveInstance<ICertificateOfOriginsInternalProxy>().HandleImportAuthenticationRequestDeliveryReminderForImporterSent(OnHandleImportAuthenticationRequestDeliveryReminderForImporterSentCompleted, AuthenticationRequestFileViewModel.SelectedRequest);
            }
            else if (!AuthenticationRequestFileViewModel.CurrentEntity.IsVendorOrCustomsHouse) // OnSendDeliveryForImporter
            {
                ResolveInstance<ICertificateOfOriginsInternalProxy>().HandleImportAuthenticationRequestDeliveryForImporterSent(OnHandleImportAuthenticationRequestDeliveryForImporterSentCompleted, AuthenticationRequestFileViewModel.SelectedRequest);
            }
            else
            {
                // Service that handles vendor / CustomsHouse
                ResolveInstance<ICertificateOfOriginsInternalProxy>()
                    .HandleImportAuthenticationRequestDeliveryAndReminderForVendorSent(
                        HandleImportAuthenticationRequestDeliveryAndReminderForVendorSentCompleted,
                        AuthenticationRequestFileViewModel.CurrentEntity,
                        AuthenticationRequestFileViewModel.CurrentEntity.IsDelivery);
            }
        }

        [CallbackMethod(true, true)]
        private void HandleImportAuthenticationRequestDeliveryAndReminderForVendorSentCompleted(object sender, CallbackEventArgs<CertificateOfOriginsImportAuthenticationFileDetails> args)
        {
            switch (args.Status)
            {
                case CallbackStatus.Success:
                    if (args.Result != null)
                    {
                        InternalAuthenticationRequestFileViewModel.CurrentEntity = args.Result;
                        AuthenticationRequestFileViewModel.RefreshAuthenticationRequestFileCommand.ExecuteFromModule();
                    }
                    break;
            }
        }

        [CallbackMethod(true, true)]
        private void OnHandleImportAuthenticationRequestDeliveryForImporterSentCompleted(object sender, CallbackEventArgs<CertificateOfOriginsImportAuthenticationRequest> args)
        {
            switch (args.Status)
            {
                case CallbackStatus.Success:
                    if (args.Result != null)
                    {
                        InternalAuthenticationRequestFileViewModel.SelectedRequest = args.Result;
                        AuthenticationRequestFileViewModel.RefreshAuthenticationRequestFileCommand.ExecuteFromModule();
                    }
                    break;
            }
        }

        [CallbackMethod(true, true)]
        private void OnHandleImportAuthenticationRequestDeliveryReminderForImporterSentCompleted(object sender, CallbackEventArgs<CertificateOfOriginsImportAuthenticationRequest> args)
        {
            switch (args.Status)
            {
                case CallbackStatus.Success:
                    if (args.Result != null)
                    {
                        InternalAuthenticationRequestFileViewModel.SelectedRequest = args.Result;
                        InternalAuthenticationRequestFileViewModel.SelectedRequest.IsSendReminderForImporterTaskExists = false;
                    }
                    break;
            }
        }


        #endregion Distribution To Importer

        [CommandMethod]
        private void OnSelectCustomItemCommand(CustomsItemDTO customItem)
        {
            var goodsItem = AuthenticationRequestFileViewModel.SelectedRequest.CertificateOfOriginsItemDetails.FirstOrDefault(gi => gi.CustomItemID == customItem.ID);
            if (goodsItem != null)
            {
                goodsItem.GoodsDescription = customItem.GoodsDescription;
                goodsItem.MeasurementUnitId = customItem.MeasurementUnitID;

                if (goodsItem.MeasurementUnitId == null)
                {
                    goodsItem.IsMeasurementUnitIdSelctable = true;
                }
            }
        }
        private void OnSelectCustomItemCommandException(CustomsItemDTO customItem, Exception exc)
        {
        }

        #endregion Commands Operations

        public override string IsEnabledInVersionConfigParamName => "IsCertificateOfOriginsVisible";

        #region Callback Operations

        [CallbackMethod(true, true)]
        private void OnCheckIfExistsAdditionalRequestsForVendorCompleted(object sender, CallbackEventArgs<bool> e)
        {
            switch (e.Status)
            {
                case CallbackStatus.Success:
                    AuthenticationRequestFileViewModel.CurrentEntity.CertificateOfOriginsImportAuthenticationRequest
                        .Where(c => c.VendorId == AuthenticationRequestFileViewModel.VendorID).ToList()
                        .ForEach(c => c.IsExistsAdditionalRequestsForVendor = e.Result);
                    break;
            }
        }

        [CallbackMethod(true, true)]
        private void OnGetPathsForNavigationToVendorCompleted(object sender, CallbackEventArgs<NavigationToVendorView> e)
        {
            switch (e.Status)
            {
                case CallbackStatus.Success:
                    AuthenticationRequestFileViewModel.CurrentEntity.CertificateOfOriginsImportAuthenticationRequest
                        .Where(c => c.VendorId == AuthenticationRequestFileViewModel.VendorID).ToList()
                        .ForEach(c => c.NavigationToVendorView = e.Result);
                    break;
            }
        }

        [CallbackMethod(false, false)]
        private void OnGetAuthenticationRequestFileByIDCompleted(object sender, CallbackEventArgs<CertificateOfOriginsImportAuthenticationFileDetails> e)
        {
            switch (e.Status)
            {
                case CallbackStatus.Success:
                    AuthenticationRequestFileViewModel.IsEditEnable = true;
                    var authenticationFileStatusID = e.Result.AuthenticationFileStatusID;
                    AuthenticationRequestFileViewModel.StatusPredicate = string.Format(CertificateOfOriginsConsts.StatusPredicateString, authenticationFileStatusID,
                        (int)EAuthenticationFileStatus.RightAuthenticationAnswer, (int)EAuthenticationFileStatus.WrongAuthenticationAnswer,
                        (int)EAuthenticationFileStatus.ClarificationRequired, (int)EAuthenticationFileStatus.ReceivedAnswerInFile, (int)EAuthenticationFileStatus.CancelledFile);
                    if(authenticationFileStatusID == (int)EAuthenticationFileStatus.AuthenticationRequestWasSend)
                        AuthenticationRequestFileViewModel.StatusPredicate += string.Format(CertificateOfOriginsConsts.AddAuthenticationRequestReminderWasSend, (int)EAuthenticationFileStatus.AuthenticationRequestReminderWasSend);
                    InternalGetAuthenticationRequestFileByIDCompleted(e);
                    break;
            }
        }
        private void OnGetAuthenticationRequestFileByIDCompletedFinally(object sender, CallbackEventArgs<CertificateOfOriginsImportAuthenticationFileDetails> e)
        {
            AuthenticationRequestFileViewModel.ProgressBarVisibility = Visibility.Collapsed;
            AuthenticationRequestFileViewModel.ViewState = ViewStates.ReadOnly;
            //AuthenticationRequestFileViewModel.IsEditModeEnabled = false;

        }
        private void OnGetAuthenticationRequestFileByIDCompletedException(object sender, CallbackEventArgs<CertificateOfOriginsImportAuthenticationFileDetails> e, Exception exc)
        {
            AuthenticationRequestFileViewModel.ProgressBarVisibility = Visibility.Collapsed;
        }

        [CallbackMethod(false, false)]
        private void OnSaveImportProcessCompleted(object sender, CallbackEventArgs<CertificateOfOriginsImportAuthenticationFileDetails> e)
        {
            switch (e.Status)
            {
                case CallbackStatus.Success:
                    AuthenticationRequestFileViewModel.CurrentEntity = e.Result;
                    AuthenticationRequestFileViewModel.ViewState = ViewStates.ReadOnly;
                    AuthenticationRequestFileViewModel.IsEditModeEnabled = false;
                    if (_isCloseAfterSave)
                    {
                        ClientEnvironment.Instance.ClientServices.NavigationEngine.CloseMapping(this);
                    }

                    InternalGetAuthenticationRequestFileByIDCompleted(e);
                    break;
            }
        }

        private void InternalGetAuthenticationRequestFileByIDCompleted(CallbackEventArgs<CertificateOfOriginsImportAuthenticationFileDetails> e)
        {

            if (e.Result == null)
            {
                AuthenticationRequestFileViewModel.ProgressBarVisibility = Visibility.Collapsed;
                return;
            }

            AuthenticationRequestFileViewModel.CurrentEntity = e.Result;
            AuthenticationRequestFileViewModel.CurrentEntity.OriginalAuthenticationFileStatusID = AuthenticationRequestFileViewModel.CurrentEntity.AuthenticationFileStatusID;

            AuthenticationRequestFileViewModel.SelectedRequest = AuthenticationRequestFileViewModel.CurrentEntity.CertificateOfOriginsImportAuthenticationRequest.FirstOrDefault();

            var allowedCustomsHouses = 0;
            if (AuthenticationRequestFileViewModel.SelectedRequest != null)
            {
                if (AuthenticationRequestFileViewModel.SelectedRequest.CustomerID.HasValue)
                    AuthenticationRequestFileViewModel.CurrentEntity.CustomerID = AuthenticationRequestFileViewModel.SelectedRequest.CustomerID.Value;

                AuthenticationRequestFileViewModel.DecisionForClaliMakorWorker = AuthenticationRequestFileViewModel.SelectedRequest.Decisions.Where(d => d.IsForClaliMakorWorker).ToList();

                allowedCustomsHouses = AuthenticationRequestFileViewModel.UserPermissionProfile.AllowedCustomHouses.Select(c => c)
                        .FirstOrDefault(id => (id == 99 || id == 599)
                                              && id == AuthenticationRequestFileViewModel.SelectedRequest.OrganizationUnitID);
            }

            AuthenticationRequestFileViewModel.IsRequestSent = AuthenticationRequestFileViewModel.CurrentEntity.DeliveryMethodID != (int)EDeliveryMethod.WasNotSend;
            AuthenticationRequestFileViewModel.IsUserAllowedToSendReminderToVendorOrCustomsHouse = allowedCustomsHouses > 0 && !AuthenticationRequestFileViewModel.IsRequestSent;
            AuthenticationRequestFileViewModel.FileStatusFilterIds.Clear();
            AuthenticationRequestFileViewModel.FileStatusFilterIds.AddRange(AuthenticationRequestFileViewModel.CurrentEntity.FileStatuses.Where(fs => !fs.IsAutomatic).Select(s => s.ID).ToList());
            if (AuthenticationRequestFileViewModel.CurrentEntity.FileStatuses.Any(fs => fs.ID == AuthenticationRequestFileViewModel.CurrentEntity.AuthenticationFileStatusID
                                                                                 && fs.IsAutomatic))
            {
                AuthenticationRequestFileViewModel.FileStatusFilterIds.Add(AuthenticationRequestFileViewModel.CurrentEntity.AuthenticationFileStatusID);
            }

            foreach (var request in AuthenticationRequestFileViewModel.CurrentEntity.CertificateOfOriginsImportAuthenticationRequest)
            {
                var decisionFilterForFileIds = new List<int>();

                if (request.DecisionID != null)
                {
                    request.OriginalRequestDecisionID = request.DecisionID;
                    //AuthenticationRequestFileViewModel.DecisionFilterForFileIds.Add((int)request.DecisionID); // current decision
                    decisionFilterForFileIds.Add((int)request.DecisionID); // current decision
                }
                if (request.DocumentIssuingDate < DateTime.Now.AddYears(-3))
                {
                    request.IsOldIndication = true;
                }
                if (request.Decisions.Any(d => d.ID == request.DecisionID && d.IsAutomatic)) // decision of reminders
                {
                    //AuthenticationRequestFileViewModel.DecisionFilterForFileIds.Add((int)request.DecisionID);
                    decisionFilterForFileIds.Add((int)request.DecisionID);
                }

                AuthenticationRequestFileViewModel.DecisionFilterForFileIds = AuthenticationRequestFileViewModel.DecisionForClaliMakorWorker.Select(s => s.ID).ToList().Union(decisionFilterForFileIds).ToList();

                if (request.VendorId.HasValue && request.VendorId != 0)
                {
                    AuthenticationRequestFileViewModel.VendorID = request.VendorId.Value;
                    ResolveInstance<ICertificateOfOriginsInternalProxy>().CheckIfExistsAdditionalRequestsForVendor(OnCheckIfExistsAdditionalRequestsForVendorCompleted, request.VendorId.Value);

                    //Get Paths For Navigation To Vendor
                    ResolveInstance<ICertificateOfOriginsInternalProxy>().GetPathsForNavigationToVendor(OnGetPathsForNavigationToVendorCompleted);
                }
            }

            AuthenticationRequestFileViewModel.ProgressBarVisibility = Visibility.Collapsed;

        }

        private void OnSaveImportProcessCompletedFinally(object sender, CallbackEventArgs<CertificateOfOriginsImportAuthenticationFileDetails> e)
        {
            if (_isCloseAfterSave) return;
            AuthenticationRequestFileViewModel.IsEditModeEnabled = false;
            AuthenticationRequestFileViewModel.ProgressBarVisibility = Visibility.Collapsed;
        }

        private void OnSaveImportProcessCompletedException(object sender, CallbackEventArgs<CertificateOfOriginsImportAuthenticationFileDetails> e, Exception exc)
        {
            AuthenticationRequestFileViewModel.ProgressBarVisibility = Visibility.Collapsed;

        }

        #endregion Callback Operations

        #region LoadData

        public override void LoadData(INavigationObject navObj, params object[] arguments)
        {
            base.LoadData(navObj, arguments);

            if (!navObj.EntityInstance.IsNewInstance())
            {
                ResolveInstance<ICertificateOfOriginsInternalProxy>().GetAuthenticationRequestFileByID(
                 OnGetAuthenticationRequestFileByIDCompleted, navObj.EntityId);
            }
        }
        #endregion LoadData

        #region PrivateMethod
        private void AddVersion()
        {
            AuthenticationRequestFileViewModel.AddVersion(CertificateOfOriginsConstants.CurrentEntity, AuthenticationRequestFileViewModel.CurrentEntity);
        }
        private void SetEditModeStateToReadyOnly(bool isReadOnly)
        {
            // check update enabled conditions
            if (isReadOnly)
            {
                // disables editing for all field, influences on update, save and cancel buttons (update inversed to save and cancel).
                AuthenticationRequestFileViewModel.IsEditModeEnabled = false;
                // set the viewStates
                SetViewState(ViewStates.ReadOnly);
            }
            else
            {
                // isEditModeEnabled influences on update, save and cancel buttons (update inversed to save and cancel).
                AuthenticationRequestFileViewModel.IsEditModeEnabled = true;
                // set the viewStates
                SetViewState(ViewStates.Edit);
            }
        }

        private void SetViewState(ViewStates viewState)
        {
            AuthenticationRequestFileViewModel.ViewState = viewState;
        }

        private void ResetVersion()
        {
            AuthenticationRequestFileViewModel.ResetValue(CertificateOfOriginsConstants.CurrentEntity);
            AuthenticationRequestFileViewModel.RaiseCurrentEntityPropertyChanged(CertificateOfOriginsConstants.CurrentEntity);
        }

        private void SaveAuthenticationRequestFile()
        {
            var authenticationRequestFile = AuthenticationRequestFileViewModel.CurrentEntity;
            ResolveInstance<ICertificateOfOriginsInternalProxy>().SaveAuthenticationRequestFile(OnSaveImportProcessCompleted, authenticationRequestFile);
        }

        private void SendReminderOrNotificationToCustomer(List<int> customersList, ETemplate templateType, IEntity currentEntity, bool sendToImporter = false, bool closeTask = false)
        {
            var sendToVendor = SendToVendorOrCustomsHouse();
            var vendorID = sendToVendor ? AuthenticationRequestFileViewModel.CurrentEntity.CertificateOfOriginsImportAuthenticationRequest.FirstOrDefault()?.VendorId : null;

            if (!vendorID.HasValue && !sendToImporter)
            {
                if (AuthenticationRequestFileViewModel.CurrentEntity.CustomerID == 0)
                {
                    throw new InfException(EMessages.ValueNull,
                        new[] { new MalamValidationParameter { Value = CertificateOfOriginsConsts.ForeignCustomsHouseName } });
                }
                customersList.Clear();
                customersList.Add(AuthenticationRequestFileViewModel.CurrentEntity.CustomerID);
            }

            var customerFilter = new CustomerIdentificationFilter
            {
                CustomerId = vendorID.HasValue && !sendToImporter ? vendorID : AuthenticationRequestFileViewModel.CurrentEntity.CustomerID,
                AddressFilter = new AddressIdentificationFilter() { AddressPurpose = EAddressPurpose.Authentication }
            };

            var customerDTO = ResolveInstance<ICustomersExternalProxy>().GetCustomerDTOByCustomerIdentificationSync_NotCached(customerFilter);
            var customerAddressPurpose = customerDTO?.Addresses.Select(ad => ad.AddressPurprose).FirstOrDefault(ap => ap != null && ap.Value == (int)EAddressPurpose.Authentication);

            var addressPurpose = customerAddressPurpose.HasValue
                ? EAddressPurpose.Authentication
                : EAddressPurpose.General;
            var organizationUnitID = Int32.Parse(ClientEnvironment.Instance.MMIFacade.GetConfigurationValue<string>(CertificateOfOriginsConstants.OrgUnitIdsForReadMails).Split(';').FirstOrDefault());
            DistributionToCustomerArguments args;
            var certificateOfOriginsImportAuthenticationRequest = AuthenticationRequestFileViewModel.CurrentEntity.CertificateOfOriginsImportAuthenticationRequest.First();
            if (sendToImporter)
            {
                args = new DistributionToCustomerArguments(addressPurpose, currentEntity)
                {
                    DefaultCustomerIds = customersList,
                    IsDeliveryTemplateOrDocRadioVisible = true,
                    CurrentEntity = new VirtualEntity(currentEntity) { OrganizationUnitID = organizationUnitID != null? organizationUnitID : currentEntity.OrganizationUnitID},
                    DistributionID = null,
                    VendorID = !sendToImporter && sendToVendor ? vendorID : null,
                    AdditionalEntityForSendDocs = new VirtualEntity(currentEntity),
                    CustomerIdsToExclude = certificateOfOriginsImportAuthenticationRequest.ImporterID.HasValue?
                    new List<int>() {
                    certificateOfOriginsImportAuthenticationRequest.ImporterID.Value
                    } :
                    null,
                    IsOutSeaDest = false,
                };
            }
            else
            {
                args = new DistributionToCustomerArguments(addressPurpose, currentEntity)
                {
                    DefaultCustomerIds = customersList,
                    IsDeliveryTemplateOrDocRadioVisible = true,
                    CurrentEntity = new VirtualEntity(currentEntity) { OrganizationUnitID = organizationUnitID != null ? organizationUnitID : currentEntity.OrganizationUnitID },
                    DistributionID = null,
                    VendorID = !sendToImporter && sendToVendor ? vendorID : null,
                    AdditionalEntityForSendDocs = new VirtualEntity(currentEntity),
                    AdditionalDocIds = AuthenticationRequestFileViewModel.CurrentEntity.CertificateOfOriginsImportAuthenticationRequest.Select(cooiar => cooiar.DocumentID).ToList(),
                    IsOutSeaDest = true,
                    EntityToAdditionalDocTypesTuple = sendToVendor ? null :
                        AuthenticationRequestFileViewModel.CurrentEntity.CertificateOfOriginsImportAuthenticationRequest.Count > 1 ? null :
                        new Tuple<VirtualEntity, List<int>>(new VirtualEntity(AuthenticationRequestFileViewModel.SelectedRequest),new List<int>() {(int) EDocumentType.Commercialinvoice})
                };
            }

            if (currentEntity.EntityType == EEntityType.ImportAuthenticationRequest)
            {
                args.EntityTypeToOverideRelatedEntity = (int)AuthenticationRequestFileViewModel.CurrentEntity.EntityType;
                args.RelatedEntites = new List<VirtualEntity> { new VirtualEntity(AuthenticationRequestFileViewModel.CurrentEntity) };
            }

            using (var deliveryPresenter = ResolveInstance<IDistributionToCustomerApi>())
            {
                deliveryPresenter.ShowDistrinutionToCustomerView(args, true);
            }

            if (sendToImporter)
                _isCloseTaskSendReminderForImporter = closeTask;
        }

        protected override bool Dispose(bool disposing)
        {
            if (!disposing) return false;
            CALFacade.Instance.ClientEventAggregator.Unsubscribe<SendDistributionToCustomerPayload>(OnDistributionSent);
            return base.Dispose(true);
        }

        private bool SendToVendorOrCustomsHouse()
        {
            var sendToVendor = false;
            var systemTablesUtil = CALFacade.Instance.ClientUnityContainer.Resolve(typeof(ISystemTablesUtil), null) as ISystemTablesUtil;
            if (systemTablesUtil == null) return false;

            var filter = new SystemTablesFilter
            {
                AssemblyQualifiedName = typeof(CertificateOfOriginSupplierDeliveryCountryConfig).AssemblyQualifiedName,
                IDs = new List<int> { AuthenticationRequestFileViewModel.SelectedRequest.IssuingCountryID }
            };

            try
            {
                var country = systemTablesUtil.GetIdByCode<CertificateOfOriginSupplierDeliveryCountryConfig>("ConutryID",
                    AuthenticationRequestFileViewModel.SelectedRequest.IssuingCountryID);

                return country > 0;
            }
            catch (Exception e)
            {
                return false;
            }


            return sendToVendor;
        }
        private bool CheckBeforeSendNotificationAndReminder()
        {
            if (!AuthenticationRequestFileHelper.IsVendor(AuthenticationRequestFileViewModel.CurrentEntity.CertificateOfOriginsImportAuthenticationRequest.First().IssuingCountryID))
            {
                if (AuthenticationRequestFileViewModel.CurrentEntity.CustomerID == -1)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            return true;
        }
        
        #endregion PrivateMethod

    }
}
