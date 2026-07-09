using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// Values verified against the legacy enum:
// Malam.Infrastructure\MalamTeam.Infrastructure.GeneralServices.Environment\Enums\EEventType.cs
// Only the members this service uses are migrated — add members with their legacy ids as needed.
public enum EEventType
{
    [Display(Name = "Export declaration submission succeeded")]
    [Description("ExportDeclarationSubmissionSucceeded")]
    ExportDeclarationSubmissionSucceeded = 240,

    [Display(Name = "Export declaration amendment request completed")]
    [Description("ExportDeclarationAmendmentRequestCompleted")]
    ExportDeclarationAmendmentRequestCompleted = 334,

    [Display(Name = "Cancellation request commited")]
    [Description("CancellationRequestCommited")]
    CancellationRequestCommited = 554,

    [Display(Name = "Certificate of origin certificate replaced")]
    [Description("CertificateOfOriginCertificateReplaced")]
    CertificateOfOriginCertificateReplaced = 639,

    [Display(Name = "Certificate of origin certificate issued")]
    [Description("CertificateOfOriginCertificateIssued")]
    CertificateOfOriginCertificateIssued = 640,

    [Display(Name = "Certificate of origin certificate match declaration")]
    [Description("CertificateOfOriginCertificateMatchDeclaration")]
    CertificateOfOriginCertificateMatchDeclaration = 642,

    [Display(Name = "Certificate of origin certificate declaration mismatch")]
    [Description("CertificateOfOriginCertificateDeclarationMismatch")]
    CertificateOfOriginCertificateDeclarationMismatch = 643,

    [Display(Name = "Export declaration released")]
    [Description("ExportDeclarationReleased")]
    ExportDeclarationReleased = 1423,

    [Display(Name = "Assembly shared release accepted")]
    [Description("AssemblySharedReleaseAccepted")]
    AssemblySharedReleaseAccepted = 1790,

    [Display(Name = "Export declaration connect to certificate of origin canceled")]
    [Description("ExportDeclarationConnectToCertificateOfOriginCanceled")]
    ExportDeclarationConnectToCertificateOfOriginCanceled = 1910,

    [Display(Name = "Certificate of origin certificate declaration has warnings")]
    [Description("CertificateOfOriginCertificateDeclarationHasWarnings")]
    CertificateOfOriginCertificateDeclarationHasWarnings = 2171,

    [Display(Name = "Open task handling the replacement of an import certificate")]
    [Description("OpenTaskHandlingTheReplacementOfAnImportCertificate")]
    OpenTaskHandlingTheReplacementOfAnImportCertificate = 2172
}
