# אפיון: CertificateOfOrigins — AuthenticationRequest API

> **תאריך:** 28/07/2026
> **Controller:** `AuthenticationRequestController` (`/AuthenticationRequest`)

---

## 1. תיאור כללי
ה-controller חושף חיפוש ובדיקות עבור בקשות אימות יבוא (Import Authentication Request) — התהליך שבו יבואן מבקש אימות מסמך העדפה (Preference Document) מול בית מכס. צרכן: ה-SPA הפנימי (Internal). כולל חיפוש לפי מסנן, שליפה לפי מסמכים מובילים, שליפת המסמכים המצורפים למסמך מוביל (לצורך צירוף לבקשת אימות), ובדיקות עסקיות (יבואן ברשימה חסומה, ריבוי בקשות).

---

## 2. נקודות קצה

### AuthenticationRequestByFilter
| שדה | ערך |
|-----|-----|
| **HTTP** | GET |
| **נתיב** | `/AuthenticationRequest/AuthenticationRequestByFilter` |
| **תיאור** | חיפוש בקשות אימות יבוא לפי מסנן (Internal WCF: `GetAuthenticationRequestByFilter`) |

**פרמטרים:**
| שם | סוג | תיאור |
|----|-----|--------|
| `filter` | `ImportAuthenticationRequestFilterDto` | קריטריוני חיפוש (סוג מסמך העדפה, מדינות, תאריכי בקשה, בית מכס, יבואן, ספק, לקוח ועוד) |

**ערך מוחזר:** `List<GetImportAuthenticationRequestResultDto>` — רשימת בקשות תואמות (ריקה אם אין)

**לוגיקה עסקית:**

**מקבל:** מסנן חיפוש עם עד 17 קריטריונים אופציונליים; טווח תאריך הבקשה (`FromRequestDate`/`ToRequestDate`) מוחל תמיד ע"י ה-stored procedure

**מבצע:**
1. בונה פרמטרים ל-stored procedure מתוך שדות המסנן
2. מריץ את השאילתה מול ה-DAL ומקבל את רשימת הבקשות התואמות
3. מעשיר כל בקשה בשם היבואן (`CustomerId` שמייצג את היבואן) דרך שירות הלקוחות (Customers proxy)
4. מעשיר כל בקשה בשם הספק (`VendorId`) דרך שירות הספקים (Vendors proxy)
5. ממלא את שם מדינת ההנפקה (`IssuingCountryId`) דרך lookup משותף של Country, לפי המזהה הגולמי
6. ממלא את שם היחידה הארגונית (`OrganizationUnitId`) דרך lookup משותף של OrganizationUnit, לפי המזהה הגולמי
7. **הערה:** `LeadDocumentTitle` נשאר `null` — דורש proxy לשירות המסמכים המובילים שטרם קיים; המזהה הגולמי מוחזר במקום

**מחזיר:** רשימת בקשות מועשרות בשמות; רשימה ריקה אם אין התאמות (חיפוש — אף פעם לא 404)

---

### AuthenticationRequestByLeadDocumentIDs
| שדה | ערך |
|-----|-----|
| **HTTP** | POST |
| **נתיב** | `/AuthenticationRequest/AuthenticationRequestByLeadDocumentIDs` |
| **תיאור** | שליפת בקשות אימות יבוא עבור רשימת מזהי מסמכים מובילים (Internal WCF: `GetAuthenticationRequestByLeadDocumentIDs`) |

**פרמטרים:**
| שם | סוג | תיאור |
|----|-----|--------|
| `leadDocumentIds` | `List<int>` (מגוף הבקשה) | רשימת מזהי מסמכים מובילים |

**ערך מוחזר:** `List<GetAuthenticationRequestByLeadDocumentResultDto>` — רשימת בקשות תואמות (ריקה אם אין)

**לוגיקה עסקית:**

**מקבל:** רשימת מזהי מסמכים מובילים

**מבצע:**
1. ממיר את רשימת המזהים לפרמטר טבלאי (TVP) מסוג `Shared.IntArray`
2. מריץ את השאילתה מול ה-DAL ומקבל את הבקשות התואמות
3. ממלא את שם מדינת היבוא (`ImportCountryId`) דרך lookup משותף של Country
4. ממלא את שם היחידה הארגונית (`OrganizationUnitId`) דרך lookup משותף של OrganizationUnit
5. **הערה:** `LeadDocumentTitle` נשאר `null` — דורש proxy לשירות המסמכים המובילים שטרם קיים

