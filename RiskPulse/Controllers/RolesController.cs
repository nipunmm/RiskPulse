using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RiskPulse.Models.AppModel;
using RiskPulse.Models.ViewModel;
using RiskPulse.Services.AccessControlService;

namespace RiskPulse.Controllers;

[Authorize(Policy = $"Permission:{PermissionCatalog.Roles}")]
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

    [HttpGet]
    public async Task<IActionResult> Grid()
    {
        var rows = (await _rolesService.GetAllRolesAsync()).Select(r => new RoleGridRow
        {
            RoleId = r.RoleId,
            RoleDesc = r.RoleDesc,
            DefaultPermissionId = r.DefaultPermissionId,
            DefaultPermissionDesc = r.DefaultPermission?.PermissionDesc ?? PermissionCatalog.Dashboard,
            PermissionIds = r.RolePermissions.Select(rp => rp.PermissionId).ToList(),
            PermissionDescs = r.RolePermissions.Select(rp => rp.Permission?.PermissionDesc).ToList()
        });
        return Json(ApiResponse.Ok(rows));
    }

    [HttpPost]
    public async Task<IActionResult> Save([FromBody] RoleSaveModel model)
    {
        if (model == null || !ModelState.IsValid)
        {
            var message = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .FirstOrDefault() ?? "Please correct the form errors and try again.";

            return Json(ApiResponse.Fail<object>(message));
        }

        if (model.DefaultPermissionId.HasValue && !model.PermissionIds.Contains(model.DefaultPermissionId.Value))
        {
            return Json(ApiResponse.Fail<object>("The default page must be one of the assigned permissions."));
        }

        try
        {
            var isNew = model.RoleId == 0;
            var saved = isNew
                ? await _rolesService.CreateRoleAsync(model.RoleDesc, model.PermissionIds, model.DefaultPermissionId)
                : await _rolesService.UpdateRoleAsync(model.RoleId, model.RoleDesc, model.PermissionIds, model.DefaultPermissionId);

            return Json(ApiResponse.Ok(new { id = saved.RoleId }, isNew ? "Role created successfully." : "Role updated successfully."));
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
}
