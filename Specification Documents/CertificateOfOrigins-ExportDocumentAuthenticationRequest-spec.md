# אפיון: CertificateOfOrigins — ExportDocumentAuthenticationRequest API

> **תאריך:** 28/07/2026
> **Controller:** `ExportDocumentAuthenticationRequestController` (`/ExportDocumentAuthenticationRequest`)

---

## 1. תיאור כללי
ה-controller חושף חיפוש ושליפה עבור בקשות אימות מסמכי יצוא (Export Document Authentication Request) — התהליך שבו בית מכס זר מאמת מסמכי העדפה עבור טובין מיוצאים, וכן שליפת פרטי לקוח (בית מכס זר) הדרושים לתהליך. צרכן: ה-SPA הפנימי (Internal). כולל שליפת לקוח בודד, שליפת בית מכס זר לפי מדינה, שליפת בקשה בודדת עם כל הגרף שלה, וחיפוש לפי מסנן.

---

## 2. נקודות קצה

### CustomerInformation
| שדה | ערך |
|-----|-----|
| **HTTP** | GET |
| **נתיב** | `/ExportDocumentAuthenticationRequest/CustomerInformation/{customerId}` |
| **תיאור** | שליפת פרטי לקוח בודד לפי מזהה, כולל כתובות (Internal WCF: `GetCustomerInformation`) |

**פרמטרים:**
| שם | סוג | תיאור |
|----|-----|--------|
| `customerId` | `int` (מהנתיב) | מזהה הלקוח |

**ערך מוחזר:** `CustomerDto` — פרטי הלקוח כולל כתובות; מזהה לא קיים מחזיר 404

**לוגיקה עסקית:**

**מקבל:** מזהה לקוח

**מבצע:**
1. שולף משירות הלקוחות (Customers proxy) את פרטי הלקוח לפי מזהה
2. אם לא נמצא לקוח — נזרקת חריגת 404 (`RestNotFoundException`)
3. בחירת הכתובת הרלוונטית (מטרת Authentication, או הראשונה) מתבצעת בצד הלקוח (SPA) ולא ב-BL

**מחזיר:** פרטי הלקוח כולל רשימת כתובות; 404 אם המזהה לא קיים

---

### CustomerInformationByCountry
| שדה | ערך |
|-----|-----|
| **HTTP** | GET |
| **נתיב** | `/ExportDocumentAuthenticationRequest/CustomerInformationByCountry/{countryId}` |
| **תיאור** | שליפת בית המכס הזר עבור מדינה נתונה (Internal WCF: `GetCustomerInformationByCountry`) |

**פרמטרים:**
| שם | סוג | תיאור |
|----|-----|--------|
| `countryId` | `int` (מהנתיב) | מזהה המדינה |

**ערך מוחזר:** `CustomerDto` — פרטי בית המכס הזר הראשון התואם; אין התאמה מחזיר 404

**לוגיקה עסקית:**

**מקבל:** מזהה מדינה

**מבצע:**
1. שולף משירות הלקוחות (Customers proxy) את הלקוחות עבור המדינה, מסונן לפי סוג פעילות "בית מכס זר" (Foreign customs house) — הסינון מתבצע בתוך ה-proxy
2. אם לא נמצאה אף תוצאה — נזרקת חריגת 404
3. אחרת מוחזרת התוצאה הראשונה מהרשימה

**מחזיר:** פרטי בית המכס הזר הראשון; 404 אם אין בית מכס זר למדינה זו

---

### ExportDocumentAuthenticationRequestByID
| שדה | ערך |
|-----|-----|
| **HTTP** | GET |
| **נתיב** | `/ExportDocumentAuthenticationRequest/ExportDocumentAuthenticationRequestByID/{id}` |
| **תיאור** | שליפת בקשת אימות מסמך יצוא בודדת עם אוספי הבת שלה (Internal WCF: `GetExportDocumentAuthenticationRequestByID`) |

**פרמטרים:**
| שם | סוג | תיאור |
|----|-----|--------|
| `id` | `int` (מהנתיב) | מזהה בקשת אימות מסמך היצוא |

