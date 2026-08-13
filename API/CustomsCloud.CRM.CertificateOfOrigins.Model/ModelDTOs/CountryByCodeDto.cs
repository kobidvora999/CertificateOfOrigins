namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// A country resolved by its 2-letter alpha code (legacy SystemTablesUtil.GetIdByCode<Country>(PropCountryAlphaCode_2,
// code)). The incoming certificate message carries country alpha-2 codes; the create branch resolves them to country
// ids. ILookupUtil<Country> is by-id only (no alpha code on the ILookup contract), so this is resolved via the
// SystemTables microservice.
public class CountryByCodeDto
{
    public int Id { get; set; }

    public string? AlphaCode2 { get; set; }

    public string? EnglishName { get; set; }
}
