# הערכת זמנים — שלב 3, CertificateOfOrigins

2026-10-12 · תמר קליימן

## סיכום

**הערכה מעודכנת: 321–582 שעות מפתח, צפי סביר כ-450 שעות (כ-56 ימי עבודה, כ-3 חודשים למפתח אחד).** בסיום שלב 3 המודול מוכן ל-ROLLOUT ל-PRODUCTION: כל הפניות נכונות ומבצעות את כל מה שהלגאסי עשה. עם שני מפתחים במקביל: 7–9 שבועות קלנדריים.

**העיקרון:** שירות שקיים בלגאסי לא מפותח מחדש — בונים לו מעטפת בענן, במודול המתאים, שמפנה ל-WCF הלגאסי. פיתוח חדש נדרש רק כשאין שירות כזה בלגאסי.

**מיפוי מול הלגאסי (C:\Repos\Main) של 34 הקריאות היוצאות:**

- **28 קריאות — מעטפת בלבד.** כולן קיימות כפעולות בחוזי ה-WCF של 9 מודולים: Collateral, CommonServices, MessageManagement, Customers, CustomsBook, Documents, ExportDealFile, Users, Tasks. במיוחד, ExportDealFile **לא** צריך להיבנות מאפס — כל 6 הפעולות קיימות ב-`IExportDealFileExternalContract`.
- **6 קריאות — פיתוח חדש.** בלגאסי הן היו JOIN ישיר לטבלאות של מודול אחר בתוך SP, בלי שירות: שמות לקוחות, ספקים ומשתמשים לפי מזהים, מסמך לפי מזהה, תאריך הגשת תיק וכותרת התיק.
- **Lookups ו-NavigationPath** — בלגאסי נקראו דרך SystemTablesUtil או view. מוסיפים אותם כטיפוסי Lookup בתשתית.
- **ערוץ EAI היוצא** — קיים כבר בענן (`Common/OutgoingMessage`, ה-Worker משתמש בו). ה-talk-back לסוכן הוא פעולה של Documents (`SendMessageToAgent`), כלומר מעטפת.

**הנחות:**

- המפתח עובד ברשת הפנימית בלי Claude, בעבודה ידנית.
- יש דפוס קיים לקריאה מהענן ל-WCF לגאסי (`ICustomsWcfProxy` / Connected Services, כמו ב-Worker). מעטפת פשוטה היא 2–4 שעות, מורכבת 4–6.
- במודולים שכבר יש להם REST בענן (Customers, Documents, Tasks, Common, Users), כ-40% מה-routes לא יימצאו ויצריכו מעטפת.
- נכללת תוספת של 15% לתיאום מול צוותים ובדיקות קוד.

## לאן הולכות השעות

שליש מהשעות הולך לחיבור ל-9 מודולים חיצוניים. כל קריאה נוחתת בריפו של צוות אחר, ולכן גם מעטפת פשוטה נושאת תקורה קבועה: branch, Postman, PR, deploy ואימות חוזר במודול שלהם.

| קבוצה | חבילות | שעות | חלק (בצפי הסביר) |
| --- | --- | --- | --- |
| חיבור למודולים חיצוניים — 28 מעטפות + 6 endpoints | 1–3 | 107–204 | 34% |
| סגירת פערים מול הלגאסי — טקסטים, EAI, עמודות חסרות, החלטות | 5, 6, 7, 9 | 62–116 | 20% |
| תיאום ובדיקות קוד (15%) | — | 42–76 | 13% |
| תפעול — Worker, Planar, תבניות, ניתוב נכנס | 10–13, 16 | 38–66 | 12% |
| בדיקות מול שירותים אמיתיים | 14 | 40–64 | 12% |
| תלויות פלטפורמה — Lookups, Lock, RequestMetadata | 4, 8 | 32–56 | 9% |

הערך העליון מניח מקרה גרוע בכל סעיף. אם רוב ה-routes במודולים עם REST קיימים וההחלטות מתקבלות מהר, הסכום קרוב לערך התחתון (כ-320 שעות).

## פירוט לפי חבילות עבודה

המעטפות ללגאסי (חבילות 1–2) והבדיקות בסביבה הפנימית הן כשליש מההערכה. הפיתוח החדש בפועל מצומצם ל-6 endpoints.

