# דוח חוסרים לפני הכנסה לסביבה פנימית — CertificateOfOrigins
תאריך: 2026-09-27 · Scope: `all` (כל 30 פעולות ה-WCF ב-3 החוזים + endpoints נוספים) · Branch/Commit: `master` @ `7615be3`

> ריצה **מאפס** — לא נעשה שימוש בדוחות קודמים (last-run / gap-analysis / pre-internal-gap / check2-parity).
> build, בדיקות יחידה, ריצת coverage, השוואת פאריטי, code review ו-replay של ה-DB — כולם הורצו מחדש בריצה זו.
> פאריטי: 8 סוקרים עצמאיים (לא הממירים), מתודה-מתודה ברמת statement; כל ממצא HIGH אומת מחדש ע"י ה-orchestrator
> מול שני המקורות לפני שנכנס לדוח (ממצא אחד — A-01 — הורד ל-MED אחרי האימות).

## סיכום מנהלים
| בדיקה | סטטוס | חוסרים |
|---|---|---|
| 1. כיסוי חוזים        | ✅ | 0 — 30/30 פעולות ממופות לכל השכבות |
| 2. פאריטי התנהגותי    | ❌ | 11 HIGH · 42 MED · ~85 LOW |
| 3. Code Review        | ❌ | C 1 / M 4 / m 14 · 8 warnings שאינן TODO |
| 4. שלמות סקריפטי DB   | ❌ | 2 חוסמים · 2 Major · 4 Minor |
| 5. Postman workload collections | ❌ | 15+1 אוספי Internal, 9 Dependency נמצאו · 4 אוספי Internal אדומים · 9 probes שבורים · קובץ JSON ישן |

**מוכן להכנסה לסביבה פנימית? לא** — 11 פערי פאריטי HIGH מאומתים (בהם אובדן נתונים במסך התיק ויצירת בקשה שאינה
עובדת), רינדור התבנית החדשה (CR 194221) שבור (double-encode + קובץ תבנית חסר), טבלה שאין לה סקריפט, ו-probes
של dependency שלא יכולים להוכיח דבר.

---

## מעקב סקירה (מתעדכן תוך כדי מעבר ממצא-ממצא)
| # | ממצא | החלטה | סטטוס |
|---|---|---|---|
| — | `DateTimeOffset` ב-64 מקומות (עמודות ה-DB הן `datetime`/`date`) | הוסר מכל השכבות; נוסף שער חוסם בסקילים | ✅ תוקן — `ce4fa8e` |
| 1 | F-01 — יצירת בקשת אימות חדשה החזירה 404 | upsert כמו הלגאסי | ✅ תוקן — `1b13471` |
| 2 | F-02 + E-F3 — עריכות בקשות-הבן במסך התיק לא נשמרו | קריאה + שמירה של שדות המסך; null = לא נוגע | ✅ תוקן — `476feb1` |
| 3 | F-03 — זיכוי/גביית בטוחות לפי סטטוס תיק | **תקין.** החלטת אנליסט: ההתנהגות ב-master מממשת את CR 194221 כנדרש (6 ו-10 משחררים, 8 גובה) | ✅ נסגר כתקין — ללא שינוי קוד |
| 3 | F-04 — "CR 194221 סותר את הכוונה שלו" | נסגר יחד עם F-03 (החלטת אנליסט) | ✅ נסגר כתקין |
| 4 | F-01 (תיקון) — upsert לפי תוצאת DB | חדש/עדכון לפי IsNewInstance בלבד; עדכון שלא מצא שורה = 404 | ✅ תוקן — `b901dc1` |
| 4 | G-F1 — FirstProvideContactDate לא נרשם במשלוח לספק | משלוח (לא תזכורת) רושם את התאריך כשהוא ריק | ✅ תוקן — `4308fe6` |
| — | named arguments ומשתנים חד-פעמיים | הוסרו; כלל נוסף לסקילים | ✅ תוקן — `5bbdebc` |
| 5 | A-02 — אירוע התאמה להצהרה: 1913 בלי משימה ל-4 סיבות + Export org-unit type | שוחזר לפי RaiseEventUtil; 7 בדיקות יחידה; 2 כללים לסקילים (טבלת helpers, enum בלי הפניה) | ✅ תוקן — `419b291` |
| 6 | B-H1 — ה-reconcile מחפש assessor לפי קישור ההצהרה הישן | ה-backfill נכתב על התעודה עצמה (כמו הלגאסי ומסלול ה-release); בדיקת יחידה | ✅ תוקן — `35aa7fd` |
| 7 | B-H2 — ממצאי הפרטים נספרים גם לתעודה בלי שורות חשבונית | **כמו הייצור** (החלטת אנליסט): בלי שורות חשבונית → התאמה | ✅ תוקן — `51b0376` (fix/parity-findings) |
| 8 | B-H3 — ב-release התעודה המוחלפת לא מבוטלת | CertificateIdToCancel נוסף ל-projection של GetCertificatesByIds | ✅ תוקן — `9521a65` (fix/parity-findings) |

