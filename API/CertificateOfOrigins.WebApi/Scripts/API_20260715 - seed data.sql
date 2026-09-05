-- ---------------------------------------------------------------------------------------------
-- The reference-data blocks below are NOT ordered by foreign key: a child table (e.g.
-- CRM.CertificateOfOrigins_c_OriginCriterion, CRM.CertificateOfOrigins_cl_DetailsPerCertificate)
-- is seeded before the enum table it references, so a from-zero run failed with Msg 547.
-- Rather than reorder ~4,900 lines of generated seed, FK checking is suspended for the duration
-- and RE-VALIDATED at the end of this script with WITH CHECK CHECK CONSTRAINT ALL — the data is
-- still proven consistent, it just no longer depends on statement order.
-- ---------------------------------------------------------------------------------------------
DECLARE @coo_nocheck nvarchar(max) = N'';
SELECT @coo_nocheck = @coo_nocheck + N'ALTER TABLE [' + s.name + N'].[' + t.name + N'] NOCHECK CONSTRAINT ALL;'
FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = N'CRM';
EXEC sp_executesql @coo_nocheck;

IF NOT EXISTS (
    SELECT 1
    FROM [CRM].[CertificateOfOrigins_c_OriginCriterion]
)
BEGIN
 SET IDENTITY_INSERT CRM.CertificateOfOrigins_c_OriginCriterion ON
INSERT INTO CRM.CertificateOfOrigins_c_OriginCriterion (ID, Name, State, Description, EnglishName, StartDate, EndDate, CertificateOfOriginTypeCodeID, OriginCriterionCode) VALUES

(1, N'SET1', 1, N'A', N'A', CONVERT(DATETIME, '1900-01-01 00:00:00.000', 121), CONVERT(DATETIME, '9999-12-31 00:00:00.000', 121), 3, N'A')

 
 
INSERT INTO CRM.CertificateOfOrigins_c_OriginCriterion (ID, Name, State, Description, EnglishName, StartDate, EndDate, CertificateOfOriginTypeCodeID, OriginCriterionCode) VALUES

(2, N'SET2', 1, N'B', N'B', CONVERT(DATETIME, '1900-01-01 00:00:00.000', 121), CONVERT(DATETIME, '9999-12-31 00:00:00.000', 121), 3, N'B')

 
 
INSERT INTO CRM.CertificateOfOrigins_c_OriginCriterion (ID, Name, State, Description, EnglishName, StartDate, EndDate, CertificateOfOriginTypeCodeID, OriginCriterionCode) VALUES

(3, N'SET3', 1, N'C', N'C', CONVERT(DATETIME, '1900-01-01 00:00:00.000', 121), CONVERT(DATETIME, '9999-12-31 00:00:00.000', 121), 3, N'C')

 
 
INSERT INTO CRM.CertificateOfOrigins_c_OriginCriterion (ID, Name, State, Description, EnglishName, StartDate, EndDate, CertificateOfOriginTypeCodeID, OriginCriterionCode) VALUES

(4, N'SET4', 1, N'A', N'A', CONVERT(DATETIME, '1900-01-01 00:00:00.000', 121), CONVERT(DATETIME, '9999-12-31 00:00:00.000', 121), 4, N'A')

 
 
INSERT INTO CRM.CertificateOfOrigins_c_OriginCriterion (ID, Name, State, Description, EnglishName, StartDate, EndDate, CertificateOfOriginTypeCodeID, OriginCriterionCode) VALUES

(5, N'SET5', 1, N'B', N'B', CONVERT(DATETIME, '1900-01-01 00:00:00.000', 121), CONVERT(DATETIME, '9999-12-31 00:00:00.000', 121), 4, N'B')

 
 
INSERT INTO CRM.CertificateOfOrigins_c_OriginCriterion (ID, Name, State, Description, EnglishName, StartDate, EndDate, CertificateOfOriginTypeCodeID, OriginCriterionCode) VALUES

(6, N'SET6', 1, N'C', N'C', CONVERT(DATETIME, '1900-01-01 00:00:00.000', 121), CONVERT(DATETIME, '9999-12-31 00:00:00.000', 121), 4, N'C')

 
 
INSERT INTO CRM.CertificateOfOrigins_c_OriginCriterion (ID, Name, State, Description, EnglishName, StartDate, EndDate, CertificateOfOriginTypeCodeID, OriginCriterionCode) VALUES

(7, N'SET7', 1, N'D', N'D', CONVERT(DATETIME, '1900-01-01 00:00:00.000', 121), CONVERT(DATETIME, '9999-12-31 00:00:00.000', 121), 4, N'D')

 
 
INSERT INTO CRM.CertificateOfOrigins_c_OriginCriterion (ID, Name, State, Description, EnglishName, StartDate, EndDate, CertificateOfOriginTypeCodeID, OriginCriterionCode) VALUES

(8, N'SET8', 1, N'A', N'A', NULL, NULL, 6, N'A')

 
 
INSERT INTO CRM.CertificateOfOrigins_c_OriginCriterion (ID, Name, State, Description, EnglishName, StartDate, EndDate, CertificateOfOriginTypeCodeID, OriginCriterionCode) VALUES

(9, N'SET9', 1, N'B', N'B', NULL, NULL, 6, N'B')

 
 
INSERT INTO CRM.CertificateOfOrigins_c_OriginCriterion (ID, Name, State, Description, EnglishName, StartDate, EndDate, CertificateOfOriginTypeCodeID, OriginCriterionCode) VALUES

(10, N'SET10', 1, N'C', N'C', NULL, NULL, 6, N'C')

 
 
INSERT INTO CRM.CertificateOfOrigins_c_OriginCriterion (ID, Name, State, Description, EnglishName, StartDate, EndDate, CertificateOfOriginTypeCodeID, OriginCriterionCode) VALUES

(11, N'SET11', 1, N'D', N'D', NULL, NULL, 6, N'D')

 
 
INSERT INTO CRM.CertificateOfOrigins_c_OriginCriterion (ID, Name, State, Description, EnglishName, StartDate, EndDate, CertificateOfOriginTypeCodeID, OriginCriterionCode) VALUES

(12, N'SET12', 1, N'OP', N'OP', NULL, NULL, 7, N'OP')

 
 
INSERT INTO CRM.CertificateOfOrigins_c_OriginCriterion (ID, Name, State, Description, EnglishName, StartDate, EndDate, CertificateOfOriginTypeCodeID, OriginCriterionCode) VALUES

(13, N'SET13', 1, N'PE', N'PE', NULL, NULL, 7, N'PE')

 
 
INSERT INTO CRM.CertificateOfOrigins_c_OriginCriterion (ID, Name, State, Description, EnglishName, StartDate, EndDate, CertificateOfOriginTypeCodeID, OriginCriterionCode) VALUES

(14, N'SET14', 1, N'PSR', N'PSR', NULL, NULL, 7, N'PSR')

 
 
INSERT INTO CRM.CertificateOfOrigins_c_OriginCriterion (ID, Name, State, Description, EnglishName, StartDate, EndDate, CertificateOfOriginTypeCodeID, OriginCriterionCode) VALUES

(15, N'SET15', 1, N'WO', N'WO', NULL, NULL, 7, N'WO')

 
 
INSERT INTO CRM.CertificateOfOrigins_c_OriginCriterion (ID, Name, State, Description, EnglishName, StartDate, EndDate, CertificateOfOriginTypeCodeID, OriginCriterionCode) VALUES

(16, N'SET16', 1, N'PE', N'PE', NULL, NULL, 9, N'PE')

 
 
INSERT INTO CRM.CertificateOfOrigins_c_OriginCriterion (ID, Name, State, Description, EnglishName, StartDate, EndDate, CertificateOfOriginTypeCodeID, OriginCriterionCode) VALUES

(17, N'SET17', 1, N'PSR', N'PSR', NULL, NULL, 9, N'PSR')

 
 
INSERT INTO CRM.CertificateOfOrigins_c_OriginCriterion (ID, Name, State, Description, EnglishName, StartDate, EndDate, CertificateOfOriginTypeCodeID, OriginCriterionCode) VALUES

(18, N'SET18', 1, N'WO', N'WO', NULL, NULL, 9, N'WO')

 SET IDENTITY_INSERT CRM.CertificateOfOrigins_c_OriginCriterion OFF
END


IF NOT EXISTS (
    SELECT 1
    FROM [CRM].[CertificateOfOrigins_cf_CertificateOfOriginTypeByTradeAgreement]
)
BEGIN 

 SET IDENTITY_INSERT CRM.CertificateOfOrigins_cf_CertificateOfOriginTypeByTradeAgreement ON

INSERT INTO CRM.CertificateOfOrigins_cf_CertificateOfOriginTypeByTradeAgreement (ID, CertificateOfOriginTypeCodeID, TradeAgreementID, ValidFrom, ValidTo) VALUES

(2, 2, 2, CONVERT(DATETIME, '2014-10-23 20:30:30.507', 121), CONVERT(DATETIME, '2017-10-23 20:30:30.507', 121))

 
 
INSERT INTO CRM.CertificateOfOrigins_cf_CertificateOfOriginTypeByTradeAgreement (ID, CertificateOfOriginTypeCodeID, TradeAgreementID, ValidFrom, ValidTo) VALUES

(3, 1, 17, CONVERT(DATETIME, '2014-10-23 20:30:30.600', 121), CONVERT(DATETIME, '2017-10-23 20:30:30.600', 121))

 
 
INSERT INTO CRM.CertificateOfOrigins_cf_CertificateOfOriginTypeByTradeAgreement (ID, CertificateOfOriginTypeCodeID, TradeAgreementID, ValidFrom, ValidTo) VALUES

(4, 3, 19, CONVERT(DATETIME, '2014-10-23 20:30:30.607', 121), CONVERT(DATETIME, '2017-10-23 20:30:30.607', 121))

 
 
INSERT INTO CRM.CertificateOfOrigins_cf_CertificateOfOriginTypeByTradeAgreement (ID, CertificateOfOriginTypeCodeID, TradeAgreementID, ValidFrom, ValidTo) VALUES

(5, 3, 20, CONVERT(DATETIME, '2014-10-23 20:30:30.613', 121), CONVERT(DATETIME, '2017-10-23 20:30:30.613', 121))

 
 
INSERT INTO CRM.CertificateOfOrigins_cf_CertificateOfOriginTypeByTradeAgreement (ID, CertificateOfOriginTypeCodeID, TradeAgreementID, ValidFrom, ValidTo) VALUES

(6, 3, 21, CONVERT(DATETIME, '2014-10-23 20:30:30.613', 121), CONVERT(DATETIME, '2017-10-23 20:30:30.613', 121))

 
 
INSERT INTO CRM.CertificateOfOrigins_cf_CertificateOfOriginTypeByTradeAgreement (ID, CertificateOfOriginTypeCodeID, TradeAgreementID, ValidFrom, ValidTo) VALUES

(7, 3, 22, CONVERT(DATETIME, '2014-10-23 20:30:30.613', 121), CONVERT(DATETIME, '2017-10-23 20:30:30.613', 121))

 
 
INSERT INTO CRM.CertificateOfOrigins_cf_CertificateOfOriginTypeByTradeAgreement (ID, CertificateOfOriginTypeCodeID, TradeAgreementID, ValidFrom, ValidTo) VALUES

(8, 5, 2, CONVERT(DATETIME, '2014-10-23 20:30:30.613', 121), CONVERT(DATETIME, '2017-10-23 20:30:30.613', 121))

 
 
INSERT INTO CRM.CertificateOfOrigins_cf_CertificateOfOriginTypeByTradeAgreement (ID, CertificateOfOriginTypeCodeID, TradeAgreementID, ValidFrom, ValidTo) VALUES

(9, 5, 3, CONVERT(DATETIME, '2014-10-23 20:30:30.613', 121), CONVERT(DATETIME, '2017-10-23 20:30:30.613', 121))

 
 
INSERT INTO CRM.CertificateOfOrigins_cf_CertificateOfOriginTypeByTradeAgreement (ID, CertificateOfOriginTypeCodeID, TradeAgreementID, ValidFrom, ValidTo) VALUES

(10, 5, 4, CONVERT(DATETIME, '2014-10-23 20:30:30.617', 121), CONVERT(DATETIME, '2017-10-23 20:30:30.617', 121))

 
 
INSERT INTO CRM.CertificateOfOrigins_cf_CertificateOfOriginTypeByTradeAgreement (ID, CertificateOfOriginTypeCodeID, TradeAgreementID, ValidFrom, ValidTo) VALUES

(11, 5, 5, CONVERT(DATETIME, '2014-10-23 20:30:30.617', 121), CONVERT(DATETIME, '2017-10-23 20:30:30.617', 121))

 
 
INSERT INTO CRM.CertificateOfOrigins_cf_CertificateOfOriginTypeByTradeAgreement (ID, CertificateOfOriginTypeCodeID, TradeAgreementID, ValidFrom, ValidTo) VALUES

(12, 5, 6, CONVERT(DATETIME, '2014-10-23 20:30:30.617', 121), CONVERT(DATETIME, '2017-10-23 20:30:30.617', 121))

 
 
