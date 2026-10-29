# Parity re-check C: Incoming `GetPC_MSG2280_2281_CertificateOfOriginRequest` (2026-10-11)

Scope: `POST community/CertificateOfOrigins/Request`. This is a re-check after the 2026-09-27 fix round, followed by a fresh statement-by-statement pass. The review was read-only.

Abbreviations:
- **LP**: legacy `Server\Customs.CRM.CertificateOfOrigins.Service\CertificateOfOriginsIncomingMessageServicePartial.cs` (1720 lines, read in full)
- **LW**: legacy wrapper `Common\Internal\...InternalCommon\CertificateOfOriginsIncomingMessageService.cs`
- **LBL**: legacy `...BL\CertificateOfOriginsBL.cs` (CheckDeclarationStatus 648-669, CheckCertificateOfOriginOnDeclarationSubmited 695-719, CancelCertificateOfOriginFromMessage 2020-2035, GetCertificateOfOriginByExternalId 2037-2047)
- **LU**: legacy `CertificateOfOriginsUtil.cs` 44-105
- **LSA**: legacy `ServicesAdapter.cs` (IsTradeAgreementForCountry / GetCustomerIDByExternalID)
- **NB**: `API\CertificateOfOrigins.BL\CertificateOfOriginsBl.cs` (1-381, 960-1560, 2178-2195)
- **NV**: `...Bl.MessageValidation.cs` (full)
- **NX**: `...Bl.MessageCrossField.cs` (full)
- **NR**: `...Bl.MessagePerReason.cs` (full)
- **NI**: `...Bl.MessageInvoice.cs` (full)
- **ND**: `...Bl.MessageDeclarationCheck.cs` (full)
- **NM**: `...Bl.MessageValidationMessages.cs` (full)

Also read:
- `Validations\SaveCertificateOfOriginRequestValidator.cs`
- `Controllers\Community\CertificateOfOriginsController.cs`
- DAL `GetLatestCertificateByNumberForFeedback`, `CancelCertificate`, `GetPreviousCertificateIdByTitle`
- `ERequestReason.cs`, the message DTOs and `CustomsBookProxy.GetCustomsItemIdByFullClassification`
- the LEDGER, the previous appendix, and MIGRATION-STATUS / MIGRATION-NOT-DONE

**Not verifiable:** `BaseMessageService.HandleMessageException` is framework code outside the module. The exact header shape of a legacy fault therefore cannot be checked, for example whether the outer `RequestDataNotValid` is listed with its inner exceptions, or which code a non-Inf exception gets. The new code assumes it was "inner list only" and code 0 for a general failure.

## 1. Previous findings re-verification

