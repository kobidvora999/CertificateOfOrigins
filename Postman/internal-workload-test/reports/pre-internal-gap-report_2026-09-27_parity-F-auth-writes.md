# F — Parity review: Internal authentication-request WRITE operations

Scope: SaveImportAuthenticationRequest, CreateNewAuthenticationFile, SaveAuthenticationRequestFile, plus their side effects and the CR 194221 changes (AdministrativeClosure = 10 for both the decision and the file status).
Legacy files: `C:\Repos\Main\CRM\CertificateOfOrigins\Server\Customs.CRM.CertificateOfOrigins.BL\AuthenticationRequestBL.cs` (called "L-BL" below), `...Service\CertificateOfOriginsInternalServicePartial.cs`, and the WPF client under `...\Client\Internal\...`.
New files: `C:\Repos\CertificateOfOrigins\API\CertificateOfOrigins.BL\AuthenticationRequestBl.cs` ("N-BL"), `...DAL\CertificateOfOriginsDal.cs` ("N-DAL"), and `...WebApi\Controllers\Ui\AuthenticationRequestController.cs`.

## 1. Operation table

| Operation | Endpoint | DTOs | BL | DAL / IDal | Verdict |
|---|---|---|---|---|---|
| SaveImportAuthenticationRequest | `POST ui/AuthenticationRequest/SaveImport` | `SaveImportAuthenticationRequestRequestDto` → `GetAuthenticationRequestByIdResultDto` | N-BL:842-942 (+ RaiseNewRequestEvent 946, SendDecisionMessage 966, ChangeTempCollateralRequest 1034) | `SaveImportAuthenticationRequest` (N-DAL:604): an UPDATE only, via ExecuteUpdate | **DEVIATES**: the legacy create path is lost (F-01). The update path is at parity apart from LOW items. |
| CreateNewAuthenticationFile | `POST ui/AuthenticationRequest/CreateNewFile` | `List<GetImportAuthenticationRequestResultDto>` → `CreateNewAuthenticationFileResultDto` | N-BL:369-457 | `GetFirstRequestAlreadyLinkedToFile` (1124), `LinkRequestsToAuthenticationFile` (1134), BaseBL `AddEntity` | **PARITY** (LOW only) |
| SaveAuthenticationRequestFile | `POST ui/AuthenticationRequest/SaveFile` | `SaveAuthenticationRequestFileRequestDto` / `SaveAuthenticationRequestFileChildDto` → `GetAuthenticationRequestFileByIdResultDto` | N-BL:1056-1374 (ManageRequestStatus, ApplyFileStatusCollateralOutcome, ManageFileStatus, CheckStatusAndOpenTask, RaiseFileStatusUpdateEvent, RaiseStatusMessage) | `UpdateImportRequestDecision` (1072), `UpdateAuthenticationFile` (1086), `UnlinkAllRequestsFromFile` (1111) | **DEVIATES**: child-request edits are dropped (F-02), the collateral side effect never ran in legacy (F-03), and the org unit now comes from the client (F-04). |

## 2. Findings

### F-01 — SaveImportAuthenticationRequest: the CREATE path for a new request is gone
- **Severity:** HIGH · **Class:** dropped side effect / replaced persistence (the legacy call inserted or updated; the new one only updates)
- **Legacy:** L-BL:201-202 `_uow.Repository.Save(importAuthenticationRequestResult)` saves a self-tracking entity, which inserts when the entity is new. The client creates the new entity in `ImportProcessFormPresenter.cs:265-291` (`OnShowAuthenticationRequest` → `InitNewImportProcess`, lines 322-358: `IsNewInstance = true`, `DecisionID = 1`, `RequestCircumstancesID = 1`, `OrganizationUnitTypeID`, `ResponsePhoneNum`, ...). It then saves through the same `SaveImportAuthenticationRequest` (presenter line 162).
- **New:** N-BL:931-937 calls `DataLayer.SaveImportAuthenticationRequest`. That is `ExecuteUpdateAsync ... Where(DocumentId == ...)` (N-DAL:604-668). When there is no row it returns false and the BL throws `RestNotFoundException`. No INSERT path for `CertificateOfOriginsImportAuthenticationRequest` exists anywhere in the BL or DAL (a grep for AddEntity or new-entity inserts finds only the file, the certificate and the export request). The DTO also has no `OrganizationUnitTypeId`.
- **Why it matters:** In legacy, opening a new request from the declaration screen (decision 1) raised `NewAuthenticationRequest` (opens task 406) and inserted the row. The new code does the following before returning 404:
  - calls `ChangeTempCollateralRequest`, which makes the collaterals permanent and binds them to a DocumentId that has no row;
  - checks `IsTaskExistsOnEntity`;
  - raises `NewAuthenticationRequest`.
  The result is an orphan task and orphan collaterals, and no request is ever created.