INSERT INTO CRM.CertificateOfOrigins_cf_CertificateOfOriginTypeByTradeAgreement (ID, CertificateOfOriginTypeCodeID, TradeAgreementID, ValidFrom, ValidTo) VALUES

(13, 5, 7, CONVERT(DATETIME, '2014-10-23 20:30:30.617', 121), CONVERT(DATETIME, '2017-10-23 20:30:30.617', 121))

 
 
INSERT INTO CRM.CertificateOfOrigins_cf_CertificateOfOriginTypeByTradeAgreement (ID, CertificateOfOriginTypeCodeID, TradeAgreementID, ValidFrom, ValidTo) VALUES

(14, 5, 8, CONVERT(DATETIME, '2014-10-23 20:30:30.617', 121), CONVERT(DATETIME, '2017-10-23 20:30:30.617', 121))

 
 
INSERT INTO CRM.CertificateOfOrigins_cf_CertificateOfOriginTypeByTradeAgreement (ID, CertificateOfOriginTypeCodeID, TradeAgreementID, ValidFrom, ValidTo) VALUES

(15, 5, 9, CONVERT(DATETIME, '2014-10-23 20:30:30.617', 121), CONVERT(DATETIME, '2017-10-23 20:30:30.617', 121))

 
 
INSERT INTO CRM.CertificateOfOrigins_cf_CertificateOfOriginTypeByTradeAgreement (ID, CertificateOfOriginTypeCodeID, TradeAgreementID, ValidFrom, ValidTo) VALUES

(16, 5, 10, CONVERT(DATETIME, '2014-10-23 20:30:30.617', 121), CONVERT(DATETIME, '2017-10-23 20:30:30.617', 121))

 
 
INSERT INTO CRM.CertificateOfOrigins_cf_CertificateOfOriginTypeByTradeAgreement (ID, CertificateOfOriginTypeCodeID, TradeAgreementID, ValidFrom, ValidTo) VALUES

(17, 5, 11, CONVERT(DATETIME, '2014-10-23 20:30:30.623', 121), CONVERT(DATETIME, '2017-10-23 20:30:30.623', 121))

 
 
INSERT INTO CRM.CertificateOfOrigins_cf_CertificateOfOriginTypeByTradeAgreement (ID, CertificateOfOriginTypeCodeID, TradeAgreementID, ValidFrom, ValidTo) VALUES

(18, 5, 12, CONVERT(DATETIME, '2014-10-23 20:30:30.633', 121), CONVERT(DATETIME, '2017-10-23 20:30:30.633', 121))

 
 
INSERT INTO CRM.CertificateOfOrigins_cf_CertificateOfOriginTypeByTradeAgreement (ID, CertificateOfOriginTypeCodeID, TradeAgreementID, ValidFrom, ValidTo) VALUES

(19, 5, 13, CONVERT(DATETIME, '2014-10-23 20:30:30.633', 121), CONVERT(DATETIME, '2017-10-23 20:30:30.633', 121))

 
 
INSERT INTO CRM.CertificateOfOrigins_cf_CertificateOfOriginTypeByTradeAgreement (ID, CertificateOfOriginTypeCodeID, TradeAgreementID, ValidFrom, ValidTo) VALUES

(20, 5, 14, CONVERT(DATETIME, '2014-10-23 20:30:30.640', 121), CONVERT(DATETIME, '2017-10-23 20:30:30.640', 121))

 
 
INSERT INTO CRM.CertificateOfOrigins_cf_CertificateOfOriginTypeByTradeAgreement (ID, CertificateOfOriginTypeCodeID, TradeAgreementID, ValidFrom, ValidTo) VALUES

(21, 5, 15, CONVERT(DATETIME, '2014-10-23 20:30:30.643', 121), CONVERT(DATETIME, '2017-10-23 20:30:30.643', 121))

 
 
INSERT INTO CRM.CertificateOfOrigins_cf_CertificateOfOriginTypeByTradeAgreement (ID, CertificateOfOriginTypeCodeID, TradeAgreementID, ValidFrom, ValidTo) VALUES

(22, 5, 16, CONVERT(DATETIME, '2014-10-23 20:30:30.647', 121), CONVERT(DATETIME, '2017-10-23 20:30:30.647', 121))

 
 
INSERT INTO CRM.CertificateOfOrigins_cf_CertificateOfOriginTypeByTradeAgreement (ID, CertificateOfOriginTypeCodeID, TradeAgreementID, ValidFrom, ValidTo) VALUES

(23, 5, 17, CONVERT(DATETIME, '2014-10-23 20:30:30.650', 121), CONVERT(DATETIME, '2017-10-23 20:30:30.650', 121))

 
 
INSERT INTO CRM.CertificateOfOrigins_cf_CertificateOfOriginTypeByTradeAgreement (ID, CertificateOfOriginTypeCodeID, TradeAgreementID, ValidFrom, ValidTo) VALUES

(24, 5, 18, CONVERT(DATETIME, '2014-10-23 20:30:30.650', 121), CONVERT(DATETIME, '2017-10-23 20:30:30.650', 121))

 
 
INSERT INTO CRM.CertificateOfOrigins_cf_CertificateOfOriginTypeByTradeAgreement (ID, CertificateOfOriginTypeCodeID, TradeAgreementID, ValidFrom, ValidTo) VALUES

(25, 5, 19, CONVERT(DATETIME, '2014-10-23 20:30:30.650', 121), CONVERT(DATETIME, '2017-10-23 20:30:30.650', 121))

 
 
INSERT INTO CRM.CertificateOfOrigins_cf_CertificateOfOriginTypeByTradeAgreement (ID, CertificateOfOriginTypeCodeID, TradeAgreementID, ValidFrom, ValidTo) VALUES

(26, 5, 20, CONVERT(DATETIME, '2014-10-23 20:30:30.653', 121), CONVERT(DATETIME, '2017-10-23 20:30:30.653', 121))

 
 
INSERT INTO CRM.CertificateOfOrigins_cf_CertificateOfOriginTypeByTradeAgreement (ID, CertificateOfOriginTypeCodeID, TradeAgreementID, ValidFrom, ValidTo) VALUES

(27, 5, 21, CONVERT(DATETIME, '2014-10-23 20:30:30.660', 121), CONVERT(DATETIME, '2017-10-23 20:30:30.660', 121))

 
 
INSERT INTO CRM.CertificateOfOrigins_cf_CertificateOfOriginTypeByTradeAgreement (ID, CertificateOfOriginTypeCodeID, TradeAgreementID, ValidFrom, ValidTo) VALUES

(28, 5, 22, CONVERT(DATETIME, '2014-10-23 20:30:30.660', 121), CONVERT(DATETIME, '2017-10-23 20:30:30.660', 121))

 
 
INSERT INTO CRM.CertificateOfOrigins_cf_CertificateOfOriginTypeByTradeAgreement (ID, CertificateOfOriginTypeCodeID, TradeAgreementID, ValidFrom, ValidTo) VALUES

(30, 6, 105, CONVERT(DATETIME, '2014-10-23 20:30:30.633', 121), CONVERT(DATETIME, '2017-10-23 20:30:30.507', 121))

 
 
INSERT INTO CRM.CertificateOfOrigins_cf_CertificateOfOriginTypeByTradeAgreement (ID, CertificateOfOriginTypeCodeID, TradeAgreementID, ValidFrom, ValidTo) VALUES

(32, 4, 108, CONVERT(DATETIME, '2014-10-23 20:30:30.633', 121), CONVERT(DATETIME, '2017-10-23 20:30:30.507', 121))

 
 
INSERT INTO CRM.CertificateOfOrigins_cf_CertificateOfOriginTypeByTradeAgreement (ID, CertificateOfOriginTypeCodeID, TradeAgreementID, ValidFrom, ValidTo) VALUES

(33, 2, 113, CONVERT(DATETIME, '2014-10-23 20:30:30.507', 121), CONVERT(DATETIME, '2017-10-23 20:30:30.507', 121))

 
 
INSERT INTO CRM.CertificateOfOrigins_cf_CertificateOfOriginTypeByTradeAgreement (ID, CertificateOfOriginTypeCodeID, TradeAgreementID, ValidFrom, ValidTo) VALUES

(34, 2, 112, CONVERT(DATETIME, '2014-10-23 20:30:30.507', 121), CONVERT(DATETIME, '2017-10-23 20:30:30.507', 121))

 
 
INSERT INTO CRM.CertificateOfOrigins_cf_CertificateOfOriginTypeByTradeAgreement (ID, CertificateOfOriginTypeCodeID, TradeAgreementID, ValidFrom, ValidTo) VALUES

(35, 2, 123, CONVERT(DATETIME, '2014-10-23 20:30:30.507', 121), CONVERT(DATETIME, '2017-10-23 20:30:30.507', 121))

 
 
INSERT INTO CRM.CertificateOfOrigins_cf_CertificateOfOriginTypeByTradeAgreement (ID, CertificateOfOriginTypeCodeID, TradeAgreementID, ValidFrom, ValidTo) VALUES

(36, 2, 125, CONVERT(DATETIME, '2014-10-23 20:30:30.507', 121), CONVERT(DATETIME, '2017-10-23 20:30:30.507', 121))

 
 
INSERT INTO CRM.CertificateOfOrigins_cf_CertificateOfOriginTypeByTradeAgreement (ID, CertificateOfOriginTypeCodeID, TradeAgreementID, ValidFrom, ValidTo) VALUES

(37, 2, 114, CONVERT(DATETIME, '2014-10-23 20:30:30.507', 121), CONVERT(DATETIME, '2017-10-23 20:30:30.507', 121))

 
 
INSERT INTO CRM.CertificateOfOrigins_cf_CertificateOfOriginTypeByTradeAgreement (ID, CertificateOfOriginTypeCodeID, TradeAgreementID, ValidFrom, ValidTo) VALUES

(39, 1, 112, CONVERT(DATETIME, '2014-10-23 20:30:30.507', 121), CONVERT(DATETIME, '2017-10-23 20:30:30.507', 121))

 
 
INSERT INTO CRM.CertificateOfOrigins_cf_CertificateOfOriginTypeByTradeAgreement (ID, CertificateOfOriginTypeCodeID, TradeAgreementID, ValidFrom, ValidTo) VALUES

(40, 1, 125, CONVERT(DATETIME, '2014-10-23 20:30:30.507', 121), CONVERT(DATETIME, '2017-10-23 20:30:30.507', 121))

 
 
INSERT INTO CRM.CertificateOfOrigins_cf_CertificateOfOriginTypeByTradeAgreement (ID, CertificateOfOriginTypeCodeID, TradeAgreementID, ValidFrom, ValidTo) VALUES

(41, 1, 114, CONVERT(DATETIME, '2014-10-23 20:30:30.507', 121), CONVERT(DATETIME, '2017-10-23 20:30:30.507', 121))

 
 
INSERT INTO CRM.CertificateOfOrigins_cf_CertificateOfOriginTypeByTradeAgreement (ID, CertificateOfOriginTypeCodeID, TradeAgreementID, ValidFrom, ValidTo) VALUES

(42, 1, 115, CONVERT(DATETIME, '2014-10-23 20:30:30.507', 121), CONVERT(DATETIME, '2017-10-23 20:30:30.507', 121))

 
 
INSERT INTO CRM.CertificateOfOrigins_cf_CertificateOfOriginTypeByTradeAgreement (ID, CertificateOfOriginTypeCodeID, TradeAgreementID, ValidFrom, ValidTo) VALUES

(43, 3, 118, CONVERT(DATETIME, '2014-10-23 20:30:30.507', 121), CONVERT(DATETIME, '2017-10-23 20:30:30.507', 121))

 
 
INSERT INTO CRM.CertificateOfOrigins_cf_CertificateOfOriginTypeByTradeAgreement (ID, CertificateOfOriginTypeCodeID, TradeAgreementID, ValidFrom, ValidTo) VALUES

(44, 4, 122, CONVERT(DATETIME, '2014-10-23 20:30:30.507', 121), CONVERT(DATETIME, '2017-10-23 20:30:30.507', 121))

 
 
INSERT INTO CRM.CertificateOfOrigins_cf_CertificateOfOriginTypeByTradeAgreement (ID, CertificateOfOriginTypeCodeID, TradeAgreementID, ValidFrom, ValidTo) VALUES

(45, 6, 121, CONVERT(DATETIME, '2014-10-23 20:30:30.507', 121), CONVERT(DATETIME, '2017-10-23 20:30:30.507', 121))

 
 
INSERT INTO CRM.CertificateOfOrigins_cf_CertificateOfOriginTypeByTradeAgreement (ID, CertificateOfOriginTypeCodeID, TradeAgreementID, ValidFrom, ValidTo) VALUES

(46, 7, 128, CONVERT(DATETIME, '2014-10-23 20:30:30.633', 121), CONVERT(DATETIME, '2017-10-23 20:30:30.507', 121))

 
 
INSERT INTO CRM.CertificateOfOrigins_cf_CertificateOfOriginTypeByTradeAgreement (ID, CertificateOfOriginTypeCodeID, TradeAgreementID, ValidFrom, ValidTo) VALUES

(47, 8, 129, CONVERT(DATETIME, '2014-10-23 20:30:30.633', 121), CONVERT(DATETIME, '2017-10-23 20:30:30.507', 121))

 
 
