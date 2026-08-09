using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RiskPulse.Authorization;

namespace RiskPulse.Controllers
{
    [Authorize]
    [PermissionAuthorize("Submissions")]
    public class SubmissionsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
