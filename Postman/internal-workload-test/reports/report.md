# Internal-workload report — CertificateOfOrigins

שירות CertificateOfOrigins · 17 collections (`CertificateOfOrigins Internal Workload - *`) · ריצה 2026-09-28 ·
port 9034 · branch `feature/internal-workload-lifecycle` · מקביליות 6 תחת session ‏dotnet-coverage אחד.

## 1. GATE — ⚠️ PASS-WITH-BLOCKERS

**Assertions: 994/994 passed** · 17/17 collections · 321 requests · 0 collections ללא assertions.
`Param IssueByWorker` (מעבר נפרד, `run-issue-by-worker.ps1`): 10/10.
שני מעברים ⏸ BLOCKED (skip גלוי, לא מכשילים) — סעיף 3.

| | לפני (אותו יום, master) | אחרי |
|---|---|---|
| Collections ירוקים | 11/15 | **17/17** |
| Assertions שנכשלו | 10 | **0** |
| תהליכים עם chain מלא | 0 | **3** |
| בקשות שפונות לנתון קבוע שלא נוצר בריצה | 62 | **0** |
| seed לפני ריצה | נדרש | **אין** |

## 2. מה תוקן כדי שהחבילה תהיה ירוקה

| Collection | סיבה | סוג | תיקון |
|---|---|---|---|
| Mock Features (4) + חלק מ-Reconcile Outcomes | **באג ב-BL**: ה-reconcile שאחרי קליטת מסר (`ReconcileWithSubmittedDeclaration`) קרא ל-dispatcher ‏`UpdateCertificateOfOrigins` בלי `EventType` → ענף `default` → `[]`. ההצלבה לא רצה אף פעם ממסר. ב-legacy הקריאה ישירה (`CheckCertificateOfOriginOnDeclarationSubmited` → `UpdateCertrificateOfOrigins`). | קוד | `CertificateOfOriginsBl.MessageDeclarationCheck.cs` — קריאה ישירה ל-`ReconcileCertificatesAgainstDeclaration` |
| Auth File Status (0 assertions) | גרש ב-`'…this run's…'` שבר את כל ה-script | fixture | ניסוח מחדש |
| Per Reason / 70-unknown-reason | assertion ישן (400). השירות דוחה in-band ‏(200 + 5020 `RequestReasonNotExist`) — כמו legacy | fixture | assertion על 5020 + שלא נוצרה תעודה |
| Reconcile Outcomes (Warnings ×2) | ה-fixture שלח ארץ יעד כ-id (`"32"`); תעודה חדשה מקבלת קוד alpha-2 ומתרגמת ל-id (legacy, f3640f7) → 102 ≠ 32 | fixture | `"AR"` → 148 בכל 5 התרחישים |

## 3. Process coverage

| תהליך | Collection | מעברים מוכחים | BLOCKED |
|---|---|---|---|
| תעודת מקור | `Certificate Lifecycle` | C1, C6, C4, C9, C12, C3, C13 + כל הקריאות על התעודה שנוצרה | ⏸ `Main/90-retransmit-status-query` |
| תעודה — חלופות | `Bl Core`, `Per Reason`, `Reconcile Outcomes`, `Mock Features` | C1a, C2, C5c, C7, C8, החלפה/עדכון | — |
| בקשת אימות יבוא + תיק | `Import Auth Lifecycle` | R0, F0 (+guard), F1, F6, F3, F7, F4, F5, F8, S1→5, S1→6, S1→9 + תיק חדש | ⏸ `Main/75-template-letter` |
| תיק — מכונת סטטוס | `Auth File Status`, `Save Import Decisions` | S1 לכל הסטטוסים, R1 | — |
| בקשת אימות יצוא | `Export Auth Lifecycle` | X0→X1→X2→X3→X4→X5(6), X5(7), X6(9) | — |

