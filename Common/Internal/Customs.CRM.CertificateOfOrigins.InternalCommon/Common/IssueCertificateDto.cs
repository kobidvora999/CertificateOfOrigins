using System;

namespace Customs.CRM.CertificateOfOrigins.InternalCommon.Common
{
    public class IssueCertificateDto
    {
        public int ReportId { get; set; }
        public int CertificateOfOriginId { get; set; }
        public string CertificateNumber { get; set; }
        public int CertificateTypeID { get; set; }
        public string CertificateTypeName { get; set; }
        public int RequestReasonCode { get; set; }
        public int CertificateOfOriginStatusId { get; set; }
        public bool IsInPublishingProcess { get; set; }
        public int CreateCustomerID { get; set; }
        public string RejectCancelReason { get; set; }
        public string InternalApplication { get; set; }
        public string FeedbackRemark { get; set; }
        public DateTime? IssuingDate { get; set; }
        public bool? IsDeclarationReleased { get; set; }
        public Guid? GUID { get; set; }
        public int? OrganizationUnitID { get; set; }
    }
}
