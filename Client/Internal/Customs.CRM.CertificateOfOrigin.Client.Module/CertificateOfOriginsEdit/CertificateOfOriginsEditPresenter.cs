using System;
using System.Windows;
using Customs.CertificateOfOrigins.Entities;
using Customs.CRM.CertificateOfOrigins.Client.Api;
using Customs.CRM.CertificateOfOrigins.Client.Api.CertificateOfOriginsEdit;
using Customs.CRM.CertificateOfOrigins.InternalProxy;
using Customs.CRM.Entities;
using Customs.CRM.Entities.CertificateOfOriginsPartial;
using Customs.Inf.MMI.Common.Aspects;
using Customs.Inf.MMI.Common.CAL;
using Customs.Inf.MMI.Common.Core;
using Customs.Inf.MMI.Common.Extensions;
using Customs.Inf.MMI.Common.Navigation;
using Customs.Inf.MMI.Common.Toolbox.CustomMessageBox;
using Customs.Inf.MMI.Services.Module.ClientManager;
using Customs.Infrastructure.DocumentManagement.ExternalCommon;
using Customs.Infrastructure.Entities.Enums;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.Communication;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.Extensions;
using MalamTeam.Infrastructure.GeneralServices.Environment.Enums;

namespace Customs.CRM.CertificateOfOrigins.Client.Module.CertificateOfOriginsEdit
{
    /// <summary>
    /// CertificateOfOriginsEditPresenter
    /// </summary>
    public class CertificateOfOriginsEditPresenter : PresenterBase, ICertificateOfOriginsEditPresenter
    {
        bool _isCloseAfterSave;

        /// <summary>
        /// Gets the view model object.
        /// </summary>
        /// <value>The current view model object.</value>
        public CertificateOfOriginsEditViewModel CertificateOfOriginsEditViewModel
        {
            get
            {
                IViewModel viewModel;
                ViewModels.TryGetValue("CertificateOfOriginsEditViewModel", out viewModel);
                return viewModel as CertificateOfOriginsEditViewModel;
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

			var viewModel = new CertificateOfOriginsEditViewModel { Name = "CertificateOfOriginsEditViewModel" };
			ViewModels.Add("CertificateOfOriginsEditViewModel", viewModel);
			

			#region Register Commands

            viewModel.Commands.RegisterCommand<bool>(OnSaveCertificateOfOriginsCommand, ModuleCommands.SaveCertificateOfOriginsCommand);
            viewModel.Commands.RegisterCommand<CertificateOfOrigin>(OnEditCertificateOfOriginsCommandSuccessful, ModuleCommands.EditCertificateOfOriginsCommand,
                    OnEditCertificateOfOriginsCommandRequested, OnEditCertificateOfOriginsCommandFailed);
            viewModel.Commands.RegisterCommand(OnCancelCertificateOfOriginsCommand, ModuleCommands.CancelCertificateOfOriginsCommand);
            viewModel.Commands.RegisterCommand(OnRefreshCertificateOfOriginsCommand, ModuleCommands.RefreshCertificateOfOriginsCommand);
            viewModel.Commands.RegisterCommand<DocumentCommandArgs>(OnDocumentsSaveCompleted, "CustomerSaveDocumentsCommand"); //TODO: command
            viewModel.Commands.RegisterCommand(OnCancelCertificateOfOriginsByUserCommand, "CancelCertificateOfOriginsByUserCommand"); //TODO: command

			#endregion Register Commands
		}

        #endregion ViewModel

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

            ResolveInstance<ICertificateOfOriginsInternalProxy>().GetCertificateOfOriginById(OnGetCertificateOfOriginByIdCompleted, navObj.EntityId);

        }

        #endregion Load Operations

        #region Commands Operations