**BLOCKED:**
1. **`Main/90-retransmit-status-query` — באג פלטפורמה.** `CustomsCloud.InfrastructureCore.Lock` ‏(1.10.38, וגם 1.10.42 האחרונה):
   `LockUntilAsync` כותב ל-Redis DB 2, `SafeReleaseAsync`/`ReleaseAsync` קוראים ומוחקים ב-DB 0 → הנעילה לא משתחררת 5 דקות;
   שידור חוזר של אותה תעודה מקבל 400 "locked". דורש תיקון בצוות InfrastructureCore.
2. **`Main/75-template-letter`** — שירות Templates / קובץ התבנית לא זמינים (INTERNAL_INTEGRATION.md §4). כרגע 500 (proxy connection refused).

## 4. Line coverage (per class, reportgenerator)

| Class | לפני | אחרי |
|---|---|---|
| `CertificateOfOriginsBl` | 86% | **92.1%** ▲ |
| `AuthenticationRequestBl` | 85% | **85.9%** ▲ |
| `ExportDocumentAuthenticationRequestBl` | 94.8% | **95.9%** ▲ |
| `CertificateOfOriginsDal` | 91.8% | **93.9%** ▲ |
| `Api.CertificateOfOriginsController` | 84% | **92%** ▲ |
| שאר ה-controllers | 100% | 100% |

`AuthenticationRequestBl` נמוך יחסית בגלל `AuthenticationRequestBl.Schedulers.cs` — קוד Planar שאין לו HTTP trigger
(ראה `coverage-baseline.json` → history.c17). הכלי הנכון שם הוא unit tests, לא Postman.

## 5. Inventory — אין שום בקשה שפונה לנתון קבוע

35 endpoints (כולל 4 ‏`[HttpQuery]`). 0 ORPHAN. כל endpoint נקרא על ישות שנוצרה באותה ריצה (חוץ מ-`Template`, BLOCKED).

ניקוי (2026-09-28):
- **`API` נמחק** (40 בקשות, רובן על ids מומצאים: `900500`, `File/1`, `id: 999`, `IL0000116895`, GUID קבוע). כל
  ה-endpoints שלו מכוסים ב-lifecycle collections; חוזי ה-not-found הייחודיים עברו ל-`Negative`. גם ה-export הישן
  `Postman/CertificateOfOrigins-Internal.postman_collection.json` נמחק.
- **`seed_ImportAuthenticationRequests.sql` נמחק.** `Mock Features/AuthRequest` יוצר בקשה ותיק משלו (במקום 990101/990001).
- **מספרי תעודה ביצירה** (`BLC-*`, `RC-*`, `MF-COO-*`, `POSTMAN-COO-1`) מקבלים סיומת `-{{runId}}`. מספר קבוע נופל,
  מהריצה השנייה, לענף "מספר קיים → גרסה חדשה, הישנה מבוטלת".
- **חיפושים** מסוננים לפי ערך שהריצה יצרה (מספר חשבונית / DocumentID). `Export Doc Request/20-search` חיפש בכל הטבלה;
  היא גדלה מעבר לתקרת השורות של הפלטפורמה והבקשה נפלה ב-500.
- **בדיקות קיום** (`CheckIfExistsAdditionalRequestsFor{Vendor,Importer}`): ספק ויבואן חדשים בכל ריצה, כך שהתשובה
  תלויה רק בריצה.
- **Not-found:** `2147483647` (int.MaxValue — identity לא יגיע אליו) ו-`NO-SUCH-{{runId}}` למספרים, במקום `99999999`.
- `Auth File Status`: בדיקות ה-delivery עברו ל-`FileStatus/95-96`, אחרי שהתיק נוצר (תיקיות רצות לפי סדר אלפביתי).

