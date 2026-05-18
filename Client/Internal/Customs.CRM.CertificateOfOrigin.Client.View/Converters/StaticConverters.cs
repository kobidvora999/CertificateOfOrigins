using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace Customs.CRM.CertificateOfOrigins.Client.View.Converters
{
    /// <summary>
    /// StaticConverters
    /// </summary>
    public static class StaticConverters
    {
        public static RejectCancelReasonVisibilityConverter RejectCancelReasonVisibilityConverter = new RejectCancelReasonVisibilityConverter();
        public static InvoiceTabVisibilityConverter InvoiceTabVisibilityConverter = new InvoiceTabVisibilityConverter();
        public static ReadOnlyPropertyViewStateConverter ReadOnlyPropertyViewStateConverter = new ReadOnlyPropertyViewStateConverter();
        public static ExporterVisibilityConverter ExporterVisibilityConverter = new ExporterVisibilityConverter();
        public static ZeroToNullConverter ZeroToNullConverter = new ZeroToNullConverter();
        public static ZeroOrNullToIsEnabledConverter ZeroOrNullToIsEnabledConverter = new ZeroOrNullToIsEnabledConverter();
        public static IsEnabledByUserConverter IsEnabledByUserConverter = new IsEnabledByUserConverter();
        public static NavigationToVendorConverter NavigationToVendorConverter = new NavigationToVendorConverter();
        public static ViewStateToBooleanConverter ViewStateToBooleanConverter = new ViewStateToBooleanConverter();

        public static readonly LeadDocumentIDNotZeroToNavigationEnabledConverter LeadDocumentIDNotZeroToNavigationEnabledConverter = new LeadDocumentIDNotZeroToNavigationEnabledConverter();

    }
}
