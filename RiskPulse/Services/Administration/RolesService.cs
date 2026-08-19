using Microsoft.EntityFrameworkCore;
using RiskPulse.Data;
using RiskPulse.Data.Entries;
using RiskPulse.Data.Extensions;
using RiskPulse.Models.Dto;
using RiskPulse.Models.ViewModel;
using RiskPulse.Services.Login;

namespace RiskPulse.Services.Administration;

public class RolesService
{
    private readonly AppDbContext _db;

    public RolesService(AppDbContext db)
    {
        _db = db;
    }

    // --- Reads (page dropdowns + grid) ---
    public async Task<List<OptionViewModel>> GetAllPermissionsAsync()
    {
        return await _db.Permissions.AsNoTracking()
            .OrderBy(p => p.PermissionId)
            .ToOptionListAsync(p => p.PermissionId, p => p.PermissionDesc);
    }

    public async Task<List<RoleGridRowViewModel>> GetGridRowsAsync()
    {
        return await _db.Roles
            .Include(r => r.DefaultPermission)
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .AsNoTracking()
            .OrderBy(r => r.RoleId)
            .Select(r => new RoleGridRowViewModel
            {
                RoleId = r.RoleId,
                RoleDesc = r.RoleDesc,
                DefaultPermissionId = r.DefaultPermissionId,
                DefaultPermissionDesc = r.DefaultPermission != null ? r.DefaultPermission.PermissionDesc : PermissionCatalog.Dashboard,
                PermissionIds = r.RolePermissions.Select(rp => rp.PermissionId).ToList(),
                PermissionDescs = r.RolePermissions.Select(rp => rp.Permission != null ? rp.Permission.PermissionDesc : null).ToList()
            })
            .ToListAsync();
    }

    // --- Role create/update ---
    public async Task<SaveResultDto> CreateRoleAsync(RoleSaveDto model)
    {
        var roleDesc = model.RoleDesc.Trim();

        ValidateDefaultPermission(model.DefaultPermissionId, model.PermissionIds);

        await _db.Roles.EnsureUniqueAsync(r => r.RoleDesc.ToLower() == roleDesc.ToLower(), "Role name", roleDesc);

        var role = new Role
        {
            RoleDesc = roleDesc,
            DefaultPermissionId = model.DefaultPermissionId,
            RolePermissions = model.PermissionIds
                .Distinct()
                .Select(permissionId => new RolePermission { PermissionId = permissionId })
                .ToList()
        };

        _db.Roles.Add(role);
        await _db.SaveChangesAsync();
        return new SaveResultDto { Id = role.RoleId };
    }

    public async Task<SaveResultDto> UpdateRoleAsync(RoleSaveDto model)
    {
        var roleDesc = model.RoleDesc.Trim();

        ValidateDefaultPermission(model.DefaultPermissionId, model.PermissionIds);

        await _db.Roles.EnsureUniqueAsync(r => r.RoleDesc.ToLower() == roleDesc.ToLower() && r.RoleId != model.RoleId, "Role name", roleDesc);

        var existing = await _db.Roles
            .Include(r => r.RolePermissions)
            .FirstOrDefaultAsync(r => r.RoleId == model.RoleId)
            ?? throw new InvalidOperationException($"Role with Id {model.RoleId} was not found.");

        existing.RoleDesc = roleDesc;
        existing.DefaultPermissionId = model.DefaultPermissionId;
        existing.RolePermissions.Clear();

        foreach (var permissionId in model.PermissionIds.Distinct())
        {
            existing.RolePermissions.Add(new RolePermission { PermissionId = permissionId });
        }

        await _db.SaveChangesAsync();
        return new SaveResultDto { Id = existing.RoleId };
    }

    private static void ValidateDefaultPermission(int? defaultPermissionId, List<int> permissionIds)
    {
        if (defaultPermissionId.HasValue && !permissionIds.Contains(defaultPermissionId.Value))
        {
            throw new InvalidOperationException("The default page must be one of the assigned permissions.");
        }
    }

    // --- Role delete ---
    public async Task DeleteRoleAsync(int roleId)
    {
        var role = await _db.Roles.FindAsync(roleId)
            ?? throw new InvalidOperationException($"Role with Id {roleId} was not found.");

        var inUseByUsers = await _db.Users.AnyAsync(u => u.RoleId == roleId);
        if (inUseByUsers)
        {
            throw new InvalidOperationException("Cannot delete a role that is assigned to one or more users.");
        }

        _db.Roles.Remove(role);
        await _db.SaveChangesAsync();
    }
}
