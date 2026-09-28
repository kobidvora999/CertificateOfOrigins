# Internal-workload report — CertificateOfOrigins

שירות CertificateOfOrigins · 18 collections (`CertificateOfOrigins Internal Workload - *`) · ריצה 2026-09-28 ·
port 9034 · branch `feature/internal-workload-lifecycle` · מקביליות 6 תחת session ‏dotnet-coverage אחד.

## 1. GATE — ⚠️ PASS-WITH-BLOCKERS

**Assertions: 1073/1073 passed** · 18/18 collections · 342 requests · 0 collections ללא assertions.
שני מעברים ⏸ BLOCKED (skip גלוי, לא מכשילים) — סעיף 3.

| | לפני (אותו יום, master) | אחרי |
|---|---|---|
| Collections ירוקים | 11/15 | **18/18** |
| Assertions שנכשלו | 10 | **0** |
| תהליכים עם chain מלא | 0 | **3** |

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

## 5. Inventory

35 endpoints (כולל 4 ‏`[HttpQuery]`). 0 ORPHAN. כל endpoint נקרא לפחות פעם אחת ב-chain על ישות שנוצרה באותה ריצה,
חוץ מ-`Template` (BLOCKED). קריאות literal-id שנותרו ב-collection ‏`API` (smoke) ממשיכות לרוץ, אך אינן ההוכחה לתהליך.

## 6. ממצאים לבירור (לא תוקנו)

1. **SaveFile לא אטומי.** `SaveAuthenticationRequestFile` שומר קודם את ה-child requests (`UpdateFileChildRequest`),
   ורק אחר כך מריץ events. חריגה ב-`ManageFileStatus` משאירה החלטות של בקשות שמורות, בזמן שסטטוס התיק לא השתנה
   ולא נשלחו events/הודעות. נצפה בפועל: A:3, B:2 נשמרו, התיק נשאר 3, התשובה 500.
2. **`GET ui/AuthenticationRequest/File/{id}` לא מחזיר `organizationUnitId`.** לקוח שקורא את התיק ושומר אותו
   בחזרה מקבל 500 (`organizationUnitId must be greater then 0` ב-event builder). ה-chain שולח את ה-org unit
   שנלכד ביצירת התיק.
3. `Export Doc Request/25-search-multi-invoice` — ה-assertion השני מסתיים ב-`|| j.length > 0` (טאוטולוגיה).
4. שאלות העסק ב-`lifecycle-map.md` §5 (אין אכיפת מעברים בשרת; UI create בסטטוס 8 מדלג על הפרסום; פרסום אוטומטי
   אחרי 7 לא הומר; `MessageTypeId = 0` ב-feedback).

## 7. Levers applied

- **CHAIN:** 3 collections חדשים (81 requests, 259 assertions).
- **קוד:** תיקון BL אחד (reconcile אחרי מסר).
- **fixtures:** 3 collections תוקנו (script syntax, assertion ישן, קודי ארץ).
- MOCK/DB-STATE: אין שינויים. `seed_ImportAuthenticationRequests.sql` עדיין נדרש ל-`Auth File Status` בין ריצות.

## 8. Paths

`lifecycle-map.md` · `coverage-baseline.json` · collections: `Postman/postman/collections/CertificateOfOrigins Internal Workload - {Certificate,Import Auth,Export Auth} Lifecycle`.
תנאי ריצה: `node tools/local-lookup-stub.js` + Consul ‏`Main/CentralConfig` → `CustomsDb` = ‏CertificateOfOrigins.
