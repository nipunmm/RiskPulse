using Microsoft.EntityFrameworkCore;
using RiskPulse.Data;
using RiskPulse.Data.Entries;
using RiskPulse.Models.Dto;
using RiskPulse.Models.Enum;
using RiskPulse.Models.ViewModel;
using RiskPulse.Services.Utilities;

namespace RiskPulse.Services.Templates;

public class SaqTemplatesService
{
    private readonly AppDbContext _db;
    private readonly CodeGeneratorService _codeService;

    public SaqTemplatesService(AppDbContext db, CodeGeneratorService codeService)
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

    public async Task<SaveResultDto> SaveFullAsync(SaqTemplateSaveDto model)
    {
        var headerDto = model.Header;
        var desc = headerDto.SaqDesc?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(desc))
        {
            throw new InvalidOperationException("Template description is required.");
        }

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

        // --- Validate the question set (final-state semantics) before mutating anything ---
        var questions = NormalizeQuestions(model.Questions);

        if (headerDto.SaqHeaderId == 0)
        {
            if (questions.Count == 0)
            {
                throw new InvalidOperationException("At least one question is required.");
            }

            var code = await _codeService.GenerateSaqCodeAsync();

            var header = new SaqHeader
            {
                SaqDesc = desc,
                GroupId = hasGroup ? headerDto.GroupId : null,
                UnitId = hasUnit ? headerDto.UnitId : null,
                SaqStatus = headerDto.SaqStatus,
                SaqCode = code
            };

            _db.SaqHeaders.Add(header);

            var displayOrder = 0;
            foreach (var questionDto in questions)
            {
                displayOrder++;
                header.SaqQuestions.Add(BuildQuestion(questionDto, displayOrder));
            }

            await _db.SaveChangesAsync();
            return new SaveResultDto { Id = header.SaqHeaderId };
        }

        var existing = await _db.SaqHeaders
            .Include(h => h.SaqQuestions)
            .ThenInclude(q => q.SaqQuestionOptions)
            .FirstOrDefaultAsync(h => h.SaqHeaderId == headerDto.SaqHeaderId)
            ?? throw new InvalidOperationException($"Template with Id {headerDto.SaqHeaderId} was not found.");

        if (existing.SaqStatus == SaqStatus.Locked)
        {
            throw new InvalidOperationException("Cannot modify a locked template.");
        }

        // Reject stale question ids (deleted by another session) instead of silently creating them.
        var byId = existing.SaqQuestions.ToDictionary(q => q.QuestionId);
        var incomingIds = questions.Where(q => q.QuestionId != 0).Select(q => q.QuestionId).ToHashSet();
        foreach (var questionId in incomingIds)
        {
            if (!byId.ContainsKey(questionId))
            {
                throw new InvalidOperationException($"Question with Id {questionId} was not found.");
            }
        }

        existing.SaqDesc = desc;
        existing.GroupId = hasGroup ? headerDto.GroupId : null;
        existing.UnitId = hasUnit ? headerDto.UnitId : null;
        existing.SaqStatus = headerDto.SaqStatus;

        // Remove persisted questions that are not part of the payload (options first,
        // then the question — the same removal path DeleteQuestionAsync used).
        foreach (var question in existing.SaqQuestions.Where(q => !incomingIds.Contains(q.QuestionId)).ToList())
        {
            _db.SaqQuestionOptions.RemoveRange(question.SaqQuestionOptions);
            _db.SaqQuestions.Remove(question);
        }

        // Upsert questions in payload order with sequential display order.
        var order = 0;
        foreach (var questionDto in questions)
        {
            order++;
            if (questionDto.QuestionId == 0)
            {
                existing.SaqQuestions.Add(BuildQuestion(questionDto, order));
                continue;
            }

            var question = byId[questionDto.QuestionId];
            question.QuestionText = questionDto.QuestionText;
            question.QuestionType = QuestionType.Dropdown;
            question.AllowComment = questionDto.AllowComment;
            question.DisplayOrder = order;
            question.SaqQuestionOptions.Clear();
            for (var i = 0; i < questionDto.Options.Count; i++)
            {
                question.SaqQuestionOptions.Add(new SaqQuestionOption
                {
                    OptionText = questionDto.Options[i].OptionText,
                    DisplayOrder = i + 1
                });
            }
        }

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

