using CustomsCloud.CRM.CertificateOfOrigins.BL.Proxies;
using CustomsCloud.CRM.CertificateOfOrigins.DAL;
using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore;
using CustomsCloud.InfrastructureCore.BL;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL;

public class ExportDocumentAuthenticationRequestBl(IServiceProvider serviceProvider, ICustomerProxy customerProxy)
    : BaseBL<ExportDocumentAuthenticationRequestBl, ICertificateOfOriginDal>(serviceProvider)
{
    public async Task<CustomerDto?> GetCustomerInformation(int customerId)
    {
        var customer = await customerProxy.GetCustomerByIdentification(customerId);
        if (customer == null)
        {
            throw new RestValidationException(customerId.ToString(), "Invalid identification number", "404");
        }
        return customer;
    }

    public async Task<CustomerDto?> GetCustomerInformationByCountry(int countryId)
    {
        // ECustomerActivityType.Foreign_customs_house = 40 (MalamTeam.Infrastructure.GeneralServices.Environment.Enums.ECustomerActivityType)
        const int foreignCustomsHouseActivityType = 40;

        var customers = await customerProxy.GetCustomersByCountryId(countryId, foreignCustomsHouseActivityType);
        if (customers == null || customers.Count == 0)
        {
            throw new RestValidationException(countryId.ToString(), "לא הוגדר בית מכס למדינה זו. יש להגדיר כתובת מתאימה", "404");
        }
        return customers.FirstOrDefault();
    }
}
