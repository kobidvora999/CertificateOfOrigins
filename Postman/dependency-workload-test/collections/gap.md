# Dependency-workload — outbound-call map & gaps (CertificateOfOrigins)

Derived from `API/**/BL/Proxies/*Proxy.cs` (real proxies, excluding `*MockProxy.cs`) + their BL callers + the
controller endpoints. **9 external services**, one v3 collection each. Runs **pre-prod with REAL proxies** (infra
flips proxies + stands up the environment; these collections only point the Postman CLI at the live service).

| External service (`CustomsMicroServices.X`) | Outbound calls (proxy methods) | Probe endpoint (fires the call) | Liveness assertion |
|---|---|---|---|
| SystemTables | Country/CountriesByAlphaCodes · Site/SitesByExternalNumbers · InternationalSite/InternationalSitesByLocodes · PackingType/PackingTypesByCodes · MeasurementUnit/MeasurementUnitsByCodes · CurrencyType/CurrencyTypesBy{Ids,Codes} · CustomsBook/CustomsItemsByIds · CountryGroup · OrganizationUnit | `POST /CertificateOfOrigins/CertificateOfOriginRequest` (message validation fans out to all of them) | HTTP 200 |
| Customers | Customer/CustomersByIds · GetCustomerIdByExternalId | `GET /CertificateOfOrigins/CertificateOfOriginById/{id}` (enriches customer name) | HTTP 200 |
| Users | User/UsersByIds | `GET /CertificateOfOrigins/CertificateOfOriginById/{id}` (enriches handling-user name) | HTTP 200 |
| Vendors | Vendor/VendorsByIds | `GET /AuthenticationRequest/AuthenticationRequestByID/{documentId}` (enriches vendor) | HTTP 200 |
| ExportDealFile | ExportDeclarationDetailsForCertificateOfOrigin · ExportDeclarationInfoForPc · GetLeadDocumentByCertificateOfOriginId · GetLeadDocumentSubmissionDate | `GET /CertificateOfOrigins/LoadDataFromExportDeclaration?leadDocumentId=…&requestReasonCode=1` | HTTP 200 |
| Documents | Document/DocumentsByEntity · AttachDocumentsToEntity · DeleteDocuments · GetDocumentById | `GET /AuthenticationRequest/EntityDocuments/{leadDocumentId}` (DocumentsByEntity) | HTTP 200 |
| Collaterals | GetCollateralRequest · ChangeTempCollateralRequest · GetCollateralRequestIdsByRelatedEntity · GrantAllCollateralRequests | `GET /AuthenticationRequest/AuthenticationRequestByID/{documentId}` (resolves collateral) | HTTP 200 |
| Common | Message/SendMessage · CommonServices/CreateQrCode · GenerateTemplate (SSRS) | `POST /CertificateOfOrigins/CertificateOfOriginRequest` (fires SendMessage feedback) | HTTP 200 |
| Tasks | Task/IsTaskExist · Task/LatestUserHandlingEntityTasksWithTaskUnification | `GET /AuthenticationRequest/AuthenticationRequestByID/{documentId}` (IsTaskExist) | HTTP 200 |

## Placeholder collection-variables — MUST be set to real pre-prod ids before the run
The probe URLs reference variables that need valid pre-prod values (do **not** invent business data — supply from
the pre-prod dataset):
- `{{certificateId}}` — an existing certificate id (Customers, Users probes).
- `{{authenticationRequestDocumentId}}` — an existing import-authentication-request DocumentId (Vendors, Collaterals, Tasks probes).
- `{{leadDocumentId}}` — an existing lead-document id with linked documents + an export declaration (Documents, ExportDealFile probes).

Set them in each `.resources/definition.yaml` `variables:` block, or pass `--env-var` at run time.

## Notes / partial coverage
- **Common — CreateQrCode + GenerateTemplate are NOT fired by the probe.** They run only on the **publish path**
  (`SaveCertificateOfOrigin` with a published status). The current probe proves Common via `Message/SendMessage`
  (request feedback). To also exercise QR/SSRS, add a publish-path probe once a publishable pre-prod certificate exists.
- **Documents — AttachDocumentsToEntity / DeleteDocuments** are write-side (Save/attach flows); the probe proves
  Documents liveness via the read `DocumentsByEntity`. Liveness of the write endpoints is proven by a successful
  Save flow (out of scope for a read-only probe).
- **Assertions are HTTP-200 liveness only.** Per the skill, where the outbound effect isn't cleanly visible in the
  response (in-band feedback, enrichment that may be null for a given id), liveness is proven by the call succeeding.
  Tighten to enriched-field assertions once real pre-prod ids yield deterministic responses.
