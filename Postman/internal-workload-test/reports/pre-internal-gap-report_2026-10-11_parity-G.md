# Parity re-check G: delivery flows, schedulers, IssueCertificateOfOrigin worker (2026-10-11)

Read-only review. N = `C:\Repos\CertificateOfOrigins`, L = `C:\Repos\Main\CRM\CertificateOfOrigins`.

## 1. Scope and files read

- **New:**
  - `API/CertificateOfOrigins.BL/AuthenticationRequestBl.cs` (346-371, 479-640, 1316-1360) and `AuthenticationRequestBl.Schedulers.cs` (all)
  - `API/CertificateOfOrigins.DAL/CertificateOfOriginsDal.cs` (1081-1110, 1210-1258)
  - the four request DTOs (`ChangeStatusAfterDeliverySent`, `CloseReminderTask`, `HandleDeliveryAndReminderForVendorSent`, `HandleDeliveryOrReminderForImporterSent`), `ReminderForImporterSchedulerDto`, `AuthenticationRequestsForSchedulerDto`, `VirtualEntityDto`, `GetAuthenticationRequestFileByIdResultDto`, `AuthenticationFileRequestDto`
  - `Scripts/API_20260907101500`, `Scripts/API_20260907101600`, `Scripts/API_20260716 - add params.sql`
  - `CertificateOfOriginsConsts.cs:85-127`
  - the two Planar jobs
  - `CertificateOfOriginsBl.cs:1080-1102, 1537-1660` (the side that sends to the worker)
  - Worker: `Workers/IssueCertificateOfOrigin.Worker/{IssueCertificateOfOriginWorker,Handler,BL,Consts}.cs`, `Dto/*`, `Dal/IssueCertificateDal.cs`, `Proxies/CommonCloudProxy.cs`
  - Worker host: `Workers/CertificateOfOriginsWorkers/ServiceConfiguration.cs`, `appsettings*.json`
  - Decompiled `CustomsCloud.InfrastructureCore.Worker 1.10.109` (`GetEntity`)
- **Legacy:**
  - `Server/...BL/AuthenticationRequestBL.cs` (295-350, 495-525, 695-830, 1265-1300) and `BaseAuthenticationRequestBL.cs`
  - `Server/...BL/CertificateOfOriginsBL.cs` (379-440, 1125-1320) and `CertificateOfOriginsUtil.cs:44-72`
  - `Server/...Service/CertificateOfOriginsInternalServicePartial.cs` (206-278), `CertificateOfOriginsExternalServicePartial.cs:71-95`, and both files in `ScheduledTasks/`
  - Client `AuthenticationRequestFilePresenter.cs:245-405`
  - The entity partial `Customs.CRM.Common/Customs.CRM.Entities/CertificateOfOriginsPartial/CertificateOfOriginsImportAuthenticationFileDetailsPartial.cs:102-127`
  - `Infrastructure/EAI/EAISchema/Customs.Inf.CertificateOfOrigins.EAISchema/PC_NG_2281_MSG02_CertificateOfOriginRequestFeedback.cs`
  - Both legacy SPs in `CustumsDev_Database/.../CRM/Stored Procedures`
- **Docs:** the LEDGER (G-F1..F10), the previous G appendix, MIGRATION-STATUS (the Workers section), MIGRATION-NOT-DONE (`HandleAuthenticationRequestDeliverySent`, Pattern A).

**Which legacy flow the worker replaces:** In legacy `CertificateOfOriginsBL.CreateAttacmentsAndSendFeedBackMessage` (L BL:379-418), the parameter `IssueCertificateOfOriginByWorker` controls publishing:
- **Off:** the API renders the certificate inline (`PrintCertificateOfOriginAndSaveAttachments`), saves it as a document and sends feedback 2281.
- **On:** the API sets `IsInPublishingProcess`. `PrintCertificateOfOriginAndSaveAttachments` then only calls `SendCertificateToIssueQueue` (L BL:1148-1153, 1243-1262) and returns null, so the API sends nothing. The worker does the render, the document save, resetting `IsInPublishingProcess` and sending 2281.

