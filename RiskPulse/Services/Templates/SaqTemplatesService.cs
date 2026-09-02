using Microsoft.EntityFrameworkCore;
using RiskPulse.Data;
using RiskPulse.Data.Entries;
using RiskPulse.Data.Extensions;
using RiskPulse.Models.Dto;
using RiskPulse.Models.Enum;
using RiskPulse.Models.ViewModel;

namespace RiskPulse.Services.Templates;

public class SaqTemplatesService
{
    private readonly AppDbContext _db;
    private readonly TemplateCodeService _codeService;

    public SaqTemplatesService(AppDbContext db, TemplateCodeService codeService)
    {
        _db = db;
        _codeService = codeService;
    }

    // --- SAQ template headers (grid/save/delete) ---
    public async Task<List<SaqGridRowViewModel>> GetHeaderRowsAsync()
    {
        return await _db.SaqHeaders
            .AsNoTracking()
            .OrderByDescending(h => h.SaqHeaderId)
            .Select(h => new SaqGridRowViewModel
            {
                SaqHeaderId = h.SaqHeaderId,
                SaqCode = h.SaqCode ?? string.Empty,
                SaqDesc = h.SaqDesc,
                GroupId = h.GroupId,
                GroupDesc = h.Group != null ? h.Group.GroupDesc : string.Empty,
                UnitId = h.UnitId,
                UnitDesc = h.Unit != null ? h.Unit.UnitDesc : string.Empty,
                SaqStatus = h.SaqStatus.ToString(),
                QuestionCount = h.SaqQuestions.Count
            })
            .ToListAsync();
    }

    public async Task<SaveResultDto> SaveHeaderAsync(SaqHeaderSaveDto model)
    {
        var desc = model.SaqDesc?.Trim() ?? string.Empty;

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

        if (model.SaqHeaderId == 0)
        {
            var code = await _codeService.GenerateSaqCodeAsync();

            var header = new SaqHeader
            {
                SaqDesc = desc,
                GroupId = hasGroup ? model.GroupId : null,
                UnitId = hasUnit ? model.UnitId : null,
                SaqStatus = model.SaqStatus,
                SaqCode = code
            };

            _db.SaqHeaders.Add(header);
            await _db.SaveChangesAsync();
            return new SaveResultDto { Id = header.SaqHeaderId };
        }

        var existing = await _db.SaqHeaders.FindAsync(model.SaqHeaderId)
            ?? throw new InvalidOperationException($"Template with Id {model.SaqHeaderId} was not found.");

        if (existing.SaqStatus == SaqStatus.Locked)
        {
            throw new InvalidOperationException("Cannot modify a locked template.");
        }

        existing.SaqDesc = desc;
        existing.GroupId = hasGroup ? model.GroupId : null;
        existing.UnitId = hasUnit ? model.UnitId : null;
        existing.SaqStatus = model.SaqStatus;

        await _db.SaveChangesAsync();
        return new SaveResultDto { Id = existing.SaqHeaderId };
    }

    public async Task DeleteHeaderAsync(int saqHeaderId)
    {
        var header = await _db.SaqHeaders.FindAsync(saqHeaderId)
            ?? throw new InvalidOperationException($"Template with Id {saqHeaderId} was not found.");

        if (header.SaqStatus == SaqStatus.Locked)
        {
            throw new InvalidOperationException("Cannot delete a locked template.");
        }

        _db.SaqHeaders.Remove(header);
        await _db.SaveChangesAsync();
    }

    // --- SAQ questions + options (grid/save/delete) ---
    public async Task<List<SaqQuestionGridRowViewModel>> GetQuestionRowsAsync(int saqHeaderId)
    {
        return await _db.SaqQuestions
            .Include(q => q.SaqQuestionOptions)
            .AsNoTracking()
            .Where(q => q.SaqHeaderId == saqHeaderId)
            .OrderBy(q => q.DisplayOrder)
            .ThenBy(q => q.QuestionId)
            .Select(q => new SaqQuestionGridRowViewModel
            {
                QuestionId = q.QuestionId,
                QuestionText = q.QuestionText,
                AllowComment = q.AllowComment,
                DisplayOrder = q.DisplayOrder,
                Options = q.SaqQuestionOptions
                    .OrderBy(o => o.DisplayOrder)
                    .ThenBy(o => o.OptionId)
                    .Select(o => new SaqOptionGridRowViewModel
                    {
                        OptionId = o.OptionId,
                        OptionText = o.OptionText
                    })
                    .ToList()
            })
            .ToListAsync();
    }

