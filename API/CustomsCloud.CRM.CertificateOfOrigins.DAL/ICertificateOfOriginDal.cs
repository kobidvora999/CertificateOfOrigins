using CustomsCloud.CRM.CertificateOfOrigins.Model.CertificateOfOriginDb;
using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore.DAL;

namespace CustomsCloud.CRM.CertificateOfOrigins.DAL;

public interface ICertificateOfOriginDal : IBaseDal
{
    Task<List<CertificateOfOriginResultDto>> GetCertificateOfOriginsByFilter(object? parameters);

    Task<CertificateOfOriginDto?> GetCertificateOfOriginById(int certificateOfOriginId);

    Task<List<CertificateMilestoneRowDto>> GetCertificateMilestoneRows(string? certificateTitle);

    Task<List<GetImportAuthenticationRequestResultDto>> GetAuthenticationRequestByFilter(object? parameters);

    Task<List<int>> GetRequestedDocumentIdsByLeadDocumentId(int leadDocumentId);

    Task<List<int>> GetDocumentIdsUsedByOtherLeadDocuments(List<int> documentIds, int leadDocumentId);

    Task<ImportAuthenticationRequest?> GetFirstRequestWithAuthenticationFile(List<int> documentIds);

    Task<ImportAuthenticationFileDetails> CreateAuthenticationFile(ImportAuthenticationFileDetails file);

    Task<int> UpdateRequestsAuthenticationFileId(List<int> documentIds, int authenticationFileId);

    Task<ImportAuthenticationFileDetailsDto?> GetAuthenticationFileById(int fileId);

    Task<List<ImportAuthenticationRequestDto>> GetRequestsByAuthenticationFileId(int fileId);

    Task<List<CertificateOfOriginsItemDetailDto>> GetItemDetailsByRequestIds(List<int> requestIds);

    Task<List<CertificateOfOriginsDecisionDto>> GetAllDecisions();

    Task<List<CertificateOfOriginsAuthenticationFileStatusDto>> GetAllAuthenticationFileStatuses();

    Task<ImportAuthenticationRequestDto?> GetAuthenticationRequestById(int documentId);

    Task<bool> IsVendorCountry(int issuingCountryId);

    Task<List<ImportAuthenticationRequestByLeadDocumentDto>> GetAuthenticationRequestsByLeadDocumentIds(List<int> leadDocumentIds);

    Task<List<ExportDocumentAuthenticationRequestSearchResultDto>> GetExportDocumentAuthenticationRequestSearch(object? parameters);

    Task<ExportDocumentAuthenticationRequestDto?> GetExportDocumentAuthenticationRequestById(int id);

    Task<bool> CheckIfExistsAdditionalRequestsForImporter(int importerId, int? vendorId, int? customerId, int countryId, int daysForLastDelivery);

    Task<bool> CheckIfExistsAdditionalRequestsForVendor(int vendorId);

    Task<int?> CheckImporterOfImportAuthentication(int importerId);

    Task<ImportAuthenticationRequestDto> SaveImportAuthenticationRequest(ImportAuthenticationRequestDto dto);

    Task SaveAuthenticationFileDetails(ImportAuthenticationFileDetailsDto dto);

    Task<int> TouchRequestsUpdateDateByFileId(int fileId, DateTime updateDate);

    Task<int?> GetCertificateOfOriginIdByNumber(string certificateNumber);

    Task<string?> GetCertificateOfOriginTypeCodeName(int typeCodeId);
}
