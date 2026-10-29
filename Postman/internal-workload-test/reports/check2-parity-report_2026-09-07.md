# CHECK 2 — פאריטי פונקציה↔פונקציה (WCF → .NET 10)

**תאריך:** 2026-09-07 · **שירות:** CertificateOfOrigins · **היקף:** 35 אופרציות (7 External + 26 Internal + 2 Incoming; `TempSync` מושמטת — stub מת)

## פסק דין: 🔴 לא עובר

‏CHECK 2 בוצע במלואו — ‏8 סוקרים אדוורסריים (הסוקר נפרד מהממיר), כל אחד קרא את **שני** הצדדים במלואם ומיפה
statement ללגאסי מול המקבילה החדשה. כל ממצא HIGH אומת על ידי מול הקוד בפועל לפני שנכנס לדוח, ובמקרים שניתן —
מול ה-DB החי ומול ריצת ה-workload.

**7 חוסמי-שחרור מאומתים.** כולם שקטים: אין שגיאה, אין בדיקה נכשלת, ואין warning בבילד.

---

## 1. שלושה דפוסים שיטתיים (לא באגים בודדים)

הממצאים אינם 60 טעויות עצמאיות. הם שלוש הנחות שגויות שחזרו על עצמן.

### דפוס א' — ‏`ITasksProxy.IsTaskExist` נחשב "יש משימה **פתוחה**". הוא לא.

‏`Infrastructure.usp_Tasks_IsTaskExist` — כל סינון ה-`TaskStatusID` שבו **מוער**; השאילתה החיה מחזירה
`IsTaskInProgress` כ**עמודה מחושבת** (`IIF(TaskStatusID IN (1,4),1,0)`), לא כמסנן. כלומר הקריאה מחזירה גם
משימות **סגורות**.

מתוך 7 אתרי קריאה ב-BL, **אף אחד לא מסנן לפי סטטוס**. חמישה עושים `Count > 0` בלבד; שניים עושים `.Any(...)`
על **מזהה משתמש**, לא על סטטוס. הלגאסי סינן `TaskStatusID != 2` או `IsTaskInProgress = true`, תלוי באתר.

| אתר | ההשפעה |
|---|---|
| `AuthenticationRequestBl.Schedulers.cs:34` | 🔴 תזכורת ליבואן נשלחת **פעם אחת בלבד לכל בקשה, לעולם** — ראה חוסם #2 |
| `AuthenticationRequestBl.cs:837` | משימת `SetDecisionBeforeAssociation` **סגורה** מדכאת את אירוע `NewAuthenticationRequest`; הבקשה לא מוקצית מחדש לרכז |
| `AuthenticationRequestBl.cs:304` | דגל `IsSendReminderForImporterTaskExists` שגוי (קדם-C17) |
| `Schedulers.cs:161`, `:851` | אותה סמנטיקה, השפעה נמוכה יותר |

🛑 **התיקון אינו** `.Any(t => t.IsTaskInProgress)`: זה `status IN (1,4)`, בעוד הלגאסי חסם על `!= 2` — כלומר גם
Canceled(3) ו-Suspended(5) חסמו. פאריטי מדויק דורש את הסטטוס הגולמי, שה-endpoint לא מחזיר. **וממילא**
`TasksProxy.cs:18` נושא `TODO(blocking): confirm endpoint name/route` — ה-route עצמו לא אושר מול שירות ה-Tasks.

### דפוס ב' — עמודות `NOT NULL` בלי ברירת מחדל, שנלקחות verbatim מה-request

אין שכבה שמחתימה אותן; מי שלא שולח — מקבל 0.

- 🔴 **`State`**: מסלול המסר (`BuildSaveRequestFromMessage`) לא מציב אותו כלל → `State = 0`, וה-SP של החיפוש
  מסנן `WHERE (F.State = 1)`. **אומת ב-DB: 29 תעודות שנוצרו בריצה של היום יושבות על `State = 0`.**
- ⚠️ **`ExportDocumentAuthenticationRequest`**: אותו קוד בדיוק (`State = request.State`,
  `OrganizationUnitId = request.OrganizationUnitId`, טבלה בלי DEFAULT). **לא מתממש היום** — כל 175 השורות על
  `State = 1`, כי ה-fixtures שולחים אותם במפורש. חולשה רדומה, לא תקלה פעילה.

ההבדל בין השניים הוא בדיוק הלקח: אותו פגם בקוד, ורק לאחד יש מסלול קורא שמשמיט את השדה.

### דפוס ג' — שמירת-ישות-מלאה הוחלפה ברשימת-עמודות סגורה

הלגאסי עשה `_uow.Repository.Save(entity)` — כל עמודה נכתבת. החדש עושה `ExecuteUpdateAsync` עם set-list מפורש,
וה-DTO נושא רק חלק מהשדות. עמודה שלא ברשימה **לא ניתנת לכתיבה בכלל**.

הקורבן הכבד: **`DecisionCircumstences`** — הנימוק המחייב לשינוי החלטה. אומת: הוא קיים **אך ורק** כמיפוי עמודה
בישות ה-EF; אינו ב-DTO, אינו ברשימת ה-set, ואף שורת קוד ב-BL אינה קוראת או כותבת אותו. רכז שמשנה החלטה
וכותב נימוק — הנימוק נעלם.

---

## 2. חוסמי שחרור מאומתים

