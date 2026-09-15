# שירותים שלא הומרו — תיעוד (מתעדכן תוך כדי עבודה)

> ## ✅ עדכון סטטוס — ההמרה הושלמה (2026-08-17)
>
> מצאי אדוורסרי מלא הצליב את **3 חוזי ה-WCF** (36 אופרציות) מול ה-controllers + ה-BL על master:
> **35/36 הומרו** (endpoint חי + BL לא-stub; אין `NotImplementedException`/`#error` בקוד). אופרציה **אחת בלבד**
> לא הומרה, בכוונה: `TempSync` (stub מת בלגסי).
>
> **תוקן 2026-09-15 (אודיט נאמנות):** הניסוח הקודם כאן ספר 34/36 וטען ש-`GetPathsForNavigationToVendor` הושמטה
> בכוונה — **זה לא נכון יותר**. היא הומרה חלקית ב-2026-08-20 (endpoint חי `GET AuthenticationRequest/PathsForNavigationToVendor`
> + BL + DTOs), ומחזירה `ViewPaths` ריק עם `TODO(blocking)` עד שתתווסף טיפוס Lookup‏ `NavigationPath` בפלטפורמה.
> ראה MIGRATION-STATUS.md — שם הרישום מדויק.
>
> **⚠️ הסעיפים הבאים במסמך זה מיושנים (STALE) — המתודות כבר הומרו על master** (endpoint חי + BL אמיתי;
> החסמים שהם מציינים — תשתית Notifications, `ICollateralProxy`, `AttachDocumentsToEntity`, template — נפתרו
> מאז דרך `MessageManagementProxy.SendMessage`, `CollateralProxy`, `DocumentsProxy.AttachDocumentsToEntity`,
> `CommonServicesProxy.GenerateTemplate/SSRS`). ייתכנו TODO(blocking) שיוריים של אימות נתיבי-endpoint ל-rollout:
> **SaveCertificateOfOrigin · SaveImportAuthenticationRequest · SaveAuthenticationRequestFile ·
> SaveExportDocumentAuthenticationRequest · UpdateCetrificateOfOrigins**. הסעיפים נשמרים כהיסטוריית-הכרעות בלבד.

## Internal: SaveCertificateOfOrigin — ✅ הומר על master (הסעיף למטה מיושן — נשמר כהיסטוריה)

**סיבות:**
1. **זיהוי שינוי סטטוס/הערות מבוסס `ChangeTracker.OriginalValues`** — הלוגיקה המקורית קוראת את הערכים
   המקוריים מה-self-tracking entity שהגיע מהלקוח (`certificateOfOrigin.ChangeTracker.OriginalValues`),
   ומחליטה לפיהם אם הסטטוס/הערות השתנו (isStatusChanged/isRemarksChanged) — מה שמפעיל שרשרת אירועים
   ופעולות המשך. ב-REST אין self-tracking: נדרשת הכרעת עיצוב — האם להשוות מול שורת ה-DB לפני העדכון,
   או לקבל את הערך הקודם מהלקוח. לפי כלל "לעולם אל תנחש מקור-ערך" — עצירה.
2. **מתודות המשך שלובות באותה אופרציה WCF** (מופעלות ב-InternalSaveCertificateOfOrigin אחרי השמירה):
   - `SendRequestFeedback` — בניית הודעת פידבק PC_NG_2281 ושליחה דרך SendMessages (תשתית הודעות).
   - `CreateAttacmentsAndSendFeedBackMessage` — הפקת מסמכים/תבניות + צרופות (TEMPLATE_PRINT + Documents).
   - `HandleCertificateReplacement` — טיפול בהחלפת תעודה.
   - `CheckCertificateOfOriginOnDeclarationReleased` → `DeclarationReleased` — סנכרון מול הצהרת יצוא.
   בלעדיהן ההתנהגות שונה מהמקור (הנחיה 10); הרחבתן = יחידות מיגרציה נפרדות גדולות התלויות בתשתיות
   הודעות/תבניות שטרם קיימות בשירות היעד.
3. **פרוקסי חדשים עם endpoints לא מאומתים:** ExportDealFile (4 מתודות), CommonServices.CreateQRCode,
   CustomsBook.IsTradeAgreementForCountry, Tasks.GetLatestUserHandlingEntityTasksWithTaskUnification,
   הרחבות Customer/User proxies, והעלאת קובץ QR ל-Document repository.
