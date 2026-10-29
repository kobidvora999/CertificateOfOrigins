# Parity review A — certificate core (Internal contract)

Scope: InternalGetCertificateOfOriginsByFilter, InternalIsCertificateOfOriginByExternalIdExist, InternalGetCertificateOfOriginById,
InternalSaveCertificateOfOrigin, InternalLoadDataFromExportDeclaration.

Abbreviations:
- **L-SVC** = `C:\Repos\Main\CRM\CertificateOfOrigins\Server\Customs.CRM.CertificateOfOrigins.Service\CertificateOfOriginsInternalServicePartial.cs`
- **L-BL** = `C:\Repos\Main\CRM\CertificateOfOrigins\Server\Customs.CRM.CertificateOfOrigins.BL\CertificateOfOriginsBL.cs`
- **L-EVT** = `C:\Repos\Main\CRM\CertificateOfOrigins\Server\Customs.CRM.CertificateOfOrigins.BL\RaiseEventUtil.cs`
- **L-SP** = `C:\Repos\Main\CustumsDev_Database\CustumsDev_Database\CRM\Stored Procedures\usp_CertificateOfOrigins_*.sql`
- **N-CTL** = `C:\Repos\CertificateOfOrigins\API\CertificateOfOrigins.WebApi\Controllers\Ui\CertificateOfOriginsController.cs`
- **N-BL** = `C:\Repos\CertificateOfOrigins\API\CertificateOfOrigins.BL\CertificateOfOriginsBl.cs`
- **N-VAL** = `C:\Repos\CertificateOfOrigins\API\CertificateOfOrigins.BL\Validations\SaveCertificateOfOriginRequestValidator.cs`
- **N-DAL** = `C:\Repos\CertificateOfOrigins\API\CertificateOfOrigins.DAL\CertificateOfOriginsDal.cs`
- **N-SP** = `C:\Repos\CertificateOfOrigins\API\CertificateOfOrigins.WebApi\Scripts\API_* - dbo.*.sql`

## 1. Per-operation table

| Operation | New endpoint | DTOs present | BL method | DAL / IDal | Parity verdict |
|---|---|---|---|---|---|
| GetCertificateOfOriginsByFilter | `QUERY ui/CertificateOfOrigins/ByFilter` (body) | CertificateOfOriginFilterDto, CertificateOfOriginResultDto | `GetCertificateOfOriginsByFilter` + `FillCustomersInformation` (N-BL:724-793) | `GetCertificateOfOriginsByFilter` → `dbo.GetCertificateOfOriginsByFilter` (N-DAL:757) | gaps (MED x2, LOW x2) |
| IsCertificateOfOriginByExternalIdExist | `GET ui/CertificateOfOrigins/ByExternalIdExist?certificateOfOriginExternalId=` | CertificateOfOriginResultDto | `IsCertificateOfOriginByExternalIdExist` (N-BL:716) | via ByFilter | clean (LOW note: legacy was probably broken) |
| GetCertificateOfOriginById | `GET ui/CertificateOfOrigins/{id}` | CertificateOfOriginDto (+ detail/invoice/item/milestone DTOs) | `GetCertificateOfOriginById` + `FillMilestoneUserNames` (N-BL:26-63) | `GetCertificateOfOriginById` → `dbo.GetCertificateOfOriginByID`, 7 result sets (N-DAL:990; DbContextExtension:119) | clean (LOW x2) |
| SaveCertificateOfOrigin | `POST ui/CertificateOfOrigins` | SaveCertificateOfOriginRequestDto, CertificateOfOriginDetailDto | `SaveCertificateOfOrigin` (N-BL:910-1041) + helpers | GetLatestCertificateByNumber, CancelPreviousCertificate, Stage*/MergeChildrenAsync, Update* (N-DAL) | **gaps (HIGH x2, MED x7, LOW x7)** |
| LoadDataFromExportDeclaration | `QUERY ui/CertificateOfOrigins/LoadDataFromExportDeclaration` (body) | LoadDataFromExportDeclarationRequestDto | `LoadDataFromExportDeclaration` (N-BL:657) | none (IExportDealFileProxy) | clean (LOW: by-ref mutation dropped by documented decision; proxy endpoint TODO(blocking)) |

