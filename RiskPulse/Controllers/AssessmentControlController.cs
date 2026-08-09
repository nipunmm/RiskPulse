using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RiskPulse.Authorization;

namespace RiskPulse.Controllers
{
    [Authorize]
    [PermissionAuthorize("Assessment Control")]
    public class AssessmentControlController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
