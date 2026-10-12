# Parity re-check H — export-document authentication requests (2026-10-11)

Scope: GetExportDocumentAuthenticationRequestSearch, GetExportDocumentAuthenticationRequestByID, SaveExportDocumentAuthenticationRequest (children, events, message, attach, TimeStamp, atomicity), GetCustomerInformation, GetCustomerInformationByCountry.

Files read:
- New: `API/CertificateOfOrigins.BL/ExportDocumentAuthenticationRequestBl.cs` (all 430 lines); `API/CertificateOfOrigins.DAL/CertificateOfOriginsDal.cs:660-669, 820-931, 949-1071, 1112-1164`; `API/CertificateOfOrigins.WebApi/Controllers/Ui/ExportDocumentAuthenticationRequestController.cs`; `BL/Validations/SaveExportDocumentAuthenticationRequestRequestValidator.cs`; `BL/Proxies/Customer/CustomerProxy.cs:13-45`; DTOs (Save request, ById result, Search filter/result, CustomerDto, EExportAuthenticationRequestStatus); tests `ExportRequestChildrenMergeTests.cs` (+ names of the TimeStamp/Status/RelatedEntity/Title tests); decompiled `CustomsCloud.InfrastructureCore.BL 1.10.97` BaseBL (AddEntity/UpdateEntity/SaveChangesAsync).
- Legacy: `Server/.../BL/ExportDocumentAuthenticationRequestBL.cs` (all 198 lines); `Server/.../Service/CertificateOfOriginsInternalServicePartial.cs:150-195`; `CertificateOfOriginsConsts.cs:68`; SP `usp_CertificateOfOrigins_CROSS_ExportDocumentAuthenticationRequestSearch.sql` (all); `Customs.CRM.Entities/.../ExportDocumentAuthenticationRequestSearchFilter.cs` + `...SearchResult.cs`; client `ExportDocumentAuthenticationRequestPresenter.cs:85-125, 420-475`.
- Ledger rows H-1..H-14; previous appendix `pre-internal-gap-report_2026-09-27_parity-H-export-docs.md`.

## Previous findings re-verification

| ID | Prior status | Verified now? | Evidence |
|---|---|---|---|
| H-1 | FIXED | YES | BL:67-77 fills State, OrganizationUnitId, CreateDate, CreateUserId, UpdateDate, UpdateUserId from `GetExportRequestProjectionColumns` (DAL:827-842); DTO ById:14-19 |
| H-2 | CLOSED-OK | YES (matches decision) | Validator: TimeStamp NotEmpty when Id>0 -> 400; stale TimeStamp still 409 via BaseBL.SaveChangesAsync (DbUpdateConcurrencyException -> RestConflictException) |
| H-3 | FIXED | YES | DAL:915-930 loads dropped children and `RemoveRange` (tracker, no ExecuteDelete); new parent bound by navigation (DAL:867,873,879). BL:193-194 single `SaveChangesAsync`. Decompiled BaseBL: AddEntity/UpdateEntity/SaveChangesAsync all go through `DataLayer` (IBaseDal.DbContext), the same context the DAL's `Context.Set<>` stages into -> one commit. Test ExportRequestChildrenMergeTests covers both branches |
| H-4 | FIXED | YES (OrganizationUnit still open, noted in code) | BL:279-289 `ToRelatedEntity` sets Id, Title, EntityType, CustomerId; used for attach (BL:208) and message (BL:200/361) |
| H-5 | NOT-A-DEFECT-DOC | YES | Events still after commit (BL:194 then 198-201) |
| H-6 | FIXED | YES | Validator `StatusId NotNull` -> 400 before any write (BL:145-151) |
| H-7 | CLOSED-OK | YES | DAL:961-962 IS NOT NULL guards + comment |
| H-8 | FIXED | YES | BL:413, 418 use `CustomerDto.Title`; CustomerDto.cs:12 has Title |
| H-9 | CLOSED-OK | YES | DAL:1004-1024 comma split + Contains, blank = no filter |
| H-10 | CLOSED-OK | YES | Filter criteria DAL:964-1039 match SP:79-115 one by one; enum INNER JOINs DAL:1043-1052; OUTER APPLY TOP 1 by Id DAL:1060-1063; ORDER BY Id DAL:1053 |
| H-11 | FIXED | YES | BL:371-380 -> DAL:660-669 reads `ExportAuthenticationRequestStatuses.Name` (no State filter, like SystemTables GetCodeById(...,true)) |
| H-12 | CLOSED-OK | YES | BuildEntity BL:225 takes `request.OrganizationUnitId` on insert (legacy client set it, Presenter:429) |
| H-13 | FIXED | YES | BL:343-348 AdditionalInfo not set when null; ExportNewAuthenticationRequest gets `id.ToString()` (BL:298) |
| H-14 | FIXED | YES (UIMessage text TODO(confirm) kept) | BL:35-36, 49-52 throw `RestValidationException` (400) with the two domain messages |

