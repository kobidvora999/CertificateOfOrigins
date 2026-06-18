# אפיון: CertificateOfOrigins — Internal API

> **תאריך:** 18/06/2026 (עודכן: 18/06/2026)
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

### GetCertificateOfOriginById
| שדה | ערך |
|-----|-----|
| **HTTP** | GET |
| **נתיב** | `/Internal/GetCertificateOfOriginById` |
| **תיאור** | מחזיר תעודת מקור מלאה (כולל כל האובייקטים המקוננים) לפי מזהה פנימי |

**פרמטרים:**
| שם | סוג | תיאור |
|----|-----|--------|
| `certificateOfOriginId` | `int` | המזהה הפנימי של תעודת המקור (`[FromQuery]`) |

**ערך מוחזר:** `CertificateOfOriginDto?` — תעודת המקור המלאה עם כל האובייקטים המקוננים שלה, או `null` אם לא נמצאה

**לוגיקה עסקית:**

**מקבל:** מספר שלם `certificateOfOriginId` המייצג את המזהה הפנימי של תעודת המקור

**מבצע:**
1. יוצר אובייקט `DynamicParameters` ומוסיף את הפרמטר `@CertificateOfOriginId` מסוג `Int32` עם הערך שהתקבל
2. קורא ל-DAL (`DataLayer.GetCertificateOfOriginById`) עם הפרמטרים שנוצרו

**מחזיר:** אובייקט `CertificateOfOriginDto` המאוכלס במלואו אם נמצאה תעודה תואמת — כולל אוספי Milestones, CertificateOfOriginVsDeclarationError, CertificateOfOriginDetails (כל אחד עם `CertificateDetailsTypeCode` מקונן), ו-CertificateOfOriginInvoiceDetail (כל אחד עם אוסף `CertificateOfOriginItemDetail` מקונן). מחזיר `null` אם לא קיימת תעודה עם המזהה שניתן.

---

### GetAuthenticationRequestByFilter
| שדה | ערך |
|-----|-----|
| **HTTP** | GET |
| **נתיב** | `/Internal/GetAuthenticationRequestByFilter` |
| **תיאור** | מחזיר רשימת בקשות אימות יבוא לפי פילטר חיפוש |

**פרמטרים:**
| שם | סוג | תיאור |
|----|-----|--------|
| `filter` | `ImportAuthenticationRequestFilterDto` | אובייקט פילטר המכיל את כל קריטריוני החיפוש (`[FromQuery]`) |

**ערך מוחזר:** `List<GetImportAuthenticationRequestResultDto>?` — רשימת בקשות אימות יבוא התואמות לפילטר, או `null` אם לא נמצאו

**לוגיקה עסקית:**

**מקבל:** אובייקט פילטר `ImportAuthenticationRequestFilterDto` המכיל קריטריוני סינון שונים

**מבצע:**
1. מאתר את `AuthenticationRequestBl` מה-`IServiceProvider` (מחלקת לוגיקה נפרדת מ-`CertificateOfOriginBl`)
2. בונה אובייקט `DynamicParameters` עבור פרוצדורת ה-DB `usp_CertificateOfOrigins_GetImportAuthenticationRequestByFilter` באמצעות הפונקציה הפרטית `BuildParameterForProcedure` — ממפה את כל שדות הפילטר לפרמטרים של סוג `DbType` מתאים: `PrefernceDocumentType` (Int32), `GoodsOrigionCountry` (Int32), `IssuingCountry` (Int32), `ImportCountry` (Int32), `FromRequestDate` (DateTimeOffset), `ToRequestDate` (DateTimeOffset), `CustomsHouseId` (Int32), `RequestReason` (Int32), `LeadDocumentId` (Int32), `ImporterId` (Int32), `VendorId` (Int32), `DecisionId` (Int32), `CustomerId` (Int32), `DocumentId` (Int32), `InvoiceNumber` (String), `DocumentNumber` (String), `AuthenticationFileId` (Int32), `CreateUserId` (Int32)
3. קורא ל-DAL עם הפרמטרים שנבנו (`DataLayer.GetAuthenticationRequestByFilter`)

**מחזיר:** רשימת `GetImportAuthenticationRequestResultDto` כפי שהוחזרה מה-DAL, או `null` אם אין תוצאות

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

