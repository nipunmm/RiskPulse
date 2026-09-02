using Microsoft.EntityFrameworkCore;
using RiskPulse.Data;
using RiskPulse.Data.Entries;
using RiskPulse.Data.Extensions;
using RiskPulse.Models.Dto;
using RiskPulse.Models.Enum;
using RiskPulse.Models.ViewModel;

namespace RiskPulse.Services.Templates;

public class KriTemplatesService
{
    private readonly AppDbContext _db;
    private readonly TemplateCodeService _codeService;

    public KriTemplatesService(AppDbContext db, TemplateCodeService codeService)
    {
        _db = db;
        _codeService = codeService;
    }

    // --- KRI template headers (grid/save/delete) ---
    public async Task<List<KriGridRowViewModel>> GetHeaderRowsAsync()
    {
        return await _db.KriHeaders
            .AsNoTracking()
            .OrderByDescending(h => h.KriHeaderId)
            .Select(h => new KriGridRowViewModel
            {
                KriHeaderId = h.KriHeaderId,
                KriCode = h.KriCode ?? string.Empty,
                KriHeaderDesc = h.KriHeaderDesc,
                GroupId = h.GroupId,
                GroupDesc = h.Group != null ? h.Group.GroupDesc : string.Empty,
                UnitId = h.UnitId,
                UnitDesc = h.Unit != null ? h.Unit.UnitDesc : string.Empty,
                KriStatus = h.KriStatus.ToString(),
                KriCount = h.Kris.Count
            })
            .ToListAsync();
    }

    public async Task<SaveResultDto> SaveHeaderAsync(KriHeaderSaveDto model)
    {
        var desc = model.KriHeaderDesc?.Trim() ?? string.Empty;

        var hasGroup = model.GroupId.HasValue && model.GroupId.Value > 0;
        var hasUnit = model.UnitId.HasValue && model.UnitId.Value > 0;
        if (hasGroup == hasUnit)
        {
            throw new InvalidOperationException("Please select either a unit group or a unit, not both.");
        }

        if (hasGroup)
        {
            var groupExists = await _db.Groups.AnyAsync(g => g.GroupId == model.GroupId!.Value);
            if (!groupExists)
            {
                throw new InvalidOperationException("Please select a valid unit group.");
            }
        }

        if (hasUnit)
        {
            var unitExists = await _db.Units.AnyAsync(u => u.UnitId == model.UnitId!.Value);
            if (!unitExists)
            {
                throw new InvalidOperationException("Please select a valid unit.");
            }
        }

        if (model.KriHeaderId == 0)
        {
            var code = await _codeService.GenerateKriCodeAsync();

            var header = new KriHeader
            {
                KriHeaderDesc = desc,
                GroupId = hasGroup ? model.GroupId : null,
                UnitId = hasUnit ? model.UnitId : null,
                KriStatus = model.KriStatus,
                KriCode = code
            };

            _db.KriHeaders.Add(header);
            await _db.SaveChangesAsync();
            return new SaveResultDto { Id = header.KriHeaderId };
        }

        var existing = await _db.KriHeaders.FindAsync(model.KriHeaderId)
            ?? throw new InvalidOperationException($"Template with Id {model.KriHeaderId} was not found.");

        if (existing.KriStatus == KriStatus.Locked)
        {
            throw new InvalidOperationException("Cannot modify a locked template.");
        }

        existing.KriHeaderDesc = desc;
        existing.GroupId = hasGroup ? model.GroupId : null;
        existing.UnitId = hasUnit ? model.UnitId : null;
        existing.KriStatus = model.KriStatus;

        await _db.SaveChangesAsync();
        return new SaveResultDto { Id = existing.KriHeaderId };
    }

