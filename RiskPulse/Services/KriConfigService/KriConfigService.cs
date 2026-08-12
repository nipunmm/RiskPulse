using Microsoft.EntityFrameworkCore;
using RiskPulse.Data;
using RiskPulse.Models.DbModel.Kri;
using RiskPulse.Models.ViewModel;

namespace RiskPulse.Services.KriConfigService;

public class KriConfigService
{
    private readonly AppDbContext _db;

    public KriConfigService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<KriThresholdColor>> GetColorsAsync()
    {
        return await _db.KriThresholdColors
            .AsNoTracking()
            .OrderBy(c => c.ColorId)
            .ToListAsync();
    }

    public async Task<List<KriColorGridRow>> GetColorRowsAsync()
    {
        return await _db.KriThresholdColors
            .AsNoTracking()
            .OrderBy(c => c.ColorId)
            .Select(c => new KriColorGridRow
            {
                ColorId = c.ColorId,
                ColorDesc = c.ColorDesc,
                HexCode = c.HexCode
            })
            .ToListAsync();
    }

    public async Task<KriThresholdColor> SaveColorAsync(KriColorSaveModel model)
    {
        var desc = model.ColorDesc.Trim();
        var hex = model.HexCode.Trim().ToUpper();

        var exists = await _db.KriThresholdColors.AnyAsync(c =>
            c.ColorDesc.ToLower() == desc.ToLower() && c.ColorId != model.ColorId);
        if (exists)
        {
            throw new InvalidOperationException($"Color '{desc}' already exists.");
        }

        var hexExists = await _db.KriThresholdColors.AnyAsync(c =>
            c.HexCode.ToLower() == hex.ToLower() && c.ColorId != model.ColorId);
        if (hexExists)
        {
            throw new InvalidOperationException($"Color with hex code '{hex}' already exists.");
        }

        if (model.ColorId == 0)
        {
            var color = new KriThresholdColor { ColorDesc = desc, HexCode = hex };
            _db.KriThresholdColors.Add(color);
            await _db.SaveChangesAsync();
            return color;
        }

        var existing = await _db.KriThresholdColors.FindAsync(model.ColorId)
            ?? throw new InvalidOperationException($"Color with Id {model.ColorId} was not found.");

        existing.ColorDesc = desc;
        existing.HexCode = hex;

        await _db.SaveChangesAsync();
        return existing;
    }

    public async Task DeleteColorAsync(int colorId)
    {
        var color = await _db.KriThresholdColors.FindAsync(colorId)
            ?? throw new InvalidOperationException($"Color with Id {colorId} was not found.");

        var inUse = await _db.KriThresholds.AnyAsync(t => t.ColorId == colorId);
        if (inUse)
        {
            throw new InvalidOperationException("Cannot delete a color that is assigned to a threshold band.");
        }

        _db.KriThresholdColors.Remove(color);
        await _db.SaveChangesAsync();
    }

    public async Task<List<KriGroupGridRow>> GetGroupRowsAsync()
    {
        return await _db.KriThresholdGroups
            .AsNoTracking()
            .OrderBy(g => g.KriThresholdGroupId)
            .Select(g => new KriGroupGridRow
            {
                KriThresholdGroupId = g.KriThresholdGroupId,
                KriThresholdGroupDesc = g.KriThresholdGroupDesc,
                BandCount = g.KriThresholds.Count
            })
            .ToListAsync();
    }

    public async Task<KriThresholdGroup> SaveGroupAsync(KriThresholdGroupSaveModel model)
    {
        var desc = model.KriThresholdGroupDesc.Trim();

        var exists = await _db.KriThresholdGroups.AnyAsync(g =>
            g.KriThresholdGroupDesc.ToLower() == desc.ToLower() && g.KriThresholdGroupId != model.KriThresholdGroupId);
        if (exists)
        {
            throw new InvalidOperationException($"Group '{desc}' already exists.");
        }

        if (model.KriThresholdGroupId == 0)
        {
            var group = new KriThresholdGroup { KriThresholdGroupDesc = desc };
            _db.KriThresholdGroups.Add(group);
            await _db.SaveChangesAsync();
            return group;
        }

        var existing = await _db.KriThresholdGroups.FindAsync(model.KriThresholdGroupId)
            ?? throw new InvalidOperationException($"Group with Id {model.KriThresholdGroupId} was not found.");

        existing.KriThresholdGroupDesc = desc;

        await _db.SaveChangesAsync();
        return existing;
    }

    public async Task DeleteGroupAsync(int groupId)
    {
        var group = await _db.KriThresholdGroups.FindAsync(groupId)
            ?? throw new InvalidOperationException($"Group with Id {groupId} was not found.");

        var inUse = await _db.Kris.AnyAsync(k => k.KriThresholdGroupId == groupId);
        if (inUse)
        {
            throw new InvalidOperationException("Cannot delete a group that is assigned to a KRI.");
        }

        await _db.KriThresholds.Where(t => t.KriThresholdGroupId == groupId).ExecuteDeleteAsync();

        _db.KriThresholdGroups.Remove(group);
        await _db.SaveChangesAsync();
    }

    public async Task<List<KriBandGridRow>> GetBandsAsync(int groupId)
    {
        return await _db.KriThresholds
            .AsNoTracking()
            .Where(t => t.KriThresholdGroupId == groupId)
            .OrderBy(t => t.MinValue)
            .Select(t => new KriBandGridRow
            {
                KriThresholdId = t.KriThresholdId,
                ColorId = t.ColorId,
                ColorDesc = t.Color != null ? t.Color.ColorDesc : null,
                HexCode = t.Color != null ? t.Color.HexCode : null,
                MinValue = t.MinValue,
                MaxValue = t.MaxValue
            })
            .ToListAsync();
    }

    public async Task SaveBandsAsync(int groupId, List<KriBandSaveModel> bands)
    {
        var group = await _db.KriThresholdGroups.FindAsync(groupId)
            ?? throw new InvalidOperationException($"Group with Id {groupId} was not found.");

        var validBands = bands.Where(b => b.ColorId > 0).ToList();
        if (validBands.Count == 0)
        {
            throw new InvalidOperationException("At least one threshold band is required.");
        }

        foreach (var band in validBands)
        {
            var colorExists = await _db.KriThresholdColors.AnyAsync(c => c.ColorId == band.ColorId);
            if (!colorExists)
            {
                throw new InvalidOperationException("A selected color is not valid.");
            }

            if (band.MinValue > band.MaxValue)
            {
                throw new InvalidOperationException("Minimum value cannot be greater than maximum value.");
            }
        }

        await _db.KriThresholds.Where(t => t.KriThresholdGroupId == groupId).ExecuteDeleteAsync();

        foreach (var band in validBands)
        {
            _db.KriThresholds.Add(new KriThreshold
            {
                KriThresholdGroupId = groupId,
                ColorId = band.ColorId,
                MinValue = band.MinValue,
                MaxValue = band.MaxValue
            });
        }

        await _db.SaveChangesAsync();
    }
}
