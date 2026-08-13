namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// An international site resolved by its UN/LOCODE (legacy SystemTablesUtil.GetIdByCode<InternationalSite>(PropLocode,
// code)). The create-branch port/shipment fields (PortOfEntrance / ExitPort / ExportPort / PortOfShipment) carry a
// locode that is resolved to the international-site id + English name. Resolved via the SystemTables microservice
// (no ILookupUtil type exposes the locode → site mapping).
public class InternationalSiteByLocodeDto
{
    public int Id { get; set; }

    public string? Locode { get; set; }

    public string? EnglishName { get; set; }
}
