# אפיון: CertificateOfOrigins — Internal API

> **תאריך:** 21/05/2026
> **Controller:** `CertificateOfOriginInternalController` (`/Internal`)

---

## 1. תיאור כללי

ה-Internal API של שירות CertificateOfOrigins חושף נקודות קצה לשימוש פנים-ארגוני בין מיקרו-שירותים. ה-controller מאפשר חיפוש רשימת תעודות מקור לפי פילטר, בדיקת קיום תעודה לפי מספר חיצוני, ואחזור תעודה מלאה לפי מזהה פנימי. הדומיין עוסק בניהול תעודות מקור (CertificateOfOrigins) במערכת המכס.

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

**ערך מוחזר:** `List<CertificateOfOriginResultDto>?` — רשימת תוצאות תעודות מקור, או `null` אם אין תוצאות.

**לוגיקה עסקית:**

**מקבל:** אובייקט פילטר מסוג `CertificateOfOriginFilterDto`.

**מבצע:**
1. בונה `DynamicParameters` עם כל 16 שדות הפילטר (כולל `null` לשדות שלא סופקו) דרך `BuildParameterForProcedure`.
2. קורא ל-`DataLayer.GetCertificateOfOriginsByFilter` עם הפרמטרים.
3. ה-DAL מפעיל `CRM.usp_CertificateOfOrigins_GetCertificateOfOriginsByFilter` דרך Dapper על `ReadOnlyContext`.
4. מאמת תוצאות ה-SP דרך `DapperHelper.DapperCheckRows`.

**מחזיר:** רשימת `CertificateOfOriginResultDto` התואמות לפילטר, או `null`.

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

**ערך מוחזר:** `CertificateOfOriginResultDto?` — פרטי התעודה אם נמצאה, או `null`.

**לוגיקה עסקית:**

**מקבל:** מחרוזת המייצגת מספר תעודה חיצוני.

**מבצע:**
1. יוצר `CertificateOfOriginFilterDto` עם `CertificateNumber` בלבד מאוכלס.
2. בונה `DynamicParameters` דרך `BuildParameterForProcedure` (כל 16 פרמטרים מועברים ל-SP, שאר השדות `null`).
3. קורא ל-`DataLayer.GetCertificateOfOriginsByFilter` — אותו SP של סינון.
4. מחזיר `FirstOrDefault` מהתוצאה.

**מחזיר:** רשומה ראשונה מסוג `CertificateOfOriginResultDto` שתאמה, או `null`.

---

### GetCertificateOfOriginById

| שדה | ערך |
|-----|-----|
| **HTTP** | GET |
| **נתיב** | `/Internal/GetCertificateOfOriginById` |
| **תיאור** | מחזיר תעודת מקור מלאה לפי מזהה פנימי, כולל פרטים, חשבוניות ומיילסטונים |

**פרמטרים:**

| שם | סוג | תיאור |
|----|-----|--------|
| `certificateOfOriginId` | `int` | מזהה פנימי של תעודת המקור |

**ערך מוחזר:** `CertificateOfOriginDto?` — תעודה מלאה עם כל ה-collections, או `null` אם לא נמצאה.

**לוגיקה עסקית:**

**מקבל:** מזהה פנימי מסוג `int`.

**מבצע:**
1. בונה `DynamicParameters` עם פרמטר יחיד `@CertificateOfOriginId`.
2. קורא ל-`DataLayer.GetCertificateOfOriginById` עם הפרמטרים.
3. ה-DAL קורא ל-`ReadOnlyContext.GetCertificateOfOriginById` שמפעיל `CRM.usp_CertificateOfOrigins_GetCertificateOfOriginByID` דרך Dapper `QueryMultipleAsync`.
4. קורא 7 result sets ברצף: התעודה הראשית, שגיאות הצהרה, קודי סוג פרטים, פרטי תעודה, חשבוניות, פריטי חשבוניות, מיילסטונים.
5. מחשב `StakeholderIds` = `[CustomerId, CreateCustomerId]`.
6. לכל `CertificateOfOriginDetailsDto` — מקשר את אובייקט `CertificateDetailsTypeCode` לפי `CertificateDetailsTypeCodeId`.
7. לכל `CertificateOfOriginInvoiceDetailDto` — מקשר את רשימת ה-`Items` מתוך פריטי החשבוניות לפי `CertificateOfOriginInvoiceDetailId`.
8. מאכלס `DeclarationErrors`, `Details`, `Invoices`, `Milestones` על אובייקט התעודה.

**מחזיר:** `CertificateOfOriginDto` מלא עם כל ה-collections מקושרות, או `null` אם לא נמצאה תעודה.

---

## 3. מודלי נתונים

