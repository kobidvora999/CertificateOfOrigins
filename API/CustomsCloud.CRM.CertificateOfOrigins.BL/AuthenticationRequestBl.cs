using CustomsCloud.CRM.CertificateOfOrigins.BL.Proxies;
using CustomsCloud.CRM.CertificateOfOrigins.DAL;
using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore.BL;
using CustomsCloud.InfrastructureCore.Lookup;
using CustomsCloud.InfrastructureCore.Lookup.Entities;
using CustomsCloud.InfrastructureCore.Parameters;
using CustomsCloud.InfrastructureCore.Utils.Users;
using Dapper;
using System.Data;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL;

public class AuthenticationRequestBl(
    IServiceProvider serviceProvider,
    ICollateralProxy collateralProxy,
    ITasksProxy tasksProxy,
    ICustomerProxy customerProxy,
    IVendorProxy vendorProxy,
    IDocumentsProxy documentsProxy,
    ILookupUtil lookupUtil,
    IParametersUtil parametersUtil)
    : BaseBL<AuthenticationRequestBl, ICertificateOfOriginDal>(serviceProvider)
{
    #region LEGACY_WCF

    // public List<DocumentDTO> GetEntityDocuments(CertificateOfOriginsImportAuthenticationRequest importAuthenticationRequest)
    // {
    //     var requestedDocuments = GetQuery<CertificateOfOriginsImportAuthenticationRequest>()
    //         .Where(cr => cr.LeadDocumentID == request.LeadDocumentID).ToList();
    //     var documentTypeIDs = Configuration.GetConfig<string>("CertificateOfOriginsDocumentsFilter"); // CSV
    //     var entityDocuments = Container.Resolve<IDocumentsExternalProxy>()
    //         .GetDocumentsByEntitySync(request.LeadDocumentID, EEntityType.ImportDeclaration);
    //     ... filters: exclude already-requested IDs, keep configured TypeIDs, exclude ID==0,
    //         exclude documents already used by a different lead document ...
    //     foreach: map to DocumentDTO { Notes = "{ID} {Title} {TypeName}", StringDynamicParams = entity.Notes,
    //              TypeName = SystemTablesUtil.GetCodeById<DocumentType>(TypeID).Name, OtherRelatedEntities }
    // }
    // The Documents microservice response already carries TypeName + OtherRelatedEntities, so the
    // DocumentType lookup is not needed in the new implementation.
    #endregion
    public async Task<List<DocumentDto>> GetEntityDocuments(ImportAuthenticationRequestDto importAuthenticationRequest)
    {
        var requestedDocumentIds = await DataLayer.GetRequestedDocumentIdsByLeadDocumentId(importAuthenticationRequest.LeadDocumentId);

        var documentTypeIds = await parametersUtil.Get<string>("CertificateOfOriginsDocumentsFilter");
        var documentFilter = documentTypeIds
            .Split(",", StringSplitOptions.RemoveEmptyEntries)
            .Select(s => Convert.ToInt32(s))
            .ToList();

        var entityDocuments = await documentsProxy.GetDocumentsByEntity(
            importAuthenticationRequest.LeadDocumentId,
            1055); // EntityType.ImportDeclaration = 1055 (CustomsCloud.Infrastructure.Documents.Model.DTO.EntityType)
        entityDocuments = entityDocuments?.Where(entDoc => !requestedDocumentIds.Contains(entDoc.Id)).ToList();
        if (entityDocuments == null)
        {
            return new List<DocumentDto>();
        }

        var entityDocumentsFilteredList = entityDocuments.Where(d => documentFilter.Any(f => f == d.TypeId)).ToList();
        if (requestedDocumentIds.Count > 0)
        {
            entityDocumentsFilteredList = entityDocumentsFilteredList
                .Where(d => requestedDocumentIds.All(requestedId => requestedId != d.Id) && d.Id != 0)
                .ToList();
        }

        var ids = entityDocumentsFilteredList.Select(e => e.Id).ToList();
        var documentIdsByOtherLeadDocumentId = await DataLayer.GetDocumentIdsUsedByOtherLeadDocuments(ids, importAuthenticationRequest.LeadDocumentId);
        if (documentIdsByOtherLeadDocumentId.Count > 0)
        {
            entityDocumentsFilteredList = entityDocumentsFilteredList
                .Where(d => !documentIdsByOtherLeadDocumentId.Contains(d.Id))
                .ToList();
        }

        if (entityDocumentsFilteredList.Count == 0)
        {
            return new List<DocumentDto>();
        }

        var docs = new List<DocumentDto>();
        foreach (var entityDocument in entityDocumentsFilteredList)
        {
            var doc = new DocumentDto
            {
                Id = entityDocument.Id,
                IsIncoming = entityDocument.IsIncoming,
                CreateDate = entityDocument.CreateDate,
                Title = entityDocument.Title,
                TypeName = entityDocument.TypeName,
                IsAccepted = entityDocument.IsAccepted,
                IsRequired = entityDocument.IsRequired,
                Notes = entityDocument.Id + " " + entityDocument.Title + " " + entityDocument.TypeName,
                ExternalId = entityDocument.ExternalId,
                TypeId = entityDocument.TypeId,
                StringDynamicParams = entityDocument.Notes,
                OtherRelatedEntities = entityDocument.OtherRelatedEntities ?? new List<EntityDocumentDto>()
            };
            docs.Add(doc);
        }

        return docs;
    }

    #region LEGACY_WCF

    // public List<GetImportAuthenticationRequestResult> GetAuthenticationRequestByFilter(ImportAuthenticationRequestFilter filter)
    // {
    //     var result = _uow.Repository.ExecuteFunction<GetImportAuthenticationRequestResult>(filter).ToList();
    //     return result;
    // }
    // The legacy SP [CRM].[usp_CertificateOfOrigins_GetImportAuthenticationRequestByFilter] assembled a
    // dynamic WHERE over CRM.CertificateOfOrigins_ImportAuthenticationRequest with optional predicates,
    // joined local tables (PrefernceDocumentType, ImportAuthenticationFileDetails) and remote-replica
    // tables for display names (Country, OrganizationUnit, Vendor, Customer, DealFile_LeadDocument),
    // TOP(ufn_GetMaxRows()) ordered by CreateDate DESC. Migrated to LINQ; display names are enriched via
    // ILookupUtil (Country, OrganizationUnit) and the Customers/Vendors microservices because those
    // tables are not replicated to this service's DB.
    #endregion
    public async Task<List<GetImportAuthenticationRequestResultDto>> GetAuthenticationRequestByFilter(ImportAuthenticationRequestFilterDto filter)
    {
        var result = await DataLayer.GetAuthenticationRequestByFilter(filter);
        await FillAuthenticationRequestNames(result);
        return result;
    }

    private async Task FillAuthenticationRequestNames(List<GetImportAuthenticationRequestResultDto> requests)
    {
        if (requests.Count == 0)
        {
            return;
        }

        var countries = await lookupUtil.All<Country>();
        var organizationUnits = await lookupUtil.All<OrganizationUnit>();

        var customerIds = requests.Where(r => r.CustomerId.HasValue).Select(r => r.CustomerId!.Value).Distinct().ToList();
        var customers = customerIds.Count > 0 ? await customerProxy.GetCustomersByIds(customerIds) : null;

        var vendorIds = requests.Where(r => r.VendorId.HasValue).Select(r => r.VendorId!.Value).Distinct().ToList();
        var vendors = vendorIds.Count > 0 ? await vendorProxy.GetVendorsByIds(vendorIds) : null;

        foreach (var request in requests)
        {
            request.IssuingCountryId = countries?.FirstOrDefault(c => c.Id == request.IssuingCountryIdNum)?.Name;
            request.OrganizationUnitId = organizationUnits?.FirstOrDefault(o => o.Id == request.OrganizationUnitIdNum)?.Name;
            request.ImporterName = customers?.FirstOrDefault(c => c.Id == request.CustomerId)?.Name;
            request.VendorName = vendors?.FirstOrDefault(v => v.Id == request.VendorId)?.Title;

            // TODO (blocking): LeadDocumentTitle came from CRP.DealFile_LeadDocument — no DealFile
            // microservice exists in CustomsMicroServices; the data source must be decided with the team.
            request.LeadDocumentTitle = null;
        }
    }
}
