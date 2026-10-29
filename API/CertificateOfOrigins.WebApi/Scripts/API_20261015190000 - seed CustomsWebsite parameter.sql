-- USE removed: DbUp already connects to the target database (see the earlier parameter seeds).
GO

-- The importer alert letter (template 2244) prints the customs website in its footer (<CustomsWebsite_ForSignature>).
-- Legacy: the template merger's AddSignatureDetails, Configuration.GetConfig<string>("CustomsWebsite") ->
-- parametersUtil.Get<string>("CustomsWebsite"). Value from the legacy configuration (supplied by the developer, 15.10.2026).
IF NOT EXISTS (
    SELECT 1
    FROM [Infrastructure].[Parameters]
    WHERE [Name] = 'CustomsWebsite'
)
BEGIN
    INSERT INTO [Infrastructure].[Parameters]
           ([Name],[Description],[Value],[UpdateDate],[UpdateUser],[Regex],[Level],[Active])
    VALUES
           ('CustomsWebsite',
            N'כתובת אתר המכס המודפסת בכותרת התחתונה של מכתבי התבניות',
            N'http://go.gov.il/taxes',
            GETDATE(),
            NULL,
            NULL,
            1,
            1);
END
GO
