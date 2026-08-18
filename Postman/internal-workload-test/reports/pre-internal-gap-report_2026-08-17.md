# דוח חוסרים לפני הכנסה לסביבה פנימית — CertificateOfOrigins
תאריך: 2026-08-17 · Scope: all (כל השירות) · Branch/Commit: master @ b2d63fa

## סיכום מנהלים
| בדיקה | סטטוס | חוסרים |
|---|---|---|
| 1. כיסוי חוזים | ✅ | 0 (34/36 הומרו; 2 מושמטים בכוונה) |
| 2. פאריטי התנהגותי | ✅/⚠️ | fan-out מלא רץ (7 סוכנים, 34 מתודות, 2026-08-18). 3 רגרסיות קונקרטיות תוקנו; 2 מחלקות רוחביות + דחיות מתועדות → הכרעת קובי. פירוט: PatternGaps/check2-migration-parity-lessons.md |
| 3. Code Review | ✅ | build ✅ 0 errors · **warnings-clean פרט ל-S1135** (תוקן 2026-08-18: 30 warnings — S125/S6580/CS8629/S1172/S1643/S2166/S3267) · NU1900 סביבתי בלבד (feed לא נגיש) · 62 TODO(blocking) נשארים כמצאי מכוון (S1135) |
| 4. שלמות סקריפטי DB | ✅ | כל 9 ה-SPs שהקוד מפעיל מסוקרפטים; 6 SPs `dbo` ב-localhost הם orphans (לא בשימוש) |
| 5. Postman workload collections | ✅ | Group 1 (9 אוספי dependency) + Group 2 (internal v3 + baseline coverage) + runners — נוצרו (2026-08-18) |

**מוכן להכנסה לסביבה פנימית? כמעט** — **אין חוסם 🔴**. CHECK 2 (fan-out פאריטי) רץ 2026-08-18: 3 רגרסיות
קונקרטיות תוקנו; נותרו 2 הכרעות רוחביות לקובי (fail-fast→silent · אטומיות טרנזקציה) + דחיות מתועדות. CHECK 3/4/5 תקינים.

## פירוט חוסרים (לפי חומרה)

### ✅ CHECK 4 — תקין (הבהרה 2026-08-17: החשד הראשוני היה false-positive)
1. **כל 9 ה-SPs שהקוד מפעיל בפועל — מסוקרפטים** ב-`Scripts/`: `GetCertificateOfOriginsByFilter` ·
   `GetImportAuthenticationRequestByFilter` · `ExportDocumentAuthenticationRequestSearch` ·
   `GetAuthenticationRequestByLeadDocumentID` · `CheckIfExistsAdditionalRequestsForVendor` ·
   `CheckIfExistsAdditionalRequestsForImporter` · `GetCertificateOfOriginByID` ·
   `GetCertificateOfOriginDataForWebQuery` · `GetCertificateOfOriginNumber`.
   **6 SPs `dbo` שהופיעו ב-localhost הם orphans** (‏`GetAuthenticationRequestsForScheduler`,
   `GetExportDocumentAuthenticationRequestByID`, `GetImportAuthenticationFileDetailsAndRequests`,
   `GetImportAuthenticationRequestById`, `GetImportAuthenticationRequestsForReminderForImporterScheduler`,
   `UpdateImportAuthenticationRequest`) — **הקוד המומר מימש כל אחת מהן inline** (LINQ/`ExecuteUpdateAsync`, למשל
   `LinkRequestsToAuthenticationFile` — הכרעת מפתח 2026-07-30 להחליף את ה-SP+TVP), ופרויקט Planar ריק. **0 הפעלות
   ב-DbContextExtension/DAL/Planar** → אינם נדרשים כסקריפט (שאריות ניסוי ב-localhost). הבדיקה המוסמכת: `/db-scripts-check`.

### ✅ CHECK 5 — תוקן (2026-08-18): שתי קבוצות ה-workload קיימות
2. **Group 2 (`internal-workload-test`)** — אוסף v3 `CertificateOfOrigins Internal Workload - API` ב-`Postman/postman/collections/`,
   רץ תחת `dotnet-coverage` (‏runner ב-`Postman/internal-workload-test/run.ps1`). **verdict ירוק** (passed:1, 37 בקשות, 0 כשלים).
   כיסוי per-class: Controllers 91–100% · DAL 80% · Auth/Export BL 77–78% · **CertificateOfOriginsBl 35.5%** (צינור ה-message —
   דורש scenario-variants + lifecycle כדי להעלות; ה-follow-up העמוק דרך `postman-coverage`). כלים הותקנו: Postman CLI 1.47, dotnet-coverage, reportgenerator.
   - **Group 1 (`dependency-workload-test`)** — 9 אוספי v3 `CertificateOfOrigins Dependency Workload - {SystemTables,Customers,Users,
   Vendors,ExportDealFile,Documents,Collaterals,Common,Tasks}` ב-`Postman/dependency-workload-test/collections/` (lint נקי 9/9),
   מפה + placeholders ב-`collections/gap.md`, runner ב-`Postman/dependency-workload-test/run.ps1`. **הרצה = pre-prod עם proxies אמיתיים (תשתית)** —
   לא רצה מקומית לפי תכנון ה-skill; repo-complete-check דורש רק קיום.

