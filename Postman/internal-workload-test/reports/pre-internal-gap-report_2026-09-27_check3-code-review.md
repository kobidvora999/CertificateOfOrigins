# CHECK 3 — Static code review: CertificateOfOrigins

Repo `C:\Repos\CertificateOfOrigins` @ `master` / `7615be3` (clean). Diagnose-only — no repo file was modified.
Reviewed against `net10-code-review` (principles + warnings policy), `~/.claude/skills/_patterns.md` → `_shared/{bl-rules,dal-rules,di-servicesconfiguration,proxy-pattern,template-print-pattern,ca1707-rule}.md`, repo `CLAUDE.md`, `.editorconfig`, and the EDMX `C:\Repos\Main\CRM\Customs.CRM.Common\Customs.CRM.EF4Model\CertificateOfOriginsObjectModel.edmx`.

## 1. Build result

Source: the orchestrator's `dotnet build CertificateOfOrigins.slnx --no-incremental` (`build-tail.txt`, `build-warnings.txt`). I did not run a build myself.

| Item | Value |
|---|---|
| Errors | **0** |
| Warnings (MSBuild summary) | **355** (355 unique lines after dedupe) |
| S1135 (TODO, the allowed exception) | 173 |
| REVIEW001 / 002 / 004 (C18 review-marker warnings from `Directory.Build.targets` → `tools/review-markers.ps1`) | 72 / 10 / 92 = **174** |
| **Other warnings** | **8**: S125 ×5, S3267 ×1, CS8601 ×2 |

### Classification of every non-S1135 warning

| file:line | code | Classification | Reason / fix |
|---|---|---|---|
| (174 rows) REVIEW001/002/004 | REVIEW00x | **justified by design, no suppression** | C18 raises every review marker (TODO(blocking) / TODO(confirm) / TODO, including in .sql/.yml/.yaml) as a build warning on purpose. They are the same items as the §4 inventory and go away only when the marker is resolved. 168 come from `.cs` files. The 6 non-`.cs` ones: `Scripts/API_20260728122720 - dbo.GetCertificateOfOriginDataForWebQuery.sql:9, :46` (REVIEW001), `Scripts/API_20260907101500 - dbo.GetImportAuthenticationRequestsForReminderForImporterScheduler.sql:23` (REVIEW002), `Postman/.../CertificateOfOrigins Internal Workload - API/.resources/definition.yaml:77` (REVIEW002), and both Planar `Jobs/Yaml/*.yml:13` schedules (REVIEW002). |
| API/CertificateOfOrigins.BL/CertificateOfOriginsBl.cs:250 | S125 | **fix-needed** | Legacy expression quoted in a comment (`Attachment = (reason == … ) ? null : CreateAttachments(certificate);`) **outside** `#region LEGACY_WCF`. Rewrite as prose (skill S125 recipe) or move into the LEGACY_WCF region with its pragma. |
| API/CertificateOfOrigins.BL/CertificateOfOriginsBl.cs:1726 | S125 | **fix-needed** | Prose comment above `DeclarationReleased` (the TODO(confirm) block) that the analyzer reads as code. Rephrase: no trailing `;`, no `(A — B)` code shapes. |
| API/CertificateOfOrigins.BL/CertificateOfOriginsBl.cs:1765 | S125 | **fix-needed** | The comment "CreateQrCodeIfNeeded stamped Guid + QrImage … ;" is prose that ends in code punctuation. Rephrase. |
| API/CertificateOfOrigins.BL/ServicesConfiguration.cs:111 | S125 | **fix-needed**, **new in 7615be3** | "(CertificateOfOriginsBl.Templates.cs);" ends with `;`. Rephrase, e.g. "see CertificateOfOriginsBl.Templates.cs / ITemplateUtil". |
| API/CertificateOfOrigins.Model/ModelDTOs/EEventType.cs:96 | S125 | **fix-needed** | The section banner `// --- Reminder schedulers (the two C17 Planar jobs) ---` together with the following lines is read as code. Rephrase the banner. |
| API/CertificateOfOrigins.BL/CertificateOfOriginsBl.cs:1703 | S3267 | **fix-needed** | `foreach (var certificate in certificates) { if (string.IsNullOrEmpty(certificate.ExportDeclarationNumber)) … }`. The `.Where(...)` filter is behaviourally equivalent, since the awaited body stays inside the loop. (Unlike MessageInvoice.cs:28, which is suppressed with a reason, nothing here justifies a suppression.) |
| API/CertificateOfOrigins.Test/SaveCertificateOfOriginQrCodeTests.cs:152, :153 | CS8601 | **fix-needed** (test only) | `CertificateNumber = request.CertificateNumber` / `Title = request.CertificateNumber` assign a `string?` to a non-nullable entity property in the seed. Use `request.CertificateNumber!` (the test always sets it) or a literal. |

**Warnings policy "clean except S1135": NO.** There are 8 fix-needed warnings. All are trivial comment rephrasings, one LINQ `.Where`, and one test nullability fix, and **none justifies a suppression**. The 174 REVIEW00x are the intended C18 marker warnings and do not count against the policy's spirit. (I also re-ran `tools/review-markers.ps1` read-only per project and got the same 174 = 72/10/92.) Side note: `CertificateOfOrigins.DAL.csproj` has no `SonarAnalyzer.CSharp` reference, so Sonar rules, including S1135 on the DAL's TODO, never run there (see m9).

## 2. Findings

Every Critical and Major finding below was re-read in the source. C1 and M1 were also verified by decompiling (ilspycmd) the InfrastructureCore packages the project actually resolves: Utils 1.10.105, Interfaces 1.10.38, and Proxy `BaseCustomsProxy`.

### Critical (1)

| # | file:line | Finding | Suggested fix |
|---|---|---|---|
| C1 | API/CertificateOfOrigins.BL/CertificateOfOriginsBl.Templates.cs:31 (CR 194221, 7615be3) | **The template data is JSON-encoded twice.** `GetTemplateData` already serializes the Result DTO to a camelCase JSON **string** (:62, `PrintTemplateDto.Data`). `GenerateTemplate` then passes that string to `ITemplateRequestBuilder.WithData(object)`. Decompiled `TemplateRequestBuilder.WithData` calls `CoreSerializer.SpecialSerialize(templateData)` (Newtonsoft `JsonSerializer`), which turns a `string` into a JSON **string literal** (`"{\"fileNo\":…}"`). Templates therefore receives a string, not an object, so every YAML JSONPath (`$.fileNo`, `$.letterDate`, …) resolves to nothing, and the South-Korea letter renders empty or fails. Tests miss it because Postman deliberately has no happy-path render (per the commit message). The shared `_shared/template-print-pattern.md` "templateId variant" snippet (`.WithData(dto.Data)`) has the same bug. | `.WithJsonData(printTemplate.Data)`: the builder has it, it passes JSON through unchanged and keeps the 150K length check. Fix the shared pattern file too. Add a render happy-path test (a stub `ITemplateUtil` in the NUnit project). |

### Major (4)

| # | file:line | Finding | Suggested fix |
|---|---|---|---|
| M1 | API/CertificateOfOrigins.BL/Proxies/Customer/CustomerProxy.cs:22-29, :43-48; Proxies/ExportDealFile/ExportDealFileProxy.cs:35-42, :53-60, :70-77; Proxies/Documents/DocumentsProxy.cs:36-42 (all route-style GETs) | **Route-style proxies do not survive a 404.** Every proxy uses `BaseCustomsProxy.ExecuteAsync`, which (decompiled) is `ExecuteWithoutValidationAsync` + `Validate(new List<HttpStatusCode>())`, so a 404 throws `ProxyExecutionException` (500). The BL expects `null` for not-found, so that handling is **unreachable**. `ExportDocumentAuthenticationRequestBl.cs:32` `?? throw new RestNotFoundException()` never fires: a missing customer returns 500, not 404. `CertificateOfOriginsBl.MessageValidation.cs:540-543` never reaches its `CustomerNotInCustomers` branch, so an EAI message crashes instead of returning a validation error. `CertificateOfOriginsBl.cs:1298` has the same problem. At `CertificateOfOriginsBl.MessagePerReason.cs:204`, a null lead document ("no linked declaration") becomes an exception. Mock mode hides all of this. | Per the skill and `proxy-pattern.md`: `var response = await ExecuteWithoutValidationAsync(req); response.Validate(HttpStatusCode.NotFound); return await response.GetResult<T>();` for every by-id or by-key call. |
| M2 | API/CertificateOfOrigins.BL/Proxies/Customer/CustomerProxy.cs:46; Proxies/CustomsBook/CustomsBookProxy.cs:42 (+17 more `WithResource($"…{x}…")`) | **Path segments are built by string interpolation** instead of a `{placeholder}` + `AddUrlSegmentParameter`, so they are not URL-encoded. Two of them take **string** keys: `externalId` and `fullClassification`. `MessageValidation.cs:540` passes `field.Value ?? string.Empty`; an empty value produces `api/Customer/IdByExternalId/`, a different route (404/405), which then becomes a 500 via M1. | `.WithResource("api/Customer/IdByExternalId/{externalId}").AddUrlSegmentParameter("externalId", externalId)` in all 19 routes. Short-circuit an empty key to null in the BL. |
| M3 | API/CertificateOfOrigins.BL/CertificateOfOriginsBl.cs:1427 (called from :1022) | **Live side effect with a placeholder message type.** `SendRequestFeedback` sends `MessageTypeId = 0` through the **real** `IMessageManagementProxy` (the default without `x-mock-mode`). It runs on every non-Published status change and every remarks change, **after** `SaveChangesAsync` has committed (:971). If MessageManagement rejects type 0, `ExecuteAsync` throws and the save returns 500 even though the DB committed. If it accepts, customers get a message of an undefined type. This is tracked as TODO(blocking), but the code path is active now. | Until the EMessageTypes value is confirmed, skip the send (log a warning) or gate it behind a parameter. Never send type 0. |
| M4 | API/CertificateOfOrigins.WebApi/Controllers/Api/CertificateOfOriginsController.cs:75-80; BL CertificateOfOriginsBl.Templates.cs:44-48 | **The CR 194221 integration contract is unconfirmed.** The endpoint returns a **rendered PDF** and describes itself as "the surface the distribution flow calls". The platform distribution consumer (`DeliverTemplates.Worker`, see `template-print-pattern.md`) never calls a render endpoint on the owning service. It takes the embedded `Data` or a `DataResource` callback that returns the **data**, then renders itself. `GetTemplateData` deliberately has no endpoint, so a DataResource callback has nothing to call. The internal-workload skill also expects a per-template GetTemplateData → GenerateTemplate data service. The template id (`ECertificateOfOriginsTemplate.SouthKoreaOriginVerificationLetter = 1`) is service-local and TODO(confirm). | Confirm the flow with the distribution owner. If delivery goes through `DeliverTemplate`, use the `ITemplateUtil.DeliverTemplate` recipe (WithData object + TemplateTypeId + destination), and/or expose a data endpoint (`GET api/CertificateOfOrigins/TemplateData/{templateId}/{entityId}` → `PrintTemplateDto`). Keep the render endpoint only if a consumer really needs the file. |

