# אפיון: CertificateOfOrigins — Internal API

> **תאריך:** 03/07/2026
> **Controller:** `CertificateOfOriginInternalController` (`/Internal`)

---

## 1. תיאור כללי
ה-controller חושף נקודות קצה פנימיות לצריכה בין-שירותית (inter-service) בתוך מערכת CustomsCloud, סביב הדומיין של תעודות מקור (Certificates of Origin) ובקשות אימות יבוא (Import/Export Authentication Requests). הוא מנוהל בעיקר על ידי שתי מחלקות לוגיקה — `AuthenticationRequestBl` (ניהול בקשות ותיקי אימות יבוא, כולל זרימות משלוח/תזכורת) ו-`ExportDocumentAuthenticationRequestBl` (שאילתות לקוחות/בתי מכס לצורך מסמכי ייצוא) — לצד קריאות ישירות ל-`CertificateOfOriginBl` עבור תעודות מקור עצמן.
השירות הומר מ-WCF ל-.NET 10 במסגרת מיגרציית מודול מלאה; חלק מהאופרציות (שמירות מרכזיות, הודעות, תבניות) טרם הומרו — מתועדות ב-`MIGRATION-NOT-DONE.md` בשורש הריפו.

---

## 2. נקודות קצה

### HandleImportAuthenticationRequestDeliveryForImporterSent
| שדה | ערך |
|-----|-----|
| **HTTP** | POST |
| **נתיב** | `/Internal/HandleImportAuthenticationRequestDeliveryForImporterSent` |
| **תיאור** | מטפל באירוע משלוח בקשת אימות ליבואן — קובע החלטה, מעדכן תאריכים ומעדכן את תיק האימות הקשור |

**פרמטרים:**
| שם | סוג | תיאור |
|----|-----|--------|
| `authenticationRequest` | `ImportAuthenticationRequestDto` | בקשת האימות לעדכון (`[FromBody]`) |

**ערך מוחזר:** `ImportAuthenticationRequestDto` — בקשת האימות המעודכנת

**לוגיקה עסקית:**

**מקבל:** אובייקט `ImportAuthenticationRequestDto` המייצג בקשת אימות יבוא.

**מבצע:**
1. קורא לפונקציה המשותפת הפרטית `HandleReminderOrDeliveryRequestSentToImporter` עם סוג אירוע `NewDeliveryForImporterSent` (1511) והחלטה `LetterForImporterWasSent` (8).
2. בתוך הפונקציה המשותפת: מגדיר את `DecisionId` להחלטה שהתקבלה, ומעדכן את `LastDeliveryForImporter` ואת `UpdateDate` לתאריך היום.
3. אם קיים `AuthenticationFileId` על הבקשה — טוען את תיק האימות המשויך מה-DB, ואם נמצא, מריץ את מכונת המצבים `UpdateFileAfterDelivery` (ראו תיאור מפורט תחת `HandleImportAuthenticationRequestDeliveryAndReminderForVendorSent` להלן) על התיק ושומר אותו.
4. שומר את בקשת האימות המעודכנת ב-DB.
5. בונה ומפעיל אירוע (`IEventUtil`) מסוג האירוע שהתקבל, עם מזהה ישות = מזהה המסמך, סוג ישות `ImportAuthenticationRequest` (12384), וכותרת = מזהה המסמך; אם קיים תיק אימות משויך — מוסיף אותו כישות קשורה מסוג `AuthenticationRequestFile` (12385).

**מחזיר:** את בקשת האימות עם ההחלטה, תאריך המשלוח ותאריך העדכון המעודכנים.

---

### HandleImportAuthenticationRequestDeliveryReminderForImporterSent
| שדה | ערך |
|-----|-----|
| **HTTP** | POST |
| **נתיב** | `/Internal/HandleImportAuthenticationRequestDeliveryReminderForImporterSent` |
| **תיאור** | מטפל באירוע שליחת תזכורת ליבואן — זהה במבנה למשלוח הרגיל, בהחלטה ובסוג אירוע שונים |

**פרמטרים:**
| שם | סוג | תיאור |
|----|-----|--------|
| `authenticationRequest` | `ImportAuthenticationRequestDto` | בקשת האימות לעדכון (`[FromBody]`) |

**ערך מוחזר:** `ImportAuthenticationRequestDto` — בקשת האימות המעודכנת

**לוגיקה עסקית:**

**מקבל:** אובייקט `ImportAuthenticationRequestDto`.

**מבצע:**
1. קורא לאותה פונקציה משותפת `HandleReminderOrDeliveryRequestSentToImporter` (ראו תיאור מלא לעיל תחת `HandleImportAuthenticationRequestDeliveryForImporterSent`), הפעם עם סוג אירוע `NewDeliveryReminderForImporterSent` (1512) והחלטה `ReminderForImporterWasSent` (9).

**מחזיר:** את בקשת האימות עם ההחלטה, תאריך המשלוח ותאריך העדכון המעודכנים.

---

### HandleImportAuthenticationRequestDeliveryAndReminderForVendorSent
| שדה | ערך |
|-----|-----|
| **HTTP** | POST |
| **נתיב** | `/Internal/HandleImportAuthenticationRequestDeliveryAndReminderForVendorSent` |
| **תיאור** | מטפל במשלוח/תזכורת לתיק אימות מול ספק — מקדם את מכונת המצבים של סטטוס התיק ושיטת המשלוח |

**פרמטרים:**
| שם | סוג | תיאור |
|----|-----|--------|
| `authenticationFile` | `ImportAuthenticationFileDetailsDto` | תיק האימות לעדכון (`[FromBody]`) |
| `isDelivery` | `bool` | `true` עבור משלוח רגיל, `false` עבור תזכורת (`[FromQuery]`) |

**ערך מוחזר:** `ImportAuthenticationFileDetailsDto` — תיק האימות המעודכן

**לוגיקה עסקית:**

**מקבל:** תיק אימות ודגל המבחין בין משלוח לתזכורת.

**מבצע:**
1. אם `isDelivery` הוא `false` (כלומר מדובר בתזכורת) — מעדכן את `AuthenticationFileStatusId` לערך `AuthenticationRequestReminderWasSend` (3).
2. מריץ את `UpdateFileAfterDelivery` על התיק — מכונת מצבים המקדמת את הסטטוס/שיטת המשלוח:
   - אם הסטטוס `WaitingForSendingLetter` (1) — מעדכן לסטטוס `AuthenticationRequestWasSend` (2) ושיטת משלוח `PostedMailing` (2).
   - אם הסטטוס `AuthenticationRequestWasSend` (2): אם שיטת המשלוח `PostedMailing` (2) או `SentByEmailRequest` (3) — מעדכן ל-`FirstRemindSent` (4); אם שיטת המשלוח כבר `FirstRemindSent` (4) — מעדכן ל-`SecondRemindSent` (5).
   - אם הסטטוס `AuthenticationRequestReminderWasSend` (3) ושיטת המשלוח `FirstRemindSent` (4) — מעדכן ל-`SecondRemindSent` (5).
   - מעדכן את `LastDelivery` ו-`UpdateDate` לתאריך היום.
   - שומר את התיק ב-DB, ומעדכן את `UpdateDate` של כל בקשות האימות המשויכות לתיק לזמן הנוכחי.

**מחזיר:** את תיק האימות עם הסטטוס ושיטת המשלוח המעודכנים.

---

### HandleSendRemindDeliverNotification
| שדה | ערך |
|-----|-----|
| **HTTP** | POST |
| **נתיב** | `/Internal/HandleSendRemindDeliverNotification` |
| **תיאור** | סוגר משימת תזכורת פתוחה (3 חודשים) עבור תיק אימות באמצעות הפעלת אירוע ייעודי |

**פרמטרים:**
| שם | סוג | תיאור |
|----|-----|--------|
| `file` | `ImportAuthenticationFileDetailsDto` | תיק האימות שעבורו נסגרת התזכורת (`[FromBody]`) |

**ערך מוחזר:** `bool` — `true` תמיד לאחר הפעלת האירוע

**לוגיקה עסקית:**

**מקבל:** תיק אימות.

**מבצע:**
1. בונה בקשת אירוע מסוג `CloseTaskReminderNotice3Months` (1745), עם מזהה ישות = מזהה התיק, סוג ישות `AuthenticationRequestFile` (12385), כותרת = מזהה התיק, ומוסיף את התיק עצמו כישות קשורה.
2. מפעיל את האירוע דרך `IEventUtil`.

**מחזיר:** `true` — תמיד.

---

### ChangeStatusAfterDeliverySent
| שדה | ערך |
|-----|-----|
| **HTTP** | POST |
| **נתיב** | `/Internal/ChangeStatusAfterDeliverySent` |
| **תיאור** | סוגר את כל המשימות הפתוחות עבור תיק אימות אחרי משלוח, באמצעות הפעלת אירוע ייעודי |

**פרמטרים:**
| שם | סוג | תיאור |
|----|-----|--------|
| `importAuthenticationRequest` | `ImportAuthenticationFileDetailsDto` | תיק האימות שעבורו נסגרות המשימות (`[FromBody]`) |

