using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using RiskPulse.Models;
using System.Diagnostics;

namespace RiskPulse.Controllers
{
    public class ErrorController : Controller
    {
        private readonly IWebHostEnvironment _env;

        public ErrorController(IWebHostEnvironment env)
        {
            _env = env;
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Index(int? statusCode = null)
        {
            var model = new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                StatusCode = statusCode ?? 500
            };

            var exceptionFeature = HttpContext.Features.Get<IExceptionHandlerFeature>();
            if (exceptionFeature != null)
            {
                model.ErrorMessage = exceptionFeature.Error.Message;
                model.TechnicalDetails = _env.IsDevelopment()
                    ? exceptionFeature.Error.ToString()
                    : null;
            }

            Response.StatusCode = model.StatusCode;

            return View("Error", model);
        }
    }
}
