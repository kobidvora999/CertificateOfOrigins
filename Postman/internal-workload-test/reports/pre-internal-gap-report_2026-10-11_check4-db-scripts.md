# CHECK 4 (re-check) — DB scripts completeness (CertificateOfOrigins)

Date: 2026-10-11 · Repo: `C:\Repos\CertificateOfOrigins` (master, clean before and after) · Mode: diagnose only. No repo file and no working-DB object was changed.

- Working DB: `localhost` / `CertificateOfOrigins`. `sqlcmd -S localhost -E` (Windows auth) is **rejected**: "Login failed for user '...\Guest'". The server is SQL Server 2022 running in a Linux container. I used the `sa` login from `API/CertificateOfOrigins.WebApi/appsettings.Development.json`, as the previous check did.
- Scripts: `API/CertificateOfOrigins.WebApi/Scripts/*.sql`, now **21 files** (previously 18). The 3 new ones are API_20260928120000, API_20261008110054 and API_20261008110055.
- Scratch DB: `CertificateOfOrigins_FromZero_20261011140942`. I created it, replayed every script twice, ran an extra test of the parameter seed against the real platform table shape, then **DROPPED** it. I checked `sys.databases` afterwards: only master, tempdb, model, msdb and CertificateOfOrigins remain.
- The DbUp journal in the working DB lists all 21 current scripts. The deployed bodies of all 11 scripted procedures match the replay, ignoring whitespace.

## Summary

| # | Check | Status | Findings |
|---|---|---|---|
| 1 | DATA tables (`[Table]` + yaml + Workers) | ✅ PASS | All 22 `[Table]` entities have a CREATE TABLE with the exact same schema and name. Column inventory: the replay equals the working DB for every CRM table, with no diff |
| 2 | enum/C lookup tables + seed | ✅ PASS | 15 `_enum_`, 2 `_c_` and 2 `_cf_` tables. Replay counts equal the working DB. Decision and AuthenticationFileStatus ID=10 arrive with correct Hebrew (UNICODE 1505). Small difference: `_c_VerificationProhibitedImporters` has 1 row in the DB (ID 1, value 999, test data) and 0 in the scripts |
| 3 | Stored procedures | ✅ PASS for code; 🟡 drift | All 11 SPs the code calls, plus TVP `Shared.IntArray` and the sequence, are scripted. Bodies are identical to the working DB. The working DB still holds 22 leftover procedures with no script, none called by code |
| 4 | IParametersUtil keys | ✅ PASS | All 14 keys are in the DB and in the seed. Both values reported in the last check now match. A **new** value differs: `IssueCertificateOfOriginByWorker`. 8 seeded keys have no consumer, and 1 key exists only in the DB |
| 5 | Naming / ordering | 🟡 | Unchanged: 5 files carry a date only or date+letter. The rest use 14-digit timestamps, ascending, with no collisions. The new files follow the 14-digit form |
| 6 | From-zero replay ×2 | ✅ **PASS** | Pass 1: 21/21 rc=0. **Pass 2 on the same DB: 21/21 rc=0, so the scripts are idempotent.** Replay = working DB on tables, columns, procedures the code needs, the TVP and the sequence |
| W | Workers + Planar | ✅ PASS | They need no DB object or parameter beyond what the scripts provide (details below) |
| 4a+ | Templates | 🟡 partly fixed | The .docx/.yml pair is now in `Templates/`. There is still no `tools/upload-templates.ps1` and no other bucket-deploy mechanism |

## Previous findings: re-verification