## פירוט חוסרים (לפי חומרה)

### 🔴 חוסמים (Blockers)

**פאריטי (CHECK 2) — HIGH, מאומתים**
| # | פעולה | הסטייה | חדש | לגאסי |
|---|---|---|---|---|
| ~~F-01~~ ✅ תוקן | SaveImportAuthenticationRequest | הלגאסי עשה upsert (`Repository.Save`); החדש מעדכן בלבד ומחזיר 404 אם אין שורה — **יצירת בקשה חדשה מהפופאפ לא עובדת**. גרוע מזה: ה-events וה-collaterals רצים **לפני** השמירה → task/collateral יתומים. | `AuthenticationRequestBl.cs:896-937` | `AuthenticationRequestBL.cs` SaveImportAuthenticationRequest (`_uow.Repository.Save`) |
| ~~F-02 + E-F3~~ ✅ תוקן | SaveAuthenticationRequestFile / GetAuthenticationRequestFileByID | מסך התיק: הקריאה לא מחזירה 12 עמודות של בקשות-הבן (CirumstanceDetails, DecisionCircumstences, Remarks, DocumentNumber, IsOldIndication, RequestCircumstancesID, ...) והשמירה כותבת רק DecisionId/IsOldIndication — **עריכות במסך התיק נזרקות** (DecisionCircumstences חובה בשינוי החלטה). | `CertificateOfOriginsDal.cs:536-539`, `AuthenticationFileRequestDto.cs` | Full-save של כל child |
| ~~G-F1~~ ✅ תוקן | Delivery (ChangeStatusAfterDeliverySent / Vendor) | `FirstProvideContactDate` לא נכתב במסלול ה-delivery (הלקוח הלגאסי קבע אותו ל-Today לפני הקריאה). ה-SP של ה-scheduler מחשב את סולם התזכורות מ-`ISNULL(FirstProvideContactDate, LastDelivery)` → התזכורות נמדדות מהתזכורת האחרונה, רמות 6/9/10 חודשים מתעכבות/לא נורות. | `CertificateOfOriginsDal.cs:1033-1047` | `AuthenticationRequestFilePresenter.cs:250-251` |
| ~~A-02~~ ✅ תוקן | SaveCertificateOfOrigin | DeclarationMatch: לסיבות EmptyCertificate/Draft/GetRequestStatus/CertificateCancellation הלגאסי הרים `CertificateMatchDeclarationWithoutTask` (1913, בלי משימה) — החדש תמיד פותח משימה; וגם `OrganizationUnitType=Export` נשמט מ-Match/Mismatch. | `CertificateOfOriginsBl.cs:1383-1409` | `RaiseEventUtil.cs:220-247` |
| ~~B-H1~~ ✅ תוקן | UpdateCetrificateOfOrigins (Reconcile) | קישור ההצהרה נכתב למשתנים מקומיים; חיפוש ה-assessor קורא את הערך הישן מה-DB → תעודה שמקושרת לראשונה: match בלי preferred user, warnings בלי שיוך משימה. | `CertificateOfOriginsBl.cs:1640-1651, 1903, 1956` | `CertificateOfOriginsBL.cs:492-500, 607` |
| ~~B-H2~~ ✅ תוקן | UpdateCetrificateOfOrigins | בדיקות destination/exporter/מספר-הצהרה/import-replacement רצות תמיד; בלגאסי נספרו רק כשיש לתעודה שורות חשבונית → אותה תעודה מסתיימת Rejected/Mismatch במקום DeclarationMatch. (ייתכן באג לגאסי — אבל זו התנהגות הייצור; החלטת מפתח.) | `CertificateOfOriginsBl.cs:2028-2040` | `CertificateOfOriginsBL.cs:772-875` |
| ~~B-H3~~ ✅ תוקן | UpdateCetrificateOfOrigins | `GetCertificatesByIds` לא מקרין `CertificateIdToCancel` → `HandleCertificateReplacement` תמיד חוזר מיד; **התעודה המוחלפת לעולם לא מבוטלת** ולא מקבלת Replaced event. | `CertificateOfOriginsDal.cs:181-200`, `CertificateOfOriginsBl.cs:1529` | `CertificateOfOriginsBL.cs:341-375` |
| C-01 | GetPC_MSG2280_2281 | ביטול תעודה ב-DeclarationMatch/Mismatch (יש משימה פתוחה לעובד מכס) — הלגאסי חסם; ענף הביטול החדש לא מריץ את השמירה (היא קיימת רק בענף היצירה) → מבטל, כותב ל-DB ומרים event. | `CertificateOfOriginsBl.cs:157-178` | `CertificateOfOriginsIncomingMessageServicePartial.cs:1000-1010` |
| C-02 = A-07 | GetPC_MSG2280_2281 / SaveCertificateOfOrigin | הולידטור המשותף דורש `CustomerId>0` ו-`OrganizationUnitId>0`; הלגאסי שמר 0 (EmptyCertificate בלי גוף, NonManipulation, סוגים בלי CustomsHouse) → **400 ושום דבר לא נשמר**. נמצא ע"י שני סוקרים בלתי תלויים. | `SaveCertificateOfOriginRequestValidator.cs:30-36` | `...IncomingMessageServicePartial.cs:1544, 1551` |
| C-03 | GetPC_MSG2280_2281 | החלטת צירוף המסמך לפי ה-reason **של התעודה השמורה** במקום של ההודעה הנכנסת → שאילתת סטטוס על תעודה Published מרנדרת SSRS ושומרת attachment **בכל poll**. | `CertificateOfOriginsBl.cs:263-276` | `...IncomingMessageServicePartial.cs:1708` |

