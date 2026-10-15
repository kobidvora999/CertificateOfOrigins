namespace CertificateOfOrigins.DAL;

// The six columns of an export-document authentication request that the main read projection leaves out (the platform
// interceptor rejects a result of 30 or more columns and the entity has 35): State, OrganizationUnitId and the four audit
// columns. Read separately, by id.
public class ExportRequestStoredColumns
{
    public int State { get; set; }

    public int OrganizationUnitId { get; set; }

    public DateTime CreateDate { get; set; }

    public int CreateUserId { get; set; }

    public DateTime UpdateDate { get; set; }

    public int UpdateUserId { get; set; }
}