        [CommandMethod]
        private void OnSaveCertificateOfOriginsCommand(bool param)
        {
            _isCloseAfterSave = param;
            if(CertificateOfOriginsEditViewModel.CertificateOfOriginStatusIDPrev!=CertificateOfOriginsEditViewModel.CurrentEntity.CertificateOfOriginStatusID
                && CertificateOfOriginsEditViewModel.CurrentEntity.CertificateOfOriginStatusID == (int)ECertificateOfOriginStatus.Published)

                //todo - delete condition(CertificateOfOriginsEditViewModel.CurrentEntity.ExportDeclarationNumber == null) when Export
                if (CertificateOfOriginsEditViewModel.CurrentEntity.LeadDocumentID == null &&
                    CertificateOfOriginsEditViewModel.CurrentEntity.ExportDeclarationNumber == null)
                {
                    MsgBox.ShowWarning(EMessages.CertificateWithoutLeadDocument);
                }
                else if (CertificateOfOriginsEditViewModel.CurrentEntity.IsDeclarationReleased.HasValue
                         && !CertificateOfOriginsEditViewModel.CurrentEntity.IsDeclarationReleased.Value)
                {
                    MsgBox.ShowWarning(EMessages.DeclarationReleased);
                }

            CertificateOfOriginsEditViewModel.ProgressBarVisibility = Visibility.Visible;

            if (CertificateOfOriginsEditViewModel.SaveTheDocumentsCommand != null)
            {
                CertificateOfOriginsEditViewModel.SaveTheDocumentsCommand.Execute(null);
            }
            else
            {
                SaveCertificateOfOrigin();
            }           
        }
        private void OnSaveCertificateOfOriginsCommandException(bool param, Exception exc)
        {
            CertificateOfOriginsEditViewModel.ProgressBarVisibility = Visibility.Collapsed;
        }

        [CommandMethod]
        private void OnEditCertificateOfOriginsCommandSuccessful(CertificateOfOrigin entity)
        {            
            CertificateOfOriginsEditViewModel.IsEditButtonEnabled = false;
            CertificateOfOriginsEditViewModel.EditViewState = ViewStates.Edit;
            CertificateOfOriginsEditViewModel.CertificateOfOriginStatusIDPrev = CertificateOfOriginsEditViewModel.CurrentEntity.CertificateOfOriginStatusID;
                AddVersion();
        }
        private void OnEditCertificateOfOriginsCommandSuccessfulException(CertificateOfOrigin entity, Exception exc){}
        private void OnEditCertificateOfOriginsCommandRequested(CertificateOfOrigin entity){}
        private void OnEditCertificateOfOriginsCommandFailed(CertificateOfOrigin entity){}

        [CommandMethod]
        private void OnCancelCertificateOfOriginsCommand()
        {
            ResetVersion();
            CertificateOfOriginsEditViewModel.IsEditButtonEnabled = true;
            CertificateOfOriginsEditViewModel.EditViewState = ViewStates.ReadOnly;
        }
        private void OnCancelCertificateOfOriginsCommandException(Exception exc) {}

        [CommandMethod]
        private void OnRefreshCertificateOfOriginsCommand()
        {
            CertificateOfOriginsEditViewModel.ProgressBarVisibility = Visibility.Visible;
            ResolveInstance<ICertificateOfOriginsInternalProxy>().GetCertificateOfOriginById(OnGetCertificateOfOriginByIdCompleted, CertificateOfOriginsEditViewModel.CurrentEntity.ID);
        }
        private void OnRefreshCertificateOfOriginsCommandException(Exception exc)
        {
            CertificateOfOriginsEditViewModel.ProgressBarVisibility = Visibility.Collapsed;
        }

        private void OnCancelCertificateOfOriginsByUserCommand()
        {
           
            CertificateOfOriginsEditViewModel.CurrentEntity.RejectCancelReason = MsgBox.ShowInput(EMessages.CancelReason, maxLength: 30);
            if (!string.IsNullOrEmpty(CertificateOfOriginsEditViewModel.CurrentEntity.RejectCancelReason))
            {
                CertificateOfOriginsEditViewModel.ProgressBarVisibility = Visibility.Visible;
                CertificateOfOriginsEditViewModel.CurrentEntity.CertificateOfOriginStatusID = (int)ECertificateOfOriginStatus.Cancelled;
                SaveCertificateOfOrigin();
            }
            
        }

        #endregion Commands Operations

        #region Callback Operations