INSERT INTO CRM.CertificateOfOrigins_cf_CertificateOfOriginTypeByTradeAgreement (ID, CertificateOfOriginTypeCodeID, TradeAgreementID, ValidFrom, ValidTo) VALUES

(48, 9, 131, CONVERT(DATETIME, '2014-10-23 20:30:30.633', 121), CONVERT(DATETIME, '2017-10-23 20:30:30.507', 121))

 
 
INSERT INTO CRM.CertificateOfOrigins_cf_CertificateOfOriginTypeByTradeAgreement (ID, CertificateOfOriginTypeCodeID, TradeAgreementID, ValidFrom, ValidTo) VALUES

(49, 9, 134, CONVERT(DATETIME, '2014-10-23 20:30:30.633', 121), CONVERT(DATETIME, '2017-10-23 20:30:30.507', 121))

 
 
INSERT INTO CRM.CertificateOfOrigins_cf_CertificateOfOriginTypeByTradeAgreement (ID, CertificateOfOriginTypeCodeID, TradeAgreementID, ValidFrom, ValidTo) VALUES

(50, 10, 132, CONVERT(DATETIME, '2014-10-23 20:30:30.633', 121), CONVERT(DATETIME, '2017-10-23 20:30:30.507', 121))

 
 
INSERT INTO CRM.CertificateOfOrigins_cf_CertificateOfOriginTypeByTradeAgreement (ID, CertificateOfOriginTypeCodeID, TradeAgreementID, ValidFrom, ValidTo) VALUES

(51, 10, 133, CONVERT(DATETIME, '2014-10-23 20:30:30.633', 121), CONVERT(DATETIME, '2017-10-23 20:30:30.507', 121))

 
 
INSERT INTO CRM.CertificateOfOrigins_cf_CertificateOfOriginTypeByTradeAgreement (ID, CertificateOfOriginTypeCodeID, TradeAgreementID, ValidFrom, ValidTo) VALUES

(52, 11, 17, CONVERT(DATETIME, '2014-10-23 20:30:30.600', 121), CONVERT(DATETIME, '2100-10-23 20:30:30.600', 121))

 
 
INSERT INTO CRM.CertificateOfOrigins_cf_CertificateOfOriginTypeByTradeAgreement (ID, CertificateOfOriginTypeCodeID, TradeAgreementID, ValidFrom, ValidTo) VALUES

(53, 11, 112, CONVERT(DATETIME, '2014-10-23 20:30:30.507', 121), CONVERT(DATETIME, '2100-10-23 20:30:30.507', 121))
 SET IDENTITY_INSERT CRM.CertificateOfOrigins_cf_CertificateOfOriginTypeByTradeAgreement OFF

 END

 IF NOT EXISTS (
    SELECT 1
    FROM [CRM].[CertificateOfOrigins_cf_SupplierDeliveryCountryConfig]
)
BEGIN
   SET IDENTITY_INSERT CRM.CertificateOfOrigins_cf_SupplierDeliveryCountryConfig ON

INSERT INTO CRM.CertificateOfOrigins_cf_SupplierDeliveryCountryConfig (ID, ConutryID, State) VALUES
(1, 840, 1)
 
 
INSERT INTO CRM.CertificateOfOrigins_cf_SupplierDeliveryCountryConfig (ID, ConutryID, State) VALUES
(2, 124, 1)
 
 
INSERT INTO CRM.CertificateOfOrigins_cf_SupplierDeliveryCountryConfig (ID, ConutryID, State) VALUES
(3, 484, 1)
   SET IDENTITY_INSERT CRM.CertificateOfOrigins_cf_SupplierDeliveryCountryConfig OFF

 END

  IF NOT EXISTS (
    SELECT 1
    FROM [CRM].[CertificateOfOrigins_cl_DetailsPerCertificate]
)
BEGIN
   SET IDENTITY_INSERT CRM.CertificateOfOrigins_cl_DetailsPerCertificate ON

INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(2, 1, 1, 1, 1)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(3, 1, 1, 2, 2)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(4, 1, 1, 3, 3)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(5, 1, 1, 4, 4)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(6, 1, 1, 5, 5)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(7, 1, 3, 6, 6)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(8, 1, 3, 7, 7)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(9, 1, 2, 8, 8)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(10, 1, 2, 9, 9)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(11, 1, 3, 10, 10)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(12, 1, 2, 11, 11)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(13, 1, 2, 12, 12)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(14, 1, 3, 13, 13)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(15, 1, 3, 14, 14)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(17, 1, 3, 16, 16)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(18, 1, 2, 17, 17)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(19, 1, 2, 18, 18)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(20, 1, 1, 19, 19)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(21, 1, 3, 20, 20)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(22, 1, 3, 21, 21)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(23, 1, 3, 22, 22)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(24, 1, 3, 23, 23)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(26, 1, 2, 26, 25)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(27, 1, 1, 27, 26)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(28, 1, 1, 28, 27)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(29, 1, 1, 29, 28)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(30, 1, 1, 30, 29)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(31, 2, 1, 1, 1)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(32, 2, 1, 2, 2)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(33, 2, 1, 3, 3)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(34, 2, 1, 4, 4)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(35, 2, 1, 5, 5)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(36, 2, 3, 6, 6)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(37, 2, 3, 7, 7)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(38, 2, 2, 8, 8)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(39, 2, 2, 9, 9)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(40, 2, 3, 10, 10)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(41, 2, 2, 11, 11)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(42, 2, 2, 12, 12)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(43, 2, 3, 13, 13)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(44, 2, 3, 14, 14)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(46, 2, 3, 16, 16)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(47, 2, 2, 17, 17)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(49, 2, 3, 22, 19)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(50, 2, 3, 23, 20)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(51, 2, 2, 24, 21)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(52, 2, 2, 26, 22)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(53, 2, 1, 27, 23)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(54, 2, 1, 28, 24)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(55, 2, 1, 29, 25)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(56, 2, 1, 30, 26)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(57, 3, 1, 1, 1)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(58, 3, 1, 2, 2)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(59, 3, 1, 3, 3)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(60, 3, 1, 4, 4)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(61, 3, 1, 8, 5)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(62, 3, 1, 9, 6)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(63, 3, 1, 10, 7)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(64, 3, 2, 12, 9)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(65, 3, 1, 13, 10)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(67, 3, 2, 17, 13)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(69, 3, 2, 22, 15)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(70, 3, 2, 23, 16)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(71, 3, 1, 33, 17)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(72, 3, 1, 31, 18)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(73, 3, 1, 32, 19)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(74, 4, 1, 1, 1)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(75, 4, 1, 2, 2)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(76, 4, 1, 3, 3)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(77, 4, 1, 4, 4)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(78, 4, 1, 8, 5)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(79, 4, 1, 9, 6)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(80, 4, 1, 10, 7)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(81, 4, 2, 12, 8)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(82, 4, 1, 13, 9)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(84, 4, 2, 22, 11)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(85, 4, 2, 23, 12)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(86, 4, 1, 33, 13)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(87, 4, 1, 31, 14)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(88, 4, 1, 32, 15)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(89, 5, 3, 48, 1)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(90, 5, 1, 34, 2)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(91, 5, 1, 35, 3)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(92, 5, 1, 36, 4)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(93, 5, 1, 37, 5)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(94, 5, 1, 38, 6)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(95, 5, 1, 39, 7)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(96, 5, 1, 40, 8)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(97, 5, 1, 41, 9)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(98, 5, 1, 42, 10)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(99, 5, 1, 43, 11)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(100, 5, 1, 44, 12)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(101, 5, 1, 45, 13)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(102, 5, 1, 46, 14)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(103, 5, 1, 47, 15)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(104, 1, 2, 24, 24)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(105, 1, 1, 31, 30)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(106, 2, 2, 18, 27)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(107, 2, 1, 31, 28)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(108, 3, 2, 18, 20)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(109, 4, 2, 24, 16)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(110, 1, 3, 15, 15)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(111, 2, 3, 15, 15)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(112, 3, 1, 15, 12)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(113, 4, 1, 15, 10)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(114, 3, 2, 24, 14)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(115, 5, 2, 27, 16)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(116, 3, 2, 27, 8)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(117, 4, 2, 27, 17)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(118, 6, 1, 1, 1)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(119, 6, 1, 2, 2)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(120, 6, 1, 3, 3)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(121, 6, 1, 4, 4)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(122, 6, 1, 8, 5)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(123, 6, 1, 9, 6)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(124, 6, 1, 10, 7)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(125, 6, 2, 12, 8)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(126, 6, 2, 22, 11)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(127, 6, 2, 23, 12)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(128, 6, 1, 33, 13)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(129, 6, 1, 31, 14)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(130, 6, 1, 32, 15)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(131, 6, 2, 24, 16)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(132, 6, 1, 15, 10)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(133, 6, 2, 27, 17)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(134, 6, 1, 13, 4)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(135, 7, 1, 1, 1)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(136, 7, 1, 2, 2)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(137, 7, 1, 3, 3)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(138, 7, 1, 4, 4)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(139, 7, 1, 8, 5)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(140, 7, 1, 9, 6)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(141, 7, 1, 10, 7)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(142, 7, 2, 12, 9)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(143, 7, 1, 13, 10)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(144, 7, 2, 17, 13)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(145, 7, 1, 22, 15)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(146, 7, 2, 23, 16)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(147, 7, 1, 33, 17)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(148, 7, 1, 31, 18)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(149, 7, 1, 32, 19)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(150, 7, 2, 18, 20)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(151, 7, 1, 15, 12)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(152, 7, 2, 24, 14)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(153, 7, 2, 27, 8)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(154, 8, 1, 1, 1)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(155, 8, 1, 2, 2)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(156, 8, 1, 3, 3)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(157, 8, 1, 4, 4)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(158, 8, 1, 8, 5)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(159, 8, 1, 9, 6)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(160, 8, 1, 10, 8)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(161, 8, 2, 12, 9)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(162, 8, 1, 13, 10)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(163, 8, 2, 17, 13)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(164, 8, 2, 22, 15)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(165, 8, 2, 23, 16)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(166, 8, 1, 33, 18)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(167, 8, 1, 31, 18)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(168, 8, 1, 32, 19)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(169, 8, 2, 18, 20)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(170, 8, 1, 15, 12)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(171, 8, 2, 24, 14)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(172, 8, 2, 27, 8)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(173, 9, 1, 1, 1)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(174, 9, 1, 2, 2)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(175, 9, 1, 3, 3)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(176, 9, 1, 4, 4)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(177, 9, 1, 8, 5)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(178, 9, 1, 9, 6)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(179, 9, 1, 10, 8)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(180, 9, 2, 12, 9)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(181, 9, 1, 13, 10)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(182, 9, 2, 17, 13)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(183, 9, 2, 22, 15)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(184, 9, 2, 23, 16)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(185, 9, 1, 33, 18)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(186, 9, 1, 31, 18)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(187, 9, 1, 32, 19)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(188, 9, 2, 18, 20)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(189, 9, 1, 15, 12)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(190, 9, 2, 24, 14)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(191, 9, 2, 27, 8)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(192, 10, 1, 1, 1)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(193, 10, 1, 2, 2)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(194, 10, 1, 3, 3)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(195, 10, 1, 4, 4)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(196, 10, 1, 8, 5)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(197, 10, 1, 9, 6)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(198, 10, 1, 10, 8)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(199, 10, 2, 12, 9)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(200, 10, 1, 13, 10)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(201, 10, 2, 17, 13)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(202, 10, 2, 22, 15)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(203, 10, 2, 23, 16)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(204, 10, 1, 33, 18)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(205, 10, 1, 31, 18)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(206, 10, 1, 32, 19)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(207, 10, 2, 18, 20)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(208, 10, 1, 15, 12)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(209, 10, 2, 24, 14)
 
 
INSERT INTO CRM.CertificateOfOrigins_cl_DetailsPerCertificate (ID, CertificateOfOriginTypeCodeID, ConstraintTypeEnumID, CertificateDetailsTypeCodeID, [Order]) VALUES
(210, 10, 2, 27, 8)
   SET IDENTITY_INSERT CRM.CertificateOfOrigins_cl_DetailsPerCertificate OFF


 END

-- REMOVED: 457 seed rows for CRM.CertificateOfOrigins_cl_ExportAuthenticationRequestManufacturingArea.
-- Not reference data: these are runtime CHILD ROWS of an ExportDocumentAuthenticationRequest, written and
-- deleted by the DAL (MergeExportDocumentAuthenticationRequestChildren). Left over from a database dump
-- without their parent requests, so on a fresh database they were orphans and failed FK validation.

 
-- REMOVED: 1 seed row for CRM.CertificateOfOrigins_cl_ExportDocumentAuthenticationRequestLeadDocument.
-- Same reason as the ManufacturingArea block above: a runtime child row whose parent request is
-- transactional and correctly has no seed. A fresh install starts with no export requests.

 IF NOT EXISTS (
    SELECT 1
    FROM [CRM].[CertificateOfOrigins_enum_AuthenticationFileStatus]
)
BEGIN
      -- IDENTITY_INSERT (ON) removed: CRM.CertificateOfOrigins_enum_AuthenticationFileStatus.ID has no IDENTITY property -> Msg 8106. Explicit IDs insert fine without it.

