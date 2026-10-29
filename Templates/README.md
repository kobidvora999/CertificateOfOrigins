# Templates

מקור האמת לתבניות המסמכים של השירות. כל תבנית היא **זוג** קבצים באותו שם:

| קובץ | תפקיד |
|---|---|
| `{Name}.docx` | המסמך עצמו, עם content controls (Developer) שה-`w:tag` שלהם הוא מפתח המיזוג |
| `{Name}.yml` | המיפוי `tag → field path` שמודול התבניות ממזג לפיו |

`{Name}` הוא ה-`Enumeration` של התבנית כפי שהיא רשומה במודול התבניות — **הוא מפתח החיפוש**,
לא תווית. `PrintTemplateDto.Name` נשלח איתו, והמודול טוען את שני הקבצים לפיו; שם שגוי נופל
ב-runtime עם "template not found".

## איך תבנית מחוברת לקוד

```
GET api/CertificateOfOrigins/Template/{templateId}/{entityId}?additionalInfo=&freeText=
  → CertificateOfOriginsBl.GenerateTemplate
      → GetTemplateMeta(templateId)   ← ממפה id → (Result DTO, {Name}, Format, IsFreeTextAllowed)
      → dbo.GetTemplateData           ← @EntityID, @TemplateTypeID, @CreateUserID, @AdditionalInfo (חתימת Object Maker); ענף פר-תבנית
      → EnrichTemplateData            ← נתונים חוצי-שירות (ספק, לקוח, מדינה, בלוק חתימה) מ-proxy/lookup
      → FreeText                      ← הטקסט החופשי של המשתמש, בתבנית שמתירה (רווח בודד כשריק — כמו הלגאסי)
      → ITemplateUtil.GenerateTemplate ← מודול התבניות ממזג ומחזיר את המסמך
```

מזהה התבנית במכתבי הלגאסי הוא **מזהה הלגאסי** (`General_enum_Template.ID`), כך שצרכן שמגיע מהלגאסי שולח את
אותו מספר ששלח תמיד.

תבנית חדשה = ערך ב-`ECertificateOfOriginsTemplate` + `case` ב-`GetTemplateMeta` + Result DTO +
ענף ב-`dbo.GetTemplateData` + זוג הקבצים כאן.

⚠️ שמות ה-property ב-Result DTO עוברים `JsonNamingPolicy.CamelCase`, ולכן ה-`field path` ב-YAML
חייב להיות camelCase של שם ה-property (`FileNo` → `$.fileNo`).

## התבניות שכאן

| קובץ | מזהה | Result DTO | ישות (`@EntityID`) |
|---|---|---|---|
| `ExportAuthenticationSendDeliveryForExporterSouthKoreaTemplate` (CR 194221) | 1 ⚠️ `TODO(confirm)` | `SouthKoreaOriginVerificationLetterResult` | תיק אימות |
| `RequesrForVerificationEurope` | 2251 | `ImportVerificationLetterResult` | תיק אימות |
| `RequesrForVerificationJordan` | 2252 | `ImportVerificationLetterResult` | תיק אימות |
| `RequesrForVerificationMexico` | 2253 | `ImportVerificationLetterResult` | תיק אימות |
| `RequesrForVerificationMercosur` | 2254 | `ImportVerificationLetterResult` | תיק אימות |
| `RequesrForVerificationCanada` | 2255 | `ImportVerificationLetterResult` | תיק אימות |
| `RequesrForVerificationTurkey` | 2256 | `ImportVerificationLetterResult` | תיק אימות |
| `RequesrForVerificationGeneral` | 2257 | `ImportVerificationLetterResult` | תיק אימות |
| `RequesrForVerificationUSATemplate_New` | 2258 | `ImportVerificationLetterResult` | תיק אימות |
| `RequesrForVerificationUSATemplate` | 2259 | `ImportVerificationLetterResult` | תיק אימות |
| `ImportAuthenticationRequestImporterLetter` | 2244 | `ImporterAlertLetterResult` | בקשת אימות (`DocumentID`) |
| `ExportAuthenticationRequestEurope` | 2289 | `ExportVerificationLetterResult` | בקשת אימות יצוא |
| `ExportAuthenticationSendDeliveryForExporterMercosurTemplate` | 2290 | `ExportVerificationLetterResult` | בקשת אימות יצוא |
| `ExportAuthenticationSendDeliveryForExporterTurkeyJordanTemplate` | 2291 | `ExportVerificationLetterResult` | בקשת אימות יצוא |
| `ExportAuthenticationSendDeliveryForExporterEuropeTemplate` | 2293 | `ExportVerificationLetterResult` | בקשת אימות יצוא |
| `ExportAuthenticationRequestMercosur` | 2295 | `ExportVerificationLetterResult` | בקשת אימות יצוא |

