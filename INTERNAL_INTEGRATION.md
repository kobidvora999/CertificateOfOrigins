# INTERNAL_INTEGRATION — CertificateOfOrigins
עדכון אחרון: 2026-09-27 · Branch/Commit: `master` @ `7615be3` · מקור: `Postman/internal-workload-test/reports/pre-internal-gap-report_2026-09-27.md`

> נכתב מחדש מאפס בריצת repo-complete-check של 2026-09-27. ברשת הפנימית אין Claude — כל מה שנדרש לשלבי
> האינטגרציה מרוכז כאן. מה שלא ניתן לדעת מבחוץ מסומן `לבירור בפנים`.

## 1. שירותים שהמיקרו-סרוויס צורך (outbound) — 19 proxies
כל ה-proxies רשומים ב-`API/CertificateOfOrigins.BL/ServicesConfiguration.cs` כ-`AddProxy<I, Real, Mock>` — ה-Real הוא ברירת
המחדל, ה-Mock רץ רק עם header `x-mock-mode`. **כל ה-routes מסומנים `TODO(blocking): confirm endpoint`** (41 מופעים).

| Proxy | שירות יעד (`CustomsMicroServices.X`) | Resource | מצב צפוי בפנים | פעולה בפנים |
|---|---|---|---|---|
| CustomerProxy | Customers | `api/Customer/...` | REST קיים (לאמת) | אמת route · הפנה |
| VendorProxy | Vendors | `api/Vendor/...` | REST קיים (לאמת) | אמת route · הפנה |
| UserProxy | Users | `api/User/...` | REST קיים (לאמת) | אמת route + חוזה `UserDto` (`TODO(blocking)` ב-`UserDto.cs:11`) |
| ExportDealFileProxy | ExportDealFile | `api/ExportDealFile/...` (7 routes) | **שירות לא עומד** | אם אין — פתח REST חדש (היה JOIN ל-CRP.*) |
| DataDictionaryFieldProxy | SystemTables | `api/DataDictionaryField/...` | לבירור בפנים | אמת route |
| CurrencyTypeProxy | SystemTables | `api/CurrencyType/...` | REST קיים (לאמת) | אמת route |
| CountryProxy | SystemTables | `api/Country/CountriesByAlphaCodes` | REST קיים (לאמת) | אמת route |
| SiteProxy | SystemTables | `api/Site/...` | REST קיים (לאמת) | אמת route |
| InternationalSiteProxy | SystemTables | `api/InternationalSite/...` | REST קיים (לאמת) | אמת route |
| PackingTypeProxy | SystemTables | `api/PackingType/...` | REST קיים (לאמת) | אמת route |
| MeasurementUnitProxy | SystemTables | `api/MeasurementUnit/...` | REST קיים (לאמת) | אמת route |
| CustomsBookProxy | SystemTables | `api/CustomsBook/...` | **לא מאושר** | אמת שירות יעד (אולי אינו SystemTables) |
| OrganizationUnitProxy | SystemTables | — | **לא מאושר** | אמת שירות יעד |
| CountryGroupProxy | SystemTables | — | **לא מאושר** | אמת route |
| DocumentsProxy | Documents | `api/Document/...` | REST קיים (לאמת) | אמת route (4 routes) |
| CollateralProxy | Collaterals | `api/Collateral/...` | REST קיים (לאמת) | אמת route + חוזה `CollateralRequestDto` + משמעות הארגומנט `true` שנשמט ב-`ChangeTempCollateralRequest` |
| TasksProxy | Tasks | `api/Task/...` | REST קיים (לאמת) | אמת route (3) |
| MessageManagementProxy | Common | `api/Message/...` | REST קיים (לאמת) | אמת route; `SendRequestFeedback` שולח `MessageTypeId=0` — לתקן לפני חיבור |
| CommonServicesProxy | Common | `api/CommonServices/...` (QR + SSRS GenerateTemplate) | **לא מאושר** | אמת route (`CommonServicesProxy.cs:24` `TODO(blocking)`) |

