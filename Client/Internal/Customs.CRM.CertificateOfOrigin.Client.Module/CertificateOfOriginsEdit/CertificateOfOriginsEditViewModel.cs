using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Customs.CertificateOfOrigins.Entities;
using Customs.CRM.CertificateOfOrigins.Client.Api.CertificateOfOriginsEdit;
using Customs.CRM.Entities;
using Customs.CRM.Entities.CertificateOfOriginsPartial;
using Customs.Inf.MMI.Common.CAL;
using Customs.Inf.MMI.Common.Core;
using Customs.Inf.MMI.Common.Navigation;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.Extensions;
using MalamTeam.Infrastructure.GeneralServices.Environment.Enums;

namespace Customs.CRM.CertificateOfOrigins.Client.Module.CertificateOfOriginsEdit
{
    /// <summary>
    /// CertificateOfOriginsEditViewModel
    /// </summary>
    public class CertificateOfOriginsEditViewModel : CustomsViewModel, ICertificateOfOriginsEditViewModel
    {
        #region C'tor

        public CertificateOfOriginsEditViewModel()
        {
            if (IsTrackablePropertySupported(typeof(CertificateOfOrigin)))
                TrackableProperties.Add(CertificateOfOriginsConstants.CurrentEntity, new TrackableProperty(CertificateOfOriginsConstants.CurrentEntity, 1));

            EditViewState = ViewStates.ReadOnly;
            IsStatusEditable = false;
            IsEditButtonEnabled = true;
            OrganizationUnitTypeIds = new ObservableCollection<EOrganizationUnitType> { EOrganizationUnitType.BateiMeches };
        }

        #endregion C'tor


        #region Properties

        /// <summary>
        /// Gets or sets the current entity.
        /// </summary>
        /// <value>The current entity.</value>
        public CertificateOfOrigin CurrentEntity
        {
            get { return GetValue<CertificateOfOrigin>(); }
            set { SetValue(value); }
        }

        public int CertificateOfOriginStatusIDPrev
        {
            get { return GetValue<int>(); }
            set { SetValue(value); }
        }


        public ViewStates EditViewState
        {
            get { return GetValue<ViewStates>(); }
            set
            {
                if (value == ViewStates.Edit)
                {
                        //TODO: Change this to 'RecordEditable' when export customs version 
                        IsStatusEditable = CurrentEntity.SelectedStatus.IsRecordEditableNotExport;
                }
                
                SetValue(value);
                if (CurrentEntity == null) return;
                SetStatusPredicate(value);               
            }
        }

        public bool IsStatusEditable
        {
            get { return GetValue<bool>(); }
            set { SetValue(value); }
        }        

        public bool IsEditButtonEnabled
        {
            get { return GetValue<bool>(); }
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

        public Visibility ProgressBarVisibility
        {
            get { return GetValue<Visibility>(); }
            set { SetValue(value); }
        }

        public ObservableCollection<EOrganizationUnitType> OrganizationUnitTypeIds
        {
            get { return GetValue<ObservableCollection<EOrganizationUnitType>>(); }
            set { SetValue(value); }
        }

        public bool IsRelevantToCurrentVersion
        {
            get
            {
                return true;
                //return ClientEnvironment.Instance.MMIFacade.GetConfigurationValue<int>(CertificateOfOriginsConstants.CustomsVersionNumber) < 4;
            }
        }

        public CertificateOfOriginLOV SelectedCertificateToCancel
        {
            get { return GetValue<CertificateOfOriginLOV>(); }
            set { SetValue(value); }
        }

        public CustomObservableCollection<INavigationMapping> ConnectedEntities
        {
            get { return GetValue<CustomObservableCollection<INavigationMapping>>(); }
            set { SetValue(value); }
        }

        #endregion Properties
        
        #region Commands
        
        /// <summary>
        /// Gets or sets the edit command.
        /// </summary>
        /// <value>The current entity.</value>
        public ICommand EditCertificateOfOriginsCommand
        {
            get { return GetCommand<ICommand>(); }
            set { SetCommand(value); }
        }

        /// <summary>
        /// Gets or sets the save command.
        /// </summary>
        /// <value>The current entity.</value>
        public ICommand SaveCertificateOfOriginsCommand
        {
            get { return GetCommand<ICommand>(); }
            set { SetCommand(value); }
        }

        /// <summary>
        /// Gets or sets the cancel command.
        /// </summary>
        /// <value>The current entity.</value>
        public ICommand CancelCertificateOfOriginsCommand
        {
            get { return GetCommand<ICommand>(); }
            set { SetCommand(value); }
        }

        /// <summary>
        /// Gets or sets the refresh command.
        /// </summary>
        /// <value>The refresh command</value>
        public ICommand RefreshCertificateOfOriginsCommand
        {
            get { return GetCommand<ICommand>(); }
            set { SetCommand(value); }
        }

        public ICommand CustomerSaveDocumentsCommand
        {
            get { return GetCommand<ICommand>(); }
            set { SetCommand(value); }
        }

        public ICommand SaveTheDocumentsCommand
        {
            get { return GetCommand<ICommand>(); }
            set { SetCommand(value); }
        }
        public ICommand CancelCertificateOfOriginsByUserCommand
        {
            get { return GetCommand<ICommand>(); }
            set { SetCommand(value); }
        }

        #endregion Commands

        #region Methods

        public void RaiseCheckManagementPropertyChanged(string propertyName)
        {
            this.RaisePropertyChanged(propertyName);
        }

        private void SetStatusPredicate(ViewStates editViewState)
        {
            var currentStatusId = CurrentEntity.CertificateOfOriginStatusID; 
            var predicate = CurrentEntity.StatusPredicate = string.Format(CertificateOfOriginsConstants.StatusTypePredicate, currentStatusId);
            var specificVersionPredicate = string.Format(CertificateOfOriginsConstants.StatusTypePredicateForSpecificVersion, 
                                                         (int)ECertificateOfOriginStatus.Error, (int)ECertificateOfOriginStatus.Received, currentStatusId);

            switch (editViewState)
            {
                case ViewStates.ReadOnly:
                    CurrentEntity.StatusPredicate = string.Empty;
                    break;
                case ViewStates.Edit:
                case ViewStates.Dirty:
                    CurrentEntity.StatusPredicate = IsRelevantToCurrentVersion ? specificVersionPredicate : predicate;                                        
                    break;
            }
        }

        #endregion Methods
    }
}
