namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// The certificate body of the incoming message (PC_NG_2280_MSG01 CertificateOfOrigin) — the ~30 certificate fields the
// agent supplies, plus the invoice detail lines. Field names/types mirror the legacy EAI contract; the XML *Specified
// flags are dropped (null = not supplied).
public class CertificateOfOriginMessageDto
{
    public List<CertificateOfOriginMessageInvoiceDetailDto> InvoiceDetails { get; set; } = [];

    public string? ExporterId { get; set; }

    public string? ExporterName { get; set; }

    public string? ExporterAddress { get; set; }

    public string? ExporterCountry { get; set; }

    public string? TradeAgreementCountry1 { get; set; }

    public string? TradeAgreementCountry2 { get; set; }

    public int? TradeAgreementGroupOfCountries { get; set; }

    public string? ConsigneeName { get; set; }

    public string? ConsigneeAddress { get; set; }

    public string? ConsigneeCountry { get; set; }

    public string? ConsigneeRemarks { get; set; }

    public bool? IsConsigneeForPrint { get; set; }

    public string? OriginCountry { get; set; }

    public int? OriginGroupOfCountries { get; set; }

    public string? DestinationCountry { get; set; }

    public int? DestinationGroupOfCountries { get; set; }

    public string? Transport { get; set; }

    public string? PortOfShipment { get; set; }

    public bool? IsCumulation { get; set; }

    public string? CumulationCountry { get; set; }

    public int? CumulationGroupOfCountries { get; set; }

    public int? PlaceOfManufacture { get; set; }

    public int? ZipCodeOfManufacture { get; set; }

    public string? Observations { get; set; }

    public bool? IsExportDecForPrint { get; set; }

    public string? CustomsHouse { get; set; }

    public string? IssuingCountry { get; set; }

    public int? CityOfDeclaration { get; set; }

    public string? CountryOfDeclaration { get; set; }

    public DateTime DateOfDeclaration { get; set; }

    public bool? IsDeclaredByManufacturer { get; set; }

    public bool? IsDeclaredByExporter { get; set; }

    public bool? IsAttachedList { get; set; }

    public bool? InSufficentworkingInd { get; set; }

    public string? InsufficentWorkingText { get; set; }
}
