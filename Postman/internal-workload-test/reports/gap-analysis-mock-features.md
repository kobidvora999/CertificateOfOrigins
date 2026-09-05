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

---

# Round 3 — the reconciliation outcome arms

Date: 2026-09-04 · collection `CertificateOfOrigins Internal Workload - Reconcile Outcomes`

## Why two of the three arms were dead

`ApplyReconciliationOutcome` (`CertificateOfOriginsBl.cs:1520`) has three arms, each leaving a different status:

| condition | event | status |
|---|---|---|
| no errors, no warnings | `RaiseCertificatePreferredAssessorEvent` → `ResolveAssessorUserId` | DeclarationMatch (6) |
| any error | mismatch event | Rejected (3) |
| warnings only | `RaiseDeclarationHasWarningsEvent` | DeclarationMismatch (5) |

Every earlier collection reached the reconciliation only through the **message** path, where the declaration
comes from `ExportDealFileMockProxy` and always disagrees with the certificate. So every run produced
Error-level findings and only the middle arm ever ran.

The fix was not another mock flag but a different **door**: `POST /CertificateOfOrigins/Reconcile`, where the
declaration payload is ours. Two sides that can be made to agree — or to disagree in exactly one controlled way.

## Result

| method | before | after |
|---|---|---|
| `RaiseDeclarationHasWarningsEvent` | **0/21** | **21/21** |
| `RaiseCertificatePreferredAssessorEvent` | **0/17** | **17/17** |
| `ResolveAssessorUserId` | **0/14** | 12/14 |
| `ApplyReconciliationOutcome` | 10/19 | **19/19** |
| `ValidateCertificateInvoices` | 15/24 | 22/24 |

| | round 2 | round 3 |
|---|---|---|
| Line (merged) | 84.9% | **87.4%** (4111/4704) |
| Branch (pass 1) | 69.9% | **72.9%** (1096/1504) |

8 collections, 144 requests, 443 assertions, 0 failures (+3 requests / 10 assertions in the issue-by-worker pass).

The scenarios: `Match` (no certificate invoices → the invoice block is skipped → zero findings),
`MatchNoAssessor` (same, plus `Tasks.NoAssessor`), `Warnings` (one certificate invoice whose number the
declaration does not carry → `ExportInvoiceNotMatch`, a WARNING, and the forward loop returns immediately so no
Error can follow), `WarningsImportReplace` (reason 5 + `DealFile.NoAssociatedGoodsItems`) and
`CustomsItemMismatch`. Each folder then **reads the certificate back**: the status is the branch proof, because
the three arms are the only writers of 6 / 5 / 3.

## The round-1 blocker, resolved from the other side

Round 1 recorded that `CustomsBook.CustomsItemMismatch` could not reach its branch, because the mock's
declaration goods item carries `CertificateOfOriginId` 0 and the certificate-link Error fires first — and that
fixing it needed a mock code change. Through `/Reconcile` no mock change is needed: the payload is ours, so
`certificateOfOriginId` is the real id, the link check passes, and both directions of the 6-digit comparison
are reached (`CustomsItemMismatch` forward, `CustomsItemInDeclarationNotInCertificate` in reverse). The mock
gap is still real for the message path; it is simply no longer the only way in.

## Two traps found and fixed

### 1. A silently-ignored request field — `certificateOfOriginInvoiceDetail`
The save DTO property is `CertificateOfOriginInvoiceDetail**s**` (plural). Every fixture in this repo sent the
singular key. The binder accepted the body and dropped the field, so **no certificate ever got an invoice row** —
which is why `GetCertificateOfOriginInvoiceDetails` sat at 8/23 and `GetCurrencyCodes` at 6/11 after the mock
round, and why the first run of this collection reported "no findings" for scenarios built around invoices.
Fixed in 14 fixtures across three collections; those two methods are now 23/23 and 11/11.

The lesson generalises: a JSON key that does not match a DTO property is not an error, it is a silent no-op.
An assertion that only checks status 200 cannot see it — only a value read back can.

