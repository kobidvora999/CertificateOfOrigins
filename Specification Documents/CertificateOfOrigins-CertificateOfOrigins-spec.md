# אפיון: CertificateOfOrigins — CertificateOfOrigins API

> **תאריך:** 28/07/2026
> **Controller:** `CertificateOfOriginsController` (`/CertificateOfOrigins`)

---

## 1. תיאור כללי
ה-controller המרכזי של תעודות מקור (Certificate of Origin). חושף חיפוש, שליפה בודדת, בדיקות קיום, אינטגרציית ESB/EAI (Convert), ואת נקודת הקצה הציבורית לאימות תעודה מהפורטל החיצוני (CertificateRequestByGuid). צרכנים: ה-SPA הפנימי (Internal), שירותים חיצוניים דרך ה-ESB (External), והפורטל הציבורי (Incoming).

---

## 2. נקודות קצה

### CertificateOfOriginByExternalIdExist
| שדה | ערך |
|-----|-----|
| **HTTP** | GET |
| **נתיב** | `/CertificateOfOrigins/CertificateOfOriginByExternalIdExist` |
| **תיאור** | בדיקת קיום תעודת מקור לפי מספר תעודה חיצוני (Internal WCF: `IsCertificateOfOriginByExternalIdExist`) |

**פרמטרים:**
| שם | סוג | תיאור |
|----|-----|--------|
| `certificateOfOriginExternalId` | `string` | מספר התעודה החיצוני (משמש כ-CertificateNumber בסינון) |

**ערך מוחזר:** `CertificateOfOriginResultDto?` — התעודה התואמת האחרונה, או `null` אם לא נמצאה (בדיקת קיום — לא 404)

**לוגיקה עסקית:**

**מקבל:** מספר תעודה חיצוני (מחרוזת)

**מבצע:**
1. בונה מסנן (`CertificateOfOriginFilterDto`) עם `CertificateNumber` שווה למספר שהתקבל
2. מפעיל את אותה לוגיקת החיפוש כמו `CertificateOfOriginsByFilter` (חיפוש LIKE, ההתאמה העדכנית ביותר)
3. לוקח את הרשומה הראשונה מהתוצאות, אם קיימת

**מחזיר:** תוצאה בודדת או `null` — אין מקרה 404, זו בדיקה ולא שליפת משאב

---

### CertificateOfOriginID
| שדה | ערך |
|-----|-----|
| **HTTP** | GET |
| **נתיב** | `/CertificateOfOrigins/CertificateOfOriginID/{certificateNumber}` |
| **תיאור** | שליפת מזהה תעודת מקור לפי מספר תעודה (External WCF: `GetCertificateOfOriginID`) |

**פרמטרים:**
| שם | סוג | תיאור |
|----|-----|--------|
| `certificateNumber` | `string` (מהנתיב) | מספר התעודה — מפתח חלופי |

**ערך מוחזר:** `int` — מזהה התעודה התואמת האחרונה; מספר לא קיים מחזיר 404

**לוגיקה עסקית:**

**מקבל:** מספר תעודה (מהנתיב)

**מבצע:**
1. שולף מה-DAL את המזהה העדכני ביותר עבור מספר התעודה
2. אם לא נמצאה התאמה — נזרקת חריגת 404 (`RestNotFoundException`)

**מחזיר:** מזהה תעודה (int); 404 אם המספר לא קיים

---

### GoodsItemCerificateDTO
| שדה | ערך |
|-----|-----|
| **HTTP** | POST |
| **נתיב** | `/CertificateOfOrigins/GoodsItemCerificateDTO` |
| **תיאור** | העשרת רשימת פריטי טובין במזהה תעודת המקור התואם (External WCF: `GetGoodsItemCerificateDTO`) |

**פרמטרים:**
| שם | סוג | תיאור |
|----|-----|--------|
| `goodsItemCerificateDTOs` | `List<GoodsItemCerificateDto>` (מגוף הבקשה) | רשימת פריטי טובין, כל אחד עם מספר תעודה אופציונלי |

