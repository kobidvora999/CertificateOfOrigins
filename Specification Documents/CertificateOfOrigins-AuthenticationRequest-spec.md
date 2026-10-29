# אפיון: CertificateOfOrigins — AuthenticationRequest API

> **תאריך:** 30/07/2026
> **Controller:** `AuthenticationRequestController` (`/AuthenticationRequest`)

---

## 1. תיאור כללי
ה-controller חושף חיפוש, בדיקות, ועדכוני מצב עבור בקשות אימות יבוא (Import Authentication Request) — התהליך שבו יבואן מבקש אימות מסמך העדפה (Preference Document) מול בית מכס. צרכן: ה-SPA הפנימי (Internal). כולל חיפוש לפי מסנן, שליפה לפי מסמכים מובילים, שליפת המסמכים המצורפים למסמך מוביל (לצורך צירוף לבקשת אימות), בדיקות עסקיות (יבואן ברשימה חסומה, ריבוי בקשות), העלאת אירועי סגירת משימות, ועדכון סטטוס/שיטת משלוח של תיק בקשת האימות בעקבות משלוח לספק/בית מכס או תזכורת (`HandleImportAuthenticationRequestDeliveryAndReminderForVendorSent` — הכותב הראשון בפועל ל-DB בשירות זה) או בעקבות משלוח ליבואן (`HandleImportAuthenticationRequestDeliveryForImporterSent` — משתמש בפעולת עזר משותפת `HandleReminderOrDeliveryRequestSentToImporter` המשלבת כתיבת DB עם העלאת אירוע).

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

### HandleImportAuthenticationRequestDeliveryAndReminderForVendorSent
| שדה | ערך |
|-----|-----|
| **HTTP** | POST |
| **נתיב** | `/AuthenticationRequest/HandleImportAuthenticationRequestDeliveryAndReminderForVendorSent` |
| **תיאור** | עדכון סטטוס ושיטת משלוח של תיק בקשת אימות יבוא בעקבות משלוח לספק/בית מכס או תזכורת (Internal WCF: `HandleImportAuthenticationRequestDeliveryAndReminderForVendorSent`) |

**פרמטרים:**
| שם | סוג | תיאור |
|----|-----|--------|
| `request` | `HandleDeliveryAndReminderForVendorSentRequestDto` (מגוף הבקשה) | מזהה תיק האימות, הסטטוס הנוכחי, שיטת המשלוח הנוכחית, והאם זהו משלוח בפועל (`true`) או תזכורת (`false`) |

**ערך מוחזר:** `HandleDeliveryAndReminderForVendorSentResultDto` — מזהה התיק, הסטטוס החדש ושיטת המשלוח החדשה לאחר הרצת מכונת המצבים

**לוגיקה עסקית:**

**מקבל:** `Id`, `AuthenticationFileStatusId`, `DeliveryMethodId`, `IsDelivery` — הזרימה הראשונה בשירות זה שכותבת בפועל ל-DB. החלטת מפתח (29/07/2026): נאמנות מלאה ל-WCF המקורי — מכונת המצבים פועלת על הסטטוס ושיטת המשלוח **כפי שנשלחו מהלקוח** (ללא שליפה מה-DB, "trust the client"); אין העלאת אירוע בזרימה זו

**מבצע:**
1. אם `IsDelivery=false` (תזכורת ולא משלוח בפועל) — הסטטוס מוגדר תחילה ל-`AuthenticationRequestReminderWasSend`(3); אם `IsDelivery=true` — הסטטוס נשאר כפי שנשלח מהלקוח (`AuthenticationFileStatusId`)
2. מריץ את מכונת המצבים הישנה (`UpdateFileAfterDelivery`, מועתקת 1:1 מה-WCF) על הסטטוס ושיטת המשלוח שחושבו בצעד 1 (טבלת המעברים המלאה מפורטת בסעיף 5 — הערות):
   - אם הסטטוס `WaitingForSendingLetter`(1) — הסטטוס הופך ל-`AuthenticationRequestWasSend`(2) ושיטת המשלוח הופכת ל-`PostedMailing`(2), ללא תלות בשיטת המשלוח שנשלחה
   - אחרת אם הסטטוס `AuthenticationRequestWasSend`(2) ושיטת המשלוח `PostedMailing`(2) או `SentByEmailRequest`(3) — שיטת המשלוח הופכת ל-`FirstRemindSent`(4) (הסטטוס עצמו נשאר `AuthenticationRequestWasSend`)
   - אחרת אם הסטטוס `AuthenticationRequestWasSend`(2) ושיטת המשלוח `FirstRemindSent`(4) — שיטת המשלוח הופכת ל-`SecondRemindSent`(5) (הסטטוס נשאר `AuthenticationRequestWasSend`)
   - אחרת אם הסטטוס `AuthenticationRequestReminderWasSend`(3) ושיטת המשלוח `FirstRemindSent`(4) — שיטת המשלוח הופכת ל-`SecondRemindSent`(5) (הסטטוס נשאר `AuthenticationRequestReminderWasSend`)
   - בכל שילוב אחר — אין שינוי נוסף מעבר למחושב בצעד 1
