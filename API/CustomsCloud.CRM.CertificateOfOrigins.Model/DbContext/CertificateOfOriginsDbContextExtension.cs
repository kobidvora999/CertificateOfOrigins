using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;
using Dapper;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace CustomsCloud.CRM.CertificateOfOrigins.DAL;

public partial class CertificateOfOriginsDbContext
{
    // dbo.GetCertificateOfOriginsByFilter — dynamic-SQL search; exporter/agent titles are NULL from the SP
    // (customer JOINs removed) and enriched in the BL via the Customers proxy. A search legitimately returns
    // an empty set, so no row-count assertion is applied.
    public async Task<IEnumerable<CertificateOfOriginResultDto>> GetCertificateOfOriginsByFilter(object? parameters = null, CancellationToken cancellationToken = default)
    {
        var conn = Database.GetDbConnection();
        var cmd = new CommandDefinition(
            commandText: "dbo.GetCertificateOfOriginsByFilter",
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken,
            parameters: parameters);
        var result = await conn.QueryAsync<CertificateOfOriginResultDto>(cmd);
        return result;
    }

    // dbo.GetImportAuthenticationRequestByFilter — dynamic-SQL search; importer/vendor/country names are NULL
    // from the SP (cross-service JOINs removed) and enriched in the BL. A search legitimately returns an empty
    // set, so no row-count assertion is applied.
    public async Task<IEnumerable<GetImportAuthenticationRequestResultDto>> GetImportAuthenticationRequestByFilter(object? parameters = null, CancellationToken cancellationToken = default)
    {
        var conn = Database.GetDbConnection();
        var cmd = new CommandDefinition(
            commandText: "dbo.GetImportAuthenticationRequestByFilter",
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken,
            parameters: parameters);
        var result = await conn.QueryAsync<GetImportAuthenticationRequestResultDto>(cmd);
        return result;
    }

    // dbo.ExportDocumentAuthenticationRequestSearch — dynamic-SQL search; country/customer names are NULL from the
    // SP (cross-service JOINs removed) and enriched in the BL. A search legitimately returns an empty set, so no
    // row-count assertion is applied.
    public async Task<IEnumerable<GetExportDocumentAuthenticationRequestSearchResultDto>> GetExportDocumentAuthenticationRequestSearch(object? parameters = null, CancellationToken cancellationToken = default)
    {
        var conn = Database.GetDbConnection();
        var cmd = new CommandDefinition(
            commandText: "dbo.ExportDocumentAuthenticationRequestSearch",
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken,
            parameters: parameters);
        var result = await conn.QueryAsync<GetExportDocumentAuthenticationRequestSearchResultDto>(cmd);
        return result;
    }

    // dbo.GetAuthenticationRequestByLeadDocumentID — import-authentication requests for a set of lead-document ids
    // passed as a Shared.IntArray TVP (@LeadDocumentIDs). Country/org-unit names are NULL from the SP (cross-service
    // JOINs removed) and enriched in the BL. A lookup by ids legitimately returns an empty set — no row assertion.
    public async Task<IEnumerable<GetAuthenticationRequestByLeadDocumentResultDto>> GetAuthenticationRequestByLeadDocumentID(object? parameters = null, CancellationToken cancellationToken = default)
    {
        var conn = Database.GetDbConnection();
        var cmd = new CommandDefinition(
            commandText: "dbo.GetAuthenticationRequestByLeadDocumentID",
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken,
            parameters: parameters);
        var result = await conn.QueryAsync<GetAuthenticationRequestByLeadDocumentResultDto>(cmd);
        return result;
    }

    // dbo.CheckIfExistsAdditionalRequestsForVendor — scalar bit: >1 import-authentication request for the
    // vendor within the last 3 years.
    public async Task<bool> CheckIfExistsAdditionalRequestsForVendor(object? parameters = null, CancellationToken cancellationToken = default)
    {
        var conn = Database.GetDbConnection();
        var cmd = new CommandDefinition(
            commandText: "dbo.CheckIfExistsAdditionalRequestsForVendor",
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken,
            parameters: parameters);
        var result = await conn.ExecuteScalarAsync<bool>(cmd);
        return result;
    }

