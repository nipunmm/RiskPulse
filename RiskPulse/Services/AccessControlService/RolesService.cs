using Microsoft.EntityFrameworkCore;
using RiskPulse.Data;
using RiskPulse.Models.DbModel.AccessControl;

namespace RiskPulse.Services.AccessControlService;

public class RolesService : IRolesService
{
    private readonly AppDbContext _db;

    public RolesService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<Role>> GetAllRolesAsync()
    {
        return await _db.Roles
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .AsNoTracking()
            .OrderBy(r => r.RoleId)
            .ToListAsync();
    }

    public async Task<List<Permission>> GetAllPermissionsAsync()
    {
        return await _db.Permissions
            .AsNoTracking()
            .OrderBy(p => p.PermissionId)
            .ToListAsync();
    }

    public async Task<Role> CreateRoleAsync(string roleDesc, List<int> permissionIds)
    {
        roleDesc = roleDesc.Trim();

        var exists = await _db.Roles.AnyAsync(r => r.RoleDesc.ToLower() == roleDesc.ToLower());
        if (exists)
        {
            throw new InvalidOperationException($"Role name '{roleDesc}' already exists.");
        }

        var role = new Role
        {
            RoleDesc = roleDesc,
            RolePermissions = permissionIds
                .Distinct()
                .Select(permissionId => new RolePermission { PermissionId = permissionId })
                .ToList()
        };

        _db.Roles.Add(role);
        await _db.SaveChangesAsync();
        return role;
    }

    public async Task<Role> UpdateRoleAsync(int roleId, string roleDesc, List<int> permissionIds)
    {
        roleDesc = roleDesc.Trim();

        var exists = await _db.Roles.AnyAsync(r =>
            r.RoleDesc.ToLower() == roleDesc.ToLower() && r.RoleId != roleId);
        if (exists)
        {
            throw new InvalidOperationException($"Role name '{roleDesc}' already exists.");
        }

        var existing = await _db.Roles
            .Include(r => r.RolePermissions)
            .FirstOrDefaultAsync(r => r.RoleId == roleId)
            ?? throw new InvalidOperationException($"Role with Id {roleId} was not found.");

        existing.RoleDesc = roleDesc;
        existing.RolePermissions.Clear();

        foreach (var permissionId in permissionIds.Distinct())
        {
            existing.RolePermissions.Add(new RolePermission { PermissionId = permissionId });
        }

        await _db.SaveChangesAsync();
        return existing;
    }
}
