namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// A customs item (tariff line) resolved from the CustomsBook service (legacy ServicesAdapter.GetCustomsItemsByIdsSync).
// UpdateCertificateOfOrigins reconciliation compares the first 6 digits of FullClassification between the certificate's
// goods items and the export declaration's.
public class CustomsItemDto
{
    public int Id { get; set; }

    public string? FullClassification { get; set; }
}

// The per-id + date filter the CustomsBook cache lookup expects (legacy CustomsItemsIdsCacheFilter). The date scopes
// the classification to the version valid at that time.
public class CustomsItemsIdsCacheFilterDto
{
    public int? CustomsItemId { get; set; }

    public DateTime Date { get; set; }
}
