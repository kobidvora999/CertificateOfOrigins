# Gap analysis — the MOCK lever (Mock Features collection)

Round: 2026-09-03 · service `CertificateOfOrigins` · collection `CertificateOfOrigins Internal Workload - Mock Features`

## Why this round existed

The service ships **34 scenario flags** in its mock proxies (`IProxyMockUtil.HasMockFeature`). Before this
collection, **zero** of them were ever sent — the header `x-mock-feature--{Flag}` appeared only in a comment in
each `definition.yaml`. Every proxy therefore always returned its happy answer, and the negative side of every
proxy-driven `if` was unreachable. That is the whole explanation for a repo sitting at 80.9% line / **63.6%
branch**: the lines ran, but only ever down one side.

## Result

| | before | after | Δ |
|---|---|---|---|
| Line (our code, excl. generated) | 3805/4704 = **80.9%** | 3961/4704 = **84.2%** | **+3.3 pt** |
| Branch | 956/1504 = **63.6%** | 1052/1504 = **69.9%** | **+6.3 pt** |
| BL line | 76.6% | **80.6%** | +4.0 |
| BL branch | 61.8% | **68.6%** | +6.8 |

49 requests · 139 assertions · 0 failures. 31 of the 34 flags exercised.

## Per-method movement

| method | before | after |
|---|---|---|
| `CreateQrCodeIfNeeded` | 0/12 | **12/12** |
| `PrintCertificateOfOriginAndSaveAttachments` | 0/17 | **17/17** |
| `LinkLeadDocument` | partial | **14/14** |
| `SendRequestFeedback` | 0/11 | **11/11** |
| `GetFieldLabels` | 6/11, br 1/10 | **11/11, br 10/10** |
| `CheckIfCountryIsInTradeAgreement` | 8/21, br 2/14 | **21/21, br 13/14** |
| `RaiseStatusEvents` | 13/19 | **41/46** |
| `CheckIfCountryGroupIsInTradeAgreement` | 9/23, br 3/14 | 19/23, br 6/14 |
| `ValidateCertificateDetails` | 20/46 | 32/46 |
| `ValidateImportReplacement` | 7/25 | 17/25 |
| `PublishAttachments` | **0/15** | 9/15 |
| `BuildHeaderFields` | 16/38 | 22/38 |
| `ResolveCertificateForReason` | 31/61 | 33/61 |

## Levers applied

- **MOCK** — 31 flags, one per scenario, carried in the header name (`x-mock-feature--{Flag}: true`, double
  dash — `ProxyMockUtil.HasMockFeature(name)` reads `'x-mock-feature-' + '-name'`). **No code changed.**
- **INPUT** (where a flag alone could not reach the branch) — destination expressed as a country *group* instead
  of a country (`CheckCountryXorGroup` forbids both); `requestReasonCode` 2/3/5/14 to enter the per-reason arms;
  `currencyType` added to the invoice fixture, which no earlier fixture carried at all, so `ResolveCurrencyTypeId`
  had never been entered.
- **DB-STATE** — the `WebQuery` publish chains. A new certificate saved directly at status 8 is *not* a status
  change (`isNewInstance` counts as changed only at `Received`, `CertificateOfOriginsBl.cs:942`), which is why
  every earlier collection missed the publish path entirely. Each chain saves at Received, reads back the
  rowversion, then transitions the SAME row to Published.

## Corrections made after the first run (all verified live before asserting)

| scenario | first guess | verified reality |
|---|---|---|
| `ExportDoc/10,20` | 200 | **404** — `Customers.NotFound` flips the customer read to not-found. Control run confirms the 200. |
| `Message/90-site-not-found` | 200 | **400** — the site never resolves, `context.OrganizationUnitId` is never set, the validator rejects on `OrganizationUnitId`. The 400 *is* the early-return branch seen from outside. |
| `Message/120` | 14027173 | **no exception at all.** `ExportDealFile.NotFound` makes the declaration *null*; 14027173 needs a declaration that EXISTS in Draft/Canceled state. Renamed `120-declaration-unresolvable` and asserted against the control: reason 2 without the flag returns status 3 + 4 findings, with it status 2 + none. |
| `Reconcile/30` | CustomsItemMismatch | **CertificateNumberNotInDealFile** — see the blocker below. |

