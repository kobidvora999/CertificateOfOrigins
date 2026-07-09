using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// Values verified against legacy Customs.CRM.Entities\CertificateOfOriginsPartial\Enums\ECertificateDetailsType.cs
public enum ECertificateDetailsType
{
    [Display(Name = "Exporter id")]
    [Description("ExporterId")]
    ExporterId = 1,

    [Display(Name = "Origin country")]
    [Description("OriginCountry")]
    OriginCountry = 13,

    [Display(Name = "Origin group of countries")]
    [Description("OriginGroupOfCountries")]
    OriginGroupOfCountries = 14,

    [Display(Name = "Destination country")]
    [Description("DestinationCountry")]
    DestinationCountry = 15,

    [Display(Name = "Destination group of countries")]
    [Description("DestinationGroupOfCountries")]
    DestinationGroupOfCountries = 16
}