        [CallbackMethod]
        public void OnGetCertificateOfOriginByIdCompleted(object sender, CallbackEventArgs<CertificateOfOrigin> e)
        {
            switch (e.Status)
            {
                case CallbackStatus.Success:

                    ResolveInstance<ICertificateOfOriginsInternalProxy>().LoadDataFromExportDeclaration(OnLoadDataFromExportDeclarationCompleted, e.Result);
                    
                    ArgumentValidator.AssertNotNull(e.Result, "e.Result", true, EMessages.CertificateNumberNotFound, e.StateObject);

                    foreach (var certificateOfOriginDetail in e.Result.CertificateOfOriginDetails)
                    {
                        FillCorrectValue(certificateOfOriginDetail);
                    }

                    CertificateOfOriginsEditViewModel.CurrentEntity = e.Result;
                    NavigationObject.EntityInstance = e.Result;

                    InitConnectedEntities();

                    if (CertificateOfOriginsEditViewModel.CurrentEntity.CertificateOfOriginStatusID == (int)ECertificateOfOriginStatus.Cancelled ||
                        CertificateOfOriginsEditViewModel.CurrentEntity.RequestReasonCode == (int)ERequestReason.Draft ||
                        CertificateOfOriginsEditViewModel.CurrentEntity.RequestReasonCode == (int)ERequestReason.EmptyCertificate ||
                        CertificateOfOriginsEditViewModel.CurrentEntity.RequestReasonCode == (int)ERequestReason.CertificateCancellation)
                    {
                        CertificateOfOriginsEditViewModel.IsEditButtonEnabled = false;
                        CertificateOfOriginsEditViewModel.EditViewState = ViewStates.ReadOnly;
                    }

                    break;
            }
        }

        private void OnLoadDataFromExportDeclarationCompleted(object sender, CallbackEventArgs<bool> e)
        {
            if (CertificateOfOriginsEditViewModel.CurrentEntity != null)
            {
                CertificateOfOriginsEditViewModel.CurrentEntity.IsDeclarationReleasedAndNotRetrospectiveCertificate = e.Result;
            }
        }

        private void InitConnectedEntities()
        {
            var currentEntity = CertificateOfOriginsEditViewModel.CurrentEntity;
            CertificateOfOriginsEditViewModel.ConnectedEntities = new CustomObservableCollection<INavigationMapping>();

            //TODO: replace CreateCustomerID with connectedEntities
            //if (currentEntity.CreateCustomerID != null)
            //{
                //var navMapping = new NavigationMappingBase
                //{
                //    //NavigationArguments = new object[] { EEntityType.CargoVersion, currentEntity.Cargo.LastCargoVersionID },
                //    EntityId = currentEntity.CreateCustomerID,
                //    EntityType = EEntityType.Customer,
                //    MappingType = NavigationMappingType.Edit,
                //    //Title = string.Format("{0} {1}", Strings.GetString(LocalStrings.Cargo), currentEntity.Cargo.CargoIdentifierKey2)
                //    Title="Customer 0 לקוח",
                //    ToolTip = "לקוח"
                //};

                //CertificateOfOriginsEditViewModel.ConnectedEntities.Add(navMapping);
            //}
        }

        private void OnGetCertificateOfOriginByIdCompletedFinally(object sender, CallbackEventArgs<CertificateOfOrigin> e)
        {
            CertificateOfOriginsEditViewModel.EditViewState = ViewStates.ReadOnly;
            CertificateOfOriginsEditViewModel.ProgressBarVisibility = Visibility.Collapsed;
        }
        private void OnGetCertificateOfOriginByIdCompletedException(object sender, CallbackEventArgs<CertificateOfOrigin> e, Exception exc){}


