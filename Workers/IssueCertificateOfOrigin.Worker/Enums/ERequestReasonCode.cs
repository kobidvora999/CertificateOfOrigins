namespace IssueCertificateOfOrigin.Worker.Enums
{
    public enum ERequestReasonCode
    {
        NewCertificate = 1,
        RetrospectiveCertificate = 2,
        CertificateUpdate = 3,
        CertificateReplacement = 4,
        ImportCertificateReplacement = 5,
        EmptyCertificate = 10,
        Draft = 12,
        GetRequestStatus = 13,
        CertificateCancellation = 14
    }
}
