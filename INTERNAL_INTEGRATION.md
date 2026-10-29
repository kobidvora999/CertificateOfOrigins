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
| 9096 אימות תעודה באתר הציבורי (`web`) | טיוטת REST טהור (camelCase, 404/400 במקום `ExeptionDescription`) — מבטלת את D-F1. מסמך: https://claude.ai/artifact/NJNVyr6nSb8aZ7RYT9s9Nt | 29/10/2026 | ממתין למכס ולצוות האתר | **לא מומש** — ה-endpoint הקיים `GET web/certificate-of-origins/request-by-guid` לא השתנה בהתנהגות (רק הכתובת עברה ל-kebab, §6) |

מסמכים לקהילה (2280/2281): השוואה V2 מול REST טהור — https://claude.ai/artifact/2Cs54oEsJchsFuHp2REk3o · חוזה ה-API — https://claude.ai/artifact/RiJhNbYDfxSHjtVFzkdPRu

## 2. צרכנים שצריך לעדכן (inbound)
| צורך | לפני | אחרי | פעולה בפנים |
|---|---|---|---|
| סוכני המכס (קהילה), דרך פלטפורמת המסרים | `POST community/CertificateOfOrigins/Request` — פעולה לפי `requestReasonCode`, הכל 200 | `POST community/certificate-of-origins` (1/2/10/12) · `PUT .../{certificateId}` (1/2/12 עם מספר, 3) · `POST .../{certificateIdToCancel}/replacement` (4/5) · `GET .../{certificateId}` (13) · `DELETE .../{certificateId}` (14); שגיאות 400/404/409 עם ProblemDetails | TODO(internal): לעדכן את ניתוב פלטפורמת המסרים ל-5 הכתובות, ולוודא שהיא מעבירה `MessageSenderId` |
| — | הצרופה במשוב החזירה את תוכן ה-PDF | `attachment[].externalAttachmentId` בלבד (המסמך מועלה ל-Documents עם `ExternalId`) | TODO(internal): לוודא ש-Documents שומר את `ExternalId` מ-`IDocumentUtil.UploadDocument` ושהקהילה יכולה למשוך לפיו |

## 3. בנייה והרצה
- **DTOs:** `Shared/CertificateOfOrigins.Model/OpenApiModels.tt` → Run Custom Tool מייצר את `CertificateOfOriginsCommunityModels.cs` מהחוזה. הקובץ שבמאגר נוצר מחוץ לרשת עם NSwag 14.7.1 באותן אפשרויות. TODO(internal): להריץ את ה-TT ולוודא שאין הבדל.
- **Postman:** 108 בקשות הומרו לכתובות החדשות; קוד האתר בגוף הבקשות שונה ל-`SIT01` (‏`CustomsHouse` מוגבל ל-5 תווים ב-XSD). TODO(internal): לוודא ש-`SIT01` קיים ב-Site lookup הפנימי (`ExternalSiteNumberForMessages`), או להחליף לקוד אמיתי.

## 4. התאמה לסקילים (29/10/2026)
- **חוזה התבניות הגנרי:** נוסף `GET api/certificate-of-origins/template-data/{templateId:int}/{entityId:int}` (‏`PrintTemplateDto`, בלי רינדור), לצד `template/{templateId:int}/{entityId:int}` (ה-PDF); מזהה שאינו מספר מחזיר 404 במקום 400.
- **Postman:** הסביבות עברו ל-v3 ב-`Postman/postman/environments/*.environment.yaml` (הקבצים הישנים ב-`Postman/Environments/` נמחקו), ונוסף המניפסט `Postman/.postman/resources.yaml`. TODO(internal): להוסיף את מזהה ה-workspace של Postman למניפסט.
- **מסד נתונים מקומי:** `DatabaseMigrationUtil` מושבת ב-`Program.cs`, ולכן סקריפטים חדשים לא רצים בעלייה. TODO(internal): להחיל את `API_20261015180000 - dbo.GetTemplateData preference letters.sql` בכל סביבה שבה `GetTemplateData` נכשל ב-"too many arguments".
- **CustomerProxy.GetCustomersByCountry:** הנתיב `Customer/customers` (מהקוד מבפנים) בלי `{countryId}`/`{activityType}`, ולכן שני הפרמטרים לא נשלחים והקריאה לא מסוננת. TODO(internal): לברר את שמות ה-query string ש-Customers מצפה להם ולעבור ל-`AddQueryStringParameter`.

## 5. מפת ה-routes לחיבור הצרכנים (שלב 4)
ה-client וה-ESB עדיין לא מחוברים ל-REST, ולכן אף צרכן לא נשבר. כשמחברים אותם (שלב 4), לפנות רק לכתובות האלה.
כל הכתובות ב-kebab-case (§6); מזהה בנתיב עם `:int` — מזהה שאינו מספר מחזיר 404.

