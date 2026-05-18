using System;
using System.Collections.Generic;
using System.Windows;
using Customs.CertificateOfOrigins.Entities;
using Customs.CRM.CertificateOfOrigins.Client.Api;
using Customs.CRM.CertificateOfOrigins.Client.Api.CertificateOfOriginsSearch;
using Customs.CRM.CertificateOfOrigins.InternalCommon.Common;
using Customs.CRM.CertificateOfOrigins.InternalProxy;
using Customs.CRM.Entities.CertificateOfOriginsPartial;
using Customs.Inf.MMI.Common.Aspects;
using Customs.Inf.MMI.Common.CAL;
using Customs.Inf.MMI.Common.Common;
using Customs.Inf.MMI.Common.Extensions;
using Customs.Inf.MMI.Common.Navigation;
using Customs.Inf.MMI.Common.Toolbox.CustomEventArgs;
using Customs.Inf.MMI.Services.Api.Search;
using Customs.Inf.MMI.Services.Module.ClientManager;
using Customs.Inf.MMI.Services.Module.Search;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.Communication;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.Extensions;
using MalamTeam.Infrastructure.GeneralServices.Environment.Enums;


namespace Customs.CRM.CertificateOfOrigins.Client.Module.CertificateOfOriginsSearch
{
    public class CertificateOfOriginsSearchPresenter : SearchConsumerBase, ICertificateOfOriginsSearchPresenter
    {
        #region ViewModel

        /// <summary>
        /// Gets the view model object.
        /// </summary>
        /// <value>The current view model object.</value>
        public CertificateOfOriginsSearchViewModel CertificateOfOriginsSearchViewModel
        {
            get
            {
                IViewModel viewModel;
                ViewModels.TryGetValue(CertificateOfOriginsConstants.CertificateOfOriginsSearchViewModel, out viewModel);
                return viewModel as CertificateOfOriginsSearchViewModel;
            }
        }

        /// <summary>
        /// Initializes the view models.<para/>
        /// (called before "InternalInitViews")
        /// </summary>
        protected override void InternalInitViewModels()
        {
            base.InternalInitViewModels();

            var viewModel = new CertificateOfOriginsSearchViewModel { Name = CertificateOfOriginsConstants.CertificateOfOriginsSearchViewModel };

            #region Register Commands

            viewModel.Commands.RegisterCommand<CertificateOfOriginFilter>(OnSearchCertificateOfOriginsCommand, ModuleCommands.SearchCertificateOfOriginsCommand);            
            viewModel.Commands.RegisterCommand(OnCertificateOfOriginsClearCommand, ModuleCommands.CertificateOfOriginsClearCommand);
            viewModel.Commands.RegisterCommand<CustomDataGridEventArgs>(OnCertificateOfOriginsRowDoubleClickCommand, ModuleCommands.CertificateOfOriginsRowDoubleClickCommand);

            #endregion Register Commands

            ViewModels.Add(CertificateOfOriginsConstants.CertificateOfOriginsSearchViewModel, viewModel);
        }

        #endregion ViewModel


        #region Commands Operations

        [CommandMethod]
        public void OnSearchCertificateOfOriginsCommand(CertificateOfOriginFilter filter)
        {            
            CertificateOfOriginsSearchViewModel.ProgressBarPanelVisibile = Visibility.Visible;
            var searchResultCollection = CertificateOfOriginsSearchViewModel.SearchResult;
            if (searchResultCollection != null) searchResultCollection.Clear();

            ResolveInstance<ICertificateOfOriginsInternalProxy>().GetCertificateOfOriginsByFilter(OnGetCertificateOfOriginsCompleted, filter);
        }
        private void OnSearchCertificateOfOriginsCommandException(CertificateOfOriginFilter filter, Exception exc)
        {            
            CertificateOfOriginsSearchViewModel.ProgressBarPanelVisibile = Visibility.Collapsed;
        }


        [CommandMethod]
        public void OnCertificateOfOriginsClearCommand()
        {
            CertificateOfOriginsSearchViewModel.CurrentFilter = new CertificateOfOriginFilter();
        }
        private void OnCertificateOfOriginsClearCommandException(Exception exc)
        {

        }

