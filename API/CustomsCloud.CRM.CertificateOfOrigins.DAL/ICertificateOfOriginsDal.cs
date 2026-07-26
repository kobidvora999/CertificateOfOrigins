using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore.DAL;

namespace CustomsCloud.CRM.CertificateOfOrigins.DAL;

public interface ICertificateOfOriginsDal : IBaseDal
{
    Task<int?> GetCertificateOfOriginIdByNumber(string certificateNumber);

    Task<List<CertificateOfOriginResultDto>> GetCertificateOfOriginsByFilter(object? parameters);

    Task<List<GetImportAuthenticationRequestResultDto>> GetImportAuthenticationRequestByFilter(object? parameters);

    Task<List<GetExportDocumentAuthenticationRequestSearchResultDto>> GetExportDocumentAuthenticationRequestSearch(object? parameters);

    Task<int?> CheckImporterOfImportAuthentication(int importerId);

    Task<bool> CheckIfExistsAdditionalRequestsForVendor(int vendorId);

    Task<bool> CheckIfExistsAdditionalRequestsForImporter(int importerId, int? vendorId, int? customerId, int countryId);
}
