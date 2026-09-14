using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CertificateOfOrigins.Model.ModelDTOs;

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

    // Display/Description are the customer-facing type name used on the generated certificate document title +
    // filename; they MUST mirror the CRM.CertificateOfOrigins_enum_CertificateOfOriginTypeCode.Name column (seed data).
    // Id 7's Name is "Korea" in the table (NOT "SouthKorea") — the member identifier stays SouthKorea for code refs.
    [Display(Name = "Korea", Description = "Korea")]
    [Description("Korea")]
    SouthKorea = 7,

    [Display(Name = "UnitedArabEmirates", Description = "UnitedArabEmirates")]
    [Description("UnitedArabEmirates")]
    UnitedArabEmirates = 8,

    [Display(Name = "Vietnam", Description = "Vietnam")]
    [Description("Vietnam")]
    Vietnam = 9,
}
