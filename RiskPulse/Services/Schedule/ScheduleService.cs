using Microsoft.EntityFrameworkCore;
using RiskPulse.Data;
using RiskPulse.Data.Entries;
using RiskPulse.Data.Extensions;
using RiskPulse.Models.Dto;
using RiskPulse.Models.Enum;
using RiskPulse.Models.ViewModel;
using RiskPulse.Services.Assessment;
using RiskPulse.Services.Utilities;
using ScheduleEntity = RiskPulse.Data.Entries.Schedule;

namespace RiskPulse.Services.Schedule;

public class ScheduleService
{
    private readonly AppDbContext _db;
    private readonly CodeGeneratorService _codeService;
    private readonly AssessmentService _assessmentService;

    public ScheduleService(AppDbContext db, CodeGeneratorService codeService, AssessmentService assessmentService)
    {
        _db = db;
        _codeService = codeService;
        _assessmentService = assessmentService;
    }

    // --- Schedule grid ---
    public async Task<List<ScheduleGridRowViewModel>> GetAllAsync()
    {
        var schedules = await _db.Schedules
            .AsNoTracking()
            .OrderByDescending(s => s.ScheduleId)
            .Select(s => new
            {
                s.ScheduleId,
                s.ScheduleCode,
                s.ScheduleType,
                s.ScheduleStatus,
                s.StartDate,
                s.EndDate,
                s.StartMonth,
                s.RecurringDay
            })
            .ToListAsync();

        var items = await _db.ScheduleItems
            .AsNoTracking()
            .ToListAsync();

        var saqIds = items.Where(i => i.ItemType == ScheduleItemType.Saq).Select(i => i.ItemId).Distinct().ToList();
        var kriIds = items.Where(i => i.ItemType == ScheduleItemType.Kri).Select(i => i.ItemId).Distinct().ToList();

        var saqHeaders = await _db.SaqHeaders.AsNoTracking()
            .Where(h => saqIds.Contains(h.SaqHeaderId))
            .ToDictionaryAsync(h => h.SaqHeaderId);
        var kriHeaders = await _db.KriHeaders.AsNoTracking()
            .Where(h => kriIds.Contains(h.KriHeaderId))
            .ToDictionaryAsync(h => h.KriHeaderId);

        return schedules.Select(s =>
        {
            var saqCodes = items
                .Where(i => i.ScheduleId == s.ScheduleId && i.ItemType == ScheduleItemType.Saq)
                .Select(i => saqHeaders.TryGetValue(i.ItemId, out var h)
                    ? HeaderLabel(h.SaqDesc, h.SaqCode)
                    : null)
                .Where(l => l != null)
                .OrderBy(l => l)
                .Select(l => l!)
                .ToList();

            var kriCodes = items
                .Where(i => i.ScheduleId == s.ScheduleId && i.ItemType == ScheduleItemType.Kri)
                .Select(i => kriHeaders.TryGetValue(i.ItemId, out var h)
                    ? HeaderLabel(h.KriHeaderDesc, h.KriCode)
                    : null)
                .Where(l => l != null)
                .OrderBy(l => l)
                .Select(l => l!)
                .ToList();

            return new ScheduleGridRowViewModel
            {
                ScheduleId = s.ScheduleId,
                ScheduleCode = s.ScheduleCode,
                SaqCount = saqCodes.Count,
                SaqCodes = saqCodes,
                KriCount = kriCodes.Count,
                KriCodes = kriCodes,
                ScheduleType = s.ScheduleType.ToString(),
                StartDate = s.StartDate,
                EndDate = s.EndDate,
                StartMonth = s.StartMonth,
                RecurringDay = s.RecurringDay,
                ScheduleStatus = s.ScheduleStatus.ToString()
            };
        }).ToList();
    }

