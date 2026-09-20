using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RiskPulse.Models.Dto;
using RiskPulse.Services.Assessment;
using RiskPulse.Services.Login;

namespace RiskPulse.Controllers;

[Authorize(Policy = $"Permission:{PermissionCatalog.Submissions}")]
public class SubmissionsController : Controller
{
    private readonly SubmissionsService _submissionsService;

    public SubmissionsController(SubmissionsService submissionsService)
    {
        _submissionsService = submissionsService;
    }

    private int CurrentUserId =>
        int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;

    // --- Page load (Index) ---
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    // --- My-unit grid ---
    [HttpGet]
    public async Task<IActionResult> Grid()
    {
        return await ControllerHelpers.TryExecute(async () =>
        {
            var rows = await _submissionsService.GetGridRowsAsync(CurrentUserId);
            return Json(ApiResponse.Ok(rows));
        }, "Unable to load assessments. Please try again.");
    }

    // --- Assessment detail (own unit only) ---
    [HttpGet]
    public async Task<IActionResult> Detail(int id)
    {
        try
        {
            var model = await _submissionsService.GetDetailAsync(id, CurrentUserId);
            return View(model);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    // --- SAQ entry (own unit only) ---
    [HttpGet]
    public async Task<IActionResult> SaqEntry(int id)
    {
        try
        {
            var model = await _submissionsService.GetSaqEntryAsync(id, CurrentUserId);
            return View(model);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    // --- KRI entry (own unit only) ---
    [HttpGet]
    public async Task<IActionResult> KriEntry(int id)
    {
        try
        {
            var model = await _submissionsService.GetKriEntryAsync(id, CurrentUserId);
            return View(model);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    // --- Draft saves ---
    [HttpPost]
    public async Task<IActionResult> SaveSaq([FromBody] SaveSaqAnswersDto model)
    {
        var error = ControllerHelpers.ValidateModel(model, ModelState);
        if (error != null) return error;

        return await ControllerHelpers.TryExecute(async () =>
        {
            await _submissionsService.SaveSaqAnswersAsync(model!, CurrentUserId);
            return Json(ApiResponse.Ok<object>(new { }, "Answers saved successfully."));
        }, "An error occurred while saving your answers. Please try again.");
    }

    [HttpPost]
    public async Task<IActionResult> SaveKri([FromBody] SaveKriValuesDto model)
    {
        var error = ControllerHelpers.ValidateModel(model, ModelState);
        if (error != null) return error;

        return await ControllerHelpers.TryExecute(async () =>
        {
            await _submissionsService.SaveKriValuesAsync(model!, CurrentUserId);
            return Json(ApiResponse.Ok<object>(new { }, "Values saved successfully."));
        }, "An error occurred while saving your values. Please try again.");
    }

    // --- Workflow transitions ---
    [HttpPost]
    public async Task<IActionResult> SubmitItem([FromBody] ItemTransitionDto model)
    {
        var error = ControllerHelpers.ValidateModel(model, ModelState);
        if (error != null) return error;

        return await ControllerHelpers.TryExecute(async () =>
        {
            await _submissionsService.SubmitItemAsync(model!.AssessmentItemId, CurrentUserId);
            return Json(ApiResponse.Ok<object>(new { }, "Item submitted successfully."));
        }, "An error occurred while submitting the item. Please try again.");
    }

    [HttpPost]
    public async Task<IActionResult> ApproveItem([FromBody] ItemTransitionDto model)
    {
        var error = ControllerHelpers.ValidateModel(model, ModelState);
        if (error != null) return error;

        return await ControllerHelpers.TryExecute(async () =>
        {
            await _submissionsService.ApproveItemAsync(model!.AssessmentItemId, CurrentUserId);
            return Json(ApiResponse.Ok<object>(new { }, "Item approved successfully."));
        }, "An error occurred while approving the item. Please try again.");
    }

    [HttpPost]
    public async Task<IActionResult> AuthorizeUnit([FromBody] UnitAuthorizeDto model)
    {
        var error = ControllerHelpers.ValidateModel(model, ModelState);
        if (error != null) return error;

        return await ControllerHelpers.TryExecute(async () =>
        {
            await _submissionsService.AuthorizeUnitAsync(model!.AssessmentUnitId, CurrentUserId);
            return Json(ApiResponse.Ok<object>(new { }, "Assessment authorized successfully."));
        }, "An error occurred while authorizing the assessment. Please try again.");
    }
}