INSERT INTO CRM.CertificateOfOrigins_enum_AuthenticationFileStatus (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate, IsAutomatic) VALUES

(1, N'ממתין לשליחת מכתב', 1, N'ממתין לשליחת מכתב', N'WaitingForSendingLetter', N'WaitingForSendingLetter', NULL, NULL, CONVERT(bit, 'True'))

 
 
INSERT INTO CRM.CertificateOfOrigins_enum_AuthenticationFileStatus (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate, IsAutomatic) VALUES

(2, N'נשלחה פניית אימות', 1, N'נשלחה פניית אימות', N'AuthenticationRequestWasSend', N'AuthenticationRequestWasSend', NULL, NULL, CONVERT(bit, 'True'))

 
 
INSERT INTO CRM.CertificateOfOrigins_enum_AuthenticationFileStatus (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate, IsAutomatic) VALUES

(3, N'נשלחה תזכורת לבית המכס', 1, N'נשלחה תזכורת לבית המכס', N'AuthenticationRequestReminderWasSend', N'AuthenticationRequestReminderWasSend', NULL, NULL, CONVERT(bit, 'True'))

 
 
INSERT INTO CRM.CertificateOfOrigins_enum_AuthenticationFileStatus (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate, IsAutomatic) VALUES

(4, N'התקבל מענה חלקי בתיק', 0, N'התקבל מענה חלקי בתיק', N'ReceivedPartialAnswerInFile ', N'ReceivedPartialAnswerInFile', NULL, NULL, CONVERT(bit, 'False'))

 
 
INSERT INTO CRM.CertificateOfOrigins_enum_AuthenticationFileStatus (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate, IsAutomatic) VALUES

(5, N'התקבל מענה  בתיק', 1, N'התקבל מענה  בתיק', N'ReceivedAnswerInFile ', N'ReceivedAnswerInFile', NULL, NULL, CONVERT(bit, 'False'))

 
 
INSERT INTO CRM.CertificateOfOrigins_enum_AuthenticationFileStatus (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate, IsAutomatic) VALUES

(6, N'תיק תקין', 1, N'תשובת האימות תקינה', N'RightAuthenticationAnswer ', N'RightAuthenticationAnswer', NULL, NULL, CONVERT(bit, 'False'))

 
 
INSERT INTO CRM.CertificateOfOrigins_enum_AuthenticationFileStatus (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate, IsAutomatic) VALUES

(7, N'נדרשת הבהרה', 1, N'נדרשת הבהרה', N'ClarificationRequired ', N'ClarificationRequired', NULL, NULL, CONVERT(bit, 'False'))

 
 
INSERT INTO CRM.CertificateOfOrigins_enum_AuthenticationFileStatus (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate, IsAutomatic) VALUES

(8, N'תיק פסול', 1, N'תשובת האימות פסולה', N'WrongAuthenticationAnswer ', N'WrongAuthenticationAnswer', NULL, NULL, CONVERT(bit, 'False'))

 
 
INSERT INTO CRM.CertificateOfOrigins_enum_AuthenticationFileStatus (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate, IsAutomatic) VALUES

(9, N'תיק מבוטל', 1, N'תיק מבוטל', N'CancelledFile', N'CancelledFile', NULL, NULL, CONVERT(bit, 'True'))
      -- IDENTITY_INSERT (OFF) removed: CRM.CertificateOfOrigins_enum_AuthenticationFileStatus.ID has no IDENTITY property -> Msg 8106. Explicit IDs insert fine without it.

 END

IF NOT EXISTS (
    SELECT 1
    FROM [CRM].[CertificateOfOrigins_enum_CertificateDetailsTypeCode]
)
BEGIN
       SET IDENTITY_INSERT CRM.CertificateOfOrigins_enum_CertificateDetailsTypeCode ON

INSERT INTO CRM.CertificateOfOrigins_enum_CertificateDetailsTypeCode (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate, Comment, DetailTypeFormat, DataTypeID) VALUES

(1, N'מס'' היצואן', 1, N'מס'' היצואן', N'Exporter Id', N'ExporterId', CONVERT(DATETIME, '1900-01-01 00:00:00.000', 121), CONVERT(DATETIME, '9999-12-31 00:00:00.000', 121), NULL, N'Customs.Shared.Entities.CustomerLOV, Customs.Shared.Entities, Version=2.0.0.2, Culture=neutral, PublicKeyToken=null', 8)

 
 
INSERT INTO CRM.CertificateOfOrigins_enum_CertificateDetailsTypeCode (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate, Comment, DetailTypeFormat, DataTypeID) VALUES

(2, N'שם יצואן', 1, N'שם יצואן', N'Exporter Name', N'ExporterName', CONVERT(DATETIME, '1900-01-01 00:00:00.000', 121), CONVERT(DATETIME, '9999-12-31 00:00:00.000', 121), NULL, NULL, 3)

 
 
INSERT INTO CRM.CertificateOfOrigins_enum_CertificateDetailsTypeCode (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate, Comment, DetailTypeFormat, DataTypeID) VALUES

(3, N'כתובת יצואן', 1, N'כתובת יצואן', N'Exporter Address', N'ExporterAddress', CONVERT(DATETIME, '1900-01-01 00:00:00.000', 121), CONVERT(DATETIME, '9999-12-31 00:00:00.000', 121), NULL, NULL, 3)

 
 
INSERT INTO CRM.CertificateOfOrigins_enum_CertificateDetailsTypeCode (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate, Comment, DetailTypeFormat, DataTypeID) VALUES

(4, N'מדינת יצואן', 1, N'מדינת יצואן', N'Exporter Country', N'ExporterCountry', CONVERT(DATETIME, '1900-01-01 00:00:00.000', 121), CONVERT(DATETIME, '9999-12-31 00:00:00.000', 121), NULL, N'Customs.Shared.Entities.Country, Customs.Shared.Entities, Version=2.0.0.2, Culture=neutral, PublicKeyToken=null', 8)

 
 
INSERT INTO CRM.CertificateOfOrigins_enum_CertificateDetailsTypeCode (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate, Comment, DetailTypeFormat, DataTypeID) VALUES

(5, N'מדינה ראשונה בהסכם', 1, N'מדינה ראשונה בהסכם', N'Trade Agreement Country 1', N'TradeAgreementCountry1', CONVERT(DATETIME, '1900-01-01 00:00:00.000', 121), CONVERT(DATETIME, '9999-12-31 00:00:00.000', 121), NULL, N'Customs.Shared.Entities.Country, Customs.Shared.Entities, Version=2.0.0.2, Culture=neutral, PublicKeyToken=null', 8)

 
 
INSERT INTO CRM.CertificateOfOrigins_enum_CertificateDetailsTypeCode (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate, Comment, DetailTypeFormat, DataTypeID) VALUES

(6, N'מדינה שניה בהסכם', 1, N'מדינה שניה בהסכם', N'Trade Agreement Country 2', N'TradeAgreementCountry2', CONVERT(DATETIME, '1900-01-01 00:00:00.000', 121), CONVERT(DATETIME, '9999-12-31 00:00:00.000', 121), NULL, N'Customs.Shared.Entities.Country, Customs.Shared.Entities, Version=2.0.0.2, Culture=neutral, PublicKeyToken=null', 8)

 
 
INSERT INTO CRM.CertificateOfOrigins_enum_CertificateDetailsTypeCode (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate, Comment, DetailTypeFormat, DataTypeID) VALUES

(7, N'קבוצת מדינות בהסכם', 1, N'קבוצת מדינות בהסכם', N'Trade Agreement Group Of Countries', N'TradeAgreementGroupOfCountries', CONVERT(DATETIME, '1900-01-01 00:00:00.000', 121), CONVERT(DATETIME, '9999-12-31 00:00:00.000', 121), NULL, N'Customs.Shared.Entities.CountryGroup, Customs.Shared.Entities, Version=2.0.0.2, Culture=neutral, PublicKeyToken=null', 8)

 
 
INSERT INTO CRM.CertificateOfOrigins_enum_CertificateDetailsTypeCode (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate, Comment, DetailTypeFormat, DataTypeID) VALUES

(8, N'שם הנשגר/יבואן', 1, N'שם הנשגר/יבואן', N'Consignee Name', N'ConsigneeName', CONVERT(DATETIME, '1900-01-01 00:00:00.000', 121), CONVERT(DATETIME, '9999-12-31 00:00:00.000', 121), NULL, NULL, 3)

 
 
INSERT INTO CRM.CertificateOfOrigins_enum_CertificateDetailsTypeCode (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate, Comment, DetailTypeFormat, DataTypeID) VALUES

(9, N'כתובת הנשגר/יבואן', 1, N'כתובת הנשגר/יבואן', N'Consignee Address', N'ConsigneeAddress', CONVERT(DATETIME, '1900-01-01 00:00:00.000', 121), CONVERT(DATETIME, '9999-12-31 00:00:00.000', 121), NULL, NULL, 3)

 
 
INSERT INTO CRM.CertificateOfOrigins_enum_CertificateDetailsTypeCode (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate, Comment, DetailTypeFormat, DataTypeID) VALUES

(10, N'מדינת הנשגר/יבואן', 1, N'מדינת הנשגר/יבואן', N'Consignee Country', N'ConsigneeCountry', CONVERT(DATETIME, '1900-01-01 00:00:00.000', 121), CONVERT(DATETIME, '9999-12-31 00:00:00.000', 121), NULL, N'Customs.Shared.Entities.Country, Customs.Shared.Entities, Version=2.0.0.2, Culture=neutral, PublicKeyToken=null', 8)

 
 
INSERT INTO CRM.CertificateOfOrigins_enum_CertificateDetailsTypeCode (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate, Comment, DetailTypeFormat, DataTypeID) VALUES

(11, N'הערות לגבי נשגר/ יבואן', 1, N'הערות לגבי נשגר/ יבואן', N'Consignee Remarks', N'ConsigneeRemarks', CONVERT(DATETIME, '1900-01-01 00:00:00.000', 121), CONVERT(DATETIME, '9999-12-31 00:00:00.000', 121), NULL, NULL, 3)

 
 
INSERT INTO CRM.CertificateOfOrigins_enum_CertificateDetailsTypeCode (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate, Comment, DetailTypeFormat, DataTypeID) VALUES

(12, N'האם להדפיס נתוני נשגר/יבואן?', 1, N'האם להדפיס נתוני נשגר/יבואן?', N'Is Consignee For Print', N'IsConsigneeForPrint', CONVERT(DATETIME, '1900-01-01 00:00:00.000', 121), CONVERT(DATETIME, '9999-12-31 00:00:00.000', 121), NULL, NULL, 5)

 
 
INSERT INTO CRM.CertificateOfOrigins_enum_CertificateDetailsTypeCode (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate, Comment, DetailTypeFormat, DataTypeID) VALUES

(13, N'מדינת המקור', 1, N'מדינת המקור', N'Origin Country', N'OriginCountry', CONVERT(DATETIME, '1900-01-01 00:00:00.000', 121), CONVERT(DATETIME, '9999-12-31 00:00:00.000', 121), NULL, N'Customs.Shared.Entities.Country, Customs.Shared.Entities, Version=2.0.0.2, Culture=neutral, PublicKeyToken=null', 8)

 
 
INSERT INTO CRM.CertificateOfOrigins_enum_CertificateDetailsTypeCode (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate, Comment, DetailTypeFormat, DataTypeID) VALUES

(14, N'קבוצת מדינות המקור', 1, N'קבוצת מדינות המקור', N'Origin Group Of Countries', N'OriginGroupOfCountries', CONVERT(DATETIME, '1900-01-01 00:00:00.000', 121), CONVERT(DATETIME, '9999-12-31 00:00:00.000', 121), NULL, N'Customs.Shared.Entities.CountryGroup, Customs.Shared.Entities, Version=2.0.0.2, Culture=neutral, PublicKeyToken=null', 8)

 
 
INSERT INTO CRM.CertificateOfOrigins_enum_CertificateDetailsTypeCode (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate, Comment, DetailTypeFormat, DataTypeID) VALUES

(15, N'מדינת היעד', 1, N'מדינת היעד', N'Destination Country', N'DestinationCountry', CONVERT(DATETIME, '1900-01-01 00:00:00.000', 121), CONVERT(DATETIME, '9999-12-31 00:00:00.000', 121), NULL, N'Customs.Shared.Entities.Country, Customs.Shared.Entities, Version=2.0.0.2, Culture=neutral, PublicKeyToken=null', 8)

 
 
INSERT INTO CRM.CertificateOfOrigins_enum_CertificateDetailsTypeCode (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate, Comment, DetailTypeFormat, DataTypeID) VALUES

(16, N'קבוצת מדינות היעד', 1, N'קבוצת מדינות היעד', N'Destination Group Of Countries', N'DestinationGroupOfCountries', CONVERT(DATETIME, '1900-01-01 00:00:00.000', 121), CONVERT(DATETIME, '9999-12-31 00:00:00.000', 121), NULL, N'Customs.Shared.Entities.CountryGroup, Customs.Shared.Entities, Version=2.0.0.2, Culture=neutral, PublicKeyToken=null', 8)

 
 