### 2. `IParametersUtil` caches in Redis with NO TTL — restoring the DB row is not enough
`Parameters.CertificateOfOrigins` is a Redis hash (field names lowercased, no expiry). The issue-by-worker pass
sets the parameter, the service caches it, and restoring the row afterwards leaves Redis holding the old value.
The next ordinary run then reads **True** from cache while the DB says False.

That is not hypothetical — it happened here. A pass-1 run measured `SendCertificateToIssueQueue` at 25/25 and
`PublishAttachments` at 14/15 with the parameter False in the database, because the inline-template line 1333
never ran. The suite was green and the number was wrong. `run-issue-by-worker.ps1` now deletes the cache key
after setting the parameter AND after restoring it (raw RESP over TCP, no redis-cli needed), and refuses to
start at all if the parameter is already True — otherwise it would capture True as the "original" and
faithfully restore the poisoned state.

After the fix, pass 1 reports `SendCertificateToIssueQueue` 0/25 and `PublishAttachments` 9/15, and only the
merge shows 25/25 and 15/15. That is what correct looks like.

## Still open

| method | lines | what it needs |
|---|---|---|
| `ValidateCertificateGoodsItem` | 31/49 | the origin-country-group arm (an OriginGroupOfCountries detail plus `CountryGroup.NotInGroup`) |
| `ValidateCertificateDetails` | 32/46 | the individual mismatch findings in isolation — currently only some fire together |
| `RaiseNewRequestEvent` (Auth) | 0/14 | `SaveImportAuthenticationRequest` with `decisionId` = NewAuthenticationRequest on an EXISTING row |
| `CheckDeclarationAssociatedWithCertificate` | 0/8 | unreachable for reason 14 — migration duplication, see round 1 |

---

# Round 4 — the SaveImportAuthenticationRequest decision switch

Date: 2026-09-04 · collection `CertificateOfOrigins Internal Workload - Save Import Decisions`

## Why the whole method was half-dead

The stock fixture posted `documentId: 900001`, which does not exist. The set-based update affects 0 rows, the
BL throws `RestNotFoundException`, and **everything after the switch** — the `VendorId 0 -> null`
normalisation, the `AuthenticationNeedless` rejection event, the save itself and the re-read — was unreachable.
Of the four switch paths only `default` ever ran, and `RaiseNewRequestEvent` sat at 0/14.

The fix is a seed, not a mock: two rows dedicated to this collection (990103 / 990104), deliberately separate
from the Auth Lifecycle pair. This save is a set-based UPDATE that writes `AuthenticationFileID`, `DecisionID`
and `VendorID` — exactly the columns Auth Lifecycle asserts — and the collections run in **parallel**, so
sharing rows would have been a race.

## Result

| method | before | after |
|---|---|---|
| `RaiseNewRequestEvent` | **0/14** | **14/14** (br 2/2) |
| `SaveImportAuthenticationRequest` | 31/74, br 4/18 | **74/74**, br 16/18 |
| `GetAuthenticationRequestByID` | partial | **42/42** |
| `ChangeTempCollateralRequest` | 0/6 | **6/6** |
| `AuthenticationRequestBl.cs` | 80.7% | **88.2%** |

| | round 3 | round 4 |
|---|---|---|
| Line (merged) | 87.4% | **88.9%** (4181/4704) |
| Branch (pass 1) | 72.9% | **74.1%** (1114/1504) |

9 collections, 152 requests, 476 assertions, 0 failures. Green on the first run.

## The eight scenarios

Both sides of every guard, not just the interesting side:

| scenario | decision | what it proves |
|---|---|---|
| `10-new-request-with-file` | 1 + `Tasks.Empty` | `RaiseNewRequestEvent` **and** its `AddRelatedEntity` branch |
| `20-new-request-no-file` | 1 + `Tasks.Empty`, file null | the other side of that `if` |
| `30-new-request-user-handles` | 1, `IsCurrentUserHandleRequest` true | the guard's short-circuit |
| `40-new-request-task-exists` | 1, default tasks mock | the `Count == 0` false side |
| `50-authreq-rejected-task` | 6, default mock | processed-after-rejection event + a second entry into `RaiseNewRequestEvent` |
| `60-authreq-no-rejected` | 6 + `Tasks.Empty` | the empty side of that guard |
| `70-needless-collaterals` | 7, vendorId 0, one collateral | the `default` arm, the rejection event, the `VendorId 0 -> null` normalisation and `CollateralId` from the first collateral |
| `80-missing-row` | 1, documentId 900001 | the 404 the stock fixture hit on every run — kept deliberately, as a scenario rather than as the only behaviour |