ה-docx של מכתבי הלגאסי הם הגרסה האחרונה מה-share (`TemplateBackUp`), שהמפתחת העבירה ב-15.10.2026. ענפי ה-SP
מכסים כבר את כל המשפחה (2251–2259, 2289–2295, 2317) — מכתב שנשאר להמיר צריך רק docx + YAML + `case` ב-`GetTemplateMeta`.

**נשאר להמיר (אין docx):** 2292, 2294, 2317 — שלושתם מכתבי יצוא, אותו Result DTO וענף SP (2294 חתום ע"י המשתמש הקבוע
וכבר מטופל ב-BL). אין להם קריאה מפורשת בקוד הלגאסי — המשתמש בוחר אותם ידנית (`IsAllowManualCreation = 1`).

### מקור כל שדה — הכרעות לפי הלגאסי

- **מחרוזות תאריך** — כמו בלגאסי, ה-SP מחזיר מחרוזת מעוצבת: `CONVERT(..., 107)` ("Oct 15, 2026") לתאריך המכתב
  ולתאריך התגובה (היום + 3 חודשים), `CONVERT(..., 101)` ("10/15/2026") לתאריכי המסמך/החשבונית ביצוא.
- **ספק** (מכתבי היבוא) — מ-Vendors: שם, כתובת אנגלית (`שורה ␠\nעיר,מיקוד`), מדינה באנגלית. ספק בלי כתובת →
  כל בלוק הספק ריק (בלגאסי INNER JOIN לכתובת).
- **תיק עם כמה ספקים** — בלגאסי הודפסה הקבוצה ש"יצאה ראשונה" (`Rows[0]`, סדר לא מוגדר). כאן: מזהה הספק הנמוך.
- **מחלקת המכס הזרה** (יצוא) — מ-Customers, רק אם ללקוח פעילות 40, לפי הכתובת הראשונה. ⚠️ בלגאסי לקוח שאינו
  מחלקת מכס (או בלי כתובת) ביטל את **כל** שורת הנתונים; כאן רק בלוק הנמען ריק.
- **היצואן** — `ExporterCustomer` = `Title`; `ExporterName` = השם האנגלי הפרטי, ואם הוא ריק — שם התאגיד באנגלית.
- **`IsDocument` / `IsInvoice`** — שדות `Bool` שמסירים את הפסקה כשאין מסמך ראשי / חשבוניות.
- **`CustomsWebsite_ForSignature`** (2244) — הפרמטר `CustomsWebsite` (`http://go.gov.il/taxes`, מהקונפיגורציה של הלגאסי), נזרע ב-`API_20261015190000`.

### פתוח — דורש החלטה / מקור