| # | חבילת עבודה | מה כוללת | שעות (מינ׳–מקס׳) | ביטחון | תלוי ב |
| --- | --- | --- | --- | --- | --- |
| 1 | מעטפות — מודולים עם REST בענן | 14 קריאות (Customers 3, Documents 4, Tasks 3, Common 3, Users 1): אימות route ו-DTO, ומעטפת ללגאסי לכ-6 שחסרות | 33–72 | בינוני | צוותי המודולים |
| 2 | מעטפות — מודולים בלי REST בענן | 14 קריאות (ExportDealFile 6, Collateral 5, CustomsBook 3): הקמת לקוח WCF בכל מודול, 6 מעטפות פשוטות ו-8 מורכבות (כתיבות ערבות, DTO עשיר) | 50–84 | בינוני | צוותי המודולים |
| 3 | פיתוח חדש — 6 endpoints | מה שבלגאסי היה JOIN ב-SP: CustomersByIds, VendorsByIds, UsersByIds, Document/{id}, LeadDocumentSubmissionDate, LeadDocumentTitle | 24–48 | בינוני | Customers, Vendors, Users, Documents, ExportDealFile |
| 4 | Lookups בתשתית | 7 טיפוסים (בלגאסי דרך SystemTablesUtil) + NavigationPath (view בלגאסי), הסרת ה-stub המקומי | 26–44 | בינוני | צוות הפלטפורמה |
| 5 | הודעות וטקסטים (ValidationMessages / resx) | הפעלת `AddValidationMessages`, 16 `TODO(migration)`, קודי EMessages, טקסטי UIMessage | 16–28 | גבוה | חבילת BaseValidationMessages |
| 6 | ערוץ EAI יוצא | משוב 2281 בפרסום דרך `Common/OutgoingMessage` הקיים, מיפוי MessageTypeId (ה-talk-back נכלל בחבילה 1) | 8–16 | גבוה | שירות Common |
| 7 | שדות בלי מקור ושינויי סכמה | עמודות IsCreateAttachments / IsMessageSent, בדיקת title-mismatch, סינון EProfession, חותם וחתימה במכתב דרום קוריאה (אין שירות חתימה בלגאסי — פיתוח חדש או ויתור) | 22–40 | נמוך | החלטות מוצר |
| 8 | פריטי פלטפורמה | `RequestMetadata.User`, באג ה-Lock | 6–12 | בינוני | צוות הפלטפורמה |
| 9 | החלטות וסגירת פאריטי מול הלגאסי | `TODO(confirm)` (Canceled/Suspended, EN-01, אירוע 5458, ETemplate), ובדיקת פערים שעלו במיפוי (ראו "סיכונים") | 16–32 | בינוני | קובי / מוצר |
| 10 | הגדרות ניתוב נכנס בענן | רישום HandleDeliverySent ב-Events, ניתוב שער ה-EAI ופורטל ה-Web ל-routes החדשים | 6–12 | בינוני | Events, שער EAI |
| 11 | Worker — IssueCertificateOfOrigin | כתובות WCF, רישיות שמות שדות, הודעה מקצה לקצה, pipeline ל-host | 16–24 | בינוני | DevOps |
| 12 | Planar jobs | לוחות זמנים, מיפוי global params, deploy | 6–10 | גבוה | DevOps |
| 13 | תבניות | מכתב דרום קוריאה (CR 194221), העלאה ל-MinIO, כלי העלאה | 8–16 | בינוני | מוצר |
| 14 | בדיקות בסביבה הפנימית | 9 אוספי dependency-workload, internal-workload מלא, coverage, סבבי תיקון | 40–64 | בינוני | כל הנ״ל |
| 16 | ניקוי סימונים ב-Mocks | כ-50 הערות "dummy data", מסמכי מעקב | 2–4 | גבוה | — |
| | **סה״כ עבודה** | | **279–506** | | |
| | תיאום ובדיקות קוד (15%) | | 42–76 | | |
| | **סה״כ כולל** | | **321–582** | | |

## שירותים חיצוניים — מיפוי מול הלגאסי

כל שירות יעד קיים בלגאסי. השאלה היחידה לכל קריאה היא אם הלגאסי קרא לפעולה בחוזה (מעטפת) או קרא את הטבלה ישירות ב-SQL (פיתוח חדש).

| מודול יעד | REST בענן | מעטפת — פעולות בחוזה הלגאסי | פיתוח חדש (SQL בלגאסי) | שעות |
| --- | --- | --- | --- | --- |
| ExportDealFile | אין | 6: GetExportDeclarationDetailsForCertificateOfOrigion, GetLeadDocumentByOld…UpdateToNew…, GetLeadDocumentByCertificateOfOriginId, ChangeCertificateOfOriginIDForLeadDocument, GetExportDeclarationInfoForPC, GetDetailsForExportAssociatedGoodsItemsByLeadDocumentId | 2: SubmitDate, LeadDocumentTitle | 34–56 |
| Collateral | אין | 5: GetCollateralRequest, ChangeTempCollateralRequest(list, true), GetCollateralRequestIDsByRelatedEntityDTO, GrantAllCollateralRequests, DebitCreditCollateralRequest | — | 20–34 |
| CustomsBook | אין | 3: IsTradeAgreementForCountry, GetCustomsItemsByIdsSync, GetCustomsItemIdByFullClassificationSync | — | 10–18 |
| Documents | קיים | 4: GetDocumentsByEntitySync, DeleteDocument, AttachDocumentsToEntity, SendMessageToAgent | 1: Document/{id} | 12–26 |
| Customers | קיים | 3 (2 פעולות לגאסי): GetCustomerDTOByCustomerIdentificationSync_NotCached (מידע + לפי מזהה חיצוני), GetCustomeDTOByCountryID | 1: CustomersByIds | 9–20 |
| Tasks | קיים | 3: IsTaskExist, IsTaskExistsOnEntity, GetLatestUserHandlingEntityTasksWithTaskUnification | — | 6–14 |
| Common | קיים | 3: CreateQRCode, GenerateReportByParams / CreatePDFTemplateSync, MessageManagement.SendMessage | — | 6–14 |
| Users | קיים | 1: GetOrganizationUnitWithTypeByID (ה-IsCustomsHouse חושב בצד שלנו) | 1: UsersByIds | 6–14 |
| Vendors | קיים | — | 1: VendorsByIds | 4–8 |
| **סה״כ** | | **28** | **6** | **107–204** |

