using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL.Proxies;

public interface IMeasurementUnitProxy
{
    Task<List<MeasurementUnitByCodeDto>?> GetMeasurementUnitsByCodes(List<string> externalIdNumbers);
}
