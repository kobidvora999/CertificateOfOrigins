using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Customs.CRM.CertificateOfOrigins.InternalCommon.Common;
using Customs.CRM.External;
using Customs.Inf.MMI.Common.Navigation;
using Customs.Inf.MMI.Services.Api.NavigationManager;
using MalamTeam.Infrastructure.GeneralServices.Environment.Enums;

namespace Customs.CRM.CertificateOfOrigins.Client.View.Converters
{
    /// <summary>
    /// ZeroToNullConverter
    /// </summary>
    public class NavigationToVendorConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] == null || values[1] == null || !(values[0] is int) || (values[0] is int && System.Convert.ToInt32(values[0]) == 0)) return null;

            var vendorID = System.Convert.ToInt32(values[0]);
            var navObj = new NavigationObjectBase
            {
                EntityType = EEntityType.Vendor,
                EntityId = vendorID,
            };
            var view = values[1] as NavigationToVendorView;

            var arg = ConstractRoutedNavigationEventArgs(view, navObj);
            return arg;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
        
        private static RoutedNavigationEventArgs ConstractRoutedNavigationEventArgs(NavigationToVendorView view, NavigationObjectBase navObj)
        {
            var queue = new Queue<NavigationPathItem>();
            queue.Enqueue(new NavigationPathItem(navObj));

            //get the root element of the view
            var rootParentPath = view.ViewPaths.Single(vp => vp.ParentPathRouteID == null);
            InsertToNavigationQueue(rootParentPath, view, queue);

            return new RoutedNavigationEventArgs(navObj, NavigationMappingType.Edit, queue);
        }

        private static void InsertToNavigationQueue(NavigationToVendorPath path, NavigationToVendorView view, Queue<NavigationPathItem> queue)
        {
            var navigationPathItemType = path.ViewID != null ? NavigationPathItemType.View : NavigationPathItemType.Page;
            queue.Enqueue(new NavigationPathItem(path.Name, navigationPathItemType));

            var parentPath = view.ViewPaths.SingleOrDefault(vp => vp.ParentPathRouteID == path.ID);
            if (parentPath != null)
                InsertToNavigationQueue(parentPath, view, queue);
            else if (path.ViewID.HasValue)
                view.ViewID = (int)path.ViewID;
        }
    }
}