    public async Task DeleteHeaderAsync(int kriHeaderId)
    {
        var header = await _db.KriHeaders.FindAsync(kriHeaderId)
            ?? throw new InvalidOperationException($"Template with Id {kriHeaderId} was not found.");

        if (header.KriStatus == KriStatus.Locked)
        {
            throw new InvalidOperationException("Cannot delete a locked template.");
        }

        await _db.Kris.Where(k => k.KriHeaderId == kriHeaderId).ExecuteDeleteAsync();

        _db.KriHeaders.Remove(header);
        await _db.SaveChangesAsync();
    }

    // --- KRI items (grid/save/delete) ---
    public async Task<List<KriItemGridRowViewModel>> GetKrisAsync(int kriHeaderId)
    {
        return await _db.Kris
            .AsNoTracking()
            .Where(k => k.KriHeaderId == kriHeaderId)
            .OrderBy(k => k.KriId)
            .Select(k => new KriItemGridRowViewModel
            {
                KriId = k.KriId,
                KriDesc = k.KriDesc,
                AllowComment = k.AllowComment,
                KriThresholdGroupId = k.KriThresholdGroupId,
                KriThresholdGroupDesc = k.KriThresholdGroup != null ? k.KriThresholdGroup.KriThresholdGroupDesc : null
            })
            .ToListAsync();
    }

    public async Task<SaveResultDto> SaveKriAsync(KriSaveDto model)
    {
        var header = await _db.KriHeaders.FindAsync(model.KriHeaderId)
            ?? throw new InvalidOperationException($"Template with Id {model.KriHeaderId} was not found.");

        if (header.KriStatus == KriStatus.Locked)
        {
            throw new InvalidOperationException("Cannot modify a locked template.");
        }

        var groupExists = await _db.KriThresholdGroups.AnyAsync(g => g.KriThresholdGroupId == model.KriThresholdGroupId);
        if (!groupExists)
        {
            throw new InvalidOperationException("Please select a valid threshold group.");
        }

        var desc = model.KriDesc.Trim();

        await _db.Kris.EnsureUniqueAsync(k => k.KriHeaderId == model.KriHeaderId && k.KriId != model.KriId && k.KriDesc.ToLower() == desc.ToLower(), "KRI description", desc);

        if (model.KriId == 0)
        {
            var kri = new Kri
            {
                KriHeaderId = model.KriHeaderId,
                KriDesc = desc,
                AllowComment = model.AllowComment,
                KriThresholdGroupId = model.KriThresholdGroupId
            };

            _db.Kris.Add(kri);
            await _db.SaveChangesAsync();
            return new SaveResultDto { Id = kri.KriId };
        }

        var existing = await _db.Kris.FindAsync(model.KriId)
            ?? throw new InvalidOperationException($"KRI with Id {model.KriId} was not found.");

        existing.KriDesc = desc;
        existing.AllowComment = model.AllowComment;
        existing.KriThresholdGroupId = model.KriThresholdGroupId;

        await _db.SaveChangesAsync();
        return new SaveResultDto { Id = existing.KriId };
    }

    public async Task DeleteKriAsync(int kriId)
    {
        var kri = await _db.Kris.FindAsync(kriId)
            ?? throw new InvalidOperationException($"KRI with Id {kriId} was not found.");

        var header = await _db.KriHeaders.FindAsync(kri.KriHeaderId);
        if (header?.KriStatus == KriStatus.Locked)
        {
            throw new InvalidOperationException("Cannot modify a locked template.");
        }

        _db.Kris.Remove(kri);
        await _db.SaveChangesAsync();
    }

    // --- Threshold configuration (colors/groups/bands) ---
    public async Task<List<OptionViewModel>> GetThresholdGroupsAsync()
    {
        return await _db.KriThresholdGroups.AsNoTracking()
            .OrderBy(g => g.KriThresholdGroupId)
            .ToOptionListAsync(g => g.KriThresholdGroupId, g => g.KriThresholdGroupDesc);
    }