INSERT INTO CRM.CertificateOfOrigins_enum_CertificateDetailsTypeCode (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate, Comment, DetailTypeFormat, DataTypeID) VALUES

(17, N'אמצעי הובלה', 1, N'אמצעי הובלה', N'Transport', N'Transport', CONVERT(DATETIME, '1900-01-01 00:00:00.000', 121), CONVERT(DATETIME, '9999-12-31 00:00:00.000', 121), NULL, NULL, 3)

 
 
INSERT INTO CRM.CertificateOfOrigins_enum_CertificateDetailsTypeCode (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate, Comment, DetailTypeFormat, DataTypeID) VALUES

(18, N'נמל מוצא', 1, N'נמל מוצא', N'Port Of Shipment', N'PortOfShipment', CONVERT(DATETIME, '1900-01-01 00:00:00.000', 121), CONVERT(DATETIME, '9999-12-31 00:00:00.000', 121), NULL, N'Customs.Shared.Entities.InternationalSite, Customs.Shared.Entities, Version=2.0.0.2, Culture=neutral, PublicKeyToken=null', 8)

 
 
INSERT INTO CRM.CertificateOfOrigins_enum_CertificateDetailsTypeCode (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate, Comment, DetailTypeFormat, DataTypeID) VALUES

(19, N'צבירה', 1, N'צבירה', N'Is Cumulation', N'IsCumulation', CONVERT(DATETIME, '1900-01-01 00:00:00.000', 121), CONVERT(DATETIME, '9999-12-31 00:00:00.000', 121), NULL, NULL, 5)

 
 
INSERT INTO CRM.CertificateOfOrigins_enum_CertificateDetailsTypeCode (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate, Comment, DetailTypeFormat, DataTypeID) VALUES

(20, N'מדינת צבירה', 1, N'מדינת צבירה', N'Cumulation Country', N'CumulationCountry', CONVERT(DATETIME, '1900-01-01 00:00:00.000', 121), CONVERT(DATETIME, '9999-12-31 00:00:00.000', 121), NULL, N'Customs.Shared.Entities.Country, Customs.Shared.Entities, Version=2.0.0.2, Culture=neutral, PublicKeyToken=null', 8)

 
 
INSERT INTO CRM.CertificateOfOrigins_enum_CertificateDetailsTypeCode (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate, Comment, DetailTypeFormat, DataTypeID) VALUES

(21, N'קבוצת מדינות צבירה', 1, N'קבוצת מדינות צבירה', N'Cumulation Group Of Countries', N'CumulationGroupOfCountries', CONVERT(DATETIME, '1900-01-01 00:00:00.000', 121), CONVERT(DATETIME, '9999-12-31 00:00:00.000', 121), NULL, N'Customs.Shared.Entities.CountryGroup, Customs.Shared.Entities, Version=2.0.0.2, Culture=neutral, PublicKeyToken=null', 8)

 
 
INSERT INTO CRM.CertificateOfOrigins_enum_CertificateDetailsTypeCode (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate, Comment, DetailTypeFormat, DataTypeID) VALUES

(22, N'מקום ייצור הטובין', 1, N'מקום ייצור הטובין', N'Place Of Manufacture', N'PlaceOfManufacture', CONVERT(DATETIME, '1900-01-01 00:00:00.000', 121), CONVERT(DATETIME, '9999-12-31 00:00:00.000', 121), NULL, N'Customs.Shared.Entities.City, Customs.Shared.Entities, Version=2.0.0.2, Culture=neutral, PublicKeyToken=null', 8)

 
 
INSERT INTO CRM.CertificateOfOrigins_enum_CertificateDetailsTypeCode (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate, Comment, DetailTypeFormat, DataTypeID) VALUES

(23, N'מיקוד של ייצור הטובין', 1, N'מיקוד של ייצור הטובין', N'Zip Code Of Manufacture', N'ZipCodeOfManufacture', CONVERT(DATETIME, '1900-01-01 00:00:00.000', 121), CONVERT(DATETIME, '9999-12-31 00:00:00.000', 121), NULL, NULL, 3)

 
 
INSERT INTO CRM.CertificateOfOrigins_enum_CertificateDetailsTypeCode (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate, Comment, DetailTypeFormat, DataTypeID) VALUES

(24, N'הערות', 1, N'הערות', N'Observations', N'Observations', CONVERT(DATETIME, '1900-01-01 00:00:00.000', 121), CONVERT(DATETIME, '9999-12-31 00:00:00.000', 121), NULL, NULL, 3)

 
 
INSERT INTO CRM.CertificateOfOrigins_enum_CertificateDetailsTypeCode (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate, Comment, DetailTypeFormat, DataTypeID) VALUES

(25, N'הערות מעריך', 1, N'הערות מעריך', N'Feedback', N'Feedback', CONVERT(DATETIME, '1900-01-01 00:00:00.000', 121), CONVERT(DATETIME, '9999-12-31 00:00:00.000', 121), NULL, NULL, 3)

 
 
INSERT INTO CRM.CertificateOfOrigins_enum_CertificateDetailsTypeCode (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate, Comment, DetailTypeFormat, DataTypeID) VALUES

(26, N'האם להדפיס את מספר הצהרת היצוא?', 1, N'האם להדפיס את מספר הצהרת היצוא?', N'Is Export Dec For Print', N'IsExportDecForPrint', CONVERT(DATETIME, '1900-01-01 00:00:00.000', 121), CONVERT(DATETIME, '9999-12-31 00:00:00.000', 121), NULL, NULL, 5)

 
 
INSERT INTO CRM.CertificateOfOrigins_enum_CertificateDetailsTypeCode (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate, Comment, DetailTypeFormat, DataTypeID) VALUES

(27, N'בית מכס', 1, N'בית מכס', N'Customs House', N'CustomsHouse', CONVERT(DATETIME, '1900-01-01 00:00:00.000', 121), CONVERT(DATETIME, '9999-12-31 00:00:00.000', 121), NULL, N'Customs.Shared.Entities.CustomsHouseType, Customs.Shared.Entities, Version=2.0.0.2, Culture=neutral, PublicKeyToken=null', 8)

 
 
INSERT INTO CRM.CertificateOfOrigins_enum_CertificateDetailsTypeCode (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate, Comment, DetailTypeFormat, DataTypeID) VALUES

(28, N'מדינה מנפיקה', 1, N'מדינה מנפיקה', N'Issuing Country', N'IssuingCountry', CONVERT(DATETIME, '1900-01-01 00:00:00.000', 121), CONVERT(DATETIME, '9999-12-31 00:00:00.000', 121), NULL, N'Customs.Shared.Entities.Country, Customs.Shared.Entities, Version=2.0.0.2, Culture=neutral, PublicKeyToken=null', 8)

 
 
INSERT INTO CRM.CertificateOfOrigins_enum_CertificateDetailsTypeCode (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate, Comment, DetailTypeFormat, DataTypeID) VALUES

(29, N'יישוב הצהרת יצואן', 1, N'יישוב הצהרת יצואן', N'City Of Declaration', N'CityOfDeclaration', CONVERT(DATETIME, '1900-01-01 00:00:00.000', 121), CONVERT(DATETIME, '9999-12-31 00:00:00.000', 121), NULL, N'Customs.Shared.Entities.City, Customs.Shared.Entities, Version=2.0.0.2, Culture=neutral, PublicKeyToken=null', 8)

 
 
INSERT INTO CRM.CertificateOfOrigins_enum_CertificateDetailsTypeCode (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate, Comment, DetailTypeFormat, DataTypeID) VALUES

(30, N'מדינת הצהרת יצואן', 1, N'מדינת הצהרת יצואן', N'Country Of Declaration', N'CountryOfDeclaration', CONVERT(DATETIME, '1900-01-01 00:00:00.000', 121), CONVERT(DATETIME, '9999-12-31 00:00:00.000', 121), NULL, N'Customs.Shared.Entities.Country, Customs.Shared.Entities, Version=2.0.0.2, Culture=neutral, PublicKeyToken=null', 8)

 
 
INSERT INTO CRM.CertificateOfOrigins_enum_CertificateDetailsTypeCode (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate, Comment, DetailTypeFormat, DataTypeID) VALUES

(31, N'תאריך הצהרת יצואן', 1, N'תאריך הצהרת יצואן', N'Date Of Declaration', N'DateOfDeclaration', CONVERT(DATETIME, '1900-01-01 00:00:00.000', 121), CONVERT(DATETIME, '9999-12-31 00:00:00.000', 121), NULL, NULL, 4)

 
 
INSERT INTO CRM.CertificateOfOrigins_enum_CertificateDetailsTypeCode (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate, Comment, DetailTypeFormat, DataTypeID) VALUES

(32, N'המצהיר הוא היצרן', 1, N'המצהיר הוא היצרן', N'Is Declared By Manufacturer', N'IsDeclaredByManufacturer', CONVERT(DATETIME, '1900-01-01 00:00:00.000', 121), CONVERT(DATETIME, '9999-12-31 00:00:00.000', 121), NULL, NULL, 5)

 
 
INSERT INTO CRM.CertificateOfOrigins_enum_CertificateDetailsTypeCode (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate, Comment, DetailTypeFormat, DataTypeID) VALUES

(33, N'המצהיר הוא היצואן', 1, N'המצהיר הוא היצואן', N'Is Declared By Exporter', N'IsDeclaredByExporter', CONVERT(DATETIME, '1900-01-01 00:00:00.000', 121), CONVERT(DATETIME, '9999-12-31 00:00:00.000', 121), NULL, NULL, 5)

 
 
INSERT INTO CRM.CertificateOfOrigins_enum_CertificateDetailsTypeCode (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate, Comment, DetailTypeFormat, DataTypeID) VALUES

(34, N'תאריך היצוא', 1, N'תאריך היצוא', N'Export Date', N'ExportDate', CONVERT(DATETIME, '1900-01-01 00:00:00.000', 121), CONVERT(DATETIME, '9999-12-31 00:00:00.000', 121), NULL, NULL, 4)

 
 
INSERT INTO CRM.CertificateOfOrigins_enum_CertificateDetailsTypeCode (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate, Comment, DetailTypeFormat, DataTypeID) VALUES

(35, N'מדינת היצוא', 1, N'מדינת היצוא', N'ExportCountry', N'ExportCountry', CONVERT(DATETIME, '1900-01-01 00:00:00.000', 121), CONVERT(DATETIME, '9999-12-31 00:00:00.000', 121), NULL, N'Customs.Shared.Entities.Country, Customs.Shared.Entities, Version=2.0.0.2, Culture=neutral, PublicKeyToken=null', 8)

 
 
INSERT INTO CRM.CertificateOfOrigins_enum_CertificateDetailsTypeCode (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate, Comment, DetailTypeFormat, DataTypeID) VALUES

(36, N'מספר שטר מטען יבוא לישראל', 1, N'מספר שטר מטען יבוא לישראל', N'Import Bill Of Lading Num', N'ImportBillOfLadingNum', CONVERT(DATETIME, '1900-01-01 00:00:00.000', 121), CONVERT(DATETIME, '9999-12-31 00:00:00.000', 121), NULL, NULL, 3)

 
 
INSERT INTO CRM.CertificateOfOrigins_enum_CertificateDetailsTypeCode (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate, Comment, DetailTypeFormat, DataTypeID) VALUES

(37, N'נמל המוצא/ יצוא', 1, N'נמל המוצא/ יצוא', N'Export Port', N'ExportPort', CONVERT(DATETIME, '1900-01-01 00:00:00.000', 121), CONVERT(DATETIME, '9999-12-31 00:00:00.000', 121), NULL, N'Customs.Shared.Entities.InternationalSite, Customs.Shared.Entities, Version=2.0.0.2, Culture=neutral, PublicKeyToken=null', 8)

 
 
INSERT INTO CRM.CertificateOfOrigins_enum_CertificateDetailsTypeCode (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate, Comment, DetailTypeFormat, DataTypeID) VALUES

(38, N'תאריך היבוא לישראל', 1, N'תאריך היבוא לישראל', N'Import Date', N'ImportDate', CONVERT(DATETIME, '1900-01-01 00:00:00.000', 121), CONVERT(DATETIME, '9999-12-31 00:00:00.000', 121), NULL, NULL, 4)

 
 
INSERT INTO CRM.CertificateOfOrigins_enum_CertificateDetailsTypeCode (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate, Comment, DetailTypeFormat, DataTypeID) VALUES

(39, N'מספר שטר מטען יצוא מישראל', 1, N'מספר שטר מטען יצוא מישראל', N'Export Bill OF Lading Num', N'ExportBillOFLadingNum', CONVERT(DATETIME, '1900-01-01 00:00:00.000', 121), CONVERT(DATETIME, '9999-12-31 00:00:00.000', 121), NULL, NULL, 3)

 
 
INSERT INTO CRM.CertificateOfOrigins_enum_CertificateDetailsTypeCode (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate, Comment, DetailTypeFormat, DataTypeID) VALUES

(40, N'מדינת הביניים', 1, N'מדינת הביניים', N'Transir Country', N'TransirCountry', CONVERT(DATETIME, '1900-01-01 00:00:00.000', 121), CONVERT(DATETIME, '9999-12-31 00:00:00.000', 121), NULL, N'Customs.Shared.Entities.Country, Customs.Shared.Entities, Version=2.0.0.2, Culture=neutral, PublicKeyToken=null', 8)

 
 
