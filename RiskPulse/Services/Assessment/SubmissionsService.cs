using Microsoft.EntityFrameworkCore;
using RiskPulse.Data;
using RiskPulse.Data.Entries;
using RiskPulse.Models.Dto;
using RiskPulse.Models.Enum;
using RiskPulse.Models.ViewModel;

namespace RiskPulse.Services.Assessment;

public class SubmissionsService
{
    private const string ItemSubmittedStepCode = "Submitted";
    private const string ItemApprovedStepCode = "Approved";
    private const string UnitAuthorizedStepCode = "Authorized";
    private const string ItemWorkflowCode = "ASSESSMENT-ITEM";
    private const string UnitWorkflowCode = "ASSESSMENT-UNIT";

    private readonly AppDbContext _db;

    public SubmissionsService(AppDbContext db)
    {
        _db = db;
    }

    // --- My-unit grid ---
    public async Task<List<SubmissionGridRowViewModel>> GetGridRowsAsync(int userId)
    {
        var unitId = await GetUserUnitIdAsync(userId);

        return await _db.AssessmentUnits
            .AsNoTracking()
            .Where(u => u.UnitId == unitId)
            .OrderByDescending(u => u.AssessmentHeader!.PeriodStart)
            .Select(u => new SubmissionGridRowViewModel
            {
                AssessmentUnitId = u.AssessmentUnitId,
                AssessmentCode = u.AssessmentHeader!.AssessmentCode,
                UnitDesc = u.Unit!.UnitDesc,
                PeriodStart = u.AssessmentHeader.PeriodStart,
                PeriodEnd = u.AssessmentHeader.PeriodEnd,
                StatusLabel = u.WorkflowStep!.StepLabel,
                ItemCount = u.AssessmentItems.Count,
                SubmittedCount = u.AssessmentItems.Count(i => i.WorkflowStep!.StepCode == ItemSubmittedStepCode
                                                           || i.WorkflowStep!.StepCode == ItemApprovedStepCode)
            })
            .ToListAsync();
    }

    // --- Detail (own unit only) ---
    public async Task<AssessmentDetailViewModel> GetDetailAsync(int assessmentUnitId, int userId)
    {
        var unitId = await GetUserUnitIdAsync(userId);

        var unit = await _db.AssessmentUnits
            .AsNoTracking()
            .Where(u => u.AssessmentUnitId == assessmentUnitId && u.UnitId == unitId)
            .Include(u => u.AssessmentHeader)
            .Include(u => u.Unit)
            .Include(u => u.WorkflowStep)
            .Include(u => u.AuthorizedBy)
            .Include(u => u.AssessmentItems).ThenInclude(i => i.WorkflowStep)
            .Include(u => u.AssessmentItems).ThenInclude(i => i.SubmittedBy)
            .Include(u => u.AssessmentItems).ThenInclude(i => i.ApprovedBy)
            .SingleOrDefaultAsync()
            ?? throw new InvalidOperationException("The requested assessment was not found.");

        var items = unit.AssessmentItems.ToList();

        return new AssessmentDetailViewModel
        {
            AssessmentUnitId = unit.AssessmentUnitId,
            AssessmentCode = unit.AssessmentHeader!.AssessmentCode,
            UnitDesc = unit.Unit!.UnitDesc,
            PeriodStart = unit.AssessmentHeader.PeriodStart,
            PeriodEnd = unit.AssessmentHeader.PeriodEnd,
            StatusLabel = unit.WorkflowStep!.StepLabel,
            CanAuthorize = unit.WorkflowStep.StepCode != UnitAuthorizedStepCode,
            ItemCount = items.Count,
            ApprovedCount = items.Count(i => i.WorkflowStep!.StepCode == ItemApprovedStepCode),
            AuthorizedBy = unit.AuthorizedBy?.Username ?? string.Empty,
            AuthorizedOn = unit.AuthorizedOn,
            Items = (await BuildItemRowsAsync(items))
        };
    }

