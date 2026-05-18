using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Customs.StockPileData.Customers.ExternalCommon.Common;

namespace Customs.CRM.CertificateOfOrigins.BL.ServiceAdapters
{
    public interface ICustomerServiceAdapter
    {
        /// <summary>
        /// Gets the customer DTO by customer identification.
        /// </summary>
        /// <param name="identificationParam">The identification param.</param>
        /// <returns></returns>
        CustomerDTO GetCustomerDTOByCustomerIdentification(CustomerIdentificationFilter identificationParam);

        List<CustomerDTO> GetCustomeDTOByCountryID(int countryID, int customerActivityTypeID);
    }
}
