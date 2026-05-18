using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Input;
using Customs.CRM.CertificateOfOrigins.Client.Api;
using Customs.CRM.CertificateOfOrigins.Client.Api.AuthenticationRequest.ImportProcessForm;
using Customs.CRM.Entities.CertificateOfOriginsPartial;
using Customs.CertificateOfOrigins.Entities;
using Customs.CRM.CertificateOfOrigins.ExternalCommon.Common;
using Customs.Inf.MMI.Common.CAL;
using Customs.Inf.MMI.Common.Core;
using Customs.Inf.MMI.Common.Navigation;
using Customs.Inf.MMI.Common.Toolbox.CustomsWindowBases;
using Customs.Infrastructure.DocumentManagement.ExternalCommon;
using Customs.KnowledgeStore.CustomsBook.ExternalCommon.Common;
using Customs.Shared.Entities;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.AOP;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.Base_Entities;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.EntityValidation;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.Enum;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.Extensions;
using MalamTeam.Infrastructure.GeneralServices.Environment.Enums;
using Customs.Inf.MMI.Common.Assets;
using LocalStrings = Customs.CRM.CertificateOfOrigins.Client.Api.LocalStrings;
using System;

namespace Customs.CRM.CertificateOfOrigins.Client.Module.AuthenticationRequest.ImportProcessForm
{
    /// <summary>
    /// ImportProcessFormViewmodel
    /// </summary>
    public class ImportProcessFormViewmodel : ValidatableViewModel, IImportProcessFormViewModel
    {
        #region C'tor

        /// <summary>
        /// Initializes a new instance of the <see cref="ImportProcessFormViewmodel"/> class.
        /// </summary>
        public ImportProcessFormViewmodel()
        {
            if (IsTrackablePropertySupported(typeof(CertificateOfOriginsImportAuthenticationRequest)))
                TrackableProperties.Add(CertificateOfOriginsConstants.CurrentEntity, new TrackableProperty(CertificateOfOriginsConstants.CurrentEntity, 1));

            VendorPredicate = CertificateOfOriginsConstants.VendorStatusID;
            IsEnabledInvoiceID = true;
            DocumentIDEditViewState = ViewStates.Edit;
        }

        #endregion C'tor

            public VirtualEntity EntityForAddExistingDocument
            {
                get { return GetValue<VirtualEntity>(); }
                set { SetValue(value); }
            }

        #region Properties

        /// <summary>
        /// Gets or sets the current entity.
        /// </summary>
        /// <value>The current entity.</value>
        /// 
        public CertificateOfOriginsImportAuthenticationRequest CurrentEntity
        {
            get { return GetValue<CertificateOfOriginsImportAuthenticationRequest>(); }
            set
            {
                SetValue(value);

                if (value != null)
                {
                    EntityForAddExistingDocument= new VirtualEntity(value.LeadDocumentID,(int)EEntityType.ImportDeclaration);
                }

            }
        }

        public CertificateOfOriginsItemDetails SelectedCertificateOfOriginsItemDetails
        { 
            get { return GetValue< CertificateOfOriginsItemDetails>(); }
            set { SetValue(value); }
        }

        public CertificateOfOriginsItemDetails AuthenticationRequestItemDetails
        {
            get { return GetValue<CertificateOfOriginsItemDetails>(); }
            set { SetValue(value); }
        }
        public ViewStates DocumentIDEditViewState
        {
            get { return GetValue<ViewStates>(); }
            set { SetValue(value); }
        }
        public Visibility ProgressBarVisibility
        {
            get { return GetValue<Visibility>(); }
            set { SetValue(value); }
        }

        public bool IsImportProcessEditable
        {
            get { return GetValue<bool>(); }
            set { SetValue(value); }
        }

        public bool IsEditModeEnabled
        {
            get { return GetValue<bool>(); }
            set { SetValue(value); }
        }

        public bool IsImportProcessDataGridDirty
        {
            get { return GetValue<bool>(); }
            set { SetValue(value); }
        }

        public CustomObservableCollection<INavigationMapping> ConnectedEntities
        {
            get { return GetValue<CustomObservableCollection<INavigationMapping>>(); }
            set { SetValue(value); }
        }

        public ViewStates ViewState
        {
            get { return GetValue<ViewStates>(); }
            set
            {
                SetValue(value);
                IsEditModeEnabledForEditBtn = ViewState == ViewStates.ReadOnly || ViewState == ViewStates.None;
            }
        }

        public bool IsEnabledInvoiceID
        {
            get { return GetValue<bool>(); }
            set { SetValue(value); }
        }

        public bool IsSaveAndExitPressed
        {
            get { return GetValue<bool>(); }
            set { SetValue(value); }
        }
        public bool IsEditButtonEnabled
        {
            get { return GetValue<bool>(); }
            set { SetValue(value); }
        }

        public List<DocumentDTO> AttachmentDocuments
        {
            get { return GetValue<List<DocumentDTO>>(); }
            set { SetValue(value); }
        }

        public List<InvoiceDetailDTO> InvoiceDetails
        {
            get { return GetValue<List<InvoiceDetailDTO>>(); }
            set { SetValue(value); }
        }

        public bool CloseAfterSaveTrue
        {
            get { return true; }
            set { SetValue(value); }
        }
        public AuthenticationRequestDTO AuthenticationRequestDTO
        {
            get { return GetValue<AuthenticationRequestDTO>(); }
            set { SetValue(value); }
        }

        public bool CloseAfterSaveFalse
        {
            get { return false; }
            set { SetValue(value); }
        }

        public string VendorPredicate
        {
            get { return GetValue<string>(); }
            set { SetValue(value); }
        }

