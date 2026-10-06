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

שאר מכתבי מסמכי ההעדפה (‏2290 / 2293 / 2294 ואחרים) **טרם הומרו** — הם ממתינים ל-`Enumeration` של
כל תבנית ולהכרעה על מקור חתימת המשתמש (`UserSignature`, שדה תמונה ללא מקור בשירות).

## דברים פתוחים בתבנית הקוריאנית

- **אין letterhead** — למכתב אין קבצי header/footer כלל, בשונה מ-8 תבניות המכתבים האחרות
  שנושאות לוגו + `ReferenceNumber` / `CreateDate` / `HebrewCreateDate`. לוודא שזה מכוון.
- נוסח המכתב נערך ב-23.9.2026 (מפריד בשורת הצהרות החשבון, ריפוד שורת מספר התיק, ושם המדינה
  שהיה `Korea (Democratic People's Republic of)` — צפון קוריאה — ותוקן ל-`Korea (Republic of)`).
  הנוסח המקורי הוא של הילה; התיקונים בוצעו לבקשת המפתחת.
