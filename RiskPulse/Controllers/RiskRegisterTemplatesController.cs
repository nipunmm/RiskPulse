using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RiskPulse.Services.Login;

namespace RiskPulse.Controllers;

[Authorize(Policy = $"Permission:{PermissionCatalog.RiskRegister}")]
public class RiskRegisterTemplatesController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