**ערך מוחזר:** `ImportAuthenticationFileDetailsDto` — תיק האימות ללא שינוי

**לוגיקה עסקית:**

**מקבל:** תיק אימות.

**מבצע:**
1. בונה בקשת אירוע מסוג `CloseAllTaskForImportAuthenticationRequestFile` (1525), עם מזהה ישות = מזהה התיק, סוג ישות `AuthenticationRequestFile` (12385), כותרת = מזהה התיק ויחידה ארגונית מהתיק.
2. מפעיל את האירוע דרך `IEventUtil`.

**מחזיר:** את תיק האימות שהתקבל, ללא שינוי בנתונים.

---

### CheckIfExistsAdditionalRequestsForImporter
| שדה | ערך |
|-----|-----|
| **HTTP** | GET |
| **נתיב** | `/Internal/CheckIfExistsAdditionalRequestsForImporter` |
| **תיאור** | בודק האם קיימות בקשות אימות יבוא נוספות עבור יבואן, תוך הפרדה בין מסלול ספק למסלול לקוח לפי מדינה |

**פרמטרים:**
| שם | סוג | תיאור |
|----|-----|--------|
| `importerId` | `int` | מזהה היבואן (`[FromQuery]`) |
| `vendorId` | `int?` | מזהה הספק — אופציונלי (`[FromQuery]`) |
| `customerId` | `int?` | מזהה הלקוח — אופציונלי (`[FromQuery]`) |
| `countryId` | `int` | מזהה המדינה לקביעת מסלול ספק/לקוח (`[FromQuery]`) |

**ערך מוחזר:** `bool` — `true` אם קיימות בקשות אימות נוספות, `false` אחרת

**לוגיקה עסקית:**

**מקבל:** מזהה יבואן, מזהה ספק (אופציונלי), מזהה לקוח (אופציונלי) ומזהה מדינה.

**מבצע:**
1. בודק ב-DAL האם המדינה מוגדרת כמדינת ספק (`IsVendorDeliveryCountryConfigured`).
2. שולף מ-`IParametersUtil` את מספר הימים לחיפוש אחורה (`AdditionalRequestsForSearchInDays`).
3. קורא ל-DAL (`CheckIfExistsAdditionalRequestsForImporter`) עם מזהה היבואן, מזהה הספק/לקוח, דגל "האם ספק" ומספר הימים — ה-DAL בוחר לפי הדגל אם לחפש לפי `VendorId` או `CustomerId`, בטווח הימים שהוגדר.

**מחזיר:** `true` אם קיימות בקשות אימות נוספות ליבואן בטווח הזמן המוגדר, `false` אחרת.

---

### CheckIfExistsAdditionalRequestsForVendor
| שדה | ערך |
|-----|-----|
| **HTTP** | GET |
| **נתיב** | `/Internal/CheckIfExistsAdditionalRequestsForVendor` |
| **תיאור** | בודק האם קיימות יותר מבקשת אימות יבוא אחת עבור ספק נתון בשלוש השנים האחרונות |

**פרמטרים:**
| שם | סוג | תיאור |
|----|-----|--------|
| `vendorId` | `int` | מזהה הספק לבדיקה (`[FromQuery]`) |

**ערך מוחזר:** `bool` — `true` אם קיימת יותר מבקשה אחת, `false` אחרת

**לוגיקה עסקית:**

**מקבל:** מזהה ספק.

**מבצע:**
1. קורא ל-DAL (`CheckIfExistsAdditionalRequestsForVendor`) המבצע ספירה של בקשות אימות עבור הספק בשלוש השנים האחרונות.

**מחזיר:** `true` אם מספר בקשות האימות לספק בטווח שלוש השנים גדול מאחת, `false` אחרת.

---

### CheckImporterOfImportAuthentication
| שדה | ערך |
|-----|-----|
| **HTTP** | GET |
| **נתיב** | `/Internal/CheckImporterOfImportAuthentication` |
| **תיאור** | בודק האם יבואן נתון מופיע ברשימת יבואנים מוגבלים (Prohibited Importers) |

**פרמטרים:**
| שם | סוג | תיאור |
|----|-----|--------|
| `importerId` | `int` | מזהה היבואן לבדיקה (`[FromQuery]`) |

**ערך מוחזר:** `int?` — מזהה ישות של הרשומה האוסרת אם נמצאה, או `null` אם היבואן אינו מוגבל

**לוגיקה עסקית:**

**מקבל:** מזהה יבואן.

**מבצע:**
1. קורא ל-DAL (`CheckImporterOfImportAuthentication`) עם מזהה היבואן, המבצע חיפוש ברשימת היבואנים המוגבלים.

**מחזיר:** מזהה הישות של הרשומה האוסרת אם היבואן מוגבל, או `null` אם אינו מוגבל.

---

### GetCustomerInformation
| שדה | ערך |
|-----|-----|
| **HTTP** | GET |
| **נתיב** | `/Internal/GetCustomerInformation` |
| **תיאור** | מחזיר פרטי לקוח לפי מספר זיהוי |

**פרמטרים:**
| שם | סוג | תיאור |
|----|-----|--------|
| `customerId` | `int` | מספר הזיהוי של הלקוח (`[FromQuery]`) |

**ערך מוחזר:** `CustomerDto` — פרטי הלקוח המלאים

**לוגיקה עסקית:**

**מקבל:** מספר זיהוי לקוח.

**מבצע:**
1. קורא ל-`ICustomerProxy.GetCustomerByIdentification` עם מספר הזיהוי שהתקבל.
2. אם הלקוח לא נמצא — זורק שגיאת ולידציה בקוד 404 עם ההודעה "הלקוח לא קיים במערכת".

**מחזיר:** `CustomerDto` עם כל פרטי הלקוח אם נמצא; שגיאת 404 אם לא נמצא לקוח עם מספר הזיהוי הנתון.

---

### GetCustomerInformationByCountry
| שדה | ערך |
|-----|-----|
| **HTTP** | GET |
| **נתיב** | `/Internal/GetCustomerInformationByCountry` |
| **תיאור** | מחזיר פרטי בית מכס זר (לקוח מסוג "בית מכס זר") עבור מדינה מסוימת |

**פרמטרים:**
| שם | סוג | תיאור |
|----|-----|--------|
| `countryId` | `int` | מזהה המדינה לחיפוש בית מכס זר (`[FromQuery]`) |

**ערך מוחזר:** `CustomerDto` — פרטי בית המכס הזר הראשון הרשום עבור המדינה

**לוגיקה עסקית:**

**מקבל:** מזהה מדינה.

**מבצע:**
1. קורא ל-`ICustomerProxy.GetCustomersByCountryId` עם מזהה המדינה וסוג פעילות "בית מכס זר" (40).
2. אם לא הוחזרו לקוחות — זורק שגיאת ולידציה בקוד 404 עם ההודעה "לא הוגדר בית מכס למדינה זו. יש להגדיר כתובת מתאימה".
3. מחזיר את הרשומה הראשונה מהרשימה שהתקבלה.

**מחזיר:** `CustomerDto` — הרשומה הראשונה מרשימת הלקוחות שהוחזרה; שגיאת 404 אם אין לקוחות הרשומים כבית מכס זר עבור אותה מדינה.

---

### GetExportDocumentAuthenticationRequestByID
| שדה | ערך |
|-----|-----|
| **HTTP** | GET |
| **נתיב** | `/Internal/GetExportDocumentAuthenticationRequestByID` |
| **תיאור** | מחזיר בקשת אימות מסמך ייצוא מלאה לפי מזהה, כולל פריטי מכס, מסמכים מובילים ואזורי ייצור |

**פרמטרים:**
| שם | סוג | תיאור |
|----|-----|--------|
| `id` | `int` | מזהה בקשת אימות מסמך הייצוא (`[FromQuery]`) |

**ערך מוחזר:** `ExportDocumentAuthenticationRequestDto?` — הבקשה המלאה, או `null` אם לא נמצאה

**לוגיקה עסקית:**

**מקבל:** מזהה בקשה.

**מבצע:**
1. קורא ל-DAL (`GetExportDocumentAuthenticationRequestById`) לשליפת הבקשה עם כל האוספים המקוננים שלה (פריטי מכס, מסמכים מובילים, אזורי ייצור).
2. אם לא נמצאה בקשה — מחזיר `null`.
3. מעתיק את `StatusId` (או 0 אם ריק) אל `OriginalStatusId` — שדה מעקב-שינויים המשמש בסיס להשוואה בעת שמירה עתידית.
4. בונה את `EntityTypeAndIdsToSearch` — מילון עם מפתח `ExportDeclaration` (12414) וערך רשימת כל מזהי המסמכים המובילים (`LeadDocumentId`) שאינם ריקים מתוך `ExportDocumentAuthenticationRequestLeadDocument`.

**מחזיר:** אובייקט `ExportDocumentAuthenticationRequestDto` מלא עם כל האוספים המקוננים, או `null` אם לא נמצאה בקשה עם המזהה שניתן.

