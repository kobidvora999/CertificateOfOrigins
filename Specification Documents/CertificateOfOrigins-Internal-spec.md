# אפיון: CertificateOfOrigins — Internal API

> **תאריך:** 18/06/2026
> **Controller:** `CertificateOfOriginInternalController` (`/Internal`)

---

## 1. תיאור כללי
ה-controller חושף נקודות קצה פנימיות לניהול ושאילתת תעודות מקור (Certificates of Origin).
הוא מיועד לצריכה פנים-שירותית על ידי שירותים אחרים במערכת CustomsCloud.
הדומיין עוסק בתעודות מקור המשויכות להצהרות יצוא, מייצאים, וסוכני מכס.

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
| `filter` | `CertificateOfOriginFilterDto` | אובייקט פילטר המכיל את כל קריטריוני החיפוש (`[FromQuery]`) |

**ערך מוחזר:** `List<CertificateOfOriginResultDto>?` — רשימת תעודות מקור התואמות לפילטר, או `null` אם לא נמצאו

**לוגיקה עסקית:**

**מקבל:** אובייקט פילטר `CertificateOfOriginFilterDto` המכיל קריטריוני סינון שונים

**מבצע:**
1. בונה אובייקט `DynamicParameters` עבור פרוצדורת ה-DB באמצעות הפונקציה הפרטית `BuildParameterForProcedure` — ממפה את כל שדות הפילטר לפרמטרים של סוג `DbType` מתאים: `CertificateNumber` (String), `CertificateOfOriginStatusId` (Int32), `CertificateOfOriginTypeId` (Int32), `CustomsAgentId` (Int32), `CustomsHouseId` (Int32), `DestinationCountry` (Int32), `ExportDeclarationId` (Int32), `ExportDeclarationNum` (String), `ExporterCustomerId` (Int32), `FromIssuingDate` (DateTimeOffset), `ToIssuingDate` (DateTimeOffset), `FromRequestDate` (DateTimeOffset), `ToRequestDate` (DateTimeOffset), `RequestReasonId` (Int32), `VersionNumber` (Int32), `IsLastVersion` (Boolean)
2. קורא ל-DAL עם הפרמטרים שנבנו (`DataLayer.GetCertificateOfOriginsByFilter`)

**מחזיר:** רשימת `CertificateOfOriginResultDto` כפי שהוחזרה מה-DAL, או `null` אם אין תוצאות

---

### IsCertificateOfOriginByExternalIdExist
| שדה | ערך |
|-----|-----|
| **HTTP** | GET |
| **נתיב** | `/Internal/IsCertificateOfOriginByExternalIdExist` |
| **תיאור** | בודק האם קיימת תעודת מקור לפי מזהה חיצוני ומחזיר אותה אם נמצאת |

**פרמטרים:**
| שם | סוג | תיאור |
|----|-----|--------|
| `certificateOfOriginExternalId` | `string` | המזהה החיצוני של תעודת המקור לחיפוש (`[FromQuery]`) |

**ערך מוחזר:** `CertificateOfOriginResultDto?` — תעודת המקור אם נמצאה, או `null` אם לא קיימת

**לוגיקה עסקית:**

**מקבל:** מחרוזת `certificateOfOriginExternalId` המייצגת את המזהה החיצוני

**מבצע:**
1. יוצר אובייקט `CertificateOfOriginFilterDto` חדש ומגדיר את השדה `CertificateNumber` לערך ה-`certificateOfOriginExternalId` שהתקבל
2. קורא ל-`GetCertificateOfOriginsByFilter` עם הפילטר שנוצר — הלוגיקה המלאה של חיפוש זה מפורטת תחת `GetCertificateOfOriginsByFilter` לעיל
3. מחזיר את הרשומה הראשונה מהרשימה שהתקבלה (באמצעות `FirstOrDefault`)

**מחזיר:** הרשומה הראשונה (`CertificateOfOriginResultDto`) אם קיימת תעודת מקור התואמת למזהה, או `null` אם לא נמצאה

---

## 3. מודלי נתונים

### CertificateOfOriginFilterDto
| שדה | סוג | חובה | תיאור |
|-----|-----|------|--------|
| `CertificateNumber` | `string?` | — | מספר תעודת המקור |
| `CertificateOfOriginStatusId` | `int?` | — | מזהה סטטוס תעודת המקור |
| `CertificateOfOriginTypeId` | `int?` | — | מזהה סוג תעודת המקור |
| `CustomsAgentId` | `int?` | — | מזהה סוכן המכס |
| `CustomsHouseId` | `int?` | — | מזהה בית המכס |
| `DestinationCountry` | `int?` | — | מזהה מדינת יעד |
| `ExportDeclarationId` | `int?` | — | מזהה הצהרת הייצוא |
| `ExportDeclarationNum` | `string?` | — | מספר הצהרת הייצוא |
| `ExporterCustomerId` | `int?` | — | מזהה לקוח המייצא |
| `FromIssuingDate` | `DateTimeOffset?` | — | תאריך הנפקה מתאריך |
| `ToIssuingDate` | `DateTimeOffset?` | — | תאריך הנפקה עד תאריך |
| `FromRequestDate` | `DateTimeOffset?` | — | תאריך בקשה מתאריך |
| `ToRequestDate` | `DateTimeOffset?` | — | תאריך בקשה עד תאריך |
| `RequestReasonId` | `int?` | — | מזהה סיבת הבקשה |
| `VersionNumber` | `int?` | — | מספר גרסה |
| `IsLastVersion` | `bool?` | — | האם זוהי הגרסה האחרונה |

### CertificateOfOriginResultDto
| שדה | סוג | חובה | תיאור |
|-----|-----|------|--------|
| `Id` | `int` | ✓ | מזהה ייחודי של תעודת המקור |
| `CertificateNumber` | `string?` | — | מספר תעודת המקור |
| `Name` | `string?` | — | שם תעודת המקור |
| `CustomesAgentId` | `int` | ✓ | מזהה סוכן המכס |
| `CustomesAgentTitle` | `string?` | — | שם סוכן המכס |
| `CustomesAgentExternalIdNum` | `string?` | — | מספר זיהוי חיצוני של סוכן המכס |
| `ExporterId` | `int` | ✓ | מזהה המייצא |
| `ExporterTitle` | `string?` | — | שם המייצא |
| `ExporterExternalIdNum` | `string?` | — | מספר זיהוי חיצוני של המייצא |
| `ExportDeclarationNumber` | `string?` | — | מספר הצהרת הייצוא |
| `VersionNumber` | `int` | ✓ | מספר גרסה |
| `OrganizationUnitId` | `int` | ✓ | מזהה יחידה ארגונית |
| `RequestReasonCode` | `int` | ✓ | קוד סיבת הבקשה |
| `IssuingDate` | `DateTimeOffset?` | — | תאריך הנפקה |
| `LeadDocumentId` | `int?` | — | מזהה מסמך מוביל |

---

## 4. תלויות חיצוניות
| רכיב | תיאור שימוש |
|-------|-------------|
| `ICertificateOfOriginDal` | שכבת גישת הנתונים — מבצעת את קריאות ה-DB בפועל |

---

## 5. הערות
- אין