### Minor (14)

| # | file:line | Finding | Suggested fix |
|---|---|---|---|
| m1 | API/CertificateOfOrigins.WebApi/Controllers/Api/CertificateOfOriginsController.cs:79 | `File(result, "application/pdf")` is hardcoded, while the BL chooses the `Format` in `GetTemplateMeta`. A future Docx template would be served with the wrong content type. There is no download file name, and no OpenAPI response attribute for the binary. | Return the format from the BL (a `(Stream, Format)` tuple or DTO), map it to the MIME type, and add a file-response attribute. |
| m2 | API/CertificateOfOrigins.BL/CertificateOfOriginsBl.Templates.cs:54-60 | Reflection + `((dynamic)task).Result`. It works (the binder falls back to `Task<T>.Result`) and follows the shared pattern. But it is the only `.Result` in the codebase: fragile, not trim-safe, and it hides type errors until run time. | Put a typed delegate per case in `GetTemplateMeta` (`() => DataLayer.GetTemplateData<SouthKoreaOriginVerificationLetterResult>(…)`), or read the result via `task.GetType().GetProperty("Result")`. |
| m3 | API/CertificateOfOrigins.BL/CertificateOfOriginsBl.Templates.cs:35 | The stream returned by `ITemplateUtil.GenerateTemplate` is never disposed. | `await using var template = await templateUtil.GenerateTemplate(templateRequest);` |
| m4 | API/CertificateOfOrigins.Model/ModelDTOs/SouthKoreaOriginVerificationLetterResult.cs:16; `Scripts/API_20260923104500 - dbo.GetTemplateData.sql` | `LetterDate` (`DateTime`) serializes as ISO `2026-09-27T00:00:00` and will appear raw in the letter unless the YAML formats it. The SP also hardcodes `@TemplateID = 1`, duplicating the TODO(confirm) enum value; the two must change together. | Confirm the YAML date formatter, or return a formatted string. Cross-reference the SP branch and the enum value in comments. |
| m5 | API/CertificateOfOrigins.BL/AuthenticationRequestBl.cs:27, :161; Controllers/Ui/AuthenticationRequestController.cs:31, :154, :165; Ui/ExportDocumentAuthenticationRequestController.cs:45; Api/CertificateOfOriginsController.cs:21, :31 | Breaks the "Id not ID" rule: `GetAuthenticationRequestByID`, `GetAuthenticationRequestFileByID`, `…ByID` actions, `CertificateOfOriginID`, `GoodsItemCerificateDTO`, `ByLeadDocumentIDs`. | Rename the members. Keep the routes if they are a published contract. |
| m6 | API/CertificateOfOrigins.BL/AuthenticationRequestBl.cs:941, :1082; CertificateOfOriginsBl.cs:1041, :1593, :1596, :1598, :1718, :1803, :1881; ExportDocumentAuthenticationRequestBl.cs:190 | `return await X(...)` breaks the repo's `var result = await …; return result;` rule. (Proxies follow `proxy-pattern.md` and are fine.) | Split into `var result = …; return result;`. |
| m7 | API/CertificateOfOrigins.DAL/CertificateOfOriginsDal.cs:905, :914, :990 | These SP DAL methods take typed args and build parameters in the DAL, instead of `(object? parameters)` with `BuildParameterForProcedure` in the BL. (`GetTemplateData<T>(int,int)` is allowed by the template pattern.) | Move parameter building to the BL. |
| m8 | API/CertificateOfOrigins.BL/AuthenticationRequestBl.cs:1395-1420 | `#pragma warning disable S125` hides commented-out *future* code (the NavigationPath wiring) **outside** `#region LEGACY_WCF`. The policy allows S125 suppression only for the legacy WCF body. | Describe it in prose, or move the snippet to INTERNAL_INTEGRATION.md, and drop S125 from the pragma. |
| m9 | API/CertificateOfOrigins.DAL/CertificateOfOrigins.DAL.csproj; all csproj `<NoWarn>…NU1803;NU1603;NU1608</NoWarn>` | DAL does not reference `SonarAnalyzer.CSharp`, so Sonar rules never run on it. Every project also has a blanket NoWarn for NU1803/NU1603/NU1608, and the policy asks for targeted suppressions with a documented reason. | Add SonarAnalyzer to DAL. Document the NU16xx suppression, or replace it with pins. |
| m10 | API/CertificateOfOrigins.BL/Proxies/CustomsBook/CustomsBookProxy.cs:10; Proxies/OrganizationUnit/OrganizationUnitProxy.cs:9 | The base targets `CustomsMicroServices.SystemTables`, but the routes are `api/CustomsBook/…` / `api/OrganizationUnit/…`. The `CustomsMicroServices.CustomsBook` value exists, so real mode (the default) sends these calls to the wrong host. Covered by the TODO(blocking) "confirm owning microservice". | Use `CustomsMicroServices.CustomsBook`. Confirm the owner of OrganizationUnit. |
| m11 | API/CertificateOfOrigins.BL/Proxies/CountryGroup/CountryGroupProxy.cs:17, :28 | The route `api/SystemTables/…` uses the service name as the controller segment (the proxy-pattern double-resource rule). | Confirm the target controller name. |
| m12 | API/CertificateOfOrigins.WebApi/Controllers/Ui/AuthenticationRequestController.cs:31 | `[HttpPost("ByLeadDocumentIDs")]` is a read with a list body; the sibling reads use `[HttpQuery]` (C9). | Use `[HttpQuery]`. |
| m13 | API/CertificateOfOrigins.BL/AuditUserStamp.cs:22, :30 | `requestUserId ?? 0` silently stamps user 0 when `CC-USER-ID` is missing, the same audit loss the helper was written to fix (TODO(platform)). | Log or throw on a null UserId on write paths. |
| m14 | see §1 | **8 build warnings break the "clean except S1135" policy**: S125 ×5 (CertificateOfOriginsBl.cs:250, :1726, :1765; ServicesConfiguration.cs:111, new in 7615be3; EEventType.cs:96), S3267 (CertificateOfOriginsBl.cs:1703), CS8601 ×2 (Test/SaveCertificateOfOriginQrCodeTests.cs:152-153). | All fix-needed, none needs a suppression. Recipes are in §1. |

### Checks that passed

- **Async:** no `.Wait()`, `async void`, or `GetAwaiter().GetResult()`; the only `.Result` is m2. No `Async` suffix in BL, DAL or controllers (only the private test helpers `RunSaveAsync`/`RunDispatchAsync` have one, and tests are exempt).
- **Migration leftovers:** 0 `#error MIGRATION`. 0 `PushUtil`/`PushMessageToClient` mentions anywhere, including plain comments. The `LEGACY_WCF` regions are well-formed.
- **Braces:** a heuristic scan of every `if`/`else`/`for`/`foreach`/`while` found no brace-less bodies.
- **CA1707:** no underscores in type or member names.
- **Nullable:** enabled in every csproj.
- **BL constructors:** they inject only `IServiceProvider` / `IParametersUtil` / `ILookupUtil`. Everything else comes from a lazy `Resolve<>`.
- **Entity boundary:** no DB entity appears in a controller or in a public BL signature. `RestNotFoundException` is thrown on every GetById (AuthenticationRequestBl.cs:34, :166; CertificateOfOriginsBl.cs:32, :645, :681; ExportDocumentAuthenticationRequestBl.cs:33, :58).
- **DAL:** reads go through `ReadOnlyContext`. `Context` is used only for `ExecuteUpdate`/`ExecuteDelete` and for staged child merges committed by `BaseBL.SaveChangesAsync`, per the "BaseBL is the default save path" memory rule.
- **Hardcoded user:** no hardcoded user context (`=1`). User ids come from `RequestMetadata.UserId`.
- **Entities vs EDMX:** a script compared all 21 `[Table]` entities with the EDMX SSDL. 20 of 21 match schema + table, with **0 column-name, type or nullability mismatches**. `CertificateOfOriginsDecision` maps 9 of 11 columns: the new CR flags `IsForCoordinator`/`IsForClaliMakorWorker` are present and match `bit NOT NULL`, while `EndDate`/`IsAutomatic` are intentionally unmapped. `CertificateOfOriginsSupplierDeliveryCountryConfig` (`CRM.CertificateOfOrigins_cf_SupplierDeliveryCountryConfig`) is not in the EDMX but is created and seeded in `Scripts/API_20260715 - create tables.sql` / `- seed data.sql`.
- **CR 194221 other parts:** `AdministrativeClosure = 10` (Decision + FileStatus) is wired at both special-case sites (collateral grant :1171, openTaskStatuses :1278), has the explicit `SendDecisionMessage` case (:996), and has an idempotent seed script. `GetRequestReasonName` is safe on unmapped codes. The `GetTemplateData` DAL uses `ReadOnlyContext`, and the SP filters `State <> 99`.

