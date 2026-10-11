using IssueCertificateOfOrigin.Worker.Dto;
using IssueCertificateOfOrigin.Worker.Models;
using IssueCertificateOfOrigin.Worker;
using Microsoft.EntityFrameworkCore;
using Polly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IssueCertificateOfOrigin.Worker.Dal
{
    public class IssueCertificateDal(CustomsDbContext context) : IIssueCertificateDal
    {
        private CustomsDbContext Context { get; set; } = context;

        public async Task UpdateCertificateOfOriginStatus(int certificateOfOriginId, int status)
        {
            await Context.CertificateOfOriginsCertificateOfOrigins
                        .Where(c => c.Id == certificateOfOriginId)
                        .ExecuteUpdateAsync(s => s.SetProperty(c => c.CertificateOfOriginStatusId, status));
        }

        public async Task UpdateIsInPublishingProcess(int certificateOfOriginId, bool value)
        {
            await Context.CertificateOfOriginsCertificateOfOrigins
                        .Where(c => c.Id == certificateOfOriginId)
                        .ExecuteUpdateAsync(s => s.SetProperty(c => c.IsInPublishingProcess, value));
        }

        public async Task<CertificateOfOriginDetailsDto?> GetCertificateOfOriginById(int certificateOfOriginId)
        {
            var result = await Context.CertificateOfOriginsCertificateOfOrigins
                .Where(c => c.Id == certificateOfOriginId)
                .Select(c => new CertificateOfOriginDetailsDto()
                {
                    CertificateNumber = c.CertificateNumber,
                    CertificateTypeID = c.TypeId,
                    RequestReasonCode = c.RequestReasonCode,
                    StatusId = c.CertificateOfOriginStatusId,
                    IsInPublishingProcess = c.IsInPublishingProcess,
                })
                .FirstOrDefaultAsync();
            return result;
        }
    }
}