        [CallbackMethod]
        private void OnSaveCertificateOfOriginsCompleted(object sender, CallbackEventArgs<CertificateOfOrigin> e)
        {
            switch (e.Status)
            {
                case CallbackStatus.Success:
                    var certificateOfOrigin = e.Result;
                    foreach (var detail in certificateOfOrigin.CertificateOfOriginDetails)
                    {
                        FillCorrectValue(detail);
                    }
                    CertificateOfOriginsEditViewModel.CurrentEntity = certificateOfOrigin;
                    NavigationObject.EntityInstance = CertificateOfOriginsEditViewModel.CurrentEntity;
                    CertificateOfOriginsEditViewModel.EditViewState = ViewStates.ReadOnly;

                    if (_isCloseAfterSave)
                    {
                        ClientEnvironment.Instance.ClientServices.NavigationEngine.CloseMapping(this);
                    }
                    break;
            }
        }
        private void OnSaveCertificateOfOriginsCompletedFinally(object sender, CallbackEventArgs<CertificateOfOrigin> e)
        {
            if (_isCloseAfterSave) return;
            CertificateOfOriginsEditViewModel.IsEditButtonEnabled = true;
            CertificateOfOriginsEditViewModel.ProgressBarVisibility = Visibility.Collapsed;

            if (CertificateOfOriginsEditViewModel.CurrentEntity.CertificateOfOriginStatusID == (int)ECertificateOfOriginStatus.Cancelled)
            {
                CertificateOfOriginsEditViewModel.IsEditButtonEnabled = false;
                CertificateOfOriginsEditViewModel.EditViewState = ViewStates.ReadOnly;
            }
            
        }
        private void OnSaveCertificateOfOriginsCompletedException(object sender, CallbackEventArgs<CertificateOfOrigin> e, Exception exc)
        {
            CertificateOfOriginsEditViewModel.ProgressBarVisibility = Visibility.Collapsed;
        }


        [CallbackMethod]
        private void OnDocumentsSaveCompleted(DocumentCommandArgs args)
        {
            if (args == null || args.Status == CallbackStatus.Success)
            {
                SaveCertificateOfOrigin();
            }
        }
        private void OnDocumentsSaveCompletedException(DocumentCommandArgs args, Exception exc)
        {

        }
        private void OnDocumentsSaveCompletedFinally(DocumentCommandArgs args)
        {
            if (args != null && args.Status == CallbackStatus.Fail)
            {
                CertificateOfOriginsEditViewModel.ProgressBarVisibility = Visibility.Collapsed;
            }
        }


        #endregion Callback Operations


        #region Private Methods

        private void AddVersion()
        {
            CertificateOfOriginsEditViewModel.AddVersion(CertificateOfOriginsConstants.CurrentEntity, CertificateOfOriginsEditViewModel.CurrentEntity);
        }
        private void ResetVersion()
        {
            CertificateOfOriginsEditViewModel.ResetValue(CertificateOfOriginsConstants.CurrentEntity);
            foreach (var detail in CertificateOfOriginsEditViewModel.CurrentEntity.CertificateOfOriginDetails)
            {
                FillCorrectValue(detail);
            }
            CertificateOfOriginsEditViewModel.RaiseCheckManagementPropertyChanged(CertificateOfOriginsConstants.CurrentEntity);
        }

        private void SaveCertificateOfOrigin()
        {
            var certificate = CertificateOfOriginsEditViewModel.CurrentEntity;
            ResolveInstance<ICertificateOfOriginsInternalProxy>().SaveCertificateOfOrigin(OnSaveCertificateOfOriginsCompleted, certificate);
        }

        private void FillCorrectValue(CertificateOfOriginDetails certificateOfOriginDetail)
        {
            switch (certificateOfOriginDetail.CertificateDetailsTypeCode.DataTypeID)
            {
                case (int)EDataType.ComplexType:
                case (int)EDataType.Int:
                    int intVal;
                    if (int.TryParse(certificateOfOriginDetail.Value, out intVal))
                        certificateOfOriginDetail.ValueInt = intVal;
                    break;
                case (int)EDataType.String:
                    certificateOfOriginDetail.Valuestring = certificateOfOriginDetail.Value;
                    break;
                case (int)EDataType.DateTime:
                    DateTime date;
                    if (DateTime.TryParse(certificateOfOriginDetail.Value, out date))
                        certificateOfOriginDetail.ValueDate = date;
                    break;
                case (int)EDataType.Bool:
                    bool boolVal;
                    if (bool.TryParse(certificateOfOriginDetail.Value, out boolVal))
                        certificateOfOriginDetail.ValueBit = boolVal;
                    break;
            }
        }

        #endregion Private Methods
    }
}
