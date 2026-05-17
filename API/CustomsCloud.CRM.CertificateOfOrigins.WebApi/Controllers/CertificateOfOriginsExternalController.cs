using CustomsCloud.CRM.CertificateOfOrigins.BL;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace CustomsCloud.CRM.CertificateOfOrigins.WebApi.Controllers
{
    public class CertificateOfOriginsExternalController(IServiceProvider serviceProvider):Controller
    {
        public IActionResult Index()
        {
            var bl = serviceProvider.GetRequiredService<CertificateOfOriginBl>();
            return View();
        }
    }
}
