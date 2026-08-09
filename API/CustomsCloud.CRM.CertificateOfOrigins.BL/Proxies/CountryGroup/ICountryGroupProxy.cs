namespace CustomsCloud.CRM.CertificateOfOrigins.BL.Proxies;

public interface ICountryGroupProxy
{
    // Legacy: ISystemTablesUtil.GetTablesSync<CountryCountryGroup>(CountryID == countryId && CountryGroupID ==
    // countryGroupId) (UpdateCertificateOfOrigins reconciliation) — whether a country belongs to a country group,
    // used for the destination / origin country-group agreement checks.
    Task<bool> IsCountryInCountryGroup(int countryId, int countryGroupId);
}
