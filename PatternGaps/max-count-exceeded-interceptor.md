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
