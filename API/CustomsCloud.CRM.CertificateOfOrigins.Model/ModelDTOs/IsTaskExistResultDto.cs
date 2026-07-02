namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// Mirrors Customs.Infrastructure.Tasks IsTaskExistResultDTO.
public class IsTaskExistResultDto
{
    public int TaskTypeId { get; set; }
    public bool IsTaskInProgress { get; set; }
    public int? UserId { get; set; }
}
