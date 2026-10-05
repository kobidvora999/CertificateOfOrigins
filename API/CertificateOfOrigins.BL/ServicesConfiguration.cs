using CertificateOfOrigins.BL.Lookups;
using CertificateOfOrigins.BL.Proxies;
using CertificateOfOrigins.DAL;
using CustomsCloud.InfrastructureCore;
using CustomsCloud.InfrastructureCore.Interfaces.DependencyInjection;
using CustomsCloud.InfrastructureCore.Lock;
using CustomsCloud.InfrastructureCore.Lookup;
using CustomsCloud.InfrastructureCore.Lookup.Infrastructure;
using CustomsCloud.InfrastructureCore.Parameters;
using CustomsCloud.InfrastructureCore.Proxy.Rest;
using CustomsCloud.InfrastructureCore.Queue;
using Lookup;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

namespace CertificateOfOrigins.BL;

public class ServicesConfiguration : IServicesConfiguration
{
    public void RegisterServices([NotNull] IConfiguration configuration, [NotNull] IServiceCollection services)
    {
        services.AddCustomsDbContext<CertificateOfOriginsDbContext, CertificateOfOriginsDbReadOnlyContext>();
        services.AddDataLayer<ICertificateOfOriginsDal, CertificateOfOriginsDal>();
        services.AddBusinessLayer<CertificateOfOriginsBl>();
        services.AddBusinessLayer<AuthenticationRequestBl>();
        services.AddBusinessLayer<ExportDocumentAuthenticationRequestBl>();

        // C14: the event-response BL (api/EventsResponse) — thin, delegates to AuthenticationRequestBl.
        services.AddBusinessLayer<EventsResponseBl>();

        // Platform mock convention (InfrastructureCore.Proxy 1.10.80+): REAL proxies are the default; a request
        // enables ALL in-service mocks via the single global header 'x-mock-mode: x-mock-mode' (IMockUtil.IsMockMode).
        // TODO(blocking): verify the real Customers endpoint (CustomersByIds) before ROLLOUT.
        services.AddHttpProxy();  // IHttpProxy for the real proxies (BaseCustomsProxy) + IProxyMockUtil + global mock-mode selection
        services.AddProxy<ICustomerProxy, CustomerProxy, CustomerMockProxy>();

        // TODO(blocking): verify the real Vendors endpoint (VendorsByIds) before ROLLOUT.
        services.AddProxy<IVendorProxy, VendorProxy, VendorMockProxy>();

        // Milestone user-name enrichment for GetCertificateOfOriginById. The SP returns only the acting user id
        // (the cross-service Infrastructure.UserMng_User JOIN was removed).
        // TODO(blocking): verify the real Users endpoint (User/UsersByIds) before ROLLOUT.
        services.AddProxy<IUserProxy, UserProxy, UserMockProxy>();

        // TODO(blocking): the ExportDealFile microservice is not yet stood up — the mock is the practical
        // default (enabled via x-mock-mode); switch to the real endpoint once it exists.
        services.AddProxy<IExportDealFileProxy, ExportDealFileProxy, ExportDealFileMockProxy>();

        // Web-query field labels for GetCertificateRequestByGuid — legacy read them from SystemTables DataDictionaryField
        // (no ILookupUtil type exists for it). TODO(blocking): verify the real SystemTables endpoint before ROLLOUT.
        services.AddProxy<IDataDictionaryFieldProxy, DataDictionaryFieldProxy, DataDictionaryFieldMockProxy>();

        // Entity documents for GetEntityDocuments (was IDocumentsExternalProxy.GetDocumentsByEntitySync).
        // TODO(blocking): verify the real Documents endpoint (Document/DocumentsByEntity) before ROLLOUT.
        services.AddProxy<IDocumentsProxy, DocumentsProxy, DocumentsMockProxy>();

        // Collateral + Tasks enrichment for GetAuthenticationRequestByID.
        // TODO(blocking): verify the real Collateral (Collateral/CollateralRequestByEntity) endpoint before ROLLOUT.
        services.AddProxy<ICollateralProxy, CollateralProxy, CollateralMockProxy>();

        // TODO(blocking): verify the real Tasks (Task/IsTaskExist) endpoint before ROLLOUT.
        services.AddProxy<ITasksProxy, TasksProxy, TasksMockProxy>();

        // Status-change messages for SaveExportDocumentAuthenticationRequest (was IMessageManagementExternalProxy via
        // the Common service). TODO(blocking): verify the real Message-Management (Message/SendMessage) endpoint before ROLLOUT.
        services.AddProxy<IMessageManagementProxy, MessageManagementProxy, MessageManagementMockProxy>();

        // SaveCertificateOfOrigin (#33) outbound proxies — the trade-agreement (CustomsBook), QR-code, Templates and
        // org-unit services are not yet stood up, so the mocks are the practical default (via x-mock-mode).
        // TODO(blocking): confirm each owning microservice + endpoint route before ROLLOUT.
        services.AddProxy<ICustomsBookProxy, CustomsBookProxy, CustomsBookMockProxy>();
        services.AddProxy<ICommonServicesProxy, CommonServicesProxy, CommonServicesMockProxy>();
        services.AddProxy<IOrganizationUnitProxy, OrganizationUnitProxy, OrganizationUnitMockProxy>();

        // QueryURL config for GetCertificateRequestByGuid + document-type filter for GetEntityDocuments
        // (both were Configuration.GetConfig<string>; keys seeded in the local Infrastructure.Parameters).
        // CertificateOfOriginQueryURL is already seeded in the local Infrastructure.Parameters table.
        services.AddParametersUtil();

        // Event raising for ChangeStatusAfterDeliverySent (was EventUtil.RaiseEvent) — resolved lazily via IEventUtil.
        services.AddEventUtil();

        // Attachment upload for SaveCertificateOfOriginAttachments (was IDocumentServiceAdapter.UploadDocumentAndSave)
        // — resolved lazily via IDocumentUtil.
        services.AddDocumentUtil();

        // Issue-by-worker publishing for SaveCertificateOfOrigin (was QueueUtilFactory → the IssueCertificateOfOrigin
        // RabbitMQ exchange) — resolved lazily via IQueueUtil.
        services.AddQueueUtil();

        // CR 194221 — document rendering through the Templates microservice (CertificateOfOriginsBl.Templates.cs), via
        // ITemplateUtil. Note this is a different path from the SSRS certificate rendering, which stays on
        // ICommonServicesProxy.GenerateTemplate.
        services.AddTemplateUtil();

        // Optional per-certificate distributed lock for GetPC_MSG2280_2281 (was LockFactory.GetLock) — gated at runtime
        // by the IsNeedToLockCertificateOfOrigin parameter; ILockUtil.
        services.AddLockServices();

        // Name enrichment for AuthenticationRequest search (Country + OrganizationUnit via ILookupUtil).
        services.AddLookup<Country>();
        services.AddLookup<OrganizationUnit>();

        // Detail id→name display enrichment for SaveCertificateOfOrigin (city via ILookupUtil). No lookup type exists
        // for country-group / international-site — those id→name lookups need a SystemTables proxy (rollout TODO).
        services.AddLookup<City>();

        // Document-type names for GetEntityDocuments (was SystemTablesUtil.GetCodeById<DocumentType>.Name).
        services.AddLookup<DocumentType>();

        // Invoice item measure-type code → id for GetPC_MSG2280_2281 (was SystemTablesUtil.GetIdByCode<MeasurementUnit>).
        services.AddLookup<MeasurementUnit>();

        // TODO(internal): lookup types missing from the platform Lookup package — local stand-ins (Lookups/), loaded and
        // cached like a platform lookup. Replace each with services.AddLookup<Lookup.T>() once the platform has the type.
        services.AddLocalLookup<Lookups.InternationalSite>(CustomsMicroServices.SystemTables);
        services.AddLocalLookup<Lookups.Site>(CustomsMicroServices.Sites);

        // General_enum_CountryGroup + the General_cl_CountryCountryGroup membership: loaded from the same source as the
        // platform Lookup.Country (General_c_Country), the Common service. TODO(internal): confirm the source with the platform.
        services.AddLocalLookup<Lookups.CountryGroup>(CustomsMicroServices.Common);
        services.AddLocalLookup<Lookups.CountryCountryGroup>(CustomsMicroServices.Common);

        // General_enum_CurrencyType: invoice currency code ↔ id (the message conversion and the web query). Loaded from Common,
        // like the other General tables. TODO(internal): confirm the source with the platform.
        services.AddLocalLookup<Lookups.CurrencyType>(CustomsMicroServices.Common);

        // CargoControl_c_PackingType: invoice item package-type code → id. Loaded from the Cargos service, the owner of the
        // CargoControl tables. TODO(internal): confirm the source with the platform.
        services.AddLocalLookup<Lookups.PackingType>(CustomsMicroServices.Cargos);

        // TODO(blocking): GetPathsForNavigationToVendor needs a NavigationPath lookup. NavigationPath is a shared
        // GeneralServices reference table with no platform lookup type yet — once InfrastructureCore.Lookup adds a
        // NavigationPath type AND a source service exposes GET /lookup/NavigationPath (both done internally, see
        // INTERNAL_INTEGRATION.md), register it here via AddLookup and the BL will populate ViewPaths. Until then the
        // BL returns an empty list.
    }
}