## 3. DI registration (API/CertificateOfOrigins.BL/ServicesConfiguration.cs)

Every type obtained through `Resolve<>` or constructor injection is registered. The remaining `Resolve<IXxxExternalProxy>` / `IUnitOfWork` hits are only inside legacy comments.

| Kind | Registration | Line |
|---|---|---|
| DbContext | `AddCustomsDbContext<CertificateOfOriginsDbContext, CertificateOfOriginsDbReadOnlyContext>` | 22 |
| DAL | `AddDataLayer<ICertificateOfOriginsDal, CertificateOfOriginsDal>` | 23 |
| BL | `CertificateOfOriginsBl`, `AuthenticationRequestBl`, `ExportDocumentAuthenticationRequestBl`, `EventsResponseBl` | 24-29 |
| Utils | `AddHttpProxy`, `AddParametersUtil`, `AddEventUtil`, `AddDocumentUtil`, `AddQueueUtil`, `AddTemplateUtil` (new, CR 194221), `AddLockServices` | 34, 98-118 |
| Lookups | `Country`, `OrganizationUnit`, `City`, `DocumentType`. All are used; `NavigationPath` is missing (TODO(blocking), no platform type) | 121-129 |
| Program.cs | `.AddValidationMessages<ValidationMessages>()` is disabled (TODO(blocking), the package is not on the feed) | WebApi/Program.cs:20 |

**Proxies:** all 19 are registered as `AddProxy<IX, XProxy, XMockProxy>`. **The real proxy is the default for every one**; the mock is used only when the request carries the global `x-mock-mode` header. No proxy is registered mock-only.

| Proxy | Real target (CustomsMicroServices) | Effective status | Line |
|---|---|---|---|
| ICustomerProxy | Customers | Real (route TODO(blocking)) | 35 |
| IVendorProxy | Vendors | Real (route TODO(blocking)) | 38 |
| IUserProxy | Users | Real (route + DTO field TODO(blocking)) | 43 |
| IExportDealFileProxy | DealFile | **Mock is the practical default**: service not stood up | 47 |
| IDataDictionaryFieldProxy | SystemTables | Real (route TODO(blocking)) | 51 |
| ICurrencyTypeProxy | SystemTables | Real (route TODO(blocking)) | 55 |
| ICountryProxy | SystemTables | Real (route TODO(blocking)) | 63 |
| ISiteProxy | SystemTables | Real (route TODO(blocking)) | 64 |
| IInternationalSiteProxy | SystemTables | Real (route TODO(blocking)) | 65 |
| IPackingTypeProxy | SystemTables | Real (route TODO(blocking)) | 66 |
| IMeasurementUnitProxy | SystemTables | Real (route TODO(blocking)) | 67 |
| IDocumentsProxy | Documents | Real (route TODO(blocking)) | 71 |
| ICollateralProxy | Collaterals | Real (route + response contract TODO(blocking)); the mock throws on 3 write scenarios | 75 |
| ITasksProxy | Tasks | Real (route TODO(blocking)) | 78 |
| IMessageManagementProxy | Common | Real (route TODO(blocking)); see M3 | 82 |
| ICustomsBookProxy | **SystemTables** (should be CustomsBook, m10) | **Mock is the practical default**: service not stood up | 87 |
| ICommonServicesProxy (QR + SSRS template) | Common | **Mock is the practical default**: Templates/QR not migrated | 88 |
| IOrganizationUnitProxy | SystemTables (owner unconfirmed) | **Mock is the practical default** | 89 |
| ICountryGroupProxy | SystemTables | **Mock is the practical default** (route unconfirmed) | 93 |

## 4. TODO inventory

Command: `grep -rniE "//\s*TODO|/\*\s*TODO|TODO\(|FIXME|HACK|XXX\b" --include=*.cs API/` (bin/obj excluded). **168 hits. 0 FIXME / 0 HACK / 0 XXX.** Planar `.cs` has 0 TODOs.

### Summary counts

| Category | Count |
|---|---|
| hardcoded =1 user context | **0** |
| TODO(confirm) enum/event/process | **6** |
| Mock / switch-to-real proxy: dummy mock data (`// TODO: dummy data`) | **74** |
| Mock / switch-to-real proxy: ServicesConfiguration "verify real endpoint before ROLLOUT" (blocking) | **13** |
| confirm endpoint: proxy route / owning service (all TODO(blocking)) | **41** |
| TODO(infra)/no source: resx/ValidationMessages, no lookup type, platform (3 of these are blocking) | **18** |
| TODO(blocking): functional gap / design decision | **6** |
| TODO(blocking): confirm DTO contract | **2** |
| other: deferred migration item (TODO(migration), not infra) | **3** |
| other: cross-reference to a blocking TODO elsewhere | **5** |
| **Total** | **168** |
| **Actionable TODO(blocking)** | **65** (13 switch-to-real + 41 confirm-endpoint + 3 infra + 6 functional + 2 contract) |

**PushUtil / PushMessageToClient in plain comments:** **0**, so nothing needs upgrading to TODO(infra).

**TODO words the grep pattern does not match.** The text contains "TODO" but not in a `// TODO` or `TODO(` form, and it still raises S1135. Each should be upgraded to a proper `TODO(...)` or rewritten:

| file:line | text | suggestion |
|---|---|---|
| API/CertificateOfOrigins.BL/AuthenticationRequestBl.cs:1395 | pragma comment "…the legacy-mapping reference in the TODO is intentional" | reword (see m8) |
| API/CertificateOfOrigins.BL/CertificateOfOriginsBl.cs:77 | "See the default-branch TODO." | cross-ref, fine |
| API/CertificateOfOrigins.BL/CertificateOfOriginsBl.cs:903 | "…core with mock proxies + TODO for the un-stood-up services" | cross-ref, fine |
| API/CertificateOfOrigins.BL/CertificateOfOriginsBl.cs:1350 | "SystemTables proxy — rollout TODO" | → `TODO(infra)` |
| API/CertificateOfOrigins.BL/CertificateOfOriginsBl.MessageValidation.cs:678 | "Country-group id→name has no ILookupUtil type … (rollout TODO)" | → `TODO(infra)` |
| API/CertificateOfOrigins.BL/ServicesConfiguration.cs:125 | "…need a SystemTables proxy (rollout TODO)" | → `TODO(infra)` |
| API/CertificateOfOrigins.WebApi/Controllers/Ui/AuthenticationRequestController.cs:197 | "(see the BL TODO + INTERNAL_INTEGRATION.md)" | cross-ref, fine |

### TODO(blocking): one row each (65)