**Code review (CHECK 3)**
- **C1 — רינדור התבנית החדשה (CR 194221) שבור:** `.WithData(printTemplate.Data)` מקבל מחרוזת JSON ומסוריאלז אותה שוב (double-encode) — שדות מכתב דרום-קוריאה ייצאו ריקים. `_shared/template-print-pattern.md:188` אוסר זאת במפורש. תיקון: להעביר את האובייקט הגולמי או `WithJsonData`. `CertificateOfOriginsBl.Templates.cs:31`. (דוגמת הקוד ב-`template-print-pattern.md:131` מכילה את אותו באג.)

**DB (CHECK 4)**
- **טבלה ללא סקריפט:** `CRM.CertificateOfOrigins_CustomsItemToExportDocumentAuthenticationRequest` — ה-DAL כותב אליה (`CertificateOfOriginsDal.cs:686-712`, Save של בקשת יצוא); קיימת ב-localhost רק משאריות. בפריסה נקייה — Save נכשל.
- **קובץ התבנית חסר:** `ExportAuthenticationSendDeliveryForExporterSouthKoreaTemplate.docx` + `.yml` לא קיים ברפו, לא ב-`C:\Repos\Templates\TemplatesBucket`, ולא ב-Main → "template not found" (אלא אם הועלה ידנית ל-MinIO — לא נבדק).

