using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Customs.CRM.CertificateOfOrigins.Client.Api;
using Customs.CRM.CertificateOfOrigins.Client.Api.ExportDocumentAuthenticationRequestEdit;
using Customs.CRM.CertificateOfOrigins.InternalProxy;
using Customs.CRM.Entities;
using Customs.CertificateOfOrigins.Entities;
using Customs.Inf.Delivery.Client.External.Api.DistributionToCustomer;
using Customs.Inf.MMI.Common.Aspects;
using Customs.Inf.MMI.Common.CAL;
using Customs.Inf.MMI.Common.Common;
using Customs.Inf.MMI.Common.Extensions;
using Customs.Inf.MMI.Common.Navigation;
using Customs.Inf.MMI.Common.Toolbox.CustomMessageBox;
using Customs.Inf.MMI.Services.Module.ClientManager;
using Customs.Infrastructure.DocumentManagement.ExternalCommon;
using Customs.StockPileData.Customers.ExternalCommon.Common;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.Communication;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.Extensions;
using MalamTeam.Infrastructure.GeneralServices.Environment.Enums;
using Customs.Inf.Delivery.ExternalCommon.Payload;
using Microsoft.Practices.Prism.Events;
using ObjectWithChangeTrackerExtensions = Customs.CertificateOfOrigins.Entities.ObjectWithChangeTrackerExtensions;
using Customs.StockPileData.Customers.ExternalProxy;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.Base_Entities;

namespace Customs.CRM.CertificateOfOrigins.Client.Module.ExportDocumentAuthenticationRequestEdit
{
    /// <summary>
    /// ExportDocumentAuthenticationRequestPresenter
    /// </summary>
    public class ExportDocumentAuthenticationRequestPresenter : PresenterBase, IExportDocumentAuthenticationRequestPresenter
    {
        /// <summary>
        /// Gets the view model object.
        /// </summary>
        /// <value>The current view model object.</value>
        public ExportDocumentAuthenticationRequestViewModel ExportDocumentAuthenticationRequestViewModel
        {
            get
            {
                IViewModel viewModel;
                ViewModels.TryGetValue("ExportDocumentAuthenticationRequestViewModel", out viewModel);
                return viewModel as ExportDocumentAuthenticationRequestViewModel;
            }
        }

        #region ViewModel

