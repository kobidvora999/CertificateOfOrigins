using MessagePack;

namespace CertificateOfOrigins.Model.ModelDTOs.ResolverDto;

// Cache payload for CertificateTypeTradeAgreementsResolver: the trade agreements of one certificate type.
[MessagePackObject]
public class CertificateTypeTradeAgreementsResolverDto
{
    [Key(0)]
    public int CertificateOfOriginTypeCodeId { get; set; }

    [Key(1)]
    public List<int> TradeAgreementIds { get; set; } = [];
}
