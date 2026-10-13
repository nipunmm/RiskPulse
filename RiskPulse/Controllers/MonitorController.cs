using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RiskPulse.Models.Enum;
using RiskPulse.Models.ViewModel;
using RiskPulse.Services.Assessment;
using RiskPulse.Services.Login;

namespace RiskPulse.Controllers;

[Authorize(Policy = $"Permission:{PermissionCatalog.Monitoring}")]
public class MonitorController : Controller
{
    private readonly MonitorService _monitorService;

    public MonitorController(MonitorService monitorService)
    {
        _monitorService = monitorService;
    }

    // --- Monitoring page (fully server-rendered cross-unit status matrix) ---
    [HttpGet]
    public async Task<IActionResult> Index(int? assessmentHeaderId, UnitType? unitType)
    {
        var model = new MonitorIndexViewModel();

        try
        {
            model = await _monitorService.GetMonitorAsync(assessmentHeaderId, unitType);
        }
        catch (InvalidOperationException ex)
        {
            model.LoadError = ex.Message;
        }

        return View(model);
    }
}
