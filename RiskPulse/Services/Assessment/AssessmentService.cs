using Microsoft.EntityFrameworkCore;
using RiskPulse.Data;
using RiskPulse.Data.Entries;
using RiskPulse.Data.Extensions;
using RiskPulse.Models.Dto;
using RiskPulse.Models.Enum;
using RiskPulse.Models.ViewModel;

namespace RiskPulse.Services.Assessment;

public class AssessmentService
{
    private readonly AppDbContext _db;

    public AssessmentService(AppDbContext db)
    {
        _db = db;
    }

    // --- Assessment headers (grid/draft/templates) ---
    public async Task<List<AssessmentGridRowViewModel>> GetAllAsync()
    {
        return await _db.AssessmentHeaders
            .AsNoTracking()
            .OrderByDescending(a => a.AssessmentHeaderId)
            .Select(a => new AssessmentGridRowViewModel
            {
                AssessmentHeaderId = a.AssessmentHeaderId,
                AssessmentName = a.AssessmentName,
                SaqDesc = a.SaqHeader != null ? a.SaqHeader.SaqDesc : string.Empty,
                KriHeaderDesc = a.KriHeader != null ? a.KriHeader.KriHeaderDesc : string.Empty,
                ScheduleDesc = a.ScheduleHeaders
                    .OrderBy(s => s.ScheduleHeaderId)
                    .Select(s => s.ScheduleDesc)
                    .FirstOrDefault() ?? string.Empty,
                AssessmentStatus = a.AssessmentStatus.ToString()
            })
            .ToListAsync();
    }

    public async Task<SaveResultDto> CreateDraftAsync(string name)
    {
        var trimmed = name.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            throw new InvalidOperationException("Assessment name is required.");
        }

        var header = new AssessmentHeader
        {
            AssessmentName = trimmed,
            AssessmentStatus = AssessmentStatus.Draft
        };

