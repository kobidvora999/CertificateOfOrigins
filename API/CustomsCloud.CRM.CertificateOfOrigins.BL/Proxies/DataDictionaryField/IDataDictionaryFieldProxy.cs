using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL.Proxies;

public interface IDataDictionaryFieldProxy
{
    Task<List<DataDictionaryFieldDto>?> GetDataDictionaryFieldsByIds(List<int> fieldIds);
}