בנוסף — תשתית (לא proxy): `ITemplateUtil` (`AddTemplateUtil`, Templates microservice + MinIO bucket `templates`), `IQueueUtil`,
`ILockUtil`, 4 lookups ב-`ILookupUtil` (Country/City/DocumentType/OrganizationUnit), `IParametersUtil`.

## 2. MockProxies להחלפה בפנים
אין proxy שהוא mock-only. ה-Mock הוא ברירת המחדל **המעשית** בטסט עבור 5 שירותים שעוד אינם עומדים/מאושרים — לחבר ל-Real בפנים:
| MockProxy | Proxy אמיתי | תלוי בשירות |
|---|---|---|
| ExportDealFileMockProxy | ExportDealFileProxy | ExportDealFile (לא עומד) |
| CustomsBookMockProxy | CustomsBookProxy | SystemTables? (לא מאושר) |
| CommonServicesMockProxy | CommonServicesProxy | Common — QR + SSRS |
| OrganizationUnitMockProxy | OrganizationUnitProxy | לא מאושר |
| CountryGroupMockProxy | CountryGroupProxy | SystemTables (לא מאושר) |
13 הערות `switch to ...Proxy` ב-`ServicesConfiguration.cs` (שורות 33–92) — לעבור עליהן אחת-אחת בזמן החיבור.

## 3. צרכנים של השירות (inbound — מי קרא ל-WCF הישן)
| מודול צורך | מתודת WCF ישנה | Endpoint חדש | פעולה |
|---|---|---|---|
| CRM WPF client (מסכי תעודות / בקשות אימות / מסמכי יצוא) | 27 פעולות `ICertificateOfOriginsInternalContract` | `ui/CertificateOfOrigins/*`, `ui/AuthenticationRequest/*`, `ui/ExportDocumentAuthenticationRequest/*` | הפנה ל-REST החדש (ה-SPA) |
| ExportDealFile / DealFile (שחרור הצהרה) | `UpdateCetrificateOfOrigins` (External) | `POST api/CertificateOfOrigins/Reconcile` | הפנה ל-REST החדש |
| External — שאר הפעולות | GetCertificateOfOriginID, GoodsItemCerificateDTO, Convert, SaveAttachments, HandleAuthenticationRequestDeliverySent | `api/CertificateOfOrigins/{ID,GoodsItemCerificateDTO,Convert,SaveAttachments}`, `api/EventsResponse/HandleDeliverySent` | צרכנים מדויקים — לבירור בפנים |
| EAI — הודעות סוכן מכס (PC_NG_2280/2281) | `GetPC_MSG2280_2281_CertificateOfOriginRequest` (Incoming) | `POST community/CertificateOfOrigins/Request` | הפנה; ⚠️ `CustomerId` נלקח מגוף ההודעה — לוודא שה-gateway דורס אותו |
| פורטל Web (שאילתת תעודה לפי GUID) | `GetCertificateRequestByGuid` (Incoming ForWeb) | `GET web/CertificateOfOrigins/RequestByGuid` | הפנה; ⚠️ שמות שדות התשובה ופורמט התאריכים השתנו — לתאם עם הפורטל |
| הפצה ("דיוור") — מכתב דרום-קוריאה | — (חדש, CR 194221) | `GET api/CertificateOfOrigins/Template/{templateId}/{entityId}` | לתאם חוזה + `templateId` (`TODO(confirm)`, כרגע 1) |
| Planar | ScheduledTasks: `ReminderForImporterScheduler`, `AuthenticationRequestReminder` | Planar jobs תחת `Planar/` | לפרוס את ה-jobs |
רשימת צרכנים מלאה מעבר למודול CRM — לבירור בפנים.