**מחזיר:** רשימת בקשות מועשרות בשמות מדינה/יחידה ארגונית; רשימה ריקה אם אין התאמות

---

### CheckImporterOfImportAuthentication
| שדה | ערך |
|-----|-----|
| **HTTP** | GET |
| **נתיב** | `/AuthenticationRequest/CheckImporterOfImportAuthentication` |
| **תיאור** | בדיקה האם יבואן מסוים ברשימת יבואנים חסומי-אימות (Internal WCF: `CheckImporterOfImportAuthentication`) |

**פרמטרים:**
| שם | סוג | תיאור |
|----|-----|--------|
| `importerId` | `int` | מזהה היבואן לבדיקה |

**ערך מוחזר:** `int?` — מזהה היבואן אם אינו חסום; `null` אם הוא ברשימה החסומה

**לוגיקה עסקית:**

**מקבל:** מזהה יבואן

**מבצע:**
1. בודק מול ה-DAL האם היבואן קיים ברשימת "יבואנים חסומי אימות" (Verification Prohibited Importers)

**מחזיר:** את מזהה היבואן עצמו אם אינו חסום; `null` אם הוא ברשימה החסומה (בדיקה — לא 404)

---

### CheckIfExistsAdditionalRequestsForVendor
| שדה | ערך |
|-----|-----|
| **HTTP** | GET |
| **נתיב** | `/AuthenticationRequest/CheckIfExistsAdditionalRequestsForVendor` |
| **תיאור** | בדיקה האם לספק יש יותר מבקשת אימות אחת בשלוש השנים האחרונות (Internal WCF: `CheckIfExistsAdditionalRequestsForVendor`) |

**פרמטרים:**
| שם | סוג | תיאור |
|----|-----|--------|
| `vendorId` | `int` | מזהה הספק לבדיקה |

**ערך מוחזר:** `bool` — `true` אם קיימות בקשות נוספות

**לוגיקה עסקית:**

**מקבל:** מזהה ספק

**מבצע:**
1. בודק מול ה-DAL האם קיימות יותר מבקשת אימות יבוא אחת לספק זה בטווח שלוש השנים האחרונות

**מחזיר:** `true`/`false` (בדיקה — לא 404)

---

### EntityDocuments
| שדה | ערך |
|-----|-----|
| **HTTP** | GET |
| **נתיב** | `/AuthenticationRequest/EntityDocuments/{leadDocumentId}` |
| **תיאור** | שליפת המסמכים המצורפים למסמך המוביל, הזמינים לצירוף לבקשת אימות (Internal WCF: `GetEntityDocuments`) |

**פרמטרים:**
| שם | סוג | תיאור |
|----|-----|--------|
| `leadDocumentId` | `int` (route) | מזהה מסמך מוביל |

**ערך מוחזר:** `List<DocumentDto>` — רשימת מסמכים תואמים (ריקה אם אין)

**לוגיקה עסקית:**

**מקבל:** מזהה מסמך מוביל (`leadDocumentId`) — במקור ה-WCF קיבל את ישות הבקשה המלאה, אך השתמש רק בשדה `LeadDocumentID` שלה; כאן שוטח לפרמטר סקלרי (אותו תקדים כמו `CheckIfExistsAdditionalRequestsForImporter`)

