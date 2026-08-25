# מצאי המרה — CertificateOfOrigins

> ## ✅ ההמרה הושלמה (2026-08-17)
>
> מצאי אדוורסרי מלא (הצלבת 3 חוזי ה-WCF — 36 אופרציות — מול ה-controllers + ה-BL על master):
> **35/36 הומרו** עם endpoint חי + BL אמיתי. `GetPathsForNavigationToVendor` הומרה חלקית (2026-08-20, הוכרע Lookup):
> endpoint + BL + DTOs קיימים, מחזיר ViewPaths ריק עם `TODO(blocking)` עד שהפלטפורמה תוסיף טיפוס Lookup בשם
> `NavigationPath` + שירות-מקור שיחשוף `GET /lookup/NavigationPath` (בפנים — ראה INTERNAL_INTEGRATION.md).
> מושמטת אחת בלבד: `TempSync` (stub מת בלגסי, NotImplementedException — לא נדרשת).

> ## ⚠️ קרא קודם — עדכון סטטוס (2026-07-22)
>
> **הרפו אופס ל-scratch slate** (commit `5fce824` "Reset service to a clean scratch slate for from-scratch migration").
> כל עבודת ההמרה שקדמה ל-reset **נמחקה מ-master** — ההמרה מתבצעת עכשיו **מאפס**.
>
> לכן: **הטבלאות שלמטה (המסומנות ✅ הומרה / 🔴 חסומה מ-2026-07-05) הן מצאי-הלגאסי = רשימת היעד** —
> מה קיים במונוליט WCF שצריך להמיר, ומה חסום שם. הן **אינן** מד-התקדמות של המאמץ הנוכחי.
>
> ### מה הומר בפועל על master (post-reset) — מקור-האמת להתקדמות
>
> | # | Commit | מתודה | חוזה | Controller |
> |---|---|---|---|---|
> | 1 | `f3fe10d` | GetCertificateOfOriginID | External | CertificateOfOrigins |
> | 2 | `ff5b089` | IsCertificateOfOriginByExternalIdExist | Internal | CertificateOfOrigins |
> | 3 | `db8b09e` | CheckImporterOfImportAuthentication | Internal | AuthenticationRequest |
> | … | (ראה git log) | רוב מתודות ה-Get/Save/Handle | External/Internal | CertificateOfOrigins / AuthenticationRequest / ExportDocumentAuthenticationRequest |
> | ~28 | `a750038`+ | GetPC_MSG2280_2281_CertificateOfOriginRequest | Incoming | CertificateOfOrigins (✅ הומרה במלואה — ראה למטה) |
>
> **הטבלה הזו אינה ממצה** — מאז נוספו עוד מתודות רבות (ראה `git log`); שורת ה-GetPC נוספה כי היא ה-work הנוכחי.
> **בפועל: 3 controllers, עשרות endpoints מומרים.**
> תשתית משותפת שכבר קיימת אך **טרם חשופה כ-endpoint**: מתודת ה-BL `GetCertificateOfOriginsByFilter`
> (+ SP `dbo.GetCertificateOfOriginsByFilter` + DAL + `CertificateOfOriginResultDto` + Customer proxy) —
> נוצרה כתשתית ל-#2; ההמרה המלאה שלה (עם endpoint) היא #7 בתוכנית.
> כל שאר המתודות שמסומנות ✅ למטה (מ-2026-07-05) **צריכות המרה מחדש**.
>
> **מתכונת עבודה מסודרת:** מתודה אחת בכל פעם, במאגר הראשי (לא ב-worktrees מקבילים), **commit בסוף כל מיגרציה**.
> עדכן את הטבלה הזו אחרי כל commit.

---

## מצאי-הלגאסי (wcf-orchestrate, 2026-07-05) — רשימת היעד