**ערך מוחזר:** `GetExportDocumentAuthenticationRequestByIdResultDto` — הבקשה המלאה עם אוספי הבת; מזהה לא קיים מחזיר 404

**לוגיקה עסקית:**

**מקבל:** מזהה בקשת אימות מסמך יצוא

**מבצע:**
1. שולף מה-DAL את הבקשה כולל שלושת אוספי הבת שלה (פריטי מכס, מסמכים מובילים, אזורי ייצור)
2. אם לא נמצאה בקשה — נזרקת חריגת 404 (`RestNotFoundException`)
3. ממפה את הבקשה ל-DTO התשובה, כולל חישוב `OriginalStatusId` (תמונת מצב של הסטטוס הנוכחי, לשימוש בבדיקת "עדכון מלוכלך" אופטימית בעת שמירה עתידית)
4. מחשב את `ExportDeclarationIds` — רשימת מזהי המסמכים המובילים מתוך אוסף המסמכים המובילים (מחליף את המילון `EntityTypeAndIDsToSearch` המקורי, ששימש רק את בורר צירוף המסמכים הישן ב-WPF)

**מחזיר:** הבקשה המלאה עם אוספי הבת; 404 אם המזהה לא קיים

---

### ExportDocumentAuthenticationRequestSearch
| שדה | ערך |
|-----|-----|
| **HTTP** | GET |
| **נתיב** | `/ExportDocumentAuthenticationRequest/ExportDocumentAuthenticationRequestSearch` |
| **תיאור** | חיפוש בקשות אימות מסמכי יצוא לפי מסנן (Internal WCF: `GetExportDocumentAuthenticationRequestSearch`) |

**פרמטרים:**
| שם | סוג | תיאור |
|----|-----|--------|
| `filter` | `ExportDocumentAuthenticationRequestSearchFilterDto` | קריטריוני חיפוש (מדינה, סוג מסמך, מזהה בקשה, בית מכס זר, תאריכי פתיחת בקשה ועוד) |

**ערך מוחזר:** `List<GetExportDocumentAuthenticationRequestSearchResultDto>` — רשימת בקשות תואמות (ריקה אם אין)

**לוגיקה עסקית:**

**מקבל:** מסנן חיפוש עם עד 12 קריטריונים אופציונליים

**מבצע:**
1. בונה פרמטרים ל-stored procedure מתוך שדות המסנן
2. מריץ את השאילתה מול ה-DAL ומקבל את רשימת הבקשות התואמות
3. מעשיר כל בקשה בשם בית המכס הזר (`CustomerId`) ובשם מגיש הבקשה (`ExporterCustomerId`) — שניהם דרך שירות הלקוחות (Customers proxy)
4. ממלא את שם המדינה (`CountryId`) דרך lookup משותף של Country, לפי המזהה הגולמי

**מחזיר:** רשימת בקשות מועשרות בשמות; רשימה ריקה אם אין התאמות (חיפוש — אף פעם לא 404)

---

## 3. מודלי נתונים

### CustomerDto [*(חיצוני)* — פרויקציה של Customers microservice]
| שדה | סוג | חובה | תיאור |
|-----|-----|------|--------|
| `Id` | `int` | ✓ | מזהה לקוח |
| `Name` | `string?` | — | שם הלקוח |
| `ExternalIdNum` | `string?` | — | מספר מזהה חיצוני |
| `IsActive` | `bool` | ✓ | האם פעיל |
| `Addresses` | `List<CustomerAddressDto>?` | — | רשימת כתובות |

### CustomerAddressDto [*(חיצוני)*]
| שדה | סוג | חובה | תיאור |
|-----|-----|------|--------|
| `AddressPurpose` | `int` | ✓ | קוד מטרת הכתובת |
| `AddressSingleLine` | `string?` | — | הכתובת כשורה אחת |