---

### GetExportDocumentAuthenticationRequestSearch
| שדה | ערך |
|-----|-----|
| **HTTP** | GET |
| **נתיב** | `/Internal/GetExportDocumentAuthenticationRequestSearch` |
| **תיאור** | מחפש בקשות אימות מסמכי ייצוא לפי פילטר, ומעשיר את התוצאות בשמות מדינה ולקוחות |

**פרמטרים:**
| שם | סוג | תיאור |
|----|-----|--------|
| `filter` | `ExportDocumentAuthenticationRequestSearchFilterDto` | פילטר חיפוש (`[FromQuery]`) |

**ערך מוחזר:** `List<ExportDocumentAuthenticationRequestSearchResultDto>` — רשימת תוצאות החיפוש

**לוגיקה עסקית:**

**מקבל:** אובייקט פילטר עם קריטריוני חיפוש (מדינה, סוג מסמך, מזהה בקשה, בית מכס זר, טווחי תאריכים ועוד).

**מבצע:**
1. קורא ל-DAL (`GetExportDocumentAuthenticationRequestSearch`) עם הפילטר לביצוע החיפוש.
2. אם אין תוצאות — מחזיר את הרשימה הריקה מיידית.
3. שולף את כל המדינות (`ILookupUtil.All<Country>`).
4. אוסף את כל מזהי הלקוחות הרלוונטיים (הלקוח עצמו + לקוח מייצא, כאשר קיים) ושולף אותם דרך `ICustomerProxy.GetCustomersByIds`.
5. עבור כל תוצאה: ממלא את `CountryName` לפי המדינה, `ForeignCustomsHouseName` לפי הלקוח, ו-`RequestIssuerName` לפי הלקוח המייצא.

**מחזיר:** רשימת תוצאות מועשרות בשמות תצוגה (מדינה, בית מכס זר, מגיש הבקשה).

---

### GetAuthenticationRequestByLeadDocumentIDs
| שדה | ערך |
|-----|-----|
| **HTTP** | GET |
| **נתיב** | `/Internal/GetAuthenticationRequestByLeadDocumentIDs` |
| **תיאור** | מחזיר רשימת בקשות אימות יבוא שטוחות לפי רשימת מזהי מסמכים מובילים, מועשרות בשמות מדינה ויחידה ארגונית |

**פרמטרים:**
| שם | סוג | תיאור |
|----|-----|--------|
| `leadDocumentIDs` | `List<int>` | רשימת מזהי מסמכים מובילים (`[FromQuery]`) |

**ערך מוחזר:** `List<ImportAuthenticationRequestByLeadDocumentDto>` — רשימת בקשות אימות תואמות

**לוגיקה עסקית:**

**מקבל:** רשימת מזהי מסמכים מובילים.

**מבצע:**
1. אם הרשימה ריקה או `null` — מחזיר רשימה ריקה מיידית.
2. קורא ל-DAL (`GetAuthenticationRequestsByLeadDocumentIds`) לשליפת הבקשות התואמות.
3. אם אין תוצאות — מחזיר את הרשימה הריקה.
4. שולף את כל המדינות ואת כל היחידות הארגוניות (`ILookupUtil.All`).
5. עבור כל בקשה: ממלא את `ImportCountryName` ואת `OrganizationUnitName` לפי הערכים המספריים המתאימים. שדה `LeadDocumentTitle` נותר `null` — במקור הגיע מטבלת DealFile שאין לה מיקרו-שירות מקביל.

**מחזיר:** רשימת בקשות אימות שטוחות מועשרות בשמות תצוגה; שדות `ImporterId` ו-`LastDeliveryForImporter` נותרים ריקים בהתאמה לפרוצדורה המקורית שמעולם לא אכלסה אותם.

---

### GetAuthenticationRequestByID
| שדה | ערך |
|-----|-----|
| **HTTP** | GET |
| **נתיב** | `/Internal/GetAuthenticationRequestByID` |
| **תיאור** | מחזיר אגרגט מלא של בקשת אימות יבוא לפי מזהה מסמך, כולל פרטי כותרת, פריטים, מסמך, החלטות, בטחונות, דגלים הנגזרים ממשימות, ופרמטרים נוספים |

**פרמטרים:**
| שם | סוג | תיאור |
|----|-----|--------|
| `documentId` | `int` | מזהה המסמך של בקשת האימות (`[FromQuery]`) |

**ערך מוחזר:** `ImportAuthenticationRequestDto?` — האגרגט המלא, או `null` אם לא נמצא

**לוגיקה עסקית:**

**מקבל:** מזהה מסמך.

**מבצע:**
1. שולף מה-DAL את כותרת בקשת האימות (`GetAuthenticationRequestById`); אם לא נמצאה — מחזיר `null` מיידית.
2. שולף את פרטי הפריטים (`ItemDetails`) של הבקשה מה-DAL.
3. שולף את המסמך המצורף דרך `IDocumentsProxy.GetDocumentsByIds`; אם נמצא מסמך — ממלא את `Document.FileUrl` בשם סוג המסמך (`TypeName`), לשמירת התאמה עם ההתנהגות המקורית שהשתמשה בשדה זה לתצוגת שם הסוג.
4. מבצע שליפת מידע נוסף (`GetAdditionalInfoForRequest`):
   - שולף את רשימת כל ההחלטות האפשריות מה-DAL.
   - שולף בטחונות (Collaterals) מ-`ICollateralProxy.GetCollateralRequest` עבור סוג ישות `ImportAuthenticationRequest` (12384) ומזהה המסמך.
   - שולף מ-`ITasksProxy.IsTaskExist` משימות מהסוגים `SetDecisionBeforeAssociation` (406), `SendReminderForImporter` (404) ו-`HandleRejectedAuthenticationRequest` (407).
   - שולף את מזהה המשתמש הנוכחי (`IUserUtil`) וקובע את `IsCurrentUserHandleRequest` (קיימת משימה כלשהי של המשתמש) ואת `IsCurrentUserHasOpenTask` (קיימת משימה פתוחה של המשתמש).
   - בונה `EntityTypeAndIdsToSearch` עם מפתח `ImportDeclaration` (1055) וערך רשימת `LeadDocumentId`.
5. שולף מ-`IParametersUtil` את `AdditionalRequestsForSearchInDays` וממלא את השדה המתאים.
6. בודק ב-DAL (`IsVendorCountry`) האם מדינת ההנפקה מוגדרת כמדינת ספק, וממלא את `IsVendorByIssuingCountryId`.

**מחזיר:** אובייקט `ImportAuthenticationRequestDto` מלא, או `null` אם לא נמצאה בקשה עם המזהה שניתן.

---

### GetAuthenticationRequestFileByID
| שדה | ערך |
|-----|-----|
| **HTTP** | GET |
| **נתיב** | `/Internal/GetAuthenticationRequestFileByID` |
| **תיאור** | מחזיר אגרגט מלא של תיק אימות יבוא לפי מזהה תיק, כולל כותרת התיק, רשימת בקשות האימות הכלולות, סטטוסי תיק זמינים ודגל טיפול של המשתמש הנוכחי |

**פרמטרים:**
| שם | סוג | תיאור |
|----|-----|--------|
| `fileId` | `int` | מזהה תיק האימות (`[FromQuery]`) |

**ערך מוחזר:** `ImportAuthenticationFileDetailsDto?` — האגרגט המלא, או `null` אם לא נמצא

**לוגיקה עסקית:**

**מקבל:** מזהה תיק אימות.

**מבצע:**
1. שולף מה-DAL את כותרת תיק האימות (`GetAuthenticationFileById`); אם לא נמצא — מחזיר `null` מיידית.
2. מאתחל את `CustomerIdList` כרשימה ריקה (במקור מעולם לא אוכלסה בנתיב זה).
3. שולף את בקשות האימות המשויכות לתיק (`GetRequestsByAuthenticationFileId`) ומגדיר את `EntityTypeAndIdsToSearch` עם מפתח `ImportDeclaration` (1055) וערך כל מזהי המסמכים המובילים.
4. אם קיימות בקשות עם מזהי מסמך — שולף את המסמכים (`IDocumentsProxy.GetDocumentsByIds`) ואת פרטי הפריטים עבור כל הבקשות, ומצרף לכל בקשה את המסמך שלה, פרטי הפריטים שלה, ומילון חיפוש ישויות ייעודי. (הערה: השדות `LeadDocumentSubmissionDate` ו-`IsSendReminderForImporterTaskExists` שהיו קיימים במקור לא הומרו — אין מיקרו-שירות DealFile מקביל.)
5. אם `CustomerId` על התיק הוא 0 — מעדכן אותו ל-`-1` (לשמירת התאמה עם המקור).
6. שולף את כל ההחלטות ואת כל סטטוסי תיק האימות מה-DAL, וממלא לכל בקשה את רשימת ההחלטות; עבור כל בקשה שולף גם בטחונות (Collaterals) מ-`ICollateralProxy.GetCollateralRequest` עבור סוג ישות `ImportAuthenticationRequest` (12384).
7. שולף מ-`ITasksProxy.IsTaskExist` משימות מהסוגים `ReminderNotice6Months` (339), `ReminderNotice10Months` (340), `HandleAuthenticationRequestFile` (408) ו-`SendReminderForImporter` (404) עבור סוג ישות `AuthenticationRequestFile` (12385) ומזהה התיק.
8. שולף את מזהה המשתמש הנוכחי וקובע `IsCurrentUserHandleFile = true` אם קיימת משימה כלשהי של המשתמש הנוכחי.

