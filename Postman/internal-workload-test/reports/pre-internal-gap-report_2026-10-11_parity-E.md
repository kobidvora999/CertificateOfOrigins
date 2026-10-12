# Parity re-check E — Internal contract, authentication-request READ operations (2026-10-11)

Scope: GetAuthenticationRequestByFilter, GetEntityDocuments, GetAuthenticationRequestFileByID, GetAuthenticationRequestByID, GetAuthenticationRequestByLeadDocumentIDs, CheckIfExistsAdditionalRequestsForImporter, CheckIfExistsAdditionalRequestsForVendor, GetPathsForNavigationToVendor, CheckImporterOfImportAuthentication. Read-only.

Files read
- New: `API/CertificateOfOrigins.BL/AuthenticationRequestBl.cs` (1-352, 641-862, 1500-1533); `API/CertificateOfOrigins.DAL/CertificateOfOriginsDal.cs` (435-704, 706-770, 941-947, 1073-1079, 1188-1208, 1354-1378); `API/CertificateOfOrigins.Model/DbContext/CertificateOfOriginsDbContextExtension.cs` (25-113); DTOs (GetImportAuthenticationRequestResultDto, GetAuthenticationRequestByLeadDocumentResultDto, ImportAuthenticationRequestFilterDto, GetAuthenticationRequestFileByIdResultDto, AuthenticationFileRequestDto, DocumentDto); `WebApi/Controllers/Ui/AuthenticationRequestController.cs`; proxies Documents/Customer/Vendor; Scripts: `API_20261008110055 …ByFilter date bounds.sql` (current version), `API_20260726165855 …ByLeadDocumentID.sql`, `API_20260722 …ForVendor.sql`, `API_20260722b …ForImporter.sql`, `API_20260716 - add params.sql`.
- Legacy: `Server/…BL/AuthenticationRequestBL.cs` (63-132, 207-284, 352-427, 475-497, 621-676, 876-922, 1295-1299); `CertificateOfOriginsBL.cs` 1386-1426; `Service/CertificateOfOriginsInternalServicePartial.cs` 93-286 (no exception wrapper on any of the 9 ops — faults propagate); `InternalCommon/Common/AuthenticationRequestFileHelper.cs`; client `AuthenticationRequestSearchPresenter.cs`, `ImportProcessFormPresenter.cs:664`; legacy SPs `usp_CertificateOfOrigins_GetImportAuthenticationRequestByFilter`, `…GetAuthenticationRequestByLeadDocumentID`, `…CheckIfExistsAdditionalRequestsForImporter`, `…CheckIfExistsAdditionalRequestsForVendor`, `…GetImportAuthenticationFileDetailsAndRequests`, `…CertificateOfOrigins_GetImportAuthenticationRequestById`.
- Ledger rows E-F1..E-F17 and the 2026-09-27 appendix.

## SP column-by-column summary

