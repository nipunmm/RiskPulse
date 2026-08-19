using Microsoft.EntityFrameworkCore;
using RiskPulse.Data;
using RiskPulse.Models.Dto;

namespace RiskPulse.Services.Login;

// --- User authorization lookup (role/permissions/unit) ---
public class DbAuthorizationService
{
    private readonly AppDbContext _db;

    public DbAuthorizationService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<UserAuthorizationDto?> GetUserDetailsAsync(string username)
    {
        return await _db.Users
            .Where(u => u.Username == username)
            .AsNoTracking()
            .Select(u => new UserAuthorizationDto
            {
                UserId = u.Id,
                Username = u.Username,
                IsActive = u.IsActive,
                RoleDesc = u.Role != null ? u.Role.RoleDesc : null,
                UnitDesc = u.Unit != null ? u.Unit.UnitDesc : null,
                DefaultPermissionDesc = u.Role != null && u.Role.DefaultPermission != null
                    ? u.Role.DefaultPermission.PermissionDesc
                    : null,
                PermissionDescs = u.Role != null
                    ? u.Role.RolePermissions
                        .Where(rp => rp.Permission != null)
                        .Select(rp => rp.Permission!.PermissionDesc)
                        .ToList()
                    : new List<string>()
            })
            .FirstOrDefaultAsync();
    }
}
