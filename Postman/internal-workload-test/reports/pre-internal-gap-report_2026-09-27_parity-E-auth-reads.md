# Parity review E — Internal contract, authentication-request READ operations

Reviewer: adversarial and read-only. Legacy = `C:\Repos\Main\CRM\CertificateOfOrigins\` plus the legacy SPs under `C:\Repos\Main\CustumsDev_Database\CustumsDev_Database\CRM\Stored Procedures\`. New = `C:\Repos\CertificateOfOrigins\API\`.
Abbreviations: L-BL = `Server\Customs.CRM.CertificateOfOrigins.BL\AuthenticationRequestBL.cs`; N-BL = `CertificateOfOrigins.BL\AuthenticationRequestBl.cs`; N-DAL = `CertificateOfOrigins.DAL\CertificateOfOriginsDal.cs`; N-CTL = `CertificateOfOrigins.WebApi\Controllers\Ui\AuthenticationRequestController.cs`.

## 1. Operation table

| Operation | Endpoint | DTOs (in → out) | BL | DAL / IDal | Verdict |
|---|---|---|---|---|---|
| GetAuthenticationRequestByFilter | `QUERY ui/AuthenticationRequest/ByFilter` (HttpQuery, FromBody) | ImportAuthenticationRequestFilterDto → List<GetImportAuthenticationRequestResultDto> | N-BL:671 (+FillAuthenticationRequestNames :703) | N-DAL:765 → `dbo.GetImportAuthenticationRequestByFilter` (script `API_20260907180000 …multi-invoice.sql`) | Deviations: F1 (MED), F2 (MED), F9, F10, F11 (LOW) |
| GetEntityDocuments | `GET ui/AuthenticationRequest/EntityDocuments/{leadDocumentId}` | int → List<DocumentDto> | N-BL:614 | N-DAL:1012, :1023 + IDocumentsProxy.GetDocumentsByEntity | Mostly parity. F4 (MED, PLAUSIBLE), F14 (LOW) |
| GetAuthenticationRequestByID | `GET ui/AuthenticationRequest/{documentId}` | int → GetAuthenticationRequestByIdResultDto | N-BL:27 (+MapToResultDto :115) | N-DAL:375, :455, :470, :490 + Collateral/Tasks/Documents/ExportDealFile proxies | Parity on the core fields. F6, F7, F12, F13 (LOW) |
| GetAuthenticationRequestFileByID (plus decision and file-status lookups) | `GET ui/AuthenticationRequest/File/{fileId}` | int → GetAuthenticationRequestFileByIdResultDto | N-BL:161, :258 | N-DAL:499, :526, :470, :569, :589 | **F3 (HIGH)**. F5, F8, F12 (LOW) |
| GetAuthenticationRequestByLeadDocumentIDs | `POST ui/AuthenticationRequest/ByLeadDocumentIDs` | List<int> → List<GetAuthenticationRequestByLeadDocumentResultDto> | N-BL:765, :791 | N-DAL:897 → `dbo.GetAuthenticationRequestByLeadDocumentID` | F2 (MED), F10, F15 (LOW) |
| CheckIfExistsAdditionalRequestsForImporter | `GET ui/AuthenticationRequest/CheckIfExistsAdditionalRequestsForImporter?importerId&vendorId&customerId&countryId` | 4 scalars → bool | N-BL:827 | N-DAL:1160 → `dbo.CheckIfExistsAdditionalRequestsForImporter` | SP matches the legacy SP. F16 (LOW) |
| CheckIfExistsAdditionalRequestsForVendor | `GET …/CheckIfExistsAdditionalRequestsForVendor?vendorId` | int → bool | N-BL:818 | N-DAL:1152 → `dbo.CheckIfExistsAdditionalRequestsForVendor` | PARITY (identical SP body: COUNT > 1, last 3 years, no State guard, same as legacy) |
| CheckImporterOfImportAuthentication | `GET …/CheckImporterOfImportAuthentication?importerId` | int → int? | N-BL:812 | N-DAL:1145 (LINQ `AnyAsync` on VerificationProhibitedImporters) | PARITY (not prohibited → importerId; prohibited → null; no State guard, same as legacy L-BL:1295) |
| GetPathsForNavigationToVendor | `GET …/PathsForNavigationToVendor` | none → NavigationToVendorViewDto | N-BL:1396 | none (stub) | Documented deferral: F17 (LOW) |

## 2. Findings

### F3 — GetAuthenticationRequestFileByID — HIGH — returned fields dropped (the file screen shows and edits them)
- Legacy: `usp_CertificateOfOrigins_GetImportAuthenticationFileDetailsAndRequests.sql` result set 2 (lines ~53-88) returns, for each child request, `CirumstanceDetails`, `DecisionCircumstences`, `RequestCircumstancesID`, `Remarks`, `ResponsePhoneNum`, `DocumentNumber`, `IsOldIndication`, `OrganizationUnitTypeID`, `ItemDetailID`, `CreateUserID`, `AllInvoiceGoodsItemTaxDifference` and `InvoiceGoodsItemTaxDifference`. The SP header says these tax-difference columns were added to this SP specifically (CR, 04/11/2021). L-BL:876 `MaterializeForGetFile` materializes them onto the full entity.
- New: N-DAL:526-567 `GetRequestsByFileId` projects only 20 columns. The comment at N-DAL:534-539 says the others are "deliberately NOT projected" and gives the 30-column interceptor as the reason. `AuthenticationFileRequestDto` has no properties for them either.
- Impact: the legacy file screen binds these fields on the selected request: `AuthenticationRequestFileGeneralView.xaml` (InvoiceGoodsItemTaxDifference ~l.225-250, AllInvoiceGoodsItemTaxDifference ~l.266-287, RequestCircumstancesID l.303, CirumstanceDetails l.312 [editable], DecisionCircumstences l.362 [editable, beside the decision dropdown]) and `FileRequestDocumentEditView.xaml` (DocumentNumber l.256, Remarks l.338, IsOldIndication l.348). In the new service all of them come back absent. The comment's own workaround (the second projection pass that N-DAL:375 already uses for GetAuthenticationRequestByID) was not applied here.
- Cross-note (write side, out of my scope): `SaveAuthenticationRequestFileChildDto` does not carry DecisionCircumstences or CirumstanceDetails either. The legacy full-entity save (L-BL `UpdateAndSaveImportAuthenticationRequest`) persisted them, so edits to these fields on the file screen are now lost too.
- Marked TODO: no. It is a deliberate comment, not a TODO. Verdict: **CONFIRMED**. Already documented in MIGRATION-NOT-DONE: no.

### F1 — GetAuthenticationRequestByFilter — MED — hardcoded row cap replaces a config function
- Legacy SP l.80: `SELECT TOP (shared.ufn_GetMaxRows())`. New SP `API_20260907180000…sql` l.49: `SELECT TOP (200)`.
- The platform max-rows value becomes the literal 200. The script header (`API_20260726130203`) documents it as a "generic row cap", but I found no evidence that 200 equals the legacy value. I could not locate the body of `ufn_GetMaxRows` in `C:\Repos\Main`. If the platform value is larger, searches are now silently truncated, sorted by CreateDate DESC, so older rows drop off.
- Marked TODO: no. Verdict: PLAUSIBLE (the legacy value is unverified). Already documented: no.

### F2 — ByFilter and ByLeadDocumentIDs — MED — `LeadDocumentTitle` is always NULL
- Legacy: ByFilter SP l.86 `DFLD.Title LeadDocumentTitle` (INNER JOIN CRP.DealFile_LeadDocument l.101). ByLeadDocumentID SP `dfld.Title LeadDocumentTitle`.
- New: `CAST(NULL AS NVARCHAR(255)) LeadDocumentTitle` (ByFilter SP l.55; LeadDoc SP l.25). There is no enrichment; N-BL:761 and N-BL:809 have `TODO(migration)`.
- Impact: this column is the navigation hyperlink in `AuthenticationRequestSearchView.xaml` l.244 and in the external `ImportAuthenticationRequestControlDictionaryView.xaml` l.49, which other modules consume. The TODO calls it unconvertible ("no proxy"), but the service already has a DealFile proxy (`IExportDealFileProxy`), so it is a pending wiring rather than a true blocker. That is why I rate it above the LOW used for deferrals.
- Marked TODO: yes. Verdict: CONFIRMED. Already documented in MIGRATION-NOT-DONE: no (no hit for LeadDocumentTitle).

### F4 — GetEntityDocuments — MED — the Documents-service payload is assumed to already be a DocumentDto
- Legacy L-BL:105-121 builds a DocumentDTO from the remote Document entity: `StringDynamicParams = entityDocument.Notes` and `OtherRelatedEntities` from `entityDocument.EntityDocument`.
- New: `DocumentsProxy.GetDocumentsByEntity` deserializes the response straight into `DocumentDto`. N-BL:659-660 assumes that "StringDynamicParams (raw notes) and OtherRelatedEntities are already populated by the proxy", and N-BL:663 overwrites `Notes`. The endpoint route is `TODO(blocking)`, so the remote shape is unconfirmed. If Documents returns an entity-shaped document (`notes`, `entityDocument`), `StringDynamicParams` comes back null, `OtherRelatedEntities` comes back empty, and the raw notes are overwritten. Nothing reports the loss.
- Marked TODO: the route only, not the field mapping. Verdict: PLAUSIBLE. Already documented: no.

### F5 — GetAuthenticationRequestFileByID — LOW — IsSendReminderForImporterTaskExists status semantics
- Legacy SP OUTER APPLY: `T.TaskStatusID != 2`. New N-BL:327-329: filters on `IsTaskInProgress` (status 1 or 4), so Canceled (3) and Suspended (5) now count as "not exists".
- Marked TODO(confirm): yes. CONFIRMED. Already documented: yes ("Pattern A").

### F6 — GetAuthenticationRequestByID — LOW — missing id returns 404 instead of the legacy exception
- Legacy L-BL:237-241 dereferences a null result (NRE, surfaced as a WCF fault). New N-BL:33-34 throws RestNotFoundException (404). This is a deliberate improvement (developer decision 2026-08-02 in the comment). The legacy also threw an NRE when the Docs_Document row was missing (L-BL:490); the new code returns `Document = null` instead. CONFIRMED. Documented in code.

### F7 — GetAuthenticationRequestByID — LOW — six legacy result-set columns not returned
- Legacy SP (`…_GetImportAuthenticationRequestById.sql`) result set 1 also returns `CreateUserID`, `UpdateDate`, `UpdateUserID`, `ItemDetailID`, `IsOldIndication` and `OrganizationUnitTypeID`. None of them is on the new DTO (N-DAL:375-452).
- Impact is low: the ImportProcessForm client computes IsOldIndication itself (ImportProcessFormPresenter:633), and it sets OrganizationUnitTypeID from the current user only when it creates a request. The one CreateUserID consumer (IsEnabledByUserConverter) is dead code. The save set-list does not write these columns, so a round-trip erases nothing. CONFIRMED. Already documented: no.

### F8 — GetAuthenticationRequestFileByID: a missing file returns 404 instead of null
- Legacy L-BL:621-641 returns `null` (a normal response). New N-BL:165-166 returns 404. Callers that probed for existence depended on the null. The one known caller (HandleAuthenticationRequestDeliverySent) was deliberately rerouted to a header read (N-BL:~340). LOW, CONFIRMED. Documented in code.

### F9 — GetAuthenticationRequestByFilter — LOW — invoice-number search: full-text CONTAINS becomes substring LIKE
- Legacy l.155: `CONTAINS(R.InvoiceNumber, '"a" OR "b"')`, a word match. New l.117: `EXISTS(STRING_SPLIT … LIKE '%term%')`. Substring matching widens results, e.g. `100` now matches `1001`. The comma split and OR were restored on 2026-09-07. The switch is deliberate and documented in the script header (no FTS catalog). CONFIRMED.

### F10 — ByFilter and ByLeadDocumentIDs — LOW — dropping cross-service INNER JOINs widens the row set
- Legacy ByFilter SP l.101-103 INNER JOINs `CRP.DealFile_LeadDocument`, `Shared.General_c_Country` and `Infrastructure.UserMng_OrganizationUnit`, so a request with a dangling reference was excluded. The legacy ByLeadDocumentID SP does the same (LeadDocument, Country, OrgUnit). The new SPs drop these joins, so such rows are now returned with null names. Low impact if referential integrity holds. CONFIRMED.

### F11 — ByFilter — LOW — name-source changes
- ImporterName: legacy `Customer.Title`, new `CustomerDto.Name` (the Customers route is TODO(blocking)). OrganizationUnit: legacy `O.Title`, new is the OrganizationUnit lookup `Name`. Equivalence depends on the proxies and lookups. PLAUSIBLE.

### F12 — Decision lookup (GetByID and File) — LOW — `EndDate` and `IsAutomatic` not returned
- Legacy: `GetQuery<CertificateOfOriginsDecision>().ToList()` returns every column. EDMX `CertificateOfOrigins_enum_Decision` has ID, Name, State, Description, EnglishName, Enumeration, StartDate, **EndDate**, **IsAutomatic**, IsForCoordinator and IsForClaliMakorWorker.
- New: `CertificateOfOriginsDecisionDto` and N-DAL:470 omit EndDate and IsAutomatic. IsForCoordinator and IsForClaliMakorWorker (CR 194221) **are** present in both reads. The file-status lookup is complete: IsAutomatic and EndDate are included.
- Consumer: AuthenticationRequestFilePresenter.cs:560 reads `Decisions…IsAutomatic`, but only to add the current DecisionID, which line 551 already adds unconditionally, so there is no functional loss in the legacy client. Nothing filters on EndDate. There is no State filter in either version. CONFIRMED, LOW. Already documented: no.

### F13 — GetByID — LOW — IsVendorByIssuingCountryId adds a State guard
- Legacy `AuthenticationRequestFileHelper.IsVendor` → `SystemTablesUtil.GetIdByCode<SupplierDeliveryCountryConfig>`. New N-DAL:490 adds `State != 99`. Equivalent unless inactive config rows exist. The legacy SP CheckIfExistsAdditionalRequestsForImporter has no State guard in either version. PLAUSIBLE.

### F14 — GetEntityDocuments — LOW — null config and unknown TypeId
- Legacy threw an NRE when `CertificateOfOriginsDocumentsFilter` was null (L-BL:71-73) or when a DocumentType id was unknown (L-BL:108 `.Name`). New treats a null config as an empty filter (N-BL:622), which returns [] silently, and gives a null TypeName. The core filter chain matches the legacy: already-requested exclusion, type filter, the `d.Id != 0` guard applied only when requests exist, the claimed-by-another-lead exclusion, and the Notes composition. CONFIRMED.

### F15 — ByLeadDocumentIDs — LOW — null or empty input
- Legacy L-CertificateOfOriginsBL:1386 `GetLeadDocumentIDsSqlParameters` returns null for an empty list, so the TVP value is null. New sends an empty table and returns []. `ImporterID` and `LastDeliveryForImporter` exist on the legacy DTO but never came from the SP (always null), and the new DTO drops them. CONFIRMED.

### F16 — CheckIfExistsAdditionalRequestsForImporter — LOW — binding is stricter than legacy
- Legacy passed the entity's `ImporterID` (nullable) and `IssuingCountryID`. A null ImporterID gave `R.ImporterID = NULL`, which returned false. New N-CTL uses `[BindRequired] int importerId` and `int countryId`, so a null importer now returns 400 instead of false. The SP body is identical to the legacy SP. `@DaysForLastDelivery` now reads `Infrastructure.Parameters` (seeded in `API_20260716 - add params.sql` l.269). CONFIRMED.

### F17 — GetPathsForNavigationToVendor — LOW (documented deferral) — always an empty ViewPaths
- Legacy L-BL:401-427 reads NavigationPath where PathID = 359. New N-BL:1396-1422 returns `PathId = 359, ViewPaths = []` (`TODO(blocking)`, also at ServicesConfiguration.cs:131). The legacy file screen calls it whenever VendorId != 0 (AuthenticationRequestFilePresenter:571). The endpoint returns 200 with empty data, which does not look like a failure to the caller. CONFIRMED. Already documented: yes (MIGRATION-NOT-DONE l.11, 132-139).

## Grep categories (in-scope files)
- PushUtil / `#error`: none. Hardcoded user `= 1`: none in the in-scope reads (current user = `RequestMetadata.UserId`).
- Mock proxies: every in-scope proxy (Customer, Vendor, Documents, Tasks, Collateral, ExportDealFile) has a real implementation plus a Mock implementation registered through `AddProxy<I,Real,Mock>`. Every real route is marked `TODO(blocking): confirm endpoint`. This is a rollout risk, not a parity deviation.
- TODO(confirm): N-BL:327 (F5). TODO(migration): N-BL:761, :809 (F2). "out of scope": none.
- Verified parity details: enum constants (ImportAuthenticationRequest = 12384, AuthenticationRequestFile = 12385, SendReminderForImporter = 404, the task-type lists for the request and file IsTaskExist calls). File `CustomerId = -1`. `AdditionalRequestsForSearchInDays` comes from parametersUtil. The ByFilter date-boundary math. All 18 filter parameters are present with the same `=` / `LIKE` operators. The DocumentNumber LIKE is unchanged. The Decision INNER JOIN in the ByLeadDocumentID SP is kept.
