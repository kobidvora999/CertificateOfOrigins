using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore.DAL;

namespace CustomsCloud.CRM.CertificateOfOrigins.DAL;

public interface ICertificateOfOriginDal : IBaseDal
{
    Task<List<CertificateOfOriginResultDto>?> GetCertificateOfOriginsByFilter(object? parameters);

    Task<CertificateOfOriginDto?> GetCertificateOfOriginById(object? parameters);

    Task<List<GetImportAuthenticationRequestResultDto>?> GetAuthenticationRequestByFilter(object? parameters);

    Task<ImportAuthenticationRequestDto?> GetAuthenticationRequestById(object? parameters);

    Task<List<CertificateOfOriginsDecisionDto>?> GetCertificateOfOriginsDecisions();

    Task<bool> IsVendorCountry(int issuingCountryId);

    Task<bool> CheckIfExistsAdditionalRequestsForVendor(object? parameters);

    Task<int?> CheckImporterOfImportAuthentication(int importerId);
}