השעות כאן הן סכום חבילות 1–3 מהטבלה למעלה, מפורטות לפי מודול. קבצי החוזים הלגאסיים: ראו "מקורות".

## מחוץ לסקופ

שלב 3 מטפל רק בקוד הענן. שינויים בקליינט ה-WPF ובמודולים לגאסיים (למשל הפניית ExportDealFile הלגאסי ל-Reconcile) אינם בהערכה. גם ה-ROLLOUT הוא שלב נפרד: הרצת הסקריפטים, עדכון SchemaVersions, פרמטרים, הרצה מקבילה של שבועיים והעתקת נתונים.

## סיכונים ופערים שעלו במיפוי

רוב העבודה נעשית ברפו של צוותים אחרים, ולכן הקלנדר תלוי בהם יותר משעות העבודה.

- **UsersByIds — אין מקביל בלגאסי.** לא נמצאה שליפת שם משתמש בקוד או ב-SP של המודול (רק שם יחידה ארגונית). לבדוק אם הקריאה נחוצה או שזו התנהגות חדשה שצריך להסיר.
- **GetLeadDocumentByCertificateOfOriginId — אין קורא בלגאסי.** הפעולה קיימת בחוזה אבל לא נמצא מי שקורא לה. הקוד שלנו משתמש בה בביטול תעודה — לאמת שזו פאריטי.
- **חתימת משתמש במכתב דרום קוריאה — אין שירות בלגאסי.** זה CR חדש; צריך החלטת מוצר אם לפתח או לוותר.
- **פעולות לגאסי שלא ב-proxies שלנו:** UploadDocumentAndSave (Documents) ו-SendDeliverySync (Delivery). לאמת שהן מכוסות על ידי `IDocumentUtil` וה-Events; אם לא — עוד 2 מעטפות.
- **דגל `IsExportDeclarationActive`** — בלגאסי הפרסום האוטומטי (CheckCertificateOfOriginOnDeclarationReleased) מותנה בו. לוודא שהוא קיים כפרמטר.
- **גרסאות חבילות תשתית** — Lookup, BaseValidationMessages ו-Lock דורשות שחרור מצוות הפלטפורמה (חבילות 4, 5, 8).
- **תלות בזמינות הלגאסי** — המעטפות משאירות את ה-WCF הלגאסי כתלות של PRODUCTION. כשהמודולים האלה יומרו בעתיד, רק גוף המעטפת מתחלף — ה-route שלנו נשאר.

## מקורות

- `INTERNAL_INTEGRATION.md`, `MIGRATION-STATUS.md` וסריקת ה-TODO ב-`API/` (169 מופעים, 107 פעילים).
- מתאמי השירות בלגאסי: `C:\Repos\Main\CRM\CertificateOfOrigins\Server\Customs.CRM.CertificateOfOrigins.BL\ServiceAdapters\`.
- חוזי ה-WCF הלגאסיים (תחת `C:\Repos\Main\`):
    - ExportDealFile: `CRP\ExportDealFile\...\IExportDealFileExternalContract.cs`
    - Collateral: `FinanceInfr\Collateral\...\ICollateralExternalContract.cs`
    - CustomsBook: `KnowledgeStore\CustomsBook\...\ICustomsBookExternalContract.cs`
    - Documents: `Infrastructure\Documents\...\IDocumentsExternalContract.cs`
    - Customers: `StockPileData\Customers\...\ICustomersExternalContract.cs`
    - Tasks: `Infrastructure\Tasks\...\ITasksExternalContract.cs`
    - Common: `Infrastructure\CommonServices\...\ICommonServicesExternalContract.cs`, `IMessageManagementExternalContract.cs`
    - Users: `Infrastructure\Users\...\IUsersExternalServiceContract.cs`
- ה-SPs הלגאסיים עם ה-JOINs: `C:\Repos\Main\CustumsDev_Database\CustumsDev_Database\CRM\Stored Procedures\`.