    // dbo.CheckIfExistsAdditionalRequestsForImporter — scalar bit: an additional import-authentication request
    // exists for the importer within the last @DaysForLastDelivery days (config, read inside the SP from the
    // local Infrastructure.Parameters), branching on vendor vs customer by the country's delivery config.
    public async Task<bool> CheckIfExistsAdditionalRequestsForImporter(object? parameters = null, CancellationToken cancellationToken = default)
    {
        var conn = Database.GetDbConnection();
        var cmd = new CommandDefinition(
            commandText: "dbo.CheckIfExistsAdditionalRequestsForImporter",
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken,
            parameters: parameters);
        var result = await conn.ExecuteScalarAsync<bool>(cmd);
        return result;
    }

    // dbo.GetCertificateOfOriginByID — a single certificate with its full graph, materialized from a 7-result-set SP
    // (1 header · 2 declaration errors · 3 detail type-code lookup · 4 details · 5 invoices · 6 invoice items · 7
    // milestones). Ports the legacy MaterializeForCertificateOfOrigin: result set 3 enriches each detail's type-code
    // (by id), result set 6 nests each invoice's item lines (by invoice id). Returns null when the id has no header row.
    public async Task<CertificateOfOriginDto?> GetCertificateOfOriginById(object? parameters = null, CancellationToken cancellationToken = default)
    {
        var conn = Database.GetDbConnection();
        var cmd = new CommandDefinition(
            commandText: "dbo.GetCertificateOfOriginByID",
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken,
            parameters: parameters);

        using var grid = await conn.QueryMultipleAsync(cmd);

        var certificate = (await grid.ReadAsync<CertificateOfOriginDto>()).FirstOrDefault();
        if (certificate == null)
        {
            return null;
        }

        var declarationErrors = (await grid.ReadAsync<CertificateOfOriginVsDeclarationErrorDto>()).ToList();
        var detailTypeCodes = (await grid.ReadAsync<CertificateDetailsTypeCodeDto>()).ToList();
        var details = (await grid.ReadAsync<CertificateOfOriginDetailDto>()).ToList();
        var invoices = (await grid.ReadAsync<CertificateOfOriginInvoiceDetailDto>()).ToList();
        var invoiceItems = (await grid.ReadAsync<CertificateOfOriginItemDetailDto>()).ToList();
        var milestones = (await grid.ReadAsync<CertificateMilestoneDto>()).ToList();

        // Computed in the legacy materializer: exporter (CustomerId) + customs-agent (CreateCustomerId).
        certificate.StakeholdersIds = [certificate.CustomerId, certificate.CreateCustomerId];
        certificate.Milestones = milestones;
        certificate.CertificateOfOriginVsDeclarationError = declarationErrors;

        // result set 3 → each detail's type-code (by CertificateDetailsTypeCodeId). No match ⇒ left null
        // (legacy assumed a matching type-code row always exists).
        var typeCodeById = detailTypeCodes
            .GroupBy(t => t.Id)
            .ToDictionary(g => g.Key, g => g.First());
        foreach (var detail in details)
        {
            typeCodeById.TryGetValue(detail.CertificateDetailsTypeCodeId, out var typeCode);
            detail.CertificateDetailsTypeCode = typeCode;
        }

        certificate.CertificateOfOriginDetails = details;

        // result set 6 → invoice item lines nested under their invoice (by CertificateOfOriginInvoiceDetailId).
        var itemsByInvoiceId = invoiceItems
            .GroupBy(i => i.CertificateOfOriginInvoiceDetailId)
            .ToDictionary(g => g.Key, g => g.ToList());
        foreach (var invoice in invoices)
        {
            if (itemsByInvoiceId.TryGetValue(invoice.Id, out var items))
            {
                invoice.CertificateOfOriginItemDetail = items;
            }
        }

        certificate.CertificateOfOriginInvoiceDetail = invoices;

        return certificate;
    }
}
