using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore.Proxy.Rest;
using System.Diagnostics.CodeAnalysis;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL.Proxies;

// The real ExportDealFile service is not yet stood up, so this mock is the practical default for local/test
// runs (enabled via x-mock-mode). Default = a released declaration whose cargo has exited customs regulation
// (so LoadDataFromExportDeclaration can return true); feature "ExportDealFile.NotFound" flips to no details.
[ExcludeFromCodeCoverage]
public class ExportDealFileMockProxy(IProxyMockUtil mockUtil) : IExportDealFileProxy, IMockProxy
{
    public Task<ExportDeclarationDetailsDto?> GetExportDeclarationDetailsForCertificateOfOrigion(int? leadDocumentId, string? exportDeclarationNumber)
    {
        if (mockUtil.HasMockFeature("ExportDealFile.NotFound"))
        {
            return Task.FromResult<ExportDeclarationDetailsDto?>(null);
        }

        var result = new ExportDeclarationDetailsDto
        {
            LeadDocumentId = leadDocumentId ?? 0,          // TODO: dummy data
            IsDeclarationReleased = true,                  // TODO: dummy data
            IsCargoExitedOfCustomsRegulation = true,       // TODO: dummy data
            IsDeclarationInAmendmentProcess = false,
            LeadDocumentStateId = 0,                       // TODO: dummy data
        };
        return Task.FromResult<ExportDeclarationDetailsDto?>(result);
    }

    // Default = a fixed submission date; feature "DealFile.NoSubmissionDate" returns none.
    public Task<DateTimeOffset?> GetLeadDocumentSubmissionDate(int leadDocumentId)
    {
        if (mockUtil.HasMockFeature("DealFile.NoSubmissionDate"))
        {
            return Task.FromResult<DateTimeOffset?>(null);
        }

        return Task.FromResult<DateTimeOffset?>(new DateTimeOffset(2026, 1, 15, 0, 0, 0, TimeSpan.Zero)); // TODO: dummy data
    }

    // Default = a linked lead document whose title matches (no mismatch); feature "DealFile.NoLeadDocument" returns none.
    public Task<LeadDocumentByCertificateOfOriginDto?> GetLeadDocumentByOldCertificateOfOriginIdAndUpdateToNewCertificateOfOriginId(int oldCertificateOfOriginId, int newCertificateOfOriginId)
    {
        if (mockUtil.HasMockFeature("DealFile.NoLeadDocument"))
        {
            return Task.FromResult<LeadDocumentByCertificateOfOriginDto?>(null);
        }

        return Task.FromResult<LeadDocumentByCertificateOfOriginDto?>(new LeadDocumentByCertificateOfOriginDto
        {
            LeadDocumentId = (newCertificateOfOriginId * 10) + 7, // TODO: dummy data
            LeadDocumentTitle = null,                             // null → the BL does not flag a title mismatch
        });
    }

    // Default = NO associated declaration (null) so a cancellation is allowed; feature "DealFile.HasAssociatedDeclaration"
    // returns a lead document so the "linked declaration must be cancelled first" path is exercised.
    public Task<LeadDocumentByCertificateOfOriginDto?> GetLeadDocumentByCertificateOfOriginId(int certificateOfOriginId)
    {
        if (!mockUtil.HasMockFeature("DealFile.HasAssociatedDeclaration"))
        {
            return Task.FromResult<LeadDocumentByCertificateOfOriginDto?>(null);
        }

        return Task.FromResult<LeadDocumentByCertificateOfOriginDto?>(new LeadDocumentByCertificateOfOriginDto
        {
            LeadDocumentId = (certificateOfOriginId * 10) + 3, // TODO: dummy data
            LeadDocumentTitle = $"DECL-{certificateOfOriginId}", // TODO: dummy data
        });
    }

    // No-op repoint for local/testing.
    public Task ChangeCertificateOfOriginIdForLeadDocument(int leadDocumentId, int oldCertificateOfOriginId, int newCertificateOfOriginId)
    {
        return Task.CompletedTask;
    }

    // Default = a released declaration with one invoice/goods-item (feature "DealFile.NotReleased" flips the state so the
    // reconciliation gate does not fire; "DealFile.NoDeclarationInvoices" returns an empty invoice list so the
    // no-export-invoices mismatch path is exercised).
    public Task<ExportDeclarationInfoDto?> GetExportDeclarationInfoForPc(int leadDocumentId)
    {
        var invoices = mockUtil.HasMockFeature("DealFile.NoDeclarationInvoices")
            ? new List<ExportInvoiceInfoDto>()
            :
            [
                new ExportInvoiceInfoDto
                {
                    ExternalIdNum = "INV1", // TODO: dummy data — matches the mock certificate invoice number
                    ExportGoodsItemInfoList =
                    [
                        new ExportGoodsItemInfoDto { CustomsItemId = 123456, OriginCountryId = 32 }, // TODO: dummy data
                    ],
                },
            ];

        return Task.FromResult<ExportDeclarationInfoDto?>(new ExportDeclarationInfoDto
        {
            LeadDocumentId = leadDocumentId,
            LeadDocumentState = mockUtil.HasMockFeature("DealFile.NotReleased") ? 0 : (int)ELeadDocumentState.Released, // TODO: dummy data
            DestinationCountryId = 32,   // TODO: dummy data
            ExporterCustomerId = 888,    // TODO: dummy data
            OrganizationUnitId = 1,      // TODO: dummy data
            ExportInvoiceInfoList = invoices,
        });
    }

    // Default = one associated goods item whose origin country (32) the CustomsBook mock reports as in the trade
    // agreement (so the import-replacement warning does not fire); feature "DealFile.NoAssociatedGoodsItems" returns
    // an empty list so the warning path is exercised.
    public Task<List<ExportAssociatedGoodsItemDto>?> GetDetailsForExportAssociatedGoodsItemsByLeadDocumentId(int leadDocumentId)
    {
        if (mockUtil.HasMockFeature("DealFile.NoAssociatedGoodsItems"))
        {
            return Task.FromResult<List<ExportAssociatedGoodsItemDto>?>([]);
        }

        return Task.FromResult<List<ExportAssociatedGoodsItemDto>?>(
        [
            new ExportAssociatedGoodsItemDto { AssociatedOriginCountryId = 32 }, // TODO: dummy data
        ]);
    }
}
