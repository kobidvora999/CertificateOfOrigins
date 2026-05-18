namespace Customs.CRM.CertificateOfOrigins.InternalCommon
{
    /// <summary>
    /// CertificateOfOriginsConsts
    /// </summary>
    public static class CertificateOfOriginsConsts
    {
        public const int NavigationToVendorPathID = 359;
        public const string PrefixIL = "IL";
        public const string SuffixFormat10Digit = "0000000000";
        public const long DefaultNumerator = 700000;
        public const string DbType = "Shared.IntArray";
        public const string RowName = "val";
        public const string ForeignCustomsHouseName = "בית מכס זר";
        public const string IssuingDate = "Issuing Date";
        public const string ImportProcessRequestTitle = "בקשת אימות מסמך מקור";
        public const string ExportProcessRequestTitle = "בקשת אימות מסמך מקור (יצוא)";
        public const int NabagCustomsHouse = 4;

        public const string CannotSetDecisionAuthenticationNeedless = "קיימות בטוחות על הבקשה. יש להעביר למחלקת מקור לאימות";

        public const string StatusPredicateString = "ID == {0} || ID == {1} ||ID == {2} ||ID == {3}||ID == {4}||ID == {5}";

        public const string AddAuthenticationRequestReminderWasSend = " ||ID == {0}";
        public const int PackingTypeContainer = 379;
        public const int DocumentAdditionaFieldIDForCertificateNumber = 46;
        public const int MaxLengthOfAddress = 140;
        public const int IdOfExporterIdInCertificateDetailsTypeCode = 1;
        public const int IdOfOriginCountryInCertificateDetailsTypeCode = 13;
        public const int IdOfDestinationCountryInCertificateDetailsTypeCode = 15;
        public const string IsExportDeclarationActive = "IsExportDeclarationActive";
        public const int LengthOfTaskStart = 70;
        public const int MaximumNumberOfCharactersOfTheField = 253;
        public const string Draft = "טיוטה";
        public const string Final = "סופי";
        public const string IsDraft = "isDraft";
        public const string NoMatchingCertificate = "No Matching Certificate";
        public const string InvalidGuid = "Invalid Guid";
        public const string ThereIsNoMatchBetweenTheCertificateDataAndTheDeclaration = "אין התאמה בין נתוני התעודה להצהרה";
        #region StoredProcedures
        public const string UpdateRequestsProcedure = "[CRM].[usp_CertificateOfOrigins_UpdateImportAuthenticationRequest]";
        public const string GetFileAndRequestProcedure = "[CRM].[usp_CertificateOfOrigins_GetImportAuthenticationFileDetailsAndRequests]";
        public const string GetRequestByIdProcedure = "[CRM].[usp_CertificateOfOrigins_CertificateOfOrigins_GetImportAuthenticationRequestById]";
        public const string GetAuthenticationRequestByLeadDocumentID = "[CRM].[usp_CertificateOfOrigins_GetAuthenticationRequestByLeadDocumentID]";
        public const string GetImportAuthenticationRequestsForReminderForImporterScheduler = "[CRM].[usp_CertificateOfOrigins_GetImportAuthenticationRequestsForReminderForImporterScheduler]";
        public const string GetAuthenticationRequestsForScheduler = "[CRM].[usp_CertificateOfOrigins_GetAuthenticationRequestsForScheduler]";
        public const string CheckIfExistsAdditionalRequestsForImporter = "[CRM].[usp_CertificateOfOrigins_CheckIfExistsAdditionalRequestsForImporter]";
        public const string CheckIfExistsAdditionalRequestsForVendor = "[CRM].[usp_CertificateOfOrigins_CheckIfExistsAdditionalRequestsForVendor]";
        public const string GetCertificateOfOriginDataForWebQuerySP = "CRM.usp_CertificateOfOrigin_GetCertificateOfOriginDataForWebQuery";
        public const string GetCertificateOfOriginByIDSP = "[CRM].[usp_CertificateOfOrigins_GetCertificateOfOriginByID]";
        public const string GetCertificateOfOriginsByFilterSP = "[CRM].[usp_CertificateOfOrigins_GetCertificateOfOriginsByFilter]";
        #endregion

        #region Parameters
        public const string LeadDocumentIDs = "@LeadDocumentIDs";
        public const string GuidSPParam = "@Guid";
        public const string CertificateOfOriginNumberSPParam = "@CertificateOfOriginNumber";
        public const string IssuingDateSPParam = "@IssuingDate";
        #endregion

        #region ExportDocumentAuthenticationRequest

        public const string GetExportDocumentAuthenticationRequestByIDProcName =
            "[CRM].[usp_CertificateOfOrigins_GetExportDocumentAuthenticationRequestByID]";

        public const string IDParamName = "@GetExportDocumentAuthenticationRequestID";

        public const string ExportDocumentAuthenticationRequestSearchProcName =
            "[CRM].[usp_CertificateOfOrigins_CROSS_ExportDocumentAuthenticationRequestSearch]";

        //public const string 

        #endregion ExportDocumentAuthenticationRequest
    }
}
