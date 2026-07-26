using Microsoft.AspNetCore.Mvc;

namespace RiskPulse.Controllers
{
    public class SubmissionsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
