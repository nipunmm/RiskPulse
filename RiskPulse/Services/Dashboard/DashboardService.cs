using Microsoft.EntityFrameworkCore;
using RiskPulse.Data;
using RiskPulse.Data.Entries;
using RiskPulse.Models.Enum;
using RiskPulse.Models.ViewModel;

namespace RiskPulse.Services.Dashboard;

public class DashboardService
{
    private const string ItemApprovedStepCode = "Approved";
    private const string ItemSubmittedStepCode = "Submitted";
    private const string UnitAuthorizedStepCode = "Authorized";
    private const string UnitWorkflowCode = "ASSESSMENT-UNIT";

    private readonly AppDbContext _db;

    public DashboardService(AppDbContext db)
    {
        _db = db;
    }

    // --- Landing page model ---
    public async Task<DashboardViewModel> GetDashboardAsync(int userId)
    {
        var unitId = await GetUserUnitIdAsync(userId);

        var units = await _db.AssessmentUnits.AsNoTracking()
            .Where(u => u.UnitId == unitId)
            .OrderByDescending(u => u.AssessmentHeader!.PeriodStart)
            .Select(u => new UnitSnapshot
            {
                AssessmentUnitId = u.AssessmentUnitId,
                AssessmentCode = u.AssessmentHeader!.AssessmentCode,
                UnitDesc = u.Unit!.UnitDesc,
                PeriodStart = u.AssessmentHeader.PeriodStart,
                PeriodEnd = u.AssessmentHeader.PeriodEnd,
                StepCode = u.WorkflowStep!.StepCode,
                StatusLabel = u.WorkflowStep.StepLabel,
                ItemCount = u.AssessmentItems.Count,
                SubmittedCount = u.AssessmentItems.Count(i => i.WorkflowStep!.StepCode == ItemSubmittedStepCode
                                                           || i.WorkflowStep!.StepCode == ItemApprovedStepCode),
                ApprovedCount = u.AssessmentItems.Count(i => i.WorkflowStep!.StepCode == ItemApprovedStepCode),
                AuthorizedBy = u.AuthorizedBy!.Username,
                AuthorizedOn = u.AuthorizedOn
            })
            .ToListAsync();

        var unitStepCodes = await _db.WorkflowSteps.AsNoTracking()
            .Where(s => s.Workflow!.WorkflowCode == UnitWorkflowCode)
            .OrderBy(s => s.StepOrder)
            .Select(s => new { s.StepCode, s.StepLabel })
            .ToListAsync();

        var statusCounts = units
            .GroupBy(u => u.StepCode)
            .ToDictionary(g => g.Key, g => g.Count());

        var statusSlices = unitStepCodes
            .Select(s => new DashboardStatusSliceViewModel
            {
                StepCode = s.StepCode,
                StepLabel = s.StepLabel,
                PillKind = StatusPillKind(s.StepCode),
                Count = statusCounts.GetValueOrDefault(s.StepCode)
            })
            .ToList();

        var latestPeriodStart = units.Where(u => u.PeriodStart.HasValue).Select(u => u.PeriodStart).DefaultIfEmpty().Max();
        var latestPeriodEnd = latestPeriodStart.HasValue
            ? units.Where(u => u.PeriodStart == latestPeriodStart).Select(u => u.PeriodEnd).FirstOrDefault()
            : null;

        var kriSnapshot = await BuildKriSnapshotAsync(units, latestPeriodStart);

        var attentionItems = await BuildAttentionItemsAsync(unitId);

        var openCount = units.Count(u => u.StepCode != UnitAuthorizedStepCode);
        var readyCount = units.Count(u => u.StepCode != UnitAuthorizedStepCode && u.ItemCount > 0 && u.ApprovedCount == u.ItemCount);

        var kpis = new List<DashboardKpiViewModel>
        {
            new DashboardKpiViewModel
            {
                Icon = "fa-layer-group",
                Label = "Assessments",
                Value = units.Count,
                PillText = $"{openCount} open",
                PillKind = openCount > 0 ? "warning" : "success"
            },
            new DashboardKpiViewModel
            {
                Icon = "fa-bell",
                Label = "Needs your attention",
                Value = attentionItems.Count,
                PillText = attentionItems.Count > 0 ? "take action" : "all clear",
                PillKind = attentionItems.Count > 0 ? "danger" : "neutral"
            },
            new DashboardKpiViewModel
            {
                Icon = "fa-check-double",
                Label = "Ready to authorize",
                Value = readyCount,
                PillText = readyCount > 0 ? "ready" : "waiting",
                PillKind = readyCount > 0 ? "success" : "neutral"
            },
            new DashboardKpiViewModel
            {
                Icon = "fa-triangle-exclamation",
                Label = "KRI red flags",
                Value = kriSnapshot.RedCount,
                DotKind = kriSnapshot.RedCount > 0 ? "danger" : "neutral",
                PillText = kriSnapshot.HasData ? "latest period" : "no data",
                PillKind = "neutral"
            }
        };

        return new DashboardViewModel
        {
            UnitDesc = units.FirstOrDefault()?.UnitDesc ?? string.Empty,
            CurrentPeriodLabel = latestPeriodStart.HasValue
                ? FormatPeriod(latestPeriodStart.Value, latestPeriodEnd)
                : "No scheduled assessments yet",
            Kpis = kpis,
            StatusSlices = statusSlices,
            AssessmentProgress = units
                .Select(u => new AssessmentProgressRowViewModel
                {
                    AssessmentUnitId = u.AssessmentUnitId,
                    AssessmentCode = u.AssessmentCode,
                    PeriodStart = u.PeriodStart,
                    PeriodEnd = u.PeriodEnd,
                    StatusLabel = u.StatusLabel,
                    StatusKind = StatusPillKind(u.StepCode),
                    ItemCount = u.ItemCount,
                    SubmittedCount = u.SubmittedCount
                })
                .ToList(),
            KriSnapshot = kriSnapshot,
            AttentionItems = attentionItems,
            PeriodHistory = units
                .Select(u => new PeriodHistoryRowViewModel
                {
                    AssessmentUnitId = u.AssessmentUnitId,
                    AssessmentCode = u.AssessmentCode,
                    PeriodStart = u.PeriodStart,
                    PeriodEnd = u.PeriodEnd,
                    StatusLabel = u.StatusLabel,
                    StatusKind = StatusPillKind(u.StepCode),
                    ItemCount = u.ItemCount,
                    SubmittedCount = u.SubmittedCount,
                    ApprovedCount = u.ApprovedCount,
                    ReadyToAuthorize = u.StepCode != UnitAuthorizedStepCode && u.ItemCount > 0 && u.ApprovedCount == u.ItemCount,
                    AuthorizedBy = u.AuthorizedBy ?? string.Empty,
                    AuthorizedOn = u.AuthorizedOn
                })
                .ToList()
        };
    }