### GetExportDocumentAuthenticationRequestByIdResultDto
| שדה | סוג | חובה | תיאור |
|-----|-----|------|--------|
| `Id` | `int` | ✓ | מזהה בקשה |
| `TypeId` | `int` | ✓ | סוג בקשה |
| `Title` | `string` | ✓ | כותרת |
| `TimeStamp` | `byte[]?` | — | חותמת זמן (concurrency) |
| `CustomerId` | `int` | ✓ | מזהה בית מכס זר |
| `AuthenticationDocumentTypeId` | `int` | ✓ | סוג מסמך אימות |
| `ExporterCustomerId` | `int?` | — | מזהה יצואן/מגיש הבקשה |
| `StatusId` | `int?` | — | סטטוס נוכחי |
| `OriginalStatusId` | `int` | ✓ | תמונת מצב של הסטטוס בעת השליפה (לבדיקת עדכון מלוכלך) |
| `CountryId` | `int?` | — | מזהה מדינה |
| `CustomsHouseAddress` | `string?` | — | כתובת בית המכס |
| `VendorId` | `int?` | — | מזהה ספק |
| `AuthenticationRequestArrivalDate` | `DateTime?` | — | תאריך קבלת בקשת האימות |
| `AuthenticationRequestedByName` | `string?` | — | שם מבקש האימות |
| `AuthenticationRequestedByEmail` | `string?` | — | דוא"ל מבקש האימות |
| `AuthenticationRequestedByPhone` | `string?` | — | טלפון מבקש האימות |
| `AuthenticationRequestNotes` | `string` | ✓ | הערות לבקשת האימות |
| `ExportLeadDocumentId` | `int?` | — | מזהה מסמך יצוא מוביל |
| `DocumentId` | `int?` | — | מזהה מסמך |
| `MainDocumentTitle` | `string?` | — | כותרת המסמך הראשי |
| `LastDeliveryDate` | `DateTime?` | — | תאריך אספקה אחרון |
| `DeliveryMethodId` | `int?` | — | שיטת אספקה |
| `InvoiceNumbers` | `string?` | — | מספרי חשבוניות |
| `DetailedDecision` | `string?` | — | החלטה מפורטת |
| `ReferenceNumber` | `string?` | — | מספר אסמכתא |
| `CommentForCustomsHouseLetter` | `string?` | — | הערה למכתב בית המכס |
| `TotalDocuments` | `int?` | — | סה"כ מסמכים |
| `TotalInvoices` | `int?` | — | סה"כ חשבוניות |
| `DocumentDate` | `DateTime?` | — | תאריך מסמך |
| `InvoiceDate` | `DateTime?` | — | תאריך חשבונית |
| `ExportDeclarationIds` | `List<int>` | — | מזהי מסמכים מובילים (מחושב, לצירוף מסמכים) |
| `CustomsItems` | `List<ExportDocumentAuthenticationRequestCustomsItemDto>` | — | פריטי מכס |
| `LeadDocuments` | `List<ExportDocumentAuthenticationRequestLeadDocumentDto>` | — | מסמכים מובילים |
| `ManufacturingAreas` | `List<ExportAuthenticationRequestManufacturingAreaDto>` | — | אזורי ייצור |

> **הערה:** 6 שדות (`State`, `CreateDate`, `CreateUserId`, `UpdateDate`, `UpdateUserId`, `OrganizationUnitId`) הושמטו זמנית מה-DTO — מגבלת פלטפורמה (`MaxCountExceededInterceptor` חוסם מעל 30 עמודות תוצאה, והישות המקורית מכילה 35). ישוחזרו לאחר הוספת hash ייעודי ל-InterceptorList.

### ExportDocumentAuthenticationRequestCustomsItemDto
| שדה | סוג | חובה | תיאור |
|-----|-----|------|--------|
| `Id` | `int` | ✓ | מזהה שורה |
| `ExportDocumentAuthenticationRequestId` | `int` | ✓ | מזהה בקשת האימות |
| `CustomsItemId` | `int` | ✓ | מזהה פריט מכס |

