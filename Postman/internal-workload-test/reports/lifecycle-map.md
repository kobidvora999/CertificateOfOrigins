# Lifecycle map — CertificateOfOrigins

תאריך: 2026-09-28 · נגזר מהקוד (`postman-coverage` STEP 2). מקור האמת לכיסוי-תהליך: כל מעבר כאן חייב להיות מוכח
ב-chain שיוצר את הישות שלו באותה ריצה ומאמת את המצב שאחרי המעבר.

> הערות רוחב
> - **אין אכיפת מעברים בשרת.** הסטטוס נשלח ע"י הלקוח (נבדק רק שהוא ערך enum קיים); תופעות-לוואי רצות כשהערך שונה
>   מ-`Original*` שנשלח. ה-chains הולכים לפי המעברים שה-UI הישן התיר.
> - **Delivery endpoints מחשבים מהערכים שנשלחו**, לא מה-DB — כל צעד מזין את תשובת הקודם.
> - **Planar jobs** לא משנים סטטוס (פותחים משימות תזכורת בלבד) ואין להם HTTP trigger — מחוץ ל-chains.
> - Headers: `x-mock-mode` גלובלי, `x-mock-feature--{Flag}: true` לענפים, `CC-USER-ID: 5`.

## 1. תעודת מקור — `ECertificateOfOriginStatus`

2 נקלטה · 3 נדחתה · 4 מבוטלת · 5 אינה תואמת · 6 תואמת · 7 ממתינה להתרה · 8 מאושרת לפרסום (1 שגויה — אין מסלול).

| # | מעבר | Trigger | ערכים מניעים | קוד |
|---|---|---|---|---|
| C1 | ∅ → 2 | `POST community/CertificateOfOrigins/Request` | reason ∈ {1,2,4,5,10,12} | MessageValidation.cs:76-203 |
| C1a | 2 → 6/5/3 (אותה קריאה) | reconcile אוטומטי אחרי השמירה | reason≠10, type≠5, הצהרה Submitted/Released (ברירת מחדל של ה-mock; `DealFile.NotReleased` משאיר ב-2) | MessageDeclarationCheck.cs:15-43 |
| C2 | גרסה חדשה → 2; ישנה → 4 | Request reason 3 / UI create עם מספר קיים | `CertificateId` קיים | Bl.cs:938-994 |
| C3 | 2/5/6/7 → 3 | `POST ui/CertificateOfOrigins` | status 3 + `RejectCancelReason` + `Original…` | Bl.cs:1388 |
| C4 | 2/5/6 → 7 | same | status 7 | Bl.cs:952,1390 |
| C5 | any → 4 | same | status 4 | Bl.cs:1389 |
| C5c | 2/5/6/7 → 8 | same | status 8 (שורה קיימת) | Bl.cs:1027-1034 |
| C6/7/8 | 2 → 6 / 5 / 3 | `POST api/CertificateOfOrigins/Reconcile` eventType 240 | התאמה / אזהרות בלבד / שגיאה | Bl.cs:1627-1701 |
| C9 | 7 → 8 | Reconcile eventType 1423 / 1790 | הצהרה שוחררה | Bl.cs:1738-1792 |
| C10 | backfill → reconcile | Reconcile eventType 334 | מספר הצהרה ריק | Bl.cs:1706-1728 |
| C11 | any → 4 | Reconcile eventType 554 | ביטול הצהרה | Bl.cs:1820-1831 |
| C12 | any → 4 | Request reason 14 | `CertificateId` | Bl.cs:157-227 |
| C13 | קריאה | Request reason 13 | `CertificateId` | Bl.cs:153-155 |

Reads שייכים לתהליך (על ישות שנוצרה): `GET ui/CertificateOfOrigins/{id}`, `GET api/CertificateOfOrigins/ID/{number}`,
`GET web/CertificateOfOrigins/RequestByGuid` (אחרי 8), `POST api/…/GoodsItemCerificateDTO`, `POST api/…/Convert`,
`POST api/…/SaveAttachments`, `GET ui/…/ByExternalIdExist`, `QUERY ui/…/ByFilter`, `QUERY ui/…/LoadDataFromExportDeclaration`.

## 2. בקשת אימות יבוא + תיק אימות

File status: 1 ממתין למכתב · 2 נשלחה פנייה · 3 נשלחה תזכורת · 4 מענה חלקי · 5 התקבל מענה · 6 תקין · 7 נדרשת הבהרה · 8 פסול · 9 מבוטל · 10 סגירה מנהלית.
Decision: 1 חדשה · 2 פסולה · 3 תקינה · 4 הבהרה נוספת · 5 חלקית · 6 נדרש אימות · 7 לא תקינה · 8 נשלחה פנייה ליבואן · 9 תזכורת ליבואן · 10 סגירה מנהלית.