        [CommandMethod]
        public void OnCertificateOfOriginsRowDoubleClickCommand(CustomDataGridEventArgs args)
        {
            var certificateOfOrigin = ArgumentValidator.AssertNotNullAndOfType<CertificateOfOriginResult>(args.SelectedItem, "args.SelectedItem");
            if (certificateOfOrigin != null)
            {
                CertificateOfOriginsSearchViewModel.ProgressBarPanelVisibile = Visibility.Visible;
                ClientEnvironment.Instance.ClientServices.ProtocolManager.ShowPage(EEntityType.CertificateOfOrigin, (int)certificateOfOrigin.ID, NavigationMappingType.Edit);
                CertificateOfOriginsSearchViewModel.ProgressBarPanelVisibile = Visibility.Collapsed;
            }

        }
        private void OnCertificateOfOriginsRowDoubleClickCommandException(CustomDataGridEventArgs args, Exception exc)
        {
            CertificateOfOriginsSearchViewModel.ProgressBarPanelVisibile = Visibility.Collapsed;
        }


        /// <summary>
        /// Called when [search command executed].
        /// </summary>
        /// <param name="searchQuery">The search query.</param>
        /// <param name="selectedItem">The selected item.</param>
        [CommandMethod]
        public override void OnSearchCommandExecuted(string searchQuery, ISearchConsumerItem selectedItem)
        {            
            CertificateOfOriginsSearchViewModel.ProgressBarPanelVisibile = Visibility.Visible;

            ResolveInstance<ICertificateOfOriginsInternalProxy>().IsCertificateOfOriginByExternalIdExist(OnGetCertificateOfOriginByIdCompleted, searchQuery, searchQuery);

            base.OnSearchCommandExecuted(searchQuery, selectedItem);

            IsBusy = true;

        }
        public void OnSearchCommandExecutedException(string searchQuery, ISearchConsumerItem selectedItem, Exception exc)
        {
            IsBusy = false;
            CertificateOfOriginsSearchViewModel.ProgressBarPanelVisibile = Visibility.Collapsed;            
        }

        #endregion Commands Operations


        #region Callback Operations

        [CallbackMethod]
        public void OnGetCertificateOfOriginsCompleted(object obj, CallbackEventArgs<List<CertificateOfOriginResult>> e)
        {
            switch (e.Status)
            {
                case CallbackStatus.Success:

                    if (!CertificateOfOriginsSearchViewModel.SearchResult.IsNullOrEmpty())
                    {
                        CertificateOfOriginsSearchViewModel.SearchResult.Clear();
                    }
                    CertificateOfOriginsSearchViewModel.SearchResult.AddRange(e.Result);
                    break;
            }
        }
        private void OnGetCertificateOfOriginsCompletedException(object obj, CallbackEventArgs<List<CertificateOfOriginResult>> e, Exception exc)
        {

        }
        private void OnGetCertificateOfOriginsCompletedFinally(object obj, CallbackEventArgs<List<CertificateOfOriginResult>> e)
        {
            CertificateOfOriginsSearchViewModel.ProgressBarPanelVisibile = Visibility.Collapsed;
        }


        [CallbackMethod]
        public void OnGetCertificateOfOriginByIdCompleted(object sender, CallbackEventArgs<CertificateOfOriginResult> e)
        {
            switch (e.Status)
            {
                case CallbackStatus.Success:                            
                    ArgumentValidator.AssertNotNull(e.Result, "e.Result", true, EMessages.CertificateNumberNotFound, e.StateObject);
                    var certificateId = (int)e.Result.ID;
                    ClientEnvironment.Instance.ClientServices.ProtocolManager.ShowPage(EEntityType.CertificateOfOrigin, certificateId, NavigationMappingType.Edit);
                    break;
            }
        }
        private void OnGetCertificateOfOriginByIdCompletedException(object sender, CallbackEventArgs<CertificateOfOriginResult> e, Exception exc)
        {

        }
        private void OnGetCertificateOfOriginByIdCompletedFinally(object sender, CallbackEventArgs<CertificateOfOriginResult> e)
        {
            CertificateOfOriginsSearchViewModel.ProgressBarPanelVisibile = Visibility.Collapsed;
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
        }

        #endregion Load Operations
    }
}
