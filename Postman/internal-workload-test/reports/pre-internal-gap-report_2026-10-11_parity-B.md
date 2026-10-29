# Parity re-check B — External contract (2026-10-11)

Reviewer: independent, adversarial, read-only.

## 1. Scope

**Operations reviewed:**
- UpdateCetrificateOfOrigins (`POST api/CertificateOfOrigins/Reconcile`), all 5 dispatcher branches:
  - submission reconcile
  - release (ExportDeclarationReleased / AssemblySharedReleaseAccepted)
  - amendment
  - cancellation
- GetCertificateOfOriginID (`GET api/CertificateOfOrigins/ID/{certificateNumber}`)
- GoodsItemCerificateDTO
- Convert
- SaveAttachments
- EventsResponse HandleDeliverySent
- Template (`GET api/CertificateOfOrigins/Template/{templateId}/{entityId}`, CR 194221)

**New (.NET 10) files read:**
- `API\CertificateOfOrigins.WebApi\Controllers\Api\CertificateOfOriginsController.cs`
- `API\CertificateOfOrigins.WebApi\Controllers\Api\EventsResponseController.cs`
- `API\CertificateOfOrigins.BL\CertificateOfOriginsBl.cs`, lines 695-970 and 1225-1275 (QR) and 1540-2671
- `API\CertificateOfOrigins.BL\CertificateOfOriginsBl.Templates.cs`
- `API\CertificateOfOrigins.BL\CertificateOfOriginsBl.MessageValidation.cs`, lines 868-891 (IsTradeAgreementForCountry)
- `API\CertificateOfOrigins.BL\CertificateOfOriginsBl.MessageInvoice.cs`, lines 136-165
- `API\CertificateOfOrigins.BL\EventsResponseBl.cs`
- `API\CertificateOfOrigins.BL\AuthenticationRequestBl.cs`, lines 347-370
- `API\CertificateOfOrigins.BL\CertificateOfOriginsConsts.cs`
- `API\CertificateOfOrigins.DAL\CertificateOfOriginsDal.cs`, lines 14-38, 103-116, 228-433, 581-606, 1381
- DTOs: UpdateCertificateOfOriginsRequestDto, ExportInvoiceInfoDto, ExportGoodsItemInfoDto, CertificateReconcileInvoiceDto, RaiseEventArgsDto, VirtualEntityDto, SouthKoreaOriginVerificationLetterResult, ECertificateOfOriginsTemplate
- `Scripts\API_20260923104500 - dbo.GetTemplateData.sql`
- `~\.claude\skills\_shared\template-print-pattern.md`

**Legacy files read:**
- `Service\CertificateOfOriginsExternalServicePartial.cs` (full)
- `BL\CertificateOfOriginsBL.cs`, lines 40-80, 300-920, 1020-1320, 1925-2051
- `BL\ServicesAdapter.cs`, lines 71-92
- `BL\AuthenticationRequestBL.cs`, line 621
- `InternalCommon\CertificateOfOriginsConsts.cs`
- `ExternalCommon\Common\UpdateCetificateOfOriginsDTO.cs`
- SP `usp_CertificateOfOrigins_GetImportAuthenticationFileDetailsAndRequests.sql`

**Prior state read:**
- the LEDGER (rows B-*)
- the previous appendix `pre-internal-gap-report_2026-09-27_parity-B-external-reconcile.md`
- MIGRATION-STATUS.md and MIGRATION-NOT-DONE.md (relevant lines)

**Line-reference shorthand:**
- `Bl` = `CertificateOfOriginsBl.cs`
- `DAL` = `CertificateOfOriginsDal.cs`
- `LBL` = legacy `CertificateOfOriginsBL.cs`
- `ESP` = legacy External service partial

## 2. Previous findings re-verification