        /// <summary>
        /// Initializes the view models.<para/>
        /// (called before "InternalInitViews")
        /// </summary>
        protected override void InternalInitViewModels()
        {
            base.InternalInitViewModels();

            var viewModel = new ExportDocumentAuthenticationRequestViewModel { Name = "ExportDocumentAuthenticationRequestViewModel" };
            ViewModels.Add("ExportDocumentAuthenticationRequestViewModel", viewModel);

            #region init commands

            viewModel.EditCommand = new CustomDelegateCommand(OnEditCommand, "EditCommand");
            viewModel.SaveCommand = new CustomDelegateCommand(OnSaveCommand, "SaveCommand");
            viewModel.SaveCloseCommand = new CustomDelegateCommand(OnSaveCloseCommand, "SaveCloseCommand");
            viewModel.CancelCommand = new CustomDelegateCommand(OnCancelCommand, "CancelCommand");
            viewModel.RefreshCommand = new CustomDelegateCommand(OnRefreshCommand, "RefreshCommand");
            viewModel.SaveMainDocumentCompletedCommand = new CustomDelegateCommand<DocumentCommandArgs>(OnSaveMainDocumentCompletedCommand, "SaveMainDocumentCompletedCommand");
            viewModel.SaveAdditionalDocumentsCompletedCommand = new CustomDelegateCommand<DocumentCommandArgs>(OnSaveAdditionalDocumentsCompletedCommand, "SaveAdditionalDocumentsCompletedCommand");
            viewModel.Commands.RegisterCommand<ExportDocumentAuthenticationRequest>(OnRequestDeliverNotificationCommand, ModuleCommands.RequestDeliverNotificationCommand);
            viewModel.Commands.RegisterCommand<ExportDocumentAuthenticationRequest>(OnRemindDeliverNotificationCommand, ModuleCommands.RemindDeliverNotificationCommand);
            viewModel.DeliverToCustomsHouseNotificationCommand = new CustomDelegateCommand<ExportDocumentAuthenticationRequest>(OnDeliverToCustomsHouseNotificationCommand, "DeliverToCustomsHouseNotificationCommand");
            viewModel.Commands.RegisterCommand(OnForeignCustomerSelectedChangeCommand, ModuleCommands.ForeignCustomerSelectedChangeCommand);
            viewModel.CountrySelectedChangeCommand = new CustomDelegateCommand(OnCountrySelectedChangeCommand, "CountrySelectedChangeCommand");
            viewModel.ExportDeclarationsDeleteButtonCommand = new CustomDelegateCommand(OnExportDeclarationsDeleteButtonCommand, "ExportDeclarationsDeleteButtonCommand");
            viewModel.NewExportAuthenticationRequestManufactingAreaCommand = new CustomDelegateCommand(OnNewExportAuthenticationRequestManufactingAreaCommand, ModuleCommands.NewExportAuthenticationRequestManufactingAreaCommand);

            CALFacade.Instance.ClientEventAggregator.Subscribe<SendDistributionToCustomerPayload>(OnDistributionSent, ThreadOption.UIThread, true);
            #endregion init commands
        }
        [CommandMethod(true, true)]
        private void OnNewExportAuthenticationRequestManufactingAreaCommand()
        {
            var NewExportAuthenticationRequestManufactingArea = new ExportAuthenticationRequestManufacturingArea()
            {
                ExportAuthenticationRequestID = ExportDocumentAuthenticationRequestViewModel.CurrentEntity.ID
            };
            ExportDocumentAuthenticationRequestViewModel.CurrentEntity.ExportAuthenticationRequestManufacturingArea.Add(NewExportAuthenticationRequestManufactingArea);
        }

        private void OnDistributionSent(SendDistributionToCustomerPayload payload)
        {
            var file = ExportDocumentAuthenticationRequestViewModel.CurrentEntity;
            if (ExportDocumentAuthenticationRequestViewModel.IsRequestDelivery &&
                file.StatusID == (int)EExportAuthenticationRequestStatus.WaitingForLetterSending)
            {
                file.StatusID = (int)EExportAuthenticationRequestStatus.WaitingForExporter;
                file.DeliveryMethodID = (int)EDeliveryMethod.PostedMailing;
                file.LastDeliveryDate = DateTime.Today;
                ExportDocumentAuthenticationRequestViewModel.IsRequestDelivery = false;
                ExportDocumentAuthenticationRequestViewModel.SaveCommand.ExecuteFromModule();
            }
            else
            if (ExportDocumentAuthenticationRequestViewModel.IsRemindDelivery &&
                file.StatusID == (int)EExportAuthenticationRequestStatus.WaitingForExporter)
            {
                file.StatusID = (int)EExportAuthenticationRequestStatus.WaitingForExporterAnswerAfterNotification;
                file.LastDeliveryDate = DateTime.Today;
                if (file.DeliveryMethodID == (int)EDeliveryMethod.PostedMailing ||
                    file.DeliveryMethodID == (int)EDeliveryMethod.SentByEmailRequest)
                {
                    file.DeliveryMethodID = (int)EDeliveryMethod.FirstRemindSent;
                }
                else if (file.DeliveryMethodID == (int)EDeliveryMethod.FirstRemindSent)
                {
                    file.DeliveryMethodID = (int)EDeliveryMethod.SecondRemindSent;
                }

                ExportDocumentAuthenticationRequestViewModel.IsRemindDelivery = false;
                ExportDocumentAuthenticationRequestViewModel.SaveCommand.ExecuteFromModule();
            }

        }