INSERT INTO CRM.CertificateOfOrigins_enum_CertificateDetailsTypeCode (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate, Comment, DetailTypeFormat, DataTypeID) VALUES

(41, N'נמל כניסה לישראל', 1, N'נמל כניסה לישראל', N'Port Of Entrance', N'PortOfEntrance', CONVERT(DATETIME, '1900-01-01 00:00:00.000', 121), CONVERT(DATETIME, '9999-12-31 00:00:00.000', 121), NULL, N'Customs.Shared.Entities.InternationalSite, Customs.Shared.Entities, Version=2.0.0.2, Culture=neutral, PublicKeyToken=null', 8)

 
 
INSERT INTO CRM.CertificateOfOrigins_enum_CertificateDetailsTypeCode (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate, Comment, DetailTypeFormat, DataTypeID) VALUES

(42, N'תאריך יציאה משוער מישראל', 1, N'תאריך יציאה משוער מישראל', N'Expected Exit Date', N'ExpectedExitDate', CONVERT(DATETIME, '1900-01-01 00:00:00.000', 121), CONVERT(DATETIME, '9999-12-31 00:00:00.000', 121), NULL, NULL, 4)

 
 
INSERT INTO CRM.CertificateOfOrigins_enum_CertificateDetailsTypeCode (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate, Comment, DetailTypeFormat, DataTypeID) VALUES

(43, N'נמל יציאה מישראל', 1, N'נמל יציאה מישראל', N'Exit Port', N'ExitPort', CONVERT(DATETIME, '1900-01-01 00:00:00.000', 121), CONVERT(DATETIME, '9999-12-31 00:00:00.000', 121), NULL, N'Customs.Shared.Entities.InternationalSite, Customs.Shared.Entities, Version=2.0.0.2, Culture=neutral, PublicKeyToken=null', 8)

 
 
INSERT INTO CRM.CertificateOfOrigins_enum_CertificateDetailsTypeCode (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate, Comment, DetailTypeFormat, DataTypeID) VALUES

(44, N'תיאור הטובין בתעודת מעבר', 1, N'תיאור הטובין בתעודת מעבר', N' ods Description', N' odsDescription', CONVERT(DATETIME, '1900-01-01 00:00:00.000', 121), CONVERT(DATETIME, '9999-12-31 00:00:00.000', 121), NULL, NULL, 3)

 
 
INSERT INTO CRM.CertificateOfOrigins_enum_CertificateDetailsTypeCode (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate, Comment, DetailTypeFormat, DataTypeID) VALUES

(45, N'שם החברה המצהירה', 1, N'שם החברה המצהירה', N'Declaring Company', N'DeclaringCompany', CONVERT(DATETIME, '1900-01-01 00:00:00.000', 121), CONVERT(DATETIME, '9999-12-31 00:00:00.000', 121), NULL, NULL, 3)

 
 
INSERT INTO CRM.CertificateOfOrigins_enum_CertificateDetailsTypeCode (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate, Comment, DetailTypeFormat, DataTypeID) VALUES

(46, N'שם המצהיר (אדם ספציפי)', 1, N'שם המצהיר (אדם ספציפי)', N'Declaring Person', N'DeclaringPerson', CONVERT(DATETIME, '1900-01-01 00:00:00.000', 121), CONVERT(DATETIME, '9999-12-31 00:00:00.000', 121), NULL, NULL, 3)

 
 
INSERT INTO CRM.CertificateOfOrigins_enum_CertificateDetailsTypeCode (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate, Comment, DetailTypeFormat, DataTypeID) VALUES

(47, N'תפקיד המצהיר', 1, N'תפקיד המצהיר', N'DeclaringPosition', N'DeclaringPosition', CONVERT(DATETIME, '1900-01-01 00:00:00.000', 121), CONVERT(DATETIME, '9999-12-31 00:00:00.000', 121), NULL, NULL, 3)

 
 
INSERT INTO CRM.CertificateOfOrigins_enum_CertificateDetailsTypeCode (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate, Comment, DetailTypeFormat, DataTypeID) VALUES

(48, N'מספר מצהר', 1, N'מספר מצהר', N'Manifest Num', N'ManifestNum', CONVERT(DATETIME, '1900-01-01 00:00:00.000', 121), CONVERT(DATETIME, '9999-12-31 00:00:00.000', 121), NULL, NULL, 1)
       SET IDENTITY_INSERT CRM.CertificateOfOrigins_enum_CertificateDetailsTypeCode OFF

 END

 
IF NOT EXISTS (
    SELECT 1
    FROM [CRM].[CertificateOfOrigins_enum_CertificateOfOriginStatusCode]
)
BEGIN
        SET IDENTITY_INSERT CRM.CertificateOfOrigins_enum_CertificateOfOriginStatusCode ON

 
INSERT INTO CRM.CertificateOfOrigins_enum_CertificateOfOriginStatusCode (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate, RecordEditable, UserPermitted, IsRecordEditableNotExport, IsUserPermittedNotExport) VALUES

(1, N'שגויה', 1, N'שגויה', N'Error', N'Error', CONVERT(DATETIME, '1900-01-01 00:00:00.000', 121), CONVERT(DATETIME, '9999-12-31 00:00:00.000', 121), CONVERT(bit, 'False'), CONVERT(bit, 'False'), CONVERT(bit, 'False'), CONVERT(bit, 'False'))

 
 
INSERT INTO CRM.CertificateOfOrigins_enum_CertificateOfOriginStatusCode (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate, RecordEditable, UserPermitted, IsRecordEditableNotExport, IsUserPermittedNotExport) VALUES

(2, N'נקלטה', 1, N'נקלטה', N'Received', N'Received', CONVERT(DATETIME, '1900-01-01 00:00:00.000', 121), CONVERT(DATETIME, '9999-12-31 00:00:00.000', 121), CONVERT(bit, 'False'), CONVERT(bit, 'False'), CONVERT(bit, 'True'), CONVERT(bit, 'False'))

 
 
INSERT INTO CRM.CertificateOfOrigins_enum_CertificateOfOriginStatusCode (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate, RecordEditable, UserPermitted, IsRecordEditableNotExport, IsUserPermittedNotExport) VALUES

(3, N'נדחתה', 1, N'נדחתה', N'Rejected', N'Rejected', CONVERT(DATETIME, '1900-01-01 00:00:00.000', 121), CONVERT(DATETIME, '9999-12-31 00:00:00.000', 121), CONVERT(bit, 'False'), CONVERT(bit, 'True'), CONVERT(bit, 'False'), CONVERT(bit, 'True'))

 
 
INSERT INTO CRM.CertificateOfOrigins_enum_CertificateOfOriginStatusCode (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate, RecordEditable, UserPermitted, IsRecordEditableNotExport, IsUserPermittedNotExport) VALUES

(4, N'מבוטלת', 1, N'מבוטלת', N'Cancelled', N'Cancelled', CONVERT(DATETIME, '1900-01-01 00:00:00.000', 121), CONVERT(DATETIME, '9999-12-31 00:00:00.000', 121), CONVERT(bit, 'False'), CONVERT(bit, 'False'), CONVERT(bit, 'False'), CONVERT(bit, 'True'))

 
 
INSERT INTO CRM.CertificateOfOrigins_enum_CertificateOfOriginStatusCode (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate, RecordEditable, UserPermitted, IsRecordEditableNotExport, IsUserPermittedNotExport) VALUES

(5, N'אינה תואמת להצהרת יצוא', 1, N'אינה תואמת להצהרת יצוא', N'Declaration Mismatch', N'DeclarationMismatch', CONVERT(DATETIME, '1900-01-01 00:00:00.000', 121), CONVERT(DATETIME, '9999-12-31 00:00:00.000', 121), CONVERT(bit, 'True'), CONVERT(bit, 'False'), CONVERT(bit, 'True'), CONVERT(bit, 'True'))

 
 
INSERT INTO CRM.CertificateOfOrigins_enum_CertificateOfOriginStatusCode (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate, RecordEditable, UserPermitted, IsRecordEditableNotExport, IsUserPermittedNotExport) VALUES

(6, N'תואמת להצהרת יצוא', 1, N'תואמת להצהרת יצוא', N'Declaration Match', N'DeclarationMatch', CONVERT(DATETIME, '1900-01-01 00:00:00.000', 121), CONVERT(DATETIME, '9999-12-31 00:00:00.000', 121), CONVERT(bit, 'True'), CONVERT(bit, 'False'), CONVERT(bit, 'True'), CONVERT(bit, 'True'))

 
 
INSERT INTO CRM.CertificateOfOrigins_enum_CertificateOfOriginStatusCode (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate, RecordEditable, UserPermitted, IsRecordEditableNotExport, IsUserPermittedNotExport) VALUES

(7, N'ממתינה להתרת הצהרת יצוא', 1, N'ממתינה להתרת הצהרת יצוא', N'Pending Release', N'PendingRelease', CONVERT(DATETIME, '1900-01-01 00:00:00.000', 121), CONVERT(DATETIME, '9999-12-31 00:00:00.000', 121), CONVERT(bit, 'False'), CONVERT(bit, 'True'), CONVERT(bit, 'True'), CONVERT(bit, 'True'))

 
 
INSERT INTO CRM.CertificateOfOrigins_enum_CertificateOfOriginStatusCode (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate, RecordEditable, UserPermitted, IsRecordEditableNotExport, IsUserPermittedNotExport) VALUES

(8, N'מאושרת לפרסום באינטרנט', 1, N'מאושרת לפרסום באינטרנט', N'Published', N'Published', CONVERT(DATETIME, '1900-01-01 00:00:00.000', 121), CONVERT(DATETIME, '9999-12-31 00:00:00.000', 121), CONVERT(bit, 'False'), CONVERT(bit, 'False'), CONVERT(bit, 'False'), CONVERT(bit, 'True'))
        SET IDENTITY_INSERT CRM.CertificateOfOrigins_enum_CertificateOfOriginStatusCode OFF

END
IF NOT EXISTS (
    SELECT 1
    FROM [CRM].[CertificateOfOrigins_enum_CertificateOfOriginTypeCode]
)
BEGIN
 
         SET IDENTITY_INSERT CRM.CertificateOfOrigins_enum_CertificateOfOriginTypeCode ON

INSERT INTO CRM.CertificateOfOrigins_enum_CertificateOfOriginTypeCode (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate, IsCriterionMandatory, IsCustomApprovalRequired, ReportId, IsCustomsItemMandatory, IsZipcodeMandatory) VALUES

(1, N'EURMED', 1, N'EURMED', N'EURMED', N'EURMED', CONVERT(DATETIME, '1900-01-01 00:00:00.000', 121), CONVERT(DATETIME, '9999-12-31 00:00:00.000', 121), CONVERT(bit, 'False'), CONVERT(bit, 'True'), 7000, CONVERT(bit, 'False'), CONVERT(bit, 'True'))

 
 
INSERT INTO CRM.CertificateOfOrigins_enum_CertificateOfOriginTypeCode (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate, IsCriterionMandatory, IsCustomApprovalRequired, ReportId, IsCustomsItemMandatory, IsZipcodeMandatory) VALUES

(2, N'EUR1', 1, N'EUR1', N'EUR1', N'EUR1', CONVERT(DATETIME, '1900-01-01 00:00:00.000', 121), CONVERT(DATETIME, '9999-12-31 00:00:00.000', 121), CONVERT(bit, 'False'), CONVERT(bit, 'True'), 7000, CONVERT(bit, 'False'), CONVERT(bit, 'True'))

 
 
INSERT INTO CRM.CertificateOfOrigins_enum_CertificateOfOriginTypeCode (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate, IsCriterionMandatory, IsCustomApprovalRequired, ReportId, IsCustomsItemMandatory, IsZipcodeMandatory) VALUES

(3, N'MERCOSUR', 1, N'MERCOSUR', N'MERCOSUR', N'MERCOSUR', CONVERT(DATETIME, '1900-01-01 00:00:00.000', 121), CONVERT(DATETIME, '9999-12-31 00:00:00.000', 121), CONVERT(bit, 'True'), CONVERT(bit, 'True'), 7002, CONVERT(bit, 'True'), CONVERT(bit, 'False'))

 
 
INSERT INTO CRM.CertificateOfOrigins_enum_CertificateOfOriginTypeCode (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate, IsCriterionMandatory, IsCustomApprovalRequired, ReportId, IsCustomsItemMandatory, IsZipcodeMandatory) VALUES

(4, N'Columbia', 1, N'Columbia', N'Columbia', N'IsrCol', CONVERT(DATETIME, '1900-01-01 00:00:00.000', 121), CONVERT(DATETIME, '9999-12-31 00:00:00.000', 121), CONVERT(bit, 'True'), CONVERT(bit, 'True'), 7003, CONVERT(bit, 'False'), CONVERT(bit, 'False'))

 
 
INSERT INTO CRM.CertificateOfOrigins_enum_CertificateOfOriginTypeCode (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate, IsCriterionMandatory, IsCustomApprovalRequired, ReportId, IsCustomsItemMandatory, IsZipcodeMandatory) VALUES

