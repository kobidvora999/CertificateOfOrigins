namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// Curated subset of the platform EMessageTypes (Customs.Inf.CommonService.ExternalCommon.MessageManagement.MessagesEnums)
// — only the message types this service sends. Values are the source of truth from the platform enum (not invented).
public enum EMessageTypes
{
    // File-level status-change notification for an import authentication-request file (SaveAuthenticationRequestFile).
    ImportRequestDecision = 11102,

    // Rejection notification for an import authentication request.
    ImportRequestRejection = 11113,

    // Central-decision notification for an import authentication request (SaveImportAuthenticationRequest).
    ImportRequestCentralDecision = 11167,
}
