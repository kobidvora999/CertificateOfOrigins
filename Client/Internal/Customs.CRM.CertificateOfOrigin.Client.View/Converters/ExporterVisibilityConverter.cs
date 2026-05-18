using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using Customs.CRM.Entities;
using Customs.Inf.MMI.Common.Converters;

namespace Customs.CRM.CertificateOfOrigins.Client.View.Converters
{
    [Serializable]
    [MarkupExtensionReturnType(typeof(Visibility))]
    [ValueConversion(typeof(int), typeof(Visibility))]
    public class ExporterVisibilityConverter : ConverterBase<object, Type>, IValueConverter
    {
        public override object Convert(object values, Type targetType, object parameter, CultureInfo culture)
        {
            var visibility = Visibility.Collapsed;

            int certificateType;
            var isDefined = int.TryParse(values.ToString(), out certificateType);

            if (isDefined)
            {
                if (certificateType != (int)ECertificateOfOriginType.NonManipulation)
                {
                    visibility = Visibility.Visible;
                }
            }

            return visibility;
        }

        public override object ConvertBack(object value, Type targetTypes, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
