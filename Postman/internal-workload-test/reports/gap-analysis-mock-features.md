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
