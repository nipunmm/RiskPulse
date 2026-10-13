using Microsoft.EntityFrameworkCore;
using RiskPulse.Data;
using RiskPulse.Models.Enum;
using RiskPulse.Models.ViewModel;
using RiskPulse.Services.Assessment;

namespace RiskPulse.Services.Monitoring;

// --- Cross-unit monitoring view (units x assessments status matrix) ---
public class MonitorService
{
    private const int MaxColumns = 12;

    private const string SubmittedStepCode = WorkflowStepCodes.Submitted;
    private const string ApprovedStepCode = WorkflowStepCodes.Approved;
    private const string FinalApprovedStepCode = WorkflowStepCodes.FinalApproved;
    private const string UnitWorkflowCode = WorkflowStepCodes.AssessmentUnitWorkflow;

    private readonly AppDbContext _db;

    public MonitorService(AppDbContext db)
    {
        _db = db;
    }

    // --- Matrix model (rows = units, columns = assessments, cells = unit status) ---
    public async Task<MonitorIndexViewModel> GetMonitorAsync(int? assessmentHeaderId, UnitType? unitType)
    {
        var allHeaders = await _db.AssessmentHeaders.AsNoTracking()
            .Select(h => new HeaderSnapshot
            {
                AssessmentHeaderId = h.AssessmentHeaderId,
                AssessmentCode = h.AssessmentCode,
                PeriodStart = h.PeriodStart,
                PeriodEnd = h.PeriodEnd,
                UnitCount = h.AssessmentUnits.Count
            })
            .ToListAsync();

        allHeaders = allHeaders
            .OrderByDescending(h => h.PeriodStart)
            .ThenByDescending(h => h.AssessmentHeaderId)
            .ToList();

        var assessmentOptions = allHeaders
            .Select(h => new OptionViewModel
            {
                Value = h.AssessmentHeaderId,
                Code = h.AssessmentCode,
                Label = FormatPeriod(h.PeriodStart, h.PeriodEnd)
            })
            .ToList();

        var headers = assessmentHeaderId.HasValue
            ? allHeaders.Where(h => h.AssessmentHeaderId == assessmentHeaderId.Value).ToList()
            : allHeaders;

        var truncated = false;
        if (!assessmentHeaderId.HasValue && headers.Count > MaxColumns)
        {
            headers = headers.Take(MaxColumns).ToList();
            truncated = true;
        }

        var unitQuery = _db.Units.AsNoTracking();
        if (unitType.HasValue)
        {
            unitQuery = unitQuery.Where(u => u.UnitType == unitType.Value);
        }

        var units = await unitQuery
            .OrderBy(u => u.UnitType == UnitType.Branch ? 0 : 1)
            .ThenBy(u => u.UnitDesc)
            .Select(u => new UnitSnapshot
            {
                UnitId = u.UnitId,
                UnitCode = u.UnitCode,
                UnitDesc = u.UnitDesc,
                UnitType = u.UnitType
            })
            .ToListAsync();

        var headerIds = headers.Select(h => h.AssessmentHeaderId).ToList();
        var unitIds = units.Select(u => u.UnitId).ToList();

        var cells = await _db.AssessmentUnits.AsNoTracking()
            .Where(au => headerIds.Contains(au.AssessmentHeaderId) && unitIds.Contains(au.UnitId))
            .Select(au => new CellSnapshot
            {
                AssessmentUnitId = au.AssessmentUnitId,
                AssessmentHeaderId = au.AssessmentHeaderId,
                UnitId = au.UnitId,
                StepCode = au.WorkflowStep!.StepCode,
                StatusLabel = au.WorkflowStep!.StepLabel,
                ItemCount = au.AssessmentItems.Count,
                SubmittedCount = au.AssessmentItems.Count(i => i.WorkflowStep!.StepCode == SubmittedStepCode
                                                           || i.WorkflowStep!.StepCode == ApprovedStepCode)
            })
            .ToListAsync();

        var periodEndById = headers.ToDictionary(h => h.AssessmentHeaderId, h => h.PeriodEnd);
        var now = DateTime.UtcNow;

        var cellLookup = cells
            .GroupBy(c => (c.AssessmentHeaderId, c.UnitId))
            .ToDictionary(g => g.Key, g =>
            {
                var cell = g.Last();
                return new MonitorCellViewModel
                {
                    AssessmentUnitId = cell.AssessmentUnitId,
                    StepCode = cell.StepCode,
                    StatusLabel = cell.StatusLabel,
                    StatusKind = StatusPillKind(cell.StepCode),
                    ItemCount = cell.ItemCount,
                    SubmittedCount = cell.SubmittedCount,
                    IsOverdue = cell.StepCode != FinalApprovedStepCode
                        && periodEndById.TryGetValue(cell.AssessmentHeaderId, out var end)
                        && end.HasValue
                        && end.Value < now
                };
            });

        var columns = headers
            .Select(h => new MonitorAssessmentColumnViewModel
            {
                AssessmentHeaderId = h.AssessmentHeaderId,
                AssessmentCode = h.AssessmentCode,
                PeriodStart = h.PeriodStart,
                PeriodEnd = h.PeriodEnd,
                UnitCount = h.UnitCount,
                CompletedCount = cells.Count(c => c.AssessmentHeaderId == h.AssessmentHeaderId
                                                && c.StepCode == FinalApprovedStepCode)
            })
            .ToList();

        var rows = units
            .Select(u =>
            {
                var rowCells = headers
                    .Select(h => cellLookup.GetValueOrDefault((h.AssessmentHeaderId, u.UnitId)))
                    .ToList();

                return new MonitorUnitRowViewModel
                {
                    UnitId = u.UnitId,
                    UnitCode = u.UnitCode,
                    UnitDesc = u.UnitDesc,
                    UnitType = u.UnitType,
                    Cells = rowCells,
                    TotalCount = rowCells.Count(c => c != null),
                    CompletedCount = rowCells.Count(c => c != null && c.StepCode == FinalApprovedStepCode)
                };
            })
            .ToList();

        var cellList = cellLookup.Values.ToList();
        var completed = cellList.Count(c => c.StepCode == FinalApprovedStepCode);
        var notStarted = cellList.Count(c => c.StepCode == WorkflowStepCodes.Pending);
        var returned = cellList.Count(c => c.StepCode == WorkflowStepCodes.Returned);

        var slices = await BuildSlicesAsync(cellList);

        return new MonitorIndexViewModel
        {
            Assessments = columns,
            AssessmentOptions = assessmentOptions,
            Units = rows,
            Slices = slices,
            AssessmentHeaderId = assessmentHeaderId,
            UnitType = unitType,
            Truncated = truncated,
            Summary = new MonitorSummaryViewModel
            {
                UnitCount = units.Count,
                AssessmentCount = columns.Count,
                TargetCount = cellList.Count,
                CompletedCount = completed,
                InProgressCount = cellList.Count - completed - notStarted - returned,
                NotStartedCount = notStarted,
                ReturnedCount = returned,
                OverdueCount = cellList.Count(c => c.IsOverdue)
            }
        };
    }

