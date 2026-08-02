using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RiskPulse.Data;
using RiskPulse.Services;

namespace RiskPulse.Controllers
{

    [Authorize(Roles = "Admin")]
    public class FormBuilderController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
