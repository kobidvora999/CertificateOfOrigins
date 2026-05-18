using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using Customs.CRM.CertificateOfOrigion.Client.External.Api;
using Customs.CRM.Entities.CertificateOfOriginsDTO;
using Customs.Inf.MMI.Common.CAL;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.Base_Entities;

namespace Customs.CRM.CertificateOfOrigins.Client.Module.AuthenticationRequest.DealFileAuthenticationRequest
{
    /// <summary>
    /// DealFileAuthenticationRequestViewModelEditViewModel
    /// </summary>
    public class DealFileAuthenticationRequestViewModelEditViewModel : CustomsViewModel, IImportAuthenticationRequestExternalViewModel
    {
        /// <summary>
        /// Gets or sets the current entity.
        /// </summary>
        /// <value>The current entity.</value>
        public IEntity CurrentEntity
        {
            get { return GetValue<IEntity>(); }
            set { SetValue(value); }
        }

        public List<int> LeadDocumentIds
        {
            get { return GetValue<List<int>>(); }
            set { SetValue(value); }
        }

        public List<ImportAuthenticationRequestDTO> AuthenticationRequestForLeadDocument
        {
            get { return GetValue<List<ImportAuthenticationRequestDTO>>(); }
            set { SetValue(value); }
        }

        public Visibility ProgressBarVisibility
        {
            get { return GetValue<Visibility>(); }
            set { SetValue(value); }
        }

        public ImportAuthenticationRequestDTO SelectAuthenticationRequest
        {
            get { return GetValue<ImportAuthenticationRequestDTO>(); }
            set { SetValue(value); }
        }

        public ICustomDelegateCommand DealFileImportAuthenticationRequestRefreshCommand
        {
            get { return GetCommand<ICustomDelegateCommand>(); }
            set { SetCommand(value); }
        }

        public ICustomDelegateCommand GetAuthenticationRequestByLeadDocumentIDCommand
        {
            get { return GetCommand<ICustomDelegateCommand>(); }
            set { SetCommand(value); }
        }

        public ICustomDelegateCommand NavigateToAuthenticationRequest
        {
            get { return GetCommand<ICustomDelegateCommand>(); }
            set { SetCommand(value); }
        }



    }
}
