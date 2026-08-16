using Microsoft.EntityFrameworkCore;
using RiskPulse.Data;
using RiskPulse.Data.Entries;
using RiskPulse.Models.Dto;

namespace RiskPulse.Services.Administration;

public class UsersService
{
    private readonly AppDbContext _db;

    public UsersService(AppDbContext db)
    {
        _db = db;
    }

    // --- Reads (page dropdowns + grid) ---
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

    // --- User create/update ---
    public async Task<User> CreateUserAsync(UserSaveDto model)
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

        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return user;
    }

    public async Task<User> UpdateUserAsync(UserSaveDto model, int actingUserId)
    {
        if (model.Id == actingUserId)
        {
            throw new InvalidOperationException("You cannot edit your own user record.");
        }

        var existing = await _db.Users.FindAsync(model.Id)
            ?? throw new InvalidOperationException($"User with Id {model.Id} was not found.");

        var duplicate = await _db.Users.AnyAsync(u =>
            u.Username == model.Username && u.Id != model.Id);
        if (duplicate)
        {
            throw new InvalidOperationException($"Username '{model.Username}' already exists.");
        }

        existing.Username = model.Username;
        existing.IsActive = model.IsActive;
        existing.UnitId = model.UnitId;
        existing.RoleId = model.RoleId;

        await _db.SaveChangesAsync();
        return existing;
    }

    // --- User delete ---
    public async Task DeleteUserAsync(int id, int actingUserId)
    {
        if (id == actingUserId)
        {
            throw new InvalidOperationException("You cannot delete your own user record.");
        }

        var user = await _db.Users.FindAsync(id)
            ?? throw new InvalidOperationException($"User with Id {id} was not found.");

        _db.Users.Remove(user);
        await _db.SaveChangesAsync();
    }
}
