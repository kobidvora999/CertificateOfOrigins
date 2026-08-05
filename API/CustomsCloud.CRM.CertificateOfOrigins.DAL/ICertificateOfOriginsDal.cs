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

    Task<bool> UpdateRequestDecisionAfterDelivery(int documentId, int decisionId);

    Task<(int DocumentId, int FileId)?> GetFirstRequestAlreadyLinkedToFile(List<int> documentIds);

    Task<int> InsertAuthenticationFile(CertificateOfOriginsImportAuthenticationFileDetails file);

    Task<bool> LinkRequestsToAuthenticationFile(List<int> documentIds, int fileId);

    Task<int?> CheckImporterOfImportAuthentication(int importerId);

    Task<bool> CheckIfExistsAdditionalRequestsForVendor(int vendorId);

    Task<bool> CheckIfExistsAdditionalRequestsForImporter(int importerId, int? vendorId, int? customerId, int countryId);

    Task<CertificateOfOriginsImportAuthenticationRequest?> GetImportAuthenticationRequestById(int documentId);

    Task<List<CertificateOfOriginsItemDetails>> GetItemDetailsByRequestId(int documentId);

    Task<List<CertificateOfOriginsDecision>> GetAllDecisions();

    Task<bool> IsSupplierDeliveryCountry(int countryId);

    Task<CertificateOfOriginsImportAuthenticationFileDetails?> GetAuthenticationFileById(int fileId);

    Task<List<CertificateOfOriginsImportAuthenticationRequest>> GetRequestsByFileId(int fileId);

    Task<List<CertificateOfOriginsAuthenticationFileStatus>> GetAllFileStatuses();

    Task<List<CertificateOfOriginsItemDetails>> GetItemDetailsByRequestIds(List<int> requestIds);

    Task<int> SaveExportDocumentAuthenticationRequest(ExportDocumentAuthenticationRequest entity);
}
