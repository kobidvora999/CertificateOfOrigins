using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Customs.CertificateOfOrigins.Entities;
using Customs.Inf.MMI.Common.CAL;
using Customs.Inf.MMI.Common.Core;

namespace Customs.CRM.CertificateOfOrigins.Client.Module.AuthenticationRequest.AuthenticationRequestSearch
{
    /// <summary>
    /// AuthenticationRequestSearchViewModel
    /// </summary>
    public class AuthenticationRequestSearchViewModel : CustomsViewModel
    {
        public AuthenticationRequestSearchViewModel()
        {
            ProgressBarPanelVisibile = Visibility.Collapsed;
            CurrentFilter = new ImportAuthenticationRequestFilter();
            NumberOfMonthToSearchImportAuthenticationRequest = CALFacade.Instance.GetConfigurationValue<int>("NumberOfMonthToSearchImportAuthenticationRequest");
            CurrentFilter.FileCreateDates.FromDate = DateTime.Now.AddMonths(NumberOfMonthToSearchImportAuthenticationRequest * -1);
            CurrentFilter.FileCreateDates.ToDate = DateTime.Now;
            SearchResult = new CustomObservableCollection<GetImportAuthenticationRequestResult>();
        }


        #region Properties
        public int NumberOfMonthToSearchImportAuthenticationRequest
        {
            get { return GetValue<int>(); }
            set { SetValue(value); }
        }
        public ImportAuthenticationRequestFilter CurrentFilter
        {
            get { return GetValue<ImportAuthenticationRequestFilter>(); }
            set { SetValue(value); }   
        }
        public CustomObservableCollection<GetImportAuthenticationRequestResult> SearchResult
        {
            get { return GetValue<CustomObservableCollection<GetImportAuthenticationRequestResult>>(); }
            set { SetValue(value); }
        }

        public Visibility ProgressBarPanelVisibile
        {
            get { return GetValue<Visibility>(); }
            set { SetValue(value); }
        }

        public List<GetImportAuthenticationRequestResult> ChekedList
        {
            get { return GetValue<List<GetImportAuthenticationRequestResult>>(); }
            set { SetValue(value); }
        }

        public GetImportAuthenticationRequestResult SelectedResult
        {
            get { return GetValue<GetImportAuthenticationRequestResult>(); }
            set { SetValue(value); }
        }

        public CertificateOfOriginsImportAuthenticationFileDetails CurrentFile
        {
            get { return GetValue<CertificateOfOriginsImportAuthenticationFileDetails>(); }
            set { SetValue(value); }
        }

        public GetImportAuthenticationRequestResult SelectedRequest
        {
            get { return GetValue<GetImportAuthenticationRequestResult>(); }
            set { SetValue(value); }
        }

        public bool IsCentralImportVerificationRequests
        {
            get { return GetValue<bool>(); }
            set { SetValue(value); }
        }


        #endregion Properties

        #region Commands

        /// <summary>
        /// Gets or sets the search command.
        /// </summary>
        /// <value>The current filter.</value>
        public ICommand SearchAuthenticationRequestCommand
        {
            get { return GetCommand<ICommand>(); }
            set { SetValue(value); }
        }

        public ICommand AuthenticationRequestClearCommand
        {
            get { return GetCommand<ICommand>(); }
            set { SetValue(value); }
        }

        public ICommand AuthenticationRequestRowDoubleClickCommand
        {
            get { return GetCommand<ICommand>(); }
            set { SetValue(value); }
        }

        public ICommand CheckedAuthenticationRequestCommand
        {
            get { return GetCommand<ICommand>(); }
            set { SetValue(value); }
        }
        public ICommand MergeRequestsCommand
        {
            get { return GetCommand<ICommand>(); }
            set { SetValue(value); }
        }
        public ICommand OpenImportAuthenticationRequestCommand
        {
            get { return GetCommand<ICommand>(); }
            set { SetValue(value); }
        }


        #endregion Commands

    }
}