### CertificateOfOriginFilterDto
| שדה | סוג | חובה | תיאור |
|-----|-----|------|--------|
| `CertificateNumber` | `string?` | — | מספר תעודת המקור |
| `CertificateOfOriginStatusId` | `int?` | — | מזהה סטטוס |
| `CertificateOfOriginTypeId` | `int?` | — | מזהה סוג |
| `CustomsAgentId` | `int?` | — | מזהה סוכן מכס |
| `CustomsHouseId` | `int?` | — | מזהה בית מכס |
| `DestinationCountry` | `int?` | — | מזהה ארץ יעד |
| `ExportDeclarationId` | `int?` | — | מזהה הצהרת יצוא |
| `ExportDeclarationNum` | `string?` | — | מספר הצהרת יצוא |
| `ExporterCustomerId` | `int?` | — | מזהה לקוח יצואן |
| `FromIssuingDate` | `DateTimeOffset?` | — | תאריך הנפקה מ- |
| `ToIssuingDate` | `DateTimeOffset?` | — | תאריך הנפקה עד |
| `FromRequestDate` | `DateTimeOffset?` | — | תאריך בקשה מ- |
| `ToRequestDate` | `DateTimeOffset?` | — | תאריך בקשה עד |
| `RequestReasonId` | `int?` | — | מזהה סיבת בקשה |
| `VersionNumber` | `int?` | — | מספר גרסה |
| `IsLastVersion` | `bool?` | — | גרסה אחרונה בלבד |

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

### CertificateOfOriginDto
| שדה | סוג | חובה | תיאור |
|-----|-----|------|--------|
| `Id` | `int` | ✓ | מזהה פנימי |
| `TypeId` | `int` | ✓ | מזהה סוג התעודה |
| `Title` | `string?` | — | כותרת |
| `State` | `int` | ✓ | סטטוס רשומה |
| `CreateDate` | `DateTimeOffset` | ✓ | תאריך יצירה |
| `CreateUserId` | `int` | ✓ | מזהה משתמש יוצר |
| `UpdateDate` | `DateTimeOffset` | ✓ | תאריך עדכון |
| `UpdateUserId` | `int` | ✓ | מזהה משתמש מעדכן |
| `OrganizationUnitId` | `int` | ✓ | מזהה יחידה ארגונית |
| `CustomerId` | `int` | ✓ | מזהה יצואן |
| `CreateCustomerId` | `int` | ✓ | מזהה סוכן מכס יוצר |
| `UpdateCustomerId` | `int` | ✓ | מזהה לקוח מעדכן |
| `LeadDocumentId` | `int?` | — | מזהה מסמך מוביל |
| `CertificateIdToCancel` | `int?` | — | מזהה תעודה לביטול |
| `CertificateNumber` | `string?` | — | מספר תעודה |
| `CertificateOfOriginStatusId` | `int` | ✓ | מזהה סטטוס |
| `DestinationCountry` | `int?` | — | מזהה ארץ יעד |
| `FeedbackRemark` | `string?` | — | הערת משוב |
| `InternalApplication` | `string?` | — | יישום פנימי |
| `IssuingDate` | `DateTimeOffset?` | — | תאריך הנפקה |
| `RejectCancelReason` | `string?` | — | סיבת דחייה/ביטול |
| `ReplacementReason` | `string?` | — | סיבת החלפה |
| `RequestReasonCode` | `int` | ✓ | קוד סיבת בקשה |
| `ExportDeclarationNumber` | `string?` | — | מספר הצהרת יצוא |
| `CertificateToReplaceInImport` | `string?` | — | תעודה להחלפה ביבוא |
| `QRCodePath` | `string?` | — | נתיב QR Code |
| `IsAttachedList` | `bool` | ✓ | האם יש רשימה מצורפת |
| `InSufficentworkingInd` | `bool?` | — | אינדיקטור עבודה לא מספקת |
| `InsufficentWorkingText` | `string?` | — | טקסט עבודה לא מספקת |
| `QrImage` | `byte[]?` | — | תמונת QR |
| `ApproveUserId` | `int?` | — | מזהה משתמש מאשר |
| `VersionNumber` | `int` | ✓ | מספר גרסה |
| `IsLastVersion` | `bool` | ✓ | גרסה אחרונה |
| `IsInPublishingProcess` | `bool` | ✓ | בתהליך פרסום |
| `StakeholderIds` | `List<int>` | ✓ | מזהי בעלי עניין (מחושב: CustomerId, CreateCustomerId) |
| `DeclarationErrors` | `List<CertificateOfOriginVsDeclarationErrorDto>` | ✓ | שגיאות מול הצהרת יצוא |
| `Details` | `List<CertificateOfOriginDetailsDto>` | ✓ | פרטי תעודה |
| `Invoices` | `List<CertificateOfOriginInvoiceDetailDto>` | ✓ | חשבוניות |
| `Milestones` | `List<CertificateMilestoneDto>` | ✓ | היסטוריית מיילסטונים |

