-- Seed for the internal-workload "Authentication Lifecycle" collection.
--
-- WHY A SEED AND NOT AN API CHAIN: this service has NO insert path for
-- CRM.CertificateOfOrigins_ImportAuthenticationRequest. The DAL only reads it and updates it set-based
-- (6 ExecuteUpdateAsync sites, zero Add/Update tracked) - the rows originate in another service. So the
-- by-id read, its mapper (AuthenticationRequestBl.MapToResultDto, 26 lines) and the delivery-status machine
-- are unreachable from the API alone. postman-coverage permits a seed for exactly this case.
--
-- Idempotent: fixed ids in a 99xxxx range that no real data uses, guarded by NOT EXISTS, safe to re-run.
-- FK parents (enum_Circumstances, enum_PrefernceDocumentType, enum_Decision, ImportAuthenticationFileDetails)
-- are all seeded by API_20260715 - seed data.sql, which must have run first.

DECLARE @fileId   int = 990001;
DECLARE @docLinked   int = 990101;   -- request attached to the file above
DECLARE @docUnlinked int = 990102;   -- request with no file (the AuthenticationFileID IS NULL branch)
DECLARE @now datetime = GETDATE();

-- The parent file. Audit user 5 matches the CC-USER-ID the collections send.
IF NOT EXISTS (SELECT 1 FROM CRM.CertificateOfOrigins_ImportAuthenticationFileDetails WHERE ID = @fileId)
BEGIN
    SET IDENTITY_INSERT CRM.CertificateOfOrigins_ImportAuthenticationFileDetails ON;
    INSERT INTO CRM.CertificateOfOrigins_ImportAuthenticationFileDetails
        (ID, State, CreateDate, CreateUserID, UpdateDate, UpdateUserID, AuthenticationFileStatusID,
         RequestCountryID, UserID, PostalAdress, DeliveryMethodID, EmailAdress, ReminderMethodID, UserNameIssuingLetter)
    VALUES
        (@fileId, 1, @now, 5, @now, 5, 1, 32, 5, N'seed address', 1, N'seed@example.com', 1, N'seed user');
    SET IDENTITY_INSERT CRM.CertificateOfOrigins_ImportAuthenticationFileDetails OFF;
END

-- Two requests: one linked to the file, one unlinked, so both branches of the mapper are reachable.
IF NOT EXISTS (SELECT 1 FROM CRM.CertificateOfOrigins_ImportAuthenticationRequest WHERE DocumentID = @docLinked)
    INSERT INTO CRM.CertificateOfOrigins_ImportAuthenticationRequest
        (DocumentID, CreateDate, CreateUserID, UpdateDate, UpdateUserID, AuthenticationFileID,
         AuthenticationRequestDate, LeadDocumentID, DocumentIssuingDate, ImportCountryID, IssuingCountryID,
         ItemDetailID, Number, IsOldIndication, OriginCountryID, PreferenceDocumentTypeID, Remarks,
         RequestCircumstancesID, UserResponseID, ResponseNameEmail, ResponsePhoneNum, OrganizationUnitID, UserID,
         VendorId, VendorName)
    VALUES
        (@docLinked, @now, 5, @now, 5, @fileId, @now, 990201, @now, 32, 32,
         1, 1, 0, 32, 1, N'seed linked request', 1, 5, N'seed@example.com', N'0500000000', 1, 5, 777, N'Seed Vendor');

IF NOT EXISTS (SELECT 1 FROM CRM.CertificateOfOrigins_ImportAuthenticationRequest WHERE DocumentID = @docUnlinked)
    INSERT INTO CRM.CertificateOfOrigins_ImportAuthenticationRequest
        (DocumentID, CreateDate, CreateUserID, UpdateDate, UpdateUserID, AuthenticationFileID,
         AuthenticationRequestDate, LeadDocumentID, DocumentIssuingDate, ImportCountryID, IssuingCountryID,
         ItemDetailID, Number, IsOldIndication, OriginCountryID, PreferenceDocumentTypeID, Remarks,
         RequestCircumstancesID, UserResponseID, ResponseNameEmail, ResponsePhoneNum, OrganizationUnitID, UserID,
         VendorId, VendorName)
    VALUES
        (@docUnlinked, @now, 5, @now, 5, NULL, @now, 990202, @now, 32, 32,
         2, 2, 0, 32, 1, N'seed unlinked request', 1, 5, N'seed@example.com', N'0500000000', 1, 5, 778, N'Seed Vendor 2');

