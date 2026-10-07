# Parity ledger — מצב ממצאי הפאריטי (2026-10-07)

נבדק מחדש מול master (`ff134cd`) ב-2026-10-07 אחרי שסבב תיקונים נוסף נכנס (`1b4490e`, `729df1b`, `e151a03`, `fe13750`). נבנה מ-8 הנספחים `parity-A..H` מול הקוד הנוכחי ומול הקומיטים מאז 27.9. כל סטטוס נבדק בקוד ולא רק לפי שם קומיט; ממצאים שלא ניתן היה לאמת מסומנים UNVERIFIED.
שמות המזהים הם של הנספח המקורי (ב-C נוספה הקידומת C-, ב-F המזהים F-xx כבמקור, ב-D/E/G נוספה אות הנספח).

## סיכום

| סטטוס | משמעות | כמות |
|---|---|---|
| FIXED | תוקן בקוד | 65 |
| CLOSED-OK | נסגר: החלטת אנליסט / פאריטי מאומת / החלטת תכנון מתועדת | 40 |
| NOT-A-DEFECT-DOC | תיעוד בלבד | 7 |
| OPEN-DECISION | ממתין להחלטה של אדם | 10 |
| OPEN-BLOCKED | נדחה לשלב נפרד: תלוי בשירות/חבילה שעוד לא קיימים (לא נספר כפתוח) | 18 |
| OPEN-TODO | פער אמיתי שלא טופל | 0 |
| UNVERIFIED | לא ניתן היה לאמת | 2 |

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
| A-20 | LOW | QrCodePath stores ExternalId, not document path | FIXED | round of 2026-10-07 (parity ledger fixes commit): QrCodePath = IDocumentUtil.GetFileUrl(response.FileResource), the file-server URL legacy's GetDocumentFile stored; tests updated | Not verified against a live Documents service / printed certificate |
| A-21 | LOW | ExporterId DisplayedValue / site display fallback | FIXED | 1b4490e (ExporterId DisplayedValue left as sent) | - |
| A-22 | LOW | Validation shape: 400 instead of InfException | CLOSED-OK | Appendix itself: acceptable | HTTP mapping change by design |
| A-23 | LOW | GetById missing id returns 404 | CLOSED-OK | BL ~30-31 documented choice (RestNotFoundException) | Internal callers must handle 404 |
| A-24 | LOW | GetById milestones: INNER JOIN dropped, Name vs Title | OPEN-DECISION | BL FillMilestoneUserNames ~38-65 (user.Name, NULL for unknown) | UNVERIFIED whether Name equals legacy Title; low impact, needs sign-off |
| A-25 | LOW | ByFilter date bounds inline instead of ufn_General_GetDate* | UNVERIFIED | Scripts/API_20260721 ...ByFilter.sql:90,96 inline DATEADD | Legacy function bodies not compared; presumed equivalent |
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
| B-L1 | LOW | IsExportDeclarationActive gate dropped | OPEN-DECISION | CertificateOfOriginsBl.cs:1765 TODO(confirm) | Confirm param is always true, or reinstate gate |
| B-L2 | LOW | Legacy accumulated exceptions across certificates; new evaluates each separately | CLOSED-OK | Report recommends keeping new behaviour (silent fix); no code change needed | Not documented in code/MIGRATION-NOT-DONE; add a note if desired |
| B-L3 | LOW | Destination-group check edge cases differ (non-numeric group, null country) | OPEN-DECISION | CertificateOfOriginsBl.cs ~2240 (TryParse skip; null country flagged) | Report verdict PLAUSIBLE; no one decided; legacy edge cases are probable throws |
| B-L4 | LOW | Warnings-task assignment lacks profession/org-unit type | OPEN-BLOCKED | CertificateOfOriginsBl.cs:2148 TODO(migration) | Needs SingleUserAssignmentFilter support in event util |
| B-L5 | LOW | Exception texts placeholders, ExceptionType=0 | OPEN-BLOCKED | CertificateOfOriginsBl.cs:2177, 2551, 2560 TODO | Waits on ValidationMessages/resx package |
| B-L6 | LOW | Release publish does not send request-feedback message | OPEN-DECISION | CertificateOfOriginsBl.cs:1904 TODO(confirm) | Confirm drop or implement feedback send |
| B-L7 | LOW | Cancellation branch: agent message deferred, reason literal | OPEN-BLOCKED | CertificateOfOriginsConsts.cs:70-72; Bl.cs:1992 TODO | Needs resx text (EServerTerms.CanceledDeclaration) + agent message |
| B-L8 | LOW | HandleCertificateReplacement: FeedbackRemark, SendMessageToAgent, CertificateReplaced text deferred | OPEN-BLOCKED | CertificateOfOriginsBl.cs:1669, 1689 TODO | Reason now a literal (bb67197); UIMessage text and agent message remain |
| B-L9 | LOW | CR 194221: event 642 AdditionalInfo always the request-reason name | OPEN-DECISION | CertificateOfOriginsBl.cs:2085 TODO(blocking) | Two claimants for {2}; needs decision (intentional CR) |
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
| C-F13 | MED | Lead-document repoint by title not ported | FIXED | e3a65f8; NB LinkLeadDocument ~1470-1482 calls ChangeCertificateOfOriginIdForLeadDocument | ND comment at :60 still says "not repeated" (stale comment only) |
| C-F14 | LOW | Error accumulation/ordering differs from legacy hard-stops | CLOSED-OK | documented developer decision in code comment (NI:17-41) | response exception order differs, accepted |
| C-F15 | LOW | De-duplicated error emissions | CLOSED-OK | NB:264-270, NR:36-43 | fewer duplicate errors; harmless |
| C-F16 | LOW | Message params use enum identifiers not Hebrew table names | FIXED | 1b4490e + e151a03 (Hebrew table names); tests ff134cd | IllegalFirstCountryInAgreement params have no placeholder in the text, nothing to render |
| C-F17 | LOW | Whitespace certificateID gives CertificateDoesntExist | FIXED | 1b4490e; test ff134cd | - |
| C-F18 | LOW | Date detail Value stored as ISO "o" not culture format | OPEN-DECISION | NV:450 ToString("o"); the cloud reads (line ~524 invariant parse, ~637 he-IL parse) rely on it | culture-format Value would break the invariant parse at ~524; keep ISO or switch both ends? |
| C-F19 | LOW | Null item/invoice text becomes "" | CLOSED-OK | MIGRATION-STATUS.md 2026-10-07: the columns are NOT NULL | Closed on that statement; not independently re-verified here |
| C-F20 | LOW | Cancel reason hard-coded literal | CLOSED-OK | NB:~222 literal with explanatory comment | UIMessage catalogue unavailable |
| C-F21 | LOW | Cancel write stamps UpdateUserId 0 without user context | CLOSED-OK | Decision (Kobi, via the analyst, 2026-10-07): CC-USER-ID always carries the acting user | A 0 seen locally (no header) is acceptable; no system-user default is substituted |
| C-F22 | LOW | QueryUrl empty when Guid null (legacy always formatted) | FIXED | 1b4490e; test ff134cd | - |
| C-F23 | LOW | Latest-by-number/OriginCriterion lookups lack state filters | FIXED | round of 2026-10-07 (parity ledger fixes commit): certificate by number State = 1 + newest CreateDate (legacy SP); OriginCriterion State == 1 (SystemTables default); tests LatestCertificateByNumberTests, SystemTableActiveRowsTests | Exact number match kept on purpose (legacy SP used LIKE %n%) |
| C-F24 | LOW | Certificate number generated for reasons 6-9 | FIXED | 1b4490e (number generated only for reasons 1,2,10,4,5,12, as legacy's switch) | - |
| C-F25 | LOW | Stale SKELETON header comment | NOT-A-DEFECT-DOC | NB:70-77 still says create branch deferred | comment cleanup only |
| C-F26 | LOW | Mock proxies / TODO(blocking) deferrals | NOT-A-DEFECT-DOC | MIGRATION-NOT-DONE.md; CustomsBookProxy TODO | documented deferral |

## D+E — Web GUID and authentication reads

| ID | Severity | Title | Status | Evidence | Note |
|---|---|---|---|---|---|
| D-F1 | MEDIUM | Web response member names changed (exceptionDescription spelling, envelope fields dropped) | OPEN-DECISION | CertificateOfOriginsResponseDto.cs (ExceptionDescription, QueryUrl, DocumentId, no base envelope) | Deliberately left open (portal interface); needs a rename/shape decision with the portal owner |
| D-F2 | MEDIUM | Dates serialized as ISO datetime instead of yyyy-MM-dd | FIXED | b1cc78a (WebDateJsonConverter, CertificateOfOriginsBl.cs:564) | - |
| D-F3 | MEDIUM | DateOfDeclaration parsed with container culture | FIXED | b1cc78a (LegacyHostCulture he-IL / day-first fallback, CertificateOfOriginsBl.cs:617-630) | - |
| D-F4 | MEDIUM | Proxy failures/missing config now HTTP errors instead of in-band error; reverse NRE cases silent | OPEN-DECISION | Controller unchanged; badb7ea only maps external failures to 502/503; DocumentsProxy.cs:19 TODO(blocking) | Deliberately left open; no in-band error for proxy failures, Documents route unconfirmed |
| D-F5 | LOW | DocumentId now via Documents proxy; route unconfirmed, deleted-doc filtering may differ | OPEN-BLOCKED | DocumentsProxy.cs:19 TODO(blocking); MIGRATION-NOT-DONE decision #4 | Needs Documents service route confirmation |
| D-F6 | LOW | Empty-string guid query binds to null so "Invalid Guid" becomes "No Matching Certificate" | FIXED | e151a03 (empty guid/number kept as empty string) | Not verified with a real call: [DisplayFormat(ConvertEmptyStringToNull=false)] to be checked in Postman with CertificateOfOriginGuid= |
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
| G-F4 | LOW | HandleDeliverySent: TypeID fallback dropped | OPEN-BLOCKED | NOT-DONE:259; BL:355-356 | Depends on what the external Events service sends. |
| G-F5 | LOW | Importer delivery: null-file guard added (legacy threw NRE) | CLOSED-OK | BL:555 | More lenient, unreachable from the legacy client. |
| G-F6 | LOW | Importer reminder SP: Docs INNER JOIN existence filter gone | CLOSED-OK | SP 20260907101500 header note | Documented in the SP header. Rows without a Docs_Document are now returned. |
| G-F7 | LOW | Delivery flows return bool/slim DTO instead of full entity | FIXED | Delivery results carry every field the server changed (request: DecisionId, LastDeliveryForImporter, UpdateDate; file: status, delivery method, LastDelivery, UpdateDate), one clock passed BL to DAL; tests in DeliveryAuditUserTests | No GetById re-read: it calls Collateral/Tasks/Documents/ExportDealFile. FirstProvideContactDate not returned (legacy set it on the client) |
| G-F8 | LOW | CloseReminderTask Title is a literal | CLOSED-OK | BL:1301 FileEventTitle; F-06 review (partial:103-106) | The literal equals the legacy computed label. |
| G-F9 | LOW | Reminder job swallows per-row exceptions (legacy aborted) | FIXED | round of 2026-10-07 (parity ledger fixes commit): plain loop, no catch, effected rows = due count, as legacy ExecuteTask | Planar project builds |
| G-F10 | LOW | Scheduler UDF id to parameter-name mapping inferred | CLOSED-OK | Pairing confirmed: 5 ids by legacy DataScript inserts, 1148 = DaysForFirstReminderInAuthenticationRequest1 (value 6) checked by the analyst in the legacy General_enum_GlobalParam, 2026-10-07 | TODO(confirm) removed; comments in Schedulers.cs and Consts.cs cite the sources |
| H-1 | MED | GetById omits 6 columns (CreateDate, CreateUserId, UpdateDate, UpdateUserId, State, OrganizationUnitId) | FIXED | e151a03 (the 6 columns read in the narrow second query) | Not checked against a live DB |
| H-2 | MED | Optimistic concurrency added (legacy last-writer-wins) | OPEN-DECISION | c1b46d0 (partial) | A missing TimeStamp is now a 400. A stale TimeStamp still gives 409, which legacy did not. |
| H-3 | MED | Child merge not atomic with the parent | FIXED | c1b46d0; DAL:870-875 | Deletes staged through the tracker, one commit. |
| H-4 | MED | Attach/message VirtualEntity had no Title/CustomerId | FIXED | b7f2249; ExportBl ToRelatedEntity | OrganizationUnitId still not on VirtualEntityDto (open contract question, noted in code). |
| H-5 | LOW | Events raised after the commit, not before | NOT-A-DEFECT-DOC | report text ("arguably an improvement") | |
| H-6 | LOW | Null StatusId no longer throws after save | FIXED | round of 2026-10-07 (parity ledger fixes commit): StatusId required by the save validator (400 before any write); test ExportRequestStatusRequiredTests | Stricter than legacy, which saved and raised the events before failing |
| H-7 | LOW | Search INNER JOINs became IS NOT NULL guards | CLOSED-OK | DAL comment | Documented design choice. |
| H-8 | LOW | Search names from Customer.Name vs legacy Customers.Title | UNVERIFIED | ExportBl FillExportRequestNames | Needs a data check of Title vs Name in the Customers service. |
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
| X-2 | LOW | The certificate-type name ignores the type's State | OPEN-DECISION | CertificateOfOriginsBl.cs GetCertificateTypeName ([Display] of the enum); enum members 10 Guatemala and 11 EUR1-ACCUMULATION added (names were '10'/'11') | The seed has no inactive type today; reading the name from the table with State == 1 is the remaining option |
| X-3 | MED | Short dates printed in the container's culture | FIXED | LegacyHostCulture (he-IL, dd/MM/yyyy, '/'): every ToShortDateString in the BL; test LegacyShortDateTests | Under ICU even he-IL prints 25.9.2025; legacy printed 25/09/2025 |
