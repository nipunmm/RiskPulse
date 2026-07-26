using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RiskPulse.Controllers
{
    [Authorize]
    public class FormBuilderController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
