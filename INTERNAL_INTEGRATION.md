# INTERNAL_INTEGRATION — CertificateOfOrigins
עדכון אחרון: 2026-08-20 · Branch/Commit: `master` @ `fcaab2f`

ברשת הפנימית אין Claude — כל הידע לאינטגרציה חייב להיות ברפו. מסמך זה מרכז את מה שצריך לעשות בפנים.

## 1. שירותים שהמיקרו-סרוויס צורך (outbound) — 19 proxies
כל ה-proxies רשומים בתבנית `AddProxy<I, Real, Mock>` ורצים כרגע ב-**Mock**. בפנים יש לאשר endpoint ולעבור ל-real.

| Proxy | שירות יעד (CustomsMicroServices) | פעולה בפנים |
|---|---|---|
| CustomerProxy | Customers | לאשר endpoint → real |
| VendorProxy | Vendors | לאשר endpoint → real |
| UserProxy | Users | לאשר endpoint → real |
| ExportDealFileProxy | ExportDealFile | לאשר endpoint → real (היה חסום; Mock עד שיוקם) |
| DocumentsProxy | Documents | לאשר endpoint → real |
| CollateralProxy | Collaterals | לאשר endpoint → real |
| TasksProxy | Tasks | לאשר endpoint → real |
| CommonServicesProxy, MessageManagementProxy | Common | לאשר endpoint → real |
| DataDictionaryFieldProxy, CurrencyTypeProxy, CountryProxy, CountryGroupProxy, SiteProxy, InternationalSiteProxy, PackingTypeProxy, MeasurementUnitProxy, CustomsBookProxy, OrganizationUnitProxy | SystemTables | לאשר endpoints → real |

> **הערה — lookups (ILookupUtil):** Country/City/DocumentType/OrganizationUnit נטענים ע"י ה-resolvers של הפלטפורמה
> ישירות משירותי-המקור (GET `{svc}/lookup/{Type}`), **מחוץ** לשכבת ה-proxy הזו ולא מושפעים מ-`x-mock-mode`. בפנים
> שירותי-המקור קיימים; מקומית משתמשים ב-`tools/local-lookup-stub.js`.

## 2. MockProxies להחלפה בפנים
כל 19 ה-Mock (`*MockProxy` תואם לכל שורה ב-§1) — לכבות ולעבור ל-real לפי זמינות שירות היעד. מנגנון המעבר: header
`x-mock-mode` (‏InfrastructureCore.Proxy 1.10.80+) — בהיעדרו ה-proxies הם real כברירת מחדל.

## 3. צרכנים של השירות (inbound — מי קרא ל-WCF הישן)
**לבירור בפנים.** רשימת הצרכנים המלאה לא ניתנת לגזירה מהרפו. ⚠️ **קריטי:** C9/C10 שינו את החוזה — כל צרכן חייב
לעדכן את ה-proxy שלו (verbs QUERY + routes חדשים). המיפוי המלא ישן→חדש הועבר למפתחת (Tamar) בנפרד.

## 4. DB — סקריפטים ו-ROLLOUT
**סדר הרצה** (מ-`API/CustomsCloud.CRM.CertificateOfOrigins.WebApi/Scripts/`, לפי חותמת זמן):
1. `API_20260715 - create schema.sql`
2. `API_20260715 - create tables.sql`
3. `API_20260715 - seed data.sql`
4. `API_20260716 - add params.sql`
5. פרוצדורות `dbo.*` (9): GetCertificateOfOriginsByFilter, CheckIfExistsAdditionalRequestsFor{Vendor,Importer}, GetImportAuthenticationRequestByFilter, ExportDocumentAuthenticationRequestSearch, GetAuthenticationRequestByLeadDocumentID, GetCertificateOfOriginByID, GetCertificateOfOriginDataForWebQuery, GetCertificateOfOriginNumber (+sequence)
6. `API_20260813 - seed CountryIsrael parameter.sql`

> קבצי `CRM.usp_CertificateOfOrigins_*` הם הלגסי המקורי (רפרנס); הקוד קורא לעותקי `dbo.*`.

**צ'ק-ליסט ROLLOUT:** הרצת סקריפטים בפנים → הרצת הקוד החדש על ה-DB הישן (תקופת הרצה מקבילה) → יום ROLLOUT
(העתקת נתוני הטבלאות ל-DB החדש). ⚠️ **from-zero replay לא הורץ** — מומלץ `/db-scripts-check` לפני פריסה.

## 5. חוסרים פתוחים שחוסמים אינטגרציה (מדוח 6a)
- **62 `TODO(blocking)`** — מעבר Mock→real (‏§1–2). חוסם אינטגרציה אמיתית, לא פריסה.
- **שדות deferred (null):** DocumentId (web-query, Docs חוצה-סכמה), LeadDocumentSubmissionDate/Document ב-GetAuthenticationRequestByIdResultDto.
- **`GetPathsForNavigationToVendor` — הומרה חלקית עם `TODO(blocking)`** (2026-08-20, הוכרע Lookup): endpoint חי
  `GET AuthenticationRequest/PathsForNavigationToVendor` + BL + DTOs, אך מחזיר `ViewPaths` ריק עד שהפלטפורמה תוסיף
  טיפוס Lookup בשם `NavigationPath` ל-`InfrastructureCore.Lookup` + שירות-מקור יחשוף `GET /lookup/NavigationPath`
  (שניהם **בפנים**). חיווט סופי: לבטל את ההערה `services.AddLookup<NavigationPath>()` ב-ServicesConfiguration
  ולהחליף את גוף ה-BL בשליפת `lookupUtil.Search<NavigationPath>(p => p.PathId == 359)` (המיפוי שמור כהערה ב-BL).
- **אופרציה מושמטת נוספת:** `TempSync` — stub מת בלגסי (NotImplementedException), לא נדרשת.
- **rollout endpoints לאישור:** SystemTables — CurrencyTypesByIds, DataDictionaryFieldsByIds.