    // --- SAQ entry screen ---
    public async Task<SaqEntryViewModel> GetSaqEntryAsync(int assessmentItemId, int userId)
    {
        var item = await LoadOwnedItemAsync(assessmentItemId, userId);

        if (item.ItemType != ScheduleItemType.Saq)
        {
            throw new InvalidOperationException("This item is not a Self Assessment Questionnaire.");
        }

        var saq = await _db.SaqHeaders.AsNoTracking()
            .Where(h => h.SaqHeaderId == item.ItemId)
            .Include(h => h.SaqQuestions.OrderBy(q => q.DisplayOrder)).ThenInclude(q => q.SaqQuestionOptions.OrderBy(o => o.DisplayOrder))
            .SingleOrDefaultAsync()
            ?? throw new InvalidOperationException("The SAQ template for this item could not be found.");

        var answers = await _db.SaqAssessmentAnswers.AsNoTracking()
            .Where(a => a.AssessmentItemId == item.AssessmentItemId)
            .ToDictionaryAsync(a => a.QuestionId);

        return new SaqEntryViewModel
        {
            AssessmentItemId = item.AssessmentItemId,
            AssessmentUnitId = item.AssessmentUnitId,
            AssessmentCode = item.AssessmentUnit!.AssessmentHeader!.AssessmentCode,
            ItemTitle = saq.SaqDesc,
            UnitDesc = item.AssessmentUnit.Unit!.UnitDesc,
            StatusLabel = item.WorkflowStep!.StepLabel,
            Editable = IsEditable(item),
            Questions = saq.SaqQuestions
                .Select(q =>
                {
                    answers.TryGetValue(q.QuestionId, out var answer);
                    return new SaqEntryQuestionViewModel
                    {
                        QuestionId = q.QuestionId,
                        QuestionText = q.QuestionText,
                        AllowComment = q.AllowComment,
                        DisplayOrder = q.DisplayOrder,
                        SelectedOptionId = answer?.OptionId,
                        Comment = answer?.Comment,
                        Options = q.SaqQuestionOptions
                            .Select(o => new SaqOptionGridRowViewModel { OptionId = o.OptionId, OptionText = o.OptionText })
                            .ToList()
                    };
                })
                .ToList()
        };
    }

    // --- KRI entry screen ---
    public async Task<KriEntryViewModel> GetKriEntryAsync(int assessmentItemId, int userId)
    {
        var item = await LoadOwnedItemAsync(assessmentItemId, userId);

        if (item.ItemType != ScheduleItemType.Kri)
        {
            throw new InvalidOperationException("This item is not a Key Risk Indicator set.");
        }

        var kriHeader = await _db.KriHeaders.AsNoTracking()
            .Where(h => h.KriHeaderId == item.ItemId)
            .Include(h => h.Kris)
            .SingleOrDefaultAsync()
            ?? throw new InvalidOperationException("The KRI template for this item could not be found.");

        var values = await _db.KriAssessmentValues.AsNoTracking()
            .Where(v => v.AssessmentItemId == item.AssessmentItemId)
            .ToDictionaryAsync(v => v.KriId);

        return new KriEntryViewModel
        {
            AssessmentItemId = item.AssessmentItemId,
            AssessmentUnitId = item.AssessmentUnitId,
            AssessmentCode = item.AssessmentUnit!.AssessmentHeader!.AssessmentCode,
            ItemTitle = kriHeader.KriHeaderDesc ?? kriHeader.KriCode ?? string.Empty,
            UnitDesc = item.AssessmentUnit.Unit!.UnitDesc,
            StatusLabel = item.WorkflowStep!.StepLabel,
            Editable = IsEditable(item),
            Values = kriHeader.Kris
                .OrderBy(k => k.KriId)
                .Select(k =>
                {
                    values.TryGetValue(k.KriId, out var value);
                    return new KriEntryValueViewModel
                    {
                        KriId = k.KriId,
                        KriDesc = k.KriDesc,
                        AllowComment = k.AllowComment,
                        GreenLimit = k.GreenLimit,
                        AmberLimit = k.AmberLimit,
                        RedLimit = k.RedLimit,
                        Value = value?.Value,
                        Comment = value?.Comment
                    };
                })
                .ToList()
        };
    }

