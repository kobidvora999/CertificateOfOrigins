using CustomsCloud.InfrastructureCore.Model;

namespace CustomsCloud.CRM.CertificateOfOrigins.Model.CertificateOfOriginsDb;

// Companion partial (survives scaffold regen). See CertificateOfOrigin.Entity.cs — same rationale.
// CreateDate/UpdateDate are mapped as DateTime (not the repo's usual DateTimeOffset) because ICloudEntity
// declares them as DateTime; the underlying SQL columns are [datetime], so they map natively and simply skip
// the DateTimeOffset value converter in CertificateOfOriginsDbContext.OnModelCreating.
public partial class CertificateOfOriginsImportAuthenticationFileDetails : ICloudEntity
{
}
