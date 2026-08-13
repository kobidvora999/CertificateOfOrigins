namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// A measurement unit resolved by its external id number (legacy SystemTablesUtil.GetIdByCode<MeasurementUnit>(
// PropExternalIDNum, code)). The invoice item-detail MeasureType field carries a code that is resolved to the
// measurement-unit id. Resolved via the SystemTables microservice.
public class MeasurementUnitByCodeDto
{
    public int Id { get; set; }

    public string? ExternalIdNumber { get; set; }

    public string? EnglishName { get; set; }
}
