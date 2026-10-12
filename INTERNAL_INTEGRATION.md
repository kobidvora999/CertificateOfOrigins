# INTERNAL_INTEGRATION — CertificateOfOrigins
עדכון אחרון: 2026-10-11 · Branch/Commit: `master` @ `f9374bf` · מקור: `Postman/internal-workload-test/reports/pre-internal-gap-report_2026-10-11.md`

ברשת הפנימית אין Claude, ולכן כל הידע לאינטגרציה חייב להיות ברפו. המסמך הזה מרכז את מה שצריך לעשות בפנים.

## 1. שירותים שהמיקרו-סרוויס צורך (outbound) — 11 proxies
כל ה-proxies רשומים כ-`AddProxy<I, Real, Mock>` (`API/CertificateOfOrigins.BL/ServicesConfiguration.cs`).
- **ברירת המחדל היא real.** ה-mock פעיל רק כשהבקשה נושאת `x-mock-mode`.
- לכל route יש `TODO(blocking)` לאישור מול השירות האמיתי.

| Proxy | שירות יעד (CustomsMicroServices) | מצב צפוי בפנים | פעולה בפנים |
|---|---|---|---|
| CustomerProxy | Customers | REST קיים | לאשר routes (CustomersByIds, CustomerInformation, CustomersByCountry, IdByExternalId). לוודא ש-404 על ארץ בלי בית מכס לא הופך ל-5xx (HN-01) |
| VendorProxy | Vendors | REST קיים | לאשר VendorsByIds |
| UserProxy | Users | REST קיים | לאשר User/UsersByIds |
| ExportDealFileProxy | ExportDealFile | **השירות טרם הוקם** | לפתוח REST חדש (7 routes, ראו `Postman/dependency-workload-test/collections/... - ExportDealFile`) |
| DocumentsProxy | Documents | REST קיים | לאשר DocumentsByEntity / DeleteDocuments / AttachDocumentsToEntity / Document/{id} (D-F5, E-F4) |
| CollateralProxy | Collaterals | לבירור | לאשר routes. להעביר את הדגל `sendMessageOnChangeTempRequest=true` ב-ChangeTempCollateralRequest (F-08) |
| TasksProxy | Tasks | REST קיים | לאשר IsTaskExist / IsTaskExistsOnEntity / LatestUserHandlingEntityTasksWithTaskUnification. ‏TaskStatusID גולמי ל-G-F2 |
| MessageManagementProxy | Common | REST קיים | לאשר Message/SendMessage |
| CommonServicesProxy | Common | REST קיים | לאשר CreateQrCode / GenerateTemplate (SSRS) |
| CustomsBookProxy | CustomsBook | לבירור | לאשר 3 routes, כולל העברת תאריך הייחוס (`TODO(confirm)`) |
| OrganizationUnitProxy | SystemTables | REST קיים | לאשר IsCustomsHouse |

**תלויות לא-proxy:**
- `IDocumentUtil` (העלאת מסמכים ו-QR, Documents).
- `ITemplateUtil` (Templates, CR 194221).
- `IQueueUtil` (exchange `IssueCertificateOfOrigin`).
- `IEventUtil`, `ILockUtil`, `IParametersUtil`.

**Worker** (`Workers/CertificateOfOriginsWorkers` מארח את `IssueCertificateOfOrigin.Worker`):
- **שירותים שהוא קורא להם:** שירותי ה-WCF הלגאסיים CommonServicesExternal (SSRS) ו-DocumentsExternal, דרך Connected Services; ו-`POST Common/OutgoingMessage` לשליחת משוב 2281.
- **לפני הפעלה בפנים** (ראו §5):
  - לאשר את כתובות ה-WCF.
  - להריץ הודעה אמיתית אחת מקצה לקצה (לא הורץ מחוץ לרשת).
  - לתקן את ה-XML של 2281 (GN-01).

## 2. MockProxies להחלפה בפנים
| MockProxy | Proxy אמיתי | תלוי בשירות |
|---|---|---|
| CustomerMockProxy | CustomerProxy | Customers |
| VendorMockProxy | VendorProxy | Vendors |
| UserMockProxy | UserProxy | Users |
| ExportDealFileMockProxy | ExportDealFileProxy | ExportDealFile (טרם הוקם) |
| DocumentsMockProxy | DocumentsProxy | Documents |
| CollateralMockProxy | CollateralProxy | Collaterals |
| TasksMockProxy | TasksProxy | Tasks |
| MessageManagementMockProxy | MessageManagementProxy | Common |
| CommonServicesMockProxy | CommonServicesProxy | Common |
| CustomsBookMockProxy | CustomsBookProxy | CustomsBook |
| OrganizationUnitMockProxy | OrganizationUnitProxy | SystemTables |

אין צורך בשינוי קוד כדי לעבור ל-real: מספיק שהבקשות לא ישאו `x-mock-mode`. ה-Mocks נשארים לבדיקות.

