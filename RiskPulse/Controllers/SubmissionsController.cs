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

    private string CurrentUserRole =>
        User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

    // --- Page load (Index) ---
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    // --- Submissions grid (role-aware: all units for Risk/Admin, own unit for branch) ---
    [HttpGet]
    public async Task<IActionResult> Grid()
    {
        return await ControllerHelpers.TryExecute(async () =>
        {
            var rows = await _submissionsService.GetGridRowsAsync(CurrentUserId, CurrentUserRole);
            return Json(ApiResponse.Ok(rows));
        }, "Unable to load assessments. Please try again.");
    }

    // --- Assessment detail (role-aware) ---
    [HttpGet]
    public async Task<IActionResult> Detail(int id)
    {
        try
        {
            var model = await _submissionsService.GetDetailAsync(id, CurrentUserId, CurrentUserRole);
            return View(model);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    // --- SAQ entry ---
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

    // --- KRI entry ---
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

    // --- Workflow transitions: 4 Levels ---
    
    // Level 1: Unit Initiator submits an item
    [HttpPost]
    public async Task<IActionResult> SubmitItem([FromBody] ItemTransitionDto model)
    {
        var error = ControllerHelpers.ValidateModel(model, ModelState);
        if (error != null) return error;

        return await ControllerHelpers.TryExecute(async () =>
        {
            await _submissionsService.SubmitItemAsync(model!.AssessmentItemId, CurrentUserId, CurrentUserRole);
            return Json(ApiResponse.Ok<object>(new { }, "Item submitted successfully."));
        }, "An error occurred while submitting the item. Please try again.");
    }

    // Level 2: Unit Approver approves an item
    [HttpPost]
    public async Task<IActionResult> ApproveItem([FromBody] ItemTransitionDto model)
    {
        var error = ControllerHelpers.ValidateModel(model, ModelState);
        if (error != null) return error;

        return await ControllerHelpers.TryExecute(async () =>
        {
            await _submissionsService.ApproveItemAsync(model!.AssessmentItemId, CurrentUserId, CurrentUserRole);
            return Json(ApiResponse.Ok<object>(new { }, "Item approved successfully."));
        }, "An error occurred while approving the item. Please try again.");
    }

    // Level 2: Unit Approver signs off unit assessment and passes to Risk Dept
    [HttpPost]
    public async Task<IActionResult> UnitApprove([FromBody] UnitAuthorizeDto model)
    {
        var error = ControllerHelpers.ValidateModel(model, ModelState);
        if (error != null) return error;

        return await ControllerHelpers.TryExecute(async () =>
        {
            await _submissionsService.UnitApproveAsync(model!.AssessmentUnitId, CurrentUserId, CurrentUserRole);
            return Json(ApiResponse.Ok<object>(new { }, "Assessment authorized and passed to the Risk Department."));
        }, "An error occurred while authorizing the assessment. Please try again.");
    }

    // Level 3: Risk Dept Reviewer reviews and passes to Approver (CRO)
    [HttpPost]
    public async Task<IActionResult> RiskReview([FromBody] UnitAuthorizeDto model)
    {
        var error = ControllerHelpers.ValidateModel(model, ModelState);
        if (error != null) return error;

        return await ControllerHelpers.TryExecute(async () =>
        {
            await _submissionsService.RiskReviewAsync(model!.AssessmentUnitId, CurrentUserId, CurrentUserRole, model.Remarks);
            return Json(ApiResponse.Ok<object>(new { }, "Assessment reviewed and passed to the Final Approver."));
        }, "An error occurred while reviewing the assessment. Please try again.");
    }

    // Level 4: Risk Dept Approver (CRO) provides final sign-off
    [HttpPost]
    public async Task<IActionResult> FinalApprove([FromBody] UnitAuthorizeDto model)
    {
        var error = ControllerHelpers.ValidateModel(model, ModelState);
        if (error != null) return error;

        return await ControllerHelpers.TryExecute(async () =>
        {
            await _submissionsService.FinalApproveAsync(model!.AssessmentUnitId, CurrentUserId, CurrentUserRole, model.Remarks);
            return Json(ApiResponse.Ok<object>(new { }, "Assessment final approval completed successfully."));
        }, "An error occurred while approving the assessment. Please try again.");
    }

    // Return to Unit Initiator for revision
    [HttpPost]
    public async Task<IActionResult> ReturnAssessment([FromBody] UnitAuthorizeDto model)
    {
        var error = ControllerHelpers.ValidateModel(model, ModelState);
        if (error != null) return error;

        return await ControllerHelpers.TryExecute(async () =>
        {
            await _submissionsService.ReturnAssessmentAsync(model!.AssessmentUnitId, CurrentUserId, CurrentUserRole, model.Remarks);
            return Json(ApiResponse.Ok<object>(new { }, "Assessment returned to unit for revision."));
        }, "An error occurred while returning the assessment. Please try again.");
    }
}