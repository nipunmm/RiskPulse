using Microsoft.EntityFrameworkCore;
using RiskPulse.Data;
using RiskPulse.Data.Entries;
using RiskPulse.Models.Dto;
using RiskPulse.Models.Enum;
using RiskPulse.Models.ViewModel;
using RiskPulse.Services.Utilities;

namespace RiskPulse.Services.Templates;

public class AssessmentTemplatesService
{
    private readonly AppDbContext _db;
    private readonly CodeGeneratorService _codeService;

    public AssessmentTemplatesService(AppDbContext db, CodeGeneratorService codeService)
    {
        _db = db;
        _codeService = codeService;
    }

    // --- Assessment template headers (grid/save/delete) ---
    public async Task<List<AssessmentTemplateGridRowViewModel>> GetHeaderRowsAsync()
    {
        var headers = await _db.AssessmentTemplates
            .AsNoTracking()
            .Include(t => t.AssessmentTemplateItems)
            .OrderByDescending(t => t.AssessmentTemplateId)
            .ToListAsync();

        var saqCodes = await _db.SaqHeaders.AsNoTracking()
            .Where(h => h.SaqCode != null)
            .ToDictionaryAsync(h => h.SaqHeaderId, h => h.SaqCode!);

        var kriCodes = await _db.KriHeaders.AsNoTracking()
            .Where(h => h.KriCode != null)
            .ToDictionaryAsync(h => h.KriHeaderId, h => h.KriCode!);

        return headers.Select(t =>
        {
            var saqItems = t.AssessmentTemplateItems.Where(i => i.ItemType == AssessmentItemType.Saq).ToList();
            var kriItems = t.AssessmentTemplateItems.Where(i => i.ItemType == AssessmentItemType.Kri).ToList();

            return new AssessmentTemplateGridRowViewModel
            {
                AssessmentTemplateId = t.AssessmentTemplateId,
                AssessmentTemplateCode = t.AssessmentTemplateCode ?? string.Empty,
                AssessmentTemplateDesc = t.AssessmentTemplateDesc,
                IsActive = t.IsActive,
                SaqCount = saqItems.Count,
                KriCount = kriItems.Count,
                SaqHeaderIds = saqItems.Select(i => i.ItemId).ToList(),
                KriHeaderIds = kriItems.Select(i => i.ItemId).ToList(),
                SaqCodes = saqItems.Where(i => saqCodes.ContainsKey(i.ItemId)).Select(i => saqCodes[i.ItemId]).ToList(),
                KriCodes = kriItems.Where(i => kriCodes.ContainsKey(i.ItemId)).Select(i => kriCodes[i.ItemId]).ToList()
            };
        }).ToList();
    }

    // --- Selectable SAQ / KRI templates (compose an assessment template from these) ---
    public async Task<List<OptionViewModel>> GetSaqOptionsAsync()
    {
        return await _db.SaqHeaders
            .AsNoTracking()
            .OrderByDescending(h => h.SaqHeaderId)
            .Select(h => new OptionViewModel
            {
                Value = h.SaqHeaderId,
                Label = string.IsNullOrEmpty(h.SaqDesc) ? (h.SaqCode ?? string.Empty) : h.SaqDesc,
                Code = h.SaqCode
            })
            .ToListAsync();
    }

    public async Task<List<OptionViewModel>> GetKriOptionsAsync()
    {
        return await _db.KriHeaders
            .AsNoTracking()
            .OrderByDescending(h => h.KriHeaderId)
            .Select(h => new OptionViewModel
            {
                Value = h.KriHeaderId,
                Label = string.IsNullOrEmpty(h.KriHeaderDesc) ? (h.KriCode ?? string.Empty) : h.KriHeaderDesc,
                Code = h.KriCode
            })
            .ToListAsync();
    }

    // --- Save (header + polymorphic item set) ---
    public async Task<SaveResultDto> SaveAsync(AssessmentTemplateSaveDto model)
    {
        var desc = model.AssessmentTemplateDesc?.Trim() ?? string.Empty;

        var saqIds = (model.SaqHeaderIds ?? new List<int>()).Where(id => id > 0).Distinct().ToList();
        var kriIds = (model.KriHeaderIds ?? new List<int>()).Where(id => id > 0).Distinct().ToList();

        if (saqIds.Count == 0 && kriIds.Count == 0)
        {
            throw new InvalidOperationException("Select at least one SAQ or KRI template.");
        }

        if (saqIds.Count > 0)
        {
            var found = await _db.SaqHeaders.CountAsync(h => saqIds.Contains(h.SaqHeaderId));
            if (found != saqIds.Count)
            {
                throw new InvalidOperationException("One or more selected SAQ templates no longer exist.");
            }
        }

        if (kriIds.Count > 0)
        {
            var found = await _db.KriHeaders.CountAsync(h => kriIds.Contains(h.KriHeaderId));
            if (found != kriIds.Count)
            {
                throw new InvalidOperationException("One or more selected KRI templates no longer exist.");
            }
        }

        var desired = saqIds.Select(id => (ItemType: AssessmentItemType.Saq, ItemId: id))
            .Concat(kriIds.Select(id => (ItemType: AssessmentItemType.Kri, ItemId: id)))
            .ToList();

        AssessmentTemplate header;
        if (model.AssessmentTemplateId == 0)
        {
            header = new AssessmentTemplate
            {
                AssessmentTemplateDesc = desc,
                IsActive = model.IsActive,
                AssessmentTemplateCode = await _codeService.GenerateAssessmentTemplateCodeAsync()
            };
            _db.AssessmentTemplates.Add(header);
        }
        else
        {
            header = await _db.AssessmentTemplates
                .Include(t => t.AssessmentTemplateItems)
                .FirstOrDefaultAsync(t => t.AssessmentTemplateId == model.AssessmentTemplateId)
                ?? throw new InvalidOperationException($"Assessment template with Id {model.AssessmentTemplateId} was not found.");

            header.AssessmentTemplateDesc = desc;
            header.IsActive = model.IsActive;

            // Reconcile the item set (final-state semantics) without churning unchanged rows.
            foreach (var item in header.AssessmentTemplateItems.ToList())
            {
                if (!desired.Contains((item.ItemType, item.ItemId)))
                {
                    _db.AssessmentTemplateItems.Remove(item);
                }
            }
        }

        var existingKeys = header.AssessmentTemplateItems
            .Select(i => (i.ItemType, i.ItemId))
            .ToHashSet();

        foreach (var key in desired)
        {
            if (existingKeys.Add(key))
            {
                header.AssessmentTemplateItems.Add(new AssessmentTemplateItem
                {
                    ItemType = key.ItemType,
                    ItemId = key.ItemId
                });
            }
        }

        await _db.SaveChangesAsync();
        return new SaveResultDto { Id = header.AssessmentTemplateId, Code = header.AssessmentTemplateCode ?? string.Empty };
    }

    public async Task DeleteAsync(int assessmentTemplateId)
    {
        var header = await _db.AssessmentTemplates.FindAsync(assessmentTemplateId)
            ?? throw new InvalidOperationException($"Assessment template with Id {assessmentTemplateId} was not found.");

        _db.AssessmentTemplates.Remove(header);
        await _db.SaveChangesAsync();
    }
}