| ID | Prior status | Verified now? | Evidence |
|---|---|---|---|
| B-H1 | FIXED | YES | Bl:1815-1824. The backfill is assigned onto `certificate.ExportDeclarationNumber` and `LeadDocumentId` before ApplyReconciliationOutcome, so the assessor lookup at Bl:2088 and Bl:2210 reads the backfilled value. This matches LBL:491-499. |
| B-H2 | CLOSED-OK | YES (matches decision) | Bl:2268-2277. No certificate invoice rows gives a finding-free match. The detail and import-replacement checks run only after that early return, as LBL:772/871 do. |
| B-H3 | FIXED | YES | DAL:228-262. The projection now includes CertificateIdToCancel, Title, CustomerId, QrCodePath, Guid, CreateCustomerId, InternalApplication and FeedbackRemark. Bl:1669-1691 cancels the replaced certificate. |
| B-M1 | FIXED | YES | Bl:2261-2264 returns `HasNoExportInvoices` with no exception. Bl:2019 sets hasErrors. No error row is written (Bl:1856) and AdditionalInfo stays empty (Bl:2030). This matches LBL:508-509. |
| B-M2 | FIXED | YES | Consts:73 equals the legacy InternalCommon consts:39 character for character. Used at Bl:2035. |
| B-M3 | FIXED | YES | Bl:1697-1714: `WithTitle(certificate.Title)` and `WithCustomerId`. The DAL projection loads Title and CustomerId. The import-replacement event keeps the fixed title and no customer (Bl:1718-1730), which matches LBL:586-592. |
| B-M4 | FIXED | YES | Bl:1605-1612: the ReportId comes from the type-code table and the call is skipped when there is none. This matches LBL:1135-1156. |
| B-M5 | FIXED | YES | QrCodePath and Guid are loaded, so the Bl:1232 guard now works. The queue gate reads ReportId at Bl:1580-1584. The payload fields come from the loaded entity (Bl:1635-1653). |
| B-M6 | FIXED | YES | DAL:103-116 (CancelCertificate) replaces RejectCancelReason and does not touch IsLastVersion. This matches LBL:364-365. |
| B-M7 | FIXED | YES | The DTO lists are nullable (UpdateCertificateOfOriginsRequestDto, ExportInvoiceInfoDto). The BL guards are at Bl:1794, 1872, 1907, 1994, 2261, 2164 and 2411. |
| B-L1 | CLOSED-OK | YES | The decision comment is at Bl:1759-1762, and the gate is still absent. |
| B-L2 | CLOSED-OK | YES | Bl:1836-1839 evaluates each certificate on its own. |
| B-L3 | CLOSED-OK | YES | Bl:2314-2329, with the decision comment. |
| B-L4 | OPEN-BLOCKED | YES | TODO(migration) at Bl:2223-2225. |
| B-L5 | OPEN-BLOCKED | YES | TODOs at Bl:2252-2253, 2626 and 2635. |
| B-L6 | OPEN-BLOCKED | YES | TODO(blocking) at Bl:1901-1904. |
| B-L7 | OPEN-BLOCKED | YES | TODO at Bl:1990-1991 and Consts:82. |
| B-L8 | OPEN-BLOCKED | YES | TODOs at Bl:1665-1668 and 1685. |
| B-L9 | FIXED | YES, with a caveat | Bl:2086-2173: the reason name, then ". " and message 6541 (Consts:78), with the 1 / 2 / ">2 לדוגמה" cases. These cases are identical to LBL:1970-1990. The DAL filter (DAL:24-38) has the same statuses as LBL:2012-2016. **Caveat:** the local legacy copy predates the CR update (it has no reason-name prefix and no `?.` null guard), so the reason-name part was verified only against the ledger's description. TODO(confirm live) is present at Bl:2141. |
| B-L10 | CLOSED-OK | YES | `RequestMetadata.UserId ?? 0` at Bl:1799, 1877, 1912 and 1999. |
| B-L11 | FIXED | YES | Bl:767-769: a null number gives a null id. |
| B-L12 | CLOSED-OK | YES | Bl:739-740 throws a 404. The query is the same: DAL:14-22 (order by CreateDate desc). |
| B-L13 | CLOSED-OK | YES | Bl:698-714. The Title and CustomerId mapping matches ESP:60-66. |
| B-L14 | CLOSED-OK | YES | Bl:854-909, unchanged. |
| B-L15 | NOT-A-DEFECT-DOC | YES | TODO(confirm) is present at ECertificateOfOriginsTemplate.cs:11-13. **Change since the last review:** an unregistered template id now returns 400 (RestValidationException, Templates.cs:101). See BN-03. |

## 3. New findings