`dbo.GetCertificateOfOriginNumber`: diffed against `usp_CertificateOfOrigins_GetCertificateOfOriginNumber`. Same body (`NEXT VALUE FOR CRM.sq_CertificateOfOrigins_CertificateOfOrigin`). Clean. None of the five operations calls it directly; only the message create path uses it.

## 2. Findings

### HIGH

**A-01: SaveCertificateOfOrigin. Received + CertificateUpdate no longer raises ApplicationCorrected**
- Class: dropped side effect (event).
- Legacy: L-EVT:69-78. `RaiseEventForStatusReceived` raises ApplicationReceived. Then, when `RequestReasonCode == CertificateUpdate`, it also raises `CertificateOfOriginApplicationCorrected`, followed by `RaiseNewCertificateOfOriginCreatedEvent`. That last call is a no-op because IsExportDeclarationActive is always true.
- New: N-BL:1383-1409. `RaiseStatusEvents` maps Received to ApplicationReceived only, with no reason-code branch. The only ApplicationCorrected in the new code is the supersede one (N-BL:989). It fires only when a previous version with the same number exists, and legacy raised that one separately at L-BL:950.
- Impact: a Received certificate whose reason is CertificateUpdate loses one ApplicationCorrected event. This matters whenever no same-number predecessor exists. When one does exist, legacy raised two events and the new code raises one. Downstream task and event rules keyed on 610 are affected.
- Marked TODO: no. Verdict: CONFIRMED.

**A-02: SaveCertificateOfOrigin. DeclarationMatch event type and org-unit type are wrong**
- Class: changed event and literal (enum), dropped conjunct.
- Legacy: L-EVT:220-240.
  - For RequestReasonCode in {EmptyCertificate, Draft, GetRequestStatus, CertificateCancellation}, the event is `CertificateMatchDeclarationWithoutTask` (1913). Otherwise it is `CertificateOfOriginCertificateMatchDeclaration`.
  - The VirtualEntity org-unit type is set to `EOrganizationUnitType.Export`.
  - DeclarationMismatch (L-EVT:242-247) also sets org-unit type Export.
- New: N-BL:1391-1392 and 1408. DeclarationMatch always raises `CertificateOfOriginCertificateMatchDeclaration`. `EEventType.CertificateMatchDeclarationWithoutTask` is defined in `Model/ModelDTOs/EEventType.cs:84` but never used. `RaiseCertificateEvent` is called without `organizationUnitTypeId` for both Match and Mismatch.
- Impact: for the no-task reasons, the new code opens a task that legacy suppressed (CR 156814). Routing of match and mismatch events loses the Export org-unit type.
- Marked TODO: no. Verdict: CONFIRMED.

### MED

**A-03: SaveCertificateOfOrigin. Omitted detail rows are hard-deleted**
- Class: replaced side effect (data-loss risk).
- Legacy: L-BL:1103-1107. A self-tracking-entity `Repository.Save(certificateOfOrigin)` persists only tracked changes. A detail row missing from the collection is not deleted unless it was marked deleted.
- New:
  - N-BL:974 → N-DAL:128-139 `StageCertificateOfOriginDetails` → `MergeChildrenAsync` (N-DAL:730-742) runs `ExecuteDeleteAsync` on every existing detail whose id was not sent. An empty list deletes all.
  - The DTO defaults `CertificateOfOriginDetails = []`.
  - The same risk was recognised and guarded for invoices (N-DAL:141-150 empty-list guard) but not for details.
  - `ExecuteDeleteAsync` runs immediately. It is not staged, despite the "stage" comment.
- Impact: any caller that saves a header-only body, such as a status change, wipes the certificate's details.
- Marked TODO: no. Verdict: CONFIRMED (the code path). The impact depends on whether every caller round-trips the full detail graph.

**A-04: SaveCertificateOfOrigin. Detail enrichment and validation now also run for new instances**
- Class: added behaviour. The branch condition was dropped.
- Legacy: L-BL:974-1000.
  - `if (!IsNewInstance)`: CheckSpecificField runs per detail.
  - `else`: only the DestinationCountry (alpha-2 → id) and PortOfShipment (locode → id) conversions run.
  - New instances keep the client's Value and DisplayedValue.
