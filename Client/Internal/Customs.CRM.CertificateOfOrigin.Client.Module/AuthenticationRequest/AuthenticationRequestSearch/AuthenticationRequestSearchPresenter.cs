using Customs.CertificateOfOrigins.Entities;
using Customs.CRM.CertificateOfOrigins.Client.Api;
using Customs.CRM.CertificateOfOrigins.Client.Api.AuthenticationRequest.AuthenticationRequestSearch;
using Customs.CRM.CertificateOfOrigins.InternalProxy;
using Customs.CRM.Entities;
using Customs.CRM.Entities.CertificateOfOriginsPartial;
using Customs.Inf.MMI.Common.Aspects;
using Customs.Inf.MMI.Common.CAL;
using Customs.Inf.MMI.Common.Common;
using Customs.Inf.MMI.Common.Core;
using Customs.Inf.MMI.Common.Extensions;
using Customs.Inf.MMI.Common.Navigation;
using Customs.Inf.MMI.Common.Toolbox.CustomEventArgs;
using Customs.Inf.MMI.Common.Toolbox.CustomMessageBox;
using Customs.Inf.MMI.Services.Module.ClientManager;
using Customs.Inf.MMI.Services.Module.Search;
using Customs.Infrastructure.UserManagement.ExternalProxy;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.BaseClasses;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.Communication;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.Extensions;
using MalamTeam.Infrastructure.GeneralServices.Environment.Enums;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Customs.Inf.MMI.Services.Api.Search;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.Base_Entities;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.Exceptions;

namespace Customs.CRM.CertificateOfOrigins.Client.Module.AuthenticationRequest.AuthenticationRequestSearch
{
    /// <summary>
    /// AuthenticationRequestSearchPresenter
    /// </summary>
    public class AuthenticationRequestSearchPresenter : SearchConsumerBase, IAuthenticationRequestSearchPresenter
    {
        #region ViewModel

        public AuthenticationRequestSearchViewModel AuthenticationRequestSearchViewModel
        {
            get
            {
                IViewModel viewModel;
                ViewModels.TryGetValue(CertificateOfOriginsConstants.AuthenticationRequestSearchViewModel, out viewModel);
                return viewModel as AuthenticationRequestSearchViewModel;
            }
        }

        protected override void InternalInitViewModels()
        {
            base.InternalInitViewModels();

            var viewModel = new AuthenticationRequestSearchViewModel { Name = CertificateOfOriginsConstants.AuthenticationRequestSearchViewModel };

            #region Register Commands

            viewModel.Commands.RegisterCommand<ImportAuthenticationRequestFilter>(OnSearchAuthenticationRequestCommand, ModuleCommands.SearchAuthenticationRequestCommand);
            viewModel.Commands.RegisterCommand(OnAuthenticationRequestClearCommand, ModuleCommands.AuthenticationRequestClearCommand);
            viewModel.Commands.RegisterCommand<CustomDataGridEventArgs>(OnAuthenticationRequestRowDoubleClickCommand, ModuleCommands.AuthenticationRequestRowDoubleClickCommand);
            viewModel.Commands.RegisterCommand<GetImportAuthenticationRequestResult>(OnCheckedAuthenticationRequestCommand, ModuleCommands.CheckedAuthenticationRequestCommand);
            viewModel.Commands.RegisterCommand(OnMergeRequestsCommand, ModuleCommands.MergeRequestsCommand);
            viewModel.Commands.RegisterCommand<int>(OnOpenImportAuthenticationRequestCommand, ModuleCommands.OpenImportAuthenticationRequestCommand);

            #endregion Register Commands

            ViewModels.Add(CertificateOfOriginsConstants.AuthenticationRequestSearchViewModel, viewModel);
        }

        #endregion ViewModel

        public override string IsEnabledInVersionConfigParamName => "IsCertificateOfOriginsVisible";

        #region Commands Operations 

