-- CR 194221 — "סגירה מנהלית" (administrative closure).
--
-- ⚠️ ENCODING: this file is UTF-8 without a BOM (the repo convention — see "API_20260715 - seed data.sql"). sqlcmd
-- defaults to the ANSI code page for -i input and will silently insert mojibake for the Hebrew names; run it as
--     sqlcmd -f 65001 -S <server> -d <db> -i "<this file>"
-- or apply it through SSMS / Azure Data Studio, which read UTF-8 correctly. Verify afterwards with
--     SELECT UNICODE(SUBSTRING(Name,1,1)) FROM CRM.CertificateOfOrigins_enum_Decision WHERE ID = 10;  -- must be 1505
--
-- Two new lookup values, both id 10 (the next free id in each table — verified against the repo seed, the local
-- database, and the legacy CRM database: both tables end at 9).
--
--  1. CRM.CertificateOfOrigins_enum_Decision           — a request-level decision.
--  2. CRM.CertificateOfOrigins_enum_AuthenticationFileStatus — a file-level status.
--
-- Flag choices (confirmed with the analyst, 2026-09-22):
--   Decision.IsForClaliMakorWorker = 1 — the decision dropdown on the authentication-file screen is filtered by this
--     flag (legacy AuthenticationRequestFilePresenter: Decisions.Where(d => d.IsForClaliMakorWorker)). IsForCoordinator
--     stays 0: that flag feeds a different screen (ImportProcessForm), which this CR does not touch.
--   Decision.IsAutomatic = 0 — set by a user, not by the system.
--   AuthenticationFileStatus.IsAutomatic = 0 — the file-status dropdown is filtered by !IsAutomatic (same presenter),
--     so an automatic status would never be selectable.
--
-- Idempotent per row, because both tables are already populated (the original seed guards on the whole table).

IF NOT EXISTS (SELECT 1 FROM CRM.CertificateOfOrigins_enum_Decision WHERE ID = 10)
BEGIN
    INSERT INTO CRM.CertificateOfOrigins_enum_Decision
        (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate, IsAutomatic, IsForCoordinator, IsForClaliMakorWorker)
    VALUES
        (10, N'סגירה מנהלית', 1, N'סגירה מנהלית', N'AdministrativeClosure', N'AdministrativeClosure', NULL, NULL,
         CONVERT(bit, 'False'), CONVERT(bit, 'False'), CONVERT(bit, 'True'));
END

IF NOT EXISTS (SELECT 1 FROM CRM.CertificateOfOrigins_enum_AuthenticationFileStatus WHERE ID = 10)
BEGIN
    INSERT INTO CRM.CertificateOfOrigins_enum_AuthenticationFileStatus
        (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate, IsAutomatic)
    VALUES
        (10, N'סגירה מנהלית', 1, N'סגירה מנהלית', N'AdministrativeClosure', N'AdministrativeClosure', NULL, NULL,
         CONVERT(bit, 'False'));
END
