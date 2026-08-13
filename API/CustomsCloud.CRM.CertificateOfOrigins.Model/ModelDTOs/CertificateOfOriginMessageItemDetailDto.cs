namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// One item line under an invoice of the incoming certificate message (PC_NG_2280_MSG01 CertificateOfOriginRequestItemDetail).
public class CertificateOfOriginMessageItemDetailDto
{
    public int? ItemSerial { get; set; }

    public string? ItemId { get; set; }

    public string? OriginCriterion { get; set; }

    public string? MarksAndNumbers { get; set; }

    public int? PackageQuantity { get; set; }

    public string? PackageType { get; set; }

    public string? ItemDescription { get; set; }

    public decimal Weight { get; set; }

    public string? MeasureType { get; set; }

    public string? ContainerIsoCode { get; set; }
}