(5, N'Non Manipulation Certificate', 1, N'Non Manipulation Certificate', N'Non Manipulation Certificate', N'NonManipulation', CONVERT(DATETIME, '1900-01-01 00:00:00.000', 121), CONVERT(DATETIME, '9999-12-31 00:00:00.000', 121), CONVERT(bit, 'False'), CONVERT(bit, 'True'), 7006, CONVERT(bit, 'False'), CONVERT(bit, 'False'))

 
 
INSERT INTO CRM.CertificateOfOrigins_enum_CertificateOfOriginTypeCode (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate, IsCriterionMandatory, IsCustomApprovalRequired, ReportId, IsCustomsItemMandatory, IsZipcodeMandatory) VALUES

(6, N'Panama', 1, N'Panama', N'Panama', N'Panama', CONVERT(DATETIME, '1900-01-01 00:00:00.000', 121), CONVERT(DATETIME, '9999-12-31 00:00:00.000', 121), CONVERT(bit, 'True'), CONVERT(bit, 'True'), 7001, CONVERT(bit, 'True'), CONVERT(bit, 'False'))

 
 
INSERT INTO CRM.CertificateOfOrigins_enum_CertificateOfOriginTypeCode (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate, IsCriterionMandatory, IsCustomApprovalRequired, ReportId, IsCustomsItemMandatory, IsZipcodeMandatory) VALUES

(7, N'Korea', 1, N'Korea', N'Korea', N'Korea', CONVERT(DATETIME, '1900-01-01 00:00:00.000', 121), CONVERT(DATETIME, '9999-12-31 00:00:00.000', 121), CONVERT(bit, 'True'), CONVERT(bit, 'True'), 7004, CONVERT(bit, 'True'), CONVERT(bit, 'True'))

 
 
INSERT INTO CRM.CertificateOfOrigins_enum_CertificateOfOriginTypeCode (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate, IsCriterionMandatory, IsCustomApprovalRequired, ReportId, IsCustomsItemMandatory, IsZipcodeMandatory) VALUES

(8, N'UnitedArabEmirates', 1, N'UnitedArabEmirates', N'UnitedArabEmirates', N'UnitedArabEmirates', CONVERT(DATETIME, '1900-01-01 00:00:00.000', 121), CONVERT(DATETIME, '9999-12-31 00:00:00.000', 121), CONVERT(bit, 'False'), CONVERT(bit, 'True'), 7004, CONVERT(bit, 'True'), CONVERT(bit, 'True'))

 
 
INSERT INTO CRM.CertificateOfOrigins_enum_CertificateOfOriginTypeCode (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate, IsCriterionMandatory, IsCustomApprovalRequired, ReportId, IsCustomsItemMandatory, IsZipcodeMandatory) VALUES

(9, N'Vietnam', 1, N'Vietnam', N'Vietnam', N'Vietnam', CONVERT(DATETIME, '1900-01-01 00:00:00.000', 121), CONVERT(DATETIME, '9999-12-31 00:00:00.000', 121), CONVERT(bit, 'True'), CONVERT(bit, 'True'), 7007, CONVERT(bit, 'False'), CONVERT(bit, 'True'))

 
 
INSERT INTO CRM.CertificateOfOrigins_enum_CertificateOfOriginTypeCode (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate, IsCriterionMandatory, IsCustomApprovalRequired, ReportId, IsCustomsItemMandatory, IsZipcodeMandatory) VALUES

(10, N'Guatemala', 1, N'Guatemala', N'Guatemala', N'Guatemala', CONVERT(DATETIME, '1900-01-01 00:00:00.000', 121), CONVERT(DATETIME, '9999-12-31 00:00:00.000', 121), CONVERT(bit, 'False'), CONVERT(bit, 'True'), 7004, CONVERT(bit, 'True'), CONVERT(bit, 'False'))

 
 
INSERT INTO CRM.CertificateOfOrigins_enum_CertificateOfOriginTypeCode (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate, IsCriterionMandatory, IsCustomApprovalRequired, ReportId, IsCustomsItemMandatory, IsZipcodeMandatory) VALUES

(11, N'EUR1-ACCUMULATION', 1, N'EUR1-ACCUMULATION', N'EUR1-ACCUMULATION', N'EUR1_ACCUMULATION', CONVERT(DATETIME, '1900-01-01 00:00:00.000', 121), CONVERT(DATETIME, '9999-12-31 00:00:00.000', 121), CONVERT(bit, 'False'), CONVERT(bit, 'True'), 7000, CONVERT(bit, 'False'), CONVERT(bit, 'True'))
         SET IDENTITY_INSERT CRM.CertificateOfOrigins_enum_CertificateOfOriginTypeCode OFF

END
IF NOT EXISTS (
    SELECT 1
    FROM [CRM].[CertificateOfOrigins_enum_Circumstances]
)
BEGIN
 
          -- IDENTITY_INSERT (ON) removed: CRM.CertificateOfOrigins_enum_Circumstances.ID has no IDENTITY property -> Msg 8106. Explicit IDs insert fine without it.

INSERT INTO CRM.CertificateOfOrigins_enum_Circumstances (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate) VALUES

(1, N'חשד למקור הטובין', 1, N'חשד למקור הטובין', N'ReasonableSuspicionSource', N'ReasonableSuspicionSource', NULL, NULL)

 
 
INSERT INTO CRM.CertificateOfOrigins_enum_Circumstances (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate) VALUES

(2, N'החלטת ועדת שוק', 1, N'החלטת ועדת שוק', N'Shuk', N'Shuk', NULL, NULL)

 
 
INSERT INTO CRM.CertificateOfOrigins_enum_Circumstances (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate) VALUES

(3, N'בדיקה פיזית', 1, N'בדיקה פיזית', N'Check', N'Check', NULL, NULL)

 
 
INSERT INTO CRM.CertificateOfOrigins_enum_Circumstances (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate) VALUES

(4, N'ליקוי טכני', 1, N'ליקוי טכני', N'Fault', N'Fault', NULL, NULL)

 
 
INSERT INTO CRM.CertificateOfOrigins_enum_Circumstances (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate) VALUES

(5, N'חקירה', 1, N'חקירה', N'Investigation', N'Investigation', NULL, NULL)

 
 
INSERT INTO CRM.CertificateOfOrigins_enum_Circumstances (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate) VALUES

(6, N'אקראי', 1, N'אקראי', N'Random', N'Random', NULL, NULL)

 
 
INSERT INTO CRM.CertificateOfOrigins_enum_Circumstances (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate) VALUES

(7, N'פסילת תעודות בעבר', 1, N'פסילת תעודות בעבר', N'Past', N'Past', NULL, NULL)

 
 
INSERT INTO CRM.CertificateOfOrigins_enum_Circumstances (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate) VALUES

(8, N'הובלה שלא במישרין', 1, N'הובלה שלא במישרין', N'Direct', N'Direct', NULL, NULL)
          -- IDENTITY_INSERT (OFF) removed: CRM.CertificateOfOrigins_enum_Circumstances.ID has no IDENTITY property -> Msg 8106. Explicit IDs insert fine without it.

END

IF NOT EXISTS (
    SELECT 1
    FROM [CRM].[CertificateOfOrigins_enum_ConstraintTypeEnum]
)
BEGIN
           SET IDENTITY_INSERT CRM.CertificateOfOrigins_enum_ConstraintTypeEnum ON

 
INSERT INTO CRM.CertificateOfOrigins_enum_ConstraintTypeEnum (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate) VALUES

(1, N'Mandatory', 1, N'Mandatory', N'Mandatory', N'Mandatory', CONVERT(DATETIME, '1900-01-01 00:00:00.000', 121), CONVERT(DATETIME, '9999-12-31 00:00:00.000', 121))

 
 
INSERT INTO CRM.CertificateOfOrigins_enum_ConstraintTypeEnum (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate) VALUES

(2, N'Optional', 1, N'Optional', N'Optional', N'Optional', CONVERT(DATETIME, '1900-01-01 00:00:00.000', 121), CONVERT(DATETIME, '9999-12-31 00:00:00.000', 121))

 
 
INSERT INTO CRM.CertificateOfOrigins_enum_ConstraintTypeEnum (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate) VALUES

(3, N'Condition', 1, N'Condition', N'Condition', N'Condition', CONVERT(DATETIME, '1900-01-01 00:00:00.000', 121), CONVERT(DATETIME, '9999-12-31 00:00:00.000', 121))
           SET IDENTITY_INSERT CRM.CertificateOfOrigins_enum_ConstraintTypeEnum OFF

END

IF NOT EXISTS (
    SELECT 1
    FROM [CRM].[CertificateOfOrigins_enum_CustomHouse]
)
BEGIN
            -- IDENTITY_INSERT (ON) removed: CRM.CertificateOfOrigins_enum_CustomHouse.ID has no IDENTITY property -> Msg 8106. Explicit IDs insert fine without it.

INSERT INTO CRM.CertificateOfOrigins_enum_CustomHouse (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate, customHouseAddress, CountryID) VALUES