Scenario 70 is the one with real assertions rather than coverage-only proof: `vendorId` comes back null and
`collateralId` comes back 9901, both visible in the re-read.

## Still open

| method | lines | what it needs |
|---|---|---|
| `CertificateOfOriginsBl.MessagePerReason.cs` | 50.3% | the per-reason arms not yet driven (Replacement, Retrospective, Draft) |
| `SendDecisionMessage` | 25/36, br 4/10 | the responder-differs-from-creator arms |
| `ValidateCertificateGoodsItem` | 31/49 | the origin-country-group arm |
| `CheckDeclarationAssociatedWithCertificate` | 0/8 | unreachable for reason 14 — migration duplication, see round 1 |

---

# Round 5 — the per-reason switch

Date: 2026-09-04 · collection `CertificateOfOrigins Internal Workload - Per Reason`

## Result

| | before | after |
|---|---|---|
| `MessagePerReason.cs` line | 50.3% | **88.4%** (130/147) |
| `MessagePerReason.cs` branch | 49.4% | **89.4%** (76/85) |
| `ResolveCertificateForReason` | 33/61 | **55/61**, br 34/37 |
| the static helpers (`<sync>`) | 14/39 | **39/39**, br 21/22 |
| `CheckCertificateNumber` | 9/12 | **12/12** |
| `CheckIfCertificatePublishedOrCanceled` | 5/8 | **8/8**, br 8/8 |

| | round 4 | round 5 |
|---|---|---|
| Line (merged) | 88.9% | **90.0%** (4235/4704) |
| Branch (pass 1) | 74.1% | **76.4%** (1149/1504) |

10 collections, 175 requests, 541 assertions, 0 failures.

Every earlier fixture sent the same two shapes — reason 1 without a certificateId, reason 3/4 with one — so whole
arms had never been entered and the helpers were only ever driven down their success side. The 18 requests here
walk every arm and, for the guards, **both** sides: the clean cases assert the ABSENCE of their siblings' codes,
which is what stops a validator that fires on everything from looking equally green.

## Three things the live service taught us

### 1. The message lock is never released — a certificate id is single-use for five minutes
`GetPC22802281CertificateOfOriginRequest` takes an `ILockUtil` lease keyed on `AgentRequest.CertificateId`
(`CertificateOfOriginsBl.cs:88`) whenever that id is non-empty, and releases it with `SafeReleaseAsync` in a
`finally`. **It does not release.** Reproduced directly against the live service with a fresh id:

```
call 1 -> HTTP=200  ok
call 2 -> HTTP=400  locked by another request
call 3 -> HTTP=400  locked by another request
```

The lease is `TimeSpan.FromMinutes(5)`, and `LockUntilAsync` appears to mean exactly that — held until the
deadline, regardless of the release call. The BL's intent is plainly otherwise ("released in a finally" is in
its own comment). **An agent that retransmits the same certificate within five minutes gets a spurious 400.**

For the collection this meant two changes: every scenario that transmits a certificateId needs its own row
(six published rows, not one), and every id is minted per RUN via a `runId` suffix — otherwise the suite cannot
be run twice inside five minutes. Verified: two consecutive full runs, both 10/10 green.

### 2. The switch's `default` arm is dead code behind validation
`requestReasonCode: 99` never reaches the switch — a validator rejects it first with 400
`RequestReasonCode must be a defined ERequestReason value`. The scenario is kept and now asserts that 400: it is
what proves the arm is unreachable rather than merely untested. Nothing anywhere raises `RequestReasonNotExist`
(5020), which the migrated code still defines.

### 3. Reaching `IllegalCertificateTypeUpdate` requires the STORED certificate to be NonManipulation
Sending `certificateOfOriginTypeCode: 5` makes the message itself a NonManipulation message, whose field
catalogue demands manifest/import/export values an EURMED body does not carry — the run came back with only
`MandatoryValue(2387)` and never reached `CheckCertificateUpdate`. Inverting it (stored certificate type 5,
transmitted as type 1) reaches the arm.