3. מעדכן (set-based, `Context.ExecuteUpdateAsync`, ללא טעינת שורה) את שורת תיק האימות (`CRM.CertificateOfOrigins_ImportAuthenticationFileDetails` לפי `Id`): `AuthenticationFileStatusId` ו-`DeliveryMethodId` לערכים שחושבו, וכן `LastDelivery` ו-`UpdateDate` לזמן הנוכחי
4. מעדכן (set-based) את `UpdateDate` של כל בקשות האימות המשויכות (`CRM.CertificateOfOrigins_ImportAuthenticationRequest` שבהן `AuthenticationFileID = Id`)

**מחזיר:** `HandleDeliveryAndReminderForVendorSentResultDto` עם מזהה התיק, הסטטוס החדש ושיטת המשלוח החדשה (לא 404 — כתיבה set-based ללא בדיקת קיום השורה מראש)

---

### HandleImportAuthenticationRequestDeliveryForImporterSent
| שדה | ערך |
|-----|-----|
| **HTTP** | POST |
| **נתיב** | `/AuthenticationRequest/HandleImportAuthenticationRequestDeliveryForImporterSent` |
| **תיאור** | עדכון סטטוס/שיטת משלוח של תיק בקשת אימות יבוא והחלטת הבקשה, בעקבות משלוח ליבואן (Internal WCF: `HandleImportAuthenticationRequestDeliveryForImporterSent`, אותו שם) |

**פרמטרים:**
| שם | סוג | תיאור |
|----|-----|--------|
| `request` | `HandleDeliveryOrReminderForImporterSentRequestDto` (מגוף הבקשה) | מזהה הבקשה, מזהה היחידה הארגונית, מזהה תיק אב (אופציונלי), הסטטוס ושיטת המשלוח הנוכחיים של תיק האב |

**ערך מוחזר:** `HandleDeliveryOrReminderForImporterSentResultDto` — מזהה הבקשה, ההחלטה שהוחתמה, הסטטוס החדש ושיטת המשלוח החדשה של תיק האב

**לוגיקה עסקית:**

