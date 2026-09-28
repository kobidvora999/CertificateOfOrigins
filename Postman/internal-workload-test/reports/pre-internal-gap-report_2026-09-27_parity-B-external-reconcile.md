# Parity review B — External: UpdateCetrificateOfOrigins (Reconcile) + other Api/CertificateOfOriginsController endpoints

Reviewer: adversarial, read-only. Legacy root `C:\Repos\Main\CRM\CertificateOfOrigins\` (abbrev. **L:**), new root
`C:\Repos\CertificateOfOrigins\API\` (abbrev. **N:**).

Key files:
- L: `Server/Customs.CRM.CertificateOfOrigins.Service/CertificateOfOriginsExternalServicePartial.cs` (ESP)
- L: `Server/Customs.CRM.CertificateOfOrigins.BL/CertificateOfOriginsBL.cs` (LBL)
- N: `CertificateOfOrigins.WebApi/Controllers/Api/CertificateOfOriginsController.cs` (CTRL)
- N: `CertificateOfOrigins.BL/CertificateOfOriginsBl.cs` (NBL), `CertificateOfOriginsBl.Templates.cs` (NTPL)
- N: `CertificateOfOrigins.DAL/CertificateOfOriginsDal.cs` (DAL)

Note: all "non-contract" endpoints turned out to be real External WCF operations (ESP), so each was compared 1:1.

## 1. Per-operation table

| Operation (legacy) | New endpoint | DTOs present | BL method | DAL / IDal | Parity verdict |
|---|---|---|---|---|---|
| UpdateCetrificateOfOrigins — dispatcher (ESP:96-123) | `POST api/CertificateOfOrigins/Reconcile` | UpdateCertificateOfOriginsRequestDto, ExportInvoiceInfoDto, ExportGoodsItemInfoDto, CertificateOfOriginExceptionDto | UpdateCertificateOfOrigins (NBL:1588) | — | OK, except the IsExportDeclarationActive gate is dropped (TODO(confirm)) |
| ↳ ExportDeclarationSubmissionSucceeded → UpdateCertrificateOfOrigins (LBL:468-646, 720-887, 1935-1995) | same | same | ReconcileCertificatesAgainstDeclaration / ValidateReconciliation / ApplyReconciliationOutcome / RaiseCertificatePreferredAssessorEvent / RaiseDeclarationHasWarningsEvent (NBL:1616-2373) | GetCertificatesByIds, GetCertificateDetailsByCertificateIds, GetCertificateInvoiceDetailsByCertificateIds, GetCertificateTypeIsCustomsItemMandatory, UpdateCertificateReconciliation, AddCertificateVsDeclarationErrors | **DEVIATES**: H1, H2, M1, M2, M3, M4, M7 |
| ↳ ExportDeclarationReleased / AssemblySharedReleaseAccepted → DeclarationReleased (LBL:310-354, 356-420) | same | same | DeclarationReleased (NBL:1729), HandleCertificateReplacement (NBL:1529), PublishAttachments (NBL:1437) | GetCertificatesByIds, UpdateCertificateReconciliation, UpdateCertificateQrCode, UpdateCertificateDeclarationLink, CancelPreviousCertificate | **DEVIATES**: H3, M5, M6, M1 (stripped-DTO path) |
| ↳ ExportDeclarationAmendmentRequestCompleted → ExportDeclarationAmendmentSuccess (LBL:446-467) | same | same | ExportDeclarationAmendmentSuccess (NBL:1697) | UpdateCertificateDeclarationLink | OK (inherits reconcile findings) |
| ↳ CancellationRequestCommited → ExportDeclarationCancellationRequestCommited (LBL:889-914) | same | same | ExportDeclarationCancellationRequestCommited (NBL:1811) | CancelCertificateFromMessage | OK apart from documented deferrals (L8) and M3 |
| GetCertificateOfOriginID (ESP:125-132) | `GET api/CertificateOfOrigins/ID/{certificateNumber}` | — (int) | GetCertificateOfOriginID (NBL:677) | GetCertificateOfOriginIdByNumber (DAL:13) | OK logic; not-found → 404 instead of null (L13) |
| GetGoodsItemCerificateDTO (ESP:134-146) | `POST api/CertificateOfOrigins/GoodsItemCerificateDTO` | GoodsItemCerificateDto | GetGoodsItemCerificateDTO (NBL:703) | GetCertificateOfOriginIdByNumber | OK; minor null-number difference (L12) |
| Convert (ESP:33-69) | `POST api/CertificateOfOrigins/Convert` | ConnectedEntityDto, VirtualEntityDto | Convert (NBL:639) | GetCertificateOfOriginsByFilter (SP) | OK; 404 vs InfException, extra Customers dependency (L14) |
| SaveCertificateOfOriginAttachments (ESP:148-155 → LBL:1275-1316) | `POST api/CertificateOfOrigins/SaveAttachments` | SaveCertificateAttachmentsArgsDto, TemplateResultDto | SaveCertificateOfOriginAttachments (NBL:795) | — (IDocumentsProxy / IDocumentUtil / IUserProxy) | OK (documented decisions, L15) |
| — (CR 194221, new) | `GET api/CertificateOfOrigins/Template/{templateId}/{entityId}` | SouthKoreaOriginVerificationLetterResult, PrintTemplateDto, ECertificateOfOriginsTemplate | GenerateTemplate / GetTemplateData (NTPL:24-80) | GetTemplateData<T> (DAL:1172) → `dbo.GetTemplateData` | No legacy counterpart; compared to closest analogue — LOW notes only (L16) |

## 2. Findings

### H1: The reconcile path resolves the assessor from the stale (pre-backfill) LeadDocumentId
- **Operation:** UpdateCetrificateOfOrigins / ExportDeclarationSubmissionSucceeded
- **Severity:** HIGH
- **Class:** computed-but-not-propagated value
- **Legacy:** LBL:492-500 backfills `item.LeadDocumentID` in memory. The same item then goes to `RaiseTaskNewCertificateOfOriginCheck(item, …)` (LBL:526 → 1946-1956, `if (certificateOfOrigin.LeadDocumentID.HasValue)`) and to the warnings branch (LBL:607 `if (item.LeadDocumentID.HasValue)`).
- **New:** NBL:1640-1651 computes the backfill into **local variables** `exportDeclarationNumber` / `leadDocumentId`. It persists them only at NBL:1681 and never assigns them to `certificate`. `ApplyReconciliationOutcome` → `RaiseCertificatePreferredAssessorEvent` (NBL:1903) and `RaiseDeclarationHasWarningsEvent` (NBL:1956) both read `certificate.LeadDocumentId`, which is still the DB value.
- **Effect:** a certificate reconciled for the first time usually has no LeadDocumentId in the DB. For it, `ResolveAssessorUserId` returns null. The match event (642) then carries no PreferredUserId, and the warnings event carries no task assignment. The legacy set both.
- **Marked TODO:** no. **Verdict:** CONFIRMED (re-read both sides).

### H2: Scalar and import-replacement findings now count when the certificate has no invoice rows; the legacy discarded them
- **Operation:** UpdateCetrificateOfOrigins / reconcile validator
- **Severity:** HIGH
- **Class:** changed boolean logic / dropped guard (the reverse direction)
- **Legacy:** ValidateExportDeclarationInfoForPCIsMatch (LBL:720-876) collects these findings into a **local** `exceptions` list: destination country (733-737), destination group (740-751), declaration number (754-758), exporter (759-760) and the import-replacement warning (761-771). The only hand-off to `_requestExceptions`, which drives hasErrors/hasWarnings at LBL:513-514, is `PassGroupdExceptionListToEntity`. That call happens only **inside** `if (!item.CertificateOfOriginInvoiceDetail.IsNullOrEmpty() && !ExportInvoiceInfoDTOList.IsNullOrEmpty())` (LBL:772 … 800 / 871-874, block closes at 875).
- **Effect in the legacy:** a certificate with no invoice rows loses every scalar mismatch and ends as **DeclarationMatch** (event 642). `isLinkedToImportDeclaration` can still be true, but no warning is raised.
- **New:** ValidateReconciliation (NBL:2028-2040) always adds the ValidateCertificateDetails and ValidateImportReplacement findings. Only the invoice matching is gated on `invoices.Count > 0`.
- **Effect in the new code:** the same certificate ends as **Rejected** (event DeclarationMismatch) or DeclarationMismatch (warnings event). The new code also writes VsDeclarationError rows.
- **Why it matters:** this is a different status outcome for the same input. The legacy behaviour is arguably a bug, but it is what production does, and nothing in the code documents the change.
- **Marked TODO:** no. **Verdict:** CONFIRMED (brace structure re-read).

### H3: On release, the replaced certificate is never cancelled because CertificateIdToCancel is not loaded
- **Operation:** UpdateCetrificateOfOrigins / ExportDeclarationReleased + AssemblySharedReleaseAccepted
- **Severity:** HIGH
- **Class:** dropped side effect
- **Legacy:** LBL:341-343 → HandleCertificateReplacement (LBL:356-375) uses `certificate.CertificateIDToCancel` (a mapped EF column, EDMX `CertificateIDToCancel`). It cancels the old certificate, sets its RejectCancelReason and raises CertificateOfOriginCertificateReplaced.
- **New:** DeclarationReleased loads certificates through `DataLayer.GetCertificatesByIds` (DAL:181-200). That projection selects Id, TypeId, CertificateNumber, StatusId, RequestReasonCode, LeadDocumentId, ExportDeclarationNumber, OrganizationUnitId, RejectCancelReason and CreateDate, but **not CertificateIdToCancel**. So `HandleCertificateReplacement` (NBL:1531) always sees null and returns.
- **Effect:** there is no cancellation, no Replaced event, and the old certificate stays active.
- **Marked TODO:** no. The TODO at NBL:1524-1528 covers only the texts and the agent message. **Verdict:** CONFIRMED.

### M1: A "no export invoices" finding the legacy never produced is persisted, returned and sent as AdditionalInfo
- **Operation:** UpdateCetrificateOfOrigins (submission, plus the release path's stripped DTO)
- **Severity:** MED
- **Class:** added side effect
- **Legacy:** LBL:508-509 sets `hasErrors = true` when there are no invoices and adds **no** exception. The result is Rejected, no VsDeclarationError row, AdditionalInfo empty, and an empty list returned.
- **New:** NBL:2018-2026 adds `NoExportInvoices` (Error, text "אין חשבוניות בהצהרת היצוא"). It is written as a CertificateOfOriginVsDeclarationError row (NBL:1686), put into the mismatch event AdditionalInfo (NBL:1843-1847), and returned to the caller.
- **Effect:** the DeclarationReleased path always strips the invoices (NBL:1796-1802). So every backfilled Received certificate on release gets this extra row and text.
- **Marked TODO:** no. It is only commented "migration-added" at NBL:2382. **Verdict:** CONFIRMED.

### M2: The Rejected reason text differs from the legacy constant
- **Operation:** UpdateCetrificateOfOrigins
- **Severity:** MED
- **Class:** literal changed
- **Legacy:** `CertificateOfOriginsConsts.ThereIsNoMatchBetweenTheCertificateDataAndTheDeclaration = "אין התאמה בין נתוני התעודה להצהרה"`, a code constant in `Common/Internal/Customs.CRM.CertificateOfOrigins.InternalCommon/CertificateOfOriginsConsts.cs:39`, used at LBL:569.
- **New:** `ReconciliationMismatchReason = "אין התאמה בין נתוני התעודה לבין ההצהרה"` (N: `CertificateOfOrigins.BL/CertificateOfOriginsConsts.cs:64`), used at NBL:1848.
- **Effect:** a different persisted RejectCancelReason, which the customer sees.
- **Marked TODO:** yes, but the TODO is wrong. It says "source from ValidationMessages/resx", while the legacy value is a plain code constant that could be copied exactly. **Verdict:** CONFIRMED.

### M3: Event entity Title and CustomerId differ from the legacy (cross-cutting)
- **Operation:** all branches of UpdateCetrificateOfOrigins
- **Severity:** MED
- **Class:** changed event payload
- **Legacy:** events are built with `new VirtualEntity(item)` / `EventUtilArguments(evt, item)`. The constructor (`Malam.Infrastructure/…/Base Entities/VirtualEntity.cs:23,31`) copies `Title = entity.Title`, which is CertificateNumber (Incoming partial :1610), and `CustomerID = entity.CustomerID`. Call sites: LBL:570, 597, 909, 1941. The import-replacement event uses `Title = "OpenTaskHandlingTheReplacementOfAnImportCertificate"` (LBL:588).
- **New:** RaiseCertificateEvent (NBL:1557), RaiseCertificatePreferredAssessorEvent (NBL:1908) and RaiseDeclarationHasWarningsEvent (NBL:1961) all use `.WithTitle(certificateId.ToString())` and never set a customer.
- **Effect:** task and event titles show the internal id instead of the certificate number, and the customer link is lost.
- **Marked TODO:** no. **Verdict:** CONFIRMED for the payload difference; the user-visible impact (task title rendering) is PLAUSIBLE.

### M4: The draft re-print passes the certificate TypeId as the template/report id
- **Operation:** UpdateCetrificateOfOrigins reconcile re-print (and the publish path)
- **Severity:** MED
- **Class:** wrong value source
- **Legacy:** PrintCertificateOfOriginAndSaveAttachments (LBL:1127-1215) renders SSRS by `certificateTypeCode.ReportId` from the type-code table. When there is no ReportId, it picks a per-type template id (e.g. SouthKorea 2391/2392), with 2-page variants chosen by `IsAttachedList`.
- **New:** NBL:1473 calls `commonServicesProxy.GenerateTemplate(certificate.TypeId, certificate.Id, additionalInfo)`. That sends the type id (1..9) as `{templateId}`. The new model has no ReportId column at all.
- **Effect:** even under the "SSRS only" developer decision, the id sent is not the legacy report id.
- **Marked TODO:** the proxy route is TODO(blocking) (CommonServicesProxy.cs:24), but the TypeId-as-id substitution itself is not flagged. **Verdict:** CONFIRMED.

### M5: The release publish runs on a partial projection
- **Operation:** DeclarationReleased
- **Severity:** MED
- **Class:** silent default values / dropped guard
- **Cause:** GetCertificatesByIds (DAL:181-200) omits several columns.
  - **QrCodePath:** CreateQrCodeIfNeeded (NBL:1159) always sees an empty path. It regenerates the QR and **overwrites the Guid**, then persists at NBL:1768. The legacy skipped this when a path existed (LBL:1034).
  - **Issue-queue payload fields:** SendCertificateToIssueQueue (NBL:1496-1512) puts CreateCustomerId, InternalApplication and FeedbackRemark into the payload from the entity, and these are not loaded. They go out as 0/false/null.
  - **ReportId:** the legacy queued only when the type had a ReportId, and included ReportId in the payload (LBL:1143-1148). The new code queues whenever IssueCertificateOfOriginByWorker is true, with no ReportId.
- **Marked TODO:** no. **Verdict:** CONFIRMED for the projection gap. The worker impact is PLAUSIBLE.

### M6: HandleCertificateReplacement reuses the supersede write — it clears IsLastVersion and does not replace the reason (latent while H3 stands)
- **Operation:** DeclarationReleased
- **Severity:** MED
- **Class:** added side effect
- **Legacy:** LBL:364-365 sets the old certificate to Cancelled and **replaces** RejectCancelReason with EMessages.CertificateReplaced. It does not touch IsLastVersion.
- **New:** NBL:1543 → `CancelPreviousCertificate(id, string.Empty, …)` (DAL:360-372). That **appends** an empty string to the reason and **sets IsLastVersion = false**.
- **Effect:** once H3 is fixed, replaced certificates drop out of IsLastVersion searches.
- **Marked TODO:** partially. The reason text is marked; the IsLastVersion change is not. **Verdict:** CONFIRMED.

### M7: Explicit `null` for the lists now fails with 400 instead of the legacy's null handling
- **Operation:** UpdateCetrificateOfOrigins
- **Severity:** MED
- **Class:** changed HTTP/exception mapping
- **Legacy:** `IsNullOrEmpty` guards on `CertificateOfOriginsIDs` (LBL:471 → return) and `ExportInvoiceInfoDTOList` (LBL:508 → Rejected).
- **New:** these are non-nullable `List<>` properties with `<Nullable>enable</Nullable>` on an `[ApiController]`. ASP.NET treats them as implicitly required, so an explicit `null` from DealFile gives a 400 and nothing is processed. The per-item `ExportGoodsItemInfoList` behaves the same way.
- **Marked TODO:** no. **Verdict:** PLAUSIBLE (depends on the DealFile payload).

### L1: The IsExportDeclarationActive gate is dropped
- **Operation:** UpdateCetrificateOfOrigins dispatcher
- **Severity:** LOW
- **Class:** dropped guard
- **Legacy:** ESP:98. **New:** NBL:1585-1588.
- **Detail:** the gate is dropped on the grounds that the parameter is always true.
- **Marked TODO:** yes, TODO(confirm). **Verdict:** CONFIRMED (documented).

### L2: Legacy exceptions accumulated across certificates; the new code evaluates each certificate separately
- **Operation:** reconcile
- **Severity:** LOW
- **Class:** changed behaviour (a silent fix)
- **Legacy:** LBL:54, 71, 513-514, 533. `_requestExceptions` is a BL instance field that is never reset per item. So in a multi-certificate call, certificate N inherits the errors of certificates 1..N-1: it is Rejected, gets duplicate VsDeclarationError rows, and its AdditionalInfo is inflated.
- **New:** NBL:1664-1666 evaluates each certificate on its own.
- **Recommendation:** keep the new behaviour, but document it.
- **Marked TODO:** no. **Verdict:** CONFIRMED.

### L3: Destination-group check edge cases differ
- **Operation:** reconcile
- **Severity:** LOW
- **Class:** changed edge-case behaviour
- **Legacy:** LBL:740-751. When the group value is non-numeric, `groupOfDestinationCountry` stays a new instance, so the legacy adds an exception. A null `DestinationCoutryID` builds the predicate `"CountryID ==  && …"`, which probably throws.
- **New:** NBL:2071-2083 skips the check when the group does not parse, and flags a discrepancy when the destination country is null. The null case is explained in a code comment.
- **Marked TODO:** no. **Verdict:** PLAUSIBLE.

### L4: The warnings-task assignment no longer carries the profession or org-unit type
- **Operation:** reconcile warnings
- **Severity:** LOW
- **Class:** dropped filter
- **Legacy:** LBL:620-632 assigns with SingleUserAssignmentFilter including Profession Marech and OrgUnitType Export.
- **New:** NBL:1979-1981 assigns by user id only.
- **Marked TODO:** yes. **Verdict:** CONFIRMED (documented).

### L5: Exception texts are placeholders and ExceptionType is 0
- **Operation:** reconcile
- **Severity:** LOW
- **Class:** placeholder literals
- **Legacy:** LBL:538-562. **New:** NBL:2008-2009, 2369, 2375-2379.
- **Detail:** the texts are hard-coded Hebrew placeholders, and `ExceptionType = 0`.
- **Marked TODO:** yes. **Verdict:** CONFIRMED (documented).

### L6: The release publish does not send the request-feedback message
- **Operation:** DeclarationReleased
- **Severity:** LOW
- **Class:** dropped side effect
- **Legacy:** LBL:339 → 418 (SendRequestFeedback). **New:** NBL:1726-1728.
- **Marked TODO:** yes, TODO(confirm). **Verdict:** CONFIRMED (documented).

### L7: Cancellation branch — agent message deferred, reason text is a literal
- **Operation:** CancellationRequestCommited
- **Severity:** LOW
- **Class:** dropped side effect; literal replacing a server term
- **Legacy:** LBL:900-901, 907. **New:** NBL:1809-1820 and N `CertificateOfOriginsConsts.cs:68`.
- **Detail:** SendMessageToAgent is deferred, and the reason is the literal "ההצהרה בוטלה" instead of `EServerTerms.CanceledDeclaration`.
- **Marked TODO:** yes. **Verdict:** CONFIRMED (documented).

### L8: HandleCertificateReplacement — FeedbackRemark, SendMessageToAgent and the CertificateReplaced text are deferred
- **Operation:** DeclarationReleased
- **Severity:** LOW
- **Class:** dropped side effects
- **Legacy:** LBL:362-365. **New:** NBL:1524-1528.
- **Marked TODO:** yes. **Verdict:** CONFIRMED (documented).

### L9: CR 194221 — event 642 AdditionalInfo is now always the request-reason name
- **Operation:** RaiseCertificatePreferredAssessorEvent
- **Severity:** LOW
- **Class:** changed value source
- **Legacy:** LBL:1972-1993 set the "additional certificates on this declaration" message, and only when such certificates existed.
- **New:** NBL:1924-1929 always sets the Display name of the request reason. The comment says 599 is unreachable in the legacy; that was checked and matches the legacy RaiseEventUtil:122 gating described in the code.
- **Correctness:** `GetRequestReasonName` handles unmapped codes (returns empty), and every reachable reason has a Display name.
- **Marked TODO:** yes, TODO(blocking) at NBL:1896-1900. **Verdict:** CONFIRMED (documented, intentional CR).

### L10: Audit user falls back to 0 when there is no user header
- **Operation:** all reconcile writes
- **Severity:** LOW
- **Class:** user context
- **Legacy:** EF4 audit. **New:** NBL:1624, 1699, 1731, 1813.
- **Detail:** `RequestMetadata.UserId ?? 0` means UpdateUserId becomes 0 when the header is missing. There is no hardcoded `=1`.
- **Marked TODO:** no. **Verdict:** CONFIRMED.

### L11: GetGoodsItemCerificateDTO keeps the client-sent id when the certificate number is null
- **Operation:** GetGoodsItemCerificateDTO
- **Severity:** LOW
- **Class:** flag source of truth
- **Legacy:** ESP:138-142 always overwrote `certificateOfOriginID`, so a null number produced null (EF4 `= NULL` never matches).
- **New:** NBL:707-710 skips the item and keeps whatever id the client sent.
- **Marked TODO:** no. **Verdict:** CONFIRMED.

### L12: GetCertificateOfOriginID returns 404 where the legacy returned null
- **Operation:** GetCertificateOfOriginID
- **Severity:** LOW
- **Class:** changed HTTP/exception mapping
- **Legacy:** ESP:129-130 returned `int?` null. **New:** NBL:680-681 and CTRL:19-25.
- **Detail:** callers that expect null now get an exception. The query itself (order by CreateDate desc, no State filter) matches the legacy.
- **Marked TODO:** no, but it is a commented convention. **Verdict:** CONFIRMED.

### L13: Convert — 404 instead of InfException, and a new Customers dependency
- **Operation:** Convert
- **Severity:** LOW
- **Class:** changed HTTP/exception mapping; new dependency
- **Legacy:** ESP:43-58. **New:** NBL:643-645 → GetCertificateOfOriginsByFilter → FillCustomersInformation (NBL:754).
- **Detail:** not-found raises 404 instead of `InfException ConversionOfEntityFaildEntityNotExist`. The new chain also calls the Customers proxy, so a Customers outage now fails Convert. The Title (`S.Name` from the SP) and CustomerId mapping match the legacy.
- **Marked TODO:** no. **Verdict:** CONFIRMED.

### L14: SaveCertificateOfOriginAttachments — documented implementation changes
- **Operation:** SaveCertificateOfOriginAttachments
- **Severity:** LOW
- **Class:** changed value sources
- **Legacy:** LBL:1275-1316 and 1253-1262. **New:** NBL:795-898.
- **Changes:**
  - The org unit comes from the Users proxy, falling back to 0; the legacy used `UserUtil.Current`.
  - The type name comes from the enum Display name instead of SystemTables. The values were checked and match, including "Korea".
  - The filename is sanitized.
  - The call is synchronous; the legacy queued a `WcfInvoker SaveCertificateAttachmentsAction`.
  - Delete-inside-loop is preserved bug-for-bug.
  - Title, draft sentinel, doc-type 329 and additional field 46 constants match the legacy.
- **Marked TODO:** developer decisions, commented. **Verdict:** CONFIRMED.

### L15: GenerateTemplate (CR 194221) has no legacy data source to compare against
- **Operation:** GenerateTemplate
- **Severity:** LOW
- **Class:** notes only
- **Legacy:** no Korea letter data source exists. The only "SouthKorea" hits are the certificate-type templates 2391/2392 (LBL:1201-1204; `usp_Template_INNER_CROSS_CertificateOfOrigin.sql:648`). The closest analogue is `Infrastructure.usp_Template_INNER_CROSS_ImportRequestForVerificationEUR`, which takes the file, aggregates **all** non-null DocumentNumbers with no type split, and also returns Reference/UserID/OwnerEMail/RequestDate.
- **New:** NTPL:24-80 and `Scripts/API_20260923104500 - dbo.GetTemplateData.sql`.
- **Checked and correct:**
  - camelCase JSON matches `$.fileNo` etc.
  - An unknown id or a missing row gives 404.
  - `F.State <> 99` is a sound addition. The child request table has no State column, so there is no missing filter.
  - Blank numbers are skipped.
- **Minor points:**
  - LetterDate is serialized as a DateTime with `T00:00:00`, so the YAML/docx has to format it.
  - The controller hardcodes `application/pdf` whatever the Format is. That is fine today because only Pdf is registered.
  - Template id 1 is TODO(confirm).
- **Marked TODO:** yes (the id). **Verdict:** PLAUSIBLE (no legacy to prove against).

### Grep-category sweep (in scope)
- **PushUtil:** none in the legacy chain.
- **Hardcoded `=1` user:** none (see L10 for the `?? 0` fallback).
- **Mock proxies:** used only in x-mock-mode (ServicesConfiguration.cs:35-93). The real CommonServicesProxy route is TODO(blocking).
- **TODO(confirm):** L1, L6, L15.
- **"out of scope" / "heavy Save()":** none in scope.

### Already documented in MIGRATION-NOT-DONE.md (checked last)
- **H1, H2, H3, M1, M2, M3, M4, M5, M6, M7:** **no**.
  - The replacement flow is mentioned only generically (line 32).
  - IsCreateAttachments/IsMessageSent guards are mentioned (line 169), but not the missing CertificateIdToCancel projection.
- **L1, L4-L8, L9, L15:** yes, as code TODOs. The "resx texts" and SSRS-only decision are at lines 175-181.
- **L2, L3, L10-L14:** no.
