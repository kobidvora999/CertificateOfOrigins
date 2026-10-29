# Parity review D: Incoming `GetCertificateRequestByGuid` (web query)

Reviewer: adversarial parity pass, read-only. I did not consult the Postman reports or the MIGRATION-* docs to reach verdicts. I checked MIGRATION-NOT-DONE.md only at the end, to fill in the "already documented" tags.

## Sources compared

| Side | File |
|---|---|
| Legacy wrapper | `C:\Repos\Main\CRM\CertificateOfOrigins\Common\Internal\Customs.CRM.CertificateOfOrigins.InternalCommon\CertificateOfOriginsIncomingMessageService.cs:141-202` |
| Legacy entry | `C:\Repos\Main\CRM\CertificateOfOrigins\Server\Customs.CRM.CertificateOfOrigins.Service\CertificateOfOriginsIncomingMessageServiceForWeb.cs:17-32` |
| Legacy BL | `C:\Repos\Main\CRM\CertificateOfOrigins\Server\Customs.CRM.CertificateOfOrigins.Service\CertificateOfOriginWebBL.cs:34-280` |
| Legacy serializer | `C:\Repos\Main\CRM\CertificateOfOrigins\Common\Internal\Customs.CRM.CertificateOfOrigins.InternalCommon\Common\JSONSerializeHelper.cs` (DataContractJsonSerializer, `DateTimeFormat("yyyy-MM-dd")`) |
| Legacy wire contract | `C:\Repos\Main\WebCommon\Customs.Inf.WebMultiDotNetSupport.CertificateOfOrigins\CertificateOfOrigins.cs` |
| Legacy SP | `C:\Repos\Main\CustumsDev_Database\CustumsDev_Database\CRM\Stored Procedures\usp_CertificateOfOrigin_GetCertificateOfOriginDataForWebQuery.sql` |
| New controller | `C:\Repos\CertificateOfOrigins\API\CertificateOfOrigins.WebApi\Controllers\Web\CertificateOfOriginsController.cs:23-29` |
| New BL | `C:\Repos\CertificateOfOrigins\API\CertificateOfOrigins.BL\CertificateOfOriginsBl.cs:320-637` |
| New DAL / materializer | `CertificateOfOrigins.DAL\CertificateOfOriginsDal.cs:1001-1010`, `CertificateOfOrigins.Model\DbContext\CertificateOfOriginsDbContextExtension.cs:184-227` |
| New SP | `CertificateOfOrigins.WebApi\Scripts\API_20260728122720 - dbo.GetCertificateOfOriginDataForWebQuery.sql` |
| New contract | `CertificateOfOrigins.WebApi\.spec\OpenApi\web-certificaterequest.openapi.json` |

## (1) Operation table

| Operation | Endpoint | DTOs | BL | DAL | Verdict |
|---|---|---|---|---|---|
| GetCertificateRequestByGuid (+ Sync) — GetPC_Web_9096_CertificateRequest | `GET web/CertificateOfOrigins/RequestByGuid` `[FromQuery]` | `CertificateOfOriginsRequestDto` → `CertificateOfOriginsResponseDto` (`FieldDataDto`, `CertificateOfOriginWebInvoiceDetailDto`, `CertificateOfOriginWebItemDetailDto`); internal `CertificateOfOriginWebQueryDto`, `CertificateOfOriginWebDetailDto`, `CertificateOfOriginWebPrintOutDto`, `CertificateOfOriginInvoiceDetailDto`, `CertificateDetailsTypeCodeDto` | `CertificateOfOriginsBl.GetCertificateRequestByGuid` → `ConstructWebResponse`, `ResolveWebQueryDocumentId`, `GetCertificateOfOriginInvoiceDetails`, `IsInvoiceIncluded`, `BuildHeaderFields`, `MapDetailField`, `GetQueryUrl` | `ReadOnlyContext.GetCertificateOfOriginDataForWebQuery` (Dapper QueryMultiple, 5 sets) | **PASS WITH DEVIATIONS.** The business logic matches statement by statement. The deviations are in the wire format (F1, F2), in date parsing culture (F3), and in error handling (F4). |

