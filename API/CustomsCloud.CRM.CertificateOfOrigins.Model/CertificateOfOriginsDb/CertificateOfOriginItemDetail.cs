using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CustomsCloud.CRM.CertificateOfOrigins.Model.CertificateOfOriginsDb;

// CRM.CertificateOfOrigins_CertificateOfOriginItemDetail — the goods-item rows under a certificate invoice. Read-only
// reconciliation (UpdateCertificateOfOrigins) uses CustomsItemID; the full column set (mapped from the EF4 EDMX) is the
// write shape the GetPC_MSG2280_2281 create branch persists.
[Table("CertificateOfOrigins_CertificateOfOriginItemDetail", Schema = "CRM")]
public class CertificateOfOriginItemDetail
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("CertificateOfOriginInvoiceDetailID")]
    public int CertificateOfOriginInvoiceDetailId { get; set; }

    [Column("PackingTypeID")]
    public int? PackingTypeId { get; set; }

    [Column("CustomsItemID")]
    public int? CustomsItemId { get; set; }

    [Column("GrossWeight")]
    public decimal GrossWeight { get; set; }

    [Column("ItemGoodsDescription")]
    public string ItemGoodsDescription { get; set; } = null!;

    [Column("MarksAndNumbers")]
    public string MarksAndNumbers { get; set; } = null!;

    [Column("MeasurementUnitID")]
    public int MeasurementUnitId { get; set; }

    [Column("OriginCriterionID")]
    public int? OriginCriterionId { get; set; }

    [Column("Quantity")]
    public int Quantity { get; set; }

    [Column("RowNum")]
    public int RowNum { get; set; }

    [Column("FullClassification")]
    public string FullClassification { get; set; } = null!;

    [Column("ContainerISOCode")]
    public string? ContainerIsoCode { get; set; }
}