- New: N-BL:949 → N-BL:1277-1355. `EnrichAndValidateDetails` runs the new-instance code resolution, then the full CheckSpecificField-equivalent loop for every save, including `entity.Id == 0`.
  - For a new instance, ExporterId `Value` is looked up with `GetCustomerIdByExternalId`. On the message create path the value is already the resolved internal id, so a numeric collision can rewrite it to another customer.
  - DisplayedValue is overwritten for every detail. Text types get the raw value; country-group and international-site types get the raw id.
  - Extra CustomsBook trade-agreement calls are made.
- Marked TODO: no. The NOT-DONE doc presents it as "improves fidelity", but it is a deviation for new instances. Verdict: CONFIRMED.

**A-05: SaveCertificateOfOrigin. isRemarksChanged semantics changed; can send feedback on create**
- Class: operator / flag source of truth.
- Legacy: L-BL:936, 1098-1108. The original value comes from the self-tracking-entity OriginalValues. For an Added entity, and for an unchanged remark, it is null, so `isRemarksChanged = false`. It is true only when a non-null original differs.
- New: N-BL:1005. `!string.Equals(entity.FeedbackRemark, request.OriginalFeedbackRemark)`.
  - A new certificate with any FeedbackRemark (OriginalFeedbackRemark null) gives true, which calls `SendRequestFeedback` (N-BL:1024-1027). Legacy never sent feedback on create for a remark alone.
  - null → value on update gives true; legacy gave false.
- Marked TODO: no. Verdict: CONFIRMED.

**A-06: SaveCertificateOfOrigin. isStatusChanged relies on a client-sent non-nullable snapshot**
- Class: flag source of truth. The Received quirk is not reproduced.
- Legacy: L-BL:1058-1092.
  - The original status comes from the self-tracking entity. When it is not tracked (status unchanged) and the status is Received, legacy still treats it as changed, re-raising the Received events and feedback.
  - A tracked change compares against the original value.
- New: N-BL:1002-1004. For an existing instance, `entity.Status != request.OriginalCertificateOfOriginStatusId`. That DTO field is `int` with default 0 (SaveCertificateOfOriginRequestDto:43).
  - A caller that omits it gets `isStatusChanged = true` on every save. That re-raises status events, sends feedback and, when Published, re-runs PublishAttachments: IssuingDate is reset, the template is regenerated and the issue queue is sent again. No idempotency guard exists (see A-14).
  - Conversely, the legacy "Received and unchanged → changed" re-fire is gone.
- Marked TODO: no. Verdict: CONFIRMED (code). The impact depends on the SPA contract.

**A-07: SaveCertificateOfOrigin. Validator rejects OrganizationUnitId = 0, which legacy accepted**
- Class: added guard (stricter than legacy).
- Legacy: L-BL:1101-1102, 1111-1122. Only the entity's own `Validate()` is applied. The new code's own comment (N-BL:1557-1559) states that legacy saved NonManipulation certificates with OrganizationUnitID 0.
- New: N-VAL:35-37 requires `OrganizationUnitId > 0`. The message create path builds `OrganizationUnitId = context.OrganizationUnitId ?? 0` (MessageValidation.cs:254).
- Impact: a certificate without a customs house (NonManipulation) gets a 400 instead of being saved.
- Marked TODO: no. Verdict: CONFIRMED (code). Needs a check of whether OU 0 can still occur.

**A-08: SaveCertificateOfOrigin. HandleCertificateReplacement also clears IsLastVersion on the replaced certificate**
- Class: added side effect, reused helper.
- Legacy: L-BL:356-377. It sets `certificateToCancel` status to Cancelled and overwrites RejectCancelReason with the CertificateReplaced text. It does not touch IsLastVersion.
- New: N-BL:1543 calls `CancelPreviousCertificate(id, string.Empty, …)` (N-DAL:360-372). That sets Cancelled, appends "" to the old reason instead of overwriting it, and sets `IsLastVersion = false`.
- Impact: the replaced certificate, which has a different number, disappears from `isLastVersion = 1` searches.
- Marked TODO: partially. The RejectCancelReason text is TODO(migration) at N-BL:1524-1528. The IsLastVersion change is not marked. Verdict: CONFIRMED.

