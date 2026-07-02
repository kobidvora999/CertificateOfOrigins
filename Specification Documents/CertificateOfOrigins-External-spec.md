# אפיון: CertificateOfOrigins — External API

> **תאריך:** 03/07/2026
> **Controller:** `CertificateOfOriginExternalController` (`/External`)

---

## 1. תיאור כללי
ה-controller חושף נקודות קצה חיצוניות (עבור צרכנים מחוץ לשירות — למשל מסגרת ה-VirtualEntity/Conversion הכללית של המערכת ותהליכי עיבוד אירועים) סביב הדומיין של תעודות מקור (Certificates of Origin). הוא כולל המרת ישות לצורך אינטגרציה כללית עם מסגרת הישויות הוירטואליות, שליפת מזהי תעודות לפי מספר, שיוך תעודות לפריטי סחורה, שמירת צרופות מסמכי תעודה, וטיפול באירוע משלוח בקשת אימות.
השירות הומר מ-WCF ל-.NET 10 במסגרת מיגרציית מודול מלאה; חלק מהאופרציות (שמירות מרכזיות, הודעות, תבניות) טרם הומרו — מתועדות ב-`MIGRATION-NOT-DONE.md` בשורש הריפו.

---

## 2. נקודות קצה

### Convert
| שדה | ערך |
|-----|-----|
| **HTTP** | POST |
| **נתיב** | `/External/Convert` |
| **תיאור** | ממיר ישות מחוברת (ConnectedEntity) לפי מספר תעודת מקור לישות וירטואלית כללית (VirtualEntity) |

**פרמטרים:**
| שם | סוג | תיאור |
|----|-----|--------|
| `connectedEntity` | `ConnectedEntityDto` | הישות המחוברת — מכילה את מספר תעודת המקור בשדה `EntityIdKey1` (`[FromBody]`) |

**ערך מוחזר:** `VirtualEntityDto` — ישות וירטואלית המייצגת את תעודת המקור

**לוגיקה עסקית:**

**מקבל:** אובייקט `ConnectedEntityDto` שהשדה `EntityIdKey1` שלו מכיל את מספר תעודת המקור לחיפוש.

**מבצע:**
1. בונה `CertificateOfOriginFilterDto` עם `CertificateNumber` = `EntityIdKey1` שהתקבל.
2. קורא ל-`GetCertificateOfOriginsByFilter` (מוגדר ב-`CertificateOfOriginBl` הפנימי) ולוקח את התוצאה הראשונה.
3. אם לא נמצאה תעודה תואמת — זורק שגיאת ולידציה בקוד 404 עם הודעה שהמרת הישות נכשלה כיוון שתעודת המקור אינה קיימת.
4. בונה `VirtualEntityDto` עם `Id` ו-`Title` (שם התעודה) מהתוצאה שנמצאה, `EntityType` קבוע לערך `CertificateOfOrigin` (12319), ו-`CustomerId` = מזהה סוכן המכס של התעודה.

**מחזיר:** ישות וירטואלית עם המזהה, הכותרת, סוג הישות וסוכן המכס; שגיאת 404 אם לא נמצאה תעודת מקור עם המספר שניתן.

---

### GetCertificateOfOriginID
| שדה | ערך |
|-----|-----|
| **HTTP** | GET |
| **נתיב** | `/External/GetCertificateOfOriginID` |
| **תיאור** | מחזיר את המזהה הפנימי העדכני ביותר של תעודת מקור לפי מספר תעודה |

**פרמטרים:**
| שם | סוג | תיאור |
|----|-----|--------|
| `certificateNumber` | `string` | מספר תעודת המקור לחיפוש (`[FromQuery]`) |

**ערך מוחזר:** `int?` — מזהה תעודת המקור אם נמצאה, או `null` אם לא קיימת

**לוגיקה עסקית:**

**מקבל:** מחרוזת מספר תעודה.

**מבצע:**
1. קורא ל-DAL (`GetCertificateOfOriginIdByNumber`) המבצע חיפוש לפי מספר התעודה, וממיין לפי תאריך יצירה יורד כדי להחזיר את הגרסה העדכנית ביותר.

**מחזיר:** מזהה תעודת המקור העדכנית ביותר התואמת למספר שניתן, או `null` אם לא נמצאה.

---

