using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CustomsCloud.CRM.CertificateOfOrigins.Model.CertificateOfOriginDb;

[Table("CertificateOfOrigins_CertificateOfOriginItemDetail", Schema = "CRM")]
public class CertificateOfOriginItemDetail
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("PackingTypeID")]
    public int? PackingTypeId { get; set; }

    [Column("CustomsItemID")]
    public int? CustomsItemId { get; set; }

    [Column(TypeName = "numeric(10, 2)")]
    public decimal GrossWeight { get; set; }

    [Column("CertificateOfOriginInvoiceDetailID")]
    public int CertificateOfOriginInvoiceDetailId { get; set; }

    [StringLength(4000)]
    public string ItemGoodsDescription { get; set; } = null!;

    [StringLength(255)]
    public string MarksAndNumbers { get; set; } = null!;

    [Column("MeasurementUnitID")]
    public int MeasurementUnitId { get; set; }

    [Column("OriginCriterionID")]
    public int? OriginCriterionId { get; set; }

    public int Quantity { get; set; }

    public int RowNum { get; set; }

    [StringLength(13)]
    public string FullClassification { get; set; } = null!;

    [Column("ContainerISOCode")]
    [StringLength(20)]
    public string? ContainerIsoCode { get; set; }
}