**מבצע:**
1. שולף מה-DAL את רשימת מזהי המסמכים שכבר רשומים תחת מסמך מוביל זה (`CRM.CertificateOfOrigins_ImportAuthenticationRequest`)
2. שולף מפרמטרי התשתית (`IParametersUtil`, מפתח `CertificateOfOriginsDocumentsFilter`) את רשימת סוגי המסמכים המותרים (CSV של TypeIDs)
3. שולף משירות המסמכים (Documents microservice, דרך `IDocumentsProxy`) את המסמכים המצורפים להצהרת היבוא של המסמך המוביל (`EEntityType.ImportDeclaration` = 1055)
4. מסנן החוצה מסמכים שכבר רשומים תחת מסמך מוביל זה (מהצעד 1)
5. משאיר רק מסמכים מסוג מותר (מהצעד 2)
6. שולף מה-DAL אילו מהמסמכים שנותרו כבר נתפסו (claimed) ע"י מסמך מוביל **אחר**, ומסנן אותם החוצה
7. אם הרשימה שנותרה ריקה — מחזיר רשימה ריקה
8. מעשיר כל מסמך בשם סוג המסמך (`TypeName`) דרך lookup משותף של DocumentType, לפי `TypeId`
9. מרכיב לכל מסמך שדה `Notes` בפורמט `"{Id} {Title} {TypeName}"` (תאימות לגרסה הישנה); `StringDynamicParams` נשאר עם הערות המסמך הגולמיות מהשירות; `OtherRelatedEntities` (קישורי הישות של המסמך) מגיע כפי שהוא מהשירות

**מחזיר:** רשימת מסמכים מסוננים ומועשרים; רשימה ריקה אם אין מסמכים תואמים (שליפה — אף פעם לא 404)

---

### CheckIfExistsAdditionalRequestsForImporter
| שדה | ערך |
|-----|-----|
| **HTTP** | GET |
| **נתיב** | `/AuthenticationRequest/CheckIfExistsAdditionalRequestsForImporter` |
| **תיאור** | בדיקה האם קיימת בקשת אימות יבוא נוספת ליבואן בחלון הזמן המוגדר (Internal WCF: `CheckIfExistsAdditionalRequestsForImporter`) |

**פרמטרים:**
| שם | סוג | תיאור |
|----|-----|--------|
| `importerId` | `int` | מזהה היבואן |
| `vendorId` | `int?` | מזהה ספק (אופציונלי) |
| `customerId` | `int?` | מזהה לקוח (אופציונלי) |
| `countryId` | `int` | מזהה מדינה |

**ערך מוחזר:** `bool` — `true` אם קיימת בקשה נוספת בחלון הזמן

**לוגיקה עסקית:**

**מקבל:** ארבעת השדות הסקלריים — במקור ה-WCF קיבל את ישות הבקשה המלאה, אך השתמש רק בארבעת אלה; כאן הם שוטחו לפרמטרי query

**מבצע:**
1. מעביר את ארבעת הפרמטרים ל-DAL
2. חלון הזמן (`@DaysForLastDelivery`) נשאר בתוך ה-stored procedure (נקרא מפרמטרי התשתית המקומיים) — אינו עניין של ה-BL/קונפיגורציה כאן

**מחזיר:** `true`/`false` (בדיקה — לא 404)

---

### ChangeStatusAfterDeliverySent
| שדה | ערך |
|-----|-----|
| **HTTP** | POST |
| **נתיב** | `/AuthenticationRequest/ChangeStatusAfterDeliverySent` |
| **תיאור** | העלאת אירוע סגירת משימות פתוחות לתיק אימות יבוא לאחר משלוח (Internal WCF: `ChangeStatusAfterDeliverySent`) |

**פרמטרים:**
| שם | סוג | תיאור |
|----|-----|--------|
| `request` | `ChangeStatusAfterDeliverySentRequestDto` (מגוף הבקשה) | מזהה תיק האימות ומזהה היחידה הארגונית |

**ערך מוחזר:** `bool` — `true` תמיד (הצלחה)

**לוגיקה עסקית:**

**מקבל:** `Id` (מזהה תיק אימות) ו-`OrganizationUnitId` — במקור ה-WCF קיבל את ישות `CertificateOfOriginsImportAuthenticationFileDetails` המלאה, אך השתמש רק בשני השדות הללו; כאן שוטחו לשני שדות סקלריים ב-DTO ייעודי

**מבצע:**
1. מעביר passthrough טהור: אינו כותב שום דבר ל-DB בעצמו (שינוי הסטטוס בפועל התבצע במקור בצד הלקוח לפני הקריאה)
2. בונה ומעלה (`IEventUtil`) אירוע מסוג `CloseAllTaskForImportAuthenticationRequestFile` (event-type id 1525) עבור VirtualEntity מסוג `AuthenticationRequestFile` (entity-type id 12385), עם `EntityId` = `Id`, `Title` = `Id` (כמחרוזת), ו-`OrganizationUnitId` = `OrganizationUnitId`
3. שירות ה-Events הוא זה שמטפל בפועל בסגירת המשימות הפתוחות עבור קובץ בקשת האימות, דרך handler התגובה שלו

