using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

public enum ECommunicationType
{
    [Display(Name = "Fixed line telephone")]
    [Description("Fixed_Line_Telephone")]
    FixedLineTelephone = 1,

    [Display(Name = "Mobile phone")]
    [Description("Mobile_Phone")]
    MobilePhone = 2,

    [Display(Name = "Fax")]
    [Description("FAX")]
    Fax = 3,

    [Display(Name = "Email")]
    [Description("Email")]
    Email = 4,

    [Display(Name = "Website")]
    [Description("Website")]
    Website = 5,

    [Display(Name = "Fixed line telephone abroad")]
    [Description("Fixed_Line_Telephone_Abroad")]
    FixedLineTelephoneAbroad = 6,

    [Display(Name = "Mobile phone abroad")]
    [Description("Mobile_Phone_Abroad")]
    MobilePhoneAbroad = 7,

    [Display(Name = "Fax abroad")]
    [Description("FAX_Abroad")]
    FaxAbroad = 8
}