## Fresh statement-by-statement mapping (no deviation found)

- Save insert/update discriminator: legacy `IsNewInstance()` / new `Id == 0`; legacy double commit on insert collapses to one commit (no observable difference; id known before events in both).
- Status-change trigger: legacy `StatusID != OriginalStatusID` (L-BL:62), new `request.StatusId != request.OriginalStatusId` (BL:198) — same round-tripped flag; client sets OriginalStatusID on load (Presenter:467-470), new GetById sets `StatusId ?? 0` (BL:85) = L-BL:86.
- CheckStatus switch: case 5 -> update + ExportNewAuthenticationRequest(id) + message; 6/7/8 -> update + AfterClosing; 9/2 -> update + ChangeFileStatus; default -> update + message (BL:295-314 = L-BL:102-123). Status values 1..9 match the client's usage (WaitingForLetterSending=1 at Presenter:433, WaitingForExporter=2 at :99).
- Update event: type, entity type/id, Title = id, AdditionalInfo format string identical (BL:322-333 vs L-BL:130-137).
- Message: type 11102, params [id, status name], single destination = current user (BL:356-366 vs L-BL:152-166).
- Attach: only when the list is non-empty (BL:204 vs L-BL:70); null list is rejected by MVC implicit-required on a non-nullable `List<int>` rather than crashing.
- Update path restores State/OrganizationUnitId from the DB (BL:178-183); the legacy client never changes either on edit (only on new, Presenter:429-436), so equivalent.
- GetById: `.Single` -> 404 (BL:61-62); three children mapped column by column; ExportDeclarationIds = L-BL:93.
- Search: SP date bounds use raw `<= @RequestOpenDateTo` and the legacy filter passes the client value untouched (no date-end UDF in this SP) — DAL:989-992 matches; the SP's extra output columns `DocumentIssueDateFrom` / `RequestOpenDateFrom` (SP:56-57) are not in the legacy result class, so they were never on the wire.
- GetCustomerInformation: proxy maps 404 -> null -> 400 (CustomerProxy:29-31, BL:35-36) = L-BL:178-181. ByCountry: first of list, 400 on empty = L-BL:190-195.

## New findings

| ID | Severity | Operation | Deviation | New file:line | Legacy file:line | Why it matters |
|---|---|---|---|---|---|---|
| HN-01 | LOW | GetCustomerInformationByCountry | The proxy call uses validating `ExecuteAsync` (CustomerProxy:43), unlike `GetCustomerInformation` which tolerates 404 (CustomerProxy:29-30). If the (still unconfirmed, TODO(blocking)) Customers route answers "no customers for this country" with 404 instead of an empty list, the call becomes a proxy failure (5xx via ProxyFailureExceptionFilter) and the BL's `NoCustomHouseForThisCountry` 400 at BL:49-52 is never reached. | `BL/Proxies/Customer/CustomerProxy.cs:43`; `ExportDocumentAuthenticationRequestBl.cs:48-52` | `ExportDocumentAuthenticationRequestBL.cs:188-193` | Country with no foreign customs house -> 5xx instead of the legacy domain message. Contingent on the blocked route contract; settle together with the route TODO. |
| HN-02 | LOW | Customer endpoints / Save (documentation only) | Stale contract text after the fixes: the controller comments and attributes still say the two customer endpoints return 404 via `RestNotFoundException` (controller comments above `CustomerInformation` / `CustomerInformationByCountry`, `[NotFoundResponse]`), but the BL now throws 400 (H-14). Also BL:139-140 and the `EExportAuthenticationRequestStatus` header say the status name comes from the enum Display attribute (now from the table, H-11), and BL:175-177 / DAL:823-826 say GetById omits State/OrganizationUnitId (it returns them since H-1). | `WebApi/Controllers/Ui/ExportDocumentAuthenticationRequestController.cs` (comments over the two customer actions); `ExportDocumentAuthenticationRequestBl.cs:139-140, 175-177`; `CertificateOfOriginsDal.cs:823-826`; `Model/ModelDTOs/EExportAuthenticationRequestStatus.cs:6-9` | n/a | No runtime effect; the OpenAPI doc and the spec derived from comments would advertise 404 where the API returns 400. |

No HIGH or MED findings. All 14 previous items are verified, with no regressions.