        _db.AssessmentHeaders.Add(header);
        await _db.SaveChangesAsync();
        return new SaveResultDto { Id = header.AssessmentHeaderId };
    }

    public async Task UpdateNameAsync(int assessmentHeaderId, string name)
    {
        var header = await RequireDraftAsync(assessmentHeaderId);

        var trimmed = name.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            throw new InvalidOperationException("Assessment name is required.");
        }

        header.AssessmentName = trimmed;
        await _db.SaveChangesAsync();
    }

    public async Task<AssessmentWizardViewModel> GetWizardAsync(int assessmentHeaderId)
    {
        var saqOptions = await GetSaqOptionsAsync();
        var kriOptions = await GetKriOptionsAsync();

        if (assessmentHeaderId == 0)
        {
            return new AssessmentWizardViewModel
            {
                SaqOptions = saqOptions,
                KriOptions = kriOptions,
                CanEdit = true
            };
        }

        var header = await _db.AssessmentHeaders
            .AsNoTracking()
            .SingleOrDefaultAsync(a => a.AssessmentHeaderId == assessmentHeaderId)
            ?? throw new InvalidOperationException($"Assessment with Id {assessmentHeaderId} was not found.");

        var schedule = await _db.ScheduleHeaders
            .AsNoTracking()
            .Where(s => s.AssessmentHeaderId == assessmentHeaderId)
            .OrderBy(s => s.ScheduleHeaderId)
            .FirstOrDefaultAsync();

        return new AssessmentWizardViewModel
        {
            AssessmentHeaderId = header.AssessmentHeaderId,
            AssessmentName = header.AssessmentName,
            SaqHeaderId = header.SaqHeaderId ?? 0,
            KriHeaderId = header.KriHeaderId ?? 0,
            ScheduleDesc = schedule?.ScheduleDesc ?? string.Empty,
            StartDate = schedule?.StartDate,
            EndDate = schedule?.EndDate,
            SaqOptions = saqOptions,
            KriOptions = kriOptions,
            CompletedSaq = header.SaqHeaderId.HasValue,
            CompletedKri = header.KriHeaderId.HasValue,
            CompletedSchedule = schedule != null,
            CanEdit = header.AssessmentStatus == AssessmentStatus.Draft
        };
    }

    public async Task SetSaqTemplateAsync(int assessmentHeaderId, int saqHeaderId)
    {
        var header = await RequireDraftAsync(assessmentHeaderId);

        var saq = await _db.SaqHeaders
            .AsNoTracking()
            .SingleOrDefaultAsync(h => h.SaqHeaderId == saqHeaderId)
            ?? throw new InvalidOperationException("Selected SAQ template was not found.");

        if (saq.SaqStatus == SaqStatus.Locked)
        {
            throw new InvalidOperationException("A locked SAQ template cannot be selected.");
        }

        header.SaqHeaderId = saqHeaderId;
        await _db.SaveChangesAsync();
    }

    public async Task SetKriTemplateAsync(int assessmentHeaderId, int kriHeaderId)
    {
        var header = await RequireDraftAsync(assessmentHeaderId);

        var kri = await _db.KriHeaders
            .AsNoTracking()
            .SingleOrDefaultAsync(h => h.KriHeaderId == kriHeaderId)
            ?? throw new InvalidOperationException("Selected KRI template was not found.");

        if (kri.KriStatus == KriStatus.Locked)
        {
            throw new InvalidOperationException("A locked KRI template cannot be selected.");
        }

        header.KriHeaderId = kriHeaderId;
        await _db.SaveChangesAsync();
    }

    // --- Schedule ---
    public async Task UpsertScheduleAsync(ScheduleSaveDto model)
    {
        var header = await RequireDraftAsync(model.AssessmentHeaderId);

        if (model.StartDate.HasValue && model.EndDate.HasValue && model.EndDate < model.StartDate)
        {
            throw new InvalidOperationException("End date cannot be before the start date.");
        }

        var schedule = await _db.ScheduleHeaders
            .Where(s => s.AssessmentHeaderId == model.AssessmentHeaderId)
            .OrderBy(s => s.ScheduleHeaderId)
            .FirstOrDefaultAsync();

        if (schedule == null)
        {
            schedule = new ScheduleHeader { AssessmentHeaderId = model.AssessmentHeaderId };
            _db.ScheduleHeaders.Add(schedule);
        }

        schedule.ScheduleDesc = model.ScheduleDesc.Trim();
        // timestamptz columns require UTC DateTime values; the JSON binder yields Kind=Unspecified.
        schedule.StartDate = model.StartDate.HasValue
            ? DateTime.SpecifyKind(model.StartDate.Value, DateTimeKind.Utc)
            : null;
        schedule.EndDate = model.EndDate.HasValue
            ? DateTime.SpecifyKind(model.EndDate.Value, DateTimeKind.Utc)
            : null;

        await _db.SaveChangesAsync();
    }

    // --- Finalize/delete ---
    public async Task FinalizeAsync(int assessmentHeaderId, AssessmentStatus status)
    {
        var header = await _db.AssessmentHeaders.FindAsync(assessmentHeaderId)
            ?? throw new InvalidOperationException($"Assessment with Id {assessmentHeaderId} was not found.");

        if (status == AssessmentStatus.Active)
        {
            if (!header.SaqHeaderId.HasValue)
            {
                throw new InvalidOperationException("Choose a SAQ template before activating the assessment.");
            }

            if (!header.KriHeaderId.HasValue)
            {
                throw new InvalidOperationException("Choose a KRI template before activating the assessment.");
            }
        }

        header.AssessmentStatus = status;
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int assessmentHeaderId)
    {
        var header = await _db.AssessmentHeaders.FindAsync(assessmentHeaderId)
            ?? throw new InvalidOperationException($"Assessment with Id {assessmentHeaderId} was not found.");

        if (header.AssessmentStatus != AssessmentStatus.Draft)
        {
            throw new InvalidOperationException("Only draft assessments can be deleted.");
        }

        _db.AssessmentHeaders.Remove(header);
        await _db.SaveChangesAsync();
    }

    private async Task<AssessmentHeader> RequireDraftAsync(int assessmentHeaderId)
    {
        var header = await _db.AssessmentHeaders.FindAsync(assessmentHeaderId)
            ?? throw new InvalidOperationException($"Assessment with Id {assessmentHeaderId} was not found.");

        if (header.AssessmentStatus != AssessmentStatus.Draft)
        {
            throw new InvalidOperationException("Only draft assessments can be edited.");
        }

        return header;
    }

    private async Task<List<OptionViewModel>> GetSaqOptionsAsync()
    {
        return await _db.SaqHeaders.AsNoTracking()
            .Where(h => h.SaqStatus != SaqStatus.Locked)
            .OrderBy(h => h.SaqDesc)
            .ToOptionListAsync(h => h.SaqHeaderId, h => h.SaqDesc);
    }

    private async Task<List<OptionViewModel>> GetKriOptionsAsync()
    {
        return await _db.KriHeaders.AsNoTracking()
            .Where(h => h.KriStatus != KriStatus.Locked)
            .OrderBy(h => h.KriHeaderDesc)
            .ToOptionListAsync(h => h.KriHeaderId, h => h.KriHeaderDesc);
    }
}
