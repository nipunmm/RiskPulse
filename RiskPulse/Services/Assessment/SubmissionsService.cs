using Microsoft.EntityFrameworkCore;
using RiskPulse.Data;
using RiskPulse.Data.Entries;
using RiskPulse.Models.Dto;
using RiskPulse.Models.Enum;
using RiskPulse.Models.ViewModel;

namespace RiskPulse.Services.Assessment;

public class SubmissionsService
{
    // Item-level step codes
    public const string ItemPendingStepCode = "Pending";
    public const string ItemSubmittedStepCode = "Submitted";
    public const string ItemApprovedStepCode = "Approved";

    // Unit-level step codes (4 levels)
    public const string UnitPendingStepCode = "Pending";
    public const string UnitInProgressStepCode = "InProgress";
    public const string UnitApprovedStepCode = "UnitApproved";
    public const string UnitRiskReviewedStepCode = "RiskReviewed";
    public const string UnitFinalApprovedStepCode = "FinalApproved";
    public const string UnitReturnedStepCode = "Returned";

    // Workflows
    private const string ItemWorkflowCode = "ASSESSMENT-ITEM";
    private const string UnitWorkflowCode = "ASSESSMENT-UNIT";

    // System Roles
    public const string RoleAdmin = "Administrator";
    public const string RoleUnitInitiator = "Unit Initiator";
    public const string RoleUnitApprover = "Unit Approver";
    public const string RoleRiskReviewer = "Risk Dept Reviewer";
    public const string RoleRiskApprover = "Risk Dept Approver";

    private readonly AppDbContext _db;

    public SubmissionsService(AppDbContext db)
    {
        _db = db;
    }

    private static bool IsRiskOrAdminRole(string role) =>
        role == RoleAdmin || role == RoleRiskReviewer || role == RoleRiskApprover;

