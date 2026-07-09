using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// Values verified against legacy Customs.CRM.Entities\CertificateOfOriginsPartial\Enums\ERequestReason.cs
// (RetrospectiveCertificate = 2 was already relied on by LoadDataFromExportDeclaration.)
public enum ERequestReason
{
    [Display(Name = "Retrospective certificate")]
    [Description("RetrospectiveCertificate")]
    RetrospectiveCertificate = 2,

    [Display(Name = "Certificate replacement")]
    [Description("CertificateReplacement")]
    CertificateReplacement = 4,

    [Display(Name = "Import certificate replacement")]
    [Description("ImportCertificateReplacement")]
    ImportCertificateReplacement = 5,

    [Display(Name = "Empty certificate")]
    [Description("EmptyCertificate")]
    EmptyCertificate = 10,

    [Display(Name = "Draft")]
    [Description("Draft")]
    Draft = 12,

    [Display(Name = "Get request status")]
    [Description("GetRequestStatus")]
    GetRequestStatus = 13,

    [Display(Name = "Certificate cancellation")]
    [Description("CertificateCancellation")]
    CertificateCancellation = 14
}