## 4. DB — סקריפטים ו-ROLLOUT
סקריפטים להרצה בפנים (סדר לקסיקלי = סדר הרצה, DbUp), מ-`API/CertificateOfOrigins.WebApi/Scripts/` — 18 קבצים:
1. `API_20260715 - create schema.sql`
2. `API_20260715 - create tables.sql` ⚠️ לא idempotent (בלוק FK בסוף) — להריץ פעם אחת בלבד
3. `API_20260715 - seed data.sql`
4. `API_20260716 - add params.sql`
5. `API_20260721 - dbo.GetCertificateOfOriginsByFilter.sql`
6. `API_20260722 - dbo.CheckIfExistsAdditionalRequestsForVendor.sql`
7. `API_20260722b - dbo.CheckIfExistsAdditionalRequestsForImporter.sql`
8. `API_20260726130203 - dbo.GetImportAuthenticationRequestByFilter.sql`
9. `API_20260726165855 - dbo.GetAuthenticationRequestByLeadDocumentID.sql`
10. `API_20260727172359 - dbo.GetCertificateOfOriginByID.sql`
11. `API_20260728122720 - dbo.GetCertificateOfOriginDataForWebQuery.sql`
12. `API_20260813180359 - seed CountryIsrael parameter.sql`
13. `API_20260813183834 - dbo.GetCertificateOfOriginNumber + sequence.sql`
14. `API_20260907101500 - dbo.GetImportAuthenticationRequestsForReminderForImporterScheduler.sql`
15. `API_20260907101600 - dbo.GetAuthenticationRequestsForScheduler.sql`
16. `API_20260907180000 - dbo.GetImportAuthenticationRequestByFilter multi-invoice.sql`
17. `API_20260922093000 - seed AdministrativeClosure decision and file status.sql` (UTF-8 בלי BOM — `sqlcmd -f 65001`)
18. `API_20260923104500 - dbo.GetTemplateData.sql`

🛑 **חסר סקריפט:** `CRM.CertificateOfOrigins_CustomsItemToExportDocumentAuthenticationRequest` — חייב להיווצר לפני העלייה
(Save של בקשת מסמך יצוא כותב אליה).

שינויי DB **מחוץ לרפו** שנדרשים:
- `Infrastructure.Tasks_enum_TaskType` (181, `PC_Tsk01_NewCertificateOfOriginCheck`) — `Description` צריך להיות
  `'טיפול בבקשה להנפקת תעודת מקור - ({2})'` (CR 194221).
- ערכי פרמטר לאימות מול הסביבה: `CertificateOfOriginQueryURL` (ה-seed מכיל כתובת DEV ולא דורס ערך קיים),
  `CertificateOfOriginsDocumentsFilter` (ב-localhost `329,461`; ב-seed `124,184,318,185,175,126,18`).

ארטיפקטים שאינם DB:
- תבנית `ExportAuthenticationSendDeliveryForExporterSouthKoreaTemplate.docx` + `.yml` — **לא נמצאה באף מקום**; להשיג ולהעלות ל-MinIO bucket `templates`. אין `tools/upload-templates.ps1`.

צ'ק-ליסט:
- [ ] הרצת 18 הסקריפטים + סקריפט הטבלה החסרה
- [ ] עדכון `Tasks_enum_TaskType` 181
- [ ] העלאת התבנית ל-bucket
- [ ] שבועיים קוד חדש על DB ישן
- [ ] יום ROLLOUT — העתקת נתוני הטבלאות ל-DB החדש

## 5. חוסרים פתוחים שחוסמים אינטגרציה (מדוח 6a)
- 11 פערי פאריטי HIGH מאומתים: F-01 (יצירת בקשה), F-02/E-F3 (אובדן עריכות מסך התיק), F-03 (collateral מקוד מת), G-F1
  (`FirstProvideContactDate`), A-02 (event WithoutTask), B-H1/B-H2/B-H3 (Reconcile), C-01/C-02/C-03 (הודעה נכנסת).
- רינדור תבנית CR 194221: double-encode (`CertificateOfOriginsBl.Templates.cs:31`) + קובץ תבנית חסר.
- proxies הופכים 404 ל-500 (`BaseCustomsProxy.ExecuteAsync`) — טיפול ה-"not found" ב-BL לא רץ.
- 41 routes של proxies לא מאושרים + 5 שירותים שה-Mock הוא ברירת המחדל המעשית שלהם.
- probes של dependency-workload (Group 1) שבורים (routes בלי `ui/`/`community/`) — לתקן לפני ריצת פרה-פרוד.
