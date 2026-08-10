using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RiskPulse.Models.DbModel.AccessControl;
using RiskPulse.Models.ViewModel;
using RiskPulse.Services.AccessControlService;

namespace RiskPulse.Controllers;

[Authorize(Policy = "Permission:Users")]
public class UsersController : Controller
{
    private readonly UsersService _userService;

    public UsersController(UsersService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var users = await _userService.GetAllAsync();
        var units = await _userService.GetAllUnitsAsync();
        var roles = await _userService.GetAllRolesAsync();

        return View(new UsersIndexViewModel
        {
            CurrentUserId = Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier)),
            Users = users,
            Units = units,
            Roles = roles
        });
    }

    [HttpPost]
    public async Task<IActionResult> Save([FromBody] User user)
    {
        if (!ModelState.IsValid)
        {
            return Json(new
            {
                success = false,
                message = "Please correct the errors below.",
                modelState = GetModelStateErrors()
            });
        }

        try
        {
            var isNew = user.Id == 0;

            if (!isNew)
            {
                var currentUserId = Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));
                if (currentUserId == user.Id)
                {
                    return Json(new { success = false, message = "You cannot edit your own user record." });
                }
            }

            var saved = isNew
                ? await _userService.CreateUserAsync(user)
                : await _userService.UpdateUserAsync(user);

            return Json(new { success = true, message = isNew ? "User created successfully." : "User updated successfully.", id = saved.Id });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    private Dictionary<string, string[]> GetModelStateErrors()
    {
        return ModelState
            .Where(kvp => (kvp.Value?.Errors.Count ?? 0) > 0)
            .ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray());
    }
}
