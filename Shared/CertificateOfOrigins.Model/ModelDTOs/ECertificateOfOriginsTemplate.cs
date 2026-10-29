namespace CertificateOfOrigins.Model.ModelDTOs;

// The templates this service can render through the Templates microservice. The caller passes the value as the
// templateId; CertificateOfOriginsBl.GetTemplateMeta maps it to the registered template name + data contract.
public enum ECertificateOfOriginsTemplate
{
    // CR 194221 — the origin-verification request letter sent to the Korea Customs Service
    // ("מכתב פנייה לדרום קוריאה"). Registered in the Templates module as
    // ExportAuthenticationSendDeliveryForExporterSouthKoreaTemplate.
    //
    // TODO(confirm): the numeric value. The legacy keyed these off the shared ETemplate enum, which lives only in a
    // binary (Shared Binaries) and has no .NET 10 counterpart, so this id is service-local. The caller wired up in
    // step 4 must agree on it — confirm with the developer before that integration, and do not treat 1 as final.
    SouthKoreaOriginVerificationLetter = 1,

    // The legacy preference-document letters. The value is the legacy template id (Infrastructure.General_enum_Template.ID,
    // the ETemplate value the legacy callers sent), so a caller ported from legacy passes the id it always passed. The
    // name is the template's Enumeration without underscores (CA1707); the Enumeration itself is the name registered in the
    // Templates module (GetTemplateMeta).

    // Import verification — the authentication file (CRM.CertificateOfOrigins_ImportAuthenticationFileDetails.ID).
    RequesrForVerificationEurope = 2251,
    RequesrForVerificationJordan = 2252,
    RequesrForVerificationMexico = 2253,
    RequesrForVerificationMercosur = 2254,
    RequesrForVerificationCanada = 2255,
    RequesrForVerificationTurkey = 2256,
    RequesrForVerificationGeneral = 2257,
    RequesrForVerificationUsaTemplateNew = 2258,
    RequesrForVerificationUsaTemplate = 2259,

    // The importer alert letter — the authentication request (CRM.CertificateOfOrigins_ImportAuthenticationRequest.DocumentID).
    ImportAuthenticationRequestImporterLetter = 2244,

    // Export verification — the export authentication request (CRM.CertificateOfOrigins_ExportDocumentAuthenticationRequest.ID).
    ExportAuthenticationRequestEurope = 2289,
    ExportAuthenticationSendDeliveryForExporterMercosurTemplate = 2290,
    ExportAuthenticationSendDeliveryForExporterTurkeyJordanTemplate = 2291,
    ExportAuthenticationSendDeliveryForExporterEuropeTemplate = 2293,
    ExportAuthenticationRequestMercosur = 2295,
}
