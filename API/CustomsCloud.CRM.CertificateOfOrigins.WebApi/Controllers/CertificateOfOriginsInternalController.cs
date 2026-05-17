using Microsoft.AspNetCore.Mvc;

namespace CustomsCloud.CRM.CertificateOfOrigins.WebApi.Controllers
{
    public class CertificateOfOriginsInternalController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
