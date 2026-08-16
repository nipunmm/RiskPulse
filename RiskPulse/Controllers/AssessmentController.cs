using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RiskPulse.Models.Dto;
using RiskPulse.Models.ViewModel;
using RiskPulse.Services.Assessment;
using RiskPulse.Services.Login;

namespace RiskPulse.Controllers;

[Authorize(Policy = $"Permission:{PermissionCatalog.Assessment}")]
public class AssessmentController : Controller
{
    private readonly AssessmentService _assessmentService;

    public AssessmentController(AssessmentService assessmentService)
    {
        _assessmentService = assessmentService;
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
        var rows = await _assessmentService.GetAllAsync();
        return Json(ApiResponse.Ok(rows));
    }

    [HttpGet]
    public async Task<IActionResult> Wizard(int id = 0)
    {
        AssessmentWizardViewModel model;
        try
        {
            model = await _assessmentService.GetWizardAsync(id);
        }
        catch (InvalidOperationException ex)
        {
            TempData["WizardError"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }

        return View(model);
    }

    // --- Wizard steps (create draft/templates) ---
    [HttpPost]
    public async Task<IActionResult> SaveName([FromBody] AssessmentNameSaveDto model)
    {
        if (model == null || !ModelState.IsValid)
        {
            var message = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .FirstOrDefault() ?? "Please correct the form errors and try again.";

            return Json(ApiResponse.Fail<object>(message));
        }

        try
        {
            var isNew = model.AssessmentHeaderId == 0;
            int id;

            if (isNew)
            {
                var header = await _assessmentService.CreateDraftAsync(model.AssessmentName);
                id = header.AssessmentHeaderId;
            }
            else
            {
                await _assessmentService.UpdateNameAsync(model.AssessmentHeaderId, model.AssessmentName);
                id = model.AssessmentHeaderId;
            }

            return Json(ApiResponse.Ok(new { id }, isNew ? "Draft assessment created." : "Assessment name updated."));
        }
        catch (InvalidOperationException ex)
        {
            return Json(ApiResponse.Fail<object>(ex.Message));
        }
        catch (Exception)
        {
            return Json(ApiResponse.Fail<object>("An error occurred while saving the assessment. Please try again."));
        }
    }

    [HttpPost]
    public async Task<IActionResult> SaveSaq([FromBody] AssessmentTemplateSaveDto model)
    {
        if (model == null || !ModelState.IsValid)
        {
            return Json(ApiResponse.Fail<object>("Please correct the form errors and try again."));
        }

        try
        {
            await _assessmentService.SetSaqTemplateAsync(model.AssessmentHeaderId, model.TemplateHeaderId);
            return Json(ApiResponse.Ok<object>(new { }, "SAQ template saved."));
        }
        catch (InvalidOperationException ex)
        {
            return Json(ApiResponse.Fail<object>(ex.Message));
        }
        catch (Exception)
        {
            return Json(ApiResponse.Fail<object>("An error occurred while saving the SAQ template. Please try again."));
        }
    }

    [HttpPost]
    public async Task<IActionResult> SaveKri([FromBody] AssessmentTemplateSaveDto model)
    {
        if (model == null || !ModelState.IsValid)
        {
            return Json(ApiResponse.Fail<object>("Please correct the form errors and try again."));
        }

        try
        {
            await _assessmentService.SetKriTemplateAsync(model.AssessmentHeaderId, model.TemplateHeaderId);
            return Json(ApiResponse.Ok<object>(new { }, "KRI template saved."));
        }
        catch (InvalidOperationException ex)
        {
            return Json(ApiResponse.Fail<object>(ex.Message));
        }
        catch (Exception)
        {
            return Json(ApiResponse.Fail<object>("An error occurred while saving the KRI template. Please try again."));
        }
    }

    // --- Wizard steps (schedule) ---
    [HttpPost]
    public async Task<IActionResult> SaveSchedule([FromBody] ScheduleSaveDto model)
    {
        if (model == null || !ModelState.IsValid)
        {
            var message = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .FirstOrDefault() ?? "Please correct the form errors and try again.";

            return Json(ApiResponse.Fail<object>(message));
        }

        try
        {
            await _assessmentService.UpsertScheduleAsync(model);
            return Json(ApiResponse.Ok<object>(new { }, "Schedule saved."));
        }
        catch (InvalidOperationException ex)
        {
            return Json(ApiResponse.Fail<object>(ex.Message));
        }
        catch (Exception)
        {
            return Json(ApiResponse.Fail<object>("An error occurred while saving the schedule. Please try again."));
        }
    }

    // --- Finalize/delete ---
    [HttpPost]
    public async Task<IActionResult> Finalize([FromBody] AssessmentFinalizeDto model)
    {
        if (model == null || !ModelState.IsValid)
        {
            return Json(ApiResponse.Fail<object>("Please correct the form errors and try again."));
        }

        try
        {
            await _assessmentService.FinalizeAsync(model.AssessmentHeaderId, model.Status);
            var message = model.Status == RiskPulse.Data.Entries.AssessmentStatus.Active
                ? "Assessment activated."
                : "Assessment saved as draft.";
            return Json(ApiResponse.Ok<object>(new { }, message));
        }
        catch (InvalidOperationException ex)
        {
            return Json(ApiResponse.Fail<object>(ex.Message));
        }
        catch (Exception)
        {
            return Json(ApiResponse.Fail<object>("An error occurred while finalizing the assessment. Please try again."));
        }
    }

    [HttpPost]
    public async Task<IActionResult> Delete([FromBody] AssessmentDeleteRequestDto request)
    {
        if (request == null || !ModelState.IsValid)
        {
            return Json(ApiResponse.Fail<object>("Please correct the form errors and try again."));
        }

        try
        {
            await _assessmentService.DeleteAsync(request.Id);
            return Json(ApiResponse.Ok<object>(new { }, "Draft assessment deleted."));
        }
        catch (InvalidOperationException ex)
        {
            return Json(ApiResponse.Fail<object>(ex.Message));
        }
        catch (Exception)
        {
            return Json(ApiResponse.Fail<object>("An error occurred while deleting the assessment. Please try again."));
        }
    }
}
