using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using Customs.CRM.CertificateOfOrigins.Client.Api.ExportDocumentAuthenticationRequestSearch;
using Customs.CRM.CertificateOfOrigins.InternalProxy;
using Customs.CRM.Entities.Entities.ExportDocumentAuthenticationRequestSearch;
using Customs.Inf.MMI.Common.CAL;
using Customs.Inf.MMI.Common.Common;
using Customs.Inf.MMI.Common.Navigation;
using Customs.Inf.MMI.Common.Toolbox.CustomEventArgs;
using Customs.Inf.MMI.Services.Module.ClientManager;
using Customs.Inf.MMI.Services.Module.Search;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.Communication;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.Extensions;
using MalamTeam.Infrastructure.GeneralServices.Environment.Enums;

namespace Customs.CRM.CertificateOfOrigins.Client.Module.ExportDocumentAuthenticationRequestSearch
{
    public class ExportDocumentAuthenticationRequestSearchPresenter : SearchConsumerBase, IExportDocumentAuthenticationRequestSearchPresenter
    {
        /// <summary>
        /// Gets the view model object.
        /// </summary>
        /// <value>The current view model object.</value>
        public ExportDocumentAuthenticationRequestSearchViewModel ExportDocumentAuthenticationRequestSearchViewModel
        {
            get
            {
                IViewModel viewModel;
                ViewModels.TryGetValue("ExportDocumentAuthenticationRequestSearchViewModel", out viewModel);
                return viewModel as ExportDocumentAuthenticationRequestSearchViewModel;
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

            var viewModel = new ExportDocumentAuthenticationRequestSearchViewModel { Name = "ExportDocumentAuthenticationRequestSearchViewModel" };
            ViewModels.Add("ExportDocumentAuthenticationRequestSearchViewModel", viewModel);

            #region register commands

            viewModel.SearchCommand = new CustomDelegateCommand(OnSearchCommand, "SearchCommand");
            viewModel.ClearCommand = new CustomDelegateCommand(OnClearCommand, "ClearCommand");
            viewModel.SelectResultRowCommand = new CustomDelegateCommand<CustomDataGridEventArgs>(OnSelectResultRowCommand, "SelectResultRowCommand");

            #endregion register commands
        }

        #endregion ViewModel

        #region Callback Operations


        #endregion Callback Operations

        #region Commands Operations

        private void OnSearchCommand()
        {
            ExportDocumentAuthenticationRequestSearchViewModel.ProgressBarVisibility = Visibility.Visible;
            ResolveInstance<ICertificateOfOriginsInternalProxy>()
                .GetExportDocumentAuthenticationRequestSearch(OnSearchResult, CurrentFilter);
        }

        private void OnSearchResult(object sender, CallbackEventArgs<List<ExportDocumentAuthenticationRequestSearchResult>> e)
        {
            ExportDocumentAuthenticationRequestSearchViewModel.ProgressBarVisibility = Visibility.Collapsed;
            switch (e.Status)
            {
                case CallbackStatus.Success:
                    ExportDocumentAuthenticationRequestSearchViewModel.Results =
                        new ObservableCollection<ExportDocumentAuthenticationRequestSearchResult>(e.Result);
                    break;
            }
        }

        private ExportDocumentAuthenticationRequestSearchFilter CurrentFilter
        {
            get { return ExportDocumentAuthenticationRequestSearchViewModel.CurrentFilter; }
        }

        private void OnClearCommand()
        {
            ExportDocumentAuthenticationRequestSearchViewModel.CurrentFilter = new ExportDocumentAuthenticationRequestSearchFilter();
        }

        private void OnSelectResultRowCommand(CustomDataGridEventArgs selectedRowArgs)
        {
            ArgumentValidator.AssertNotNull(selectedRowArgs, "no selected row");
            ClientEnvironment.Instance.ClientServices.ProtocolManager.ShowPage(
                EEntityType.ExportDocumentAuthenticationRequest,
                ((ExportDocumentAuthenticationRequestSearchResult) selectedRowArgs.SelectedItem).RequestID,
                NavigationMappingType.Edit);
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
            ExportDocumentAuthenticationRequestSearchViewModel.CurrentFilter = new ExportDocumentAuthenticationRequestSearchFilter();
            OnDataTransferCallback(new DataTransferCallbackEventArgs(null, DataTransferCallbackType.LoadData));
            ExportDocumentAuthenticationRequestSearchViewModel.ProgressBarVisibility = Visibility.Collapsed;
        }

        #endregion Load Operations
    }
}
