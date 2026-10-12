# Parity re-check, cluster F: Internal authentication-request WRITE operations (2026-10-11)

**Scope:** SaveImportAuthenticationRequest, CreateNewAuthenticationFile, SaveAuthenticationRequestFile, with all their side effects: events, ChangeTempCollateral, grant/debit, tasks, file-status transitions, the CancelledFile unlink and CR 194221 (AdministrativeClosure = 10).

**Files read**
- New BL: `API\CertificateOfOrigins.BL\AuthenticationRequestBl.cs`, lines 27-156 (GetById round-trip), 372-477, 864-1483.
- New DAL: `API\CertificateOfOrigins.DAL\CertificateOfOriginsDal.cs`, lines 435-500, 706-821, 1261-1352.
- New DTOs:
  - `SaveImportAuthenticationRequestRequestDto`
  - `GetAuthenticationRequestByIdResultDto`
  - `SaveAuthenticationRequestFileRequestDto` / `SaveAuthenticationRequestFileChildDto`
  - `AuthenticationFileRequestDto`
  - `GetAuthenticationRequestFileByIdResultDto`
  - `VirtualEntityDto`
- New entities: the `CertificateOfOriginsImportAuthenticationRequest` and `...FileDetails` entities.
- New proxies: Collateral, Tasks and MessageManagement.
- Legacy BL: `Server\Customs.CRM.CertificateOfOrigins.BL\AuthenticationRequestBL.cs`, lines 134-205, 428-473, 530-697, 820-1264.
- Legacy entity partials: `Customs.CRM.Common\Customs.CRM.Entities\CertificateOfOriginsPartial\CertificateOfOriginsImportAuthenticationFileDetailsPartial.cs` and `CertificateOfOriginsImportAuthenticationRequest.cs`.
- STE model and template: `Customs.CRM.EF4Model\CertificateOfOriginsObjectModel.edmx` (association at 808-815, navigation properties at 1847 and 1925) and `CertificateOfOriginsSelfTrack.Types.tt` (FK-setter fixup, lines 313-355).
- Legacy client presenters: `AuthenticationRequestFilePresenter.cs` (lines 120, 124, 519, 527, 552) and `ImportProcessFormPresenter.cs`.
- Legacy SPs:
  - `usp_CertificateOfOrigins_UpdateImportAuthenticationRequest.sql`
  - `usp_CertificateOfOrigins_GetImportAuthenticationFileDetailsAndRequests.sql`
- Tracking docs: `MIGRATION-NOT-DONE.md`, and the LEDGER rows for F-01 to F-12.

## Previous findings re-verification

| ID | Prior status | Verified now? | Evidence |
|---|---|---|---|
| F-01 create path lost | FIXED | **YES** | BL:969-981 picks insert or update from `request.IsNewInstance`, not from "row not found". The update path is still a 404 when the row is missing (BL:976-980). DAL:769-821 `AddImportAuthenticationRequest` writes every DTO column plus Create/Update audit, `OrganizationUnitTypeId`, `RequestCircumstancesId ?? 1` and `Remarks ?? ""`. The DTO carries `IsNewInstance` and `OrganizationUnitTypeId` (DTO lines 15 and 91). The legacy entity has `IsNewInstance` (request partial:264). Residual risk is noted as FN-04. |
| F-02 child edits dropped | FIXED | **YES** (one residue, FN-03) | `SaveAuthenticationRequestFileChildDto` lines 74-96. DAL:1261-1293 writes Decision, the circumstances fields, Remarks, DocumentNumber, InvoiceNumber, Vendor, Customer, Importer, the countries, PreferenceDocType and IssuingDate. "" clears a string, which matches the legacy client writing `""` (FilePresenter:120 and 124). |
| F-03 file-status collateral = legacy dead code | CLOSED-OK | still matches the decision | BL:1231-1282. It is still called from ManageFileStatus (BL:1346). Legacy `CheckStatus` (L-BL:927) is still uncalled. |
| F-04 CR 194221 decision 10 holds vs file 10 releases | CLOSED-OK | still matches | BL:1234-1235 (file 10 grants). BL:1184 grants on Approval only. |
| F-05 file org unit from the client | FIXED | **YES**, with a caveat (FN-02) | BL:1329-1334 `FileOrganizationUnitId` is the first child's unit, falling back to the file's. It is used at BL:1355, 1400 and 1429. This matches the legacy getter (FileDetailsPartial:108-118). |
| F-06 file event Title | FIXED | **YES** | BL:1316-1319 equals the legacy partial (FileDetailsPartial:101-105). It is used at BL:1399 and 1428. The close-all and status-update events keep the bare id, as legacy does (L-BL:1043, 1123). |
| F-07 Needless rejection lacks org unit / customer | FIXED | **YES** | BL:1289-1308: org unit when > 0, customer when > 0. Legacy `IEntity.CustomerID` returned -1 when there was no customer (request partial:52-58). |
| F-08 ChangeTemp `true` flag / null RelatedEntity | OPEN-BLOCKED | marker present; the null half is fixed | BL:1085-1091 rejects a null RelatedEntity with a 400 before any side effect. `CollateralProxy.cs:28-32` keeps its TODO(blocking) for the flag. |
| F-09 Approval grant quirk (file id with request entity type) | CLOSED-OK | still matches | BL:1183-1194 = L-BL:1005-1015 |
| F-10 side effects before the 404 | FIXED | **YES** for the update and file paths | SaveImport: BL:882-885, an up-front 404 when `!IsNewInstance`. SaveFile: BL:1119-1122, an up-front 404. Per-step commits remain by design. The insert path has no pre-check (FN-04). |
| F-11 CreateNewFile minor diffs | FIXED | **YES**, and now fully verifiable | BL:405-410 rejects a null or <= 0 org unit before any event or write. The link SP source **is** in the SP folder, and `usp_CertificateOfOrigins_UpdateImportAuthenticationRequest` (`SET AuthenticationFileID ... WHERE DocumentID IN (...) AND AuthenticationFileID IS NULL`, no audit stamp) is exactly DAL:1348-1350. The "unverifiable" note can be closed as PARITY. |
| F-12 proxy routes unconfirmed | OPEN-BLOCKED | marker present | CollateralProxy.cs:18, TasksProxy.cs:18/31/39, MessageManagementProxy.cs:18 (TODO(blocking)) |

