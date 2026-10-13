using Microsoft.EntityFrameworkCore;
using RiskPulse.Data;
using RiskPulse.Data.Entries;
using RiskPulse.Models.Dto;
using RiskPulse.Models.Enum;
using RiskPulse.Models.ViewModel;
using RiskPulse.Services.Utilities;

namespace RiskPulse.Services.Assessment;

public class AssessmentActivationService
{
    private readonly AppDbContext _db;
    private readonly CodeGeneratorService _codeService;
    private readonly AssessmentService _assessmentService;

    public AssessmentActivationService(AppDbContext db, CodeGeneratorService codeService, AssessmentService assessmentService)
    {
        _db = db;
        _codeService = codeService;
        _assessmentService = assessmentService;
    }

    // --- Activation grid ---
    public async Task<List<AssessmentActivationGridRowViewModel>> GetAllAsync()
    {
        var rows = await _db.AssessmentActivations
            .AsNoTracking()
            .OrderByDescending(a => a.AssessmentActivationId)
            .Select(a => new
            {
                a.AssessmentActivationId,
                a.ActivationCode,
                a.ActivationStatus,
                a.AssessmentType,
                a.StartDate,
                a.EndDate,
                a.StartMonth,
                a.RecurringDay,
                a.AssessmentTemplateId,
                TemplateDesc = a.AssessmentTemplate != null ? a.AssessmentTemplate.AssessmentTemplateDesc : null,
                TemplateCode = a.AssessmentTemplate != null ? a.AssessmentTemplate.AssessmentTemplateCode : null,
                SaqCount = a.AssessmentTemplate != null
                    ? a.AssessmentTemplate.AssessmentTemplateItems.Count(i => i.ItemType == AssessmentItemType.Saq)
                    : 0,
                KriCount = a.AssessmentTemplate != null
                    ? a.AssessmentTemplate.AssessmentTemplateItems.Count(i => i.ItemType == AssessmentItemType.Kri)
                    : 0
            })
            .ToListAsync();

        return rows.Select(a => new AssessmentActivationGridRowViewModel
        {
            AssessmentActivationId = a.AssessmentActivationId,
            ActivationCode = a.ActivationCode,
            AssessmentTemplateId = a.AssessmentTemplateId,
            AssessmentTemplateDesc = string.IsNullOrWhiteSpace(a.TemplateDesc) ? a.TemplateCode ?? string.Empty : a.TemplateDesc,
            SaqCount = a.SaqCount,
            KriCount = a.KriCount,
            AssessmentType = a.AssessmentType.ToString(),
            StartDate = a.StartDate,
            EndDate = a.EndDate,
            StartMonth = a.StartMonth,
            RecurringDay = a.RecurringDay,
            ActivationStatus = a.ActivationStatus.ToString()
        }).ToList();
    }

    // --- Active assessment-template options for the activation modal ---
    public async Task<List<OptionViewModel>> GetTemplateOptionsAsync()
    {
        return await _db.AssessmentTemplates.AsNoTracking()
            .Where(t => t.IsActive)
            .OrderBy(t => t.AssessmentTemplateDesc)
            .ThenBy(t => t.AssessmentTemplateCode)
            .Select(t => new OptionViewModel
            {
                Value = t.AssessmentTemplateId,
                Label = string.IsNullOrWhiteSpace(t.AssessmentTemplateDesc) ? (t.AssessmentTemplateCode ?? string.Empty) : t.AssessmentTemplateDesc,
                Code = t.AssessmentTemplateCode
            })
            .ToListAsync();
    }

    // --- Step 1: activation details (create or update) ---
    public async Task<SaveResultDto> CreateOrUpdateAsync(AssessmentActivationSaveDto model)
    {
        ValidateModel(model);

        AssessmentActivation activation;

        if (model.AssessmentActivationId == 0)
        {
            activation = new AssessmentActivation
            {
                ActivationCode = await _codeService.GenerateActivationCodeAsync(),
                ActivationStatus = AssessmentActivationStatus.Draft
            };
            _db.AssessmentActivations.Add(activation);
        }
        else
        {
            activation = await RequireEditableAsync(model.AssessmentActivationId);
        }

        activation.AssessmentType = model.AssessmentType;
        activation.AssessmentTemplateId = model.AssessmentTemplateId;
        // timestamptz columns require UTC DateTime values; the JSON binder yields Kind=Unspecified.
        activation.StartDate = model.StartDate.HasValue
            ? DateTime.SpecifyKind(model.StartDate.Value, DateTimeKind.Utc)
            : null;
        activation.EndDate = model.EndDate.HasValue
            ? DateTime.SpecifyKind(model.EndDate.Value, DateTimeKind.Utc)
            : null;
        activation.StartMonth = model.StartMonth.HasValue
            ? DateTime.SpecifyKind(model.StartMonth.Value, DateTimeKind.Utc)
            : null;
        activation.RecurringDay = model.RecurringDay;

        await _db.SaveChangesAsync();
        return new SaveResultDto { Id = activation.AssessmentActivationId, Code = activation.ActivationCode };
    }