| SP | Columns | Filters / TOP / order / dates | Verdict |
|---|---|---|---|
| dbo.GetImportAuthenticationRequestByFilter (current = API_20261008110055) | 17 vs 17, same aliases and order. Legacy C.Name / O.Title / DFLD.Title / vv.Title / Customer.Title → CAST(NULL); BL fills IssuingCountryID (Country lookup), OrganizationUnitID (OrgUnit lookup Name), VendorName (vendor.Title), ImporterName (customer.Title). LeadDocumentTitle stays NULL (E-F2). P.Name, AuthenticationFileStatusID (LEFT JOIN) kept. | All 18 params same; 16 dynamic predicates identical (=, LIKE on DocumentNumber); invoice CONTAINS→STRING_SPLIT LIKE (E-F9); TOP(200) (E-F1); ORDER BY CreateDate DESC; from = CAST(date), to = date+86399s = legacy ufn_General_GetDateEnd (A-25 fix present); cross-service INNER JOINs removed (E-F10). READ UNCOMMITTED / EXECUTE AS OWNER dropped (no behavior impact). | Parity apart from known items; EN-03 edge. |
| dbo.GetAuthenticationRequestByLeadDocumentID | 17 vs 17, same names/order. LeadDocumentTitle NULL (E-F2); ImportCountryName / OrganizationUnitName NULL then BL lookups (BL:830-838). IsCollateralExists identical expression. | Same TVP INNER JOIN, same local joins (PT INNER, IAFD LEFT, AFS LEFT, D INNER). DealFile/Country/OrgUnit INNER JOINs dropped (E-F10). No TOP/order in either. | Parity apart from known items. |
| dbo.CheckIfExistsAdditionalRequestsForVendor | single bit | Body identical (COUNT > 1, CreateDate >= now-3y, no State). | PARITY |
| dbo.CheckIfExistsAdditionalRequestsForImporter | single bit | Body identical except @DaysForLastDelivery source: `Infrastructure.General_enum_GlobalParam` → local `Infrastructure.Parameters` (seeded '10', add params.sql:266-281). @IsVendor still has no State filter, as legacy. Missing param → DATEADD NULL → 0 in both. | PARITY (seed value 10 not verifiable against prod from source). |
| GetAuthenticationRequestByID (legacy SP → new 2-pass LINQ, DAL:435-513) | Legacy RS1 37 cols. New returns 31 (+LeadDocumentSubmissionDate via DealFile proxy). Missing: CreateUserID, UpdateDate, UpdateUserID, ItemDetailID, IsOldIndication, OrganizationUnitTypeID (E-F7). Extra: LastDeliveryForImporter (not in legacy SP — EN-01). RS2 item lines identical (DAL:515). RS3 Docs_Document → Documents proxy GetDocumentById(documentId); legacy FileUrl=type name → new TypeName (client mapped FileUrl→TypeName at ImportProcessFormPresenter:664 — equivalent). | No State filter either side. | Parity apart from known items. |
| GetAuthenticationRequestFileByID (legacy SP → LINQ DAL:581-704) | RS1 header: new omits TimeStamp, CreateUserID, UpdateDate, UpdateUserID (EN-02). RS2 requests: 28 of 33 cols (E-F3 fix); still missing CreateUserID, ItemDetailID, ResponsePhoneNum, OrganizationUnitTypeID (recorded in ledger); IsSendReminderForImporterTaskExists via Tasks proxy (E-F5), LeadDocumentSubmissionDate via DealFile proxy. RS3 documents via Documents proxy per request. RS4 item lines identical (batched). CustomerID=-1 identical (legacy RS1 has no CustomerID → 0 → -1). | WHERE AuthenticationFileID = @FileID both; no ordering both. | Parity apart from known items. |

## Previous findings re-verification

| ID | Prior status | Verified now? | Evidence |
|---|---|---|---|
| E-F1 TOP(200) | CLOSED-OK | YES — still TOP (200) with comment citing prod value 200 | `Scripts/API_20261008110055 …date bounds.sql` l.42-43 |
| E-F2 LeadDocumentTitle NULL | OPEN-BLOCKED | YES — still CAST(NULL) in both SPs + TODO(migration) | ByFilter script l.49; LeadDoc script l.25; `AuthenticationRequestBl.cs:792`, `:840` (ledger cited 777/825 — line drift only) |
| E-F3 File drops child fields | FIXED | YES — all 8 (CirumstanceDetails, DecisionCircumstences, RequestCircumstancesId, Remarks, DocumentNumber, IsOldIndication, InvoiceGoodsItemTaxDifference, AllInvoiceGoodsItemTaxDifference) projected and mapped; DTO has the properties | `CertificateOfOriginsDal.cs:647-654`; `AuthenticationRequestBl.cs:290-297`; `AuthenticationFileRequestDto.cs:53-67` |
| E-F4 Documents payload shape | OPEN-BLOCKED | YES — assumption comment + route TODO(blocking) unchanged | `AuthenticationRequestBl.cs:692-697`; `Proxies/Documents/DocumentsProxy.cs:19` |
| E-F5 reminder-task status semantics | NOT-A-DEFECT-DOC | YES — TODO(confirm) present, filter IsTaskInProgress | `AuthenticationRequestBl.cs:335-338` |
| E-F6 GetByID 404 | CLOSED-OK | YES | `AuthenticationRequestBl.cs:33-34` |
| E-F7 six GetByID cols | CLOSED-OK | YES — DAL projection unchanged in that respect | `CertificateOfOriginsDal.cs:448-497` |
| E-F8 File 404 | CLOSED-OK | YES | `AuthenticationRequestBl.cs:167-168` |
| E-F9 CONTAINS→LIKE | CLOSED-OK | YES | date-bounds script, invoice predicate (STRING_SPLIT … LIKE) |
| E-F10 cross-service INNER JOINs dropped | CLOSED-OK | YES | both SP scripts — only local joins |
| E-F11 name sources | OPEN-BLOCKED | YES — ImporterName/VendorName now read `.Title` (matches legacy Customer.Title / vv.Title); Customers/Vendors routes still TODO(blocking) | `AuthenticationRequestBl.cs:756`, `:774`; `CustomerProxy.cs:17`; `VendorProxy.cs:16` |
| E-F12 Decision EndDate/IsAutomatic | FIXED | YES — DAL read + both BL maps carry EndDate and IsAutomatic | `CertificateOfOriginsDal.cs:543-544`; `AuthenticationRequestBl.cs:61-62`, `:184-185` |
| E-F13 IsVendor State guard | FIXED | YES — `State == 1` (legacy GetIdByCode active rows; legacy helper `AuthenticationRequestFileHelper.cs:24-28`) | `CertificateOfOriginsDal.cs:576-577` |
| E-F14 null config / unknown TypeId | CLOSED-OK | YES | `AuthenticationRequestBl.cs:653-657` |
| E-F15 empty LeadDocumentIDs | CLOSED-OK | YES — empty TVP sent; legacy null TVP; both give [] | `AuthenticationRequestBl.cs:804-819`; legacy `CertificateOfOriginsBL.cs:1409` |
| E-F16 [BindRequired] importerId | CLOSED-OK | YES | `AuthenticationRequestController.cs:78` |
| E-F17 empty ViewPaths | OPEN-BLOCKED | YES — TODO(blocking), PathId 359, ViewPaths [] | `AuthenticationRequestBl.cs:1505-1530`; MIGRATION-NOT-DONE.md:130-139 |

