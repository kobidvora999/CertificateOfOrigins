namespace CertificateOfOrigins.BL.Proxies;

public interface IOrganizationUnitProxy
{
    // Legacy: servicesAdapter.IsOrganzationUnitCustomHouse(orgUnitId) → UserServiceAdapter → the Users service
    // (SaveCertificateOfOrigin / message CustomsHouse validation) — whether the given organization unit is a customs house.
    Task<bool> IsOrganizationUnitCustomsHouse(int organizationUnitId);
}
