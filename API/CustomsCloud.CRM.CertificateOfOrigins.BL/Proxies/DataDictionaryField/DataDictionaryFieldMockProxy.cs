using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore.Proxy.Rest;
using System.Diagnostics.CodeAnalysis;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL.Proxies;

[ExcludeFromCodeCoverage]
public class DataDictionaryFieldMockProxy(IProxyMockUtil mockUtil) : IDataDictionaryFieldProxy, IMockProxy
{
    // Default = realistic dummy field labels; feature "DataDictionaryField.NotFound" flips to the not-found branch.
    public Task<List<DataDictionaryFieldDto>?> GetDataDictionaryFieldsByIds(List<int> fieldIds)
    {
        if (mockUtil.HasMockFeature("DataDictionaryField.NotFound"))
        {
            return Task.FromResult<List<DataDictionaryFieldDto>?>(null);
        }

        var result = fieldIds.Select(id => new DataDictionaryFieldDto
        {
            Id = id,                       // TODO: dummy data
            EnglishName = "Field " + id,   // TODO: dummy data
        }).ToList();
        return Task.FromResult<List<DataDictionaryFieldDto>?>(result);
    }
}