    // --- Save (draft) ---
    public async Task SaveSaqAnswersAsync(SaveSaqAnswersDto dto, int userId)
    {
        var item = await LoadOwnedItemForUpdateAsync(dto.AssessmentItemId, userId);

        if (item.ItemType != ScheduleItemType.Saq)
        {
            throw new InvalidOperationException("This item is not a Self Assessment Questionnaire.");
        }
        EnsureEditable(item, "Answers can only be edited before the item is submitted.");

        var questions = await _db.SaqQuestions.AsNoTracking()
            .Where(q => q.SaqHeaderId == item.ItemId)
            .Include(q => q.SaqQuestionOptions)
            .ToListAsync();

        var payload = dto.Answers
            .GroupBy(a => a.QuestionId)
            .ToDictionary(g => g.Key, g => g.Last());

        foreach (var (questionId, answer) in payload)
        {
            var question = questions.FirstOrDefault(q => q.QuestionId == questionId)
                ?? throw new InvalidOperationException("One of the answers does not belong to this SAQ item.");

            if (answer.OptionId != 0 && !question.SaqQuestionOptions.Any(o => o.OptionId == answer.OptionId))
            {
                throw new InvalidOperationException("One of the selected answers is no longer valid.");
            }
        }

        var existing = await _db.SaqAssessmentAnswers
            .Where(a => a.AssessmentItemId == item.AssessmentItemId)
            .ToListAsync();

        foreach (var stored in existing)
        {
            if (!payload.TryGetValue(stored.QuestionId, out var answer)) continue;

            if (answer.OptionId == 0)
            {
                _db.SaqAssessmentAnswers.Remove(stored);
            }
            else
            {
                stored.OptionId = answer.OptionId;
                stored.Comment = answer.Comment;
            }
        }

        var existingQuestionIds = existing.Select(a => a.QuestionId).ToHashSet();
        foreach (var (questionId, answer) in payload)
        {
            if (answer.OptionId == 0 || existingQuestionIds.Contains(questionId)) continue;

            _db.SaqAssessmentAnswers.Add(new SaqAssessmentAnswer
            {
                AssessmentItemId = item.AssessmentItemId,
                QuestionId = questionId,
                OptionId = answer.OptionId,
                Comment = answer.Comment
            });
        }

        await _db.SaveChangesAsync();
    }

    public async Task SaveKriValuesAsync(SaveKriValuesDto dto, int userId)
    {
        var item = await LoadOwnedItemForUpdateAsync(dto.AssessmentItemId, userId);

        if (item.ItemType != ScheduleItemType.Kri)
        {
            throw new InvalidOperationException("This item is not a Key Risk Indicator set.");
        }
        EnsureEditable(item, "Values can only be edited before the item is submitted.");

        var kriIds = await _db.Kris.AsNoTracking()
            .Where(k => k.KriHeaderId == item.ItemId)
            .Select(k => k.KriId)
            .ToHashSetAsync();

        var payload = dto.Values
            .GroupBy(v => v.KriId)
            .ToDictionary(g => g.Key, g => g.Last());

        foreach (var (kriId, value) in payload)
        {
            if (!kriIds.Contains(kriId))
            {
                throw new InvalidOperationException("One of the entries does not belong to this KRI item.");
            }
        }

        var existing = await _db.KriAssessmentValues
            .Where(v => v.AssessmentItemId == item.AssessmentItemId)
            .ToListAsync();

        foreach (var stored in existing)
        {
            if (!payload.TryGetValue(stored.KriId, out var value)) continue;

            if (!value.Value.HasValue)
            {
                _db.KriAssessmentValues.Remove(stored);
            }
            else
            {
                stored.Value = value.Value.Value;
                stored.Comment = value.Comment;
            }
        }

        var existingKriIds = existing.Select(v => v.KriId).ToHashSet();
        foreach (var (kriId, value) in payload)
        {
            if (!value.Value.HasValue || existingKriIds.Contains(kriId)) continue;

            _db.KriAssessmentValues.Add(new KriAssessmentValue
            {
                AssessmentItemId = item.AssessmentItemId,
                KriId = kriId,
                Value = value.Value.Value,
                Comment = value.Comment
            });
        }

        await _db.SaveChangesAsync();
    }

