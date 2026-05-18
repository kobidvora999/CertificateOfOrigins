using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Customs.CRM.CertificateOfOrigins.Client.Api.ExportDocumentAuthenticationRequestEdit;
using Customs.CertificateOfOrigins.Entities;
using Customs.Inf.MMI.Common.CAL;
using Customs.Inf.MMI.Common.Core;
using Customs.Infrastructure.DocumentManagement.ExternalCommon;
using Customs.Infrastructure.Entities;
using MalamTeam.Infrastructure.GeneralServices.Environment.Enums;

namespace Customs.CRM.CertificateOfOrigins.Client.Module.ExportDocumentAuthenticationRequestEdit
{
    /// <summary>
    /// ExportDocumentAuthenticationRequestViewModel
    /// </summary>
    public class ExportDocumentAuthenticationRequestViewModel : CustomsViewModel, IExportDocumentAuthenticationRequestViewModel
    {

        /// <summary>
        /// ExportDocumentAuthenticationRequestViewModel default ctor
        /// </summary>
        public ExportDocumentAuthenticationRequestViewModel()
        {
            if (IsTrackablePropertySupported(typeof(ExportDocumentAuthenticationRequest)))
            {
                TrackableProperties.Add(CurrentEntityParamName, new TrackableProperty(CurrentEntityParamName, 1));
            }
            EntityTypes = new ObservableCollection<EEntityType>
                              {
                                  EEntityType.ExportDeclaration
                              };
            RelevantDocumentTypes =
                CALFacade.Instance.GetConfigurationValue<string>("ExportAuthenticationRelevantDocumentTypes");

        }

        #region members

        public ExportDocumentAuthenticationRequest CurrentEntity
        {
            get { return GetValue<ExportDocumentAuthenticationRequest>(); }
            set { SetValue(value); }
        }

        public ViewStates CurrentEntityViewState
        {
            get { return GetValue<ViewStates>(); }
            set { SetValue(value); }
        }

        public Visibility ProgressBarVisibility
        {
            get { return GetValue<Visibility>(); }
            set { SetValue(value); }
        }

        public Document Document
        {
            get { return GetValue<Document>(); }
            set { SetValue(value); }
        }

        public ObservableCollection<EEntityType> EntityTypes
        {
            get { return GetValue<ObservableCollection<EEntityType>>(); }
            set { SetValue(value); }
        }

        public bool IsRequestSent
        {
            get { return GetValue<bool>(); }

            set { SetValue(value); }
        }

        public string RelevantDocumentTypes
        {
            get { return GetValue<string>();}
            set { SetValue(value); }
        }

        public ExportDocumentAuthenticationRequestLeadDocument SelectedExportDeclaration
        {
            get { return GetValue<ExportDocumentAuthenticationRequestLeadDocument>(); }
            set { SetValue(value); }
        }

        public bool IsRemindDelivery
        {
            get { return GetValue<bool>(); }

            set { SetValue(value); }
        }

        public bool IsRequestDelivery
        {
            get { return GetValue<bool>(); }

            set { SetValue(value); }
        }
        #endregion members

        #region commands

        public CustomDelegateCommand EditCommand
        {
            get { return GetCommand<CustomDelegateCommand>(); }
            set { SetCommand(value); }
        }

        public CustomDelegateCommand SaveCommand
        {
            get { return GetCommand<CustomDelegateCommand>(); }
            set { SetCommand(value); }
        }

        public CustomDelegateCommand SaveCloseCommand
        {
            get { return GetCommand<CustomDelegateCommand>(); }
            set { SetCommand(value); }
        }

        public CustomDelegateCommand CancelCommand
        {
            get { return GetCommand<CustomDelegateCommand>(); }
            set { SetCommand(value); }
        }

        public CustomDelegateCommand RefreshCommand
        {
            get { return GetCommand<CustomDelegateCommand>(); }
            set { SetCommand(value); }
        }

        public CustomDelegateCommand<DocumentCommandArgs> SaveMainDocumentCompletedCommand
        {
            get { return GetCommand<CustomDelegateCommand<DocumentCommandArgs>>(); }
            set { SetCommand(value); }
        }

        public CustomDelegateCommand<DocumentCommandArgs> SaveAdditionalDocumentsCompletedCommand
        {
            get { return GetCommand<CustomDelegateCommand<DocumentCommandArgs>>(); }
            set { SetCommand(value); }
        }

        public ICommand SaveDocumentsOnSaveEntityCommand
        {
            get { return GetCommand<ICommand>(); }
            set { SetCommand(value); }
        }
        public ICommand RequestDeliverNotificationCommand
        {
            get { return GetCommand<ICommand>(); }
            set { SetCommand(value); }
        }

        public ICommand SaveDocumentsCommand
        {
            get { return GetCommand<ICommand>(); }
            set { SetCommand(value); }
        }

        public ICommand RemindDeliverNotificationCommand
        {
            get { return GetCommand<ICommand>(); }
            set { SetCommand(value); }
        }

        public CustomDelegateCommand<ExportDocumentAuthenticationRequest> DeliverToCustomsHouseNotificationCommand
        {
            get { return GetCommand<CustomDelegateCommand<ExportDocumentAuthenticationRequest>>(); }
            set { SetCommand(value); }
        }

        public ICommand ForeignCustomerSelectedChangeCommand
        {
            get { return GetCommand<ICommand>(); }
            set { SetCommand(value); }
        }

        public CustomDelegateCommand CountrySelectedChangeCommand
        {
            get { return GetCommand<CustomDelegateCommand>(); }
            set { SetCommand(value); }
        }
        public CustomDelegateCommand NewExportAuthenticationRequestManufactingAreaCommand
        {
            get { return GetCommand<CustomDelegateCommand>(); }
            set { SetCommand(value); }
        }

        public CustomDelegateCommand ExportDeclarationsDeleteButtonCommand
        {
            get { return GetCommand<CustomDelegateCommand>(); }
            set { SetCommand(value); }
        }
        
        #endregion commands

        #region methods

        private string CurrentEntityParamName = "CurrentEntity";

        /// <summary>
        /// add version on edit
        /// </summary>
        public void AddVersion()
        {
            AddVersion(CurrentEntityParamName, CurrentEntity);
        }

        /// <summary>
        /// reset version on cancel edit
        /// </summary>
        public void ResetVersion()
        {
            ResetValue(CurrentEntityParamName);
            OnPropertyChanged(CurrentEntityParamName);
        }

        #endregion methods
    }
}
