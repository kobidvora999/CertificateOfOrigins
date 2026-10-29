# Gap analysis — CertificateOfOrigins internal-workload coverage

תאריך: 2026-08-23 · Branch: `feature/repo-align-c12-reverify` · Baseline: `CertificateOfOriginsBl` 48.1% line

## איטרציה 1 — INPUT: תאריכי fixture שנרקבו ✅

| | |
|---|---|
| **Lever** | INPUT |
| **Gap** | כל מנוע היצירה + ההצלבה (reconciliation) ב-0% |
| **סיבה** | `CertificateOfOriginRequest (Create MERCOSUR)` נשא `dateOfDeclaration: 2026-08-12` קשיח. חוק הצהרת-היצואן מתיר `today .. -5 ימים`; ב-2026-08-23 הערך בן 11 יום, אז כל ריצה נדחתה in-band (`exceptionType 5010`) **לפני** שהמנוע רץ. הבקשה החזירה 200, כך שגם assertion על status לא הייתה תופסת זאת. |
| **Fix** | משתני תאריך יחסיים ב-prerequest של האוסף (`{{today}}`, `{{twoDaysAgo}}`, `{{tenDaysAgo}}`, `{{inOneMonth}}`) והחלפת 5 תאריכים קשיחים ב-2 בקשות ה-Create. |
| **תוצאה** | `CertificateOfOriginsBl` **48.1% → 59.3%** (+11.2). היצירה מצליחה באמת (`applicationId: 118797`, `certificateId: IL0000116906`) ומריצה את ההצלבה מול הצהרת היצוא. |

> 🛑 **לקח:** תאריך קשיח ב-fixture הוא פצצת-זמן. כל fixture עם תאריך שמשתתף בוולידציה חייב להיות יחסי.

## מה שנשאר — מסווג ללפי lever

### 🟠 INPUT — cross-field validation (`CertificateOfOriginsBl.MessageCrossField.cs`)

נקראים מ-`MessageCrossField.cs:42`, כולם מותנים בערכי שדות שאין להם כיסוי:

| מתודה | שורות | תנאי ההפעלה |
|---|---|---|
| `CheckCertificateUpdate` | 17 | בקשת עדכון לתעודה קיימת |
| `CheckCumulationCountry` | 15 | `IsCumulation = true` |
| `CheckCountryXorGroup` | 10 | `IsCumulation` + ארץ/קבוצה |
| `ValidateCertificateInReplacement` | 10 | תעודה מחליפה |
| `CheckCertificateIdToCancelHasValue` | 6 | ביטול **עם** `certificateIdToCancel` |
| `CheckCertificateIdIsEmpty` | 6 | ביטול **בלי** מזהה |

**חסום על החלטת מפתח.** הסקיל אוסר להמציא ערכים עסקיים ("Never invent business values — ask"). נדרשים ערכים
שעוברים ולידציה עבור: `isCumulation` + ארץ/קבוצת cumulation, ומזהה תעודה בר-ביטול. **~64 שורות.**

### 🟠 DB-STATE — מסלול ה-web-query (`CertificateOfOriginsBl.cs:464-541`)

| מתודה | שורות |
|---|---|
| `MapConsigneeField` | 13 |
| `MapDateOfDeclarationField` | 11 |
| `MapDetailField` | 7 |
| `PrintOutField` | 7 |

נקראים מה-switch ב-`CertificateOfOriginsBl.cs:496`, במסלול `RequestByGuid` (‏`GetCertificateOfOriginDataForWebQuery`).
כרגע ה-fixture שולח GUID אקראי → אין שורה → לולאת המיפוי לא רצה. **Fix:** תרחיש self-contained —
`00-setup` יוצר תעודה, לוכד את ה-`Guid` שלה, ו-`10-main` קורא ל-`RequestByGuid` עם אותו GUID. **~38 שורות.**

### 🟠 DB-STATE — `AuthenticationRequestBl.MapToResultDto` (26 שורות)

הגדול ביותר שנותר. הבקשה `AuthenticationRequestByID` מכוונת ל-`900500` שלא קיים → 404 (וזה **נכון**, זה חוזה
ה-not-found שנטען עכשיו). כדי לכסות את המפה צריך תרחיש נוסף שיוצר בקשת-אימות אמיתית ואז קורא לה לפי id.

### ✅ אומת כהתנהגות נכונה — לא פער

