-- USE removed: DbUp already connects to the target database. A hardcoded USE redirected these statements
-- to a database literally named CertificateOfOrigins, so on any other target (or a from-zero replay) the
-- objects/parameters landed in the wrong database while the script still reported success.
GO

-- GetPC_MSG2280_2281 message field-validation engine: the Israel country-id used by the country validators
-- (legacy Configuration.GetConfig<int>(CertificateOfOriginsConstants.CountryIsrael) -> parametersUtil.Get<int>("CountryIsrael")).
IF NOT EXISTS (
    SELECT 1
    FROM [Infrastructure].[Parameters]
    WHERE [Name] = 'CountryIsrael'
)
BEGIN
    INSERT INTO [Infrastructure].[Parameters]
           ([Name],[Description],[Value],[UpdateDate],[UpdateUser],[Regex],[Level],[Active])
    VALUES
           ('CountryIsrael',
            N'מזהה מדינת ישראל (country id) לבדיקות ארץ-ישראל במנוע ולידציית מסר תעודת מקור',
            N'376',
            GETDATE(),
            NULL,
            NULL,
            1,
            1);
END
GO
