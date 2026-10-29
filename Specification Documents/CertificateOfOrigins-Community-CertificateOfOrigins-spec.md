# אפיון: CertificateOfOrigins — Community CertificateOfOrigins API

> **תאריך:** 29/10/2026
> **Controller:** `Community/CertificateOfOriginsController` (`/community/CertificateOfOrigins`)
> **חוזה:** `.spec/OpenApi/certificateOfOriginsCommunity.json` (שורש ה-repo). ה-DTOs נוצרים מהחוזה (`CertificateOfOriginsCommunityModels.cs`).

---

## 1. תיאור כללי
מסר בקשת תעודת המקור מסוכן מכס (Incoming/EAI: `GetPC_MSG2280_2281_CertificateOfOriginRequest`, מסר `PC_NG_2280` ומשוב `PC_NG_2281`) בגרסת REST טהור.

ב-WCF מסר אחד ביצע חמש פעולות, לפי `agentRequest.requestReasonCode`. לפי חלופה 2 (אושרה ע"י המכס ב-28/10/2026) לכל פעולה יש כתובת ו-METHOD משלה, ומספר התעודה עבר מהגוף לכתובת. הלוגיקה העסקית (בדיקות המסר, השמירה, ההתאמה מול הצהרת היצוא) לא השתנתה.

**השולח:** מזהה הסוכן מגיע מ-header ‏`MessageSenderId` שפלטפורמת המסרים מציבה (`RequestMetadata.MessageSenderId`), לא מהגוף.

**שגיאות (במקום תשובה ב-200 עם `exception[]` בגוף):**
| מצב | קוד | גוף |
|-----|-----|-----|
| הבקשה נדחתה ולא נשמר דבר | 400 | ProblemDetails. כל חריגה ישנה היא פריט ב-`errors[]`: `errorCode` = `ExceptionType` הישן (מזהה UIMessage), `field` = שם ההודעה, `detail` = הטקסט, `level` = `ExceptionLevel` |
| התעודה לא קיימת, או שלא השולח יצר אותה | 404 | ProblemDetails |
| התעודה נעולה ע"י בקשה מקבילה | 409 | ProblemDetails (בעבר: `ConcurrencyErrorTryAgain` ב-200) |
| תקלה טכנית | 500 | ProblemDetails (בעבר: `GeneralException` ב-200) |

אזהרות התאמה מול הצהרת היצוא, כשהתעודה **נשמרה**, נשארות בגוף התשובה (`exception[]`).

---

## 2. נקודות קצה

### Create
| שדה | ערך |
|-----|-----|
| **HTTP** | POST |
| **נתיב** | `/community/CertificateOfOrigins` |
| **תיאור** | יצירת תעודת מקור — סיבות בקשה 1 (חדשה), 2 (בדיעבד), 10 (ריקה), 12 (טיוטה), בלי מספר תעודה |

**פרמטרים:** `request` (`CertificateOfOriginRequest`, גוף) — `agentRequest`, `certificateOfOrigin` או `nonManipulationCertificate`

**ערך מוחזר:** 201 + `CertificateOfOriginResult`

**לוגיקה עסקית:**

**מקבל:** גוף בקשה מלא

**מבצע:**
1. סיבת בקשה שאינה 1, 2, 10 או 12 — 400 (`AgentRequest.RequestReasonCode`)
2. ממפה את הגוף למסר הפנימי ומריץ את עיבוד המסר: בדיקת תיקון בהצהרה, בדיקות השדות והבדיקות בין שדות, שמירה, והתאמה מול הצהרת יצוא שהוגשה
3. לא נשמרה תעודה — 400 עם החריגות שנצברו

**מחזיר:** התעודה שנוצרה; `exception[]` = אזהרות התאמה; `attachment[]` = מזהה מסמך התעודה (`externalAttachmentId`) לטיוטה או לתעודה שפורסמה

---

### Status
| שדה | ערך |
|-----|-----|
| **HTTP** | GET |
| **נתיב** | `/community/CertificateOfOrigins/{certificateId}` |
| **תיאור** | שאילתת סטטוס — סיבת בקשה 13 |

**פרמטרים:** `certificateId` (`string`, נתיב) — מספר התעודה

**ערך מוחזר:** 200 + `CertificateOfOriginResult`, בלי צרופה

**לוגיקה עסקית:**

**מקבל:** מספר תעודה

**מבצע:**
1. שולף את התעודה האחרונה עם המספר. לא נמצאה, או שהשולח לא יצר אותה (`CreateCustomerId` ≠ `MessageSenderId`) — 404
2. בונה את המשוב (כולל `queryUrl` מהפרמטר `CertificateOfOriginQueryURL`)

**מחזיר:** מצב התעודה. **שינוי מול legacy (אושר 28/10/2026):** ב-WCF שאילתת סטטוס עברה גם את בדיקות גוף המסר; ב-GET אין גוף והבדיקה ירדה

---

### Update
| שדה | ערך |
|-----|-----|
| **HTTP** | PUT |
| **נתיב** | `/community/CertificateOfOrigins/{certificateId}` |
| **תיאור** | שליחה מחדש של תעודה שעוד לא פורסמה (סיבות 1, 2, 12) או תיקון תעודה (סיבה 3) |

**פרמטרים:** `certificateId` (`string`, נתיב); `request` (`CertificateOfOriginRequest`, גוף) — `requestReasonCode` נשאר בגוף כי הוא מבחין בין שליחה מחדש לתיקון ונשמר על התעודה

**ערך מוחזר:** 200 + `CertificateOfOriginResult`

**לוגיקה עסקית:**

**מקבל:** מספר תעודה וגוף בקשה מלא

**מבצע:**
1. סיבת בקשה שאינה 1, 2, 3 או 12 — 400
2. התעודה לא נמצאה או לא של השולח — 404
3. מריץ את עיבוד המסר עם מספר התעודה מהכתובת, בתוך נעילת התעודה (`IsNeedToLockCertificateOfOrigin`); נעולה — 409
4. לא נשמרה תעודה — 400

**מחזיר:** התעודה אחרי השמירה

---

### Cancel
| שדה | ערך |
|-----|-----|
| **HTTP** | DELETE |
| **נתיב** | `/community/CertificateOfOrigins/{certificateId}` |
| **תיאור** | ביטול תעודה — סיבת בקשה 14 |

**פרמטרים:** `certificateId` (`string`, נתיב)

**ערך מוחזר:** 200 + `CertificateOfOriginResult` (סטטוס מבוטל)

**לוגיקה עסקית:**

**מקבל:** מספר תעודה

**מבצע:**
1. התעודה לא נמצאה או לא של השולח — 404
2. בתוך נעילת התעודה: הצהרה שעדיין מקושרת לתעודה, או משימה פתוחה לעובד מכס — 400 ולא מבטלים
3. אחרת מבטל (סיבת ביטול "בוטל ממסר") ומעלה את אירוע הביטול

**מחזיר:** התעודה המבוטלת

---

### Replacement
| שדה | ערך |
|-----|-----|
| **HTTP** | POST |
| **נתיב** | `/community/CertificateOfOrigins/{certificateIdToCancel}/Replacement` |
| **תיאור** | תעודה חדשה שמחליפה את התעודה שבכתובת, שמבוטלת — סיבות 4 (החלפה), 5 (החלפה ביבוא) |

**פרמטרים:** `certificateIdToCancel` (`string`, נתיב); `request` (`CertificateOfOriginRequest`, גוף, כולל `replacementReason`)

**ערך מוחזר:** 201 + `CertificateOfOriginResult`

**לוגיקה עסקית:**

**מקבל:** מספר התעודה המוחלפת וגוף בקשה מלא

**מבצע:**
1. סיבת בקשה שאינה 4 או 5 — 400
2. התעודה המוחלפת לא נמצאה או לא של השולח — 404
3. מריץ את עיבוד המסר עם מספר התעודה המוחלפת מהכתובת; נעולה — 409; לא נשמר — 400

**מחזיר:** התעודה החדשה

---

## 3. מודלי נתונים
נוצרים מהחוזה (`Api.Models.CertificateOfOriginsCommunity`) ולא נכתבים ידנית. כל שינוי מול V2 מתועד בחוזה ב-`x-legacy`.

### CertificateOfOriginRequest
| שדה | סוג | תיאור |
|-----|-----|--------|
| `agentRequest` | `AgentRequest` | `internalApplication`, `certificateOfOriginTypeCode`, `requestReasonCode`, `replacementReason`, `exportDeclarationNum` (`certificateId` ו-`certificateIdToCancel` עברו לכתובת) |
| `certificateOfOrigin` | `CertificateOfOrigin` | גוף תעודה רגילה, כולל `invoiceDetail[]` / `itemDetail[]` |
| `nonManipulationCertificate` | `NonManipulationCertificate` | גוף תעודת Non Manipulation |

### CertificateOfOriginResult
| שדה | סוג | תיאור |
|-----|-----|--------|
| `applicationId` | `int` | מזהה התעודה |
| `certificateId` | `string` | מספר התעודה |
| `certificateOfOriginTypeCode` / `certificateOfOriginStatusCode` / `requestReasonCode` | `int` | סוג, סטטוס וסיבת הבקשה השמורים על התעודה |
| `internalApplication`, `feedbackRemark`, `rejectCancelReason`, `queryUrl` | `string` | |
| `exception[]` | `MessageException` | אזהרות התאמה; `null` כשאין |
| `attachment[]` | `Attachment` | `externalAttachmentId` של מסמך התעודה ב-Documents; `null` כשאין |

---

## 4. תלויות חיצוניות
| רכיב | תיאור שימוש |
|-------|-------------|
| `ILockUtil` | נעילת התעודה לפי מספרה בזמן העיבוד (פרמטר `IsNeedToLockCertificateOfOrigin`) |
| `IExportDealFileProxy` | פרטי הצהרת היצוא, תיקון בהצהרה, הצהרה מקושרת לפני ביטול |
| `IDocumentUtil` / `IDocumentsProxy` | העלאת מסמך התעודה (עם `ExternalId` שמוחזר כ-`externalAttachmentId`) |
| `ICommonServicesProxy` | הפקת מסמך התעודה (SSRS) |
| `IParametersUtil` | `IsNeedToLockCertificateOfOrigin`, `CertificateOfOriginQueryURL` |

---

## 5. הערות
- **התאמה מדויקת של מספר התעודה (חדש מול legacy, החלטת מפתח 29/10/2026):** המספר בכתובת הוא מזהה המשאב ומאותר בהתאמה מלאה. ב-WCF התעודה אותרה ב-LIKE (`'%' + מספר + '%'`, ממצא C F-23), כך שמספר חלקי יכול היה להגיע לתעודה אחרת — ובביטול גם לבטל אותה.
- **בדיקת בעלות (חדש מול legacy, החלטת מפתח 29/10/2026):** GET, PUT, DELETE ו-Replacement מחזירים 404 לתעודה שלא השולח יצר. ב-WCF התעודה אותרה לפי מספר בלבד.
- **צרופה:** ב-WCF הוחזר תוכן ה-PDF במסר; ב-V2 וב-REST טהור — מזהה בלבד, והתוכן נשלף משירות המסמכים.
- הבדיקות `MustSendCertificateID` / `CertificateIdToCancel` ריק / סיבת בקשה שאינה קיימת אינן נגישות יותר מה-API: המספר הוא חלק מהכתובת, וכל כתובת מקבלת רק את הסיבות שלה (400 עם `RequestReasonNotAllowedForOperation`).