### Statement-level checks

| Legacy step | New | Status |
|---|---|---|
| Guid not null and `!Guid.TryParse` → `ExeptionDescription="Invalid Guid"` (BL:37-43) | BL:329-332 | Same. Edge case: an empty-string guid differs, see F6. |
| 3 SqlParameters: Guid (UniqueIdentifier/DBNull), Number (VarChar/DBNull), IssuingDate (.Date/DBNull) (BL:45-50) | BL:334-337 (DbType.Guid/String/Date, nulls) | Same. The SP param is NVARCHAR(35), so switching VarChar to String is harmless. |
| Result null → `"No Matching Certificate"` (BL:61-64) | BL:340-343 | Same. The materializer returns null when result set 1 is empty (legacy `SingleOrDefault`, new `FirstOrDefault`). The SP selects `WHERE ID=@CertificateOfOriginID`, so at most 1 row comes back and the two behave the same. |
| `DocumentID = cert.DocumentID` (from the SP) | `ResolveWebQueryDocumentId` via the Documents proxy, types {329,461}, newest by CreateDate, else 0 | Enriched. Filter matches, EntityType 12319 matches. See F5. |
| `CertificateNumber` | same | Same |
| `QueryURL = string.Format(GetConfig("CertificateOfOriginQueryURL"), guid)` or `""` | `parametersUtil.Get<string>` same key, same format, `string.Empty` | Same. The config literal is not replaced. |
| Invoice filter `(IsrCol \|\| MERCOSUR && IsToPrint) \|\| (EURMED \|\| EUR1)` | `IsInvoiceIncluded` with explicit parentheses | Same precedence. Operator check passed. |
| InvoiceNumber = IsToPrint ? number : "" ; Amount, Date, GoodsDescription | same | Same |
| CurrencyCode = `GetCodeById<CurrencyType>(id).CurrencyCode` when HasValue | batched `ICurrencyTypeProxy`, `GetValueOrDefault` | Same value. A missing code gives null here, where legacy threw an NRE (F4). |
| ItemDetailDTOs = empty list | `[]` | Same |
| Retrospective (RequestReasonCode==2) → label(FieldID 20310), "Issued Retrospectively" | same, FieldId const 20310 | Same. I verified the FieldIDs 20306/20310/20661 against `CertificateOfOrigin.cs:527/728/761` in the EF4 model. |
| CertificateIDToCancel.HasValue → label(20306), "Replacing certificate {id}" | same | Same |
| Always add `{ "Issuing Date", IssuingDate }` | same, `IssuingDateLabel="Issuing Date"` | Same value. The serialized format differs (F2). |
| ExportDecNumber non-blank && any detail IsExportDecForPrint(26) with `bool.TryParse` succeeding → label(20661) | same | Same. The legacy quirk that TryParse success, not the parsed value, is the gate is preserved on both sides. |
| Detail switch: 23 print-out types → print-out label/value | same 23 cases, one merged case list | Same set. I diffed the enums: ECertificateDetailsType, ERequestReason and ECertificateOfOriginType are identical, apart from an added `Vietnam=9` that nothing on this path uses. |
| DateOfDeclaration: non-blank && `DateTime.TryParse(value)` → `"dd MMMM yyyy"` en-US | `TryParse(value, CurrentCulture, None)` | Same logic. The culture environment differs (F3). |
| Consignee name/address/country: any IsConsigneeForPrint(12) with TryParse succeeding → EUR1/EURMED only if the print-out IsToPrint (always false), else print | `MapConsigneeField` | Same, bug-for-bug. |
| Materializer: type-code and print-out matched to each detail with FirstOrDefault by type id | GroupBy + First (keeps the original order) | Same |
| Wrapper: exception → `HandleMessageException` returns the error in-band inside `WebInternalServiceOutResponse` | none; an exception propagates to the platform's error pipeline | Deviation (F4) |
| Serialize with DCJS: PascalCase, dates as `yyyy-MM-dd`, base fields Status/TechnicalInformation/SevriceInstanceID/CallID | System.Text.Json: camelCase, ISO-8601 datetime, renamed members | Deviation (F1, F2) |

