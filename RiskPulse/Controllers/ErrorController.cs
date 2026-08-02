using Microsoft.AspNetCore.Mvc;
using RiskPulse.Models.ViewModel;
using System.Diagnostics;

namespace RiskPulse.Controllers
{
    public class ErrorController : Controller
    {
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Index()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
