using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Customs.CertificateOfOrigins.Entities;
using Customs.CRM.CertificateOfOrigins.Client.Api.CertificateOfOriginsSearch;
using Customs.CRM.CertificateOfOrigins.InternalCommon.Common;
using Customs.CRM.Entities.CertificateOfOriginsPartial;
using Customs.Inf.MMI.Common.CAL;
using Customs.Inf.MMI.Common.Core;
using Customs.Shared.Entities;
using MalamTeam.Infrastructure.GeneralServices.Environment.Enums;

namespace Customs.CRM.CertificateOfOrigins.Client.Module.CertificateOfOriginsSearch
{
    /// <summary>
    /// CertificateOfOriginsSearchViewModel
    /// </summary>
    public class CertificateOfOriginsSearchViewModel : CustomsViewModel, ICertificateOfOriginsSearchViewModel
    {
        public CertificateOfOriginsSearchViewModel()
        {
            ProgressBarPanelVisibile = Visibility.Collapsed;
            CurrentFilter = new CertificateOfOriginFilter();
            SearchResult = new CustomObservableCollection<CertificateOfOriginResult>();
            OrganizationUnitTypeIds = new ObservableCollection<EOrganizationUnitType> { EOrganizationUnitType.BateiMeches };           
        }


        #region Properties

        /// <summary>
        /// Gets or sets the current filter.
        /// </summary>
        /// <value>The current filter.</value>
        public CertificateOfOriginFilter CurrentFilter
        {
            get { return GetValue<CertificateOfOriginFilter>(); }
            set { SetValue(value); }
        }

        public CustomObservableCollection<CertificateOfOriginResult> SearchResult
        {
            get { return GetValue<CustomObservableCollection<CertificateOfOriginResult>>(); }
            set { SetValue(value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the progress bar panel is visibile.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the progress bar panel is visibile; otherwise, <c>false</c>.
        /// </value>
        public Visibility ProgressBarPanelVisibile
        {
            get { return GetValue<Visibility>(); }
            set { SetValue(value); }
        }

        public ObservableCollection<EOrganizationUnitType> OrganizationUnitTypeIds
        {
            get { return GetValue<ObservableCollection<EOrganizationUnitType>>(); }
            set { SetValue(value); }
        }

        #endregion Properties


        #region Commands

        /// <summary>
        /// Gets or sets the search command.
        /// </summary>
        /// <value>The current filter.</value>
        public ICommand SearchCertificateOfOriginsCommand
        {
            get { return GetCommand<ICommand>(); }
            set { SetValue(value); }
        }

        public ICommand CertificateOfOriginsClearCommand
        {
            get { return GetCommand<ICommand>(); }
            set { SetValue(value); }
        }

        public ICommand CertificateOfOriginsRowDoubleClickCommand
        {
            get { return GetCommand<ICommand>(); }
            set { SetValue(value); }
        }

        #endregion Commands
    }
}
