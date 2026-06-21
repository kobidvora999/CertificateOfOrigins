# אפיון: CertificateOfOrigins — Internal API

> **תאריך:** 18/06/2026 (עודכן: 20/06/2026 — נוסף CheckIfExistsAdditionalRequestsForVendor)
> **Controller:** `CertificateOfOriginInternalController` (`/Internal`)

---

## 1. תיאור כללי
ה-controller חושף נקודות קצה פנימיות לניהול ושאילתת תעודות מקור (Certificates of Origin) ובקשות אימות יבוא.
הוא מיועד לצריכה פנים-שירותית על ידי שירותים אחרים במערכת CustomsCloud.
הדומיין עוסק בתעודות מקור המשויכות להצהרות יצוא, מייצאים, וסוכני מכס, כמו גם בשליפת פרטי לקוחות ובתי מכס זרים לצורך עיבוד מסמכי ייצוא.

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

**ערך מוחזר:** `CustomerDto?` — פרטי הלקוח המלאים, או שגיאת 404 אם הלקוח לא נמצא.

**לוגיקה עסקית:**

**מקבל:** מספר זיהוי לקוח כמספר שלם.

**מבצע:**
1. מאתר את `ExportDocumentAuthenticationRequestBl` מה-`IServiceProvider`.
2. קורא ל-`ICustomerProxy.GetCustomerByIdentification` עם מספר הזיהוי שהתקבל.
3. אם הלקוח לא נמצא (ערך null מוחזר) — זורק `RestValidationException` עם קוד 404 וההודעה "Invalid identification number".

**מחזיר:** `CustomerDto?` עם כל פרטי הלקוח אם נמצא; שגיאת 404 אם לא נמצא לקוח עם מספר הזיהוי הנתון.

---

### GetCustomerInformationByCountry
| שדה | ערך |
|-----|-----|
| **HTTP** | GET |
| **נתיב** | `/Internal/GetCustomerInformationByCountry` |
| **תיאור** | מחזיר פרטי בית מכס זר (לקוח מסוג `Foreign_customs_house`) עבור מדינה מסוימת |

**פרמטרים:**
| שם | סוג | תיאור |
|----|-----|--------|
| `countryId` | `int` | מזהה המדינה לחיפוש בית מכס זר (`[FromQuery]`) |

**ערך מוחזר:** `CustomerDto?` — פרטי בית המכס הזר הראשון הרשום עבור המדינה, או שגיאת 404 אם לא הוגדר בית מכס למדינה זו.

**לוגיקה עסקית:**

**מקבל:** מזהה מדינה כמספר שלם.

**מבצע:**
1. מאתר את `ExportDocumentAuthenticationRequestBl` מה-`IServiceProvider`.
2. מגדיר קבוע `foreignCustomsHouseActivityType = 40` המתאים לסוג פעילות `ECustomerActivityType.Foreign_customs_house` (מ-`MalamTeam.Infrastructure.GeneralServices.Environment.Enums`).
3. קורא ל-`ICustomerProxy.GetCustomersByCountryId` עם מזהה המדינה וסוג הפעילות 40.
4. אם לא הוחזרו לקוחות (רשימה ריקה או null) — זורק `RestValidationException` עם קוד 404 וההודעה "לא הוגדר בית מכס למדינה זו. יש להגדיר כתובת מתאימה".
5. מחזיר את הרשומה הראשונה מהרשימה.

**מחזיר:** `CustomerDto?` — הרשומה הראשונה מרשימת הלקוחות שהוחזרה; שגיאת 404 אם אין לקוחות הרשומים כבית מכס זר עבור אותה מדינה.

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

**ערך מוחזר:** `ImportAuthenticationRequestDto?` — האגרגט המלא של בקשת האימות, או `null` אם לא נמצא מסמך עם המזהה שניתן

**לוגיקה עסקית:**

**מקבל:** מספר שלם `documentId` המזהה את בקשת האימות המבוקשת

