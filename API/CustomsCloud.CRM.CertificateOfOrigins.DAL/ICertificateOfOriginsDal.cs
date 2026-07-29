using CustomsCloud.CRM.CertificateOfOrigins.Model.CertificateOfOriginsDb;
using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore.DAL;

namespace CustomsCloud.CRM.CertificateOfOrigins.DAL;

public interface ICertificateOfOriginsDal : IBaseDal
{
    Task<int?> GetCertificateOfOriginIdByNumber(string certificateNumber);

    Task<List<CertificateOfOriginResultDto>> GetCertificateOfOriginsByFilter(object? parameters);

    Task<List<GetImportAuthenticationRequestResultDto>> GetImportAuthenticationRequestByFilter(object? parameters);

    Task<List<GetExportDocumentAuthenticationRequestSearchResultDto>> GetExportDocumentAuthenticationRequestSearch(object? parameters);

    Task<List<GetAuthenticationRequestByLeadDocumentResultDto>> GetAuthenticationRequestByLeadDocumentIDs(object? parameters);

    Task<ExportDocumentAuthenticationRequest?> GetExportDocumentAuthenticationRequestById(int id);

    Task<CertificateOfOriginDto?> GetCertificateOfOriginById(int certificateOfOriginId);

    Task<CertificateOfOriginWebQueryDto?> GetCertificateOfOriginDataForWebQuery(object? parameters);

    Task<List<int>> GetImportAuthenticationRequestDocumentIdsByLeadDocumentId(int leadDocumentId);

    Task<List<int>> GetImportAuthenticationRequestDocumentIdsClaimedByOtherLeadDocuments(List<int> documentIds, int leadDocumentId);

    Task<bool> UpdateFileAfterDelivery(int fileId, int authenticationFileStatusId, int deliveryMethodId);

    Task<int?> CheckImporterOfImportAuthentication(int importerId);

    Task<bool> CheckIfExistsAdditionalRequestsForVendor(int vendorId);

    Task<bool> CheckIfExistsAdditionalRequestsForImporter(int importerId, int? vendorId, int? customerId, int countryId);
}
