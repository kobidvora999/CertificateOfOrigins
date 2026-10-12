# Parity re-check A — certificate core (Internal contract), 2026-10-11

Scope: GetCertificateOfOriginsByFilter, IsCertificateOfOriginByExternalIdExist, GetCertificateOfOriginById,
SaveCertificateOfOrigin (full chain: supersede, validation/enrichment, QR, details save, lead-document link,
status events via RaiseEventUtil, feedback, publish/attachments/issue queue, replacement), LoadDataFromExportDeclaration.

Files read:
- **N-BL** `C:\Repos\CertificateOfOrigins\API\CertificateOfOrigins.BL\CertificateOfOriginsBl.cs` (1-90, 695-1766)
- **N-DAL** `C:\Repos\CertificateOfOrigins\API\CertificateOfOrigins.DAL\CertificateOfOriginsDal.cs` (100-434, 933-1175)
- **N-VAL** `...\CertificateOfOrigins.BL\Validations\SaveCertificateOfOriginRequestValidator.cs`
- **N-DTO** `...\CertificateOfOrigins.Model\ModelDTOs\SaveCertificateOfOriginRequestDto.cs`, `IssueCertificateDto.cs`, `ECertificateDetailsType.cs`, `ERequestReason.cs`, `ECertificateOfOriginStatus.cs`, entity `CertificateOfOriginsDb\CertificateOfOrigin.cs`
- **N-EXT** `...\CertificateOfOrigins.Model\DbContext\CertificateOfOriginsDbContextExtension.cs` (115-175)
- **N-CTL** `...\CertificateOfOrigins.WebApi\Controllers\Ui\CertificateOfOriginsController.cs`
- **N-SP** `...\WebApi\Scripts\API_20261008110054 - dbo.GetCertificateOfOriginsByFilter date bounds.sql`
- **N-WRK** `C:\Repos\CertificateOfOrigins\Workers\IssueCertificateOfOrigin.Worker\IssueCertificateOfOriginBL.cs` (only to follow the attachment-delete path)
- **L-SVC** `C:\Repos\Main\CRM\CertificateOfOrigins\Server\Customs.CRM.CertificateOfOrigins.Service\CertificateOfOriginsInternalServicePartial.cs` (full)
- **L-BL** `...\Customs.CRM.CertificateOfOrigins.BL\CertificateOfOriginsBL.cs` (60-450, 640-720, 925-1320, 1440-2000)
- **L-EVT** `...\Customs.CRM.CertificateOfOrigins.BL\RaiseEventUtil.cs` (full)
- **L-SP** `C:\Repos\Main\CustumsDev_Database\CustumsDev_Database\CRM\Stored Procedures\usp_CertificateOfOrigins_GetCertificateOfOriginsByFilter.sql`; `Rpt\Stored Procedures\CertificateOfOrigin_GeneralData.sql` (DisplayedValue usage)
- **L-DOC** `C:\Repos\Main\Infrastructure\Documents\Customs.Infrastructure.DocumentManagemen.RepositoryProxy\DocumentRepositoryUtil.cs` (UploadFile / GetDocumentFile)
- Ledger, previous appendix A, MIGRATION-STATUS.md, MIGRATION-NOT-DONE.md.

## 1. Previous findings re-verification

