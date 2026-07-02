namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// Mirrors MalamTeam.Infrastructure VirtualEntity.
// Enum-typed fields are kept as int — the original enum is noted in a comment.
public class VirtualEntityDto
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public int State { get; set; }
    public DateTime UpdateDate { get; set; }
    public DateTime CreateDate { get; set; }
    public int CreateUserId { get; set; }
    public int TypeId { get; set; }
    public int UpdateUserId { get; set; }
    public int OrganizationUnitId { get; set; }
    public int CustomerId { get; set; }
    public int EntityType { get; set; } // EEntityType
    public byte[]? TimeStamp { get; set; }
    public List<VirtualEntityDto>? RelatedEntities { get; set; }
    public int? IEntityFUpdateCustomerId { get; set; }
    public int? ICustomerEntityUpdateCustomerId { get; set; }
    public int? IEntitySpecializationSpecializationId { get; set; }
    public int? IOrganizationUnitTypeOrganizationUnitTypeId { get; set; }
    public bool? IsAddAttachmentAllowed { get; set; }
    public int? EaddAttachmentNotAllowedStatus { get; set; } // EAddAttachmentNotAllowedStatus?
    public int? ETaskPriorityCode { get; set; } // ETaskPriorityCode?
    public string? EntityIdKeys { get; set; }
    public string? Path { get; set; }
}