    public async Task<List<KriGroupGridRowViewModel>> GetGroupRowsAsync()
    {
        return await _db.KriThresholdGroups
            .AsNoTracking()
            .OrderBy(g => g.KriThresholdGroupId)
            .Select(g => new KriGroupGridRowViewModel
            {
                KriThresholdGroupId = g.KriThresholdGroupId,
                KriThresholdGroupDesc = g.KriThresholdGroupDesc,
                BandCount = g.KriThresholds.Count
            })
            .ToListAsync();
    }

    public async Task<SaveResultDto> SaveGroupAsync(KriThresholdGroupSaveDto model)
    {
        var desc = model.KriThresholdGroupDesc.Trim();

        await _db.KriThresholdGroups.EnsureUniqueAsync(g => g.KriThresholdGroupDesc.ToLower() == desc.ToLower() && g.KriThresholdGroupId != model.KriThresholdGroupId, "Group", desc);

        if (model.KriThresholdGroupId == 0)
        {
            var group = new KriThresholdGroup { KriThresholdGroupDesc = desc };
            _db.KriThresholdGroups.Add(group);
            await _db.SaveChangesAsync();
            return new SaveResultDto { Id = group.KriThresholdGroupId };
        }

        var existing = await _db.KriThresholdGroups.FindAsync(model.KriThresholdGroupId)
            ?? throw new InvalidOperationException($"Group with Id {model.KriThresholdGroupId} was not found.");

        existing.KriThresholdGroupDesc = desc;

        await _db.SaveChangesAsync();
        return new SaveResultDto { Id = existing.KriThresholdGroupId };
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

        _db.KriThresholdGroups.Remove(group);
        await _db.SaveChangesAsync();
    }

    public async Task<List<KriColorOptionViewModel>> GetColorsAsync()
    {
        return await _db.KriThresholdColors
            .AsNoTracking()
            .OrderBy(c => c.ColorId)
            .Select(c => new KriColorOptionViewModel
            {
                ColorId = c.ColorId,
                ColorDesc = c.ColorDesc,
                HexCode = c.HexCode
            })
            .ToListAsync();
    }

    public async Task<List<KriColorGridRowViewModel>> GetColorRowsAsync()
    {
        return await _db.KriThresholdColors
            .AsNoTracking()
            .OrderBy(c => c.ColorId)
            .Select(c => new KriColorGridRowViewModel
            {
                ColorId = c.ColorId,
                ColorDesc = c.ColorDesc,
                HexCode = c.HexCode
            })
            .ToListAsync();
    }

    public async Task<SaveResultDto> SaveColorAsync(KriColorSaveDto model)
    {
        var desc = model.ColorDesc.Trim();
        var hex = model.HexCode.Trim().ToUpper();

        await _db.KriThresholdColors.EnsureUniqueAsync(c => c.ColorDesc.ToLower() == desc.ToLower() && c.ColorId != model.ColorId, "Color", desc);
        await _db.KriThresholdColors.EnsureUniqueAsync(c => c.HexCode.ToLower() == hex.ToLower() && c.ColorId != model.ColorId, "Color", hex);

        if (model.ColorId == 0)
        {
            var color = new KriThresholdColor { ColorDesc = desc, HexCode = hex };
            _db.KriThresholdColors.Add(color);
            await _db.SaveChangesAsync();
            return new SaveResultDto { Id = color.ColorId };
        }

        var existing = await _db.KriThresholdColors.FindAsync(model.ColorId)
            ?? throw new InvalidOperationException($"Color with Id {model.ColorId} was not found.");

        existing.ColorDesc = desc;
        existing.HexCode = hex;

        await _db.SaveChangesAsync();
        return new SaveResultDto { Id = existing.ColorId };
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

    public async Task<List<KriBandGridRowViewModel>> GetBandsAsync(int groupId)
    {
        return await _db.KriThresholds
            .AsNoTracking()
            .Where(t => t.KriThresholdGroupId == groupId)
            .OrderBy(t => t.MinValue)
            .Select(t => new KriBandGridRowViewModel
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

    public async Task SaveBandsAsync(int groupId, List<KriBandSaveDto> bands)
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
