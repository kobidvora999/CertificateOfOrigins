# דוח חוסרים לפני הכנסה לסביבה פנימית — CertificateOfOrigins
תאריך: 2026-08-20 · Scope: all · Branch/Commit: `master` @ `fcaab2f`

## סיכום מנהלים
| בדיקה | סטטוס | חוסרים |
|---|---|---|
| 0. Conventions drift | ✅ | aligned-through **C12** = ה-changelog האחרון — אין drift |
| 1. כיסוי חוזים | ✅ | 34/36 הומרו (endpoint חי + BL אמיתי); 2 מושמטים בכוונה |
| 2. פאריטי התנהגותי | ✅ | fan-out אדוורסרי בוצע ב-`fbe1fa6` — 3 רגרסיות אותרו ותוקנו |
| 3. Code Review | ✅ | build נקי (0 errors, 0 warnings פרט S1135); 62 `TODO(blocking)` (כולם Mock→real) |
| 4. שלמות סקריפטי DB | ✅* | 9/9 dbo SPs שהקוד קורא קיימים כסקריפט; *from-zero replay לא הורץ |
| 5. Postman workload (קיום) | ✅ | internal-workload v3 (1 אוסף, last-run עבר 1/1) + dependency-workload קיימים; אין JSON פרוש |

**מוכן להכנסה לסביבה פנימית? כן — בהסתייגות** — הקוד בנוי, החוזים מכוסים, הפאריטי נבדק והסקריפטים קיימים; ההסתייגות היחידה היא שכל ה-proxies רצים ב-**Mock** וצריכים אישור-endpoint ומעבר ל-real במהלך האינטגרציה הפנימית (ראה `INTERNAL_INTEGRATION.md`).

## פירוט חוסרים (לפי חומרה)

### 🔴 חוסמים (Blockers)
- **אין חוסמים לפריסה עצמה.** ה-build נקי, כל ה-endpoints חיים, כל ה-SPs קיימים.

### 🟠 לטיפול לפני/במהלך אינטגרציה פנימית (Major)
- **62 `TODO(blocking)` — מעבר Mock→real proxies.** כל ה-proxies (Customer, Vendor, Collateral, Documents, ExportDealFile, CustomsBook, CommonServices, Tasks, MessageManagement, Country/CountryGroup, Site/InternationalSite, OrganizationUnit, MeasurementUnit, PackingType, CurrencyType, User) רשומים עם Mock ב-`ServicesConfiguration` + `TODO(blocking): confirm endpoint / CustomsMicroServices / switch to real`. לפני חיבור אמיתי צריך לאשר את שמות ה-endpoints ולכבות את ה-Mock. מרוכז ב-`INTERNAL_INTEGRATION.md` §1–2.
- **שדות deferred עם `TODO(blocking)`** (Model DTOs): `DocumentId` ב-web-query (Docs חוצה-סכמה), `LeadDocumentSubmissionDate`/`Document` ב-GetAuthenticationRequestByIdResultDto, ועוד — מוחזרים null עד שהשירות/סכמה הרלוונטיים זמינים.
- **C9/C10 שוברים חוזה כלפי קוראים** — שירותים קוראים צריכים לעדכן proxies (`.UseQueryMethod()` + routes חדשים). *(המסמך המפורט נשלח למפתחת; לא נכנס ל-repo לבקשתה.)*

### 🟡 לתשומת לב (Minor / Deferred)
- **2 אופרציות מושמטות בכוונה** (MIGRATION-STATUS): `GetPathsForNavigationToVendor` (מושהה עד תשובת קובי על מקור `NavigationPath`) + אופרציה נוספת שאינה נדרשת.
- **Coverage (הרצה חוזרת 2026-08-20 עם `tools/local-lookup-stub.js`, 37/37 → 200):** BL **59.4%** (CertificateOfOriginsBl 57.4%, AuthenticationRequestBl 82%, ExportDocumentAuthenticationRequestBl 78.6%, ServicesConfiguration 100%), DAL **69.8%** (Dal 87.2%, DbContext 86.3%), Model 15.1% (DTO holders). עלייה מ-45%→59.4% ב-BL אחרי שה-stub פתר את ה-500 של הלוקאפים. `CertificateOfOriginsConsts` 0% — consts בלבד.
- **from-zero replay של Scripts/ לא הורץ** — כל ה-SPs קיימים ב-localhost והשירות רץ מולם, אך שחזור-מאפס על DB נקי (‏`db-scripts-check`) לא בוצע בריצה זו.

## נספח: פירוט לכל בדיקה
- **CHECK 1:** 3 controllers (CertificateOfOrigins/Authentication/ExportDocument) מכסים External(7)+Internal(27)+Incoming(2). 34/36 עם BL אמיתי; GetPC_MSG2280_2281 ו-GetCertificateRequestByGuid כלולים.
- **CHECK 2:** בוצע fan-out סטטמנט-אחר-סטטמנט מול הלגסי (commit `fbe1fa6`), 3 רגרסיות מאומתות תוקנו (LeadDocumentId backfill · RejectCancelReason ב-events · ReconciliationFinding). לקחים ב-`PatternGaps/check2-migration-parity-lessons.md`.
- **CHECK 3:** `dotnet build --no-incremental` → Build succeeded, 0 errors, 0 warnings פרט S1135. אין PushUtil, אין `out of scope`, אין TODO(confirm) פתוח.
- **CHECK 4:** 9 dbo SPs (GetCertificateOfOriginsByFilter, GetCertificateOfOriginByID, GetCertificateOfOriginDataForWebQuery, GetCertificateOfOriginNumber, CheckIfExistsAdditionalRequestsFor{Importer,Vendor}, ExportDocumentAuthenticationRequestSearch, GetAuthenticationRequestByLeadDocumentID, GetImportAuthenticationRequestByFilter) — לכולם קובץ גרסה ב-`Scripts/`. + create schema/tables, seed data, add params.
- **CHECK 5:** internal-workload — אוסף v3 יחיד `CertificateOfOrigins Internal Workload - API` (37 בקשות), `last-run.json`: 1/1 passed. dependency-workload — אוספי `CertificateOfOrigins Dependency Workload - *` + runner. שני ה-runners קיימים. הרצה מקומית מלאה (עם lookup-stub): 37/37 → 200.
