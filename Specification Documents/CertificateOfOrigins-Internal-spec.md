# אפיון: CertificateOfOrigins — Internal API

> **תאריך:** 21/05/2026
> **Controller:** `CertificateOfOriginInternalController` (`/Internal`)

---

## 1. תיאור כללי

ה-Internal API של שירות CertificateOfOrigins חושף נקודות קצה לשימוש פנים-ארגוני בין מיקרו-שירותים. ה-controller מאפשר חיפוש ואחזור תעודות מקור לפי קריטריונים מגוונים, וכן בדיקת קיום תעודה לפי מספר חיצוני. הדומיין עוסק בניהול תעודות מקור (CertificateOfOrigins) במערכת המכס.

---

## 2. נקודות קצה

### GetCertificateOfOriginsByFilter

| שדה | ערך |
|-----|-----|
| **HTTP** | GET |
| **נתיב** | `/Internal/GetCertificateOfOriginsByFilter` |
| **תיאור** | מחזיר רשימת תעודות מקור לפי פילטר חיפוש |

**פרמטרים:**

| שם | סוג | תיאור |
|----|-----|--------|
| `CertificateNumber` | `string?` | מספר תעודת המקור |
| `CertificateOfOriginStatusId` | `int?` | מזהה סטטוס התעודה |
| `CertificateOfOriginTypeId` | `int?` | מזהה סוג התעודה |
| `CustomsAgentId` | `int?` | מזהה סוכן המכס |
| `CustomsHouseId` | `int?` | מזהה בית המכס |
| `DestinationCountry` | `int?` | מזהה ארץ יעד |
| `ExportDeclarationId` | `int?` | מזהה הצהרת יצוא |
| `ExportDeclarationNum` | `string?` | מספר הצהרת יצוא |
| `ExporterCustomerId` | `int?` | מזהה לקוח היצואן |
| `FromIssuingDate` | `DateTimeOffset?` | תאריך הנפקה מ- |
| `ToIssuingDate` | `DateTimeOffset?` | תאריך הנפקה עד |
| `FromRequestDate` | `DateTimeOffset?` | תאריך בקשה מ- |
| `ToRequestDate` | `DateTimeOffset?` | תאריך בקשה עד |
| `RequestReasonId` | `int?` | מזהה סיבת הבקשה |
| `VersionNumber` | `int?` | מספר גרסה |
| `IsLastVersion` | `bool?` | האם להציג גרסה אחרונה בלבד |

**ערך מוחזר:** `List<CertificateOfOriginResultDto>?` — רשימת תוצאות תעודות מקור התואמות לפילטר, או `null` אם אין תוצאות.

**לוגיקה עסקית:**

**מקבל:** אובייקט פילטר מסוג `CertificateOfOriginFilterDto` המכיל קריטריוני חיפוש אופציונליים.

**מבצע:**
1. בונה אובייקט `DynamicParameters` עם כל שדות הפילטר (כולל ערכי `null` עבור שדות שלא סופקו) באמצעות `BuildParameterForProcedure`.
2. מעביר את הפרמטרים לשכבת ה-DAL דרך `DataLayer.GetCertificateOfOriginsByFilter`.
3. ה-DAL מפעיל את ה-Stored Procedure `CRM.usp_CertificateOfOrigins_GetCertificateOfOriginsByFilter` דרך Dapper על ה-`ReadOnlyContext`.
4. מאמת את תוצאות ה-SP באמצעות `DapperHelper.DapperCheckRows`.

**מחזיר:** רשימת `CertificateOfOriginResultDto` התואמות לפילטר. מחזיר `null` אם ה-SP לא החזיר תוצאות.

---

### IsCertificateOfOriginByExternalIdExist

| שדה | ערך |
|-----|-----|
| **HTTP** | GET |
| **נתיב** | `/Internal/IsCertificateOfOriginByExternalIdExist` |
| **תיאור** | בודק אם תעודת מקור קיימת לפי מספר חיצוני ומחזיר את פרטיה |

**פרמטרים:**

| שם | סוג | תיאור |
|----|-----|--------|
| `certificateOfOriginExternalId` | `string` | מספר התעודה החיצוני לחיפוש |

**ערך מוחזר:** `CertificateOfOriginResultDto?` — פרטי התעודה אם נמצאה, או `null` אם לא קיימת.

**לוגיקה עסקית:**

