using System;
using System.Collections.Generic;
using System.Windows;
using Customs.CertificateOfOrigins.Entities;
using Customs.CRM.CertificateOfOrigins.InternalProxy;
using Customs.CRM.CertificateOfOrigion.Client.External.Api;
using Customs.CRM.Entities.CertificateOfOriginsDTO;
using Customs.Inf.MMI.Common.Aspects;
using Customs.Inf.MMI.Common.CAL;
using Customs.Inf.MMI.Common.Common;
using Customs.Inf.MMI.Common.Extensions;
using Customs.Inf.MMI.Common.Navigation;
using Customs.Infrastructure.DocumentManagement.ExternalCommon;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.Communication;

namespace Customs.CRM.CertificateOfOrigins.Client.Module.AuthenticationRequest.DealFileAuthenticationRequest
{
    /// <summary>
    /// DealFileAuthenticationRequestPresenterEditPresenter
    /// </summary>
    public class DealFileAuthenticationRequestPresenterEditPresenter : PresenterBase, IImportAuthenticationRequestExternalPresenter
    {
        /// <summary>
        /// Gets the view model object.
        /// </summary>
        /// <value>The current view model object.</value>
        public IImportAuthenticationRequestExternalViewModel DealFileAuthenticationRequestViewModelEditViewModel
        {
            get
            {
                IViewModel viewModel;
                ViewModels.TryGetValue("DealFileAuthenticationRequestViewModelEditViewModel", out viewModel);
                return viewModel as IImportAuthenticationRequestExternalViewModel;
            }
        }

        DealFileAuthenticationRequestViewModelEditViewModel ViewModel;

        #region ViewModel

        /// <summary>
        /// Initializes the view models.<para/>
        /// (called before "InternalInitViews")
        /// </summary>
        protected override void InternalInitViewModels()
        {
            base.InternalInitViewModels();

            var viewModel = new DealFileAuthenticationRequestViewModelEditViewModel { Name = "DealFileAuthenticationRequestViewModelEditViewModel" };
            ViewModels.Add("DealFileAuthenticationRequestViewModelEditViewModel", viewModel);

            ViewModel = viewModel;

            #region Register Commands

            viewModel.DealFileImportAuthenticationRequestRefreshCommand = new CustomDelegateCommand(OnDealFileImportAuthenticationRequestRefreshCommand, "DealFileImportAuthenticationRequestRefreshCommand");
            viewModel.GetAuthenticationRequestByLeadDocumentIDCommand = new CustomDelegateCommand(OnGetAuthenticationRequestByLeadDocumentIDCommand, "GetAuthenticationRequestByLeadDocumentIDCommand");
            viewModel.NavigateToAuthenticationRequest = new CustomDelegateCommand<int>(OnNavigateToAuthenticationRequest, "NavigateToAuthenticationRequest");


            #endregion Register Commands

        }


        [CommandMethod]
        private void OnNavigateToAuthenticationRequest(int authenticationRequestID)
        {
            ViewModel.ProgressBarVisibility = Visibility.Visible;    
            ResolveInstance<ICertificateOfOriginsInternalProxy>().
                GetAuthenticationRequestByID(OnGetAuthenticationRequestByIDCompleted, authenticationRequestID);
        }

        

        private void OnNavigateToAuthenticationRequestException(int authenticationRequestID, Exception ex)
        {
            ViewModel.ProgressBarVisibility = Visibility.Collapsed;
        }

        private void OnNavigateToAuthenticationRequestFinally(int authenticationRequestID)
        {
            ViewModel.ProgressBarVisibility = Visibility.Collapsed;
        }

        #endregion ViewModel

        #region Callback Operations

        [CallbackMethod]
        private void OnGetAuthenticationRequestByLeadDocumentIDCompleted(object sender, CallbackEventArgs<List<ImportAuthenticationRequestDTO>> e)
        {
            switch (e.Status)
            {
                case CallbackStatus.Success:
                    if (e.Result.IsNullOrEmpty()) return;
                    ViewModel.AuthenticationRequestForLeadDocument = e.Result;
                    break;

                case CallbackStatus.Fail:
                    break;
            }
        }  
        private void OnGetAuthenticationRequestByLeadDocumentIDCompletedException(object sender, CallbackEventArgs<List<ImportAuthenticationRequestDTO>> e, Exception ex)
        {
            ViewModel.ProgressBarVisibility = Visibility.Collapsed;

        }
        private void OnGetAuthenticationRequestByLeadDocumentIDCompletedFinally(object sender, CallbackEventArgs<List<ImportAuthenticationRequestDTO>> e)
        {
            ViewModel.ProgressBarVisibility = Visibility.Collapsed;
        }

        [CallbackMethod]
        private void OnGetAuthenticationRequestByIDCompleted(object sender, CallbackEventArgs<CertificateOfOriginsImportAuthenticationRequest> e)
        {
            switch (e.Status)
            {
                case CallbackStatus.Success:
                    DealFileAuthenticationRequestViewModelEditViewModel.ProgressBarVisibility = Visibility.Collapsed;
                    var request = e.Result;
                    if (request == null)
                        return;

                    var proxy = CALFacade.Instance.ClientUnityContainer
                        .Resolve(typeof(Api.AuthenticationRequest.ImportProcessForm.IImportProcessFormPresenter),
                        null) as Api.AuthenticationRequest.ImportProcessForm.IImportProcessFormPresenter;
                    proxy.ShowImportProcessPopUp(request);


                    break;
            }
        }

        private void OnGetAuthenticationRequestByIDCompletedException(object sender, CallbackEventArgs<CertificateOfOriginsImportAuthenticationRequest> e, Exception ex)
        {
            DealFileAuthenticationRequestViewModelEditViewModel.ProgressBarVisibility = Visibility.Collapsed;
        }

        private void OnGetAuthenticationRequestByIDCompletedFinally(object sender, CallbackEventArgs<CertificateOfOriginsImportAuthenticationRequest> e)
        {
            DealFileAuthenticationRequestViewModelEditViewModel.ProgressBarVisibility = Visibility.Collapsed;
        }

        #endregion Callback Operations

        #region Commands Operations


        private void OnDealFileImportAuthenticationRequestRefreshCommand()
        {
            GetAuthenticationRequestByLeadDocumentID(ViewModel.LeadDocumentIds);
        }
        public void OnGetAuthenticationRequestByLeadDocumentIDCommand()
        {
            GetAuthenticationRequestByLeadDocumentID(ViewModel.LeadDocumentIds);
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
            //OnDataTransferCallback(new DataTransferCallbackEventArgs(null, DataTransferCallbackType.LoadData));
        }

        public void GetAuthenticationRequestByLeadDocumentID(List<int> leadDocumentIDs)
        {
            ViewModel.ProgressBarVisibility = Visibility.Visible;
            ResolveInstance<ICertificateOfOriginsInternalProxy>().GetAuthenticationRequestByLeadDocumentIDs(OnGetAuthenticationRequestByLeadDocumentIDCompleted, leadDocumentIDs);
        }
        

        #endregion Load Operations
    }
}
