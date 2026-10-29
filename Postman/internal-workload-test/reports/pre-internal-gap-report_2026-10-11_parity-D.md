# Parity re-check D: Incoming `GetCertificateRequestByGuid` (GET web/CertificateOfOrigins/RequestByGuid), 2026-10-11

Read-only adversarial re-review after the 2026-10-07 / 2026-10-11 fix rounds.

## 1. Scope and files read

| Side | File |
|---|---|
| Legacy wrapper | `C:\Repos\Main\CRM\CertificateOfOrigins\Common\Internal\Customs.CRM.CertificateOfOrigins.InternalCommon\CertificateOfOriginsIncomingMessageService.cs:141-202` |
| Legacy entry | `...\Server\Customs.CRM.CertificateOfOrigins.Service\CertificateOfOriginsIncomingMessageServiceForWeb.cs:17-32` |
| Legacy BL | `...\Server\Customs.CRM.CertificateOfOrigins.Service\CertificateOfOriginWebBL.cs:34-280` |
| Legacy serializer | `...\InternalCommon\Common\JSONSerializeHelper.cs:14-26` (DCJS, `DateTimeFormat("yyyy-MM-dd")`) |
| Legacy wire contract | `C:\Repos\Main\WebCommon\Customs.Inf.WebMultiDotNetSupport.CertificateOfOrigins\CertificateOfOrigins.cs:8-69` |
| Legacy SP | `CustumsDev_Database\...\CRM\Stored Procedures\usp_CertificateOfOrigin_GetCertificateOfOriginDataForWebQuery.sql` |
| New controller | `API\CertificateOfOrigins.WebApi\Controllers\Web\CertificateOfOriginsController.cs:15-36` + `Filters\ProxyFailureExceptionFilterAttribute.cs` |
| New BL | `API\CertificateOfOrigins.BL\CertificateOfOriginsBl.cs:383-696`, `LegacyHostCulture.cs`, `CertificateOfOriginsConsts.cs:13-58`, `ServicesConfiguration.cs:128-138`, `Lookups\DataDictionaryField.cs` |
| New DAL / materializer | `CertificateOfOriginsDal.cs:1177-1186`, `Model\DbContext\CertificateOfOriginsDbContextExtension.cs:178-226` |
| New DTOs | `CertificateOfOriginsRequestDto`, `CertificateOfOriginsResponseDto`, `FieldDataDto`, `CertificateOfOriginWebInvoiceDetailDto`, `CertificateOfOriginWebItemDetailDto`, `WebDateJsonConverter`, `CertificateOfOriginWebQueryDto`, `CertificateOfOriginWebDetailDto`, `CertificateOfOriginWebPrintOutDto`, `CertificateDetailsTypeCodeDto`, `DocumentDto` |
| New SP | `WebApi\Scripts\API_20260728122720 - dbo.GetCertificateOfOriginDataForWebQuery.sql` |
| New contract / tests | `WebApi\.spec\OpenApi\web-certificaterequest.openapi.json`, `Test\WebQueryResponseFormatTests.cs` |
| Prior state | 2026-09-27 appendix D, LEDGER rows D-F1..D-F8, X-1 |

## 2. Previous findings re-verification

