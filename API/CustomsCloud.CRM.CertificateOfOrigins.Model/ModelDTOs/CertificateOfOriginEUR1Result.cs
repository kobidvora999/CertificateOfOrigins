namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// Data contract for the EUR1 certificate-of-origin template (legacy template id 136 / report id 7000). The generic
// template-data SP must return columns matching these property names; the Templates module merges the JSON (camelCase)
// into CertificateOfOriginEUR1.yml. Property names mirror the .docx SDT tags exactly so the camelCase field paths match.
public class CertificateOfOriginEUR1Result
{
    public string? CertificateID { get; set; }

    public string? ExporterName { get; set; }

    public string? ExporterAddress { get; set; }

    public string? ConsigneeName { get; set; }

    public string? ConsigneeAddress { get; set; }

    public string? CustomsHouse { get; set; }

    public string? DestinationCountryORGroupOfCountries { get; set; }

    public string? TradeAgreementCountryORGroupOfCountries { get; set; }

    public string? Transport { get; set; }

    public string? Observations { get; set; }

    public string? RequestReasonObservations { get; set; }

    public string? PlaceOfManufacture { get; set; }

    public string? ZipCodeOfManufacture { get; set; }

    public string? DeclarationPlaceAndDate { get; set; }

    public DateTime? IssuingDate { get; set; }

    // Legacy draft/final label ("טיוטה" / "סופי").
    public string? IsDraft { get; set; }

    // Enrichment (not from the certificate row): the QR barcode + stamp/signature images are filled in the BL.
    public string? QRCode { get; set; }

    public string? SiteStamp { get; set; }

    public string? UserSignature { get; set; }

    // The goods-item lines rendered by the REGEON_tbl1 repeating table (one entry per certificate item).
    public List<CertificateOfOriginEUR1GoodsLine> GoodsItems { get; set; } = [];
}

// One goods-item line in the EUR1 certificate table (the 6 columns of the REGEON_tbl1 row).
public class CertificateOfOriginEUR1GoodsLine
{
    public string? MarksAndNumbers { get; set; }

    public string? Quantity { get; set; }

    public string? PackingType { get; set; }

    public string? GoodsDescription { get; set; }

    public string? GrossWeightAndMeasureType { get; set; }

    public string? InvoiceNumber { get; set; }
}
