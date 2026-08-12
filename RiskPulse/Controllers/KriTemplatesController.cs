using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RiskPulse.Services.AccessControlService;

namespace RiskPulse.Controllers
{
    [Authorize(Policy = $"Permission:{PermissionCatalog.Kri}")]
    public class KriTemplatesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