| ID | Prior status | Verified now? | Evidence |
|---|---|---|---|
| D-F1 legacy wire names | FIXED | YES | `[JsonPropertyName]` on every member: `CertificateOfOriginsResponseDto.cs:15-31` (CertificateNumber, QueryURL, DocumentID, CertificateOfOriginDetails, CertificateOfOriginInvoiceDetails, ExeptionDescription), `FieldDataDto.cs:10-14`, `CertificateOfOriginWebInvoiceDetailDto.cs:12-30` (incl. `CertificateOfOriginItemDetailDTOs`), `CertificateOfOriginWebItemDetailDto.cs:10-23`. They match legacy `CertificateOfOrigins.cs:21-68` name for name and case for case. Test `TheWebResponseUsesTheLegacyWireNames` (WebQueryResponseFormatTests.cs:57-79). The envelope is not reproduced and nulls are omitted; both are recorded in the ledger. See DN-01 for the null-versus-empty-list remainder. |
| D-F2 dates as yyyy-MM-dd | FIXED | YES | The "Issuing Date" Value is `IssuingDate?.ToString("yyyy-MM-dd", Invariant)` (BL:561-565). InvoiceDate uses `[JsonConverter(WebDateJsonConverter)]` (InvoiceDetailDto:16-18, converter:20-23). No other DateTime goes on the wire; DateOfDeclaration is already a string. Test at :26-53. |
| D-F3 DateOfDeclaration culture | FIXED | YES | `TryParse(value, LegacyHostCulture.Culture, None)` (BL:627-628). The culture is he-IL cloned with `dd/MM/yyyy` and "/", falling back to invariant with the same pattern (LegacyHostCulture.cs:21-39). Output stays `"dd MMMM yyyy"` en-US as legacy (BL:633 vs legacy:175). Tests at :84-127. |
| D-F4 technical failure = 5xx | CLOSED-OK | YES (matches decision) | `[ProxyFailureExceptionFilter]` on the controller gives 502/503 (Controller:16, filter:15-31). Anything else gives a platform 500. The decision is recorded at Controller:25-28 and in openapi.json:5,24-26. The reverse cases (a legacy NRE now returns a silent null) are still kept: `GetValueOrDefault` (BL:467,547,554,569) and `?.` (BL:632,667-668). |
| D-F5 DocumentId via Documents proxy | OPEN-BLOCKED | YES (marker present) | `DocumentsProxy.cs:19` still carries `TODO(blocking)` on the route. The BL keeps the filter {329,461}, entity type 12319 and newest CreateDate, falling back to 0 (BL:431-441). |
| D-F6 empty guid binds to null | FIXED | YES | `[DisplayFormat(ConvertEmptyStringToNull = false)]` on Guid and Number (RequestDto:13-17). `""` fails `Guid.TryParse` and returns "Invalid Guid" (BL:392-395), matching legacy:37-42. An empty number reaches the SP as `''` and finds no match, as legacy DCJS did. |
| D-F7 DEV QueryURL seed | NOT-A-DEFECT-DOC | YES | Key unchanged: `parametersUtil.Get<string>("CertificateOfOriginQueryURL")` + `string.Format` (BL:692-693). The value is per environment, as recorded. |
| D-F8 [NotFoundResponse] | FIXED | YES | Controller:30 has only `[BadRequestResponse][OkJsonResponse]`. The BL never throws a 404. |
| X-1 (active-state lookups, this path) | FIXED | YES | CurrencyType and DataDictionaryField now go through `lookupUtil.Search(... State == ActiveState)` (BL:495, 678). This mirrors legacy `GetCodeById` returning active rows only (Consts:13-16). |

## 3. Fresh statement-by-statement review: result

Every legacy statement in `GetCertificateDetailForWeb`, `ConstructWebResponse`, `GetCertificateOfOriginInvoiceDetails`, `GetCertificateOfOriginDetails`, `GetQueryURL` and the materializer has a matching new statement:
- **Guard and parameters.** Same parameters, same null and DBNull handling, `.Date` on IssuingDate.
- **Not found.** "No Matching Certificate" on both sides.
- **Invoice filter.** Same precedence. `IsToPrint` gates only the MERCOSUR branch; checked operator by operator.
- **Invoice fields.** `InvoiceNumber` uses the same IsToPrint/empty-string rule. The CurrencyCode lookup is guarded by `HasValue`. The item list is always empty.
- **Header field order.** Retrospective (20310), then Replacing (20306), then Issuing Date (always present), then ExportDec (20661).
- **TryParse quirks.** The gates still pass on TryParse success alone, regardless of the parsed value.
- **Detail switch.** All 23 print-out types are covered, plus DateOfDeclaration and the consignee rule with the always-false IsToPrint for EUR1/EURMED.
- **Materializer.** Result set 4 and result set 5 attach by type id using the first match (GroupBy+First keeps the original order).
- **SP.** The body is identical apart from the removed Docs JOIN (`@DocumentID = NULL`, enriched in the BL) and the dead temp table. The SP has no other column returned as CAST(NULL), and Dapper's case-insensitive matching maps `TypeID`, `GUID` and `CertificateIDToCancel` correctly.
- **Side effects.** None on either side.

No HIGH or MED deviations found.

## 4. New findings