**מבצע:**
1. מאתר את `AuthenticationRequestBl` מה-`IServiceProvider`.
2. בונה `DynamicParameters` עם הפרמטר `@DocumentID` מסוג `Int32` וקורא ל-DAL (`DataLayer.GetAuthenticationRequestById`) — הפרוצדורה מחזירה מספר result-sets: result-set 1 הוא כותרת הבקשה, result-set 2 הם פרטי הפריטים (`ItemDetails`), ו-result-set 3 הוא המסמך המצורף (`Document`).
3. אם ה-DAL מחזיר `null` (מסמך לא קיים) — מחזיר `null` מיידית.
4. מבצע העשרת URL למסמך (`FillDocumentFileUrl`): אם `Document` אינו `null`, מבצע lookup של `DocumentType` לפי `Document.TypeId` באמצעות `ILookupUtil`, וממלא את `Document.FileUrl` בשם הסוג שהתקבל.
5. מבצע שליפת מידע נוסף (`GetAdditionalInfoForRequest`):
   - שולף את רשימת כל ההחלטות האפשריות (`Decisions`) מה-DAL באמצעות `DataLayer.GetCertificateOfOriginsDecisions`.
   - שולף בטחונות (Collaterals) מ-`ICollateralProxy.GetCollateralRequest` עבור `EEntityType.ImportAuthenticationRequest` (ערך 12384) ומזהה המסמך — אם הוחזרו בטחונות, ממלא את `request.Collaterals`.
   - שולף פרטי משימות מ-`ITasksProxy.IsTaskExist` עבור שלושה סוגי משימה: `ETaskType.SetDecisionBeforeAssociation` (406), `ETaskType.SendReminderForImporter` (404), ו-`ETaskType.HandleRejectedAuthenticationRequest` (407), עבור אותה ישות.
   - פותר `IUserUtil` דרך `Resolve<IUserUtil>()` ומשיג את מזהה המשתמש הנוכחי מ-`RequestMetadata`.
   - קובע `IsCurrentUserHandleRequest = true` אם קיימת משימה כלשהי ששייכת למשתמש הנוכחי.
   - קובע `IsCurrentUserHasOpenTask = true` אם קיימת משימה פתוחה (IsTaskInProgress) ששייכת למשתמש הנוכחי.
   - בונה `EntityTypeAndIdsToSearch` — מילון עם ערך יחיד: מפתח 1055 (`EEntityType.ImportDeclaration`) וערך הוא רשימה המכילה את `LeadDocumentId` של הבקשה.
6. שולף מ-`IParametersUtil` את ערך התצורה `AdditionalRequestsForSearchInDays` ומאכלס את `request.AdditionalRequestsForSearchInDays`.
7. בודק ב-DAL (`DataLayer.IsVendorCountry`) האם מדינת ההנפקה (`IssuingCountryId`) מוגדרת כמדינת ספק, וממלא את `request.IsVendorByIssuingCountryId`.

**מחזיר:** אובייקט `ImportAuthenticationRequestDto` מאוכלס במלואו הכולל כותרת הבקשה, רשימת פרטי פריטים (`ItemDetails`), מסמך מצורף עם URL מועשר (`Document`), רשימת החלטות (`Decisions`), בטחונות (`Collaterals`), דגלים הנגזרים ממשימות (`IsCurrentUserHandleRequest`, `IsCurrentUserHasOpenTask`), מילון ישויות לחיפוש (`EntityTypeAndIdsToSearch`), ימי חיפוש נוספים (`AdditionalRequestsForSearchInDays`), ודגל מדינת ספק (`IsVendorByIssuingCountryId`). מחזיר `null` אם לא נמצאה בקשה עם המזהה שניתן.

---

### CheckIfExistsAdditionalRequestsForVendor
| שדה | ערך |
|-----|-----|
| **HTTP** | GET |
| **נתיב** | `/Internal/CheckIfExistsAdditionalRequestsForVendor` |
| **תיאור** | בודק האם קיימות יותר מבקשת אימות ייבוא אחת עבור ספק נתון בשלוש השנים האחרונות |

**פרמטרים:**
| שם | סוג | תיאור |
|----|-----|--------|
| `vendorId` | `int` | מזהה הספק לבדיקה (`[FromQuery]`) |

**ערך מוחזר:** `bool` — `true` אם קיימת יותר מבקשת `ImportAuthenticationRequest` אחת לספק זה בשלוש שנים אחרונות, `false` אחרת

**לוגיקה עסקית:**

**מקבל:** מספר שלם `vendorId` המזהה את הספק.

**מבצע:**
1. מאתר את `AuthenticationRequestBl` מה-`IServiceProvider`.
2. בונה `DynamicParameters` עם הפרמטר `@VendorID` מסוג `Int32`.
3. קורא ל-DAL לביצוע פרוצדורת `usp_CertificateOfOrigins_CheckIfExistsAdditionalRequestsForVendor` המחזירה ערך סקלרי.

**מחזיר:** `true` אם מספר בקשות האימות לספק בטווח של שלוש שנים אחורה גדול מאחד, `false` אחרת.

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

