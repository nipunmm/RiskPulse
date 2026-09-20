using Microsoft.EntityFrameworkCore;
using RiskPulse.Data;
using RiskPulse.Data.Entries;
using RiskPulse.Data.Extensions;
using RiskPulse.Models.Dto;
using RiskPulse.Models.Enum;
using RiskPulse.Models.ViewModel;
using RiskPulse.Services.Utilities;

namespace RiskPulse.Services.Templates;

public class KriTemplatesService
{
    private readonly AppDbContext _db;
    private readonly CodeGeneratorService _codeService;

    public KriTemplatesService(AppDbContext db, CodeGeneratorService codeService)
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

        await _db.ScheduleItems
            .Where(si => si.ItemType == ScheduleItemType.Kri && si.ItemId == kriHeaderId)
            .ExecuteDeleteAsync();

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
                GreenLimit = k.GreenLimit,
                AmberLimit = k.AmberLimit,
                RedLimit = k.RedLimit
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

        if (model.GreenLimit > model.AmberLimit)
        {
            throw new InvalidOperationException("Green limit must not exceed amber limit.");
        }

        if (model.AmberLimit > model.RedLimit)
        {
            throw new InvalidOperationException("Amber limit must not exceed red limit.");
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
                GreenLimit = model.GreenLimit,
                AmberLimit = model.AmberLimit,
                RedLimit = model.RedLimit
            };

            _db.Kris.Add(kri);
            await _db.SaveChangesAsync();
            return new SaveResultDto { Id = kri.KriId };
        }

        var existing = await _db.Kris.FindAsync(model.KriId)
            ?? throw new InvalidOperationException($"KRI with Id {model.KriId} was not found.");

        existing.KriDesc = desc;
        existing.AllowComment = model.AllowComment;
        existing.GreenLimit = model.GreenLimit;
        existing.AmberLimit = model.AmberLimit;
        existing.RedLimit = model.RedLimit;

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

    // --- Preview ---
    public async Task<KriPreviewViewModel> GetKriPreviewAsync(int kriHeaderId)
    {
        var header = await _db.KriHeaders
            .AsNoTracking()
            .Where(h => h.KriHeaderId == kriHeaderId)
            .Select(h => new
            {
                h.KriHeaderId,
                h.KriCode,
                h.KriHeaderDesc,
                h.KriStatus,
                AssignmentLabel = h.Group != null ? h.Group.GroupDesc : (h.Unit != null ? h.Unit.UnitDesc : string.Empty)
            })
            .SingleOrDefaultAsync()
            ?? throw new InvalidOperationException($"Template with Id {kriHeaderId} was not found.");

        return new KriPreviewViewModel
        {
            KriHeaderId = header.KriHeaderId,
            KriCode = header.KriCode ?? string.Empty,
            KriHeaderDesc = header.KriHeaderDesc,
            KriStatus = header.KriStatus.ToString(),
            AssignmentLabel = header.AssignmentLabel,
            Kris = await GetKrisAsync(kriHeaderId)
        };
    }
}