### ExportDocumentAuthenticationRequestLeadDocumentDto
| שדה | סוג | חובה | תיאור |
|-----|-----|------|--------|
| `Id` | `int` | ✓ | מזהה שורה |
| `ExportRequestId` | `int` | ✓ | מזהה בקשת היצוא |
| `LeadDocumentId` | `int?` | — | מזהה מסמך מוביל |
| `LeadDocumentTitle` | `string` | ✓ | כותרת המסמך המוביל |

### ExportAuthenticationRequestManufacturingAreaDto
| שדה | סוג | חובה | תיאור |
|-----|-----|------|--------|
| `Id` | `int` | ✓ | מזהה שורה |
| `ExportAuthenticationRequestId` | `int` | ✓ | מזהה בקשת האימות |
| `ManufacturingArea` | `string?` | — | אזור ייצור |
| `ManufacturingZipcode` | `string?` | — | מיקוד אזור הייצור |

### GetExportDocumentAuthenticationRequestSearchResultDto
| שדה | סוג | חובה | תיאור |
|-----|-----|------|--------|
| `RequestId` | `int` | ✓ | מזהה בקשה |
| `CountryName` | `string?` | — | שם מדינה (מועשר) |
| `CountryId` | `int?` | — | מזהה מדינה גולמי |
| `ForeignCustomsHouseName` | `string?` | — | שם בית המכס הזר (מועשר) |
| `CustomerId` | `int?` | — | מזהה בית המכס הזר |
| `DocumentTypeName` | `string?` | — | שם סוג מסמך (JOIN מקומי) |
| `ExportDeclarationTitle` | `string?` | — | כותרת הצהרת יצוא (JOIN מקומי) |
| `RequestStatusName` | `string?` | — | שם סטטוס בקשה (JOIN מקומי) |
| `RequestIssuerName` | `string?` | — | שם מגיש הבקשה (מועשר) |
| `ExporterCustomerId` | `int?` | — | מזהה יצואן/מגיש הבקשה גולמי |
| `ExportLeadDocumentId` | `int?` | — | מזהה מסמך יצוא מוביל |

### ExportDocumentAuthenticationRequestSearchFilterDto
| שדה | סוג | חובה | תיאור |
|-----|-----|------|--------|
| `CountryId` | `int?` | — | מדינה |
| `DocumentTypeId` | `int?` | — | סוג מסמך |
| `RequestId` | `int?` | — | מזהה בקשה |
| `ForeignCustomsHouseId` | `int?` | — | בית מכס זר |
| `ExportDeclarationId` | `int?` | — | מזהה הצהרת יצוא (פרמטר מת ב-SP, נשמר לצורך נאמנות חוזה) |
| `RequestOpenDateFrom` / `RequestOpenDateTo` | `DateTime?` | — | טווח תאריך פתיחת בקשה |
| `ExportAuthenticationDocumentId` | `int?` | — | מזהה מסמך אימות יצוא |
| `InvoiceIdNum` | `string?` | — | מספר חשבונית |
| `MainDocumentTitle` | `string?` | — | כותרת מסמך ראשי |
| `ExporterCustomerId` | `int?` | — | מזהה יצואן/מגיש הבקשה |
| `ExportAuthenticationRequestStatusId` | `int?` | — | סטטוס בקשת אימות יצוא |
| `CreateUserId` | `int?` | — | משתמש יוצר |

---

## 4. תלויות חיצוניות
| רכיב | תיאור שימוש |
|-------|-------------|
| `ICustomerProxy` | שליפת פרטי לקוח בודד (`CustomerInformation`), שליפת בתי מכס זרים לפי מדינה (`CustomerInformationByCountry`), והעשרת שמות בית מכס זר/מגיש בקשה בחיפוש (`ExportDocumentAuthenticationRequestSearch`) |
| `ILookupUtil` | העשרת שם המדינה (`Country`) בחיפוש (`ExportDocumentAuthenticationRequestSearch`) |

---

## 5. הערות
- `GetExportDocumentAuthenticationRequestByIdResultDto`: 6 שדות הושמטו זמנית עקב מגבלת `MaxCountExceededInterceptor` (מעל 30 עמודות) — TODO(migration) לשחזור לאחר הוספת hash ל-InterceptorList
- אין
