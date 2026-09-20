using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RiskPulse.Models.Dto;
using RiskPulse.Models.Enum;
using RiskPulse.Models.ViewModel;
using RiskPulse.Services.Login;
using RiskPulse.Services.Schedule;
using RiskPulse.Services.Templates;

namespace RiskPulse.Controllers;

[Authorize(Policy = $"Permission:{PermissionCatalog.Schedule}")]
public class ScheduleController : Controller
{
    private readonly ScheduleService _scheduleService;
    private readonly SaqTemplatesService _saqTemplatesService;
    private readonly KriTemplatesService _kriTemplatesService;

    public ScheduleController(ScheduleService scheduleService, SaqTemplatesService saqTemplatesService, KriTemplatesService kriTemplatesService)
    {
        _scheduleService = scheduleService;
        _saqTemplatesService = saqTemplatesService;
        _kriTemplatesService = kriTemplatesService;
    }

    // --- Page load (Index/Grid/Wizard) ---
    [HttpGet]
    public IActionResult Index()
    {
        ViewData["WizardError"] = TempData["WizardError"];
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> Grid()
    {
        var rows = await _scheduleService.GetAllAsync();
        return Json(ApiResponse.Ok(rows));
    }

    [HttpGet]
    public async Task<IActionResult> Wizard(int id = 0)
    {
        ScheduleWizardViewModel model;
        try
        {
            model = await _scheduleService.GetWizardAsync(id);
        }
        catch (InvalidOperationException ex)
        {
            TempData["WizardError"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }

        return View(model);
    }

    // --- Wizard step 1: schedule type/details ---
    [HttpPost]
    public async Task<IActionResult> SaveSchedule([FromBody] ScheduleSaveDto model)
    {
        var error = ControllerHelpers.ValidateModel(model, ModelState);
        if (error != null) return error;

        return await ControllerHelpers.TryExecute(async () =>
        {
            var isNew = model!.ScheduleId == 0;
            var result = await _scheduleService.CreateOrUpdateScheduleAsync(model);
            return Json(ApiResponse.Ok(new { id = result.Id, code = result.Code }, isNew ? "Schedule draft created." : "Schedule details updated."));
        }, "An error occurred while saving the schedule. Please try again.");
    }

    // --- Wizard steps (templates) ---
    [HttpPost]
    public async Task<IActionResult> SaveSaq([FromBody] ScheduleTemplatesSaveDto model)
    {
        var error = ControllerHelpers.ValidateModel(model, ModelState);
        if (error != null) return error;

        return await ControllerHelpers.TryExecute(async () =>
        {
            await _scheduleService.SetSaqTemplatesAsync(model!.ScheduleId, model.TemplateHeaderIds);
            return Json(ApiResponse.Ok<object>(new { }, "SAQ templates saved."));
        }, "An error occurred while saving the SAQ templates. Please try again.");
    }

    [HttpPost]
    public async Task<IActionResult> SaveKri([FromBody] ScheduleTemplatesSaveDto model)
    {
        var error = ControllerHelpers.ValidateModel(model, ModelState);
        if (error != null) return error;

        return await ControllerHelpers.TryExecute(async () =>
        {
            await _scheduleService.SetKriTemplatesAsync(model!.ScheduleId, model.TemplateHeaderIds);
            return Json(ApiResponse.Ok<object>(new { }, "KRI templates saved."));
        }, "An error occurred while saving the KRI templates. Please try again.");
    }

    // --- Template previews ---
    [HttpGet]
    public async Task<IActionResult> SaqPreview(int id)
    {
        return await ControllerHelpers.TryExecute(async () => Json(ApiResponse.Ok(await _saqTemplatesService.GetSaqPreviewAsync(id))), "An error occurred while loading the SAQ preview. Please try again.");
    }

    [HttpGet]
    public async Task<IActionResult> KriPreview(int id)
    {
        return await ControllerHelpers.TryExecute(async () => Json(ApiResponse.Ok(await _kriTemplatesService.GetKriPreviewAsync(id))), "An error occurred while loading the KRI preview. Please try again.");
    }

    // --- Finalize/delete ---
    [HttpPost]
    public async Task<IActionResult> Finalize([FromBody] ScheduleFinalizeDto model)
    {
        var error = ControllerHelpers.ValidateModel(model, ModelState);
        if (error != null) return error;

        return await ControllerHelpers.TryExecute(async () =>
        {
            await _scheduleService.FinalizeAsync(model!.ScheduleId, model.Status);
            var message = model.Status == ScheduleStatus.Active
                ? "Schedule activated."
                : "Schedule saved as draft.";
            return Json(ApiResponse.Ok<object>(new { }, message));
        }, "An error occurred while finalizing the schedule. Please try again.");
    }

    [HttpPost]
    public async Task<IActionResult> Delete([FromBody] DeleteRequestDto request)
    {
        return await ControllerHelpers.TryDelete(request, ModelState,
            id => _scheduleService.DeleteAsync(id), "Draft schedule");
    }
}