    // --- Finalize: activating generates (snapshots) the real assessment from the template ---
    public async Task FinalizeAsync(int activationId, AssessmentActivationStatus status)
    {
        var activation = await RequireDraftAsync(activationId);

        if (status == AssessmentActivationStatus.Active)
        {
            if (!activation.AssessmentTemplateId.HasValue)
            {
                throw new InvalidOperationException("Select an assessment template before activating.");
            }

            await _assessmentService.CreateAssessmentAsync(activationId);
        }

        activation.ActivationStatus = status;
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int activationId)
    {
        var activation = await _db.AssessmentActivations.FindAsync(activationId)
            ?? throw new InvalidOperationException($"Assessment activation with Id {activationId} was not found.");

        if (activation.ActivationStatus != AssessmentActivationStatus.Draft)
        {
            throw new InvalidOperationException("Only draft activations can be deleted.");
        }

        _db.AssessmentActivations.Remove(activation);
        await _db.SaveChangesAsync();
    }

    // --- Enable / disable: an Active activation can be disabled and re-enabled by a risk user ---
    public async Task SetEnabledAsync(int activationId, bool enabled)
    {
        var activation = await _db.AssessmentActivations.FindAsync(activationId)
            ?? throw new InvalidOperationException($"Assessment activation with Id {activationId} was not found.");

        if (enabled)
        {
            if (activation.ActivationStatus != AssessmentActivationStatus.Disabled)
            {
                throw new InvalidOperationException("Only a disabled activation can be enabled.");
            }
        }
        else if (activation.ActivationStatus != AssessmentActivationStatus.Active)
        {
            throw new InvalidOperationException("Only an active activation can be disabled.");
        }

        activation.ActivationStatus = enabled ? AssessmentActivationStatus.Active : AssessmentActivationStatus.Disabled;
        await _db.SaveChangesAsync();
    }

    private static void ValidateModel(AssessmentActivationSaveDto model)
    {
        if (!model.AssessmentTemplateId.HasValue)
        {
            throw new InvalidOperationException("Select an assessment template.");
        }

        if (model.AssessmentType == AssessmentType.OneTime)
        {
            if (!model.StartDate.HasValue)
            {
                throw new InvalidOperationException("Start date is required for a one-time assessment.");
            }

            if (!model.EndDate.HasValue)
            {
                throw new InvalidOperationException("End date is required for a one-time assessment.");
            }

            if (model.EndDate < model.StartDate)
            {
                throw new InvalidOperationException("End date cannot be before the start date.");
            }
        }
        else if (model.AssessmentType == AssessmentType.Recurring)
        {
            if (!model.StartMonth.HasValue)
            {
                throw new InvalidOperationException("Start month is required for a recurring assessment.");
            }

            if (!model.RecurringDay.HasValue)
            {
                throw new InvalidOperationException("Recurring day is required for a recurring assessment.");
            }

            if (model.RecurringDay < 1 || model.RecurringDay > 31)
            {
                throw new InvalidOperationException("Recurring day must be between 1 and 31.");
            }
        }
        else
        {
            throw new InvalidOperationException("Choose an assessment type.");
        }
    }

    private async Task<AssessmentActivation> RequireDraftAsync(int activationId)
    {
        var activation = await _db.AssessmentActivations.FindAsync(activationId)
            ?? throw new InvalidOperationException($"Assessment activation with Id {activationId} was not found.");

        if (activation.ActivationStatus != AssessmentActivationStatus.Draft)
        {
            throw new InvalidOperationException("Only draft activations can be activated.");
        }

        return activation;
    }

    private async Task<AssessmentActivation> RequireEditableAsync(int activationId)
    {
        var activation = await _db.AssessmentActivations.FindAsync(activationId)
            ?? throw new InvalidOperationException($"Assessment activation with Id {activationId} was not found.");

        if (activation.ActivationStatus == AssessmentActivationStatus.Completed)
        {
            throw new InvalidOperationException("Completed activations cannot be edited.");
        }

        return activation;
    }
}
