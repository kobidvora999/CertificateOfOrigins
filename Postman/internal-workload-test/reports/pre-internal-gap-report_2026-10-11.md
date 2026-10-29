# דוח חוסרים לפני הכנסה לסביבה פנימית — CertificateOfOrigins
תאריך: 2026-10-11 · Scope: `all` (30 פעולות WCF ב-3 החוזים, 6 endpoints נוספים, 2 schedulers ו-worker ה-IssueCertificateOfOrigin) · Branch/Commit: `master` @ `f9374bf`

> **ריצת אימות חוזרת** אחרי סבב התיקונים של ממצאי 2026-09-27 (ה-ledger מראה 71 FIXED, 46 CLOSED-OK, 19 OPEN-BLOCKED ו-0 פתוחים).
> המטרה: לוודא שהתיקונים אכן בקוד ושלא נוצרו רגרסיות, ולחפש מחדש פערים שלא נמצאו בפעם הקודמת.
> - פאריטי: 8 סוקרים עצמאיים (לא הממירים) עברו על כל מתודה statement אחר statement, ואימתו כל ממצא קודם מול הקוד.
> - כל ממצא HIGH/MED אומת מחדש ע"י ה-orchestrator בקריאה של שני המקורות לפני שנכנס לדוח.
> - build, בדיקות יחידה ו-from-zero replay של ה-DB (פעמיים) הורצו מחדש.
> - ריצת ה-coverage **לא הצליחה** מסיבות סביבה. ראו CHECK 5.

## סיכום מנהלים
| בדיקה | סטטוס | חוסרים |
|---|---|---|
| 1. כיסוי חוזים        | ✅ | 0. ‏35/36 פעולות ממופות לכל השכבות; `TempSync` הושמט בכוונה (stub מת בלגאסי) |
| 2. פאריטי התנהגותי    | ❌ | **2 HIGH · 8 MED** חדשים (כולם CONFIRMED) · ~25 LOW · 0 רגרסיות · 3 תיקונים חלקיים |
| 3. Code Review        | ⚠️ | C 0 / M 2 / m 9 · ‏5 warnings שאינן TODO · ‏165 TODO (63 `blocking`) |
| 4. שלמות סקריפטי DB   | ⚠️ | 0 חוסמים · 1 Major (אין מנגנון העלאת תבניות) · 6 Minor |
| 5. Postman workload collections | ⚠️ | 17+1 אוספי Internal ו-9 Dependency, לכולם assertions · **הריצה הדינמית לא נבדקה** (סביבה) |

**מוכן להכנסה לסביבה פנימית? לא.** הסיבות:
- 2 פערי פאריטי HIGH מאומתים: ב-Reconcile, תעודה שפריטיה בלי פרט מכס מסתיימת DeclarationMatch במקום Rejected; וב-worker, ה-XML של משוב 2281 לא תואם לסכמת ה-EAI.
- 8 פערי MED מאומתים, בהם מחיקת מסמך ה-QR בפרסום ושמירת תעודה שאינה אטומית.
- כיסוי ה-Postman לא נמדד אחרי התיקונים.

### מה השתנה מאז 27.9
- **כל 10 החוסמים מ-27.9 אומתו כמתוקנים:** F-01, F-02/E-F3, G-F1, A-02, B-H1/H2/H3, C-01/02/03, ו-C1 (התבנית, עכשיו `WithJsonData`).
- **DB:** הטבלה החסרה נוספה, `create tables.sql` עכשיו idempotent, וערכי הפרמטרים תואמים ל-seed.
- **Postman:** ה-probes של dependency נבנו מחדש מול routes אמיתיים, וקובץ ה-JSON הישן הוסר.
- **0 רגרסיות.** 3 ממצאים שסומנו FIXED או מתועדים תוקנו רק חלקית: A-21 (פורטים), C-F04 (הוליד את CN-01), ו-G-F4 (אין סמן TODO בקוד למרות שה-ledger מצטט אותו).
- **הממצאים החדשים** הם ברובם פערים שלא נתפסו בסבב הקודם. חלקם בקוד שנכנס אחרי 27.9: ה-worker (`2a3a98e`), ה-QR כמסמך (A-20), ותיקון C-F04.