| נושא | מכתבים | מצב |
|---|---|---|
| `REGEON_tbl` (אזורי ייצור, TOP 4) | 2289 | ה-SDT עוטף **שורה** (`w:tr`) — מודול התבניות לא ממזג repeating-row. הושמט מה-YAML ומה-SP; הטבלה תצא ריקה |
| `HebrewCreateDate` / `CreateDate` בכותרת | 2244, 2290, 2291, 2293 | בלגאסי מילא אותם המיזוג הגנרי, לא ה-SP. כרגע ריקים |
| כתובת/טלפון/פקס יחידת החתימה בכותרת התחתונה | 2244 | בלגאסי מטבלאות מערכת (`OrganizationUnitPhoneNumberLOV`) — אין מקור. ריקים |
| `Bool` עם שדות מקוננים | 2291, 2293 | `IsDocument` / `IsInvoice` עוטפים את `DocumentDate` וכו' — `TODO(confirm)` שמודול התבניות ממזג שדות בתוך SDT מסוג Bool |
| `freeText` ב-query string | 2251–2256, 2258, 2259, 2289, 2295 | ה-endpoint הוא GET, ולכן טקסט חופשי ארוך מוגבל באורך ה-URL |
| כותרת המסמך הראשי (`LeadDocumentTitle...`) | 2244 | אין proxy לתיק העסקה ביבוא (אותו פער כמו בקריאות בקשות האימות) |
| `EmployeesSignature` | 2258, 2259 | בגרסת ה-share תג התמונה נקרא `EmployeesSignature`, וה-SP בלגאסי לא החזיר עמודה בשם הזה — לכן יצא ריק. נשמר ריק (פאריטי) |
| המפריד של `ufn_Util_clr_Conc` | 2251–2259 | הונח `', '` |
| endpoints של Users / Vendors / Customers | כולם | `TODO(blocking)` — ה-routes והשדות החדשים (כתובת ספק, שמות אנגליים, פעילות לקוח, טלפון משתמש) ממתינים לחוזה |

### מכתבי לגאסי (18 פעילים — נשלף מ-`Infrastructure.General_enum_Template`, 14.10.2026)

ה-`Enumeration` הוא ה-`{Name}` שיירשם במודול התבניות. כולם `DocumentTypeID = 14`.

| מזהה לגאסי | `Enumeration` | שם | ישות (`Related_EntityTypeID`) | טקסט חופשי |
|---|---|---|---|---|
| 2244 | `ImportAuthenticationRequestImporterLetter` | מכתב התראה ליבואן - בקשת אימות | 12384 בקשת אימות | לא |
| 2251 | `RequesrForVerificationEurope` | מכתב אימות האיחוד האירופי | 12385 תיק אימות | כן |
| 2252 | `RequesrForVerificationJordan` | מכתב אימות ירדן | 12385 | כן |
| 2253 | `RequesrForVerificationMexico` | מכתב אימות מקסיקו | 12385 | כן |
| 2254 | `RequesrForVerificationMercosur` | מכתב אימות מרקוסור | 12385 | כן |
| 2255 | `RequesrForVerificationCanada` | מכתב אימות קנדה | 12385 | כן |
| 2256 | `RequesrForVerificationTurkey` | מכתב אימות תורכיה | 12385 | כן |
| 2257 | `RequesrForVerificationGeneral` | מכתב תזכורת לכל הארצות | 12385 | לא |
| 2258 | `RequesrForVerificationUSATemplate_New` | מכתב אימות ארצות הברית - חדש | 12385 | כן |
| 2259 | `RequesrForVerificationUSATemplate` | מכתב אימות ארצות הברית - רגיל | 12385 | כן |
| 2289 | `ExportAuthenticationRequestEurope` | מאפייני תדפיס אירופה ים תיכוני – תקין ותקין חלקי | 12386 בקשת אימות יצוא | כן |
| 2290 | `ExportAuthenticationSendDeliveryForExporterMercosurTemplate` | מכתב ליצואן - מרקוסור | 12386 | לא |
| 2291 | `ExportAuthenticationSendDeliveryForExporterTurkeyJordanTemplate` | מכתב ליצואן – תורכיה וירדן | 12386 | לא |
| 2292 | `ExportAuthenticationSendDeliveryForExporterTextileTemplate` | מכתב הצהרת טקסטיל | 12386 | לא |
| 2293 | `ExportAuthenticationSendDeliveryForExporterEuropeTemplate` | מכתב ליצואן – אירופאי (אימות יצוא) | 12386 | לא |
| 2294 | `ExportAuthenticationSendReminderForExporterTemplate` | מכתב התראה (אימות יצוא) | 12386 | לא |
| 2295 | `ExportAuthenticationRequestMercosur` | מרקוסור – תקין ותקין חלקי (אימות יצוא) | 12386 | כן |
| 2317 | `ExportAuthenticationRequestEmptyTemplate` | תבנית ריקה | 12386 | כן |

