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
        if (model == null || !ModelState.IsValid)
        {
            var message = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .FirstOrDefault() ?? "Please correct the form errors and try again.";

            return Json(ApiResponse.Fail<object>(message));
        }

        try
        {
            var isNew = model.UnitId == 0;
            var saved = await _unitsService.SaveUnitAsync(model);

            return Json(ApiResponse.Ok(new { id = saved.UnitId }, isNew ? "Unit created successfully." : "Unit updated successfully."));
        }
        catch (InvalidOperationException ex)
        {
            return Json(ApiResponse.Fail<object>(ex.Message));
        }
        catch (Exception)
        {
            return Json(ApiResponse.Fail<object>("An error occurred while saving. Please try again."));
        }
    }

    [HttpPost]
    public async Task<IActionResult> DeleteUnit([FromBody] DeleteRequestDto request)
    {
        if (request == null || !ModelState.IsValid)
        {
            return Json(ApiResponse.Fail<object>("Please correct the form errors and try again."));
        }

        try
        {
            await _unitsService.DeleteUnitAsync(request.Id);
            return Json(ApiResponse.Ok<object>(new { }, "Unit deleted successfully."));
        }
        catch (InvalidOperationException ex)
        {
            return Json(ApiResponse.Fail<object>(ex.Message));
        }
        catch (Exception)
        {
            return Json(ApiResponse.Fail<object>("An error occurred while deleting. Please try again."));
        }
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
        if (model == null || !ModelState.IsValid)
        {
            var message = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .FirstOrDefault() ?? "Please correct the form errors and try again.";

            return Json(ApiResponse.Fail<object>(message));
        }

        try
        {
            var isNew = model.GroupId == 0;
            var saved = await _unitsService.SaveGroupAsync(model);

            return Json(ApiResponse.Ok(new { id = saved.GroupId }, isNew ? "Group created successfully." : "Group updated successfully."));
        }
        catch (InvalidOperationException ex)
        {
            return Json(ApiResponse.Fail<object>(ex.Message));
        }
        catch (Exception)
        {
            return Json(ApiResponse.Fail<object>("An error occurred while saving. Please try again."));
        }
    }

    [HttpPost]
    public async Task<IActionResult> DeleteGroup([FromBody] DeleteRequestDto request)
    {
        if (request == null || !ModelState.IsValid)
        {
            return Json(ApiResponse.Fail<object>("Please correct the form errors and try again."));
        }

        try
        {
            await _unitsService.DeleteGroupAsync(request.Id);
            return Json(ApiResponse.Ok<object>(new { }, "Group deleted successfully."));
        }
        catch (InvalidOperationException ex)
        {
            return Json(ApiResponse.Fail<object>(ex.Message));
        }
        catch (Exception)
        {
            return Json(ApiResponse.Fail<object>("An error occurred while deleting. Please try again."));
        }
    }
}
