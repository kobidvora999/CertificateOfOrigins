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
}