### CustomerDto
| שדה | סוג | חובה | תיאור |
|-----|-----|------|--------|
| `Id` | `int` | ✓ | מזהה פנימי של הלקוח |
| `TypeId` | `int` | ✓ | מזהה סוג לקוח |
| `GenderId` | `int?` | — | מזהה מגדר |
| `IsCorporation` | `bool` | ✓ | האם תאגיד |
| `IsMalkar` | `bool` | ✓ | האם מלכר |
| `IsFromShaam` | `bool` | ✓ | האם מש"מ |
| `VatNumber` | `int?` | — | מספר מע"מ |
| `VatOpenDate` | `DateTimeOffset?` | — | תאריך פתיחת מע"מ |
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
| `ShaamFinancialInsitituteInMalkarId` | `int?` | — | מזהה מוסד פיננסי במלכר |
| `TaxDeductionCustomerTypeId` | `int?` | — | מזהה סוג לקוח לניכוי מס |
| `LastShaamDetailsBeforeIrrevesibleActionUpdate` | `DateTimeOffset?` | — | תאריך עדכון מש"מ לפני פעולה בלתי הפיכה |
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
| `InvoiceNumber` | `string?` | — | מספר חשבונית |
| `InvoiceGoodsItemTaxDifference` | `decimal?` | — | הפרש מס לפריט סחורה בחשבונית |
| `AllInvoiceGoodsItemTaxDifference` | `decimal?` | — | סך הפרשי מס לכלל פריטי הסחורה |
| `LeadDocumentSubmissionDate` | `DateTimeOffset?` | — | תאריך הגשת המסמך המוביל |
| `ItemDetails` | `List<CertificateOfOriginsItemDetailDto>` | ✓ | פרטי הפריטים — result-set 2 מהפרוצדורה |
| `Document` | `DocumentDto?` | — | המסמך המצורף עם URL מועשר — result-set 3 מהפרוצדורה |
| `Decisions` | `List<CertificateOfOriginsDecisionDto>` | ✓ | רשימת כל ההחלטות האפשריות — מועשר ב-BL |
| `Collaterals` | `List<CollateralRequestDto>` | ✓ | בטחונות הקשורים לבקשה — מועשר ב-BL דרך `ICollateralProxy` |
| `IsCurrentUserHandleRequest` | `bool` | ✓ | האם המשתמש הנוכחי מטפל בבקשה — נגזר ממשימות |
| `IsCurrentUserHasOpenTask` | `bool` | ✓ | האם למשתמש הנוכחי יש משימה פתוחה על בקשה זו — נגזר ממשימות |
| `EntityTypeAndIdsToSearch` | `Dictionary<int, List<int>>` | ✓ | מילון ישויות לחיפוש — מפתח: `EEntityType.ImportDeclaration` (1055), ערך: `[LeadDocumentId]` |
| `AdditionalRequestsForSearchInDays` | `int` | ✓ | מספר ימים לחיפוש בקשות נוספות — נשלף מ-`IParametersUtil` |
| `IsVendorByIssuingCountryId` | `bool` | ✓ | האם מדינת ההנפקה מוגדרת כמדינת ספק — נשלף מ-DAL |

---

## 4. תלויות חיצוניות
| רכיב | תיאור שימוש |
|-------|-------------|
| `ICertificateOfOriginDal` | שכבת גישת הנתונים — מבצעת את קריאות ה-DB בפועל |
| `AuthenticationRequestBl` | מחלקת לוגיקה עסקית נפרדת לניהול בקשות אימות יבוא — נפתרת דרך `IServiceProvider` ב-`GetAuthenticationRequestByFilter` וב-`GetAuthenticationRequestByID` |
| `ExportDocumentAuthenticationRequestBl` | מחלקת לוגיקה עסקית לשליפת פרטי לקוחות לצורך עיבוד מסמכי ייצוא — נפתרת דרך `IServiceProvider` ב-`GetCustomerInformation` וב-`GetCustomerInformationByCountry` |
| `ICustomerProxy` | Proxy לשירות לקוחות חיצוני — משמש ב-`GetCustomerInformation` לשליפת לקוח לפי מספר זיהוי, וב-`GetCustomerInformationByCountry` לשליפת לקוחות לפי מדינה וסוג פעילות |
| `ICollateralProxy` | Proxy לשירות בטחונות — משמש ב-`GetAuthenticationRequestByID` לשליפת בטחונות הקשורים לבקשת האימות לפי `EEntityType.ImportAuthenticationRequest` (12384) |
| `ITasksProxy` | Proxy לשירות משימות — משמש ב-`GetAuthenticationRequestByID` לבדיקת קיום משימות פתוחות מסוגים 406, 404 ו-407 עבור בקשת האימות |
| `ILookupUtil` | שירות lookup — משמש ב-`GetAuthenticationRequestByID` לשליפת שם סוג מסמך (`DocumentType`) לצורך העשרת `Document.FileUrl` |
| `IParametersUtil` | שירות פרמטרי תצורה — משמש ב-`GetAuthenticationRequestByID` לשליפת `AdditionalRequestsForSearchInDays` |
| `IUserUtil` | שירות זיהוי משתמש — משמש ב-`GetAuthenticationRequestByID` לשליפת מזהה המשתמש הנוכחי לצורך חישוב דגלי המשימות |

---

## 5. הערות
- אין