**מקבל:** מחרוזת המייצגת את מספר התעודה החיצוני.

**מבצע:**
1. יוצר אובייקט `CertificateOfOriginFilterDto` חדש עם שדה `CertificateNumber` בלבד מאוכלס בערך שהתקבל; כל שאר השדות נשארים `null`.
2. בונה אובייקט `DynamicParameters` מהפילטר באמצעות `BuildParameterForProcedure` (כל 16 הפרמטרים מועברים ל-SP, שאר הפרמטרים ריקים).
3. מעביר את הפרמטרים לשכבת ה-DAL דרך `DataLayer.GetCertificateOfOriginsByFilter` — אותו SP של סינון תעודות.
4. מחזיר את התוצאה הראשונה בלבד באמצעות `FirstOrDefault`.

**מחזיר:** הרשומה הראשונה מסוג `CertificateOfOriginResultDto` שהתאימה למספר החיצוני. מחזיר `null` אם לא נמצאה תעודה תואמת.

---

## 3. מודלי נתונים

### CertificateOfOriginFilterDto
| שדה | סוג | חובה | תיאור |
|-----|-----|------|--------|
| `CertificateNumber` | `string?` | — | מספר תעודת המקור |
| `CertificateOfOriginStatusId` | `int?` | — | מזהה סטטוס התעודה |
| `CertificateOfOriginTypeId` | `int?` | — | מזהה סוג התעודה |
| `CustomsAgentId` | `int?` | — | מזהה סוכן המכס |
| `CustomsHouseId` | `int?` | — | מזהה בית המכס |
| `DestinationCountry` | `int?` | — | מזהה ארץ יעד |
| `ExportDeclarationId` | `int?` | — | מזהה הצהרת יצוא |
| `ExportDeclarationNum` | `string?` | — | מספר הצהרת יצוא |
| `ExporterCustomerId` | `int?` | — | מזהה לקוח היצואן |
| `FromIssuingDate` | `DateTimeOffset?` | — | תאריך הנפקה מ- |
| `ToIssuingDate` | `DateTimeOffset?` | — | תאריך הנפקה עד |
| `FromRequestDate` | `DateTimeOffset?` | — | תאריך בקשה מ- |
| `ToRequestDate` | `DateTimeOffset?` | — | תאריך בקשה עד |
| `RequestReasonId` | `int?` | — | מזהה סיבת הבקשה |
| `VersionNumber` | `int?` | — | מספר גרסה |
| `IsLastVersion` | `bool?` | — | האם להציג גרסה אחרונה בלבד |

### CertificateOfOriginResultDto
| שדה | סוג | חובה | תיאור |
|-----|-----|------|--------|
| `Id` | `int` | ✓ | מזהה תעודת המקור |
| `CertificateNumber` | `string?` | — | מספר תעודת המקור |
| `Name` | `string?` | — | שם התעודה |
| `CustomesAgentId` | `int` | ✓ | מזהה סוכן המכס |
| `CustomesAgentTitle` | `string?` | — | שם סוכן המכס |
| `CustomesAgentExternalIdNum` | `string?` | — | מספר זיהוי חיצוני של סוכן המכס |
| `ExporterId` | `int` | ✓ | מזהה היצואן |
| `ExporterTitle` | `string?` | — | שם היצואן |
| `ExporterExternalIdNum` | `string?` | — | מספר זיהוי חיצוני של היצואן |
| `ExportDeclarationNumber` | `string?` | — | מספר הצהרת יצוא |
| `VersionNumber` | `int` | ✓ | מספר גרסה |
| `OrganizationUnitId` | `int` | ✓ | מזהה יחידה ארגונית |
| `RequestReasonCode` | `int` | ✓ | קוד סיבת הבקשה |
| `IssuingDate` | `DateTimeOffset?` | — | תאריך הנפקה |
| `LeadDocumentId` | `int?` | — | מזהה מסמך מוביל |

---

## 4. תלויות חיצוניות

| רכיב | תיאור שימוש |
|-------|-------------|
| `CRM.usp_CertificateOfOrigins_GetCertificateOfOriginsByFilter` | Stored Procedure לחיפוש תעודות מקור לפי פילטר — משמש גם עבור `IsCertificateOfOriginByExternalIdExist` |
| `DapperHelper` | אימות תוצאות ה-SP (`DapperCheckRows`) |

---

## 5. הערות

- אין
