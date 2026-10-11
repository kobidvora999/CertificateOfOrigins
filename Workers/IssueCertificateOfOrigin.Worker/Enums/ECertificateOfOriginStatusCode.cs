namespace IssueCertificateOfOrigin.Worker.Enums
{
    public enum ECertificateOfOriginStatusCode
    {
        None = 0,
        Error = 1,
        Received = 2,
        Rejected = 3,
        Cancelled = 4,
        DeclarationMismatch = 5,
        DeclarationMatch = 6,
        PendingRelease = 7,
        Published = 8
    }
}
