namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

public class CertificateDetailsTypeCodeEnumDto
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public int State { get; set; }
    public string? Description { get; set; }
    public string? EnglishName { get; set; }
    public string? Enumeration { get; set; }
    public DateTimeOffset? StartDate { get; set; }
    public DateTimeOffset? EndDate { get; set; }
    public string? Comment { get; set; }
    public string? DetailTypeFormat { get; set; }
    public int DataTypeId { get; set; }
}