    // --- Submissions grid (role-aware: all units for Risk/Admin, own unit for branch) ---
    public async Task<List<SubmissionGridRowViewModel>> GetGridRowsAsync(int userId, string userRole)
    {
        IQueryable<AssessmentUnit> query = _db.AssessmentUnits.AsNoTracking();

        if (!IsRiskOrAdminRole(userRole))
        {
            var unitId = await GetUserUnitIdAsync(userId);
            query = query.Where(u => u.UnitId == unitId);
        }

        return await query
            .OrderByDescending(u => u.AssessmentHeader!.PeriodStart)
            .ThenBy(u => u.Unit!.UnitDesc)
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

    // --- Detail (role-aware) ---
    public async Task<AssessmentDetailViewModel> GetDetailAsync(int assessmentUnitId, int userId, string userRole)
    {
        IQueryable<AssessmentUnit> query = _db.AssessmentUnits
            .AsNoTracking()
            .Where(u => u.AssessmentUnitId == assessmentUnitId);

        if (!IsRiskOrAdminRole(userRole))
        {
            var unitId = await GetUserUnitIdAsync(userId);
            query = query.Where(u => u.UnitId == unitId);
        }

        var unit = await query
            .Include(u => u.AssessmentHeader)
            .Include(u => u.Unit)
            .Include(u => u.WorkflowStep)
            .Include(u => u.UnitApprovedBy)
            .Include(u => u.RiskReviewedBy)
            .Include(u => u.FinalApprovedBy)
            .Include(u => u.AssessmentItems).ThenInclude(i => i.WorkflowStep)
            .Include(u => u.AssessmentItems).ThenInclude(i => i.SubmittedBy)
            .Include(u => u.AssessmentItems).ThenInclude(i => i.ApprovedBy)
            .SingleOrDefaultAsync()
            ?? throw new InvalidOperationException("The requested assessment was not found.");

        var items = unit.AssessmentItems.ToList();
        var stepCode = unit.WorkflowStep!.StepCode;
        var allApproved = items.Count > 0 && items.All(i => i.WorkflowStep!.StepCode == ItemApprovedStepCode);

        var canUnitApprove = (userRole == RoleUnitApprover || userRole == RoleAdmin)
            && (stepCode == UnitPendingStepCode || stepCode == UnitInProgressStepCode || stepCode == UnitReturnedStepCode)
            && allApproved;

        var canRiskReview = (userRole == RoleRiskReviewer || userRole == RoleAdmin)
            && stepCode == UnitApprovedStepCode;

        var canFinalApprove = (userRole == RoleRiskApprover || userRole == RoleAdmin)
            && stepCode == UnitRiskReviewedStepCode;

        var canReturn = (userRole == RoleRiskReviewer || userRole == RoleRiskApprover || userRole == RoleAdmin)
            && (stepCode == UnitApprovedStepCode || stepCode == UnitRiskReviewedStepCode);

        return new AssessmentDetailViewModel
        {
            AssessmentUnitId = unit.AssessmentUnitId,
            AssessmentCode = unit.AssessmentHeader!.AssessmentCode,
            UnitDesc = unit.Unit!.UnitDesc,
            PeriodStart = unit.AssessmentHeader.PeriodStart,
            PeriodEnd = unit.AssessmentHeader.PeriodEnd,
            StatusLabel = unit.WorkflowStep.StepLabel,
            StepCode = stepCode,
            CurrentUserRole = userRole,
            CanUnitApprove = canUnitApprove,
            CanRiskReview = canRiskReview,
            CanFinalApprove = canFinalApprove,
            CanReturn = canReturn,
            ItemCount = items.Count,
            ApprovedCount = items.Count(i => i.WorkflowStep!.StepCode == ItemApprovedStepCode),
            UnitApprovedBy = unit.UnitApprovedBy?.Username,
            UnitApprovedOn = unit.UnitApprovedOn,
            RiskReviewedBy = unit.RiskReviewedBy?.Username,
            RiskReviewedOn = unit.RiskReviewedOn,
            RiskReviewerRemarks = unit.RiskReviewerRemarks,
            FinalApprovedBy = unit.FinalApprovedBy?.Username,
            FinalApprovedOn = unit.FinalApprovedOn,
            FinalApproverRemarks = unit.FinalApproverRemarks,
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

    // --- Workflow transitions: Level 1 Submit / Level 2 Unit Approve / Level 3 Risk Review / Level 4 Final Approve / Return ---
    public async Task SubmitItemAsync(int assessmentItemId, int userId, string userRole)
    {
        if (userRole != RoleUnitInitiator && userRole != RoleAdmin)
        {
            throw new InvalidOperationException("Only Unit Initiators can submit assessment items.");
        }

        var item = await LoadOwnedItemForUpdateAsync(assessmentItemId, userId, userRole);
        EnsureEditable(item, "This item has already been submitted.");
        await EnsureCompleteAsync(item);

        var step = await GetStepAsync(ItemWorkflowCode, ItemSubmittedStepCode);
        item.WorkflowStepId = step.WorkflowStepId;
        item.SubmittedById = userId;
        item.SubmittedOn = DateTime.UtcNow;

        // Auto-advance unit to InProgress if currently Pending or Returned
        var unit = item.AssessmentUnit!;
        if (unit.WorkflowStep!.StepCode == UnitPendingStepCode || unit.WorkflowStep!.StepCode == UnitReturnedStepCode)
        {
            var unitStep = await GetStepAsync(UnitWorkflowCode, UnitInProgressStepCode);
            unit.WorkflowStepId = unitStep.WorkflowStepId;
        }

        await _db.SaveChangesAsync();
    }

    public async Task ApproveItemAsync(int assessmentItemId, int userId, string userRole)
    {
        if (userRole != RoleUnitApprover && userRole != RoleAdmin)
        {
            throw new InvalidOperationException("Only Unit Approvers can approve assessment items.");
        }

        var item = await LoadOwnedItemForUpdateAsync(assessmentItemId, userId, userRole);

        if (item.WorkflowStep!.StepCode == ItemApprovedStepCode)
        {
            throw new InvalidOperationException("This item has already been approved.");
        }
        if (item.WorkflowStep!.StepCode != ItemSubmittedStepCode)
        {
            throw new InvalidOperationException("Only items that have been submitted can be approved.");
        }
        if (item.SubmittedById == userId && userRole != RoleAdmin)
        {
            throw new InvalidOperationException("Segregation of duties violation: You cannot approve an item you submitted yourself.");
        }
        if (item.AssessmentUnit!.WorkflowStep!.StepCode == UnitFinalApprovedStepCode)
        {
            throw new InvalidOperationException("This assessment has already been finalized.");
        }

        var step = await GetStepAsync(ItemWorkflowCode, ItemApprovedStepCode);
        item.WorkflowStepId = step.WorkflowStepId;
        item.ApprovedById = userId;
        item.ApprovedOn = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    // Level 2: Unit Approver signs off the unit assessment and passes to Risk Dept
    public async Task UnitApproveAsync(int assessmentUnitId, int userId, string userRole)
    {
        if (userRole != RoleUnitApprover && userRole != RoleAdmin)
        {
            throw new InvalidOperationException("Only Unit Approvers can authorize and pass assessments to the Risk Department.");
        }

        var unit = await LoadUnitForUpdateAsync(assessmentUnitId, userId, userRole);

        var notApproved = unit.AssessmentItems.Count(i => i.WorkflowStep!.StepCode != ItemApprovedStepCode);
        if (notApproved > 0)
        {
            throw new InvalidOperationException("All items must be approved by the Unit Approver before passing to the Risk Department.");
        }

        var step = await GetStepAsync(UnitWorkflowCode, UnitApprovedStepCode);
        unit.WorkflowStepId = step.WorkflowStepId;
        unit.UnitApprovedById = userId;
        unit.UnitApprovedOn = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    // Level 3: Risk Dept Reviewer reviews and passes to CRO / Final Approver
    public async Task RiskReviewAsync(int assessmentUnitId, int userId, string userRole, string? remarks)
    {
        if (userRole != RoleRiskReviewer && userRole != RoleAdmin)
        {
            throw new InvalidOperationException("Only Risk Department Reviewers can review assessments.");
        }

        var unit = await LoadUnitForUpdateAsync(assessmentUnitId, userId, userRole);

        if (unit.WorkflowStep!.StepCode != UnitApprovedStepCode)
        {
            throw new InvalidOperationException("Only unit-approved assessments can be reviewed by the Risk Department.");
        }

        var step = await GetStepAsync(UnitWorkflowCode, UnitRiskReviewedStepCode);
        unit.WorkflowStepId = step.WorkflowStepId;
        unit.RiskReviewedById = userId;
        unit.RiskReviewedOn = DateTime.UtcNow;
        unit.RiskReviewerRemarks = remarks;
        await _db.SaveChangesAsync();
    }

    // Level 4: Risk Dept Approver (CRO) gives final sign-off
    public async Task FinalApproveAsync(int assessmentUnitId, int userId, string userRole, string? remarks)
    {
        if (userRole != RoleRiskApprover && userRole != RoleAdmin)
        {
            throw new InvalidOperationException("Only Risk Department Approvers can provide final approval.");
        }

        var unit = await LoadUnitForUpdateAsync(assessmentUnitId, userId, userRole);

        if (unit.WorkflowStep!.StepCode != UnitRiskReviewedStepCode)
        {
            throw new InvalidOperationException("Only assessments that have undergone Risk Department review can receive final approval.");
        }

        var step = await GetStepAsync(UnitWorkflowCode, UnitFinalApprovedStepCode);
        unit.WorkflowStepId = step.WorkflowStepId;
        unit.FinalApprovedById = userId;
        unit.FinalApprovedOn = DateTime.UtcNow;
        unit.FinalApproverRemarks = remarks;
        await _db.SaveChangesAsync();
    }

    // Return to Unit Initiator for revisions (Levels 3 or 4)
    public async Task ReturnAssessmentAsync(int assessmentUnitId, int userId, string userRole, string? remarks)
    {
        if (!IsRiskOrAdminRole(userRole))
        {
            throw new InvalidOperationException("Only Risk Department staff or Administrators can return assessments for revision.");
        }

        var unit = await LoadUnitForUpdateAsync(assessmentUnitId, userId, userRole);

        if (unit.WorkflowStep!.StepCode == UnitFinalApprovedStepCode)
        {
            throw new InvalidOperationException("Final approved assessments cannot be returned.");
        }

        var step = await GetStepAsync(UnitWorkflowCode, UnitReturnedStepCode);
        unit.WorkflowStepId = step.WorkflowStepId;
        unit.RiskReviewerRemarks = string.IsNullOrWhiteSpace(remarks) ? unit.RiskReviewerRemarks : remarks;

        // Reset items to Pending so the Unit Initiator can edit and re-submit
        var pendingItemStep = await GetStepAsync(ItemWorkflowCode, ItemPendingStepCode);
        foreach (var item in unit.AssessmentItems)
        {
            item.WorkflowStepId = pendingItemStep.WorkflowStepId;
            item.ApprovedById = null;
            item.ApprovedOn = null;
        }

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

    private async Task<AssessmentUnit> LoadUnitForUpdateAsync(int assessmentUnitId, int userId, string userRole)
    {
        IQueryable<AssessmentUnit> query = _db.AssessmentUnits
            .Where(u => u.AssessmentUnitId == assessmentUnitId);

        if (!IsRiskOrAdminRole(userRole))
        {
            var unitId = await GetUserUnitIdAsync(userId);
            query = query.Where(u => u.UnitId == unitId);
        }

        return await query
            .Include(u => u.WorkflowStep)
            .Include(u => u.AssessmentItems).ThenInclude(i => i.WorkflowStep)
            .SingleOrDefaultAsync()
            ?? throw new InvalidOperationException("The requested assessment was not found.");
    }

    private async Task<AssessmentItem> LoadOwnedItemAsync(int assessmentItemId, int userId, string userRole = "")
    {
        return await QueryOwnedItemAsync(assessmentItemId, userId, userRole, asNoTracking: true)
            ?? throw new InvalidOperationException("The requested item was not found.");
    }

    private async Task<AssessmentItem> LoadOwnedItemForUpdateAsync(int assessmentItemId, int userId, string userRole = "")
    {
        return await QueryOwnedItemAsync(assessmentItemId, userId, userRole, asNoTracking: false)
            ?? throw new InvalidOperationException("The requested item was not found.");
    }

    private async Task<AssessmentItem?> QueryOwnedItemAsync(int assessmentItemId, int userId, string userRole, bool asNoTracking)
    {
        IQueryable<AssessmentItem> query = _db.AssessmentItems
            .Where(i => i.AssessmentItemId == assessmentItemId);

        if (!IsRiskOrAdminRole(userRole))
        {
            var unitId = await GetUserUnitIdAsync(userId);
            query = query.Where(i => i.AssessmentUnit!.UnitId == unitId);
        }

        query = query
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

        // An item is editable only while the unit is Pending/InProgress/Returned AND the item itself is not Submitted/Approved
        var unitAllowsEdit = unitStepCode == UnitPendingStepCode 
                          || unitStepCode == UnitInProgressStepCode 
                          || unitStepCode == UnitReturnedStepCode;

        return unitAllowsEdit
            && itemStepCode != ItemSubmittedStepCode
            && itemStepCode != ItemApprovedStepCode;
    }
}