| # | מעבר | Trigger | ערכים מניעים | קוד |
|---|---|---|---|---|
| R0 | ∅ → decision 1 | `POST ui/AuthenticationRequest/SaveImport` | `IsNewInstance=true`, `DocumentId` נקבע ע"י הקורא | ArBl:849-961 |
| R1 | decision 1 → 6/7/… | SaveImport `IsNewInstance=false` | `DecisionId` | ArBl:862-936 |
| F0 | ∅ → file (1, dm 1) | `POST ui/AuthenticationRequest/CreateNewFile` | רשימת בקשות | ArBl:373-459 |
| F1 | (1,*) → (2,2) | `POST …/HandleImportDeliveryAndReminderForVendorSent` | `IsDelivery=true` | ArBl:502-613 |
| F3 | → (3,dm) | same | `IsDelivery=false` | ArBl:505-610 |
| F4 | decision → 8 | `POST …/HandleImportDeliveryForImporterSent` | file ids | ArBl:524-584 |
| F5 | decision → 9 | `POST …/HandleImportDeliveryReminderForImporterSent` | same | ArBl:535-542 |
| F6 | (tasks) | `POST …/ChangeStatusAfterDeliverySent` | `{Id, OrganizationUnitId}` | ArBl:465-477 |
| F7 | (tasks) | `POST …/CloseReminderTask` | same | ArBl:483-496 |
| F8 | (check) | `POST api/EventsResponse/HandleDeliverySent` | relatedEntities 12385 | ArBl:350-366 |
| S1 | file → 5/6/7/8/9/10 + decisions | `POST ui/AuthenticationRequest/SaveFile` | status + `Original…`, children decisions | ArBl:1075-1328 |

Reads: `GET …/{documentId}`, `GET …/File/{fileId}`, `QUERY …/ByFilter`, `POST …/ByLeadDocumentIDs`,
`GET …/CheckImporterOfImportAuthentication`, `GET …/CheckIfExistsAdditionalRequestsFor{Vendor,Importer}`,
`GET …/EntityDocuments/{leadDocumentId}`, `GET …/PathsForNavigationToVendor`.

Template: `GET api/CertificateOfOrigins/Template/1/{fileId}` — **entityId = תיק אימות יבוא**; ⏸ BLOCKED עד שקובץ
התבנית יועלה ל-MinIO (INTERNAL_INTEGRATION.md §4).

## 3. בקשת אימות מסמך יצוא — `EExportAuthenticationRequestStatus`

1 ממתין למכתב · 2 ממתין ליצואן · 3 ממתין לאחר התראה · 4 ממתין לפרטים · 5 מוכן לטיפול · 6 סגור תקין · 7 סגור לא תקין · 8 סגור חלקי · 9 בוטל.
Write יחיד: `POST ui/ExportDocumentAuthenticationRequest` (`StatusId` + `OriginalStatusId`), ExBl:129-273.
Reads: `GET …/{id}`, `QUERY …/Search`, `GET …/CustomerInformation/{customerId}`, `GET …/CustomerInformationByCountry/{countryId}`.

## 4. מיפוי ל-collections

| תהליך | Collection | מצב |
|---|---|---|
| תעודה — מסלול ראשי 2→6→7→8→קריאות→ביטול | `Certificate Lifecycle` | חדש |
| תעודה — חלופות (דחייה, החלפה, עדכון, אזהרות) | `Bl Core`, `Per Reason`, `Reconcile Outcomes`, `Mock Features` | קיים |
| יבוא — בקשה→תיק→משלוחים→החלטות→סגירה→תבנית, ביטול→תיק חדש | `Import Auth Lifecycle` | חדש |
| יבוא — קריאות ראשונות + FirstProvideContactDate | `Auth Lifecycle` | קיים |
| יבוא — מכונת סטטוס התיק | `Auth File Status`, `Save Import Decisions` | קיים |
| יצוא — 1→2→3→4→5→6 + סיום חלופי | `Export Auth Lifecycle` | חדש |

## 5. שאלות פתוחות (לא חוסמות בניית chain)

1. אין אכיפת מעברים בשרת (תעודה/תיק/יצוא) — האם זה מכוון, או שצריך ולידציית מעברים כמו ה-UI הישן?
2. UI create עם `Id=0` בסטטוס 8 מייצר QR אבל מדלג על כל שלב הפרסום (IssuingDate, event 640, template).
3. פרסום אוטומטי אחרי שמירה ל-7 כשההצהרה כבר שוחררה — לא הומר (Bl.cs:1036-1040).
4. `SendRequestFeedback` שולח `MessageTypeId = 0` — MessageManagement האמיתי צפוי לדחות.