    // --- KRI RAG partition for the latest period ---
    private async Task<KriSnapshotViewModel> BuildKriSnapshotAsync(List<UnitSnapshot> units, DateTime? latestPeriodStart)
    {
        if (!latestPeriodStart.HasValue)
        {
            return new KriSnapshotViewModel();
        }

        var latestUnitIds = units
            .Where(u => u.PeriodStart == latestPeriodStart)
            .Select(u => u.AssessmentUnitId)
            .ToList();

        var latestKriHeaderIds = await _db.AssessmentItems.AsNoTracking()
            .Where(i => latestUnitIds.Contains(i.AssessmentUnitId) && i.ItemType == ScheduleItemType.Kri)
            .Select(i => i.ItemId)
            .Distinct()
            .ToListAsync();

        var expectedCount = latestKriHeaderIds.Count > 0
            ? await _db.Kris.AsNoTracking().CountAsync(k => latestKriHeaderIds.Contains(k.KriHeaderId))
            : 0;

        var values = await _db.KriAssessmentValues.AsNoTracking()
            .Where(v => latestUnitIds.Contains(v.AssessmentItem!.AssessmentUnitId))
            .Select(v => new { v.Value, v.Kri!.GreenLimit, v.Kri.AmberLimit })
            .ToListAsync();

        var greenCount = 0;
        var amberCount = 0;
        var redCount = 0;

        foreach (var value in values)
        {
            if (value.Value <= value.GreenLimit) greenCount++;
            else if (value.Value <= value.AmberLimit) amberCount++;
            else redCount++;
        }

        return new KriSnapshotViewModel
        {
            PeriodLabel = FormatPeriod(latestPeriodStart.Value, units.First(u => u.PeriodStart == latestPeriodStart).PeriodEnd),
            GreenCount = greenCount,
            AmberCount = amberCount,
            RedCount = redCount,
            EnteredCount = values.Count,
            ExpectedCount = expectedCount
        };
    }

