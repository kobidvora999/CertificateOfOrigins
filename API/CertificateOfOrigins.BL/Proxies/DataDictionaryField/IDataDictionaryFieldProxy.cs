using CertificateOfOrigins.Model.ModelDTOs;

namespace CertificateOfOrigins.BL.Proxies;

public interface IDataDictionaryFieldProxy
{
    Task<List<DataDictionaryFieldDto>?> GetDataDictionaryFieldsByIds(List<int> fieldIds);
}
