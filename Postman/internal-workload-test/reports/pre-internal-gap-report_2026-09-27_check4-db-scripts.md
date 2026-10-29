# CHECK 4 — DB scripts completeness (CertificateOfOrigins)

Date: 2026-09-27 · Repo: `C:\Repos\CertificateOfOrigins` (branch master, clean) · Mode: **diagnose only** (no repo file or working-DB change).

- Working DB: `localhost` / `CertificateOfOrigins` (sa, from `appsettings.Development.json`). Consul `ConnectionStrings.CustomsDb` points at `PreRulings`, so it was ignored, as the coordinator instructed. Nothing was written to Consul.
- Scripts: `API/CertificateOfOrigins.WebApi/Scripts/*.sql`, 18 files, all UTF-8 without a BOM.
- Scratch replay DB: `CertificateOfOrigins_ZeroReplay_20260927171745`. It was created, replayed twice and **dropped**. No `ZeroReplay` database is left on the server.
- Timing note: at the start of the check, the DbUp journal did not list `API_20260922093000` (AdministrativeClosure) or `API_20260923104500` (GetTemplateData). The working DB still had the old July `dbo.GetTemplateData`, and the ID=10 rows were missing. At 17:18 the orchestrator's service start ran DbUp and applied both scripts. The final state below is after that run: ID=10 rows are present with correct Hebrew (UNICODE 1505), and the GetTemplateData body is the new one.

## Summary

