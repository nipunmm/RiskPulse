using Microsoft.EntityFrameworkCore;
using RiskPulse.Data;
using RiskPulse.Data.Entries;
using RiskPulse.Data.Extensions;
using RiskPulse.Models.Dto;
using RiskPulse.Models.ViewModel;

namespace RiskPulse.Services.Administration;

public class UsersService
{
    private readonly AppDbContext _db;

    public UsersService(AppDbContext db)
    {
        _db = db;
    }

    // --- Reads (page dropdowns + grid) ---
    public async Task<List<UserGridRowViewModel>> GetGridRowsAsync()
    {
        return await _db.Users
            .Include(u => u.Unit)
            .Include(u => u.Role)
            .AsNoTracking()
            .OrderBy(u => u.Id)
            .Select(u => new UserGridRowViewModel
            {
                Id = u.Id,
                Username = u.Username,
                UnitId = u.UnitId,
                UnitDesc = u.Unit != null ? u.Unit.UnitDesc : string.Empty,
                RoleId = u.RoleId,
                RoleDesc = u.Role != null ? u.Role.RoleDesc : string.Empty,
                IsActive = u.IsActive
            })
            .ToListAsync();
    }

    public async Task<List<OptionViewModel>> GetAllRolesAsync()
    {
        return await _db.Roles.AsNoTracking()
            .OrderBy(r => r.RoleDesc)
            .ToOptionListAsync(r => r.RoleId, r => r.RoleDesc);
    }

    // --- User create/update ---
    public async Task<SaveResultDto> CreateUserAsync(UserSaveDto model)
    {
        await _db.Users.EnsureUniqueAsync(u => u.Username.ToLower() == model.Username.ToLower(), "Username", model.Username);

        var user = new User
        {
            Username = model.Username,
            IsActive = model.IsActive,
            UnitId = model.UnitId,
            RoleId = model.RoleId
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return new SaveResultDto { Id = user.Id };
    }

    public async Task<SaveResultDto> UpdateUserAsync(UserSaveDto model, int actingUserId)
    {
        if (model.Id == actingUserId)
        {
            throw new InvalidOperationException("You cannot edit your own user record.");
        }

        var existing = await _db.Users.FindAsync(model.Id)
            ?? throw new InvalidOperationException($"User with Id {model.Id} was not found.");

        await _db.Users.EnsureUniqueAsync(u => u.Username.ToLower() == model.Username.ToLower() && u.Id != model.Id, "Username", model.Username);

        existing.Username = model.Username;
        existing.IsActive = model.IsActive;
        existing.UnitId = model.UnitId;
        existing.RoleId = model.RoleId;

        await _db.SaveChangesAsync();
        return new SaveResultDto { Id = existing.Id };
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