        [CommandMethod]
        private void OnSearchAuthenticationRequestCommand(ImportAuthenticationRequestFilter filter)
        {
            if (filter == null) return;
            filter.FromRequestDate = filter.FileCreateDates.FromDate;
            filter.ToRequestDate = filter.FileCreateDates.ToDate;
            AuthenticationRequestSearchViewModel.ProgressBarPanelVisibile = Visibility.Visible;
            AuthenticationRequestSearchViewModel.ChekedList = new List<GetImportAuthenticationRequestResult>();
            var searchResultCollection = AuthenticationRequestSearchViewModel.SearchResult;
            searchResultCollection?.Clear();

            ResolveInstance<ICertificateOfOriginsInternalProxy>().GetAuthenticationRequestByFilter(OnGetAuthenticationRequestCompleted, filter);
        }
        private void OnSearchAuthenticationRequestCommandException(ImportAuthenticationRequestFilter filter, Exception exc)
        {
            AuthenticationRequestSearchViewModel.ProgressBarPanelVisibile = Visibility.Collapsed;
        }


        [CommandMethod(true, false)]
        private void OnAuthenticationRequestClearCommand()
        {
            AuthenticationRequestSearchViewModel.CurrentFilter = new ImportAuthenticationRequestFilter();
            AuthenticationRequestSearchViewModel.CurrentFilter.FileCreateDates.FromDate = DateTime.Now.AddMonths(AuthenticationRequestSearchViewModel.NumberOfMonthToSearchImportAuthenticationRequest * -1);
            AuthenticationRequestSearchViewModel.CurrentFilter.FileCreateDates.ToDate = DateTime.Now;
        }
        private void OnAuthenticationRequestClearCommandException(Exception exc) { }


        [CommandMethod]
        private void OnAuthenticationRequestRowDoubleClickCommand(CustomDataGridEventArgs args)
        {
            var authenticationRequest = ArgumentValidator.AssertNotNullAndOfType<GetImportAuthenticationRequestResult>(args.SelectedItem, "args.SelectedItem");
            if (authenticationRequest != null)
            {
                AuthenticationRequestSearchViewModel.ProgressBarPanelVisibile = Visibility.Visible;
                if (authenticationRequest.DocumentID != null)
                    ClientEnvironment.Instance.ClientServices.ProtocolManager.ShowPage(EEntityType.ImportAuthenticationRequest, (int)authenticationRequest.DocumentID, NavigationMappingType.Edit);
                AuthenticationRequestSearchViewModel.ProgressBarPanelVisibile = Visibility.Collapsed;
            }
        }
        private void OnAuthenticationRequestRowDoubleClickCommandException(CustomDataGridEventArgs args, Exception exc)
        {
            AuthenticationRequestSearchViewModel.ProgressBarPanelVisibile = Visibility.Collapsed;
        }

        [CommandMethod]
        private void OnCheckedAuthenticationRequestCommand(GetImportAuthenticationRequestResult importAuthenticationRequestResult)
        {
            if (importAuthenticationRequestResult.IsCheck)
            {
                AuthenticationRequestSearchViewModel.ChekedList.Add(importAuthenticationRequestResult);
            }
            else
            {
                if (AuthenticationRequestSearchViewModel.ChekedList.Contains(importAuthenticationRequestResult))
                {
                    AuthenticationRequestSearchViewModel.ChekedList.Remove(importAuthenticationRequestResult);
                }

            }
        }
        private void OnCheckedAuthenticationRequestCommandException(GetImportAuthenticationRequestResult importAuthenticationRequestResult, Exception exc) { }

        [CommandMethod]
        private void OnMergeRequestsCommand()
        {

            var isForMerge = CheckMergerRequests();
            if (isForMerge == null)
            {
                return;
            }
            if (isForMerge == true)
            {
                ResolveInstance<ICertificateOfOriginsInternalProxy>().CreateNewAuthenticationFile(OnCreateNewAuthenticationFileCompleted, AuthenticationRequestSearchViewModel.ChekedList);
                AuthenticationRequestSearchViewModel.ChekedList = new List<GetImportAuthenticationRequestResult>();
            }
            else
            {
                MsgBox.ShowMessage(EMessages.UnableToMergeRequests, "", CustomMessageBoxStandardIcon.Error);
            }
        }
        private void OnMergeRequestsCommandException(Exception exc) { }

