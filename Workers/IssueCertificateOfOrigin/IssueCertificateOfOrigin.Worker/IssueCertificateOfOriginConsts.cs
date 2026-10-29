using CustomsCloud.InfrastructureCore.Queue;
using CustomsCloud.InfrastructureCore.Worker;
using System.Diagnostics.CodeAnalysis;

namespace IssueCertificateOfOrigin.Worker;

public static class IssueCertificateOfOriginConsts
{
    public const string Draft = "טיוטה";
    public const string Final = "סופי";
    public const string IsDraft = "isDraft";
    public const string CertificateName = "תעודת {0} מספר {1}.pdf";
    public const string CertificateNameExtension = ".pdf";
    public const string CertificateOfOriginQueryURL = "CertificateOfOriginQueryURL";
    public const string EntityTypeCustomer = "לקוח";
    public const string EntityID = "EntityID";
    public const string AdditionalInfo = "AdditionalInfo";
    public const string PDF = "PDF";
}