**Postman (CHECK 5)**
- **9 ה-probes של dependency-workload שבורים:** כל ה-URLs חסרי קידומת route (`/AuthenticationRequest/{id}` במקום `/ui/AuthenticationRequest/{id}`, `/CertificateOfOrigins/Request` במקום `/community/...`, `/CertificateOfOrigins/{id}` במקום `/ui/...`) → 404 מול השירות האמיתי; הם לא יכולים להוכיח liveness.
- **ה-assertions של אוספי ה-dependency יושבים ברמת בקשה** (`scripts:` בתוך `*.request.yaml`) ו-`definition.yaml` מכיל 0 `pm.test(` — לפי הסקיל מודל ה-http-request של v3 דוחה זאת (`Failed to load V3 collection`).
- **קובץ JSON פרש עדיין ב-git:** `Postman/CertificateOfOrigins-Internal.postman_collection.json` (commit `46491ae`).
- **4 אוספי Internal אדומים — 14 assertions נכשלו** (774 רצו על 249 בקשות). ניתוח שורש — **אף אחד אינו רגרסיה של קוד**, כולם סחף בדיקות אחרי `f3640f7`/נתונים:
  - *Reconcile Outcomes* (8) + כנראה *Mock Features* (4): `f3640f7` הוסיף `ResolveNewInstanceDetailCodes`; ה-setup שולח ארץ יעד כמספר `"32"` וה-`CountryMockProxy` "פותר" כל קוד ל-id דמה (`CountryMockProxy.cs:23`) → mismatch של ארץ יעד בכל תרחיש. תיקון: ה-setup ישלח קוד alpha-2, וה-mock יחזיר null לקוד לא-alpha.
  - *Per Reason* (1): `70-unknown-reason` מצפה ל-400, אבל מאז `f3640f7` reason לא ידוע נדחה ב-feedback (200, כמו הלגאסי). הבדיקה מיושנת.
  - *Auth File Status* (1): `70-administrative-closure` — הבנים 990105/990106 נותקו מהתיק (`AuthenticationFileID=NULL`) ע"י `90-cancelled` בריצה קודמת; ה-runner לא מריץ את `seed/seed_ImportAuthenticationRequests.sql` → האוסף אינו חזרתי.

### 🟠 לטיפול לפני merge (Major)
- **פאריטי MED (42)** — פירוט מלא בנספחים. הבולטים:
  - A-01 (הורד מ-HIGH): ApplicationCorrected נורה על `previousCertificateId != 0` ולא על `Received && CertificateUpdate` (`CertificateOfOriginsBl.cs:987-989`); C-06: האירוע השני שמקשר לתעודה המוחלפת נשמט.
  - A-03: שורות details שלא נשלחו נמחקות פיזית — רשימה ריקה מוחקת הכל (`CertificateOfOriginsDal.cs:128-139, 730-742`).
  - A-06: "status changed" נשען על `OriginalStatusId` שנשלח מהלקוח (ברירת מחדל 0) → re-publish ללא guard.
  - A-08 / B-M6: ביטול בהחלפה מאפס `IsLastVersion`.
  - A-11 / E-F1: `TOP (shared.ufn_GetMaxRows())` → `TOP (200)` קשיח (לא אומת).
  - F-05: OrganizationUnitId של התיק מגיע מהלקוח; קריאת התיק לא מחזירה אותו → events עם 0.
  - C-12: `CustomerId` של הסוכן נלקח מגוף ההודעה ולא מה-header המאומת (ייתכן spoofing).
  - D-F1/F2/F3: שינוי שמות שדות תשובה (`ExeptionDescription`), תאריכים ISO במקום `yyyy-MM-dd`, parse תרבותי של DateOfDeclaration.
  - H-2: `[Timestamp]` חדש על בקשת יצוא → 409 היכן שהלגאסי קיבל last-writer-wins. H-3: Save לא אטומי.
  - G-F3: כתיבות ה-delivery לא מעדכנות `UpdateUserId`.
  - B-M1: שגיאת "אין חשבוניות יצוא" חדשה נכתבת בכל reconcile ממסלול ה-release.