The original worker source is not in either repo (commit 2a3a98e imported it "as given"). So the worker is compared against two things: the legacy in-process path it replaces, and the message contract the API sends.

## 2. Previous findings re-verification

| ID | Prior status | Verified now? | Evidence |
|---|---|---|---|
| G-F1 | FIXED | YES | `CertificateOfOriginsDal.cs:1230-1231` sets `FirstProvideContactDate = today` only when `stampFirstContactDate && FirstProvideContactDate == null`. `AuthenticationRequestBl.cs:529` passes `request.IsDelivery`. This matches the legacy client (L Presenter:250-251), which stamped only on `OnRequestDeliverNotificationCommand` and never on the reminder command (:268-279). The importer flow passes false (BL:579), which matches legacy. |
| G-F2 | OPEN-BLOCKED | YES (marker present) | `AuthenticationRequestBl.Schedulers.cs:27-30` TODO(confirm). The filter is still `IsTaskInProgress` (:52). NOT-DONE "Pattern A" still lists it. |
| G-F3 | FIXED | YES | `Dal.cs:1233` (file `UpdateUserId`), `:1239` (child requests), `:1257` (request). The BL passes `RequestMetadata.UserId ?? 0` at BL:529, 572 and 579. |
| G-F4 | OPEN-BLOCKED | PARTIAL (doc only) | Code still matches on `EntityType` only (BL:361-362) and `VirtualEntityDto` has no `TypeId`, which is consistent with NOT-DONE:276-279. The ledger cites a marker at "BL:355-356", but no TODO marker exists in the code, and git history shows there never was one. The item is documented only in NOT-DONE. |
| G-F5 | CLOSED-OK | YES | BL:577-580 `if (request.AuthenticationFileId.HasValue)` is unchanged. |
| G-F6 | CLOSED-OK | YES | The SP header in `API_20260907101500` (lines 11-16) still documents it. |
| G-F7 | FIXED | YES | Vendor result: BL:531-538 returns status, delivery method, `LastDelivery` and `UpdateDate`. Importer result: BL:598-608 returns `DecisionId`, `LastDeliveryForImporter`, `UpdateDate` (now when there is a file, otherwise the date only, matching the legacy loop at L BL:731-734), plus the file fields. One clock runs from the BL to the DAL. |
| G-F8 | CLOSED-OK | YES | The literal at BL:1316-1319 matches the legacy partial `Title` (L Entities partial:104) byte for byte, including the two leading spaces. |
| G-F9 | FIXED | YES | `ReminderForImporterSchedulerJob.cs` uses a plain `foreach` with no catch and reports `SetEffectedRowsAsync(due.Count)`. This matches legacy `ScheduledTasks/ReminderForImporterScheduler.cs:22-27`. |
| G-F10 | CLOSED-OK | YES | `Schedulers.cs:85-89` carries no TODO and cites the sources. Consts:110-127 names and SP header lines 9-15 are consistent. All seven parameters are seeded in `API_20260716 - add params.sql` (lines 29-335). |

Both scheduler SPs were re-diffed against the authoritative legacy bodies. All 7 UNION branches of `GetAuthenticationRequestsForScheduler` are identical, with the same operators, statuses (2/3), delivery methods (2,3 / 4), vendor-country IN/NOT IN, the `CAST(... AS DATE)` on the 4-month rungs and export `StatusID NOT IN (6,7,8)`. The only change is that the UDFs became parameters. The parameter values come from `parametersUtil` and are not literals (`Schedulers.cs:33, 92-97`). The ladder switch (Schedulers.cs:130-163) matches L BL:749-783 on every branch. `OpenReminderTask` matches L BL:788-820: the entity type follows `IsImport`, any existing task skips (`!IsNullOrEmpty`, which equals `Count > 0`), CloseOld, and `Title = Id`. `RaiseEventForReminderForImporterScheduler` matches L BL:308-325, including ClaliMakor, CloseOld and the related file.

