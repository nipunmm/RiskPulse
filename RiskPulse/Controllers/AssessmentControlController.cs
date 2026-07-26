using Microsoft.AspNetCore.Mvc;

namespace RiskPulse.Controllers
{
    public class AssessmentControlController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