4. שדות transient על הישות (CertificateOfOriginIdOfReplacement, ExportDeclarationDetailsDTO) ולוגיקת
   `_requestExceptions` שנצברת ולא נזרקת (bug-for-bug שדורש אישור מוצר).

**מה כן קיים מוכן לשימוש עתידי:** כל ישויות ה-DB, ‏CertificateOfOriginDto המלא, ‏Customer/User proxies.

**המלצה:** לפרק לשלב נפרד עם מפתח: (א) הכרעת מנגנון original-values, (ב) אימות endpoints,
(ג) מיגרציה של 4 מתודות ההמשך כיחידות עצמאיות לפני ה-Save עצמו.

## Internal: SaveImportAuthenticationRequest — ✅ הומר על master (הסעיף למטה מיושן — נשמר כהיסטוריה)

**סיבות:**
1. **SendDecisionMessage** — בענף ה-default של ה-switch על DecisionID נשלחת הודעת החלטה למשתמשים דרך
   `ServicesAdapter.SendMessage(SendMessageDTO)` (SendMessages infra). המקבילה ב-.NET 10 היא חבילת
   `CustomsCloud.Infrastructure.Notifications` שאינה מותקנת בריפו, ואין proxy הודעות מאומת. השמטה שקטה
   של ההודעה = שינוי התנהגות (הנחיה 10); `#error MIGRATION` ישבור את ה-build לכל שאר המודול.
2. אירוע `AuthenticationRequestRejected` עם `TaskAssignmentArguments.SingleUserTaskAssignmentFilter` —
   נדרש אימות שה-IEventUtil builder תומך בהקצאת משימה למשתמש בודד.
3. פרוקסי Tasks (‏IsTaskExistsOnEntity, IsTaskExist) ו-Collateral (‏ChangeTempCollateralRequest) — קיימים
   שלדים בריפו אך ה-endpoints לא מאומתים (ב-master היו קיימים חלקית — לשחזר בעת ההמרה).

