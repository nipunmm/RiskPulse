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
        if (string.IsNullOrWhiteSpace(model.RoleDesc))
        {
            return Json(new { success = false, message = "Role name is required." });
        }

        if (model.PermissionIds == null || model.PermissionIds.Count == 0)
        {
            return Json(new { success = false, message = "At least one permission is required." });
        }

        try
        {
            var isNew = model.RoleId == 0;
            var saved = isNew
                ? await _rolesService.CreateRoleAsync(model.RoleDesc, model.PermissionIds, model.DefaultPermissionId)
                : await _rolesService.UpdateRoleAsync(model.RoleId, model.RoleDesc, model.PermissionIds, model.DefaultPermissionId);

            return Json(new { success = true, message = isNew ? "Role created successfully." : "Role updated successfully.", id = saved.RoleId });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }
}
