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
    /// ExportDocumentAuthenticationStatusViewStateConverter
    /// </summary>
    public class ExportDocumentAuthenticationStatusViewStateConverter : IMultiValueConverter
    {
        /// <summary>
        /// Convert
        /// </summary>
        /// <param name="values"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length != 2 || values[0] == null
                || values[0] == DependencyProperty.UnsetValue || !(values[0] is ViewStates)
                || values[1] == null || values[1] == DependencyProperty.UnsetValue || !(values[1] is bool)
                || (ViewStates) values[0] == ViewStates.ReadOnly || (bool) values[1])
                return ViewStates.ReadOnly;
            return ViewStates.Edit;
        }

        /// <summary>
        /// ConvertBack
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetTypes"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
