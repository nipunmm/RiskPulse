using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RiskPulse.Controllers
{
    [Authorize]
    public class AssessmentControlController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
