using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Customs.CertificateOfOrigins.Entities;
using Customs.CRM.Entities;
using Customs.Inf.MMI.Services.Module.ClientManager;
using Customs.Infrastructure.UserManagement.ExternalProxy;
using MalamTeam.Infrastructure.GeneralServices.Environment.Enums;
using Microsoft.Practices.Unity;

namespace Customs.CRM.CertificateOfOrigins.Client.View.Converters
{
    /// <summary>
    /// ZeroToNullConverter
    /// </summary>
    public class IsEnabledByUserConverter : IValueConverter
    {
        /// <summary>
        /// Converts a value.
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || !(value is CertificateOfOriginsImportAuthenticationRequest)) return false;

            var request = value as CertificateOfOriginsImportAuthenticationRequest;

            var currentWithFullDetails = ClientEnvironment.Instance.MMIFacade.ClientUnityContainer.Resolve<IUsersExternalUtil>().CurrentWithFullDetails;

            if (request.CreateUserID == ClientEnvironment.Instance.ServerServices.UserManagementService.CurrentUser.UserID &&
                request.DecisionID == (int) EAuthenticationRequestDecision.NewAuthenticationRequest)
            {
                //יוצר הבקשה אינו מרכז והבקשה חדשה
                if (parameter != null && parameter.Equals("DecisionID") &&
                    !currentWithFullDetails.SpecializationIDs.Contains((int) ESpecialization
                        .Centralimportverificationrequests))
                    return false;
                return true;
            }

            return request.IsCurrentUserHandleRequest;
        }

        /// <summary>
        /// Converts a value.
        /// </summary>
        /// <param name="value">The value that is produced by the binding target.</param>
        /// <param name="targetType">The type to convert to.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
