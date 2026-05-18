using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Customs.CRM.CertificateOfOrigins.Client.Api.ExportDocumentAuthenticationRequestSearch;
using Customs.CRM.Entities.Entities.ExportDocumentAuthenticationRequestSearch;
using Customs.Inf.MMI.Common.CAL;
using Customs.Inf.MMI.Common.Toolbox.CustomEventArgs;
using MalamTeam.Infrastructure.GeneralServices.Environment.Enums;

namespace Customs.CRM.CertificateOfOrigins.Client.Module.ExportDocumentAuthenticationRequestSearch
{
    /// <summary>
    /// ExportDocumentAuthenticationRequestSearchViewModel
    /// </summary>
    public class ExportDocumentAuthenticationRequestSearchViewModel : CustomsViewModel, IExportDocumentAuthenticationRequestSearchViewModel
    {
        /// <summary>
        /// Gets or sets the current filter.
        /// </summary>
        /// <value>The current filter.</value>
        public ExportDocumentAuthenticationRequestSearchFilter CurrentFilter
        {
            get { return GetValue<ExportDocumentAuthenticationRequestSearchFilter>(); }
            set { SetValue(value); }
        }

        #region commands

        /// <summary>
        /// Gets or sets the search command.
        /// </summary>
        /// <value>The current filter.</value>
        public CustomDelegateCommand SearchCommand
        {
            get { return GetCommand<CustomDelegateCommand>(); }
            set { SetCommand(value); }
        }

        public CustomDelegateCommand ClearCommand
        {
            get { return GetCommand<CustomDelegateCommand>(); }
            set { SetCommand(value); }
        }

        public CustomDelegateCommand<CustomDataGridEventArgs> SelectResultRowCommand
        {
            get { return GetCommand<CustomDelegateCommand<CustomDataGridEventArgs>>(); }
            set { SetCommand(value); }
        }

        #endregion commands

        #region members

        public ObservableCollection<ExportDocumentAuthenticationRequestSearchResult> Results
        {
            get { return GetValue<ObservableCollection<ExportDocumentAuthenticationRequestSearchResult>>(); }
            set { SetValue(value); }
        }

        public Visibility ProgressBarVisibility
        {
            get { return GetValue<Visibility>(); }
            set { SetValue(value); }
        }

        #endregion members
    }
}
