namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

public class ExportGoodsItemInfoDto
{
    public int SequenceNumber { get; set; }

    public int GoodsItemId { get; set; }

    public int? OriginCountryId { get; set; }

    public int CustomsItemId { get; set; }

    public int? CertificateOfOriginId { get; set; }
}