- **Code review Major:** M1 — proxies הופכים 404 ל-500 (`ExecuteAsync` זורק; טיפול ה-null ב-BL לעולם לא רץ); M2 — 19 routes ב-string interpolation ללא URL-encoding; M3 — `SendRequestFeedback` שולח `MessageTypeId=0` לפרוקסי האמיתי אחרי commit (`CertificateOfOriginsBl.cs:1427`); M4 — חוזה ה-endpoint `GET api/CertificateOfOrigins/Template/{templateId}/{entityId}` לא מאושר מול צרכן ההפצה, templateId=1 `TODO(confirm)`.
- **Build:** 0 errors, 355 warnings — 173 S1135 (מותר) + 174 REVIEW00x (סמני סקירה מכוונים של `tools/review-markers.ps1`) + **8 לתיקון**: S125×5 (`CertificateOfOriginsBl.cs:250,1726,1765`, `ServicesConfiguration.cs:111` — חדש ב-7615be3, `EEventType.cs:96`), S3267×1 (`CertificateOfOriginsBl.cs:1703`), CS8601×2 (`SaveCertificateOfOriginQrCodeTests.cs:152-153`).
- **DB Major:** `API_20260715 - create tables.sql` אינו idempotent (בלוק FK בסוף — Msg 2714 בהרצה שנייה); אין מנגנון פריסה ל-bucket התבניות (`tools/upload-templates.ps1` לא קיים).
- **Coverage:** `Api/CertificateOfOriginsController` 84% line (<90%) — מסלול ה-render של התבנית לא מכוסה (אין happy-path מכוון, `ITemplateUtil` אינו proxy ולא ממוקק).
- **Postman:** לא בוצע מבחן מוטציה על האוספים בריצה זו (לא הוכח שה-gate מבחין).

### 🟡 לתשומת לב (Minor / Deferred)
- **מצאי TODO:** 168 מופעים (0 FIXME/HACK/XXX, 0 PushUtil לא מסומן). 65 `TODO(blocking)` בני-ביצוע: 6 פערים פונקציונליים (`CertificateOfOriginsBl.cs:1012,1036,1414,1427,1896`, `CommonServicesProxy.cs:24`), 3 infra/no-source (`AuthenticationRequestBl.cs:1400`, `ServicesConfiguration.cs:131`, `Program.cs:20`), 2 חוזי DTO (`CollateralRequestDto.cs:4`, `UserDto.cs:11`), 13 switch-to-real (`ServicesConfiguration.cs:33-92`), 41 confirm-endpoint. 6 `TODO(confirm)`. 0 hardcoded `=1`. טבלה מלאה בנספח CHECK 3.
- **7615be3 Minor:** controller מחזיר `application/pdf` בלי קשר ל-Format; reflection+`dynamic` `.Result`; ה-stream של Templates לא משוחרר; `LetterDate` יודפס כ-ISO גולמי; ה-SP מקודד את ערך ה-enum 1.
- **DB Minor:** 5 שמות סקריפט לא בפורמט (`API_20260715`, `API_20260716`, `API_20260721`, `API_20260722`, `API_20260722b`) — הסדר הלקסיקלי עדיין נכון; 26 פרוצדורות + `CRM.General_enum_UIMessage` + 2 עמודות (`IsCreateAttachments`, `IsMessageSent`) קיימות ב-localhost ללא סקריפט וללא שימוש בקוד; ערכי פרמטר שונים בין DB לסקריפט: `CertificateOfOriginQueryURL`, `CertificateOfOriginsDocumentsFilter` — לבירור איזה נכון.
- **Collateral:** הארגומנט `true` ב-`ChangeTempCollateralRequest` נשמט (F-08) — CRP שולחים `false`; לברר מול Collaterals מה ברירת המחדל.
- **LOW פאריטי:** ~85 פריטים (דחיות מסומנות TODO, שינויי חוזה 404-במקום-null, טקסטים) — בנספחים.

---

## נספח: פירוט מלא לכל בדיקה

