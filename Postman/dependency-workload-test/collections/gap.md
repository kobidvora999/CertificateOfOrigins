# Dependency-workload — outbound-call map & gaps (CertificateOfOrigins)

Derived from the **real** proxies `API/CertificateOfOrigins.BL/Proxies/*/*Proxy.cs` (excluding `*MockProxy.cs` and the
interfaces): **42 outbound calls to 9 external services**, one v3 collection per service, **one request per outbound
call**, sent **straight to the route the proxy calls** (`{{base_URL}}/api/{Controller}/{Endpoint}`) — not through
CertificateOfOrigins. Runs **pre-prod** against the real services (infrastructure stands them up).

Rebuilt 2026-09-29. The previous 9 probes called CertificateOfOrigins itself on routes without the `ui/` / `community/`
prefix (404), read `{{certificateId}}` / `{{authenticationRequestDocumentId}}` / `{{leadDocumentId}}` that were defined
nowhere, and kept their assertions in request-level `scripts:`. None of them proved a dependency.

## Design: no pre-prod data, no side effects
Every entity id a probe sends is **2147483647** (int.MaxValue — no identity reaches it), external codes are
**standard codes** (`IL`, `USD`, `KG`, `BX`, `ILHFA`) or `NO-SUCH-…`. So:
- a probe never depends on data that happens to exist in pre-prod, and
- a **write** probe (collateral grant/debit/change, document attach/delete, lead-document re-point, message send) can
  only be validated and refused — or accepted as a no-op — it can never change a real record.

Assertions live at **collection level**, keyed on `pm.info.requestName`: no 5xx, < 15 s, valid JSON when the body is
JSON, and the status from the endpoint contract:

| Kind | Expected | Why |
|---|---|---|
| list / by-ids / by-codes read | 200 | an empty list, never 404 |
| by-id / lookup read | 200, 204 or 404 | both prove the service answered for an id that cannot exist |
| write with an id that cannot exist | 200, 204, 400 or 404 | validated and refused, or a no-op |

## The map
| Service | Calls | Probes |
|---|---|---|
| Collaterals | CollateralRequestByEntity · CollateralRequestIdsByEntity · ChangeTempCollateralRequest · GrantAllCollateralRequests · DebitCreditCollateralRequest | 5 |
| Common | CommonServices/CreateQrCode · CommonServices/GenerateTemplate · Message/SendMessage | 3 |
| Customers | CustomersByIds · CustomerInformation · CustomersByCountry (activity type 40) · IdByExternalId | 4 |
| Documents | DocumentsByEntity · Document/{id} · AttachDocumentsToEntity · DeleteDocuments | 4 |
| ExportDealFile | ExportDeclarationDetailsForCertificateOfOrigin · LeadDocumentSubmissionDate · LeadDocumentByCertificateOfOrigin (update) · LeadDocumentByCertificateOfOriginId · ChangeCertificateOfOriginForLeadDocument · ExportDeclarationInfo · DetailsForExportAssociatedGoodsItems | 7 |
| SystemTables | CountryGroupExists · IsCountryInCountryGroup · CountriesByAlphaCodes · CurrencyTypesByCodes · CurrencyTypesByIds · CustomsItemIdByFullClassification · CustomsItemsByIds · IsTradeAgreementForCountry · DataDictionaryFieldsByIds · InternationalSitesByLocodes · MeasurementUnitsByCodes · OrganizationUnit/IsCustomsHouse · PackingTypesByCodes · SitesByExternalNumbers | 14 |
| Tasks | IsTaskExist · IsTaskExistsOnEntity · LatestUserHandlingEntityTasksWithTaskUnification | 3 |
| Users | UsersByIds | 1 |
| Vendors | VendorsByIds | 1 |

Verified locally against a stub that answers `200 []`: all 9 collections load, all 42 requests go out on the proxy
route with the proxy's body and no unresolved variable, 168 assertions run. That proves the wiring, **not** the
services — only a pre-prod run does.

## Open
- **Every route is still `TODO(blocking): confirm endpoint`** in the proxies (INTERNAL_INTEGRATION.md). A 404 on a
  route that does not exist looks the same as a 404 for a missing id; the first pre-prod run must check that the
  by-id 404s come from the service (a problem-details body) and not from the gateway.
- **base_URL is one host.** If pre-prod does not front all services behind a gateway, run per service with its own
  host (`run.ps1 -Prefix "CertificateOfOrigins Dependency Workload - {Service}" -BaseUrl <host>`).
- `LatestUserHandlingEntityTasksWithTaskUnification` sends the values the BL sends: the export lead-document entity type and
  `organizationUnitTypeId: 18` (Export, `CertificateOfOriginsConsts.ExportOrganizationUnitType`).
