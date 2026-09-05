using CertificateOfOrigins.BL;
using CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore.WebApi;
using CustomsCloud.InfrastructureCore.WebApi.OpenApiOperations;
using Microsoft.AspNetCore.Mvc;

namespace CertificateOfOrigins.WebApi.Controllers.Api;

// C14 event-response controller. The deciding signal is the INPUT TYPE, not the wording of the operation: a legacy
// External operation whose incoming DTO is RaiseEventArgs/EventResponseArgs is an event response. It always lives
// under the `api` domain, and it is the one case where a second controller/BL pair appears inside a single domain —
// the class is named after its own BL (EventsResponseBl), which delegates to the business BL.
[ApiController]
[Route("api/[controller]")]
public class EventsResponseController(IServiceProvider serviceProvider)
    : BaseController<EventsResponseBl>(serviceProvider)
{
    // External WCF: HandleAuthenticationRequestDeliverySent(raiseEventArgs) — an Events-subsystem callback fired on a
    // delivery-sent event for an authentication file. As shipped it is a pure existence check (the legacy status-write
    // is commented out): it verifies the file exists and returns true/false. A callback with a body → POST.
    [HttpPost("HandleDeliverySent")]
    [BadRequestResponse][NotFoundResponse][OkJsonResponse(typeof(bool))]
    public async Task<ActionResult<bool>> HandleAuthenticationRequestDeliverySent([FromBody] RaiseEventArgsDto request)
    {
        var result = await BusinessLayer.HandleAuthenticationRequestDeliverySent(request);
        return Ok(result);
    }
}