        [CommandMethod]
        private void OnOpenImportAuthenticationRequestCommand(int documentID)
        {
            AuthenticationRequestSearchViewModel.ProgressBarPanelVisibile = Visibility.Visible;
            ResolveInstance<ICertificateOfOriginsInternalProxy>().GetAuthenticationRequestByID(OnGetAuthenticationRequestByIDCompleted, documentID);
        }

        private void OnOpenImportAuthenticationRequestCommandException(int documentID, Exception exc) { }

        public override string OnSearchEntityCommandExecuted(string searchQuery, ISearchConsumerItem selectedItem)
        {
            if (!int.TryParse(searchQuery, out var authenticationRequestByID))
            {
                OnSearchEntityCallback(new SearchEntityCallbackEventArgs(null, null));
                return null;
            }
            return ResolveInstance<ICertificateOfOriginsInternalProxy>().GetAuthenticationRequestByID(OnGetEntityCompleted, authenticationRequestByID);
        }
        public override string OnSearchEntityCommandExecuted(IEntity entity, ISearchConsumerItem selectedItem)
        {
            return ResolveInstance<ICertificateOfOriginsInternalProxy>().GetAuthenticationRequestByID(OnGetEntityCompleted, entity.ID);
        }
        public override void OnSearchCommandExecuted(string searchQuery, ISearchConsumerItem selectedItem)
        {

            if (!int.TryParse(searchQuery, out var authenticationRequestSearchPresenter))
            {

                throw new InfException(EMessages.InvalidText);
            }
            ResolveInstance<ICertificateOfOriginsInternalProxy>().GetAuthenticationRequestByID(OnQuickGetCertificateRequestCompleted, authenticationRequestSearchPresenter);
        }


        #endregion Commands Operations

        #region Callback Operations

        [CallbackMethod]
        public void OnGetAuthenticationRequestCompleted(object obj, CallbackEventArgs<List<GetImportAuthenticationRequestResult>> e)
        {
            switch (e.Status)
            {
                case CallbackStatus.Success:

                    if (!AuthenticationRequestSearchViewModel.SearchResult.IsNullOrEmpty())
                    {
                        AuthenticationRequestSearchViewModel.SearchResult.Clear();
                    }
                    AuthenticationRequestSearchViewModel.SearchResult.AddRange(e.Result);

                    foreach (var result in AuthenticationRequestSearchViewModel.SearchResult)
                    {
                        if (result.AuthenticationFileID != null)
                        {
                            result.IsFile = true;
                        }
                    }
                    break;
            }
        }
        private void OnGetAuthenticationRequestCompletedException(object obj, CallbackEventArgs<List<GetImportAuthenticationRequestResult>> e, Exception exc) { }
        private void OnGetAuthenticationRequestCompletedFinally(object obj, CallbackEventArgs<List<GetImportAuthenticationRequestResult>> e)
        {
            AuthenticationRequestSearchViewModel.ProgressBarPanelVisibile = Visibility.Collapsed;
        }


        [CallbackMethod]
        public void OnCreateNewAuthenticationFileCompleted(object obj, CallbackEventArgs<CertificateOfOriginsImportAuthenticationFileDetails> e)
        {
            switch (e.Status)
            {
                case CallbackStatus.Success:
                    AuthenticationRequestSearchViewModel.CurrentFile = new CertificateOfOriginsImportAuthenticationFileDetails();
                    AuthenticationRequestSearchViewModel.CurrentFile = e.Result;

                    foreach (var result in AuthenticationRequestSearchViewModel.SearchResult)
                    {
                        if (result.IsCheck)
                        {
                            result.AuthenticationFileID = AuthenticationRequestSearchViewModel.CurrentFile.ID;
                            result.IsFile = true;
                            result.IsCheck = false;
                        }
                    }
                    break;
            }
        }
        private void OnCreateNewAuthenticationFileCompletedException(object obj, CallbackEventArgs<CertificateOfOriginsImportAuthenticationFileDetails> e, Exception exc) { }
        private void OnCreateNewAuthenticationFileCompletedFinally(object obj, CallbackEventArgs<CertificateOfOriginsImportAuthenticationFileDetails> e)
        {
            AuthenticationRequestSearchViewModel.ProgressBarPanelVisibile = Visibility.Collapsed;
        }


