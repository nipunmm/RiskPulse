using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RiskPulse.Models.AppModel;
using RiskPulse.Models.DbModel.Kri;
using RiskPulse.Models.ViewModel;
using RiskPulse.Services.AccessControlService;
using RiskPulse.Services.KriService;

namespace RiskPulse.Controllers;

[Authorize(Policy = $"Permission:{PermissionCatalog.Kri}")]
public class KriTemplatesController : Controller
{
    private readonly KriService _kriService;

    public KriTemplatesController(KriService kriService)
    {
        _kriService = kriService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var statuses = Enum.GetValues<KriStatus>()
            .Select(s => new KriStatusOption { Value = s.ToString(), Label = s.ToString() })
            .ToList();

        var groups = (await _kriService.GetThresholdGroupsAsync())
            .Select(g => new KriGroupOption { Value = g.KriThresholdGroupId, Label = g.KriThresholdGroupDesc })
            .ToList();

        return View(new KriTemplatesIndexViewModel
        {
            KriStatuses = statuses,
            KriGroups = groups
        });
    }

    [HttpGet]
    public async Task<IActionResult> Grid()
    {
        var rows = await _kriService.GetHeaderRowsAsync();
        return Json(ApiResponse.Ok(rows));
    }

    [HttpPost]
    public async Task<IActionResult> Save([FromBody] KriHeaderSaveModel model)
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
            var saved = await _kriService.SaveHeaderAsync(model);

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
    public async Task<IActionResult> Delete([FromBody] KriDeleteRequest request)
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
            await _kriService.DeleteHeaderAsync(request.Id);
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

    [HttpGet]
    public async Task<IActionResult> KrisGrid(int kriHeaderId)
    {
        var rows = await _kriService.GetKrisAsync(kriHeaderId);
        return Json(ApiResponse.Ok(rows));
    }

    [HttpPost]
    public async Task<IActionResult> SaveKri([FromBody] KriSaveModel model)
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
            var saved = await _kriService.SaveKriAsync(model);

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
    public async Task<IActionResult> DeleteKri([FromBody] KriDeleteRequest request)
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
            await _kriService.DeleteKriAsync(request.Id);
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
}
