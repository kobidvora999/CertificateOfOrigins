using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Customs.CertificateOfOrigins.Entities;
using Customs.Inf.MMI.Common.CAL;
using Customs.Infrastructure.SystemTables.ExternalCommon;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.Utils;
using Microsoft.Practices.Unity;

namespace Customs.CRM.CertificateOfOrigins.InternalCommon.Common
{
    public  class AuthenticationRequestFileHelper
    {
      
        public static bool IsVendor(int issuingCountryID)
        {
           var systemTablesUtil = UnityContainerManager.DefaultContainer.Resolve<ISystemTablesUtil>();
            if (systemTablesUtil == null) return false;

            try
            {
                var country = systemTablesUtil.GetIdByCode<CertificateOfOriginSupplierDeliveryCountryConfig>(
                    "ConutryID",
                    issuingCountryID);

                return country > 0;
            }
            catch (Exception e)
            {
                return false;
            }

            return false;
        }
    }
}
