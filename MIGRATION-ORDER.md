# סדר המרה — CertificateOfOrigins (from-scratch, נקבע 2026-07-21)

מיגרציה **מאפס** של כל חוזי ה-WCF. הסדר מהקל לכבד; כל גל מקים תשתית שהגל הבא נשען עליה
(entity+DbContext → SP/YAML → proxies → events → messaging). מקור ה-WCF: `C:\Repos\Main\CRM\CertificateOfOrigins\Server`.

`TempSync` מדולגת — stub מת (`NotImplementedException`).

## מקרא סטטוס
`⬜ טרם` · `🔄 בעבודה` · `✅ הומרה` · `⚠️ חסום/לברר`

## גל 1 — קריאות טריוויאליות (entity ראשי + DbContext + controllers)
| # | מתודה | חוזה (ClassName) | סטטוס |
|---|---|---|---|
| 1 | GetCertificateOfOriginID | External (CertificateOfOriginsExternalService) | ✅ הומרה (branch `feature/migrate-get-certificate-of-origin-id`) |
| 2 | IsCertificateOfOriginByExternalIdExist | Internal (CertificateOfOriginsInternalService) | ✅ הומרה (branch `feature/migrate-is-certificate-of-origin-by-external-id-exist`) — SP dbo.GetCertificateOfOriginsByFilter (JOIN הוסר) + Customer proxy (mock) |
| 3 | CheckImporterOfImportAuthentication | Internal | ✅ הומרה (branch `feature/migrate-check-importer-of-import-authentication`) — BL+controller חדשים `AuthenticationRequest` + entity VerificationProhibitedImporters |
| 4 | GetGoodsItemCerificateDTO | External | ✅ הומרה (branch `feature/migrate-get-goods-item-cerificate-dto`) — reuse מלא של GetCertificateOfOriginIdByNumber + DTO חדש |

## גל 2 — SP סקלרי + חיפוש לפי פילטר (YAML + DbContextExtension + SP)
| # | מתודה | חוזה | סטטוס |
|---|---|---|---|
| 5 | CheckIfExistsAdditionalRequestsForVendor | Internal | ✅ הומרה (branch `feature/migrate-check-if-exists-additional-requests-for-vendor`) — SP סקלרי dbo.CheckIfExistsAdditionalRequestsForVendor (execute-scalar) |
| 6 | CheckIfExistsAdditionalRequestsForImporter | Internal | ✅ הומרה (branch `feature/migrate-check-if-exists-additional-requests-for-importer`) — SP סקלרי dbo (Infrastructure.Parameters במקום General_enum_GlobalParam); חתימה שוטחה entity→4 סקלרים |
| 7 | GetCertificateOfOriginsByFilter | Internal | ✅ הומרה (branch `feature/migrate-get-certificate-of-origins-by-filter`) — כל התשתית (SP/DAL/BL/DTO/enrichment) כבר הוקמה ב-#2; נוסף רק ה-controller endpoint |
| 8 | GetAuthenticationRequestByFilter | Internal | ✅ הומרה (branch `feature/migrate-get-authentication-request-by-filter`) — SP דינמי dbo.GetImportAuthenticationRequestByFilter (5 JOINs בין-שירותיים הוסרו, UDF+CONTAINS הוחלפו) + 2 DTOs חדשים + IVendorProxy חדש (mock) + ILookupUtil<Country>+<OrganizationUnit>; רק LeadDocumentTitle נשאר null (CRP.DealFile — צריך proxy) |
| 9 | GetExportDocumentAuthenticationRequestSearch | Internal | ✅ הומרה (branch `feature/migrate-get-export-document-authentication-request-search`) — SP דינמי dbo.ExportDocumentAuthenticationRequestSearch (CROSS_ הוסר, 3 JOINs בין-שירותיים הוסרו, FTS→LIKE) + 2 DTOs + BL+controller חדשים (ExportDocumentAuthenticationRequest); העשרה: Country lookup + Customer proxy (x2: foreign-customs-house + issuer) |
| 10 | GetAuthenticationRequestByLeadDocumentIDs (TVP) | Internal | ✅ הומרה (branch `feature/migrate-get-authentication-request-by-lead-document-ids`) — **דפוס TVP חדש**: `@LeadDocumentIDs Shared.IntArray READONLY` דרך Dapper `AsTableValuedParameter` (POST+FromBody List<int>); SP dbo.GetAuthenticationRequestByLeadDocumentID (3 JOINs בין-שירותיים הוסרו) + DTO חדש + endpoint ב-AuthenticationRequest; העשרה: Country + OrganizationUnit lookups |

