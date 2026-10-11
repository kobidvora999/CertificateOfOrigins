using IssueCertificateOfOrigin.Worker.Dto;
using IssueCertificateOfOrigin.Worker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IssueCertificateOfOrigin.Worker.Dal
{
    internal interface IIssueCertificateDal
    {
        Task UpdateCertificateOfOriginStatus(int certificateOfOriginId, int status);

        Task UpdateIsInPublishingProcess(int certificateOfOriginId, bool value);

        Task<CertificateOfOriginDetailsDto?> GetCertificateOfOriginById(int certificateOfOriginId);
    }
}
