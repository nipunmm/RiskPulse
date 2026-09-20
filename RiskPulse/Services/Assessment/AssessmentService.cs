using Microsoft.EntityFrameworkCore;
using RiskPulse.Data;
using RiskPulse.Data.Entries;
using RiskPulse.Models.Enum;
using RiskPulse.Services.Utilities;

namespace RiskPulse.Services.Assessment;

public class AssessmentService
{
    private readonly AppDbContext _db;
    private readonly CodeGeneratorService _codeService;

    public AssessmentService(AppDbContext db, CodeGeneratorService codeService)
    {
        _db = db;
        _codeService = codeService;
    }

    // --- Assessment generation (triggered by schedule activation) ---
    // Adds header/unit/item rows only; the caller owns the transaction (single SaveChanges).
    public async Task CreateAssessmentAsync(int scheduleId)
    {
        var schedule = await _db.Schedules
            .Include(s => s.ScheduleItems)
            .SingleOrDefaultAsync(s => s.ScheduleId == scheduleId)
            ?? throw new InvalidOperationException($"Schedule with Id {scheduleId} was not found.");

        if (schedule.ScheduleType != ScheduleType.OneTime)
        {
            throw new InvalidOperationException("Recurring schedules are not supported yet; only one-time schedules can be activated.");
        }

        var unitWorkflowStep = await GetInitialStepAsync("ASSESSMENT-UNIT");
        var itemWorkflowStep = await GetInitialStepAsync("ASSESSMENT-ITEM");

        var duplicate = await _db.AssessmentHeaders
            .AnyAsync(h => h.ScheduleId == scheduleId && h.PeriodStart == schedule.StartDate);
        if (duplicate)
        {
            throw new InvalidOperationException("An assessment already exists for this schedule and period.");
        }

        var unitItems = await ResolveUnitItemsAsync(schedule.ScheduleItems);

        var assessment = new AssessmentHeader
        {
            ScheduleId = scheduleId,
            AssessmentCode = await _codeService.GenerateAssessmentCodeAsync(),
            PeriodStart = ToUtc(schedule.StartDate),
            PeriodEnd = ToUtc(schedule.EndDate),
            AssessmentStatus = AssessmentStatus.Pending
        };

        foreach (var (unitId, items) in unitItems)
        {
            assessment.AssessmentUnits.Add(new AssessmentUnit
            {
                UnitId = unitId,
                WorkflowStepId = unitWorkflowStep.WorkflowStepId,
                AssessmentItems = items
                    .Select(i => new AssessmentItem
                    {
                        ItemType = i.ItemType,
                        ItemId = i.ItemId,
                        WorkflowStepId = itemWorkflowStep.WorkflowStepId
                    })
                    .ToList()
            });
        }

        _db.AssessmentHeaders.Add(assessment);
    }

    private async Task<WorkflowStep> GetInitialStepAsync(string workflowCode)
    {
        return await _db.WorkflowSteps
            .AsNoTracking()
            .Where(s => s.Workflow!.WorkflowCode == workflowCode && s.IsInitial)
            .OrderBy(s => s.WorkflowStepId)
            .FirstOrDefaultAsync()
            ?? throw new InvalidOperationException("Assessment workflow steps have not been configured yet. Add workflow definitions before activating schedules.");
    }

    private async Task<Dictionary<int, List<(ScheduleItemType ItemType, int ItemId)>>> ResolveUnitItemsAsync(ICollection<ScheduleItem> scheduleItems)
    {
        var saqIds = scheduleItems.Where(i => i.ItemType == ScheduleItemType.Saq).Select(i => i.ItemId).Distinct().ToList();
        var kriIds = scheduleItems.Where(i => i.ItemType == ScheduleItemType.Kri).Select(i => i.ItemId).Distinct().ToList();

        var saqHeaders = await _db.SaqHeaders.AsNoTracking()
            .Where(h => saqIds.Contains(h.SaqHeaderId))
            .Select(h => new { h.SaqHeaderId, h.GroupId, h.UnitId })
            .ToDictionaryAsync(h => h.SaqHeaderId);

        var kriHeaders = await _db.KriHeaders.AsNoTracking()
            .Where(h => kriIds.Contains(h.KriHeaderId))
            .Select(h => new { h.KriHeaderId, h.GroupId, h.UnitId })
            .ToDictionaryAsync(h => h.KriHeaderId);

        var result = new Dictionary<int, List<(ScheduleItemType, int)>>();

        foreach (var item in scheduleItems)
        {
            int? groupId;
            int? unitId;

            if (item.ItemType == ScheduleItemType.Saq)
            {
                if (!saqHeaders.TryGetValue(item.ItemId, out var header)) continue;
                groupId = header.GroupId;
                unitId = header.UnitId;
            }
            else
            {
                if (!kriHeaders.TryGetValue(item.ItemId, out var header)) continue;
                groupId = header.GroupId;
                unitId = header.UnitId;
            }

            var targetUnitIds = new List<int>();
            if (unitId.HasValue)
            {
                targetUnitIds.Add(unitId.Value);
            }
            else if (groupId.HasValue)
            {
                targetUnitIds.AddRange(await _db.UnitGroups.AsNoTracking()
                    .Where(ug => ug.GroupId == groupId.Value)
                    .Select(ug => ug.UnitId)
                    .ToListAsync());
            }
            else
            {
                continue;
            }

            foreach (var targetUnitId in targetUnitIds.Distinct())
            {
                if (!result.TryGetValue(targetUnitId, out var list))
                {
                    list = new List<(ScheduleItemType, int)>();
                    result[targetUnitId] = list;
                }

                if (!list.Contains((item.ItemType, item.ItemId)))
                {
                    list.Add((item.ItemType, item.ItemId));
                }
            }
        }

        return result;
    }

    private static DateTime? ToUtc(DateTime? value)
    {
        return value.HasValue ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc) : null;
    }
}