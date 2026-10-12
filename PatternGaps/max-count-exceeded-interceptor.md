# PatternGap: MaxCountExceededInterceptor חוסם קריאת ישות רחבה (>30 עמודות)

תאריך: 2026-07-27 · מתודה: GetExportDocumentAuthenticationRequestByID (#14) ·
קובץ לגאסי: `C:\Repos\Main\CRM\CertificateOfOrigins\Server\Customs.CRM.CertificateOfOrigins.BL\ExportDocumentAuthenticationRequestBL.cs:83-95`

## קטע הקוד הלגאסי
```csharp
var result = _uow.Repository.GetQuery<ExportDocumentAuthenticationRequest>().Single(edar => edar.ID == id);
_uow.Repository.LoadProperty(result, ...CustomsItem...);
_uow.Repository.LoadProperty(result, ...LeadDocument...);
_uow.Repository.LoadProperty(result, ...ManufacturingArea...);
```
קריאת EF ישירה של ישות בת **35 עמודות** + 3 אוספי-בן.

## מה לא זוהה / מה הייתה העמימות
בזמן ריצה, קריאת ה-EF המלאה (35 עמודות) מחזירה **500** מ-
`CustomsCloud.InfrastructureCore.DbInterceptionException: Result fields count (35) exceeded max error level of 30`.

הפלטפורמה מפעילה `MaxCountExceededInterceptor` (ב-`CustomsCloud.InfrastructureCore.DAL`) שחוסם כל
פקודה שמחזירה מעל הסף. הספים בברירת מחדל (`DatabaseConsts`): עמודות error=30 / warn=20, שורות error=250 / warn=201.

הפתרון היחיד שמוזכר בסקילז הוא `.ExcludeInterceptor("T7e0Y38X2y")` (ב-CLAUDE.md, ל-soft-delete),
אך **המנגנון לא היה מתועד**: `ExcludeInterceptor(hash)` אינו "מבטל interceptor" אלא מעלה את הספים לשאילתה,
לפי טבלת `InterceptorList._exclusionHash` ב-CORE. כל hash הוא key ל-use-case ספציפי של צוות מסוים
(למשל `T7e0Y38X2y` → "PreRuling. Find by id", `ErrorColumnsLevel=55`). **אין hash ל-CertificateOfOrigins**,
ואי-אפשר לגלות מיפוי hash↔interceptor מהבינארי — צריך להוסיף רשומה חדשה ב-CORE.

## ההחלטה הזמנית שהתקבלה (הכריעה: המפתחת)
1. **זמני (read-only):** projection ל-**29** עמודות ב-DAL, השמטת **6** שדות
   (State, CreateDate, CreateUserId, UpdateDate, UpdateUserId, OrganizationUnitId) — כדי לא להיחסם ולסיים את #14.
   שים לב: הסף **כולל** (`count (30) exceeded max error level of 30`) — כלומר 30 עמודות כבר נכשל; מותר ≤29.
2. **קבוע (במקביל):** להוסיף רשומת hash **אחת** ל-CertificateOfOrigins ב-`InterceptorList._exclusionHash`
   ב-CORE, ממודדת לכל השירות (ErrorColumnsLevel≥40, ErrorRowsLevel נדיב), ולשחרר גרסה חדשה של
   `CustomsCloud.InfrastructureCore.DAL`. אז לעבור ל-`.Include(...) + .ExcludeInterceptor("<hash>")`
   ולהחזיר את 5 השדות.
3. **קריטי:** ל-**writers** (Save ב-Fetch & Merge, למשל SaveExportDocumentAuthenticationRequest) projection
   **לא אפשרי** — שם צריך את הישות המלאה המנוטרת — ולכן ה-hash ב-CORE הוא בלתי-נמנע.

## למרכז (קובי)
לתעד ב-`_shared/dal-rules.md`: (א) מה עושה `ExcludeInterceptor` בפועל (מעלה ספים, לא מבטל),
(ב) הספים בברירת מחדל (30 עמודות / 250 שורות), (ג) שכל שירות שקורא ישות רחבה צריך רשומת hash משלו
ב-`InterceptorList` ב-CORE, (ד) רשימת/מיקום ה-hashes הקיימים כדי שמפתחים לא ינחשו.

## ✅ נפתר (2026-10-12)
ב-`CustomsCloud.InfrastructureCore.DAL` **1.10.83** (הגרסה בפרויקט) ה-hash `j4XSVK6Fl8` רשום ב-`InterceptorList._exclusionHash`
(עמודות warn 40 / error 50, שורות warn 201 / error 250). ב-1.10.58 הוא לא היה רשום, ולכן הבדיקה מ-2026-09-15 נכשלה
והסיקה בטעות שה-interceptor "מתעלם" מה-tag. לא נדרש עדכון NuGet.

- `GetExportDocumentAuthenticationRequestById` — הרשומה המלאה (35 עמודות) עם `.ExcludeInterceptor("j4XSVK6Fl8")`,
  ו-3 אוספי-הבן בשאילתות נפרדות (כמו `LoadProperty` בלגאסי; JOIN של שלושה אוספים מכפיל שורות לעבר 250).
  ה-BL לוקח את State, OrganizationUnitId ועמודות ה-audit מהישות עצמה.
- `GetImportAuthenticationRequestById` — הקריאה הכפולה (21+10) אוחדה לשאילתה אחת של 31 עמודות עם אותו hash.
- נשאר במכוון: `GetExportRequestProjectionColumns` משמש את ה-Save בלבד, כדי לשמור את State/OrganizationUnitId מהשורה
  (החלטת בטיחות נפרדת, לא חלק מהמעקף).
- **אומת מול SQL Server מקומי** עם ה-interceptor של הפלטפורמה: בלי ה-hash שתי השאילתות נכשלות
  (`Result fields count (35/31) exceeded max eror level of 30`), עם ה-hash שתיהן עוברות ומחזירות את כל השדות והבנים.
- הכלל נוסף לסקיל `_shared/dal-rules.md`.
