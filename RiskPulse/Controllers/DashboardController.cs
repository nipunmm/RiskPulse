using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RiskPulse.Models.ViewModel;
using RiskPulse.Services.Dashboard;
using RiskPulse.Services.Login;

namespace RiskPulse.Controllers;

[Authorize(Policy = $"Permission:{PermissionCatalog.Dashboard}")]
public class DashboardController : Controller
{
    private readonly DashboardService _dashboardService;

    public DashboardController(DashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    private int CurrentUserId =>
        int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;

    // --- Landing page (fully server-rendered) ---
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var model = new DashboardViewModel();

        try
        {
            model = await _dashboardService.GetDashboardAsync(CurrentUserId);
        }
        catch (InvalidOperationException ex)
        {
            model.LoadError = ex.Message;
        }

        return View(model);
    }
}