## Remaining gaps

### 1. `CustomsBook.CustomsItemMismatch` cannot reach its branch — mock DATA, not the flag
`ValidateCertificateInvoices` filters the declaration goods items by
`goodsItem.CertificateOfOriginId == certificate.Id`. `ExportDealFileMockProxy.GetExportDeclarationInfoForPc`
never sets `CertificateOfOriginId`, so the filtered list is always empty and `CertificateNumberNotInDealFile`
is raised before the 6-digit comparison. **Fix needs a mock code change** (echo the requested certificate id
back on the goods item) — a code change, so it is flagged here rather than made.

### 2. Two implementations of one legacy check; one is unreachable
`TheLinkedDeclarationMustBeCanceledBeforeCancelingTheCertificate` (14027174) is raised in two places:
- `CertificateOfOriginsBl.cs:153` — the CANCEL branch's inline copy. **Now covered.**
- `CertificateOfOriginsBl.MessagePerReason.cs:207` in `CheckDeclarationAssociatedWithCertificate` — reached only
  via `ResolveCertificateForReason`'s `CertificateCancellation` arm, which is on the CREATE branch. Reason 14
  short-circuits into the cancel branch at `CertificateOfOriginsBl.cs:143` and never enters it. Still **0/8**.

  This looks like migration duplication rather than a test gap. A developer should decide whether the
  MessagePerReason copy is dead code to delete or a path that is missing a caller.

### 3. Still at 0% — needs levers this round did not use
| method | lines | what it needs |
|---|---|---|
| `SendCertificateToIssueQueue` | 0/25 | parameter `IssueCertificateOfOriginByWorker = true` (an `IParametersUtil` value, i.e. a DB/seed change, not a mock flag) |
| `RaiseDeclarationHasWarningsEvent` | 0/21 | the reconciliation must end in `DeclarationMismatch` through the `/UpdateCertificateOfOrigins` endpoint |
| `RaiseCertificatePreferredAssessorEvent` | 0/17 | the reconciler match event |
| `ResolveAssessorUserId` | 0/14 | called only from the above — `Tasks.NoAssessor` alone cannot reach it |
| `RaiseNewRequestEvent` (Auth) | 0/14 | `SaveImportAuthenticationRequest` with `decisionId` = NewAuthenticationRequest **on an existing row** (the stock fixture uses documentId 900001, which does not exist → 404 before the tail) |

### 4. Deliberately NOT exercised — an open contract question, not a gap
`Collateral.ChangeFail`, `Collateral.GrantFail`, `MessageManagement.Fail` each
`throw new InvalidOperationException` inside the mock. They add **no branch** (the exception aborts the call),
and the BL does not catch them, so they surface as **500**. Whether a downstream transport failure should be a
500 or a 502 is a contract decision for the developer. Asserting the current 500 would freeze an unreviewed
answer, so these three are left out and recorded here instead.

## Environment findings (not code)

1. **`tools/local-lookup-stub.js` must be running.** Without it the WebApi fails its readiness gate on
   `localhost:9000/9006` and never starts under coverage. Worth adding to the runner's prerequisites.
2. **Consul `Main/CentralConfig` → `ConnectionStrings.CustomsDb` pointed at the `Customers` database**, not
   `CertificateOfOrigins`. Every save produced `Invalid object name` and ALL SEVEN collections failed —
   including the six that passed on 2026-08-25. An `ConnectionStrings__CustomsDb` environment variable does
   **not** override it; central config wins. Repointed for this run and restored afterwards.