### CertificateOfOriginDto
| שדה | סוג | חובה | תיאור |
|-----|-----|------|--------|
| `Id` | `int` | ✓ | מזהה ייחודי של תעודת המקור |
| `TypeId` | `int` | ✓ | מזהה סוג תעודת המקור |
| `Title` | `string?` | — | כותרת / שם התעודה |
| `State` | `int` | ✓ | מצב הרשומה |
| `CreateDate` | `DateTimeOffset` | ✓ | תאריך יצירה |
| `CreateUserId` | `int` | ✓ | מזהה משתמש יוצר |
| `UpdateDate` | `DateTimeOffset` | ✓ | תאריך עדכון אחרון |
| `UpdateUserId` | `int` | ✓ | מזהה משתמש מעדכן |
| `OrganizationUnitId` | `int` | ✓ | מזהה יחידה ארגונית |
| `CustomerId` | `int` | ✓ | מזהה לקוח (מייצא) |
| `CreateCustomerId` | `int` | ✓ | מזהה לקוח יוצר |
| `UpdateCustomerId` | `int` | ✓ | מזהה לקוח מעדכן |
| `LeadDocumentId` | `int?` | — | מזהה מסמך מוביל |
| `CertificateIdToCancel` | `int?` | — | מזהה תעודה לביטול |
| `CertificateNumber` | `string?` | — | מספר תעודת המקור |
| `CertificateOfOriginStatusId` | `int` | ✓ | מזהה סטטוס תעודת המקור |
| `DestinationCountry` | `int?` | — | מזהה מדינת יעד |
| `FeedbackRemark` | `string?` | — | הערת משוב |
| `InternalApplication` | `string?` | — | אפליקציה פנימית |
| `IssuingDate` | `DateTimeOffset?` | — | תאריך הנפקה |
| `RejectCancelReason` | `string?` | — | סיבת דחייה / ביטול |
| `ReplacementReason` | `string?` | — | סיבת החלפה |
| `RequestReasonCode` | `int` | ✓ | קוד סיבת הבקשה |
| `ExportDeclarationNumber` | `string?` | — | מספר הצהרת הייצוא |
| `CertificateToReplaceInImport` | `string?` | — | תעודה להחלפה ביבוא |
| `Guid` | `Guid?` | — | מזהה ייחודי גלובלי |
| `QrCodePath` | `string?` | — | נתיב קובץ ה-QR |
| `IsAttachedList` | `bool` | ✓ | האם מצורפת רשימה |
| `InSufficentworkingInd` | `bool?` | — | אינדיקטור עבודה לא מספקת |
| `InsufficentWorkingText` | `string?` | — | טקסט עבודה לא מספקת |
| `QrImage` | `byte[]?` | — | תמונת ה-QR כמערך בתים |
| `ApproveUserId` | `int?` | — | מזהה משתמש מאשר |
| `VersionNumber` | `int` | ✓ | מספר גרסה |
| `IsLastVersion` | `bool` | ✓ | האם זוהי הגרסה האחרונה |
| `IsInPublishingProcess` | `bool` | ✓ | האם נמצאת בתהליך פרסום |
| `CertificateOfOriginTypeCodeName` | `string?` | — | שם קוד סוג תעודת המקור |
| `DocumentId` | `int` | ✓ | מזהה מסמך |
| `IsDeclarationReleased` | `bool?` | — | האם ההצהרה שוחררה |
| `IsCargoExitedOfCustomsRegulation` | `bool?` | — | האם המטען יצא מרגולציית המכס |
| `IsDeclarationReleasedAndNotRetrospectiveCertificate` | `bool?` | — | האם ההצהרה שוחררה ואינה תעודה רטרואקטיבית |
| `StakeholdersIds` | `List<int>` | ✓ | רשימת מזהי בעלי עניין |
| `Milestones` | `List<CertificateMilestonesDto>` | ✓ | רשימת אבני הדרך של התעודה |
| `CertificateOfOriginVsDeclarationError` | `List<CertificateOfOriginVsDeclarationErrorDto>` | ✓ | רשימת שגיאות בהשוואת תעודה-להצהרה |
| `CertificateOfOriginDetails` | `List<CertificateOfOriginDetailsDto>` | ✓ | רשימת פרטי תעודת המקור |
| `CertificateOfOriginInvoiceDetail` | `List<CertificateOfOriginInvoiceDetailDto>` | ✓ | רשימת פרטי חשבוניות של תעודת המקור |