SELECT (SELECT COUNT(*) FROM CRM.CertificateOfOrigins_ImportAuthenticationFileDetails WHERE ID = @fileId) AS SeededFiles,
       (SELECT COUNT(*) FROM CRM.CertificateOfOrigins_ImportAuthenticationRequest WHERE DocumentID IN (@docLinked, @docUnlinked)) AS SeededRequests;

-- Reset the seeded rows to their starting state on every run. The delivery scenarios ADVANCE the file's status
-- machine, so without this reset a second run would start from an advanced status and take different branches --
-- the run would stop being repeatable. These ids are test-only, so resetting them is safe; the NOT EXISTS guards
-- above deliberately do not update, this does.
UPDATE CRM.CertificateOfOrigins_ImportAuthenticationFileDetails
   SET AuthenticationFileStatusID = 1, DeliveryMethodID = 1, ReminderMethodID = 1, LastDelivery = NULL,
       UpdateDate = @now, UpdateUserID = 5
 WHERE ID = @fileId;

UPDATE CRM.CertificateOfOrigins_ImportAuthenticationRequest
   SET DecisionID = NULL, CollateralID = NULL, IsOldIndication = 0, UpdateDate = @now, UpdateUserID = 5
 WHERE DocumentID IN (@docLinked, @docUnlinked);

-- ---------------------------------------------------------------------------------------------------------
-- Two MORE requests, dedicated to the "Save Import Decisions" collection (SaveImportAuthenticationRequest).
-- They are separate from 990101/990102 on purpose: that save is a set-based UPDATE that writes
-- AuthenticationFileID, DecisionID, VendorID and more, and the Auth Lifecycle collection asserts exactly those
-- columns on 990101/990102. The collections run in PARALLEL, so sharing rows would be a race.
DECLARE @docDecisionA int = 990103;   -- exercised WITH an authentication file on the request
DECLARE @docDecisionB int = 990104;   -- exercised with AuthenticationFileID null on the request

IF NOT EXISTS (SELECT 1 FROM CRM.CertificateOfOrigins_ImportAuthenticationRequest WHERE DocumentID = @docDecisionA)
    INSERT INTO CRM.CertificateOfOrigins_ImportAuthenticationRequest
        (DocumentID, CreateDate, CreateUserID, UpdateDate, UpdateUserID, AuthenticationFileID,
         AuthenticationRequestDate, LeadDocumentID, DocumentIssuingDate, ImportCountryID, IssuingCountryID,
         ItemDetailID, Number, IsOldIndication, OriginCountryID, PreferenceDocumentTypeID, Remarks,
         RequestCircumstancesID, UserResponseID, ResponseNameEmail, ResponsePhoneNum, OrganizationUnitID, UserID,
         VendorId, VendorName)
    VALUES
        (@docDecisionA, @now, 5, @now, 5, @fileId, @now, 990203, @now, 32, 32,
         3, 3, 0, 32, 1, N'seed decision request A', 1, 5, N'seed@example.com', N'0500000000', 1, 5, 777, N'Seed Vendor');

IF NOT EXISTS (SELECT 1 FROM CRM.CertificateOfOrigins_ImportAuthenticationRequest WHERE DocumentID = @docDecisionB)
    INSERT INTO CRM.CertificateOfOrigins_ImportAuthenticationRequest
        (DocumentID, CreateDate, CreateUserID, UpdateDate, UpdateUserID, AuthenticationFileID,
         AuthenticationRequestDate, LeadDocumentID, DocumentIssuingDate, ImportCountryID, IssuingCountryID,
         ItemDetailID, Number, IsOldIndication, OriginCountryID, PreferenceDocumentTypeID, Remarks,
         RequestCircumstancesID, UserResponseID, ResponseNameEmail, ResponsePhoneNum, OrganizationUnitID, UserID,
         VendorId, VendorName)
    VALUES
        (@docDecisionB, @now, 5, @now, 5, NULL, @now, 990204, @now, 32, 32,
         4, 4, 0, 32, 1, N'seed decision request B', 1, 5, N'seed@example.com', N'0500000000', 1, 5, 778, N'Seed Vendor 2');

-- The decision scenarios overwrite DecisionID / AuthenticationFileID / VendorID on these two rows, so reset them
-- the same way as the pair above.
UPDATE CRM.CertificateOfOrigins_ImportAuthenticationRequest
   SET DecisionID = NULL, CollateralID = NULL, AuthenticationFileID = CASE WHEN DocumentID = @docDecisionA THEN @fileId ELSE NULL END,
       VendorId = CASE WHEN DocumentID = @docDecisionA THEN 777 ELSE 778 END,
       UpdateDate = @now, UpdateUserID = 5
 WHERE DocumentID IN (@docDecisionA, @docDecisionB);