- **Marked TODO:** no. The class comment at N-BL:838-839 says "The persist is a set-based update on the existing row ... Missing row → 404".
- **Verdict:** CONFIRMED (both sources re-read). **Already documented:** no.

### F-02 — SaveAuthenticationRequestFile: each child request's edits other than DecisionId are silently discarded
- **Severity:** HIGH · **Class:** dropped side effect / partial write (same defect class that CHECK 2 fixed for SaveImport, but never applied here)
- **Legacy:** L-BL:918-925 `UpdateAndSaveImportAuthenticationRequest` runs `_uow.Repository.Save(request)` on every child request, a full self-tracking-entity write. The file screen edits several child fields two-way on `SelectedRequest`:
  - in `AuthenticationRequestFileGeneralView.xaml`: `DecisionCircumstences` (362), `CirumstanceDetails` (310) and `DecisionID` (342);
  - in `FileRequestDocumentEditView.xaml`: `VendorId` (267), `ImportCountryID`, `CustomerID` (the foreign customs house, 315), `Remarks` (338) and `DocumentNumber`.
  `DecisionCircumstences` is mandatory whenever the decision changes (entity `ValidateField`, CertificateOfOriginsImportAuthenticationRequest.cs:392-405).
- **New:** `SaveAuthenticationRequestFileChildDto` carries only DocumentId, AuthenticationFileId, DecisionId, OriginalRequestDecisionId, DocumentIssuingDate, OrganizationUnitId, UserId and UserResponseId. `UpdateImportRequestDecision` (N-DAL:1072-1084) writes only `DecisionId`, `IsOldIndication` and the update audit fields.
- **Why it matters:** The justification a worker types for a decision change on the file screen is lost, and so are vendor, customs-house (CustomerID) and remarks edits. CustomerID then feeds the letter and reminder delivery that the client sends to the customs house.
- **Marked TODO:** no.
- **Verdict:** CONFIRMED. **Already documented:** no. MIGRATION-NOT-DONE records the CHECK 2 fix for SaveImport only.

### F-03 — The file-status collateral outcome (grant / debit) ports legacy DEAD code; CR 194221 builds on it
- **Severity:** HIGH · **Class:** added side effect (an unexpected financial effect in the Collateral service)
- **Legacy:** The collateral switch is in `CheckStatus` (L-BL:927-977): Grant on RightAuthenticationAnswer, `DebitCreditCollateralRequest` per collateral on WrongAuthenticationAnswer. Nothing calls `CheckStatus`: the only `CheckStatus(` call in `C:\Repos\Main\CRM` is `ExportDocumentAuthenticationRequestBL.cs:64`, which calls that class's own private method. `SaveAuthenticationRequestFile` (L-BL:685-697) calls only `UpdateAndSave...`, `ManageImportAuthenticationRequestStatus` and `ManageImportAuthenticationFileStatus`, and the last has no collateral code (L-BL:1025-1070).
- **New:** N-BL:1169-1220 `ApplyFileStatusCollateralOutcome` is called from `ManageFileStatus` (N-BL:1232):
  - `GrantAllCollateralRequests(EntityId = file.Id, EntityTypeId = AuthenticationRequestFile)` on RightAuthenticationAnswer **or AdministrativeClosure**;
  - `DebitCreditCollateralRequest` for each collateral id on WrongAuthenticationAnswer.

  The code comment at N-BL:1162 ("Legacy ManageImportAuthenticationFileStatus, the collateral half of its switch") is factually wrong, because that method has no switch.
