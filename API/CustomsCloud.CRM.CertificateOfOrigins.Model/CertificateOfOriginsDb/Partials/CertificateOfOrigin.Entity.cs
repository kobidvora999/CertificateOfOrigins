using CustomsCloud.InfrastructureCore.Model;

namespace CustomsCloud.CRM.CertificateOfOrigins.Model.CertificateOfOriginsDb;

// Companion partial (survives scaffold regen). First-class aggregate with int audit columns
// (CreateDate/CreateUserId, UpdateDate/UpdateUserId) → ICloudEntity: the audit fields are stamped
// server-side by BaseBL.SetEntityFields from RequestMetadata, and the row is persisted through
// BaseBL.AddEntity/UpdateEntity + SaveChangesAsync (never a DAL SaveChangesAsync). See C12.
public partial class CertificateOfOrigin : ICloudEntity
{
}