        private void OnSaveAdditionalDocumentsCompletedCommand(DocumentCommandArgs args)
        {
            switch (args.Status)
            {
                case CallbackStatus.Success:
                    if (args.Result != null && args.Result.Count > 0)
                    {
                        if (CurrentEntity.ListOfAdditionalDocumentsIDs == null)
                            CurrentEntity.ListOfAdditionalDocumentsIDs = new List<int>();
                        CurrentEntity.ListOfAdditionalDocumentsIDs.AddRange(args.Result.Select(d => d.ID));
                    }
                    ResolveInstance<ICertificateOfOriginsInternalProxy>()
                        .SaveExportDocumentAuthenticationRequest(OnSaveComplete, CurrentEntity);
                    break;
            }
        }

        private void OnSaveMainDocumentCompletedCommand(DocumentCommandArgs args)
        {
            switch (args.Status)
            {
                case CallbackStatus.Success:
                    if (args.Result != null)
                        CurrentEntity.DocumentID = args.Result.First().ID;
                    break;
            }
        }

        private ExportDocumentAuthenticationRequest CurrentEntity
        {
            get { return ExportDocumentAuthenticationRequestViewModel.CurrentEntity; }
            set { ExportDocumentAuthenticationRequestViewModel.CurrentEntity = value; }
        }

        private void OnRefreshCommand()
        {
            if (CurrentEntity.IsNew) return;
            ExportDocumentAuthenticationRequestViewModel.ProgressBarVisibility = Visibility.Visible;
            ResolveInstance<ICertificateOfOriginsInternalProxy>().GetExportDocumentAuthenticationRequestByID(OnLoadExistingRequestCompleted, CurrentEntity.ID);
        }

        private void OnCancelCommand()
        {
            if (CurrentEntity.IsNew)
            {
                ClientEnvironment.Instance.ClientServices.NavigationEngine.CloseMapping(this);
                return;
            }
            if (ExportDocumentAuthenticationRequestViewModel.CurrentEntityViewState == ViewStates.Dirty)
                ExportDocumentAuthenticationRequestViewModel.ResetVersion();
            ExportDocumentAuthenticationRequestViewModel.CurrentEntityViewState = ViewStates.ReadOnly;
        }

        private bool IsSaveForClose { get; set; }

        private void OnSaveCloseCommand()
        {
            IsSaveForClose = true;
            OnSaveCommand();
        }

        private void OnSaveCommand()
        {
            ExportDocumentAuthenticationRequestViewModel.ProgressBarVisibility = Visibility.Visible;
            if (!CurrentEntity.DocumentID.HasValue)
            {
                MsgBox.ShowError(EMessages.ExportDocumentAuthenticationRequestMainDocumentMandatory);
                ExportDocumentAuthenticationRequestViewModel.ProgressBarVisibility = Visibility.Collapsed;
                IsSaveForClose = false;
                return;
            }
            ExportDocumentAuthenticationRequestViewModel.CurrentEntityViewState = ViewStates.ReadOnly;
            CurrentEntity.Title = "";

            var manufacturingAreaToRemove = new List<ExportAuthenticationRequestManufacturingArea>();
            manufacturingAreaToRemove.AddRange(CurrentEntity.ExportAuthenticationRequestManufacturingArea.Where(ma =>
                String.IsNullOrWhiteSpace(ma.ManufacturingArea) && String.IsNullOrWhiteSpace(ma.ManufacturingZipcode)));
            foreach (var manufacturingArea in manufacturingAreaToRemove)
            {
                CurrentEntity.ExportAuthenticationRequestManufacturingArea.Remove(manufacturingArea);
            }

            if (ExportDocumentAuthenticationRequestViewModel.SaveDocumentsOnSaveEntityCommand != null)
                ExportDocumentAuthenticationRequestViewModel.SaveDocumentsOnSaveEntityCommand.Execute(null);
            else
                ResolveInstance<ICertificateOfOriginsInternalProxy>().SaveExportDocumentAuthenticationRequest(OnSaveComplete, CurrentEntity);
        }