- **Why it matters:**
  - Production never debited or collected guarantees on a wrong answer and never granted them at file level. The new code does both.
  - CR 194221 extends the grant to status 10 on the premise that it "mirrors RightAuthenticationAnswer at ... the collateral grant". That site is not live legacy behaviour.
  - The grant is sent with the FILE entity type, while collaterals are bound to ImportAuthenticationRequest entities (ChangeTempCollateralRequest, `RelatedEntityID = request`). The file-level grant may therefore match nothing (a silent no-op), while the Wrong arm debits concretely by collateral id.
- **Marked TODO:** no; the code treats it as a restoration.
- **Verdict:** CONFIRMED (dead code verified by grep). **Already documented:** yes. MIGRATION-NOT-DONE.md, section "⚠️ SaveAuthenticationRequestFile — שינוי התנהגות שדורש אישור מוצר", covers Right/Wrong but does not cover the CR extension to AdministrativeClosure.

### F-04 — CR 194221: the AdministrativeClosure decision does not release collaterals, but the AdministrativeClosure file status does
- **Severity:** MED · **Class:** internal inconsistency against the stated intent
- **Commit 7615be3:** the decision "does NOT grant the collaterals — that stays Approval-only". The file status "mirrors RightAuthenticationAnswer ... the collateral grant".
- **In code, grant and release are the same call.** `GrantAllCollateralRequests` is the only release API. The enum comment (`EAuthenticationFileStatus.cs:17-18`, "collaterals are released") and N-BL:1171 ("releases the collaterals exactly like RightAuthenticationAnswer") both describe the call at N-BL:1201 as a release. So "release" in the file-status wording and "grant" in the decision wording are the same effect.
- **What actually happens:**
  - Setting a request's decision to 10 (N-BL:1121, where only Approval grants) leaves its collaterals held.
  - Setting the file status to 10 in the same save releases them for the whole file through the file arm.
  - A worker who closes every request administratively and then closes the file administratively therefore gets a full release anyway. That contradicts "stays Approval-only" at the request level.
  - The file-status half also rests on dead legacy code (F-03).
- **Also:** `openTaskStatuses` (N-BL:1279) opens a HandleImportAuthenticationRequest task for an administratively closed file. The CloseAllTaskForImportAuthenticationRequestFile event at N-BL:1235-1245 fires right after it, same as for status 6. That follows the stated intent, but it is odd for a closure and worth confirming.
- **Other switches, checked for a missing AdministrativeClosure case:**
  - SendDecisionMessage: included (N-BL:996).
  - SaveImport switch: falls to `default`, same as the peer group.
  - ManageRequestStatus: closes tasks, since it is not DemandAnotherClarification.
  - ManageFileStatus: the Rejection loop is not applicable.
  - AdvanceDeliveryStatus and the scheduler SPs use positive include-lists (2, 3, 8, 9), so status 10 is correctly excluded.
  - Result: no missing case found.
- **Marked TODO:** no.
- **Verdict:** CONFIRMED (the inconsistency is in the code and the commit text; whether it is acceptable is a product call). **Already documented:** no.

