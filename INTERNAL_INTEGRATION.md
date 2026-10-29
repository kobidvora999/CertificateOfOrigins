# INTERNAL_INTEGRATION — CertificateOfOrigins
עדכון אחרון: 2026-10-29 · Branch: main

צ'ק-ליסט לפעולות ברשת הפנימית (שם אין Claude). כרגע הקובץ מתעד את המעבר של מסרי הקהילה וה-web ל-REST טהור.

## 1. החלטות על מסרים נכנסים (REST טהור)
| מסר | החלטה | תאריך | מי | מצב |
|---|---|---|---|---|
| 2280/2281 בקשת תעודת מקור מסוכן (`community`) | חלופה 2: פיצול לפי METHOD, מספר התעודה בכתובת. מסמך: https://claude.ai/artifact/Lr6ZGVbaaUHnDGeXdU77tL | 28/10/2026 | המכס (אושר דרך המפתח) | מומש: `.spec/OpenApi/certificateOfOriginsCommunity.json` |
| 2280/2281 — בדיקת בעלות | GET/PUT/DELETE/Replacement מחזירים 404 לתעודה שלא השולח יצר (`CreateCustomerId` ≠ `MessageSenderId`). חדש מול legacy | 29/10/2026 | המפתח | מומש |
| 2280/2281 — מספר התעודה בכתובת | התאמה מדויקת (`CertificateNumber == id`) במקום LIKE מכיל (ממצא C F-23) | 29/10/2026 | המפתח | מומש |
| 2280/2281 — שמות מערכים | נשארים ביחיד כמו ב-XSD וב-V2 (`invoiceDetail`, `itemDetail`, `exception`, `attachment`) | 29/10/2026 | המפתח | מומש |
| 9096 אימות תעודה באתר הציבורי (`web`) | טיוטת REST טהור (camelCase, 404/400 במקום `ExeptionDescription`) — מבטלת את D-F1. מסמך: https://claude.ai/artifact/NJNVyr6nSb8aZ7RYT9s9Nt | 29/10/2026 | ממתין למכס ולצוות האתר | **לא מומש** — ה-endpoint הקיים `GET web/CertificateOfOrigins/RequestByGuid` לא השתנה |

מסמכים לקהילה (2280/2281): השוואה V2 מול REST טהור — https://claude.ai/artifact/2Cs54oEsJchsFuHp2REk3o · חוזה ה-API — https://claude.ai/artifact/RiJhNbYDfxSHjtVFzkdPRu

## 2. צרכנים שצריך לעדכן (inbound)
| צורך | לפני | אחרי | פעולה בפנים |
|---|---|---|---|
| סוכני המכס (קהילה), דרך פלטפורמת המסרים | `POST community/CertificateOfOrigins/Request` — פעולה לפי `requestReasonCode`, הכל 200 | `POST community/CertificateOfOrigins` (1/2/10/12) · `PUT .../{certificateId}` (1/2/12 עם מספר, 3) · `POST .../{certificateIdToCancel}/Replacement` (4/5) · `GET .../{certificateId}` (13) · `DELETE .../{certificateId}` (14); שגיאות 400/404/409 עם ProblemDetails | TODO(internal): לעדכן את ניתוב פלטפורמת המסרים ל-5 הכתובות, ולוודא שהיא מעבירה `MessageSenderId` |
| — | הצרופה במשוב החזירה את תוכן ה-PDF | `attachment[].externalAttachmentId` בלבד (המסמך מועלה ל-Documents עם `ExternalId`) | TODO(internal): לוודא ש-Documents שומר את `ExternalId` מ-`IDocumentUtil.UploadDocument` ושהקהילה יכולה למשוך לפיו |

## 3. בנייה והרצה
- **DTOs:** `Shared/CertificateOfOrigins.Model/OpenApiModels.tt` → Run Custom Tool מייצר את `CertificateOfOriginsCommunityModels.cs` מהחוזה. הקובץ שבמאגר נוצר מחוץ לרשת עם NSwag 14.7.1 באותן אפשרויות. TODO(internal): להריץ את ה-TT ולוודא שאין הבדל.
- **Postman:** 108 בקשות הומרו לכתובות החדשות; קוד האתר בגוף הבקשות שונה ל-`SIT01` (‏`CustomsHouse` מוגבל ל-5 תווים ב-XSD). TODO(internal): לוודא ש-`SIT01` קיים ב-Site lookup הפנימי (`ExternalSiteNumberForMessages`), או להחליף לקוד אמיתי.

## 4. התאמה לסקילים (29/10/2026)
- **חוזה התבניות הגנרי:** נוסף `GET api/CertificateOfOrigins/TemplateData/{templateId:int}/{entityId:int}` (‏`PrintTemplateDto`, בלי רינדור). ‏`Template/{templateId:int}/{entityId:int}` לא השתנה בכתובת; מזהה שאינו מספר מחזיר עכשיו 404 במקום 400.
- **Postman:** הסביבות עברו ל-v3 ב-`Postman/postman/environments/*.environment.yaml` (הקבצים הישנים ב-`Postman/Environments/` נמחקו), ונוסף המניפסט `Postman/.postman/resources.yaml`. TODO(internal): להוסיף את מזהה ה-workspace של Postman למניפסט.
- **מסד נתונים מקומי:** `DatabaseMigrationUtil` מושבת ב-`Program.cs`, ולכן סקריפטים חדשים לא רצים בעלייה. TODO(internal): להחיל את `API_20261015180000 - dbo.GetTemplateData preference letters.sql` בכל סביבה שבה `GetTemplateData` נכשל ב-"too many arguments".
- **CustomerProxy.GetCustomersByCountry:** הנתיב `Customer/customers` (מהקוד מבפנים) בלי `{countryId}`/`{activityType}`, ולכן שני הפרמטרים לא נשלחים והקריאה לא מסוננת. TODO(internal): לברר את שמות ה-query string ש-Customers מצפה להם ולעבור ל-`AddQueryStringParameter`.