כל מצב נוצר ע"י מעבר, לא מוצהר (2026-09-29):
- **12 תעודות נוצרו ישירות במצב מאושרת (8) / תואמת (6)** (`Bl Core` ×4, `Per Reason` ×7, `Web Query` ×1) — השמירה מקבלת,
  אבל שלב הפרסום (QR, תאריך הנפקה, event, תבנית) לא רץ. עכשיו: יצירה ב-2 ואז שמירת סטטוס של העובד (2→8 / 2→6)
  על אותה תעודה, עם הסטטוס המקורי מהתשובה.
- **בקשות יצוא נוצרו ישירות בסטטוס 2/5/6/9.** `Export Doc Request/30-50` נמחקו (המעברים 5/6/7/9 מוכחים ב-`Export Auth
  Lifecycle`); כל יצירה מתחילה ב-1.
- **`Auth File Status` שלח "סטטוס מקורי" קבוע (1)** בכל שמירה, גם אחרי שהתיק כבר זז — מעבר מ-1 שלא קרה. עכשיו כל שמירה
  שולחת את הסטטוס וההחלטות שהשמירה הקודמת החזירה.
- **`Auth Lifecycle/40-reminder`** שלח מצב delivery ‏(1,1) במקום ה-(2,2) ששלב 30 החזיר; עכשיו משורשר ומאומת בדיוק.
- **`Bl Core/20-convert`** עבר על שורה ישנה (`BLC-FILTER` מריצות קודמות) — עכשיו על מספר הריצה, ומאמת שחזר ה-id של התעודה שנוצרה.

נשארו בכוונה (לא נתון של השירות): template id `1`/`99` (קוד), לקוח `777`/`888` וארץ `32` — ישויות חיצוניות
שה-mock עונה עליהן לכל id.

## 6. ממצאים לבירור (לא תוקנו)

1. **SaveFile לא אטומי.** `SaveAuthenticationRequestFile` שומר קודם את ה-child requests (`UpdateFileChildRequest`),
   ורק אחר כך מריץ events. חריגה ב-`ManageFileStatus` משאירה החלטות של בקשות שמורות, בזמן שסטטוס התיק לא השתנה
   ולא נשלחו events/הודעות. נצפה בפועל: A:3, B:2 נשמרו, התיק נשאר 3, התשובה 500.
2. **`GET ui/AuthenticationRequest/File/{id}` לא מחזיר `organizationUnitId`.** לקוח שקורא את התיק ושומר אותו
   בחזרה מקבל 500 (`organizationUnitId must be greater then 0` ב-event builder). ה-chain שולח את ה-org unit
   שנלכד ביצירת התיק.
3. **`SaveImport` (update) בלי תאריכים → 500** (`datetime2 → datetime out-of-range`): ה-UPDATE נשלח עם תאריך אפס לפני
   שבודקים שהשורה קיימת. תאריך חובה חסר צריך להיות 400.
4. **חיפוש בקשות יצוא בלי פילטר → 500** כשהתוצאה עוברת את תקרת השורות של הפלטפורמה
   (`DbInterceptionException: Result rows count`). צריך 400 או paging.
5. שאלות העסק ב-`lifecycle-map.md` §5 (אין אכיפת מעברים בשרת; UI create בסטטוס 8 מדלג על הפרסום; פרסום אוטומטי
   אחרי 7 לא הומר; `MessageTypeId = 0` ב-feedback).

## 7. Levers applied

- **CHAIN:** 3 collections חדשים.
- **קוד:** תיקון BL אחד (reconcile אחרי מסר).
- **fixtures:** script syntax, assertion ישן, קודי ארץ; ניקוי כל הערכים הקבועים (סעיף 5).
- **DB-STATE:** אין seed — כל collection יוצר את הנתונים שלו.

## 8. Paths

`lifecycle-map.md` · `coverage-baseline.json` · collections: `Postman/postman/collections/CertificateOfOrigins Internal Workload - *`.
תנאי ריצה (אין seed): `node tools/local-lookup-stub.js` + Consul ‏`Main/CentralConfig` → `CustomsDb` = ‏CertificateOfOrigins.