### CHECK 1 — כיסוי חוזים (30/30 ✅)
| חוזה | פעולת WCF | Endpoint | BL | DAL | סטטוס |
|---|---|---|---|---|---|
| External | UpdateCetrificateOfOrigins | `POST api/CertificateOfOrigins/Reconcile` | CertificateOfOriginsBl | ✅ | ✅ (פערי פאריטי) |
| Incoming | GetPC_MSG2280_2281_CertificateOfOriginRequest | `POST community/CertificateOfOrigins/Request` | CertificateOfOriginsBl.Message* | ✅ | ✅ (פערי פאריטי) |
| Incoming | GetCertificateRequestByGuid | `GET web/CertificateOfOrigins/RequestByGuid` | CertificateOfOriginsBl | ✅ | ✅ |
| Internal | GetCertificateOfOriginsByFilter | `QUERY ui/CertificateOfOrigins/ByFilter` | CertificateOfOriginsBl | ✅ | ✅ |
| Internal | IsCertificateOfOriginByExternalIdExist | `GET ui/CertificateOfOrigins/ByExternalIdExist` | ✅ | ✅ | ✅ |
| Internal | GetCertificateOfOriginById | `GET ui/CertificateOfOrigins/{id}` | ✅ | ✅ | ✅ |
| Internal | SaveCertificateOfOrigin | `POST ui/CertificateOfOrigins` | ✅ | ✅ | ✅ (פערי פאריטי) |
| Internal | LoadDataFromExportDeclaration | `QUERY ui/CertificateOfOrigins/LoadDataFromExportDeclaration` | ✅ | proxy | ✅ |
| Internal | SaveImportAuthenticationRequest | `POST ui/AuthenticationRequest/SaveImport` | AuthenticationRequestBl | ✅ | ✅ (F-01) |
| Internal | GetAuthenticationRequestByFilter | `QUERY ui/AuthenticationRequest/ByFilter` | ✅ | ✅ | ✅ |
| Internal | GetEntityDocuments | `GET ui/AuthenticationRequest/EntityDocuments/{leadDocumentId}` | ✅ | proxy | ✅ |
| Internal | CreateNewAuthenticationFile | `POST ui/AuthenticationRequest/CreateNewFile` | ✅ | ✅ | ✅ |
| Internal | GetAuthenticationRequestFileByID | `GET ui/AuthenticationRequest/File/{fileId}` | ✅ | ✅ | ✅ (E-F3) |
| Internal | SaveAuthenticationRequestFile | `POST ui/AuthenticationRequest/SaveFile` | ✅ | ✅ | ✅ (F-02 תוקן) |
| Internal | GetAuthenticationRequestByID | `GET ui/AuthenticationRequest/{documentId}` | ✅ | ✅ | ✅ |
| Internal | GetAuthenticationRequestByLeadDocumentIDs | `POST ui/AuthenticationRequest/ByLeadDocumentIDs` | ✅ | ✅ | ✅ |
| Internal | GetExportDocumentAuthenticationRequestSearch | `QUERY ui/ExportDocumentAuthenticationRequest/Search` | ExportDocumentAuthenticationRequestBl | ✅ | ✅ |
| Internal | GetExportDocumentAuthenticationRequestByID | `GET ui/ExportDocumentAuthenticationRequest/{id}` | ✅ | ✅ | ✅ |
| Internal | SaveExportDocumentAuthenticationRequest | `POST ui/ExportDocumentAuthenticationRequest` | ✅ | ✅ | ✅ (טבלה חסרה ב-Scripts) |
| Internal | GetCustomerInformation | `GET ui/ExportDocumentAuthenticationRequest/CustomerInformation/{customerId}` | ✅ | proxy | ✅ |
| Internal | GetCustomerInformationByCountry | `GET ui/ExportDocumentAuthenticationRequest/CustomerInformationByCountry/{countryId}` | ✅ | proxy | ✅ |
| Internal | HandleImportAuthenticationRequestDeliveryForImporterSent | `POST ui/AuthenticationRequest/HandleImportDeliveryForImporterSent` | ✅ | ✅ | ✅ |
| Internal | HandleImportAuthenticationRequestDeliveryReminderForImporterSent | `POST ui/AuthenticationRequest/HandleImportDeliveryReminderForImporterSent` | ✅ | ✅ | ✅ |
| Internal | HandleImportAuthenticationRequestDeliveryAndReminderForVendorSent | `POST ui/AuthenticationRequest/HandleImportDeliveryAndReminderForVendorSent` | ✅ | ✅ | ✅ (G-F1) |
| Internal | CheckIfExistsAdditionalRequestsForImporter | `GET ui/AuthenticationRequest/CheckIfExistsAdditionalRequestsForImporter` | ✅ | ✅ | ✅ |
| Internal | CheckIfExistsAdditionalRequestsForVendor | `GET ui/AuthenticationRequest/CheckIfExistsAdditionalRequestsForVendor` | ✅ | ✅ | ✅ |
| Internal | GetPathsForNavigationToVendor | `GET ui/AuthenticationRequest/PathsForNavigationToVendor` | ✅ | — | ✅ (מחזיר ריק — TODO(blocking)) |
| Internal | HandleSendRemindDeliverNotification | `POST ui/AuthenticationRequest/CloseReminderTask` | ✅ | ✅ | ✅ |
| Internal | ChangeStatusAfterDeliverySent | `POST ui/AuthenticationRequest/ChangeStatusAfterDeliverySent` | ✅ | ✅ | ✅ (G-F1) |
| Internal | CheckImporterOfImportAuthentication | `GET ui/AuthenticationRequest/CheckImporterOfImportAuthentication` | ✅ | ✅ | ✅ |

