# Parity review H: Export-document authentication request (5 operations)

Reviewer: adversarial and read-only. Legacy root: `C:\Repos\Main\CRM\CertificateOfOrigins\`. New root: `C:\Repos\CertificateOfOrigins\API\`.
Abbreviations: L-BL = `Server\Customs.CRM.CertificateOfOrigins.BL\ExportDocumentAuthenticationRequestBL.cs`; N-BL = `CertificateOfOrigins.BL\ExportDocumentAuthenticationRequestBl.cs`; N-DAL = `CertificateOfOrigins.DAL\CertificateOfOriginsDal.cs`; SP = `C:\Repos\Main\CustumsDev_Database\CustumsDev_Database\CRM\Stored Procedures\usp_CertificateOfOrigins_CROSS_ExportDocumentAuthenticationRequestSearch.sql`.

## 1. Operation table

| Operation | Endpoint | DTOs | BL | DAL / IDal | Verdict |
|---|---|---|---|---|---|
| GetExportDocumentAuthenticationRequestSearch | `QUERY ui/ExportDocumentAuthenticationRequest/Search` (body) | `ExportDocumentAuthenticationRequestSearchFilterDto` → `List<GetExportDocumentAuthenticationRequestSearchResultDto>` | N-BL:339-386 (DAL + Customers proxy + Country lookup) | N-DAL:773-895 (LINQ port of the SP) | PARITY, with LOW deviations (H-7, H-8, H-9, H-10) |
| GetExportDocumentAuthenticationRequestByID | `GET ui/ExportDocumentAuthenticationRequest/{id}` | → `GetExportDocumentAuthenticationRequestByIdResultDto` | N-BL:53-121 | N-DAL:936-988 (29-column projection) | DEVIATION: MEDIUM (H-1) |
| SaveExportDocumentAuthenticationRequest | `POST ui/ExportDocumentAuthenticationRequest` | `SaveExportDocumentAuthenticationRequestRequestDto` → ById result | N-BL:129-337 | N-DAL:670-755 (`GetExportRequestProjectionColumns`, `MergeExportDocumentAuthenticationRequestChildren`) | DEVIATIONS: MEDIUM (H-2, H-3, H-4), LOW (H-5, H-6, H-11, H-12, H-13) |
| GetCustomerInformation | `GET ui/ExportDocumentAuthenticationRequest/CustomerInformation/{customerId}` | → `CustomerDto` (Id, Name, ExternalIdNum, IsActive, Addresses) | N-BL:26-35 | `ICustomerProxy.GetCustomerInformation` (real proxy by default; the route is marked TODO(blocking)) | PARITY. The client uses only `Addresses` and `ID`, and both are carried (LOW H-14) |
| GetCustomerInformationByCountry | `GET ui/ExportDocumentAuthenticationRequest/CustomerInformationByCountry/{countryId}` | → `CustomerDto` | N-BL:37-51 | `ICustomerProxy.GetCustomersByCountry` (activity type 40; the route is marked TODO(blocking)) | PARITY (LOW H-14) |

Proxies: `ServicesConfiguration.cs:35,71,82` registers the Customer, Documents and MessageManagement proxies as REAL by default. The mock is used only when the `x-mock-mode` header is sent. Each real route carries `TODO(blocking): confirm endpoint`. None of the in-scope files contain PushUtil, a hardcoded `=1` user context, `TODO(confirm)` or "out of scope" markers.

## 2. Findings

### H-1 — GetById silently drops CreateDate, CreateUserId, UpdateDate, UpdateUserId, State and OrganizationUnitId
- Operation: GetExportDocumentAuthenticationRequestByID (and the Save response, which returns the same DTO)
- Severity: MEDIUM | Class: returned fields dropped
- Legacy: L-BL:85. It returns the full EF entity (all 35 EDMX columns, EDMX line 1994+).
- New: N-DAL:949-986 (29-column projection); `GetExportDocumentAuthenticationRequestByIdResultDto.cs:9-13`
- Description: The edit screen binds `CurrentEntity.CreateUserID` (the initiating user) and `CurrentEntity.CreateDate` as read-only fields (`Client\...View\ExportDocumentAuthenticationRequestEdit\ExportDocumentAuthenticationRequestEditView.xaml:277,284`). Neither field is in the new contract, so an SPA port of that screen has no data for them. The stated reason is the 30-column interceptor. However, the code already works around the same limit with a second narrow read (`GetExportRequestProjectionColumns`, N-DAL:670), so these fields could be returned the same way. They are not unconvertible.
- Marked-TODO: yes. The code says "TEMPORARY" / "NOTE (temporary)", not TODO.
- Verdict: CONFIRMED | Already documented: yes (MIGRATION-NOT-DONE §"30-column limit")

### H-2 — Optimistic concurrency added where the legacy had none (stale or missing TimeStamp now returns 409)
- Operation: Save (update path)
- Severity: MEDIUM | Class: guard added / exception contract changed
- Legacy: In the EDMX (`CertificateOfOriginsObjectModel.edmx:2002`, entity `ExportDocumentAuthenticationRequest` at 1994), `TimeStamp` has `StoreGeneratedPattern="Computed"` and no `ConcurrencyMode="Fixed"`. Compare line 1289, where another entity does have it. The legacy STE save (L-BL:66-67) was therefore last-writer-wins.
- New: The scaffold entity `ExportDocumentAuthenticationRequest.cs` has `[Timestamp] TimeStamp`. N-BL:201 sets `TimeStamp = request.TimeStamp!`, and `SaveExportDocumentAuthenticationRequestRequestDto.TimeStamp` is `byte[]?`.
- Description: EF now adds `WHERE TimeStamp = @orig` to the parent UPDATE. Two cases now fail where the legacy saved:
  - Concurrent edits: the second writer gets `DbUpdateConcurrencyException` (409).
  - Any update body that omits TimeStamp: it matches `TimeStamp IS NULL`, updates 0 rows, and gets 409.
- Marked-TODO: no. The DTO comment presents it as intended ("round-trips the row-version for optimistic concurrency").
- Verdict: CONFIRMED | Already documented: no

### H-3 — The child merge is not atomic with the parent and runs ExecuteDeleteAsync immediately
- Operation: Save
- Severity: MEDIUM | Class: transactional side effect changed
- Legacy: L-BL:66-67. On update, the single `Repository.Save(entity)` plus `CommitAllChanges()` persists the parent and all three child collections in one commit.
- New: The parent is committed by N-BL:167 `SaveChangesAsync`. Then N-DAL:739-741 `ExecuteDeleteAsync` runs straight against the DB (not staged, despite the comment at N-DAL:723). Adds and updates are committed later by N-BL:172.
- Description: Suppose the second `SaveChangesAsync` fails, for example a child `Update` whose Id no longer exists (0 rows gives a concurrency exception) or an FK/constraint error. Then:
  - the parent update is already committed;
  - the dropped children are already deleted;
  - the new and updated children are lost.

  The legacy could not end in that partial state. Whether an ambient transaction exists was not verified.
- Marked-TODO: no
- Verdict: PLAUSIBLE (the code path is confirmed; the impact depends on the absence of an ambient transaction) | Already documented: no

### H-4 — Document attach and status message send a stripped VirtualEntity (no Title, CustomerID, OrganizationUnitID)
- Operation: Save
- Severity: MEDIUM | Class: side-effect payload reduced
- Legacy: L-BL:76 builds `Entity = new VirtualEntity(entity)`. The constructor (`Malam.Infrastructure\...CommonBase\Base Entities\VirtualEntity.cs:20-33`) copies ID, Title, State, dates, users, TypeID, OrganizationUnitID, CustomerID and EntityType. The message at L-BL:154 is `new SendMessageDTO(file, …)` (the constructor source was not available; it likely wraps the same entity).
- New: N-BL:185 and N-BL:316 send `new VirtualEntityDto { Id, EntityType }` only. `VirtualEntityDto` has Title and CustomerId, but they are left empty.
- Description: The Documents service receives `CustomerId = 0` and `Title = null` for the attached-to entity. Document-to-customer association or visibility, and the message's related-entity title, may differ from the legacy. The same repo does pass `CustomerId` in a similar case (`CertificateOfOriginsBl.cs:1426`), which suggests the field matters downstream.
- Marked-TODO: no
- Verdict: PLAUSIBLE (the downstream effect depends on the external service) | Already documented: no

### H-5 — Order of events versus commit changed
- Operation: Save | Severity: LOW | Class: side-effect ordering
- Legacy: L-BL:62-67. On update, `CheckStatus` (events and message) runs before the final Save and Commit, so the events fire even if the commit then fails.
- New: N-BL:175-178 raises events only after both commits succeed.
- Description: This is arguably an improvement. Note it only.
- Verdict: CONFIRMED | Already documented: no

### H-6 — A null StatusId no longer throws after the save
- Operation: Save | Severity: LOW | Class: exception contract
- Legacy: L-BL:68 `entity.StatusID.Value` throws InvalidOperationException after the commit when StatusID is null.
- New: No equivalent. A null StatusId with `OriginalStatusId` 0 takes the default branch, raises the events and sends the message with an empty status name (N-BL:326-331).
- Verdict: CONFIRMED | Already documented: no

### H-7 — Row membership: the cross-service INNER JOINs became IS NOT NULL guards
- Operation: Search | Severity: LOW | Class: dropped conjunct (a documented design choice)
- Legacy: SP:62-64 has INNER JOINs to `Shared.General_c_Country`, `Customers_Customer CC` (CustomerID) and `CC1` (ExporterCustomerID).
- New: N-DAL:786 keeps only `CountryId != null && ExporterCustomerId != null`.
- Description: Rows whose country or customer id no longer resolves are now returned, with null names. The legacy excluded them. The DAL comment documents this.
- Verdict: CONFIRMED | Already documented: no (explained in a code comment)

### H-8 — Search names come from Customer.Name, while the legacy used Customers_Customer.Title
- Operation: Search | Severity: LOW | Class: output source column
- Legacy: SP:49 `CC.Title AS ForeignCustomsHouseName`; SP:54 `CC1.Title AS RequestIssuerName`.
- New: N-BL:370 and 375 use `CustomerDto.Name`.
- Description: The two differ if Title and Name differ in Customers (for example, a Title that includes an id). `CustomerDto` has no Title field.
- Verdict: PLAUSIBLE | Already documented: no

### H-9 — InvoiceIdNum: CONTAINS (full-text word match) became a substring LIKE; MainDocumentTitle LIKE wildcards are now escaped
- Operation: Search | Severity: LOW | Class: operator
- Legacy: SP:37 and 102 use `CONTAINS(EAR.InvoiceNumbers, 'a OR b')`; SP:105 builds `LIKE '%'+@MainDocumentTitle+'%'` by string concatenation.
- New: N-DAL:834-841 splits on commas and ORs `Contains` terms; N-DAL:847 uses `Contains`.
- Description: Full-text matches on word boundaries, so a substring can now match inside a longer number (`100` matches `1001`). User-typed `%` and `_` characters are escaped in the new code, while the legacy used them as wildcards. The comma split was correctly preserved.
- Verdict: CONFIRMED | Already documented: yes (per the DAL comment, REWIRE-PLAN accepted losing CONTAINS)

### H-10 — Filter field renames and the HTTP verb
- Operation: Search | Severity: LOW | Class: contract
- Description: Legacy `ForeignCustomsHouseCustomerID` became `ForeignCustomsHouseId`; `ExporterID` became `ExporterCustomerId`. The endpoint uses `[HttpQuery]` with `[FromBody]`, while the repo convention for a `Get…` method is GET with FromQuery. The filter semantics match the SP criterion by criterion:

  | Filter | SP line |
  |---|---|
  | CountryId | 79 |
  | DocumentTypeId | 82 |
  | RequestId | 85 |
  | ForeignCustomsHouseId | 88 |
  | CreateDate from / to | 91 / 94 |
  | DocumentId | 97 |
  | Invoice | 102 |
  | MainDocumentTitle | 105 |
  | ExporterCustomerId | 108 |
  | StatusId | 111 |
  | CreateUserId | 114 |

  `ExportDeclarationID` is not used in the SP either.

  The enum INNER JOINs (SP:65-66) are kept (N-DAL:867-876), and so are the OUTER APPLY TOP 1 ordered by ID (N-DAL:884-887) and ORDER BY ID.
- Verdict: CONFIRMED (parity) | Already documented: n/a

### H-11 — The status name comes from the enum [Display] attribute, while the legacy read the DB lookup table
- Operation: Save (event AdditionalInfo and message parameter) | Severity: LOW | Class: literal replacing config/lookup
- Legacy: L-BL:136 and 159 use `SystemTablesUtil.GetCodeById<ExportAuthenticationRequestStatus>(…).Name`.
- New: N-BL:326-337 uses the hard-coded Hebrew names in `EExportAuthenticationRequestStatus.cs`.
- Description: The local table `CRM.CertificateOfOrigins_enum_ExportAuthenticationRequestStatus` is mapped in this service (`ExportAuthenticationRequestStatus.cs`, already used by the search join). The sister flow reads names from the DB (`AuthenticationRequestBl.GetFileStatusName`). If the table rows are edited, the names drift. Also, for a null status the default branch sends an empty name, where the legacy looked up id 0.
- Verdict: CONFIRMED | Already documented: no

### H-12 — OrganizationUnitId on insert is taken from the client body with no server default
- Operation: Save (insert) | Severity: LOW | Class: audit field source
- Legacy: The WPF client set `OrganizationUnitID = CurrentUser.OrganizationUnitID` (Presenter:429-430). The server trusted the entity.
- New: N-BL:202 uses `request.OrganizationUnitId` (DTO default 0). No server-side fallback exists, although `CertificateOfOriginsBl.GetCurrentUserOrganizationUnitId` does exist.
- Description: Parity only if the SPA sends the org unit. Otherwise the row is written with 0. CreateUserId and UpdateUserId are now server-stamped (`AuditUserStamp`), which is fine.
- Verdict: PLAUSIBLE | Already documented: no

### H-13 — Specific-event AdditionalInfo is "" instead of null; DisplayName comes from RequestMetadata.Fullname
- Operation: Save | Severity: LOW | Class: payload literal
- Legacy: L-BL:147 sends `AdditionalInfo = null` for the AfterClosing and ChangeFileStatus events.
- New: N-BL:304 sends `additionalInfo ?? string.Empty`.
- Description: Status transitions are otherwise 1:1 with the legacy:
  - 5: FileStatusUpdate + ExportNewAuthenticationRequest(id) + message
  - 6, 7, 8: FileStatusUpdate + AfterClosing
  - 9, 2: FileStatusUpdate + ChangeFileStatus
  - default: FileStatusUpdate + message

  The flag source is also the same: the round-tripped `OriginalStatusId`, which the legacy also round-tripped as a `[DataMember]`, not via ChangeTracker. The message recipient is the current user in both (`MultipleMessageDestinations`, `IsGroupMessage` false in both). Verified against the legacy enums in `Malam.Infrastructure\...GeneralServices.Environment\Enums`: event types 1282, 1307, 1617 and 2047 (`EEventType.cs:5574,5699,7194,9339`), entity type 12386 (`EEntityType.cs:1642`) and activity type 40 (`ECustomerActivityType.cs:129`) all match. Message type 11102 and the status enum values 1–9 were not located in the legacy sources.
- Verdict: CONFIRMED | Already documented: no

### H-14 — The Customer endpoints return 404 without the legacy domain message
- Operation: GetCustomerInformation, GetCustomerInformationByCountry | Severity: LOW | Class: exception/HTTP code
- Legacy: L-BL:180 throws `InfException(EMessages.InvalidIdentificationNumber)`; L-BL:192 throws `InfException(EMessages.NoCustomHouseForThisCountry)`. The WPF client showed these messages to the user.
- New: N-BL:33 and 47 throw a bare `RestNotFoundException()`, so the specific user message ("no customs house for this country") is lost. Otherwise both endpoints match the legacy: the full address list (no address filter in legacy either) and first-of-list. `CustomerDto` carries `Id` and `Addresses` (AddressPurpose, AddressSingleLine), which are the only fields the legacy client consumed (Presenter:247-253, 270).
- Verdict: CONFIRMED | Already documented: no

### Checked and at parity (no finding)
- GetById: `.Single` becomes 404. The three child collections are covered column by column: all EDMX child columns are mapped. `OriginalStatusId = StatusId ?? 0` matches L-BL:86. `ExportDeclarationIds` equals the legacy `EntityTypeAndIDsToSearch[ExportDeclaration]` (L-BL:93).
- Save insert/update decision (`Id == 0` vs `IsNewInstance`), the post-save attach condition (non-empty list), and the returned graph (legacy returned the in-memory entity; new re-reads it, which is equivalent or better).
- On update, State and OrganizationUnitId are restored from the DB before the save (N-BL:156-161), preventing zeroing.
- The Search result keeps all legacy fields (RequestID, CountryName, ForeignCustomsHouseName, CustomerID, DocumentTypeName, ExportDeclarationTitle, RequestStatusName, RequestIssuerName, ExportLeadDocumentID). Name enrichment is null only when the proxy or lookup misses.