**ערך מוחזר:** `List<GoodsItemCerificateDto>` — אותה רשימה, מועשרת

**לוגיקה עסקית:**

**מקבל:** רשימת פריטי טובין, חלקם עם מספר תעודת מקור

**מבצע:**
1. עובר על כל פריט ברשימה
2. אם קיים מספר תעודה — שולף מה-DAL את המזהה העדכני ביותר התואם ומשייך אותו לפריט (`CertificateOfOriginId`)
3. פריט ללא מספר תעודה נשאר ללא שינוי

**מחזיר:** הרשימה כולה, כל פריט מועשר במזהה תעודה תואם (או `null` אם לא נמצא)

---

### CertificateOfOriginsByFilter
| שדה | ערך |
|-----|-----|
| **HTTP** | GET |
| **נתיב** | `/CertificateOfOrigins/CertificateOfOriginsByFilter` |
| **תיאור** | חיפוש תעודות מקור לפי מסנן מלא (Internal WCF: `GetCertificateOfOriginsByFilter`) |

**פרמטרים:**
| שם | סוג | תיאור |
|----|-----|--------|
| `filter` | `CertificateOfOriginFilterDto` | קריטריוני חיפוש (מספר תעודה, סטטוס, סוג, סוכן מכס, בית מכס, יעד, תאריכים וכו') |

**ערך מוחזר:** `List<CertificateOfOriginResultDto>` — רשימת תעודות תואמות (ריקה אם אין)

**לוגיקה עסקית:**

**מקבל:** מסנן חיפוש עם עד 16 קריטריונים אופציונליים

**מבצע:**
1. בונה פרמטרים ל-stored procedure מתוך שדות המסנן
2. מריץ את השאילתה מול ה-DAL ומקבל את רשימת התעודות התואמות
3. מעשיר כל תעודה בשם ומספר חיצוני של היצואן ושל סוכן המכס — דרך שירות הלקוחות (Customers proxy), לפי מזהי `ExporterId` ו-`CustomesAgentId`

**מחזיר:** רשימת תעודות מועשרות בפרטי לקוח; רשימה ריקה אם אין התאמות (חיפוש — אף פעם לא 404)

---

### Convert
| שדה | ערך |
|-----|-----|
| **HTTP** | POST |
| **נתיב** | `/CertificateOfOrigins/Convert` |
| **תיאור** | פעולת ESB/EAI — פענוח ישות מקושרת (Connected Entity) לישות וירטואלית גנרית (External WCF: `Convert`) |

**פרמטרים:**
| שם | סוג | תיאור |
|----|-----|--------|
| `connectedEntity` | `ConnectedEntityDto` (מגוף הבקשה) | ישות מקושרת גנרית; המפתח הרלוונטי הוא `EntityIdKey1` (מספר תעודת המקור) |

**ערך מוחזר:** `VirtualEntityDto` — קישור ישות גנרי; תעודה לא קיימת מחזירה 404

**לוגיקה עסקית:**

**מקבל:** ישות מקושרת, שבה `EntityIdKey1` הוא מספר תעודת המקור

**מבצע:**
1. בונה מסנן חיפוש לפי מספר התעודה (`EntityIdKey1`)
2. מריץ את אותה לוגיקת החיפוש כמו `CertificateOfOriginsByFilter` ולוקח את התוצאה הראשונה
3. אם לא נמצאה תעודה — נזרקת חריגת 404
4. בונה ישות וירטואלית: `Id` ו-`Title` (שם) מהתעודה, `EntityType` קבוע (12319 — CertificateOfOrigin), `CustomerId` = סוכן המכס של התעודה

**מחזיר:** ישות וירטואלית עם 4 שדות ממופים; 404 אם התעודה לא קיימת

---

### CertificateOfOriginById
| שדה | ערך |
|-----|-----|
| **HTTP** | GET |
| **נתיב** | `/CertificateOfOrigins/CertificateOfOriginById/{certificateOfOriginId}` |
| **תיאור** | שליפת תעודת מקור בודדת עם כל הגרף שלה (Internal WCF: `GetCertificateOfOriginById`) |

**פרמטרים:**
| שם | סוג | תיאור |
|----|-----|--------|
| `certificateOfOriginId` | `int` (מהנתיב) | מזהה תעודת המקור |

**ערך מוחזר:** `CertificateOfOriginDto` — התעודה עם כל הישויות המקושרות; מזהה לא קיים מחזיר 404

**לוגיקה עסקית:**

**מקבל:** מזהה תעודת מקור

**מבצע:**
1. שולף מה-DAL את התעודה המלאה (כותרת + שגיאות מול הצהרת יצוא + פרטי תעודה + פרטי חשבוניות + אבני דרך) — מבוסס על 7 result sets מה-stored procedure `dbo.GetCertificateOfOriginByID`
2. אם לא נמצאה תעודה — נזרקת חריגת 404
3. עבור כל אבן דרך (Milestone) עם מזהה משתמש — אוסף את מזהי המשתמשים הייחודיים
4. שולף את שמות המשתמשים דרך שירות המשתמשים (User proxy) וממלא אותם באבני הדרך

**מחזיר:** התעודה המלאה, כולל שמות משתמשים באבני הדרך; 404 אם המזהה לא קיים

---

### CertificateRequestByGuid
| שדה | ערך |
|-----|-----|
| **HTTP** | GET |
| **נתיב** | `/CertificateOfOrigins/CertificateRequestByGuid` |
| **תיאור** | אימות תעודת מקור לצורך הפורטל הציבורי, לפי guid או לפי מספר תעודה + תאריך הנפקה (Incoming WCF: `GetCertificateRequestByGuid` / `GetPC_Web_9096_CertificateRequest`) |

**פרמטרים:**
| שם | סוג | תיאור |
|----|-----|--------|
| `request` | `CertificateOfOriginsRequestDto` | `CertificateOfOriginGuid` (guid כמחרוזת, אופציונלי), `CertificateOfOriginNumber` (מספר תעודה, אופציונלי), `IssuingDate` (תאריך הנפקה, אופציונלי) |

**ערך מוחזר:** `CertificateOfOriginsResponseDto` — תשובת שאילתת ה-web

**לוגיקה עסקית:**

**מקבל:** guid ו/או מספר תעודה + תאריך הנפקה

**מבצע:**
1. אם התקבל guid ואינו תקין (לא ניתן ל-parse) — מוחזרת תשובה עם `ExceptionDescription = "Invalid Guid"` (HTTP 200, **לא** 404 — שימור חוזה השגיאה המקורי מה-WCF כדי לא לפגוע בפורטל החיצוני)
2. בונה פרמטרים לשאילתה: guid (אם תקין), מספר תעודה, ותאריך הנפקה
3. שולף מה-DAL את נתוני התעודה לשאילתת ה-web; אם לא נמצאה התאמה — מוחזרת תשובה עם `ExceptionDescription = "No Matching Certificate"` (HTTP 200, לא 404)
4. אחרת, בונה את תשובת ה-web:
   - `DocumentId` — כרגע תמיד 0 (השדה נפתר בעבר משירות המסמכים החיצוני; חסום לפיתוח עתידי — TODO)
   - `CertificateNumber` — ממופה ישירות
   - `QueryUrl` — נבנה מפורמט המוגדר בפרמטר `CertificateOfOriginQueryURL` (`IParametersUtil`) עם ה-guid; ריק אם אין guid
   - `CertificateOfOriginDetails` — רשימת שדות תווית/ערך (`FieldDataDto`), הכוללת:
     - שדות כותרת: אם התעודה הונפקה רטרואקטיבית (סיבת בקשה = Retrospective Certificate) — שורת "Issued Retrospectively"; אם קיים מזהה תעודה מוחלפת — שורת "Replacing certificate {מזהה}"; שורת תאריך הנפקה תמיד; אם קיים מספר הצהרת יצוא ומסומן להדפסה — שורת מספר ההצהרה. תוויות השדות נשלפות בבת אחת משירות שדות הדיקשנרי (Data Dictionary Field proxy)
     - שדות פרטי תעודה: כל פרט ב-`CertificateOfOriginDetails` ממופה לפי סוג (מקום ייצור, מיקוד ייצור, הערות, יצואן/יבואן, מדינות, בית מכס וכו' — מודפסים כפי שהם; תאריך הצהרה מפורמט; שדות נמען (Consignee) מוצגים רק אם מסומנים להדפסה, עם לוגיקה ייחודית לסוגי EUR1/EURMED)
   - `CertificateOfOriginInvoiceDetails` — רשימת פרטי חשבונית, מסוננת לפי סוג התעודה: **quirk מקורי משומר** — עבור IsrCol תמיד נכללת; עבור MERCOSUR רק אם מסומנת להדפסה; עבור EURMED/EUR1 תמיד נכללת (ללא תלות בדגל ההדפסה, עקב סדר קדימות אופרטורים במקור). `CurrencyCode` תמיד `null` (אין lookup זמין ל-CurrencyType — TODO). `CertificateOfOriginItemDetails` תמיד ריקה (quirk מקורי משומר)

**מחזיר:** תשובת שאילתת web מלאה; **במקרה של guid לא תקין או אי-התאמה — HTTP 200 עם `ExceptionDescription` ממולא, לא 404** (שימור החוזה מול הפורטל החיצוני)

---

### LoadDataFromExportDeclaration
| שדה | ערך |
|-----|-----|
| **HTTP** | GET |
| **נתיב** | `/CertificateOfOrigins/LoadDataFromExportDeclaration` |
| **תיאור** | בדיקה האם ניתן להמשיך בטיפול בתעודה על סמך מצב הצהרת היצוא הקשורה (Internal WCF: `LoadDataFromExportDeclaration`) |

**פרמטרים:**
| שם | סוג | תיאור |
|----|-----|--------|
| `request` | `LoadDataFromExportDeclarationRequestDto` | `LeadDocumentId` (אופציונלי), `ExportDeclarationNumber` (אופציונלי), `RequestReasonCode` |

**ערך מוחזר:** `bool` — האם המטען יצא מפיקוח המכס והבקשה אינה רטרואקטיבית

**לוגיקה עסקית:**

**מקבל:** מזהה מסמך מוביל ו/או מספר הצהרת יצוא, וקוד סיבת בקשה

**מבצע:**
1. בדיקת תקינות בסיסית: אם אין מזהה מסמך מוביל ואין מספר הצהרת יצוא — מוחזר `false` מיידית
2. שולף משירות תיק היצוא (ExportDealFile proxy) את פרטי הצהרת היצוא עבור מזהה המסמך המוביל ו/או מספר ההצהרה
3. בודק אם המטען יצא מפיקוח המכס (`IsCargoExitedOfCustomsRegulation`, ברירת מחדל `false` אם לא נמצאו פרטים)
4. **הערה:** במקור ה-WCF עדכן את הישות עצמה (by-reference) בשדות `IsDeclarationReleased`/`IsCargoExitedOfCustomsRegulation`; ב-REST מוחזר רק הדגל המחושב (החלטת מפתח)

**מחזיר:** `true` רק אם המטען יצא מפיקוח המכס **וגם** קוד סיבת הבקשה אינו "תעודה רטרואקטיבית"

---

## 3. מודלי נתונים

### CertificateOfOriginResultDto
| שדה | סוג | חובה | תיאור |
|-----|-----|------|--------|
| `Id` | `int` | ✓ | מזהה תעודה |
| `CertificateNumber` | `string?` | — | מספר תעודה |
| `Name` | `string?` | — | שם/כותרת |
| `CustomesAgentId` | `int` | ✓ | מזהה סוכן מכס |
| `CustomesAgentTitle` | `string?` | — | שם סוכן מכס (מועשר) |
| `CustomesAgentExternalIdNum` | `string?` | — | מספר חיצוני של סוכן המכס (מועשר) |
| `ExporterId` | `int` | ✓ | מזהה יצואן |
| `ExporterTitle` | `string?` | — | שם יצואן (מועשר) |
| `ExporterExternalIdNum` | `string?` | — | מספר חיצוני של היצואן (מועשר) |
| `ExportDeclarationNumber` | `string?` | — | מספר הצהרת יצוא |
| `VersionNumber` | `int` | ✓ | מספר גרסה |
| `OrganizationUnitId` | `int` | ✓ | מזהה יחידה ארגונית |
| `RequestReasonCode` | `int` | ✓ | קוד סיבת בקשה |
| `IssuingDate` | `DateTime?` | — | תאריך הנפקה |
| `LeadDocumentId` | `int?` | — | מזהה מסמך מוביל |

### CertificateOfOriginFilterDto
| שדה | סוג | חובה | תיאור |
|-----|-----|------|--------|
| `CertificateNumber` | `string?` | — | מספר תעודה (LIKE) |
| `CertificateOfOriginStatusId` | `int?` | — | סטטוס תעודה |
| `CertificateOfOriginTypeId` | `int?` | — | סוג תעודה |
| `CustomsAgentId` | `int?` | — | מזהה סוכן מכס |
| `CustomsHouseId` | `int?` | — | מזהה בית מכס |
| `DestinationCountry` | `int?` | — | מדינת יעד |
| `ExportDeclarationId` | `int?` | — | מזהה הצהרת יצוא |
| `ExportDeclarationNum` | `string?` | — | מספר הצהרת יצוא |
| `ExporterCustomerId` | `int?` | — | מזהה לקוח-יצואן |
| `FromIssuingDate` / `ToIssuingDate` | `DateTime?` | — | טווח תאריך הנפקה |
| `FromRequestDate` / `ToRequestDate` | `DateTime?` | — | טווח תאריך בקשה |
| `RequestReasonId` | `int?` | — | סיבת בקשה |
| `VersionNumber` | `int?` | — | מספר גרסה |
| `IsLastVersion` | `bool?` | — | האם הגרסה האחרונה |

### GoodsItemCerificateDto
| שדה | סוג | חובה | תיאור |
|-----|-----|------|--------|
| `GoodsItemId` | `int` | ✓ | מזהה פריט טובין |
| `CertificateNumber` | `string?` | — | מספר תעודה לחיפוש |
| `CertificateOfOriginId` | `int?` | — | מזהה תעודה תואם (מועשר) |

### VirtualEntityDto
| שדה | סוג | חובה | תיאור |
|-----|-----|------|--------|
| `Id` | `int` | ✓ | מזהה |
| `Title` | `string?` | — | כותרת |
| `EntityType` | `int` | ✓ | קוד סוג ישות (12319 = CertificateOfOrigin) |
| `CustomerId` | `int` | ✓ | מזהה לקוח (סוכן מכס) |

### ConnectedEntityDto
| שדה | סוג | חובה | תיאור |
|-----|-----|------|--------|
| `EntityIdKey1` | `string?` | — | מפתח ראשי (מספר תעודה) |
| `EntityIdKey2` / `EntityIdKey3` | `string?` | — | מפתחות נוספים (לא בשימוש כאן) |
| `EntityPath` | `string?` | — | נתיב ישות (לא בשימוש כאן) |
| `EntityType` | `int` | ✓ | סוג ישות |
| `EntityIdExternalReferenceId` | `string?` | — | מזהה חיצוני (לא בשימוש כאן) |

### CertificateOfOriginDto
| שדה | סוג | חובה | תיאור |
|-----|-----|------|--------|
| `Id` | `int` | ✓ | מזהה תעודה |
| `TypeId` | `int` | ✓ | סוג תעודה |
| `Title` | `string?` | — | כותרת |
| `State` | `int` | ✓ | מצב רשומה |
| `TimeStamp` | `byte[]?` | — | חותמת זמן (concurrency) |
| `CreateDate` / `UpdateDate` | `DateTime` | ✓ | תאריכי יצירה/עדכון |
| `CreateUserId` / `UpdateUserId` | `int` | ✓ | משתמשי יצירה/עדכון |
| `OrganizationUnitId` | `int` | ✓ | יחידה ארגונית |
| `CustomerId` | `int` | ✓ | לקוח (יצואן) |
| `CreateCustomerId` / `UpdateCustomerId` | `int` | ✓ | לקוח יצירה/עדכון (סוכן מכס) |
| `LeadDocumentId` | `int?` | — | מזהה מסמך מוביל |
| `CertificateIdToCancel` | `int?` | — | מזהה תעודה להחלפה/ביטול |
| `CertificateNumber` | `string?` | — | מספר תעודה |
| `CertificateOfOriginStatusId` | `int` | ✓ | סטטוס |
| `DestinationCountry` | `int?` | — | מדינת יעד |
| `FeedbackRemark` | `string?` | — | הערת משוב |
| `InternalApplication` | `string?` | — | יישום פנימי |
| `IssuingDate` | `DateTime?` | — | תאריך הנפקה |
| `RejectCancelReason` | `string?` | — | סיבת דחייה/ביטול |
| `ReplacementReason` | `string?` | — | סיבת החלפה |
| `RequestReasonCode` | `int` | ✓ | קוד סיבת בקשה |
| `ExportDeclarationNumber` | `string?` | — | מספר הצהרת יצוא |
| `CertificateToReplaceInImport` | `string?` | — | תעודה להחלפה ביבוא |
| `Guid` | `Guid?` | — | מזהה ייחודי לאימות |
| `QrCodePath` | `string?` | — | נתיב קוד QR |
| `IsAttachedList` | `bool` | ✓ | דגל רשימה מצורפת |
| `InSufficentworkingInd` | `bool?` | — | דגל עבודה לא מספקת |
| `InsufficentWorkingText` | `string?` | — | טקסט עבודה לא מספקת |
| `VersionNumber` | `int` | ✓ | מספר גרסה |
| `IsLastVersion` | `bool` | ✓ | האם גרסה אחרונה |
| `ApproveUserId` | `int?` | — | מאשר |
| `IsInPublishingProcess` | `bool` | ✓ | בתהליך פרסום |
| `StakeholdersIds` | `List<int>` | — | מזהי בעלי עניין (יצואן + סוכן מכס) |
| `Milestones` | `List<CertificateMilestoneDto>` | — | אבני דרך (מועשרות בשם משתמש) |
| `CertificateOfOriginDetails` | `List<CertificateOfOriginDetailDto>` | — | פרטי תעודה |
| `CertificateOfOriginVsDeclarationError` | `List<CertificateOfOriginVsDeclarationErrorDto>` | — | שגיאות מול הצהרת יצוא |
| `CertificateOfOriginInvoiceDetail` | `List<CertificateOfOriginInvoiceDetailDto>` | — | פרטי חשבוניות |

### CertificateOfOriginsRequestDto
| שדה | סוג | חובה | תיאור |
|-----|-----|------|--------|
| `CertificateOfOriginGuid` | `string?` | — | guid לאימות (מחרוזת) |
| `CertificateOfOriginNumber` | `string?` | — | מספר תעודה (חלופי ל-guid) |
| `IssuingDate` | `DateTime?` | — | תאריך הנפקה (בשילוב עם מספר תעודה) |

### CertificateOfOriginsResponseDto
| שדה | סוג | חובה | תיאור |
|-----|-----|------|--------|
| `CertificateNumber` | `string?` | — | מספר תעודה |
| `QueryUrl` | `string?` | — | קישור לשאילתת אימות |
| `DocumentId` | `int` | ✓ | מזהה מסמך (כרגע תמיד 0 — TODO) |
| `CertificateOfOriginDetails` | `List<FieldDataDto>` | — | שורות תווית/ערך להצגה |
| `CertificateOfOriginInvoiceDetails` | `List<CertificateOfOriginWebInvoiceDetailDto>` | — | פרטי חשבוניות להצגה |
| `ExceptionDescription` | `string?` | — | תיאור שגיאה בערוץ ה-in-band (guid לא תקין / אין התאמה); HTTP 200 גם כשממולא |

### FieldDataDto
| שדה | סוג | חובה | תיאור |
|-----|-----|------|--------|
| `Label` | `string?` | — | תווית השדה |
| `Value` | `object?` | — | ערך השדה (מחרוזת או תאריך, כפי שהוא) |

### CertificateOfOriginWebInvoiceDetailDto
| שדה | סוג | חובה | תיאור |
|-----|-----|------|--------|
| `InvoiceNumber` | `string?` | — | מספר חשבונית (ריק אם לא מסומן להדפסה) |
| `InvoiceDate` | `DateTime` | ✓ | תאריך חשבונית |
| `InvoiceAmount` | `decimal` | ✓ | סכום חשבונית |
| `CurrencyCode` | `string?` | — | קוד מטבע (תמיד `null` כרגע — TODO) |
| `InvoiceGoodsDescription` | `string?` | — | תיאור טובין |
| `CertificateOfOriginItemDetails` | `List<CertificateOfOriginWebItemDetailDto>` | — | תמיד ריקה (quirk מקורי משומר) |

### LoadDataFromExportDeclarationRequestDto
| שדה | סוג | חובה | תיאור |
|-----|-----|------|--------|
| `LeadDocumentId` | `int?` | — | מזהה מסמך מוביל |
| `ExportDeclarationNumber` | `string?` | — | מספר הצהרת יצוא |
| `RequestReasonCode` | `int` | ✓ | קוד סיבת בקשה |

---

## 4. תלויות חיצוניות
| רכיב | תיאור שימוש |
|-------|-------------|
| `ICustomerProxy` | העשרת שם/מספר חיצוני של יצואן וסוכן מכס בחיפוש ותוצאות; משמש גם ב-`Convert` (דרך החיפוש) |
| `IExportDealFileProxy` | שליפת פרטי הצהרת יצוא (`GetExportDeclarationDetailsForCertificateOfOrigion`) ב-`LoadDataFromExportDeclaration` |
| `IUserProxy` | העשרת שמות משתמשים באבני הדרך של תעודה (`CertificateOfOriginById`) |
| `IDataDictionaryFieldProxy` | שליפת תוויות שדות (English Name) עבור שדות הכותרת ב-`CertificateRequestByGuid` |
| `IParametersUtil` | קריאת פרמטר `CertificateOfOriginQueryURL` (פורמט URL לאימות) ב-`CertificateRequestByGuid` |

---

## 5. הערות
- `CertificateRequestByGuid`: `DocumentId` תמיד מוחזר כ-0 — TODO(blocking) לפתרון דרך שירות המסמכים (Documents service)
- `CertificateRequestByGuid`: `CurrencyCode` בפרטי חשבונית תמיד `null` — TODO(blocking), אין `ILookupUtil<CurrencyType>` זמין בפלטפורמה
- `CertificateRequestByGuid`: יש לוודא זריעת פרמטר `CertificateOfOriginQueryURL` — TODO(blocking)
- `CertificateRequestByGuid`: quirk מקורי משומר במכוון בסינון פרטי חשבונית (סדר קדימות אופרטורים בין MERCOSUR ל-IsrCol/EURMED/EUR1) ובאי-הצגת שדות נמען ל-EUR1/EURMED — תועד ואושר ע"י מפתח ב-28/07/2026
- `LoadDataFromExportDeclaration`: אובדן ההתנהגות המקורית של עדכון הישות by-reference — הוחלט להחזיר רק את הדגל המחושב (החלטת מפתח)
