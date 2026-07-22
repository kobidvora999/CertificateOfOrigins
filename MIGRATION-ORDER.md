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
| 4 | GetGoodsItemCerificateDTO | External | ⬜ |

## גל 2 — SP סקלרי + חיפוש לפי פילטר (YAML + DbContextExtension + SP)
| # | מתודה | חוזה | סטטוס |
|---|---|---|---|
| 5 | CheckIfExistsAdditionalRequestsForVendor | Internal | ⬜ |
| 6 | CheckIfExistsAdditionalRequestsForImporter | Internal | ⬜ |
| 7 | GetCertificateOfOriginsByFilter | Internal | ⬜ |
| 8 | GetAuthenticationRequestByFilter | Internal | ⬜ |
| 9 | GetExportDocumentAuthenticationRequestSearch | Internal | ⬜ |
| 10 | GetAuthenticationRequestByLeadDocumentIDs (TVP) | Internal | ⬜ |

## גל 3 — קריאות עם proxy יחיד (תבנית proxy)
| # | מתודה | חוזה | סטטוס |
|---|---|---|---|
| 11 | GetCustomerInformation | Internal | ⬜ |
| 12 | GetCustomerInformationByCountry | Internal | ⬜ |
| 13 | LoadDataFromExportDeclaration (DealFile/mock) | Internal | ⬜ |
| 14 | GetExportDocumentAuthenticationRequestByID | Internal | ⬜ |
| 15 | Convert (תלוי ב-#7) | External | ⬜ |
| 16 | GetPathsForNavigationToVendor | Internal | ⚠️ לברר — טבלת תשתית cross-DB, אולי לא רלוונטי ל-SPA |

## גל 4 — קריאות multi-result-set
| # | מתודה | חוזה | סטטוס |
|---|---|---|---|
| 17 | GetCertificateOfOriginById (7 sets) | Internal | ⬜ |
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
