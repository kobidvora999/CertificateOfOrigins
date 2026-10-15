using CertificateOfOrigins.DAL;
using CertificateOfOrigins.Model.ModelDTOs.ResolverDto;
using CustomsCloud.InfrastructureCore.Interfaces.General;
using Microsoft.Extensions.DependencyInjection;

namespace CertificateOfOrigins.BL.Resolver;

// Distributed cache over CRM.CertificateOfOrigins_cf_CertificateOfOriginTypeByTradeAgreement (our own configuration
// table), keyed by certificate type: the trade agreements the type is issued under.
public sealed class CertificateTypeTradeAgreementsResolver(IServiceProvider serviceProvider, IServiceScopeFactory serviceScopeFactory)
    : BaseDistributedResolver<int, CertificateTypeTradeAgreementsResolverDto>(serviceProvider, 360, 15, null, false, ResolverSerializer.MessagePack)
{
    protected override async Task<Dictionary<int, CertificateTypeTradeAgreementsResolverDto>> LoadDataAsync(CancellationToken cancellationToken = default)
    {
        await using var scope = serviceScopeFactory.CreateAsyncScope();
        var dal = scope.ServiceProvider.GetRequiredService<ICertificateOfOriginsDal>();
        var result = await dal.GetTradeAgreementsByCertificateType();
        return result.ToDictionary(item => item.CertificateOfOriginTypeCodeId);
    }
}
