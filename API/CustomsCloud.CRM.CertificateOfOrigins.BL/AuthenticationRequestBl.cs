using CustomsCloud.CRM.CertificateOfOrigins.DAL;
using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore.BL;
using Dapper;
using System.Data;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL;

public class AuthenticationRequestBl(IServiceProvider serviceProvider)
    : BaseBL<AuthenticationRequestBl, ICertificateOfOriginDal>(serviceProvider)
{
    private static DynamicParameters BuildParameterForProcedure(ImportAuthenticationRequestFilterDto filter)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@PrefernceDocumentType", filter.PrefernceDocumentType, DbType.Int32);
        parameters.Add("@GoodsOrigionCountry", filter.GoodsOrigionCountry, DbType.Int32);
        parameters.Add("@IssuingCountry", filter.IssuingCountry, DbType.Int32);
        parameters.Add("@ImportCountry", filter.ImportCountry, DbType.Int32);
        parameters.Add("@FromRequestDate", filter.FromRequestDate, DbType.DateTimeOffset);
        parameters.Add("@ToRequestDate", filter.ToRequestDate, DbType.DateTimeOffset);
        parameters.Add("@CustomsHouseId", filter.CustomsHouseId, DbType.Int32);
        parameters.Add("@RequestReason", filter.RequestReason, DbType.Int32);
        parameters.Add("@LeadDocumentId", filter.LeadDocumentId, DbType.Int32);
        parameters.Add("@ImporterId", filter.ImporterId, DbType.Int32);
        parameters.Add("@VendorId", filter.VendorId, DbType.Int32);
        parameters.Add("@DecisionId", filter.DecisionId, DbType.Int32);
        parameters.Add("@CustomerId", filter.CustomerId, DbType.Int32);
        parameters.Add("@DocumentId", filter.DocumentId, DbType.Int32);
        parameters.Add("@InvoiceNumber", filter.InvoiceNumber, DbType.String);
        parameters.Add("@DocumentNumber", filter.DocumentNumber, DbType.String);
        parameters.Add("@AuthenticationFileId", filter.AuthenticationFileId, DbType.Int32);
        parameters.Add("@CreateUserId", filter.CreateUserId, DbType.Int32);
        return parameters;
    }

    public async Task<List<GetImportAuthenticationRequestResultDto>?> GetAuthenticationRequestByFilter(ImportAuthenticationRequestFilterDto filter)
    {
        var parameters = BuildParameterForProcedure(filter);
        var result = await DataLayer.GetAuthenticationRequestByFilter(parameters);
        return result;
    }
}
