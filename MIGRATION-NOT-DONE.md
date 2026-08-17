# שירותים שלא הומרו — תיעוד (מתעדכן תוך כדי עבודה)

> ## ✅ עדכון סטטוס — ההמרה הושלמה (2026-08-17)
>
> מצאי אדוורסרי מלא הצליב את **3 חוזי ה-WCF** (36 אופרציות) מול ה-controllers + ה-BL על master:
> **34/36 הומרו** (endpoint חי + BL לא-stub; אין `NotImplementedException`/`#error` בקוד). 2 האופרציות
> שלא הומרו — **שתיהן בכוונה**: `TempSync` (stub מת בלגסי) ו-`GetPathsForNavigationToVendor`
> (הוכרע 2026-08-17: לא רלוונטי ל-SPA).
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

## Internal: GetPathsForNavigationToVendor — ⏭️ לא נדרש (הכרעת מוצר 2026-08-17: לא רלוונטי ל-SPA)

קורא את טבלת `NavigationPath` (T_1696) מ-DB התשתית (`InfrastructureConsts.InfrastructureORMMapping` —
חיבור שונה מזה של המודול), PathID=359 — מנגנון ניווט התפריטים של הקליינט ה-WPF הישן.
**הוכרע (2026-08-17): המנגנון אינו רלוונטי ב-SPA החדש → המתודה מושמטת בכוונה** (כמו TempSync). אין צורך
בגישה חוצת-DB ל-NavigationPath ביעד. אם ייווצר צורך עתידי — יידרש proxy לשירות התשתית שחושף את הטבלה.

## Internal: LoadDataFromExportDeclaration — ✅ הומר (2026-07-05, branch `feature/migrate-load-data-from-export-declaration`)

הומר במלואו: endpoint‏ `POST Internal/LoadDataFromExportDeclaration` המקבל `CertificateOfOriginDto` ומחזיר אותו
מועשר (`IsDeclarationReleased`, `IsCargoExitedOfCustomsRegulation`, וכן
`IsDeclarationReleasedAndNotRetrospectiveCertificate` — השדה שהלקוח הישן הציב מה-bool המוחזר).
נוצרו `IExportDealFileProxy` + `ExportDealFileProxy` + `ExportDealFileMockProxy`; ה-**Mock רשום ב-DI**.
`TODO(blocking)`: מעבר ל-proxy האמיתי + אימות שם ה-endpoint כשיוקם שירות ExportDealFile
(הערך `CustomsMicroServices.ExportDealFile` קיים ב-enum ומקומפל).

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