**מקבל:** `DocumentId`, `OrganizationUnitId`, `AuthenticationFileId` (אופציונלי — מזהה תיק אימות האב), `AuthenticationFileStatusId` ו-`DeliveryMethodId` (הסטטוס ושיטת המשלוח הנוכחיים של תיק האב, **כפי שנשלחו מהלקוח**). המתודה עצמה היא עטיפה דקה: מאצילה לפעולת עזר משותפת (`HandleReminderOrDeliveryRequestSentToImporter`) עם `EEventType.NewDeliveryForImporterSent`(1511) ו-`EAuthenticationRequestDecision.LetterForImporterWasSent`(8). הערה: מתודת האחות `HandleImportAuthenticationRequestDeliveryReminderForImporterSent` (#24, ראו להלן) משתמשת באותה פעולת עזר עם `NewDeliveryReminderForImporterSent`(1512) ו-`ReminderForImporterWasSent`(9) — ההבדל היחיד בין השתיים הוא זוג הערכים הזה

**מבצע** (בפעולת העזר המשותפת `HandleReminderOrDeliveryRequestSentToImporter`):
1. מחתים (set-based, `Context.ExecuteUpdateAsync`) את שורת הבקשה (`CRM.CertificateOfOrigins_ImportAuthenticationRequest` לפי `DocumentId`): `DecisionID` = ההחלטה שהועברה (כאן `LetterForImporterWasSent`=8), וכן `LastDeliveryForImporter` ו-`UpdateDate` לתאריך היום (DAL: `UpdateRequestDecisionAfterDelivery`)
2. מריץ את מכונת המצבים המשותפת (`AdvanceDeliveryStatus` — אותה מכונה כמו בזרימת הספק ב-`HandleImportAuthenticationRequestDeliveryAndReminderForVendorSent`, טבלת המעברים המלאה בסעיף 5) על `AuthenticationFileStatusId`/`DeliveryMethodId` **כפי שנשלחו מהלקוח**, ללא שליפה מה-DB ("trust the client"); בשונה מזרימת הספק — כאן **אין** קביעה מוקדמת של סטטוס "תזכורת" לפני הרצת המכונה (אין דגל `IsDelivery` בזרימה זו)
3. אם `AuthenticationFileId` קיים (has value) — מעדכן (set-based, DAL: `UpdateFileAfterDelivery`) את שורת תיק האב (`CRM.CertificateOfOrigins_ImportAuthenticationFileDetails`): `AuthenticationFileStatusId`/`DeliveryMethodId` לערכים שחושבו, ו-`LastDelivery`/`UpdateDate` לזמן הנוכחי, וכן את `UpdateDate` של כל בקשות האימות המשויכות לאותו תיק (`AuthenticationFileID = AuthenticationFileId`); אם `AuthenticationFileId` הוא `null` — הצעד הזה נדלג (אין תיק אב לעדכן)
4. בונה ומעלה (`IEventUtil`) אירוע מסוג `NewDeliveryForImporterSent` (event-type id 1511) עבור VirtualEntity מסוג `ImportAuthenticationRequest` (entity-type id 12384), עם `EntityId` = `DocumentId`, `Title` = `DocumentId` (כמחרוזת), ו-`OrganizationUnitId` = `OrganizationUnitId`; אם `AuthenticationFileId` קיים — מוסיף related-entity המצביע על `AuthenticationRequestFile` (entity-type id 12385) באותו מזהה

**מחזיר:** `HandleDeliveryOrReminderForImporterSentResultDto` עם `DocumentId`, `DecisionId` (ההחלטה שהוחתמה), ו-`AuthenticationFileStatusId`/`DeliveryMethodId` החדשים כפי שחושבו ע"י מכונת המצבים (לא 404 — כתיבה set-based ללא בדיקת קיום השורה מראש)

---

### HandleImportAuthenticationRequestDeliveryReminderForImporterSent
| שדה | ערך |
|-----|-----|
| **HTTP** | POST |
| **נתיב** | `/AuthenticationRequest/HandleImportAuthenticationRequestDeliveryReminderForImporterSent` |
| **תיאור** | עדכון סטטוס/שיטת משלוח של תיק בקשת אימות יבוא והחלטת הבקשה, בעקבות **תזכורת** משלוח ליבואן — התאום-תזכורת של `HandleImportAuthenticationRequestDeliveryForImporterSent` (#23) (Internal WCF: `HandleImportAuthenticationRequestDeliveryReminderForImporterSent`, אותו שם) |

**פרמטרים:**
| שם | סוג | תיאור |
|----|-----|--------|
| `request` | `HandleDeliveryOrReminderForImporterSentRequestDto` (מגוף הבקשה) | אותו DTO בדיוק כמו ב-`HandleImportAuthenticationRequestDeliveryForImporterSent` (ראו סעיף 3) |

**ערך מוחזר:** `HandleDeliveryOrReminderForImporterSentResultDto` — אותו DTO בדיוק (ראו סעיף 3)

**לוגיקה עסקית:**

**מקבל:** זהה במלואו ל-`HandleImportAuthenticationRequestDeliveryForImporterSent` (#23) — `DocumentId`, `OrganizationUnitId`, `AuthenticationFileId` (אופציונלי), `AuthenticationFileStatusId` ו-`DeliveryMethodId` (הסטטוס ושיטת המשלוח הנוכחיים של תיק האב, **כפי שנשלחו מהלקוח**). המתודה היא עטיפה דקה זהה במבנה, מאצילה לאותה פעולת עזר משותפת `HandleReminderOrDeliveryRequestSentToImporter`, אך עם זוג ערכים שונה: `EEventType.NewDeliveryReminderForImporterSent`(1512) ו-`EAuthenticationRequestDecision.ReminderForImporterWasSent`(9) — במקום `NewDeliveryForImporterSent`(1511)/`LetterForImporterWasSent`(8) ב-#23. זהו **ההבדל היחיד** בין שתי המתודות; כל שאר ההתנהגות (מכונת המצבים, כתיבות ה-DB, "trust the client") זהה במדויק ל-#23

**מבצע** (באותה פעולת עזר משותפת `HandleReminderOrDeliveryRequestSentToImporter` — ראו פירוט מלא בסעיף `HandleImportAuthenticationRequestDeliveryForImporterSent` לעיל; להלן רק ההבדלים):
1. מחתים (set-based, DAL: `UpdateRequestDecisionAfterDelivery`) את שורת הבקשה: `DecisionID` = `ReminderForImporterWasSent`(9) — במקום 8 ב-#23 — וכן `LastDeliveryForImporter`/`UpdateDate`
2. מריץ את מכונת המצבים המשותפת `AdvanceDeliveryStatus` על `AuthenticationFileStatusId`/`DeliveryMethodId` כפי שנשלחו מהלקוח — זהה ל-#23
3. אם `AuthenticationFileId` קיים — מעדכן (DAL: `UpdateFileAfterDelivery`) את תיק האב ובקשותיו המשויכות — זהה ל-#23
4. בונה ומעלה (`IEventUtil`) אירוע מסוג `NewDeliveryReminderForImporterSent` (event-type id 1512 — במקום 1511 ב-#23) עבור אותו VirtualEntity (`ImportAuthenticationRequest`, entity-type id 12384), עם `EntityId`=`DocumentId`, `Title`=`DocumentId` (כמחרוזת), ו-`OrganizationUnitId`; אם `AuthenticationFileId` קיים — אותו related-entity ל-`AuthenticationRequestFile` (entity-type id 12385) — זהה במבנה ל-#23

**מחזיר:** `HandleDeliveryOrReminderForImporterSentResultDto` עם `DocumentId`, `DecisionId`=9, ו-`AuthenticationFileStatusId`/`DeliveryMethodId` החדשים כפי שחושבו ע"י מכונת המצבים (לא 404 — כתיבה set-based ללא בדיקת קיום השורה מראש)

---

### CreateNewAuthenticationFile
| שדה | ערך |
|-----|-----|
| **HTTP** | POST |
| **נתיב** | `/AuthenticationRequest/CreateNewAuthenticationFile` |
| **תיאור** | יצירת תיק בקשת אימות יבוא חדש מתוך קבוצת בקשות וקישורן אליו (Internal WCF: `CreateNewAuthenticationFile`, אותו שם) |

**פרמטרים:**
| שם | סוג | תיאור |
|----|-----|--------|
| `importAuthenticationRequests` | `List<GetImportAuthenticationRequestResultDto>` (מגוף הבקשה) | קבוצת בקשות האימות שיש לקשר לתיק החדש (אותו DTO המוחזר מ-`AuthenticationRequestByFilter`, ראו סעיף 3 — לא מתועד כפול כאן) |

**ערך מוחזר:** `CreateNewAuthenticationFileResultDto` — תיק בקשת האימות שנוצר

**לוגיקה עסקית:**

**מקבל:** רשימת בקשות אימות יבוא (`GetImportAuthenticationRequestResultDto`); רשימה ריקה/`null` מחזירה `null` (קלט לא תקין — אין כאן סמנטיקת 404)

**מבצע:**
1. אוסף את `DocumentId` מכל הבקשות ברשימה
2. **ולידציה:** בודק מול ה-DAL (`GetFirstRequestAlreadyLinkedToFile`) האם מי מהבקשות כבר משויכת לתיק קיים (`AuthenticationFileId != null`); אם כן — זורק `RestValidationException` (400) עם ההודעה "עבור בקשה {DocumentId} קיים תיק מספר {FileId}" (Legacy `EMessages.FileExistForRequest`, message id 14070)
3. בונה את התיק החדש (`CertificateOfOriginsImportAuthenticationFileDetails`) מהשדות של הבקשה **הראשונה** ברשימה בלבד (trust-client, ללא שליפת DB) — נאמנות מלאה ל-WCF המקורי (החלטת מפתח 30/07/2026): `RequestCountryId` = `IssuingCountryIdNum` של הבקשה הראשונה, `EmailAdress` = `ResponseNameEmail` שלה, `AuthenticationFileStatusId` = `WaitingForSendingLetter`(1) קבוע, `DeliveryMethodId`/`ReminderMethodId` = 1 קבועים, `PostalAdress`/`UserNameIssuingLetter` = placeholder-ים ליטרליים מהמקור ("gg"/"ss" בהתאמה, נשמרו כפי שהם — לא TODO), `UserId`/`CreateUserId`/`UpdateUserId` = `RequestMetadata.UserId`, `CreateDate`/`UpdateDate` = הזמן הנוכחי. השדה הישן הזמני `CustomerIDList` הושמט במכוון (transient/לא בשימוש, החלטת מפתח 2026-07-30)
4. לכל בקשה ברשימה (**לפני** ה-INSERT, נאמנות לסדר ב-WCF המקורי) — בונה ומעלה (`IEventUtil`) אירוע `NewDecisionBeforeAssociation` (event-type id 1515) עבור VirtualEntity מסוג `ImportAuthenticationRequest` (entity-type id 12384), עם `EntityId`=`DocumentId`, `Title`=`DocumentId` (כמחרוזת), `AdditionalInfo`=`DocumentId` (כמחרוזת) — סוגר את משימת `SetDecisionBeforeAssociation` של כל בקשה
5. מכניס (`Context.Add` + `SaveChanges`) את שורת התיק החדש (`CRM.CertificateOfOrigins_ImportAuthenticationFileDetails`) ומקבל את מזהה התיק שנוצר
6. מקשר (set-based, `Context.ExecuteUpdateAsync`) את הבקשות ברשימה לתיק החדש: מעדכן `AuthenticationFileID` רק בשורות שעדיין אינן משויכות לתיק אחר (`AuthenticationFileID == null`) — מחליף את ה-SP+TVP הישן (`usp_CertificateOfOrigins_UpdateImportAuthenticationRequest` + `Shared.IntArray`), החלטת מפתח 2026-07-30
7. בונה ומעלה (`IEventUtil`) אירוע סופי `NewAuthenticationRequestFile` (event-type id 1517) עבור VirtualEntity מסוג `AuthenticationRequestFile` (entity-type id 12385), עם `EntityId`=מזהה התיק החדש, `Title`=מזהה התיק (כמחרוזת), `OrganizationUnitId`=`OrganizationUnitIdNum` של הבקשה הראשונה (שדה transient על הישות הישנה, בשימוש לאירוע זה בלבד), `AdditionalInfo`=מזהה התיק (כמחרוזת) — פותח את משימת `HandleAuthenticationRequestFile`

**מחזיר:** `CreateNewAuthenticationFileResultDto` עם מזהה התיק החדש, הסטטוס, מדינת הבקשה, היחידה הארגונית, שיטת המשלוח/התזכורת, הדוא"ל, ותאריך היצירה; `CustomerId` = `CustomerId` של הבקשה הראשונה (או 1 כברירת מחדל אם `null`) — לא 404 (יצירה, לא שליפה); 400 (`RestValidationException`) אם מי מהבקשות כבר משויכת לתיק אחר

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

### HandleDeliveryAndReminderForVendorSentRequestDto
| שדה | סוג | חובה | תיאור |
|-----|-----|------|--------|
| `Id` | `int` | ✓ | מזהה תיק בקשת האימות |
| `AuthenticationFileStatusId` | `int` | ✓ | סטטוס נוכחי של התיק כפי שנשלח מהלקוח (`EAuthenticationFileStatus`) — קלט למכונת המצבים; לא נשלף מה-DB |
| `DeliveryMethodId` | `int` | ✓ | שיטת משלוח נוכחית כפי שנשלחה מהלקוח (`EDeliveryMethod`) — קלט למכונת המצבים; לא נשלף מה-DB |
| `IsDelivery` | `bool` | ✓ | `true` = משלוח בפועל; `false` = תזכורת (הסטטוס מוגדר תחילה ל-`AuthenticationRequestReminderWasSend` לפני הרצת מכונת המצבים) |

### HandleDeliveryAndReminderForVendorSentResultDto
| שדה | סוג | חובה | תיאור |
|-----|-----|------|--------|
| `Id` | `int` | ✓ | מזהה תיק בקשת האימות |
| `AuthenticationFileStatusId` | `int` | ✓ | הסטטוס החדש לאחר הרצת מכונת המצבים (`EAuthenticationFileStatus`) |
| `DeliveryMethodId` | `int` | ✓ | שיטת המשלוח החדשה לאחר הרצת מכונת המצבים (`EDeliveryMethod`) |

### HandleDeliveryOrReminderForImporterSentRequestDto
| שדה | סוג | חובה | תיאור |
|-----|-----|------|--------|
| `DocumentId` | `int` | ✓ | מזהה בקשת האימות (מפתח השורה המוחתמת) |
| `OrganizationUnitId` | `int` | ✓ | מזהה היחידה הארגונית (מועבר לאירוע) |
| `AuthenticationFileId` | `int?` | — | מזהה תיק אימות האב; כאשר `null` — מכונת המצבים מחושבת אך תיק האב (ובקשותיו) אינם נכתבים |
| `AuthenticationFileStatusId` | `int` | ✓ | סטטוס נוכחי של תיק האב כפי שנשלח מהלקוח (`EAuthenticationFileStatus`) — קלט למכונת המצבים; לא נשלף מה-DB |
| `DeliveryMethodId` | `int` | ✓ | שיטת משלוח נוכחית של תיק האב כפי שנשלחה מהלקוח (`EDeliveryMethod`) — קלט למכונת המצבים; לא נשלפת מה-DB |

### HandleDeliveryOrReminderForImporterSentResultDto
| שדה | סוג | חובה | תיאור |
|-----|-----|------|--------|
| `DocumentId` | `int` | ✓ | מזהה בקשת האימות |
| `DecisionId` | `int` | ✓ | ההחלטה שהוחתמה על הבקשה (`EAuthenticationRequestDecision`) |
| `AuthenticationFileStatusId` | `int` | ✓ | הסטטוס החדש של תיק האב לאחר הרצת מכונת המצבים (`EAuthenticationFileStatus`) |
| `DeliveryMethodId` | `int` | ✓ | שיטת המשלוח החדשה של תיק האב לאחר הרצת מכונת המצבים (`EDeliveryMethod`) |

### CreateNewAuthenticationFileResultDto
| שדה | סוג | חובה | תיאור |
|-----|-----|------|--------|
| `Id` | `int` | ✓ | מזהה התיק שנוצר |
| `AuthenticationFileStatusId` | `int` | ✓ | סטטוס התיק החדש (קבוע: `WaitingForSendingLetter`=1) |
| `OrganizationUnitId` | `int` | ✓ | מזהה היחידה הארגונית — מ-`OrganizationUnitIdNum` של הבקשה הראשונה (שדה transient על הישות הישנה) |
| `RequestCountryId` | `int` | ✓ | מדינת הבקשה — מ-`IssuingCountryIdNum` של הבקשה הראשונה |
| `CustomerId` | `int` | ✓ | מזהה הלקוח — מ-`CustomerId` של הבקשה הראשונה (1 כברירת מחדל אם `null`) |
| `DeliveryMethodId` | `int` | ✓ | שיטת משלוח (קבוע: 1) |
| `ReminderMethodId` | `int` | ✓ | שיטת תזכורת (קבוע: 1) |
| `EmailAdress` | `string?` | — | דוא"ל למענה — מ-`ResponseNameEmail` של הבקשה הראשונה |
| `CreateDate` | `DateTimeOffset` | ✓ | תאריך יצירת התיק |

הערה: הקלט לפעולה זו (`GetImportAuthenticationRequestResultDto`) כבר מתועד לעיל; השדה הישן הזמני `CustomerIDList` הושמט במכוון מ-DTO זה (transient, לא בשימוש).

### EAuthenticationFileStatus (enum)
| ערך | שם | תיאור |
|-----|-----|--------|
| 1 | WaitingForSendingLetter | ממתין למשלוח מכתב |
| 2 | AuthenticationRequestWasSend | בקשת האימות נשלחה |
| 3 | AuthenticationRequestReminderWasSend | תזכורת לבקשת האימות נשלחה |
| 4 | ReceivedPartialAnswerInFile | התקבלה תשובה חלקית בתיק |
| 5 | ReceivedAnswerInFile | התקבלה תשובה בתיק |
| 6 | RightAuthenticationAnswer | תשובת אימות תקינה |
| 7 | ClarificationRequired | נדרש בירור |
| 8 | WrongAuthenticationAnswer | תשובת אימות שגויה |
| 9 | CancelledFile | תיק בוטל |

מקור: `CRM.CertificateOfOrigins_enum_AuthenticationFileStatus` (ערכי enum פלטפורמה — לא הומצאו). רלוונטי ל-`HandleImportAuthenticationRequestDeliveryAndReminderForVendorSent` ול-`HandleImportAuthenticationRequestDeliveryForImporterSent` (אותה מכונת מצבים משותפת, `AdvanceDeliveryStatus`).

### EDeliveryMethod (enum)
| ערך | שם | תיאור |
|-----|-----|--------|
| 1 | WasNotSend | טרם נשלח |
| 2 | PostedMailing | נשלח בדואר |
| 3 | SentByEmailRequest | נשלח בבקשת דוא"ל |
| 4 | FirstRemindSent | תזכורת ראשונה נשלחה |
| 5 | SecondRemindSent | תזכורת שנייה נשלחה |

מקור: `CRM.CertificateOfOrigins_enum_DeliveryMethod` (ערכי enum פלטפורמה — לא הומצאו). רלוונטי ל-`HandleImportAuthenticationRequestDeliveryAndReminderForVendorSent` ול-`HandleImportAuthenticationRequestDeliveryForImporterSent` (אותה מכונת מצבים משותפת, `AdvanceDeliveryStatus`).

### EAuthenticationRequestDecision (enum)
| ערך | שם | תיאור |
|-----|-----|--------|
| 8 | LetterForImporterWasSent | מכתב ליבואן נשלח |
| 9 | ReminderForImporterWasSent | תזכורת ליבואן נשלחה |

מקור: `CRM.CertificateOfOrigins_enum...Decision` (ערכי enum פלטפורמה — קבוצת-משנה נבחרת (curated subset), רק ההחלטות שהשירות קובע; לא הומצאו). רלוונטי ל-`HandleImportAuthenticationRequestDeliveryForImporterSent` (#23, משתמש ב-8) ול-`HandleImportAuthenticationRequestDeliveryReminderForImporterSent` (#24, משתמש ב-9).

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
| `IEventUtil` | העלאת אירוע `CloseAllTaskForImportAuthenticationRequestFile` (event-type id 1525) ב-`ChangeStatusAfterDeliverySent`, אירוע `CloseTaskReminderNotice3Months` (event-type id 1745) ב-`CloseReminderTask`, אירוע `NewDeliveryForImporterSent` (event-type id 1511) ב-`HandleImportAuthenticationRequestDeliveryForImporterSent`, אירוע `NewDeliveryReminderForImporterSent` (event-type id 1512) ב-`HandleImportAuthenticationRequestDeliveryReminderForImporterSent`, אירוע `NewDecisionBeforeAssociation` (event-type id 1515, פר בקשה) ואירוע `NewAuthenticationRequestFile` (event-type id 1517, סופי) ב-`CreateNewAuthenticationFile`; נפתר lazily (`Resolve<IEventUtil>()`), רשום דרך `AddEventUtil()` |

`HandleImportAuthenticationRequestDeliveryAndReminderForVendorSent`: **אין** תלויות חיצוניות — כתיבת DAL טהורה (ללא proxy, ללא lookup, ללא אירוע).

`HandleImportAuthenticationRequestDeliveryForImporterSent`: תלות יחידה — `IEventUtil` (העלאת `NewDeliveryForImporterSent`, בשילוב עם כתיבת DAL set-based; ללא proxy, ללא lookup).

`HandleImportAuthenticationRequestDeliveryReminderForImporterSent`: תלות יחידה, זהה ל-#23 — `IEventUtil` (העלאת `NewDeliveryReminderForImporterSent`, בשילוב עם כתיבת DAL set-based; ללא proxy, ללא lookup).

`CreateNewAuthenticationFile`: תלות יחידה — `IEventUtil` (העלאת `NewDecisionBeforeAssociation` פר-בקשה ו-`NewAuthenticationRequestFile` סופי), בשילוב עם כתיבות DAL (ולידציית שיוך-קיים, INSERT של התיק, ו-`ExecuteUpdateAsync` set-based לקישור הבקשות); ללא proxy, ללא lookup.

---

## 5. הערות
- `LeadDocumentTitle` נשאר `null` בשתי מתודות החיפוש — TODO(migration): דורש proxy לשירות הבעלים של מסמך ה-CRP.DealFile, שטרם קיים
- `EntityDocuments`: הנתיב (route) של ה-endpoint בשירות המסמכים (Documents microservice) טרם אושר מול הצוות האחראי — TODO(blocking) ב-`DocumentsProxy.GetDocumentsByEntity`, ראו הערה בקוד
- `ChangeStatusAfterDeliverySent`: אירוע `CloseAllTaskForImportAuthenticationRequestFile` (event-type id 1525) מועלה עבור VirtualEntity מסוג `AuthenticationRequestFile` (entity-type id 12385). ה-endpoint הוא passthrough בלבד — אינו כותב סטטוס ל-DB; שינוי הסטטוס בפועל וסגירת המשימות מטופלים ב-side של שירות ה-Events (response handler). החלטת מפתח (29/07/2026): לשמור נאמנות מלאה ל-WCF המקורי — event-raise בלבד
- `CloseReminderTask`: שם המתודה ב-BL/endpoint (`CloseReminderTask`) שונה משם המתודה בחוזה ה-WCF המקורי (`HandleSendRemindDeliverNotification`) — נשמר כאן לתיעוד. אירוע `CloseTaskReminderNotice3Months` (event-type id 1745) מועלה עבור VirtualEntity מסוג `AuthenticationRequestFile` (entity-type id 12385), כולל related-entity לאותה ישות. ה-endpoint הוא passthrough בלבד — אינו כותב ל-DB; סגירת משימת התזכורת בפועל מטופלת ב-side של שירות ה-Events (response handler). אין DAL, אין proxy — תלות יחידה היא `IEventUtil`
- `HandleImportAuthenticationRequestDeliveryAndReminderForVendorSent`: הכותב הראשון בפועל לבסיס הנתונים בשירות זה (DAL כתיבה set-based, ללא proxy/lookup/event). ישות חדשה שהוכנסה (onboarded): `CRM.CertificateOfOrigins_ImportAuthenticationFileDetails`; enums חדשים: `EAuthenticationFileStatus`, `EDeliveryMethod` (ראו סעיף 3). החלטת מפתח (29/07/2026): נאמנות מלאה ל-WCF המקורי — מכונת המצבים (`UpdateFileAfterDelivery`, מועתקת 1:1) פועלת על הסטטוס ושיטת המשלוח **כפי שנשלחו מהלקוח**, ללא שליפה מה-DB ("trust the client"); אין העלאת אירוע בזרימה זו. טבלת המעברים המלאה של מכונת המצבים:

  | סטטוס בכניסה למכונה | שיטת משלוח בכניסה למכונה | סטטוס ביציאה | שיטת משלוח ביציאה |
  |---|---|---|---|
  | `WaitingForSendingLetter`(1) | כל ערך | `AuthenticationRequestWasSend`(2) | `PostedMailing`(2) |
  | `AuthenticationRequestWasSend`(2) | `PostedMailing`(2) או `SentByEmailRequest`(3) | ללא שינוי (`AuthenticationRequestWasSend`) | `FirstRemindSent`(4) |
  | `AuthenticationRequestWasSend`(2) | `FirstRemindSent`(4) | ללא שינוי (`AuthenticationRequestWasSend`) | `SecondRemindSent`(5) |
  | `AuthenticationRequestReminderWasSend`(3) | `FirstRemindSent`(4) | ללא שינוי (`AuthenticationRequestReminderWasSend`) | `SecondRemindSent`(5) |
  | כל שילוב אחר | כל ערך | ללא שינוי (כפי שחושב בצעד ה-`IsDelivery`) | ללא שינוי (כפי שנשלח מהלקוח) |

  לפני הרצת המכונה: אם `IsDelivery=false` הסטטוס בכניסה נקבע ל-`AuthenticationRequestReminderWasSend`(3) (במקום הערך שנשלח); אם `IsDelivery=true` הסטטוס בכניסה הוא `AuthenticationFileStatusId` שנשלח כמות שהוא. לאחר הרצת המכונה: `LastDelivery` ו-`UpdateDate` מתעדכנים על תיק האימות (`CRM.CertificateOfOrigins_ImportAuthenticationFileDetails`), ו-`UpdateDate` מתעדכן על כל בקשות האימות המשויכות (`CRM.CertificateOfOrigins_ImportAuthenticationRequest` שבהן `AuthenticationFileID = Id`) — שתי הכתיבות set-based (`ExecuteUpdateAsync`), ללא טעינת שורות
- `HandleImportAuthenticationRequestDeliveryForImporterSent`: הכתיבה השנייה בפועל לבסיס הנתונים בשירות זה, וגם הראשונה המשלבת כתיבת DB עם העלאת אירוע באותה זרימה. המתודה הפומבית היא עטיפה דקה סביב פעולת עזר משותפת פרטית, `HandleReminderOrDeliveryRequestSentToImporter(request, eventTypeId, decisionId)`, שמקורה במתודת ה-WCF המשותפת `HandleReminderOrDeliveryRequestSentToImporter` (לא נחשפה כ-endpoint משל עצמה בחוזה המקורי). היא חולקת עם `HandleImportAuthenticationRequestDeliveryAndReminderForVendorSent` הן את מכונת המצבים `AdvanceDeliveryStatus` והן את ה-DAL `UpdateFileAfterDelivery`; ומוסיפה DAL חדש — `UpdateRequestDecisionAfterDelivery` (מחתים `DecisionID`/`LastDeliveryForImporter`/`UpdateDate` על שורת הבקשה). enums חדשים: `EAuthenticationRequestDecision` (LetterForImporterWasSent=8, ReminderForImporterWasSent=9); `EEventType` התווסף `NewDeliveryForImporterSent`=1511 ו-`NewDeliveryReminderForImporterSent`=1512 (האחרון בשימוש ב-`HandleImportAuthenticationRequestDeliveryReminderForImporterSent`, #24 — ראו הבא); `EEntityType` התווסף `ImportAuthenticationRequest`=12384 (ה-VirtualEntity שעליו מועלה האירוע כאן, להבדיל מ-`AuthenticationRequestFile`=12385 שהוא ה-related-entity של תיק האב). החלטת מפתח (29–30/07/2026): נאמנות מלאה ל-WCF המקורי — מכונת המצבים פועלת על הסטטוס ושיטת המשלוח של תיק האב **כפי שנשלחו מהלקוח** (ללא שליפה מה-DB, "trust the client"), בדיוק כמו בזרימת הספק; **בשונה** מזרימת הספק, זרימה זו **אינה** קובעת מראש סטטוס "תזכורת" לפני הרצת המכונה (אין דגל `IsDelivery` — הקריאה הזו היא תמיד "משלוח", לא תזכורת; תזכורת ליבואן היא `HandleImportAuthenticationRequestDeliveryReminderForImporterSent`, #24, עם אותה פעולת עזר ו-decision/event שונים)
- `HandleImportAuthenticationRequestDeliveryReminderForImporterSent`: התאום-תזכורת של `HandleImportAuthenticationRequestDeliveryForImporterSent` (#23) — ההתנהגות זהה במדויק (אותה מכונת מצבים `AdvanceDeliveryStatus`, אותם DAL `UpdateRequestDecisionAfterDelivery`/`UpdateFileAfterDelivery`, אותה עטיפה דקה סביב פעולת העזר המשותפת `HandleReminderOrDeliveryRequestSentToImporter`, אותה "trust the client" ואותה סמנטיקת "לא 404"); ההבדל היחיד: מעלה אירוע `NewDeliveryReminderForImporterSent` (event-type id 1512, במקום `NewDeliveryForImporterSent`=1511) ומחתים החלטה `ReminderForImporterWasSent` (decision id 9, במקום `LetterForImporterWasSent`=8). לא הוצגו enums/DTOs חדשים — כולם כבר תועדו עבור #23 (ראו סעיף 3). תלות יחידה, זהה ל-#23 — `IEventUtil` בלבד (ללא proxy, ללא lookup)
- `CreateNewAuthenticationFile`: המתודה האחרונה בקונטרולר זה. החלטת מפתח (30/07/2026) — נאמנות מלאה ל-WCF המקורי: התיק החדש נבנה מהשדות של הבקשה **הראשונה** ברשימה בלבד ("trust the client", ללא שליפת ישויות מה-DB, בדיוק כמו בזרימות ה-delivery/reminder לעיל); השדה הישן הזמני `CustomerIDList` הושמט במכוון (transient/לא בשימוש). ה-SP+TVP הישן לקישור הבקשות לתיק (`usp_CertificateOfOrigins_UpdateImportAuthenticationRequest` + `Shared.IntArray`) הומר ל-`Context.ExecuteUpdateAsync` set-based (החלטת מפתח 2026-07-30) — מקשר רק בקשות שעדיין לא משויכות לתיק אחר. ולידציה: `RestValidationException` (400) אם מי מהבקשות כבר משויכת לתיק קיים, עם הודעת ה-legacy `EMessages.FileExistForRequest` (message id 14070): "עבור בקשה {DocumentId} קיים תיק מספר {FileId}". אירועים חדשים: `NewDecisionBeforeAssociation` (event-type id 1515) מועלה פר-בקשה **לפני** ה-INSERT (סדר תואם ל-WCF המקורי), ו-`NewAuthenticationRequestFile` (event-type id 1517) מועלה פעם אחת בסוף עבור VirtualEntity `AuthenticationRequestFile` (entity-type id 12385), עם `OrganizationUnitId` מהבקשה הראשונה. `UserId` (על התיק עצמו, וגם `CreateUserId`/`UpdateUserId`) מגיע מ-`RequestMetadata.UserId`