| ID | Prior status | Verified now? | Evidence |
|---|---|---|---|
| C-F01 | FIXED | YES | NB:228 `AddOpenCustomsEmployeeTaskException` runs in the cancel branch before the cancel gate (NB:233). Same tail in the create branch, NR:100-101. |
| C-F02 | FIXED | YES | `SaveCertificateOfOriginRequestValidator.cs` no longer requires OrganizationUnitId > 0 (comment in its place). CustomerId > 0 kept per the analyst decision. |
| C-F03 | FIXED | YES | NB:310, 333-339: `BuildFeedbackAttachments(certificate, requestReasonCode)` is gated on the **request** reason, same expression as LP:1708. |
| C-F04 | FIXED | **PARTIAL / REGRESSED** | NB:201-213: the status branch now calls `ValidateMessageBody`. That method also runs `CheckMessageCrossFields` (NV:188-191, 211-214), which legacy explicitly skipped for GetRequestStatus (LP:95). See CN-01. |
| C-F05 | FIXED | YES | NV:830-841: an unknown port locode raises TheValueInFieldNotExistsInSystem. NX:171-176: an unknown CustomsHouse site raises the same error. |
| C-F06 | FIXED | YES | NB:1522-1527 raises Received + CertificateUpdate → ApplicationCorrected with `relatedCertificateId: certificateToUpdateId`. NV:109 passes `context.CertificateToUpdateId`. |
| C-F07 | FIXED | YES | NB:1369-1373: a new instance only runs `ResolveNewInstanceDetailCodes`, so the exporter is not re-resolved. |
| C-F08 | FIXED | YES | NB:1332-1358 touches Value only (DestinationCountry alpha-2 → id, PortOfShipment locode → site id), as LBL new-instance branch. The DisplayedValue computed by the message step is kept. |
| C-F09 | FIXED | YES | NV:706-707: DisplayedValue = the group's EnglishName. |
| C-F10 | FIXED | YES | NB:241-244: a blocked cancel sets the certificate to null, so the response has an empty feedback and ApplicationId 0. |
| C-F11 | FIXED | YES | NB:87-100 catch-all → in-band GeneralException. Lock failure → in-band ConcurrencyErrorTryAgain (NB:128-134). Null AgentRequest → throw → in-band (NB:108-111). |
| C-F12 | FIXED | YES | NV:108 `RequestMetadata.MessageSenderId ?? 0`; there is no sender id in the body DTO. (Header dependency: see CN-03.) |
| C-F13 | FIXED | YES | NB:1466-1478 calls `ChangeCertificateOfOriginIdForLeadDocument(leadDocId, replacementOldId or GetPreviousCertificateIdByTitle, newId)`. The DAL query (DAL:118-129: Title ==, OrderByDescending Id, Skip 1) matches LBL:656-660. |
| C-F14 | CLOSED-OK | Still matches | The accumulate-not-throw decision is still in place (NI:14-16 comment, NV:151-160). |
| C-F15 | CLOSED-OK | Still matches | De-duplicated errors (NR:118-133, NX:129-135). |
| C-F16 | FIXED | YES | NR:154, 182 use `GetRequestReasonName` / `GetCertificateStatusName`, which read the enum Display name (Hebrew, NB:2178-2190). |
| C-F17 | FIXED | YES | NB:266 and NR:120 use `IsNullOrWhiteSpace`. |
| C-F18 | CLOSED-OK | Still matches | NV:459-465 keeps ISO "o"; the decision comment is in place. |
| C-F19 | CLOSED-OK | Still matches | NI:93, 103-105 use `?? string.Empty`. |
| C-F20 | CLOSED-OK | Still matches | NB:287-290 has the literal plus its comment. |
| C-F21 | CLOSED-OK | Still matches | NB:291 `RequestMetadata.UserId ?? 0`. |
| C-F22 | FIXED | YES | NB:361-362 always formats the URL. |
| C-F23 | FIXED | YES | DAL:47-50: State = 1, newest CreateDate then Id, and now `LIKE '%n%'` exactly as the legacy SP. Note: the ledger text says "exact match kept"; the code now uses LIKE, which is the legacy behaviour. |
| C-F24 | FIXED | YES | NV:223-242: a number is generated only for reasons 1, 2, 10, 4, 5, 12 (LP:1563-1607). Reasons 6-9 are also removed from the enum (X-4), so the new code returns RequestReasonNotExist (NB:185-188), as legacy did. |
| C-F25 | NOT-A-DEFECT-DOC | Still matches | The header comment NB:72-85 describes the three branches. |
| C-F26 | NOT-A-DEFECT-DOC | Still matches | `CustomsBookProxy.cs:45` TODO(blocking); NB:1085-1113 TODO(blocking) items unchanged. |

### Rule-by-rule re-check

All ~41 legacy rules were checked again against the current code (same rule IDs R1-R41 as the previous appendix §1b). Each was re-checked for condition, message code, parameter, stop-or-accumulate and order. All match, or differ only in ways already recorded (C-F14/C-F15/C-F17), except:

- R4: field names in the message text; see CN-02.
- R3: the missing-body field name; see CN-02.
- The GetRequestStatus cross-field set; see CN-01.

Spot confirmations:

| Rules | Legacy | New |
|---|---|---|
| R6 exporter | LP:452-465 | NV:568-580 |
| R7/R18 Israel checks | LP:467-525 | NV:584-628 |
| R8 first country | LP:527-547 | NV:632-649 |
| R10 group | LP:592-629 | NV:678-708 |
| R11/R13/R16 dates | LP:694-751 | NV:711-762 |
| R14 city | LP:761-793 | NV:782-806 |
| R15 exemption | LP:432-441 | NV:810-824 |
| R19 ports | LP:681-692 | NV:830-841 |
| R20-R25 conditional | LP:805-915 | NX:27-197 |
| R26-R28 per type | LP:1163-1277 | NX:200-311 |
| R29-R32 items/invoice | LP:1283-1354, 1614-1652 | NI |
| R33-R41 per reason + tail | LP:919-1157 | NR:16-232 |

