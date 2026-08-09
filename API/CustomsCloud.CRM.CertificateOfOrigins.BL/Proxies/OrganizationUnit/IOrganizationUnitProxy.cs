namespace CustomsCloud.CRM.CertificateOfOrigins.BL.Proxies;

public interface IOrganizationUnitProxy
{
    // Legacy: servicesAdapter.IsOrganzationUnitCustomHouse(orgUnitId) (SaveCertificateOfOrigin, CustomsHouse field
    // validation) — whether the given organization unit is a customs house.
    Task<bool> IsOrganizationUnitCustomsHouse(int organizationUnitId);
}
