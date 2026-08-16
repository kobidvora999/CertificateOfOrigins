namespace CustomsCloud.CRM.CertificateOfOrigins.BL.Proxies;

public interface ICountryGroupProxy
{
    // Legacy: ISystemTablesUtil.GetTablesSync<CountryCountryGroup>(CountryID == countryId && CountryGroupID ==
    // countryGroupId) (UpdateCertificateOfOrigins reconciliation) — whether a country belongs to a country group,
    // used for the destination / origin country-group agreement checks.
    Task<bool> IsCountryInCountryGroup(int countryId, int countryGroupId);

    // Legacy: SystemTablesUtil.GetIdByCode<CountryGroup>(CountryGroup.PropID, id) (GetPC_MSG2280_2281 field validation) —
    // whether the given country-group id exists in the SystemTables CountryGroup table (a non-existent id is a
    // "value not in system" validation error).
    Task<bool> CountryGroupExists(int countryGroupId);
}
