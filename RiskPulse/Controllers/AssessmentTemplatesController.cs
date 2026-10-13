using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RiskPulse.Models.Dto;
using RiskPulse.Models.ViewModel;
using RiskPulse.Services.Login;
using RiskPulse.Services.Templates;

namespace RiskPulse.Controllers;

[Authorize(Policy = $"Permission:{PermissionCatalog.AssessmentTemplates}")]
public class AssessmentTemplatesController : Controller
{
    private readonly AssessmentTemplatesService _assessmentTemplatesService;
    private readonly SaqTemplatesService _saqTemplatesService;
    private readonly KriTemplatesService _kriTemplatesService;

    public AssessmentTemplatesController(
        AssessmentTemplatesService assessmentTemplatesService,
        SaqTemplatesService saqTemplatesService,
        KriTemplatesService kriTemplatesService)
    {
        _assessmentTemplatesService = assessmentTemplatesService;
        _saqTemplatesService = saqTemplatesService;
        _kriTemplatesService = kriTemplatesService;
    }

    // --- Page load (Index) ---
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        return View(new AssessmentTemplatesIndexViewModel
        {
            Saqs = await _assessmentTemplatesService.GetSaqOptionsAsync(),
            Kris = await _assessmentTemplatesService.GetKriOptionsAsync()
        });
    }

    // --- Assessment templates (grid/full-save/delete) ---
    [HttpGet]
    public async Task<IActionResult> Grid()
    {
        var rows = await _assessmentTemplatesService.GetHeaderRowsAsync();
        return Json(ApiResponse.Ok(rows));
    }

    // --- Read-only template previews (SAQ/KRI pickers in the modal) ---
    [HttpGet]
    public async Task<IActionResult> SaqPreview(int id)
    {
        return await ControllerHelpers.TryExecute(
            async () => Json(ApiResponse.Ok(await _saqTemplatesService.GetSaqPreviewAsync(id))),
            "An error occurred while loading the SAQ preview. Please try again.");
    }

    [HttpGet]
    public async Task<IActionResult> KriPreview(int id)
    {
        return await ControllerHelpers.TryExecute(
            async () => Json(ApiResponse.Ok(await _kriTemplatesService.GetKriPreviewAsync(id))),
            "An error occurred while loading the KRI preview. Please try again.");
    }

    [HttpPost]
    public async Task<IActionResult> Save([FromBody] AssessmentTemplateSaveDto model)
    {
        return await ControllerHelpers.TrySave(model, ModelState, m => m.AssessmentTemplateId,
            m => _assessmentTemplatesService.SaveAsync(m), "Assessment template");
    }

    [HttpPost]
    public async Task<IActionResult> Delete([FromBody] DeleteRequestDto request)
    {
        return await ControllerHelpers.TryDelete(request, ModelState,
            id => _assessmentTemplatesService.DeleteAsync(id), "Assessment template");
    }
}
