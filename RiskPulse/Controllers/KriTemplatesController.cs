using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RiskPulse.Models.Dto;
using RiskPulse.Data.Entries;
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
        var statuses = Enum.GetValues<KriStatus>()
            .Select(s => new KriStatusOptionViewModel { Value = s.ToString(), Label = s.ToString() })
            .ToList();

        var unitGroups = await _unitsService.GetUnitGroupOptionsAsync();
        var units = await _unitsService.GetUnitOptionsAsync();

        var groups = (await _kriTemplatesService.GetThresholdGroupsAsync())
            .Select(g => new KriGroupOptionViewModel { Value = g.KriThresholdGroupId, Label = g.KriThresholdGroupDesc })
            .ToList();

        var colors = await _kriTemplatesService.GetColorsAsync();

        return View(new KriTemplatesIndexViewModel
        {
            KriStatuses = statuses,
            UnitGroups = unitGroups,
            Units = units,
            KriGroups = groups,
            Colors = colors
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
            var isNew = model.KriHeaderId == 0;
            var saved = await _kriTemplatesService.SaveHeaderAsync(model);

            return Json(ApiResponse.Ok(new { id = saved.KriHeaderId }, isNew ? "Template created successfully." : "Template updated successfully."));
        }
        catch (InvalidOperationException ex)
        {
            return Json(ApiResponse.Fail<object>(ex.Message));
        }
        catch (Exception)
        {
            return Json(ApiResponse.Fail<object>("An error occurred while saving. Please try again."));
        }
    }

    [HttpPost]
    public async Task<IActionResult> Delete([FromBody] DeleteRequestDto request)
    {
        if (request == null || !ModelState.IsValid)
        {
            var message = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .FirstOrDefault() ?? "Please correct the form errors and try again.";

            return Json(ApiResponse.Fail<object>(message));
        }

        try
        {
            await _kriTemplatesService.DeleteHeaderAsync(request.Id);
            return Json(ApiResponse.Ok<object>(new { }, "Template deleted successfully."));
        }
        catch (InvalidOperationException ex)
        {
            return Json(ApiResponse.Fail<object>(ex.Message));
        }
        catch (Exception)
        {
            return Json(ApiResponse.Fail<object>("An error occurred while deleting. Please try again."));
        }
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
            var isNew = model.KriId == 0;
            var saved = await _kriTemplatesService.SaveKriAsync(model);

            return Json(ApiResponse.Ok(new { id = saved.KriId }, isNew ? "KRI saved successfully." : "KRI updated successfully."));
        }
        catch (InvalidOperationException ex)
        {
            return Json(ApiResponse.Fail<object>(ex.Message));
        }
        catch (Exception)
        {
            return Json(ApiResponse.Fail<object>("An error occurred while saving the KRI. Please try again."));
        }
    }

    [HttpPost]
    public async Task<IActionResult> DeleteKri([FromBody] DeleteRequestDto request)
    {
        if (request == null || !ModelState.IsValid)
        {
            var message = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .FirstOrDefault() ?? "Please correct the form errors and try again.";

            return Json(ApiResponse.Fail<object>(message));
        }

        try
        {
            await _kriTemplatesService.DeleteKriAsync(request.Id);
            return Json(ApiResponse.Ok<object>(new { }, "KRI deleted successfully."));
        }
        catch (InvalidOperationException ex)
        {
            return Json(ApiResponse.Fail<object>(ex.Message));
        }
        catch (Exception)
        {
            return Json(ApiResponse.Fail<object>("An error occurred while deleting the KRI. Please try again."));
        }
    }

    // --- Threshold colors (grid/save/delete) ---
    [HttpGet]
    public async Task<IActionResult> ColorsGrid()
    {
        var rows = await _kriTemplatesService.GetColorRowsAsync();
        return Json(ApiResponse.Ok(rows));
    }

    [HttpPost]
    public async Task<IActionResult> SaveColor([FromBody] KriColorSaveDto model)
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
            var isNew = model.ColorId == 0;
            var saved = await _kriTemplatesService.SaveColorAsync(model);

            return Json(ApiResponse.Ok(new { id = saved.ColorId }, isNew ? "Color created successfully." : "Color updated successfully."));
        }
        catch (InvalidOperationException ex)
        {
            return Json(ApiResponse.Fail<object>(ex.Message));
        }
        catch (Exception)
        {
            return Json(ApiResponse.Fail<object>("An error occurred while saving. Please try again."));
        }
    }

    [HttpPost]
    public async Task<IActionResult> DeleteColor([FromBody] DeleteRequestDto request)
    {
        if (request == null || !ModelState.IsValid)
        {
            var message = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .FirstOrDefault() ?? "Please correct the form errors and try again.";

            return Json(ApiResponse.Fail<object>(message));
        }

        try
        {
            await _kriTemplatesService.DeleteColorAsync(request.Id);
            return Json(ApiResponse.Ok<object>(new { }, "Color deleted successfully."));
        }
        catch (InvalidOperationException ex)
        {
            return Json(ApiResponse.Fail<object>(ex.Message));
        }
        catch (Exception)
        {
            return Json(ApiResponse.Fail<object>("An error occurred while deleting. Please try again."));
        }
    }

    // --- Threshold groups (grid/save/delete) ---
    [HttpGet]
    public async Task<IActionResult> GroupsGrid()
    {
        var rows = await _kriTemplatesService.GetGroupRowsAsync();
        return Json(ApiResponse.Ok(rows));
    }

    [HttpPost]
    public async Task<IActionResult> SaveGroup([FromBody] KriThresholdGroupSaveDto model)
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
            var isNew = model.KriThresholdGroupId == 0;
            var saved = await _kriTemplatesService.SaveGroupAsync(model);

            return Json(ApiResponse.Ok(new { id = saved.KriThresholdGroupId }, isNew ? "Group created successfully." : "Group updated successfully."));
        }
        catch (InvalidOperationException ex)
        {
            return Json(ApiResponse.Fail<object>(ex.Message));
        }
        catch (Exception)
        {
            return Json(ApiResponse.Fail<object>("An error occurred while saving. Please try again."));
        }
    }

    [HttpPost]
    public async Task<IActionResult> DeleteGroup([FromBody] DeleteRequestDto request)
    {
        if (request == null || !ModelState.IsValid)
        {
            var message = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .FirstOrDefault() ?? "Please correct the form errors and try again.";

            return Json(ApiResponse.Fail<object>(message));
        }

        try
        {
            await _kriTemplatesService.DeleteGroupAsync(request.Id);
            return Json(ApiResponse.Ok<object>(new { }, "Group deleted successfully."));
        }
        catch (InvalidOperationException ex)
        {
            return Json(ApiResponse.Fail<object>(ex.Message));
        }
        catch (Exception)
        {
            return Json(ApiResponse.Fail<object>("An error occurred while deleting. Please try again."));
        }
    }

    // --- Threshold bands (grid/save/delete) ---
    [HttpGet]
    public async Task<IActionResult> BandsGrid(int kriThresholdGroupId)
    {
        var rows = await _kriTemplatesService.GetBandsAsync(kriThresholdGroupId);
        return Json(ApiResponse.Ok(rows));
    }

    [HttpPost]
    public async Task<IActionResult> SaveBands([FromBody] KriBandsSaveDto model)
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
            await _kriTemplatesService.SaveBandsAsync(model.KriThresholdGroupId, model.Bands);
            return Json(ApiResponse.Ok<object>(new { }, "Bands saved successfully."));
        }
        catch (InvalidOperationException ex)
        {
            return Json(ApiResponse.Fail<object>(ex.Message));
        }
        catch (Exception)
        {
            return Json(ApiResponse.Fail<object>("An error occurred while saving the bands. Please try again."));
        }
    }
}
