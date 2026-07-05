# תוכנית חיווט LINQ→SP — כל 10 המתודות (2026-07-05)

**החלטת מפתח:** כל 10 המתודות שהומרו יחזרו לקרוא לעותקי ה-`dbo` (במקום LINQ).
**עיקרון:** העשרת שמות חוצי-שירות (לקוחות/משתמשים/מדינות/יחידות) נשארת ב-BL דרך proxies/lookup —
ה-SPs המקומיים לא יחזירו JOINs לטבלאות שלא קיימות ב-DB של השירות (db-proc Pattern B/F).

## סטטוס

| # | פרוצדורה | מתודה | ניתוח SQL נדרש | סטטוס |
|---|---|---|---|---|
| 1 | dbo.CheckIfExistsAdditionalRequestsForVendor | CheckIfExistsAdditionalRequestsForVendor | עצמאית — אימות לוגיקה מול LINQ (חלון 3 שנים, ‏>1) | גל א |
| 2 | dbo.CheckIfExistsAdditionalRequestsForImporter | CheckIfExistsAdditionalRequestsForImporter | `Infrastructure.General_enum_GlobalParam` חסרה → פרמטר `@DaysForLastDelivery` (ה-BL כבר קורא parametersUtil); ‏isVendor מחושב ב-SP מטבלה מקומית — הקריאות המקדימות ב-BL מתייתרות חלקית | גל א |
| 3 | dbo.GetAuthenticationRequestByLeadDocumentID | GetAuthenticationRequestByLeadDocumentIDs | להעיר JOINs: ‏CRP.DealFile_LeadDocument, Shared.General_c_Country, Infrastructure.UserMng_OrganizationUnit → NULL: ‏LeadDocumentTitle, ImportCountryName, OrganizationUnitName (BL מעשיר lookup) | גל א |
| 4 | dbo.GetImportAuthenticationRequestById | GetAuthenticationRequestByID | ‏3 result sets; להעיר `CRP.DealFile_LeadDocumentSubmissionData`‏ (set 1) → NULL; ‏set 3 (‏Infrastructure.Docs_Document) → SELECT ריק תואם-מבנה (המסמכים מגיעים מ-proxy) | גל א |
| 5 | dbo.GetCertificateOfOriginsByFilter | GetCertificateOfOriginsByFilter | dynamic SQL; להסיר JOIN ל-`Shared.rStockPileData_Customers_Customer` בתוך המחרוזת → NULL לעמודות Title/ExternalIdNum (BL מעשיר proxy) | גל ב |
| 6 | dbo.GetImportAuthenticationRequestByFilter | GetAuthenticationRequestByFilter | dynamic SQL; להסיר JOINs לחמש טבלאות חסרות; לוודא CONTAINS (FTS) — אם אין קטלוג מקומי → LIKE; עמודות שם → NULL, ‏IDNum נשארות | גל ב |
| 7 | dbo.CROSS_ExportDocumentAuthenticationRequestSearch | GetExportDocumentAuthenticationRequestSearch | dynamic SQL; להסיר JOINs ל-Country/Customer; **להוסיף** עמודות id גולמיות (CountryID, ExporterCustomerID) שה-BL צריך להעשרה; שמות → NULL | גל ב |
| 8 | dbo.GetCertificateOfOriginByID | GetCertificateOfOriginById | ‏7 result sets; ‏1–6 מקומיים; ‏set 7 (milestones) — JOIN ל-UserMng_User → להחזיר UserID גולמי (IIF סטטוס=8) במקום UserName; מחליף גם את GetCertificateMilestoneRows | גל ג |
| 9 | dbo.GetImportAuthenticationFileDetailsAndRequests | GetAuthenticationRequestFileByID | ‏4 sets; מאחד 3 קריאות DAL לקריאת query-multiple אחת; ‏set 3 (Docs) → מבנה ריק; ‏set 2 סוגר שני TODO (LeadDocumentSubmissionDate, IsSendReminderForImporterTaskExists — לאמת מקור מקומי) | גל ג |
| 10 | dbo.GetExportDocumentAuthenticationRequestByID | GetExportDocumentAuthenticationRequestByID | 🛑 **חסום** — ה-proc ב-DB הוא stub‏ (`SELECT 1`, בלי פרמטרים). נדרש הגוף האמיתי מה-DB הישן (ההעתקה המקורית הייתה חלקית) | חסום |

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
