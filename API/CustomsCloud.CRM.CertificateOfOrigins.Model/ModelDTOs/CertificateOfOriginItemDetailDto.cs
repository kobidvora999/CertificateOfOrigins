namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

public class CertificateOfOriginItemDetailDto
{
    public int Id { get; set; }
    public int? PackingTypeId { get; set; }
    public int? CustomsItemId { get; set; }
    public decimal GrossWeight { get; set; }
    public int CertificateOfOriginInvoiceDetailId { get; set; }
    public string? ItemGoodsDescription { get; set; }
    public string? MarksAndNumbers { get; set; }
    public int MeasurementUnitId { get; set; }
    public int? OriginCriterionId { get; set; }
    public int Quantity { get; set; }
    public int RowNum { get; set; }
    public string? FullClassification { get; set; }
    public string? ContainerIsoCode { get; set; }
}