        private int _documentID;       
        [DataMember]
       // [MandatoryField]
        [DisplayName(LocalStrings.CancellationRequestDetails)]
        [ListOfValuesMetadata(ItemsSourcePropertyName = "AttachmentDocuments", DisplayMemberPath = "Notes", SelectedValuePath = "ID")]
        public int DocumentID
        {
            get { return GetValue<int>(); }
            set { SetValue(value); }
        }

        public DocumentDTO SelectedDocument
        {
            get { return GetValue<DocumentDTO>(); }
            set { SetValue(value); }
        }

        public int? VendorID
        {
            get { return GetValue<int?>(); }
            set { SetValue(value); }
        }

        public int? InvoiceID
        {
            get { return GetValue<int?>(); }
            set { SetValue(value); }
        }

        public string Invoicenumber
        {
            get { return GetValue<string>(); }
            set { SetValue(value); }
        }

        public decimal? InvoiceGoodsItemTaxDifferences
        {
            get { return GetValue<decimal>(); }
            set { SetValue(value); }
        }

        public decimal? AllInvoicesGoodsItemTaxDifferences
        {
            get { return GetValue<decimal>(); }
            set { SetValue(value); }
        }

        public EDocumentType SelectedDocumentType
        {
            get { return GetValue<EDocumentType>(); }
            set { SetValue(value); }
        }

        public UserLOV SelectedUser
        {
            get { return GetValue<UserLOV>(); }
            set { SetValue(value); }
        }
        public List<CustomsItemDTO> CustomsItemsFilter
        {
            get { return GetValue<List<CustomsItemDTO>>(); }
            set { SetValue(value); }
        }


        public CustomWindowBase AuthenticationRequestPopUpWindow
        {
            get { return GetValue<CustomWindowBase>(); }
            set { SetValue(value); }
        }

        public bool IsEditModeEnabledForEditBtn
        {
            get { return GetValue<bool>(); }
            set
            {
                SetValue(value);
            }
        }

        public VirtualEntity EntityForCollateral
        {
            get { return GetValue<VirtualEntity>(); }

            set { SetValue(value); }
        }

        public List<int> DecisionFilterIds
        {
            get { return GetValue<List<int>>(); }
            set { SetValue(value); }
        }

        public List<CertificateOfOriginsDecision> DecisionForCoordinator
        {
            get { return GetValue<List<CertificateOfOriginsDecision>>(); }
            set { SetValue(value); }
        }
        public CertificateOfOriginsDecision CurrentDecision
        {
            get { return GetValue<CertificateOfOriginsDecision>(); }
            set { SetValue(value); }
        }

        public bool IsMeasurementTypeReadOnly
        {
            get { return GetValue<bool>(); }
            set { SetValue(value); }
        }
        public DateTime? IssuingDate
        {
            get { return GetValue<DateTime?>(); }
            set { SetValue(value); }
        }
        #endregion Properties

        #region Commands
        public ICommand PreferenceDocumentTypeIDChangedCommand
        {
            get { return GetCommand<ICommand>(); }
            set { SetCommand(value); }
        }
        public ICommand EditImportProcessCommand
        {
            get { return GetCommand<ICommand>(); }
            set { SetCommand(value); }
        }

        public ICommand SaveImportProcessCommand
        {
            get { return GetCommand<ICommand>(); }
            set { SetCommand(value); }
        }

        public ICommand CancelImportProcessCommand
        {
            get { return GetCommand<ICommand>(); }
            set { SetCommand(value); }
        }
        
        public ICommand SelectCustomItemCommand
        {
            get { return GetCommand<ICommand>(); }
            set { SetCommand(value); }
        }

        public ICommand SelectDocumentChangeCommand
        {
            get { return GetCommand<ICommand>(); }
            set { SetCommand(value); }
        }

        public ICommand SelectInvoiceChangeCommand
        {
            get { return GetCommand<ICommand>(); }
            set { SetCommand(value); }
        }

        public ICommand WindowLoadedCommand
        {
            get { return GetCommand<ICommand>(); }
            set { SetCommand(value); }
        }
        public ICommand ImportProcessFormViewmodelClosingCommand
        {
            get { return GetCommand<ICommand>(); }
            set { SetCommand(value); }
        }
        
        public ICommand SaveDocumentsCommand
        {
            get { return GetCommand<ICommand>(); }
            set { SetCommand(value); }
        }

        public ICommand IsExistsAdditionalRequestsForImporterOrVendorCommand
        {
            get { return GetCommand<ICommand>(); }
            set { SetCommand(value); }
        }

        public ICommand ImporterIDChangedCommand
        {
            get { return GetCommand<ICommand>(); }
            set { SetCommand(value); }
        }

        #endregion Commands

        #region Method

        /// <summary>
        /// Raises the current entity property changed.
        /// </summary>
        /// <param name="propertyName"></param>
        public void RaiseCurrentEntityPropertyChanged(string propertyName)
        {
            this.RaisePropertyChanged(propertyName);
        }



        #endregion Method
        public static MalamValidationArgument ValidateField(ImportProcessFormViewmodel vm,
                                                                MalamValidationArgument arg, string fieldName)
        {
            var argsList = new List<MalamValidationArgument>();
            switch (fieldName)
            {
                case "DocumentID":
                    {
                        if (vm.DocumentID <= 0)
                            argsList.Add(new MalamValidationArgument
                            {
                                Parameters = new List<MalamValidationParameter>
                                                              {
                                                                  new MalamValidationParameter {FieldName = "CancellationRequestDetails"},
                                                              },
                                ValidationType = EEntityValidationType.Warning,
                                UserMessage = EMessages.ValueNull,

                            });
                    }
                    break;
            }
            return CombineArguments(arg, argsList);
        }

    }
}
