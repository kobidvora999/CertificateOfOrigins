using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IssueCertificateOfOrigin.Worker.Dto;

public interface IAppSettings
{
    public string CertificateOfOriginQueryURL { get; set; }
}

public class AppSettings : IAppSettings
{
    [Required]
    public string CertificateOfOriginQueryURL { get; set; } = null!;
}