    // --- Legend / status distribution ---
    private async Task<List<MonitorStatusSliceViewModel>> BuildSlicesAsync(List<MonitorCellViewModel> cells)
    {
        var unitStepCodes = await _db.WorkflowSteps.AsNoTracking()
            .Where(s => s.Workflow!.WorkflowCode == UnitWorkflowCode)
            .OrderBy(s => s.StepOrder)
            .Select(s => new { s.StepCode, s.StepLabel })
            .ToListAsync();

        var counts = cells
            .GroupBy(c => c.StepCode)
            .ToDictionary(g => g.Key, g => g.Count());

        return unitStepCodes
            .Select(s => new MonitorStatusSliceViewModel
            {
                StepCode = s.StepCode,
                StepLabel = s.StepLabel,
                PillKind = StatusPillKind(s.StepCode),
                Count = counts.GetValueOrDefault(s.StepCode)
            })
            .ToList();
    }

    private static string StatusPillKind(string stepCode) => stepCode switch
    {
        WorkflowStepCodes.Pending => "neutral",
        WorkflowStepCodes.InProgress => "warning",
        WorkflowStepCodes.UnitApproved => "info",
        WorkflowStepCodes.RiskReviewed => "info",
        WorkflowStepCodes.FinalApproved => "success",
        WorkflowStepCodes.Returned => "danger",
        _ => "neutral"
    };

    private static string FormatPeriod(DateTime? start, DateTime? end)
    {
        if (!start.HasValue)
        {
            return "No period";
        }

        return end.HasValue
            ? $"{start.Value:dd MMM yy} \u2192 {end.Value:dd MMM yy}"
            : start.Value.ToString("dd MMM yy");
    }

    private class HeaderSnapshot
    {
        public int AssessmentHeaderId { get; set; }

        public string AssessmentCode { get; set; } = string.Empty;

        public DateTime? PeriodStart { get; set; }

        public DateTime? PeriodEnd { get; set; }

        public int UnitCount { get; set; }
    }

    private class UnitSnapshot
    {
        public int UnitId { get; set; }

        public string UnitCode { get; set; } = string.Empty;

        public string UnitDesc { get; set; } = string.Empty;

        public UnitType UnitType { get; set; }
    }

    private class CellSnapshot
    {
        public int AssessmentUnitId { get; set; }

        public int AssessmentHeaderId { get; set; }

        public int UnitId { get; set; }

        public string StepCode { get; set; } = string.Empty;

        public string StatusLabel { get; set; } = string.Empty;

        public int ItemCount { get; set; }

        public int SubmittedCount { get; set; }
    }
}
