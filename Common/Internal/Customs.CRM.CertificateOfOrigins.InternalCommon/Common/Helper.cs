using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Customs.CRM.CertificateOfOrigins.InternalCommon.Common
{
    public static class Helper
    {
        public static bool In<T>(this T source, params T[] list)
        {
            return list.Contains(source);
        }

        public static bool NotIn<T>(this T source, params T[] list)
        {
            return !list.Contains(source);
        }
    }
}
