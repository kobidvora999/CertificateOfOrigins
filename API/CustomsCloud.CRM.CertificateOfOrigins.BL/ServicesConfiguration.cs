using CustomsCloud.CRM.CertificateOfOrigins.BL.Proxies;
using CustomsCloud.CRM.CertificateOfOrigins.DAL;
using CustomsCloud.InfrastructureCore;
using CustomsCloud.InfrastructureCore.Interfaces.DependencyInjection;
using CustomsCloud.InfrastructureCore.Lookup;
using CustomsCloud.InfrastructureCore.Lookup.Infrastructure;
using CustomsCloud.InfrastructureCore.Parameters;
using CustomsCloud.InfrastructureCore.Proxy.Rest;
using Lookup;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL;

public class ServicesConfiguration : IServicesConfiguration
{
    public void RegisterServices([NotNull] IConfiguration configuration, [NotNull] IServiceCollection services)
    {
        services.AddCustomsDbContext<CertificateOfOriginsDbContext, CertificateOfOriginsDbReadOnlyContext>();
        services.AddDataLayer<ICertificateOfOriginsDal, CertificateOfOriginsDal>();
        services.AddBusinessLayer<CertificateOfOriginsBl>();
        services.AddBusinessLayer<AuthenticationRequestBl>();
        services.AddBusinessLayer<ExportDocumentAuthenticationRequestBl>();

        // Platform mock-proxy convention: REAL is the default; a request selects the mock per interface via the
        // x-mock-proxy header. TODO(blocking): verify the real Customers endpoint (CustomersByIds) before ROLLOUT.
        services.AddHttpProxy();  // IHttpProxy for the real proxies (BaseCustomsProxy) + IProxyMockUtil + per-request mock selection
        services.AddProxy<ICustomerProxy, CustomerProxy, CustomerMockProxy>();

        // TODO(blocking): verify the real Vendors endpoint (VendorsByIds) before ROLLOUT.
        services.AddProxy<IVendorProxy, VendorProxy, VendorMockProxy>();

        // Milestone user-name enrichment for GetCertificateOfOriginById. The SP returns only the acting user id
        // (the cross-service Infrastructure.UserMng_User JOIN was removed).
        // TODO(blocking): verify the real Users endpoint (User/UsersByIds) before ROLLOUT.
        services.AddProxy<IUserProxy, UserProxy, UserMockProxy>();

        // TODO(blocking): the ExportDealFile microservice is not yet stood up — the mock is the practical
        // default (selected via x-mock-proxy); switch to the real endpoint once it exists.
        services.AddProxy<IExportDealFileProxy, ExportDealFileProxy, ExportDealFileMockProxy>();

        // Web-query field labels for GetCertificateRequestByGuid — legacy read them from SystemTables DataDictionaryField
        // (no ILookupUtil type exists for it). TODO(blocking): verify the real SystemTables endpoint before ROLLOUT.
        services.AddProxy<IDataDictionaryFieldProxy, DataDictionaryFieldProxy, DataDictionaryFieldMockProxy>();

        // Invoice currency codes for GetCertificateRequestByGuid — legacy read them from SystemTables CurrencyType
        // (no ILookupUtil type exists for it). TODO(blocking): verify the real SystemTables endpoint before ROLLOUT.
        services.AddProxy<ICurrencyTypeProxy, CurrencyTypeProxy, CurrencyTypeMockProxy>();

        // Entity documents for GetEntityDocuments (was IDocumentsExternalProxy.GetDocumentsByEntitySync).
        // TODO(blocking): verify the real Documents endpoint (Document/DocumentsByEntity) before ROLLOUT.
        services.AddProxy<IDocumentsProxy, DocumentsProxy, DocumentsMockProxy>();

        // Collateral + Tasks enrichment for GetAuthenticationRequestByID.
        // TODO(blocking): verify the real Collateral (Collateral/CollateralRequestByEntity) endpoint before ROLLOUT.
        services.AddProxy<ICollateralProxy, CollateralProxy, CollateralMockProxy>();

        // TODO(blocking): verify the real Tasks (Task/IsTaskExist) endpoint before ROLLOUT.
        services.AddProxy<ITasksProxy, TasksProxy, TasksMockProxy>();

        // QueryURL config for GetCertificateRequestByGuid + document-type filter for GetEntityDocuments
        // (both were Configuration.GetConfig<string>; keys seeded in the local Infrastructure.Parameters).
        // CertificateOfOriginQueryURL is already seeded in the local Infrastructure.Parameters table.
        services.AddParametersService();

        // Event raising for ChangeStatusAfterDeliverySent (was EventUtil.RaiseEvent) — resolved lazily via IEventUtil.
        services.AddEventUtil();

        // Attachment upload for SaveCertificateOfOriginAttachments (was IDocumentServiceAdapter.UploadDocumentAndSave)
        // — resolved lazily via IDocumentUtil.
        services.AddDocumentUtil();

        // Name enrichment for AuthenticationRequest search (Country + OrganizationUnit via ILookupUtil).
        services.AddLookup<Country>();
        services.AddLookup<OrganizationUnit>();

        // Document-type names for GetEntityDocuments (was SystemTablesUtil.GetCodeById<DocumentType>.Name).
        services.AddLookup<DocumentType>();
    }
}
