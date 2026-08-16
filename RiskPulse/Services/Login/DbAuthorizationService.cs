using Microsoft.EntityFrameworkCore;
using RiskPulse.Data;
using RiskPulse.Data.Entries;

namespace RiskPulse.Services.Login;

// --- User authorization lookup (role/permissions/unit) ---
public class DbAuthorizationService
{
    private readonly AppDbContext _db;

    public DbAuthorizationService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<User?> GetUserDetailsAsync(string username)
    {
        return await _db.Users
            .Include(u => u.Role)
                .ThenInclude(r => r!.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .Include(u => u.Role)
                .ThenInclude(r => r!.DefaultPermission)
            .Include(u => u.Unit)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Username == username);
    }
}
