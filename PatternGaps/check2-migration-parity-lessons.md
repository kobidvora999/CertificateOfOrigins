# PatternGap: לקחי CHECK 2 — מלכודות פאריטי חוזרות במעבר WCF→.NET10

תאריך: 2026-08-18 · מקור: audit אדוורסרי מלא (7 סוכנים) על 34 המתודות המומרות ב-CertificateOfOrigins,
מול קוד ה-WCF הלגסי ב-`C:\Repos\Main\CRM\CertificateOfOrigins\Server`.

**למנהל ה-skills (קובי):** 5 המחלקות למטה חזרו על עצמן במתודות שונות. כל אחת מצביעה על כלל חסר ב-skill
ספציפי. מומלץ להטמיע את הכללים ב-`_shared/` + ה-skill הרלוונטי, ולחווט את ה-parity fan-out הזה כשלב
אוטומטי ב-`repo-complete-check` CHECK 2 (עד היום לא רץ כ-fan-out מלא — וזה מה שתפס את כל אלה).

---

## לקח 1 — דריסת עמודות ב-`Context.Update` עם DTO מוקרן  ⟶  net10-dal / _shared/dal-rules.md

**הבאג (HIGH, SaveExportDocumentAuthenticationRequest):** ה-read (`GetById`) מקרין רק 29 עמודות (מגבלת
`MaxCountExceededInterceptor` ≥30), ומשמיט `State`/`OrganizationUnitId`. ה-write עשה `Context.Update(entity)`
מלא — כך שהערכים שלא הוקרנו חזרו כ-0 מהלקוח ונדרסו ב-DB בכל עדכון של רשומה קיימת.

**השורש:** read-projection ≠ write-columns. ה-DAL הגן רק על `CreateDate`/`CreateUserId`, לא על שאר העמודות המושמטות.

**כלל ל-skill:** בכל `Context.Update(entity)` שמקבל DTO מוקרן — **כל עמודה שלא נכללת ב-DTO ה-read חייבת
`Context.Entry(entity).Property(x => x.Col).IsModified = false`**. צ'קליסט אכיפה: `read-projection columns == write columns`,
אחרת guard מפורש לכל הפרש. (התיקון שהוחל: הוספת guard ל-State + OrganizationUnitId.)

---

## לקח 2 — null-guard דיפולטיבי שהופך fail-fast ל-silent  ⟶  wcf-migrate / net10-code-review

**הבאגים (Medium/Low, מספר מתודות):** הלגסי זרק/NRE על נתון עסקי פגום (collateral בלי `RelatedEntity`;
`decisionId` לא-ממופה; delivery על תיק לא-מקושר) — הפעולה נכשלה, כלום לא נשמר. ה-.NET10 הוסיף `?? 0` /
`?? string.Empty` / `if (x.HasValue)` והמשיך — כתב `RelatedEntityId=0` פגום, שלח הודעה עם שם ריק, או שמר חלקית.

**השורש:** תוך המרה מוסיפים null-guard כדי "להרגיע" NRE/warning — אבל זה משנה סמנטיקה: שגיאה קשה הופכת
להצלחה-שקטה עם נתון שגוי (הפרה של "אל תשנה התנהגות בשקט").

**כלל ל-skill:** null-guard שלא היה בלגסי חייב הצדקה מתועדת. אם הלגסי הסתמך על ה-NRE/throw כ-fail-fast על
נתון עסקי — **לזרוק `RestValidationException` ב-.NET10, לא `?? default`**. (הכרעה רוחבית — לקובי: מדיניות אחידה.)

---

## לקח 3 — set-based writes בלי טרנזקציה עוטפת  ⟶  _shared/dal-rules.md / db-writer-pattern

**הבאג (HIGH, SaveAuthenticationRequestFile ואחרים):** הלגסי עבד עם UoW + `CommitAllChanges()` אחד — כל
הכתיבות (שורות-ילד + סטטוס-אב + side-effects) בטרנזקציה אחת, all-or-nothing. ה-.NET10 עבר ל-`ExecuteUpdateAsync`
פר-ישות, שכל אחד מתחייב מיד ובנפרד, בלי טרנזקציה עוטפת → כשל באמצע משאיר מצב חלקי, ו-retry מדלג על הודעות.

**כלל ל-skill:** מתודת Save עם **>1 כתיבה** (child + parent + proxy side-effects) חייבת לעטוף ב-
`await using var tx = await Context.Database.BeginTransactionAsync(); … await tx.CommitAsync();` — כתחליף מפורש
ל-`CommitAllChanges` הלגסי. (הכרעה ארכיטקטונית — לקובי.)

---

## לקח 4 — guard מותנה שהושמט ב"פישוט"  ⟶  wcf-migrate (אכיפה, לא רק כלל)

**הבאג (Medium, UpdateCertificateOfOrigins):** הלגסי עשה backfill ל-`LeadDocumentID` דו-מסלולי, כולל תנאי
`item.ExportDeclarationNumber == dto.ExportDeclarationNum`. ה-.NET10 קיצר ל-`certificate.LeadDocumentId ?? request.LeadDocumentId`
— הסיר את בדיקת התאמת-המספר, כך שתעודה עלולה להתקשר להצהרה הלא-נכונה.

**השורש:** תנאי רב-ענפי "פושט" לביטוי אחד קצר שנראה שקול אך משמיט guard.

**כלל ל-skill:** `wcf-migrate` כבר אומר "אל תפשט לוגיקה / שמור מבנה תנאים" — אבל צריך **אכיפה**: ה-parity
fan-out (CHECK 2) הוא מנגנון האכיפה. (התיקון שהוחל: החזרת שני המסלולים.)

---

## לקח 5 — מטא-דאטה של אירועים שהושמט ב-builder  ⟶  net10-eventutil

**הבאגים (Major/Medium, SaveCertificateOfOrigin + SaveImportAuthenticationRequest):** אירועי Rejected/Cancelled
איבדו את `AdditionalInfo` (‏RejectCancelReason); אירוע על ענף AuthenticationNeedless איבד `OrganizationUnitId`.

**השורש:** ה-builder של `IEventUtil` הוא opt-in לכל שדה (`WithAdditionalInfo`, `WithOrganizationUnitId`, ...),
בעוד הלגסי בנה `EventUtilArguments` שנשא את השדות מהישות. קל לשכוח שדה.

**כלל ל-skill:** `net10-eventutil` — צ'קליסט חובה: לכל `EventUtil.RaiseEvent` לגסי, למפות **כל שדה** של
`EventUtilArguments` (במיוחד `AdditionalInfo`, `OrganizationUnitID`, `RelatedEntities`), לא רק EventType+EntityId.
(התיקון שהוחל: RejectCancelReason חזר לאירועי Rejected/Cancelled.)
