# דוח חוסרים לפני הכנסה לסביבה פנימית — CertificateOfOrigins
תאריך: 2026-08-17 · Scope: all (כל השירות) · Branch/Commit: master @ b2d63fa

## סיכום מנהלים
| בדיקה | סטטוס | חוסרים |
|---|---|---|
| 1. כיסוי חוזים | ✅ | 0 (34/36 הומרו; 2 מושמטים בכוונה) |
| 2. פאריטי התנהגותי | ⚠️ | לא-רץ-מלא (fan-out 34-מתודות לא בוצע בריצה זו) |
| 3. Code Review | ⚠️ | build ✅ 0 errors · ~40 warnings לא-S1135 (מהם 2 חדשים) · 62 TODO(blocking) |
| 4. שלמות סקריפטי DB | ✅ | כל 9 ה-SPs שהקוד מפעיל מסוקרפטים; 6 SPs `dbo` ב-localhost הם orphans (לא בשימוש) |
| 5. Postman workload collections | ❌ | Group 1 + Group 2 (v3) + runners חסרים |

**מוכן להכנסה לסביבה פנימית? לא** — נותר חוסם ב-CHECK 5 (אוספי workload חסרים); הקוד אינו warnings-clean. **CHECK 4 תקין.**

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

### 🔴 חוסמים (Blockers)
2. **CHECK 5 — אוספי ה-workload חסרים** תחת `Postman/`:
   - Group 2 (`internal-workload-test`, coverage+life-cycle) — אין אוספי v3 `CertificateOfOrigins Internal Workload - *`.
   - Group 1 (`dependency-workload-test`, liveness חיצוני) — אין אוספי `CertificateOfOrigins Dependency Workload - *`.
   - חסרים runners לשתי הקבוצות. קיים רק אוסף העבודה `Postman/CertificateOfOrigins.postman_collection.json` (JSON — לא v3).
   **תיקון:** הרץ `/internal-workload-test CertificateOfOrigins` ו-`/dependency-workload-test CertificateOfOrigins`.

### 🟠 לטיפול לפני merge (Major)
3. **CHECK 3 — הקוד אינו warnings-clean** (מדיניות: נקי פרט ל-S1135). ~40 warnings לא-S1135, בעיקר:
   S125 (קוד מוער) · S6580 (date parse ללא format provider) · S1172 (`agentRequest` לא בשימוש) · S1643 · S2166 · S3267.
   **2 חדשים מריצה זו:** S6580 ב-[MessageValidation.cs:488](API/CustomsCloud.CRM.CertificateOfOrigins.BL/CertificateOfOriginsBl.MessageValidation.cs:488)
   ו-[CertificateOfOriginsBl.cs:1138](API/CustomsCloud.CRM.CertificateOfOrigins.BL/CertificateOfOriginsBl.cs:1138) (הוסף `CultureInfo.InvariantCulture` ל-`DateTime.TryParse`).
   **תיקון:** `/net10-code-review CertificateOfOrigins`.
4. **CHECK 2 — פאריטי סטייטמנט-לרמה לא רץ כ-fan-out מלא** על 34 המתודות בריצה זו. פאריטי אומת אינקרמנטלית
   בכל commit של המרה + אודיט אדוורסרי עמוק ל-GetPC_MSG2280_2281 (סשן זה) + תיקון EnrichAndValidateDetails.
   **המלצה:** fan-out ייעודי (סוכן אדוורסרי למתודה) לכיסוי מלא, או לפחות למתודות בסיכון גבוה (Save*/Update*).
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