The Israel short-circuit in IsTradeAgreementForCountry is a literal 376 in legacy too (LSA:25, 73), so it is not a config regression.

## 2. New findings

| ID | Sev | Operation | Deviation | New file:line | Legacy file:line | Why it matters (input → wrong output) |
|---|---|---|---|---|---|---|
| CN-01 | MED | GetRequestStatus | Guard widened (the C-F04 fix over-reached) | NB:205 → NV:188-191 and NV:211-214 (`CheckMessageCrossFields`, with no reason gate). The stale comment at NX:15-16 still claims the caller excludes GetRequestStatus. | LP:95-98: `CheckFields` runs only when the reason is NOT GetRequestStatus or CertificateCancellation. LP:89-93 (field and invoice validation) does run for status. | A status query that carries a body hits the cross-field rules legacy never applied to a status query. Example: a standard-type body with no DestinationCountry, or an EUR1 body without second/origin country, or with IsCumulation=true and no cumulation country. **Legacy:** returns the saved certificate's status. **New:** returns DestinationCountryRequired / SecondCountryRequired / CumulationCountryRequired / PlaceOfManufactureAndZipcodeRequired / CustomsHouse errors, with an empty feedback and ApplicationId 0. The status answer is suppressed. Fix: skip `CheckMessageCrossFields` when the reason is GetRequestStatus. |
| CN-02 | LOW | All create reasons + status | Message parameter text (field names) | NI:21 `"InvoiceDetails"`, NI:33 `"ItemDetails"`; NV:146 `"NonManipulationCertificate"` / `"CertificateOfOrigin"`; NV:158 `nameof(CertificateOfOriginTypeCode)` | LP:190 `"CertificateOfOriginRequestInvoiceDetail"`, LP:199 `"CertificateOfOriginRequestItemDetail"`, LP:160/172 `"request.Content.NonManipulationCertificate"` / `"request.Content.CertificateOfOrigin"`, LP:1366 `CertificateOfOriginsConstants.CertificateOfOriginTypeCode` | MandatoryValue "אין ערך בשדה חובה {0}" renders the new JSON property names instead of the legacy EAI field names. Same code, different text. Noted in the previous §1b R4 but never given an ID or a ledger row. |
| CN-03 | LOW (PLAUSIBLE) | Create branch, NonManipulation or no-body requests | Source-of-truth dependency introduced by the C-F12 fix | NV:108 `RequestMetadata.MessageSenderId ?? 0` → NV:255-257 `CustomerId = agentId`; validator `CustomerId > 0` | LP:1532, 1544 `agentId = request.CustomerID` (EAI envelope, always present) | If the gateway does not deliver the MessageSenderId header, every NonManipulation certificate and every EmptyCertificate without a body gets CustomerId 0. The validator then throws, and the agent gets an in-band GeneralException with nothing saved (legacy saved it). Other reasons save with CreateCustomerId/UpdateCustomerId = 0. No test or contract confirms the header reaches this endpoint; it should be verified live. |
| CN-04 | LOW | All reasons (failure path) | Exception → in-band code collapsed | NB:94-99: every exception becomes GeneralException (code 0) | LW:84-87 → HandleMessageException keeps the InfException's own code (e.g. an InfException thrown from the legacy BL save) | A typed business failure thrown inside the save or render reaches the agent as "שגיאת מערכת" instead of the specific message. This cannot be fully verified because HandleMessageException is outside the module. The observable difference is only the code and text in the header. |

No HIGH findings.

Checked and found equivalent:
- lock key/gate
- amendment guard (except the empty-number skip, already LOW)
- reason 0 / unknown
- cancel write, event and response
- create-branch number, CustomerId, OrganizationUnitId, DestinationCountry and CertificateIdToCancel mapping
- invoice clamp and code lookups
- container ISO skipped for status
- reconciliation gate (reason ≠ Empty and type ≠ NM; states Submited/Released/Paroled/Closed)
- feedback fields vs LU:44-73 (issue dates are null in both)
- attachment gate
- ApplicationId on success and failure
- re-read of the saved certificate (State defaults to 1, `SaveCertificateOfOriginRequestDto.cs:22`)