**לא להמרה — לא פעילות בלגאסי (`State = 0`):** 2215 `ImportRequestForVerificationEUR`,
2216 `ImportRequestForVerificationEURreminder`, 2217 `ExporterLetter`. הקליינט הישן עדיין מפנה לשתי הראשונות,
אבל התבנית כבויה. התבנית הקוריאנית **אינה רשומה** בלגאסי — היא חדשה.

**מקור ה-docx:** `FileLocationName` מצביע על הגרסה האחרונה ב-`\\Gen-Prd-DOCFS\TemplateBackUp\...` /
`\\JR-PLT-DOCFS\TemplateBackUp\...` (2020–2022), שעשויה להיות חדשה מהעותק ב-`C:\Repos\Main\...\TemplateDocuments`.
ל-2292, 2294, 2317 אין גיבוי — העותק ב-Main הוא המקור.

## בלוק החתימה (`EmployeeFullName` / `EmployeeEmailAddress` / `UserSignature`) — לפי הלגאסי

בלגאסי הבלוק נשלף ב-SP הנתונים של כל מכתב מ-`UserMng_UserTemplateDetails` (שם אנגלי + `SignatureImageUrl`)
ב-INNER JOIN ל-`UserMng_User` (אימייל). כאן אלה טבלאות של שירות המשתמשים, ולכן הבלוק מגיע ב-BL דרך
`IUserProxy.GetUserTemplateDetails` (ה-route ממתין לאישור — `TODO(blocking)`). למשתמש בלי template details כל הבלוק ריק.

**מי החותם — משתנה בין המכתבים בלגאסי, ויש לשמר את זה בכל המרה:**

| מכתבים | החותם בלגאסי | SP לגאסי |
|---|---|---|
| 2251–2259 + הקוריאני | המשתמש שמפיק את המכתב (`@CreateUserID` → `RequestMetadata.UserId`) | `usp_Template_INNER_CROSS_CertificateOfOrigin` |
| 2289, 2291, 2292, 2295, 2317 | המשתמש שמפיק את המכתב; הטלפון מ-`UserMng_ContactDetailsFromCustoms` (CommunicationTypeID 1) | `usp_Template_INNER_CROSS_ExportDocumentAuthenticationRequestLetter` |
| 2290, 2293, 2294 | **משתמש קבוע 69591766** (hard-coded ב-SP), כולל טלפון מ-`UserMng_User.PhoneNumber` | אותו SP |
| 2244 | החתימה של `UpdateUserID` של הבקשה; `UserTitle` של המפיק | `usp_Template_INNER_CROSS_ImportAuthenticationRequestImporterLetter` |

ℹ️ נבדק ב-DB (14.10.2026): למשתמש 69591766 (שרה ביטון) יש שורת `UserTemplateDetails`, אבל השם האנגלי
והחתימה ריקים. לכן 2290, 2293 ו-2294 יוצאים בלגאסי בלי שם ובלי חתימה, רק עם האימייל והטלפון. אם מקבלים את
הלגאסי כמו שהוא, זו ההתנהגות. מתוך 7,282 משתמשים עם template details, ל-546 יש חתימה.

`UserSignature` הוא URL (`SignatureImageUrl`) שנשלח לשדה Image. ⚠️ `TODO(confirm)`: שמודול התבניות מקבל URL בשדה Image.

## דברים פתוחים בתבנית הקוריאנית

- **אין letterhead** — למכתב אין קבצי header/footer כלל, בשונה מ-8 תבניות המכתבים האחרות
  שנושאות לוגו + `ReferenceNumber` / `CreateDate` / `HebrewCreateDate`. לוודא שזה מכוון.
- נוסח המכתב נערך ב-23.9.2026 (מפריד בשורת הצהרות החשבון, ריפוד שורת מספר התיק, ושם המדינה
  שהיה `Korea (Democratic People's Republic of)` — צפון קוריאה — ותוקן ל-`Korea (Republic of)`).
  הנוסח המקורי הוא של הילה; התיקונים בוצעו לבקשת המפתחת.
