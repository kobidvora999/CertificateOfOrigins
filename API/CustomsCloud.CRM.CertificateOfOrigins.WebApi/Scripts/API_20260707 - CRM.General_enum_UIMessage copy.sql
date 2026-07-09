-- CRM.General_enum_UIMessage — copy of the system UI-messages table into this service's DB.
-- Fixes over the draft script:
--   1. CREATE TABLE if missing (the table does not exist in the CertificateOfOrigins DB).
--   2. Explicit column list on the INSERT — required when IDENTITY_INSERT is ON.
--   3. Idempotent — inserts only IDs that are not already present (safe to re-run).
-- Source: the local PreRulings extraction. NOTE: the PreRulings copy is a SUBSET of the monolith
-- table — the certificate-of-origin message IDs (6540..14027278, see ErrorMessagesResources.resx)
-- are NOT in it; re-run this script against the full internal table before internal integration.
IF OBJECT_ID('CRM.General_enum_UIMessage') IS NULL
BEGIN
    CREATE TABLE CRM.General_enum_UIMessage
    (
        ID INT IDENTITY(1, 1) NOT NULL CONSTRAINT PK_General_enum_UIMessage PRIMARY KEY,
        Name NVARCHAR(255) NOT NULL,
        State INT NOT NULL,
        Description NVARCHAR(255) NULL,
        EnglishName NVARCHAR(1000) NOT NULL,
        Enumeration NVARCHAR(120) NOT NULL,
        UserFriendlyMessage NVARCHAR(1000) NULL,
        UIMessageTypeID INT NOT NULL,
        ModuleID INT NOT NULL,
        StartDate DATETIME NULL,
        EndDate DATETIME NULL,
        Arabic_UserFriendlyMessage NVARCHAR(1000) NULL,
        French_UserFriendlyMessage NVARCHAR(1000) NULL
    );
END
GO

SET IDENTITY_INSERT CRM.General_enum_UIMessage ON;

INSERT INTO CRM.General_enum_UIMessage
    (ID, Name, State, Description, EnglishName, Enumeration, UserFriendlyMessage,
     UIMessageTypeID, ModuleID, StartDate, EndDate, Arabic_UserFriendlyMessage, French_UserFriendlyMessage)
SELECT GEUM.ID,
       GEUM.Name,
       GEUM.State,
       GEUM.Description,
       GEUM.EnglishName,
       GEUM.Enumeration,
       GEUM.UserFriendlyMessage,
       GEUM.UIMessageTypeID,
       GEUM.ModuleID,
       GEUM.StartDate,
       GEUM.EndDate,
       GEUM.Arabic_UserFriendlyMessage,
       GEUM.French_UserFriendlyMessage
FROM PreRulings.CRM.General_enum_UIMessage GEUM
WHERE NOT EXISTS (SELECT 1 FROM CRM.General_enum_UIMessage T WHERE T.ID = GEUM.ID);

SET IDENTITY_INSERT CRM.General_enum_UIMessage OFF;
GO
