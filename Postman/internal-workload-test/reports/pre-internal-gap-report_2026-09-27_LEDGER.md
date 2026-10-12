# Parity ledger — מצב ממצאי הפאריטי (2026-10-07, עודכן 2026-10-11)

נבדק מחדש מול master (`ff134cd`) ב-2026-10-07 אחרי שסבב תיקונים נוסף נכנס (`1b4490e`, `729df1b`, `e151a03`, `fe13750`). נבנה מ-8 הנספחים `parity-A..H` מול הקוד הנוכחי ומול הקומיטים מאז 27.9. כל סטטוס נבדק בקוד ולא רק לפי שם קומיט; ממצאים שלא ניתן היה לאמת מסומנים UNVERIFIED.
שמות המזהים הם של הנספח המקורי (ב-C נוספה הקידומת C-, ב-F המזהים F-xx כבמקור, ב-D/E/G נוספה אות הנספח).

## סיכום

| סטטוס | משמעות | כמות |
|---|---|---|
| FIXED | תוקן בקוד | 71 |
| CLOSED-OK | נסגר: החלטת אנליסט / פאריטי מאומת / החלטת תכנון מתועדת | 46 |
| NOT-A-DEFECT-DOC | תיעוד בלבד | 7 |
| OPEN-DECISION | ממתין להחלטה של אדם | 0 |
| OPEN-BLOCKED | נדחה לשלב נפרד: תלוי בשירות/חבילה שעוד לא קיימים (לא נספר כפתוח) | 19 |
| OPEN-TODO | פער אמיתי שלא טופל | 0 |
| UNVERIFIED | לא ניתן היה לאמת | 0 |

## A — Certificate core

| ID | Severity | Title | Status | Evidence | Note |
|---|---|---|---|---|---|
| A-01 | HIGH | Received + CertificateUpdate misses ApplicationCorrected | FIXED | 66d103c; CertificateOfOriginsBl.cs ~1522 | - |
| A-02 | HIGH | DeclarationMatch event type / Export org-unit type | FIXED | 419b291; CertificateOfOriginsBl.cs ~1500-1520 | - |
| A-03 | MED | Omitted detail rows deleted | CLOSED-OK | MIGRATION-STATUS.md 2026-10-07: the client always sends every detail; GetById returns Id for each row | Closed on that statement; not independently re-verified here |
| A-04 | MED | Enrichment/validation ran for new instances | FIXED | 7a101aa; BL EnrichAndValidateDetails ~1371 (Id==0 only resolves country/port codes) | - |
| A-05 | MED | isRemarksChanged semantics / feedback on create | CLOSED-OK | 41dd845; BL ~1076-1081 comment (analyst decision 2026-09-29) | Remark change sends no feedback, as in production |
| A-06 | MED | isStatusChanged snapshot, Received quirk | FIXED | 9250aaa; BL ~1083-1086 | Idempotency guard still missing (A-14) |
| A-07 | MED | Validator rejects OrganizationUnitId=0 | FIXED | 87f5039; SaveCertificateOfOriginRequestValidator.cs (rule removed, comment) | - |
| A-08 | MED | Replacement clears IsLastVersion on replaced cert | FIXED | bb67197; BL HandleCertificateReplacement ~1695 (DataLayer.CancelCertificate keeps IsLastVersion) | Reason text still TODO(confirm) (A-16) |
| A-09 | MED | CheckDeclarationStatus lead-doc repoint dropped | FIXED | e3a65f8; BL LinkLeadDocument ~1466-1481 | Relies on ExportDealFile endpoint (real route unconfirmed, mock otherwise) |
| A-10 | MED | Issue-queue payload lacks ReportId/IsDeclarationReleased | FIXED | 507f43c; BL PublishAttachments ~1581, SendCertificateToIssueQueue ~1641-1654 | Worker consumer contract not re-verified |
| A-11 | MED | TOP(200) vs ufn_GetMaxRows | CLOSED-OK | e8fcb47; Scripts/API_20260721 - dbo.GetCertificateOfOriginsByFilter.sql:34 (production returns 200, confirmed 2026-10-04) | - |
| A-12 | MED | Customer INNER JOIN removed; titles can be NULL | CLOSED-OK | MIGRATION-STATUS.md 2026-10-07: INNER-JOIN rule (guard only for a nullable FK) | Closed on that statement; the Customers route itself stays deferred |
| A-13 | LOW | Publish feedback + PDF attachment not sent | OPEN-BLOCKED | BL ~1093-1101 TODO(blocking) #1; MIGRATION-NOT-DONE.md:158-168 | EAI outgoing channel removed from platform |
| A-14 | LOW | IsCreateAttachments/IsMessageSent guards missing | OPEN-BLOCKED | BL ~1097-1101 TODO(blocking) #3; MIGRATION-NOT-DONE.md:168 | Needs schema columns (DB + entity + seed) |
| A-15 | LOW | Auto-publish on released declaration not migrated | OPEN-BLOCKED | BL ~1117-1121 TODO(blocking) #2; MIGRATION-NOT-DONE.md:164 | Needs ExportDealFile service |
| A-16 | LOW | Replacement texts / talk-back deferred | OPEN-BLOCKED | BL ~1669-1673, ~1689 TODO(migration)/TODO(confirm) | ValidationMessages/resx source and SendMessageToAgent missing |
| A-17 | LOW | SendRequestFeedback MessageTypeId=0, no attachment | OPEN-BLOCKED | BL ~1542-1558 TODO(blocking) | Platform/product decision on channel |
| A-18 | LOW | Hardcoded `?? 0` user fallback | CLOSED-OK | Decision (Kobi, via the analyst, 2026-10-07): CC-USER-ID always carries the acting user | A 0 seen locally (no header) is acceptable; no system-user default is substituted |
| A-19 | LOW | Title-mismatch validation dropped | CLOSED-OK | BL ~1462 TODO(migration); finding itself says no behavioural effect | Legacy list never thrown on the Internal path |
| A-20 | LOW | QrCodePath stores ExternalId, not document path | FIXED | round of 2026-10-07 (parity ledger fixes commit): QrCodePath = IDocumentUtil.GetFileUrl(response.FileResource), the file-server URL legacy's GetDocumentFile stored; tests updated | Not verified against a live Documents service / printed certificate — TODO(confirm live) at the QrCodePath assignment (REVIEW002) |
| A-21 | LOW | ExporterId DisplayedValue / site display fallback | FIXED | 1b4490e (ExporterId DisplayedValue left as sent) | - |
| A-22 | LOW | Validation shape: 400 instead of InfException | CLOSED-OK | Appendix itself: acceptable | HTTP mapping change by design |
| A-23 | LOW | GetById missing id returns 404 | CLOSED-OK | BL ~30-31 documented choice (RestNotFoundException) | Internal callers must handle 404 |
| A-24 | LOW | GetById milestones: INNER JOIN dropped, Name vs Title | CLOSED-OK | Decision 2026-10-08: keep every milestone row (empty name for an unknown user); the name is UserDto.Title (legacy UserMng_User.Title, aliased UserName) | UserDto.Title carries TODO(blocking) until the Users service contract is published |
| A-25 | LOW | ByFilter date bounds inline instead of ufn_General_GetDate* | FIXED | Legacy ufn_General_GetDateEnd = date + 86399 s (23:59:59.000; body read from the legacy DB 2026-10-08); new scripts API_20261008110054 / 110055 use it instead of 23:59:59.997 | GetDateStart (midnight) was already equal; scripts compiled on the local DB in a rolled-back transaction, not applied |
| A-26 | LOW | ExternalIdExist legacy likely broken | NOT-A-DEFECT-DOC | Appendix A-26 text | Intended behaviour, not a regression |
| A-27 | LOW | LoadDataFromExportDeclaration by-ref mutation dropped | CLOSED-OK | BL ~738-746 (decision 2026-07-27) | ExportDealFile real route still unconfirmed (shared with A-09/A-15) |

