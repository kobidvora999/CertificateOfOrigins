# Template migration — EUR1 pilot (PAUSED)

**Status:** paused 2026-08-07, pending a decision from Kobi (skills owner).
**Open question:** the legacy issues certificates via **SSRS (ReportId)** when the type has a ReportId — and **all 8
certificate types have a ReportId** (7000–7006). So migrating the **.docx** templates may be unnecessary; the real
target might be reconstructing the SSRS reports instead. Do not resume the .docx template migration until this is settled.

## What was built (compiles green; additive — does not change existing behaviour)

Generic template infrastructure (first template pipeline in this repo):
- `Model/ModelDTOs/CertificateOfOriginEUR1Result.cs` — data contract (+ `CertificateOfOriginEUR1GoodsLine` list).
- `Model/ModelDTOs/PrintTemplateDto.cs` — `{Name, Data(camelCase JSON), Format}`.
- `DAL.GetTemplateData<T>` — generic `QueryMultipleAsync` + single-row `MergeNonNull` (one per repo).
- `BL`: `GetTemplateMeta` registry (one `case` per template) + `GetTemplateData` + `GenerateTemplate` + `EnrichTemplateData` hook. Injects `ITemplateUtil`.
- `WebApi`: `GET TemplateData/{templateId}/{entityId}` + `GET GenerateTemplate/{templateId}/{entityId}`.
- DI: `services.AddTemplateUtil()`.
- `CertificateOfOriginsDb.yaml`: registered `dbo.usp_Template_INNER_CROSS_CertificateOfOrigin` (hand-written generic).
- `CertificateOfOriginsConsts.CertificateOfOriginEUR1TemplateTypeId = 136`.

Template assets:
- `Templates/CertificateOfOriginEUR1.yml` — 22 mapped fields + the goods-item Table.
- `Templates/CertificateOfOriginEUR1.docx` — **corrected copy** (legacy untouched).

## Verified API facts (real, against InfrastructureCore.Utils 1.10.99 — the skill's examples were older)
- `ITemplateUtil.CreateRequestBuilder()` — note the **e** (`Create`, not the skill's `Creat`).
- Builder: `.WithName(string)`, `.WithJsonData(string)` (for a pre-serialized JSON string — **not** `WithData(object)`, which would double-encode), `.WithFormat(Format)`, `.Build()`.
- `Format` enum in `CustomsCloud.InfrastructureCore.Utils.Templates`: None=0, Docx=20, Pdf=40.
- `AddTemplateUtil()` in `CustomsCloud.InfrastructureCore`.

## The table blocker — diagnosed and fixed
- `REGEON_tbl1` (goods-item table) was an SDT wrapping a `<w:tr>` (repeating **row**) **inside** the `<w:tbl>`.
- The **new** Templates module's `TemplateBL.MergeTableField` requires the SDT's first child to be a whole `<w:tbl>`
  (the legacy module supported repeating rows via `OpenXmlRepeatingBlock`; the new one dropped it).
- **Fix applied:** OpenXML node-move so the `REGEON_tbl1` SDT wraps the whole `<w:tbl>`. Validated: identical element
  counts + length (pure move), well-formed XML, valid docx (`[Content_Types].xml` + all parts + media preserved).
- **Correction found while fixing:** `GoodsDescription`, `GrossWeightAndMeasureType`, `InvoiceNumber` are **goods-table
  columns** (inside `<w:tc>` in the row), not certificate-level scalars — the YAML/DTO were corrected accordingly.
- **Residual (needs live render):** the table has a single (data) row; the new module's convention is row0=header
  (skipped) / row1=repeat-template. A single-row table may need a header row added — verify with a live render.

## What remains if resumed (all developer-owned / need the environment)
1. **The SP** `dbo.usp_Template_INNER_CROSS_CertificateOfOrigin` (db-proc) — the data mapping: exporter/consignee from
   `CertificateOfOriginDetails` by detail-type code, goods items from the invoice/item tables, etc. Does not exist yet.
2. **Enrichment** (`EnrichCertificateOfOriginEUR1`, currently a TODO stub) — QR (from `QrCodePath`), site stamp, user
   signature, customs-house name (`ILookupUtil<OrganizationUnit>`), and the goods-item list (multi-row read).
3. **Live render** — upload `CertificateOfOriginEUR1.docx` + `.yml` to the MinIO `templates` bucket and confirm render
   (esp. the goods table header-row question).
4. **Wire the publish flow** — `PublishAttachments` still calls the placeholder `commonServicesProxy.GenerateTemplate`;
   switch it to the new `GenerateTemplate(templateId, entityId)` once the SP + enrichment are real.

## Reference: type → template/report ids (from the investigation)
| ID | Type | 1-page docx | 2-page docx | ReportId |
|---|---|---|---|---|
| 1 | EURMED | 135 | 1131 | 7000 |
| 2 | EUR1 | 136 | 1132 | 7000 |
| 3 | MERCOSUR | 134 | 1126 | 7002 |
| 4 | IsrCol | 133 | 1127 | 7003 |
| 5 | NonManipulation | 137 | — | 7006 |
| 6 | Panama | 2376 | 2377 | 7001 |
| 7 | SouthKorea | 2391 | 2392 | 7004 |
| 8 | UnitedArabEmirates | 2393 | 2394 | 7004 |

Panama/SouthKorea/UAE have **no source `.docx`** — served only via SSRS ReportId — which is the crux of the open question.
