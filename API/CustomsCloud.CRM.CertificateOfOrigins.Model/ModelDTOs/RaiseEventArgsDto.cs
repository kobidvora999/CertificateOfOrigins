namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// Mirrors MalamTeam.Infrastructure RaiseEventArgs — only the members consumed by
// HandleAuthenticationRequestDeliverySent are modeled.
public class RaiseEventArgsDto
{
    public List<VirtualEntityDto>? RelatedEntities { get; set; }
}
