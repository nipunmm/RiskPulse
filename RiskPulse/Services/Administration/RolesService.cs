using Microsoft.EntityFrameworkCore;
using RiskPulse.Data;
using RiskPulse.Data.Entries;

namespace RiskPulse.Services.Administration;

public class RolesService
{
    private readonly AppDbContext _db;

    public RolesService(AppDbContext db)
    {
        _db = db;
    }

    // --- Reads (page dropdowns + grid) ---
    public async Task<List<Role>> GetAllRolesAsync()
    {
        return await _db.Roles
            .Include(r => r.DefaultPermission)
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

    // --- Role create/update ---
    public async Task<Role> CreateRoleAsync(string roleDesc, List<int> permissionIds, int? defaultPermissionId)
    {
        roleDesc = roleDesc.Trim();

        ValidateDefaultPermission(defaultPermissionId, permissionIds);

        var exists = await _db.Roles.AnyAsync(r => r.RoleDesc.ToLower() == roleDesc.ToLower());
        if (exists)
        {
            throw new InvalidOperationException($"Role name '{roleDesc}' already exists.");
        }

        var role = new Role
        {
            RoleDesc = roleDesc,
            DefaultPermissionId = defaultPermissionId,
            RolePermissions = permissionIds
                .Distinct()
                .Select(permissionId => new RolePermission { PermissionId = permissionId })
                .ToList()
        };

        _db.Roles.Add(role);
        await _db.SaveChangesAsync();
        return role;
    }

    public async Task<Role> UpdateRoleAsync(int roleId, string roleDesc, List<int> permissionIds, int? defaultPermissionId)
    {
        roleDesc = roleDesc.Trim();

        ValidateDefaultPermission(defaultPermissionId, permissionIds);

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
        existing.DefaultPermissionId = defaultPermissionId;
        existing.RolePermissions.Clear();

        foreach (var permissionId in permissionIds.Distinct())
        {
            existing.RolePermissions.Add(new RolePermission { PermissionId = permissionId });
        }

        await _db.SaveChangesAsync();
        return existing;
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
