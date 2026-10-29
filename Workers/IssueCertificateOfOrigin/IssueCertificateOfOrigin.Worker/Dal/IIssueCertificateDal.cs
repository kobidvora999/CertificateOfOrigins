namespace IssueCertificateOfOrigin.Worker.Dal
{
    // The worker's own writes — methods the module DAL (Shared/CertificateOfOrigins.DAL) does not have.
    internal interface IIssueCertificateDal
    {
        Task UpdateCertificateOfOriginStatus(int certificateOfOriginId, int status);

        Task UpdateIsInPublishingProcess(int certificateOfOriginId, bool value);
    }
}
