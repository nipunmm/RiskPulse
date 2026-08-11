using Microsoft.EntityFrameworkCore;
using RiskPulse.Data;
using RiskPulse.Models.DbModel.AccessControl;
using RiskPulse.Models.ViewModel;

namespace RiskPulse.Services.AccessControlService;

public class UsersService
{
    private readonly AppDbContext _db;

    public UsersService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<User>> GetAllAsync()
    {
        return await _db.Users
            .Include(u => u.Unit)
            .Include(u => u.Role)
            .AsNoTracking()
            .OrderBy(u => u.Id)
            .ToListAsync();
    }

    public async Task<List<Unit>> GetAllUnitsAsync()
    {
        return await _db.Units
            .AsNoTracking()
            .OrderBy(u => u.UnitId)
            .ToListAsync();
    }

    public async Task<List<Role>> GetAllRolesAsync()
    {
        return await _db.Roles
            .AsNoTracking()
            .OrderBy(r => r.RoleId)
            .ToListAsync();
    }

    public async Task<User> CreateUserAsync(UserSaveModel model)
    {
        var exists = await _db.Users.AnyAsync(u => u.Username == model.Username);
        if (exists)
        {
            throw new InvalidOperationException($"Username '{model.Username}' already exists.");
        }

        var user = new User
        {
            Username = model.Username,
            IsActive = model.IsActive,
            UnitId = model.UnitId,
            RoleId = model.RoleId
        };

        if (user.UnitId == 0)
        {
            user.UnitId = await GetDefaultUnitIdAsync();
        }

        if (user.RoleId == 0)
        {
            user.RoleId = await GetDefaultRoleIdAsync();
        }

        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return user;
    }

    public async Task<User> UpdateUserAsync(UserSaveModel model, int actingUserId)
    {
        if (model.Id == actingUserId)
        {
            throw new InvalidOperationException("You cannot edit your own user record.");
        }

        var existing = await _db.Users.FindAsync(model.Id)
            ?? throw new InvalidOperationException($"User with Id {model.Id} was not found.");

        existing.IsActive = model.IsActive;
        existing.UnitId = model.UnitId;
        existing.RoleId = model.RoleId;

        await _db.SaveChangesAsync();
        return existing;
    }

    private async Task<int> GetDefaultUnitIdAsync()
    {
        var id = await _db.Units.Select(u => (int?)u.UnitId).FirstOrDefaultAsync() ?? 0;
        if (id == 0)
        {
            throw new InvalidOperationException("No units exist. Create a unit before adding users.");
        }
        return id;
    }

    private async Task<int> GetDefaultRoleIdAsync()
    {
        var id = await _db.Roles.Select(r => (int?)r.RoleId).FirstOrDefaultAsync() ?? 0;
        if (id == 0)
        {
            throw new InvalidOperationException("No roles exist. Create a role before adding users.");
        }
        return id;
    }
}