נוספים (External לגאסי / חדשים): `GET api/CertificateOfOrigins/ID/{certificateNumber}`, `POST .../GoodsItemCerificateDTO`,
`POST .../Convert`, `POST .../SaveAttachments`, `POST api/EventsResponse/HandleDeliverySent`, ו-`GET api/CertificateOfOrigins/Template/{templateId}/{entityId}` (CR 194221, חדש).
Scheduled tasks: `ReminderForImporterScheduler`, `AuthenticationRequestReminder` → BL + Planar — אין משימה לגאסית ללא מקבילה.
DI: כל 19 ה-proxies רשומים כ-`AddProxy<I, Real, Mock>` (real ברירת מחדל, mock לפי `x-mock-mode`); `AddTemplateUtil`, `AddQueueUtil`, `AddLockServices`, 4 lookups — רשומים.

### CHECK 2 — פאריטי (ספירה לפי אשכול)
| אשכול | פעולות | HIGH | MED | LOW | נספח |
|---|---|---|---|---|---|
| A — ליבת תעודה | 5 | 1 (A-02) | 11 (כולל A-01 שהורד) | 15 | `_parity-A-certificate-core.md` |
| B — Reconcile + Api | 1 + 5 | 3 | 7 | 15 | `_parity-B-external-reconcile.md` |
| C — הודעה נכנסת 2280 | 1 (41 כללים) | 3 | 10 | 13 | `_parity-C-incoming-msg2280.md` |
| D — שאילתת Web | 1 | 0 | 4 | 4 | `_parity-D-web-guid.md` |
| E — קריאות בקשות אימות | 9 | 1 (F3) | 3 | 13 | `_parity-E-auth-reads.md` |
| F — כתיבות בקשות אימות | 3 | 3 | 2 | 7 | `_parity-F-auth-writes.md` |
| G — delivery + schedulers | 7 + 2 jobs | 1 | 1 | 8 | `_parity-G-delivery-schedulers.md` |
| H — בקשות מסמכי יצוא | 5 | 0 | 4 | ~10 | `_parity-H-export-docs.md` |

> ⚠️ **תיקון לנספח A:** A-01 מסומן בנספח כ-HIGH. אחרי אימות הוא **MED** — ApplicationCorrected **כן** נורה
> (`CertificateOfOriginsBl.cs:987-989`), אבל על התנאי `previousCertificateId != 0` ולא על `Received && CertificateUpdate`.
> C-02 ו-A-07 הם אותו ממצא. E-F3 ו-F-02 הם שני צדדים של אותו פער.

סריקת קטגוריות-חוסר: PushUtil — 0; hardcoded `=1` — 0 (יש `RequestMetadata.UserId ?? 0` fallback); Mock — אף proxy אינו mock-only; `TODO(confirm)` — 6; "out of scope"/"heavy Save()" — 0.

### CHECK 3 — Code Review
נספח: `pre-internal-gap-report_2026-09-27_check3-code-review.md` (כולל טבלת 168 ה-TODO המלאה ושורה לכל `TODO(blocking)`).
Entities מול EDMX: 20/21 תואמים (0 פערי עמודה/טיפוס/nullability); `SupplierDeliveryCountryConfig` לא ב-EDMX אך נוצר בסקריפטים.

