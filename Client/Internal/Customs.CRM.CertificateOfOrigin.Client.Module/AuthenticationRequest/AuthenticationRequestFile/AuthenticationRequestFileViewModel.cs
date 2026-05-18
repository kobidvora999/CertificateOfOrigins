using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Customs.CRM.CertificateOfOrigins.Client.Api.AuthenticationRequest.AuthenticationRequestFile;
using Customs.CRM.Entities.CertificateOfOriginsPartial;
using Customs.CertificateOfOrigins.Entities;
using Customs.Inf.MMI.Common.CAL;
using Customs.Inf.MMI.Common.Core;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.Extensions;
using MalamTeam.Infrastructure.GeneralServices.Environment.Enums;
using Customs.Inf.MMI.Common.Toolbox.CustomEventArgs;
using Customs.Infrastructure.UserManagement.ExternalCommon;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.Base_Entities;

namespace Customs.CRM.CertificateOfOrigins.Client.Module.AuthenticationRequest.AuthenticationRequestFile
{
    /// <summary>
    /// AuthenticationRequestFileViewModel
    /// </summary>
    public class AuthenticationRequestFileViewModel : CustomsViewModel, IAuthenticationRequestFileViewModel
    {
         #region C'tor

        public AuthenticationRequestFileViewModel()
        {
            if (IsTrackablePropertySupported(typeof(CertificateOfOriginsImportAuthenticationFileDetails)))
                TrackableProperties.Add(CertificateOfOriginsConstants.CurrentEntity, new TrackableProperty(CertificateOfOriginsConstants.CurrentEntity, 1));
            FileStatusFilterIds = new ObservableCollection<int>();
        }
        
        #endregion C'tor

        #region Properties

        /// <summary>
        /// Gets or sets the current entity.
        /// </summary>
        /// <value>
        /// The current entity.
        /// </value>
        public CertificateOfOriginsImportAuthenticationFileDetails CurrentEntity
        {
            get
            {
                var a = GetValue<CertificateOfOriginsImportAuthenticationFileDetails>();
                return a;
            }
            set
            {
                var a = value;
                SetValue(a);
            }
        }

        public Visibility ProgressBarVisibility
        {
            get { return GetValue<Visibility>(); }
            set { SetValue(value); }
        }
        public string StatusPredicate
        {
            get { return GetValue<string>(); }
            set { SetValue(value); }
        }
        public bool IsEditEnable
        {
            get { return GetValue<bool>(); }
            set { SetValue(value); }
        }


        public CertificateOfOriginsImportAuthenticationRequest SelectedRequest 
        {
            get { return GetValue<CertificateOfOriginsImportAuthenticationRequest>(); }
            set
            {
                SetValue(value);
                if (value != null)
                {
                    EntityForAddExistingDocument = new VirtualEntity(value.LeadDocumentID, (int)EEntityType.ImportDeclaration);
                }
            }
        }
        public VirtualEntity EntityForAddExistingDocument
        {
            get { return GetValue<VirtualEntity>(); }
            set { SetValue(value); }
        }

        public bool IsEditModeEnabled 
        {
            get { return GetValue<bool>(); }
            set { SetValue(value); }
        }

        public ViewStates ViewState
        {
            get { return GetValue<ViewStates>(); }
            set { SetValue(value); }
        }
    

        public bool CloseAfterSaveTrue
        {
            get { return true; }

            set { SetValue(value); }
        }

        public bool CloseAfterSaveFalse
        {
            get { return false; }
            set { SetValue(value); }
        }

        public bool IsRequestSent
        {
            get { return GetValue<bool>(); }

            set { SetValue(value); }
        }

        public List<int> DecisionFilterForFileIds
        {
            get { return GetValue<List<int>>(); }
            set { SetValue(value); }
        }

        public List<CertificateOfOriginsDecision> DecisionForClaliMakorWorker
        {
            get { return GetValue<List<CertificateOfOriginsDecision>>(); }
            set { SetValue(value); }
        }

        public ObservableCollection<int> FileStatusFilterIds
        {
            get { return GetValue<ObservableCollection<int>>(); }
            set { SetValue(value); }
        }

        public bool IsUserAllowedToSendReminderToVendorOrCustomsHouse
        {
            get { return GetValue<bool>(); }
            set { SetValue(value); }
        }

        public UserPermissionProfile UserPermissionProfile
        {
            get { return GetValue<UserPermissionProfile>(); }
            set { SetValue(value); }
        }

        public int VendorID { get; set; }

        #endregion Properties


        #region Commands

        public ICommand CancelAuthenticationRequestFileCommand
        {
            get { return GetCommand<ICommand>(); }
            set { SetCommand(value); }
        }
        public ICommand PreferenceDocumentTypeIDChangedCommand
        {
            get { return GetCommand<ICommand>(); }
            set { SetCommand(value); }
        }

        public ICommand IssuingCountryIDChangedCommand
        {
            get { return GetCommand<ICommand>(); }
            set { SetCommand(value); }
        }
        
        public ICommand EditAuthenticationRequestFileCommand
        {
            get { return GetCommand<ICommand>(); }
            set { SetCommand(value); }
        }

        public ICommand SaveAuthenticationRequestFileCommand
        {
            get { return GetCommand<ICommand>(); }
            set { SetCommand(value); }
        }
        public ICommand RefreshAuthenticationRequestFileCommand
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

        public ICommand SelectCustomItemCommand
        {
            get { return GetCommand<ICommand>(); }
            set { SetCommand(value); }
        }

        public ICommand SendDeliveryForImporter
        {
            get { return GetCommand<ICommand>(); }
            set { SetCommand(value); }
        }
        
        public ICommand SendReminderForImporter
        {
            get { return GetCommand<ICommand>(); }
            set { SetCommand(value); }
        }

        public ICustomDelegateCommand NavigateToImportAuthenticationRequestFromUserTaskCommand
        {
            get { return GetCommand<ICustomDelegateCommand>(); }
            set { SetCommand(value); }
        }

        public CustomDelegateCommand<CustomDataGridEventArgs> RequestSelection
        {
            get { return GetCommand<CustomDelegateCommand<CustomDataGridEventArgs>>(); }
            set { SetCommand(value); }
        }

        #endregion Commands


        /// <summary>
        /// Raises the current entity property changed.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        public void RaiseCurrentEntityPropertyChanged(string propertyName)
        {
            this.RaisePropertyChanged(propertyName);
        }
    }
}