    // --- Wizard ---
    public async Task<ScheduleWizardViewModel> GetWizardAsync(int scheduleId)
    {
        var saqOptions = await GetSaqOptionsAsync();
        var kriOptions = await GetKriOptionsAsync();

        if (scheduleId == 0)
        {
            return new ScheduleWizardViewModel
            {
                SaqOptions = saqOptions,
                KriOptions = kriOptions,
                CanEdit = true
            };
        }

        var schedule = await _db.Schedules
            .AsNoTracking()
            .SingleOrDefaultAsync(s => s.ScheduleId == scheduleId)
            ?? throw new InvalidOperationException($"Schedule with Id {scheduleId} was not found.");

        var items = await _db.ScheduleItems
            .AsNoTracking()
            .Where(si => si.ScheduleId == scheduleId)
            .ToListAsync();

        var saqIds = items.Where(i => i.ItemType == ScheduleItemType.Saq).Select(i => i.ItemId).Distinct().ToList();
        var kriIds = items.Where(i => i.ItemType == ScheduleItemType.Kri).Select(i => i.ItemId).Distinct().ToList();

        var saqHeaders = await _db.SaqHeaders.AsNoTracking()
            .Where(h => saqIds.Contains(h.SaqHeaderId))
            .ToDictionaryAsync(h => h.SaqHeaderId);
        var kriHeaders = await _db.KriHeaders.AsNoTracking()
            .Where(h => kriIds.Contains(h.KriHeaderId))
            .ToDictionaryAsync(h => h.KriHeaderId);

        var orderedSaqIds = saqIds
            .Where(id => saqHeaders.ContainsKey(id))
            .OrderBy(id => HeaderLabel(saqHeaders[id].SaqDesc, saqHeaders[id].SaqCode))
            .ToList();
        var orderedKriIds = kriIds
            .Where(id => kriHeaders.ContainsKey(id))
            .OrderBy(id => HeaderLabel(kriHeaders[id].KriHeaderDesc, kriHeaders[id].KriCode))
            .ToList();

        return new ScheduleWizardViewModel
        {
            ScheduleId = schedule.ScheduleId,
            ScheduleCode = schedule.ScheduleCode,
            ScheduleType = schedule.ScheduleType,
            StartDate = schedule.StartDate,
            EndDate = schedule.EndDate,
            StartMonth = schedule.StartMonth,
            RecurringDay = schedule.RecurringDay,
            SaqHeaderIds = orderedSaqIds,
            KriHeaderIds = orderedKriIds,
            SaqOptions = saqOptions,
            KriOptions = kriOptions,
            CompletedSaq = orderedSaqIds.Count > 0,
            CompletedKri = orderedKriIds.Count > 0,
            CompletedSchedule = true,
            CanEdit = schedule.ScheduleStatus == ScheduleStatus.Draft
        };
    }

    // --- Step 1: schedule type/details (create or update) ---
    public async Task<SaveResultDto> CreateOrUpdateScheduleAsync(ScheduleSaveDto model)
    {
        ValidateScheduleModel(model);

        ScheduleEntity schedule;

        if (model.ScheduleId == 0)
        {
            schedule = new ScheduleEntity
            {
                ScheduleCode = await _codeService.GenerateScheduleCodeAsync(),
                ScheduleStatus = ScheduleStatus.Draft
            };
            _db.Schedules.Add(schedule);
        }
        else
        {
            schedule = await RequireDraftAsync(model.ScheduleId);
        }

        schedule.ScheduleType = model.ScheduleType;
        // timestamptz columns require UTC DateTime values; the JSON binder yields Kind=Unspecified.
        schedule.StartDate = model.StartDate.HasValue
            ? DateTime.SpecifyKind(model.StartDate.Value, DateTimeKind.Utc)
            : null;
        schedule.EndDate = model.EndDate.HasValue
            ? DateTime.SpecifyKind(model.EndDate.Value, DateTimeKind.Utc)
            : null;
        schedule.StartMonth = model.StartMonth.HasValue
            ? DateTime.SpecifyKind(model.StartMonth.Value, DateTimeKind.Utc)
            : null;
        schedule.RecurringDay = model.RecurringDay;

        await _db.SaveChangesAsync();
        return new SaveResultDto { Id = schedule.ScheduleId, Code = schedule.ScheduleCode };
    }