### ✅ CHECK 3 — תוקן (2026-08-18): warnings-clean פרט ל-S1135
3. כל 30 ה-warnings הלא-S1135 טופלו (‏`/net10-code-review`): **S6580** (5× date-parse → `CultureInfo.InvariantCulture`) ·
   **S125** (5× הערות שנראו כקוד → נוסחו מחדש) · **CS8629** (null-safe pattern ב-EnrichAndValidateDetails) ·
   **S1643** (StringBuilder ב-BuildReconciliationAdditionalInfo) · **S2166** (‏`ReconciliationException`→`ReconciliationFinding`) ·
   **S1172** (הוסר פרמטר `agentRequest` לא-בשימוש) · **S3267** (דוכא נקודתית — לולאת ולידציה per-invoice עם side-effects).
   Build `--no-incremental` → **S1135 בלבד** (‏NU1900 הוא feed-availability סביבתי, לא warning קוד). אומת חי מול internal-workload.
4. **CHECK 2 — fan-out פאריטי מלא רץ (2026-08-18, 7 סוכנים אדוורסריים, 34 מתודות).**
   **3 רגרסיות קונקרטיות תוקנו:** ‏(HIGH) SaveExportDoc — State/OrganizationUnitId נדרסו ל-0 בעדכון (guard ב-DAL) ·
   ‏(MEDIUM) UpdateCertificateOfOrigins — backfill של LeadDocumentId בלי בדיקת התאמת-מספר (הוחזר) ·
   ‏(MAJOR) SaveCertificateOfOrigin — אירועי Rejected/Cancelled איבדו RejectCancelReason (הוחזר).
   **2 מחלקות רוחביות → הכרעת קובי:** ‏(א) fail-fast→silent (‏`?? default` על נתון עסקי במקום throw) ·
   ‏(ב) אובדן אטומיות טרנזקציה (set-based writes בלי BeginTransaction עוטף).
   **דחיות מתועדות (‏TODO(blocking)):** ‏DeclarationReleased sync · SendRequestFeedback/הודעות/תבניות · DocumentId · repoint שני של DealFile.
   **‏3 quirks של GetCertificateRequestByGuid — אומתו שנשמרו bug-for-bug.** לקחים מלאים: `PatternGaps/check2-migration-parity-lessons.md`.
5. **CHECK 0b.5 — conventions drift אפשרי.** `.claude/conventions-state.md`: aligned-through **C5** (2026-07-22).
   אם ה-changelog התקדם מאז → הרץ `/repo-align`.

### 🟡 לתשומת לב (Minor / Deferred)
6. **62 `TODO(blocking)`** — פריטי rollout מתועדים (mock→real proxy, אימות נתיבי endpoint ל-SystemTables/Documents/
   ExportDealFile). לא חוסמים מקומית (mock פעיל) אך **יישברו בשקט** במעבר לשירותים אמיתיים. ראה INTERNAL_INTEGRATION.md.

## נספח: פירוט לכל בדיקה
- **CHECK 1:** מצאי אדוורסרי מלא — 3 חוזי WCF (7 External + 27 Internal + 2 Incoming = 36). 34 ✅ (endpoint חי + BL אמיתי,
  ללא `NotImplementedException`/`#error`). 2 מושמטים בכוונה: `TempSync` (stub מת), `GetPathsForNavigationToVendor`
  (הכרעת מוצר 2026-08-17: לא רלוונטי ל-SPA).
- **CHECK 3:** `dotnet build --no-incremental` → **0 errors, 182 warnings** (רובן S1135/TODO). מלאי TODO: 153 סמנים
  (62 `TODO(blocking)`, 0 `TODO(confirm)`, 0 `#error`, 0 `NotImplementedException`).
- **CHECK 4:** 30 קבצי `.sql` ב-Scripts/. SPs של CertificateOfOrigins מכוסים (GetCertificateOfOriginByID/Number/
  DataForWebQuery/ByFilter). 6 SPs של Authentication/ExportDocument חסרי סקריפט (ראה חוסם #1).
- **CHECK 5:** ראה חוסם #2.