## 3. צרכנים של השירות (inbound — מי קרא ל-WCF הישן)
| מודול צורך | מתודת WCF ישנה | Endpoint חדש | פעולה |
|---|---|---|---|
| לקוח CRM (מסכי תעודה) | Internal: GetCertificateOfOriginsByFilter / ById / IsCertificateOfOriginByExternalIdExist / SaveCertificateOfOrigin / LoadDataFromExportDeclaration | `QUERY ui/CertificateOfOrigins/ByFilter`, `GET ui/CertificateOfOrigins/{id}`, `GET .../ByExternalIdExist`, `POST ui/CertificateOfOrigins`, `QUERY .../LoadDataFromExportDeclaration` | להפנות ל-REST החדש |
| לקוח CRM (בקשות אימות ותיקים) | Internal: 17 פעולות AuthenticationRequest | `ui/AuthenticationRequest/*` | להפנות ל-REST החדש |
| לקוח CRM (מסמכי יצוא) | Internal: 5 פעולות ExportDocumentAuthenticationRequest | `ui/ExportDocumentAuthenticationRequest/*` | להפנות ל-REST החדש |
| ExportDealFile (שחרור / שינוי הצהרה) | External: UpdateCetrificateOfOrigins | `POST api/CertificateOfOrigins/Reconcile` | להפנות. **לבירור בפנים** מי השולח המדויק |
| Events (משלוח) | External: HandleAuthenticationRequestDeliverySent | `PATCH api/EventsResponse/HandleDeliverySent` (גוף: `EventResponseArgs`) | **אין רישום**: אף שורה ב-`Infrastructure.Events_EventTypeResponse` לא מפעילה אותה; שורה 5458 (אירוע 870) מפנה למתודה שאינה קיימת בלגסי. להחליט אם לרשום, לאיזה אירוע וב-verb איזה (MIGRATION-NOT-DONE) |
| מודולים חיצוניים | External: GetCertificateOfOriginID / GetGoodsItemCerificateDTO / Convert / SaveCertificateOfOriginAttachments | `GET api/CertificateOfOrigins/ID/{n}`, `POST .../GoodsItemCerificateDTO`, `/Convert`, `/SaveAttachments` | **לבירור בפנים**: רשימת הצרכנים |
| שער EAI (סוכני מכס) | Incoming: GetPC_MSG2280_2281 | `POST community/CertificateOfOrigins/Request` | להפנות. לוודא שה-header `MessageSenderId` מגיע (CN-03) |
| פורטל Web | Incoming: GetCertificateRequestByGuid | `GET web/CertificateOfOrigins/RequestByGuid` | להפנות. שמות השדות הלגאסיים נשמרו (D-F1) |
| הפצת מסמכים (CR 194221) | חדש | `GET api/CertificateOfOrigins/Template/{templateId}/{entityId}` | לאשר את החוזה מול הצרכן ואת ה-templateId (`TODO(confirm)`) |

⚠️ **שינוי חוזה:** הצרכנים חייבים לעדכן verbs (QUERY) ו-routes. אי אפשר לגזור מהרפו את רשימת הצרכנים המלאה, ולכן היא **לבירור בפנים**.

## 3a. Lookups מקומיים להחלפה בתשתית (TODO(internal))
| טיפוס מקומי (`BL/Lookups/`) | שירות מקור (`lookup/{Type}`) | פעולה בפנים |
|---|---|---|
| InternationalSite | SystemTables (הנחה) | להוסיף את הטיפוס לחבילה `CustomsCloud.InfrastructureCore.Lookup` ולוודא ששירות המקור מגיש `GET lookup/InternationalSite` |
| Site | Sites | כנ"ל |
| CountryGroup | Common (הנחה) | כנ"ל |
| CountryCountryGroup | Common (הנחה) | כנ"ל |
| CurrencyType | Common (הנחה) | כנ"ל |
| PackingType | Cargos | כנ"ל |
| DataDictionaryField | SystemTables (הנחה, אין שירות DataDictionary) | כנ"ל. בלעדיו שדות 20306/20310/20661 בשאילתת ה-Web יוצאים בלי תווית (DN-05) |

**פעולה קבועה לכל טיפוס:**
1. למחוק את ה-record המקומי.
2. להחליף `AddLocalLookup<Lookups.T>` ב-`AddLookup<Lookup.T>`.
3. להחליף `Lookups.T` ב-`Lookup.T` בקוד ה-BL.
4. להסיר את הטיפוס מ-`tools/local-lookup-stub.js`.

**חסר לגמרי:** `NavigationPath` (GetPathsForNavigationToVendor מחזיר ריק, ‏E-F17).

## 3b. רשימת הפעולות בפנים — כל ה-TODO
מקור יחיד: אזהרות REVIEW00x של `dotnet build` (`tools/review-markers.ps1`). רשימה מלאה עם קובץ:שורה נמצאת ב-
`Postman/internal-workload-test/reports/pre-internal-gap-report_2026-10-11_check3-todo-inventory.md` (165 מופעים).

