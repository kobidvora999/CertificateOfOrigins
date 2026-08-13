namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// A packing type resolved by its common code (legacy SystemTablesUtil.GetIdByCode<PackingType>(PropCommonCode, code)).
// The invoice item-detail PackageType field carries a code that is resolved to the packing-type id. Resolved via the
// SystemTables microservice.
public class PackingTypeByCodeDto
{
    public int Id { get; set; }

    public string? CommonCode { get; set; }

    public string? EnglishName { get; set; }
}
