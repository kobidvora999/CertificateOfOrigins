using CertificateOfOrigins.DAL;
using CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore.BL;

namespace CertificateOfOrigins.BL;

// C14 event-response BL. The deciding signal for an event response is the INPUT TYPE: a legacy External operation
// whose incoming DTO is RaiseEventArgs/EventResponseArgs is an event response, and it lives under the `api` domain
// as api/EventsResponse with its own controller and its own BL.
//
// This BL is deliberately thin: it owns no logic of its own and delegates straight to the business BL. Its only
// reason to exist is that C14 binds one controller to one BL, and the event-response controller must not be bound
// to the business BL that also serves the ui domain.
public class EventsResponseBl(IServiceProvider serviceProvider)
    : BaseBL<EventsResponseBl, ICertificateOfOriginsDal>(serviceProvider)
{
    // External WCF: HandleAuthenticationRequestDeliverySent(raiseEventArgs) — an Events-subsystem callback fired on
    // a delivery-sent event for an authentication file. Delegates to AuthenticationRequestBl, which owns the
    // behaviour (a pure existence check, faithful to the legacy whose status-write is commented out).
    public async Task<bool> HandleAuthenticationRequestDeliverySent(RaiseEventArgsDto request)
    {
        var authenticationRequestBl = Resolve<AuthenticationRequestBl>();
        var result = await authenticationRequestBl.HandleAuthenticationRequestDeliverySent(request);
        return result;
    }
}
