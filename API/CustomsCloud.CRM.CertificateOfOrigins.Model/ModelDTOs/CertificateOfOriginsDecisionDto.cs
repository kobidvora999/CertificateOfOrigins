namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

public class CertificateOfOriginsDecisionDto
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public int State { get; set; }
    public string? Description { get; set; }
    public string? EnglishName { get; set; }
    public string? Enumeration { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsAutomatic { get; set; }
    public bool IsForCoordinator { get; set; }
    public bool IsForClaliMakorWorker { get; set; }
}
