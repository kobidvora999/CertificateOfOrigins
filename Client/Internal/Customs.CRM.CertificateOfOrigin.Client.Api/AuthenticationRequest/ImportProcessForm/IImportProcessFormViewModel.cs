using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Customs.CertificateOfOrigins.Entities;
using Customs.Inf.MMI.Common.CAL;
using Customs.Inf.MMI.Common.Core;
using Customs.Inf.MMI.Common.Navigation;
using Customs.Infrastructure.DocumentManagement.ExternalCommon;
using MalamTeam.Infrastructure.GeneralServices.Environment.Enums;

namespace Customs.CRM.CertificateOfOrigins.Client.Api.AuthenticationRequest.ImportProcessForm
{
    /// <summary>
    /// IImportProcessFormViewModel
    /// </summary>
    public interface IImportProcessFormViewModel : IViewModel
    {
        #region Props
        CertificateOfOriginsImportAuthenticationRequest CurrentEntity { get; set; }
        DocumentDTO SelectedDocument{ get; set; }
        EDocumentType SelectedDocumentType{ get; set; }
        bool IsImportProcessEditable { get; set; }
        bool IsEditModeEnabled { get; set; }
        bool IsEditModeEnabledForEditBtn { get; set; }
        ViewStates ViewState { get; set; }
        bool IsImportProcessDataGridDirty { get; set; }
        CustomObservableCollection<INavigationMapping> ConnectedEntities { get; set; }
        bool IsSaveAndExitPressed { get; set; }
        #endregion
        #region Metohd
        /// <summary>
        /// Adds the version.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        void AddVersion(string name, object value);

        /// <summary>
        /// Resets the value.
        /// </summary>
        /// <param name="name">The name.</param>
        void ResetValue(string name);

        /// <summary>
        /// Raises the current entity property changed.
        /// </summary>
        void RaiseCurrentEntityPropertyChanged(string propertyName);

        #endregion Metohd

    }

}
