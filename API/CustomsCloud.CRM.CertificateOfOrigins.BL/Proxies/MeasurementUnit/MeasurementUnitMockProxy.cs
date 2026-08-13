using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore.Proxy.Rest;
using System.Diagnostics.CodeAnalysis;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL.Proxies;

[ExcludeFromCodeCoverage]
public class MeasurementUnitMockProxy(IProxyMockUtil mockUtil) : IMeasurementUnitProxy, IMockProxy
{
    // Default = a resolved measurement unit per requested code; feature "MeasurementUnit.NotFound" flips to null.
    public Task<List<MeasurementUnitByCodeDto>?> GetMeasurementUnitsByCodes(List<string> externalIdNumbers)
    {
        if (mockUtil.HasMockFeature("MeasurementUnit.NotFound"))
        {
            return Task.FromResult<List<MeasurementUnitByCodeDto>?>(null);
        }

        var result = externalIdNumbers.Select(code => new MeasurementUnitByCodeDto
        {
            Id = DummyId(code),          // TODO: dummy data
            ExternalIdNumber = code,
            EnglishName = $"Unit {code}", // TODO: dummy data
        }).ToList();
        return Task.FromResult<List<MeasurementUnitByCodeDto>?>(result);
    }

    private static int DummyId(string code)
    {
        var sum = 0;
        foreach (var ch in code ?? string.Empty)
        {
            sum += ch;
        }

        return (sum % 1000) + 1;
    }
}
