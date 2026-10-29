# CHECK 3 — מצאי TODO מלא (2026-10-11, master @ f9374bf)

נוצר ע"י `grep -rniE "//\s*TODO|/\*\s*TODO|TODO\(|FIXME|HACK|XXX\b" --include=*.cs API/ Workers/ Planar/` (בלי bin/obj/Connected Services). 165 מופעים.

| קובץ:שורה | טקסט |
|---|---|
| `API/CertificateOfOrigins.BL/AuditUserStamp.cs:15` | // TODO(platform): the proper fix is for the request pipeline to populate RequestMetadata.User, after which these |
| `API/CertificateOfOrigins.BL/AuthenticationRequestBl.cs:335` | // TODO(confirm): not exact parity — IsTaskInProgress is TaskStatusID IN (1,4), while the legacy `!= 2` also |
| `API/CertificateOfOrigins.BL/AuthenticationRequestBl.cs:792` | // TODO(migration): LeadDocumentTitle stays null — it's a CRP.DealFile document (no lookup type; needs the |
| `API/CertificateOfOrigins.BL/AuthenticationRequestBl.cs:840` | // TODO(migration): LeadDocumentTitle stays null — CRP.DealFile document, needs the owning service's proxy. |
| `API/CertificateOfOrigins.BL/AuthenticationRequestBl.cs:1509` | // TODO(blocking): NavigationPath is a shared GeneralServices reference table with no platform lookup type yet. |
| `API/CertificateOfOrigins.BL/AuthenticationRequestBl.Schedulers.cs:29` | // TODO(confirm): exact parity needs the raw TaskStatusID (or a status filter) from the Tasks service; until |
| `API/CertificateOfOrigins.BL/CertificateOfOriginsBl.cs:40` | // Users service contract is not published yet: UserDto.Title carries a TODO(blocking)). |
| `API/CertificateOfOrigins.BL/CertificateOfOriginsBl.cs:430` | // service (IDocumentsProxy.GetDocumentsByEntity; route is a rollout TODO(blocking)); 0 when there is no such document. |
| `API/CertificateOfOrigins.BL/CertificateOfOriginsBl.cs:673` | // TODO(internal): the three web-query row titles (fields 20306 / 20310 / 20661) come from the DataDictionary field |
| `API/CertificateOfOrigins.BL/CertificateOfOriginsBl.cs:968` | // TODO(migration): country-group + international-site id→name (no ILookupUtil type — need a SystemTables proxy) and |
| `API/CertificateOfOrigins.BL/CertificateOfOriginsBl.cs:1085` | // TODO(blocking) audited gaps vs legacy SaveCertificateOfOrigin, deferred pending platform/schema: |
| `API/CertificateOfOrigins.BL/CertificateOfOriginsBl.cs:1109` | // TODO(blocking) (#2): legacy called CheckCertificateOfOriginOnDeclarationReleased here (via the service partial, |
| `API/CertificateOfOrigins.BL/CertificateOfOriginsBl.cs:1270` | // TODO(confirm live): not verified against a real Documents service — check that the stored URL opens the QR |
| `API/CertificateOfOrigins.BL/CertificateOfOriginsBl.cs:1327` | // TODO(migration): country-group id→name has no ILookupUtil type yet; the not-in-system / not-in-agreement validation exceptions + date/format checks are deferred (resx). |
| `API/CertificateOfOrigins.BL/CertificateOfOriginsBl.cs:1458` | // own explicit write to persist. TODO(migration): the title-mismatch validation is deferred. |
API/CertificateOfOrigins.BL/CertificateOfOriginsBl.cs:1538:    // 🛑 TODO(blocking): legacy delivered this as an EAI OUTGOING message (OutgoingMessageProxy →
| `API/CertificateOfOrigins.BL/CertificateOfOriginsBl.cs:1551` | MessageTypeId = 0, // TODO(blocking): map the real EMessageTypes value for the certificate request feedback. |
| `API/CertificateOfOrigins.BL/CertificateOfOriginsBl.cs:1665` | // TODO(migration): two resx-sourced texts are still deferred (no ValidationMessages source yet) plus the agent |
| `API/CertificateOfOrigins.BL/CertificateOfOriginsBl.cs:1685` | // TODO(confirm): the UIMessage text of EMessages.CertificateReplaced (not in the local UIMessage copy). |
| `API/CertificateOfOrigins.BL/CertificateOfOriginsBl.cs:1789` | // TODO(migration): only the exception text + EMessages code + Error/Warning level source (ValidationMessages/resx, |
| `API/CertificateOfOrigins.BL/CertificateOfOriginsBl.cs:1901` | // TODO(blocking): the legacy release-publish also sent the request-feedback message (2281) with the rendered certificate PDF. |
| `API/CertificateOfOrigins.BL/CertificateOfOriginsBl.cs:1902` | // It is the same blocker as SaveCertificateOfOrigin TODO(blocking) #1 (parity B-L6): the outgoing EAI channel that carried the |
| `API/CertificateOfOrigins.BL/CertificateOfOriginsBl.cs:1990` | // TODO(migration): the agent talk-back message (legacy servicesAdapter.SendMessageToAgent) is deferred — the same |
| `API/CertificateOfOrigins.BL/CertificateOfOriginsBl.cs:2141` | // TODO(confirm live): B-L9 is covered by unit tests only — run a reconcile match against a real ExportDealFile service |
| `API/CertificateOfOrigins.BL/CertificateOfOriginsBl.cs:2223` | // TODO(migration): the legacy SingleUserAssignmentFilter also carried EProfession.Marech + the Export |
| `API/CertificateOfOrigins.BL/CertificateOfOriginsBl.cs:2252` | // TODO(migration): the localized + English exception texts and their EMessages codes are placeholders here — source |
| `API/CertificateOfOrigins.BL/CertificateOfOriginsBl.cs:2626` | ExceptionType = 0, // TODO(migration): the real EMessages code (legacy GetUIMessageWithEnglishAndLevel). |
| `API/CertificateOfOrigins.BL/CertificateOfOriginsBl.cs:2635` | // BaseValidationMessages package (see Program.cs). TODO(migration): when it lands, set ExceptionType to the real |
| `API/CertificateOfOrigins.BL/CertificateOfOriginsBl.MessageValidationMessages.cs:12` | // TODO(migration): when the resx / BaseValidationMessages pipeline lands (see Program.cs), source the text from |
| `API/CertificateOfOrigins.BL/CertificateOfOriginsBl.MessageValidationMessages.cs:121` | [EMessageCode.CertificateTypeNotExist] = "סוג תעודה לא קיים במערכת", // TODO(migration): confirm exact UIMessage text for code 5022 against the module's UIMessage export. |
| `API/CertificateOfOrigins.BL/CertificateOfOriginsBl.Templates.cs:86` | // TODO(internal): the South-Korea letter's .docx + .yml do not exist yet (new in CR 194221, no legacy source). To |
| `API/CertificateOfOrigins.BL/CertificateOfOriginsConsts.cs:69` | public const string CertificateUpdateReceived = "התקבל עדכון לתעודה"; // TODO(migration): source from ValidationMessages/resx. |
| `API/CertificateOfOrigins.BL/CertificateOfOriginsConsts.cs:82` | public const string CanceledDeclarationReason = "ההצהרה בוטלה"; // TODO(migration): source the exact text from ValidationMessages/resx (EServerTerms.CanceledDeclaration). |
| `API/CertificateOfOrigins.BL/ExportDocumentAuthenticationRequestBl.cs:34` | // TODO(confirm): the UIMessage text of InvalidIdentificationNumber (not in the local UIMessage copy). |
| `API/CertificateOfOrigins.BL/ExportDocumentAuthenticationRequestBl.cs:47` | // TODO(confirm): the UIMessage text of NoCustomHouseForThisCountry (not in the local UIMessage copy). |
| `API/CertificateOfOrigins.BL/Lookups/CountryCountryGroup.cs:5` | // TODO(internal): Lookup.CountryCountryGroup (the link table General_cl_CountryCountryGroup: which country belongs to which |
| `API/CertificateOfOrigins.BL/Lookups/CountryGroup.cs:5` | // TODO(internal): Lookup.CountryGroup (General_enum_CountryGroup) is missing from CustomsCloud.InfrastructureCore.Lookup |
| `API/CertificateOfOrigins.BL/Lookups/CurrencyType.cs:5` | // TODO(internal): Lookup.CurrencyType (General_enum_CurrencyType) is missing from CustomsCloud.InfrastructureCore.Lookup |
| `API/CertificateOfOrigins.BL/Lookups/DataDictionaryField.cs:5` | // TODO(internal): Lookup.DataDictionaryField (the DataDictionary module's field catalogue, Customs.DataDictionary.Entities) |
| `API/CertificateOfOrigins.BL/Lookups/InternationalSite.cs:5` | // TODO(internal): Lookup.InternationalSite is missing from CustomsCloud.InfrastructureCore.Lookup 1.10.120. Legacy read it |
| `API/CertificateOfOrigins.BL/Lookups/LocalLookupResolver.cs:9` | // TODO(internal): local stand-in for the platform's BaseLookupResolver<T>, which is internal to |
| `API/CertificateOfOrigins.BL/Lookups/LocalLookupServicesConfiguration.cs:9` | // TODO(internal): the registration twin of the platform's AddLookup<T> for the local lookup types in this folder (same |
| `API/CertificateOfOrigins.BL/Lookups/PackingType.cs:5` | // TODO(internal): Lookup.PackingType (CargoControl_c_PackingType) is missing from CustomsCloud.InfrastructureCore.Lookup |
| `API/CertificateOfOrigins.BL/Lookups/Site.cs:5` | // TODO(internal): Lookup.Site exists in CustomsCloud.InfrastructureCore.Lookup 1.10.120 but lacks the two fields legacy read |
| `API/CertificateOfOrigins.BL/Proxies/Collateral/CollateralMockProxy.cs:22` | CollateralRequestId = (entityId * 10) + 1,       // TODO: dummy data |
| `API/CertificateOfOrigins.BL/Proxies/Collateral/CollateralMockProxy.cs:23` | CustomerId = 279366,                             // TODO: dummy data |
| `API/CertificateOfOrigins.BL/Proxies/Collateral/CollateralMockProxy.cs:24` | CollateralType = 1,                              // TODO: dummy data |
| `API/CertificateOfOrigins.BL/Proxies/Collateral/CollateralMockProxy.cs:25` | AmountToGrant = 1000m,                           // TODO: dummy data |
| `API/CertificateOfOrigins.BL/Proxies/Collateral/CollateralMockProxy.cs:27` | CollateralRequestStatus = 1,                     // TODO: dummy data |
| `API/CertificateOfOrigins.BL/Proxies/Collateral/CollateralMockProxy.cs:28` | CollateralRequestStatusName = "Mock collateral", // TODO: dummy data |
| `API/CertificateOfOrigins.BL/Proxies/Collateral/CollateralMockProxy.cs:54` | return Task.FromResult<List<int>?>([(entityId * 10) + 1]); // TODO: dummy data |
| `API/CertificateOfOrigins.BL/Proxies/Collateral/CollateralProxy.cs:18` | .WithResource("api/Collateral/CollateralRequestByEntity/{entityType}/{entityId}") // TODO(blocking): confirm endpoint name/route with the Collateral microservice |
| `API/CertificateOfOrigins.BL/Proxies/Collateral/CollateralProxy.cs:28` | // TODO(blocking): the legacy call passed a second argument, `true` (ExternalProxy.ChangeTempCollateralRequest(list, true); |
| `API/CertificateOfOrigins.BL/Proxies/Collateral/CollateralProxy.cs:39` | .WithResource("api/Collateral/ChangeTempCollateralRequest") // TODO(blocking): confirm endpoint name/route with the Collateral microservice |
| `API/CertificateOfOrigins.BL/Proxies/Collateral/CollateralProxy.cs:50` | .WithResource("api/Collateral/CollateralRequestIdsByEntity/{entityType}/{entityId}") // TODO(blocking): confirm endpoint name/route with the Collateral microservice |
| `API/CertificateOfOrigins.BL/Proxies/Collateral/CollateralProxy.cs:62` | .WithResource("api/Collateral/GrantAllCollateralRequests") // TODO(blocking): confirm endpoint name/route with the Collateral microservice |
| `API/CertificateOfOrigins.BL/Proxies/Collateral/CollateralProxy.cs:73` | .WithResource("api/Collateral/DebitCreditCollateralRequest") // TODO(blocking): confirm endpoint name/route with the Collateral microservice |
| `API/CertificateOfOrigins.BL/Proxies/CommonServices/CommonServicesMockProxy.cs:19` | return Task.FromResult<byte[]?>([0xFF, 0xD8, 0xFF, 0xD9]); // TODO: dummy JPEG bytes |
| `API/CertificateOfOrigins.BL/Proxies/CommonServices/CommonServicesMockProxy.cs:33` | Name = $"Mock certificate template {templateId}",           // TODO: dummy data |
| `API/CertificateOfOrigins.BL/Proxies/CommonServices/CommonServicesMockProxy.cs:34` | Content = Encoding.ASCII.GetBytes("%PDF-1.4\nmock certificate\n%%EOF"), // TODO: dummy PDF bytes |
| `API/CertificateOfOrigins.BL/Proxies/CommonServices/CommonServicesMockProxy.cs:35` | DocumentTypeId = 0,                                          // TODO: dummy data |
| `API/CertificateOfOrigins.BL/Proxies/CommonServices/CommonServicesProxy.cs:16` | .WithResource("api/CommonServices/CreateQrCode") // TODO(blocking): confirm endpoint name/route with the Common (QR) microservice |
| `API/CertificateOfOrigins.BL/Proxies/CommonServices/CommonServicesProxy.cs:24` | // TODO(blocking): the Templates generation service + the certificate-of-origin templates are not yet migrated |
| `API/CertificateOfOrigins.BL/Proxies/Customer/CustomerMockProxy.cs:20` | Id = id,                          // TODO: dummy data |
| `API/CertificateOfOrigins.BL/Proxies/Customer/CustomerMockProxy.cs:21` | Title = "Test Customer " + id,     // TODO: dummy data |
| `API/CertificateOfOrigins.BL/Proxies/Customer/CustomerMockProxy.cs:37` | Id = customerId,                  // TODO: dummy data |
| `API/CertificateOfOrigins.BL/Proxies/Customer/CustomerMockProxy.cs:43` | new CustomerAddressDto { AddressPurpose = 1, AddressSingleLine = "רחוב הבדיקה 1, חיפה" }, // TODO: dummy authentication address |
| `API/CertificateOfOrigins.BL/Proxies/Customer/CustomerMockProxy.cs:60` | Id = countryId * 100,                          // TODO: dummy data |
| `API/CertificateOfOrigins.BL/Proxies/Customer/CustomerMockProxy.cs:61` | Title = "Foreign Customs House " + countryId,   // TODO: dummy data |
| `API/CertificateOfOrigins.BL/Proxies/Customer/CustomerMockProxy.cs:78` | return Task.FromResult<int?>(int.TryParse(externalId, out var id) ? id : 777); // TODO: dummy data |
| `API/CertificateOfOrigins.BL/Proxies/Customer/CustomerProxy.cs:17` | .WithResource("api/Customer/CustomersByIds") // TODO(blocking): confirm endpoint name/route with the Customers microservice |
| `API/CertificateOfOrigins.BL/Proxies/Customer/CustomerProxy.cs:27` | .WithResource("api/Customer/CustomerInformation/{customerId}") // TODO(blocking): confirm endpoint name/route with the Customers microservice |
| `API/CertificateOfOrigins.BL/Proxies/Customer/CustomerProxy.cs:40` | .WithResource("api/Customer/CustomersByCountry/{countryId}/{activityType}") // TODO(blocking): confirm endpoint name/route with the Customers microservice |
| `API/CertificateOfOrigins.BL/Proxies/Customer/CustomerProxy.cs:51` | .WithResource("api/Customer/IdByExternalId/{externalId}") // TODO(blocking): confirm endpoint name/route with the Customers microservice |
| `API/CertificateOfOrigins.BL/Proxies/CustomsBook/CustomsBookMockProxy.cs:51` | return Task.FromResult<int?>((sum % 100000) + 1); // TODO: dummy data |
| `API/CertificateOfOrigins.BL/Proxies/CustomsBook/CustomsBookProxy.cs:17` | // TODO(blocking): confirm the CustomsBook endpoint route (until then the mock is enabled via x-mock-mode). |
| `API/CertificateOfOrigins.BL/Proxies/CustomsBook/CustomsBookProxy.cs:34` | // TODO(blocking): confirm the CustomsBook endpoint route (until then the mock is enabled via x-mock-mode). |
| `API/CertificateOfOrigins.BL/Proxies/CustomsBook/CustomsBookProxy.cs:45` | // TODO(blocking): confirm the CustomsBook endpoint route (until then the mock is enabled via x-mock-mode). "Export" |
| `API/CertificateOfOrigins.BL/Proxies/CustomsBook/CustomsBookProxy.cs:47` | // as the reference date — TODO(confirm): how the CustomsBook route takes it. |
| `API/CertificateOfOrigins.BL/Proxies/Documents/DocumentsMockProxy.cs:22` | Id = (entityId * 10) + 1,               // TODO: dummy data |
| `API/CertificateOfOrigins.BL/Proxies/Documents/DocumentsMockProxy.cs:23` | TypeId = 124,                            // TODO: dummy data (a value from CertificateOfOriginsDocumentsFilter) |
| `API/CertificateOfOrigins.BL/Proxies/Documents/DocumentsMockProxy.cs:26` | Title = "Mock document A",              // TODO: dummy data |
| `API/CertificateOfOrigins.BL/Proxies/Documents/DocumentsMockProxy.cs:30` | StringDynamicParams = "mock notes A",  // TODO: dummy data |
| `API/CertificateOfOrigins.BL/Proxies/Documents/DocumentsMockProxy.cs:35` | Id = (entityId * 10) + 2,               // TODO: dummy data |
| `API/CertificateOfOrigins.BL/Proxies/Documents/DocumentsMockProxy.cs:36` | TypeId = 184,                            // TODO: dummy data |
| `API/CertificateOfOrigins.BL/Proxies/Documents/DocumentsMockProxy.cs:39` | Title = "Mock document B",              // TODO: dummy data |
| `API/CertificateOfOrigins.BL/Proxies/Documents/DocumentsMockProxy.cs:43` | StringDynamicParams = "mock notes B",  // TODO: dummy data |
| `API/CertificateOfOrigins.BL/Proxies/Documents/DocumentsMockProxy.cs:48` | Id = (entityId * 10) + 3,               // TODO: dummy data |
| `API/CertificateOfOrigins.BL/Proxies/Documents/DocumentsMockProxy.cs:52` | Title = "Mock certificate document",    // TODO: dummy data |
| `API/CertificateOfOrigins.BL/Proxies/Documents/DocumentsMockProxy.cs:56` | StringDynamicParams = "mock notes C",  // TODO: dummy data |
| `API/CertificateOfOrigins.BL/Proxies/Documents/DocumentsMockProxy.cs:79` | Id = documentId,                                        // TODO: dummy data |
| `API/CertificateOfOrigins.BL/Proxies/Documents/DocumentsMockProxy.cs:80` | TypeId = 124,                                           // TODO: dummy data |
| `API/CertificateOfOrigins.BL/Proxies/Documents/DocumentsMockProxy.cs:81` | Title = "Mock lead document",                          // TODO: dummy data |
| `API/CertificateOfOrigins.BL/Proxies/Documents/DocumentsMockProxy.cs:83` | ExternalId = "EXT-" + documentId,                      // TODO: dummy data |
| `API/CertificateOfOrigins.BL/Proxies/Documents/DocumentsMockProxy.cs:84` | StringDynamicParams = "mock notes",                    // TODO: dummy data |
| `API/CertificateOfOrigins.BL/Proxies/Documents/DocumentsProxy.cs:19` | .WithResource("api/Document/DocumentsByEntity/{entityId}/{entityTypeId}") // TODO(blocking): confirm endpoint name/route with the Documents microservice |
| `API/CertificateOfOrigins.BL/Proxies/Documents/DocumentsProxy.cs:32` | .WithResource("api/Document/DeleteDocuments") // TODO(blocking): confirm endpoint name/route with the Documents microservice |
| `API/CertificateOfOrigins.BL/Proxies/Documents/DocumentsProxy.cs:43` | .WithResource("api/Document/{documentId}") // TODO(blocking): confirm endpoint name/route with the Documents microservice |
| `API/CertificateOfOrigins.BL/Proxies/Documents/DocumentsProxy.cs:55` | .WithResource("api/Document/AttachDocumentsToEntity") // TODO(blocking): confirm endpoint name/route with the Documents microservice |
| `API/CertificateOfOrigins.BL/Proxies/ExportDealFile/ExportDealFileMockProxy.cs:22` | LeadDocumentId = leadDocumentId ?? 0,          // TODO: dummy data |
| `API/CertificateOfOrigins.BL/Proxies/ExportDealFile/ExportDealFileMockProxy.cs:23` | IsDeclarationReleased = true,                  // TODO: dummy data |
| `API/CertificateOfOrigins.BL/Proxies/ExportDealFile/ExportDealFileMockProxy.cs:24` | IsCargoExitedOfCustomsRegulation = true,       // TODO: dummy data |
| `API/CertificateOfOrigins.BL/Proxies/ExportDealFile/ExportDealFileMockProxy.cs:29` | LeadDocumentStateId = 0,                       // TODO: dummy data |
| `API/CertificateOfOrigins.BL/Proxies/ExportDealFile/ExportDealFileMockProxy.cs:42` | return Task.FromResult<DateTime?>(new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Unspecified)); // TODO: dummy data |
| `API/CertificateOfOrigins.BL/Proxies/ExportDealFile/ExportDealFileMockProxy.cs:55` | LeadDocumentId = (newCertificateOfOriginId * 10) + 7, // TODO: dummy data |
| `API/CertificateOfOrigins.BL/Proxies/ExportDealFile/ExportDealFileMockProxy.cs:71` | LeadDocumentId = (certificateOfOriginId * 10) + 3, // TODO: dummy data |
| `API/CertificateOfOrigins.BL/Proxies/ExportDealFile/ExportDealFileMockProxy.cs:72` | LeadDocumentTitle = $"DECL-{certificateOfOriginId}", // TODO: dummy data |
| `API/CertificateOfOrigins.BL/Proxies/ExportDealFile/ExportDealFileMockProxy.cs:93` | ExternalIdNum = "INV1", // TODO: dummy data — matches the mock certificate invoice number |
| `API/CertificateOfOrigins.BL/Proxies/ExportDealFile/ExportDealFileMockProxy.cs:96` | new ExportGoodsItemInfoDto { CustomsItemId = 123456, OriginCountryId = 32 }, // TODO: dummy data |
| `API/CertificateOfOrigins.BL/Proxies/ExportDealFile/ExportDealFileMockProxy.cs:104` | LeadDocumentState = mockUtil.HasMockFeature("DealFile.NotReleased") ? 0 : (int)ELeadDocumentState.Released, // TODO: dummy data |
| `API/CertificateOfOrigins.BL/Proxies/ExportDealFile/ExportDealFileMockProxy.cs:105` | DestinationCountryId = 32,   // TODO: dummy data |
| `API/CertificateOfOrigins.BL/Proxies/ExportDealFile/ExportDealFileMockProxy.cs:106` | ExporterCustomerId = 888,    // TODO: dummy data |
| `API/CertificateOfOrigins.BL/Proxies/ExportDealFile/ExportDealFileMockProxy.cs:107` | OrganizationUnitId = 1,      // TODO: dummy data |
| `API/CertificateOfOrigins.BL/Proxies/ExportDealFile/ExportDealFileMockProxy.cs:124` | new ExportAssociatedGoodsItemDto { AssociatedOriginCountryId = 32 }, // TODO: dummy data |
| `API/CertificateOfOrigins.BL/Proxies/ExportDealFile/ExportDealFileProxy.cs:15` | // TODO(blocking): the ExportDealFile microservice is not yet stood up — confirm the endpoint name/route |
| `API/CertificateOfOrigins.BL/Proxies/ExportDealFile/ExportDealFileProxy.cs:40` | .WithResource("api/ExportDealFile/LeadDocumentSubmissionDate/{leadDocumentId}") // TODO(blocking): confirm endpoint name/route with the DealFile microservice |
| `API/CertificateOfOrigins.BL/Proxies/ExportDealFile/ExportDealFileProxy.cs:51` | .WithResource("api/ExportDealFile/LeadDocumentByCertificateOfOrigin/{oldCertificateOfOriginId}/{newCertificateOfOriginId}") // TODO(blocking): confirm endpoint name/route with the DealFile microservice |
| `API/CertificateOfOrigins.BL/Proxies/ExportDealFile/ExportDealFileProxy.cs:62` | .WithResource("api/ExportDealFile/LeadDocumentByCertificateOfOriginId/{certificateOfOriginId}") // TODO(blocking): confirm endpoint name/route with the DealFile microservice |
| `API/CertificateOfOrigins.BL/Proxies/ExportDealFile/ExportDealFileProxy.cs:73` | .WithResource("api/ExportDealFile/ChangeCertificateOfOriginForLeadDocument/{leadDocumentId}/{oldCertificateOfOriginId}/{newCertificateOfOriginId}") // TODO(blocking): confirm endpoint name/route with the DealFile microservice |
| `API/CertificateOfOrigins.BL/Proxies/ExportDealFile/ExportDealFileProxy.cs:84` | .WithResource("api/ExportDealFile/ExportDeclarationInfo/{leadDocumentId}") // TODO(blocking): confirm endpoint name/route with the DealFile microservice |
| `API/CertificateOfOrigins.BL/Proxies/ExportDealFile/ExportDealFileProxy.cs:95` | .WithResource("api/ExportDealFile/DetailsForExportAssociatedGoodsItems/{leadDocumentId}") // TODO(blocking): confirm endpoint name/route with the DealFile microservice |
| `API/CertificateOfOrigins.BL/Proxies/MessageManagement/MessageManagementProxy.cs:18` | .WithResource("api/Message/SendMessage") // TODO(blocking): confirm endpoint name/route with the Message-Management (Common) microservice |
| `API/CertificateOfOrigins.BL/Proxies/OrganizationUnit/OrganizationUnitProxy.cs:16` | // TODO(blocking): confirm the Users endpoint route (until then the mock is enabled via x-mock-mode). |
| `API/CertificateOfOrigins.BL/Proxies/Tasks/TasksMockProxy.cs:25` | TaskTypeId = taskTypeId,   // TODO: dummy data |
| `API/CertificateOfOrigins.BL/Proxies/Tasks/TasksMockProxy.cs:27` | UserId = 5,                // TODO: dummy data |
| `API/CertificateOfOrigins.BL/Proxies/Tasks/TasksMockProxy.cs:43` | return Task.FromResult<int?>(mockUtil.HasMockFeature("Tasks.NoAssessor") ? null : 5); // TODO: dummy data |
| `API/CertificateOfOrigins.BL/Proxies/Tasks/TasksProxy.cs:18` | .WithResource("api/Task/IsTaskExist") // TODO(blocking): confirm endpoint name/route with the Tasks microservice |
| `API/CertificateOfOrigins.BL/Proxies/Tasks/TasksProxy.cs:31` | .WithResource("api/Task/IsTaskExistsOnEntity") // TODO(blocking): confirm endpoint name/route with the Tasks microservice |
| `API/CertificateOfOrigins.BL/Proxies/Tasks/TasksProxy.cs:39` | // TODO(blocking): confirm endpoint name/route with the Tasks microservice. The real endpoint returns a |
| `API/CertificateOfOrigins.BL/Proxies/User/UserMockProxy.cs:20` | Id = id,                      // TODO: dummy data |
| `API/CertificateOfOrigins.BL/Proxies/User/UserMockProxy.cs:21` | Title = "Test User " + id,     // TODO: dummy data |
| `API/CertificateOfOrigins.BL/Proxies/User/UserMockProxy.cs:22` | OrganizationUnit = 6,         // TODO: dummy data (e.g. חיפה) |
| `API/CertificateOfOrigins.BL/Proxies/User/UserProxy.cs:16` | .WithResource("api/User/UsersByIds") // TODO(blocking): confirm endpoint name/route with the Users microservice |
| `API/CertificateOfOrigins.BL/Proxies/Vendor/VendorMockProxy.cs:20` | Id = id,                       // TODO: dummy data |
| `API/CertificateOfOrigins.BL/Proxies/Vendor/VendorMockProxy.cs:21` | Title = "Test Vendor " + id,    // TODO: dummy data |
| `API/CertificateOfOrigins.BL/Proxies/Vendor/VendorProxy.cs:16` | .WithResource("api/Vendor/VendorsByIds") // TODO(blocking): confirm endpoint name/route with the Vendors microservice |
| `API/CertificateOfOrigins.BL/ServicesConfiguration.cs:36` | // TODO(blocking): verify the real Customers endpoint (CustomersByIds) before ROLLOUT. |
| `API/CertificateOfOrigins.BL/ServicesConfiguration.cs:40` | // TODO(blocking): verify the real Vendors endpoint (VendorsByIds) before ROLLOUT. |
| `API/CertificateOfOrigins.BL/ServicesConfiguration.cs:45` | // TODO(blocking): verify the real Users endpoint (User/UsersByIds) before ROLLOUT. |
| `API/CertificateOfOrigins.BL/ServicesConfiguration.cs:48` | // TODO(blocking): the ExportDealFile microservice is not yet stood up — the mock is the practical |
| `API/CertificateOfOrigins.BL/ServicesConfiguration.cs:53` | // TODO(blocking): verify the real Documents endpoint (Document/DocumentsByEntity) before ROLLOUT. |
| `API/CertificateOfOrigins.BL/ServicesConfiguration.cs:57` | // TODO(blocking): verify the real Collateral (Collateral/CollateralRequestByEntity) endpoint before ROLLOUT. |
| `API/CertificateOfOrigins.BL/ServicesConfiguration.cs:60` | // TODO(blocking): verify the real Tasks (Task/IsTaskExist) endpoint before ROLLOUT. |
| `API/CertificateOfOrigins.BL/ServicesConfiguration.cs:64` | // the Common service). TODO(blocking): verify the real Message-Management (Message/SendMessage) endpoint before ROLLOUT. |
| `API/CertificateOfOrigins.BL/ServicesConfiguration.cs:69` | // TODO(blocking): confirm each owning microservice + endpoint route before ROLLOUT. |
| `API/CertificateOfOrigins.BL/ServicesConfiguration.cs:117` | // TODO(internal): lookup types missing from the platform Lookup package — local stand-ins (Lookups/), loaded and |
| `API/CertificateOfOrigins.BL/ServicesConfiguration.cs:119` | // TODO(internal): confirm the InternationalSite source service (SystemTables assumed — Lookup.Site loads from Sites). |
| `API/CertificateOfOrigins.BL/ServicesConfiguration.cs:124` | // platform Lookup.Country (General_c_Country), the Common service. TODO(internal): confirm the source with the platform. |
| `API/CertificateOfOrigins.BL/ServicesConfiguration.cs:129` | // like the other General tables. TODO(internal): confirm the source with the platform. |
| `API/CertificateOfOrigins.BL/ServicesConfiguration.cs:133` | // CargoControl tables. TODO(internal): confirm the source with the platform. |
| `API/CertificateOfOrigins.BL/ServicesConfiguration.cs:137` | // CustomsMicroServices; legacy read it through SystemTablesUtil. TODO(internal): confirm the source with the platform. |
| `API/CertificateOfOrigins.BL/ServicesConfiguration.cs:140` | // TODO(blocking): GetPathsForNavigationToVendor needs a NavigationPath lookup. NavigationPath is a shared |
| `API/CertificateOfOrigins.DAL/CertificateOfOriginsDal.cs:1183` | // BL (TODO(blocking))", which was stale — only the proxy's endpoint route is still a rollout TODO.) |
| `API/CertificateOfOrigins.Model/ModelDTOs/CertificateOfOriginWebInvoiceDetailDto.cs:7` | // not available in the platform, so it is deferred (see the BL — TODO(blocking)). |
| `API/CertificateOfOrigins.Model/ModelDTOs/CollateralRequestDto.cs:4` | // carried as their numeric codes (the wire values). TODO(blocking): confirm the Collateral service's response |
| `API/CertificateOfOrigins.Model/ModelDTOs/ECertificateOfOriginsTemplate.cs:11` | // TODO(confirm): the numeric value. The legacy keyed these off the shared ETemplate enum, which lives only in a |
| `API/CertificateOfOrigins.Model/ModelDTOs/GetAuthenticationRequestByIdResultDto.cs:60` | // TypeName carries the legacy document FileUrl (the DocumentType name). The proxy route is a rollout TODO(blocking). |
| `API/CertificateOfOrigins.Model/ModelDTOs/GetAuthenticationRequestByIdResultDto.cs:64` | // IExportDealFileProxy.GetLeadDocumentSubmissionDate. The proxy route is a rollout TODO(blocking). |
| `API/CertificateOfOrigins.Model/ModelDTOs/SouthKoreaOriginVerificationLetterResult.cs:37` | // ⚠️ TODO(confirm): mapped to the file's Notes column, which is the only free-text field on the entity. Confirm |
API/CertificateOfOrigins.Model/ModelDTOs/SouthKoreaOriginVerificationLetterResult.cs:42:    // 🛑 TODO(blocking): no confirmed source. Candidates are the file's UserID, its UpdateUserID, or the acting user
API/CertificateOfOrigins.Model/ModelDTOs/SouthKoreaOriginVerificationLetterResult.cs:50:    // 🛑 TODO(blocking): nothing in this service stores or serves a user signature image, so the tag renders empty.
| `API/CertificateOfOrigins.Model/ModelDTOs/UserDto.cs:11` | // keeps the UserName alias. TODO(blocking): confirm the field name exposed by the Users microservice (User/UsersByIds) - |
| `API/CertificateOfOrigins.Model/ModelDTOs/UserDto.cs:16` | // attachments. TODO(blocking): confirm the field name exposed by the Users microservice (User/UsersByIds). |
| `API/CertificateOfOrigins.Model/ModelDTOs/VendorDto.cs:5` | // `vv.Title VendorName`). The Vendors service contract is not published yet (route TODO(blocking)); the field name follows the |
| `API/CertificateOfOrigins.WebApi/Program.cs:20` | // TODO(blocking): .AddValidationMessages<ValidationMessages>() — re-enable when the |
