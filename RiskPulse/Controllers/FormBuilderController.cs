using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RiskPulse.Services.AccessControlService;

namespace RiskPulse.Controllers
{

    [Authorize(Policy = $"Permission:{PermissionCatalog.FormBuilder}")]
    public class FormBuilderController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