**מחזיר:** `true` (תמיד — האירוע הועלה בהצלחה; אין כאן סמנטיקת 404 שכן אין שליפת/עדכון ישות ב-DB)

---

### CloseReminderTask
| שדה | ערך |
|-----|-----|
| **HTTP** | POST |
| **נתיב** | `/AuthenticationRequest/CloseReminderTask` |
| **תיאור** | העלאת אירוע סגירת משימת תזכורת 3 חודשים לתיק אימות יבוא (Internal WCF: `HandleSendRemindDeliverNotification`; שם המתודה ב-BL/endpoint שונה משם המתודה בחוזה ה-WCF המקורי — `CloseReminderTask`) |

**פרמטרים:**
| שם | סוג | תיאור |
|----|-----|--------|
| `request` | `CloseReminderTaskRequestDto` (מגוף הבקשה) | מזהה תיק האימות ומזהה היחידה הארגונית |

**ערך מוחזר:** `bool` — `true` תמיד (הצלחה)

**לוגיקה עסקית:**

**מקבל:** `Id` (מזהה תיק אימות) ו-`OrganizationUnitId` — במקור ה-WCF קיבל את ישות `CertificateOfOriginsImportAuthenticationFileDetails` המלאה, אך השתמש רק בשני השדות הללו; כאן שוטחו לשני שדות סקלריים ב-DTO ייעודי (אותו תקדים כמו `ChangeStatusAfterDeliverySent`)

**מבצע:**
1. מעביר passthrough טהור: אינו כותב שום דבר ל-DB (אין DAL, אין proxy)
2. בונה ומעלה (`IEventUtil`) אירוע מסוג `CloseTaskReminderNotice3Months` (event-type id 1745) עבור VirtualEntity מסוג `AuthenticationRequestFile` (entity-type id 12385), עם `EntityId` = `Id`, `Title` = כותרת עברית מחושבת של תיק האימות ("אימות מסמך מקור (יבוא) מספר פניה {Id}"), ו-`OrganizationUnitId` = `OrganizationUnitId`
3. מוסיף related-entity הצבעה על אותה ישות (`AuthenticationRequestFile` באותו `Id`)
4. שירות ה-Events הוא זה שמטפל בפועל בסגירת משימת התזכורת (3 חודשים) עבור קובץ בקשת האימות, דרך handler התגובה שלו

**מחזיר:** `true` (תמיד — האירוע הועלה בהצלחה; אין כאן סמנטיקת 404 שכן אין שליפת/עדכון ישות ב-DB)

---

## 3. מודלי נתונים

### ImportAuthenticationRequestFilterDto
| שדה | סוג | חובה | תיאור |
|-----|-----|------|--------|
| `PreferenceDocumentType` | `int?` | — | סוג מסמך העדפה |
| `GoodsOriginCountry` | `int?` | — | מדינת מקור הטובין |
| `IssuingCountry` | `int?` | — | מדינת הנפקה |
| `ImportCountry` | `int?` | — | מדינת יבוא |
| `FromRequestDate` / `ToRequestDate` | `DateTime?` | — | טווח תאריך בקשה (מוחל תמיד ב-SP) |
| `CustomsHouseId` | `int?` | — | בית מכס |
| `RequestReason` | `int?` | — | סיבת בקשה |
| `LeadDocumentId` | `int?` | — | מזהה מסמך מוביל |
| `ImporterId` | `int?` | — | מזהה יבואן |
| `VendorId` | `int?` | — | מזהה ספק |
| `DecisionId` | `int?` | — | מזהה החלטה |
| `CustomerId` | `int?` | — | מזהה לקוח |
| `DocumentId` | `int?` | — | מזהה מסמך |
| `InvoiceNumber` | `string?` | — | מספר חשבונית |
| `DocumentNumber` | `string?` | — | מספר מסמך |
| `AuthenticationFileId` | `int?` | — | מזהה תיק אימות |
| `CreateUserId` | `int?` | — | משתמש יוצר |