    // --- Steps 2/3: template multi-picks (REPLACE-set) ---
    public async Task SetSaqTemplatesAsync(int scheduleId, List<int> saqHeaderIds)
    {
        await RequireDraftAsync(scheduleId);

        var ids = (saqHeaderIds ?? new List<int>()).Distinct().ToList();

        if (ids.Count > 0)
        {
            var headers = await _db.SaqHeaders
                .AsNoTracking()
                .Where(h => ids.Contains(h.SaqHeaderId))
                .ToListAsync();

            if (headers.Count != ids.Count)
            {
                throw new InvalidOperationException("One or more selected SAQ templates were not found.");
            }

            if (headers.Any(h => h.SaqStatus != SaqStatus.Active))
            {
                throw new InvalidOperationException("Only active SAQ templates can be selected.");
            }
        }

        await ReplaceItemsAsync(scheduleId, ScheduleItemType.Saq, ids);
    }

    public async Task SetKriTemplatesAsync(int scheduleId, List<int> kriHeaderIds)
    {
        await RequireDraftAsync(scheduleId);

        var ids = (kriHeaderIds ?? new List<int>()).Distinct().ToList();

        if (ids.Count > 0)
        {
            var headers = await _db.KriHeaders
                .AsNoTracking()
                .Where(h => ids.Contains(h.KriHeaderId))
                .ToListAsync();

            if (headers.Count != ids.Count)
            {
                throw new InvalidOperationException("One or more selected KRI templates were not found.");
            }

            if (headers.Any(h => h.KriStatus != KriStatus.Active))
            {
                throw new InvalidOperationException("Only active KRI templates can be selected.");
            }
        }

        await ReplaceItemsAsync(scheduleId, ScheduleItemType.Kri, ids);
    }

    // --- Finalize/delete ---
    public async Task FinalizeAsync(int scheduleId, ScheduleStatus status)
    {
        var schedule = await _db.Schedules.FindAsync(scheduleId)
            ?? throw new InvalidOperationException($"Schedule with Id {scheduleId} was not found.");

        if (status == ScheduleStatus.Active)
        {
            var hasSaq = await _db.ScheduleItems
                .AnyAsync(si => si.ScheduleId == scheduleId && si.ItemType == ScheduleItemType.Saq);
            if (!hasSaq)
            {
                throw new InvalidOperationException("Choose at least one SAQ template before activating the schedule.");
            }

            var hasKri = await _db.ScheduleItems
                .AnyAsync(si => si.ScheduleId == scheduleId && si.ItemType == ScheduleItemType.Kri);
            if (!hasKri)
            {
                throw new InvalidOperationException("Choose at least one KRI template before activating the schedule.");
            }

            await _assessmentService.CreateAssessmentAsync(scheduleId);
        }

        schedule.ScheduleStatus = status;
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int scheduleId)
    {
        var schedule = await _db.Schedules.FindAsync(scheduleId)
            ?? throw new InvalidOperationException($"Schedule with Id {scheduleId} was not found.");

        if (schedule.ScheduleStatus != ScheduleStatus.Draft)
        {
            throw new InvalidOperationException("Only draft schedules can be deleted.");
        }

        _db.Schedules.Remove(schedule);
        await _db.SaveChangesAsync();
    }

    // --- Orphan cleanup (called by template delete services) ---
    public async Task RemoveItemsForAsync(ScheduleItemType itemType, int itemId)
    {
        var items = await _db.ScheduleItems
            .Where(si => si.ItemType == itemType && si.ItemId == itemId)
            .ToListAsync();

        if (items.Count > 0)
        {
            _db.ScheduleItems.RemoveRange(items);
            await _db.SaveChangesAsync();
        }
    }