(1, N'אסטוניה', 1, N'אסטוניה', N'ESTONIA', N'ESTONIA', NULL, NULL, N'M. Dmitri Je rov, Deputy Director General

Tax and Customs Board

Narva road 9j

15176 Tallin

ESTONIA

', 4)

            -- IDENTITY_INSERT (OFF) removed: CRM.CertificateOfOrigins_enum_CustomHouse.ID has no IDENTITY property -> Msg 8106. Explicit IDs insert fine without it.

END
IF NOT EXISTS (
    SELECT 1
    FROM [CRM].[CertificateOfOrigins_enum_Decision]
)
BEGIN
 
             -- IDENTITY_INSERT (ON) removed: CRM.CertificateOfOrigins_enum_Decision.ID has no IDENTITY property -> Msg 8106. Explicit IDs insert fine without it.

INSERT INTO CRM.CertificateOfOrigins_enum_Decision (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate, IsAutomatic, IsForCoordinator, IsForClaliMakorWorker) VALUES

(1, N'בקשת אימות חדשה', 1, N'בקשת אימות חדשה', N'NewAuthenticationRequest', N'NewAuthenticationRequest', NULL, NULL, CONVERT(bit, 'True'), CONVERT(bit, 'False'), CONVERT(bit, 'False'))

 
 
INSERT INTO CRM.CertificateOfOrigins_enum_Decision (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate, IsAutomatic, IsForCoordinator, IsForClaliMakorWorker) VALUES

(2, N'תשובת האימות פסולה', 1, N'תשובת האימות פסולה', N'Rejection', N'Rejection', NULL, NULL, CONVERT(bit, 'False'), CONVERT(bit, 'False'), CONVERT(bit, 'True'))

 
 
INSERT INTO CRM.CertificateOfOrigins_enum_Decision (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate, IsAutomatic, IsForCoordinator, IsForClaliMakorWorker) VALUES

(3, N'תשובת האימות תקינה', 1, N'תשובת האימות תקינה', N'Approval', N'Approval', NULL, NULL, CONVERT(bit, 'False'), CONVERT(bit, 'False'), CONVERT(bit, 'True'))

 
 
INSERT INTO CRM.CertificateOfOrigins_enum_Decision (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate, IsAutomatic, IsForCoordinator, IsForClaliMakorWorker) VALUES

(4, N'דרישת הבהרה נוספת', 1, N'דרישת הבהרה נוספת', N'DemandAnotherClarification', N'DemandAnotherClarification', NULL, NULL, CONVERT(bit, 'False'), CONVERT(bit, 'False'), CONVERT(bit, 'True'))

 
 
INSERT INTO CRM.CertificateOfOrigins_enum_Decision (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate, IsAutomatic, IsForCoordinator, IsForClaliMakorWorker) VALUES

(5, N'תשובת אימות חלקית', 1, N'תשובת אימות חלקית', N'Partly', N'Partly', NULL, NULL, CONVERT(bit, 'False'), CONVERT(bit, 'False'), CONVERT(bit, 'True'))

 
 
INSERT INTO CRM.CertificateOfOrigins_enum_Decision (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate, IsAutomatic, IsForCoordinator, IsForClaliMakorWorker) VALUES

(6, N'נדרש אימות', 1, N'נדרש אימות', N'AuthenticationRequried', N'AuthenticationRequried', NULL, NULL, CONVERT(bit, 'False'), CONVERT(bit, 'True'), CONVERT(bit, 'False'))

 
 
INSERT INTO CRM.CertificateOfOrigins_enum_Decision (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate, IsAutomatic, IsForCoordinator, IsForClaliMakorWorker) VALUES

(7, N'בקשת לא תקינה', 1, N'בקשת לא תקינה', N'AuthenticationNeedless', N'AuthenticationNeedless', NULL, NULL, CONVERT(bit, 'False'), CONVERT(bit, 'True'), CONVERT(bit, 'False'))

 
 
INSERT INTO CRM.CertificateOfOrigins_enum_Decision (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate, IsAutomatic, IsForCoordinator, IsForClaliMakorWorker) VALUES

(8, N'נשלחה פנייה ליבואן', 1, N'נשלחה פנייה ליבואן', N'LetterForImporterWasSent', N'LetterForImporterWasSent', NULL, NULL, CONVERT(bit, 'True'), CONVERT(bit, 'False'), CONVERT(bit, 'False'))

 
 
INSERT INTO CRM.CertificateOfOrigins_enum_Decision (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate, IsAutomatic, IsForCoordinator, IsForClaliMakorWorker) VALUES

(9, N'נשלחה תזכורת ליבואן', 1, N'נשלחה תזכורת ליבואן', N'ReminderForImporterWasSent', N'ReminderForImporterWasSent', NULL, NULL, CONVERT(bit, 'True'), CONVERT(bit, 'False'), CONVERT(bit, 'False'))
             -- IDENTITY_INSERT (OFF) removed: CRM.CertificateOfOrigins_enum_Decision.ID has no IDENTITY property -> Msg 8106. Explicit IDs insert fine without it.

END

IF NOT EXISTS (
    SELECT 1
    FROM [CRM].[CertificateOfOrigins_enum_DeliveryMethod]
)
BEGIN
 
              -- IDENTITY_INSERT (ON) removed: CRM.CertificateOfOrigins_enum_DeliveryMethod.ID has no IDENTITY property -> Msg 8106. Explicit IDs insert fine without it.

INSERT INTO CRM.CertificateOfOrigins_enum_DeliveryMethod (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate) VALUES

(1, N'לא נשלח', 1, N'לא נשלח', N'WasNotSend', N'WasNotSend', NULL, NULL)

 
 
INSERT INTO CRM.CertificateOfOrigins_enum_DeliveryMethod (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate) VALUES

(2, N'נשלחה פנייה בדואר', 1, N'נשלחה פנייה בדואר', N'Posted Mailing', N'PostedMailing', NULL, NULL)

 
 
INSERT INTO CRM.CertificateOfOrigins_enum_DeliveryMethod (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate) VALUES

(3, N'נשלחה פנייה במייל', 1, N'נשלחה פנייה במייל', N'Sent by e-mail request', N'SentByEmailRequest', NULL, NULL)

 
 
INSERT INTO CRM.CertificateOfOrigins_enum_DeliveryMethod (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate) VALUES

(4, N'נשלחה תזכורת ראשונה', 1, N'נשלחה תזכורת ראשונה', N'FirstRemindSent', N'FirstRemindSent', NULL, NULL)

 
 
INSERT INTO CRM.CertificateOfOrigins_enum_DeliveryMethod (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate) VALUES

(5, N'נשלחה תזכורת שניה', 1, N'נשלחה תזכורת שניה', N'SecondRemindSent', N'SecondRemindSent', NULL, NULL)
              -- IDENTITY_INSERT (OFF) removed: CRM.CertificateOfOrigins_enum_DeliveryMethod.ID has no IDENTITY property -> Msg 8106. Explicit IDs insert fine without it.

END
IF NOT EXISTS (
    SELECT 1
    FROM [CRM].[CertificateOfOrigins_enum_ExportAuthenticationRequestStatus]
)
BEGIN
 
               -- IDENTITY_INSERT (ON) removed: CRM.CertificateOfOrigins_enum_ExportAuthenticationRequestStatus.ID has no IDENTITY property -> Msg 8106. Explicit IDs insert fine without it.

INSERT INTO CRM.CertificateOfOrigins_enum_ExportAuthenticationRequestStatus (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate) VALUES

(1, N'ממתין לשליחת מכתב', 1, N'ממתין לשליחת מכתב', N'WaitingForLetterSending', N'WaitingForLetterSending', NULL, NULL)

 
 
INSERT INTO CRM.CertificateOfOrigins_enum_ExportAuthenticationRequestStatus (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate) VALUES

(2, N'ממתין למענה יצואן', 1, N'ממתין למענה יצואן', N'WaitingForExporter', N'WaitingForExporter', NULL, NULL)

 
 
INSERT INTO CRM.CertificateOfOrigins_enum_ExportAuthenticationRequestStatus (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate) VALUES

(3, N'ממתין למענה יצואן לאחר התראה', 1, N'ממתין למענה יצואן לאחר התראה', N'WaitingForExporterAnswerAfterNotification', N'WaitingForExporterAnswerAfterNotification', NULL, NULL)

 
 
INSERT INTO CRM.CertificateOfOrigins_enum_ExportAuthenticationRequestStatus (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate) VALUES

(4, N'ממתין לפרטים נוספים', 1, N'ממתין לפרטים נוספים', N'WaitingForAdditionalInformation', N'WaitingForAdditionalInformation', NULL, NULL)

 
 
INSERT INTO CRM.CertificateOfOrigins_enum_ExportAuthenticationRequestStatus (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate) VALUES

(5, N'מוכן לטיפול מקצועי', 1, N'מוכן לטיפול מקצועי', N'ReadyForProfessionalTreatment', N'ReadyForProfessionalTreatment', NULL, NULL)

 
 
INSERT INTO CRM.CertificateOfOrigins_enum_ExportAuthenticationRequestStatus (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate) VALUES

(6, N'סגור - תקין', 1, N'סגור - תקין', N'ClosedValid', N'ClosedValid', NULL, NULL)

 
 
INSERT INTO CRM.CertificateOfOrigins_enum_ExportAuthenticationRequestStatus (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate) VALUES

(7, N'סגור - לא תקין', 1, N'סגור - לא תקין', N'ClosedNotValid', N'ClosedNotValid', NULL, NULL)

 
 
INSERT INTO CRM.CertificateOfOrigins_enum_ExportAuthenticationRequestStatus (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate) VALUES

(8, N'סגור - תקין חלקי', 1, N'סגור - תקין חלקי', N'ClosedSemiValid', N'ClosedSemiValid', NULL, NULL)

 
 
INSERT INTO CRM.CertificateOfOrigins_enum_ExportAuthenticationRequestStatus (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate) VALUES

(9, N'בוטל', 1, N'בוטל', N'Cancelled', N'Cancelled', NULL, NULL)

                -- IDENTITY_INSERT (OFF) removed: CRM.CertificateOfOrigins_enum_ExportAuthenticationRequestStatus.ID has no IDENTITY property -> Msg 8106. Explicit IDs insert fine without it.

END
 

 IF NOT EXISTS (
    SELECT 1
    FROM  CRM.CertificateOfOrigins_enum_ImporterContactingReason
)
BEGIN
INSERT INTO CRM.CertificateOfOrigins_enum_ImporterContactingReason (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate) VALUES

(1, N' ."המכתב הוחזר בגין "הכתובת שגויה', 1, N' ."המכתב הוחזר בגין "הכתובת שגויה', N'TheLetterWasReturnedBecauseOfWrongAddress', N'TheLetterWasReturnedBecauseOfWrongAddress', NULL, NULL)
 
INSERT INTO CRM.CertificateOfOrigins_enum_ImporterContactingReason (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate) VALUES

(2, N'לא התקבל מענה מצד הספק על אף פנייתנו', 1, N'לא התקבל מענה מצד הספק על אף פנייתנו', N'VendorNoResponseToRequest', N'VendorNoResponseToRequest', NULL, NULL)

end

 
IF NOT EXISTS (
    SELECT 1
    FROM  CRM.CertificateOfOrigins_enum_PrefernceDocumentType
)
BEGIN
INSERT INTO CRM.CertificateOfOrigins_enum_PrefernceDocumentType (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate) VALUES

(1, N'תעודת מקור', 1, N'מסמך מקור', N'AuthonticationRequest', N'AuthonticationRequest', NULL, NULL)


INSERT INTO CRM.CertificateOfOrigins_enum_PrefernceDocumentType (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate) VALUES

(2, N'חשבון העדפה', 0, N'חשבון העדפה', N'AccountPreference', N'AccountPreference', NULL, NULL)

 
INSERT INTO CRM.CertificateOfOrigins_enum_PrefernceDocumentType (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate) VALUES

(3, N'תעודת מעבר', 1, N'תעודת מעבר', N'TravelDocument', N'TravelDocument', NULL, NULL)

 
 
INSERT INTO CRM.CertificateOfOrigins_enum_PrefernceDocumentType (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate) VALUES

(4, N'תעודת תנועה', 0, N'תעודת תנועה', N'MovementCertificate', N'MovementCertificate', NULL, NULL)

 
 
INSERT INTO CRM.CertificateOfOrigins_enum_PrefernceDocumentType (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate) VALUES

(5, N'הצהרת חשבונית', 0, N'הצהרת חשבונית', N'InvoiceStatement', N'InvoiceStatement', NULL, NULL)

 
 
INSERT INTO CRM.CertificateOfOrigins_enum_PrefernceDocumentType (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate) VALUES

(6, N'חשבון הצהרה', 1, N'חשבון הצהרה', N'AaccountStatment', N'AaccountStatment', NULL, NULL)

 end

 
IF NOT EXISTS (
    SELECT 1
    FROM  CRM.CertificateOfOrigins_enum_ReminderMethod
)
BEGIN
 
INSERT INTO CRM.CertificateOfOrigins_enum_ReminderMethod (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate, ReminderDate) VALUES

(1, N'תזכורת ראשונה', 1, N'תזכורת ראשונה', N'FirstReminder', N'FirstReminder', NULL, NULL, N'6')
end
 

 
IF NOT EXISTS (
    SELECT 1
    FROM  CRM.CertificateOfOrigins_enum_RequestReasonCode
)
BEGIN
 set identity_insert CRM.CertificateOfOrigins_enum_RequestReasonCode on
INSERT INTO CRM.CertificateOfOrigins_enum_RequestReasonCode (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate) VALUES

(1, N'הוספת תעודה חדשה', 1, N'הוספת תעודה חדשה', N'New Certificate', N'NewCertificate', CONVERT(DATETIME, '1900-01-01 00:00:00.000', 121), CONVERT(DATETIME, '9999-12-31 00:00:00.000', 121))

 
 
INSERT INTO CRM.CertificateOfOrigins_enum_RequestReasonCode (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate) VALUES

(2, N'הוספת תעודה בדיעבד', 1, N'הוספת תעודה בדיעבד', N'Retrospective Certificate', N'RetrospectiveCertificate', CONVERT(DATETIME, '1900-01-01 00:00:00.000', 121), CONVERT(DATETIME, '9999-12-31 00:00:00.000', 121))

 
 
INSERT INTO CRM.CertificateOfOrigins_enum_RequestReasonCode (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate) VALUES

(3, N'תיקון תעודה', 1, N'תיקון תעודה', N'Certificate Update', N'CertificateUpdate', CONVERT(DATETIME, '1900-01-01 00:00:00.000', 121), CONVERT(DATETIME, '9999-12-31 00:00:00.000', 121))

 
 
INSERT INTO CRM.CertificateOfOrigins_enum_RequestReasonCode (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate) VALUES

(4, N'החלפת תעודה', 1, N'החלפת תעודה', N'Certificate Replacement', N'CertificateReplacement', CONVERT(DATETIME, '1900-01-01 00:00:00.000', 121), CONVERT(DATETIME, '9999-12-31 00:00:00.000', 121))

 
 
INSERT INTO CRM.CertificateOfOrigins_enum_RequestReasonCode (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate) VALUES

(5, N'החלפת תעודה ביבוא', 1, N'החלפת תעודה ביבוא', N'Import Certificate Replacement', N'ImportCertificateReplacement', CONVERT(DATETIME, '1900-01-01 00:00:00.000', 121), CONVERT(DATETIME, '9999-12-31 00:00:00.000', 121))

 
 
INSERT INTO CRM.CertificateOfOrigins_enum_RequestReasonCode (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate) VALUES

(10, N'תעודה ריקה', 1, N'תעודה ריקה', N'Empty Certificate', N'EmptyCertificate', NULL, NULL)

 
 
INSERT INTO CRM.CertificateOfOrigins_enum_RequestReasonCode (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate) VALUES

(12, N'טיוטה', 1, N'טיוטה', N'Draft', N'Draft', NULL, NULL)

 
 
INSERT INTO CRM.CertificateOfOrigins_enum_RequestReasonCode (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate) VALUES

(13, N'קבלת סטטוס בקשה', 1, N'קבלת סטטוס בקשה', N'Get Request Status', N'GetRequestStatus', NULL, NULL)

 
 
INSERT INTO CRM.CertificateOfOrigins_enum_RequestReasonCode (ID, Name, State, Description, EnglishName, Enumeration, StartDate, EndDate) VALUES

(14, N'ביטול תעודה', 1, N'ביטול תעודה', N'Certificate Cancellation', N'CertificateCancellation', NULL, NULL)
 set identity_insert CRM.CertificateOfOrigins_enum_RequestReasonCode off
 END
-- ---------------------------------------------------------------------------------------------
-- Re-enable and VALIDATE every FK seeded above. WITH CHECK (not plain CHECK) forces SQL Server to
-- verify the existing rows, so a genuinely inconsistent seed still fails loudly here.
-- ---------------------------------------------------------------------------------------------
DECLARE @coo_recheck nvarchar(max) = N'';
SELECT @coo_recheck = @coo_recheck + N'ALTER TABLE [' + s.name + N'].[' + t.name + N'] WITH CHECK CHECK CONSTRAINT ALL;'
FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = N'CRM';
EXEC sp_executesql @coo_recheck;