### F-05 — SaveAuthenticationRequestFile: the file OrganizationUnitId comes from the client, but the read endpoint never returns it
- **Severity:** MED · **Class:** change in which value is trusted (the server used to compute it; the client now sends it)
- **Legacy:** On the file entity, the `OrganizationUnitID` getter returns `CertificateOfOriginsImportAuthenticationRequest.First().OrganizationUnitID` whenever the file has requests (CertificateOfOriginsImportAuthenticationFileDetailsPartial.cs:108-118). The server therefore always used the first child's org unit for `CloseAllTaskForImportAuthenticationRequestFile` (L-BL:1038-1046) and `HandleImportAuthenticationRequest` (L-BL:1086, via `VirtualEntity(file)`).
- **New:** `request.OrganizationUnitId` is a transient field taken from the client (DTO comment). N-BL:1242, 1289 and 1319 use it. `GetAuthenticationRequestFileByIdResultDto` has no `OrganizationUnitId`, so the value cannot be round-tripped from the read. Unless the SPA derives it from `Requests[0]`, it arrives as 0, and the file tasks open or close with org unit 0 or none.
- **Marked TODO:** no.
- **Verdict:** PLAUSIBLE (it depends on the SPA). **Already documented:** partly. MIGRATION-NOT-DONE:122 notes the builder applies OrganizationUnitId only when it is > 0.

### F-06 — Legacy placed the file's Hebrew label in the event Title; the new code sends the bare id
- **Severity:** LOW · **Class:** a changed value sent to the Events/Tasks services
- **Legacy:** `new VirtualEntity(file)` for HandleImportAuthenticationRequest, UpdateFileStatusVendorReminderNotice and UpdateFileStatusFinalDecisionInCase (L-BL:1086, 1108, 1112) takes Title = `"  אימות מסמך מקור (יבוא) מספר פניה " + ID` (the partial, lines 103-106).
- **New:** N-BL:1288 and 1318 use `.WithTitle(request.Id.ToString())`. The sibling `CloseReminderTask` (N-BL:488) does replicate the Hebrew label, so the code is inconsistent with itself.
- **Verdict:** CONFIRMED. **Already documented:** no.

### F-07 — SaveImport AuthenticationNeedless rejection event no longer sets OrganizationUnitId
- **Severity:** LOW · **Class:** a value that is available but not sent to Tasks
- **Legacy:** L-BL:186, `EventUtilArguments(AuthenticationRequestRejected, importAuthenticationRequestResult)`: the entity supplies the request's OrganizationUnitID and CustomerID.
- **New:** N-BL:921-927 has no `WithOrganizationUnitId`, although `request.OrganizationUnitId` is available. The task is assigned to a single user, so the impact is on org-unit reporting and filtering only.
- **Verdict:** CONFIRMED. **Already documented:** no.

### F-08 — ChangeTempCollateralRequest drops the legacy `true` argument and hides a null RelatedEntity
- **Severity:** LOW · **Class:** a value that is not propagated / a guard replaced by a default
- **Legacy:** `CollateralServiceAdapter.cs:23` calls `ExternalProxy.ChangeTempCollateralRequest(list, true)`. `collateral.RelatedEntity.ID` throws an NRE when RelatedEntity is null (L-BL:469).
- **New:** `CollateralProxy.cs:25-31` posts only the list, and N-BL:1042 sends `RelatedEntity?.Id ?? 0`, which binds the collateral to entity 0 instead of failing. The meaning of the second legacy argument could not be determined from the repo (ICollateralExternalProxy is a binary). It does vary between callers, though: other modules pass `false` (CRP Evaluation `CollateralServiceAdapter.cs:24`, TempImportOfVehicle `CollateralServiceAdapter.cs:79`), while CertificateOfOrigins deliberately passes `true`. So it is a real per-caller behavioural switch, and the new payload cannot express it. The endpoint route is `TODO(blocking)`.
- **Verdict:** PLAUSIBLE. **Already documented:** endpoint only (the routes are TODO(blocking)).

### F-09 — Approval grant keeps the legacy entity mismatch (file id sent with the request entity type)
- **Severity:** LOW (a legacy bug preserved on purpose) · **Class:** operator / identifier
- **Legacy:** L-BL:1005-1015 sends `EntityID = authenticationRequestFile.ID` with `EntityTypeID = ImportAuthenticationRequest`.
- **New:** N-BL:1120-1131 does the same and comments it as a "Legacy quirk preserved".
- **Risk:** It grants the collaterals of whichever import request has DocumentId equal to the file id, not the approved request's collaterals.
- **Verdict:** CONFIRMED parity. **Already documented:** in the code comment only.

