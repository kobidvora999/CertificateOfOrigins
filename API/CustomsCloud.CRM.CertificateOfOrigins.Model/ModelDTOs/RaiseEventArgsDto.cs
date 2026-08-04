namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// Legacy RaiseEventArgs — the Events-subsystem event-callback payload. HandleAuthenticationRequestDeliverySent
// consumes only RelatedEntities (it locates the AuthenticationRequestFile entity), so only that field is modelled;
// the Events service may send additional fields, which are ignored on deserialization.
public class RaiseEventArgsDto
{
    public List<VirtualEntityDto> RelatedEntities { get; set; } = [];
}
