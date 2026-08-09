using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RiskPulse.Authorization;
using RiskPulse.Data;
using RiskPulse.Services;

namespace RiskPulse.Controllers
{

    [Authorize]
    [PermissionAuthorize("Form Builder")]
    public class FormBuilderController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
