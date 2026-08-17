# INTERNAL_INTEGRATION — CertificateOfOrigins
עדכון אחרון: 2026-08-17 · Branch/Commit: master @ b2d63fa

> נוצר ע"י `repo-complete-check`. הרשת הפנימית ללא Claude — כל מה שנדרש לאינטגרציה חייב להיות כאן.
> **מצב המרה:** 34/36 אופרציות הומרו (2 מושמטות בכוונה: TempSync, GetPathsForNavigationToVendor).
> **לפני קידום:** ראה `Postman/internal-workload-test/reports/pre-internal-gap-report_2026-08-17.md` — יש 2 חוסמים אדומים.

## 1. שירותים שהמיקרו-סרוויס צורך (outbound proxies)
כל ה-proxies רשומים ב-`ServicesConfiguration.cs` עם `AddProxy<I, Real, Mock>` — **ברירת מחדל = Mock** (מגודר בכותרת
`x-mock-mode`) עד שהשירות האמיתי מוקם/מאומת. כל שורה נושאת `TODO(blocking)` בקוד לאימות נתיב ה-endpoint.

| Proxy | שירות יעד | endpoint (לאימות) | פעולה בפנים |
|---|---|---|---|
| ICustomerProxy | Customers | `Customer/CustomersByIds` | הפנה ל-REST אמיתי + אמת route |
| IVendorProxy | Vendors | `Vendor/VendorsByIds` | הפנה + אמת route |
| IUserProxy | Users | `User/UsersByIds` | הפנה + אמת route |
| IExportDealFileProxy | ExportDealFile | (כמה) | **השירות טרם קיים** — הקם או עטוף WCF קיים |
| IDataDictionaryFieldProxy | SystemTables | `DataDictionaryField/...ByIds` | אמת route |
| ICurrencyTypeProxy | SystemTables | `CurrencyType/CurrencyTypesByIds` | אמת route |
| ICountryProxy | SystemTables | `Country/CountriesByAlphaCodes` | אמת route |
| ISiteProxy | SystemTables | `Site/SitesByExternalNumbers` | אמת route |
| IInternationalSiteProxy | SystemTables | `InternationalSite/InternationalSitesByLocodes` | אמת route |
| IPackingTypeProxy | SystemTables | `PackingType/PackingTypesByCodes` | אמת route |
| IMeasurementUnitProxy | SystemTables | `MeasurementUnit/MeasurementUnitsByCodes` | אמת route |
| ICountryGroupProxy | SystemTables | `CountryCountryGroup/...` | אמת route |
| IDocumentsProxy | Documents | `Document/DocumentsByEntity`, `AttachDocumentsToEntity` | הפנה + אמת route |
| ICollateralProxy | Collateral | `Collateral/CollateralRequestByEntity` | הפנה + אמת route |
| ITasksProxy | Tasks | `Task/IsTaskExist` | הפנה + אמת route |
| IMessageManagementProxy | Message-Management | `Message/SendMessage` | הפנה + אמת route |
| ICustomsBookProxy | CustomsBook | trade-agreement | הפנה + אמת route |
| ICommonServicesProxy | Common | `GenerateTemplate` (SSRS) | הפנה + אמת route |
| IOrganizationUnitProxy | OrgUnit | `IsOrganizationUnitCustomsHouse` | הפנה + אמת route |

## 2. MockProxies להחלפה בפנים
כל ה-proxies בטבלה 1 רשומים עם MockProxy. בפנים: לוודא ש-`x-mock-mode` **אינו** נשלח (כדי לפגוע בפרוקסי האמיתי),
ולעבור על 62 ה-`TODO(blocking)` בקוד (grep `TODO(blocking`) — כל אחד מסמן route/ערך לאימות לפני מעבר ל-real.

## 3. צרכנים של השירות (inbound — מי קרא ל-WCF הישן)
לכל endpoint חדש יש להפנות את הצרכן הישן (שקרא ל-WCF) ל-REST החדש. **רשימת הצרכנים המלאה — לבירור בפנים**
(תלוי בקוד המונוליט שאינו ברפו זה). ה-endpoints החדשים תחת 3 controllers: `CertificateOfOrigins`,
`AuthenticationRequest`, `ExportDocumentAuthenticationRequest`.

## 4. DB — סקריפטים ו-ROLLOUT
- **סקריפטים ב-`API/CustomsCloud.CRM.CertificateOfOrigins.WebApi/Scripts/`** (30 קבצים, בסדר חותמת-זמן `API_<ts>`).
- ✅ **כל 9 ה-SPs שהקוד מפעיל מסוקרפטים** (GetCertificateOfOriginsByFilter / ByID / Number / DataForWebQuery,
  GetImportAuthenticationRequestByFilter, ExportDocumentAuthenticationRequestSearch,
  GetAuthenticationRequestByLeadDocumentID, CheckIfExistsAdditionalRequestsFor{Vendor,Importer}).
- ℹ️ 6 SPs `dbo` שהופיעו ב-localhost הם **orphans** (הקוד מימש inline; ראה gap-report) — **אינם נדרשים**.
- **הבדיקה המוסמכת:** `/db-scripts-check CertificateOfOrigins` (from-zero replay על DB נקי) — מומלץ לפני קידום.
- צ'ק-ליסט rollout: הרצת סקריפטים → תקופת קוד-חדש-על-DB-ישן → יום ROLLOUT (העתקת נתוני הטבלאות ל-DB החדש).

## 5. חוסרים פתוחים שחוסמים אינטגרציה (מדוח 6a)
1. ✅ סקריפטי SP — **תקין** (סעיף 4): כל ה-SPs בשימוש מסוקרפטים; 6 ה-orphans אינם נדרשים.
2. 🔴 אוספי Postman workload (Group 1 + Group 2) חסרים — `/internal-workload-test` + `/dependency-workload-test`.
3. 🟠 קוד אינו warnings-clean — `/net10-code-review`.
4. 🟠 פאריטי fan-out מלא (34 מתודות) לא רץ — מומלץ למתודות Save*/Update* בסיכון גבוה.
5. 🟡 62 `TODO(blocking)` — אימות routes/ערכים במעבר mock→real.
