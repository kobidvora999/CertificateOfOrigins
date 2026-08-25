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

    Task<(int State, int OrganizationUnitId)?> GetExportRequestProjectionColumns(int requestId);

    Task MergeExportDocumentAuthenticationRequestChildren(
        int requestId,
        List<CustomsItemToExportDocumentAuthenticationRequest> customsItems,
        List<ExportDocumentAuthenticationRequestLeadDocument> leadDocuments,
        List<ExportAuthenticationRequestManufacturingArea> manufacturingAreas);

    Task<bool> SaveImportAuthenticationRequest(SaveImportAuthenticationRequestRequestDto request, int userId);

    Task UpdateImportRequestDecision(int documentId, int? decisionId, bool isOldIndication, int userId);

    Task<bool> UpdateAuthenticationFile(SaveAuthenticationRequestFileRequestDto file, int userId);

    Task UnlinkAllRequestsFromFile(int fileId, int userId);

    Task<CertificateOfOrigin?> GetLatestCertificateByNumber(string certificateNumber);

    Task<CertificateOfOrigin?> GetLatestCertificateByNumberForFeedback(string certificateNumber);

    Task CancelCertificateFromMessage(int id, string rejectCancelReason, int userId);

    Task<int> GetNextCertificateOfOriginNumber();

    Task<OriginCriterion?> GetOriginCriterion(string originCriterionCode, int certificateOfOriginTypeCodeId);

    Task<List<DetailsPerCertificate>> GetDetailsPerCertificate(int certificateOfOriginTypeCodeId);

    Task StageCertificateOfOriginDetails(int certificateId, List<CertificateOfOriginDetails> details);

    Task StageCertificateOfOriginInvoiceItems(List<CertificateOfOriginInvoiceDetail> invoices);

    Task StageCertificateOfOriginInvoices(int certificateId, List<CertificateOfOriginInvoiceDetail> invoices);

    Task UpdateCertificatePublishingState(int id, DateTime issuingDate, bool isInPublishingProcess, int userId);

    Task UpdateCertificateDeclarationLink(int id, int? leadDocumentId, string? exportDeclarationNumber, int userId);

    Task UpdateCertificateQrCodePath(int id, string? qrCodePath, int userId);

    Task CancelPreviousCertificate(int id, string rejectCancelReasonSuffix, int userId);

    Task<List<CertificateOfOrigin>> GetCertificatesByIds(List<int> ids);

    Task<List<CertificateOfOriginDetails>> GetCertificateDetailsByCertificateIds(List<int> certificateIds);

    Task<List<CertificateReconcileInvoiceDto>> GetCertificateInvoiceDetailsByCertificateIds(List<int> certificateIds);

    Task<bool?> GetCertificateTypeIsCustomsItemMandatory(int certificateTypeId);

    Task<CertificateOfOriginTypeCode?> GetCertificateTypeCode(int certificateTypeId);

    Task UpdateCertificateReconciliation(int id, int statusId, string? exportDeclarationNumber, int? leadDocumentId, string? rejectCancelReason, int userId);

    Task AddCertificateVsDeclarationErrors(int certificateOfOriginId, List<string> errorTexts);
}
