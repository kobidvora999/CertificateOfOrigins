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
tail -14 "$S"

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