Side effects: none on either side. Legacy is a read-only SP plus lookups, and so is the new code. No writes, events or PushUtil calls were dropped.

### Column mapping (SP → DTO → response)

The SP body is identical between legacy and new except for the `@DocumentID` lookup and the dead temp table `#CertificateDetailsTypeCodeForWebDisplay`, which the new SP drops.

**Result set 1: header**

| SP column | Legacy source | New source | Enrichment | Status |
|---|---|---|---|---|
| ID | COOCOO.ID | same → `Id` | used as the Documents-proxy entity id | OK |
| TypeID | same | → `TypeId` | invoice filter, consignee rule | OK |
| Title, State, TimeStamp, CreateDate, CreateUserID, UpdateDate, UpdateUserID, OrganizationUnitID, CustomerID, CreateCustomerID, UpdateCustomerID, LeadDocumentID, CertificateNumber*, CertificateOfOriginStatusID, DestinationCountry, FeedbackRemark, InternalApplication, RejectCancelReason, ReplacementReason, CertificateToReplaceInImport, QRCodePath | same | same columns; Dapper ignores the undeclared ones | none. Legacy BL did not read them either. | OK (not consumed by either side) |
| CertificateNumber | same | → `CertificateNumber` | response.CertificateNumber | OK |
| CertificateIDToCancel | same | → `CertificateIdToCancel` | header field | OK |
| IssuingDate | same | → `IssuingDate` (DateTime?) | header field "Issuing Date" | OK (format: F2) |
| RequestReasonCode | same (int NOT NULL) | → `int RequestReasonCode` | retrospective field | OK |
| ExportDeclarationNumber | same | → `ExportDeclarationNumber` | header field | OK |
| GUID | same | → `Guid` | QueryUrl | OK |
| DocumentID | TOP 1 Docs_EntityDocument ⋈ Docs_Document, TypeID IN (329,461), EntityTypeID 12319, ORDER BY CreateDate DESC | `@DocumentID = NULL` → `DocumentId` (not read) | **BL enriched**: `ResolveWebQueryDocumentId` via `IDocumentsProxy.GetDocumentsByEntity(id, 12319)`, filtered to {329,461}, newest CreateDate | OK. Enriched, not a silent NULL. The proxy route is a TODO(blocking) (F5). |

**Result set 2: invoices.** The columns are ID, CertificateOfOriginID, CurrencyTypeID, InvoiceAmount, InvoiceDate, InvoiceGoodsDescription, InvoiceNumber and IsToPrint. They are identical on both sides and map to `CertificateOfOriginInvoiceDetailDto`, with CLR types matching the EF4 entity. CurrencyTypeID is enriched through `ICurrencyTypeProxy`. Status: OK.

**Result set 3: details.** ID, CertificateOfOriginID, CertificateDetailsTypeCodeID, Value and DisplayedValue are identical and map to `CertificateOfOriginWebDetailDto`. Status: OK.

**Result set 4: type codes.** ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate, Comment and DetailTypeFormat are identical. The DTO also declares `DataTypeId`, which the SP never returns, so it stays 0. Nothing on this path reads it. Status: OK.

**Result set 5: print-out.** CertificateDetailsTypeID, CertificateDetailsTypeEnglishName and CertificateDetailsTypeValue (= DisplayedValue) are identical. `CertificateDetailsTypeIsToPrint` is not returned on either side, so it is always false. This is a preserved quirk. Status: OK.

**No silent NULLs found.** The only `NULL` the new SP returns is DocumentID, and the BL enriches it.

## (2) Findings