**מה כן מוכן:** טבלת CertificateOfOrigins_ImportAuthenticationRequest ממופה במלואה (דו"ח ה-EDMX בניתוח),
טבלת ההחלטות CertificateOfOrigins_enum_Decision קיימת מקומית.

**המלצה:** להתקין את חבילת Notifications, לאמת את endpoint ה-SendMessage, ואז המתודה ניתנת להמרה מלאה
(שאר הלוגיקה — save יחיד + אירועים — סטנדרטית).

## Internal: SaveAuthenticationRequestFile — ✅ הומר על master (הסעיף למטה מיושן — נשמר כהיסטוריה)

**סיבות:**
1. **שתי שליחות הודעות** — `SendDecisionMessage` (הודעת החלטה לכל בקשה ששונתה) ו-`RaiseStatusMessage`
   (הודעת עדכון סטטוס תיק) דרך `ServicesAdapter.SendMessage` — תשתית ההודעות חסרה בשירות היעד.
2. **זיהוי דלתא מבוסס שדות Original** (`OriginalRequestDecisionID`, `OriginalAuthenticationFileStatusID`,
   `AuthenticationFileStatusIDPrev`) שממולאים ע"י ה-client-side change tracking של WCF — כל מכונת
   הסטטוסים (9 אירועים, פתיחת/סגירת משימות, ביטול תיק, מתן ערבויות) מותנית בהם. נדרשת הכרעת עיצוב:
   client שולח old+new, או השוואה מול ה-DB בצד השרת (מומלץ).
3. פערי proxy: ‏`ICollateralProxy` חסר `GetCollateralRequestIDsByRelatedEntity` ו-`GrantAllCollateralRequests`.

**מה כן מוכן:** כל הישויות וה-DTOs; ‏GetAuthenticationRequestFileByID (שה-save מחזיר בסופו) כבר הומר;
כל 9 האירועים ממופים במלואם בניתוח (types+args) — מוכנים ליישום ברגע שהחסמים ייפתרו.

## Internal: SaveExportDocumentAuthenticationRequest — ✅ הומר על master (הסעיף למטה מיושן — נשמר כהיסטוריה)

**סיבות:**
1. **RaiseStatusMessage** — נקרא ב-2 מ-4 ענפי הסטטוס (ReadyForProfessionalTreatment + default שתופס את רוב
   הסטטוסים) ושולח SendMessageDTO(EMessageTypes.ImportRequestDecision) למשתמש הנוכחי — תשתית הודעות חסרה.
2. פערים משניים (פתירים): ‏AttachDocumentsToEntity חסר בפרוקסי Documents (endpoint לא מאומת);
   ‏UserUtil.Current.DisplayName (טקסט אירוע בעברית) — נדרשת דרך מאומתת לשם-תצוגה של המשתמש הנוכחי.

**מה כן מוכן:** ‏OriginalStatusId כבר עובר round-trip ב-DTO (נקבע ב-Get שהומר) — מנגנון זיהוי שינוי הסטטוס
פתור לתחום זה; כל הישויות והילדים הומרו; ערכי EExportAuthenticationRequestStatus חולצו (1-9);
מיפוי האירועים מלא (ExportAuthenticationRequestFileStatusUpdate=1282 + 3 אירועי ענף).

## Incoming: GetPC_MSG2280_2281_CertificateOfOriginRequest — ✅ הומרה (2026-08-17; 4 החסמים העסקיים נסגרו)

**מה הומר ונבדק חי (2026-08, endpoint `POST CertificateOfOrigins/CertificateOfOriginRequest`):**
- **חוזה סינכרוני** — הכרעת מפתח: ה-callback/MSMQ החד-כיווני הוחלף ב-endpoint סינכרוני שמחזיר את הפידבק
  ישירות (כמו האחות GetCertificateRequestByGuid). שגיאות מוחזרות **in-band** (HTTP 200 + exceptions), לא נזרקות.
- **נעילה מבוזרת** — `ILockUtil` (חבילת `CustomsCloud.InfrastructureCore.Lock` 1.10.11), מגודר בפרמטר
  `IsNeedToLockCertificateOfOrigin` — נאמן ל-LockFactory הלגסי.
- **ענפי read/cancel** — GetRequestStatus + CertificateCancellation (‏DAL set-based write + IEventUtil).
- **מנוע הוולידציה המלא** (הכרעת מפתח: מתודת BL processing, לא FluentValidation — כי כל שדה גם *פותר* ערך async
  וגם *בונה* detail): לולאה מונעת-DB (`DetailsPerCertificate`), ~30 field-validators (country/Israel/agreement/
  group/date/site/city/bool), cross-field (‏EUR1/EURMED · cumulation · consignee · place-of-manufacture+zip ·
  manifest · CustomsHouse→org-unit), invoice/item (shape · customs-item 6-digit · origin-criterion · container-ISO ·
  currency/packing/measure). קטלוג הודעות מרכזי (`EMessageCode`, טקסטים מ-UIMessage) — ExceptionType נושא את הקוד.
- **מספר תעודה** — `dbo.GetCertificateOfOriginNumber` + sequence (סקריפט; sequence היה חסר מקומית).
- **lookups חדשים** ל-SystemTables: Country(alpha-2)/Site/InternationalSite/PackingType/MeasurementUnit/CurrencyType-by-code
  (proxies +mocks), CountryGroup-existence. `OriginCriterion` — entity + DAL מקומי (הטבלה בבעלות המודול).

**4 החסמים העסקיים — נסגרו:**
1. **per-reason resolution** (‏`ResolveCertificateForReason`) — ✅ מתודת switch 9-reason + helpers (Update: התאמת
   agent/type/status · Replacement: cancel-id+status · published/cancelled guards) + קביעת `CertificateIdToCancel`/
   `CertificateToReplaceInImport`; supersession דרך `SaveCertificateOfOrigin`. (commit `1f43cff`)
2. **שמירת invoices/items** — ✅ `SaveCertificateOfOrigin` overload + `SaveInvoiceDetails` (diff-merge, empty-list guard). (commit `1f43cff`)
3. **CheckCertificateOfOriginOnDeclarationSubmited** (reconciliation post-save דרך UpdateCertificateOfOrigins) +
   **amendment-linkage guard** — ✅ מחווטים + memoization ל-`GetExportDeclarationDetailsForCertificateOfOrigion`. (commits `07345b0`, `aa3b410`)
4. **NonManipulation** — ✅ פיצול השער (גוף `NonManipulationCertificate` במקום `CertificateOfOrigin`), מיפוי 15 השדות
   (ids 34-48, כולל המיפוי ל-enum ה"משובש" `ExportBillOFLadingNum`/`TransirCountry`), `AddCustomsHouseDetail`
   (הוספת CustomsHouse ללא-תנאי כשגוף CertificateOfOrigin נלווה), cross-field (‏ManifestNumber/ImportDate), דילוג על
   invoices+reconciliation. אומת אדוורסרית מול הלגסי + מול ה-DB.

**פערי-נאמנות שהתגלו באודיט הפאריטי של NonManipulation ותוקנו (מאומתים חי/DB):**
- **org-unit 0 באירועים** — הלגסי `EventUtil.RaiseEvent` סבל org-unit 0 (NonManipulation ללא CustomsHouse);
  ה-builder ב-.NET10 דרש `>0`. 3 מתודות אירוע מחילות `WithOrganizationUnitId` רק כאשר `>0`.
- **`EnrichAndValidateDetails`** (מיחידה #33, פורט מצומצם) — הושלם נאמנה ל-`CheckSpecificField` הלגסי בזמן-save:
  CustomsHouse → Value=org-unit id + DisplayedValue=שם org-unit · תאריכים → `ToShortDateString` · בוליאני → Yes/No.
  משפר נאמנות גם ל-endpoint הישיר `SaveCertificateOfOrigin`.

**עוד לא-חוסם (מגבלת סביבה, לא קוד):** seed ל-Redis של `City`/`OrganizationUnit` (‏ILookupUtil) נדרש לבדיקת save מלאה
מקומית (שם ה-org-unit ב-DisplayedValue נופל ל-id בסביבה לא-seeded).

## Internal: GetPathsForNavigationToVendor — 🟡 הומרה חלקית (2026-08-20) — הסעיף הישן להלן היה שגוי

קורא את טבלת `NavigationPath` (T_1696) מ-DB התשתית (`InfrastructureConsts.InfrastructureORMMapping` —
חיבור שונה מזה של המודול), PathID=359 — מנגנון ניווט התפריטים של הקליינט ה-WPF הישן.

**המצב בפועל (אומת באודיט 2026-09-15):** בניגוד לניסוח הקודם כאן ("מושמטת בכוונה, לא רלוונטי ל-SPA",
הכרעת 2026-08-17) — ההכרעה **התהפכה ב-2026-08-20** והמתודה כן הומרה חלקית:
`GET AuthenticationRequest/PathsForNavigationToVendor` ‏(`Ui/AuthenticationRequestController.cs`) + BL + DTOs.
‏`PathId` מאוכלס (`NavigationToVendorPathId = 359`); **`ViewPaths` מוחזר תמיד ריק** תחת `TODO(blocking)` עד
שהפלטפורמה תוסיף טיפוס Lookup‏ `NavigationPath` ושירות-מקור שיחשוף `GET /lookup/NavigationPath`. מיפוי השדות
שהלגסי ביצע נשמר כקוד מוער לעתיד. **החלקיות היא בדיוק כפי שמתועד ב-MIGRATION-STATUS — אין פער נוסף.**

## Internal: LoadDataFromExportDeclaration — ✅ הומר (2026-07-05, branch `feature/migrate-load-data-from-export-declaration`)

הומר במלואו: endpoint‏ `QUERY Ui/CertificateOfOrigins/LoadDataFromExportDeclaration` המקבל
`LoadDataFromExportDeclarationRequestDto` (‏`LeadDocumentId` + `ExportDeclarationNumber` + `RequestReasonCode` בלבד)
ומחזיר **`bool`** — הערך שהלקוח הישן הציב ב-`IsDeclarationReleasedAndNotRetrospectiveCertificate`.
(התיאור הקודם כאן — "מקבל `CertificateOfOriginDto` ומחזיר אותו מועשר" — היה **שגוי/מיושן**, תוקן באודיט 2026-09-14.
הלגסי אמנם חתם `IsDeclarationReleased`/`IsCargoExitedOfCustomsRegulation` על הישות ב-ref, אך החוזה אינו `ref`
והקליינט קרא רק את ה-bool המוחזר — ולכן ההשמטה חסרת-השפעה, אומת מול הקליינט הלגאסי.)
נוצרו `IExportDealFileProxy` + `ExportDealFileProxy` + `ExportDealFileMockProxy`; ה-**Mock רשום ב-DI**.
`TODO(blocking)`: מעבר ל-proxy האמיתי + אימות שם ה-endpoint כשיוקם שירות ExportDealFile
(הערך `CustomsMicroServices.ExportDealFile` קיים ב-enum ומקומפל).

## Internal: SaveCertificateOfOrigin — 🟡 הומר, 3 חסמים פתוחים (אודיט נאמנות 2026-09-14)

הזרימה מומרת בפועל (הסימון 🔴 הקודם ב-MIGRATION-STATUS היה מיושן). אודיט אדוורסרי מול הלגסי תיקן שניים
(org-unit+reason באירוע ה-supersede; המרת code→ID ל-DestinationCountry/PortOfShipment בישות חדשה) והשאיר שלושה
חסומים — **כולם מסומנים `TODO(blocking)` בקוד** ב-`CertificateOfOriginsBl.SaveCertificateOfOrigin` (בלוק הפרסום):

1. **פידבק+צרופה על Publish לא נשלח.** לגסי `CreateAttacmentsAndSendFeedBackMessage` → `SendRequestFeedback(cert, attachments)`
   (`CertificateOfOriginsBL.cs:397-418`) שלח ללקוח את התעודה המרונדרת. הערוץ (`OutgoingMessageProxy`, EAI) **הוסר
   מהפלטפורמה** (InfrastructureCore.Utils ≥ 1.10.105) ואין תחליף — ראה `_shared/outgoing-message-pattern.md`.
2. **אין פרסום-אוטומטי כשההצהרה כבר משוחררת.** לגסי `CheckCertificateOfOriginOnDeclarationReleased`
   (`CertificateOfOriginsBL.cs:671-694`, נקרא מ-`CertificateOfOriginsInternalServicePartial.cs:61-64`) דרך
   `CheckDeclarationStatus` ששואל את **ExportDealFile** — שירות שטרם הוקם (mock בלבד). אותה תלות חוסמת גם את
   ה-repoint של lead-document ואירוע אי-התאמת-הכותרת בזרימת ההודעה הנכנסת.
3. **אין guards אי-כפילות.** לגסי חסם פרסום/פידבק חוזרים עם `IsCreateAttachments` / `IsMessageSent`
   (`CertificateOfOriginsBL.cs:381,424`). העמודות **אינן קיימות** בטבלת `.NET10` ולא בישות → נדרש שינוי סכימה
   (DB + entity + seed) לפני שאפשר לשחזר את ההגנה. עד אז שמירה חוזרת בסטטוס Published מייצרת את התבנית מחדש.

## External: UpdateCetrificateOfOrigins — ✅ הומר (2026-07-07, branch `feature/migrate-update-cetrificate-of-origins`)

הומר במלואו: `POST CertificateOfOrigin/UpdateCetrificateOfOrigins` — dispatcher על 5 אירועים (240/1423/1790/334/554)
→ 4 מתודות BL + כל ה-helpers. החסמים ההיסטוריים נפתרו עם תשתיות חדשות שנלמדו בסקילים: IOutgoingMessageUtil
(פידבק PC_NG_2281), IQueueUtil (הנפקה ע"י worker), IDocumentUtil (QR/צרופות), רינדור התעודה דרך SSRS
(Common.GenerateTemplate), ValidationMessages+resx (טקסטי מערכת). Build ✅.
**הכרעת קובי (2026-08): רינדור רק דרך SSRS** — אין צורך בתבניות מקומיות. הפיילוט הגנרי של התבניות
(ITemplateUtil + dbo.usp_Template_INNER_CROSS_CertificateOfOrigin + DTOs + 2 endpoints) נמחק; הזרימה תמיד פונה
ל-`commonServicesProxy.GenerateTemplate` (SSRS).
**חסמים שנותרו (TODO(blocking) בקוד):** טקסטי resx מטבלת UIMessage הפנימית; חבילת BaseValidationMessages
טרם ב-feed החיצוני; CountryCountryGroup חסר בתשתית lookup (2 בדיקות מדולגות); ערכי SendService/DestinationExternalId
של הודעת הפידבק; endpoints לא מאומתים (Common, CustomsBook, Tasks, ExportDealFile);
אימות עמודות IsCreateAttachments/IsMessageSent מול המונוליט.

## External: TempSync — ⏭️ לא נדרש

‏stub מת ב-WCF (`throw new NotImplementedException()`) — לא הועבר בכוונה.

## Incoming: GetCertificateRequestByGuid — ✅ הומרה (2026-07-28)

שאילתת אימות תעודה לפורטל הציבורי (GetPC_Web_9096_CertificateRequest). הומרה end-to-end: DTOs, SP רב-תוצאות
`dbo.GetCertificateOfOriginDataForWebQuery` (5 result sets, QueryMultiple ב-DbContextExtension), DAL, BL
(`GetCertificateRequestByGuid` ב-CertificateOfOriginsBl), ו-GET endpoint ב-CertificateOfOriginsController.

**הכרעות מפתח שהתקבלו (2026-07-28):**
1. **result set 5 בלי IsToPrint** (blocker #1 המקורי) — אומת מול המקור **וגם** מול העותק הפרוס ב-Scripts:
   ה-SP באמת לא מחזיר IsToPrint (הטבלה הזמנית `#CertificateDetailsTypeCodeForWebDisplay` היא dead code).
   **הוכרע: לשמר bug-for-bug** — Consignee ב-EUR1/EURMED לעולם לא מודפס.
2. **FieldID של ExportDeclarationNumber** — טענת ה-NRE הייתה שגויה: `[FieldID(20661)]` קיים בישות.
   הערכים (20306/20310/20661) נכתבו כקבועים ב-BL (ה-DTO ביעד לא נושא attributes → אין reflection).
3. **lookups** — אומת ב-reflection ש-`CurrencyType` ו-`DataDictionaryField` **לא קיימים** כ-lookup type בפלטפורמה
   (`ILookup` אף לא מכיל `CurrencyCode`). לכן ההחלטה המקורית "ILookupUtil.Get<T>" בלתי-אפשרית. **הוכרע (2026-07-28):
   שניהם דרך proxy ל-`CustomsMicroServices.SystemTables` + MockProxy** — `IDataDictionaryFieldProxy` (תוויות
   EnglishName) ו-`ICurrencyTypeProxy` (CurrencyCode). CurrencyCode מאוכלס בפועל (נבדק חי מול mock → "ILS").
4. **DocumentID** — נפתר במקור ב-SP מ-Infrastructure.Docs_* (חוצה-סכמה). **הוכרע: 0/NULL + TODO(blocking)**;
   ה-JOIN הוסר מה-SP (מחזיר NULL). לפתור עתידית דרך שירות Documents.
5. **באג קדימות אופרטורים** בפילטר החשבוניות (blocker #5 המקורי) — **הוכרע: לשמר bug-for-bug**
   (IsToPrint מגביל רק MERCOSUR). נוספו סוגריים מפורשים שמשמרים את ההתנהגות (דרישת SA1408).
6. **CertificateOfOriginItemDetailDTOs** — תמיד רשימה ריקה (dead-init לגאסי). **הוכרע: לשמר.**

**נפתר מאז (2026-07-28):** SP הוחל ואומת מול DB חי (פרמטרים תואמים) + סקריפט גרסה ב-Scripts/ ·
`CertificateOfOriginQueryURL` קיים ומאומת ב-Infrastructure.Parameters (ה-TODO השגוי הוסר) ·
CurrencyCode מומש דרך `ICurrencyTypeProxy` · נבדק חי end-to-end (GET מול השירות).

**חוסרים חוסמים שנותרו (Manual follow-up — blocking):** ראה TODO(blocking) בקוד —
(1) **DocumentID** מוחזר NULL/0 (ה-JOIN ל-Infrastructure.Docs_* הוסר מה-SP) — לפתור דרך שירות Documents.
(2) **אימות נתיבי endpoint ב-SystemTables לפני rollout:** `CurrencyType/CurrencyTypesByIds` ו-
`DataDictionaryField/DataDictionaryFieldsByIds` (נכתבו כ-best-guess).

## חוצה-מתודות: פריטים פתוחים מאודיט הנאמנות (2026-09-14/15)

האודיט האדוורסרי כיסה 29 אופרציות מ-3 החוזים. רוב הפערים תוקנו (ראה MIGRATION-STATUS); אלה נשארו פתוחים
ואינם תלויים בנו:

### 🛑 מגבלת 30 עמודות ב-`MaxCountExceededInterceptor` — ממתין לנוגט

‏`dbo.GetExportDocumentAuthenticationRequestById` מקרין **29 מתוך 35 עמודות** ומשמיט את
`State`, `CreateDate`, `CreateUserId`, `UpdateDate`, `UpdateUserId`, `OrganizationUnitId` — כי ה-interceptor של
הפלטפורמה זורק על 30+ עמודות בתוצאה. בגלל זה גם נתיב ה-update ב-BL קורא מחדש ומשחזר `State`/`OrganizationUnitId`
לפני שמירה, כדי לא לאפס אותן.

**נבדק 2026-09-15 על `InfrastructureCore.DAL` 1.10.58** (הגרסה החדשה ביותר שפורסמה ל-feed) עם קריאה מלאה של
35 עמודות + ‏`.ExcludeInterceptor("j4XSVK6Fl8")`: **עדיין נכשל בזמן ריצה** —
`DbInterceptionException: Result fields count (35) exceeded max eror level of 30 with query -- j4XSVK6Fl8`.
ה-tag אכן נכנס ל-SQL, כלומר ה-API עובד — אבל ה-interceptor מתעלם ממנו. ⚠️ הקומפילציה עוברת, אז **חובה לאמת
בזמן ריצה** (‏`GET ui/ExportDocumentAuthenticationRequest/{id}`) ולא להסתפק ב-build.

**מה חוסם:** פרסום גרסת DAL חדשה שמכבדת את ה-exclusion (או רישום המודול ב-`InterceptorList` של הפלטפורמה).
**כשזה יקרה:** להחליף את ה-projection ב-`.Include(...)` + `.ExcludeInterceptor("j4XSVK6Fl8")`, לאמת בריצה,
ואז להחליט אם לחשוף את 6 השדות ב-`GetExportDocumentAuthenticationRequestByIdResultDto` (כרגע אינם בחוזה).

### ⚠️ "Pattern A" — סינון סטטוס משימה (`TODO(confirm)` בקוד)

הלגסי סינן משימות ב-`TaskStatusID != 2` (כל מה שאינו סגור). ב-.NET 10 אין סינון בצד ה-SP
(ענפי הסטטוס ב-`usp_Tasks_IsTaskExist` מוערים), ולכן מסננים בצד שלנו על `IsTaskInProgress` — שהוא
`TaskStatusID IN (1,4)`. **הפער שנותר:** משימות ב-Canceled(3)/Suspended(5) נחשבו "קיימות" בלגסי ואינן נחשבות
אצלנו. מופעים: `GetAuthenticationRequestFileByID`, `GetAuthenticationRequestByID`, `IsCurrentUserHandleFile`,
ושני ה-Planar jobs.

### ⚠️ `SaveAuthenticationRequestFile` — שינוי התנהגות שדורש אישור מוצר

קוד ה-collateral (‏grant על RightAuthenticationAnswer / debit על WrongAuthenticationAnswer) **פעיל** ב-.NET 10,
בעוד שבלגסי המתודה שמכילה אותו (`CheckStatus`) הייתה **dead code** שלא נקראה מאף מקום — כלומר פרודקשן מעולם לא
הפעיל את ה-side-effect הזה מול שירות ה-Collateral. זה תוקן במכוון (`8d46e33`, "Restore the file-level collateral
outcome"), אך מדובר בשינוי מול ההתנהגות בפועל — ראוי ל-sign-off מפורש.

### ⚠️ `HandleAuthenticationRequestDeliverySent` — סטייה לא-מוכחת

הלגסי איתר את הישות הקשורה לפי `EntityType` **או** `TypeID` (שני שדות נפרדים ולא-מסונכרנים על `IEntity`).
המומר בודק `EntityType` בלבד, ול-`VirtualEntityDto` אין `TypeId` כלל. אם שירות ה-Events מאכלס רק `TypeID`,
ההתאמה תיכשל בשקט והמתודה תחזיר `false` היכן שהלגסי החזיר `true`. לא ניתן להוכיח מהרפו — המפיק חיצוני;
דורש בדיקה מול חוזה שירות ה-Events.