| קטגוריה | כמות |
|---|---|
| `TODO(blocking)` — אישור routes של proxies, ערוץ 2281 inline, שדות בלי מקור | 63 |
| `TODO(internal)` — lookups מקומיים ומקורות | 17 |
| `TODO(migration)` — טקסטים (ValidationMessages/resx), LeadDocumentTitle | 16 |
| `TODO(confirm)` / `TODO(confirm live)` — enum ו-template id, QR מול Documents אמיתי, ExportDealFile אמיתי | 10 |
| `TODO(platform)` — `RequestMetadata.User` | 1 |

## 4. DB — סקריפטים ו-ROLLOUT
**סדר הרצה** (`API/CertificateOfOrigins.WebApi/Scripts/`, לפי השם):
1. `API_20260715 - create schema.sql`
2. `API_20260715 - create tables.sql`
3. `API_20260715 - seed data.sql`
4. `API_20260716 - add params.sql`
5. `API_20260721 - dbo.GetCertificateOfOriginsByFilter.sql`
6. `API_20260722 - dbo.CheckIfExistsAdditionalRequestsForVendor.sql`
7. `API_20260722b - dbo.CheckIfExistsAdditionalRequestsForImporter.sql`
8. `API_20260726130203 - dbo.GetImportAuthenticationRequestByFilter.sql`
9. `API_20260726165855 - dbo.GetAuthenticationRequestByLeadDocumentID.sql`
10. `API_20260727172359 - dbo.GetCertificateOfOriginByID.sql`
11. `API_20260728122720 - dbo.GetCertificateOfOriginDataForWebQuery.sql`
12. `API_20260813180359 - seed CountryIsrael parameter.sql`
13. `API_20260813183834 - dbo.GetCertificateOfOriginNumber + sequence.sql`
14. `API_20260907101500 - dbo.GetImportAuthenticationRequestsForReminderForImporterScheduler.sql`
15. `API_20260907101600 - dbo.GetAuthenticationRequestsForScheduler.sql`
16. `API_20260907180000 - dbo.GetImportAuthenticationRequestByFilter multi-invoice.sql`
17. `API_20260922093000 - seed AdministrativeClosure decision and file status.sql`
18. `API_20260923104500 - dbo.GetTemplateData.sql`
19. `API_20260928120000 - CertificateOfOrigins_CustomsItemToExportDocumentAuthenticationRequest table.sql`
20. `API_20261008110054 - dbo.GetCertificateOfOriginsByFilter date bounds.sql`
21. `API_20261008110055 - dbo.GetImportAuthenticationRequestByFilter date bounds.sql`

- **from-zero replay** (2026-10-11): עבר פעמיים על DB נקי, rc=0 בשני המעברים. הסקריפטים idempotent.
- **פרמטרים לקביעה לפני פריסה:**
  - `IssueCertificateOfOriginByWorker`: ה-seed הוא `False`. להחליט מה ברירת המחדל עכשיו כשה-host של ה-Workers קיים.
  - `DaysForLastDelivery`: ה-seed הוא `10`. לאמת מול ערך הייצור.
- **תבניות:** `Templates/ExportAuthenticationSendDeliveryForExporterSouthKoreaTemplate.{docx,yml}` צריכות **העלאה ידנית ל-MinIO bucket `templates`**. אין כלי ברפו (`tools/upload-templates.ps1` חסר).
- **צ'ק-ליסט ROLLOUT:**
  1. הרצת הסקריפטים בפנים.
  2. שבועיים של הקוד החדש על ה-DB הישן.
  3. יום ROLLOUT: העתקת נתוני הטבלאות ל-DB החדש.

## 5. חוסרים פתוחים שחוסמים אינטגרציה (מדוח 6a של 2026-10-11)
- **BN-01 (HIGH):** ב-Reconcile, פריטי טובין בלי פרט מכס מדולגים, והתעודה מסתיימת DeclarationMatch במקום Rejected.
- **GN-01 (HIGH):** ה-XML של משוב 2281 ב-worker לא תואם לסכמת ה-EAI (רישיות ו-namespace). לברר מול קובי אם זה המצב בייצור.
- **MED:**
  - AN-01: שמירת תעודה לא אטומית.
  - AN-02: מסמך ה-QR נמחק בפרסום.
  - CN-01: שאילתת סטטוס מריצה בדיקות cross-field.
  - FN-01: ביטול תיק פותח משימות דחייה.
  - FN-02 / GN-03: יחידה ארגונית 0 מובילה ל-500.
  - GN-02: חסרים IssueDate* במשוב.
  - GN-04: QueryURL של ה-worker נקרא מ-appsettings.
- **CR-N1:** templateId לא רשום מחזיר 500 במקום 400 (`CertificateOfOriginsBl.Templates.cs:101`).
- **Coverage לא נמדד אחרי התיקונים.**
  - הצעדים: להחזיר את `IssueCertificateOfOriginByWorker=False` ב-DB המקומי, למחוק את מפתח ה-Redis, ולהריץ ב-Development עם `tools/local-lookup-stub.js` (בלי להעביר את Consul).
  - **לא** להשתמש במעקף ה-Consul→Test: הוא שובר את ה-lookups המקומיים ואת Documents.
- **אופרציה שהושמטה בכוונה:** `TempSync` (stub מת בלגאסי).