## 3. New findings

| ID | Sev | Operation | Deviation | New file:line | Legacy file:line | Why it matters |
|---|---|---|---|---|---|---|
| GN-01 | HIGH | IssueCertificateOfOrigin worker: feedback 2281 | The XML contract of `CertificateOfOriginRequestFeedback` differs from the legacy EAI schema. (a) Six element names are PascalCase where the schema has camelCase: `InternalApplication`/`internalApplication`, `CertificateID`/`certificateID`, `CertificateOfOriginTypeCode`/`certificateOfOriginTypeCode`, `CertificateOfOriginStatusCode`/`certificateOfOriginStatusCode`, `RejectCancelReason`/`rejectCancelReason`, `RequestReasonCode`/`requestReasonCode`. (b) The child elements are in the namespace `http://malam.com/customs/EAICommon.xsd`, where the schema puts them in `.../PC_NG_2281_MSG02_CertificateOfOriginRequestFeedback`. (c) The nullability flags differ: `InternalApplication` and `CertificateID` are `IsNullable=true` in the worker and non-nullable in the schema. | `Workers/IssueCertificateOfOrigin.Worker/Dto/CertificateOfOriginRequestFeedback.cs` (XmlType ns + properties without ElementName) | `Infrastructure/EAI/EAISchema/Customs.Inf.CertificateOfOrigins.EAISchema/PC_NG_2281_MSG02_CertificateOfOriginRequestFeedback.cs:42-90` | XML is case- and namespace-sensitive. A publish with the flag on produces a 2281 message that the agent or EAI either cannot bind or rejects, because none of the type, status, certificate number or reason fields match the schema. The legacy in-process path sent the schema class itself. It could not be checked whether the original worker already behaved this way in production. |
| GN-02 | MED | Worker: feedback 2281 | `IssueDateIfReleased` / `IssueDateIfNotReleased` are dropped. Legacy set one of them from `IssuingDate` and `IsDeclarationReleased`. The API sends both values in the message (`CertificateOfOriginsBl.cs:1647-1648`) and the worker DTO receives them (`Dto/IssueCertificateDto.cs`), but the feedback DTO has no such elements and the handler never maps them. Values are computed but never sent. | `IssueCertificateOfOriginHandler.cs` `CreateCertificateOfOriginRequestFeedbackAsync`; `Dto/CertificateOfOriginRequestFeedback.cs` | `CertificateOfOriginsUtil.cs:62-69` | For every worker-issued certificate the agent receives no issue date. Legacy always sent the date when both values were present. |
| GN-03 | MED | ChangeStatusAfterDeliverySent, CloseReminderTask | The fix for the file organization unit (F-05) was not applied to these two operations. Legacy took the event unit from `file.OrganizationUnitID`, a computed getter that returns the first child request's unit (L Entities partial:109-125). That is explicit in `ChangeStatusAfterDeliverySent` (L BL:1289) and comes via `new VirtualEntity(file)` in `CloseReminderTask` (L BL:1273). New code uses the client-sent `request.OrganizationUnitId` as is. The by-id file read has no file-level `OrganizationUnitId` (`GetAuthenticationRequestFileByIdResultDto` has none, only each child in `AuthenticationFileRequestDto:32`). | `AuthenticationRequestBl.cs:483-493, 500-512` (contrast `FileOrganizationUnitId` at :1329 used only by SaveAuthenticationRequestFile) | `AuthenticationRequestBL.cs:1270-1293` | A client that maps the file it read into the request sends 0. The repo's own note (BL:400-405) says `WithOrganizationUnitId(0)` throws `InvalidDataException`, so the result is a 5xx with no event, where legacy closed the file's tasks. If the client sends the file's stored unit instead, the CloseAll / CloseReminder3Months event is routed to a unit other than legacy's. |
| GN-04 | MED | Worker | `CertificateOfOriginQueryURL` is read from the host's `appsettings.{env}.json` (`AppSettings`) and not from `parametersUtil`. Legacy read `Configuration.GetConfig<string>("CertificateOfOriginQueryURL")`, and the API reads the same key from parameters. MIGRATION-STATUS (Workers section, item 2) records it as "to check", not as a decision. | `Dto/AppSettings.cs`; handler `CreateCertificateOfOriginRequestFeedbackAsync`; `CertificateOfOriginsWorkers/appsettings.*.json` | `CertificateOfOriginsUtil.cs:46` | This breaks the config-from-parametersUtil rule. The worker and the API can diverge, so the QueryURL in the 2281 sent by the worker can differ from the one the API returns or sends. |
| GN-05 | LOW | Worker: document save | The document's `OrganizationUnitID` is `issueCertificateDto.OrganizationUnitID ?? 4` (the certificate's unit, with a literal fallback of 4). Legacy, and the API's inline path (`CertificateOfOriginsBl.cs:865-868`), use the publishing user's unit (`UserUtil.Current.OrganizationUnitID`). | `IssueCertificateOfOriginBL.cs` `SaveCertificateOfOriginDocument` | `CertificateOfOriginsBL.cs:1286` | The document is owned by a different unit when the user's unit differs from the certificate's, and by unit 4 when the message carries none. |
| GN-06 | LOW | Worker | `UpdateIsInPublishingProcess` writes only the flag, with no `UpdateDate` or `UpdateUserId`. Every other writer in this service stamps the audit columns (G-F3 precedent). | `Dal/IssueCertificateDal.cs` `UpdateIsInPublishingProcess` | legacy saved through the repository, which stamps them | The audit trail shows the publishing request's user and time, not the worker's update. |
| GN-07 | LOW | Planar schedulers | `RaiseEventForReminderForImporterScheduler` and `OpenReminderTask` pass the row's `OrganizationUnitId` to `WithOrganizationUnitId`, which throws on 0 (per BL:400-405). The job has no catch (this is the G-F9 parity), so one row with unit 0 aborts the rest of the batch. Legacy raised the event with 0 and continued. | `Schedulers.cs:72, 185` | `AuthenticationRequestBL.cs:315, 811` | This is an edge case: stored requests were checked to carry no 0 (BL:404), but export rows were not checked. |

