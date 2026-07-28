namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// A currency-type row (legacy CurrencyType, read via SystemTablesUtil.GetCodeById in the monolith). Only the
// currency code is consumed by the web query. Resolved via ICurrencyTypeProxy against the SystemTables microservice.
public class CurrencyTypeDto
{
    public int Id { get; set; }

    public string? CurrencyCode { get; set; }
}
