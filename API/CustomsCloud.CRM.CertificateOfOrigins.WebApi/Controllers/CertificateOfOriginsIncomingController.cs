using Microsoft.AspNetCore.Mvc;

namespace CustomsCloud.CRM.CertificateOfOrigins.WebApi.Controllers
{
    public class CertificateOfOriginsIncomingController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
