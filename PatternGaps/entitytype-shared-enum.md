# המלצת קונבנציה: EntityType כ-enum משותף (לא ליטרל inline)

תאריך: 2026-07-29 · מתודה: GetEntityDocuments (#19) · מגיש: תמר

## הרקע
כלל ה-`EntityType` הנוכחי ב-`wcf-migrate` (BATCH 1+2, "חיפוש ערך EEntityType עבור פרוקסי") מנחה לכתוב את
הערך כ**ליטרל עם comment**:
```csharp
// EntityType.ImportDeclaration = 1055 (...EntityType)
GetDocumentsByEntity(leadDocumentId, 1055)
```
בפועל ערכי EntityType חוזרים על עצמם בין מתודות (ב-CertificateOfOrigins: `CertificateOfOrigin=12319` ב-Convert,
`ImportDeclaration=1055` ב-GetEntityDocuments), וליטרלים מפוזרים = כפילות + סיכון חוסר-עקביות.

## ההמלצה
להחזיק enum משותף אחד לכל שירות — subset מאומת מהפלטפורמה `EEntityType` — ולהשתמש בו בכל מקום:
```csharp
// Model/ModelDTOs/EEntityType.cs — curated subset (ערכים אמיתיים מהפלטפורמה, לא מומצאים)
public enum EEntityType { ImportDeclaration = 1055, CertificateOfOrigin = 12319 }

// שימוש: GetDocumentsByEntity(leadDocumentId, (int)EEntityType.ImportDeclaration)
```
מוסיפים member חדש (עם ערכו האמיתי) בעת הצורך, במקום ליטרל חדש. זהה בקונבנציה ל-enums הקיימים
(`ECertificateOfOriginType`, `ERequestReason`) שכבר חולצו מה-DB עם ערכים אמיתיים.

## מה יושם כאן (2026-07-29)
נוצר `EEntityType.cs`; `GetEntityDocuments` ו-`Convert` שניהם משתמשים בו במקום ליטרלים.

## לפעולה
להעביר לקובי (מנהל הסקילים) — אם מאושר: לעדכן את כלל ה-EntityType ב-`wcf-migrate` להפנות ל-enum משותף
במקום ליטרל inline, ולתעד ב-`_shared/`.