### GetGoodsItemCerificateDTO
| שדה | ערך |
|-----|-----|
| **HTTP** | POST |
| **תיאור בהערת קוד** | prefix חוזה מקורי הוא "Get" אך המטען הוא רשימת DTOs המחייבת קלט מה-body |
| **נתיב** | `/External/GetGoodsItemCerificateDTO` |
| **תיאור** | מעשיר רשימת פריטי סחורה עם מזהה תעודת מקור, לפי מספר תעודה שכבר קיים על כל פריט |

**פרמטרים:**
| שם | סוג | תיאור |
|----|-----|--------|
| `goodsItemCerificateDTOs` | `List<GoodsItemCerificateDto>` | רשימת פריטי סחורה עם מספרי תעודה (`[FromBody]`) |

**ערך מוחזר:** `List<GoodsItemCerificateDto>` — אותה רשימה, מועשרת במזהי תעודה

**לוגיקה עסקית:**

**מקבל:** רשימת פריטי סחורה, כל אחד עם שדה `CertificateNumber` אופציונלי.

**מבצע:**
1. עבור כל פריט ברשימה שיש לו `CertificateNumber` שאינו ריק — קורא ל-DAL (`GetCertificateOfOriginIdByNumber`) לשליפת מזהה תעודת המקור התואם (אותה שאילתה המשמשת גם ב-`GetCertificateOfOriginID`), וממלא את `CertificateOfOriginId` של הפריט.
2. פריטים ללא מספר תעודה נותרים ללא שינוי.

**מחזיר:** את אותה רשימת הפריטים שהתקבלה, כאשר כל פריט עם מספר תעודה מכיל כעת גם את מזהה התעודה התואם (או `null` אם לא נמצאה תעודה למספר שניתן).

---

### SaveCertificateOfOriginAttachments
| שדה | ערך |
|-----|-----|
| **HTTP** | POST |
| **נתיב** | `/External/SaveCertificateOfOriginAttachments` |
| **תיאור** | שומר את מסמכי תעודת המקור המעובדים (טיוטה/סופי) כצרופות בשירות המסמכים, ומחליף צרופות קיימות מאותו סוג |

**פרמטרים:**
| שם | סוג | תיאור |
|----|-----|--------|
| `saveCertificateAttachmentsArgsDto` | `SaveCertificateAttachmentsArgsDto` | פרטי התעודה ורשימת התבניות שעובדו (`[FromBody]`) |

**ערך מוחזר:** `bool` — `true` תמיד עם סיום מוצלח

**לוגיקה עסקית:**

**מקבל:** אובייקט המכיל את רשימת התבניות המעובדות (`CertificatesTemplates` — כל אחת עם `DocumentTypeId`, `Content` ו-`FileName`), מספר התעודה, מזהה התעודה, קוד סיבת הבקשה, מזהה סוג התעודה, ומידע נוסף.

**מבצע:**
1. שולף מה-DAL את שם קוד סוג התעודה (`GetCertificateOfOriginTypeCodeName`) לפי מזהה סוג התעודה.
2. עבור כל תבנית מעובדת ברשימה:
   - קובע האם מדובר בטיוטה (קוד סיבת הבקשה = `Draft` (12), או שדה `AdditionalInfo` שווה ל-"isDraft").
   - בונה `DocumentDto` עם כותרת בפורמט "{שם סוג} - {טיוטה|סופי}", שם קובץ בפורמט "תעודת {שם סוג} מספר {מספר תעודה}.pdf", סוג המסמך מהתבנית, יחידה ארגונית (כרגע קבועה לערך 1 — ראו הערה בסעיף 5), מזהה ישות = מזהה התעודה, וסוג ישות `CertificateOfOrigin` (12319).
   - אם סוג המסמך הוא `ExportCertificateOfOrigin` (329) — מוסיף שדה נוסף למסמך עם מספר התעודה, תחת מזהה שדה 46 (מספר תעודת מקור).
   - שולף את המסמכים הקיימים המשויכים לתעודה (`IDocumentsProxy.GetDocumentsByEntity`) עבור סוג ישות `CertificateOfOrigin` (12319); אם קיימים — מוחק אותם (`IDocumentsProxy.DeleteDocuments`).
   - מעלה את המסמך החדש עם תוכן התבנית (`IDocumentsProxy.UploadDocumentAndSave`).

**מחזיר:** `true` — תמיד, לאחר עיבוד כל התבניות שהתקבלו.

---