| # | ממצא | מסלול | אימות |
|---|---|---|---|
| 1 | תעודות שנוצרו בערוץ המסרים **בלתי נראות בחיפוש** (`State = 0`) | Incoming | DB חי — 29 שורות |
| 2 | תזכורת ליבואן נורית **פעם אחת ותו לא** (משימה סגורה חוסמת לנצח, וה-event שלי סוגר אותה ב-`CloseOld`) | **C17 — שלי** | ה-SP של Tasks + הקוד שלי |
| 3 | `DecisionCircumstences` ועוד ~10 עמודות **לא ניתנות לכתיבה** | writes | grep — 0 שימושים |
| 4 | `GetAuthenticationRequestByID` משמיט **16 עמודות**; מהן `UserId`/`UserResponseId` שה-save דורש → הודעת החלטה נשלחת למשתמש 0 | reads | טבלת עמודות מלאה |
| 5 | `UpdateCetrificateOfOrigins` **מתעלם מ-`EventType`** — 4 מתוך 5 ענפי הניתוב אינם קיימים | External | grep + MIGRATION-NOT-DONE סותר |
| 6 | ה-feedback **לעולם לא מחזיר צרופות** (Draft/Published) | Incoming | בונה-תשובה יחיד, `Attachments = null` |
| 7 | חיפוש חשבונית: `CONTAINS` + פסיק→OR הפך ל-`LIKE '%…%'` — ‏`"1001,1002"` מחזיר אפס שורות | reads + export | diff של שני ה-SPs |

**כמעט-חוסם:** `HandleAuthenticationRequestDeliverySent` — ה-conjunct `|| e.TypeID == 12385` הושמט,
ו-`VirtualEntityDto` **חסר `TypeId` לחלוטין**, כך שלא ניתן לשחזר בלי שינוי DTO. ‏callback הדיוור נכשל בשקט.

---

## 3. תיקונים שעשיתי לממצאי הסוקרים

הסוקרים היו מדויקים על **מה הקוד עושה** ולא אמינים על **מה המשתמש רואה**. שני התיקונים נבעו משכבה מעל ה-BL.

1. **`requestReasonCode` לא חוקי — הורד מ-HIGH ל-MED.** נטען שנשמרת תעודה עם קוד 42. ההנחה נכונה (אין בדיקה
   ב-switch), התוצאה לא: מסלול המסר ממפה ל-`SaveCertificateOfOriginRequestDto` ו-FluentValidation דוחה ב-400.
   אומת מול הבדיקה החיה `70-unknown-reason` שעוברת. הסטייה האמיתית: חריגה in-band → HTTP 400.
2. **`State` ב-ExportDocumentAuthenticationRequest — הורד מ-HIGH לחולשה רדומה.** ה-DB מראה 175/175 על `State = 1`.

**על מהימנות השיטה שלי:** ה-greps שלי הפיקו היום יותר תוצאות שגויות מהניתוח של הסוקרים — שלוש: התאמה
תלוית-רישיות מול שמות SQL, ביטוי שהחמיץ עמודות ב-`[סוגריים]`, וספירת עמודות ב-`awk` שהחזירה 1 בשקט. כל אחת
מהן הייתה מזכה ממצא אמיתי אילו הסתמכתי על התוצאה הראשונה.

---

## 4. למה ה-workload לא תפס אף אחד מאלה

‏755 assertions, 0 נכשלו, כיסוי 92.6% — ואפס מהחוסמים נתפס. זה בדיוק מה שה-skill מזהיר עליו:
**הבדיקות נכתבו מהקוד החדש, ולכן הן מקבעות את ההתנהגות החדשה כ"נכונה" ולעולם לא יתפסו סטייה מהלגאסי.**

הדוגמה החדה: כל fixture של שמירה ישירה מקודד `"state": 1`. ה-fixture מספק בדיוק את השדה שמסלול המסר שוכח,
ואף בדיקה לא מצליבה "תעודה שנוצרה במסר" מול "החיפוש מחזיר אותה".

---

## 5. סדר תיקון מומלץ

1. **חוזה ה-Tasks** — לאשר route + סמנטיקת סטטוס מול השירות. חוסם את התיקון של דפוס א' כולו (5 אתרים).
2. **`State` במסלול המסר** — שורה אחת; 29 תעודות קיימות דורשות תיקון נתונים.
3. **`DecisionCircumstences`** + שאר העמודות → ל-DTOs ולרשימות ה-set. אובדן נתונים בלתי הפיך.
4. **`UserId`/`UserResponseId`** ל-DTO של הקריאה (חוסם #4).
5. **`EventType` dispatcher** (חוסם #5) — לוודא מול MIGRATION-NOT-DONE, שטוען שהומר במלואו.
6. **צרופות ב-feedback** (חוסם #6), **חיפוש חשבונית** (חוסם #7), **`TypeId` ב-`VirtualEntityDto`**.
7. לתקן את שלוש ההערות שלי שטוענות `"row set is identical to legacy"` — הן שקריות.

---

## 6. מה לא נבדק

- ~45 ממצאי MED ו-~35 LOW שדווחו ולא אומתו על ידי אחד-אחד.
- `Title` → `Name` בתצוגת OrganizationUnit/Vendor/Customer — דורש בדיקת ערך מול שירותי המקור, לא קריאת קוד.
- ‏`TOP (shared.ufn_GetMaxRows())` → `TOP (200)` בשני SPs — גוף ה-UDF אינו ב-repo.
- ‏global param 1148 — היחיד מהשישה שלא אושר מול DataScript.
- כל ה-`TODO(blocking): confirm endpoint name/route` על proxies של Customers/Documents/Tasks.