| # | Check | Status | Findings |
|---|---|---|---|
| 1 | DATA tables (`[Table]` + yaml) | 🔴 FAIL | 20 of 21 match on schema, name and columns. `CRM.CertificateOfOrigins_CustomsItemToExportDocumentAuthenticationRequest` has **no CREATE TABLE** in any script |
| 2 | enum/C lookup tables with seed | ✅ PASS | All 15 `_enum_` and 2 `_c_` tables, plus `_cf_`/`_cl_` config, are created and seeded. Replay row counts equal the working DB, including the ID=10 AdministrativeClosure rows |
| 3 | Stored procedures | ✅ PASS for code; ⚠️ drift | All 11 SPs the code calls, plus the TVP and the sequence, are scripted. The working DB has 26 leftover procedures with no script, none referenced by code |
| 4 | IParametersUtil keys | ✅ PASS | All 14 keys are in the DB and in the seed scripts. 8 seeded keys have no consumer (orphans). 1 DB-only key. 2 values differ between DB and script |
| 5 | Naming / ordering | 🟡 | 5 names do not follow the timestamp pattern. The rest use 14 digits (`yyyyMMddHHmmss`), not the 12-digit `yyMMddHHmmss` the convention asks for. Lexical order still equals the intended run order |
| 6 | From-zero replay | 🟡 runs clean, not IDENTICAL | 0 errors on the first pass. Missing: 1 table (Blocker, see #1). The second pass is not idempotent: the FK block fails |
| 4a+ | Templates | 🔴 FAIL | `ExportAuthenticationSendDeliveryForExporterSouthKoreaTemplate.docx/.yml` is not present anywhere. There is no `Templates/` folder and no `tools/upload-templates.ps1` |

## 1–3. Object table

Platform-owned objects are excluded: `Infrastructure.Parameters*` and `dbo.SchemaVersions*`.

| object | in localhost | has script | status |
|---|---|---|---|
| schema CRM / Shared | yes | create schema.sql | ✅ |
| TVP `Shared.IntArray` (AuthenticationRequestBl:787) | yes | create schema.sql | ✅ |
| sequence `CRM.sq_CertificateOfOrigins_CertificateOfOrigin` | yes (current 117932) | API_20260813183834 (start 116896) | ✅ |
| CRM.CertificateOfOrigins_CertificateOfOrigin | yes | create tables.sql | ✅ columns match the entity. The working DB also has `IsCreateAttachments` and `IsMessageSent` (bit NOT NULL), left by the deleted script `API_20260707 - CertificateOfOrigin add IsCreateAttachments IsMessageSent.sql`. Code does not map them, so this is drift only |
| CRM.CertificateOfOrigins_CertificateOfOriginDetails | yes | create tables.sql | ✅ |
| CRM.CertificateOfOrigins_CertificateOfOriginInvoiceDetail | yes | create tables.sql | ✅ |
| CRM.CertificateOfOrigins_CertificateOfOriginItemDetail | yes | create tables.sql | ✅ |
| CRM.CertificateOfOrigins_CertificateOfOriginVsDeclarationError | yes | create tables.sql | ✅ |
| **CRM.CertificateOfOrigins_CustomsItemToExportDocumentAuthenticationRequest** | yes (154 rows; ID int identity, ExportDocumentAuthenticationRequestID int, CustomsItemID int) | **none** | 🔴 **BLOCKER**. Declared in the entity and the yaml, and written by `CertificateOfOriginsDal.cs:686-712` (the export-request save). From zero, that save fails with "Invalid object name" |
| CRM.CertificateOfOrigins_ExportDocumentAuthenticationRequest | yes | create tables.sql | ✅ |
| CRM.CertificateOfOrigins_ImportAuthenticationFileDetails | yes | create tables.sql | ✅ |
| CRM.CertificateOfOrigins_ImportAuthenticationRequest | yes | create tables.sql | ✅ |
| CRM.CertificateOfOrigins_ItemDetails | yes | create tables.sql | ✅ |
| CRM.CertificateOfOrigins_c_OriginCriterion | yes (18) | create + seed (18) | ✅ |
| CRM.CertificateOfOrigins_c_VerificationProhibitedImporters | yes (0) | create, no seed | ✅ (empty in the DB as well) |
| CRM.CertificateOfOrigins_cf_CertificateOfOriginTypeByTradeAgreement | yes (49) | create + seed (49) | ✅ (no entity) |
| CRM.CertificateOfOrigins_cf_SupplierDeliveryCountryConfig | yes (3) | create + seed (3) | ✅ |
| CRM.CertificateOfOrigins_cl_DetailsPerCertificate | yes (202) | create + seed (202) | ✅ |
| CRM.CertificateOfOrigins_cl_ExportAuthenticationRequestManufacturingArea | yes | create tables.sql | ✅ (transactional data) |
| CRM.CertificateOfOrigins_cl_ExportDocumentAuthenticationRequestLeadDocument | yes | create tables.sql | ✅ (transactional data) |
| CRM.CertificateOfOrigins_enum_AuthenticationFileStatus | yes (10, incl. ID 10) | create + seed 9 + API_20260922093000 (ID 10) | ✅ |
| CRM.CertificateOfOrigins_enum_Decision | yes (10, incl. ID 10) | create + seed 9 + API_20260922093000 (ID 10) | ✅ Hebrew verified in the replay (1505) |
| CRM.CertificateOfOrigins_enum_CertificateDetailsTypeCode | 48 | 48 | ✅ |
| CRM.CertificateOfOrigins_enum_CertificateOfOriginStatusCode | 8 | 8 | ✅ |
| CRM.CertificateOfOrigins_enum_CertificateOfOriginTypeCode | 11 | 11 | ✅ |
| CRM.CertificateOfOrigins_enum_Circumstances | 8 | 8 | ✅ |
| CRM.CertificateOfOrigins_enum_ConstraintTypeEnum | 3 | 3 | ✅ |
| CRM.CertificateOfOrigins_enum_CustomHouse | 1 | 1 | ✅ |
| CRM.CertificateOfOrigins_enum_DeliveryMethod | 5 | 5 | ✅ |
| CRM.CertificateOfOrigins_enum_ExportAuthenticationRequestStatus | 9 | 9 | ✅ |
| CRM.CertificateOfOrigins_enum_ImporterContactingReason | 2 | 2 | ✅ |
| CRM.CertificateOfOrigins_enum_PrefernceDocumentType | 6 | 6 | ✅ |
| CRM.CertificateOfOrigins_enum_ReminderMethod | 1 | 1 | ✅ |
| CRM.CertificateOfOrigins_enum_RequestReasonCode | 9 | 9 | ✅ |
| CRM.General_enum_UIMessage | yes (65) | none (script `API_20260707 - CRM.General_enum_UIMessage copy.sql` was deleted) | 🟡 drift. No code reads it: the message texts are hard-coded in `CertificateOfOriginsBl.MessageValidationMessages.cs` |
| dbo.GetCertificateOfOriginsByFilter | yes | API_20260721 | ✅ |
| dbo.CheckIfExistsAdditionalRequestsForVendor | yes | API_20260722 | ✅ |
| dbo.CheckIfExistsAdditionalRequestsForImporter | yes | API_20260722b | ✅ |
| dbo.GetImportAuthenticationRequestByFilter | yes | API_20260726130203 + API_20260907180000 | ✅ |
| dbo.GetAuthenticationRequestByLeadDocumentID | yes | API_20260726165855 | ✅ |
| dbo.GetCertificateOfOriginByID | yes | API_20260727172359 | ✅ |
| dbo.GetCertificateOfOriginDataForWebQuery | yes | API_20260728122720 | ✅ |
| dbo.GetCertificateOfOriginNumber (DAL literal) | yes | API_20260813183834 | ✅ |
| dbo.GetImportAuthenticationRequestsForReminderForImporterScheduler | yes | API_20260907101500 | ✅ |
| dbo.GetAuthenticationRequestsForScheduler | yes | API_20260907101600 | ✅ |
| dbo.GetTemplateData | yes (new body since 17:18) | API_20260923104500 | ✅ Uses DROP+CREATE, which is idempotent. `@TemplateID=1` matches `ECertificateOfOriginsTemplate.SouthKoreaOriginVerificationLetter = 1` |
| dbo.ExportDocumentAuthenticationRequestSearch | yes | none (script deleted) | 🟡 unused: the DAL is a LINQ port |
| dbo.GetExportDocumentAuthenticationRequestByID | yes | none | 🟡 unused |
| dbo.GetImportAuthenticationFileDetailsAndRequests | yes | none | 🟡 unused |
| dbo.GetImportAuthenticationRequestById | yes | none | 🟡 unused (the DAL method of the same name is LINQ) |
| dbo.UpdateImportAuthenticationRequest | yes | none | 🟡 unused (only a DAL comment mentions it) |
| CRM.usp_CertificateOfOrigins_* (19 procs, incl. `_GetCertificateOfOriginsByFilter_test`, `CROSS_ExportDocumentAuthenticationRequestSearch`, `usp_CertificateOfOrigin_GetCertificateOfOriginDataForWebQuery`) | yes | none (16 scripts deleted in 1188096) | 🟡 legacy copies; code does not call them |
| `CRM.[CRP.usp_CertificateOfOrigins_UpdateImportAuthenticationRequest]` (the proc name itself contains a dot) | yes | none | 🟡 malformed legacy copy |

Column check: I extracted every `[Column]` and scalar property from all `[Table]` entities, including the Partials (223 table.column pairs), and compared them with the replay DB. The only missing columns are the 3 of the missing table. Every entity column also exists in the working DB.

## 4. Parameters

| key (code) | where used | in DB | in script | status |
|---|---|---|---|---|
| AdditionalRequestsForSearchInDays | AuthenticationRequestBl:89 | 10 | add params (10) | ✅ |
| CertificateOfOriginsDocumentsFilter | AuthenticationRequestBl:622 | `329,461` | add params: `124,184,318,185,175,126,18` | 🟡 value differs |
| DaysForReminderForImporterScheduler | Schedulers:33 (const) | 45 | ✅ | ✅ |
| DaysForFirstReminderInAuthenticationRequest3 | Schedulers:93 | 3 | ✅ | ✅ |
| DaysForFirstReminderInAuthenticationRequest1 | Schedulers:94 | 6 | ✅ | ✅ |
| MonthForFinalReminderInAuthenticationRequest | Schedulers:95 | 9 | ✅ | ✅ |
| DaysForFirstReminderInAuthenticationRequest2 | Schedulers:96 | 10 | ✅ | ✅ |
| DaysForFirstReminderInExportAuthenticationRequest1 | Schedulers:97 | 6 | ✅ | ✅ |
| DaysForSecondReminderInExportAuthenticationRequest2 | Schedulers:98 | 10 | ✅ | ✅ |
| IsNeedToLockCertificateOfOrigin | CertificateOfOriginsBl:88 | True | ✅ | ✅ |
| CertificateOfOriginQueryURL | CertificateOfOriginsBl:296/633/1164 | `http://10.218.22.50/...` | `http://10.25.218.28/...` | 🟡 value differs (environment-specific) |
| IssueCertificateOfOriginByWorker | CertificateOfOriginsBl:1440 | False | ✅ | ✅ |
| CountriesExemptedFromSendingThePlaceOfManufacture | MessageCrossField:140, MessageValidation:789 | 804 | ✅ | ✅ |
| CountryIsrael | MessageValidation:838 | 376 | API_20260813180359 | ✅ |

- Orphan seeds (seeded, no code consumer anywhere in API/ or Planar/): CertificateOfOriginsIssuingUser, ExportAuthenticationRelevantDocumentTypes, InvoiceGoodsItemTaxDifferenceMinValue, IsAuthenticationRequestNeedOnVersion, IsCertificateOfOriginsVisible, IsProduceTemplateOfCertificateOfOriginWithSSRS, MaxSumForAftaAndEuroExamptFromOriginCertificate, NumberOfMonthToSearchImportAuthenticationRequest. 🟡
- DB-only: `IsExportDeclarationActive` (True). It came from the deleted script `API_260714183001`. Code mentions it only in comments, as an obsolete parameter. 🟡
- Parameter count: DB 23, replay 22. The difference is IsExportDeclarationActive.
- The seed uses `IF NOT EXISTS` guards, so on a target that already has the key, the script value never overwrites the DB value.

## 5. Naming / ordering

| file | issue |
|---|---|
| `API_20260715 - create schema.sql`, `- create tables.sql`, `- seed data.sql` | date only (8 digits). The three share one prefix, so their run order depends on the words after the dash: "create schema" < "create tables" < "seed data". That happens to be correct, but it is fragile |
| `API_20260716 - add params.sql` | date only |
| `API_20260721 - …`, `API_20260722 - …` | date only |
| `API_20260722b - …` | date plus a letter suffix. It sorts after `API_20260722 - ` because space (0x20) < `b`, so the order is correct |
| all 12-digit-timestamp files | written as `yyyyMMddHHmmss` (14 digits), not `yyMMddHHmmss` as the convention says |

- Lexical (ordinal) order equals the intended run order today, and there are no collisions.
- Hazard: the journal already holds 12-digit names in the `API_26…` form (`API_260705150709` and later, from deleted scripts). A new script named by the skill's `yyMMddHHmmss` rule (`API_26…`) sorts after every `API_2026…` file. A new `API_2026…` file sorts before any `API_26…` file, whatever their dates. The repo should settle on one format.
- History: 16 legacy `CRM.usp_*` scripts and 20 older scripts (`API_20260707…`, `API_2607051507xx…`, `API_260714183xxx…`, `API_20260726160417 - dbo.ExportDocumentAuthenticationRequestSearch`) were deleted after DbUp had already applied them. That is harmless for targets already on the journal. The objects they created survive only in databases that ran them, which is the source of the drift in §1–3.

## 6. From-zero replay log

The baseline was `CREATE SCHEMA Infrastructure` plus a minimal `Infrastructure.Parameters` (Name PK, Description, Value, UpdateDate, UpdateUser, Regex, Level, Active). Then all 18 scripts ran in ordinal order with `sqlcmd -f 65001 -I`.

**Pass 1 (from zero): 18/18 scripts, rc=0, 0 errors.**

Inventory differences, working DB vs replay:
- Present only in the working DB, required by code: **CRM.CertificateOfOrigins_CustomsItemToExportDocumentAuthenticationRequest** (U). 🔴
- Present only in the working DB, not required: CRM.General_enum_UIMessage; the columns CertificateOfOrigin.IsCreateAttachments and IsMessageSent; 5 dbo procs and 21 CRM procs (listed above); the platform tables ParametersHistory, ParametersPeriods, ParametersPeriodsHistory and SchemaVersions*.
- Present only in the replay: nothing.
- enum/C/cf/cl row counts are identical. The Decision and FileStatus ID=10 rows arrive with correct Hebrew.
- Table types: 1 = 1. Sequence start: 116896 in both.

**Pass 2 (re-run on the same DB, idempotency): 17/18 clean. `create tables.sql` failed:**
```
Msg 2714, Level 16, State 5, Server 71472dfd6b32, Line 1175
There is already an object named 'FK_CertificateOfOrigins_c_OriginCriterion_CertificateOfOrigins_enum_CertificateOfOriginTypeCode' in the database.
Msg 1750, Level 16, State 1, Server 71472dfd6b32, Line 1175
Could not create constraint or index. See previous errors.
```
The trailing FK block (lines 1170–1244) is not wrapped in `IF OBJECT_ID(...) IS NULL` guards. The file has no `GO`, so it runs as one batch and the first failure aborts the rest. DbUp runs each script once, so this does not affect DbUp, but it breaks any manual re-apply on a target.

Scratch DB dropped. Verified: 0 databases named `CertificateOfOrigins_ZeroReplay%` remain.

## 4a+. Templates

- `CertificateOfOriginsBl.Templates.cs:74-77` maps template 1 to the name `"ExportAuthenticationSendDeliveryForExporterSouthKoreaTemplate"` (Pdf). The Templates module loads `{Name}.docx` + `{Name}.yml` by that name.
- This repo has **no `Templates/` folder** and no .docx/.yml template pair. `tools/` contains only `local-lookup-stub.js` and `review-markers.ps1`, so there is **no `upload-templates.ps1`** and no deploy mechanism for the MinIO `templates` bucket.
- The Templates module (`C:\Repos\Templates\TemplatesBucket`) holds only InvestigationFollowupMeetingProtocol, LetterOfApprovedCertificateRequest13, PaymentReceipt and PostalParcel, each as .docx+.yml. **The South Korea template is not there.**
- `C:\Repos\Main` has no file by that name either. The only related file is the report SP `Rpt/…/CertificateOfOrigin_GoodsItem_UnitedArabEmirates_SouthKorea.sql`.
- I did not list the live MinIO bucket, because that needs signed S3 calls. The PDF render will fail with "template not found" unless the file was uploaded out of band.

## Findings

**Blocker**
1. `CRM.CertificateOfOrigins_CustomsItemToExportDocumentAuthenticationRequest` has no CREATE TABLE script. It is in the entity and the yaml, the DAL writes to it (export-request save, `CertificateOfOriginsDal.cs:686-712`), and it exists in localhost with 154 rows only because of historical scripts. A from-zero environment cannot save an export authentication request. Suggested fix: a new script `API_<ts> - CRM.CertificateOfOrigins_CustomsItemToExportDocumentAuthenticationRequest.sql` with `IF OBJECT_ID IS NULL CREATE TABLE`, containing ID int IDENTITY PK, ExportDocumentAuthenticationRequestID int NOT NULL, CustomsItemID int NOT NULL, and an FK to ExportDocumentAuthenticationRequest. Take the DDL from the working DB.
2. The template `ExportAuthenticationSendDeliveryForExporterSouthKoreaTemplate` (.docx + .yml) is missing from this repo and from the Templates module, and there is no bucket-deploy script. The CR 194221 print path cannot render in any fresh environment.

**Major**
3. `API_20260715 - create tables.sql` is not idempotent: the unguarded FK block fails on re-apply (Msg 2714). This breaks the skill's manual-apply safety rule. The fix has to go in a new script or a guarded rewrite; do not edit an already-journaled script in place.
4. There is no deploy mechanism for the MinIO `templates` bucket (`tools/upload-templates.ps1` does not exist).

**Minor**
5. The working DB drifts from the scripts: CRM.General_enum_UIMessage (65 rows), CertificateOfOrigin.IsCreateAttachments/IsMessageSent, 5 unused dbo procs and 21 unused CRM legacy procs (one of them named `CRP.usp_…` with an embedded dot). Code does not need any of them. Either drop them locally or record them as intentional.
6. Parameter values differ between DB and seed: `CertificateOfOriginQueryURL` (host 10.218.22.50 vs 10.25.218.28) and `CertificateOfOriginsDocumentsFilter` (`329,461` vs `124,184,318,185,175,126,18`). Confirm which is correct. The `IF NOT EXISTS` seed never corrects an existing value.
7. There are 8 orphan parameter seeds with no consumer, plus the DB-only `IsExportDeclarationActive` (obsolete).
8. Naming: 5 files are date-only or date+letter, and the timestamp is 14 digits while the convention says 12. Mixing this with the journal's `API_26…` names makes the ordinal order depend on the format. Today's order is correct.
9. `dbo.GetTemplateData` is called with `TemplateID`/`EntityID` while the old DB body used `@TemplateId`/`@EntityId`. The new script uses `@TemplateID`/`@EntityID`, which matches the code. No action needed now that DbUp has applied it.
