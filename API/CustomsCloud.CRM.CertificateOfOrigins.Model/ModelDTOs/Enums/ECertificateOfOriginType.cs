using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// Values verified against legacy Customs.CRM.Entities\CertificateOfOriginsPartial\Enums\ECertificateOfOriginType.cs
public enum ECertificateOfOriginType
{
    [Display(Name = "EUR-MED")]
    [Description("EURMED")]
    Eurmed = 1,

    [Display(Name = "EUR-1")]
    [Description("EUR1")]
    Eur1 = 2,

    [Display(Name = "MERCOSUR")]
    [Description("MERCOSUR")]
    Mercosur = 3,

    [Display(Name = "Israel-Colombia")]
    [Description("IsrCol")]
    IsrCol = 4,

    [Display(Name = "Non manipulation")]
    [Description("NonManipulation")]
    NonManipulation = 5,

    [Display(Name = "Panama")]
    [Description("Panama")]
    Panama = 6,

    [Display(Name = "South Korea")]
    [Description("SouthKorea")]
    SouthKorea = 7,

    [Display(Name = "United Arab Emirates")]
    [Description("UnitedArabEmirates")]
    UnitedArabEmirates = 8
}