        private void OnSaveComplete(object sender, CallbackEventArgs<ExportDocumentAuthenticationRequest> e)
        {
            ExportDocumentAuthenticationRequestViewModel.ProgressBarVisibility = Visibility.Collapsed;
            switch (e.Status)
            {
                case CallbackStatus.Success:
                    if (IsSaveForClose)
                        ClientEnvironment.Instance.ClientServices.NavigationEngine.CloseMapping(this);
                    else
                        CurrentEntity = e.Result;
                    break;
            }
        }

        private void OnEditCommand()
        {
            ExportDocumentAuthenticationRequestViewModel.CurrentEntityViewState = ViewStates.Edit;
            ExportDocumentAuthenticationRequestViewModel.AddVersion();
        }

        #endregion ViewModel

        #region Callback Operations


        [CallbackMethod]
        private void OnGetCustomerInformationCompleted(object sender, CallbackEventArgs<CustomerDTO> e)
        {
            switch (e.Status)
            {
                case CallbackStatus.Success:
                    if (!e.Result.Addresses.IsNullOrEmpty())
                    {
                        var customerAddressDTO = e.Result.Addresses.FirstOrDefault(a => a.AddressPurprose == (int)EAddressPurpose.Authentication);
                        if (customerAddressDTO != null)
                            CurrentEntity.CustomsHouseAddress = customerAddressDTO.AddressSingleLine;
                        else
                            CurrentEntity.CustomsHouseAddress = e.Result.Addresses.First().AddressSingleLine;
                    }
                    break;
            }

        }
        private void OnGetCustomerInformationCompletedFinally(object sender, CallbackEventArgs<CustomerDTO> e) { }
        private void OnGetCustomerInformationCompletedException(object sender, CallbackEventArgs<CustomerDTO> e, Exception exc) { }

        [CallbackMethod]
        private void OnGetCustomerInformationByCountryCompleted(object sender, CallbackEventArgs<CustomerDTO> e)
        {
            switch (e.Status)
            {
                case CallbackStatus.Success:
                    var customer = e.Result;
                    if (customer != null)
                        CurrentEntity.CustomerID = customer.ID;
                    break;
            }
        }
        private void OnGetCustomerInformationByCountryCompletedFinally(object sender, CallbackEventArgs<CustomerDTO> e) { }
        private void OnGetCustomerInformationByCountryCompletedException(object sender, CallbackEventArgs<CustomerDTO> e, Exception exc) { }


        #endregion Callback Operations

        #region Commands Operations


        [CommandMethod]
        private void OnRemindDeliverNotificationCommand(ExportDocumentAuthenticationRequest file)
        {
            ExportDocumentAuthenticationRequestViewModel.IsRemindDelivery = true;
            // Remind the Exporter
            var templateTypes = new List<ETemplate> { ETemplate.ImportRequestForVerificationEURreminder };

            SendReminderOrRequestNotificationToExporter(file, templateTypes);
        }
        private void OnRemindDeliverNotificationCommandException(ExportDocumentAuthenticationRequest file, Exception exc) { }

        [CommandMethod]
        private void OnRequestDeliverNotificationCommand(ExportDocumentAuthenticationRequest file)
        {
            ExportDocumentAuthenticationRequestViewModel.IsRequestDelivery = true;
            // Notify Exporter
            var templateTypes = new List<ETemplate> { ETemplate.ImportRequestForVerificationEUR };

            SendReminderOrRequestNotificationToExporter(file, templateTypes);
        }
        private void OnRequestDeliverNotificationCommandException(ExportDocumentAuthenticationRequest file, Exception exc) { }

        [CommandMethod]
        private void OnDeliverToCustomsHouseNotificationCommand(ExportDocumentAuthenticationRequest file)
        {
            // Notify Exporter
            var templateTypes = new List<ETemplate> { ETemplate.ExportAuthenticationRequestEurope, ETemplate.RequesrForVerificationMercosur };

            SendReminderOrRequestNotificationToExporter(file, templateTypes, true);
        }
        private void OnDeliverToCustomsHouseNotificationCommandException(ExportDocumentAuthenticationRequest file, Exception exc) { }