## New findings

| ID | Severity | Operation | Deviation | New file:line | Legacy file:line | Why it matters (concrete input → wrong output) |
|---|---|---|---|---|---|---|
| FN-01 | MED | SaveAuthenticationRequestFile, status → CancelledFile | The new code raises `AuthenticationRequestRejected` (opens a HandleRejectedAuthenticationRequest task, CloseOld) for every Rejection child of a **cancelled** file. Legacy raised none. In legacy, `CheckStatusAndOpenTask` sets `request.AuthenticationFileID = null` on each child, iterating backwards. The STE FK setter then nulls the `CertificateOfOriginsImportAuthenticationFileDetails` navigation, and its fixup removes the child from `file.CertificateOfOriginsImportAuthenticationRequest`; that is why the loop iterates backwards. So the Rejection loop that follows iterates an **empty** collection. The new code unlinks in the DB (DAL:1320-1331) but loops over the unchanged `request.Requests`. | BL:1406-1409 (unlink), then BL:1360-1371 (rejection loop) | L-BL:1095-1104 (backwards unlink), L-BL:1049-1062 (loop over the same collection); STE fixup: Types.tt:313-355; navigation: edmx:1925 | Cancel a file that holds a request with decision Rejection (4). Legacy opens no task. The new code opens a "handle rejected request" task on a request that is no longer in any file, and it is routed to a coordinator. |
| FN-02 | MED | SaveAuthenticationRequestFile, status change on a file with **no linked requests** | `FileOrganizationUnitId` falls back to `request.OrganizationUnitId`, which the file read (`GetAuthenticationRequestFileByIdResultDto`) never returns, so a GET→POST round-trip sends 0. The repo itself records that the .NET 10 builder throws `InvalidDataException` on `WithOrganizationUnitId(0)` (BL:402-404; MIGRATION-NOT-DONE "org-unit 0 באירועים": legacy tolerated 0, and other methods guard with `> 0`). These calls are unguarded. | BL:1329-1334, BL:1355, 1400, 1429 | L-BL:1038-1046 and 1086, raised with org unit 0 without error | Change the status of an empty file, for example an already-cancelled file whose requests were unlinked, to anything other than ClarificationRequired. The close-all-tasks event throws, and the save fails with a 500 before step 4, so the status is never persisted. Legacy saved it. (BL:1172/1366 child units and BL:996 RaiseNewRequestEvent are also unguarded, but the reads round-trip those values, so they hit only when a caller sends 0.) |
| FN-03 | LOW | SaveAuthenticationRequestFile, child VendorId / CustomerId / ImporterId | Null means "keep the stored value" (`child.X ?? r.X`), so a coordinator who **clears** the vendor or the foreign customs house on the file screen cannot persist the clear. Legacy `Repository.Save(request)` wrote the null. Strings can be cleared with "", but these nullable ints cannot be cleared at all. | DAL:1283-1285 | L-BL:918-925 | Clear the vendor of a child request on the file screen → the old VendorId is kept. Rare: the screen usually replaces the value rather than clearing it. |
| FN-04 | LOW | SaveImportAuthenticationRequest, insert path (`IsNewInstance = true`) | There is no up-front existence check on the insert path. A duplicate DocumentId fails only at `SaveChangesAsync` (409). By then ChangeTempCollateral has made the collaterals permanent, and NewAuthenticationRequest (task 406) or the other events have been raised. This is the same order as legacy (events before Save, L-BL:141-202), so it is parity, but it is exactly the orphan task / orphan collateral case flagged for this cluster, and the update path got a pre-check (F-10) while the insert path did not. | BL:882, 969-973 | L-BL:141-202 | Re-submit the "new request" popup (double click or retry) → a second NewAuthenticationRequest task and a collateral re-bind, then a 409. |
| FN-05 | LOW | SaveAuthenticationRequestFile, audit stamping | `UpdateFileChildRequest` stamps `UpdateDate`/`UpdateUserId` on **every** child on every save, and `UpdateAuthenticationFile` stamps the file even when nothing changed. In legacy, the children were materialized and then `AcceptChanges()`, so an untouched STE child was Unchanged and `Repository.Save` wrote nothing. The file SP does not even return the children's UpdateDate/UpdateUserID. | DAL:1272-1292, 1299-1316 | L-BL:885-887 (AcceptChanges), L-BL:918-925; SP `...GetImportAuthenticationFileDetailsAndRequests`, which has no AR.UpdateDate/UpdateUserID columns | A coordinator who changes one decision in a five-request file shows as "last updated by" on all five requests. This affects audit and reporting only. |

