USE [CertificateOfOrigins];
GO

IF NOT EXISTS (
    SELECT 1
    FROM [Infrastructure].[Parameters]
    WHERE [Name] = 'MaxSumForAftaAndEuroExamptFromOriginCertificate'
)
BEGIN
    INSERT INTO [Infrastructure].[Parameters]
           ([Name],[Description],[Value],[UpdateDate],[UpdateUser],[Regex],[Level],[Active])
    VALUES
           ('MaxSumForAftaAndEuroExamptFromOriginCertificate',
            N'סכום מרבי עבור פטור תעודת מקור',
            N'6000',
            GETDATE(),
            NULL,
            NULL,
            1,
            1);
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM [Infrastructure].[Parameters]
    WHERE [Name] = 'DaysForFirstReminderInAuthenticationRequest1'
)
BEGIN
    INSERT INTO [Infrastructure].[Parameters]
           ([Name],[Description],[Value],[UpdateDate],[UpdateUser],[Regex],[Level],[Active])
    VALUES
           ('DaysForFirstReminderInAuthenticationRequest1',
            N'מספר ימים לתזכורת ראשונה בתיק אימות1',
            N'6',
            GETDATE(),
            NULL,
            NULL,
            1,
            1);
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM [Infrastructure].[Parameters]
    WHERE [Name] = 'DaysForFirstReminderInAuthenticationRequest2'
)
BEGIN
    INSERT INTO [Infrastructure].[Parameters]
           ([Name],[Description],[Value],[UpdateDate],[UpdateUser],[Regex],[Level],[Active])
    VALUES
           ('DaysForFirstReminderInAuthenticationRequest2',
            N'מספר ימים לתזכורת ראשונה בתיק אימות2',
            N'10',
            GETDATE(),
            NULL,
            NULL,
            1,
            1);
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM [Infrastructure].[Parameters]
    WHERE [Name] = 'CertificateOfOriginsDocumentsFilter'
)
BEGIN
    INSERT INTO [Infrastructure].[Parameters]
           ([Name],[Description],[Value],[UpdateDate],[UpdateUser],[Regex],[Level],[Active])
    VALUES
           ('CertificateOfOriginsDocumentsFilter',
            N'רשימת מסמכים הנתמכים ע"י מסמכי העדפה',
            N'124,184,318,185,175,126,18',
            GETDATE(),
            NULL,
            NULL,
            1,
            1);
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM [Infrastructure].[Parameters]
    WHERE [Name] = 'IsAuthenticationRequestNeedOnVersion'
)
BEGIN
    INSERT INTO [Infrastructure].[Parameters]
           ([Name],[Description],[Value],[UpdateDate],[UpdateUser],[Regex],[Level],[Active])
    VALUES
           ('IsAuthenticationRequestNeedOnVersion',
            N'האם מסמכי אימות נדרשים בגירסה',
            N'True',
            GETDATE(),
            NULL,
            NULL,
            1,
            1);
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM [Infrastructure].[Parameters]
    WHERE [Name] = 'IsCertificateOfOriginsVisible'
)
BEGIN
    INSERT INTO [Infrastructure].[Parameters]
           ([Name],[Description],[Value],[UpdateDate],[UpdateUser],[Regex],[Level],[Active])
    VALUES
           ('IsCertificateOfOriginsVisible',
            N'הצגת מסכי מסמכי העדפה',
            N'True',
            GETDATE(),
            NULL,
            NULL,
            1,
            1);
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM [Infrastructure].[Parameters]
    WHERE [Name] = 'DaysForReminderForImporterScheduler'
)
BEGIN
    INSERT INTO [Infrastructure].[Parameters]
           ([Name],[Description],[Value],[UpdateDate],[UpdateUser],[Regex],[Level],[Active])
    VALUES
           ('DaysForReminderForImporterScheduler',
            N'מספר ימים לבדיקה שליחת תזכורת ליבואן',
            N'45',
            GETDATE(),
            NULL,
            NULL,
            1,
            1);
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM [Infrastructure].[Parameters]
    WHERE [Name] = 'DaysForFirstReminderInAuthenticationRequest3'
)
BEGIN
    INSERT INTO [Infrastructure].[Parameters]
           ([Name],[Description],[Value],[UpdateDate],[UpdateUser],[Regex],[Level],[Active])
    VALUES
           ('DaysForFirstReminderInAuthenticationRequest3',
            N'מספר ימים לתזכורת ראשונה בתיק אימות3',
            N'3',
            GETDATE(),
            NULL,
            NULL,
            1,
            1);
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM [Infrastructure].[Parameters]
    WHERE [Name] = 'CertificateOfOriginsIssuingUser'
)
BEGIN
    INSERT INTO [Infrastructure].[Parameters]
           ([Name],[Description],[Value],[UpdateDate],[UpdateUser],[Regex],[Level],[Active])
    VALUES
           ('CertificateOfOriginsIssuingUser',
            N'מפיק המכתב',
            N'14742860',
            GETDATE(),
            NULL,
            NULL,
            1,
            1);
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM [Infrastructure].[Parameters]
    WHERE [Name] = 'CertificateOfOriginQueryURL'
)
BEGIN
    INSERT INTO [Infrastructure].[Parameters]
           ([Name],[Description],[Value],[UpdateDate],[UpdateUser],[Regex],[Level],[Active])
    VALUES
           ('CertificateOfOriginQueryURL',
            N'קישרון לשאילתת מסמכי מקור',
            N'http://10.25.218.28/ShaarOlami2/Dev/CertificateOfOrigin?guid={0}',
            GETDATE(),
            NULL,
            NULL,
            1,
            1);
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM [Infrastructure].[Parameters]
    WHERE [Name] = 'ExportAuthenticationRelevantDocumentTypes'
)
BEGIN
    INSERT INTO [Infrastructure].[Parameters]
           ([Name],[Description],[Value],[UpdateDate],[UpdateUser],[Regex],[Level],[Active])
    VALUES
           ('ExportAuthenticationRelevantDocumentTypes',
            N'סוגי מסמכים רלוונטיים עבור אימות יצוא',
            N'5, 14, 175, 536, 537',
            GETDATE(),
            NULL,
            NULL,
            1,
            1);
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM [Infrastructure].[Parameters]
    WHERE [Name] = 'DaysForFirstReminderInExportAuthenticationRequest1'
)
BEGIN
    INSERT INTO [Infrastructure].[Parameters]
           ([Name],[Description],[Value],[UpdateDate],[UpdateUser],[Regex],[Level],[Active])
    VALUES
           ('DaysForFirstReminderInExportAuthenticationRequest1',
            N'מספר ימים לתזכורת ראשונה בתיק אימות-יצוא',
            N'6',
            GETDATE(),
            NULL,
            NULL,
            1,
            1);
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM [Infrastructure].[Parameters]
    WHERE [Name] = 'DaysForSecondReminderInExportAuthenticationRequest2'
)
BEGIN
    INSERT INTO [Infrastructure].[Parameters]
           ([Name],[Description],[Value],[UpdateDate],[UpdateUser],[Regex],[Level],[Active])
    VALUES
           ('DaysForSecondReminderInExportAuthenticationRequest2',
            N'מספר ימים לתזכורת שניה בתיק אימות-יצוא',
            N'10',
            GETDATE(),
            NULL,
            NULL,
            1,
            1);
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM [Infrastructure].[Parameters]
    WHERE [Name] = 'AdditionalRequestsForSearchInDays'
)
BEGIN
    INSERT INTO [Infrastructure].[Parameters]
           ([Name],[Description],[Value],[UpdateDate],[UpdateUser],[Regex],[Level],[Active])
    VALUES
           ('AdditionalRequestsForSearchInDays',
            N'מספר ימים לחיפוש בקשות נוספות אימות-יבוא',
            N'10',
            GETDATE(),
            NULL,
            NULL,
            1,
            1);
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM [Infrastructure].[Parameters]
    WHERE [Name] = 'NumberOfMonthToSearchImportAuthenticationRequest'
)
BEGIN
    INSERT INTO [Infrastructure].[Parameters]
           ([Name],[Description],[Value],[UpdateDate],[UpdateUser],[Regex],[Level],[Active])
    VALUES
           ('NumberOfMonthToSearchImportAuthenticationRequest',
            N'מספר חודשים לחיפוש בקשות אימות יבוא',
            N'12',
            GETDATE(),
            NULL,
            NULL,
            1,
            1);
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM [Infrastructure].[Parameters]
    WHERE [Name] = 'InvoiceGoodsItemTaxDifferenceMinValue'
)
BEGIN
    INSERT INTO [Infrastructure].[Parameters]
           ([Name],[Description],[Value],[UpdateDate],[UpdateUser],[Regex],[Level],[Active])
    VALUES
           ('InvoiceGoodsItemTaxDifferenceMinValue',
            N'מינימום סה"כ הטבת מס (מכס + מע"מ)',
            N'5000',
            GETDATE(),
            NULL,
            NULL,
            1,
            1);
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM [Infrastructure].[Parameters]
    WHERE [Name] = 'MonthForFinalReminderInAuthenticationRequest'
)
BEGIN
    INSERT INTO [Infrastructure].[Parameters]
           ([Name],[Description],[Value],[UpdateDate],[UpdateUser],[Regex],[Level],[Active])
    VALUES
           ('MonthForFinalReminderInAuthenticationRequest',
            N'מספר חודשים לתזכורת אחרונה בתיק אימות',
            N'9',
            GETDATE(),
            NULL,
            NULL,
            1,
            1);
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM [Infrastructure].[Parameters]
    WHERE [Name] = 'IsProduceTemplateOfCertificateOfOriginWithSSRS'
)
BEGIN
    INSERT INTO [Infrastructure].[Parameters]
           ([Name],[Description],[Value],[UpdateDate],[UpdateUser],[Regex],[Level],[Active])
    VALUES
           ('IsProduceTemplateOfCertificateOfOriginWithSSRS',
            N'האם להפיק תבניות של מסמכי העדפה דרך הSSRS',
            N'True',
            GETDATE(),
            NULL,
            NULL,
            1,
            1);
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM [Infrastructure].[Parameters]
    WHERE [Name] = 'CountriesExemptedFromSendingThePlaceOfManufacture'
)
BEGIN
    INSERT INTO [Infrastructure].[Parameters]
           ([Name],[Description],[Value],[UpdateDate],[UpdateUser],[Regex],[Level],[Active])
    VALUES
           ('CountriesExemptedFromSendingThePlaceOfManufacture',
            N'מדינות הפטורות מדרישת שליחת מקום ייצור הטובין',
            N'804',
            GETDATE(),
            NULL,
            NULL,
            1,
            1);
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM [Infrastructure].[Parameters]
    WHERE [Name] = 'IsNeedToLockCertificateOfOrigin'
)
BEGIN
    INSERT INTO [Infrastructure].[Parameters]
           ([Name],[Description],[Value],[UpdateDate],[UpdateUser],[Regex],[Level],[Active])
    VALUES
           ('IsNeedToLockCertificateOfOrigin',
            N'האם לבצע נעילה על ישות תעודת מקור',
            N'True',
            GETDATE(),
            NULL,
            NULL,
            1,
            1);
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM [Infrastructure].[Parameters]
    WHERE [Name] = 'IssueCertificateOfOriginByWorker'
)
BEGIN
    INSERT INTO [Infrastructure].[Parameters]
           ([Name],[Description],[Value],[UpdateDate],[UpdateUser],[Regex],[Level],[Active])
    VALUES
           ('IssueCertificateOfOriginByWorker	',
            N'הנפקת תעודת מקור ע"י וורקר',
            N'False',
            GETDATE(),
            NULL,
            NULL,
            1,
            1);
END
GO