**מחזיר:** אובייקט `ImportAuthenticationFileDetailsDto` מלא, או `null` אם לא נמצא תיק עם המזהה שניתן.

---

### CreateNewAuthenticationFile
| שדה | ערך |
|-----|-----|
| **HTTP** | POST |
| **נתיב** | `/Internal/CreateNewAuthenticationFile` |
| **תיאור** | יוצר תיק אימות יבוא חדש מתוך רשימת בקשות אימות, משייך את הבקשות לתיק ומפעיל אירועים נלווים |

**פרמטרים:**
| שם | סוג | תיאור |
|----|-----|--------|
| `importAuthenticationRequests` | `List<GetImportAuthenticationRequestResultDto>` | רשימת בקשות האימות לשיוך לתיק החדש (`[FromBody]`) |

**ערך מוחזר:** `ImportAuthenticationFileDetailsDto?` — תיק האימות שנוצר, או `null` אם הרשימה ריקה

**לוגיקה עסקית:**

**מקבל:** רשימת בקשות אימות (תוצאת חיפוש) לשיוך לתיק חדש.

**מבצע:**
1. אם הרשימה ריקה או `null` — מחזיר `null` מיידית.
2. בודק ב-DAL האם קיימת כבר בקשה עם תיק אימות משויך מתוך רשימת מזהי המסמכים — אם כן, זורק שגיאת ולידציה (400) המציינת שכבר קיים תיק אימות עבור הבקשה.
3. יוצר אובייקט תיק אימות חדש: סטטוס `WaitingForSendingLetter` (1), מדינת בקשה מהבקשה הראשונה, משתמש יוצר/מעדכן = המשתמש הנוכחי, כתובת דואר וכינוי מנפיק המכתב עם ערכי ברירת מחדל קבועים (שמורים לצורך התאמה למקור), כתובת אימייל מהבקשה הראשונה, שיטת משלוח ותזכורת ראשוניות, ותאריכי יצירה/עדכון להיום.
4. אוסף את רשימת כל מזהי הלקוחות מתוך הבקשות (`CustomerIdList`).
5. עבור כל בקשה ברשימה — מפעיל אירוע `NewDecisionBeforeAssociation` (1515) עם מזהה הבקשה כישות מסוג `ImportAuthenticationRequest` (12384) — פעולה זו סוגרת את משימת `SetDecisionBeforeAssociation` הפתוחה.
6. קובע את `OrganizationUnitId` של התיק לפי היחידה הארגונית של הבקשה הראשונה (שדה חובה — זריקת חריגה אם ריק, בהתאמה למקור).
7. שומר את התיק החדש ב-DB, ומעדכן את כל בקשות האימות ברשימה כך שיצביעו על מזהה התיק החדש.
8. בונה ומפעיל אירוע `NewAuthenticationRequestFile` (1517) עם מזהה התיק, סוג ישות `AuthenticationRequestFile` (12385) ויחידה ארגונית — פעולה זו פותחת משימת `HandleAuthenticationRequestFile`.
9. ממפה את ישות התיק ל-DTO להחזרה, כולל רשימת מזהי הלקוחות ולקוח ברירת המחדל (הלקוח הראשון, או 1 אם ריק).

**מחזיר:** את תיק האימות שנוצר, כולל רשימת הלקוחות המשויכים; `null` אם רשימת הבקשות שהתקבלה ריקה; שגיאת 400 אם כבר קיים תיק אימות לאחת הבקשות.

---

### GetEntityDocuments
| שדה | ערך |
|-----|-----|
| **HTTP** | GET |
| **נתיב** | `/Internal/GetEntityDocuments` |
| **תיאור** | מחזיר רשימת מסמכים זמינים לשיוך לבקשת אימות יבוא, מסוננים לפי סוגי מסמך מוגדרים ולפי מסמכים שכבר משויכים |

**פרמטרים:**
| שם | סוג | תיאור |
|----|-----|--------|
| `importAuthenticationRequest` | `ImportAuthenticationRequestDto` | בקשת האימות שעבורה מחפשים מסמכים (`[FromQuery]`) |

**ערך מוחזר:** `List<DocumentDto>` — רשימת המסמכים הזמינים לשיוך

**לוגיקה עסקית:**

**מקבל:** בקשת אימות יבוא (משתמש בעיקר בשדה `LeadDocumentId` שלה).

**מבצע:**
1. שולף מה-DAL את רשימת מזהי המסמכים שכבר משויכים לבקשות קיימות עבור אותו מסמך מוביל.
2. שולף מ-`IParametersUtil` את רשימת סוגי המסמכים המורשים (`CertificateOfOriginsDocumentsFilter`, מחרוזת מופרדת בפסיקים) וממיר אותה לרשימת מספרים.
3. שולף את כל מסמכי הישות (`IDocumentsProxy.GetDocumentsByEntity`) עבור המסמך המוביל וסוג ישות `ImportDeclaration` (1055), ומסנן החוצה מסמכים שכבר משויכים לבקשה כלשהי.
4. אם לא הוחזרו מסמכים — מחזיר רשימה ריקה.
5. מסנן את המסמכים לפי סוגי המסמך המורשים בלבד.
6. אם קיימים מזהי מסמכים משויכים — מסנן גם החוצה מסמכים שמזהיהם מופיעים ברשימה, וכן מסמכים עם מזהה 0.
7. שולף מה-DAL אילו ממזהי המסמכים שנותרו כבר בשימוש על ידי מסמך מוביל אחר, ומסנן אותם החוצה גם כן.
8. אם לא נותרו מסמכים לאחר הסינון — מחזיר רשימה ריקה.
9. עבור כל מסמך שנותר — בונה `DocumentDto` עם שדה `Notes` מורכב מ"{מזהה} {כותרת} {שם סוג}", והשדה `StringDynamicParams` מקבל את ההערות המקוריות של המסמך.

**מחזיר:** רשימת מסמכים מסוננת הזמינה לשיוך לבקשת האימות; רשימה ריקה אם אין מסמכים תואמים.

---

### GetAuthenticationRequestByFilter
| שדה | ערך |
|-----|-----|
| **HTTP** | GET |
| **נתיב** | `/Internal/GetAuthenticationRequestByFilter` |
| **תיאור** | מחזיר רשימת בקשות אימות יבוא לפי פילטר חיפוש, מועשרת בשמות מדינה, יחידה ארגונית, יבואן וספק |

**פרמטרים:**
| שם | סוג | תיאור |
|----|-----|--------|
| `filter` | `ImportAuthenticationRequestFilterDto` | פילטר חיפוש (`[FromQuery]`) |

**ערך מוחזר:** `List<GetImportAuthenticationRequestResultDto>` — רשימת בקשות תואמות

**לוגיקה עסקית:**

**מקבל:** אובייקט פילטר עם קריטריוני סינון (סוג מסמך העדפה, מדינות, תאריכים, מזהי ישויות שונים).

**מבצע:**
1. קורא ל-DAL (`GetAuthenticationRequestByFilter`) לביצוע החיפוש.
2. שולף את כל המדינות ואת כל היחידות הארגוניות (`ILookupUtil.All`).
3. אוסף את מזהי הלקוחות ומזהי הספקים הרלוונטיים משאילתת התוצאות, ושולף אותם דרך `ICustomerProxy` ו-`IVendorProxy` בהתאמה.
4. עבור כל תוצאה: ממלא את שם מדינת ההנפקה, שם היחידה הארגונית, שם היבואן (מהלקוח) ושם הספק. שדה `LeadDocumentTitle` נותר `null` — אין מיקרו-שירות DealFile מקביל למקור.

**מחזיר:** רשימת בקשות אימות מועשרת בשמות תצוגה.

---

### GetCertificateOfOriginById
| שדה | ערך |
|-----|-----|
| **HTTP** | GET |
| **נתיב** | `/Internal/GetCertificateOfOriginById` |
| **תיאור** | מחזיר תעודת מקור מלאה (כולל כל האובייקטים המקוננים ואבני הדרך) לפי מזהה פנימי |

**פרמטרים:**
| שם | סוג | תיאור |
|----|-----|--------|
| `certificateOfOriginId` | `int` | המזהה הפנימי של תעודת המקור (`[FromQuery]`) |

**ערך מוחזר:** `CertificateOfOriginDto?` — תעודת המקור המלאה, או `null` אם לא נמצאה

**לוגיקה עסקית:**

**מקבל:** מזהה תעודת מקור.

