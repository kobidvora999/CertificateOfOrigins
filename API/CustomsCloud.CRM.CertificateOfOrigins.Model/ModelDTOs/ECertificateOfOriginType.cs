using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// Generated from CRM.CertificateOfOrigins_enum_CertificateOfOriginTypeCode. Values are the source of truth
// (not invented).
[Flags]
public enum ECertificateOfOriginType
{
    [Display(Name = "EURMED", Description = "EURMED")]
    [Description("EURMED")]
    EURMED = 1,

    [Display(Name = "EUR1", Description = "EUR1")]
    [Description("EUR1")]
    EUR1 = 2,

    [Display(Name = "MERCOSUR", Description = "MERCOSUR")]
    [Description("MERCOSUR")]
    MERCOSUR = 3,

    [Display(Name = "Columbia", Description = "Columbia")]
    [Description("Columbia")]
    IsrCol = 4,

    [Display(Name = "Non Manipulation Certificate", Description = "Non Manipulation Certificate")]
    [Description("Non Manipulation Certificate")]
    NonManipulation = 5,

    [Display(Name = "Panama", Description = "Panama")]
    [Description("Panama")]
    Panama = 6,

    [Display(Name = "SouthKorea", Description = "SouthKorea")]
    [Description("SouthKorea")]
    SouthKorea = 7,

    [Display(Name = "UnitedArabEmirates", Description = "UnitedArabEmirates")]
    [Description("UnitedArabEmirates")]
    UnitedArabEmirates = 8,
}
