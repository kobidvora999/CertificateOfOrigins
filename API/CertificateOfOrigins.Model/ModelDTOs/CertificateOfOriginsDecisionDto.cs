namespace CertificateOfOrigins.Model.ModelDTOs;

// A decision lookup record (CRM.CertificateOfOrigins_enum_Decision) returned with the authentication request.
public class CertificateOfOriginsDecisionDto
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public int State { get; set; }

    public string? Description { get; set; }

    public string? EnglishName { get; set; }

    public string? Enumeration { get; set; }

    public DateTimeOffset? StartDate { get; set; }

    // Which screen's decision dropdown this value belongs to (CR 194221). The legacy client filters on these; exposing
    // them lets an API consumer do the same instead of receiving the whole table unfiltered.
    public bool IsForCoordinator { get; set; }

    public bool IsForClaliMakorWorker { get; set; }
}