## גל 3 — קריאות עם proxy יחיד (תבנית proxy)
| # | מתודה | חוזה | סטטוס |
|---|---|---|---|
| 11 | GetCustomerInformation | Internal | ✅ הומרה (branch `feature/migrate-get-customer-information`) — proxy-only (אין SP/DB): מתודת proxy חדשה `ICustomerProxy.GetCustomerInformation(int)` (+mock), CustomerDto הורחב ב-Addresses+CustomerAddressDto, endpoint GET CustomerInformation/{id} ב-ExportDocumentAuthenticationRequest (404 על חוסר) |
| 12 | GetCustomerInformationByCountry | Internal | ✅ הומרה (branch `feature/migrate-get-customer-information-by-country`) — proxy-only (אין SP/DB, כמו #11): מתודת proxy חדשה `ICustomerProxy.GetCustomersByCountry(int)` (+mock) עם פילטר activity-type קבוע Foreign_customs_house=40 baked-in, מחזירה `List<CustomerDto>?`; BL `GetCustomerInformationByCountry` (404 על ריק, מחזיר `.First()` — פאריטי עם FirstOrDefault הלגאסי), endpoint GET CustomerInformationByCountry/{countryId} ב-ExportDocumentAuthenticationRequest |
| 13 | LoadDataFromExportDeclaration (DealFile/mock) | Internal | ✅ הומרה (branch `feature/migrate-load-data-from-export-declaration`) — proxy-only: `IExportDealFileProxy` חדש (+real+mock, `CustomsMicroServices.ExportDealFile` מאומת; שירות ExportDealFile טרם הוקם → mock דיפולט בפועל, `TODO(blocking)` על route). DTO תגובה `ExportDeclarationDetailsDto` + DTO בקשה `LoadDataFromExportDeclarationRequestDto` + enum `ERequestReason` (מלא, מ-DB). מתודת BL על `CertificateOfOriginsBl` מחזירה **bool בלבד** (החלטת מפתח — ב-WCF שינתה entity by-ref; שדות ההעשרה מחושבים פנימית). endpoint GET LoadDataFromExportDeclaration ([FromQuery]) |
| 14 | GetExportDocumentAuthenticationRequestByID | Internal | ✅ הומרה (branch `feature/migrate-get-export-document-authentication-request-by-id`) — קריאת EF ישירה (ללא SP): onboarding של **4 טבלאות** מה-EDMX (`ExportDocumentAuthenticationRequest` + 3 בנים: CustomsItem/LeadDocument/ManufacturingArea) עם ישויות + `[ForeignKey]` מפורש (שמות FK שונים) + DbSet ראשי + YAML. DAL דרך ReadOnlyContext עם `.Include` ל-3 האוספים (=LoadProperty הלגאסי). BL 404 על חוסר + מיפוי ל-`GetExportDocumentAuthenticationRequestByIdResultDto` (כולל `OriginalStatusId` snapshot + `ExportDeclarationIds` שמחליף את EntityTypeAndIDsToSearch של הלקוח הישן — החלטת מפתח). endpoint GET ExportDocumentAuthenticationRequestByID/{id} |
| 15 | Convert (תלוי ב-#7) | External | ✅ הומרה (branch `feature/migrate-convert`) — דפוס ESB/EAI Convert: `ConnectedEntityDto` (entityIdKey1=מספר תעודה) → שימוש חוזר ב-`GetCertificateOfOriginsByFilter` (#7) → `VirtualEntityDto` מינימלי (4 שדות: Id, Title=Name, EntityType=12319=EEntityType.CertificateOfOrigin, CustomerId=CustomesAgentId). 404 על חוסר (RestNotFoundException). endpoint POST Convert. אין SP/DAL חדש. אומת: 200 + 404 (ה-happy path דורש x-mock-proxy כי ההעשרה מפנה ל-Customers proxy) |
| 16 | GetPathsForNavigationToVendor | Internal | ⏭️ דולג (2026-07-27) — לא הומר בהחלטה. חוצה-DB: קורא `Infrastructure.vw_General_NavigationPath` (DB `Customs_Pilot`, חיבור `InfrastructureORM`) — לא ה-DB של המודול, ושובר "מיקרו-שירות אחד DB אחד". בנוסף זו תשתית ניווט-UI של הלקוח הישן (PathID=359 קשיח, שמות דפים/מסכים) שה-SPA לא צורך. לברר מוצר/ארכיטקטורה עם הצוות אם ה-SPA צריך זאת ומאיזה שירות. ראה MIGRATION-NOT-DONE.md:87-92 |

## גל 4 — קריאות multi-result-set
| # | מתודה | חוזה | סטטוס |
|---|---|---|---|
| 17 | GetCertificateOfOriginById (7 sets) | Internal | ✅ הומרה (branch `feature/migrate-get-certificate-of-origin-by-id`) — SP רב-תוצאות `dbo.GetCertificateOfOriginByID` (7 result sets) דרך `QueryMultipleAsync` ב-DbContextExtension + מיזוג (type-code→details, items→invoices) שמשחזר את `MaterializeForCertificateOfOrigin` (ה-`AcceptChanges` של WCF STE נושר). 7 DTOs חדשים (root `CertificateOfOriginDto` + 6 nested). result set 7 (Milestones): הוסרו ה-JOINs החוצי-שירות ל-`Infrastructure.UserMng_User`, מוחזר `UserId` גולמי, ו-`UserName` מועשר ב-BL דרך `IUserProxy` חדש (+real+mock, `CustomsMicroServices.Users`; endpoint `User/UsersByIds` לא מאומת → mock דיפולט, TODO(blocking)). +N prefix לליטרלי ActionName העבריים. endpoint GET CertificateOfOriginById/{id} (404 על חוסר). Scripts: `API_20260727172359 - dbo.GetCertificateOfOriginByID.sql` |
| 18 | GetCertificateRequestByGuid (5 sets + reflection) | Incoming (CertificateOfOriginsIncomingMessageService) | ⬜ |
| 19 | GetEntityDocuments (Documents proxy) | Internal | ⬜ |

## גל 5 — writers עם events (IEventUtil + כתיבה)
| # | מתודה | חוזה | סטטוס |
|---|---|---|---|
| 20 | ChangeStatusAfterDeliverySent | Internal | ⬜ |
| 21 | HandleSendRemindDeliverNotification (→CloseReminderTask) | Internal | ⬜ |
| 22 | HandleImportAuthenticationRequestDeliveryAndReminderForVendorSent | Internal | ⬜ |
| 23 | HandleImportAuthenticationRequestDeliveryForImporterSent | Internal | ⬜ |
| 24 | HandleImportAuthenticationRequestDeliveryReminderForImporterSent | Internal | ⬜ |
| 25 | CreateNewAuthenticationFile | Internal | ⬜ |
| 26 | SaveCertificateOfOriginAttachments (Documents + attachments) | External | ⬜ |

## גל 6 — קריאות multi-set + העשרה (Collateral/Tasks proxies)
| # | מתודה | חוזה | סטטוס |
|---|---|---|---|
| 27 | GetAuthenticationRequestByID | Internal | ⬜ |
| 28 | GetAuthenticationRequestFileByID | Internal | ⬜ |
| 29 | HandleAuthenticationRequestDeliverySent (עוטף את #28) | External | ⬜ |

## גל 7 — Saves עם תשתית הודעות (Notifications)
| # | מתודה | חוזה | סטטוס |
|---|---|---|---|
| 30 | SaveExportDocumentAuthenticationRequest | Internal | ⬜ |
| 31 | SaveImportAuthenticationRequest | Internal | ⬜ |
| 32 | SaveAuthenticationRequestFile | Internal | ⬜ |

## גל 8 — המפלצות (אחרונות)
| # | מתודה | חוזה | סטטוס |
|---|---|---|---|
| 33 | SaveCertificateOfOrigin (DealFile+QR+Documents+trade-agreement+events) | Internal | ⬜ |
| 34 | UpdateCetrificateOfOrigins (reconciler ~335 שורות) | External | ⬜ |
| 35 | GetPC_MSG2280_2281_CertificateOfOriginRequest (כל ה-Save + מנוע ולידציה רפלקטיבי; חייב אחרי #33) | Incoming | ⬜ |
