using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using Customs.Inf.MMI.Common.CAL;

namespace Customs.CRM.CertificateOfOrigins.Client.View.Converters
{
    /// <summary>
    /// ExportDocumentAuthenticationReadOnlyToCollapsed
    /// </summary>
    public class ExportDocumentAuthenticationReadOnlyToCollapsed : IValueConverter
    {
        /// <summary>
        /// Convert
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value != null
                    && value != DependencyProperty.UnsetValue
                    && value is ViewStates
                    && (ViewStates)value == ViewStates.ReadOnly)
                       ? Visibility.Collapsed
                       : Visibility.Visible;
        }

        /// <summary>
        /// ConvertBack
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