## B — External / reconcile

| ID | Severity | Title | Status | Evidence | Note |
|---|---|---|---|---|---|
| B-H1 | HIGH | Assessor resolved from stale LeadDocumentId (backfill not propagated) | FIXED | 35aa7fd (BL: backfill assigned onto certificate; test added) | - |
| B-H2 | HIGH | Scalar/import-replacement findings counted when no invoice rows | CLOSED-OK | 51b0376 (no invoices => match, "as in production", analyst decision 2026-09-28) | Behaviour deliberately kept as production |
| B-H3 | HIGH | Replaced certificate never cancelled (CertificateIdToCancel not loaded) | FIXED | 9521a65 + bb36fe2 (projection includes CertificateIdToCancel) | - |
| B-M1 | MED | Invented "no export invoices" finding persisted/returned | FIXED | d2014fe (HasNoExportInvoices flag; Rejected, no exception row) | - |
| B-M2 | MED | Rejected reason text differs from legacy constant | FIXED | 7289c1b; CertificateOfOriginsConsts.cs:68 | Text now identical to legacy constant |
| B-M3 | MED | Event Title/CustomerId differ from legacy VirtualEntity | FIXED | 45574ba (CreateCertificateEventBuilder: Title=certificate.Title, CustomerId) | - |
| B-M4 | MED | Re-print passes TypeId as template/report id | FIXED | 507f43c (uses type's ReportId; skips if none) | Proxy route TODO(blocking) is separate |
| B-M5 | MED | Release publish on partial projection (QrCodePath, issue-queue fields, ReportId) | FIXED | bb36fe2 (QrCodePath, Guid, CreateCustomerId, InternalApplication, FeedbackRemark loaded) + 507f43c (ReportId in payload/gate) | - |
| B-M6 | MED | Replacement cancel clears IsLastVersion / appends reason | FIXED | bb67197 (CancelCertificate replaces reason, keeps IsLastVersion) | CertificateReplaced text still TODO(confirm), see L8 |
| B-M7 | MED | Explicit null lists give 400 instead of legacy null handling | FIXED | 6f99d26 (null-safe BL guards, DTO nullable) | - |
| B-L1 | LOW | IsExportDeclarationActive gate dropped | CLOSED-OK | Decision 2026-10-08: the gate stays dropped (analyst: flag permanently true, 2026-09-23; three other gated places dropped on the same premise) | Not checked per environment |
| B-L2 | LOW | Legacy accumulated exceptions across certificates; new evaluates each separately | CLOSED-OK | Report recommends keeping new behaviour (silent fix); no code change needed | Not documented in code/MIGRATION-NOT-DONE; add a note if desired |
| B-L3 | LOW | Destination-group check edge cases differ (non-numeric group, null country) | CLOSED-OK | Decision 2026-10-08: keep. A null destination country crashed legacy (empty predicate); a non-numeric group cannot reach this point (message 2280 converts or rejects it) | - |
| B-L4 | LOW | Warnings-task assignment lacks profession/org-unit type | OPEN-BLOCKED | CertificateOfOriginsBl.cs:2148 TODO(migration) | Needs SingleUserAssignmentFilter support in event util |
| B-L5 | LOW | Exception texts placeholders, ExceptionType=0 | OPEN-BLOCKED | CertificateOfOriginsBl.cs:2177, 2551, 2560 TODO | Waits on ValidationMessages/resx package |
| B-L6 | LOW | Release publish does not send request-feedback message | OPEN-BLOCKED | Relabelled TODO(blocking) in DeclarationReleased: same blocker as SaveCertificateOfOrigin #1 (EAI outgoing channel removed) | Do not send without the PDF; when the channel returns, send from PublishAttachments for both paths |
| B-L7 | LOW | Cancellation branch: agent message deferred, reason literal | OPEN-BLOCKED | CertificateOfOriginsConsts.cs:70-72; Bl.cs:1992 TODO | Needs resx text (EServerTerms.CanceledDeclaration) + agent message |
| B-L8 | LOW | HandleCertificateReplacement: FeedbackRemark, SendMessageToAgent, CertificateReplaced text deferred | OPEN-BLOCKED | CertificateOfOriginsBl.cs:1669, 1689 TODO | Reason now a literal (bb67197); UIMessage text and agent message remain |
| B-L9 | LOW | CR 194221: event 642 AdditionalInfo always the request-reason name | FIXED | 2026-10-11: legacy RaiseTaskNewCertificateOfOriginCheck was updated by the developer (CR 194221): reason name, then ". " + UIMessage 6541 ("שים לב: להצהרת היצוא קיימות תעודות מקור נוספות {0}") when the declaration has other live certificates; ported in BuildNewCertificateCheckAdditionalInfo + DAL GetLiveCertificateNumbersByIds; test NewCertificateCheckAdditionalInfoTests | The other two methods sent (RaiseNewCertificateOfOriginCreatedEvent, UpdateCertrificateOfOrigins) were diffed: identical to the local legacy. Not run against a live ExportDealFile service — TODO(confirm live) at GetOtherCertificateNumbersOnDeclaration (REVIEW002) |
| B-L10 | LOW | Audit user falls back to 0 without user header | CLOSED-OK | RequestMetadata.UserId ?? 0 (project convention; no hardcoded 1) | Guard accepted pattern |
| B-L11 | LOW | GetGoodsItemCerificateDTO keeps client id when number is null | FIXED | 1b4490e (null certificate number yields a null id) | - |
| B-L12 | LOW | GetCertificateOfOriginID returns 404 vs legacy null | CLOSED-OK | Controller/BL 404 convention (RestNotFoundException) | Documented project convention; callers must handle 404 |
| B-L13 | LOW | Convert: 404 vs InfException, new Customers dependency | CLOSED-OK | 404 convention; Customers enrichment via FillCustomersInformation | Convention; Customers outage now fails Convert (accepted) |
| B-L14 | LOW | SaveCertificateOfOriginAttachments documented implementation changes | CLOSED-OK | Developer decisions commented in code (report verdict CONFIRMED/documented) | - |
| B-L15 | LOW | GenerateTemplate (CR 194221) no legacy source; template id/format notes | NOT-A-DEFECT-DOC | Report notes only; template id 1 is TODO(confirm) | - |

## C — Incoming message 2280

| ID | Severity | Title | Status | Evidence | Note |
|---|---|---|---|---|---|
| C-F01 | HIGH | Cancellation not blocked while customs-employee task open | FIXED | c34ba2a; NB:~253 AddOpenCustomsEmployeeTaskException in cancel branch | - |
| C-F02 | HIGH | OrganizationUnitId>0 validator rejects legacy-valid messages | FIXED | 87f5039; SaveCertificateOfOriginRequestValidator.cs:31-35 | CustomerId>0 kept by analyst decision 2026-09-28 |
| C-F03 | HIGH | Attachment gated on stored reason, re-renders on status poll | FIXED | c31839e; NB BuildFeedbackAttachments(certificate, requestReasonCode) | - |
| C-F04 | MED | Status query skips validation | FIXED | 569901f; NB switch GetRequestStatus calls ValidateMessageBody | - |
| C-F05 | MED | Unknown port locode / CustomsHouse site silently accepted | FIXED | cb9d735; NV:CheckIfInternationalSiteExist, NX:168-175 | - |
| C-F06 | MED | CertificateUpdate lacks ApplicationCorrected with related id | FIXED | 66d103c; NB:1522-1530 | - |
| C-F07 | MED | Exporter re-resolved/overwritten by shared save | FIXED | 7a101aa; NB EnrichAndValidateDetails early-returns for Id==0 | - |
| C-F08 | MED | Port display names overwritten by save | FIXED | 7a101aa (new instance only converts codes, DisplayedValue kept) | NB ResolveNewInstanceDetailCodes touches Value only |
| C-F09 | MED | Country-group display left as numeric id | FIXED | cf26f0a; NV:~667-680, NB:1445-1450 | - |
| C-F10 | MED | Blocked cancel echoes feedback and ApplicationId | FIXED | d9792cc; NB cancel branch sets certificateToResponse=null | - |
| C-F11 | MED | Lock/null-agent/validator failures return HTTP 400 not in-band | FIXED | 81ccfe9, 89fe992, aa2cdaf; NB:79-150 | - |
| C-F12 | MED | Agent customer id client-supplied in body | FIXED | cd94c42; NV:108 uses RequestMetadata.MessageSenderId | - |
| C-F13 | MED | Lead-document repoint by title not ported | FIXED | e3a65f8; NB LinkLeadDocument ~1470-1482 calls ChangeCertificateOfOriginIdForLeadDocument | The MessageDeclarationCheck comment named SupersedePreviousVersion as the repoint; corrected to LinkLeadDocument (2026-10-08) |
| C-F14 | LOW | Error accumulation/ordering differs from legacy hard-stops | CLOSED-OK | documented developer decision in code comment (NI:17-41) | response exception order differs, accepted |
| C-F15 | LOW | De-duplicated error emissions | CLOSED-OK | NB:264-270, NR:36-43 | fewer duplicate errors; harmless |
| C-F16 | LOW | Message params use enum identifiers not Hebrew table names | FIXED | 1b4490e + e151a03 (Hebrew table names); tests ff134cd | IllegalFirstCountryInAgreement params have no placeholder in the text, nothing to render |
| C-F17 | LOW | Whitespace certificateID gives CertificateDoesntExist | FIXED | 1b4490e; test ff134cd | - |
| C-F18 | LOW | Date detail Value stored as ISO "o" not culture format | CLOSED-OK | Decision 2026-10-08: keep ISO. Every Value reader uses DateTime.TryParse (ISO under any culture); print SPs read DisplayedValue | Comment at ToStringValue(DateTime) |
| C-F19 | LOW | Null item/invoice text becomes "" | CLOSED-OK | MIGRATION-STATUS.md 2026-10-07: the columns are NOT NULL | Closed on that statement; not independently re-verified here |
| C-F20 | LOW | Cancel reason hard-coded literal | CLOSED-OK | NB:~222 literal with explanatory comment | UIMessage catalogue unavailable |
| C-F21 | LOW | Cancel write stamps UpdateUserId 0 without user context | CLOSED-OK | Decision (Kobi, via the analyst, 2026-10-07): CC-USER-ID always carries the acting user | A 0 seen locally (no header) is acceptable; no system-user default is substituted |
| C-F22 | LOW | QueryUrl empty when Guid null (legacy always formatted) | FIXED | 1b4490e; test ff134cd | - |
| C-F23 | LOW | Latest-by-number/OriginCriterion lookups lack state filters | FIXED | round of 2026-10-07 (parity ledger fixes commit): certificate by number State = 1 + newest CreateDate (legacy SP); OriginCriterion State == 1 (SystemTables default); tests LatestCertificateByNumberTests, SystemTableActiveRowsTests | Exact number match kept on purpose (legacy SP used LIKE %n%) |
| C-F24 | LOW | Certificate number generated for reasons 6-9 | FIXED | 1b4490e (number generated only for reasons 1,2,10,4,5,12, as legacy's switch) | - |
| C-F25 | LOW | Stale SKELETON header comment | NOT-A-DEFECT-DOC | Cleaned 2026-10-08: the header describes the three branches; the "(once wired) reconciliation" note and the "deferred" issue-date note were rewritten too (legacy never set IsDeclarationReleased in the message flow, so both issue dates are null in every branch) | comment cleanup only |
| C-F26 | LOW | Mock proxies / TODO(blocking) deferrals | NOT-A-DEFECT-DOC | MIGRATION-NOT-DONE.md; CustomsBookProxy TODO | documented deferral |

## D+E — Web GUID and authentication reads

| ID | Severity | Title | Status | Evidence | Note |
|---|---|---|---|---|---|
| D-F1 | MEDIUM | Web response member names changed (exceptionDescription spelling, envelope fields dropped) | FIXED | Decision 2026-10-11: keep the legacy wire names. [JsonPropertyName] on the four web DTOs (ExeptionDescription, QueryURL, DocumentID, CertificateOfOriginItemDetailDTOs, PascalCase throughout); OpenAPI contract first; test TheWebResponseUsesTheLegacyWireNames; Postman web-query checks updated | Verified live 2026-10-11. The BaseWebResponse envelope (Status, TechnicalInformation, SevriceInstanceID, CallID) is not reproduced; a null member is omitted, where legacy DCJS wrote it as null |
| D-F2 | MEDIUM | Dates serialized as ISO datetime instead of yyyy-MM-dd | FIXED | b1cc78a (WebDateJsonConverter, CertificateOfOriginsBl.cs:564) | - |
| D-F3 | MEDIUM | DateOfDeclaration parsed with container culture | FIXED | b1cc78a (LegacyHostCulture he-IL / day-first fallback, CertificateOfOriginsBl.cs:617-630) | - |
| D-F4 | MEDIUM | Proxy failures/missing config now HTTP errors instead of in-band error; reverse NRE cases silent | CLOSED-OK | Decision 2026-10-11: a technical failure is an HTTP 5xx (502/503 from ProxyFailureExceptionFilter, else 500), not an in-band error; recorded at the web controller and in the OpenAPI contract (502/503 declared) | Legacy answered in-band because the ESB channel required a reply. The reverse cases (legacy NRE → now a silent null) are kept |
| D-F5 | LOW | DocumentId now via Documents proxy; route unconfirmed, deleted-doc filtering may differ | OPEN-BLOCKED | DocumentsProxy.cs:19 TODO(blocking); MIGRATION-NOT-DONE decision #4 | Needs Documents service route confirmation |
| D-F6 | LOW | Empty-string guid query binds to null so "Invalid Guid" becomes "No Matching Certificate" | FIXED | e151a03 (empty guid/number kept as empty string) | Verified live 2026-10-11 (local service): `?CertificateOfOriginGuid=` → 200 "Invalid Guid", also with a number alongside; no parameters → "No Matching Certificate"; a valid guid returns the certificate |
| D-F7 | LOW | CertificateOfOriginQueryURL seeded with DEV URL, insert-if-missing | NOT-A-DEFECT-DOC | Scripts/API_20260716 - add params.sql:186-203 insert-if-missing | A per-environment value: set CertificateOfOriginQueryURL at deployment of each environment |
| D-F8 | LOW | [NotFoundResponse] on action contradicts contract (no 404) | FIXED | round of 2026-10-07 (parity ledger fixes commit): [NotFoundResponse] removed from web RequestByGuid | Declared contract only; the behaviour (200 with exceptionDescription) is unchanged |
| E-F1 | MEDIUM | ByFilter TOP (200) replaces ufn_GetMaxRows | CLOSED-OK | e8fcb47 (analyst check 2026-10-04: prod ufn_GetMaxRows()=200) | - |
| E-F2 | MEDIUM | LeadDocumentTitle always NULL (ByFilter, ByLeadDocumentIDs) | OPEN-BLOCKED | AuthenticationRequestBl.cs:777,825 TODO(migration); SPs CAST(NULL ...) | Needs DealFile/lead-document title source from owning service |
| E-F3 | HIGH | File read drops child-request fields (circumstances, remarks, tax differences, DocumentNumber, IsOldIndication) | FIXED | 476feb1 (CertificateOfOriginsDal.cs:594-630, 28 columns incl. all 8) | Still not projected: ResponsePhoneNum, OrganizationUnitTypeID, ItemDetailID, CreateUserID (documented as not on file screen) |
| E-F4 | MEDIUM | GetEntityDocuments assumes Documents payload is already DocumentDto (StringDynamicParams/OtherRelatedEntities) | OPEN-BLOCKED | DocumentsProxy.cs:19 TODO(blocking); AuthenticationRequestBl.cs:659 assumption | Depends on Documents service response shape/route |
| E-F5 | LOW | IsSendReminderForImporterTaskExists status semantics (!=2 vs in-progress) | NOT-A-DEFECT-DOC | Documented as "Pattern A", TODO(confirm) at AuthenticationRequestBl.cs:~327 | Analyst to confirm status semantics; UNVERIFIED current resolution |
| E-F6 | LOW | Missing request id is 404 instead of NRE | CLOSED-OK | Developer decision 2026-08-02 in code comment (AuthenticationRequestBl.cs:33) | - |
| E-F7 | LOW | Six result-set columns not returned by GetByID | CLOSED-OK | Report analysis: no consumer; save does not write them (DAL 375-452 unchanged) | Reviewer's own assessment; no explicit analyst sign-off |
| E-F8 | LOW | Missing file is 404 instead of null | CLOSED-OK | Documented in code; caller rerouted to header read | - |
| E-F9 | LOW | Invoice-number search: CONTAINS -> LIKE substring | CLOSED-OK | SP script header API_20260907180000 (no FTS catalog) | Deliberate, documented |
| E-F10 | LOW | Cross-service INNER JOINs dropped, rows with dangling refs now returned | CLOSED-OK | Joins absent in API_20260907180000 (only local joins remain); design decision (INNER JOIN guard only for nullable FK) | Documented design choice |
| E-F11 | LOW | ImporterName/OrganizationUnit name sources changed | OPEN-BLOCKED | Customers proxy route TODO(blocking) | Equivalence depends on proxy/lookup; UNVERIFIED |
| E-F12 | LOW | Decision lookup omits EndDate and IsAutomatic | FIXED | round of 2026-10-07 (parity ledger fixes commit): EndDate + IsAutomatic on the decision entity, DTO, DAL read and both BL maps; test DecisionLookupColumnsTests | - |
| E-F13 | LOW | IsVendorByIssuingCountryId adds State != 99 guard | FIXED | round of 2026-10-07 (parity ledger fixes commit): IsSupplierDeliveryCountry reads State == 1 (SystemTables default); test SystemTableActiveRowsTests | Reopened: State defaults to 0, so State != 99 also matched inactive rows |
| E-F14 | LOW | GetEntityDocuments null config/unknown TypeId silent instead of NRE | CLOSED-OK | AuthenticationRequestBl.cs:~622 treats null as empty filter (intentional) | Improvement over NRE |
| E-F15 | LOW | ByLeadDocumentIDs empty input returns []; ImporterID/LastDeliveryForImporter DTO fields dropped | CLOSED-OK | Legacy fields never populated by SP | Behavior difference benign |
| E-F16 | LOW | CheckIfExistsAdditionalRequestsForImporter: null importerId is 400, not false | CLOSED-OK | AuthenticationRequestController.cs:78 [BindRequired] importerId (vendorId/customerId nullable) | Stricter binding by design |
| E-F17 | LOW | GetPathsForNavigationToVendor returns empty ViewPaths | OPEN-BLOCKED | AuthenticationRequestBl.cs ~1396 TODO(blocking); MIGRATION-NOT-DONE.md:130 | Documented deferral; needs NavigationPath source |

## F+G+H — Authentication writes, delivery/schedulers, export documents

| ID | Severity | Title | Status | Evidence | Note |
|---|---|---|---|---|---|
| F-01 | HIGH | SaveImport: create path for a new request lost | FIXED | 1b13471, b901dc1 | Insert vs update decided by IsNewInstance. |
| F-02 | HIGH | SaveFile: child-request edits dropped | FIXED | 476feb1 | Child DTO carries the screen edits; DAL writes them. |
| F-03 | HIGH | File-status collateral outcome ports legacy dead code | CLOSED-OK | analyst decision; MIGRATION-NOT-DONE.md:269; BL:1216 | Kept on purpose; product sign-off noted in doc. |
| F-04 | MED | CR 194221: decision 10 holds collaterals, file status 10 releases | CLOSED-OK | analyst decision; BL:1218 | Accepted as intended by analyst. |
| F-05 | MED | File OrganizationUnitId from client, not computed | FIXED | 8b4e9f0 | Events carry the first child request's org unit. |
| F-06 | LOW | File event Title was bare id instead of Hebrew label | FIXED | 58ab826 | BL:1301 FileEventTitle. |
| F-07 | LOW | AuthenticationNeedless rejection event lacks org unit/customer | FIXED | 64bed03 | |
| F-08 | LOW | ChangeTempCollateral: dropped `true` flag + null RelatedEntity hidden | OPEN-BLOCKED | 1cfa199 (null RelatedEntity half); CollateralProxy.cs:28-34 TODO(blocking) | Null guard fixed. The `sendMessageOnChangeTempRequest=true` flag still waits for the cloud Collateral service. |
| F-09 | LOW | Approval grant keeps file-id/request-entity-type quirk | CLOSED-OK | BL:1168-1178 ("Legacy quirk preserved") | Verified parity with legacy, deliberate. |
| F-10 | LOW | No transaction; events/side effects before the 404 | FIXED | f0ca8aa | 404 checked up front. TimeStamp concurrency judged not a deviation. Per-step commits (no single UoW) remain by design. |
| F-11 | LOW | CreateNewAuthenticationFile minor diffs (NRE to ?? 0, 400, CustomerIDList) | FIXED | 14e2936 | NRE case now rejected before any write. Rest are documented design decisions. Link-SP semantics unverifiable (SP source not in repo). |
| F-12 | LOW | Collateral/Tasks/MessageManagement proxy routes unconfirmed | OPEN-BLOCKED | MIGRATION-NOT-DONE.md:242 | Needs the cloud services to confirm routes/bodies. |
| G-F1 | HIGH | Vendor delivery: FirstProvideContactDate not persisted | FIXED | 4308fe6 | Stamped on first delivery only (IsDelivery), not on reminders. |
| G-F2 | LOW | Importer reminder SP: Canceled/Suspended tasks no longer block | OPEN-BLOCKED | BL.Schedulers.cs:29 TODO(confirm); NOT-DONE:246 | Needs raw TaskStatusID from the Tasks service. |
| G-F3 | MED | Delivery writes did not stamp UpdateUserId | FIXED | 289c757, 51adab4 | Plus a guard test for every set-based DAL write. |
| G-F4 | LOW | HandleDeliverySent: TypeID fallback dropped | FIXED | See BN-02 | - |
| G-F5 | LOW | Importer delivery: null-file guard added (legacy threw NRE) | CLOSED-OK | BL:555 | More lenient, unreachable from the legacy client. |
| G-F6 | LOW | Importer reminder SP: Docs INNER JOIN existence filter gone | CLOSED-OK | SP 20260907101500 header note | Documented in the SP header. Rows without a Docs_Document are now returned. |
| G-F7 | LOW | Delivery flows return bool/slim DTO instead of full entity | FIXED | Delivery results carry every field the server changed (request: DecisionId, LastDeliveryForImporter, UpdateDate; file: status, delivery method, LastDelivery, UpdateDate), one clock passed BL to DAL; tests in DeliveryAuditUserTests | No GetById re-read: it calls Collateral/Tasks/Documents/ExportDealFile. FirstProvideContactDate not returned (legacy set it on the client) |
| G-F8 | LOW | CloseReminderTask Title is a literal | CLOSED-OK | BL:1301 FileEventTitle; F-06 review (partial:103-106) | The literal equals the legacy computed label. |
| G-F9 | LOW | Reminder job swallows per-row exceptions (legacy aborted) | FIXED | round of 2026-10-07 (parity ledger fixes commit): plain loop, no catch, effected rows = due count, as legacy ExecuteTask | Planar project builds |
| G-F10 | LOW | Scheduler UDF id to parameter-name mapping inferred | CLOSED-OK | Pairing confirmed: 5 ids by legacy DataScript inserts, 1148 = DaysForFirstReminderInAuthenticationRequest1 (value 6) checked by the analyst in the legacy General_enum_GlobalParam, 2026-10-07 | TODO(confirm) removed; comments in Schedulers.cs and Consts.cs cite the sources |
| H-1 | MED | GetById omits 6 columns (CreateDate, CreateUserId, UpdateDate, UpdateUserId, State, OrganizationUnitId) | FIXED | e151a03 (the 6 columns read in the narrow second query) | Verified live 2026-10-11 against the local DB: GET ui/ExportDocumentAuthenticationRequest/1 returns State, OrganizationUnitId, CreateDate, CreateUserId (1), UpdateDate, UpdateUserId (5) equal to the row (a row whose create and update values differ) |
| H-2 | MED | Optimistic concurrency added (legacy last-writer-wins) | CLOSED-OK | Decision 2026-10-08: keep optimistic concurrency; client contract in the validator comment (send the last returned TimeStamp, 409 = reload) | Only writer is the edit screen's Save |
| H-3 | MED | Child merge not atomic with the parent | FIXED | c1b46d0; DAL:870-875 | Deletes staged through the tracker, one commit. |
| H-4 | MED | Attach/message VirtualEntity had no Title/CustomerId | FIXED | b7f2249; ExportBl ToRelatedEntity | OrganizationUnitId still not on VirtualEntityDto (open contract question, noted in code). |
| H-5 | LOW | Events raised after the commit, not before | NOT-A-DEFECT-DOC | report text ("arguably an improvement") | |
| H-6 | LOW | Null StatusId no longer throws after save | FIXED | round of 2026-10-07 (parity ledger fixes commit): StatusId required by the save validator (400 before any write); test ExportRequestStatusRequiredTests | Stricter than legacy, which saved and raised the events before failing |
| H-7 | LOW | Search INNER JOINs became IS NOT NULL guards | CLOSED-OK | DAL comment | Documented design choice. |
| H-8 | LOW | Search names from Customer.Name vs legacy Customers.Title | FIXED | CustomerDto / VendorDto: Name -> Title (legacy Customers_Customer.Title / Vendors_Vendor.Title in every SP; the cloud Customers CustomerDto has Title, no Name); 8 enrichment sites + mocks; test CustomerAndVendorTitleTests | Wider than reported: the guessed Name would have left every customer/vendor name null. CustomersByIds / VendorsByIds still unconfirmed (route TODO(blocking)) |
| H-9 | LOW | InvoiceIdNum CONTAINS became substring LIKE | CLOSED-OK | REWIRE-PLAN; DAL comment | Accepted loss of full-text. |
| H-10 | LOW | Filter field renames / HTTP verb | CLOSED-OK | report (parity criterion by criterion) | Semantics match the SP. |
| H-11 | LOW | Status name from the enum Display attribute, not the DB lookup | FIXED | e151a03 (status name from the ExportAuthenticationRequestStatus table row) | - |
| H-12 | LOW | OrganizationUnitId on insert taken from the client, no server default | CLOSED-OK | MIGRATION-STATUS.md 2026-10-07: legacy trusted the client; a server default is new behaviour | Closed on that statement |
| H-13 | LOW | Specific-event AdditionalInfo is "" instead of null | FIXED | e151a03 + fe13750 (AdditionalInfo left unset = null) | - |
| H-14 | LOW | Customer endpoints: 404 without the legacy domain message | FIXED | bdb1866, b720c6a | Now 400 with InvalidIdentificationNumber / NoCustomHouseForThisCountry. UIMessage text is still TODO(confirm). |

## Found outside the report (2026-10-07)

| ID | Severity | Title | Status | Evidence | Note |
|---|---|---|---|---|---|
| X-1 | LOW | Lookups that replaced SystemTablesUtil reads did not filter State == 1 | FIXED | round of 2026-10-07 (parity ledger fixes commit): `Get(id, ActiveState)` / `State == ActiveState` for Country, City, CountryGroup, CurrencyType, PackingType, MeasurementUnit, Site, InternationalSite, DataDictionaryField; test LookupActiveRowsTests | `ILookupUtil.Get(id)` and `Search` do not filter (Lookup 1.10.120). Whether the owning services' `lookup/{Type}` already returns active rows only is not verified; the filter is harmless if they do |
| X-2 | LOW | The certificate-type name ignores the type's State | FIXED | Name from ECertificateOfOriginType [Display] (recorded decision 2026-08-02, reconfirmed 2026-10-08); members 10 and 11 added; CertificateTypeEnumMatchesSeedTests pins every type row and its Name to the seed script | State not checked: no inactive type exists and the name is display-only |
| X-4 | LOW | Message 2280 accepted request reasons 6-9 that legacy rejected | FIXED | 2026-10-11: legacy CheckRequestReasonCodeEnum validated against CertificateOfOrigins_enum_RequestReasonCode, which has no 6-9 (deleted as not relevant, developer 2026-10-11); the migration validated against ERequestReason, which still had them. Members 6-9 removed; RequestReasonEnumMatchesSeedTests pins the enum to the seed both ways | Verified live: reason 8 → "סיבת בקשה לא קיימת במערכת" (5020) |
| X-3 | MED | Short dates printed in the container's culture | FIXED | LegacyHostCulture (he-IL, dd/MM/yyyy, '/'): every ToShortDateString in the BL; test LegacyShortDateTests | Under ICU even he-IL prints 25.9.2025; legacy printed 25/09/2025 |

## Re-check of 2026-10-11 (`pre-internal-gap-report_2026-10-11.md`)

8 fresh reviewers (A–H) on master `f9374bf`. Every earlier FIXED item was re-verified in code: 0 regressions. Three fixes are partial (A-21, C-F04, G-F4); they are listed below. Every HIGH/MED here was re-read in both sources by the orchestrator (CONFIRMED). Appendices: `pre-internal-gap-report_2026-10-11_parity-A..H.md`.

| ID | Severity | Title | Status | Evidence | Note |
|---|---|---|---|---|---|
| BN-01 | HIGH | Reconcile: goods items without a customs item skip origin-country / country-group / deal-file checks | FIXED | DAL keeps every goods item (GoodsItemCustomsItemIds, null = no customs item); BL loops all; 6-digit only for non-null. Test GoodsItemWithoutCustomsItemStillRunsTheDealFileCheck (mutation-checked) | Root: a legacy foreach over entities became a projection of non-null ids (HasValue added to compile). Prevent: code-review gate on HasValue filters in DAL projections |
| GN-01 | HIGH | Worker 2281 feedback XML: PascalCase elements + EAICommon namespace vs the PC_NG_2281 schema | FIXED | Worker DTO element names + namespace = the EAI schema class; test WorkerRequestFeedbackXmlTests (mutation-checked) | Root: imported worker never compared to the EAI schema. Prevent: new projects get full parity; outgoing XML compared to the schema class by a serializing test |
| AN-01 | MED | SaveCertificateOfOrigin not atomic: previous version cancelled (ExecuteUpdate) before the insert | FIXED | External calls (QR, lookups) before the first write; supersede + upsert + children in one transaction (InTransaction, inside the EF retrying strategy) | Root: commit-once split into per-mutation writes without keeping atomicity. Prevent: bl-rules: multi-write legacy commit → InTransaction |
| AN-02 | MED | Publish deletes the QR document it just created (QR now linked to the certificate) | FIXED | QR uploaded with no entity (legacy bare file, entity -1), before the upsert; QrCodePath persisted with it. Tests SaveCertificateOfOriginQrCodeTests updated | Root: the A-20 fix changed the document link without checking who else reads the entity's documents. Prevent: every parity fix gets its own review of all consumers of the changed data |
| CN-01 | MED | Status query runs the cross-field checks legacy skipped (C-F04 fix went too far) | FIXED | Cross-field pass gated on reason not GetRequestStatus/CertificateCancellation; tests MessageStatusQueryCrossFieldTests (mutation-checked) | Root: the C-F04 fix called a shared routine that runs more than legacy ran for that reason. Prevent: calling a shared routine from a legacy branch → compare every guard of the branch |
| FN-01 | MED | Cancelling a file raises AuthenticationRequestRejected for rejected children (legacy: unlinked first → none) | FIXED | Rejection loop skipped when the file is cancelled; test FileStatusChangeParityTests (mutation-checked) | Root: implicit self-tracking fixup (nulling the FK empties the parent collection). Prevent: parity-review rule on FK-null/detach + reverse loops |
| FN-02 | MED | File status change with no child requests → org unit 0 → event builder throws → 500, status not saved | FIXED | Unit applied only when > 0 (EventBuilderExtensions.WithOrganizationUnitIdWhenKnown); file read returns OrganizationUnitId = first child's (legacy getter); test FileStatusChangeParityTests | Root: the builder-refuses-0 difference was fixed in one class only. Prevent: one helper + grep gate on WithOrganizationUnitId |
| GN-03 | MED | ChangeStatusAfterDeliverySent / CloseReminderTask: client org unit, 0 → 500 (F-05 not applied) | FIXED | Same helper + the file read now returns the unit the client sends back | Root: the F-05 fix was applied in one method only. Prevent: a fix covers every consumer of the same value |
| GN-02 | MED | Worker 2281: IssueDateIfReleased / IssueDateIfNotReleased never mapped | FIXED | Worker maps IssueDateIfReleased / IssueDateIfNotReleased (+Specified) as CertificateOfOriginsUtil.cs:62-69; tested | Root: imported worker. Prevent: as GN-01 |
| GN-04 | MED | Worker reads CertificateOfOriginQueryURL from appsettings, not Parameters | FIXED | Worker reads CertificateOfOriginQueryURL from IParametersUtil (AddParametersUtil); AppSettings removed | Root: imported worker skipped the config scan. Prevent: the GetConfig→parametersUtil scan covers Workers/ and Planar/ |
| CR-N1 | MED | Unregistered templateId → RestValidationException ctor throws → 500 instead of 400 | FIXED | RestValidationException(nameof(templateId), message); test TemplateUnknownIdTests (mutation-checked) | Root: params overload compiles with one string; no error-path test. Prevent: grep gate on one-argument RestValidationException; one error request per endpoint in Postman |
| A-21 | LOW | (partial) Port detail types still show the raw id | FIXED | ApplyLegacyDisplayedValue: ExportPort / PortOfEntrance / ExitPort → InternationalSite EnglishName by id (CheckIfSiteExist); PortOfShipment by locode. Test ExistingCertificateDisplayFollowsTheLegacyCaseForEachDetailType (mutation-checked) | Root: CheckSpecificField ported as categories, not case by case. Prevent: legacy switch ported case by case |
| C-F04 | — | (partial) see CN-01 | FIXED | See CN-01 | - |
| G-F4 | LOW | (partial) no TODO marker in code for the dropped TypeID fallback | FIXED | See BN-02 | - |
| AN-03 | LOW | Saving an existing certificate rewrites display values legacy left | FIXED | Same method: Israel-only country types, ExportCountry, out-of-range dates, non-numeric ids and types outside the switch keep the client display; dates only in the legacy ranges | Root: a catch-all else wrote every type; legacy switch had no default. Prevent: parity-review rule on switch without default |
| AN-04 | LOW | New certificate: unknown DestinationCountry / PortOfShipment code saved as-is (legacy failed) | FIXED | Unknown DestinationCountry / PortOfShipment code on a new certificate → 400 (legacy GetIdByCode threw); runs before any write (AN-01); test NewCertificateWithAnUnknownCodeIsRejectedBeforeAnyWrite (mutation-checked). Message text TODO(confirm): legacy {0} = DataDictionaryTable.Description (IdByCodeLogicGetter.cs:77-79), not in this repo | Root: 'leave unresolved as-is' chosen over the legacy throw. Prevent: every lookup-by-code miss maps to the legacy outcome |
| BN-02 | LOW | HandleDeliverySent: TypeID no longer accepted in place of EntityType | FIXED (revised 2026-10-12) | The endpoint takes the platform EventResponseArgs (PATCH, per the template) and matches RelatedEntity.EntityType == AuthenticationRequestFile, loaded by EntityId. The TypeID disjunct is NOT reproduced: legacy VirtualEntity.TypeID is the business type, and the platform RelatedEntity has no such field. The custom RaiseEventArgsDto read "id", not "entityId" — against the platform body the file was never found. Test DeliverySentEventEntityTests goes through platform-shaped JSON. No Events_EventTypeResponse row invokes this callback (row 5458 points to a method absent from legacy) — MIGRATION-NOT-DONE | Root: a purpose-built DTO instead of the platform type, and a test that built the DTO in C#. Prevent: net10-code-review gate (event response = EventResponseArgs); parity-review checks the callback registration before treating it as live |
| BN-03 | LOW | Template controller comment says 404 | FIXED | Controller comment corrected (400 unregistered template, 404 no data) | - |
| CN-02 | LOW | MandatoryValue texts carry the new JSON names | FIXED | MandatoryValue parameters are the legacy texts (CertificateOfOriginRequestInvoiceDetail / ...ItemDetail / request.Content.*); Postman MUST_SAY updated to the legacy text | Root: nameof() of the new DTO used for message parameters. Prevent: message parameters copied literally from legacy |
| CN-03 | LOW | Agent id only from the MessageSenderId header | OPEN-BLOCKED | Needs a live check that the gateway sends MessageSenderId | Not verifiable from source |
| CN-04 | LOW | Every exception collapses to GeneralException (code 0) | OPEN-BLOCKED | Legacy HandleMessageException is framework code outside the module | Not verifiable from source; ask the platform team |
| DN-01 | LOW | Error responses: lists `[]` instead of null | FIXED | Response lists nullable, null on the two error answers; test AnErrorAnswerCarriesNullLists | Root: C# '= []' default on a wire contract. Prevent: wire-contract defaults compared to legacy null/empty |
| DN-02 | LOW | JSON member order / escaping differs from DataContractJsonSerializer | NOT-A-DEFECT-DOC | Member order and escaping differ; every JSON parser reads the same values | - |
| DN-03 | LOW | Malformed issuingDate → automatic 400 | NOT-A-DEFECT-DOC | Model-binding 400 on a malformed issuingDate is in the declared contract | - |
| DN-04 | LOW | Docs: lookups described as proxy 502s | FIXED | Web controller comment, openapi 502 text, WebInvoiceDetailDto comment corrected | Root: comments not updated when proxies became lookups |
| DN-05 | LOW | Field labels 20306/20310/20661 depend on the local DataDictionaryField lookup | OPEN-BLOCKED | Bl:673-681 TODO(internal) | |
| EN-01 | LOW | ById returns LastDeliveryForImporter (legacy null) | OPEN-DECISION | Dal:471; legacy ById SP never selected it, so the legacy popup save wrote null back (EF4 self-tracking ApplyChanges marks every column modified) | Parity = clear the column on every popup save, which looks like a legacy bug feeding the importer reminders. Needs an analyst decision |
| EN-02 | LOW | File header omits TimeStamp/CreateUserID/UpdateDate/UpdateUserID | NOT-A-DEFECT-DOC | No consumer of the four header columns | - |
| EN-03 | LOW | Empty InvoiceNumber → empty list (legacy full-text error) | NOT-A-DEFECT-DOC | Legacy raised a full-text SQL error for an empty term; the new empty list is not a wrong result | - |
| FN-03 | LOW | Vendor/customer/importer on a child cannot be cleared from the file screen | FIXED | UpdateFileChildRequest writes the nullable columns as sent (null clears), as legacy saved the client entity; NOT NULL columns keep the row on null | Root: 'null keeps' chosen for partial callers; the only caller round-trips the full read. Verify live (SQL) |
| FN-04 | LOW | Duplicate new request: side effects before the 409 | CLOSED-OK | Same order as legacy (side effects before the duplicate insert fails) | - |
| FN-05 | LOW | Every file save stamps UpdateDate on every child | FIXED | Child and file updates filtered to rows whose values change; an unchanged file still answers found (AnyAsync) | Root: set-based write replaced change tracking. Verify live (SQL) |
| GN-05 | LOW | Worker document org unit = certificate's or literal 4 | NOT-A-DEFECT-DOC | Legacy SendCertificateToIssueQueue sent certificate.OrganizationUnitID too (BL:1152); the worker uses it; ?? 4 is unreachable (int) | - |
| GN-06 | LOW | Worker UpdateIsInPublishingProcess writes no UpdateDate/UpdateUserId | OPEN-BLOCKED | Original worker source not available to compare | - |
| GN-07 | LOW | Planar: a row with org unit 0 aborts the batch | FIXED | Covered by the WithOrganizationUnitIdWhenKnown helper in both scheduler events | Same as FN-02 |
| HN-01 | LOW | Customers 404 → 5xx instead of the "no customs house" 400 | OPEN-BLOCKED | CustomerProxy:43 | Depends on the route contract |
| HN-02 | LOW | Stale comments / [NotFoundResponse] on the customer endpoints | FIXED | Export BL/DAL/enum/controller comments and [NotFoundResponse] on the two customer endpoints corrected | - |

### Independent review of the 2026-10-11 fixes

Two reviewers who did not write the fixes re-read every fix against legacy (scratchpad `fix-review-api.md`, `fix-review-worker.md`). Verdict: all fixes OK except the issues below, which the review found in the fixes themselves. All are now fixed.

| ID | Severity | Title | Status | Evidence | Note |
|---|---|---|---|---|---|
| RV-01 | MED | (A-21/AN-03 fix) ExportCountry lost its display name — legacy CheckExportCountry sets the country EnglishName | FIXED | ApplyLegacyDisplayedValue ExportCountry branch; legacy CertificateOfOriginsBL.cs:1857-1874; test extended | Root: grouped by the helper's name, not its body. Prevent: parity-review follows every private helper to its last line |
| RV-02 | LOW-MED | (A-21/AN-03 fix) PortOfShipment display inverted — legacy's private GetIdByCode returns the parsed value, so a stored id gets the site name | FIXED | PortOfShipment joined SiteIdDetailTypes; legacy CertificateOfOriginsBL.cs:1876-1905; test extended | Same root as RV-01 |
| RV-03 | LOW | (AN-01 fix) a transient DB error retried the transaction with ids/row version from the rolled-back attempt → 409 | FIXED | SaveAttemptSnapshot restores ids + TimeStamp and clears the tracker on a retry only (PersistCertificateInOneTransaction) | Not exercised by tests (needs SQL Server) |
| RV-04 | LOW | (FN-03 fix) Postman SaveFile 80-child-edits asserted "unsent fields keep their values" | FIXED | Request round-trips docFileB; asserts docFileA importerId cleared (legacy golden) | Other SaveFile requests still send base fields only — check in the next live run |
| RV-05 | LOW | (BN-02 fix) api.openapi.json described RelatedEntities as VirtualEntityDto | FIXED | Superseded 2026-10-12: the spec now declares EventResponseArgs / RelatedEntity under PATCH | - |
| RV-06 | LOW-MED | (GN-04 fix) worker read the query URL after replacing the document and resetting the publishing flag; a failed read dead-lettered a half-done message | FIXED | LoadQueryUrlTemplate before any side effect | - |
| RV-07 | LOW | (GN-01) Attachment element nillable; legacy Attachment[] omitted it when absent | FIXED | XmlElement(Order = 1) without IsNullable; test asserts no element | - |
| RV-08 | LOW | (DN-01) test checked only the DTO default | FIXED | TheNoMatchingCertificateAnswerCarriesNullLists runs GetCertificateRequestByGuid | - |
| RV-09 | LOW | Worker attachment fileName ".pdf" (legacy in-process sent the template's FileName); worker trusts the payload status (no DB re-check after the 5 s delay); GenerateReport null leaves IsInPublishingProcess true | OPEN-BLOCKED | Worker imported as given; the original worker source is not available to compare | Confirm with Kobi |