### CertificateMilestonesDto
| שדה | סוג | חובה | תיאור |
|-----|-----|------|--------|
| `CreateDate` | `DateTimeOffset` | ✓ | תאריך יצירת אבן הדרך |
| `ActionName` | `string?` | — | שם הפעולה שבוצעה |
| `UserName` | `string?` | — | שם המשתמש שביצע את הפעולה |
| `RejectReason` | `string?` | — | סיבת דחייה (אם רלוונטי) |
| `VersionNumber` | `int` | ✓ | מספר גרסה |

### CertificateOfOriginVsDeclarationErrorDto
| שדה | סוג | חובה | תיאור |
|-----|-----|------|--------|
| `Id` | `int` | ✓ | מזהה ייחודי של השגיאה |
| `CertificateOfOriginId` | `int` | ✓ | מזהה תעודת המקור הקשורה |
| `ErrorText` | `string?` | — | טקסט השגיאה |
| `State` | `int` | ✓ | מצב הרשומה |

### CertificateOfOriginDetailsDto
| שדה | סוג | חובה | תיאור |
|-----|-----|------|--------|
| `Id` | `int` | ✓ | מזהה ייחודי של הפרט |
| `CertificateOfOriginId` | `int` | ✓ | מזהה תעודת המקור הקשורה |
| `CertificateDetailsTypeCodeId` | `int` | ✓ | מזהה קוד סוג הפרט |
| `Value` | `string?` | — | הערך הגולמי |
| `DisplayedValue` | `string?` | — | הערך לתצוגה |
| `CertificateDetailsTypeCode` | `CertificateDetailsTypeCodeEnumDto?` | — | אובייקט מקונן עם פרטי קוד סוג הפרט |

### CertificateDetailsTypeCodeEnumDto
| שדה | סוג | חובה | תיאור |
|-----|-----|------|--------|
| `Id` | `int` | ✓ | מזהה ייחודי |
| `Name` | `string?` | — | שם בעברית |
| `State` | `int` | ✓ | מצב הרשומה |
| `Description` | `string?` | — | תיאור |
| `EnglishName` | `string?` | — | שם באנגלית |
| `Enumeration` | `string?` | — | ערך ה-Enumeration |
| `StartDate` | `DateTimeOffset?` | — | תאריך תחילת תוקף |
| `EndDate` | `DateTimeOffset?` | — | תאריך סיום תוקף |
| `Comment` | `string?` | — | הערה |
| `DetailTypeFormat` | `string?` | — | פורמט סוג הפרט |
| `DataTypeId` | `int` | ✓ | מזהה סוג הנתון |

### CertificateOfOriginInvoiceDetailDto
| שדה | סוג | חובה | תיאור |
|-----|-----|------|--------|
| `Id` | `int` | ✓ | מזהה ייחודי של פרט החשבונית |
| `CertificateOfOriginId` | `int` | ✓ | מזהה תעודת המקור הקשורה |
| `CurrencyTypeId` | `int?` | — | מזהה סוג מטבע |
| `InvoiceAmount` | `decimal` | ✓ | סכום החשבונית |
| `InvoiceDate` | `DateTimeOffset` | ✓ | תאריך החשבונית |
| `InvoiceGoodsDescription` | `string?` | — | תיאור הסחורה בחשבונית |
| `InvoiceNumber` | `string?` | — | מספר החשבונית |
| `IsToPrint` | `bool` | ✓ | האם יש להדפיס |
| `CertificateOfOriginItemDetail` | `List<CertificateOfOriginItemDetailDto>` | ✓ | רשימת פרטי פריטי החשבונית |

