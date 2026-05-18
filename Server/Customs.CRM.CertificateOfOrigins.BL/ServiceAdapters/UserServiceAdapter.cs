using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Customs.Infrastructure.UserManagement.ExternalProxy;
using MalamTeam.Infrastructure.GeneralServices.BaseLayer;

namespace Customs.CRM.CertificateOfOrigins.BL.ServiceAdapters
{
    public class UserServiceAdapter : BaseServiceAdapter<IUsersExternalUtil>, IUserServiceAdapter 
    {
        public bool IsOrganzationUnitCustomHouse(int organizationUnitID)
        {
            return ExternalProxy.IsOrganzationUnitCustomHouse(organizationUnitID);
        }
    }
}
