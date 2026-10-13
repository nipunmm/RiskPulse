using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RiskPulse.Models.Dto;
using RiskPulse.Models.Enum;
using RiskPulse.Models.ViewModel;
using RiskPulse.Services.Assessment;
using RiskPulse.Services.Login;

namespace RiskPulse.Controllers;

[Authorize(Policy = $"Permission:{PermissionCatalog.Assessment}")]
public class AssessmentController : Controller
{
    private readonly AssessmentActivationService _activationService;

    public AssessmentController(AssessmentActivationService activationService)
    {
        _activationService = activationService;
    }

    // --- Page load (grid of activations + activation modal) ---
    [HttpGet]
    public IActionResult Index() => RedirectToAction(nameof(Activation));

    [HttpGet]
    public async Task<IActionResult> Activation()
    {
        var model = new AssessmentIndexViewModel
        {
            TemplateOptions = await _activationService.GetTemplateOptionsAsync()
        };
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Grid()
    {
        var rows = await _activationService.GetAllAsync();
        return Json(ApiResponse.Ok(rows));
    }

    // --- Create or update an activation ---
    [HttpPost]
    public async Task<IActionResult> Save([FromBody] AssessmentActivationSaveDto model)
    {
        var error = ControllerHelpers.ValidateModel(model, ModelState);
        if (error != null) return error;

        return await ControllerHelpers.TryExecute(async () =>
        {
            var saved = await _activationService.CreateOrUpdateAsync(model);
            return Json(ApiResponse.Ok(new { id = saved.Id, code = saved.Code }, "Activation saved."));
        }, "An error occurred while saving the assessment activation. Please try again.");
    }

    // --- Save and activate in one step (modal primary action) ---
    [HttpPost]
    public async Task<IActionResult> SaveAndActivate([FromBody] AssessmentActivationSaveDto model)
    {
        var error = ControllerHelpers.ValidateModel(model, ModelState);
        if (error != null) return error;

        return await ControllerHelpers.TryExecute(async () =>
        {
            var saved = await _activationService.CreateOrUpdateAsync(model);
            await _activationService.FinalizeAsync(saved.Id, AssessmentActivationStatus.Active);
            return Json(ApiResponse.Ok(new { id = saved.Id, code = saved.Code }, "Assessment activated."));
        }, "An error occurred while activating the assessment. Please try again.");
    }

    [HttpPost]
    public async Task<IActionResult> Delete([FromBody] DeleteRequestDto request)
    {
        return await ControllerHelpers.TryDelete(request, ModelState,
            id => _activationService.DeleteAsync(id), "Draft assessment activation");
    }

    // --- Enable / disable an activation (risk-user control) ---
    [HttpPost]
    public async Task<IActionResult> SetEnabled([FromBody] AssessmentActivationToggleDto model)
    {
        var error = ControllerHelpers.ValidateModel(model, ModelState);
        if (error != null) return error;

        return await ControllerHelpers.TryExecute(async () =>
        {
            await _activationService.SetEnabledAsync(model.AssessmentActivationId, model.Enabled);
            return Json(ApiResponse.Ok<object>(new { }, model.Enabled ? "Activation enabled." : "Activation disabled."));
        }, "An error occurred while updating the assessment activation. Please try again.");
    }
}
