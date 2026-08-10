using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RiskPulse.Models.ViewModel;
using RiskPulse.Services.AccessControlService;

namespace RiskPulse.Controllers;

[Authorize(Policy = "Permission:Roles")]
public class RolesController : Controller
{
    private readonly RolesService _rolesService;

    public RolesController(RolesService rolesService)
    {
        _rolesService = rolesService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var roles = await _rolesService.GetAllRolesAsync();
        var permissions = await _rolesService.GetAllPermissionsAsync();

        return View(new RolesIndexViewModel
        {
            Roles = roles,
            Permissions = permissions
        });
    }

    [HttpPost]
    public async Task<IActionResult> Save([FromBody] RoleSaveModel model)
    {
        if (!ModelState.IsValid)
        {
            return Json(new { success = false, message = "Please correct the errors below.", modelState = GetModelStateErrors() });
        }

        try
        {
            var isNew = model.RoleId == 0;
            var saved = isNew
                ? await _rolesService.CreateRoleAsync(model.RoleDesc, model.PermissionIds)
                : await _rolesService.UpdateRoleAsync(model.RoleId, model.RoleDesc, model.PermissionIds);

            return Json(new { success = true, message = isNew ? "Role created successfully." : "Role updated successfully.", id = saved.RoleId });
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
