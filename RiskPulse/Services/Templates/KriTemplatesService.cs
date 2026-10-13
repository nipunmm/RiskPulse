using Microsoft.EntityFrameworkCore;
using RiskPulse.Data;
using RiskPulse.Data.Entries;
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

    public async Task<SaveResultDto> SaveFullAsync(KriTemplateSaveDto model)
    {
        var headerDto = model.Header;
        var desc = headerDto.KriHeaderDesc?.Trim() ?? string.Empty;

        var hasGroup = headerDto.GroupId.HasValue && headerDto.GroupId.Value > 0;
        var hasUnit = headerDto.UnitId.HasValue && headerDto.UnitId.Value > 0;
        if (hasGroup == hasUnit)
        {
            throw new InvalidOperationException("Please select either a unit group or a unit, not both.");
        }

        if (hasGroup)
        {
            var groupExists = await _db.Groups.AnyAsync(g => g.GroupId == headerDto.GroupId!.Value);
            if (!groupExists)
            {
                throw new InvalidOperationException("Please select a valid unit group.");
            }
        }

        if (hasUnit)
        {
            var unitExists = await _db.Units.AnyAsync(u => u.UnitId == headerDto.UnitId!.Value);
            if (!unitExists)
            {
                throw new InvalidOperationException("Please select a valid unit.");
            }
        }

        // --- Validate the KRI set (final-state semantics) before mutating anything ---
        var kris = NormalizeKris(model.Kris);

        if (kris.Count == 0)
        {
            throw new InvalidOperationException("At least one KRI is required.");
        }

        if (headerDto.KriHeaderId == 0)
        {
            var code = await _codeService.GenerateKriCodeAsync();

            var header = new KriHeader
            {
                KriHeaderDesc = desc,
                GroupId = hasGroup ? headerDto.GroupId : null,
                UnitId = hasUnit ? headerDto.UnitId : null,
                KriStatus = headerDto.KriStatus,
                KriCode = code
            };

            _db.KriHeaders.Add(header);

            foreach (var kriDto in kris)
            {
                header.Kris.Add(BuildKri(kriDto));
            }

            await _db.SaveChangesAsync();
            return new SaveResultDto { Id = header.KriHeaderId };
        }

        var existing = await _db.KriHeaders
            .Include(h => h.Kris)
            .FirstOrDefaultAsync(h => h.KriHeaderId == headerDto.KriHeaderId)
            ?? throw new InvalidOperationException($"Template with Id {headerDto.KriHeaderId} was not found.");

        if (existing.KriStatus == KriStatus.Locked)
        {
            throw new InvalidOperationException("Cannot modify a locked template.");
        }

        // Reject stale kri ids (deleted by another session) instead of silently creating them.
        var byId = existing.Kris.ToDictionary(k => k.KriId);
        var incomingIds = kris.Where(k => k.KriId != 0).Select(k => k.KriId).ToHashSet();
        foreach (var kriId in incomingIds)
        {
            if (!byId.ContainsKey(kriId))
            {
                throw new InvalidOperationException($"KRI with Id {kriId} was not found.");
            }
        }

        existing.KriHeaderDesc = desc;
        existing.GroupId = hasGroup ? headerDto.GroupId : null;
        existing.UnitId = hasUnit ? headerDto.UnitId : null;
        existing.KriStatus = headerDto.KriStatus;

        // Remove persisted KRIs that are not part of the payload (mirror DeleteKriAsync).
        foreach (var kri in existing.Kris.Where(k => !incomingIds.Contains(k.KriId)).ToList())
        {
            _db.Kris.Remove(kri);
        }

        // Upsert KRIs in payload order.
        foreach (var kriDto in kris)
        {
            if (kriDto.KriId == 0)
            {
                existing.Kris.Add(BuildKri(kriDto));
                continue;
            }

            var kri = byId[kriDto.KriId];
            kri.KriDesc = kriDto.KriDesc;
            kri.AllowComment = kriDto.AllowComment;
            kri.GreenLimit = kriDto.GreenLimit;
            kri.AmberLimit = kriDto.AmberLimit;
            kri.RedLimit = kriDto.RedLimit;
        }

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

        await _db.AssessmentTemplateItems
            .Where(i => i.ItemType == AssessmentItemType.Kri && i.ItemId == kriHeaderId)
            .ExecuteDeleteAsync();

        _db.KriHeaders.Remove(header);
        await _db.SaveChangesAsync();
    }

    // --- KRI items (grid used to load a template into the designer / previews) ---
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

    // --- KRI normalization + validation (pending payload is the source of truth) ---
    private static List<KriSaveDto> NormalizeKris(List<KriSaveDto>? kris)
    {
        var incoming = kris ?? new List<KriSaveDto>();
        var normalized = new List<KriSaveDto>();

        foreach (var kriDto in incoming)
        {
            var desc = kriDto.KriDesc?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(desc))
            {
                throw new InvalidOperationException("KRI description is required.");
            }

            if (kriDto.GreenLimit > kriDto.AmberLimit)
            {
                throw new InvalidOperationException("Green limit must not exceed amber limit.");
            }

            if (kriDto.AmberLimit > kriDto.RedLimit)
            {
                throw new InvalidOperationException("Amber limit must not exceed red limit.");
            }

            normalized.Add(new KriSaveDto
            {
                KriId = kriDto.KriId,
                KriDesc = desc,
                AllowComment = kriDto.AllowComment,
                GreenLimit = kriDto.GreenLimit,
                AmberLimit = kriDto.AmberLimit,
                RedLimit = kriDto.RedLimit
            });
        }

        // Duplicate description check against the effective final state.
        // A DB-only check would be wrong here: KRIs absent from the payload are
        // deleted by the same atomic save, so a persisted row can collide with an
        // incoming one without the final template holding a duplicate.
        for (var i = 0; i < normalized.Count; i++)
        {
            for (var j = i + 1; j < normalized.Count; j++)
            {
                if (string.Equals(normalized[i].KriDesc, normalized[j].KriDesc, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException($"KRI description '{normalized[i].KriDesc}' already exists.");
                }
            }
        }

        return normalized;
    }

    private static Kri BuildKri(KriSaveDto kriDto)
    {
        return new Kri
        {
            KriDesc = kriDto.KriDesc,
            AllowComment = kriDto.AllowComment,
            GreenLimit = kriDto.GreenLimit,
            AmberLimit = kriDto.AmberLimit,
            RedLimit = kriDto.RedLimit
        };
    }
}