using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RiskPulse.Controllers
{
    [Authorize]
    public class SubmissionsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
