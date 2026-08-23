# Conventions State

aligned-through: C12
date: 2026-08-19
notes: >
  כל C6–C12 יושרו/אומתו ב-2026-08-19 (build נקי + בדיקות 2/2). C9+C10 (שינויי-wire) יושמו לפי אישור המשתמש
  ובאחריותו לתאם את השירותים הקוראים (עדכון ה-proxies ל-.UseQueryMethod()/routes החדשים). Postman: אוסף
  ה-internal-workload v3 ואוסף ה-dependency-workload עודכנו מלא (routes + method QUERY + body ל-4 בקשות C9).
  האוסף היחיד-קובץ הישן Postman/CertificateOfOrigins.postman_collection.json (הקודמן שממנו נגזר ה-v3) נמחק
  ב-2026-08-19 אחרי שאומת שה-v3 מכסה 100% מ-34 endpoints של הקונטרולרים (37 בקשות ↔ 37 קבצי v3) ושאין
  לו הפניה מ-runner. ה-v3 הוא כעת האוסף היחיד ל-internal-workload.

history:
- C1 (2026-07-22): כבר תאם — bindings מפורשים בכל endpoint, ה-BL זורק RestNotFoundException ל-route-key not-found, אין GetById-in-query.
- C2 (2026-07-22): תוקן — הוסף הבלוק הקנוני של CustomsCloud ל-.gitignore + סעיף "File Hygiene" ל-CLAUDE.md.
- C3 (2026-07-22): כבר תאם — ה-controllers משתמשים ב-ModelDTOs בלבד (אין ישות Model/*Db חוצה גבול), אין mapper ידני.
- C4 (2026-07-22): תוקן — CustomerProxy/Mock עברו לתבנית AddProxy<ICustomerProxy, CustomerProxy, CustomerMockProxy> (REAL כברירת מחדל, mock דרך x-mock-proxy), CustomerMockProxy מממש IMockProxy ומקבל IProxyMockUtil, AddHttpProxy+AddRestProxy ב-DI, prerequest ברמת האוסף ב-Postman. אומת חי.
- C5 (2026-07-22): N/A — אין מסלול העלאת צרופה נכנסת מומר בשירות עדיין.
- C6 (2026-08-19): כבר תאם — כל ה-proxies (19 קבצים) מזריקים IHttpProxy עם ToCustomsService/Build/ExecuteAsync + GetResult<T>() + response.Validate; אין IRestProxy/BaseMicroServiceProxyAdapter/ExecuteAsync</.Data/ValidateResponse/ProxyMocking.cs/Lookup.Entities/CreatRequestBuilder.
- C7 (2026-08-19): N/A — אין stub של ValidationMessages ואין #if VALIDATION_MESSAGES_INFRA; השירות משתמש במודל שגיאה in-band ייעודי (EMessageCode catalog ב-CertificateOfOriginsBl.MessageValidationMessages.cs) ולא ב-BaseValidationMessages. אין מה להפעיל.
- C8 (2026-08-19): N/A — אין תיקיית _seed ב-repo, ו-headers של mock-feature כבר בפורמט החדש (x-mock-feature--{key}); אין DistributedResolver./Lookup.Entities ואין x-mock-feature dash-יחיד.
- C9 (2026-08-19): תוקן — 4 endpoints של קריאת פילטר עברו מ-[HttpGet]+[FromQuery] ל-[HttpQuery]+[FromBody]: CertificateOfOriginsByFilter, LoadDataFromExportDeclaration, AuthenticationRequestByFilter, ExportDocumentAuthenticationRequestSearch. CertificateRequestByGuid נשאר [HttpGet] (portal חיצוני, lookup לפי מזהה בודד — browsers לא שולחים QUERY). CheckIfExistsAdditionalRequestsForImporter נשאר multi-scalar GET (המרה דורשת DTO חדש; קיבל [BindRequired] במקום, C10). S6965 (SonarAnalyzer לא מזהה HttpQuery כ-verb) הושתק ב-.editorconfig כ-false-positive. build warning-clean.
- C10 (2026-08-19): תוקן — no-repeat naming על כל 3 ה-controllers: by-id → {id} על ה-base route (CertificateOfOrigins/{id}, AuthenticationRequest/{documentId}, ExportDocumentAuthenticationRequest/{id}); הסרת מילת ה-resource מ-segments (ByFilter, Search, RequestByGuid, SaveAttachments, Reconcile, SaveImport, SaveFile, CreateNewFile, File/{fileId}, HandleImportDelivery*, HandleDeliverySent, ByLeadDocumentIDs, ID, ByExternalIdExist, SaveImport, SaveFile); Save*→POST על ה-base (SaveCertificateOfOrigin, SaveExportDocumentAuthenticationRequest); CertificateOfOriginRequest→route "Request" (method נשאר מובחן); [BindRequired] על סקלרים בודדים mandatory (importerId/vendorId/countryId/certificateOfOriginExternalId). SaveCertificateOfOrigin נשאר POST (upsert create+update) ולא PUT/{id}. Postman טרם עודכן (ראה notes).
- C11 (2026-08-19): תוקן — 3 מחלקות BL עברו לבנאי Resolve<T>(): ExportDocumentAuthenticationRequestBl (3 תלויות), AuthenticationRequestBl (7), CertificateOfOriginsBl (18, פרוס על 7 partials, 42 שימושים). הבנאי מזריק רק IServiceProvider+IParametersUtil/ILookupUtil; שאר ה-proxies/utils דרך var x = Resolve<IX>() בראש המתודה (מעל לולאות). בדיקת QR (2/2) עוברת, build נקי.
- C12 (2026-08-19): תאם/N-A — פריטים 2 (הסרת OutgoingMessage), 3 (Utils.Shared→Interfaces.Shared), 4 (bootstrap אסינכרוני: async Main + await CloudWebApp.Build + new DatabaseMigrationUtil(app) + app.RunAsync), 5 (Postman x-mock-mode גלובלי; x-mock-proxy נותר רק בהערות הסבר) — כולם תואמים לאחר עדכון הנוגטים ל-1.10.x (commit 50cadd9). פריט 1 (ICloudEntity save) N/A — ה-repo כותב set-based (ExecuteUpdateAsync) + Context/SaveChangesAsync עם IsModified guards; אין SaveEntitiesAsync/ChangeTrackEntity להמיר.
- C12 re-verified (2026-08-23): אין drift — הקומיטים שאחרי היישור (fcaab2f, 49169fe) נבדקו מול מתכוני ה-Detection של C4/C6/C10/C11/C12, 0 ממצאים. אין delta חדש ב-changelog (C12 הוא ה-entry האחרון).