**סיכום המונוליט: 35 מתודות בחוזי ה-WCF · ‏26 קיימות/הומרו-לפני-reset · ‏8 חסומות 🔴 · ‏1 לא נדרשת ⏭️.**
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
| GetExportDocumentAuthenticationRequestSearch | ✅ הומרה | SP→LINQ (החלטת מפתח, נאמן ל-CROSS_ SP); שמירת row-membership של 3 ה-INNER JOIN החוצי-שירות (CountryID/CustomerID/ExporterCustomerID); אומת חי 6/6 (2b670b5) |
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
| GetPathsForNavigationToVendor | ✅ הומרה חלקית | 2026-08-20 (הוכרע Lookup): endpoint `GET AuthenticationRequest/PathsForNavigationToVendor` + BL + DTOs; מחזיר ViewPaths ריק עם `TODO(blocking)` עד הוספת טיפוס Lookup `NavigationPath` בפלטפורמה + endpoint מקור (בפנים) |

## Incoming — ICertificateOfOriginsIncomingMessageContract (2)

| מתודה | סטטוס | הערה |
|---|---|---|
| GetPC_MSG2280_2281_CertificateOfOriginRequest | ✅ הומרה | 2026-08-17. כל 4 החסמים העסקיים נסגרו: per-reason resolution+supersession · שמירת invoices/items · declaration-check+amendment · NonManipulation (מיפוי 15 שדות + CustomsHouse). אודיט פאריטי אדוורסרי מול הלגסי + אימות DB; תוקנו org-unit-0 באירועים ו-`EnrichAndValidateDetails` (תאריכים→short date · בוליאני→Yes/No · CustomsHouse→org-unit id+שם). פירוט: [MIGRATION-NOT-DONE.md](MIGRATION-NOT-DONE.md) |
| GetCertificateRequestByGuid | ✅ הומרה | 2026-07-28. נחשפה כ-GET ב-CertificateOfOriginsController (לא Incoming controller — הקונבנציה: controller לפי BL). SP רב-תוצאות `dbo.GetCertificateOfOriginDataForWebQuery` (5 result sets, QueryMultiple ידני) — הוחל ואומת מול DB חי + סקריפט גרסה ב-Scripts/. 3 quirks לגאסי נשמרו bug-for-bug (קדימות פילטר חשבוניות · result-set-5 בלי IsToPrint → Consignee EUR1/EURMED לא מודפס · רשימת פריטי חשבונית תמיד ריקה). QueryURL נפתר מ-Infrastructure.Parameters (קיים ומאומת). CurrencyCode ו-DataDictionaryField labels נפתרים דרך proxy ל-SystemTables (+mock) — אין להם lookup type בפלטפורמה. נבדק חי (GET מול השירות): GUID אמיתי→200 עם currencyCode; GUID לא קיים→200 עם exceptionDescription. חוסם שנותר: DocumentID (NULL — Docs חוצה-סכמה, TODO(blocking)). Rollout: אימות נתיבי endpoint של SystemTables (CurrencyTypesByIds, DataDictionaryFieldsByIds). |

*(אין עדיין Incoming controller ברפו — מתודות Incoming שהומרו נחשפות דרך ה-controller של ה-BL הרלוונטי.)*

## מסלולי פתיחה (לפי סדר מומלץ)
1. ~~**GetCertificateRequestByGuid**~~ — ✅ הומרה (2026-07-28).
2. ~~**SaveCertificateOfOrigin**~~ — ✅ הומרה.
3. ~~**UpdateCetrificateOfOrigins**~~ — ✅ הומרה.
4. ~~**GetPC_MSG2280_2281**~~ — ✅ הומרה במלואה (2026-08-17). כל 4 החסמים העסקיים נסגרו (per-reason+invoices · declaration-check+amendment · NonManipulation).
5. ~~**GetPathsForNavigationToVendor**~~ — ✅ הומרה חלקית 2026-08-20 (Lookup + `TODO(blocking)`; חיווט סופי בפנים — ראה INTERNAL_INTEGRATION.md).
