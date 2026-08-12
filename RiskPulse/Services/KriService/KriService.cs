using Microsoft.EntityFrameworkCore;
using RiskPulse.Data;
using RiskPulse.Models.DbModel.Kri;
using RiskPulse.Models.ViewModel;

namespace RiskPulse.Services.KriService;

public class KriService
{
    private readonly AppDbContext _db;

    public KriService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<KriGridRow>> GetHeaderRowsAsync()
    {
        return await _db.KriHeaders
            .AsNoTracking()
            .OrderBy(h => h.KriHeaderId)
            .Select(h => new KriGridRow
            {
                KriHeaderId = h.KriHeaderId,
                KriHeaderDesc = h.KriHeaderDesc,
                KriStatus = h.KriStatus.ToString(),
                KriCount = h.Kris.Count
            })
            .ToListAsync();
    }

    public async Task<List<KriItemGridRow>> GetKrisAsync(int kriHeaderId)
    {
        return await _db.Kris
            .AsNoTracking()
            .Where(k => k.KriHeaderId == kriHeaderId)
            .OrderBy(k => k.KriId)
            .Select(k => new KriItemGridRow
            {
                KriId = k.KriId,
                KriDesc = k.KriDesc,
                AllowComment = k.AllowComment,
                KriThresholdGroupId = k.KriThresholdGroupId,
                KriThresholdGroupDesc = k.KriThresholdGroup != null ? k.KriThresholdGroup.KriThresholdGroupDesc : null
            })
            .ToListAsync();
    }

    public async Task<List<KriThresholdGroup>> GetThresholdGroupsAsync()
    {
        return await _db.KriThresholdGroups
            .AsNoTracking()
            .OrderBy(g => g.KriThresholdGroupId)
            .ToListAsync();
    }

    public async Task<KriHeader> SaveHeaderAsync(KriHeaderSaveModel model)
    {
        var desc = model.KriHeaderDesc.Trim();

        var exists = await _db.KriHeaders.AnyAsync(h =>
            h.KriHeaderDesc.ToLower() == desc.ToLower() && h.KriHeaderId != model.KriHeaderId);
        if (exists)
        {
            throw new InvalidOperationException($"Template '{desc}' already exists.");
        }

        if (model.KriHeaderId == 0)
        {
            var header = new KriHeader
            {
                KriHeaderDesc = desc,
                KriStatus = model.KriStatus
            };

            _db.KriHeaders.Add(header);
            await _db.SaveChangesAsync();
            return header;
        }

        var existing = await _db.KriHeaders.FindAsync(model.KriHeaderId)
            ?? throw new InvalidOperationException($"Template with Id {model.KriHeaderId} was not found.");

        if (existing.KriStatus == KriStatus.Locked)
        {
            throw new InvalidOperationException("Cannot modify a locked template.");
        }

        existing.KriHeaderDesc = desc;
        existing.KriStatus = model.KriStatus;

        await _db.SaveChangesAsync();
        return existing;
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

    public async Task<Kri> SaveKriAsync(KriSaveModel model)
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

        var duplicate = await _db.Kris.AnyAsync(k =>
            k.KriHeaderId == model.KriHeaderId && k.KriId != model.KriId && k.KriDesc.ToLower() == desc.ToLower());
        if (duplicate)
        {
            throw new InvalidOperationException("The same KRI description already exists in this template.");
        }

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
            return kri;
        }

        var existing = await _db.Kris.FindAsync(model.KriId)
            ?? throw new InvalidOperationException($"KRI with Id {model.KriId} was not found.");

        existing.KriDesc = desc;
        existing.AllowComment = model.AllowComment;
        existing.KriThresholdGroupId = model.KriThresholdGroupId;

        await _db.SaveChangesAsync();
        return existing;
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
}
