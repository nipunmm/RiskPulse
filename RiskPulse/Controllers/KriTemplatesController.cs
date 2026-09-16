using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RiskPulse.Models.Dto;
using RiskPulse.Models.ViewModel;
using RiskPulse.Services.Administration;
using RiskPulse.Services.Login;
using RiskPulse.Services.Templates;

namespace RiskPulse.Controllers;

[Authorize(Policy = $"Permission:{PermissionCatalog.Kri}")]
public class KriTemplatesController : Controller
{
    private readonly KriTemplatesService _kriTemplatesService;
    private readonly UnitsService _unitsService;

    public KriTemplatesController(KriTemplatesService kriTemplatesService, UnitsService unitsService)
    {
        _kriTemplatesService = kriTemplatesService;
        _unitsService = unitsService;
    }

    // --- Page load (Index) ---
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var statuses = KriStatusOptionViewModel.GetAll();

        var unitGroups = await _unitsService.GetUnitGroupOptionsAsync();
        var units = await _unitsService.GetUnitOptionsAsync();

        return View(new KriTemplatesIndexViewModel
        {
            KriStatuses = statuses,
            UnitGroups = unitGroups,
            Units = units
        });
    }

    // --- KRI template headers (grid/save/delete) ---
    [HttpGet]
    public async Task<IActionResult> Grid()
    {
        var rows = await _kriTemplatesService.GetHeaderRowsAsync();
        return Json(ApiResponse.Ok(rows));
    }

    [HttpPost]
    public async Task<IActionResult> Save([FromBody] KriHeaderSaveDto model)
    {
        return await ControllerHelpers.TrySave(model, ModelState, m => m.KriHeaderId,
            m => _kriTemplatesService.SaveHeaderAsync(m), "Template");
    }

    [HttpPost]
    public async Task<IActionResult> Delete([FromBody] DeleteRequestDto request)
    {
        return await ControllerHelpers.TryDelete(request, ModelState,
            id => _kriTemplatesService.DeleteHeaderAsync(id), "Template");
    }

    // --- KRI items (grid/save/delete) ---
    [HttpGet]
    public async Task<IActionResult> KrisGrid(int kriHeaderId)
    {
        var rows = await _kriTemplatesService.GetKrisAsync(kriHeaderId);
        return Json(ApiResponse.Ok(rows));
    }

    [HttpPost]
    public async Task<IActionResult> SaveKri([FromBody] KriSaveDto model)
    {
        return await ControllerHelpers.TrySave(model, ModelState, m => m.KriId,
            m => _kriTemplatesService.SaveKriAsync(m), "KRI");
    }

    [HttpPost]
    public async Task<IActionResult> DeleteKri([FromBody] DeleteRequestDto request)
    {
        return await ControllerHelpers.TryDelete(request, ModelState,
            id => _kriTemplatesService.DeleteKriAsync(id), "KRI");
    }
}