**A-09: SaveCertificateOfOrigin. CheckDeclarationStatus lead-document repoint dropped**
- Class: dropped side effect (remote call).
- Legacy: L-BL:1024 → L-BL:648-669. When a replacement id or a previous same-Title certificate exists, it calls `ChangeCertificateOfOriginIDForLeadDocument(leadDocId, oldId, newId)` on ExportDealFile.
- New: N-BL:1358-1377. Only `GetLeadDocumentByOldCertificateOfOriginIdAndUpdateToNewCertificateOfOriginId` is called. CheckDeclarationStatus is not called.
- Marked TODO: yes (N-BL:1374, "CheckDeclarationStatus are deferred"; ExportDealFile not stood up). This is a documented deferral, but it is a real DealFile link gap. Verdict: CONFIRMED.

**A-10: SaveCertificateOfOrigin → PublishAttachments. Issue-queue payload differs from legacy**
- Class: computed-but-not-propagated / dropped guard.
- Legacy: L-BL:1146-1157. The queue is used only when the certificate type has a `ReportId`, and the payload carries `ReportId` and `IsDeclarationReleased`.
- New: N-BL:1494-1519 and IssueCertificateDto. There is no ReportId and no IsDeclarationReleased. The message is sent whenever IssueCertificateOfOriginByWorker is on, regardless of the type's ReportId.
- Impact: the worker may not know which report to render.
- Marked TODO: no. The comment claims "mirroring QueueUtilFactory 1:1". Verdict: PLAUSIBLE. Verify against the worker consumer contract.

**A-11: GetCertificateOfOriginsByFilter. TOP(shared.ufn_GetMaxRows()) replaced with literal TOP(200)**
- Class: literal replacing a config value.
- Legacy: L-SP `usp_CertificateOfOrigins_GetCertificateOfOriginsByFilter.sql` SELECT, `TOP (shared.ufn_GetMaxRows())`.
- New: N-SP `API_20260721 - dbo.GetCertificateOfOriginsByFilter.sql:34`, `TOP (200)`.
- The function body was not found in the repo, so I could not confirm that it returns 200.
- Marked TODO: no. Verdict: PLAUSIBLE.

**A-12: GetCertificateOfOriginsByFilter. INNER JOIN on customers removed; titles can be silently NULL**
- Class: dropped filter / silent NULL.
- Legacy: the SP INNER JOINs `Shared.rStockPileData_Customers_Customer` twice (exporter and agent). Rows without a customer are excluded, and Title and ExternalIdNum always come from the DB.
- New:
  - The SP returns NULL titles (N-SP:39-44).
  - `FillCustomersInformation` (N-BL:754-793) returns early, leaving the titles NULL, when the proxy returns null.
  - The Customers route is `TODO(blocking): confirm endpoint` (CustomerProxy.cs:17).
  - `CustomerDto.Name` is used where legacy used `Customer.Title`.
  - Rows with orphan customer ids now appear.
- Marked TODO: partially (endpoint only). Verdict: CONFIRMED.

### LOW