### GetImportAuthenticationRequestResultDto
| שדה | סוג | חובה | תיאור |
|-----|-----|------|--------|
| `DocumentId` | `int?` | — | מזהה מסמך |
| `IssuingCountryId` | `string?` | — | שם מדינת הנפקה (מועשר) |
| `OrganizationUnitId` | `string?` | — | שם יחידה ארגונית (מועשר) |
| `PreferenceDocumentTypeId` | `string?` | — | שם סוג מסמך העדפה (JOIN מקומי) |
| `AuthenticationFileId` | `int?` | — | מזהה תיק אימות |
| `LeadDocumentTitle` | `string?` | — | תמיד `null` (אין proxy); ראו `LeadDocumentId` |
| `CreateDate` | `DateTime` | ✓ | תאריך יצירה |
| `VendorName` | `string?` | — | שם ספק (מועשר) |
| `IssuingCountryIdNum` | `int?` | — | מזהה גולמי של מדינת הנפקה |
| `OrganizationUnitIdNum` | `int?` | — | מזהה גולמי של יחידה ארגונית |
| `ResponseNameEmail` | `string?` | — | דוא"ל למענה |
| `LeadDocumentId` | `int?` | — | מזהה מסמך מוביל גולמי |
| `CustomerId` | `int?` | — | מזהה היבואן (מקור: `ImporterID` ב-SP) |
| `VendorId` | `int?` | — | מזהה ספק |
| `DecisionId` | `int?` | — | מזהה החלטה |
| `ImporterName` | `string?` | — | שם היבואן (מועשר) |
| `AuthenticationFileStatusId` | `int?` | — | סטטוס תיק אימות |

### GetAuthenticationRequestByLeadDocumentResultDto
| שדה | סוג | חובה | תיאור |
|-----|-----|------|--------|
| `LeadDocumentId` | `int` | ✓ | מזהה מסמך מוביל |
| `LeadDocumentTitle` | `string?` | — | תמיד `null` (אין proxy) |
| `DocumentId` | `int` | ✓ | מזהה מסמך |
| `AuthenticationFileId` | `int?` | — | מזהה תיק אימות |
| `PreferenceDocumentTypeId` / `PreferenceDocumentTypeName` | `int?` / `string?` | — | סוג מסמך העדפה (JOIN מקומי) |
| `CreateDate` | `DateTime` | ✓ | תאריך יצירה |
| `AuthenticationFileStatusId` / `AuthenticationFileStatusName` | `int?` / `string?` | — | סטטוס תיק אימות (JOIN מקומי) |
| `DecisionId` / `DecisionName` | `int?` / `string?` | — | החלטה (JOIN מקומי) |
| `ImportCountryId` / `ImportCountryName` | `int?` / `string?` | — | מדינת יבוא (מזהה גולמי + שם מועשר) |
| `OrganizationUnitId` / `OrganizationUnitName` | `int?` / `string?` | — | יחידה ארגונית (מזהה גולמי + שם מועשר) |
| `CollateralId` | `int?` | — | מזהה בטוחה |
| `IsCollateralExists` | `bool` | ✓ | האם קיימת בטוחה |

