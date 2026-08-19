using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RiskPulse.Models.Dto;
using RiskPulse.Models.ViewModel;
using RiskPulse.Services.Administration;
using RiskPulse.Services.Login;

namespace RiskPulse.Controllers;

[Authorize(Policy = $"Permission:{PermissionCatalog.Roles}")]
public class RolesController : Controller
{
    private readonly RolesService _rolesService;

    public RolesController(RolesService rolesService)
    {
        _rolesService = rolesService;
    }

    // --- Page load (Index) ---
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var permissions = await _rolesService.GetAllPermissionsAsync();

        return View(new RolesIndexViewModel
        {
            Permissions = permissions
        });
    }

    // --- Role grid + save ---
    [HttpGet]
    public async Task<IActionResult> Grid()
    {
        var rows = await _rolesService.GetGridRowsAsync();
        return Json(ApiResponse.Ok(rows));
    }

    [HttpPost]
    public async Task<IActionResult> Save([FromBody] RoleSaveDto model)
    {
        return await ControllerHelpers.TrySave(model, ModelState, m => m.RoleId,
            m => m.RoleId == 0 ? _rolesService.CreateRoleAsync(m) : _rolesService.UpdateRoleAsync(m), "Role");
    }

    [HttpPost]
    public async Task<IActionResult> Delete([FromBody] DeleteRequestDto request)
    {
        return await ControllerHelpers.TryDelete(request, ModelState,
            id => _rolesService.DeleteRoleAsync(id), "Role");
    }
}
