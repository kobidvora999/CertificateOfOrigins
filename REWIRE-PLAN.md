# תוכנית חיווט LINQ→SP — כל 10 המתודות (2026-07-05)

**החלטת מפתח:** כל 10 המתודות שהומרו יחזרו לקרוא לעותקי ה-`dbo` (במקום LINQ).
**עיקרון:** העשרת שמות חוצי-שירות (לקוחות/משתמשים/מדינות/יחידות) נשארת ב-BL דרך proxies/lookup —
ה-SPs המקומיים לא יחזירו JOINs לטבלאות שלא קיימות ב-DB של השירות (db-proc Pattern B/F).

## סטטוס

| # | פרוצדורה | מתודה | ניתוח SQL נדרש | סטטוס |
|---|---|---|---|---|
| 1 | dbo.CheckIfExistsAdditionalRequestsForVendor | CheckIfExistsAdditionalRequestsForVendor | עצמאית — אימות לוגיקה מול LINQ (חלון 3 שנים, ‏>1) | ✅ חווטה |
| 2 | dbo.CheckIfExistsAdditionalRequestsForImporter | CheckIfExistsAdditionalRequestsForImporter | ‏GlobalParam → פרמטר `@DaysForLastDelivery`; ‏BL הפסיק לקרוא IsVendorDeliveryCountryConfigured (הוסרה מה-DAL) | ✅ חווטה |
| 3 | dbo.GetAuthenticationRequestByLeadDocumentID | GetAuthenticationRequestByLeadDocumentIDs | ‏3 JOINs הוערו → NULL; ‏BL lookup נשאר | ✅ חווטה |
| 4 | dbo.GetImportAuthenticationRequestById | GetAuthenticationRequestByID | ‏query-multiple; ‏ItemDetails מה-set השני מוצמד ב-DAL; ‏set 3 ריק (Documents proxy) | ✅ חווטה |
| 5 | dbo.GetCertificateOfOriginsByFilter | GetCertificateOfOriginsByFilter | ‏ufn_GetMaxRows→200, ‏GetDateStart/End→CAST; ‏JOIN לקוחות הוסר, ‏E.ID/C.ID→F.*; ‏BuildParameterForProcedure ב-BL | ✅ חווטה |
| 6 | dbo.GetImportAuthenticationRequestByFilter | GetAuthenticationRequestByFilter | ‏5 JOINs הוסרו; ‏CONTAINS→LIKE (אין FTS מקומי); ‏date-utils→CAST; ‏BuildParameterForProcedure (18 פרמ') | ✅ חווטה |
| 7 | dbo.CROSS_ExportDocumentAuthenticationRequestSearch | GetExportDocumentAuthenticationRequestSearch | ‏JOINs הוסרו + נוספו עמודות id גולמיות להעשרת BL; ‏WHERE שומר סמנטיקת INNER JOIN; ‏CONTAINS→LIKE | ✅ חווטה |
| 8 | dbo.GetCertificateOfOriginByID | GetCertificateOfOriginById | ‏7 sets בסיבוב אחד; milestones מחזיר UserId גולמי (העשרת שם ב-Users proxy); ‏GetCertificateMilestoneRows הוסרה | ✅ חווטה |
| 9 | dbo.GetImportAuthenticationFileDetailsAndRequests | GetAuthenticationRequestFileByID | ‏4 sets בסיבוב אחד; ‏Tasks_Task→CAST(0), ‏DealFile→NULL, ‏Docs→ריק (proxy); הוחלפו 3 קריאות DAL | ✅ חווטה |
| 10 | dbo.GetExportDocumentAuthenticationRequestByID | GetExportDocumentAuthenticationRequestByID | 🛑 **חסום** — ה-proc ב-DB הוא stub‏ (`SELECT 1`, בלי פרמטרים). נדרש הגוף האמיתי מה-DB הישן; עד אז המתודה נשארת LINQ (החלטה מתועדת) | חסום |

## כללי חיווט (לכל מתודה)
1. תיקון גוף ה-dbo copy (הערת JOINs חסרים, NULL, פרמטרים) → ALTER + עדכון קובץ ה-Scripts (אותה ריצה = אותו קובץ).
2. YAML: ‏entry ‏query / query-multiple / execute-scalar.
3. DbContextExtension: מתודה לפי _patterns (query → DapperCheckRows; scalar → ExecuteScalarAsync; multi → QueryMultipleAsync).
4. DAL: החלפת גוף המתודה (חתימות BL נשמרות ככל האפשר); TVP נבנה ב-DAL.
5. BL: התאמות רק היכן שנדרש (הסרת קריאות מקדימות מיותרות ב-#2; איחוד ב-#9).
6. build אחרי כל גל; בסוף — המלצה על ריצת internal-workload-test מלאה.

## הערות פתוחות
- ‏#6: סמנטיקת InvoiceNumber תשתנה מ-LIKE (‏LINQ) חזרה ל-CONTAINS ‏(FTS) — נאמנות למקור, בכפוף לקיום קטלוג FTS מקומי.
- ‏#10: לבקש מקובי את גוף הפרוצדורה מה-DB הישן (או הרשאה להשאיר LINQ כהחלטה מפורשת).
- העשרות BL שנשארות בכוונה: שמות לקוח/ספק/משתמש/מדינה/יחידה — דרך proxies/lookup (גבולות מיקרו-שירות).