`CertificateOfOriginRequest (GetRequestStatus)` ו-`(Cancellation)` מחזירים **400** עם
`"תעודה מספר IL0000116889 אינה קיימת במערכת"` (`exceptionType 13680`). זו תגובת not-found תקינה במודל
ה-in-band של השירות, לא fixture שבור. אפשר להעביר אותן מ-`UNVERIFIED` ל-`EXPECTED = 400`.

## עדיין לא אומת

`CertificateOfOriginByExternalIdExist` מחזיר **204** מ-endpoint שמוצהר `bool` — ככל הנראה מחזיר null.
לא נגעתי; דורש הכרעת מפתח.

## הערכת פוטנציאל

| שלב | BL line% |
|---|---|
| baseline | 48.1% |
| אחרי איטרציה 1 | **59.3%** |
| + cross-field (INPUT, חסום על ערכים) | ~+10 |
| + web-query ו-MapToResultDto (DB-STATE) | ~+10 |
| רף C13 | **≥ 70%** |

הרף בר-השגה, אבל שני המנופים הנותרים דורשים ערכים עסקיים או תרחישי setup שמייצרים נתונים — לא ניחוש.

---

## סבב CONTRACT (2026-08-25) — שני פגמים אמיתיים

סבב ה-negative תוכנן להוסיף כיסוי, ובפועל חשף שני פגמים. **לא** נכתבה עליהם assertion —
לברך על 500 זה לקבע באג. שניהם אומתו מול השירות החי.

### 🔴 1. אין שכבת ולידציה בשירות בכלל

`POST /CertificateOfOrigins` עם `{}` מחזיר **500**:

```
DbUpdateException -> SqlException: The INSERT statement conflicted with the FOREIGN KEY constraint ...
```

גוף ריק מייצר ישות עם כל מזהי-ה-FK באפס, ה-INSERT מפר FK, והחריגה בורחת כ-500.
`grep -rl AbstractValidator` על כל ה-repo מחזיר **אפס** — אין ולו validator אחד, ואין תיקיית
`Validations/` שהארכיטקטורה מגדירה. כלומר קלט זבל מגיע ישר ל-DB בכל endpoint שאין לו
`[BindRequired]` או זריקה מפורשת ב-BL.

לשם השוואה, `POST /ExportDocumentAuthenticationRequest` עם `{}` מחזיר **400** כראוי — כך
שההתנהגות לא עקבית בין endpoints.

**התיקון:** `/net10-validation` על `SaveCertificateOfOriginRequestDto`. זה שינוי התנהגות ב-endpoint
כתיבה (יתחיל לדחות בקשות שקודם הגיעו ל-DB) — דורש הכרעת מפתח.

### 🔴 2. התנגשות concurrency ב-merge של שורות-הילד בורחת כ-500 במקום 409

- `timeStamp` מעופש על שמירת ההורה → **409** ✅ (`BaseBL.SaveChangesAsync` ממפה
  `DbUpdateConcurrencyException` ל-`RestConflictException`) — זה מכוסה עכשיו באוסף Negative.
- אותה התנגשות שנוצרת בתוך ה-merge של שורות-הילד → **500** עם
  `"Entity(s) current state was changed since last read"` (הודעה מ-InfrastructureCore).

הסיבה: ב-DAL נשארו **5** קריאות `Context.SaveChangesAsync()` (שורות 131, 157, 173, 276, 621)
שלא עוברות דרך `BaseBL.SaveChangesAsync` ולכן לא מקבלות את מיפוי ה-409.

זו תוצאה ישירה של פיצול C12 שביצעתי — ההורה עבר ל-BaseBL והילדים נשארו ב-DAL. לפני C12 הכל
היה 500, כך שהחומרה לא הורעה, אבל התוצאה עכשיו **לא עקבית**: אותה שגיאה לוגית מחזירה 409 או
500 תלוי באיזה שלב של השמירה היא קרתה.

**התיקון:** לנתב את שמירות שורות-הילד דרך ה-BL, או לתפוס ולמפות ב-DAL.

### מה כן נאסר באוסף Negative

| תרחיש | סטטוס | מאומת |
|---|---|---|
| 4 מסלולי by-id עם id לא קיים | 404 | ✅ |
| `ExportDocumentAuthenticationRequest` גוף ריק | 400 | ✅ |
| `CreateNewFile` גוף שאינו מערך | 400 | ✅ |
| `timeStamp` מעופש (שמירת הורה) | 409 | ✅ |

🟡 `CreateNewFile` עם מערך ריק `[]` מחזיר **204 No Content** ולא 400. לא נאסר — לא אומת שזו הכוונה.