**מבצע:**
1. שולף מה-DAL את התעודה המלאה (`GetCertificateOfOriginById`) כולל כל הרשימות המקוננות; אם לא נמצאה — מחזיר `null`.
2. ממלא את `StakeholdersIds` ברשימה של שני מזהי לקוח: המייצא (`CustomerId`) וסוכן המכס (`CreateCustomerId`).
3. שולף את אבני הדרך של התעודה (`GetCertificateMilestoneRows`) לפי כותרת התעודה, אוסף את מזהי המשתמשים הרלוונטיים ושולף את שמותיהם דרך `IUserProxy.GetUsersByIds`, וממפה כל שורה ל-`CertificateMilestonesDto` עם שם המשתמש המתאים.

**מחזיר:** אובייקט `CertificateOfOriginDto` מלא, כולל Milestones, שגיאות השוואה מול הצהרה, פרטי תעודה וחשבוניות; `null` אם לא נמצאה תעודה עם המזהה שניתן.

---

### IsCertificateOfOriginByExternalIdExist
| שדה | ערך |
|-----|-----|
| **HTTP** | GET |
| **נתיב** | `/Internal/IsCertificateOfOriginByExternalIdExist` |
| **תיאור** | בודק האם קיימת תעודת מקור לפי מזהה חיצוני (מספר תעודה) ומחזיר אותה אם נמצאה |

**פרמטרים:**
| שם | סוג | תיאור |
|----|-----|--------|
| `certificateOfOriginExternalId` | `string` | המזהה החיצוני (מספר) של תעודת המקור לחיפוש (`[FromQuery]`) |

**ערך מוחזר:** `CertificateOfOriginResultDto?` — תעודת המקור אם נמצאה, או `null` אם לא קיימת

**לוגיקה עסקית:**

**מקבל:** מחרוזת מזהה חיצוני.

**מבצע:**
1. בונה `CertificateOfOriginFilterDto` עם `CertificateNumber` = המזהה החיצוני שהתקבל.
2. קורא ל-`GetCertificateOfOriginsByFilter` (ראו תיאור מפורט תחת `GetCertificateOfOriginsByFilter` להלן).
3. מחזיר את הרשומה הראשונה מהרשימה שהתקבלה.

**מחזיר:** הרשומה הראשונה (`CertificateOfOriginResultDto`) אם קיימת תעודת מקור התואמת למספר, או `null` אם לא נמצאה.

---

### GetCertificateOfOriginsByFilter
| שדה | ערך |
|-----|-----|
| **HTTP** | GET |
| **נתיב** | `/Internal/GetCertificateOfOriginsByFilter` |
| **תיאור** | מחזיר רשימת תעודות מקור לפי פילטר חיפוש, מועשרת בשמות ומספרי זיהוי חיצוניים של המייצא וסוכן המכס |

**פרמטרים:**
| שם | סוג | תיאור |
|----|-----|--------|
| `filter` | `CertificateOfOriginFilterDto` | אובייקט פילטר עם קריטריוני החיפוש (`[FromQuery]`) |

**ערך מוחזר:** `List<CertificateOfOriginResultDto>` — רשימת תעודות מקור תואמות

**לוגיקה עסקית:**

**מקבל:** אובייקט פילטר (מספר תעודה, סטטוס, סוג, סוכן מכס, בית מכס, מדינת יעד, טווחי תאריכים, סיבת בקשה, מספר גרסה ועוד).

**מבצע:**
1. קורא ל-DAL (`GetCertificateOfOriginsByFilter`) לביצוע החיפוש.
2. אוסף את כל מזהי הלקוחות (מייצאים + סוכני מכס) מתוך התוצאות, ושולף אותם דרך `ICustomerProxy.GetCustomersByIds`.
3. אם לא הוחזרו לקוחות — מחזיר את התוצאות כפי שהן ללא העשרה.
4. עבור כל תוצאה: ממלא את `ExporterTitle`/`ExporterExternalIdNum` לפי המייצא, ואת `CustomesAgentTitle`/`CustomesAgentExternalIdNum` לפי סוכן המכס.

**מחזיר:** רשימת תעודות מקור מועשרת בשמות ומספרי זיהוי חיצוניים.

---

## 3. מודלי נתונים

### ImportAuthenticationRequestDto
| שדה | סוג | חובה | תיאור |
|-----|-----|------|--------|
| `DocumentId` | `int` | ✓ | מזהה המסמך של בקשת האימות |
| `CreateDate` | `DateTimeOffset` | ✓ | תאריך יצירה |
| `CreateUserId` | `int` | ✓ | מזהה משתמש יוצר |
| `UpdateDate` | `DateTimeOffset` | ✓ | תאריך עדכון אחרון |
| `UpdateUserId` | `int` | ✓ | מזהה משתמש מעדכן |
| `AuthenticationFileId` | `int?` | — | מזהה תיק האימות |
| `AuthenticationRequestDate` | `DateTimeOffset` | ✓ | תאריך בקשת האימות |
| `CirumstanceDetails` | `string?` | — | פרטי נסיבות |
| `CollateralId` | `int?` | — | מזהה בטחון |
| `DecisionCircumstences` | `string?` | — | נסיבות ההחלטה |
| `DecisionId` | `int?` | — | מזהה ההחלטה |
| `LeadDocumentId` | `int` | ✓ | מזהה המסמך המוביל |
| `DocumentIssuingDate` | `DateTimeOffset` | ✓ | תאריך הנפקת המסמך |
| `ImportCountryId` | `int` | ✓ | מזהה מדינת יבוא |
| `IssuingCountryId` | `int` | ✓ | מזהה מדינה מנפיקה |
| `ItemDetailId` | `int` | ✓ | מזהה פרט הפריט |
| `Number` | `int` | ✓ | מספר בקשת האימות |
| `IsOldIndication` | `bool` | ✓ | האם אינדיקציה ישנה |
| `OriginCountryId` | `int` | ✓ | מזהה מדינת מקור |
| `PreferenceDocumentTypeId` | `int` | ✓ | מזהה סוג מסמך העדפה |
| `Remarks` | `string?` | — | הערות |
| `RequestCircumstancesId` | `int` | ✓ | מזהה נסיבות הבקשה |
| `UserResponseId` | `int` | ✓ | מזהה משתמש מגיב |
| `ResponseNameEmail` | `string?` | — | שם ואימייל של הגורם המגיב |
| `ResponsePhoneNum` | `string?` | — | מספר טלפון של הגורם המגיב |
| `OrganizationUnitId` | `int` | ✓ | מזהה יחידה ארגונית |
| `UserId` | `int` | ✓ | מזהה משתמש |
| `VendorId` | `int?` | — | מזהה ספק |
| `VendorName` | `string?` | — | שם ספק |
| `OrganizationUnitTypeId` | `int?` | — | מזהה סוג יחידה ארגונית |
| `DocumentNumber` | `string?` | — | מספר מסמך |
| `CustomerId` | `int?` | — | מזהה לקוח |
| `ImporterId` | `int?` | — | מזהה יבואן |
| `LastDeliveryForImporter` | `DateTimeOffset?` | — | תאריך משלוח אחרון ליבואן |
| `InvoiceNumber` | `string?` | — | מספר חשבונית |
| `InvoiceGoodsItemTaxDifference` | `decimal?` | — | הפרש מס לפריט סחורה בחשבונית |
| `AllInvoiceGoodsItemTaxDifference` | `decimal?` | — | סך הפרשי מס לכלל פריטי הסחורה |
| `LeadDocumentSubmissionDate` | `DateTimeOffset?` | — | תאריך הגשת המסמך המוביל |
| `IsSendReminderForImporterTaskExists` | `bool` | ✓ | האם קיימת משימת תזכורת פתוחה ליבואן |
| `ItemDetails` | `List<CertificateOfOriginsItemDetailDto>` | ✓ | פרטי הפריטים |
| `Document` | `DocumentDto?` | — | המסמך המצורף עם URL מועשר |
| `Decisions` | `List<CertificateOfOriginsDecisionDto>` | ✓ | רשימת כל ההחלטות האפשריות — מועשר ב-BL |
| `Collaterals` | `List<CollateralRequestDto>` | ✓ | בטחונות הקשורים לבקשה — מועשר ב-BL |
| `IsCurrentUserHandleRequest` | `bool` | ✓ | האם המשתמש הנוכחי מטפל בבקשה |
| `IsCurrentUserHasOpenTask` | `bool` | ✓ | האם למשתמש הנוכחי יש משימה פתוחה |
| `EntityTypeAndIdsToSearch` | `Dictionary<int, List<int>>` | ✓ | מילון ישויות לחיפוש |
| `AdditionalRequestsForSearchInDays` | `int` | ✓ | מספר ימים לחיפוש בקשות נוספות |
| `IsVendorByIssuingCountryId` | `bool` | ✓ | האם מדינת ההנפקה מוגדרת כמדינת ספק |