| ID | Severity | Operation | Deviation | New file:line | Legacy file:line | Why it matters |
|---|---|---|---|---|---|---|
| BN-01 | HIGH | UpdateCetrificateOfOrigins: submission reconcile, and amendment (which reconciles the same way) | **Dropped per-goods-item checks.** The legacy loops over *every* goods-item row of a matched invoice. The new code loops over `invoice.CustomsItemIds`, and the DAL builds that list with `item.CustomsItemId.HasValue`, so goods items without a customs item are left out. When every goods item of a matched invoice has a NULL CustomsItemId, three Error checks never run: origin country, origin country group and certificate-linked-to-deal-file. | Bl:2474-2477; DAL:270-273 (`&& item.CustomsItemId.HasValue`) | LBL:801-834 (`if (!invoice.CertificateOfOriginItemDetail.IsNullOrEmpty() && ...)` then `foreach (var goodsItem in invoice.CertificateOfOriginItemDetail)`, with only the 6-digit check depending on the customs item) | CustomsItemId is NULL on a normal path: for a type with IsCustomsItemMandatory = false, a goods item sent without a classification is saved with no customs item (MessageInvoice.cs:141-148). Example: such a certificate whose origin country is absent from the declaration's goods items, or which is not linked to any declaration goods item. The legacy result is Rejected with event DeclarationMismatch. The new result is DeclarationMatch with event 642, and no error rows. **Fix:** carry an item count (or a nullable id list) and iterate goods-item rows, not customs-item ids. Apply the 6-digit check only to non-null ids. |
| BN-02 | LOW | EventsResponse HandleDeliverySent | **Dropped alternate key.** The legacy finds the file entity by `EntityType == AuthenticationRequestFile \|\| TypeID == (int)AuthenticationRequestFile`. The new code checks EntityType only, and VirtualEntityDto has no TypeId field. | AuthenticationRequestBl.cs:361-362; RaiseEventArgsDto.cs:8; VirtualEntityDto.cs | ESP:76-79 | A callback whose related entity carries only TypeID (EntityType left 0) returned true in the legacy when the file exists. The new code returns false. This is a plausible edge; it depends on the Events payload. |
| BN-03 | LOW | Template endpoint | **Stale contract comment.** The controller says an "Unregistered template id → 404". The BL now throws RestValidationException, which is a 400. Behaviour is deliberate (Templates.cs:98-101) and the pattern file allows a distinct error; only the controller comment and OpenAPI intent disagree. | CertificateOfOriginsController.cs:73-78 vs CertificateOfOriginsBl.Templates.cs:101 | (none — new CR) | Consumers and test writers who rely on the comment expect a 404. **Fix:** text/doc only. |

**Checked with no deviation found:**
- **Reconcile gate operators:** Received; reason not Empty / GetStatus / Cancellation; type not NonManipulation (LBL:501 vs Bl:1828-1830).
- **Draft handling:** the status and event are skipped, but the error rows are kept and the template is re-printed.
- **hasErrors / hasWarnings definitions.**
- **Event org unit and org-unit type.**
- **Assessor org-unit source:** the certificate for 642, the DTO for warnings.
- **AdditionalInfo cap:** 70 / 253 characters.
- **Destination, exporter and declaration-number checks:** null semantics match.
- **Import-replacement check:** `IsNullOrEmpty || !Exists`.
- **Forward and reverse 6-digit matching:** includes the early return on an unmatched invoice and the gating on IsCustomsItemMandatory.
- **Dedup by message key.**
- **Release path:**
  - backfill
  - PendingRelease → Published
  - QR condition and persistence order
  - CertificateIssued event
  - queue versus inline print, gated on ReportId
  - the replacement runs for any status
  - the stripped DTO is passed to the reconcile
- **Amendment:** backfill, then a reconcile with the full DTO.
- **Cancellation:** every certificate is cancelled and event ExportDeclarationConnectToCertificateOfOriginCanceled is raised.
- **GetCertificateOfOriginID / GoodsItem:** same query.
- **Convert:** same mapping.
- **Template endpoint:**
  - follows template-print-pattern (`WithJsonData`, not `WithData`; camelCase; RestNotFoundException on no row)
  - the SP's not-selected Employee / Signature columns are marked TODO(blocking) in the DTO, so they are not silent NULLs
- **Idempotency flags:** the release-path publish has no IsCreateAttachments guard. This is already covered by SaveCertificateOfOrigin TODO(blocking) #3 (missing columns). In practice it cannot fire twice on release, because a second release finds the certificate Published, not PendingRelease.