| Previous finding | Now | Evidence |
|---|---|---|
| 🔴 `CRM.CertificateOfOrigins_CustomsItemToExportDocumentAuthenticationRequest` had no script | ✅ **FIXED** | `API_20260928120000 - …table.sql` adds an `IF NOT EXISTS` CREATE with ID identity PK, the 2 int columns, a UQ on the pair, and an FK to ExportDocumentAuthenticationRequest. The columns match the entity and the working DB. ⚠️ The FK is inside the `IF NOT EXISTS … BEGIN … END` block (lines 5-26), so it is never added to a DB where the table already existed. localhost is such a DB: it has the PK and UQ but **no FK** |
| 🟠 `API_20260715 - create tables.sql` failed on re-run (Msg 2714) | ✅ **FIXED** | Commit 3a69173 put `IF OBJECT_ID(...,'F') IS NULL` around every FK (23 guards, line 1177 and onwards). Pass 2 ran clean. Process note: the commit **edited an already-journaled script in place**, against the skill rule "fix = new script". It is harmless here because DbUp keys on the file name and the change only adds guards, but the rule was not followed |
| 🔴/🟠 South Korea template missing, no `upload-templates.ps1` | 🟡 **half fixed** | `Templates/ExportAuthenticationSendDeliveryForExporterSouthKoreaTemplate.docx` and `.yml` are present and match `CertificateOfOriginsBl.Templates.cs:93`. `@TemplateID = 1` in the GetTemplateData script (line 23) equals `ECertificateOfOriginsTemplate.SouthKoreaOriginVerificationLetter = 1`. **`tools/` still has only `local-lookup-stub.js` and `review-markers.ps1`.** There is no upload script and nothing in the repo deploys the pair to the Templates bucket. `Templates/README.md` still marks the template id `TODO(confirm)` and says the template has no letterhead, while the .yml header says it uses the same letterhead header/footer. The two contradict each other |
| 🟡 `CertificateOfOriginQueryURL` and `CertificateOfOriginsDocumentsFilter` differ between DB and seed | ✅ resolved | DB now equals the seed: `http://10.25.218.28/ShaarOlami2/Dev/CertificateOfOrigin?guid={0}` and `124,184,318,185,175,126,18` |

## New scripts API_20261008110054 / 110055 (date bounds)

- `110054` changes `dbo.GetCertificateOfOriginsByFilter`. A diff against `API_20260721` shows only the header comment and lines 92 and 98 changed. The upper bound is now `DATEADD(SECOND, 86399, CAST(CAST(@to… AS DATE) AS DATETIME))`, which is 23:59:59.000, the same as legacy `ufn_General_GetDateEnd`. It was 23:59:59.997 before. The lower bound `>= CAST(@from AS DATE)` is unchanged. The `IS NOT NULL` gating is kept.
- `110055` changes `dbo.GetImportAuthenticationRequestByFilter`. A diff against `API_20260907180000` (multi-invoice) shows only the header and line 39 changed. The multi-invoice STRING_SPLIT logic carries over. `@ToRequestDate` is declared `DATETIME` (line 14), so the SET keeps the time part and nothing is truncated. The `BETWEEN` at line 69 is unconditional, as in the previous version, so behavior is unchanged when a bound is NULL.
- Both use `CREATE OR ALTER`, both are idempotent (they ran twice), and both are already applied on localhost with identical bodies. Note that `110055` dropped the multi-invoice rationale comment from its header. That is cosmetic.

## Workers and Planar: DB and parameter needs

- `Workers/IssueCertificateOfOrigin.Worker` reads and updates only `CRM.CertificateOfOrigins_CertificateOfOrigin` through EF (`Dal/IssueCertificateDal.cs`). That is the same table the API uses, and it uses connection string `CustomsDb`. All 36 mapped columns in `Models/CertificateOfOriginsCertificateOfOrigin.cs`, including `IsInPublishingProcess` and `TimeStamp`, exist in the replay DB. There are no SPs, no TVPs and no `IParametersUtil` calls.
  - `CustomsDbContext.cs:29-92` declares about 40 `HasSequence` entries in other schemas (StockPileData, Logistics, CRP, _Admin_…), scaffold leftovers from Customs_Pilot. EF needs them only for migrations, and no code calls `NextValueFor`, so no DB object is required. 🟡 This is noise, and could be removed in a cleanup.
  - The worker reads `CertificateOfOriginQueryURL` from **appsettings** (`AppSettings.CertificateOfOriginQueryURL`, `IssueCertificateOfOriginHandler.cs:123`), not from Parameters. The same key therefore lives in two places: Workers appsettings.* and the DB parameter used by the API at `CertificateOfOriginsBl.cs:361/692/1237`. They agree today on Dev (10.25.218.28). 🟡 They could drift apart.
  - RabbitMQ exchange and queue `IssueCertificateOfOrigin` (+ `.Errors`) are created by `Program.cs` queue migration. That is not a DB concern.
- **Planar** (`AuthenticationRequestReminder` and `ReminderForImporterScheduler`) reuses `AuthenticationRequestBl` through the BL `ServicesConfiguration`. Its DB needs are `dbo.GetAuthenticationRequestsForScheduler`, `dbo.GetImportAuthenticationRequestsForReminderForImporterScheduler` and the 7 scheduler parameters (`CertificateOfOriginsConsts.cs:110-127`). All of them are scripted and seeded. ✅

## Parameters (14 code keys)

