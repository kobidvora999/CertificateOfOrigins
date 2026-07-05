# מצאי המרה — CertificateOfOrigins (wcf-orchestrate, 2026-07-05)

**סיכום: 35 מתודות בחוזי ה-WCF · ‏26 הומרו ✅ · ‏8 חסומות 🔴 · ‏1 לא נדרשת ⏭️ · ‏0 מוכנות ללא חסם.**
השירות ממוצה עד גבול החסמים — כל התקדמות נוספת דורשת פתיחת אחד המסלולים בסוף המסמך.
פירוט החסמים המלא: [MIGRATION-NOT-DONE.md](MIGRATION-NOT-DONE.md).

## External — ICertificateOfOriginsExternalContract (7)

| מתודה | סטטוס | הערה |
|---|---|---|
| Convert | ✅ הומרה | |
| HandleAuthenticationRequestDeliverySent | ✅ הומרה | |
| GetCertificateOfOriginID | ✅ הומרה | |
| GetGoodsItemCerificateDTO | ✅ הומרה | |
| SaveCertificateOfOriginAttachments | ✅ הומרה | |
| TempSync | ⏭️ לא נדרש | stub מת (`NotImplementedException`) |
| UpdateCetrificateOfOrigins | 🔴 חסומה | הודעות + template + DealFile — כל 5 הענפים |

## Internal — ICertificateOfOriginsInternalContract (26)

| מתודה | סטטוס | הערה |
|---|---|---|
| GetCertificateOfOriginsByFilter | ✅ הומרה | |
| IsCertificateOfOriginByExternalIdExist | ✅ הומרה | |
| GetCertificateOfOriginById | ✅ הומרה | |
| GetAuthenticationRequestByFilter | ✅ הומרה | |
| GetEntityDocuments | ✅ הומרה | |
| CreateNewAuthenticationFile | ✅ הומרה | |
| GetAuthenticationRequestFileByID | ✅ הומרה | |
| GetAuthenticationRequestByID | ✅ הומרה | |
| GetExportDocumentAuthenticationRequestSearch | ✅ הומרה | |
| GetExportDocumentAuthenticationRequestByID | ✅ הומרה | |
| GetCustomerInformation | ✅ הומרה | |
| GetCustomerInformationByCountry | ✅ הומרה | |
| HandleImportAuthenticationRequestDeliveryForImporterSent | ✅ הומרה | |
| HandleImportAuthenticationRequestDeliveryReminderForImporterSent | ✅ הומרה | |
| HandleImportAuthenticationRequestDeliveryAndReminderForVendorSent | ✅ הומרה | |
| CheckIfExistsAdditionalRequestsForImporter | ✅ הומרה | ⚠️ חתימה שונתה (entity → 4 סקלרים) — בבדיקת פאריטי |
| CheckIfExistsAdditionalRequestsForVendor | ✅ הומרה | |
| HandleSendRemindDeliverNotification | ✅ הומרה | שם ה-BL שונה מהחוזה (CloseReminderTask) |
| ChangeStatusAfterDeliverySent | ✅ הומרה | |
| CheckImporterOfImportAuthentication | ✅ הומרה | |
| LoadDataFromExportDeclaration | ✅ הומרה (2026-07-05) | Mock ב-DI; ‏TODO(blocking): מעבר ל-real כשיוקם ExportDealFile |
| SaveCertificateOfOrigin | 🔴 חסומה | הכרעת מוצר (original-values) + הודעות + template |
| SaveImportAuthenticationRequest | 🔴 חסומה | תשתית הודעות (Notifications) |
| SaveAuthenticationRequestFile | 🔴 חסומה | תשתית הודעות + מנגנון delta + פערי ICollateralProxy |
| SaveExportDocumentAuthenticationRequest | 🔴 חסומה | תשתית הודעות + AttachDocumentsToEntity + DisplayName |
| GetPathsForNavigationToVendor | 🔴 חסומה | cross-DB (טבלת תשתית) — לברר רלוונטיות ל-SPA |

## Incoming — ICertificateOfOriginsIncomingMessageContract (2)

| מתודה | סטטוס | הערה |
|---|---|---|
| GetPC_MSG2280_2281_CertificateOfOriginRequest | 🔴 חסומה | תלויה ב-SaveCertificateOfOrigin + תשתית EAI + מנוע ולידציה |
| GetCertificateRequestByGuid | 🔴 חסומה | "מוכנה להמרה מהירה" אחרי אימות SP + הכרעות (הקרובה ביותר לפתיחה) |

*(אין עדיין Incoming controller ברפו.)*

## מסלולי פתיחה (לפי סדר מומלץ)
1. **GetCertificateRequestByGuid** — אימות ה-SP הפרוס + 4 הכרעות נקודתיות → המרה מהירה.
2. **GetPathsForNavigationToVendor** — בירור מוצר בלבד (רלוונטי ל-SPA?).
3. **גל ההודעות** — התקנת `CustomsCloud.Infrastructure.Notifications` + אימות SendMessage פותחת את שלוש מתודות ה-Save יחד.
4. **SaveCertificateOfOrigin** → פותחת אחריה את ה-Incoming הראשי.
5. **UpdateCetrificateOfOrigins** — אחרונה (תלויה בכל השאר).