3. **`seed_ImportAuthenticationRequests.sql` carried a stray shell line** (`tail -14 "$S"`) at line 55,
   committed in 5a97d00. It is invalid T-SQL and would abort the script before its reset block — the block that
   keeps the Auth Lifecycle collection repeatable across runs. Removed.

---

# Round 2 — the parameter lever (`IssueCertificateOfOriginByWorker`)

Date: 2026-09-04 · collection `CertificateOfOrigins Param IssueByWorker` · runner `run-issue-by-worker.ps1`

## The branch no header can reach

`PublishAttachments` forks on `parametersUtil.Get<bool>("IssueCertificateOfOriginByWorker")`
(`CertificateOfOriginsBl.cs:1313`). That is a row in `Infrastructure.Parameters` — **service-wide, not
per-request** — so there is no `x-mock-feature--` for it and no request body can flip it. It is `False`
everywhere, which is exactly why `SendCertificateToIssueQueue` stayed at 0/25 after the mock round.

It also cannot just be switched on for the whole suite: with the flag `True`, *every* publish takes the queue
path and the inline-template path (`PrintCertificateOfOriginAndSaveAttachments`, 100% since round 1) goes dark.
So this is a **second pass** — one small collection, its own coverage session, merged afterwards. The folder is
deliberately named `CertificateOfOrigins Param IssueByWorker` so it does **not** match the runner's
`CertificateOfOrigins Internal Workload -` prefix and is skipped by the ordinary run.

`run-issue-by-worker.ps1` backs the parameter up, sets it `True`, runs the pass, restores it in a `finally`
(and verifies the restore), then merges into the main cobertura.

## Result

| method | after round 1 | after round 2 |
|---|---|---|
| `SendCertificateToIssueQueue` | **0/25** | **25/25** |
| `PublishAttachments` | 9/15 | **15/15** (both sides of the fork) |
| `PrintCertificateOfOriginAndSaveAttachments` | 17/17 | 17/17 (kept — the merge is a union) |
| `SaveCertificateOfOriginAttachments` | 39/39 | 39/39 |
| `CreateQrCodeIfNeeded` | 12/12 | 12/12 |

Merged line coverage (our code, excl. generated): **3992/4704 = 84.9%** (from 84.2%).

3 requests, 10 assertions, 0 failures. The branch proof is `isInPublishingProcess` on the response: it is set in
exactly one place — `CertificateOfOriginsBl.cs:1316`, under `if (issueByWorker)` — and that is the same
condition gating the queue hand-off two lines later.

⚠️ **The merged BRANCH figure is not comparable** to the single-pass one. `dotnet-coverage merge` renormalises
branch points: the denominator drops from 1504 to 1447 and per-method branch counts change shape (e.g.
`CreateQrCodeIfNeeded` reads 4/4 merged vs 8/10 in pass 1). Track branch % from the **single-pass** report
(69.9%) and line % from the merged one; the per-method line numbers above are reliable in both.

## Defect found and fixed: a parameter name with a trailing TAB

`Scripts/API_20260716 - add params.sql:415` inserted the name as `'IssueCertificateOfOriginByWorker<TAB>'`,
while its own existence guard on line 409 checks the clean name. Consequences on a **from-zero** database:

* the guard never matches its own insert, so the script is not idempotent — every run adds another row;
* `parametersUtil.Get<bool>("IssueCertificateOfOriginByWorker")` never matches the tabbed row (SQL Server
  ignores trailing *spaces* in `=`, but a tab is not a space), so the parameter silently reads as its default
  `false` and the issue-by-worker feature can never be turned on in a fresh environment.

This machine did not show the fault: the row was created on 2026-07-14 by the older
`API_260714183003 - SeedParameter_IssueCertificateOfOriginByWorker.sql` (now removed from `Scripts/` but still
in `dbo.SchemaVersions`, `Level=0`), so when `add params.sql` ran on 08-23 its guard matched the clean row and
skipped the tabbed insert. Only a from-zero bootstrap would hit it — which is what `db-scripts-check` exists to
catch. Tab removed; it was the only one in the file.