## Still open in this file

| method | lines | why |
|---|---|---|
| `CheckExportDeclarationNumber` | 9/12 | the Draft/Canceled declaration arm needs `LeadDocumentStateId` in {1,5,6}; `ExportDealFileMockProxy` hardcodes 0 and there is no flag for it. **A new mock feature is the only way in** — a one-line code change, so it is reported rather than made |
| `CheckDeclarationAssociatedWithCertificate` | 0/8 | unreachable for reason 14 — the cancel branch has its own inline copy. Migration duplication, see round 1 |

## Environment note

Consul `Main/CentralConfig` was pointed at **PreRulings** partway through this round by work outside this
session; a run collided with it and every collection failed with `Could not find stored procedure`. It was
repointed for the measurement and restored to **PreRulings** — the value found, not the `Customers` of earlier
rounds. Anyone re-running must set it to `CertificateOfOrigins` first.

---

# Round 6 — the invoice / item conversion

Date: 2026-09-04 · collection `CertificateOfOrigins Internal Workload - Invoice Items`

## Result

| | before | after |
|---|---|---|
| `MessageInvoice.cs` line | 77.6% | **98.8%** (159/161) |
| `MessageInvoice.cs` branch | 67.6% | **90.5%** (67/74) |
| `ValidateInvoiceShape` + the static helpers | 16/31 | **31/31** |
| `CheckAndGetCustomsItem` | 11/20, br 4/8 | **20/20**, br 8/8 |
| `CheckAndGetOriginCriterion` | 9/15, br 4/6 | **15/15**, br 6/6 |
| `ResolvePackingTypeId` / `ResolveMeasurementUnitId` | 11/13 each | **13/13** each |

| | round 5 | round 6 |
|---|---|---|
| Line (merged) | 90.0% | **90.8%** (4269/4704) |
| Branch (pass 1) | 76.4% | **77.5%** (1166/1504) |
| BL line / branch | 85.0% / 73.0% | **88.0% / 76.6%** |

11 collections, 186 requests, 577 assertions, 0 failures.

One cause for the whole gap: every fixture in the repo sent exactly ONE well-formed invoice carrying exactly ONE
well-formed item, so each guard was only ever taken down its success side. Eleven requests, each malforming
exactly one thing, closed it.

## Two things worth knowing

### `IsCustomsItemMandatory` and `IsCriterionMandatory` are NOT request fields
They look like request fields — they sit on `CertificateOfOriginAgentRequestDto` — but the BL **overwrites them
server-side** from the certificate type's C-table row (`MessageValidation.cs:105-107`). Sending them in the body
is a silent no-op, which is exactly how the first attempt failed. The only way to turn them on is to pick a type
whose row says True:

| type | criterion | customs item |
|---|---|---|
| 1 EURMED, 2 EUR1, 11 EUR1-ACC | False | False |
| **3 MERCOSUR** | **True** | **True** |
| 4 Columbia, 9 Vietnam | True | False |
| 8 UAE, 10 Guatemala | False | True |

Type 3 is the only one with both, so the three flag-dependent scenarios are MERCOSUR messages.

### The container check needs one specific packing code
`CheckContainerIsoCode` fires only when the RESOLVED `PackingTypeId` equals 379. `PackingTypeMockProxy` derives
the id as `(sum of chars % 1000) + 1`, so the code has to sum to 378: `"BOX-40"` does (66+79+88+45+52+48). Any
other code silently misses the branch.

## Left in this file

`ConvertInvoiceDetails` 15/17 — the `NonManipulation || certificate is null` early return. The NonManipulation
fixture answers 200 and the line still never runs, so that flow evidently does not pass through
`ConvertInvoiceDetails` at all; the guard looks like defensive code unreachable from the API. Two lines, recorded
rather than chased.

---

# Round 7 — the five clusters left in CertificateOfOriginsBl.cs

Date: 2026-09-04 · collection `CertificateOfOrigins Internal Workload - Bl Core`

Each cluster was blocked by a different thing, and none of them by a mock flag.