### ImportAuthenticationFileDetailsDto
| שדה | סוג | חובה | תיאור |
|-----|-----|------|--------|
| `Id` | `int` | ✓ | מזהה תיק האימות |
| `State` | `int` | ✓ | מצב הרשומה |
| `CreateDate` | `DateTimeOffset` | ✓ | תאריך יצירה |
| `CreateUserId` | `int` | ✓ | מזהה משתמש יוצר |
| `UpdateDate` | `DateTimeOffset` | ✓ | תאריך עדכון אחרון |
| `UpdateUserId` | `int` | ✓ | מזהה משתמש מעדכן |
| `AuthenticationFileStatusId` | `int` | ✓ | מזהה סטטוס תיק האימות הנוכחי |
| `Notes` | `string?` | — | הערות |
| `PostalAdress` | `string?` | — | כתובת דואר |
| `DeliveryMethodId` | `int` | ✓ | מזהה שיטת משלוח |
| `EmailAdress` | `string?` | — | כתובת אימייל |
| `ReminderMethodId` | `int` | ✓ | מזהה שיטת תזכורת |
| `RequestCountryId` | `int` | ✓ | מזהה מדינת הבקשה |
| `UserId` | `int` | ✓ | מזהה משתמש |
| `UserNameIssuingLetter` | `string?` | — | שם המשתמש שהנפיק את המכתב |
| `LastDelivery` | `DateTimeOffset?` | — | תאריך משלוח אחרון |
| `ImporterContactingReasonId` | `int?` | — | מזהה סיבת פנייה ליבואן |
| `FirstProvideContactDate` | `DateTimeOffset?` | — | תאריך פנייה ראשונה לאספקת מידע |
| `CustomerIdList` | `List<int>` | ✓ | רשימת מזהי לקוחות משויכים |
| `CustomerId` | `int` | ✓ | מזהה לקוח ראשי |
| `OrganizationUnitId` | `int` | ✓ | מזהה יחידה ארגונית |
| `EntityTypeAndIdsToSearch` | `Dictionary<int, List<int>>` | ✓ | מילון ישויות לחיפוש |
| `IsCurrentUserHandleFile` | `bool` | ✓ | האם המשתמש הנוכחי מטפל בתיק |
| `Requests` | `List<ImportAuthenticationRequestDto>` | ✓ | בקשות האימות הכלולות בתיק |
| `FileStatuses` | `List<CertificateOfOriginsAuthenticationFileStatusDto>` | ✓ | כל סטטוסי תיק האימות האפשריים |

### CustomerDto
| שדה | סוג | חובה | תיאור |
|-----|-----|------|--------|
| `Id` | `int` | ✓ | מזהה פנימי של הלקוח |
| `TypeId` | `int` | ✓ | מזהה סוג לקוח |
| `GenderId` | `int?` | — | מזהה מגדר |
| `IsCorporation` | `bool` | ✓ | האם תאגיד |
| `IsMalkar` | `bool` | ✓ | האם מלכ"ר |
| `IsFromShaam` | `bool` | ✓ | האם ממשה"ם |
| `VatNumber` | `int?` | — | מספר מע"מ |
| `VatOpenDate` | `DateTimeOffset?` | — | תאריך פתיחת תיק מע"מ |
| `VatOpenDateStr` | `string?` | — | תאריך פתיחת מע"מ כטקסט |
| `VatStatusId` | `int?` | — | מזהה סטטוס מע"מ |
| `VatStatusDate` | `DateTimeOffset?` | — | תאריך סטטוס מע"מ |
| `IsValidVatNumber` | `bool` | ✓ | האם מספר מע"מ תקין |
| `CustomerStatusByRashamId` | `int?` | — | סטטוס לקוח לפי רשם |
| `VatReportingGroupId` | `int?` | — | קבוצת דיווח מע"מ |
| `DateOfImmigration` | `DateTimeOffset?` | — | תאריך עלייה |
| `ImmigrationDate` | `string?` | — | תאריך עלייה כטקסט |
| `Activities` | `string?` | — | פעילויות |
| `Indications` | `string?` | — | אינדיקציות |
| `ExternalIdNum` | `string?` | — | מספר זיהוי חיצוני |
| `Address` | `string?` | — | כתובת |
| `Name` | `string?` | — | שם |
| `BirthDate` | `DateTimeOffset?` | — | תאריך לידה |
| `BorderCrossingEndDate` | `DateTimeOffset?` | — | תאריך סיום חציית גבול |
| `BorderCrossingStartDate` | `DateTimeOffset?` | — | תאריך תחילת חציית גבול |
| `CountryId` | `int?` | — | מזהה מדינה |
| `CustomerTypeSpecificId` | `int?` | — | מזהה סוג לקוח ספציפי |
| `CustomerTypeSpecificName` | `string?` | — | שם סוג לקוח ספציפי |
| `CustomerTypeGeneralName` | `string?` | — | שם סוג לקוח כללי |
| `IsActive` | `bool` | ✓ | האם פעיל |
| `IsActiveInVat` | `bool` | ✓ | האם פעיל במע"מ |
| `LocalFirstName` | `string?` | — | שם פרטי בעברית |
| `LocalLastName` | `string?` | — | שם משפחה בעברית |
| `EnglishFirstName` | `string?` | — | שם פרטי באנגלית |
| `EnglishLastName` | `string?` | — | שם משפחה באנגלית |
| `ShaamFinancialInsitituteInMalkarId` | `int?` | — | מזהה מוסד פיננסי במלכ"ר |
| `TaxDeductionCustomerTypeId` | `int?` | — | מזהה סוג לקוח לניכוי מס |
| `LastShaamDetailsBeforeIrrevesibleActionUpdate` | `DateTimeOffset?` | — | תאריך עדכון פרטי משה"ם לפני פעולה בלתי הפיכה |
| `CanImportExportCommercial` | `bool` | ✓ | האם מורשה לייבוא/ייצוא מסחרי |
| `IsPalestinian` | `bool` | ✓ | האם פלסטיני |
| `AuthoritiyId` | `int?` | — | מזהה רשות |
| `IsMinor` | `bool` | ✓ | האם קטין |
| `EconomicBranchVat` | `string?` | — | ענף כלכלי למע"מ |
| `CustomerTypeGeneralId` | `int?` | — | מזהה סוג לקוח כללי |
| `Addresses` | `List<CustomerAddressDto>` | ✓ | כתובות הלקוח |
| `PassportParams` | `PassportParamsDto?` | — | פרמטרי דרכון |
| `AllPassportParams` | `List<PassportParamsDto>` | ✓ | כל פרמטרי הדרכון |
| `AddressByPurposeType` | `AddressDto?` | — | כתובת לפי סוג מטרה |
| `ActiveActivityTypes` | `List<CustomerActivityDto>` | ✓ | סוגי פעילות פעילים |

### ExportDocumentAuthenticationRequestDto
| שדה | סוג | חובה | תיאור |
|-----|-----|------|--------|
| `Id` | `int` | ✓ | מזהה הבקשה |
| `TypeId` | `int` | ✓ | מזהה סוג הבקשה |
| `Title` | `string?` | — | כותרת |
| `State` | `int` | ✓ | מצב הרשומה |
| `CreateDate` | `DateTimeOffset` | ✓ | תאריך יצירה |
| `CreateUserId` | `int` | ✓ | מזהה משתמש יוצר |
| `UpdateDate` | `DateTimeOffset` | ✓ | תאריך עדכון אחרון |
| `UpdateUserId` | `int` | ✓ | מזהה משתמש מעדכן |
| `OrganizationUnitId` | `int` | ✓ | מזהה יחידה ארגונית |
| `CustomerId` | `int` | ✓ | מזהה לקוח (בית מכס זר) |
| `AuthenticationDocumentTypeId` | `int` | ✓ | מזהה סוג מסמך האימות |
| `ExporterCustomerId` | `int?` | — | מזהה הלקוח המייצא |
| `StatusId` | `int?` | — | מזהה סטטוס |
| `CountryId` | `int?` | — | מזהה מדינה |
| `CustomsHouseAddress` | `string?` | — | כתובת בית המכס |
| `VendorId` | `int?` | — | מזהה ספק |
| `AuthenticationRequestArrivalDate` | `DateTimeOffset?` | — | תאריך הגעת בקשת האימות |
| `AuthenticationRequestedByName` | `string?` | — | שם מבקש האימות |
| `AuthenticationRequestedByEmail` | `string?` | — | אימייל מבקש האימות |
| `AuthenticationRequestedByPhone` | `string?` | — | טלפון מבקש האימות |
| `AuthenticationRequestNotes` | `string?` | — | הערות בקשת האימות |
| `ExportLeadDocumentId` | `int?` | — | מזהה מסמך ייצוא מוביל |
| `DocumentId` | `int?` | — | מזהה מסמך |
| `MainDocumentTitle` | `string?` | — | כותרת המסמך הראשי |
| `LastDeliveryDate` | `DateTimeOffset?` | — | תאריך משלוח אחרון |
| `DeliveryMethodId` | `int?` | — | מזהה שיטת משלוח |
| `InvoiceNumbers` | `string?` | — | מספרי חשבוניות |
| `DetailedDecision` | `string?` | — | פירוט ההחלטה |
| `ReferenceNumber` | `string?` | — | מספר אסמכתא |
| `CommentForCustomsHouseLetter` | `string?` | — | הערה למכתב בית המכס |
| `TotalDocuments` | `int?` | — | סה"כ מסמכים |
| `TotalInvoices` | `int?` | — | סה"כ חשבוניות |
| `DocumentDate` | `DateTimeOffset?` | — | תאריך המסמך |
| `InvoiceDate` | `DateTimeOffset?` | — | תאריך החשבונית |
| `OriginalStatusId` | `int` | ✓ | סטטוס מקורי — בסיס להשוואה בעת שמירה עתידית |
| `EntityTypeAndIdsToSearch` | `Dictionary<int, List<int>>` | ✓ | מילון ישויות לחיפוש — מפתח `ExportDeclaration` (12414) |
| `ListOfAdditionalDocumentsIds` | `List<int>` | ✓ | מזהי מסמכים נוספים לצירוף |
| `CustomsItemToExportDocumentAuthenticationRequest` | `List<CustomsItemToExportDocumentAuthenticationRequestDto>` | ✓ | פריטי מכס משויכים |
| `ExportDocumentAuthenticationRequestLeadDocument` | `List<ExportDocumentAuthenticationRequestLeadDocumentDto>` | ✓ | מסמכים מובילים משויכים |
| `ExportAuthenticationRequestManufacturingArea` | `List<ExportAuthenticationRequestManufacturingAreaDto>` | ✓ | אזורי ייצור משויכים |

