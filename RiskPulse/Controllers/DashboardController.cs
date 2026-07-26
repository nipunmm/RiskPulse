using Microsoft.AspNetCore.Mvc;

namespace RiskPulse.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
