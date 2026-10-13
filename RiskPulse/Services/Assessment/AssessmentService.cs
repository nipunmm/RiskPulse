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

    // --- Generate (snapshot) the real assessment for an activation from its Assessment Template ---
    // Copies the template's SAQ/KRI membership into the assessment so later template edits do not
    // affect assessments that were already generated. One assessment is produced per activation.
    public async Task<int> CreateAssessmentAsync(int activationId)
    {
        var activation = await _db.AssessmentActivations
            .Include(a => a.AssessmentTemplate)
                .ThenInclude(t => t!.AssessmentTemplateItems)
            .SingleOrDefaultAsync(a => a.AssessmentActivationId == activationId)
            ?? throw new InvalidOperationException($"Assessment activation with Id {activationId} was not found.");

        if (activation.AssessmentTemplate == null)
        {
            throw new InvalidOperationException("Select an assessment template before activating.");
        }

        var templateItems = activation.AssessmentTemplate.AssessmentTemplateItems
            .Select(i => (i.ItemType, i.ItemId))
            .ToList();

        if (templateItems.Count == 0)
        {
            throw new InvalidOperationException("The selected assessment template has no SAQ or KRI templates.");
        }

        var duplicate = await _db.AssessmentHeaders
            .AnyAsync(h => h.AssessmentActivationId == activationId && h.PeriodStart == ToUtc(activation.StartDate));
        if (duplicate)
        {
            throw new InvalidOperationException("An assessment already exists for this activation.");
        }

        var unitWorkflowStep = await GetInitialStepAsync(WorkflowStepCodes.AssessmentUnitWorkflow);
        var itemWorkflowStep = await GetInitialStepAsync(WorkflowStepCodes.AssessmentItemWorkflow);

        var unitItems = await ResolveUnitItemsAsync(templateItems);

        var assessment = new AssessmentHeader
        {
            AssessmentActivationId = activation.AssessmentActivationId,
            AssessmentCode = await _codeService.GenerateAssessmentCodeAsync(),
            PeriodStart = ToUtc(activation.StartDate),
            PeriodEnd = ToUtc(activation.EndDate),
            AssessmentStatus = AssessmentStatus.Pending
        };

        foreach (var (unitId, items) in unitItems)
        {
            var unit = new AssessmentUnit
            {
                UnitId = unitId,
                WorkflowStepId = unitWorkflowStep.WorkflowStepId
            };

            foreach (var (itemType, itemId) in items)
            {
                unit.AssessmentItems.Add(new AssessmentItem
                {
                    ItemType = itemType,
                    ItemId = itemId,
                    WorkflowStepId = itemWorkflowStep.WorkflowStepId
                });
            }

            assessment.AssessmentUnits.Add(unit);
        }

        _db.AssessmentHeaders.Add(assessment);
        await _db.SaveChangesAsync();

        return assessment.AssessmentHeaderId;
    }

    private async Task<WorkflowStep> GetInitialStepAsync(string workflowCode)
    {
        return await _db.WorkflowSteps
            .AsNoTracking()
            .Where(s => s.Workflow!.WorkflowCode == workflowCode && s.IsInitial)
            .OrderBy(s => s.WorkflowStepId)
            .FirstOrDefaultAsync()
            ?? throw new InvalidOperationException("Assessment workflow steps have not been configured yet. Add workflow definitions before activating assessments.");
    }

    private async Task<Dictionary<int, List<(AssessmentItemType ItemType, int ItemId)>>> ResolveUnitItemsAsync(
        IReadOnlyCollection<(AssessmentItemType ItemType, int ItemId)> templateItems)
    {
        var saqIds = templateItems.Where(i => i.ItemType == AssessmentItemType.Saq).Select(i => i.ItemId).Distinct().ToList();
        var kriIds = templateItems.Where(i => i.ItemType == AssessmentItemType.Kri).Select(i => i.ItemId).Distinct().ToList();

        var saqHeaders = await _db.SaqHeaders.AsNoTracking()
            .Where(h => saqIds.Contains(h.SaqHeaderId))
            .Select(h => new { h.SaqHeaderId, h.GroupId, h.UnitId })
            .ToDictionaryAsync(h => h.SaqHeaderId);

        var kriHeaders = await _db.KriHeaders.AsNoTracking()
            .Where(h => kriIds.Contains(h.KriHeaderId))
            .Select(h => new { h.KriHeaderId, h.GroupId, h.UnitId })
            .ToDictionaryAsync(h => h.KriHeaderId);

        var result = new Dictionary<int, List<(AssessmentItemType, int)>>();

        foreach (var item in templateItems)
        {
            int? groupId;
            int? unitId;

            if (item.ItemType == AssessmentItemType.Saq)
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
                    list = new List<(AssessmentItemType, int)>();
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
