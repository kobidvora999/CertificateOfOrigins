using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Customs.CRM.CertificateOfOrigins.BL.ServiceAdapters
{
    public interface IUserServiceAdapter
    {
        bool IsOrganzationUnitCustomHouse(int organizationUnitID);
    }
}
