using CertificateOfOrigins.Model.ModelDTOs;

namespace CertificateOfOrigins.BL.Proxies;

public interface IMeasurementUnitProxy
{
    Task<List<MeasurementUnitByCodeDto>?> GetMeasurementUnitsByCodes(List<string> externalIdNumbers);
}