No regressions found.

## New findings

| ID | Severity | Operation | Deviation | New file:line | Legacy file:line | Why it matters |
|---|---|---|---|---|---|---|
| EN-01 | LOW | GetAuthenticationRequestByID | Returns `LastDeliveryForImporter`, which the legacy ById SP never selected (the legacy entity always came back with it null). The new save also writes it back, so the round-trip keeps the value. Legacy (full STE save of a null) would have cleared it. | `CertificateOfOriginsDal.cs:471`; `AuthenticationRequestBl.cs:140`; save `CertificateOfOriginsDal.cs:~745` (SetProperty LastDeliveryForImporter) | `usp_…GetImportAuthenticationRequestById.sql` RS1 (no LastDeliveryForImporter column) | A request with LastDeliveryForImporter = 2026-09-01 now shows that date on GetByID, where legacy showed nothing. Probably an improvement, but it is an undocumented behaviour difference. No consumer on the ImportProcessForm reads it, though the file screen binds it (`AuthenticationRequestFileGeneralView.xaml:385`, through the file read, which both versions return). |
| EN-02 | LOW | GetAuthenticationRequestFileByID | File header omits `TimeStamp`, `CreateUserID`, `UpdateDate`, `UpdateUserID`, which legacy result set 1 returned. | `CertificateOfOriginsDal.cs:584-603`; `GetAuthenticationRequestFileByIdResultDto.cs` | `usp_…GetImportAuthenticationFileDetailsAndRequests.sql` RS1 | No binding found in the legacy file screen (grep of AuthenticationRequestFile views/presenter is empty). TimeStamp was the STE concurrency token, but the new save is set-based and doesn't use it. Cosmetic / contract only. |
| EN-03 | LOW | GetAuthenticationRequestByFilter | An empty or whitespace `InvoiceNumber` ("" rather than null): legacy built `CONTAINS(R.InvoiceNumber, '')`, which raises an SQL full-text error (WCF fault). New: the STRING_SPLIT tokens all get filtered out, the EXISTS is false, and the result is silently `[]`. | date-bounds script, invoice predicate (`WHERE LTRIM(RTRIM(s.value)) <> ''`) | legacy ByFilter SP: `@InvoiceIDNumFTS = '''' + REPLACE(LTRIM(RTRIM(@InvoiceNumber)), ',', ' OR ') + ''''` and the CONTAINS predicate | Edge case: a client that sends "" for a cleared box gets an empty grid instead of an error. Neither version ignores the filter, so no wrong rows come back. |

Nothing new at HIGH or MED.

## Other checks (parity confirmed, no finding)
- GetEntityDocuments: same requested-ids query, same CSV type filter, same `d.Id != 0` guard only when requests exist, same claimed-by-other-lead exclusion, same Notes composition (`AuthenticationRequestBl.cs:645-700` vs legacy `AuthenticationRequestBL.cs:63-132`).
- CheckImporterOfImportAuthentication: AnyAsync gives prohibited → null, otherwise importerId. Same as legacy FirstOrDefault(...)?.ID == null. No State filter on either side (`CertificateOfOriginsDal.cs:1354-1359` vs L-BL:1295-1299).
- Task flags: GetByID task types {SetDecisionBeforeAssociation, SendReminderForImporter, HandleRejectedAuthenticationRequest} and the file types {6M, 10M, HandleAuthenticationRequestFile, SendReminderForImporter} match. IsCurrentUserHasOpenTask uses IsTaskInProgress as legacy. AdditionalRequestsForSearchInDays comes from parametersUtil.
- No soft-delete query filter or interceptor alters any of these reads (no HasQueryFilter in the repo). This matches the legacy SPs, which have no State filter.
- Legacy service wrappers have no try/catch for any of the 9 ops, so there is no in-band error response to reproduce.