### HandleAuthenticationRequestDeliverySent
| שדה | ערך |
|-----|-----|
| **HTTP** | POST |
| **נתיב** | `/External/HandleAuthenticationRequestDeliverySent` |
| **תיאור** | מטפל באירוע משלוח שהתקבל עם ישויות קשורות — מאתר את תיק האימות הקשור ומאמת את קיומו |

**פרמטרים:**
| שם | סוג | תיאור |
|----|-----|--------|
| `raiseEventArgs` | `RaiseEventArgsDto` | ארגומנטים של האירוע שהתקבל, כולל רשימת ישויות קשורות (`[FromBody]`) |

**ערך מוחזר:** `bool` — `true` אם נמצא תיק אימות תקף, `false` אחרת

**לוגיקה עסקית:**

**מקבל:** אובייקט אירוע עם רשימת ישויות קשורות (`RelatedEntities`).

**מבצע:**
1. אם רשימת הישויות הקשורות ריקה או `null` — מחזיר `false` מיידית.
2. מחפש ברשימה ישות יחידה מסוג `AuthenticationRequestFile` (12385) (לפי שדה `EntityType` או `TypeId`); אם לא נמצאה — מחזיר `false`.
3. טוען את תיק האימות המלא לפי מזהה הישות שנמצאה, באמצעות אותה לוגיקה המשמשת ב-`GetAuthenticationRequestFileByID` הפנימי (ראו תיאור מלא ב-`CertificateOfOrigins-Internal-spec.md`).
4. בודק האם התיק שנטען אינו `null`.

**הערה:** בקוד ה-WCF המקורי, עדכון בפועל של התיק (`UpdateFileAfterDelivery`) היה מושבת (בהערה) בנתיב הזה — ההתנהגות הנוכחית משמרת התנהגות זו במדויק (בדיקת קיום בלבד, ללא עדכון).

**מחזיר:** `true` אם נמצאה ישות קשורה מסוג תיק אימות והתיק אכן קיים ב-DB; `false` אם אין ישויות קשורות, לא נמצאה ישות מהסוג הנדרש, או שהתיק לא נמצא.

---

## 3. מודלי נתונים

### ConnectedEntityDto
| שדה | סוג | חובה | תיאור |
|-----|-----|------|--------|
| `EntityIdKey1` | `string?` | — | מפתח מזהה ראשי של הישות המחוברת — במקרה זה מספר תעודת המקור |

### VirtualEntityDto
| שדה | סוג | חובה | תיאור |
|-----|-----|------|--------|
| `Id` | `int` | ✓ | מזהה הישות |
| `Title` | `string?` | — | כותרת הישות |
| `State` | `int` | ✓ | מצב הרשומה |
| `UpdateDate` | `DateTime` | ✓ | תאריך עדכון אחרון |
| `CreateDate` | `DateTime` | ✓ | תאריך יצירה |
| `CreateUserId` | `int` | ✓ | מזהה משתמש יוצר |
| `TypeId` | `int` | ✓ | מזהה סוג הישות |
| `UpdateUserId` | `int` | ✓ | מזהה משתמש מעדכן |
| `OrganizationUnitId` | `int` | ✓ | מזהה יחידה ארגונית |
| `CustomerId` | `int` | ✓ | מזהה לקוח (סוכן מכס) |
| `EntityType` | `int` | ✓ | סוג הישות (EEntityType) — כאן קבוע ל-`CertificateOfOrigin` (12319) |
| `TimeStamp` | `byte[]?` | — | חותמת זמן (concurrency) |
| `RelatedEntities` | `List<VirtualEntityDto>?` | — | ישויות קשורות |
| `IEntityFUpdateCustomerId` | `int?` | — | ממשק — מזהה לקוח מעדכן |
| `ICustomerEntityUpdateCustomerId` | `int?` | — | ממשק — מזהה לקוח מעדכן |
| `IEntitySpecializationSpecializationId` | `int?` | — | ממשק — מזהה התמחות |
| `IOrganizationUnitTypeOrganizationUnitTypeId` | `int?` | — | ממשק — מזהה סוג יחידה ארגונית |
| `IsAddAttachmentAllowed` | `bool?` | — | האם מותרת הוספת צרופה |
| `EaddAttachmentNotAllowedStatus` | `int?` | — | סטטוס חוסם הוספת צרופה |
| `ETaskPriorityCode` | `int?` | — | קוד עדיפות משימה |
| `EntityIdKeys` | `string?` | — | מפתחות זיהוי הישות |
| `Path` | `string?` | — | נתיב הישות |