        private void SendReminderOrRequestNotificationToExporter(ExportDocumentAuthenticationRequest file, List<ETemplate> templateTypes, bool isSendToCustomsHouse = false)
        {

            var customerId = (isSendToCustomsHouse) ? file.CustomerID : file.ExporterCustomerID ?? 0;
            var customerFilter = new CustomerIdentificationFilter
            {
                CustomerId = customerId,
                AddressFilter = new AddressIdentificationFilter() { AddressPurpose = EAddressPurpose.Authentication }
            };

            var customerDTO = ResolveInstance<ICustomersExternalProxy>().GetCustomerDTOByCustomerIdentificationSync_NotCached(customerFilter);
            var customerAddressPurpose = customerDTO?.Addresses.Select(ad => ad.AddressPurprose).FirstOrDefault(ap => ap != null && ap.Value == (int)EAddressPurpose.Authentication);

            var addressPurpose = customerAddressPurpose.HasValue
                ? EAddressPurpose.Authentication
                : EAddressPurpose.Legal;
            var args = new DistributionToCustomerArguments(addressPurpose, ExportDocumentAuthenticationRequestViewModel.CurrentEntity)
            {
                DefaultCustomerIds = new List<int> { customerId },
                DefaultAvailableTemplates = templateTypes,
                //Disable selecting documents
                IsDeliveryTemplateOrDocRadioVisible = true,
                CurrentEntity = ExportDocumentAuthenticationRequestViewModel.CurrentEntity,
                DistributionID = null,
                AdditionalEntityForSendDocs = new VirtualEntity(ExportDocumentAuthenticationRequestViewModel.CurrentEntity),
                AdditionalDocIds = CurrentEntity.DocumentID.HasValue ?
                new List<int>
                {
                    CurrentEntity.DocumentID.Value
                } :
                null,
                IsOutSeaDest = isSendToCustomsHouse,
            };
            using (var deliveryPresenter = ResolveInstance<IDistributionToCustomerApi>())
            {
                deliveryPresenter.ShowDistrinutionToCustomerView(args, true);
            }
        }

        [CommandMethod]
        private void OnForeignCustomerSelectedChangeCommand()
        {
            ResolveInstance<ICertificateOfOriginsInternalProxy>().GetCustomerInformation(OnGetCustomerInformationCompleted, CurrentEntity.CustomerID);
        }

        private void OnForeignCustomerSelectedChangeCommandException(Exception exc) { }


        [CommandMethod]
        private void OnCountrySelectedChangeCommand()
        {
            if (CurrentEntity.CountryID.HasValue)
            {
                ResolveInstance<ICertificateOfOriginsInternalProxy>()
                    .GetCustomerInformationByCountry(OnGetCustomerInformationByCountryCompleted,
                        CurrentEntity.CountryID.Value);
            }
        }

        private void OnCountrySelectedChangeCommandException(Exception exc) { }

        [CommandMethod(true, true)]
        private void OnExportDeclarationsDeleteButtonCommand()
        {
            ExportDocumentAuthenticationRequestViewModel.SelectedExportDeclaration.MakeValid(true);
            ObjectWithChangeTrackerExtensions.MarkAsDeleted(ExportDocumentAuthenticationRequestViewModel
                .SelectedExportDeclaration);
            CurrentEntity.ExportDocumentAuthenticationRequestLeadDocument.Remove(
                ExportDocumentAuthenticationRequestViewModel.SelectedExportDeclaration);
            ExportDocumentAuthenticationRequestViewModel.SelectedExportDeclaration =
                CurrentEntity.ExportDocumentAuthenticationRequestLeadDocument.LastOrDefault();
        }

        #endregion Commands Operations

        #region Load Operations