---

## פירוט חוסרים (לפי חומרה)

### 🔴 חוסמים (Blockers)

**פאריטי — HIGH, מאומתים**
| # | פעולה | הסטייה | חדש | לגאסי |
|---|---|---|---|---|
| BN-01 | UpdateCetrificateOfOrigins (Reconcile) | **הסינון `&& item.CustomsItemId.HasValue` משמיט פריטי טובין.** הלגאסי עובר על **כל** פריטי הטובין של חשבונית מותאמת, והחדש עובר רק על פריטים שיש להם `CustomsItemId`. כשבחשבונית אין לאף פריט פרט מכס (סוג תעודה שלא מחייב פרט מכס), שלוש בדיקות לא רצות: ארץ מקור, קבוצת ארצות מקור, ותעודה מקושרת לתיק. התעודה מסתיימת **DeclarationMatch** (event 642, בלי שגיאות) במקום **Rejected + DeclarationMismatch**. תיקון: לולאה על כל שורות הפריטים, ובדיקת 6 הספרות רק כש-`CustomsItemId` אינו null. | `CertificateOfOriginsDal.cs:274` (הסינון), `CertificateOfOriginsBl.cs:2474-2477` | `CertificateOfOriginsBL.cs:803-834` (`foreach (var goodsItem in invoice.CertificateOfOriginItemDetail)`) |
| GN-01 | Worker IssueCertificateOfOrigin, משוב 2281 | **ה-XML של המשוב לא תואם לסכמת ה-EAI.** שישה אלמנטים ב-PascalCase במקום camelCase: ‏`InternalApplication`, `CertificateID`, `CertificateOfOriginTypeCode`, `CertificateOfOriginStatusCode`, `RejectCancelReason`, `RequestReasonCode` (בסכמה: `internalApplication` וכו'). ה-namespace של האלמנטים הוא `EAICommon.xsd` במקום ה-namespace של `PC_NG_2281_MSG02`. ‏XmlSerializer תלוי רישיות ו-namespace, ולכן מערכת הסוכן לא תקרא את השדות. ⚠️ ה-worker יובא "as given": **לברר מול קובי** אם זה הקוד שרץ היום בייצור. אם כן, זו התנהגות ייצור קיימת ולא רגרסיה של ההמרה. | `Workers/IssueCertificateOfOrigin.Worker/Dto/CertificateOfOriginRequestFeedback.cs:16-43` | `Infrastructure/EAI/EAISchema/Customs.Inf.CertificateOfOrigins.EAISchema/PC_NG_2281_MSG02_CertificateOfOriginRequestFeedback.cs:42-93` |

### 🟠 לטיפול לפני merge (Major)

**פאריטי — MED, מאומתים**
| # | פעולה | הסטייה | חדש | לגאסי |
|---|---|---|---|---|
| AN-01 | SaveCertificateOfOrigin | **השמירה אינה אטומית.** ‏`SupersedePreviousVersion` מבטל את הגרסה הקודמת מיד, ב-`ExecuteUpdateAsync` שמבצע commit מיידי. זה קורה **לפני** יצירת ה-QR, פענוח הקודים וה-insert. אם אחד מהם נכשל (שירות QR או lookup נופל, TypeId לא מוכר), הגרסה הישנה נשארת מבוטלת בלי גרסה שמחליפה אותה. גם אם העלאת ה-QR נכשלת אחרי שמירת ה-header, התעודה נשארת Published בלי events וצרופות, וניסיון חוזר רואה סטטוס "לא השתנה". | `CertificateOfOriginsBl.cs:1007-1009` → `CertificateOfOriginsDal.cs:420-433` | commit יחיד, `CertificateOfOriginsBL.cs:938-1009` |
| AN-02 | SaveCertificateOfOrigin (פרסום) | **הפרסום מוחק את מסמך ה-QR שהוא עצמו יצר.** ה-QR מועלה עכשיו כמסמך **מקושר לתעודה**. שלב הצרופות מוחק את כל מסמכי התעודה לפני העלאת ה-PDF, גם inline וגם ב-worker. ‏`QrCodePath`, שהדוח המודפס משתמש בו, מצביע אחרי זה על מסמך מחוק, ולכן הדפסה חוזרת תצא בלי QR. בלגאסי ה-QR הועלה כקובץ repository **לא מקושר** (entity `-1`), כך שהניקוי לא נגע בו. | `CertificateOfOriginsBl.cs:1257-1265` (‏`.WithEntityId(entity.Id)`), `:881-886`; `IssueCertificateOfOriginBL.cs:72-79` | `CertificateOfOriginsBL.cs:1047-1050` (`UploadFile(fileName, Other, -1, stream)`) |
| CN-01 | GetPC_MSG2280_2281 (שאילתת סטטוס) | **תיקון C-F04 הרחיק לכת.** ענף GetRequestStatus קורא ל-`ValidateMessageBody`, וזה מריץ תמיד את בדיקות ה-cross-field. הלגאסי דילג עליהן לסטטוס ולביטול. דוגמה: שאילתת סטטוס בלי ארץ יעד, או EUR1 בלי ארץ שנייה, מחזירה עכשיו DestinationCountryRequired וכו' עם משוב ריק, במקום הסטטוס. ההערה ב-`MessageCrossField.cs:15-16` טוענת שסטטוס מוחרג, אבל זה לא נכון. | `CertificateOfOriginsBl.cs:205`, `MessageValidation.cs:188-191, 211-214` | `CertificateOfOriginsIncomingMessageServicePartial.cs:95-98` |
| FN-01 | SaveAuthenticationRequestFile (ביטול תיק) | **ביטול תיק פותח משימות שהלגאסי לא פתח.** החדש מרים `AuthenticationRequestRejected` (משימת "טיפול בבקשה שנדחתה") לכל בן עם החלטת דחייה. בלגאסי הניתוק (`AuthenticationFileID = null`, בלולאה הפוכה) רץ **לפני** לולאת הדחייה, וה-fixup של ה-self-tracking entity מרוקן את האוסף, כך שלא הורם אף event. | `AuthenticationRequestBl.cs:1360-1371` (הלולאה על `request.Requests` שנשלחו מהלקוח) | `AuthenticationRequestBL.cs:1033` → `:1095-1104` → `:1049-1062` |
| FN-02 | SaveAuthenticationRequestFile | **שינוי סטטוס של תיק בלי בקשות-בן נכשל ב-500.** ‏`FileOrganizationUnitId` חוזר לערך שהלקוח שלח, אבל לתיק אין עמודת OrganizationUnit וקריאת התיק לא מחזירה אותו, ולכן הערך 0. ה-event builder זורק על `WithOrganizationUnitId(0)` (מתועד ב-`AuthenticationRequestBl.cs:401-403`). זה קורה לפני `UpdateAuthenticationFile`, ולכן הסטטוס לא נשמר. הלגאסי שמר עם יחידה 0. | `AuthenticationRequestBl.cs:1329-1334, 1355, 1400, 1429` | `AuthenticationRequestBL.cs:1033-1068` |
| GN-03 | ChangeStatusAfterDeliverySent, CloseReminderTask | **אותו פער כמו F-05, שלא תוקן כאן.** היחידה הארגונית נלקחת מהלקוח, וקריאת התיק לא מחזירה יחידה ברמת התיק. ערך 0 גורם ל-builder לזרוק, התשובה 5xx, ושום event לא מורם. בלגאסי היחידה נגזרה מהבן הראשון (partial ה-FileDetails). | `AuthenticationRequestBl.cs:490, 507` | `AuthenticationRequestBL.cs:1270-1293` |
| GN-02 | Worker, משוב 2281 | `IssueDateIfReleased` / `IssueDateIfNotReleased` לא ממופים. ה-API שולח `IssuingDate` ו-`IsDeclarationReleased` וה-worker מקבל אותם, אבל לא משתמש בהם. | `Workers/.../IssueCertificateOfOriginHandler.cs:119-134`, `Dto/CertificateOfOriginRequestFeedback.cs` (שני השדות חסרים) | `CertificateOfOriginsUtil.cs:62-69`; סכמה `:80-93` |
| GN-04 | Worker | ה-worker קורא את `CertificateOfOriginQueryURL` מ-`appsettings.{env}.json` ולא מ-parametersUtil. ה-API קורא את אותו מפתח מ-Parameters, כך ששני הערכים עלולים להתרחק זה מזה. | `IssueCertificateOfOriginHandler.cs:121`, `Workers/CertificateOfOriginsWorkers/appsettings*.json` | `Configuration.GetConfig`, `CertificateOfOriginsUtil.cs:46` |

**Code review — Major**
- **CR-N1 (חדש, באג):** templateId שאינו רשום מחזיר **500 במקום 400**. ‏`new RestValidationException($"Unsupported templateId {templateId}.")` קורא לבנאי עם `field` בלבד ורשימת שגיאות ריקה, והבנאי עצמו זורק `InvalidOperationException: errors parameter is null or empty`. זה נצפה בלוג השירות בריצה של היום. תיקון: `new RestValidationException(nameof(templateId), $"Unsupported templateId {templateId}.")`. כל שאר הקריאות ל-`RestValidationException` ברפו מעבירות שני ארגומנטים. ‏`CertificateOfOriginsBl.Templates.cs:101`.
- **CR-N2 — worker:** ערכים קשיחים וקוד מיובא. ‏`Task.Delay(5000)` בכל הודעה (`IssueCertificateOfOriginHandler.cs:36`); `OrganizationUnitID ?? 4` (`IssueCertificateOfOriginBL.cs:60`, ובלגאסי/API היחידה היא של המשתמש המפרסם, ראו GN-05); `DocumentTypeID = 329` ו-`DocumentAdditionaFieldID = 46` כ-literals; `UpdateIsInPublishingProcess` לא מעדכן UpdateDate/UpdateUserId (GN-06). ה-worker קורא לשירותי WCF הלגאסיים (Connected Services: CommonServicesExternal, DocumentsExternal). זה נדרש בפנים, אבל אומת רק עד עליית ה-host, וטיפול בהודעה אמיתית לא הורץ (לפי הודעת הקומיט).

**DB — Major**
- **אין מנגנון להעלאת התבנית ל-bucket:** ‏`tools/upload-templates.ps1` לא קיים. קובצי `.docx` + `.yml` של דרום-קוריאה נמצאים עכשיו ב-`Templates/`, אבל בסביבה חדשה CR 194221 לא ירונדר עד העלאה ידנית ל-MinIO.

**Postman / Coverage — Major**
- **הריצה הדינמית לא נבדקה.** ריצה ב-2026-10-11 נכשלה ב-17/17 קבוצות (1030 assertions, 398 נכשלו). שורש הבעיה **סביבתי**, לא בקוד:
  1. **מעקף ה-readiness** (העברת Consul ל-`Test`) שובר את ה-lookups המקומיים החדשים (`AddLocalLookup` ל-Common/Cargos/SystemTables) ואת `IDocumentUtil` (Documents). מחוץ ל-Development אין service locator, והתוצאה `Fail to create base url for microservice: Common/Documents/Cargos/SystemTables`, ‏~160 מופעים. ב-27.9 אותם שירותים היו MockProxies, ולכן המעקף עבד.
  2. **`IssueCertificateOfOriginByWorker = True`** נשאר ב-DB המקומי מריצת issue-by-worker קודמת (ה-seed: `False`). כל פרסום עובר לכן למסלול התור, וה-runner `run-issue-by-worker.ps1` מסרב לרוץ.
  - **נדרש להרצה:** להחזיר את הפרמטר ל-`False`, למחוק את מפתח ה-Redis `Parameters.CertificateOfOrigins`, ולהריץ ב-Development (בלי להעביר את Consul) כש-`tools/local-lookup-stub.js` מגיש גם את 9000/9006/9015. ניסיון לשנות את הפרמטר נחסם בהרשאות בריצה זו, ולכן זה ממתין למפתחת.
  - דוחות ה-coverage בתיקייה (`Summary.txt`, `index.html`, `last-run.json`, `coverage.cobertura.xml`) **נדרסו בריצה הכושלת**. אל תסתמכו עליהם.

### 🟡 לתשומת לב (Minor / Deferred)

**פאריטי — LOW** (פירוט בנספחים)
- **A:** A-21 חלקי: 4 סוגי הפורטים מוצגים כ-id ולא כשם האנגלי, אף ש-`Lookups.InternationalSite` כבר בשימוש (`Bl.cs:1436-1441`). AN-03: שמירת תעודה קיימת משכתבת ערכי תצוגה שהלגאסי השאיר. AN-04: קוד DestinationCountry/PortOfShipment לא מוכר בתעודה חדשה נשמר כמו שהוא; הלגאסי נכשל.
- **B:** BN-02: ‏HandleDeliverySent לא מקבל `TypeID` כחלופה ל-`EntityType` (`AuthenticationRequestBl.cs:361-362`). BN-03: הערת controller מיושנת: כתוב 404 על template לא רשום, ובפועל 500 (CR-N1).
- **C:** CN-02: טקסטי MandatoryValue בשמות JSON החדשים. CN-03: ה-agent id רק מה-header `MessageSenderId`, דורש אימות חי. CN-04: כל חריגה הופכת ל-GeneralException (code 0).
- **D:** DN-01: בתשובות "Invalid Guid" / "No Matching Certificate" הרשימות יוצאות `[]` ולא `null` (`CertificateOfOriginsResponseDto.cs:25,28`). DN-02: סדר שדות ו-escaping של JSON. DN-03: ‏`issuingDate` לא תקין מחזיר 400 אוטומטי. DN-04: תיעוד מיושן של 502 על lookups. DN-05: תוויות שדות 20306/20310/20661 תלויות ב-lookup המקומי.
- **E:** EN-01: ‏ById מחזיר LastDeliveryForImporter (הלגאסי החזיר null). EN-02: header התיק בלי TimeStamp/Create/Update. EN-03: InvoiceNumber ריק מחזיר רשימה ריקה.
- **F:** FN-03: אי אפשר לנקות ספק/לקוח/יבואן ממסך התיק. FN-04: insert כפול מריץ צד-השפעות לפני 409, כמו בלגאסי. FN-05: כל שמירת תיק מחתימה UpdateDate על כל הבנים.
- **G:** G-F4: אין סמן TODO בקוד (ה-ledger מצטט `BL:355-356` שלא קיים). GN-05: יחידה ארגונית של מסמך ה-worker. GN-06: אין stamp ב-worker. GN-07: ב-Planar, שורה עם יחידה 0 זורקת ומפילה את כל ה-batch.
- **H:** HN-01: ‏404 מ-Customers הופך ל-5xx במקום 400 "אין בית מכס". HN-02: הערות ו-`[NotFoundResponse]` מיושנים.

**Code review — Minor**
- **5 warnings שאינן TODO:**
  - S125 ×3: `CertificateOfOriginsBl.cs:2110`, `CollateralProxy.cs:28`, `Workers/CertificateOfOriginsWorkers/Program.cs:42`. כולן הערות פרוזה ש-Sonar מזהה בטעות כקוד, ולכן מדכאים נקודתית או מנסחים מחדש.
  - S4070: ‏`[Flags]` על `ERequestReason.cs:12`. ‏`IsDefined` לא מושפע, אבל ToString של 6 ייתן `"RetrospectiveCertificate, CertificateReplacement"`. להסיר.
  - CS8619: `CertificateOfOriginsDal.cs:37`.
- **מצאי TODO:** 165 מופעים, 0 FIXME/HACK/XXX, 0 PushUtil לא מסומן, 0 `=1` קשיח בקוד ייצור (רק במוקים ובבדיקות).
  - פילוח: `blocking` 63 · `internal` 17 · `migration` 16 · `confirm` 8 · `confirm live` 2 · `platform` 1.
  - ה-build מראה 168 S1135 ו-174 REVIEW00x (סמני `tools/review-markers.ps1`).
  - טבלה מלאה: `_check3-todo-inventory.md`.
- **הערות מיושנות:** `ServicesConfiguration.cs:107-108` ("No lookup type exists for country-group / international-site", ושניהם קיימים עכשיו כ-local lookups). שאר ההערות המיושנות מפורטות ב-DN-04, ‏HN-02 ו-BN-03.
- **TODO שאולי מיושן:** ‏`CertificateOfOriginsBl.cs:1086-1090, 1538` טוען שערוץ ה-2281 היוצא הוסר בלי תחליף, אבל ה-worker החדש שולח 2281 דרך `POST Common/OutgoingMessage`. **לברר**: ייתכן שאפשר לחבר גם את הפרסום ה-inline (A-13/A-17/B-L6).
- **שערים חוסמים:** `DateTimeOffset` — 0 ✅. חדש-או-עדכון — 2 מופעים (`AuthenticationRequestBl.cs:977, 1140`), שניהם תקינים: ההחלטה לפי `IsNewInstance`, ועדכון שלא מצא שורה מחזיר 404 ✅. ‏`.Result` — 1, אחרי `await` (`Templates.cs:59`), תקין ✅. אין `async void`, ‏`#error` או `AddRestProxy` ✅.

**DB — Minor** (נספח `_check4-db-scripts.md`)
- ה-FK של `API_20260928120000` נמצא בתוך ה-`IF NOT EXISTS` של הטבלה, ולכן DB שהטבלה כבר הייתה בו (כולל localhost) לא מקבל אותו.
- `IssueCertificateOfOriginByWorker`: ‏DB=`True`, ‏seed=`False`. לקבוע את ברירת המחדל עכשיו כשה-host של ה-Workers קיים.
- ב-DB המקומי חסרים 9 FKs שהסקריפטים יוצרים. יש בו 22 פרוצדורות בלי סקריפט ושורת בדיקה אחת ב-`_c_VerificationProhibitedImporters`.
- 8 seeds של פרמטרים יתומים, ו-`IsExportDeclarationActive` שקיים רק ב-DB.
- 5 שמות סקריפט בפורמט תאריך בלבד (הסדר תקין).
- `3a69173` ערך סקריפט שכבר רץ במקום להוסיף סקריפט חדש. זה לא גרם נזק כאן.
- Workers: `CustomsDbContext.cs:29-92` מכיל ~40 `HasSequence` scaffold של סכמות אחרות.
- `Templates/README.md` וה-`.yml` חלוקים בשאלה אם יש לטופס ראש מכתב. ה-template id עדיין `TODO(confirm)`.

**Postman — Minor**
- תיקייה מקומית ריקה `Postman/postman/collections/CertificateOfOrigins Internal Workload - API/`: לא ב-git, רק תתי-תיקיות ריקות. הפרפיקס של ה-runner תופס אותה, ולכן כדאי למחוק אותה מקומית.
- לא בוצע מבחן מוטציה על האוספים.

---

## נספח: פירוט מלא לכל בדיקה

### CHECK 1 — כיסוי חוזים (✅)
החוזים הלגאסיים מכילים External 7, Internal 27 ו-Incoming 2, ובסך הכול 36 `[OperationContract]` (בנוסף 4 callbacks).
- **WebApi:** ‏36 actions — Api 6, EventsResponse 1, Community 1, Ui 27 (‏AuthenticationRequest 17, CertificateOfOrigins 5, ExportDocument 5), Web 1.
- **כל 35 הפעולות הנדרשות ממופות** ל-endpoint, BL, DAL/proxy ו-DI, כמו בטבלה של דוח 27.9. אין שינוי במיפוי.
- **`TempSync`** לא הומרה בכוונה (`NotImplementedException` בלגאסי; `MIGRATION-NOT-DONE.md:185`).
- **endpoint נוסף:** ‏`GET api/CertificateOfOrigins/Template/{templateId}/{entityId}` (CR 194221).
- **Schedulers:** שני ה-Planar jobs.
- **Worker:** ‏`IssueCertificateOfOrigin`, תחת host חדש `Workers/CertificateOfOriginsWorkers`. הוא מחליף את מסלול ה-issue-by-worker של הלגאסי (`SendCertificateToIssueQueue`).
- **DI:** ‏11 proxies כ-`AddProxy<I, Real, Mock>`, ‏5 platform lookups, ‏7 local lookups (`TODO(internal)`), ParametersUtil, EventUtil, DocumentUtil, QueueUtil, TemplateUtil ו-LockServices.

### CHECK 2 — פאריטי (ספירה לפי אשכול)
| אשכול | ממצאים קודמים מאומתים | רגרסיה / חלקי | HIGH | MED | LOW | נספח |
|---|---|---|---|---|---|---|
| A — ליבת תעודה | 26/27 | A-21 חלקי | 0 | 2 (AN-01, AN-02) | 3 | `_parity-A.md` |
| B — Reconcile + Api | 25/25 | — | 1 (BN-01) | 0 | 2 | `_parity-B.md` |
| C — הודעה נכנסת 2280 | 25/26 | C-F04 → CN-01 | 0 | 1 (CN-01) | 3 | `_parity-C.md` |
| D — שאילתת Web | 9/9 | — | 0 | 0 | 5 | `_parity-D.md` |
| E — קריאות בקשות אימות | 17/17 | — | 0 | 0 | 3 | `_parity-E.md` |
| F — כתיבות בקשות אימות | 12/12 | — | 0 | 2 (FN-01, FN-02) | 3 | `_parity-F.md` |
| G — delivery, schedulers, worker | 9/10 | G-F4 בלי סמן | 1 (GN-01) | 3 (GN-02/03/04) | 3 | `_parity-G.md` |
| H — בקשות מסמכי יצוא | 14/14 | — | 0 | 0 | 2 | `_parity-H.md` |

**אימות ה-orchestrator** (קריאה חוזרת של שני המקורות):
- **BN-01:** אומת. הלגאסי: `CertificateOfOriginsBL.cs:809`. החדש: `Dal.cs:274` ו-`Bl.cs:2474`.
- **GN-01, GN-02:** אומתו מול הסכמה, `:48-93`.
- **AN-01:** אומת. `SupersedePreviousVersion` רץ בשורה 1009, ו-`ExecuteUpdateAsync` עושה commit מיידי.
- **AN-02:** אומת. החדש: `.WithEntityId(entity.Id)`. הלגאסי: `UploadFile(..., -1, ...)`.
- **CN-01:** אומת. הלגאסי: `:95-98`. החדש: `MessageValidation.cs:188-191, 211-214`.
- **FN-01:** אומת. סדר הלגאסי `:1033` → `:1095` → `:1049`. ‏ה-fixup של STE נתמך בלולאה ההפוכה.
- **FN-02, GN-03:** אומתו. ה-builder זורק על 0 (`Bl.cs:401-403`), ולתיק אין עמודת יחידה.

**ממצאים שתוקנו ונמצאו נכונים:**
- **ליבה ו-Reconcile:** A-01/02/04/06/07/08/09/10/20/25, ו-B-H1/H3/M1-M7/L9/L11.
- **הודעה נכנסת ו-Web:** C-F01-03/05-13/16/17/22-24, ו-D-F1-F8.
- **בקשות אימות, delivery ויצוא:** E-F3/12/13, F-01/02/05/06/07/10/11, G-F1/F3/F7/F9, ו-H-1/3/4/6/8/11/13/14.

**סריקת קטגוריות-חוסר:**
- PushUtil — 0.
- `=1` קשיח — 0 בקוד ייצור.
- Mock — כל ה-proxies real כברירת מחדל, ו-mock רק לפי `x-mock-mode`.
- `TODO(confirm)` — 10.
- "out of scope" / "heavy Save()" — 0.

### CHECK 3 — Code Review
- **build** (`dotnet build CertificateOfOrigins.slnx --no-incremental`): 0 errors.
  - warnings ייחודיות: 168 S1135 · 65 REVIEW001 · 13 REVIEW002 · 92 REVIEW004 · 2 REVIEW005 · **5 לתיקון** (S125×3, S4070, CS8619).
- **בדיקות יחידה:** ‏132/132 עברו.
- **מצאי TODO מלא:** `_check3-todo-inventory.md`.

### CHECK 4 — סקריפטי DB
נספח: `_check4-db-scripts.md`.
| בדיקה | תוצאה |
|---|---|
| טבלאות DATA (שם, schema, עמודות מול `[Table]`) | ✅ כולל `CustomsItemToExportDocumentAuthenticationRequest` |
| טבלאות enum/C עם נתונים | ✅ ספירת שורות זהה |
| פרוצדורות שהקוד קורא | ✅ ‏11/11, וגם TVP ו-sequence |
| פרמטרים (IParametersUtil) | ✅ כולם ב-seed. ⚠️ `IssueCertificateOfOriginByWorker` שונה בין DB ל-seed |
| from-zero replay ×2 (`CertificateOfOrigins_FromZero_20261011140942`, נמחק) | ✅ ‏21 סקריפטים, rc=0 בשני המעברים (idempotent) |
| Workers / Planar | ✅ אין אובייקט או פרמטר חסר |
| תבניות (4a+) | ⚠️ הקבצים קיימים; אין `tools/upload-templates.ps1` |

### CHECK 5 — Postman
| קבוצה | נמצא | assertions סטטיים (`pm.test(` ב-definition.yaml) | סטטוס |
|---|---|---|---|
| Group 2 — `Postman/postman/collections/CertificateOfOrigins Internal Workload - *` | 17 אוספים (ועוד `Param IssueByWorker` עם runner נפרד) | כולם > 0 (4–26) | קיום ✅ · ריצה: **לא נבדק** |
| Group 1 — `Postman/dependency-workload-test/collections/...` | 9: Collaterals, Common, Customers, Documents, ExportDealFile, SystemTables, Tasks, Users, Vendors. ‏42 probes ישירות ל-`/api/{Controller}/...` | 4 בכל אוסף, ברמת הקולקציה | ✅ (קיום מספיק) |
| runners | `internal-workload-test/run.ps1`, `run-issue-by-worker.ps1`, `dependency-workload-test/run.ps1` | — | ✅ |
| JSON ישן | אין | — | ✅ |
| Group 3 (`Postman/dev/...`) | לא קיים | — | לעיון בלבד |

**ריצה דינמית** (2026-10-11, שבורה סביבתית, ראו Major):
- groups 17 · passed 0 · requests 351 · assertionsExecuted 1030 · assertionsFailed 398 · groupsWithNoAssertions [].
- בלוג השירות, מעבר לכשלי ה-service locator, נמצא באג קוד אחד (CR-N1).

---

## דוח Code Coverage
**לא נמדד בריצה זו.** הסיבה בסעיף ה-Major של Postman.
- **הערכים האחרונים שנמדדו** (2026-09-27, לפני סבב התיקונים): ‏BL 79.2% line / 70.5% branch · CertificateOfOriginsBl 86.0% · AuthenticationRequestBl 85.0% · Dal 91.8% · Api/CertificateOfOriginsController 84.0% (מתחת לרף 90%).
- **אין לראות בהם ראיה לגבי הקוד הנוכחי:** מאז נוספו ה-local lookups, ה-QR כמסמך וה-worker.

> ⚠️ coverage **אינו oracle של פאריטי.** שני ה-HIGH של היום יושבים בשורות שכוסו ב-27.9:
> - BN-01: ‏`ValidateCertificateGoodsItem` רץ בכל תרחיש reconcile שיש בו פרט מכס.
> - GN-01: ה-worker לא מכוסה בכלל.
>
> ה-assertions הרלוונטיות צריכות להיגזר מערכי-הזהב של הלגאסי. למשל: תעודה בלי פרט מכס בחשבונית, עם ארץ מקור שאינה בהצהרה → Rejected.
