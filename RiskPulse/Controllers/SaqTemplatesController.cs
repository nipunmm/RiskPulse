using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RiskPulse.Models.Dto;
using RiskPulse.Data.Entries;
using RiskPulse.Models.ViewModel;
using RiskPulse.Services.Login;
using RiskPulse.Services.Templates;

namespace RiskPulse.Controllers;

[Authorize(Policy = $"Permission:{PermissionCatalog.Saq}")]
public class SaqTemplatesController : Controller
{
    private readonly SaqTemplatesService _saqTemplatesService;

    public SaqTemplatesController(SaqTemplatesService saqTemplatesService)
    {
        _saqTemplatesService = saqTemplatesService;
    }

    // --- Page load (Index) ---
    [HttpGet]
    public IActionResult Index()
    {
        var statuses = Enum.GetValues<SaqStatus>()
            .Select(s => new SaqStatusOptionViewModel { Value = s.ToString(), Label = s.ToString() })
            .ToList();

        return View(new SaqTemplatesIndexViewModel
        {
            SaqStatuses = statuses
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
            var isNew = model.SaqHeaderId == 0;
            var saved = await _saqTemplatesService.SaveHeaderAsync(model);

            return Json(ApiResponse.Ok(new { id = saved.SaqHeaderId }, isNew ? "Template created successfully." : "Template updated successfully."));
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
    public async Task<IActionResult> Delete([FromBody] SaqDeleteRequestDto request)
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
            await _saqTemplatesService.DeleteHeaderAsync(request.Id);
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

    // --- SAQ questions (grid/save/delete) ---
    [HttpGet]
    public async Task<IActionResult> QuestionsGrid(int saqHeaderId)
    {
        var questions = await _saqTemplatesService.GetQuestionsAsync(saqHeaderId);

        var rows = questions.Select(q => new SaqQuestionGridRowViewModel
        {
            QuestionId = q.QuestionId,
            QuestionText = q.QuestionText,
            AllowComment = q.AllowComment,
            DisplayOrder = q.DisplayOrder,
            Options = q.SaqQuestionOptions
                .OrderBy(o => o.DisplayOrder)
                .ThenBy(o => o.OptionId)
                .Select(o => new SaqOptionGridRowViewModel
                {
                    OptionId = o.OptionId,
                    OptionText = o.OptionText
                })
                .ToList()
        }).ToList();

        return Json(ApiResponse.Ok(rows));
    }

    [HttpPost]
    public async Task<IActionResult> SaveQuestion([FromBody] SaqQuestionSaveDto model)
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
            var isNew = model.QuestionId == 0;
            var saved = await _saqTemplatesService.SaveQuestionAsync(model);

            return Json(ApiResponse.Ok(new { id = saved.QuestionId }, isNew ? "Question saved successfully." : "Question updated successfully."));
        }
        catch (InvalidOperationException ex)
        {
            return Json(ApiResponse.Fail<object>(ex.Message));
        }
        catch (Exception)
        {
            return Json(ApiResponse.Fail<object>("An error occurred while saving the question. Please try again."));
        }
    }

    [HttpPost]
    public async Task<IActionResult> DeleteQuestion([FromBody] SaqDeleteRequestDto request)
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
            await _saqTemplatesService.DeleteQuestionAsync(request.Id);
            return Json(ApiResponse.Ok<object>(new { }, "Question deleted successfully."));
        }
        catch (InvalidOperationException ex)
        {
            return Json(ApiResponse.Fail<object>(ex.Message));
        }
        catch (Exception)
        {
            return Json(ApiResponse.Fail<object>("An error occurred while deleting the question. Please try again."));
        }
    }
}