| ID | Severity | Class | Legacy file:line | New file:line | Description | Marked TODO | Verdict |
|---|---|---|---|---|---|---|---|
| F1 | MEDIUM | returned fields (wire contract) | `WebMultiDotNetSupport.CertificateOfOrigins\CertificateOfOrigins.cs:19-58`; `CertificateOfOriginsIncomingMessageServiceForWeb.cs:29` (DCJS, PascalCase) | `ModelDTOs\CertificateOfOriginsResponseDto.cs:9-19`; `CertificateOfOriginWebInvoiceDetailDto.cs:19`; openapi.json:42-66 | Several response members changed name, and the base envelope was dropped. `ExeptionDescription` (legacy typo) became `exceptionDescription`, a different spelling. A consumer that checks `ExeptionDescription` to detect "Invalid Guid" or "No Matching Certificate" will read a successful empty response instead. `QueryURL` became `queryUrl` and `DocumentID` became `documentId`, which differ only by case and break only case-sensitive JS/TS consumers. The field `CertificateOfOriginItemDetailDTOs` became `certificateOfOriginItemDetails` (always empty). The BaseWebResponse fields `Status`, `TechnicalInformation`, `SevriceInstanceID` and `CallID` are gone. The transport changed too: an ESB `WebInternalServiceInRequest` with a JSON Content string became a direct REST GET, so the portal must be rewired regardless. The spelling change is still a silent semantic break for any adapter that maps fields by name. | No | CONFIRMED (the rename); impact PLAUSIBLE, since it depends on the portal adapter |
| F2 | MEDIUM | returned fields (value format) | `InternalCommon\Common\JSONSerializeHelper.cs:16-21` (`DateTimeFormat("yyyy-MM-dd")`); `CertificateOfOriginWebBL.cs:142,104` | `CertificateOfOriginsBl.cs:509,397` + platform System.Text.Json | Legacy serialized every DateTime as a date-only string `yyyy-MM-dd`: the header "Issuing Date" value (an object holding a DateTime) and each invoice `InvoiceDate`. The new code emits ISO-8601 date-time, for example `2026-03-01T14:22:05.123`. IssuingDate is stamped with `DateTime.Now` at issue (legacy `CertificateOfOriginsBL.cs:383`), so the portal will now show a time part it never received before. No date-only projection happens in the BL. | No | CONFIRMED |
| F3 | MEDIUM | operators / flag source (culture-dependent parse) | `CertificateOfOriginWebBL.cs:170-171` (`DateTime.TryParse(value)` under the WCF host culture, he-IL); legacy writer `CertificateOfOriginsIncomingMessageServicePartial.cs:694-707` stores the raw message value and `ToShortDateString()` in DisplayedValue | `CertificateOfOriginsBl.cs:560-561` (`CultureInfo.CurrentCulture` of the .NET 10 container) | The DateOfDeclaration field is emitted only when `Value` parses as a date. Legacy parsed under the Windows/IIS he-IL culture (dd/MM). The new service parses under whatever culture the container runs with, usually invariant or en-US (MM/dd), and nothing in the repo sets a culture: I found no RequestLocalization, DefaultThreadCurrentCulture or InvariantGlobalization. For certificates stored by the legacy system with a culture-formatted value, dates with a day above 12 fail to parse and the field silently disappears from the response. Dates with day ≤ 12 flip day and month, so "05/09/2025" is shown as "09 May 2025". Values the new code writes use `"o"` (invariant, `MessageValidation.cs:431-434`) and are safe. | No | PLAUSIBLE (depends on the stored-value format and the container culture) |
| F4 | MEDIUM | exceptions / HTTP codes | `CertificateOfOriginsIncomingMessageService.cs:145-157, 186-199` (catch → `HandleMessageException`, error carried in-band in the response header) | `CertificateOfOriginsController.cs:23-29`; `CertificateOfOriginsBl.cs:370-376, 433-447, 607-621, 633-634` | The new code calls three remote services that legacy did not need, because legacy used the SP join and the in-process SystemTablesUtil: Documents, SystemTables DataDictionaryField and SystemTables CurrencyType. Any proxy failure, or a missing `CertificateOfOriginQueryURL` (`string.Format(null)` throws ArgumentNullException), now surfaces as an HTTP error from the platform pipeline. Legacy never produced an HTTP error; every failure went into the in-band response header. The code documents the in-band contract as "the portal depends on it", but it covers only the two business errors. There is also a reverse deviation: where legacy threw an NRE and returned an in-band error (`GetCodeById` returning null, or a null print-out DTO at `CertificateOfOriginWebBL.cs:128,165`), the new code returns a null label or value silently (`GetValueOrDefault`, `?.`). | Proxy routes: yes, `TODO(blocking)` in the proxies. Error-model change: no | CONFIRMED |
| F5 | LOW | flag source of truth (DocumentId) | legacy SP:56-62 (direct `Docs_EntityDocument`/`Docs_Document`, no State filter) | `CertificateOfOriginsBl.cs:368-378`; `Proxies\Documents\DocumentsProxy.cs:14-21` | DocumentId now comes from the Documents service. The route `api/Document/DocumentsByEntity/{id}/{type}` is unconfirmed (`TODO(blocking)`). The legacy SP read the raw tables with no soft-delete filter, while the Documents service probably excludes deleted or detached documents, so the chosen document may differ in edge cases. Ties on CreateDate may also order differently from the SQL `TOP 1`. `DocumentsMockProxy` returns type 329 by default, so a mock-mode run always yields a non-zero id. | Yes (route) | PLAUSIBLE |
| F6 | LOW | dropped guard (edge input) | `CertificateOfOriginWebBL.cs:37-42` (DCJS keeps `""` as a non-null string, so it fails TryParse and returns "Invalid Guid") | `CertificateOfOriginsBl.cs:329`; `[FromQuery]` binding | ASP.NET model binding converts an empty query value (`?certificateOfOriginGuid=`) to null. The request therefore falls through to the number + date branch and returns "No Matching Certificate" instead of "Invalid Guid". | No | PLAUSIBLE (default ConvertEmptyStringToNull behavior) |
| F7 | LOW | config / environment | legacy `Configuration.GetConfig` (per-environment config) | `Scripts\API_20260716 - add params.sql:186-202` | `CertificateOfOriginQueryURL` is seeded with the DEV URL `http://10.25.218.28/ShaarOlami2/Dev/CertificateOfOrigin?guid={0}` and is insert-if-missing only. Unless each environment overrides it, PreProd and Prod will hand out DEV verification links. | No | PLAUSIBLE |
| F8 | LOW | exceptions / HTTP codes (documentation) | n/a | `CertificateOfOriginsController.cs:24` | The action is decorated `[NotFoundResponse]`, while the hand-written contract and `WebCertificateRequestContractTests` insist that no 404 exists. The BL never throws, so the only harm is misleading generated Swagger. | No | CONFIRMED (cosmetic) |

### Grep categories in scope
- PushUtil: none on either side.
- Hardcoded `=1` user context: none.
- Mock proxies: `DocumentsMockProxy`, `DataDictionaryFieldMockProxy` and `CurrencyTypeMockProxy` are registered as mock alternates, selected only by the `x-mock-mode` header. The real proxies are the default. Not a deviation, but F5 applies in mock mode.
- `TODO(confirm)` / "out of scope": none in the BL region 320-637. `TODO(blocking)` appears only on the proxy routes.

### Already documented (checked against MIGRATION-NOT-DONE.md at the end)
- F1: no
- F2: no
- F3: no
- F4: partially. Only the unconfirmed proxy routes are listed; the error-model change is not.
- F5: yes for the route (decision #4 plus the TODO(blocking)). The deleted-document filtering difference is not listed.
- F6, F7, F8: no

Everything the doc lists as a bug-for-bug quirk checked out as faithful: result set 5 has no IsToPrint, the operator precedence is preserved, the ItemDetail list is always empty, and the FieldIDs are correct.