### CertificateOfOriginItemDetailDto
| שדה | סוג | חובה | תיאור |
|-----|-----|------|--------|
| `Id` | `int` | ✓ | מזהה ייחודי של הפריט |
| `PackingTypeId` | `int?` | — | מזהה סוג אריזה |
| `CustomsItemId` | `int?` | — | מזהה פריט מכס |
| `GrossWeight` | `decimal` | ✓ | משקל ברוטו |
| `CertificateOfOriginInvoiceDetailId` | `int` | ✓ | מזהה פרט החשבונית הקשורה |
| `ItemGoodsDescription` | `string?` | — | תיאור סחורת הפריט |
| `MarksAndNumbers` | `string?` | — | סימנים ומספרים |
| `MeasurementUnitId` | `int` | ✓ | מזהה יחידת מידה |
| `OriginCriterionId` | `int?` | — | מזהה קריטריון מקור |
| `Quantity` | `int` | ✓ | כמות |
| `RowNum` | `int` | ✓ | מספר שורה |
| `FullClassification` | `string?` | — | סיווג מלא |
| `ContainerIsoCode` | `string?` | — | קוד ISO של המכולה |

### ImportAuthenticationRequestFilterDto
| שדה | סוג | חובה | תיאור |
|-----|-----|------|--------|
| `PrefernceDocumentType` | `int?` | — | מזהה סוג מסמך העדפה |
| `GoodsOrigionCountry` | `int?` | — | מזהה מדינת מקור הסחורה |
| `IssuingCountry` | `int?` | — | מזהה מדינה מנפיקה |
| `ImportCountry` | `int?` | — | מזהה מדינת יבוא |
| `FromRequestDate` | `DateTimeOffset?` | — | תאריך בקשה מתאריך |
| `ToRequestDate` | `DateTimeOffset?` | — | תאריך בקשה עד תאריך |
| `CustomsHouseId` | `int?` | — | מזהה בית המכס |
| `RequestReason` | `int?` | — | מזהה סיבת הבקשה |
| `LeadDocumentId` | `int?` | — | מזהה מסמך מוביל |
| `ImporterId` | `int?` | — | מזהה היבואן |
| `VendorId` | `int?` | — | מזהה הספק |
| `DecisionId` | `int?` | — | מזהה ההחלטה |
| `CustomerId` | `int?` | — | מזהה הלקוח |
| `DocumentId` | `int?` | — | מזהה המסמך |
| `InvoiceNumber` | `string?` | — | מספר חשבונית |
| `DocumentNumber` | `string?` | — | מספר מסמך |
| `AuthenticationFileId` | `int?` | — | מזהה תיק האימות |
| `CreateUserId` | `int?` | — | מזהה משתמש יוצר |

### GetImportAuthenticationRequestResultDto
| שדה | סוג | חובה | תיאור |
|-----|-----|------|--------|
| `DocumentId` | `int?` | — | מזהה המסמך |
| `IssuingCountryId` | `string?` | — | מזהה מדינה מנפיקה (ערך טקסטואלי) |
| `OrganizationUnitId` | `string?` | — | מזהה יחידה ארגונית (ערך טקסטואלי) |
| `PreferenceDocumentTypeId` | `string?` | — | מזהה סוג מסמך העדפה (ערך טקסטואלי) |
| `AuthenticationFileId` | `int?` | — | מזהה תיק האימות |
| `LeadDocumentTitle` | `string?` | — | כותרת המסמך המוביל |
| `CreateDate` | `DateTimeOffset` | ✓ | תאריך יצירה |
| `VendorName` | `string?` | — | שם הספק |
| `IssuingCountryIdNum` | `int?` | — | מזהה מדינה מנפיקה (ערך מספרי) |
| `OrganizationUnitIdNum` | `int?` | — | מזהה יחידה ארגונית (ערך מספרי) |
| `ResponseNameEmail` | `string?` | — | שם ואימייל של הגורם המגיב |
| `LeadDocumentId` | `int?` | — | מזהה מסמך מוביל |
| `CustomerId` | `int?` | — | מזהה הלקוח |
| `VendorId` | `int?` | — | מזהה הספק |
| `DecisionId` | `int?` | — | מזהה ההחלטה |
| `ImporterName` | `string?` | — | שם היבואן |
| `AuthenticationFileStatusId` | `int?` | — | מזהה סטטוס תיק האימות |

---

## 4. תלויות חיצוניות
| רכיב | תיאור שימוש |
|-------|-------------|
| `ICertificateOfOriginDal` | שכבת גישת הנתונים — מבצעת את קריאות ה-DB בפועל |
| `AuthenticationRequestBl` | מחלקת לוגיקה עסקית נפרדת לניהול בקשות אימות יבוא — נפתרת דרך `IServiceProvider` ב-`GetAuthenticationRequestByFilter` |

---

## 5. הערות
- אין
