using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RiskPulse.Authorization;
using RiskPulse.Models.DbModel.AccessControl;
using RiskPulse.Models.ViewModel;
using RiskPulse.Services.AccessControlService;
using RiskPulse.Validation;

namespace RiskPulse.Controllers;

[Authorize]
[PermissionAuthorize("Users")]
public class UserManagementController : Controller
{
    private readonly IUserManagementService _userService;

    public UserManagementController(IUserManagementService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var users = await _userService.GetAllAsync();
        var units = await _userService.GetAllUnitsAsync();
        var roles = await _userService.GetAllRolesAsync();

        return View(new UserManagementIndexViewModel
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
            return Json(new { success = false, message = "Please correct the form errors and try again." });
        }

        if (!UsernameValidator.IsValid(user.Username))
        {
            return Json(new { success = false, message = UsernameValidator.ErrorMessage });
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

            TempData["SuccessMessage"] = isNew ? "User created successfully." : "User updated successfully.";
            return Json(new { success = true, message = TempData["SuccessMessage"], id = saved.Id });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }
}