### ExportDocumentAuthenticationRequestSearchFilterDto
| שדה | סוג | חובה | תיאור |
|-----|-----|------|--------|
| `CountryId` | `int?` | — | מזהה מדינה |
| `DocumentTypeId` | `int?` | — | מזהה סוג מסמך |
| `RequestId` | `int?` | — | מזהה בקשה |
| `ForeignCustomsHouseCustomerId` | `int?` | — | מזהה לקוח בית מכס זר |
| `ExportDeclarationId` | `int?` | — | מזהה הצהרת ייצוא (לא בשימוש בסינון בפועל — נשמר לתאימות) |
| `RequestOpenDateFrom` | `DateTime?` | — | תאריך פתיחת בקשה מתאריך |
| `RequestOpenDateTo` | `DateTime?` | — | תאריך פתיחת בקשה עד תאריך |
| `ExportAuthenticationDocumentId` | `int?` | — | מזהה מסמך אימות ייצוא |
| `InvoiceIdNum` | `string?` | — | מספר חשבונית |
| `MainDocumentTitle` | `string?` | — | כותרת המסמך הראשי |
| `ExporterId` | `int?` | — | מזהה מייצא |
| `ExportAuthenticationRequestStatusId` | `int?` | — | מזהה סטטוס בקשה |
| `CreateUserId` | `int?` | — | מזהה משתמש יוצר |

### ExportDocumentAuthenticationRequestSearchResultDto
| שדה | סוג | חובה | תיאור |
|-----|-----|------|--------|
| `RequestId` | `int` | ✓ | מזהה הבקשה |
| `CountryName` | `string?` | — | שם המדינה — מועשר ב-BL |
| `ForeignCustomsHouseName` | `string?` | — | שם בית המכס הזר — מועשר ב-BL |
| `DocumentTypeName` | `string?` | — | שם סוג המסמך |
| `ExportDeclarationTitle` | `string?` | — | כותרת הצהרת הייצוא |
| `RequestIssuerName` | `string?` | — | שם מגיש הבקשה — מועשר ב-BL |
| `RequestStatusName` | `string?` | — | שם סטטוס הבקשה |
| `CustomerId` | `int` | ✓ | מזהה הלקוח (בית מכס זר) |
| `ExportLeadDocumentId` | `int?` | — | מזהה מסמך ייצוא מוביל |
| `CountryId` | `int?` | — | מזהה מדינה — נשא להעשרה |
| `ExporterCustomerId` | `int?` | — | מזהה לקוח מייצא — נשא להעשרה |

### ImportAuthenticationRequestByLeadDocumentDto
| שדה | סוג | חובה | תיאור |
|-----|-----|------|--------|
| `LeadDocumentId` | `int` | ✓ | מזהה המסמך המוביל |
| `DocumentId` | `int` | ✓ | מזהה המסמך |
| `AuthenticationFileId` | `int?` | — | מזהה תיק האימות |
| `PreferenceDocumentTypeId` | `int` | ✓ | מזהה סוג מסמך העדפה |
| `PreferenceDocumentTypeName` | `string?` | — | שם סוג מסמך העדפה |
| `CreateDate` | `DateTimeOffset` | ✓ | תאריך יצירה |
| `AuthenticationFileStatusId` | `int?` | — | מזהה סטטוס תיק האימות |
| `AuthenticationFileStatusName` | `string?` | — | שם סטטוס תיק האימות |
| `DecisionId` | `int` | ✓ | מזהה ההחלטה |
| `DecisionName` | `string?` | — | שם ההחלטה |
| `ImportCountryId` | `int` | ✓ | מזהה מדינת יבוא |
| `ImportCountryName` | `string?` | — | שם מדינת יבוא — מועשר ב-BL |
| `OrganizationUnitId` | `int` | ✓ | מזהה יחידה ארגונית |
| `OrganizationUnitName` | `string?` | — | שם יחידה ארגונית — מועשר ב-BL |
| `CollateralId` | `int?` | — | מזהה בטחון |
| `IsCollateralExists` | `bool` | ✓ | האם קיים בטחון |
| `ImporterId` | `int?` | — | מזהה יבואן — לא מאוכלס במקור |
| `LastDeliveryForImporter` | `DateTimeOffset?` | — | תאריך משלוח אחרון ליבואן — לא מאוכלס במקור |
| `LeadDocumentTitle` | `string?` | — | כותרת המסמך המוביל — לא מאוכלס (חסר מיקרו-שירות DealFile) |

### GetImportAuthenticationRequestResultDto
| שדה | סוג | חובה | תיאור |
|-----|-----|------|--------|
| `DocumentId` | `int?` | — | מזהה המסמך |
| `IssuingCountryId` | `string?` | — | שם מדינה מנפיקה |
| `OrganizationUnitId` | `string?` | — | שם יחידה ארגונית |
| `PreferenceDocumentTypeId` | `string?` | — | שם סוג מסמך העדפה |
| `AuthenticationFileId` | `int?` | — | מזהה תיק האימות |
| `LeadDocumentTitle` | `string?` | — | כותרת המסמך המוביל |
| `CreateDate` | `DateTimeOffset` | ✓ | תאריך יצירה |
| `VendorName` | `string?` | — | שם הספק |
| `IssuingCountryIdNum` | `int?` | — | מזהה מדינה מנפיקה (מספרי) |
| `OrganizationUnitIdNum` | `int?` | — | מזהה יחידה ארגונית (מספרי) |
| `ResponseNameEmail` | `string?` | — | שם ואימייל של הגורם המגיב |
| `LeadDocumentId` | `int?` | — | מזהה מסמך מוביל |
| `CustomerId` | `int?` | — | מזהה הלקוח |
| `VendorId` | `int?` | — | מזהה הספק |
| `DecisionId` | `int?` | — | מזהה ההחלטה |
| `ImporterName` | `string?` | — | שם היבואן |
| `AuthenticationFileStatusId` | `int?` | — | מזהה סטטוס תיק האימות |

### DocumentDto
| שדה | סוג | חובה | תיאור |
|-----|-----|------|--------|
| `Id` | `int` | ✓ | מזהה המסמך |
| `TypeId` | `int` | ✓ | מזהה סוג המסמך |
| `TypeName` | `string?` | — | שם סוג המסמך |
| `Title` | `string?` | — | כותרת |
| `CreateDate` | `DateTimeOffset` | ✓ | תאריך יצירה |
| `Notes` | `string?` | — | הערות |
| `ExternalId` | `string?` | — | מזהה חיצוני |
| `ExternalIdNum` | `string?` | — | מספר מזהה חיצוני |
| `IsIncoming` | `bool?` | — | האם מסמך נכנס |
| `IsRequired` | `bool` | ✓ | האם המסמך נדרש |
| `IsAccepted` | `bool` | ✓ | האם המסמך אושר |
| `StringDynamicParams` | `string?` | — | פרמטרים דינמיים כמחרוזת |
| `OtherRelatedEntities` | `List<EntityDocumentDto>?` | — | ישויות קשורות נוספות |
| `FileUrl` | `string?` | — | שם סוג המסמך — מועשר ב-BL |
| `FileName` | `string?` | — | שם קובץ להעלאה |
| `OrganizationUnitId` | `int?` | — | יחידה ארגונית (מטא-נתוני העלאה) |
| `EntityId` | `int?` | — | מזהה ישות (מטא-נתוני העלאה) |
| `EntityTypeId` | `int?` | — | סוג ישות (מטא-נתוני העלאה) |
| `DocumentAdditionalFieldValues` | `List<DocumentAdditionalFieldValueDto>?` | — | ערכי שדות נוספים למסמך |