- **A-13: Save. Publish feedback and PDF attachment not sent.** Legacy L-BL:379-418. New N-BL:1012-1017. Marked TODO(blocking) #1: yes. Documented deferral.
- **A-14: Save. IsCreateAttachments / IsMessageSent idempotency guards missing.** Legacy L-BL:381, 424. New N-BL:1013-1016. Marked TODO(blocking) #3: yes. Makes A-06 worse.
- **A-15: Save. CheckCertificateOfOriginOnDeclarationReleased (auto-publish) not migrated.** This covers the service-partial call at L-SVC:60-63 and the second, identical path inside `RaiseEventCertificateOfOriginUserApprovedCertificate` (L-EVT:169-195). New N-BL:1035-1039. Marked TODO(blocking) #2: yes, though the TODO names only the service-partial call site.
- **A-16: Save. HandleCertificateReplacement texts and talk-back deferred.** Covers the FeedbackRemark `UpdateExportDeclaration`, `SendMessageToAgent`, and the CertificateReplaced reason. Legacy L-BL:362-365. New N-BL:1524-1528. Marked TODO(migration): yes.
- **A-17: Save. SendRequestFeedback goes through IMessageManagementProxy with `MessageTypeId = 0` and no attachment.** Legacy L-BL:422-440. New N-BL:1421-1435. Marked TODO(blocking): yes.
- **A-18: Save. Hardcoded 0 fallback for the user.** `RequestMetadata.UserId ?? 0` is used for ApproveUserId and the audit and cancel writes (N-BL:926, 954, 1543). Legacy used `UserUtil.Current.ID` (L-BL:1003). Marked TODO: no. Negligible if the auth middleware always sets UserId.
- **A-19: Save. Title-mismatch validation dropped.** Legacy L-BL:1014-1017 adds `CertificateNumberISNotMatch…` to `_requestExceptions`, but that list is never thrown on the Internal save path, so there is no behavioural effect. New N-BL:1374, TODO: yes.
- **A-20: Save. QrCodePath stores the document ExternalId instead of `DocumentRepositoryUtil.GetDocumentFile(urlPath)`.** Legacy L-BL:1050. New N-BL:1193. The code uses it only as an "already generated" flag. Marked TODO: no.
- **A-21: Save. ExporterId DisplayedValue.** New sets it to the Value (N-BL:1300); legacy CheckSpecificField left it untouched (L-BL:1458-1462). Country-group and international-site / site DisplayedValue fall back to the raw id (TODO(migration): yes, N-BL:1241-1243).
- **A-22: Save. Validation shape changed.** Legacy `entity.Validate()` → InfException(CertificateOfOriginNotValid) (L-BL:1111-1120). New FluentValidation → RestValidationException 400 (N-BL:915-924). The HTTP-code mapping changed; this is acceptable.
- **A-23: GetById. Missing id now returns 404; legacy returned null.** N-BL:30-31. This is a documented choice in the code comment. Internal WCF callers that null-checked must now handle 404.
- **A-24: GetById milestones.** Legacy INNER JOIN `Infrastructure.UserMng_User u ON COO.UpdateUserID` dropped milestone rows with unknown users. New returns them with UserName NULL (N-SP GetCertificateOfOriginByID, result set 7). `user.Name` is used where legacy used `Title` (N-BL:59). Header and result sets 1-6 are identical to legacy (diffed).
- **A-25: ByFilter date bounds.** `Shared.ufn_General_GetDateStart/End` was replaced with inline CAST/DATEADD (day start, 23:59:59.997). This is presumed equivalent; the function bodies were not found.
- **A-26: IsCertificateOfOriginByExternalIdExist.** Legacy L-BL:186-191 calls `ExecuteFunction<>(filter)` with `CertificateOfOriginFilter.GetFunctionParameters()`. That method emits copy-pasted, duplicate ObjectParameter names (EntitlementID, CustomerID, …) (`Common\Internal\...\CertificateOfOriginsFilter.cs:75-95`), so the legacy call was very likely broken at runtime. New routes through the proper 16-parameter SP call (LIKE `%number%`, newest first). This is the intended behaviour, not a regression. The new code also makes an extra Customers proxy call.
- **A-27: LoadDataFromExportDeclaration.** The legacy by-ref mutation of IsDeclarationReleased and IsCargoExitedOfCustomsRegulation (L-BL:294-295) is not returned. WCF did not marshal it back to the caller either, so the verdict is clean. The guard and return logic are equivalent (N-BL:662-675). The ExportDealFile proxy endpoint is TODO(blocking) (ExportDealFileProxy.cs:14).

### Grep categories within scope
- **PushUtil:** none in the legacy chain for these operations.
- **Hardcoded user context:** only the `?? 0` fallbacks (A-18). No `= 1` literals.
- **Mock proxies:** all registered with `AddProxy<I, Real, Mock>`; real is the default and mock is used only under the `x-mock-mode` header. The ExportDealFile, Customers and CustomsBook endpoints are TODO(blocking) unconfirmed.
- **TODO(confirm):** none in the scoped methods.
- **"out of scope" / "heavy Save()":** none found in the scoped methods.

## 3. Already documented in MIGRATION-NOT-DONE.md (checked last)

| ID | Documented |
|---|---|
| A-01 | no |
| A-02 | no |
| A-03 | no (only the invoice empty-list guard is documented) |
| A-04 | no (described as a fidelity improvement) |
| A-05 | no |
| A-06 | no |
| A-07 | no |
| A-08 | no |
| A-09 | yes (#2 dependency, "repoint של lead-document") |
| A-10 | no |
| A-11 | no |
| A-12 | partially (endpoint TODOs only) |
| A-13 | yes (#1) |
| A-14 | yes (#3) |
| A-15 | yes (#2) |
| A-16 | yes (generic resx TODOs) |
| A-17 | yes |
| A-18 to A-27 | no (negligible) |