    // --- Workflow transitions: submit / approve / authorize ---
    public async Task SubmitItemAsync(int assessmentItemId, int userId)
    {
        var item = await LoadOwnedItemForUpdateAsync(assessmentItemId, userId);
        EnsureEditable(item, "This item has already been submitted.");
        await EnsureCompleteAsync(item);

        var step = await GetStepAsync(ItemWorkflowCode, ItemSubmittedStepCode);
        item.WorkflowStepId = step.WorkflowStepId;
        item.SubmittedById = userId;
        item.SubmittedOn = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    public async Task ApproveItemAsync(int assessmentItemId, int userId)
    {
        var item = await LoadOwnedItemForUpdateAsync(assessmentItemId, userId);

        if (item.WorkflowStep!.StepCode == ItemApprovedStepCode)
        {
            throw new InvalidOperationException("This item has already been approved.");
        }
        if (item.WorkflowStep!.StepCode != ItemSubmittedStepCode)
        {
            throw new InvalidOperationException("Only items that have been submitted can be approved.");
        }
        if (item.AssessmentUnit!.WorkflowStep!.StepCode == UnitAuthorizedStepCode)
        {
            throw new InvalidOperationException("This assessment has already been authorized.");
        }

        var step = await GetStepAsync(ItemWorkflowCode, ItemApprovedStepCode);
        item.WorkflowStepId = step.WorkflowStepId;
        item.ApprovedById = userId;
        item.ApprovedOn = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    public async Task AuthorizeUnitAsync(int assessmentUnitId, int userId)
    {
        var unitId = await GetUserUnitIdAsync(userId);

        var unit = await _db.AssessmentUnits
            .Where(u => u.AssessmentUnitId == assessmentUnitId && u.UnitId == unitId)
            .Include(u => u.WorkflowStep)
            .Include(u => u.AssessmentItems).ThenInclude(i => i.WorkflowStep)
            .SingleOrDefaultAsync()
            ?? throw new InvalidOperationException("The requested assessment was not found.");

        if (unit.WorkflowStep!.StepCode == UnitAuthorizedStepCode)
        {
            throw new InvalidOperationException("This assessment has already been authorized.");
        }

        var notApproved = unit.AssessmentItems.Count(i => i.WorkflowStep!.StepCode != ItemApprovedStepCode);
        if (notApproved > 0)
        {
            throw new InvalidOperationException("All items must be approved before the assessment can be authorized.");
        }

        var step = await GetStepAsync(UnitWorkflowCode, UnitAuthorizedStepCode);
        unit.WorkflowStepId = step.WorkflowStepId;
        unit.AuthorizedById = userId;
        unit.AuthorizedOn = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    // --- Private helpers ---
    private async Task<int> GetUserUnitIdAsync(int userId)
    {
        var unitId = await _db.Users.AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => (int?)u.UnitId)
            .SingleOrDefaultAsync();

        return unitId ?? throw new InvalidOperationException("Your user profile was not found.");
    }

    private async Task<AssessmentItem> LoadOwnedItemAsync(int assessmentItemId, int userId)
    {
        return await QueryOwnedItemAsync(assessmentItemId, userId, asNoTracking: true)
            ?? throw new InvalidOperationException("The requested item was not found.");
    }

    private async Task<AssessmentItem> LoadOwnedItemForUpdateAsync(int assessmentItemId, int userId)
    {
        return await QueryOwnedItemAsync(assessmentItemId, userId, asNoTracking: false)
            ?? throw new InvalidOperationException("The requested item was not found.");
    }

    private async Task<AssessmentItem?> QueryOwnedItemAsync(int assessmentItemId, int userId, bool asNoTracking)
    {
        var unitId = await GetUserUnitIdAsync(userId);

        IQueryable<AssessmentItem> query = _db.AssessmentItems
            .Where(i => i.AssessmentItemId == assessmentItemId && i.AssessmentUnit!.UnitId == unitId)
            .Include(i => i.AssessmentUnit!).ThenInclude(u => u.AssessmentHeader!)
            .Include(i => i.AssessmentUnit!).ThenInclude(u => u.Unit!)
            .Include(i => i.AssessmentUnit!).ThenInclude(u => u.WorkflowStep!)
            .Include(i => i.WorkflowStep!);

        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        return await query.SingleOrDefaultAsync();
    }

    private async Task<List<AssessmentItemRowViewModel>> BuildItemRowsAsync(List<AssessmentItem> items)
    {
        var saqIds = items.Where(i => i.ItemType == ScheduleItemType.Saq).Select(i => i.ItemId).Distinct().ToList();
        var kriIds = items.Where(i => i.ItemType == ScheduleItemType.Kri).Select(i => i.ItemId).Distinct().ToList();

        var saqHeaders = await _db.SaqHeaders.AsNoTracking()
            .Where(h => saqIds.Contains(h.SaqHeaderId))
            .ToDictionaryAsync(h => h.SaqHeaderId, h => new { h.SaqDesc, h.SaqCode });

        var kriHeaders = await _db.KriHeaders.AsNoTracking()
            .Where(h => kriIds.Contains(h.KriHeaderId))
            .ToDictionaryAsync(h => h.KriHeaderId, h => new { h.KriHeaderDesc, h.KriCode });

        return items
            .OrderBy(i => i.ItemType)
            .ThenBy(i => i.ItemId)
            .Select(i =>
            {
                string title;
                string code;
                if (i.ItemType == ScheduleItemType.Saq && saqHeaders.TryGetValue(i.ItemId, out var saq))
                {
                    title = saq.SaqDesc;
                    code = saq.SaqCode ?? string.Empty;
                }
                else if (i.ItemType == ScheduleItemType.Kri && kriHeaders.TryGetValue(i.ItemId, out var kri))
                {
                    title = kri.KriHeaderDesc ?? string.Empty;
                    code = kri.KriCode ?? string.Empty;
                }
                else
                {
                    title = "Unknown";
                    code = string.Empty;
                }

                return new AssessmentItemRowViewModel
                {
                    AssessmentItemId = i.AssessmentItemId,
                    ItemType = i.ItemType.ToString(),
                    ItemTitle = title,
                    ReferenceCode = code,
                    StepCode = i.WorkflowStep!.StepCode,
                    StatusLabel = i.WorkflowStep.StepLabel,
                    Editable = IsEditable(i),
                    SubmittedBy = i.SubmittedBy?.Username ?? string.Empty,
                    SubmittedOn = i.SubmittedOn,
                    ApprovedBy = i.ApprovedBy?.Username ?? string.Empty,
                    ApprovedOn = i.ApprovedOn
                };
            })
            .ToList();
    }

    private async Task EnsureCompleteAsync(AssessmentItem item)
    {
        if (item.ItemType == ScheduleItemType.Saq)
        {
            var total = await _db.SaqQuestions.AsNoTracking().CountAsync(q => q.SaqHeaderId == item.ItemId);
            var answered = await _db.SaqAssessmentAnswers.AsNoTracking().CountAsync(a => a.AssessmentItemId == item.AssessmentItemId);

            if (answered < total)
            {
                throw new InvalidOperationException($"Please answer all {total} questions before submitting.");
            }
        }
        else if (item.ItemType == ScheduleItemType.Kri)
        {
            var total = await _db.Kris.AsNoTracking().CountAsync(k => k.KriHeaderId == item.ItemId);
            var entered = await _db.KriAssessmentValues.AsNoTracking().CountAsync(v => v.AssessmentItemId == item.AssessmentItemId);

            if (entered < total)
            {
                throw new InvalidOperationException($"Please enter values for all {total} KRIs before submitting.");
            }
        }
    }

    private async Task<WorkflowStep> GetStepAsync(string workflowCode, string stepCode)
    {
        return await _db.WorkflowSteps.AsNoTracking()
            .Where(s => s.Workflow!.WorkflowCode == workflowCode && s.StepCode == stepCode)
            .SingleOrDefaultAsync()
            ?? throw new InvalidOperationException($"Workflow step '{stepCode}' has not been configured for workflow '{workflowCode}'.");
    }

    private static void EnsureEditable(AssessmentItem item, string message)
    {
        if (!IsEditable(item))
        {
            throw new InvalidOperationException(message);
        }
    }

    private static bool IsEditable(AssessmentItem item)
    {
        var unitStepCode = item.AssessmentUnit!.WorkflowStep!.StepCode;
        var itemStepCode = item.WorkflowStep!.StepCode;

        return unitStepCode != UnitAuthorizedStepCode
            && itemStepCode != ItemSubmittedStepCode
            && itemStepCode != ItemApprovedStepCode;
    }
}