### CertificateOfOriginVsDeclarationErrorDto
| שדה | סוג | חובה | תיאור |
|-----|-----|------|--------|
| `Id` | `int` | ✓ | מזהה |
| `CertificateOfOriginId` | `int` | ✓ | מזהה תעודת המקור |
| `ErrorText` | `string?` | — | טקסט השגיאה |
| `State` | `int` | ✓ | סטטוס |

### CertificateOfOriginDetailsDto
| שדה | סוג | חובה | תיאור |
|-----|-----|------|--------|
| `Id` | `int` | ✓ | מזהה |
| `CertificateOfOriginId` | `int` | ✓ | מזהה תעודת המקור |
| `CertificateDetailsTypeCodeId` | `int` | ✓ | מזהה סוג הפרט |
| `Value` | `string?` | — | ערך |
| `DisplayedValue` | `string?` | — | ערך לתצוגה |
| `CertificateDetailsTypeCode` | `CertificateDetailsTypeCodeEnumDto?` | — | אובייקט קוד סוג הפרט (מקושר לפי Id) |

### CertificateDetailsTypeCodeEnumDto
| שדה | סוג | חובה | תיאור |
|-----|-----|------|--------|
| `Id` | `int` | ✓ | מזהה |
| `Name` | `string?` | — | שם |
| `State` | `int` | ✓ | סטטוס |
| `Description` | `string?` | — | תיאור |
| `EnglishName` | `string?` | — | שם באנגלית |
| `Enumeration` | `string?` | — | ערך enum |
| `StartDate` | `DateTimeOffset?` | — | תאריך התחלה |
| `EndDate` | `DateTimeOffset?` | — | תאריך סיום |
| `Comment` | `string?` | — | הערה |
| `DetailTypeFormat` | `string?` | — | פורמט סוג הפרט |
| `DataTypeId` | `int` | ✓ | מזהה סוג נתון |

### CertificateOfOriginInvoiceDetailDto
| שדה | סוג | חובה | תיאור |
|-----|-----|------|--------|
| `Id` | `int` | ✓ | מזהה |
| `CertificateOfOriginId` | `int` | ✓ | מזהה תעודת המקור |
| `CurrencyTypeId` | `int?` | — | מזהה סוג מטבע |
| `InvoiceAmount` | `decimal` | ✓ | סכום חשבונית |
| `InvoiceDate` | `DateTimeOffset` | ✓ | תאריך חשבונית |
| `InvoiceGoodsDescription` | `string?` | — | תיאור סחורה |
| `InvoiceNumber` | `string?` | — | מספר חשבונית |
| `IsToPrint` | `bool` | ✓ | האם להדפיס |
| `Items` | `List<CertificateOfOriginItemDetailDto>` | ✓ | פריטי החשבונית (מקושרים לפי InvoiceDetailId) |

### CertificateOfOriginItemDetailDto
| שדה | סוג | חובה | תיאור |
|-----|-----|------|--------|
| `Id` | `int` | ✓ | מזהה |
| `PackingTypeId` | `int?` | — | מזהה סוג אריזה |
| `CustomsItemId` | `int?` | — | מזהה פריט מכס |
| `GrossWeight` | `decimal` | ✓ | משקל ברוטו |
| `CertificateOfOriginInvoiceDetailId` | `int` | ✓ | מזהה חשבונית אב |
| `ItemGoodsDescription` | `string?` | — | תיאור סחורת פריט |
| `MarksAndNumbers` | `string?` | — | סימנים ומספרים |
| `MeasurementUnitId` | `int` | ✓ | מזהה יחידת מידה |
| `OriginCriterionId` | `int?` | — | מזהה קריטריון מקור |
| `Quantity` | `int` | ✓ | כמות |
| `RowNum` | `int` | ✓ | מספר שורה |
| `FullClassification` | `string?` | — | סיווג מלא |
| `ContainerISOCode` | `string?` | — | קוד ISO מכולה |

### CertificateMilestoneDto
| שדה | סוג | חובה | תיאור |
|-----|-----|------|--------|
| `CreateDate` | `DateTimeOffset` | ✓ | תאריך פעולה |
| `ActionName` | `string?` | — | שם פעולה |
| `UserName` | `string?` | — | שם משתמש |
| `RejectReason` | `string?` | — | סיבת דחייה |
| `VersionNumber` | `int` | ✓ | מספר גרסה |

---

## 4. תלויות חיצוניות

| רכיב | תיאור שימוש |
|-------|-------------|
| `CRM.usp_CertificateOfOrigins_GetCertificateOfOriginsByFilter` | SP לחיפוש תעודות לפי פילטר — משמש גם עבור `IsCertificateOfOriginByExternalIdExist` |
| `CRM.usp_CertificateOfOrigins_GetCertificateOfOriginByID` | SP המחזיר 7 result sets עם מלוא פרטי התעודה |
| `DapperHelper` | אימות תוצאות ה-SP (`DapperCheckRows`) |

---

## 5. הערות

- אין
