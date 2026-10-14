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
GET api/CertificateOfOrigins/Template/{templateId}/{entityId}
  → CertificateOfOriginsBl.GenerateTemplate
      → GetTemplateMeta(templateId)   ← ממפה id → (Result DTO, {Name}, Format)
      → dbo.GetTemplateData           ← ענף פר-תבנית שמחזיר את עמודות ה-Result DTO
      → EnrichTemplateData            ← שמות חוצי-שירות (לקוח, מדינה) מ-proxy/lookup
      → ITemplateUtil.GenerateTemplate ← מודול התבניות ממזג ומחזיר את המסמך
```

תבנית חדשה = ערך ב-`ECertificateOfOriginsTemplate` + `case` ב-`GetTemplateMeta` + Result DTO +
ענף ב-`dbo.GetTemplateData` + זוג הקבצים כאן.

⚠️ שמות ה-property ב-Result DTO עוברים `JsonNamingPolicy.CamelCase`, ולכן ה-`field path` ב-YAML
חייב להיות camelCase של שם ה-property (`FileNo` → `$.fileNo`).

## התבניות שכאן

| קובץ | מזהה | CR |
|---|---|---|
| `ExportAuthenticationSendDeliveryForExporterSouthKoreaTemplate` | ⚠️ `TODO(confirm)` — ראה `ECertificateOfOriginsTemplate` | 194221 |

### מכתבי לגאסי שטרם הומרו (18 פעילים — נשלף מ-`Infrastructure.General_enum_Template`, 14.10.2026)

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