| ID | Severity | Operation | Deviation | New file:line | Legacy file:line | Why it matters (input → output) |
|---|---|---|---|---|---|---|
| DN-01 | LOW | GetCertificateRequestByGuid, in-band error responses | In the "Invalid Guid" and "No Matching Certificate" responses the two lists are initialized to `[]`. Legacy left them null, and DCJS wrote them as `null`. The ledger's D-F1 note covers only omitted null members, not this case. | `CertificateOfOriginsResponseDto.cs:25,28`; BL:394,405 | `CertificateOfOriginWebBL.cs:41,63`; `CertificateOfOrigins.cs:27,29` (no initializer) | `?CertificateOfOriginGuid=abc`: legacy wrote `"CertificateOfOriginDetails":null,"CertificateOfOriginInvoiceDetails":null`. New writes `[]` for both. A portal adapter that detects an error by `Details == null` instead of by ExeptionDescription would now treat the error as an empty success. |
| DN-02 | LOW | wire bytes (not semantics) | Member order and string escaping differ. Legacy DCJS serialized these POCO types alphabetically (base members first: CertificateNumber, CertificateOfOriginDetails, CertificateOfOriginInvoiceDetails, DocumentID, ExeptionDescription, QueryURL; in an invoice, CertificateOfOriginItemDetailDTOs comes first). It also escaped `/` as `\/` (`QueryURL` became `http:\/\/...`) and wrote non-ASCII text raw. System.Text.Json uses declaration order, does not escape `/`, and by default escapes non-ASCII text (Hebrew DisplayedValue) and `<>&'+` as `\uXXXX`, unless the platform configures a relaxed encoder, which I could not verify from this repo. | DTO declaration order; platform JSON options | `JSONSerializeHelper.cs:16-25`; `CertificateOfOrigins.cs` | The JSON is equivalent for any parser. It matters only to a consumer that compares or slices the raw string. Not byte-for-byte identical. |
| DN-03 | LOW | malformed IssuingDate | `[ApiController]` + `[FromQuery] DateTime?`: an unparseable `issuingDate` returns an automatic HTTP 400 ProblemDetails before the BL runs. Legacy DCJS failed to deserialize, and the wrapper answered in-band. The binder also accepts formats other than `yyyy-MM-dd` (invariant culture, so `03/01/2026` is read as March 1). | Controller:29-31; openapi.json:23 (400 declared) | `ForWeb.cs:19` + wrapper:186-190 | `?issuingDate=2026-13-01`: legacy returned an in-band error and the new code returns 400. This is consistent with the D-F4 spirit and with the declared contract; recorded so the portal team knows. |
| DN-04 | LOW (doc) | contract and comments stale after the proxy-to-local-lookup change (commit 0bc353c) | openapi.json:25 and Controller:26 still say that a DataDictionaryField or CurrencyType failure is a 502/503 from the proxy filter. Both are now local lookups (`AddLocalLookup`, ServicesConfiguration.cs:130,138; BL:495,678) and do not raise `ProxyExecutionException`, so a lookup-load failure becomes a platform 500, or an empty set that yields a null label or code. `CertificateOfOriginWebInvoiceDetailDto.cs:6-7` still says CurrencyCode is "deferred (TODO(blocking))", and MIGRATION-STATUS.md:183 / MIGRATION-NOT-DONE.md:201-218 still describe SystemTables proxies. | as listed | n/a | Only the declared contract and the docs are misleading. Runtime behaviour is the decided one. |
| DN-05 | LOW (marked) | header labels 20306/20310/20661 | Labels come from the local `Lookups.DataDictionaryField` loaded from SystemTables, marked `TODO(internal)` (BL:673-675, Lookups\DataDictionaryField.cs:5-8). If the source does not serve the type, the three rows lose their `Label`: null, and therefore omitted on the wire. Legacy threw an NRE and returned an in-band error in that case. | BL:543-569, 676-681 | legacy:127-149 | Marked TODO(internal), not TODO(blocking). Listed so the ledger tracks it until the platform lookup type exists. |

## 5. Counts
- Previous: 8 D items + X-1 rechecked. 9 verified, 0 regressed, 0 partial.
- New: 0 HIGH, 0 MED, 5 LOW (DN-01..DN-05, two of them doc/marked).
