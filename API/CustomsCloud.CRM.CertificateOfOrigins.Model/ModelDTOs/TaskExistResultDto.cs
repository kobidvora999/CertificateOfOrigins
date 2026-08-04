namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// Result of the Tasks microservice IsTaskExist query (legacy IsTaskExistResultDTO). One row per matching task.
public class TaskExistResultDto
{
    public int TaskTypeId { get; set; }

    public bool IsTaskInProgress { get; set; }

    public int? UserId { get; set; }
}