        [CallbackMethod]
        private void OnGetAuthenticationRequestByIDCompleted(object sender, CallbackEventArgs<CertificateOfOriginsImportAuthenticationRequest> e)
        {
            switch (e.Status)
            {
                case CallbackStatus.Success:
                    AuthenticationRequestSearchViewModel.ProgressBarPanelVisibile = Visibility.Collapsed;
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
        private void OnGetAuthenticationRequestByIDCompletedException(object sender, CallbackEventArgs<CertificateOfOriginsImportAuthenticationRequest> e, Exception exc) { }

        private void OnGetAuthenticationRequestByIDCompletedFinally(object sender, CallbackEventArgs<CertificateOfOriginsImportAuthenticationRequest> e) { }

        private void OnQuickGetCertificateRequestCompleted(object sender, CallbackEventArgs<CertificateOfOriginsImportAuthenticationRequest> e)
        {
            switch (e.Status)
            {
                case CallbackStatus.Success:
                    if (e.Result != null)
                        ClientEnvironment.Instance.ClientServices.ProtocolManager.ShowPage(e.Result, NavigationMappingType.Edit);
                    else
                        MsgBox.ShowMessage(EMessages.EntityWasNotFound);
                    break;
            }
        }

        private void OnGetEntityCompleted(object sender, CallbackEventArgs<CertificateOfOriginsImportAuthenticationRequest> e)
        {
            if (e.Result == null)
            {
                OnSearchEntityCallback(new SearchEntityCallbackEventArgs(e, null));
                return;
            }
            switch (e.Status)
            {
                case CallbackStatus.Success:
                    OnSearchEntityCallback(new SearchEntityCallbackEventArgs(e, e.Result));
                    break;

            }
        }


        #endregion Callback Operations

        #region Load Operations

        /// <summary>
        /// Loads the data.
        /// </summary>
        /// <param name="navObj">The navigation object.</param>
        /// <param name="arguments">The arguments.</param>
        public override void LoadData(INavigationObject navObj, params object[] arguments)
        {
              base.LoadData(navObj, arguments);
            OnDataTransferCallback(new DataTransferCallbackEventArgs(null, DataTransferCallbackType.LoadData));
            AuthenticationRequestSearchViewModel.SearchResult.All(re => re.IsCheck = false);

            var currentWithFullDetails = CALFacade.Instance.ClientUnityContainer.Resolve<IUsersExternalUtil>().CurrentWithFullDetails;
            if (currentWithFullDetails.SpecializationIDs.Contains((int)ESpecialization.Centralimportverificationrequests))
                AuthenticationRequestSearchViewModel.IsCentralImportVerificationRequests = true;

        }

        #endregion Load Operations

        #region Private Method

        private bool? CheckMergerRequests()
        {
            var firstRequest = AuthenticationRequestSearchViewModel.ChekedList.FirstOrDefault();
            if (firstRequest == null)
                return null;
            if (AuthenticationRequestSearchViewModel.ChekedList.All
                (cl => cl.IssuingCountryID == firstRequest.IssuingCountryID
                && cl.VendorID == firstRequest.VendorID
                && cl.DecisionID == (int)EAuthenticationRequestDecision.AuthenticationRequried))
            {
                AuthenticationRequestSearchViewModel.ChekedList.ForEach(cl => cl.AuthenticationFileStatusID = (int)EAuthenticationFileStatus.WaitingForSendingLetter);
                return true;
            }
            return false;

        }

        #endregion Private Method


    }
}
