using CertificateOfOrigins.DAL;
using Microsoft.EntityFrameworkCore;

namespace IssueCertificateOfOrigin.Worker.Dal
{
    // Runs on the module's DbContext and entity (Shared); only the worker-specific writes live here.
    public class IssueCertificateDal(CertificateOfOriginsDbContext context) : IIssueCertificateDal
    {
        private CertificateOfOriginsDbContext Context { get; set; } = context;

        public async Task UpdateCertificateOfOriginStatus(int certificateOfOriginId, int status)
        {
            await Context.CertificateOfOrigins
                        .Where(c => c.Id == certificateOfOriginId)
                        .ExecuteUpdateAsync(s => s.SetProperty(c => c.CertificateOfOriginStatusId, status));
        }

        public async Task UpdateIsInPublishingProcess(int certificateOfOriginId, bool value)
        {
            await Context.CertificateOfOrigins
                        .Where(c => c.Id == certificateOfOriginId)
                        .ExecuteUpdateAsync(s => s.SetProperty(c => c.IsInPublishingProcess, value));
        }
    }
}