| # | file:line | Category | Text |
|---|---|---|---|
| B1 | API/CertificateOfOrigins.BL/AuthenticationRequestBl.cs:1400 | TODO(infra)/no source | TODO(blocking): NavigationPath is a shared GeneralServices reference table with no platform lookup type yet. |
| B2 | API/CertificateOfOrigins.BL/CertificateOfOriginsBl.cs:1012 | TODO(blocking) — functional gap / design decision | TODO(blocking) audited gaps vs legacy SaveCertificateOfOrigin, deferred pending platform/schema: |
| B3 | API/CertificateOfOrigins.BL/CertificateOfOriginsBl.cs:1036 | TODO(blocking) — functional gap / design decision | TODO(blocking) (#2): legacy called CheckCertificateOfOriginOnDeclarationReleased here (via the service partial, |
| B4 | API/CertificateOfOrigins.BL/CertificateOfOriginsBl.cs:1414 | TODO(blocking) — functional gap / design decision | TODO(blocking): legacy delivered this as an EAI OUTGOING message (OutgoingMessageProxy → |
| B5 | API/CertificateOfOrigins.BL/CertificateOfOriginsBl.cs:1427 | TODO(blocking) — functional gap / design decision | TODO(blocking): map the real EMessageTypes value for the certificate request feedback. |
| B6 | API/CertificateOfOrigins.BL/CertificateOfOriginsBl.cs:1896 | TODO(blocking) — functional gap / design decision | TODO(blocking): {2} has TWO claimants. The legacy put the "additional certificates on this declaration" |
| B7 | API/CertificateOfOrigins.BL/Proxies/Collateral/CollateralProxy.cs:18 | confirm endpoint | TODO(blocking): confirm endpoint name/route with the Collateral microservice |
| B8 | API/CertificateOfOrigins.BL/Proxies/Collateral/CollateralProxy.cs:29 | confirm endpoint | TODO(blocking): confirm endpoint name/route with the Collateral microservice |
| B9 | API/CertificateOfOrigins.BL/Proxies/Collateral/CollateralProxy.cs:40 | confirm endpoint | TODO(blocking): confirm endpoint name/route with the Collateral microservice |
| B10 | API/CertificateOfOrigins.BL/Proxies/Collateral/CollateralProxy.cs:50 | confirm endpoint | TODO(blocking): confirm endpoint name/route with the Collateral microservice |
| B11 | API/CertificateOfOrigins.BL/Proxies/Collateral/CollateralProxy.cs:61 | confirm endpoint | TODO(blocking): confirm endpoint name/route with the Collateral microservice |
| B12 | API/CertificateOfOrigins.BL/Proxies/CommonServices/CommonServicesProxy.cs:16 | confirm endpoint | TODO(blocking): confirm endpoint name/route with the Common (QR) microservice |
| B13 | API/CertificateOfOrigins.BL/Proxies/CommonServices/CommonServicesProxy.cs:24 | TODO(blocking) — functional gap / design decision | TODO(blocking): the Templates generation service + the certificate-of-origin templates are not yet migrated |
| B14 | API/CertificateOfOrigins.BL/Proxies/Country/CountryProxy.cs:18 | confirm endpoint | TODO(blocking): confirm endpoint name/route with the SystemTables microservice |
| B15 | API/CertificateOfOrigins.BL/Proxies/CountryGroup/CountryGroupProxy.cs:13 | confirm endpoint | TODO(blocking): confirm the SystemTables CountryCountryGroup endpoint route (until then the mock is enabled |
| B16 | API/CertificateOfOrigins.BL/Proxies/CountryGroup/CountryGroupProxy.cs:24 | confirm endpoint | TODO(blocking): confirm the SystemTables CountryGroup endpoint route (until then the mock is enabled via |
| B17 | API/CertificateOfOrigins.BL/Proxies/CurrencyType/CurrencyTypeProxy.cs:18 | confirm endpoint | TODO(blocking): confirm endpoint name/route with the SystemTables microservice |
| B18 | API/CertificateOfOrigins.BL/Proxies/CurrencyType/CurrencyTypeProxy.cs:30 | confirm endpoint | TODO(blocking): confirm endpoint name/route with the SystemTables microservice |
| B19 | API/CertificateOfOrigins.BL/Proxies/Customer/CustomerProxy.cs:16 | confirm endpoint | TODO(blocking): confirm endpoint name/route with the Customers microservice |
| B20 | API/CertificateOfOrigins.BL/Proxies/Customer/CustomerProxy.cs:26 | confirm endpoint | TODO(blocking): confirm endpoint name/route with the Customers microservice |
| B21 | API/CertificateOfOrigins.BL/Proxies/Customer/CustomerProxy.cs:37 | confirm endpoint | TODO(blocking): confirm endpoint name/route with the Customers microservice |
| B22 | API/CertificateOfOrigins.BL/Proxies/Customer/CustomerProxy.cs:46 | confirm endpoint | TODO(blocking): confirm endpoint name/route with the Customers microservice |
| B23 | API/CertificateOfOrigins.BL/Proxies/CustomsBook/CustomsBookProxy.cs:14 | confirm endpoint | TODO(blocking): the CustomsBook / trade-agreement service is not yet stood up — confirm the owning |
| B24 | API/CertificateOfOrigins.BL/Proxies/CustomsBook/CustomsBookProxy.cs:25 | confirm endpoint | TODO(blocking): the CustomsBook customs-item service is not yet stood up — confirm the owning microservice + |
| B25 | API/CertificateOfOrigins.BL/Proxies/CustomsBook/CustomsBookProxy.cs:37 | confirm endpoint | TODO(blocking): the CustomsBook customs-item service is not yet stood up — confirm the owning microservice + |
| B26 | API/CertificateOfOrigins.BL/Proxies/DataDictionaryField/DataDictionaryFieldProxy.cs:18 | confirm endpoint | TODO(blocking): confirm endpoint name/route with the SystemTables microservice |
| B27 | API/CertificateOfOrigins.BL/Proxies/Documents/DocumentsProxy.cs:18 | confirm endpoint | TODO(blocking): confirm endpoint name/route with the Documents microservice |
| B28 | API/CertificateOfOrigins.BL/Proxies/Documents/DocumentsProxy.cs:29 | confirm endpoint | TODO(blocking): confirm endpoint name/route with the Documents microservice |
| B29 | API/CertificateOfOrigins.BL/Proxies/Documents/DocumentsProxy.cs:40 | confirm endpoint | TODO(blocking): confirm endpoint name/route with the Documents microservice |
| B30 | API/CertificateOfOrigins.BL/Proxies/Documents/DocumentsProxy.cs:50 | confirm endpoint | TODO(blocking): confirm endpoint name/route with the Documents microservice |
| B31 | API/CertificateOfOrigins.BL/Proxies/ExportDealFile/ExportDealFileProxy.cs:14 | confirm endpoint | TODO(blocking): the ExportDealFile microservice is not yet stood up — confirm the endpoint name/route |
| B32 | API/CertificateOfOrigins.BL/Proxies/ExportDealFile/ExportDealFileProxy.cs:39 | confirm endpoint | TODO(blocking): confirm endpoint name/route with the DealFile microservice |
| B33 | API/CertificateOfOrigins.BL/Proxies/ExportDealFile/ExportDealFileProxy.cs:48 | confirm endpoint | TODO(blocking): confirm endpoint name/route with the DealFile microservice |
| B34 | API/CertificateOfOrigins.BL/Proxies/ExportDealFile/ExportDealFileProxy.cs:57 | confirm endpoint | TODO(blocking): confirm endpoint name/route with the DealFile microservice |
| B35 | API/CertificateOfOrigins.BL/Proxies/ExportDealFile/ExportDealFileProxy.cs:66 | confirm endpoint | TODO(blocking): confirm endpoint name/route with the DealFile microservice |
| B36 | API/CertificateOfOrigins.BL/Proxies/ExportDealFile/ExportDealFileProxy.cs:74 | confirm endpoint | TODO(blocking): confirm endpoint name/route with the DealFile microservice |
| B37 | API/CertificateOfOrigins.BL/Proxies/ExportDealFile/ExportDealFileProxy.cs:83 | confirm endpoint | TODO(blocking): confirm endpoint name/route with the DealFile microservice |
| B38 | API/CertificateOfOrigins.BL/Proxies/InternationalSite/InternationalSiteProxy.cs:18 | confirm endpoint | TODO(blocking): confirm endpoint name/route with the SystemTables microservice |
| B39 | API/CertificateOfOrigins.BL/Proxies/MeasurementUnit/MeasurementUnitProxy.cs:18 | confirm endpoint | TODO(blocking): confirm endpoint name/route with the SystemTables microservice |
| B40 | API/CertificateOfOrigins.BL/Proxies/MessageManagement/MessageManagementProxy.cs:18 | confirm endpoint | TODO(blocking): confirm endpoint name/route with the Message-Management (Common) microservice |
| B41 | API/CertificateOfOrigins.BL/Proxies/OrganizationUnit/OrganizationUnitProxy.cs:13 | confirm endpoint | TODO(blocking): confirm the owning microservice (org-unit / SystemTables) + endpoint route |
| B42 | API/CertificateOfOrigins.BL/Proxies/PackingType/PackingTypeProxy.cs:18 | confirm endpoint | TODO(blocking): confirm endpoint name/route with the SystemTables microservice |
| B43 | API/CertificateOfOrigins.BL/Proxies/Site/SiteProxy.cs:19 | confirm endpoint | TODO(blocking): confirm endpoint name/route with the SystemTables microservice |
| B44 | API/CertificateOfOrigins.BL/Proxies/Tasks/TasksProxy.cs:18 | confirm endpoint | TODO(blocking): confirm endpoint name/route with the Tasks microservice |
| B45 | API/CertificateOfOrigins.BL/Proxies/Tasks/TasksProxy.cs:31 | confirm endpoint | TODO(blocking): confirm endpoint name/route with the Tasks microservice |
| B46 | API/CertificateOfOrigins.BL/Proxies/Tasks/TasksProxy.cs:39 | confirm endpoint | TODO(blocking): confirm endpoint name/route with the Tasks microservice. The real endpoint returns a |
| B47 | API/CertificateOfOrigins.BL/Proxies/User/UserProxy.cs:16 | confirm endpoint | TODO(blocking): confirm endpoint name/route with the Users microservice |
| B48 | API/CertificateOfOrigins.BL/Proxies/Vendor/VendorProxy.cs:16 | confirm endpoint | TODO(blocking): confirm endpoint name/route with the Vendors microservice |
| B49 | API/CertificateOfOrigins.BL/ServicesConfiguration.cs:33 | Mock / switch-to-real proxy | TODO(blocking): verify the real Customers endpoint (CustomersByIds) before ROLLOUT. |
| B50 | API/CertificateOfOrigins.BL/ServicesConfiguration.cs:37 | Mock / switch-to-real proxy | TODO(blocking): verify the real Vendors endpoint (VendorsByIds) before ROLLOUT. |
| B51 | API/CertificateOfOrigins.BL/ServicesConfiguration.cs:42 | Mock / switch-to-real proxy | TODO(blocking): verify the real Users endpoint (User/UsersByIds) before ROLLOUT. |
| B52 | API/CertificateOfOrigins.BL/ServicesConfiguration.cs:45 | Mock / switch-to-real proxy | TODO(blocking): the ExportDealFile microservice is not yet stood up — the mock is the practical |
| B53 | API/CertificateOfOrigins.BL/ServicesConfiguration.cs:50 | Mock / switch-to-real proxy | TODO(blocking): verify the real SystemTables endpoint before ROLLOUT. |
| B54 | API/CertificateOfOrigins.BL/ServicesConfiguration.cs:54 | Mock / switch-to-real proxy | TODO(blocking): verify the real SystemTables endpoint before ROLLOUT. |
| B55 | API/CertificateOfOrigins.BL/ServicesConfiguration.cs:60 | Mock / switch-to-real proxy | TODO(blocking): verify the real SystemTables endpoints (Country/CountriesByAlphaCodes, Site/SitesByExternalNumbers, |
| B56 | API/CertificateOfOrigins.BL/ServicesConfiguration.cs:70 | Mock / switch-to-real proxy | TODO(blocking): verify the real Documents endpoint (Document/DocumentsByEntity) before ROLLOUT. |
| B57 | API/CertificateOfOrigins.BL/ServicesConfiguration.cs:74 | Mock / switch-to-real proxy | TODO(blocking): verify the real Collateral (Collateral/CollateralRequestByEntity) endpoint before ROLLOUT. |
| B58 | API/CertificateOfOrigins.BL/ServicesConfiguration.cs:77 | Mock / switch-to-real proxy | TODO(blocking): verify the real Tasks (Task/IsTaskExist) endpoint before ROLLOUT. |
| B59 | API/CertificateOfOrigins.BL/ServicesConfiguration.cs:81 | Mock / switch-to-real proxy | TODO(blocking): verify the real Message-Management (Message/SendMessage) endpoint before ROLLOUT. |
| B60 | API/CertificateOfOrigins.BL/ServicesConfiguration.cs:86 | Mock / switch-to-real proxy | TODO(blocking): confirm each owning microservice + endpoint route before ROLLOUT. |
| B61 | API/CertificateOfOrigins.BL/ServicesConfiguration.cs:92 | Mock / switch-to-real proxy | TODO(blocking): confirm the endpoint route before ROLLOUT. |
| B62 | API/CertificateOfOrigins.BL/ServicesConfiguration.cs:131 | TODO(infra)/no source | TODO(blocking): GetPathsForNavigationToVendor needs a NavigationPath lookup. NavigationPath is a shared |
| B63 | API/CertificateOfOrigins.Model/ModelDTOs/CollateralRequestDto.cs:4 | TODO(blocking) — confirm DTO contract | TODO(blocking): confirm the Collateral service's response |
| B64 | API/CertificateOfOrigins.Model/ModelDTOs/UserDto.cs:11 | TODO(blocking) — confirm DTO contract | TODO(blocking): confirm the field name exposed by the Users microservice (User/UsersByIds). |
| B65 | API/CertificateOfOrigins.WebApi/Program.cs:20 | TODO(infra)/no source | TODO(blocking): .AddValidationMessages<ValidationMessages>() — re-enable when the |

### Full inventory (168)

| # | file:line | Category | Blocking | Text |
|---|---|---|---|---|
| 1 | API/CertificateOfOrigins.BL/AuditUserStamp.cs:15 | TODO(infra)/no source |  | TODO(platform): the proper fix is for the request pipeline to populate RequestMetadata.User, after which these |
| 2 | API/CertificateOfOrigins.BL/AuthenticationRequestBl.cs:327 | TODO(confirm) enum/event/process |  | TODO(confirm): not exact parity — IsTaskInProgress is TaskStatusID IN (1,4), while the legacy '!= 2' also |
| 3 | API/CertificateOfOrigins.BL/AuthenticationRequestBl.cs:761 | TODO(infra)/no source |  | TODO(migration): LeadDocumentTitle stays null — it's a CRP.DealFile document (no lookup type; needs the |
| 4 | API/CertificateOfOrigins.BL/AuthenticationRequestBl.cs:809 | TODO(infra)/no source |  | TODO(migration): LeadDocumentTitle stays null — CRP.DealFile document, needs the owning service's proxy. |
| 5 | API/CertificateOfOrigins.BL/AuthenticationRequestBl.cs:1400 | TODO(infra)/no source | YES | TODO(blocking): NavigationPath is a shared GeneralServices reference table with no platform lookup type yet. |
| 6 | API/CertificateOfOrigins.BL/AuthenticationRequestBl.Schedulers.cs:29 | TODO(confirm) enum/event/process |  | TODO(confirm): exact parity needs the raw TaskStatusID (or a status filter) from the Tasks service; until |
| 7 | API/CertificateOfOrigins.BL/AuthenticationRequestBl.Schedulers.cs:89 | TODO(confirm) enum/event/process |  | TODO(confirm): the id→parameter pairing for 1600↔...Request3 and 1148↔...Request1 was inferred from those |
| 8 | API/CertificateOfOrigins.BL/CertificateOfOriginsBl.cs:367 | other (cross-reference to a blocking TODO elsewhere) | ref | TODO(blocking)); 0 when there is no such document. |
| 9 | API/CertificateOfOrigins.BL/CertificateOfOriginsBl.cs:908 | TODO(infra)/no source |  | TODO(migration): country-group + international-site id→name (no ILookupUtil type — need a SystemTables proxy) and |
| 10 | API/CertificateOfOrigins.BL/CertificateOfOriginsBl.cs:1012 | TODO(blocking) — functional gap / design decision | YES | TODO(blocking) audited gaps vs legacy SaveCertificateOfOrigin, deferred pending platform/schema: |
| 11 | API/CertificateOfOrigins.BL/CertificateOfOriginsBl.cs:1036 | TODO(blocking) — functional gap / design decision | YES | TODO(blocking) (#2): legacy called CheckCertificateOfOriginOnDeclarationReleased here (via the service partial, |
| 12 | API/CertificateOfOrigins.BL/CertificateOfOriginsBl.cs:1241 | TODO(infra)/no source |  | TODO(migration): country-group + international-site id→name have no ILookupUtil type — they need a SystemTables |
| 13 | API/CertificateOfOrigins.BL/CertificateOfOriginsBl.cs:1374 | other (deferred migration item) |  | TODO(migration): the title-mismatch validation + CheckDeclarationStatus are deferred. |
| 14 | API/CertificateOfOrigins.BL/CertificateOfOriginsBl.cs:1414 | TODO(blocking) — functional gap / design decision | YES | TODO(blocking): legacy delivered this as an EAI OUTGOING message (OutgoingMessageProxy → |
| 15 | API/CertificateOfOrigins.BL/CertificateOfOriginsBl.cs:1427 | TODO(blocking) — functional gap / design decision | YES | TODO(blocking): map the real EMessageTypes value for the certificate request feedback. |
| 16 | API/CertificateOfOrigins.BL/CertificateOfOriginsBl.cs:1524 | TODO(infra)/no source |  | TODO(migration): three resx-sourced texts are still deferred (no ValidationMessages source yet) plus the agent |
| 17 | API/CertificateOfOrigins.BL/CertificateOfOriginsBl.cs:1587 | TODO(confirm) enum/event/process |  | TODO(confirm): drop the gate for good, or reinstate it if the parameter can return false. |
| 18 | API/CertificateOfOrigins.BL/CertificateOfOriginsBl.cs:1614 | TODO(infra)/no source |  | TODO(migration): only the exception text + EMessages code + Error/Warning level source (ValidationMessages/resx, |
| 19 | API/CertificateOfOrigins.BL/CertificateOfOriginsBl.cs:1726 | TODO(confirm) enum/event/process |  | TODO(confirm): the legacy release-publish also sent the request-feedback message with the rendered attachments; |
| 20 | API/CertificateOfOrigins.BL/CertificateOfOriginsBl.cs:1809 | other (deferred migration item) |  | TODO(migration): the agent talk-back message (legacy servicesAdapter.SendMessageToAgent) is deferred — the same |
| 21 | API/CertificateOfOrigins.BL/CertificateOfOriginsBl.cs:1896 | TODO(blocking) — functional gap / design decision | YES | TODO(blocking): {2} has TWO claimants. The legacy put the "additional certificates on this declaration" |
| 22 | API/CertificateOfOrigins.BL/CertificateOfOriginsBl.cs:1979 | other (deferred migration item) |  | TODO(migration): the legacy SingleUserAssignmentFilter also carried EProfession.Marech + the Export |
| 23 | API/CertificateOfOrigins.BL/CertificateOfOriginsBl.cs:2008 | TODO(infra)/no source |  | TODO(migration): the localized + English exception texts and their EMessages codes are placeholders here — source |
| 24 | API/CertificateOfOrigins.BL/CertificateOfOriginsBl.cs:2369 | TODO(infra)/no source |  | TODO(migration): the real EMessages code (legacy GetUIMessageWithEnglishAndLevel). |
| 25 | API/CertificateOfOrigins.BL/CertificateOfOriginsBl.cs:2378 | TODO(infra)/no source |  | TODO(migration): when it lands, set ExceptionType to the real |
| 26 | API/CertificateOfOrigins.BL/CertificateOfOriginsBl.MessageValidationMessages.cs:12 | TODO(infra)/no source |  | TODO(migration): when the resx / BaseValidationMessages pipeline lands (see Program.cs), source the text from |
| 27 | API/CertificateOfOrigins.BL/CertificateOfOriginsBl.MessageValidationMessages.cs:117 | TODO(infra)/no source |  | TODO(migration): confirm exact UIMessage text for code 5022 against the module's UIMessage export. |
| 28 | API/CertificateOfOrigins.BL/CertificateOfOriginsConsts.cs:61 | TODO(infra)/no source |  | TODO(migration): source from ValidationMessages/resx. |
| 29 | API/CertificateOfOrigins.BL/CertificateOfOriginsConsts.cs:64 | TODO(infra)/no source |  | TODO(migration): source from ValidationMessages/resx. |
| 30 | API/CertificateOfOrigins.BL/CertificateOfOriginsConsts.cs:68 | TODO(infra)/no source |  | TODO(migration): source the exact text from ValidationMessages/resx (EServerTerms.CanceledDeclaration). |
| 31 | API/CertificateOfOrigins.BL/Proxies/Collateral/CollateralMockProxy.cs:22 | Mock / switch-to-real proxy (dummy mock data) |  | TODO: dummy data |
| 32 | API/CertificateOfOrigins.BL/Proxies/Collateral/CollateralMockProxy.cs:23 | Mock / switch-to-real proxy (dummy mock data) |  | TODO: dummy data |
| 33 | API/CertificateOfOrigins.BL/Proxies/Collateral/CollateralMockProxy.cs:24 | Mock / switch-to-real proxy (dummy mock data) |  | TODO: dummy data |
| 34 | API/CertificateOfOrigins.BL/Proxies/Collateral/CollateralMockProxy.cs:25 | Mock / switch-to-real proxy (dummy mock data) |  | TODO: dummy data |
| 35 | API/CertificateOfOrigins.BL/Proxies/Collateral/CollateralMockProxy.cs:27 | Mock / switch-to-real proxy (dummy mock data) |  | TODO: dummy data |
| 36 | API/CertificateOfOrigins.BL/Proxies/Collateral/CollateralMockProxy.cs:28 | Mock / switch-to-real proxy (dummy mock data) |  | TODO: dummy data |
| 37 | API/CertificateOfOrigins.BL/Proxies/Collateral/CollateralMockProxy.cs:54 | Mock / switch-to-real proxy (dummy mock data) |  | TODO: dummy data |
| 38 | API/CertificateOfOrigins.BL/Proxies/Collateral/CollateralProxy.cs:18 | confirm endpoint | YES | TODO(blocking): confirm endpoint name/route with the Collateral microservice |
| 39 | API/CertificateOfOrigins.BL/Proxies/Collateral/CollateralProxy.cs:29 | confirm endpoint | YES | TODO(blocking): confirm endpoint name/route with the Collateral microservice |
| 40 | API/CertificateOfOrigins.BL/Proxies/Collateral/CollateralProxy.cs:40 | confirm endpoint | YES | TODO(blocking): confirm endpoint name/route with the Collateral microservice |
| 41 | API/CertificateOfOrigins.BL/Proxies/Collateral/CollateralProxy.cs:50 | confirm endpoint | YES | TODO(blocking): confirm endpoint name/route with the Collateral microservice |
| 42 | API/CertificateOfOrigins.BL/Proxies/Collateral/CollateralProxy.cs:61 | confirm endpoint | YES | TODO(blocking): confirm endpoint name/route with the Collateral microservice |
| 43 | API/CertificateOfOrigins.BL/Proxies/CommonServices/CommonServicesMockProxy.cs:19 | Mock / switch-to-real proxy (dummy mock data) |  | TODO: dummy JPEG bytes |
| 44 | API/CertificateOfOrigins.BL/Proxies/CommonServices/CommonServicesMockProxy.cs:33 | Mock / switch-to-real proxy (dummy mock data) |  | TODO: dummy data |
| 45 | API/CertificateOfOrigins.BL/Proxies/CommonServices/CommonServicesMockProxy.cs:34 | Mock / switch-to-real proxy (dummy mock data) |  | TODO: dummy PDF bytes |
| 46 | API/CertificateOfOrigins.BL/Proxies/CommonServices/CommonServicesMockProxy.cs:35 | Mock / switch-to-real proxy (dummy mock data) |  | TODO: dummy data |
| 47 | API/CertificateOfOrigins.BL/Proxies/CommonServices/CommonServicesProxy.cs:16 | confirm endpoint | YES | TODO(blocking): confirm endpoint name/route with the Common (QR) microservice |
| 48 | API/CertificateOfOrigins.BL/Proxies/CommonServices/CommonServicesProxy.cs:24 | TODO(blocking) — functional gap / design decision | YES | TODO(blocking): the Templates generation service + the certificate-of-origin templates are not yet migrated |
| 49 | API/CertificateOfOrigins.BL/Proxies/Country/CountryMockProxy.cs:23 | Mock / switch-to-real proxy (dummy mock data) |  | TODO: dummy data |
| 50 | API/CertificateOfOrigins.BL/Proxies/Country/CountryMockProxy.cs:25 | Mock / switch-to-real proxy (dummy mock data) |  | TODO: dummy data |
| 51 | API/CertificateOfOrigins.BL/Proxies/Country/CountryProxy.cs:18 | confirm endpoint | YES | TODO(blocking): confirm endpoint name/route with the SystemTables microservice |
| 52 | API/CertificateOfOrigins.BL/Proxies/CountryGroup/CountryGroupProxy.cs:13 | confirm endpoint | YES | TODO(blocking): confirm the SystemTables CountryCountryGroup endpoint route (until then the mock is enabled |
| 53 | API/CertificateOfOrigins.BL/Proxies/CountryGroup/CountryGroupProxy.cs:24 | confirm endpoint | YES | TODO(blocking): confirm the SystemTables CountryGroup endpoint route (until then the mock is enabled via |
| 54 | API/CertificateOfOrigins.BL/Proxies/CurrencyType/CurrencyTypeMockProxy.cs:22 | Mock / switch-to-real proxy (dummy mock data) |  | TODO: dummy data |
| 55 | API/CertificateOfOrigins.BL/Proxies/CurrencyType/CurrencyTypeMockProxy.cs:23 | Mock / switch-to-real proxy (dummy mock data) |  | TODO: dummy data |
| 56 | API/CertificateOfOrigins.BL/Proxies/CurrencyType/CurrencyTypeMockProxy.cs:38 | Mock / switch-to-real proxy (dummy mock data) |  | TODO: dummy data |
| 57 | API/CertificateOfOrigins.BL/Proxies/CurrencyType/CurrencyTypeProxy.cs:18 | confirm endpoint | YES | TODO(blocking): confirm endpoint name/route with the SystemTables microservice |
| 58 | API/CertificateOfOrigins.BL/Proxies/CurrencyType/CurrencyTypeProxy.cs:30 | confirm endpoint | YES | TODO(blocking): confirm endpoint name/route with the SystemTables microservice |
| 59 | API/CertificateOfOrigins.BL/Proxies/Customer/CustomerMockProxy.cs:20 | Mock / switch-to-real proxy (dummy mock data) |  | TODO: dummy data |
| 60 | API/CertificateOfOrigins.BL/Proxies/Customer/CustomerMockProxy.cs:21 | Mock / switch-to-real proxy (dummy mock data) |  | TODO: dummy data |
| 61 | API/CertificateOfOrigins.BL/Proxies/Customer/CustomerMockProxy.cs:37 | Mock / switch-to-real proxy (dummy mock data) |  | TODO: dummy data |
| 62 | API/CertificateOfOrigins.BL/Proxies/Customer/CustomerMockProxy.cs:43 | Mock / switch-to-real proxy (dummy mock data) |  | TODO: dummy authentication address |
| 63 | API/CertificateOfOrigins.BL/Proxies/Customer/CustomerMockProxy.cs:60 | Mock / switch-to-real proxy (dummy mock data) |  | TODO: dummy data |
| 64 | API/CertificateOfOrigins.BL/Proxies/Customer/CustomerMockProxy.cs:61 | Mock / switch-to-real proxy (dummy mock data) |  | TODO: dummy data |
| 65 | API/CertificateOfOrigins.BL/Proxies/Customer/CustomerMockProxy.cs:78 | Mock / switch-to-real proxy (dummy mock data) |  | TODO: dummy data |
| 66 | API/CertificateOfOrigins.BL/Proxies/Customer/CustomerProxy.cs:16 | confirm endpoint | YES | TODO(blocking): confirm endpoint name/route with the Customers microservice |
| 67 | API/CertificateOfOrigins.BL/Proxies/Customer/CustomerProxy.cs:26 | confirm endpoint | YES | TODO(blocking): confirm endpoint name/route with the Customers microservice |
| 68 | API/CertificateOfOrigins.BL/Proxies/Customer/CustomerProxy.cs:37 | confirm endpoint | YES | TODO(blocking): confirm endpoint name/route with the Customers microservice |
| 69 | API/CertificateOfOrigins.BL/Proxies/Customer/CustomerProxy.cs:46 | confirm endpoint | YES | TODO(blocking): confirm endpoint name/route with the Customers microservice |
| 70 | API/CertificateOfOrigins.BL/Proxies/CustomsBook/CustomsBookMockProxy.cs:51 | Mock / switch-to-real proxy (dummy mock data) |  | TODO: dummy data |
| 71 | API/CertificateOfOrigins.BL/Proxies/CustomsBook/CustomsBookProxy.cs:14 | confirm endpoint | YES | TODO(blocking): the CustomsBook / trade-agreement service is not yet stood up — confirm the owning |
| 72 | API/CertificateOfOrigins.BL/Proxies/CustomsBook/CustomsBookProxy.cs:25 | confirm endpoint | YES | TODO(blocking): the CustomsBook customs-item service is not yet stood up — confirm the owning microservice + |
| 73 | API/CertificateOfOrigins.BL/Proxies/CustomsBook/CustomsBookProxy.cs:37 | confirm endpoint | YES | TODO(blocking): the CustomsBook customs-item service is not yet stood up — confirm the owning microservice + |
| 74 | API/CertificateOfOrigins.BL/Proxies/DataDictionaryField/DataDictionaryFieldMockProxy.cs:20 | Mock / switch-to-real proxy (dummy mock data) |  | TODO: dummy data |
| 75 | API/CertificateOfOrigins.BL/Proxies/DataDictionaryField/DataDictionaryFieldMockProxy.cs:21 | Mock / switch-to-real proxy (dummy mock data) |  | TODO: dummy data |
| 76 | API/CertificateOfOrigins.BL/Proxies/DataDictionaryField/DataDictionaryFieldProxy.cs:18 | confirm endpoint | YES | TODO(blocking): confirm endpoint name/route with the SystemTables microservice |
| 77 | API/CertificateOfOrigins.BL/Proxies/Documents/DocumentsMockProxy.cs:22 | Mock / switch-to-real proxy (dummy mock data) |  | TODO: dummy data |
| 78 | API/CertificateOfOrigins.BL/Proxies/Documents/DocumentsMockProxy.cs:23 | Mock / switch-to-real proxy (dummy mock data) |  | TODO: dummy data (a value from CertificateOfOriginsDocumentsFilter) |
| 79 | API/CertificateOfOrigins.BL/Proxies/Documents/DocumentsMockProxy.cs:26 | Mock / switch-to-real proxy (dummy mock data) |  | TODO: dummy data |
| 80 | API/CertificateOfOrigins.BL/Proxies/Documents/DocumentsMockProxy.cs:30 | Mock / switch-to-real proxy (dummy mock data) |  | TODO: dummy data |
| 81 | API/CertificateOfOrigins.BL/Proxies/Documents/DocumentsMockProxy.cs:35 | Mock / switch-to-real proxy (dummy mock data) |  | TODO: dummy data |
| 82 | API/CertificateOfOrigins.BL/Proxies/Documents/DocumentsMockProxy.cs:36 | Mock / switch-to-real proxy (dummy mock data) |  | TODO: dummy data |
| 83 | API/CertificateOfOrigins.BL/Proxies/Documents/DocumentsMockProxy.cs:39 | Mock / switch-to-real proxy (dummy mock data) |  | TODO: dummy data |
| 84 | API/CertificateOfOrigins.BL/Proxies/Documents/DocumentsMockProxy.cs:43 | Mock / switch-to-real proxy (dummy mock data) |  | TODO: dummy data |
| 85 | API/CertificateOfOrigins.BL/Proxies/Documents/DocumentsMockProxy.cs:48 | Mock / switch-to-real proxy (dummy mock data) |  | TODO: dummy data |
| 86 | API/CertificateOfOrigins.BL/Proxies/Documents/DocumentsMockProxy.cs:52 | Mock / switch-to-real proxy (dummy mock data) |  | TODO: dummy data |
| 87 | API/CertificateOfOrigins.BL/Proxies/Documents/DocumentsMockProxy.cs:56 | Mock / switch-to-real proxy (dummy mock data) |  | TODO: dummy data |
| 88 | API/CertificateOfOrigins.BL/Proxies/Documents/DocumentsMockProxy.cs:79 | Mock / switch-to-real proxy (dummy mock data) |  | TODO: dummy data |
| 89 | API/CertificateOfOrigins.BL/Proxies/Documents/DocumentsMockProxy.cs:80 | Mock / switch-to-real proxy (dummy mock data) |  | TODO: dummy data |
| 90 | API/CertificateOfOrigins.BL/Proxies/Documents/DocumentsMockProxy.cs:81 | Mock / switch-to-real proxy (dummy mock data) |  | TODO: dummy data |
| 91 | API/CertificateOfOrigins.BL/Proxies/Documents/DocumentsMockProxy.cs:83 | Mock / switch-to-real proxy (dummy mock data) |  | TODO: dummy data |
| 92 | API/CertificateOfOrigins.BL/Proxies/Documents/DocumentsMockProxy.cs:84 | Mock / switch-to-real proxy (dummy mock data) |  | TODO: dummy data |
| 93 | API/CertificateOfOrigins.BL/Proxies/Documents/DocumentsProxy.cs:18 | confirm endpoint | YES | TODO(blocking): confirm endpoint name/route with the Documents microservice |
| 94 | API/CertificateOfOrigins.BL/Proxies/Documents/DocumentsProxy.cs:29 | confirm endpoint | YES | TODO(blocking): confirm endpoint name/route with the Documents microservice |
| 95 | API/CertificateOfOrigins.BL/Proxies/Documents/DocumentsProxy.cs:40 | confirm endpoint | YES | TODO(blocking): confirm endpoint name/route with the Documents microservice |
| 96 | API/CertificateOfOrigins.BL/Proxies/Documents/DocumentsProxy.cs:50 | confirm endpoint | YES | TODO(blocking): confirm endpoint name/route with the Documents microservice |
| 97 | API/CertificateOfOrigins.BL/Proxies/ExportDealFile/ExportDealFileMockProxy.cs:22 | Mock / switch-to-real proxy (dummy mock data) |  | TODO: dummy data |
| 98 | API/CertificateOfOrigins.BL/Proxies/ExportDealFile/ExportDealFileMockProxy.cs:23 | Mock / switch-to-real proxy (dummy mock data) |  | TODO: dummy data |
| 99 | API/CertificateOfOrigins.BL/Proxies/ExportDealFile/ExportDealFileMockProxy.cs:24 | Mock / switch-to-real proxy (dummy mock data) |  | TODO: dummy data |
| 100 | API/CertificateOfOrigins.BL/Proxies/ExportDealFile/ExportDealFileMockProxy.cs:29 | Mock / switch-to-real proxy (dummy mock data) |  | TODO: dummy data |
| 101 | API/CertificateOfOrigins.BL/Proxies/ExportDealFile/ExportDealFileMockProxy.cs:42 | Mock / switch-to-real proxy (dummy mock data) |  | TODO: dummy data |
| 102 | API/CertificateOfOrigins.BL/Proxies/ExportDealFile/ExportDealFileMockProxy.cs:55 | Mock / switch-to-real proxy (dummy mock data) |  | TODO: dummy data |
| 103 | API/CertificateOfOrigins.BL/Proxies/ExportDealFile/ExportDealFileMockProxy.cs:71 | Mock / switch-to-real proxy (dummy mock data) |  | TODO: dummy data |
| 104 | API/CertificateOfOrigins.BL/Proxies/ExportDealFile/ExportDealFileMockProxy.cs:72 | Mock / switch-to-real proxy (dummy mock data) |  | TODO: dummy data |
| 105 | API/CertificateOfOrigins.BL/Proxies/ExportDealFile/ExportDealFileMockProxy.cs:93 | Mock / switch-to-real proxy (dummy mock data) |  | TODO: dummy data — matches the mock certificate invoice number |
| 106 | API/CertificateOfOrigins.BL/Proxies/ExportDealFile/ExportDealFileMockProxy.cs:96 | Mock / switch-to-real proxy (dummy mock data) |  | TODO: dummy data |
| 107 | API/CertificateOfOrigins.BL/Proxies/ExportDealFile/ExportDealFileMockProxy.cs:104 | Mock / switch-to-real proxy (dummy mock data) |  | TODO: dummy data |
| 108 | API/CertificateOfOrigins.BL/Proxies/ExportDealFile/ExportDealFileMockProxy.cs:105 | Mock / switch-to-real proxy (dummy mock data) |  | TODO: dummy data |
| 109 | API/CertificateOfOrigins.BL/Proxies/ExportDealFile/ExportDealFileMockProxy.cs:106 | Mock / switch-to-real proxy (dummy mock data) |  | TODO: dummy data |
| 110 | API/CertificateOfOrigins.BL/Proxies/ExportDealFile/ExportDealFileMockProxy.cs:107 | Mock / switch-to-real proxy (dummy mock data) |  | TODO: dummy data |
| 111 | API/CertificateOfOrigins.BL/Proxies/ExportDealFile/ExportDealFileMockProxy.cs:124 | Mock / switch-to-real proxy (dummy mock data) |  | TODO: dummy data |
| 112 | API/CertificateOfOrigins.BL/Proxies/ExportDealFile/ExportDealFileProxy.cs:14 | confirm endpoint | YES | TODO(blocking): the ExportDealFile microservice is not yet stood up — confirm the endpoint name/route |
| 113 | API/CertificateOfOrigins.BL/Proxies/ExportDealFile/ExportDealFileProxy.cs:39 | confirm endpoint | YES | TODO(blocking): confirm endpoint name/route with the DealFile microservice |
| 114 | API/CertificateOfOrigins.BL/Proxies/ExportDealFile/ExportDealFileProxy.cs:48 | confirm endpoint | YES | TODO(blocking): confirm endpoint name/route with the DealFile microservice |
| 115 | API/CertificateOfOrigins.BL/Proxies/ExportDealFile/ExportDealFileProxy.cs:57 | confirm endpoint | YES | TODO(blocking): confirm endpoint name/route with the DealFile microservice |
| 116 | API/CertificateOfOrigins.BL/Proxies/ExportDealFile/ExportDealFileProxy.cs:66 | confirm endpoint | YES | TODO(blocking): confirm endpoint name/route with the DealFile microservice |
| 117 | API/CertificateOfOrigins.BL/Proxies/ExportDealFile/ExportDealFileProxy.cs:74 | confirm endpoint | YES | TODO(blocking): confirm endpoint name/route with the DealFile microservice |
| 118 | API/CertificateOfOrigins.BL/Proxies/ExportDealFile/ExportDealFileProxy.cs:83 | confirm endpoint | YES | TODO(blocking): confirm endpoint name/route with the DealFile microservice |
| 119 | API/CertificateOfOrigins.BL/Proxies/InternationalSite/InternationalSiteMockProxy.cs:20 | Mock / switch-to-real proxy (dummy mock data) |  | TODO: dummy data |
| 120 | API/CertificateOfOrigins.BL/Proxies/InternationalSite/InternationalSiteMockProxy.cs:22 | Mock / switch-to-real proxy (dummy mock data) |  | TODO: dummy data |
| 121 | API/CertificateOfOrigins.BL/Proxies/InternationalSite/InternationalSiteProxy.cs:18 | confirm endpoint | YES | TODO(blocking): confirm endpoint name/route with the SystemTables microservice |
| 122 | API/CertificateOfOrigins.BL/Proxies/MeasurementUnit/MeasurementUnitMockProxy.cs:20 | Mock / switch-to-real proxy (dummy mock data) |  | TODO: dummy data |
| 123 | API/CertificateOfOrigins.BL/Proxies/MeasurementUnit/MeasurementUnitMockProxy.cs:22 | Mock / switch-to-real proxy (dummy mock data) |  | TODO: dummy data |
| 124 | API/CertificateOfOrigins.BL/Proxies/MeasurementUnit/MeasurementUnitProxy.cs:18 | confirm endpoint | YES | TODO(blocking): confirm endpoint name/route with the SystemTables microservice |
| 125 | API/CertificateOfOrigins.BL/Proxies/MessageManagement/MessageManagementProxy.cs:18 | confirm endpoint | YES | TODO(blocking): confirm endpoint name/route with the Message-Management (Common) microservice |
| 126 | API/CertificateOfOrigins.BL/Proxies/OrganizationUnit/OrganizationUnitProxy.cs:13 | confirm endpoint | YES | TODO(blocking): confirm the owning microservice (org-unit / SystemTables) + endpoint route |
| 127 | API/CertificateOfOrigins.BL/Proxies/PackingType/PackingTypeMockProxy.cs:20 | Mock / switch-to-real proxy (dummy mock data) |  | TODO: dummy data |
| 128 | API/CertificateOfOrigins.BL/Proxies/PackingType/PackingTypeMockProxy.cs:22 | Mock / switch-to-real proxy (dummy mock data) |  | TODO: dummy data |
| 129 | API/CertificateOfOrigins.BL/Proxies/PackingType/PackingTypeProxy.cs:18 | confirm endpoint | YES | TODO(blocking): confirm endpoint name/route with the SystemTables microservice |
| 130 | API/CertificateOfOrigins.BL/Proxies/Site/SiteMockProxy.cs:21 | Mock / switch-to-real proxy (dummy mock data) |  | TODO: dummy data |
| 131 | API/CertificateOfOrigins.BL/Proxies/Site/SiteMockProxy.cs:23 | Mock / switch-to-real proxy (dummy mock data) |  | TODO: dummy data |
| 132 | API/CertificateOfOrigins.BL/Proxies/Site/SiteMockProxy.cs:24 | Mock / switch-to-real proxy (dummy mock data) |  | TODO: dummy data |
| 133 | API/CertificateOfOrigins.BL/Proxies/Site/SiteProxy.cs:19 | confirm endpoint | YES | TODO(blocking): confirm endpoint name/route with the SystemTables microservice |
| 134 | API/CertificateOfOrigins.BL/Proxies/Tasks/TasksMockProxy.cs:25 | Mock / switch-to-real proxy (dummy mock data) |  | TODO: dummy data |
| 135 | API/CertificateOfOrigins.BL/Proxies/Tasks/TasksMockProxy.cs:27 | Mock / switch-to-real proxy (dummy mock data) |  | TODO: dummy data |
| 136 | API/CertificateOfOrigins.BL/Proxies/Tasks/TasksMockProxy.cs:43 | Mock / switch-to-real proxy (dummy mock data) |  | TODO: dummy data |
| 137 | API/CertificateOfOrigins.BL/Proxies/Tasks/TasksProxy.cs:18 | confirm endpoint | YES | TODO(blocking): confirm endpoint name/route with the Tasks microservice |
| 138 | API/CertificateOfOrigins.BL/Proxies/Tasks/TasksProxy.cs:31 | confirm endpoint | YES | TODO(blocking): confirm endpoint name/route with the Tasks microservice |
| 139 | API/CertificateOfOrigins.BL/Proxies/Tasks/TasksProxy.cs:39 | confirm endpoint | YES | TODO(blocking): confirm endpoint name/route with the Tasks microservice. The real endpoint returns a |
| 140 | API/CertificateOfOrigins.BL/Proxies/User/UserMockProxy.cs:20 | Mock / switch-to-real proxy (dummy mock data) |  | TODO: dummy data |
| 141 | API/CertificateOfOrigins.BL/Proxies/User/UserMockProxy.cs:21 | Mock / switch-to-real proxy (dummy mock data) |  | TODO: dummy data |
| 142 | API/CertificateOfOrigins.BL/Proxies/User/UserMockProxy.cs:22 | Mock / switch-to-real proxy (dummy mock data) |  | TODO: dummy data (e.g. חיפה) |
| 143 | API/CertificateOfOrigins.BL/Proxies/User/UserProxy.cs:16 | confirm endpoint | YES | TODO(blocking): confirm endpoint name/route with the Users microservice |
| 144 | API/CertificateOfOrigins.BL/Proxies/Vendor/VendorMockProxy.cs:20 | Mock / switch-to-real proxy (dummy mock data) |  | TODO: dummy data |
| 145 | API/CertificateOfOrigins.BL/Proxies/Vendor/VendorMockProxy.cs:21 | Mock / switch-to-real proxy (dummy mock data) |  | TODO: dummy data |
| 146 | API/CertificateOfOrigins.BL/Proxies/Vendor/VendorProxy.cs:16 | confirm endpoint | YES | TODO(blocking): confirm endpoint name/route with the Vendors microservice |
| 147 | API/CertificateOfOrigins.BL/ServicesConfiguration.cs:33 | Mock / switch-to-real proxy | YES | TODO(blocking): verify the real Customers endpoint (CustomersByIds) before ROLLOUT. |
| 148 | API/CertificateOfOrigins.BL/ServicesConfiguration.cs:37 | Mock / switch-to-real proxy | YES | TODO(blocking): verify the real Vendors endpoint (VendorsByIds) before ROLLOUT. |
| 149 | API/CertificateOfOrigins.BL/ServicesConfiguration.cs:42 | Mock / switch-to-real proxy | YES | TODO(blocking): verify the real Users endpoint (User/UsersByIds) before ROLLOUT. |
| 150 | API/CertificateOfOrigins.BL/ServicesConfiguration.cs:45 | Mock / switch-to-real proxy | YES | TODO(blocking): the ExportDealFile microservice is not yet stood up — the mock is the practical |
| 151 | API/CertificateOfOrigins.BL/ServicesConfiguration.cs:50 | Mock / switch-to-real proxy | YES | TODO(blocking): verify the real SystemTables endpoint before ROLLOUT. |
| 152 | API/CertificateOfOrigins.BL/ServicesConfiguration.cs:54 | Mock / switch-to-real proxy | YES | TODO(blocking): verify the real SystemTables endpoint before ROLLOUT. |
| 153 | API/CertificateOfOrigins.BL/ServicesConfiguration.cs:60 | Mock / switch-to-real proxy | YES | TODO(blocking): verify the real SystemTables endpoints (Country/CountriesByAlphaCodes, Site/SitesByExternalNumbers, |
| 154 | API/CertificateOfOrigins.BL/ServicesConfiguration.cs:70 | Mock / switch-to-real proxy | YES | TODO(blocking): verify the real Documents endpoint (Document/DocumentsByEntity) before ROLLOUT. |
| 155 | API/CertificateOfOrigins.BL/ServicesConfiguration.cs:74 | Mock / switch-to-real proxy | YES | TODO(blocking): verify the real Collateral (Collateral/CollateralRequestByEntity) endpoint before ROLLOUT. |
| 156 | API/CertificateOfOrigins.BL/ServicesConfiguration.cs:77 | Mock / switch-to-real proxy | YES | TODO(blocking): verify the real Tasks (Task/IsTaskExist) endpoint before ROLLOUT. |
| 157 | API/CertificateOfOrigins.BL/ServicesConfiguration.cs:81 | Mock / switch-to-real proxy | YES | TODO(blocking): verify the real Message-Management (Message/SendMessage) endpoint before ROLLOUT. |
| 158 | API/CertificateOfOrigins.BL/ServicesConfiguration.cs:86 | Mock / switch-to-real proxy | YES | TODO(blocking): confirm each owning microservice + endpoint route before ROLLOUT. |
| 159 | API/CertificateOfOrigins.BL/ServicesConfiguration.cs:92 | Mock / switch-to-real proxy | YES | TODO(blocking): confirm the endpoint route before ROLLOUT. |
| 160 | API/CertificateOfOrigins.BL/ServicesConfiguration.cs:131 | TODO(infra)/no source | YES | TODO(blocking): GetPathsForNavigationToVendor needs a NavigationPath lookup. NavigationPath is a shared |
| 161 | API/CertificateOfOrigins.DAL/CertificateOfOriginsDal.cs:1007 | other (cross-reference to a blocking TODO elsewhere) | ref | TODO(blocking))", which was stale — only the proxy's endpoint route is still a rollout TODO.) |
| 162 | API/CertificateOfOrigins.Model/ModelDTOs/CertificateOfOriginWebInvoiceDetailDto.cs:5 | other (cross-reference to a blocking TODO elsewhere) | ref | TODO(blocking)). |
| 163 | API/CertificateOfOrigins.Model/ModelDTOs/CollateralRequestDto.cs:4 | TODO(blocking) — confirm DTO contract | YES | TODO(blocking): confirm the Collateral service's response |
| 164 | API/CertificateOfOrigins.Model/ModelDTOs/ECertificateOfOriginsTemplate.cs:11 | TODO(confirm) enum/event/process |  | TODO(confirm): the numeric value. The legacy keyed these off the shared ETemplate enum, which lives only in a |
| 165 | API/CertificateOfOrigins.Model/ModelDTOs/GetAuthenticationRequestByIdResultDto.cs:60 | other (cross-reference to a blocking TODO elsewhere) | ref | TODO(blocking). |
| 166 | API/CertificateOfOrigins.Model/ModelDTOs/GetAuthenticationRequestByIdResultDto.cs:64 | other (cross-reference to a blocking TODO elsewhere) | ref | TODO(blocking). |
| 167 | API/CertificateOfOrigins.Model/ModelDTOs/UserDto.cs:11 | TODO(blocking) — confirm DTO contract | YES | TODO(blocking): confirm the field name exposed by the Users microservice (User/UsersByIds). |
| 168 | API/CertificateOfOrigins.WebApi/Program.cs:20 | TODO(infra)/no source | YES | TODO(blocking): .AddValidationMessages<ValidationMessages>() — re-enable when the |
