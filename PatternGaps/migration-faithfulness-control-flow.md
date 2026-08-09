# PatternGap: נאמנות זרימת-בקרה ומצב בהמרת WCF → .NET 10

תאריך: 2026-08-09 · שירות: CertificateOfOrigins · מתודות: `SaveCertificateOfOrigin` (#33), `UpdateCertificateOfOrigins` (#34)
מקור לגאסי: `C:\Repos\Main\CRM\CertificateOfOrigins\Server\Customs.CRM.CertificateOfOrigins.BL\CertificateOfOriginsBL.cs` + `RaiseEventUtil.cs`
נמצא ב: סקירת רגרסיות + נאמנות-לוגית של #33/#34 (code review מול הלגסי).

## למה זה PatternGap (ולא באג נקודתי)
לא מדובר בפטרן לא-מזוהה, אלא ב**קבוצת מלכודות-נאמנות חוזרות** בהמרה — כולן וריאציות של אותו שורש: **תרגום זרימת-בקרה וסמנטיקת-מצב של קוד legacy stateful (EF change-tracking, commit-once, unit-of-work) למודל .NET 10 חסר-מצב מאבד בשקט את מה שהמצב הלגאסי הניח.** 5 באגים אמיתיים נמצאו ותוקנו + 2 ממצאים נדחו כנאמנים. המלצה: לעגן את הכללים ב-`_shared/bl-rules.md` ובצ'קליסט של `net10-code-review` כדי שזה ייתפס אוטומטית בכל שירות.

## טבלת ממצאים
| # | חומרה | קטגוריה | תמצית |
|---|---|---|---|
| 1 | HIGH | A1 change-tracker | `isStatusChanged` תמיד true לתעודה חדשה → publish/events שקריים |
| bonus | HIGH | A2 events-at-commit | אירוע שמפנה ליישות חדשה הועלה לפני ה-save (`entityId=0`) → 500 על כל supersede |
| 2 | MED | B1 emptiness | רשימת goods ריקה בהצהרה → `!Any` → mismatch שקרי → Rejected |
| 5 | MED | B2 null-guard | null-guard סביב lookup הפך "אי אפשר לבדוק"→"עובר" במקום "הפרה" |
| 4 | MED | B3 guard-scope | משמר `!= Cancelled` הורחב על כל הבלוק → שתי שורות `IsLastVersion=true` |
| 6 | LOW | C1 deferral-scope | טקסט resx דחוי → דילוג על כל payload ה-`AdditionalInfo` (גם המבנה) |
| 3 | — | D1 (נדחה) | "השמטה" שהתבררה נאמנה — `certificateToUpdateId=null` באתר-הקריאה |

---

## A. legacy stateful → .NET 10 stateless

### A1 — ענף "יישות חדשה" של change-tracker הושמט (#1, HIGH)
לגאסי `CheckIfStatusChangedAndHandleChanges` (`CertificateOfOriginsBL.cs:1054-1079`):
```csharp
if (statusOriginalValue != null)                         // יישות קיימת (יש ערך-מקור נרשם)
    isStatusChanged = current != original;
else                                                     // instance חדש — אין ערך-מקור
    isStatusChanged = (status == Received);              // ← כלל נפרד
```
**הבאג:** ההמרה החליפה את ה-change-tracker בשדה DTO `OriginalCertificateOfOriginStatusId` וכתבה השוואה **אחת** `status != OriginalStatus`. לתעודה חדשה השדה=0 → תמיד true → תעודה חדשה ב-Published/PendingRelease הריצה publish/feedback/events שהלגסי לא מריץ. ה-`!= null` הוא **מבחן חדש-מול-קיים**, לא null-guard — וה-`else` שלו הוא כלל נפרד שהושמט.
**התיקון:** `entity.Id == 0 ? status == Received : status != OriginalStatus`.
**כלל:** בהמרת `ChangeTracker.OriginalValues`/`IsNewInstance()` — לשחזר את **שני** הענפים; הדיסקרימינטור ב-.NET 10 הוא `entity.Id == 0`.

### A2 — אירוע שמפנה ליישות החדשה הועלה לפני ה-save (bonus, HIGH)
לגאסי `SaveCertificateOfOrigin` (`CertificateOfOriginsBL.cs:953`) מעלה `ApplicationCorrected(certificateOfOrigin)` בתוך ה-supersede (לפני commit); ב-EF ה-Id מוקצה ב-`SaveChanges` (commit), וה-EventUtil צובר ומעלה ב-commit.
**הבאג:** ב-.NET 10 האירוע קם **מיד**, וה-builder זורק `field 'entityId' must be greater then 0` כי `entity.Id` עדיין 0. תוצאה: 500 על **כל** supersede-עם-גרסה-קודמת (לא נבדק כי בדיקות קודמות השתמשו במספרים ייחודיים).
**התיקון:** להעלות אירועים שמפנים ליישות החדשה **אחרי** ה-save המפורש (כשה-Id מוקצה).
**כלל:** legacy צובר אירועים ומעלה ב-commit; ב-.NET 10 (raise מיידי, builder דוחה `entityId=0`) — אירוע שמפנה ליישות החדשה קם אחרי ה-save.

---

## B. משמרים/ריקות שאסור שיהפכו סמנטיקה

### B1 — משמר-ריקות של צד ההצהרה הופל (#2, MED)
לגאסי `ValidateExportDeclarationInfoForPCIsMatch` (`CertificateOfOriginsBL.cs:804`) עוטף את בדיקות ה-goods-item ב:
```csharp
if (!invoice.CertificateOfOriginItemDetail.IsNullOrEmpty() && !invoiceFromDealFile.ExportGoodsItemInfoDTOList.IsNullOrEmpty())
```
**הבאג:** ההמרה שמרה את הבדיקות הפנימיות אך הפילה את משמר-הריקות של צד-ההצהרה. עובדה קריטית: **`!collection.Any(pred)` הוא `true` על אוסף ריק** — חשבונית-הצהרה תואמת עם goods ריקים ייצרה `OriginCountryMismatch`+`CertificateNumberNotInDealFile` שקריים (Error) → Rejected שגוי.
**התיקון:** `if (declarationInvoice.ExportGoodsItemInfoList.Count == 0) continue;` לפני לולאת ה-goods.
**כלל:** בהמרת `!X.Any(...)` — לזכור ש-`!Any` על ריק = true; לשמר את משמרי-הריקות של הלגסי.

### B2 — null-guard סביב lookup שינה סמנטיקה (#5, MED)
לגאסי (`CertificateOfOriginsBL.cs:740-751`): כשיש `DestinationGroupOfCountries` — מבצע lookup של הזוג (destination-country, group); כש-`DestinationCoutryID` הוא null, ה-predicate לא מוצא כלום → **מוסיף** discrepancy.
**הבאג:** ההמרה הוסיפה `request.DestinationCountryId.HasValue &&` לפני קריאת ה-proxy (כי אי אפשר לקרוא עם null) — וזה הפך את null ל**דילוג** (עובר), במקום ל**הפרה**.
**התיקון:** כשהקבוצה מוגדרת: `inGroup = HasValue && await IsCountryInCountryGroup(...); if (!inGroup) add discrepancy;` — null → discrepancy.
**כלל:** null-guard סביב proxy/lookup אסור שיהפוך בשקט "אי אפשר לבדוק"→"עובר"; להחליט מפורשות מה null אומר בלגסי.

### B3 — היקף משמר הורחב (#4, MED)
לגאסי `SaveCertificateOfOrigin` (`CertificateOfOriginsBL.cs:946-968`): הבלוק (cancel + `IsLastVersion=false` + version + events) רץ **תמיד** כשקיימת גרסה קודמת; **רק** שורת `CertificateOfOriginIdOfReplacement` מוגדרת ב-`!= Cancelled`.
```csharp
if (certificateToCancel.CertificateOfOriginStatusID != Cancelled)
    certificateOfOrigin.CertificateOfOriginIdOfReplacement = certificateToCancel.ID;   // ← רק זה מוגדר
certificateToCancel.CertificateOfOriginStatusID = Cancelled;                            // ← תמיד
certificateToCancel.IsLastVersion = false;                                              // ← תמיד
```
**הבאג:** ההמרה עטפה את **כל** הביטול ב-`if (previous.status != Cancelled)` → גרסה שכבר Cancelled נשארה `IsLastVersion=true` → שתי שורות גרסה-אחרונה.
**התיקון:** לבטל/לנקות IsLastVersion תמיד; לגדר רק את שיוך ה-replacement-id.
**כלל:** למפות כל משמר legacy **בדיוק** לשורות שהוא עוטף — לא להרחיב `if(X){a;} b; c;` ל-`if(X){a;b;c;}`.

### B4 — INNER JOIN חוצה-שירות שהוסר מ-SP, בלי לשמר את סינון-השורות (search SP, MED, SQL)
לגאסי `usp_..._ExportDocumentAuthenticationRequestSearch` השתמש ב-3 `INNER JOIN` חוצי-סכימה (Country/Customer/ExporterCustomer).
**הבאג:** ההמרה הסירה את ה-JOINs (הטבלאות שייכות לשירותים אחרים; עמודות-השם הועברו ל-proxy/lookup enrichment) — **נכון** לגבי העמודות, אבל ל-`INNER JOIN` יש **שתי** השפעות: (א) חשיפת עמודות, ו-(ב) **סינון שורות** (החרגת שורות שה-FK שלהן NULL/לא-תואם). ההמרה שימרה (א) והשמיטה בשקט את (ב). כיוון ש-CountryID/ExporterCustomerID nullable → שורות עם FK ריק שהלגסי החריג התחילו להופיע (הוכחה חיה: שורה 1003). CustomerID הוא NOT NULL → ה-JOIN שלו לא החריג כלום.
**התיקון:** `AND EAR.CountryID IS NOT NULL AND EAR.ExporterCustomerID IS NOT NULL` ב-`@Where` (שקול ל-INNER JOIN על FK nullable). ⚠️ ב-dynamic SQL — לוודא שה-`@Where` מסתיים ב-newline כדי לא להידבק ל-`@Filter`/`@OrderBy` (נתקלנו ב-`NULLORDER`).
**כלל:** בהמרת SP שמסיר JOIN חוצה-שירות — לסווג `LEFT` (עמודות בלבד → בטוח) מול `INNER` (**גם מסנן** → לשמר `AND fk IS NOT NULL`/EXISTS). לאמת set-membership (כמה/אילו שורות), לא רק עמודות.

---

## C. היקף דחייה

### C1 — דחיית טקסט resx בלעה את המבנה (#6, LOW)
לגאסי (`CertificateOfOriginsBL.cs:549-577`) בונה `AdditionalInfo` = שרשור טקסטי-החריגות (capped: `len + LengthOfTaskStart(70) < MaximumNumberOfCharactersOfTheField(253)`) ומצרף לאירועי Mismatch/HasWarnings.
**הבאג:** מכיוון שהטקסטים דחויים ל-resx, ההמרה העבירה `null` ל-`AdditionalInfo` — ובכך דילגה על כל ה-payload, גם על ה**מבנה** (שרשור+cap) שאינו תלוי-resx.
**התיקון:** לבנות את המבנה עם הטקסטים ה-placeholder (יִשְׁתַּפֵּר אוטומטית כשה-resx ינחת).
**כלל:** דחיית **ערך** ≠ דילוג על ה**מבנה** שמרכיב אותו.

---

## D. משמעת סקירה (ל-net10-code-review)

### D1 — לאמת ממצא-השמטה מול אתר-הקריאה האמיתי (#3 + Cancelled — נדחו)
שני ממצאים נראו כהשמטות אך התבררו **נאמנים**: `ApplicationCorrected` בענף Received מוגדר ב-`certificateToUpdateId.HasValue` (`RaiseEventUtil.cs:209`), ואתר-הקריאה ה-Internal (`CertificateOfOriginsInternalServicePartial.cs:59`) מעביר `certificateToUpdateId=null` **וגם** `requestReason=null` — לכן האירוע אף פעם לא קם, וה-Cancelled תמיד `UserCancelledCertificate`. "תיקון" ההשמטה היה יוצר קוד **לא**-נאמן.
**כלל:** לפני "תיקון" ממצא-השמטה — לאמת מול אתר-הקריאה האמיתי אם הערך שמפעיל את הקוד בכלל מגיע (guards כמו `x.HasValue`, פרמטרים שמועברים null).

---

## המלצת עדכון סקילים (קונקרטי)
1. **`_shared/bl-rules.md`** — תת-פרק חדש **"נאמנות זרימת-בקרה ומצב (legacy stateful → .NET 10 stateless)"** עם הכללים A1, A2, B1, B2, B3, C1, משפט-פתיחה = "התמה המאחדת" למעלה.
2. **`db-proc`** — כלל B4: בהמרת SP שמסיר JOIN חוצה-שירות, לסווג `LEFT` (עמודות → בטוח) מול `INNER` (**גם מסנן** → לשמר `AND fk IS NOT NULL`/EXISTS על FK nullable); לאמת set-membership, לא רק עמודות. ⚠️ ב-dynamic SQL — `@Where` חייב להסתיים ב-newline (הימנעות מ-`NULLORDER`).
3. **`net10-code-review`** — צ'קליסט:
   - [ ] כל `ChangeTracker.OriginalValues`/`IsNewInstance` — שני הענפים שוחזרו (`Id==0`)?
   - [ ] אירוע שמפנה ליישות חדשה — קם **אחרי** ה-save?
   - [ ] כל `!X.Any(...)` — משמרי-הריקות של הלגסי נשמרו?
   - [ ] כל null-guard סביב proxy/lookup — null/ריק מתנהג כמו בלגסי (דילוג מול הפרה)?
   - [ ] כל משמר legacy — ממופה בדיוק לשורות שהוא עוטף (לא הורחב)?
   - [ ] SP שהוסר ממנו `INNER JOIN` חוצה-שירות — שומר `AND fk IS NOT NULL`/EXISTS (set-membership)?
   - [ ] payload שמורכב מ-value דחוי (resx) — המבנה נבנה עם placeholder ולא דולג?
   - [ ] כל ממצא-השמטה — אומת מול אתר-הקריאה האמיתי (guards / פרמטרים null)?
4. **`wcf-migrate`** — הפניה לתת-הפרק החדש בשלב BATCH 3 (שחזור גוף ה-BL לפי BODY_SEQUENCE).