    public async Task<SaveResultDto> SaveQuestionAsync(SaqQuestionSaveDto model)
    {
        var header = await _db.SaqHeaders.FindAsync(model.SaqHeaderId)
            ?? throw new InvalidOperationException($"Template with Id {model.SaqHeaderId} was not found.");

        if (header.SaqStatus == SaqStatus.Locked)
        {
            throw new InvalidOperationException("Cannot modify a locked template.");
        }

        var text = model.QuestionText.Trim();

        var options = model.Options
            .Where(o => !string.IsNullOrWhiteSpace(o.OptionText))
            .Select(o => o.OptionText.Trim())
            .ToList();

        if (options.Count == 0)
        {
            throw new InvalidOperationException("At least one option is required.");
        }

        if (options.Count != options.Distinct(StringComparer.OrdinalIgnoreCase).Count())
        {
            throw new InvalidOperationException("Duplicate options are not allowed for the same question.");
        }

        var duplicateQuestion = await HasDuplicateQuestionAsync(
            model.SaqHeaderId,
            model.QuestionId,
            text,
            options);
        if (duplicateQuestion)
        {
            throw new InvalidOperationException("The same question with the same options already exists in this template.");
        }

        if (model.QuestionId == 0)
        {
            var maxDisplayOrder = await _db.SaqQuestions
                .Where(q => q.SaqHeaderId == model.SaqHeaderId)
                .Select(q => (int?)q.DisplayOrder)
                .MaxAsync() ?? 0;

            var question = new SaqQuestion
            {
                SaqHeaderId = model.SaqHeaderId,
                QuestionText = text,
                QuestionType = QuestionType.Dropdown,
                AllowComment = model.AllowComment,
                DisplayOrder = maxDisplayOrder + 1,
                SaqQuestionOptions = options
                    .Select((optionText, index) => new SaqQuestionOption
                    {
                        OptionText = optionText,
                        DisplayOrder = index + 1
                    })
                    .ToList()
            };

            _db.SaqQuestions.Add(question);
            await _db.SaveChangesAsync();
            return new SaveResultDto { Id = question.QuestionId };
        }

        var existing = await _db.SaqQuestions
            .Include(q => q.SaqQuestionOptions)
            .FirstOrDefaultAsync(q => q.QuestionId == model.QuestionId)
            ?? throw new InvalidOperationException($"Question with Id {model.QuestionId} was not found.");

        existing.QuestionText = text;
        existing.QuestionType = QuestionType.Dropdown;
        existing.AllowComment = model.AllowComment;
        existing.SaqQuestionOptions.Clear();

        foreach (var (optionText, index) in options.Select((value, index) => (value, index)))
        {
            existing.SaqQuestionOptions.Add(new SaqQuestionOption
            {
                OptionText = optionText,
                DisplayOrder = index + 1
            });
        }

        await _db.SaveChangesAsync();
        return new SaveResultDto { Id = existing.QuestionId };
    }

    public async Task DeleteQuestionAsync(int questionId)
    {
        var question = await _db.SaqQuestions.FindAsync(questionId)
            ?? throw new InvalidOperationException($"Question with Id {questionId} was not found.");

        var header = await _db.SaqHeaders.FindAsync(question.SaqHeaderId);
        if (header?.SaqStatus == SaqStatus.Locked)
        {
            throw new InvalidOperationException("Cannot modify a locked template.");
        }

        await _db.SaqQuestionOptions
            .Where(o => o.QuestionId == questionId)
            .ExecuteDeleteAsync();

        _db.SaqQuestions.Remove(question);
        await _db.SaveChangesAsync();

        var remainingQuestions = await _db.SaqQuestions
            .Where(q => q.SaqHeaderId == question.SaqHeaderId)
            .OrderBy(q => q.DisplayOrder)
            .ThenBy(q => q.QuestionId)
            .ToListAsync();

        for (var index = 0; index < remainingQuestions.Count; index++)
        {
            remainingQuestions[index].DisplayOrder = index + 1;
        }

        await _db.SaveChangesAsync();
    }

    private async Task<bool> HasDuplicateQuestionAsync(
        int saqHeaderId,
        int questionId,
        string questionText,
        List<string> optionTexts)
    {
        var newOptions = optionTexts.ToHashSet(StringComparer.OrdinalIgnoreCase);

        var candidates = await _db.SaqQuestions
            .Include(q => q.SaqQuestionOptions)
            .AsNoTracking()
            .Where(q => q.SaqHeaderId == saqHeaderId &&
                        q.QuestionId != questionId &&
                        q.QuestionText.ToLower() == questionText.ToLower())
            .ToListAsync();

        return candidates.Any(q =>
            q.SaqQuestionOptions
                .Select(o => o.OptionText.Trim())
                .ToHashSet(StringComparer.OrdinalIgnoreCase)
                .SetEquals(newOptions));
    }
}
