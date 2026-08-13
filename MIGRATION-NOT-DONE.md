# שירותים שלא הומרו — תיעוד (מתעדכן תוך כדי עבודה)

## Internal: SaveCertificateOfOrigin — ❌ לא בוצע (נדרשות הכרעות מפתח)

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

## Internal: SaveImportAuthenticationRequest — ❌ לא בוצע (תשתית הודעות חסרה)

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

## Internal: SaveAuthenticationRequestFile — ❌ לא בוצע (אותם חסמים כמו SaveImportAuthenticationRequest)

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

## Internal: SaveExportDocumentAuthenticationRequest — ❌ לא בוצע (תשתית הודעות)

**סיבות:**
1. **RaiseStatusMessage** — נקרא ב-2 מ-4 ענפי הסטטוס (ReadyForProfessionalTreatment + default שתופס את רוב
   הסטטוסים) ושולח SendMessageDTO(EMessageTypes.ImportRequestDecision) למשתמש הנוכחי — תשתית הודעות חסרה.
2. פערים משניים (פתירים): ‏AttachDocumentsToEntity חסר בפרוקסי Documents (endpoint לא מאומת);
   ‏UserUtil.Current.DisplayName (טקסט אירוע בעברית) — נדרשת דרך מאומתת לשם-תצוגה של המשתמש הנוכחי.

**מה כן מוכן:** ‏OriginalStatusId כבר עובר round-trip ב-DTO (נקבע ב-Get שהומר) — מנגנון זיהוי שינוי הסטטוס
פתור לתחום זה; כל הישויות והילדים הומרו; ערכי EExportAuthenticationRequestStatus חולצו (1-9);
מיפוי האירועים מלא (ExportAuthenticationRequestFileStatusUpdate=1282 + 3 אירועי ענף).

## Incoming: GetPC_MSG2280_2281_CertificateOfOriginRequest — ❌ לא בוצע

**סיבות:**
1. **תלוי ב-SaveCertificateOfOrigin** (שנדחה) — זהו הצרכן המרכזי של זרימת השמירה המלאה
   (`bl.SaveCertificateOfOrigin(certificateToResponse, null, _certificateToUpdateId, ...)`).
2. **תשתית תשובת EAI חד-כיוונית** — התשובה (PC_NG_2281_MSG02) נמסרת דרך callback/MSMQ
   (`GetServiceCallback(...).OnGetPC_MSG2280_2281_CertificateOfOriginRequestComplete(response)`) —
   נדרשת הכרעת עיצוב על צורת ה-endpoint הסינכרוני + מי מלקוחות ה-EAI צורך אותו.
3. תלות ב-ExportDealFile adapter (אין שירות DealFile), נעילה מבוזרת (LockFactory), יצירת צרופות.
4. מנוע ולידציה רפלקטיבי של ~900 שורות (30+ שדות תעודה) — יחידת עבודה גדולה בפני עצמה.

**המלצה:** להמיר אחרי פתיחת חסמי SaveCertificateOfOrigin; מנוע הולידציה ראוי להמרה כ-FluentValidation
בשלב נפרד עם המפתח.

## Internal: GetPathsForNavigationToVendor — ❌ לא בוצע (טבלת תשתית חוצת-DB)

קורא את טבלת `NavigationPath` (T_1696) מ-DB התשתית (`InfrastructureConsts.InfrastructureORMMapping` —
חיבור שונה מזה של המודול), PathID=359. אין util/proxy ל-NavigationPath ביעד, והטבלה אינה בבעלות המודול.
ייתכן שהמנגנון (ניווט תפריטים של הקליינט הישן) כלל אינו רלוונטי ב-SPA החדש — **לברר עם הצוות לפני שבונים
גישה חדשה**.

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
