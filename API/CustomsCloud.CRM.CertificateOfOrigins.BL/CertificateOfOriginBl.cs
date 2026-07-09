using CustomsCloud.CRM.CertificateOfOrigins.BL.OutgoingMessages;
using CustomsCloud.CRM.CertificateOfOrigins.BL.Proxies;
using CustomsCloud.CRM.CertificateOfOrigins.DAL;
using CustomsCloud.CRM.CertificateOfOrigins.Model.CertificateOfOriginDb;
using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore.BL;
using CustomsCloud.InfrastructureCore.BL.Exceptions;
using CustomsCloud.InfrastructureCore.Lookup;
using CustomsCloud.InfrastructureCore.Lookup.Entities;
using CustomsCloud.InfrastructureCore.Parameters;
using CustomsCloud.InfrastructureCore.Queue;
using CustomsCloud.InfrastructureCore.Utils.Documents;
using CustomsCloud.InfrastructureCore.Utils.Events;
using CustomsCloud.InfrastructureCore.Utils.OutgoingMessage;
using CustomsCloud.InfrastructureCore.Utils.Templates;
using Dapper;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Text.Json;
using SharedEntityType = CustomsCloud.InfrastructureCore.Utils.Shared.EntityType;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL;

public class CertificateOfOriginBl(
    IServiceProvider serviceProvider,
    ICustomerProxy customerProxy,
    IUserProxy userProxy,
    IDocumentsProxy documentsProxy,
    IExportDealFileProxy exportDealFileProxy,
    ICommonProxy commonProxy,
    ICustomsBookProxy customsBookProxy,
    ITasksProxy tasksProxy,
    IParametersUtil parametersUtil,
    ILookupUtil lookupUtil)
    : BaseBL<CertificateOfOriginBl, ICertificateOfOriginDal>(serviceProvider)
{
    #region LEGACY_WCF

    // External Convert(ConnectedEntity): find certificate by number via GetCertificateOfOriginsByFilter,
    //   not found → InfException(ConversionOfEntityFaildEntityNotExist, ["תעודת מקור", number]);
    //   return VirtualEntity { ID, Title = result.Name, EntityType = CertificateOfOrigin(12319), CustomerID = CustomesAgentID }.
    // External GetCertificateOfOriginID(certificateNumber): latest certificate id by number (order by CreateDate desc).
    // External GetGoodsItemCerificateDTO(list): per item — resolve certificateOfOriginID by CertificateNumber (same query).
    // External SaveCertificateOfOriginAttachments(args): per rendered template — build DocumentDTO
    //   (Title = "{typeName} - {טיוטה|סופי}", FileName = "תעודת {typeName} מספר {number}.pdf",
    //    additional field 46 = certificate number when TypeID == EDocumentType.ExportCertificateOfOrigin(329)),
    //   delete existing certificate documents, upload the new content via the Documents service.
    #endregion
    public async Task<VirtualEntityDto> Convert(ConnectedEntityDto connectedEntity)
    {
        var filter = new CertificateOfOriginFilterDto { CertificateNumber = connectedEntity.EntityIdKey1 };
        var certificates = await GetCertificateOfOriginsByFilter(filter);
        var certificateOfOrigin = certificates.FirstOrDefault();
        if (certificateOfOrigin == null)
        {
            // legacy: InfException(EMessages.ConversionOfEntityFaildEntityNotExist, ["תעודת מקור", number]) → 404 (C1)
            throw new RestNotFoundException();
        }

        var result = new VirtualEntityDto
        {
            Id = certificateOfOrigin.Id,
            Title = certificateOfOrigin.Name,
            EntityType = 12319, // EntityType.CertificateOfOrigin
            CustomerId = certificateOfOrigin.CustomesAgentId
        };
        return result;
    }

    public async Task<int?> GetCertificateOfOriginID(string certificateNumber)
    {
        var result = await DataLayer.GetCertificateOfOriginIdByNumber(certificateNumber);
        return result;
    }

    public async Task<List<GoodsItemCerificateDto>> GetGoodsItemCerificateDTO(List<GoodsItemCerificateDto> goodsItemCerificateDTOs)
    {
        foreach (var item in goodsItemCerificateDTOs)
        {
            if (item.CertificateNumber != null)
            {
                item.CertificateOfOriginId = await DataLayer.GetCertificateOfOriginIdByNumber(item.CertificateNumber);
            }
        }

        return goodsItemCerificateDTOs;
    }

    public async Task<bool> SaveCertificateOfOriginAttachments(SaveCertificateAttachmentsArgsDto saveCertificateAttachmentsArgsDto)
    {
        var certificateTypeName = await DataLayer.GetCertificateOfOriginTypeCodeName(saveCertificateAttachmentsArgsDto.CertificateTypeId);

        foreach (var certificateTemplate in saveCertificateAttachmentsArgsDto.CertificatesTemplates)
        {
            var isDraft = saveCertificateAttachmentsArgsDto.CertificateRequestReasonCode == 12 // ERequestReason.Draft
                || saveCertificateAttachmentsArgsDto.AdditionalInfo == "isDraft";
            var document = new DocumentDto
            {
                Title = certificateTypeName + " - " + (isDraft ? "טיוטה" : "סופי"),
                FileName = string.Format("תעודת {0} מספר {1}.pdf", certificateTypeName, saveCertificateAttachmentsArgsDto.CertificateNumber),
                TypeId = certificateTemplate.DocumentTypeId,

                // TODO(blocking): legacy UserUtil.Current.OrganizationUnitID — the current user's organization
                // unit source in .NET 10 is unresolved (IUserUtil exposes the user id only).
                OrganizationUnitId = 1,
                EntityId = saveCertificateAttachmentsArgsDto.CertificateId,
                EntityTypeId = 12319 // EntityType.CertificateOfOrigin
            };
            if (document.TypeId == 329) // EDocumentType.ExportCertificateOfOrigin
            {
                document.DocumentAdditionalFieldValues = new List<DocumentAdditionalFieldValueDto>
                {
                    new()
                    {
                        Value = saveCertificateAttachmentsArgsDto.CertificateNumber,
                        DocumentAdditionaFieldId = 46 // CertificateOfOriginsConsts.DocumentAdditionaFieldIDForCertificateNumber
                    }
                };
            }

            var documentsOfCertificates = await documentsProxy.GetDocumentsByEntity(saveCertificateAttachmentsArgsDto.CertificateId, 12319);
            if (documentsOfCertificates != null && documentsOfCertificates.Count > 0)
            {
                await documentsProxy.DeleteDocuments(
                    documentsOfCertificates.Select(c => c.Id).ToList(),
                    saveCertificateAttachmentsArgsDto.CertificateId,
                    12319);
            }

            await documentsProxy.UploadDocumentAndSave(document, certificateTemplate.Content ?? Array.Empty<byte>());
        }

        return true;
    }

    #region LEGACY_WCF

    // public List<ImportAuthenticationRequestDTO> GetAuthenticationRequestByLeadDocumentIDs(List<int> leadDocumentIDs)
    // {
    //     TVP Shared.IntArray → ExecuteFunction<ImportAuthenticationRequestDTO>(
    //         [CRM].[usp_CertificateOfOrigins_GetAuthenticationRequestByLeadDocumentID], @LeadDocumentIDs);
    //     — SP joined local tables (requests, file details, PrefernceDocumentType/Decision/FileStatus enums)
    //       plus Shared.General_c_Country + Infrastructure.UserMng_OrganizationUnit (names) and
    //       CRP.DealFile_LeadDocument (LeadDocumentTitle). Converted to LINQ per module guidelines;
    //       country/org-unit names enriched via ILookupUtil; ImporterID/LastDeliveryForImporter were never
    //       populated by the legacy SP (parity preserved).
    // }
    #endregion
    public async Task<List<ImportAuthenticationRequestByLeadDocumentDto>> GetAuthenticationRequestByLeadDocumentIDs(List<int> leadDocumentIDs)
    {
        if (leadDocumentIDs == null || leadDocumentIDs.Count == 0)
        {
            return new List<ImportAuthenticationRequestByLeadDocumentDto>();
        }

        var result = await DataLayer.GetAuthenticationRequestsByLeadDocumentIds(leadDocumentIDs);
        if (result.Count == 0)
        {
            return result;
        }

        var countries = await lookupUtil.All<Country>();
        var organizationUnits = await lookupUtil.All<OrganizationUnit>();
        foreach (var request in result)
        {
            request.ImportCountryName = countries?.FirstOrDefault(c => c.Id == request.ImportCountryId)?.Name;
            request.OrganizationUnitName = organizationUnits?.FirstOrDefault(o => o.Id == request.OrganizationUnitId)?.Name;

            // TODO (blocking): LeadDocumentTitle came from CRP.DealFile_LeadDocument — no DealFile microservice;
            // the data source must be decided with the team.
            request.LeadDocumentTitle = null;
        }

        return result;
    }

    #region LEGACY_WCF

    // public List<CertificateOfOriginResult> GetCertificateOfOriginsByFilter(CertificateOfOriginFilter filter)
    // {
    //     var sqlParameters = new List<SqlParameter> { ... 16 parameters, one per filter field, each ?? DBNull.Value ... };
    //     var result = _uow.Repository.ExecuteFunction<CertificateOfOriginResult>(
    //         CertificateOfOriginsConsts.GetCertificateOfOriginsByFilterSP, sqlParameters);
    //     return (List<CertificateOfOriginResult>)result;
    // }
    // The legacy SP [CRM].[usp_CertificateOfOrigins_GetCertificateOfOriginsByFilter] assembled a dynamic
    // WHERE over CRM.CertificateOfOrigins_CertificateOfOrigin (one optional predicate per non-null filter
    // field), joined Shared.rStockPileData_Customers_Customer for exporter/agent titles and the local
    // status-code enum table for the status name, TOP(ufn_GetMaxRows()) ordered by CreateDate DESC.
    // Migrated to LINQ in the DAL; customer titles are enriched via the Customers microservice because
    // Shared.rStockPileData_Customers_Customer is not replicated to this service's DB.
    #endregion
    public async Task<List<CertificateOfOriginResultDto>> GetCertificateOfOriginsByFilter(CertificateOfOriginFilterDto filter)
    {
        var parameters = BuildParameterForProcedure(filter);
        var result = await DataLayer.GetCertificateOfOriginsByFilter(parameters);
        await FillCustomersInformation(result);
        return result;
    }

    private static DynamicParameters BuildParameterForProcedure(CertificateOfOriginFilterDto filter)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@CertificateNumber", filter.CertificateNumber, DbType.String);
        parameters.Add("@CertificateOfOriginStatusID", filter.CertificateOfOriginStatusId, DbType.Int32);
        parameters.Add("@CertificateOfOriginTypeID", filter.CertificateOfOriginTypeId, DbType.Int32);
        parameters.Add("@CustomsAgentID", filter.CustomsAgentId, DbType.Int32);
        parameters.Add("@CustomsHouseID", filter.CustomsHouseId, DbType.Int32);
        parameters.Add("@DestinationCountry", filter.DestinationCountry, DbType.Int32);
        parameters.Add("@ExportDeclarationID", filter.ExportDeclarationId, DbType.Int32);
        parameters.Add("@ExportDeclarationNum", filter.ExportDeclarationNum, DbType.String);
        parameters.Add("@ExporterCustomerID", filter.ExporterCustomerId, DbType.Int32);
        parameters.Add("@FromIssuingDate", filter.FromIssuingDate, DbType.DateTime);
        parameters.Add("@ToIssuingDate", filter.ToIssuingDate, DbType.DateTime);
        parameters.Add("@FromRequestDate", filter.FromRequestDate, DbType.DateTime);
        parameters.Add("@ToRequestDate", filter.ToRequestDate, DbType.DateTime);
        parameters.Add("@RequestReasonID", filter.RequestReasonId, DbType.Int32);
        parameters.Add("@VersionNumber", filter.VersionNumber, DbType.Int32);
        parameters.Add("@IsLastVersion", filter.IsLastVersion, DbType.Boolean);
        return parameters;
    }

    #region LEGACY_WCF

    // public CertificateOfOriginResult IsCertificateOfOriginByExternalIdExist(string certificateOfOriginExternalId)
    // {
    //     var filter = new CertificateOfOriginFilter { certificateNumber = certificateOfOriginExternalId };
    //     var result = _uow.Repository.ExecuteFunction<CertificateOfOriginResult>(filter).FirstOrDefault();
    //     return result;
    // }
    #endregion
    public async Task<CertificateOfOriginResultDto?> IsCertificateOfOriginByExternalIdExist(string certificateOfOriginExternalId)
    {
        var filter = new CertificateOfOriginFilterDto { CertificateNumber = certificateOfOriginExternalId };
        var certificates = await GetCertificateOfOriginsByFilter(filter);
        var result = certificates.FirstOrDefault();
        return result;
    }

    #region LEGACY_WCF

    // public CertificateOfOrigin GetCertificateOfOriginById(int certificateOfOriginId, bool isFromMessage = false)
    //     => GetCertificateOfOriginByIdSP(certificateOfOriginId);
    // GetCertificateOfOriginByIdSP executed [CRM].[usp_CertificateOfOrigins_GetCertificateOfOriginByID]
    // (7 result sets: certificate, vs-declaration errors, full details-type-code enum table, details,
    // invoices, item details, milestones) and assembled the graph in MaterializeForCertificateOfOrigin,
    // then set StakeholdersIDs = [CustomerID, CreateCustomerID].
    // The milestones result set joined Infrastructure.UserMng_User for user names — that table is not
    // replicated to this service's DB, so user names are enriched via the Users microservice instead.
    #endregion
    public async Task<CertificateOfOriginDto?> GetCertificateOfOriginById(int certificateOfOriginId)
    {
        var (certificate, milestoneRows) = await DataLayer.GetCertificateOfOriginById(certificateOfOriginId);
        if (certificate == null)
        {
            throw new RestNotFoundException(); // route-style endpoint → 404 (C1)
        }

        certificate.StakeholdersIds = new List<int>
        {
            certificate.CustomerId,       // יצואן
            certificate.CreateCustomerId  // סוכן המכס
        };
        certificate.Milestones = await GetCertificateMilestones(milestoneRows);
        return certificate;
    }

    private async Task<List<CertificateMilestonesDto>> GetCertificateMilestones(List<CertificateMilestoneRowDto> rows)
    {
        var userIds = rows.Where(r => r.UserId.HasValue).Select(r => r.UserId!.Value).Distinct().ToList();
        var users = userIds.Count > 0 ? await userProxy.GetUsersByIds(userIds) : null;
        var result = rows.Select(r => new CertificateMilestonesDto
        {
            VersionNumber = r.VersionNumber,
            ActionName = r.ActionName,
            CreateDate = r.CreateDate,
            RejectReason = r.RejectReason,
            UserName = users?.FirstOrDefault(u => u.Id == r.UserId)?.Title
        }).ToList();
        return result;
    }

    private async Task FillCustomersInformation(List<CertificateOfOriginResultDto> certificates)
    {
        if (certificates.Count == 0)
        {
            return;
        }

        var customerIds = certificates.Select(c => c.ExporterId)
            .Concat(certificates.Select(c => c.CustomesAgentId))
            .Distinct()
            .ToList();
        var customers = await customerProxy.GetCustomersByIds(customerIds);
        if (customers == null)
        {
            return;
        }

        var customersById = customers.ToDictionary(c => c.Id);
        foreach (var certificate in certificates)
        {
            if (customersById.TryGetValue(certificate.ExporterId, out var exporter))
            {
                certificate.ExporterTitle = exporter.Name;
                certificate.ExporterExternalIdNum = exporter.ExternalIdNum;
            }

            if (customersById.TryGetValue(certificate.CustomesAgentId, out var agent))
            {
                certificate.CustomesAgentTitle = agent.Name;
                certificate.CustomesAgentExternalIdNum = agent.ExternalIdNum;
            }
        }
    }

    #region LEGACY_WCF

    // Internal LoadDataFromExportDeclaration(certificateOfOrigin):
    //   if (LeadDocumentID != null || !ExportDeclarationNumber.IsNullOrEmpty())
    //   {
    //       var dto = Container.Resolve<IExportDealFileExternalServiceAdapter>()
    //           .GetExportDeclarationDetailsForCertificateOfOrigion(LeadDocumentID, ExportDeclarationNumber);
    //       certificateOfOrigin.IsDeclarationReleased = dto?.IsDeclarationReleased;
    //       certificateOfOrigin.IsCargoExitedOfCustomsRegulation = dto?.IsCargoExitedOfCustomsRegulation;
    //       return IsCargoExitedOfCustomsRegulation == true && RequestReasonCode != (int)ERequestReason.RetrospectiveCertificate;
    //   }
    //   return false;
    // The WPF presenter assigned the returned bool to CurrentEntity.IsDeclarationReleasedAndNotRetrospectiveCertificate,
    // so here the BL enriches that DTO field directly and returns the enriched certificate.
    #endregion
    public async Task<CertificateOfOriginDto> LoadDataFromExportDeclaration(CertificateOfOriginDto certificateOfOrigin)
    {
        certificateOfOrigin.IsDeclarationReleasedAndNotRetrospectiveCertificate = false;
        if (certificateOfOrigin.LeadDocumentId != null || !string.IsNullOrEmpty(certificateOfOrigin.ExportDeclarationNumber))
        {
            var exportDeclarationDetails = await exportDealFileProxy.GetExportDeclarationDetailsForCertificateOfOrigin(
                certificateOfOrigin.LeadDocumentId, certificateOfOrigin.ExportDeclarationNumber);

            certificateOfOrigin.IsDeclarationReleased = exportDeclarationDetails?.IsDeclarationReleased;
            certificateOfOrigin.IsCargoExitedOfCustomsRegulation = exportDeclarationDetails?.IsCargoExitedOfCustomsRegulation;
            if (certificateOfOrigin.IsCargoExitedOfCustomsRegulation.HasValue &&
                certificateOfOrigin.IsCargoExitedOfCustomsRegulation.Value &&
                certificateOfOrigin.RequestReasonCode != 2) // ERequestReason.RetrospectiveCertificate
            {
                certificateOfOrigin.IsDeclarationReleasedAndNotRetrospectiveCertificate = true;
            }
        }

        return certificateOfOrigin;
    }

    #region UpdateCetrificateOfOrigins (External) — event-driven certificate updates

    private const int LengthOfTaskStart = 70;                       // legacy CertificateOfOriginsConsts.LengthOfTaskStart
    private const int MaximumNumberOfCharactersOfTheField = 253;    // legacy CertificateOfOriginsConsts.MaximumNumberOfCharactersOfTheField
    private const string ThereIsNoMatchBetweenTheCertificateDataAndTheDeclaration = "אין התאמה בין נתוני התעודה להצהרה"; // legacy const (not a UIMessage)
    private const int CertificateOfOriginEntityTypeId = 12319;      // EntityType.CertificateOfOrigin
    private const int ExportCertificateOfOriginDocumentTypeId = 329; // EDocumentType.ExportCertificateOfOrigin

    private sealed class ValidationEntry
    {
        public required string Key { get; init; }

        public required string Text { get; init; }

        public required bool IsWarning { get; init; }
    }

    // dedup by key — legacy PassGroupdExceptionListToEntity grouped by UserMessage and kept the first per group
    private static void AddValidationEntry(List<ValidationEntry> entries, string key, string text, bool isWarning)
    {
        if (!entries.Exists(e => e.Key == key))
        {
            entries.Add(new ValidationEntry { Key = key, Text = text, IsWarning = isWarning });
        }
    }

    #region LEGACY_WCF

    // External UpdateCetrificateOfOrigins(dto) [IsOneWay]: guarded by config IsExportDeclarationActive,
    // dispatches by dto.EventType to 5 branches (4 BL methods; DeclarationReleased serves 2 event ids).
    // All dispatcher call sites passed certificateOfOrigin=null + isFromDealFile=true, so those
    // parameters were folded into the dto-only signatures below (parity preserved for this contract).
    #endregion
    public async Task UpdateCetrificateOfOrigins(UpdateCetificateOfOriginsDto updateCetificateOfOrigins)
    {
        var isExportDeclarationActive = await parametersUtil.Get<bool>("IsExportDeclarationActive");
        if (!isExportDeclarationActive)
        {
            return;
        }

        switch (updateCetificateOfOrigins.EventType)
        {
            case EEventType.ExportDeclarationSubmissionSucceeded:
                await UpdateCertrificateOfOrigins(updateCetificateOfOrigins);
                break;
            case EEventType.ExportDeclarationReleased:
            case EEventType.AssemblySharedReleaseAccepted:
                await DeclarationReleased(updateCetificateOfOrigins);
                break;
            case EEventType.ExportDeclarationAmendmentRequestCompleted:
                await ExportDeclarationAmendmentSuccess(updateCetificateOfOrigins);
                break;
            case EEventType.CancellationRequestCommited:
                await ExportDeclarationCancellationRequestCommited(updateCetificateOfOrigins);
                break;
        }
    }

    #region LEGACY_WCF

    // BL.DeclarationReleased: fetch certificates by ids; per item — fill declaration number/lead document,
    // PendingRelease→Published (+QR +attachments+feedback), CertificateReplacement→HandleCertificateReplacement,
    // save per item; then chain UpdateCertrificateOfOrigins for the items whose declaration number was filled.
    #endregion
    private async Task DeclarationReleased(UpdateCetificateOfOriginsDto dto)
    {
        var certificates = await DataLayer.GetCertificatesByIds(dto.CertificateOfOriginsIds);
        var chained = new UpdateCetificateOfOriginsDto
        {
            EventType = dto.EventType,
            ExportDeclarationNum = dto.ExportDeclarationNum,
            LeadDocumentId = dto.LeadDocumentId,
            OrganizationUnitId = dto.OrganizationUnitId,
            ExportInvoiceInfoList = dto.ExportInvoiceInfoList,
            CertificateOfOriginsIds = []
        };
        foreach (var item in certificates)
        {
            if (string.IsNullOrEmpty(item.ExportDeclarationNumber))
            {
                item.ExportDeclarationNumber = dto.ExportDeclarationNum;
                item.LeadDocumentId = dto.LeadDocumentId;
                chained.CertificateOfOriginsIds.Add(item.Id);
            }

            if (!item.LeadDocumentId.HasValue && item.ExportDeclarationNumber == dto.ExportDeclarationNum)
            {
                item.LeadDocumentId = dto.LeadDocumentId;
            }

            if (item.CertificateOfOriginStatusId == (int)ECertificateOfOriginStatus.PendingRelease)
            {
                item.CertificateOfOriginStatusId = (int)ECertificateOfOriginStatus.Published;
                await CreateQRCodeIfNeededAndUpload(item);
                await CreateAttacmentsAndSendFeedBackMessage(item);
            }

            if (item.RequestReasonCode == (int)ERequestReason.CertificateReplacement)
            {
                await HandleCertificateReplacement(item);
            }

            await DataLayer.SaveCertificate(item);
        }

        if (chained.CertificateOfOriginsIds.Count > 0)
        {
            await UpdateCertrificateOfOrigins(chained);
        }
    }

    #region LEGACY_WCF

    // BL.ExportDeclarationAmendmentSuccess: fill declaration number on certificates that lacked it,
    // save per item, then chain UpdateCertrificateOfOrigins for the filled ones.
    #endregion
    private async Task ExportDeclarationAmendmentSuccess(UpdateCetificateOfOriginsDto dto)
    {
        var certificates = await DataLayer.GetCertificatesByIds(dto.CertificateOfOriginsIds);
        dto.CertificateOfOriginsIds = [];
        foreach (var item in certificates)
        {
            if (string.IsNullOrEmpty(item.ExportDeclarationNumber))
            {
                item.LeadDocumentId = dto.LeadDocumentId;
                item.ExportDeclarationNumber = dto.ExportDeclarationNum;
                dto.CertificateOfOriginsIds.Add(item.Id);
                await DataLayer.SaveCertificate(item);
            }
        }

        if (dto.CertificateOfOriginsIds.Count > 0)
        {
            await UpdateCertrificateOfOrigins(dto);
        }
    }

    #region LEGACY_WCF

    // BL.ExportDeclarationCancellationRequestCommited: per certificate — Cancelled + reject reason
    // (server term CanceledDeclaration), save, talkback message to the agent, raise
    // ExportDeclarationConnectToCertificateOfOriginCanceled (closes the open task).
    #endregion
    private async Task ExportDeclarationCancellationRequestCommited(UpdateCetificateOfOriginsDto dto)
    {
        var certificates = await DataLayer.GetCertificatesByIds(dto.CertificateOfOriginsIds);
        foreach (var item in certificates)
        {
            item.CertificateOfOriginStatusId = (int)ECertificateOfOriginStatus.Cancelled;
            item.RejectCancelReason = ErrorMessagesResources.CanceledDeclaration;
            await DataLayer.SaveCertificate(item);

            await commonProxy.SendMessageToAgent(new MessageToAgentDto
            {
                EntityId = item.Id,
                EntityTypeId = CertificateOfOriginEntityTypeId,
                AgentTalkBackType = EAgentTalkBackType.CertificateOfOriginCancellation,
                Message = string.Format(ErrorMessagesResources.MessageToAgentCertificateOfOrigin, item.CertificateNumber, item.ExportDeclarationNumber),
                IsExport = true
            });

            var eventUtil = Resolve<IEventUtil>();
            var closeTaskEvent = eventUtil.CreatBuilder()
                .WithEventType((int)EEventType.ExportDeclarationConnectToCertificateOfOriginCanceled) // 1910
                .WithEntityId(item.Id)
                .WithEntityType(CertificateOfOriginEntityTypeId)
                .WithTitle(item.Title)
                .Build();
            await eventUtil.RaiseEvent(closeTaskEvent);
        }
    }

    #region LEGACY_WCF

    // BL.UpdateCertrificateOfOrigins: per certificate — fill declaration number/lead document; when the
    // certificate is Received (and reason/type eligible) validate certificate-vs-declaration, raise the
    // matching event (match / mismatch / warnings [+assessor task assignment] / import-replacement task),
    // write VsDeclarationError rows, print a draft, and save. The legacy List<Exception> return value was
    // consumed by no dispatcher caller — folded to Task (parity preserved for this contract).
    // englishAdditionalInfo was accumulated but never used in the legacy body — dropped.
    #endregion
    private async Task UpdateCertrificateOfOrigins(UpdateCetificateOfOriginsDto dto)
    {
        if (dto.CertificateOfOriginsIds.Count == 0)
        {
            return;
        }

        var certificates = await DataLayer.GetCertificatesByIds(dto.CertificateOfOriginsIds);
        foreach (var item in certificates)
        {
            if (string.IsNullOrEmpty(item.ExportDeclarationNumber))
            {
                item.ExportDeclarationNumber = dto.ExportDeclarationNum;
                item.LeadDocumentId = dto.LeadDocumentId;
            }

            if (!item.LeadDocumentId.HasValue && item.ExportDeclarationNumber == dto.ExportDeclarationNum)
            {
                item.LeadDocumentId = dto.LeadDocumentId;
            }

            var reasonEligible = item.RequestReasonCode != (int)ERequestReason.EmptyCertificate
                && item.RequestReasonCode != (int)ERequestReason.GetRequestStatus
                && item.RequestReasonCode != (int)ERequestReason.CertificateCancellation;
            if (item.CertificateOfOriginStatusId == (int)ECertificateOfOriginStatus.Received
                && reasonEligible
                && item.TypeId != (int)ECertificateOfOriginType.NonManipulation)
            {
                var entries = new List<ValidationEntry>();
                var isLinkedToImportDeclaration = false;
                bool hasErrors;
                if (dto.ExportInvoiceInfoList == null || dto.ExportInvoiceInfoList.Count == 0)
                {
                    hasErrors = true;
                }
                else
                {
                    (entries, isLinkedToImportDeclaration) = await ValidateExportDeclarationInfoForPCIsMatch(item, dto);
                    hasErrors = entries.Exists(e => !e.IsWarning);
                }

                var hasWarnings = entries.Exists(e => e.IsWarning);
                if (!hasErrors && !hasWarnings)
                {
                    if (item.RequestReasonCode != (int)ERequestReason.Draft)
                    {
                        item.CertificateOfOriginStatusId = (int)ECertificateOfOriginStatus.DeclarationMatch;
                    }

                    if (item.RequestReasonCode != (int)ERequestReason.EmptyCertificate
                        && item.RequestReasonCode != (int)ERequestReason.Draft
                        && item.RequestReasonCode != (int)ERequestReason.GetRequestStatus
                        && item.RequestReasonCode != (int)ERequestReason.CertificateCancellation)
                    {
                        await RaiseTaskNewCertificateOfOriginCheck(item, EEventType.CertificateOfOriginCertificateMatchDeclaration);
                    }
                }
                else
                {
                    await HandleCertificateMismatchFindings(item, dto, entries, hasErrors, isLinkedToImportDeclaration);
                }

                await PrintCertificateOfOriginAndSaveAttachments(item, "isDraft"); // legacy CertificateOfOriginsConsts.IsDraft
            }

            await DataLayer.SaveCertificate(item);
        }
    }

    // errors/warnings continuation of UpdateCertrificateOfOrigins (split out — module 80-line method limit)
    private async Task HandleCertificateMismatchFindings(CertificateOfOrigin item, UpdateCetificateOfOriginsDto dto, List<ValidationEntry> entries, bool hasErrors, bool isLinkedToImportDeclaration)
    {
        var additionalInfo = string.Empty;
        foreach (var entry in entries)
        {
            if (additionalInfo.Length + entry.Text.Length + LengthOfTaskStart < MaximumNumberOfCharactersOfTheField)
            {
                additionalInfo += entry.Text + " , ";
            }

            await DataLayer.AddVsDeclarationError(item.Id, entry.Text);
        }

        if (item.RequestReasonCode == (int)ERequestReason.Draft)
        {
            return;
        }

        if (hasErrors)
        {
            await RaiseDeclarationMismatchEvent(item, additionalInfo);
        }
        else
        {
            await RaiseDeclarationWarningsEvents(item, dto, additionalInfo, isLinkedToImportDeclaration);
        }
    }

    private async Task RaiseDeclarationMismatchEvent(CertificateOfOrigin item, string additionalInfo)
    {
        item.CertificateOfOriginStatusId = (int)ECertificateOfOriginStatus.Rejected;
        item.RejectCancelReason = ThereIsNoMatchBetweenTheCertificateDataAndTheDeclaration;
        var eventUtil = Resolve<IEventUtil>();
        var mismatchEvent = eventUtil.CreatBuilder()
            .WithEventType((int)EEventType.CertificateOfOriginCertificateDeclarationMismatch) // 643
            .WithEntityId(item.Id)
            .WithEntityType(CertificateOfOriginEntityTypeId)
            .WithTitle(item.Title)
            .WithOrganizationUnitId(item.OrganizationUnitId)
            .WithOrganizationUnitTypeId(InfrastructureCore.Utils.Shared.OrganizationUnitTypes.Export)
            .WithAdditionalInfo(additionalInfo)
            .Build();
        await eventUtil.RaiseEvent(mismatchEvent);
    }

    private async Task RaiseDeclarationWarningsEvents(CertificateOfOrigin item, UpdateCetificateOfOriginsDto dto, string additionalInfo, bool isLinkedToImportDeclaration)
    {
        var eventUtil = Resolve<IEventUtil>();
        if (isLinkedToImportDeclaration)
        {
            var importReplacementTask = eventUtil.CreatBuilder()
                .WithEventType((int)EEventType.OpenTaskHandlingTheReplacementOfAnImportCertificate) // 2172
                .WithEntityId(item.Id)
                .WithEntityType(CertificateOfOriginEntityTypeId)
                .WithTitle(nameof(EEventType.OpenTaskHandlingTheReplacementOfAnImportCertificate))
                .WithOrganizationUnitId(item.OrganizationUnitId)
                .Build();
            await eventUtil.RaiseEvent(importReplacementTask);
        }

        item.CertificateOfOriginStatusId = (int)ECertificateOfOriginStatus.DeclarationMismatch;
        var warningsBuilder = eventUtil.CreatBuilder()
            .WithEventType((int)EEventType.CertificateOfOriginCertificateDeclarationHasWarnings) // 2171
            .WithEntityId(item.Id)
            .WithEntityType(CertificateOfOriginEntityTypeId)
            .WithTitle(item.Title)
            .WithOrganizationUnitId(item.OrganizationUnitId)
            .WithOrganizationUnitTypeId(InfrastructureCore.Utils.Shared.OrganizationUnitTypes.Export)
            .WithAdditionalInfo(additionalInfo);
        int? assesorId = null;
        if (item.LeadDocumentId.HasValue)
        {
            assesorId = await tasksProxy.GetLatestUserHandlingEntityTasksWithTaskUnification(new LatestUserHandlingEntityTasksFilterDto
            {
                EntityId = item.LeadDocumentId.Value,
                EntityTypeId = 12320, // TODO(confirm): EEntityType.ExportLeadDocument numeric value — verify before internal integration
                OrganizationUnitTypeId = 18, // EOrganizationUnitType.Export
                OrganizationUnitId = dto.OrganizationUnitId
            });
        }

        if (assesorId.HasValue)
        {
            // legacy SingleUserTaskAssignmentFilter { ProfessionId=Marech(29), OrganizationUnitTypeId=Export, UserId } —
            // org-unit-type maps at the event level (above); the assignee maps to WithTaskAssignmentUser.
            // TODO(confirm): Specialization equivalent of EProfession.Marech (29) — no verified mapping; omitted.
            warningsBuilder = warningsBuilder.WithTaskArguments(t => t.WithTaskAssignmentUser(assesorId.Value));
        }

        await eventUtil.RaiseEvent(warningsBuilder.Build());
    }

    #region LEGACY_WCF

    // BL.HandleCertificateReplacement: when CertificateIDToCancel is set — cancel the replaced certificate,
    // set feedback/reject texts (UIMessages 6540/6544/6545), talkback to the agent, raise
    // CertificateOfOriginCertificateReplaced with the new certificate number as AdditionalInfo, save both.
    #endregion
    private async Task HandleCertificateReplacement(CertificateOfOrigin certificate)
    {
        if (certificate.CertificateIdToCancel != null)
        {
            var certificateToCancel = await DataLayer.GetCertificateById(certificate.CertificateIdToCancel.Value);
            if (certificateToCancel == null)
            {
                return;
            }

            certificate.FeedbackRemark = string.Format(ErrorMessagesResources.UpdateExportDeclaration, certificate.ExportDeclarationNumber, certificateToCancel.CertificateNumber);
            await commonProxy.SendMessageToAgent(new MessageToAgentDto
            {
                EntityId = certificate.Id,
                EntityTypeId = CertificateOfOriginEntityTypeId,
                AgentTalkBackType = EAgentTalkBackType.CertificateOfOriginReplacement,
                Message = string.Format(ErrorMessagesResources.UpdateExportDecForReplacement, certificateToCancel.ExportDeclarationNumber, certificateToCancel.CertificateNumber, certificate.CertificateNumber),
                IsExport = true
            });

            certificateToCancel.CertificateOfOriginStatusId = (int)ECertificateOfOriginStatus.Cancelled;
            certificateToCancel.RejectCancelReason = string.Format(ErrorMessagesResources.CertificateReplaced, certificate.CertificateNumber);

            var eventUtil = Resolve<IEventUtil>();
            var replacedEvent = eventUtil.CreatBuilder()
                .WithEventType((int)EEventType.CertificateOfOriginCertificateReplaced) // 639
                .WithEntityId(certificateToCancel.Id)
                .WithEntityType(CertificateOfOriginEntityTypeId)
                .WithTitle(certificateToCancel.Title)
                .WithAdditionalInfo(certificate.CertificateNumber)
                .Build();
            await eventUtil.RaiseEvent(replacedEvent);

            await DataLayer.SaveCertificate(certificateToCancel);
        }

        await DataLayer.SaveCertificate(certificate);
    }

    #region LEGACY_WCF

    // BL.CreateQRCodeIfNeededAndUpload: for a newly Published certificate without a QR — build the public
    // query URL (config CertificateOfOriginQueryURL + new GUID), generate a QR image via CommonServices,
    // upload it as an "Other" document (entity -1 in legacy), keep the file URL on QRCodePath.
    #endregion
    private async Task CreateQRCodeIfNeededAndUpload(CertificateOfOrigin certificateOfOrigin)
    {
        if (!string.IsNullOrWhiteSpace(certificateOfOrigin.QrCodePath) ||
            certificateOfOrigin.CertificateOfOriginStatusId != (int)ECertificateOfOriginStatus.Published)
        {
            return;
        }

        var url = await parametersUtil.Get<string>("CertificateOfOriginQueryURL");
        certificateOfOrigin.Guid = System.Guid.NewGuid();
        url = string.Format(url, certificateOfOrigin.Guid);
        var qrCodeByteArray = await commonProxy.CreateQRCode(url);
        certificateOfOrigin.QrImage = qrCodeByteArray;

        var documentUtil = Resolve<IDocumentUtil>();
        var fileName = certificateOfOrigin.CertificateNumber + ".jpg";
        var qrDocument = documentUtil.CreateDocumentBuilder()
            .WithFileName(fileName)
            .WithTitle(fileName)
            .WithContent(qrCodeByteArray ?? [])
            .WithTypeId((CustomsCloud.InfrastructureCore.Utils.Documents.DocumentType)81) // EDocumentType.Other = 81 (verified in legacy EDocumentType.cs)
            .WithEntityId(certificateOfOrigin.Id) // legacy uploaded with entity -1 (repository-only); linked to the certificate here
            .WithEntityTypeId(CertificateOfOriginEntityTypeId)
            .Build();
        await documentUtil.UploadDocument(qrDocument);
        certificateOfOrigin.QrCodePath = documentUtil.GetFileUrl(fileName).ToString();
    }

    #region LEGACY_WCF

    // BL.CreateAttacmentsAndSendFeedBackMessage: once per certificate (IsCreateAttachments guard) —
    // stamp IssuingDate, worker-publishing flag, save, raise CertificateOfOriginCertificateIssued,
    // Thread.Sleep(1000) [event-pipeline race workaround], render templates, wrap as attachments,
    // SendRequestFeedback. Sleep preserved as Task.Delay per developer decision (2026-07-07).
    #endregion
    private async Task CreateAttacmentsAndSendFeedBackMessage(CertificateOfOrigin certificate)
    {
        if (certificate.IsCreateAttachments)
        {
            return;
        }

        certificate.IssuingDate = DateTime.Now;
        if (await parametersUtil.Get<bool>("IssueCertificateOfOriginByWorker"))
        {
            certificate.IsInPublishingProcess = true;
        }

        await DataLayer.SaveCertificate(certificate);

        var eventUtil = Resolve<IEventUtil>();
        var issuedEvent = eventUtil.CreatBuilder()
            .WithEventType((int)EEventType.CertificateOfOriginCertificateIssued) // 640
            .WithEntityId(certificate.Id)
            .WithEntityType(CertificateOfOriginEntityTypeId)
            .WithTitle(certificate.Title)
            .Build();
        await eventUtil.RaiseEvent(issuedEvent);

        await Task.Delay(1000); // legacy Thread.Sleep(1000) — event-pipeline race workaround, preserved 1:1

        var templates = await PrintCertificateOfOriginAndSaveAttachments(certificate, string.Empty);
        if (templates != null && templates.Count > 0)
        {
            var attachments = templates.Select(t => new CertificateAttachment
            {
                DocumentTypeID = t.DocumentTypeId,
                content = t.Content,
                fileName = t.FileName
            }).ToArray();
            certificate.IsCreateAttachments = true;
            await SendRequestFeedback(certificate, attachments);
        }
    }

    #region LEGACY_WCF

    // BL.SendRequestFeedback: once per certificate (IsMessageSent guard) — build PC_NG_2281_MSG02 feedback
    // payload + attachments and send via OutgoingMessageProxy to the creating customer (the customs agent),
    // related entity = the certificate. Migrated to the IOutgoingMessageUtil builder.
    #endregion
    private async Task SendRequestFeedback(CertificateOfOrigin certificateOfOrigin, CertificateAttachment[] attachments)
    {
        if (certificateOfOrigin.IsMessageSent)
        {
            return;
        }

        var message = new CertificateOfOriginRequestFeedbackMessage
        {
            CertificateOfOriginRequestFeedback = await CreateCertificateOfOriginRequestFeedback(certificateOfOrigin),
            Attachment = attachments
        };

        var outgoingMessageUtil = Resolve<IOutgoingMessageUtil>();
        var request = outgoingMessageUtil.CreateOutgoingMessageRequestBuilder()
            .WithConsumerId(certificateOfOrigin.CreateCustomerId)          // legacy OutgoingDetails.ID
            .WithConsumerEntityType(SharedEntityType.Customer)              // legacy OutgoingDetails.EntityType
            .WithDestinationExternalId(certificateOfOrigin.CertificateNumber) // TODO(confirm): legacy routed by CreateCustomerID only — confirm the destination-external-id source for PC_NG_2281 feedback
            .AddMessage(message)
            .AddRelatedEntity(certificateOfOrigin.Id, (SharedEntityType)CertificateOfOriginEntityTypeId) // legacy RelatedEntities = [certificate]
            .Build();
        await outgoingMessageUtil.SendAsync(request);

        if (certificateOfOrigin.CertificateOfOriginStatusId == (int)ECertificateOfOriginStatus.Published)
        {
            certificateOfOrigin.IsMessageSent = true;
        }
    }

    #region LEGACY_WCF

    // CertificateOfOriginsUtil.CreateCertificateOfOriginRequestFeedback: payload from certificate fields +
    // public query URL (config CertificateOfOriginQueryURL formatted with the certificate GUID);
    // IssueDateIfReleased / IssueDateIfNotReleased split by IsDeclarationReleased.
    // IsDeclarationReleased is not a column on the .NET 10 entity — resolved via ExportDealFile (as in
    // LoadDataFromExportDeclaration).
    #endregion
    private async Task<CertificateOfOriginRequestFeedback> CreateCertificateOfOriginRequestFeedback(CertificateOfOrigin certificate)
    {
        var queryUrlFormat = await parametersUtil.Get<string>("CertificateOfOriginQueryURL");
        var queryUrl = string.Format(queryUrlFormat, certificate.Guid?.ToString());

        var requestFeedback = new CertificateOfOriginRequestFeedback
        {
            rejectCancelReason = certificate.RejectCancelReason,
            internalApplication = certificate.InternalApplication,
            certificateOfOriginTypeCode = certificate.TypeId,
            certificateOfOriginStatusCode = certificate.CertificateOfOriginStatusId,
            certificateID = certificate.CertificateNumber,
            FeedbackRemark = certificate.FeedbackRemark,
            QueryURL = queryUrl,
            requestReasonCode = certificate.RequestReasonCode
        };

        bool? isDeclarationReleased = null;
        if (certificate.LeadDocumentId != null || !string.IsNullOrEmpty(certificate.ExportDeclarationNumber))
        {
            var declarationDetails = await exportDealFileProxy.GetExportDeclarationDetailsForCertificateOfOrigin(certificate.LeadDocumentId, certificate.ExportDeclarationNumber);
            isDeclarationReleased = declarationDetails?.IsDeclarationReleased;
        }

        if (certificate.IssuingDate.HasValue && isDeclarationReleased.HasValue)
        {
            requestFeedback.IssueDateIfReleased = isDeclarationReleased.Value ? certificate.IssuingDate : null;
            requestFeedback.IssueDateIfReleasedSpecified = requestFeedback.IssueDateIfReleased != null;
            requestFeedback.IssueDateIfNotReleased = !isDeclarationReleased.Value ? certificate.IssuingDate : null;
            requestFeedback.IssueDateIfNotReleasedSpecified = requestFeedback.IssueDateIfNotReleased != null;
        }

        return requestFeedback;
    }

    #region LEGACY_WCF

    // BL.PrintCertificateOfOriginAndSaveAttachments: legacy branched — reportId (from the type-code lookup)
    // → GanerateReportAndConvertToTemplateResult, else a per-type switch of CreateTemplateSync template-id
    // constants (±isTwoPages), then SaveCertificateOfOriginAttachmentsSync (WCF self-call).
    // .NET 10: ONE uniform flow (developer decision 2026-07-07) — GetTemplateData → ITemplateUtil builder →
    // GenerateTemplate; the per-type switch and the reportId/templateId split are gone. Attachments are
    // saved via the already-migrated SaveCertificateOfOriginAttachments BL method (direct call, no self-proxy).
    // The isTwoPages template variants are handled inside the new templates (IsAttachedList travels in the data).
    #endregion
    private async Task<List<TemplateResultDto>?> PrintCertificateOfOriginAndSaveAttachments(CertificateOfOrigin certificate, string additionalInfo)
    {
        if (await parametersUtil.Get<bool>("IssueCertificateOfOriginByWorker") && certificate.IsInPublishingProcess)
        {
            var typeCode = await DataLayer.GetCertificateOfOriginTypeCode(certificate.TypeId);
            await SendCertificateToIssueQueue(new IssueCertificateDto
            {
                ReportId = typeCode?.ReportId ?? 0,
                CertificateOfOriginId = certificate.Id,
                CertificateNumber = certificate.CertificateNumber,
                CertificateOfOriginStatusId = certificate.CertificateOfOriginStatusId,
                CertificateTypeId = certificate.TypeId,
                CertificateTypeName = typeCode?.Name ?? string.Empty,
                RequestReasonCode = certificate.RequestReasonCode,
                IsInPublishingProcess = certificate.IsInPublishingProcess,
                CreateCustomerId = certificate.CreateCustomerId,
                RejectCancelReason = certificate.RejectCancelReason,
                InternalApplication = certificate.InternalApplication,
                FeedbackRemark = certificate.FeedbackRemark,
                IssuingDate = certificate.IssuingDate,
                Guid = certificate.Guid,
                OrganizationUnitId = certificate.OrganizationUnitId
            });
            return null; // legacy: the queue path renders nothing synchronously
        }

        using var memoryStream = await GenerateTemplate(certificate.TypeId, certificate.Id);

        var templateResult = new TemplateResultDto
        {
            DocumentTypeId = ExportCertificateOfOriginDocumentTypeId,
            Content = memoryStream.ToArray(),
            FileName = certificate.CertificateNumber + ".pdf"
        };
        var templateResults = new List<TemplateResultDto> { templateResult };

        await SaveCertificateOfOriginAttachments(new SaveCertificateAttachmentsArgsDto
        {
            CertificatesTemplates = templateResults,
            CertificateId = certificate.Id,
            CertificateNumber = certificate.CertificateNumber,
            CertificateTypeId = certificate.TypeId,
            CertificateRequestReasonCode = certificate.RequestReasonCode,
            AdditionalInfo = additionalInfo
        });
        return templateResults;
    }

    // Generic template contract — exposed as GenerateTemplate/{templateId}/{entityId} (C1); also feeds the print flow.
    public async Task<MemoryStream> GenerateTemplate(int templateId, int entityId)
    {
        var templateData = await GetTemplateData(templateId, entityId);
        var templateUtil = Resolve<ITemplateUtil>();
        var templateRequest = templateUtil.CreatRequestBuilder()
            .WithFormat(Enum.Parse<Format>(templateData.Format, ignoreCase: true))
            .WithName(templateData.Name)
            .WithData(templateData.Data)
            .Build();
        var template = await templateUtil.GenerateTemplate(templateRequest);
        var memoryStream = new MemoryStream();
        await template.CopyToAsync(memoryStream);
        memoryStream.Position = 0;
        return memoryStream;
    }

    // Template data for certificate printing — template-print-pattern (uniform templateId flow).
    // Public: exposed as TemplateData/{templateId}/{entityId} (generic template contract, C1).
    public async Task<PrintTemplateDto> GetTemplateData(int templateId, int entityId)
    {
        var (name, format) = GetTemplateMeta(templateId);
        var data = await DataLayer.GetTemplateData<CertificateOfOriginPrintResult>(templateId, entityId)
            ?? throw new RestNotFoundException(); // generic template contract → 404 (C1)
        var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        return new PrintTemplateDto { Name = name, Data = json, Format = format };
    }

    // TODO(blocking): confirm the template names as registered in the Templates module.
    private static (string Name, string Format) GetTemplateMeta(int templateId) => templateId switch
    {
        (int)ECertificateOfOriginType.Eurmed => ("CRM_CertificateOfOrigins_EURMED", "pdf"),
        (int)ECertificateOfOriginType.Eur1 => ("CRM_CertificateOfOrigins_EUR1", "pdf"),
        (int)ECertificateOfOriginType.Mercosur => ("CRM_CertificateOfOrigins_MERCOSUR", "pdf"),
        (int)ECertificateOfOriginType.IsrCol => ("CRM_CertificateOfOrigins_IsrCol", "pdf"),
        (int)ECertificateOfOriginType.NonManipulation => ("CRM_CertificateOfOrigins_NonManipulation", "pdf"),
        (int)ECertificateOfOriginType.Panama => ("CRM_CertificateOfOrigins_Panama", "pdf"),
        (int)ECertificateOfOriginType.SouthKorea => ("CRM_CertificateOfOrigins_SouthKorea", "pdf"),
        (int)ECertificateOfOriginType.UnitedArabEmirates => ("CRM_CertificateOfOrigins_UnitedArabEmirates", "pdf"),
        _ => throw new ArgumentOutOfRangeException(nameof(templateId))
    };

    #region LEGACY_WCF

    // BL.SendCertificateToIssueQueue: QueueUtilFactory → CreateQueueMessageBuilder →
    // SendToExchange("IssueCertificateOfOrigin") + AddCloudEventMessage(dto) → SendMessageAsync,
    // errors swallowed to the log. Migrated 1:1 to the injected IQueueUtil builder.
    #endregion
    private async Task SendCertificateToIssueQueue(IssueCertificateDto issueCertificateDto)
    {
        try
        {
            var queueUtil = Resolve<IQueueUtil>();
            var message = queueUtil.CreateQueueMessageBuilder()
                .SendToExchange("IssueCertificateOfOrigin")
                .AddCloudEventMessage(issueCertificateDto)
                .Build();
            await queueUtil.SendMessage(message);
        }
        catch (Exception exception)
        {
            Logger.LogError(exception, "SendCertificateToIssueQueue failed for certificate {CertificateOfOriginId}", issueCertificateDto.CertificateOfOriginId);
        }
    }

    #region LEGACY_WCF

    // BL.ValidateExportDeclarationInfoForPCIsMatch: certificate details (destination/origin country and
    // country-groups, exporter id) vs the declaration dto; ImportCertificateReplacement → ExportDealFile
    // associated goods items + CustomsBook trade-agreement check; invoice/goods-item cross-matching with
    // CustomsBook items (6-digit classification). Exceptions grouped by message (first per group) — here
    // entries deduped by key. CountryCountryGroup read via ILookupUtil (legacy: ISystemTablesUtil predicate).
    #endregion
    private async Task<(List<ValidationEntry> Entries, bool IsLinkedToImportDeclaration)> ValidateExportDeclarationInfoForPCIsMatch(CertificateOfOrigin item, UpdateCetificateOfOriginsDto dto)
    {
        var entries = new List<ValidationEntry>();
        var details = await DataLayer.GetCertificateDetails(item.Id);
        string? DetailValue(ECertificateDetailsType type) => details.FirstOrDefault(d => d.CertificateDetailsTypeCodeId == (int)type)?.Value;
        var destinationGroupOfCountries = DetailValue(ECertificateDetailsType.DestinationGroupOfCountries);
        var destinationCountry = DetailValue(ECertificateDetailsType.DestinationCountry);
        var exporterId = DetailValue(ECertificateDetailsType.ExporterId);
        var originCountry = DetailValue(ECertificateDetailsType.OriginCountry);
        var originGroupOfCountries = DetailValue(ECertificateDetailsType.OriginGroupOfCountries);
        var isLinkedToImportDeclaration = false;

        if (dto.DestinationCountryId.HasValue
            && !string.IsNullOrEmpty(destinationCountry)
            && int.TryParse(destinationCountry, out var destinationCountryId)
            && destinationCountryId != dto.DestinationCountryId)
        {
            AddValidationEntry(entries, nameof(ErrorMessagesResources.DestinationCountryIsNotMatchToDestinationCountryInExportDeclaration),
                ErrorMessagesResources.DestinationCountryIsNotMatchToDestinationCountryInExportDeclaration, isWarning: false);
        }

        // TODO(blocking): destination country-group vs buyer-country check (UIMessage 14027170,
        // legacy: ISystemTablesUtil.GetTablesSync<CountryCountryGroup>) — CountryCountryGroup is NOT
        // available as a lookup entity in InfrastructureCore.Lookup 1.10.37 and the table is not
        // replicated to this service's DB. Check SKIPPED until the infra exposes it (missing-infra gap).
        if (!string.IsNullOrEmpty(item.ExportDeclarationNumber) && item.ExportDeclarationNumber != dto.ExportDeclarationNum)
        {
            AddValidationEntry(entries, nameof(ErrorMessagesResources.ExportDeclarationNotInSystemForWarningMessage),
                string.Format(ErrorMessagesResources.ExportDeclarationNotInSystemForWarningMessage, dto.ExportDeclarationNum), isWarning: false);
        }

        if (!string.IsNullOrEmpty(exporterId) && int.TryParse(exporterId, out var exporter) && exporter != dto.ExporterCustomerId)
        {
            AddValidationEntry(entries, nameof(ErrorMessagesResources.ExporterNumberIsNotMatchToExporterNumberInExportDeclaration),
                ErrorMessagesResources.ExporterNumberIsNotMatchToExporterNumberInExportDeclaration, isWarning: false);
        }

        if (item.RequestReasonCode == (int)ERequestReason.ImportCertificateReplacement)
        {
            var associatedGoodsItems = await exportDealFileProxy.GetDetailsForExportAssociatedGoodsItemsByLeadDocumentId(dto.LeadDocumentId);
            var hasTradeAgreementLink = false;
            foreach (var goodsItem in associatedGoodsItems ?? [])
            {
                if (await customsBookProxy.IsTradeAgreementForCountry(item.TypeId, goodsItem.AssociatedOriginCountryId, isImport: false))
                {
                    hasTradeAgreementLink = true;
                    break;
                }
            }

            if (!hasTradeAgreementLink)
            {
                AddValidationEntry(entries, nameof(ErrorMessagesResources.ThereIsNoMerchandiseLinkedToImportDeclarationInTradeAgreement),
                    ErrorMessagesResources.ThereIsNoMerchandiseLinkedToImportDeclarationInTradeAgreement, isWarning: true);
                isLinkedToImportDeclaration = true;
            }
        }

        await ValidateInvoiceMatching(item, dto, originCountry, entries);

        return (entries, isLinkedToImportDeclaration);
    }

    // invoice/goods-item cross-matching continuation of the validation (split out — module 80-line method limit)
    private async Task ValidateInvoiceMatching(CertificateOfOrigin item, UpdateCetificateOfOriginsDto dto, string? originCountry, List<ValidationEntry> entries)
    {
        var (invoices, itemDetails) = await DataLayer.GetCertificateInvoiceGraph(item.Id);
        if (invoices.Count == 0 || dto.ExportInvoiceInfoList is not { Count: > 0 })
        {
            return;
        }

        var certificateTypeCode = await DataLayer.GetCertificateOfOriginTypeCode(item.TypeId);
        var isCustomsItemMandatory = certificateTypeCode?.IsCustomsItemMandatory.GetValueOrDefault() == true;

        var filters = itemDetails.Where(d => d.CustomsItemId.HasValue)
            .Select(d => new CustomsItemsIdsCacheFilterDto { CustomsItemId = d.CustomsItemId, Date = item.CreateDate })
            .Concat(dto.ExportInvoiceInfoList
                .Where(i => i.ExportGoodsItemInfoList is { Count: > 0 })
                .SelectMany(i => i.ExportGoodsItemInfoList!)
                .Select(g => new CustomsItemsIdsCacheFilterDto { CustomsItemId = g.CustomsItemId, Date = item.CreateDate }))
            .DistinctBy(f => f.CustomsItemId)
            .ToList();
        var customsItemsByIds = await customsBookProxy.GetCustomsItemsByIds(filters) ?? [];
        foreach (var customsItem in customsItemsByIds)
        {
            customsItem.FullClassificationBy6Digits =
                customsItem.FullClassification?.Length >= 6 ? customsItem.FullClassification[..6] : null;
        }

        foreach (var invoice in invoices)
        {
            var invoiceFromDealFile = dto.ExportInvoiceInfoList.FirstOrDefault(i => i.ExternalIdNum == invoice.InvoiceNumber);
            if (invoiceFromDealFile == null)
            {
                AddValidationEntry(entries, nameof(ErrorMessagesResources.ExportInvoiceIsNotMatchToExportInvoiceInExportDeclaration),
                    string.Format(ErrorMessagesResources.ExportInvoiceIsNotMatchToExportInvoiceInExportDeclaration, invoice.InvoiceNumber), isWarning: true);
                return; // legacy: early return after recording the group — the deal-file loop is skipped too
            }

            ValidateInvoiceGoodsItems(item, invoice, invoiceFromDealFile, itemDetails, customsItemsByIds, isCustomsItemMandatory, originCountry, entries);
        }

        await ValidateDealFileGoodsItems(item, dto, invoices, itemDetails, customsItemsByIds, isCustomsItemMandatory, entries);
    }

    private static void ValidateInvoiceGoodsItems(CertificateOfOrigin item, CertificateOfOriginInvoiceDetail invoice, ExportInvoiceInfoDto invoiceFromDealFile, List<CertificateOfOriginItemDetail> itemDetails, List<CustomsItemDto> customsItemsByIds, bool isCustomsItemMandatory, string? originCountry, List<ValidationEntry> entries)
    {
        var invoiceItems = itemDetails.Where(d => d.CertificateOfOriginInvoiceDetailId == invoice.Id).ToList();
        if (invoiceItems.Count == 0 || invoiceFromDealFile.ExportGoodsItemInfoList is not { Count: > 0 })
        {
            return;
        }

        var customsItemPerInvoice = invoiceFromDealFile.ExportGoodsItemInfoList
            .Where(g => g.CertificateOfOriginId == item.Id)
            .Select(g => (int?)g.CustomsItemId)
            .ToList();
        foreach (var goodsItem in invoiceItems)
        {
            if (!string.IsNullOrEmpty(originCountry)
                && int.TryParse(originCountry, out var originCountryId)
                && !invoiceFromDealFile.ExportGoodsItemInfoList.Any(g => g.OriginCountryId == originCountryId))
            {
                AddValidationEntry(entries, nameof(ErrorMessagesResources.OriginCountryIsNotMatchToOriginCountryInExportDeclaration),
                    ErrorMessagesResources.OriginCountryIsNotMatchToOriginCountryInExportDeclaration, isWarning: false);
            }

            // TODO(blocking): origin country-group check (UIMessage 1402654 group variant,
            // legacy: ISystemTablesUtil.GetTablesSync<CountryCountryGroup>) — CountryCountryGroup
            // lookup entity is missing from the infra (see the destination-group TODO above).
            // Check SKIPPED until the infra exposes it (missing-infra gap).
            if (!invoiceFromDealFile.ExportGoodsItemInfoList.Any(g => g.CertificateOfOriginId == item.Id))
            {
                AddValidationEntry(entries, nameof(ErrorMessagesResources.CertificateNumberIsNotMatchToCertificateNumberInExportDealFile),
                    ErrorMessagesResources.CertificateNumberIsNotMatchToCertificateNumberInExportDealFile, isWarning: false);
            }

            var customsItemBy6Digits = customsItemsByIds.FirstOrDefault(ci => ci.Id == goodsItem.CustomsItemId)?.FullClassificationBy6Digits;
            if (isCustomsItemMandatory
                && !string.IsNullOrEmpty(customsItemBy6Digits)
                && !customsItemsByIds.Any(ci => customsItemPerInvoice.Count > 0
                    && customsItemPerInvoice.Contains(ci.Id)
                    && !string.IsNullOrEmpty(ci.FullClassificationBy6Digits)
                    && ci.FullClassificationBy6Digits == customsItemBy6Digits))
            {
                AddValidationEntry(entries, nameof(ErrorMessagesResources.CustomsItemIsNotMatchToCustomsItemInExportDeclaration) + customsItemBy6Digits,
                    string.Format(ErrorMessagesResources.CustomsItemIsNotMatchToCustomsItemInExportDeclaration, customsItemBy6Digits, invoice.InvoiceNumber), isWarning: false);
            }
        }
    }

    private async Task ValidateDealFileGoodsItems(CertificateOfOrigin item, UpdateCetificateOfOriginsDto dto, List<CertificateOfOriginInvoiceDetail> invoices, List<CertificateOfOriginItemDetail> itemDetails, List<CustomsItemDto> customsItemsByIds, bool isCustomsItemMandatory, List<ValidationEntry> entries)
    {
        foreach (var invoiceFromDealFile in dto.ExportInvoiceInfoList!)
        {
            var dealFileGoodsItems = (invoiceFromDealFile.ExportGoodsItemInfoList ?? [])
                .Where(gi => gi.CertificateOfOriginId == item.Id);
            var certificateInvoice = invoices.FirstOrDefault(i => i.InvoiceNumber == invoiceFromDealFile.ExternalIdNum);
            var goodsItemsOfInvoiceInCertificate = certificateInvoice == null
                ? null
                : itemDetails.Where(d => d.CertificateOfOriginInvoiceDetailId == certificateInvoice.Id).Select(d => (int?)d.CustomsItemId).ToList();
            foreach (var goodsItemFromDealFile in dealFileGoodsItems)
            {
                var customsItemBy6Digits = customsItemsByIds.FirstOrDefault(ci => ci.Id == goodsItemFromDealFile.CustomsItemId)?.FullClassificationBy6Digits;
                if (isCustomsItemMandatory
                    && !string.IsNullOrEmpty(customsItemBy6Digits)
                    && !customsItemsByIds.Any(ci => goodsItemsOfInvoiceInCertificate is { Count: > 0 }
                        && goodsItemsOfInvoiceInCertificate.Contains(ci.Id)
                        && !string.IsNullOrEmpty(ci.FullClassificationBy6Digits)
                        && ci.FullClassificationBy6Digits == customsItemBy6Digits))
                {
                    var declarationItems = await customsBookProxy.GetCustomsItemsByIds(
                    [
                        new CustomsItemsIdsCacheFilterDto { CustomsItemId = goodsItemFromDealFile.CustomsItemId, Date = DateTime.Now }
                    ]);
                    var declarationClassification = declarationItems?.FirstOrDefault()?.FullClassification;
                    var declarationBy6Digits = declarationClassification?.Length >= 6 ? declarationClassification[..6] : declarationClassification;
                    AddValidationEntry(entries, nameof(ErrorMessagesResources.CustomsItemInDeclarationIsNotMatchToCustomsItemsInCertificate) + declarationBy6Digits,
                        string.Format(ErrorMessagesResources.CustomsItemInDeclarationIsNotMatchToCustomsItemsInCertificate, declarationBy6Digits, invoiceFromDealFile.ExternalIdNum), isWarning: false);
                }
            }
        }
    }

    #region LEGACY_WCF

    // BL.RaiseTaskNewCertificateOfOriginCheck: match event (642) with Export org-unit-type, preferred user
    // from the Tasks service (latest handler of the lead document), and AdditionalInfo listing sibling
    // certificates on the same declaration (UIMessage 6541; 1/2/">2 — לדוגמה" formats).
    #endregion
    private async Task RaiseTaskNewCertificateOfOriginCheck(CertificateOfOrigin certificateOfOrigin, EEventType eventType)
    {
        var eventUtil = Resolve<IEventUtil>();
        var builder = eventUtil.CreatBuilder()
            .WithEventType((int)eventType)
            .WithEntityId(certificateOfOrigin.Id)
            .WithEntityType(CertificateOfOriginEntityTypeId)
            .WithTitle(certificateOfOrigin.Title)
            .WithOrganizationUnitId(certificateOfOrigin.OrganizationUnitId)
            .WithOrganizationUnitTypeId(InfrastructureCore.Utils.Shared.OrganizationUnitTypes.Export);

        int? assesorId = null;
        if (certificateOfOrigin.LeadDocumentId.HasValue)
        {
            assesorId = await tasksProxy.GetLatestUserHandlingEntityTasksWithTaskUnification(new LatestUserHandlingEntityTasksFilterDto
            {
                EntityId = certificateOfOrigin.LeadDocumentId.Value,
                EntityTypeId = 12320, // TODO(confirm): EEntityType.ExportLeadDocument numeric value — verify before internal integration
                OrganizationUnitTypeId = 18, // EOrganizationUnitType.Export
                OrganizationUnitId = certificateOfOrigin.OrganizationUnitId
            });
        }

        if (assesorId.HasValue)
        {
            builder = builder.WithTaskArguments(t => t.WithPreferredUserId(assesorId.Value));
        }

        var certificateNumbers = (await GetCertificateOfOriginByDeclaration(certificateOfOrigin) ?? [])
            .Where(cn => !cn.Equals(certificateOfOrigin.CertificateNumber))
            .ToList();
        if (certificateNumbers.Count > 0)
        {
            var messageParameter = certificateNumbers.Count switch
            {
                1 => certificateNumbers[0],
                2 => certificateNumbers[0] + ", " + certificateNumbers[^1],
                _ => ". לדוגמה:" + certificateNumbers[0] + ", " + certificateNumbers[^1]
            };
            builder = builder.WithAdditionalInfo(string.Format(ErrorMessagesResources.AdditionalCertificatesToExportDeclaration, messageParameter));
        }

        await eventUtil.RaiseEvent(builder.Build());
    }

    #region LEGACY_WCF

    // BL.GetCertificateOfOriginByDeclaration: declaration info via ExportDealFile (details → info-for-PC),
    // collect the certificate ids on the declaration's goods items, return the active sibling certificate
    // numbers (excluding Cancelled/Rejected/Error). The entity-attached ExportDeclarationDetailsDTO transient
    // field has no .NET 10 equivalent — always resolved via the proxy.
    #endregion
    private async Task<List<string>?> GetCertificateOfOriginByDeclaration(CertificateOfOrigin certificateOfOrigin)
    {
        if (certificateOfOrigin.LeadDocumentId == null && certificateOfOrigin.ExportDeclarationNumber == null)
        {
            return null;
        }

        var declarationDetails = await exportDealFileProxy.GetExportDeclarationDetailsForCertificateOfOrigin(
            certificateOfOrigin.LeadDocumentId, certificateOfOrigin.ExportDeclarationNumber);
        if (declarationDetails == null)
        {
            return null;
        }

        var declarationInfo = await exportDealFileProxy.GetExportDeclarationInfoForPC(declarationDetails.LeadDocumentId);
        if (declarationInfo?.ExportInvoiceInfoList == null)
        {
            return null;
        }

        var certificateIds = declarationInfo.ExportInvoiceInfoList
            .SelectMany(i => i.ExportGoodsItemInfoList ?? [])
            .Where(gi => gi.CertificateOfOriginId.HasValue)
            .Select(gi => gi.CertificateOfOriginId!.Value)
            .Distinct()
            .ToList();
        var result = await DataLayer.GetActiveCertificateNumbersByIds(certificateIds, certificateOfOrigin.CertificateNumber);
        return result;
    }

    #endregion
}