-- ---------------------------------------------------------------------------------------------------------
-- A SECOND file (990002) with its own two requests, for the "Auth File Status" collection.
-- Separate from 990001 on purpose: that collection drives the file status machine, and one of its scenarios
-- is CancelledFile, which calls UnlinkAllRequestsFromFile - it would detach 990101 from 990001 and break the
-- Auth Lifecycle assertions. The collections run in PARALLEL.
-- 990105 carries an ImporterID; 990101-990104 leave it NULL, which is why FillAuthenticationRequestNames'
-- importer block had never run (importerIds.Count was always 0).
DECLARE @fileId2 int = 990002;
DECLARE @docFileA int = 990105;
DECLARE @docFileB int = 990106;

IF NOT EXISTS (SELECT 1 FROM CRM.CertificateOfOrigins_ImportAuthenticationFileDetails WHERE ID = @fileId2)
BEGIN
    SET IDENTITY_INSERT CRM.CertificateOfOrigins_ImportAuthenticationFileDetails ON;
    INSERT INTO CRM.CertificateOfOrigins_ImportAuthenticationFileDetails
        (ID, State, CreateDate, CreateUserID, UpdateDate, UpdateUserID, AuthenticationFileStatusID,
         RequestCountryID, UserID, PostalAdress, DeliveryMethodID, EmailAdress, ReminderMethodID, UserNameIssuingLetter)
    VALUES
        (@fileId2, 1, @now, 5, @now, 5, 1, 32, 5, N'seed address 2', 1, N'seed2@example.com', 1, N'seed user 2');
    SET IDENTITY_INSERT CRM.CertificateOfOrigins_ImportAuthenticationFileDetails OFF;
END

IF NOT EXISTS (SELECT 1 FROM CRM.CertificateOfOrigins_ImportAuthenticationRequest WHERE DocumentID = @docFileA)
    INSERT INTO CRM.CertificateOfOrigins_ImportAuthenticationRequest
        (DocumentID, CreateDate, CreateUserID, UpdateDate, UpdateUserID, AuthenticationFileID,
         AuthenticationRequestDate, LeadDocumentID, DocumentIssuingDate, ImportCountryID, IssuingCountryID,
         ItemDetailID, Number, IsOldIndication, OriginCountryID, PreferenceDocumentTypeID, Remarks,
         RequestCircumstancesID, UserResponseID, ResponseNameEmail, ResponsePhoneNum, OrganizationUnitID, UserID,
         VendorId, VendorName, ImporterID)
    VALUES
        (@docFileA, @now, 5, @now, 5, @fileId2, @now, 990205, @now, 32, 32,
         5, 5, 0, 32, 1, N'seed file request A', 1, 5, N'seed@example.com', N'0500000000', 1, 5, 777, N'Seed Vendor', 279366);

IF NOT EXISTS (SELECT 1 FROM CRM.CertificateOfOrigins_ImportAuthenticationRequest WHERE DocumentID = @docFileB)
    INSERT INTO CRM.CertificateOfOrigins_ImportAuthenticationRequest
        (DocumentID, CreateDate, CreateUserID, UpdateDate, UpdateUserID, AuthenticationFileID,
         AuthenticationRequestDate, LeadDocumentID, DocumentIssuingDate, ImportCountryID, IssuingCountryID,
         ItemDetailID, Number, IsOldIndication, OriginCountryID, PreferenceDocumentTypeID, Remarks,
         RequestCircumstancesID, UserResponseID, ResponseNameEmail, ResponsePhoneNum, OrganizationUnitID, UserID,
         VendorId, VendorName, ImporterID)
    VALUES
        (@docFileB, @now, 5, @now, 5, @fileId2, @now, 990206, @now, 32, 32,
         6, 6, 0, 32, 1, N'seed file request B', 1, 5, N'seed@example.com', N'0500000000', 1, 5, 778, N'Seed Vendor 2', 279367);

-- The status scenarios advance the file and may unlink its requests, so reset both every run.
UPDATE CRM.CertificateOfOrigins_ImportAuthenticationFileDetails
   SET AuthenticationFileStatusID = 1, DeliveryMethodID = 1, ReminderMethodID = 1, LastDelivery = NULL,
       UpdateDate = @now, UpdateUserID = 5
 WHERE ID = @fileId2;

UPDATE CRM.CertificateOfOrigins_ImportAuthenticationRequest
   SET DecisionID = NULL, CollateralID = NULL, AuthenticationFileID = @fileId2,
       UpdateDate = @now, UpdateUserID = 5
 WHERE DocumentID IN (@docFileA, @docFileB);