        /// <summary>
        /// Loads the data.
        /// </summary>
        /// <param name="navObj">The navigation object.</param>
        /// <param name="arguments">The arguments.</param>
        public override void LoadData(INavigationObject navObj, params object[] arguments)
        {
            base.LoadData(navObj, arguments);

            if (navObj.EntityInstance.IsNewInstance())
            {
                InitNewExportDocumentAuthenticationRequest();
                ExportDocumentAuthenticationRequestViewModel.CurrentEntityViewState = ViewStates.Edit;
                ExportDocumentAuthenticationRequestViewModel.ProgressBarVisibility = Visibility.Collapsed;
                OnDataTransferCallback(new DataTransferCallbackEventArgs(null, DataTransferCallbackType.LoadData));
            }
            else if (navObj.EntityId != 0)
            {
                LoadExistingRequest(navObj.EntityId);
            }

        }

        #endregion Load Operations

        #region inits

        private void InitNewExportDocumentAuthenticationRequest()
        {
            ExportDocumentAuthenticationRequestViewModel.CurrentEntity = new ExportDocumentAuthenticationRequest();
            var currentEntity = ExportDocumentAuthenticationRequestViewModel.CurrentEntity;
            currentEntity.IsNew = true;
            currentEntity.AuthenticationRequestArrivalDate = DateTime.Now;
            currentEntity.CreateUserID = ClientEnvironment.Instance.ServerServices.UserManagementService.
                                                           CurrentUser.UserID;
            currentEntity.UpdateUserID = ClientEnvironment.Instance.ServerServices.UserManagementService.
                                                           CurrentUser.UserID;
            currentEntity.OrganizationUnitID =
                ClientEnvironment.Instance.ServerServices.UserManagementService.CurrentUser.OrganizationUnitID;
            currentEntity.CreateDate = DateTime.Now;
            currentEntity.UpdateDate = DateTime.Now;
            currentEntity.StatusID = (int)EExportAuthenticationRequestStatus.WaitingForLetterSending;
            currentEntity.AuthenticationRequestNotes = "";
            currentEntity.TypeID = 0;
            currentEntity.State = (int)EState.Enabled;
            currentEntity.DeliveryMethodID = 1;
            currentEntity.DocumentID = null;
            currentEntity.ExportDocumentAuthenticationRequestLeadDocument = new Customs.CertificateOfOrigins.Entities.TrackableCollection<ExportDocumentAuthenticationRequestLeadDocument>();
            currentEntity.CustomsItemToExportDocumentAuthenticationRequest = new Customs.CertificateOfOrigins.Entities.TrackableCollection<CustomsItemToExportDocumentAuthenticationRequest>();
        }

        #endregion inits

        #region loads

        private void LoadExistingRequest(int id)
        {
            ResolveInstance<ICertificateOfOriginsInternalProxy>()
                .GetExportDocumentAuthenticationRequestByID(OnLoadExistingRequestCompleted, id);
        }

        private void OnLoadExistingRequestCompleted(object sender, CallbackEventArgs<ExportDocumentAuthenticationRequest> e)
        {
            ExportDocumentAuthenticationRequestViewModel.CurrentEntityViewState = ViewStates.ReadOnly;
            switch (e.Status)
            {
                case CallbackStatus.Success:
                    CurrentEntity = e.Result;
                    CurrentEntity.ResetChanges();

                    ExportDocumentAuthenticationRequestViewModel.IsRequestSent =
                        ExportDocumentAuthenticationRequestViewModel.CurrentEntity.DeliveryMethodID == null
                        || ExportDocumentAuthenticationRequestViewModel.CurrentEntity.DeliveryMethodID == (int)EDeliveryMethod.WasNotSend;


                    if (CurrentEntity.StatusID != null)
                    {
                        CurrentEntity.OriginalStatusID = (int)CurrentEntity.StatusID;
                    }
                    break;
            }
            ExportDocumentAuthenticationRequestViewModel.ProgressBarVisibility = Visibility.Collapsed;
        }

        #endregion loads

        protected override bool Dispose(bool disposing)
        {
            if (!disposing) return false;
            CALFacade.Instance.ClientEventAggregator.Unsubscribe<SendDistributionToCustomerPayload>(OnDistributionSent);
            return base.Dispose(true);
        }

    }
}