### Checked and at parity (fresh pass)
- **SaveImport:**
  - Collateral first → CollateralId (BL:888-892 = L-BL:137-144).
  - The decision switch: IsTaskExistsOnEntity open-only, IsTaskExist for any status, and the default case raises NewDecisionBeforeAssociation and sends the message.
  - VendorId 0 → null; AuthenticationNeedless rejection with the user assignment.
  - Update set-list covers the full GetById round-trip (read is two passes, DAL:448-500).
  - InvoiceNumber, DocumentNumber and ImporterId are written directly, so the legacy client's `= null` (ImportProcessFormPresenter:116/124/393) persists.
  - UpdateDate/UpdateUserId stamped on both paths.
- **SendDecisionMessage:** destinations and group logic, and the Rejection message type and parameters, match legacy. The AdministrativeClosure case (BL:1039) is a CR 194221 addition; legacy had no case for it and sent the message with no parameters.
- **CreateNewAuthenticationFile:**
  - Validation (any requested request already in a file → 400 FileExistForRequest).
  - Event order (NewDecisionBeforeAssociation per request → insert → link → NewAuthenticationRequestFile with the first request's unit).
  - Literals gg/ss/1/1 and status WaitingForSendingLetter.
  - Audit via AddEntity + AuditUserStamp.
  - The file table has no CustomerID or OrganizationUnitID column, so dropping legacy `CustomerID ?? 1` from the insert loses nothing; it is returned in the DTO only.
  - The link SP matches exactly.
- **SaveFile:**
  - Order: children → ManageRequestStatus → ManageFileStatus → file update, as legacy.
  - The `DecisionId == OriginalRequestDecisionId` skip; close-tasks except DemandAnotherClarification; per-child collateral list.
  - The status-changed guard; HandleImport list plus Partly; the vendor-reminder and final-decision events.
  - The status-update event (no org unit, as legacy) and the status message to `file.UserId`.
  - `Original*` snapshots are client-set in legacy too (FilePresenter:519, 552).
  - IsOldIndication `<= Now-3y`.
  - The CancelledFile unlink happens before the file update, and step 1 does not write AuthenticationFileId, so there is no relink.
- **Not reported (could not be verified from source):** legacy `new SendMessageDTO(entity, type)` probably filled the message's RelatedEntity Title, OrganizationUnit and Customer from the entity. The new code sends `VirtualEntityDto { Id, EntityType }` only (BL:1024-1028, 1455-1459). The constructor is in an infra binary.

**Counts:** previous findings: 7 FIXED, all verified YES (F-05 with the FN-02 caveat); 3 CLOSED-OK and 2 OPEN-BLOCKED unchanged. New findings: 0 HIGH, 2 MED, 3 LOW.
