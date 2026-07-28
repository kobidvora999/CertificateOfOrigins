namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// A data-dictionary field row (legacy DataDictionaryField, read via SystemTablesUtil.GetCodeById in the
// monolith). Only the English label is consumed by the web query. Resolved via IDataDictionaryFieldProxy against
// the SystemTables microservice.
public class DataDictionaryFieldDto
{
    public int Id { get; set; }

    public string? EnglishName { get; set; }
}
