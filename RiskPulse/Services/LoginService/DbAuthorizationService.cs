using Microsoft.EntityFrameworkCore;
using RiskPulse.Data;
using RiskPulse.Models.DbModel.AccessControl;

namespace RiskPulse.Services.LoginService;

// Loads a user's authorization details (role, permissions, unit) from the database.
// The result is used at login time to build the claims stored in the auth cookie.
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
            .Include(u => u.Unit)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Username == username);
    }
}