### CHECK 4 — סקריפטי DB
נספח: `pre-internal-gap-report_2026-09-27_check4-db-scripts.md`.
| בדיקה | תוצאה |
|---|---|
| טבלאות DATA (שם+schema+עמודות מול `[Table]`) | ❌ 20/21 — חסר `CustomsItemToExportDocumentAuthenticationRequest` |
| טבלאות enum/C עם נתונים | ✅ כולל שורות ID=10 AdministrativeClosure |
| פרוצדורות שהקוד קורא | ✅ 11/11 (+ `Shared.IntArray`, sequence) |
| פרמטרים (IParametersUtil) | ✅ 14/14 ב-DB וב-seed |
| from-zero replay (`CertificateOfOrigins_ZeroReplay_20260927171745`, נמחק) | ✅ 0 שגיאות במעבר ראשון; ❌ מעבר שני נכשל ב-create tables (Msg 2714) |
| תבניות (4a+) | ❌ קובץ דרום-קוריאה חסר; אין `tools/upload-templates.ps1` |

> הערה: בעליית השירות לריצת ה-coverage, DbUp החיל על ה-DB המקומי את שני הסקריפטים החדשים
> (`API_20260922093000`, `API_20260923104500`) שטרם הורצו בו — התנהגות עלייה רגילה; ה-Hebrew אומת תקין.

### CHECK 5 — Postman
| קבוצה | נמצא | assertions (סטטי, `pm.test(` ב-definition.yaml) | סטטוס |
|---|---|---|---|
| Group 2 — `Postman/postman/collections/CertificateOfOrigins Internal Workload - *` | 15 אוספים (+ `CertificateOfOrigins Param IssueByWorker` עם runner נפרד) | כולם > 0 (4–15) | ❌ 4 אדומים (ראה חוסמים) |
| Group 1 — `Postman/dependency-workload-test/collections/CertificateOfOrigins Dependency Workload - *` | 9 (Collaterals, Common, Customers, Documents, ExportDealFile, SystemTables, Tasks, Users, Vendors) | 0 ב-definition (הכל ברמת בקשה) | ❌ routes שבורים |
| runners | `internal-workload-test/run.ps1`, `dependency-workload-test/run.ps1` | — | ✅ |
| JSON פרש | `Postman/CertificateOfOrigins-Internal.postman_collection.json` | — | ❌ |
| Group 3 (`Postman/dev/...`) | לא קיים | — | לעיון בלבד, לא gate |

ריצה דינמית (2026-09-27, `last-run.json` בתיקייה זו): groups 15 · passed 11 · failed 4 · requests 249 ·
assertionsExecuted 774 · assertionsFailed 14 · groupsWithNoAssertions [] · `assertionsExecuted ≥ requests` ✅.

---

## דוח Code Coverage (Group 2, ריצה טרייה 2026-09-27)
מקור: `Summary.txt` + `last-run.json` בתיקייה זו (reportgenerator, מסונן ל-`CertificateOfOrigins.*`).

| מחלקה | line | branch | רף | סטטוס |
|---|---|---|---|---|
| CertificateOfOrigins.BL (כולל) | 79.2% | 70.5% | ≥70% / ≥50% | ✅ |
| CertificateOfOriginsBl | 86.0% | 80.6% | ≥70% | ✅ |
| AuthenticationRequestBl | 85.0% | 73.4% | ≥70% | ✅ |
| ExportDocumentAuthenticationRequestBl | 94.8% | 88.5% | ≥70% | ✅ |
| EventsResponseBl | 100% | — | ≥70% | ✅ |
| CertificateOfOriginsDal | 91.8% | 75.7% | ≥70% | ✅ |
| Api/CertificateOfOriginsController | 84.0% | — | ≥90% | 🟠 (מסלול Template) |
| Api/EventsResponse, Community, Ui×3, Web controllers | 100% | — | ≥90% | ✅ |
| שירות (CertificateOfOrigins.*) כולל | 73.1% | 55.7% | — | לעיון |

> ⚠️ ה-coverage **אינו oracle של פאריטי**: ה-BL מכוסה 86% line / 80% branch — ובכל זאת נמצאו 11 פערי HIGH.
> האוספים נכתבו מהקוד החדש ומקבעים את התנהגותו; assertions קריטיות צריכות להיגזר מערכי-הזהב של הלגאסי.
> (4 הקבוצות האדומות עדיין תרמו ל-coverage — הבקשות רצו; רק ה-assertions נכשלו.)