### GoodsItemCerificateDto
| שדה | סוג | חובה | תיאור |
|-----|-----|------|--------|
| `GoodsItemId` | `int` | ✓ | מזהה פריט הסחורה |
| `CertificateNumber` | `string?` | — | מספר תעודת המקור המשויכת |
| `CertificateOfOriginId` | `int?` | — | מזהה תעודת המקור — מועשר ב-BL |

### SaveCertificateAttachmentsArgsDto
| שדה | סוג | חובה | תיאור |
|-----|-----|------|--------|
| `CertificatesTemplates` | `List<TemplateResultDto>` | ✓ | רשימת התבניות המעובדות לשמירה כצרופות |
| `CertificateNumber` | `string?` | — | מספר תעודת המקור |
| `CertificateId` | `int` | ✓ | מזהה תעודת המקור |
| `CertificateRequestReasonCode` | `int` | ✓ | קוד סיבת הבקשה (משמש לזיהוי טיוטה) |
| `CertificateTypeId` | `int` | ✓ | מזהה סוג תעודת המקור |
| `AdditionalInfo` | `string?` | — | מידע נוסף (משמש גם לזיהוי טיוטה כאשר שווה ל-"isDraft") |

### TemplateResultDto
| שדה | סוג | חובה | תיאור |
|-----|-----|------|--------|
| `DocumentTypeId` | `int` | ✓ | מזהה סוג המסמך שאליו משויכת התבנית |
| `Content` | `byte[]?` | — | תוכן המסמך המעובד (בייטים) |
| `FileName` | `string?` | — | שם הקובץ המקורי של התבנית |

### RaiseEventArgsDto
| שדה | סוג | חובה | תיאור |
|-----|-----|------|--------|
| `RelatedEntities` | `List<VirtualEntityDto>?` | — | רשימת ישויות קשורות לאירוע שהתקבל |

---

## 4. תלויות חיצוניות
| רכיב | תיאור שימוש |
|-------|-------------|
| `ICertificateOfOriginDal` | שכבת גישת הנתונים — שליפת מזהי תעודה לפי מספר, שם קוד סוג תעודה, וחיפוש תעודות לפי פילטר |
| `CertificateOfOriginBl` (BusinessLayer) | מחלקת הלוגיקה המקושרת ישירות ל-controller זה (base class) — מבצעת את כל הפעולות של `Convert`, `GetCertificateOfOriginID`, `GetGoodsItemCerificateDTO` ו-`SaveCertificateOfOriginAttachments` |
| `AuthenticationRequestBl` | מחלקת לוגיקה נפרדת — נפתרת דרך `IServiceProvider` ב-`HandleAuthenticationRequestDeliverySent` לטעינת תיק האימות הקשור |
| `IDocumentsProxy` | Proxy לשירות מסמכים — שליפת מסמכים קיימים לפי ישות, מחיקתם, והעלאת מסמך חדש עם תוכן ב-`SaveCertificateOfOriginAttachments` |
| `ICollateralProxy`, `ITasksProxy` | תלויות עקיפות דרך `AuthenticationRequestBl.GetAuthenticationRequestFileByID` הנקרא מתוך `HandleAuthenticationRequestDeliverySent` |

---

## 5. הערות
- ב-`SaveCertificateOfOriginAttachments`: שדה `OrganizationUnitId` של המסמך החדש קבוע כרגע לערך `1` — במקור נלקח מ-`UserUtil.Current.OrganizationUnitID`, ומקור הנתון הזה עבור המשתמש הנוכחי טרם נפתר ב-.NET 10 (`IUserUtil` חושף כיום רק מזהה משתמש, לא יחידה ארגונית). TODO חוסם מסומן בקוד.
- ב-`HandleAuthenticationRequestDeliverySent`: עדכון תיק האימות בפועל (`UpdateFileAfterDelivery`) היה מושבת גם בקוד ה-WCF המקורי (בהערה) — הנתיב הנוכחי מבצע בדיקת קיום בלבד, בהתאמה להתנהגות המקורית.
- פעולות נוספות ב-External שטרם הומרו: `UpdateCetrificateOfOrigins` (תלוי בתשתית הודעות/תבניות/DealFile — מפוזר ל-5 זרימות לפי סוג אירוע) ו-`TempSync` (stub מת ב-WCF, לא נדרש מיגרציה). פירוט מלא ב-`MIGRATION-NOT-DONE.md`.