    // --- Items awaiting action (fill or approve), own unit only ---
    private async Task<List<DashboardAttentionRowViewModel>> BuildAttentionItemsAsync(int unitId)
    {
        var items = await _db.AssessmentItems.AsNoTracking()
            .Where(i => i.AssessmentUnit!.UnitId == unitId
                     && i.AssessmentUnit.WorkflowStep!.StepCode != UnitAuthorizedStepCode
                     && i.WorkflowStep!.StepCode != ItemApprovedStepCode)
            .OrderBy(i => i.AssessmentUnit!.AssessmentHeader!.PeriodEnd)
            .Select(i => new AttentionItemSnapshot
            {
                AssessmentItemId = i.AssessmentItemId,
                AssessmentUnitId = i.AssessmentUnitId,
                ItemType = i.ItemType,
                ReferenceId = i.ItemId,
                AssessmentCode = i.AssessmentUnit!.AssessmentHeader!.AssessmentCode,
                PeriodEnd = i.AssessmentUnit.AssessmentHeader.PeriodEnd,
                ItemStepCode = i.WorkflowStep!.StepCode,
                StatusLabel = i.WorkflowStep.StepLabel
            })
            .ToListAsync();

        var saqIds = items.Where(i => i.ItemType == ScheduleItemType.Saq).Select(i => i.ReferenceId).Distinct().ToList();
        var kriIds = items.Where(i => i.ItemType == ScheduleItemType.Kri).Select(i => i.ReferenceId).Distinct().ToList();

        var saqTitles = await _db.SaqHeaders.AsNoTracking()
            .Where(h => saqIds.Contains(h.SaqHeaderId))
            .ToDictionaryAsync(h => h.SaqHeaderId, h => new { h.SaqDesc, h.SaqCode });

        var kriTitles = await _db.KriHeaders.AsNoTracking()
            .Where(h => kriIds.Contains(h.KriHeaderId))
            .ToDictionaryAsync(h => h.KriHeaderId, h => new { h.KriHeaderDesc, h.KriCode });

        var nowUtc = DateTime.UtcNow;

        return items.Select(i =>
        {
            string title = "Unknown";
            string referenceCode = string.Empty;
            string itemType = i.ItemType.ToString();

            if (i.ItemType == ScheduleItemType.Saq && saqTitles.TryGetValue(i.ReferenceId, out var saq))
            {
                title = saq.SaqDesc;
                referenceCode = saq.SaqCode ?? string.Empty;
                itemType = "Saq";
            }
            else if (i.ItemType == ScheduleItemType.Kri && kriTitles.TryGetValue(i.ReferenceId, out var kri))
            {
                title = kri.KriHeaderDesc ?? string.Empty;
                referenceCode = kri.KriCode ?? string.Empty;
                itemType = "Kri";
            }

            var awaitingApproval = i.ItemStepCode == ItemSubmittedStepCode;

            return new DashboardAttentionRowViewModel
            {
                AssessmentUnitId = i.AssessmentUnitId,
                AssessmentItemId = i.AssessmentItemId,
                AssessmentCode = i.AssessmentCode,
                ItemTitle = title,
                ReferenceCode = referenceCode,
                ItemType = itemType,
                ActionKind = awaitingApproval ? "approve" : "fill",
                ActionLabel = awaitingApproval ? "Approve" : "Fill",
                StepLabel = i.StatusLabel,
                PeriodEnd = i.PeriodEnd,
                IsOverdue = i.PeriodEnd.HasValue && i.PeriodEnd.Value < nowUtc
            };
        }).ToList();
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

    private static string StatusPillKind(string stepCode) => stepCode switch
    {
        "Pending" => "neutral",
        "InProgress" => "warning",
        "Submitted" => "warning",
        "Approved" => "success",
        "Authorized" => "success",
        _ => "neutral"
    };

    private static string FormatPeriod(DateTime start, DateTime? end)
        => end.HasValue ? $"{start:dd MMM yyyy} \u2192 {end.Value:dd MMM yyyy}" : start.ToString("dd MMM yyyy");

    private class UnitSnapshot
    {
        public int AssessmentUnitId { get; set; }

        public string AssessmentCode { get; set; } = string.Empty;

        public string UnitDesc { get; set; } = string.Empty;

        public DateTime? PeriodStart { get; set; }

        public DateTime? PeriodEnd { get; set; }

        public string StepCode { get; set; } = string.Empty;

        public string StatusLabel { get; set; } = string.Empty;

        public int ItemCount { get; set; }

        public int SubmittedCount { get; set; }

        public int ApprovedCount { get; set; }

        public string? AuthorizedBy { get; set; }

        public DateTime? AuthorizedOn { get; set; }
    }

    private class AttentionItemSnapshot
    {
        public int AssessmentItemId { get; set; }

        public int AssessmentUnitId { get; set; }

        public ScheduleItemType ItemType { get; set; }

        public int ReferenceId { get; set; }

        public string AssessmentCode { get; set; } = string.Empty;

        public DateTime? PeriodEnd { get; set; }

        public string ItemStepCode { get; set; } = string.Empty;

        public string StatusLabel { get; set; } = string.Empty;
    }
}