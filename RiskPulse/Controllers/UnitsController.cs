using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RiskPulse.Models.Dto;
using RiskPulse.Models.ViewModel;
using RiskPulse.Services.Administration;
using RiskPulse.Services.Login;

namespace RiskPulse.Controllers;

[Authorize(Policy = $"Permission:{PermissionCatalog.Units}")]
public class UnitsController : Controller
{
    private readonly UnitsService _unitsService;

    public UnitsController(UnitsService unitsService)
    {
        _unitsService = unitsService;
    }

    // --- Page load (Index) ---
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var units = await _unitsService.GetAllUnitsAsync();

        return View(new UnitsIndexViewModel
        {
            Units = units
        });
    }

    // --- Units (grid/save/delete) ---
    [HttpGet]
    public async Task<IActionResult> UnitGrid()
    {
        var rows = await _unitsService.GetGridRowsAsync();
        return Json(ApiResponse.Ok(rows));
    }

    [HttpPost]
    public async Task<IActionResult> SaveUnit([FromBody] UnitSaveDto model)
    {
        return await ControllerHelpers.TrySave(model, ModelState, m => m.UnitId,
            m => _unitsService.SaveUnitAsync(m), "Unit");
    }

    [HttpPost]
    public async Task<IActionResult> DeleteUnit([FromBody] DeleteRequestDto request)
    {
        return await ControllerHelpers.TryDelete(request, ModelState,
            id => _unitsService.DeleteUnitAsync(id), "Unit");
    }

    // --- Unit groups (grid/save/delete) ---
    [HttpGet]
    public async Task<IActionResult> GroupGrid()
    {
        var rows = await _unitsService.GetGroupGridRowsAsync();
        return Json(ApiResponse.Ok(rows));
    }

    [HttpPost]
    public async Task<IActionResult> SaveGroup([FromBody] GroupSaveDto model)
    {
        return await ControllerHelpers.TrySave(model, ModelState, m => m.GroupId,
            m => _unitsService.SaveGroupAsync(m), "Group");
    }

    [HttpPost]
    public async Task<IActionResult> DeleteGroup([FromBody] DeleteRequestDto request)
    {
        return await ControllerHelpers.TryDelete(request, ModelState,
            id => _unitsService.DeleteGroupAsync(id), "Group");
    }
}
