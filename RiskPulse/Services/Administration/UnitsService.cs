using Microsoft.EntityFrameworkCore;
using RiskPulse.Data;
using RiskPulse.Data.Entries;
using RiskPulse.Models.Dto;
using RiskPulse.Models.ViewModel;

namespace RiskPulse.Services.Administration;

public class UnitsService
{
    private readonly AppDbContext _db;

    public UnitsService(AppDbContext db)
    {
        _db = db;
    }

    // --- Units (grid/save/delete) ---
    public async Task<List<UnitGridRowViewModel>> GetGridRowsAsync()
    {
        return await _db.Units
            .AsNoTracking()
            .OrderBy(u => u.UnitId)
            .Select(u => new UnitGridRowViewModel
            {
                UnitId = u.UnitId,
                UnitCode = u.UnitCode,
                UnitType = u.UnitType.ToString(),
                UnitDesc = u.UnitDesc
            })
            .ToListAsync();
    }

    public async Task<Unit> SaveUnitAsync(UnitSaveDto model)
    {
        var code = model.UnitCode.Trim();
        var desc = model.UnitDesc.Trim();

        var exists = await _db.Units.AnyAsync(u =>
            u.UnitCode.ToLower() == code.ToLower() && u.UnitId != model.UnitId);
        if (exists)
        {
            throw new InvalidOperationException($"Unit code '{code}' already exists.");
        }

        if (model.UnitId == 0)
        {
            var unit = new Unit
            {
                UnitCode = code,
                UnitType = model.UnitType,
                UnitDesc = desc
            };

            _db.Units.Add(unit);
            await _db.SaveChangesAsync();
            return unit;
        }

        var existing = await _db.Units.FindAsync(model.UnitId)
            ?? throw new InvalidOperationException($"Unit with Id {model.UnitId} was not found.");

        existing.UnitCode = code;
        existing.UnitType = model.UnitType;
        existing.UnitDesc = desc;

        await _db.SaveChangesAsync();
        return existing;
    }

    public async Task DeleteUnitAsync(int unitId)
    {
        var unit = await _db.Units.FindAsync(unitId)
            ?? throw new InvalidOperationException($"Unit with Id {unitId} was not found.");

        var inUseByUsers = await _db.Users.AnyAsync(u => u.UnitId == unitId);
        if (inUseByUsers)
        {
            throw new InvalidOperationException("Cannot delete a unit that is assigned to one or more users.");
        }
        _db.Units.Remove(unit);
        await _db.SaveChangesAsync();
    }

    // --- Unit groups (grid/save/delete) ---
    public async Task<List<UnitGroupOptionViewModel>> GetUnitGroupOptionsAsync()
    {
        return await _db.Groups
            .AsNoTracking()
            .OrderBy(g => g.GroupId)
            .Select(g => new UnitGroupOptionViewModel
            {
                Value = g.GroupId,
                Label = g.GroupDesc
            })
            .ToListAsync();
    }

    public async Task<List<GroupGridRowViewModel>> GetGroupGridRowsAsync()
    {
        return await _db.Groups
            .AsNoTracking()
            .OrderBy(g => g.GroupId)
            .Select(g => new GroupGridRowViewModel
            {
                GroupId = g.GroupId,
                GroupDesc = g.GroupDesc,
                UnitCount = g.UnitGroups.Count,
                UnitIds = g.UnitGroups.Select(ug => ug.UnitId).ToList(),
                UnitDescs = g.UnitGroups
                    .Select(ug => ug.Unit!.UnitDesc)
                    .ToList()
            })
            .ToListAsync();
    }

    public async Task<List<Unit>> GetAllUnitsAsync()
    {
        return await _db.Units
            .AsNoTracking()
            .OrderBy(u => u.UnitId)
            .ToListAsync();
    }

    public async Task<Group> SaveGroupAsync(GroupSaveDto model)
    {
        var desc = model.GroupDesc.Trim();

        var exists = await _db.Groups.AnyAsync(g =>
            g.GroupDesc.ToLower() == desc.ToLower() && g.GroupId != model.GroupId);
        if (exists)
        {
            throw new InvalidOperationException($"Group '{desc}' already exists.");
        }

        if (model.GroupId == 0)
        {
            var group = new Group
            {
                GroupDesc = desc,
                UnitGroups = model.UnitIds
                    .Distinct()
                    .Select(unitId => new UnitGroup { UnitId = unitId })
                    .ToList()
            };

            _db.Groups.Add(group);
            await _db.SaveChangesAsync();
            return group;
        }

        var existing = await _db.Groups
            .Include(g => g.UnitGroups)
            .FirstOrDefaultAsync(g => g.GroupId == model.GroupId)
            ?? throw new InvalidOperationException($"Group with Id {model.GroupId} was not found.");

        existing.GroupDesc = desc;
        existing.UnitGroups.Clear();

        foreach (var unitId in model.UnitIds.Distinct())
        {
            existing.UnitGroups.Add(new UnitGroup { UnitId = unitId });
        }

        await _db.SaveChangesAsync();
        return existing;
    }

    public async Task DeleteGroupAsync(int groupId)
    {
        var group = await _db.Groups.FindAsync(groupId)
            ?? throw new InvalidOperationException($"Group with Id {groupId} was not found.");

        var linkedToTemplate = await _db.SaqHeaders.AnyAsync(h => h.GroupId == groupId) ||
                               await _db.KriHeaders.AnyAsync(h => h.GroupId == groupId);
        if (linkedToTemplate)
        {
            throw new InvalidOperationException("Cannot delete a group that is linked to a template.");
        }

        _db.Groups.Remove(group);
        await _db.SaveChangesAsync();
    }
}