### EntityDocumentDto
| שדה | סוג | חובה | תיאור |
|-----|-----|------|--------|
| `EntityId` | `int` | ✓ | מזהה הישות |
| `EntityTypeId` | `int` | ✓ | סוג הישות |

### DocumentAdditionalFieldValueDto
| שדה | סוג | חובה | תיאור |
|-----|-----|------|--------|
| `Value` | `string?` | — | הערך |
| `DocumentAdditionaFieldId` | `int` | ✓ | מזהה השדה הנוסף |

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
| `CreateCustomerId` | `int` | ✓ | מזהה לקוח יוצר (סוכן מכס) |
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
| `QrImage` | `byte[]?` | — | תמונת ה-QR |
| `ApproveUserId` | `int?` | — | מזהה משתמש מאשר |
| `VersionNumber` | `int` | ✓ | מספר גרסה |
| `IsLastVersion` | `bool` | ✓ | האם זוהי הגרסה האחרונה |
| `IsInPublishingProcess` | `bool` | ✓ | האם בתהליך פרסום |
| `CertificateOfOriginTypeCodeName` | `string?` | — | שם קוד סוג תעודת המקור |
| `DocumentId` | `int` | ✓ | מזהה מסמך |
| `IsDeclarationReleased` | `bool?` | — | האם ההצהרה שוחררה |
| `IsCargoExitedOfCustomsRegulation` | `bool?` | — | האם המטען יצא מרגולציית המכס |
| `IsDeclarationReleasedAndNotRetrospectiveCertificate` | `bool?` | — | האם ההצהרה שוחררה ואינה תעודה רטרואקטיבית |
| `StakeholdersIds` | `List<int>` | ✓ | מזהי בעלי עניין — מועשר ב-BL |
| `Milestones` | `List<CertificateMilestonesDto>` | ✓ | אבני הדרך של התעודה — מועשר ב-BL |
| `CertificateOfOriginVsDeclarationError` | `List<CertificateOfOriginVsDeclarationErrorDto>` | ✓ | שגיאות בהשוואת תעודה-להצהרה |
| `CertificateOfOriginDetails` | `List<CertificateOfOriginDetailsDto>` | ✓ | פרטי תעודת המקור |
| `CertificateOfOriginInvoiceDetail` | `List<CertificateOfOriginInvoiceDetailDto>` | ✓ | פרטי חשבוניות התעודה |

### CertificateMilestonesDto
| שדה | סוג | חובה | תיאור |
|-----|-----|------|--------|
| `VersionNumber` | `int` | ✓ | מספר גרסה |
| `ActionName` | `string?` | — | שם הפעולה |
| `CreateDate` | `DateTimeOffset` | ✓ | תאריך יצירה |
| `RejectReason` | `string?` | — | סיבת דחייה |
| `UserName` | `string?` | — | שם המשתמש — מועשר ב-BL דרך `IUserProxy` |

### CertificateOfOriginResultDto
| שדה | סוג | חובה | תיאור |
|-----|-----|------|--------|
| `Id` | `int` | ✓ | מזהה ייחודי של תעודת המקור |
| `CertificateNumber` | `string?` | — | מספר תעודת המקור |
| `Name` | `string?` | — | שם תעודת המקור |
| `CustomesAgentId` | `int` | ✓ | מזהה סוכן המכס |
| `CustomesAgentTitle` | `string?` | — | שם סוכן המכס — מועשר ב-BL |
| `CustomesAgentExternalIdNum` | `string?` | — | מספר זיהוי חיצוני של סוכן המכס — מועשר ב-BL |
| `ExporterId` | `int` | ✓ | מזהה המייצא |
| `ExporterTitle` | `string?` | — | שם המייצא — מועשר ב-BL |
| `ExporterExternalIdNum` | `string?` | — | מספר זיהוי חיצוני של המייצא — מועשר ב-BL |
| `ExportDeclarationNumber` | `string?` | — | מספר הצהרת הייצוא |
| `VersionNumber` | `int` | ✓ | מספר גרסה |
| `OrganizationUnitId` | `int` | ✓ | מזהה יחידה ארגונית |
| `RequestReasonCode` | `int` | ✓ | קוד סיבת הבקשה |
| `IssuingDate` | `DateTime?` | — | תאריך הנפקה |
| `LeadDocumentId` | `int?` | — | מזהה מסמך מוביל |

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
| `FromIssuingDate` | `DateTime?` | — | תאריך הנפקה מתאריך |
| `ToIssuingDate` | `DateTime?` | — | תאריך הנפקה עד תאריך |
| `FromRequestDate` | `DateTime?` | — | תאריך בקשה מתאריך |
| `ToRequestDate` | `DateTime?` | — | תאריך בקשה עד תאריך |
| `RequestReasonId` | `int?` | — | מזהה סיבת הבקשה |
| `VersionNumber` | `int?` | — | מספר גרסה |
| `IsLastVersion` | `bool?` | — | האם זוהי הגרסה האחרונה |

---

## 4. תלויות חיצוניות
| רכיב | תיאור שימוש |
|-------|-------------|
| `ICertificateOfOriginDal` | שכבת גישת הנתונים המשותפת — מבצעת את כל קריאות ה-DB |
| `AuthenticationRequestBl` | מחלקת לוגיקה עסקית נפרדת לניהול בקשות ותיקי אימות יבוא — נפתרת דרך `IServiceProvider` ברוב נקודות הקצה של הבקרה (למעט `GetAuthenticationRequestByLeadDocumentIDs`, `GetCertificateOfOriginById`, `IsCertificateOfOriginByExternalIdExist`, `GetCertificateOfOriginsByFilter`, שנקראות ישירות על `CertificateOfOriginBl`) |
| `ExportDocumentAuthenticationRequestBl` | מחלקת לוגיקה עסקית לשליפת פרטי לקוחות ובקשות אימות מסמכי ייצוא — נפתרת דרך `IServiceProvider` |
| `ICustomerProxy` | Proxy לשירות לקוחות — שליפת לקוחות לפי זיהוי, מדינה+סוג פעילות, או רשימת מזהים |
| `IVendorProxy` | Proxy לשירות ספקים — שליפת שמות ספקים לפי רשימת מזהים |
| `IUserProxy` | Proxy לשירות משתמשים — שליפת שמות משתמשים עבור אבני הדרך של תעודת מקור |
| `IDocumentsProxy` | Proxy לשירות מסמכים — שליפת מסמכים לפי מזהים/ישות |
| `ICollateralProxy` | Proxy לשירות בטחונות — שליפת בטחונות הקשורים לבקשות/תיקי אימות |
| `ITasksProxy` | Proxy לשירות משימות — בדיקת קיום משימות פתוחות מסוגים שונים |
| `ILookupUtil` | שירות lookup — שליפת מדינות, יחידות ארגוניות |
| `IParametersUtil` | שירות פרמטרי תצורה — שליפת `AdditionalRequestsForSearchInDays`, `CertificateOfOriginsDocumentsFilter` |
| `IUserUtil` | שירות זיהוי משתמש נוכחי — לחישוב דגלי טיפול/משימה פתוחה |
| `IEventUtil` | בונה ומפעיל אירועים (פתיחה/סגירה של משימות) בזרימות המשלוח, התזכורת ויצירת תיק האימות |

---

## 5. הערות
- ב-`GetAuthenticationRequestFileByID`: השדות `LeadDocumentSubmissionDate` ו-`IsSendReminderForImporterTaskExists` שהיו קיימים בפרוצדורה המקורית לא הומרו — אין מיקרו-שירות DealFile מקביל בשירות היעד (ראו `MIGRATION-NOT-DONE.md`).
- ב-`GetAuthenticationRequestByID`: השדה `LeadDocumentSubmissionDate` (מקורו ב-`CRP.DealFile_LeadDocumentSubmissionData`) לא הומר מאותה סיבה.
- ב-`GetAuthenticationRequestByLeadDocumentIDs` וב-`GetAuthenticationRequestByFilter`: השדה `LeadDocumentTitle` נותר `null` — מקורו בטבלת `CRP.DealFile_LeadDocument` שאין לה מיקרו-שירות מקביל.
- שדות `PostalAdress` ו-`UserNameIssuingLetter` ב-`CreateNewAuthenticationFile` מאותחלים לערכי placeholder קבועים ("gg", "ss") בהתאמה להתנהגות המקורית ב-WCF.
- פעולות שמירה מרכזיות (`SaveCertificateOfOrigin`, `SaveImportAuthenticationRequest`, `SaveAuthenticationRequestFile`, `SaveExportDocumentAuthenticationRequest`) וכן מספר אופרציות Incoming טרם הומרו — ראו פירוט מלא של הסיבות (תשתית הודעות/תבניות חסרה, הכרעות עיצוב תלויות client-tracking) ב-`MIGRATION-NOT-DONE.md` בשורש הריפו.
