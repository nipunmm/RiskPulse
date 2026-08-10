using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RiskPulse.Controllers
{
    [Authorize(Policy = "Permission:Assessment Control")]
    public class AssessmentControlController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
