using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Customs.StockPileData.Customers.ExternalCommon.Common;
using Customs.StockPileData.Customers.ExternalProxy;
using MalamTeam.Infrastructure.GeneralServices.BaseLayer;

namespace Customs.CRM.CertificateOfOrigins.BL.ServiceAdapters
{
    public class CustomerServiceAdapter :  BaseServiceAdapter<ICustomersExternalProxy>, ICustomerServiceAdapter
    {
        public CustomerDTO GetCustomerDTOByCustomerIdentification(CustomerIdentificationFilter identificationParam)
        {
            var customerDto = ExternalProxy.GetCustomerDTOByCustomerIdentificationSync_NotCached(identificationParam);
            return customerDto;
        }

        public List<CustomerDTO> GetCustomeDTOByCountryID(int countryID, int customerActivityTypeID)
        {
            var customersDto = ExternalProxy.GetCustomeDTOByCountryID(countryID, customerActivityTypeID);
            return customersDto;
        }
    }
    
}
