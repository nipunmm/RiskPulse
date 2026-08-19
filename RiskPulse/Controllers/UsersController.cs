using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RiskPulse.Models.Dto;
using RiskPulse.Models.ViewModel;
using RiskPulse.Services.Administration;
using RiskPulse.Services.Login;

namespace RiskPulse.Controllers;

[Authorize(Policy = $"Permission:{PermissionCatalog.Users}")]
public class UsersController : Controller
{
    private readonly UsersService _userService;
    private readonly UnitsService _unitsService;

    public UsersController(UsersService userService, UnitsService unitsService)
    {
        _userService = userService;
        _unitsService = unitsService;
    }

    // --- Page load (Index) ---
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var units = await _unitsService.GetAllUnitsAsync();
        var roles = await _userService.GetAllRolesAsync();

        return View(new UsersIndexViewModel
        {
            CurrentUserId = Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier)),
            Units = units,
            Roles = roles
        });
    }

    // --- User grid + save ---
    [HttpGet]
    public async Task<IActionResult> Grid()
    {
        var rows = await _userService.GetGridRowsAsync();
        return Json(ApiResponse.Ok(rows));
    }

    [HttpPost]
    public async Task<IActionResult> Save([FromBody] UserSaveDto user)
    {
        var error = ControllerHelpers.ValidateModel(user, ModelState);
        if (error != null) return error;

        return await ControllerHelpers.TryExecute(async () =>
        {
            var isNew = user!.Id == 0;
            var currentUserId = Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var result = isNew
                ? await _userService.CreateUserAsync(user)
                : await _userService.UpdateUserAsync(user, currentUserId);

            return Json(ApiResponse.Ok(new { id = result.Id }, isNew ? "User created successfully." : "User updated successfully."));
        }, "An error occurred while saving. Please try again.");
    }

    [HttpPost]
    public async Task<IActionResult> Delete([FromBody] DeleteRequestDto request)
    {
        var error = ControllerHelpers.ValidateModel(request, ModelState);
        if (error != null) return error;

        return await ControllerHelpers.TryExecute(async () =>
        {
            var currentUserId = Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));
            await _userService.DeleteUserAsync(request!.Id, currentUserId);

            return Json(ApiResponse.Ok<object>(new { }, "User deleted successfully."));
        }, "An error occurred while deleting. Please try again.");
    }
}