    private async Task ReplaceItemsAsync(int scheduleId, ScheduleItemType type, List<int> ids)
    {
        if (ids.Count == 0)
        {
            var existing = await _db.ScheduleItems
                .Where(si => si.ScheduleId == scheduleId && si.ItemType == type)
                .ToListAsync();
            _db.ScheduleItems.RemoveRange(existing);
            await _db.SaveChangesAsync();
            return;
        }

        var allExisting = await _db.ScheduleItems
            .Where(si => si.ScheduleId == scheduleId)
            .ToListAsync();

        _db.ScheduleItems.RemoveRange(allExisting.Where(si => si.ItemType == type));
        await _db.SaveChangesAsync();

        _db.ScheduleItems.AddRange(ids.Select(id => new ScheduleItem
        {
            ScheduleId = scheduleId,
            ItemType = type,
            ItemId = id
        }));
        await _db.SaveChangesAsync();
    }

    private static void ValidateScheduleModel(ScheduleSaveDto model)
    {
        if (model.ScheduleType == ScheduleType.OneTime)
        {
            if (!model.StartDate.HasValue)
            {
                throw new InvalidOperationException("Start date is required for a one-time schedule.");
            }

            if (!model.EndDate.HasValue)
            {
                throw new InvalidOperationException("End date is required for a one-time schedule.");
            }

            if (model.EndDate < model.StartDate)
            {
                throw new InvalidOperationException("End date cannot be before the start date.");
            }
        }
        else if (model.ScheduleType == ScheduleType.Recurring)
        {
            if (!model.StartMonth.HasValue)
            {
                throw new InvalidOperationException("Start month is required for a recurring schedule.");
            }

            if (!model.RecurringDay.HasValue)
            {
                throw new InvalidOperationException("Recurring day is required for a recurring schedule.");
            }
        }
        else
        {
            throw new InvalidOperationException("Choose a schedule type.");
        }
    }

    private async Task<ScheduleEntity> RequireDraftAsync(int scheduleId)
    {
        var schedule = await _db.Schedules.FindAsync(scheduleId)
            ?? throw new InvalidOperationException($"Schedule with Id {scheduleId} was not found.");

        if (schedule.ScheduleStatus != ScheduleStatus.Draft)
        {
            throw new InvalidOperationException("Only draft schedules can be edited.");
        }

        return schedule;
    }

    private static string HeaderLabel(string desc, string? code)
    {
        return string.IsNullOrWhiteSpace(desc) ? code ?? string.Empty : desc;
    }

    private async Task<List<OptionViewModel>> GetSaqOptionsAsync()
    {
        var headers = await _db.SaqHeaders.AsNoTracking()
            .Where(h => h.SaqStatus == SaqStatus.Active)
            .OrderBy(h => h.SaqCode)
            .Select(h => new { h.SaqHeaderId, h.SaqDesc, h.SaqCode })
            .ToListAsync();

        return headers
            .Select(h => new OptionViewModel
            {
                Value = h.SaqHeaderId,
                Label = string.IsNullOrWhiteSpace(h.SaqDesc) ? h.SaqCode ?? string.Empty : h.SaqDesc,
                Code = h.SaqCode
            })
            .ToList();
    }

    private async Task<List<OptionViewModel>> GetKriOptionsAsync()
    {
        var headers = await _db.KriHeaders.AsNoTracking()
            .Where(h => h.KriStatus == KriStatus.Active)
            .OrderBy(h => h.KriCode)
            .Select(h => new { h.KriHeaderId, h.KriHeaderDesc, h.KriCode })
            .ToListAsync();

        return headers
            .Select(h => new OptionViewModel
            {
                Value = h.KriHeaderId,
                Label = string.IsNullOrWhiteSpace(h.KriHeaderDesc) ? h.KriCode ?? string.Empty : h.KriHeaderDesc,
                Code = h.KriCode
            })
            .ToList();
    }
}