| ID | Prior status | Verified now? | Evidence |
|---|---|---|---|
| A-01 | FIXED | YES | N-BL:1522-1527 raises ApplicationCorrected after ApplicationReceived when Status=Received and RequestReasonCode=CertificateUpdate (related entity only when certificateToUpdateId given) = L-EVT:69-78 |
| A-02 | FIXED | YES | N-BL:1496-1499 WithoutTask for {EmptyCertificate, Draft, GetRequestStatus, CertificateCancellation}; N-BL:1512-1515 Export org-unit type on Match and Mismatch = L-EVT:220-247 |
| A-03 | CLOSED-OK | YES (still matches; now also guarded) | N-DAL:166-175 empty detail list is a no-op (no delete-all) |
| A-04 | FIXED | YES | N-BL:1369-1373 Id==0 → only ResolveNewInstanceDetailCodes (N-BL:1332-1358) = L-BL:977-1000 |
| A-05 | CLOSED-OK | YES | N-BL:1071-1074 decision comment; no remark-driven feedback |
| A-06 | FIXED | YES | N-BL:1075-1078 `(!new && Original is int && Original != current) \|\| Received`; DTO Original is `int?` (N-DTO:44). Matches L-BL:1054-1079 incl. Received re-fire. Idempotency (A-14) still open |
| A-07 | FIXED | YES | N-VAL: OU rule removed (comment); N-BL:1708-1711 events tolerate OU 0 |
| A-08 | FIXED | YES | N-BL:1686 → N-DAL:103-116 CancelCertificate replaces reason, leaves IsLastVersion |
| A-09 | FIXED | YES | N-BL:1466-1478 = L-BL:648-661 (details lookup guard, second-newest by Title, replacement id first) |
| A-10 | FIXED | YES | N-BL:1578-1586, 1635-1653: ReportId gate + all 16 legacy payload fields incl. IsDeclarationReleased (from request) = L-BL:1146-1153. (Note: in-repo worker DTO uses legacy casing `CertificateTypeID/GUID/CreateCustomerID/OrganizationUnitID`; relies on case-insensitive CloudEvent deserialization — not verified, outside cluster) |
| A-11 | CLOSED-OK | YES | N-SP: `TOP (200)` with decision comment |
| A-12 | CLOSED-OK | YES | N-SP CAST(NULL) titles; N-BL:813-846 fills Title/ExternalIdNum from Customers |
| A-13 | OPEN-BLOCKED | YES (marker present) | N-BL:1085-1090 TODO(blocking) #1 |
| A-14 | OPEN-BLOCKED | YES (marker present) | N-BL:1091-1094 TODO(blocking) #3 |
| A-15 | OPEN-BLOCKED | YES (marker present) | N-BL:1109-1113 TODO(blocking) #2 (second legacy path L-EVT:169-195 is covered by the same dependency, not named) |
| A-16 | OPEN-BLOCKED | YES (marker present) | N-BL:1665-1668 TODO(migration), 1685 TODO(confirm) + hardcoded Hebrew reason text |
| A-17 | OPEN-BLOCKED | YES (marker present) | N-BL:1538-1555 TODO(blocking), MessageTypeId=0 |
| A-18 | CLOSED-OK | YES | N-BL:996, 1214, 1686 `UserId ?? 0` |
| A-19 | CLOSED-OK | YES | N-BL:1458 TODO(migration) |
| A-20 | FIXED | YES (code) | N-BL:1272 `GetFileUrl(response.FileResource)` + TODO(confirm live). But see AN-02: the document it points to is deleted later in the same publish |
| A-21 | FIXED | **PARTIAL** | ExporterId DisplayedValue left as sent (N-BL:1379-1389) and country-group name now resolved (N-BL:1430-1435). Site types still fall to the `else` (N-BL:1436-1441): PortOfShipment / ExportPort / PortOfEntrance / ExitPort get DisplayedValue = raw value, legacy set the site EnglishName (L-BL:1844-1856, 1894-1906). `Lookups.InternationalSite` now exists (used at N-BL:1351), so the "no ILookupUtil type" reason in the comment is stale; no formal TODO marker. Ports are not in Rpt.CertificateOfOrigin_GeneralData, so the effect is UI/display only (LOW) |
| A-22 | CLOSED-OK | YES | N-BL:985-994 RestValidationException |
| A-23 | CLOSED-OK | YES | N-BL:32-33 RestNotFoundException |
| A-24 | CLOSED-OK | YES | N-BL:38-67 every row kept, UserDto.Title |
| A-25 | FIXED | YES | N-SP: `DATEADD(SECOND, 86399, CAST(CAST(@to AS DATE) AS DATETIME))` for both to-bounds; from-bounds `CAST(@from AS DATE)` |
| A-26 | NOT-A-DEFECT-DOC | YES | N-BL:775-781 via ByFilter |
| A-27 | CLOSED-OK | YES | N-BL:716-734; guard/return equal to L-BL:286-307 |

GetById / ByFilter / ExternalIdExist / LoadDataFromExportDeclaration: fresh pass found nothing new (StakeholdersIds computed N-EXT:144 = L-BL:228-232; 7 result sets composed as legacy; ByFilter SP filters column-for-column equal to L-SP apart from the closed A-11/A-12/A-25 items).

## 2. New findings

