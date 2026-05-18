using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Customs.CRM.CertificateOfOrigins.Client.View.Converters
{
    public class LeadDocumentIDNotZeroToNavigationEnabledConverter : IValueConverter
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
            if (value == null || value == DependencyProperty.UnsetValue || !(value is int)) return false;
            var leadDocumentID = (int)value;
            return leadDocumentID != 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

      
    }
}