| method | before | after | what it needed |
|---|---|---|---|
| `BuildHeaderFields` | 22/38 | **38/38** | a published certificate that is retrospective AND has a `certificateIdToCancel` AND an `IsExportDecForPrint` detail — all three optional header rows at once |
| `Convert` | 4/13 | **13/13** | a certificate number that exists (the fixture pointed at `IL0000116895`, which matches nothing, so it 404'd every run) |
| `FillCustomersInformation` | 6/27, br 1/16 | **25/27** | the same: a filter that returns rows, so the enrichment has something to enrich |
| `GetSavedCertificateForMessage` | 6/12 | **12/12** | reason 13 with no id, and with an unknown id |
| `ValidateCertificateDetails` | 32/46 | **46/46** | each mismatch in isolation — earlier rounds matched on every field at once |
| `ValidateCertificateGoodsItem` | 31/49 | **46/49** | an `OriginGroupOfCountries` detail, which no fixture had ever sent |
| `ValidateImportReplacement` | 17/25 | **20/25** | associated goods items that EXIST but are not in the agreement — round 3 used the empty-list flag, which skips the loop |
| the static web-query mappers | 243/277 | **272/277** | a certificate carrying every detail row the field switch has a case for, published once as EURMED and once as MERCOSUR (`MapConsigneeField` branches on the type) |
| `HandleCertificateReplacement` | 0/7 | 5/7 | a real transition into Published on a reason-4 certificate |

| | round 6 | round 7 |
|---|---|---|
| `CertificateOfOriginsBl.cs` | 84.0% / 73.4% | **93.5% / 84.1%** |
| Line (merged) | 90.8% | **93.4%** (4392/4704) |
| Branch (pass 1) | 77.5% | **81.6%** (1228/1504) |
| BL line / branch | 88.0% / 76.6% | **91.3% / 81.0%** |

12 collections, 212 requests, 657 assertions, 0 failures.

## `CertificateIdToCancel` is a self-referencing foreign key
The first run failed with HTTP 500 on every setup that carried a literal `certificateIdToCancel: 4242`:
`FK_CertificateOfOrigins_CertificateOfOrigin_CertificateOfOrigins_CertificateOfOrigin`. It points at
`CertificateOfOrigin.ID`, so it cannot be an invented number — the fixtures now create a real target row first
and reference its captured id.

## Environment: the Consul key is being reset by other work
Twice during this round `Main/CentralConfig` → `ConnectionStrings.CustomsDb` flipped back to **PreRulings**
mid-run, and every collection failed with `Could not find stored procedure`. The third attempt held and is the
measurement reported here. Anyone re-running needs the key on `CertificateOfOrigins` for the duration; a guard
in `run.ps1` that reads the key at start and re-checks it at the end would turn this from a mystifying wall of
500s into one clear message.

---

# Round 8 — the authentication-file status machine

Date: 2026-09-04 · collection `CertificateOfOrigins Internal Workload - Auth File Status`

`AuthenticationRequestBl.cs` was the last file under 70% branch, and one cluster was why: a single fixture with
a single transition (2 → 5), one child with decision 3, and `userId == userResponseId`. Every other arm of a
nine-value status enum and a nine-value decision enum had never been entered.

| method | before | after |
|---|---|---|
| `ManageFileStatus` | 20/33, br 6/10 | **33/33**, br 10/10 |
| `CheckStatusAndOpenTask` | 26/32, br 9/14 | **32/32**, br 14/14 |
| `SendDecisionMessage` | 25/36, br 4/10 | **36/36**, br 8/10 |
| `FillLeadDocumentRequestNames` | 5/9, br 1/10 | **9/9**, br 10/10 |
| `FillAuthenticationRequestNames` | 27/43, br 30/42 | **41/43**, br 40/42 |
| `HandleAuthenticationRequestDeliverySent` | 7/11 | **11/11** |
| **`AuthenticationRequestBl.cs`** | **88.2% / 69.7%** | **94.9% / 85.0%** |

| | round 7 | round 8 |
|---|---|---|
| Line (merged) | 93.4% | **94.9%** (4462/4704) |
| Branch (pass 1) | 81.6% | **84.1%** (1265/1504) |
| BL line / branch | 91.3% / 81.0% | **92.9% / 83.6%** |

13 collections, 222 requests, 689 assertions, 0 failures. Green on the first run.

## The seed did as much work as the collection

Two of these were pure data problems, invisible from the request side:

* `FillAuthenticationRequestNames`' importer block never ran because **no seeded request had an `ImporterID`** —
  `importerIds` was always empty. 990105/990106 now carry one.
* `FillLeadDocumentRequestNames` returned at its empty guard on every run because the stock fixture queries
  lead-document `[1]`, which matches nothing. It now queries ids that exist.

The new file 990002 is **dedicated** to this collection, for the same reason 990103/990104 were in round 4: one
scenario is `CancelledFile`, which calls `UnlinkAllRequestsFromFile`, and it would detach the Auth Lifecycle
collection's rows from 990001 while that collection is reading them in parallel.

## Still open

| file | line | branch |
|---|---|---|
| `MessageCrossField.cs` | 85.4% | 82.7% |
| `MessageValidation.cs` | 87.0% | 74.4% |
| `MessagePerReason.cs` | 88.4% | 89.4% |
| `AuthenticationRequestBl.cs` — `GetEntityDocuments` | 25/32 | needs an entity whose documents are already requested / claimed by another lead document |

---

# Round 9 — the per-field validators

Date: 2026-09-05 · collection `CertificateOfOrigins Internal Workload - Field Validation`

One habit explained almost the whole gap: **every fixture in the repo sends `IL` for every country field and
`1` for the city.** The validators that exist precisely to reject a non-Israel country, or a city that does not
resolve, were therefore only ever driven down their happy side.

| method | before | after |
|---|---|---|
| `CheckIfCountryInSystemAndIsrael` | 8/19, br **2/12** | **18/19**, br 10/12 |
| `CheckAgreementFirstCountry` | 11/14 | **14/14** |
| `CheckIfExemptPlaceOfManufacture` | 5/12, br 1/6 | **12/12**, br 5/6 |
| `CheckExportCountry` | 8/16, br 5/10 | 11/16, br 8/10 |
| `CheckCityOfDeclaration` | 10/19 | 13/19 |
| **`MessageValidation.cs`** | **87.0% / 74.4%** | **92.4% / 81.1%** |

| | round 8 | round 9 |
|---|---|---|
| Line (merged) | 94.9% | **95.4%** (4488/4704) |
| Branch (pass 1) | 84.1% | **85.2%** (1282/1504) |
| BL line / branch | 92.9% / 83.6% | **93.7% / 84.8%** |

14 collections, 231 requests, 716 assertions, 0 failures. Green on the first run.

Nine requests, each flipping exactly ONE field. The four arms of `CheckIfCountryInSystemAndIsrael`'s DetailType
switch are four separate scenarios rather than one message with four bad fields, because a single message
carrying all of them would prove only that *something* fired.

## Two things worth recording

**The Country mock is arithmetic, not a country table.** `IL` maps to 376 — the `CountryIsrael` parameter value —
and every other code to `(sum of chars % 1000) + 1`. That is what makes one-field-at-a-time flipping work, and
it is also why `60-place-of-manufacture-exempt` sends the odd-looking destination code `zzzzzzG`: the exempt
list (`CountriesExemptedFromSendingThePlaceOfManufacture`) holds `804`, so the code has to sum to 803, and
122×6 + 71 does. It looks strange because it is arithmetic; the alternative was editing a parameter that other
runs share.

**Two fields exist only on a NonManipulation certificate.** `TransirCountry` and `ExportCountry` are never
validated by an EURMED/EUR1 message at all, so the second folder sends a NonManipulation one — and
`CheckExportCountry` is *inverted* there: the export country must NOT be Israel, so `IL` is the error case.

## Still open

| file | line | branch |
|---|---|---|
| `MessageCrossField.cs` | 85.4% | 82.7% |
| `MessagePerReason.cs` | 88.4% | 89.4% |
| `MessageValidation.cs` — `CheckIfCountryGroupIsInTradeAgreement` | 19/23, br 6/14 | the four arms of its own DetailType switch, one country-group field each |
| `MessageValidation.cs` — `<sync>` | 114/128, br 35/48 | the mandatory/blank-field guards in `BuildDetailsFromFields` |