| ID | Sev | Operation | Deviation | New file:line | Legacy file:line | Why it matters (input → wrong output) |
|---|---|---|---|---|---|---|
| AN-01 | MED | SaveCertificateOfOrigin | **Save is no longer atomic.** The previous same-number version is cancelled with an immediate `ExecuteUpdateAsync` (Cancelled, reason appended, IsLastVersion=false) BEFORE the new row is validated/enriched/inserted; header, detail merge, QR upload, declaration-link backfill and publishing-state are separate commits; no transaction anywhere in the API (grep: no Transaction/BeginTransaction). | N-BL:1007-1009 → N-DAL:420-433; N-BL:1012-1019 (QR proxy, lookups), 1041, 1043, 1049-1052 | L-BL:938-1009: the cancel of `certificateToCancel` and the new certificate are tracked in one UoW and committed once (`CommitAllChanges` at 1009); a throw anywhere before (Validate, QR, code resolution) commits nothing | New-instance save whose insert fails (e.g. TypeId not in the type table → FK, CommonServices.CreateQrCode or a lookup throws) → the previous version is left Cancelled with IsLastVersion=0 and no successor (disappears from `isLastVersion=1` searches); legacy left it untouched. Likewise a failure in UploadQrCodeDocument (Users/Documents proxy) after the header commit leaves a Published row with no QrCodePath, no status events, no attachments — and a retry reloads Original=Published so `isStatusChanged` is false and the publish chain never runs |
| AN-02 | MED | SaveCertificateOfOrigin → QR / PublishAttachments | **QR image is now a Document attached to the certificate, and the same publish deletes it.** New uploads the QR via IDocumentUtil with `EntityId = certificate id, EntityTypeId = CertificateOfOrigin`; the publish then runs SaveCertificateOfOriginAttachments (inline) or the worker, both of which fetch ALL documents of the certificate and delete them before uploading the PDF. Legacy uploaded the QR as a raw repository file (`UploadFile(fileName, Other, -1, stream)` → ServerRelativeUrl), not linked to the certificate entity, so the attachment cleanup never touched it. | N-BL:1251-1273 (upload linked to entity), N-BL:881-886 (delete all entity docs), N-WRK:72-79 (same in worker) | L-BL:1045-1050; L-DOC:196-205 (UploadFile = repository file, orgUnit -1); L-BL:1303-1312 (cleanup only sees entity documents) | Publish of any certificate → QR document created, then deleted seconds later by the attachment step; `QrCodePath` (printed as `QRCode` by Rpt.CertificateOfOrigin_GeneralData) points at a deleted document, so later re-renders (draft/reconciliation re-print) can lose the QR; if the worker fails, a stray .jpg titled with the certificate number stays in the certificate's document list. Not in MIGRATION-NOT-DONE; A-20's TODO(confirm live) does not cover this |
| AN-03 | LOW | SaveCertificateOfOrigin (existing instance) | DisplayedValue rewritten where legacy left it: (a) ExporterCountry / IssuingCountry / CountryOfDeclaration / TransirCountry get the lookup English name — legacy CheckIfCountryInSystemAndIsrael only adds (never-thrown) exceptions; (b) date types always rewritten — legacy set the short date only when in range (DateOfDeclaration today-5..today, ExportDate/ExpectedExitDate today..+3 months); (c) a non-numeric country/city/group value falls to the final `else` and DisplayedValue = raw value — legacy returned untouched; (d) inactive country/city → raw id fallback (lookup filtered by ActiveState; legacy GetCodeById unfiltered) | N-BL:1405-1409, 1415-1424, 1436-1441; sets at 1277-1285 | L-BL:1463-1467, 1559-1592, 1747-1761, 1780-1794, 1822-1836, 1643-1649 | Rpt.CertificateOfOrigin_GeneralData pivots DisplayedValue for ExporterCountry, CountryOfDeclaration, DateOfDeclaration, ExpectedExitDate → an assessor status save on an existing certificate can change the text printed on the certificate vs. legacy (e.g. client-sent country name replaced by lookup EnglishName) |
| AN-04 | LOW | SaveCertificateOfOrigin (new instance) | Unresolvable DestinationCountry alpha-2 / PortOfShipment locode is silently kept and saved; legacy `SystemTablesUtil.GetIdByCode` throws InfException (TheValueInFieldNotExistsInSystem — the exception the legacy's own GetCountryId/GetIdByCode wrappers catch) and the save faults with nothing committed. New also filters State=Active, legacy did not | N-BL:1341-1356 | L-BL:990-998; catch evidence L-BL:1598-1609, 1880-1887 | New certificate with DestinationCountry "XX" → legacy: error, no row; new: row saved with Value "XX" (later DisplayedValue/reconciliation compares a non-id) |

Counts: re-verified 27 (26 YES, 1 PARTIAL — A-21, no regression); new: 0 HIGH, 2 MED, 2 LOW.