| Verb | Route | פעולת WCF במקור |
|---|---|---|
| QUERY | `ui/certificate-of-origins` | GetCertificateOfOriginsByFilter |
| GET | `ui/certificate-of-origins/{certificateOfOriginId:int}` | GetCertificateOfOriginById |
| GET | `ui/certificate-of-origins/by-external-id-exist` | IsCertificateOfOriginByExternalIdExist |
| QUERY | `ui/certificate-of-origins/load-data-from-export-declaration` | LoadDataFromExportDeclaration |
| POST | `ui/certificate-of-origins` | SaveCertificateOfOrigin (הוספה או עדכון לפי `Id == 0`) |
| QUERY | `ui/authentication-request` | GetAuthenticationRequestByFilter |
| GET | `ui/authentication-request/{documentId:int}` | GetAuthenticationRequestByID |
| POST | `ui/authentication-request` | SaveImportAuthenticationRequest (הוספה או עדכון לפי `IsNewInstance`) |
| QUERY | `ui/authentication-request/by-lead-document-ids` | GetAuthenticationRequestByLeadDocumentIDs |
| GET | `ui/authentication-request/check-importer-of-import-authentication` | CheckImporterOfImportAuthentication |
| GET | `ui/authentication-request/additional-exist-for-vendor` | CheckIfExistsAdditionalRequestsForVendor |
| GET | `ui/authentication-request/additional-exist-for-importer` | CheckIfExistsAdditionalRequestsForImporter |
| GET | `ui/authentication-request/entity-documents/{leadDocumentId:int}` | GetEntityDocuments |
| POST | `ui/authentication-request/change-status-after-delivery-sent` | ChangeStatusAfterDeliverySent |
| POST | `ui/authentication-request/close-reminder-task` | HandleSendRemindDeliverNotification |
| POST | `ui/authentication-request/handle-import-delivery-and-reminder-for-vendor-sent` | HandleImportAuthenticationRequestDeliveryAndReminderForVendorSent |
| POST | `ui/authentication-request/handle-import-delivery-for-importer-sent` | HandleImportAuthenticationRequestDeliveryForImporterSent |
| POST | `ui/authentication-request/handle-import-delivery-reminder-for-importer-sent` | HandleImportAuthenticationRequestDeliveryReminderForImporterSent |
| POST | `ui/authentication-request/file` | CreateNewAuthenticationFile |
| GET | `ui/authentication-request/file/{fileId:int}` | GetAuthenticationRequestFileByID |
| PUT | `ui/authentication-request/file/{fileId:int}` | SaveAuthenticationRequestFile — ה-`Id` בגוף חייב להיות אותו קובץ, אחרת 400 |
| GET | `ui/authentication-request/paths-for-navigation-to-vendor` | GetPathsForNavigationToVendor |
| QUERY | `ui/export-document-authentication-request` | GetExportDocumentAuthenticationRequestSearch |
| GET | `ui/export-document-authentication-request/{id:int}` | GetExportDocumentAuthenticationRequestByID |
| POST | `ui/export-document-authentication-request` | SaveExportDocumentAuthenticationRequest |
| GET | `ui/export-document-authentication-request/customer-information/{customerId:int}` | GetCustomerInformation |
| GET | `ui/export-document-authentication-request/customer-information-by-country/{countryId:int}` | GetCustomerInformationByCountry |
| GET | `api/certificate-of-origins/id/{certificateNumber}` | GetCertificateOfOriginID |
| POST | `api/certificate-of-origins/goods-item-certificate` | GetGoodsItemCerificateDTO |
| POST | `api/certificate-of-origins/convert` | Convert (ESB/EAI) |
| POST | `api/certificate-of-origins/attachments` | SaveCertificateOfOriginAttachments |
| POST | `api/certificate-of-origins/reconcile` | UpdateCetrificateOfOrigins |
| GET | `api/certificate-of-origins/template/{templateId:int}/{entityId:int}` | CR 194221 (הדיוור) |
| GET | `api/certificate-of-origins/template-data/{templateId:int}/{entityId:int}` | חוזה התבניות הגנרי |
| PATCH | `api/events-response/handle-delivery-sent` | HandleAuthenticationRequestDeliverySent (תגובת אירוע) |
| GET | `web/certificate-of-origins/request-by-guid` | GetCertificateRequestByGuid (9096) |
| POST / PUT / GET / DELETE | `community/certificate-of-origins[/{certificateId}]`, `POST .../{certificateIdToCancel}/replacement` | 2280/2281 (§2) |

TODO(internal): כל צרכן של `api` (ExportDealFile ל-`reconcile`, ה-ESB ל-`convert`, הדיוור ל-`template`, Templates ל-`template-data`, ושאר הקוראים) מעדכן את ה-proxy שלו לכתובת שבטבלה.

## 6. מעבר ל-kebab-case (29/10/2026)
השירות עבר ל-kebab-case (ה-route של כל controller כתוב מפורש, למשל `[Route("ui/authentication-request")]`; אין transformer ב-`Program.cs`), כי הצרכנים עוד לא חוברו. באותו מעבר תוקנו ארבעה שמות שלא עמדו בכללי ה-routes:
- `api/.../SaveAttachments` → `POST api/certificate-of-origins/attachments` (בלי פועל ב-route);
- `ui/AuthenticationRequest/ByLeadDocumentIDs` → `QUERY ui/authentication-request/by-lead-document-ids` (קריאה עם רשימה היא QUERY, לא POST);
- `ui/AuthenticationRequest/CheckIfExistsAdditionalRequestsFor{Vendor,Importer}` → `additional-exist-for-{vendor,importer}` (בלי לחזור על "request");
- `api/.../GoodsItemCerificateDTO` → `POST api/certificate-of-origins/goods-item-certificate` (בלי שגיאת הכתיב ובלי DTO).

כל אוספי ה-Postman, חוזי ה-OpenAPI והאפיונים עודכנו לכתובות החדשות.

**מסמכים נוספים בבקשת אימות יצוא:** הקישור עובר דרך `IDocumentUtil.SaveDocumentToEntity` (‏`POST {Documents}/document/SaveDocumentToEntity`). TODO(internal): לוודא שה-route הזה קיים בשירות Documents הפנימי (ה-probe ב-dependency-workload בודק אותו).

