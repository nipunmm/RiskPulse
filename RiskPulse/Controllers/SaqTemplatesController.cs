using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RiskPulse.Models.Dto;
using RiskPulse.Models.ViewModel;
using RiskPulse.Services.Administration;
using RiskPulse.Services.Login;
using RiskPulse.Services.Templates;

namespace RiskPulse.Controllers;

[Authorize(Policy = $"Permission:{PermissionCatalog.Saq}")]
public class SaqTemplatesController : Controller
{
    private readonly SaqTemplatesService _saqTemplatesService;
    private readonly UnitsService _unitsService;

    public SaqTemplatesController(SaqTemplatesService saqTemplatesService, UnitsService unitsService)
    {
        _saqTemplatesService = saqTemplatesService;
        _unitsService = unitsService;
    }

    // --- Page load (Index) ---
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var statuses = SaqStatusOptionViewModel.GetAll();

        var unitGroups = await _unitsService.GetUnitGroupOptionsAsync();
        var units = await _unitsService.GetUnitOptionsAsync();

        return View(new SaqTemplatesIndexViewModel
        {
            SaqStatuses = statuses,
            UnitGroups = unitGroups,
            Units = units
        });
    }

    // --- SAQ template headers (grid/save/delete) ---
    [HttpGet]
    public async Task<IActionResult> Grid()
    {
        var rows = await _saqTemplatesService.GetHeaderRowsAsync();
        return Json(ApiResponse.Ok(rows));
    }

    [HttpPost]
    public async Task<IActionResult> Save([FromBody] SaqHeaderSaveDto model)
    {
        return await ControllerHelpers.TrySave(model, ModelState, m => m.SaqHeaderId,
            m => _saqTemplatesService.SaveHeaderAsync(m), "Template");
    }

    [HttpPost]
    public async Task<IActionResult> Delete([FromBody] DeleteRequestDto request)
    {
        return await ControllerHelpers.TryDelete(request, ModelState,
            id => _saqTemplatesService.DeleteHeaderAsync(id), "Template");
    }

    // --- SAQ questions (grid/save/delete) ---
    [HttpGet]
    public async Task<IActionResult> QuestionsGrid(int saqHeaderId)
    {
        var rows = await _saqTemplatesService.GetQuestionRowsAsync(saqHeaderId);
        return Json(ApiResponse.Ok(rows));
    }

    [HttpPost]
    public async Task<IActionResult> SaveQuestion([FromBody] SaqQuestionSaveDto model)
    {
        return await ControllerHelpers.TrySave(model, ModelState, m => m.QuestionId,
            m => _saqTemplatesService.SaveQuestionAsync(m), "Question");
    }

    [HttpPost]
    public async Task<IActionResult> DeleteQuestion([FromBody] DeleteRequestDto request)
    {
        return await ControllerHelpers.TryDelete(request, ModelState,
            id => _saqTemplatesService.DeleteQuestionAsync(id), "Question");
    }
}