### DocumentDto
| שדה | סוג | חובה | תיאור |
|-----|-----|------|--------|
| `Id` | `int` | ✓ | מזהה מסמך |
| `TypeId` | `int` | ✓ | מזהה סוג מסמך |
| `TypeName` | `string?` | — | שם סוג מסמך (מועשר ע"י ה-BL דרך lookup של DocumentType) |
| `IsIncoming` | `bool?` | — | האם מסמך נכנס |
| `CreateDate` | `DateTime` | ✓ | תאריך יצירה |
| `Title` | `string?` | — | כותרת המסמך |
| `IsAccepted` | `bool` | ✓ | האם המסמך אושר |
| `IsRequired` | `bool` | ✓ | האם המסמך נדרש |
| `Notes` | `string?` | — | מורכב ע"י ה-BL: `"{Id} {Title} {TypeName}"` (תאימות לגרסה הישנה) |
| `ExternalId` | `string?` | — | מזהה חיצוני (משירות המסמכים) |
| `StringDynamicParams` | `string?` | — | הערות המסמך הגולמיות (Notes המקורי משירות המסמכים) |
| `OtherRelatedEntities` | `List<EntityDocumentDto>` | ✓ | ישויות נוספות שהמסמך מקושר אליהן |

### EntityDocumentDto
| שדה | סוג | חובה | תיאור |
|-----|-----|------|--------|
| `EntityId` | `int` | ✓ | מזהה הישות המקושרת |
| `EntityTypeId` | `int` | ✓ | סוג הישות המקושרת |

### ChangeStatusAfterDeliverySentRequestDto
| שדה | סוג | חובה | תיאור |
|-----|-----|------|--------|
| `Id` | `int` | ✓ | מזהה תיק בקשת האימות (`EntityId` של האירוע המועלה) |
| `OrganizationUnitId` | `int` | ✓ | מזהה היחידה הארגונית (מועבר לאירוע) |

### CloseReminderTaskRequestDto
| שדה | סוג | חובה | תיאור |
|-----|-----|------|--------|
| `Id` | `int` | ✓ | מזהה תיק בקשת האימות (`EntityId` של האירוע המועלה, וגם ה-related-entity) |
| `OrganizationUnitId` | `int` | ✓ | מזהה היחידה הארגונית (מועבר לאירוע) |

---

## 4. תלויות חיצוניות
| רכיב | תיאור שימוש |
|-------|-------------|
| `ICustomerProxy` | העשרת שם היבואן (`ImporterName`) לפי `CustomerId` |
| `IVendorProxy` | העשרת שם הספק (`VendorName`) לפי `VendorId` |
| `IDocumentsProxy` | שליפת המסמכים המצורפים למסמך המוביל משירות המסמכים (Documents microservice) ב-`EntityDocuments` |
| `ILookupUtil` | העשרת שמות מדינה (`Country`) ויחידה ארגונית (`OrganizationUnit`) בשתי מתודות החיפוש; העשרת שם סוג מסמך (`DocumentType`) ב-`EntityDocuments` |
| `IParametersUtil` | קריאת רשימת סוגי המסמכים המותרים (מפתח `CertificateOfOriginsDocumentsFilter`) ב-`EntityDocuments` |
| TVP `Shared.IntArray` | העברת רשימת מזהי מסמכים מובילים ל-stored procedure ב-`AuthenticationRequestByLeadDocumentIDs` |
| `IEventUtil` | העלאת אירוע `CloseAllTaskForImportAuthenticationRequestFile` (event-type id 1525) ב-`ChangeStatusAfterDeliverySent`, ואירוע `CloseTaskReminderNotice3Months` (event-type id 1745) ב-`CloseReminderTask`; נפתר lazily (`Resolve<IEventUtil>()`), רשום דרך `AddEventUtil()` |

---

## 5. הערות
- `LeadDocumentTitle` נשאר `null` בשתי מתודות החיפוש — TODO(migration): דורש proxy לשירות הבעלים של מסמך ה-CRP.DealFile, שטרם קיים
- `EntityDocuments`: הנתיב (route) של ה-endpoint בשירות המסמכים (Documents microservice) טרם אושר מול הצוות האחראי — TODO(blocking) ב-`DocumentsProxy.GetDocumentsByEntity`, ראו הערה בקוד
- `ChangeStatusAfterDeliverySent`: אירוע `CloseAllTaskForImportAuthenticationRequestFile` (event-type id 1525) מועלה עבור VirtualEntity מסוג `AuthenticationRequestFile` (entity-type id 12385). ה-endpoint הוא passthrough בלבד — אינו כותב סטטוס ל-DB; שינוי הסטטוס בפועל וסגירת המשימות מטופלים ב-side של שירות ה-Events (response handler). החלטת מפתח (29/07/2026): לשמור נאמנות מלאה ל-WCF המקורי — event-raise בלבד
- `CloseReminderTask`: שם המתודה ב-BL/endpoint (`CloseReminderTask`) שונה משם המתודה בחוזה ה-WCF המקורי (`HandleSendRemindDeliverNotification`) — נשמר כאן לתיעוד. אירוע `CloseTaskReminderNotice3Months` (event-type id 1745) מועלה עבור VirtualEntity מסוג `AuthenticationRequestFile` (entity-type id 12385), כולל related-entity לאותה ישות. ה-endpoint הוא passthrough בלבד — אינו כותב ל-DB; סגירת משימת התזכורת בפועל מטופלת ב-side של שירות ה-Events (response handler). אין DAL, אין proxy — תלות יחידה היא `IEventUtil`
