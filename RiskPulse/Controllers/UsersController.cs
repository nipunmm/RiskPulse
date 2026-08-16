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

    public UsersController(UsersService userService)
    {
        _userService = userService;
    }

    // --- Page load (Index) ---
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var units = await _userService.GetAllUnitsAsync();
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
        var rows = (await _userService.GetAllAsync()).Select(u => new UserGridRowViewModel
        {
            Id = u.Id,
            Username = u.Username,
            UnitId = u.UnitId,
            UnitDesc = u.Unit?.UnitDesc ?? string.Empty,
            RoleId = u.RoleId,
            RoleDesc = u.Role?.RoleDesc ?? string.Empty,
            IsActive = u.IsActive
        });
        return Json(ApiResponse.Ok(rows));
    }

    [HttpPost]
    public async Task<IActionResult> Save([FromBody] UserSaveDto user)
    {
        if (user == null || !ModelState.IsValid)
        {
            var message = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .FirstOrDefault() ?? "Please correct the form errors and try again.";

            return Json(ApiResponse.Fail<object>(message));
        }

        try
        {
            var isNew = user.Id == 0;
            var currentUserId = Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var saved = isNew
                ? await _userService.CreateUserAsync(user)
                : await _userService.UpdateUserAsync(user, currentUserId);

            return Json(ApiResponse.Ok(new { id = saved.Id }, isNew ? "User created successfully." : "User updated successfully."));
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
    public async Task<IActionResult> Delete([FromBody] DeleteRequestDto request)
    {
        if (request == null || !ModelState.IsValid)
        {
            return Json(ApiResponse.Fail<object>("Please correct the form errors and try again."));
        }

        try
        {
            var currentUserId = Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));
            await _userService.DeleteUserAsync(request.Id, currentUserId);

            return Json(ApiResponse.Ok<object>(new { }, "User deleted successfully."));
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
