namespace CertificateOfOrigins.Model.ModelDTOs;

// The CustomsBook trade-agreement question, as legacy CustomsBookServicesAdapter sent it
// (KnowledgeStore.CustomsBook IfExistTradeAgreementForCountryFilter).
public class IfExistTradeAgreementForCountryFilterDto
{
    public int TradeAgreementId { get; set; }

    public int CountryOrGroupId { get; set; }

    public bool IsGroup { get; set; }

    public DateTime ReferenceDate { get; set; }
}
