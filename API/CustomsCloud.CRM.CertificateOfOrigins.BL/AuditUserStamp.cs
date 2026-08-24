using CustomsCloud.InfrastructureCore.Model;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL;

// BaseBL.SetEntityFields (called by AddEntity/UpdateEntity) takes the acting user from RequestMetadata.User.
// That property is NOT populated in this service's request pipeline — only the flat RequestMetadata.UserId is,
// from the CC-USER-ID header (see the same note on GetCurrentUserOrganizationUnitId). SetEntityFields therefore
// stamps CreateUserId/UpdateUserId = 0, silently losing the audit trail: an internal-workload run on 2026-08-23
// wrote CreateUserID = 0 where the pre-C12 code wrote the real id.
//
// These helpers re-stamp the audit user from the id that IS populated. They run AFTER AddEntity/UpdateEntity so
// the infrastructure still owns the dates and the "CreateDate/CreateUserId not-modified" guard on update; only
// the user ids are corrected.
//
// TODO(platform): the proper fix is for the request pipeline to populate RequestMetadata.User, after which these
// helpers become redundant. IRequestMetadata.User is get-only, so it cannot be set from here.
internal static class AuditUserStamp
{
    // Insert: SetEntityFields stamped both user ids, both are wrong.
    internal static void ForInsert(ICloudEntity entity, int? requestUserId)
    {
        var userId = requestUserId ?? 0;
        entity.CreateUserId = userId;
        entity.UpdateUserId = userId;
    }

    // Update: CreateUserId is deliberately left alone — UpdateEntity marks it not-modified so the original
    // creator is preserved. Only the updating user needs correcting.
    internal static void ForUpdate(ICloudEntity entity, int? requestUserId)
    {
        entity.UpdateUserId = requestUserId ?? 0;
    }
}