### F-10 — Side effects are committed step by step with no transaction; events fire before the 404
- **Severity:** LOW · **Class:** exceptions / atomicity
- **Legacy:** Row writes shared one UoW and were committed together at L-BL:692-693 (the CancelledFile branch also commits early, at L-BL:1103).
- **New:** Each ExecuteUpdate commits on its own. In SaveFile, the child decisions (step 1) are persisted before the `UpdateAuthenticationFile` 404 check (N-BL:1075-1079). In SaveImport, the events and the collateral change run before the 404 (N-BL:847-937). Legacy events were not transactional either, so the change is mostly the partial row persistence. There is also no TimeStamp concurrency check; legacy STE had an optimistic-concurrency TimeStamp.
- **Verdict:** CONFIRMED. **Already documented:** no.

### F-11 — CreateNewAuthenticationFile: minor differences
- **Severity:** LOW
- `OrganizationUnitIDNum.Value`, which threw an NRE in legacy (L-BL:564), became `?? 0` (N-BL:422).
- `InfException(FileExistForRequest)` became `RestValidationException`, which returns 400 (N-BL:380-385).
- `CustomerIDList` is dropped (a documented decision).
- Link SP → ExecuteUpdate that links only requests with `AuthenticationFileId == null` (N-DAL:1134-1142). The legacy SP source is not in the repo, so this could not be verified.
- Otherwise at parity: event order (NewDecisionBeforeAssociation per request, then NewAuthenticationRequestFile), the literals `"gg"`/`"ss"`/1/1, `CustomerId ?? 1` and status WaitingForSendingLetter.
- **Verdict:** CONFIRMED / documented design decisions.

### F-12 — Proxy routes are unconfirmed
- **Severity:** LOW (a documented deferral) · Collateral, Tasks and MessageManagement endpoint routes are all marked `TODO(blocking): confirm endpoint name/route` (CollateralProxy.cs:18-61, TasksProxy.cs:31, MessageManagementProxy.cs:18). Mocks are active only under the `x-mock-mode` header. **Already documented:** yes.

## 3. Checked and at parity (no finding)
- **SaveImport decision switch:**
  - NewAuthenticationRequest uses `IsTaskExistsOnEntity` (open-only) together with `!IsCurrentUserHandleRequest`.
  - AuthenticationRequried uses `IsTaskExist` (any status, same as legacy) and then raises ProcessedWithWasRejected and NewAuthenticationRequest.
  - The default case raises NewDecisionBeforeAssociation and calls SendDecisionMessage.
  - VendorId 0 becomes null; AuthenticationNeedless raises Rejected with the task assigned to UserResponseId.
  - `CollateralId` is taken from the first collateral.
- **SendDecisionMessage:** the destinations rule (UserResponseId, plus UserId when different), group vs single send, and the Rejection message type and parameters.
- **ManageRequestStatus:** the `DecisionId == OriginalRequestDecisionId` skip, the close-tasks exception for DemandAnotherClarification, the related file entity, and the DecisionUpdate event text.
- **ManageFileStatus:** the status-changed guard; `CloseAllTaskForImportAuthenticationRequestFile` unless the status is ClarificationRequired; the Rejection loop with CloseOld; the status event and message sent once.
- **CheckStatusAndOpenTask:** the status list plus any `Partly` decision; CancelledFile unlinks the requests.
- **Dropping the `AuthenticationFileStatusIDPrev` guard is equivalent:** the legacy client never sets that field (grep: its only assignment is its own setter), so in legacy it was always 0 and the guard was always true.
- `IsOldIndication <= Now-3y`; `SaveFile` returns the re-read `GetAuthenticationRequestFileByID`.
- **Grep categories:** no `PushUtil`. There is no hardcoded user or org unit = 1 in scope apart from the legacy `CustomerId ?? 1` literal. `TODO(confirm)` appears only in reads and schedulers outside this scope.
