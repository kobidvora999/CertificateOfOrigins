using CustomsCloud.InfrastructureCore.Proxy.Rest;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;

namespace CertificateOfOrigins.WebApi.Filters;

// A failed call to another microservice is not a fault of this service, so it is not answered with the platform's
// default 500: 503 when the service could not be reached (no response, 408, 503, 504), 502 when it answered with an
// error. The detail names the failed call. Legacy: the WCF fault reached the client as an error (analyst decision
// 2026-09-29). A 404 of a by-key lookup never gets here - those proxies map it to null.
[AttributeUsage(AttributeTargets.Class)]
public sealed class ProxyFailureExceptionFilterAttribute : ExceptionFilterAttribute
{
    public override void OnException(ExceptionContext context)
    {
        if (context.Exception is not ProxyExecutionException proxyException)
        {
            return;
        }

        var isUnreachable = proxyException.Response?.StatusCode is null or (HttpStatusCode)0
            or HttpStatusCode.RequestTimeout or HttpStatusCode.ServiceUnavailable or HttpStatusCode.GatewayTimeout;
        context.Result = new ObjectResult(new ProblemDetails
        {
            Title = isUnreachable ? "An external service is unavailable." : "An external service failed.",
            Detail = proxyException.Message,
        })
        {
            StatusCode = isUnreachable ? (int)HttpStatusCode.ServiceUnavailable : (int)HttpStatusCode.BadGateway,
        };
        context.ExceptionHandled = true;
    }
}