        await _db.ScheduleItems
            .Where(si => si.ItemType == ScheduleItemType.Saq && si.ItemId == saqHeaderId)
            .ExecuteDeleteAsync();

        _db.SaqHeaders.Remove(header);
        await _db.SaveChangesAsync();
    }

    // --- SAQ questions (grid used to load a template into the designer / previews) ---
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

    // --- Preview ---
    public async Task<SaqPreviewViewModel> GetSaqPreviewAsync(int saqHeaderId)
    {
        var header = await _db.SaqHeaders
            .AsNoTracking()
            .Where(h => h.SaqHeaderId == saqHeaderId)
            .Select(h => new
            {
                h.SaqHeaderId,
                h.SaqCode,
                h.SaqDesc,
                h.SaqStatus,
                AssignmentLabel = h.Group != null ? h.Group.GroupDesc : (h.Unit != null ? h.Unit.UnitDesc : string.Empty)
            })
            .SingleOrDefaultAsync()
            ?? throw new InvalidOperationException($"Template with Id {saqHeaderId} was not found.");

        return new SaqPreviewViewModel
        {
            SaqHeaderId = header.SaqHeaderId,
            SaqCode = header.SaqCode ?? string.Empty,
            SaqDesc = header.SaqDesc,
            SaqStatus = header.SaqStatus.ToString(),
            AssignmentLabel = header.AssignmentLabel,
            Questions = await GetQuestionRowsAsync(saqHeaderId)
        };
    }

    // --- Question normalization + validation (pending payload is the source of truth) ---
    private static List<SaqQuestionSaveDto> NormalizeQuestions(List<SaqQuestionSaveDto>? questions)
    {
        var incoming = questions ?? new List<SaqQuestionSaveDto>();
        var normalized = new List<SaqQuestionSaveDto>();

        foreach (var questionDto in incoming)
        {
            var text = questionDto.QuestionText?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(text))
            {
                throw new InvalidOperationException("Question text is required.");
            }

            var optionTexts = (questionDto.Options ?? new List<SaqOptionSaveDto>())
                .Where(o => !string.IsNullOrWhiteSpace(o.OptionText))
                .Select(o => o.OptionText.Trim())
                .ToList();

            if (optionTexts.Count == 0)
            {
                throw new InvalidOperationException("At least one option is required.");
            }

            if (optionTexts.Count != optionTexts.Distinct(StringComparer.OrdinalIgnoreCase).Count())
            {
                throw new InvalidOperationException("Duplicate options are not allowed for the same question.");
            }

            normalized.Add(new SaqQuestionSaveDto
            {
                QuestionId = questionDto.QuestionId,
                QuestionText = text,
                AllowComment = questionDto.AllowComment,
                Options = optionTexts
                    .Select(o => new SaqOptionSaveDto { OptionText = o })
                    .ToList()
            });
        }

        // Duplicate "text + same options set" check against the effective final state.
        // A DB-only check would be wrong here: questions absent from the payload are
        // deleted by the same atomic save, so a persisted row can collide with an
        // incoming one without the final template holding a duplicate.
        for (var i = 0; i < normalized.Count; i++)
        {
            for (var j = i + 1; j < normalized.Count; j++)
            {
                var a = normalized[i];
                var b = normalized[j];
                var aOptions = a.Options.Select(o => o.OptionText).ToHashSet(StringComparer.OrdinalIgnoreCase);
                var bOptions = b.Options.Select(o => o.OptionText).ToHashSet(StringComparer.OrdinalIgnoreCase);
                if (string.Equals(a.QuestionText, b.QuestionText, StringComparison.OrdinalIgnoreCase) &&
                    aOptions.SetEquals(bOptions))
                {
                    throw new InvalidOperationException("The same question with the same options already exists in this template.");
                }
            }
        }

        return normalized;
    }

    private static SaqQuestion BuildQuestion(SaqQuestionSaveDto questionDto, int displayOrder)
    {
        return new SaqQuestion
        {
            QuestionText = questionDto.QuestionText,
            QuestionType = QuestionType.Dropdown,
            AllowComment = questionDto.AllowComment,
            DisplayOrder = displayOrder,
            SaqQuestionOptions = questionDto.Options
                .Select((o, index) => new SaqQuestionOption
                {
                    OptionText = o.OptionText,
                    DisplayOrder = index + 1
                })
                .ToList()
        };
    }
}