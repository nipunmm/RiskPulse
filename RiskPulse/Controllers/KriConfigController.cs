using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RiskPulse.Models.AppModel;
using RiskPulse.Models.ViewModel;
using RiskPulse.Services.AccessControlService;
using RiskPulse.Services.KriConfigService;

namespace RiskPulse.Controllers;

[Authorize(Policy = $"Permission:{PermissionCatalog.KriConfig}")]
public class KriConfigController : Controller
{
    private readonly KriConfigService _kriConfigService;

    public KriConfigController(KriConfigService kriConfigService)
    {
        _kriConfigService = kriConfigService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var colors = await _kriConfigService.GetColorsAsync();

        return View(new KriConfigIndexViewModel
        {
            Colors = colors
        });
    }

    [HttpGet]
    public async Task<IActionResult> ColorsGrid()
    {
        var rows = await _kriConfigService.GetColorRowsAsync();
        return Json(ApiResponse.Ok(rows));
    }

    [HttpPost]
    public async Task<IActionResult> SaveColor([FromBody] KriColorSaveModel model)
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
            var saved = await _kriConfigService.SaveColorAsync(model);

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
    public async Task<IActionResult> DeleteColor([FromBody] KriDeleteRequest request)
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
            await _kriConfigService.DeleteColorAsync(request.Id);
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

    [HttpGet]
    public async Task<IActionResult> GroupsGrid()
    {
        var rows = await _kriConfigService.GetGroupRowsAsync();
        return Json(ApiResponse.Ok(rows));
    }

    [HttpPost]
    public async Task<IActionResult> SaveGroup([FromBody] KriThresholdGroupSaveModel model)
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
            var saved = await _kriConfigService.SaveGroupAsync(model);

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
    public async Task<IActionResult> DeleteGroup([FromBody] KriDeleteRequest request)
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
            await _kriConfigService.DeleteGroupAsync(request.Id);
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

    [HttpGet]
    public async Task<IActionResult> BandsGrid(int kriThresholdGroupId)
    {
        var rows = await _kriConfigService.GetBandsAsync(kriThresholdGroupId);
        return Json(ApiResponse.Ok(rows));
    }

    [HttpPost]
    public async Task<IActionResult> SaveBands([FromBody] KriBandsSaveModel model)
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
            await _kriConfigService.SaveBandsAsync(model.KriThresholdGroupId, model.Bands);
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
