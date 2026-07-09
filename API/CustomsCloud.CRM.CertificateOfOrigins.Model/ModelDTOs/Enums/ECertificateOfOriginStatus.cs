using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// Values verified against legacy Customs.CRM.Entities\CertificateOfOriginsPartial\Enums\ECertificateOfOriginStatus.cs
public enum ECertificateOfOriginStatus
{
    [Display(Name = "Error")]
    [Description("Error")]
    Error = 1,

    [Display(Name = "Received")]
    [Description("Received")]
    Received = 2,

    [Display(Name = "Rejected")]
    [Description("Rejected")]
    Rejected = 3,

    [Display(Name = "Cancelled")]
    [Description("Cancelled")]
    Cancelled = 4,

    [Display(Name = "Declaration mismatch")]
    [Description("DeclarationMismatch")]
    DeclarationMismatch = 5,

    [Display(Name = "Declaration match")]
    [Description("DeclarationMatch")]
    DeclarationMatch = 6,

    [Display(Name = "Pending release")]
    [Description("PendingRelease")]
    PendingRelease = 7,

    [Display(Name = "Published")]
    [Description("Published")]
    Published = 8
}