All of these are in the DB and the seed: AdditionalRequestsForSearchInDays, CertificateOfOriginsDocumentsFilter, DaysForReminderForImporterScheduler, DaysForFirstReminderInAuthenticationRequest1/2/3, MonthForFinalReminderInAuthenticationRequest, DaysForFirstReminderInExportAuthenticationRequest1, DaysForSecondReminderInExportAuthenticationRequest2, IsNeedToLockCertificateOfOrigin, CertificateOfOriginQueryURL, IssueCertificateOfOriginByWorker, CountriesExemptedFromSendingThePlaceOfManufacture, CountryIsrael.

- 🟡 **Value differs:** `IssueCertificateOfOriginByWorker` is `true` in the DB but `False` in the seed (`API_20260716 - add params.sql:417`). The worker path (`CertificateOfOriginsBl.cs:1564`) is therefore off in any fresh environment. Someone needs to confirm the intended default now that the Workers host exists.
- Orphan seeds, unchanged (8): CertificateOfOriginsIssuingUser, ExportAuthenticationRelevantDocumentTypes, InvoiceGoodsItemTaxDifferenceMinValue, IsAuthenticationRequestNeedOnVersion, IsCertificateOfOriginsVisible, IsProduceTemplateOfCertificateOfOriginWithSSRS, MaxSumForAftaAndEuroExamptFromOriginCertificate, NumberOfMonthToSearchImportAuthenticationRequest.
- DB-only: `IsExportDeclarationActive`, now `false` (it was True). It is obsolete. Count: 23 in the DB, 22 in the replay.
- I also ran both seed scripts against a copy of the real platform shape (Name varchar(255) PK; Description, Value, UpdateDate and Active NOT NULL; UpdateUser varchar). rc=0, 22 rows, no `?` corruption in the Hebrew descriptions.

## Replay diff, working DB vs scratch (after pass 2)

- Only in the working DB, not needed: 13 `CRM.usp_CertificateOfOrigins_*` procedures, 5 dbo procedures (ExportDocumentAuthenticationRequestSearch, GetExportDocumentAuthenticationRequestByID, GetImportAuthenticationFileDetailsAndRequests, GetImportAuthenticationRequestById, UpdateImportAuthenticationRequest), 4 `dbo.usp_*` copies, and the platform tables Parameters*History/Periods. `CRM.General_enum_UIMessage` and the `IsCreateAttachments`/`IsMessageSent` columns are **gone** from the working DB now, so that earlier drift is resolved.
- **Only in the replay: 9 FKs** that the working DB lacks:
  - c_OriginCriterion→enum_CertificateOfOriginTypeCode
  - CertificateOfOriginItemDetail→c_OriginCriterion
  - CertificateOfOriginItemDetail→CertificateOfOriginInvoiceDetail
  - cl_DetailsPerCertificate→3 enums
  - cl_ExportAuthenticationRequestManufacturingArea→ExportDocumentAuthenticationRequest
  - cl_ExportDocumentAuthenticationRequestLeadDocument→ExportDocumentAuthenticationRequest
  - CustomsItemToExportDocumentAuthenticationRequest→ExportDocumentAuthenticationRequest

  🟡 This is localhost drift, but it means the local Postman and coverage runs never exercised these constraints. A fresh environment enforces them.
- Columns, enum/C counts, the TVP and the sequence start (116896) are identical. Default and PK names differ only in system-generated suffixes.

## Findings

**Blocker:** none.

**Major**
1. No deploy mechanism for the Templates bucket. `tools/upload-templates.ps1` is still missing. The template pair is now in `Templates/`, but a fresh environment cannot render CR 194221 unless someone uploads it by hand.

**Minor**
2. `API_20260928120000 …table.sql:5-26`: the FK sits inside the `IF NOT EXISTS` table block, so DBs that already had the table (localhost) never get it. Add a separately guarded `IF OBJECT_ID(FK) IS NULL ADD CONSTRAINT` in a new script if the FK matters there.
3. `IssueCertificateOfOriginByWorker`: DB `true` vs seed `False` (`API_20260716 - add params.sql:417`). Confirm the default.
4. The working DB lacks 9 FKs that the scripts create, so local testing never hit them. The working DB still holds 22 unscripted legacy procedures, plus a test row in `_c_VerificationProhibitedImporters`.
5. Commit 3a69173 edited the journaled `API_20260715 - create tables.sql` in place, against the "new script" rule. Harmless here.
6. Unchanged from the last check: 8 orphan parameter seeds, DB-only `IsExportDeclarationActive`, 5 date-only file names.
7. Workers: `CertificateOfOriginQueryURL` is duplicated between appsettings and the DB parameter. `CustomsDbContext.cs` carries about 40 scaffolded foreign-schema `HasSequence` entries.
8. `Templates/README.md` contradicts the .yml on the letterhead, and the template id is still `TODO(confirm)`.