**Info, not a defect in G, for the orchestrator.** `CertificateOfOriginsBl.cs:1086-1090` and `:1538` (TODO(blocking)) say the 2281 outgoing channel "was removed from the platform with no replacement". The new worker does send 2281 via `ICommonCloudProxy.SendOutgoingMessage` (`POST Common/OutgoingMessage`, `Proxies/CommonCloudProxy.cs`). The API's TODO rationale may be stale and the channel may be reusable for the inline publish feedback.

**Checked, no deviation:**
- The MIGRATION-STATUS concern about field-name casing between API and worker (`CertificateTypeId` vs `CertificateTypeID` and so on). The worker reads with Newtonsoft `JToken.ToObject<T>(CoreSerializer.Serializer)` (decompiled `InfrastructureCore.Worker` 1.10.109, `GetEntity`), and Newtonsoft matches property names case-insensitively by default. Unless `CoreSerializer` overrides the contract resolver (not decompiled), the four fields bind.
- The API send gate: `issueByWorker && IsInPublishingProcess && ReportId` matches L BL:1148.
- The worker gate: Published + `IsInPublishingProcess`.
- Document deletion before upload, the draft/final title, document type 329 (= `EDocumentType.ExportCertificateOfOrigin`) and additional field 46 (= `CertificateOfOriginsConsts.DocumentAdditionaFieldIDForCertificateNumber`) all match the legacy `SaveCertificateOfOriginAttachments`.
- The status machine `AdvanceDeliveryStatus` (BL:614-640) matches L BL:701-726.
- The vendor reminder override (BL:521-523) matches L BL:346-347.
- The importer event (type, title, unit from the request, related file only when present) is raised after the writes, matching L BL:501-520.
- The ChangeStatus and CloseReminderTask event types, titles and related entity are unchanged.
