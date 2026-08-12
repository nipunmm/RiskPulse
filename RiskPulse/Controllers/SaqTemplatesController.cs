using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RiskPulse.Models.AppModel;
using RiskPulse.Models.DbModel.Saq;
using RiskPulse.Models.ViewModel;
using RiskPulse.Services.AccessControlService;
using RiskPulse.Services.SaqService;

namespace RiskPulse.Controllers;

[Authorize(Policy = $"Permission:{PermissionCatalog.Saq}")]
public class SaqTemplatesController : Controller
{
    private readonly SaqService _saqService;

    public SaqTemplatesController(SaqService saqService)
    {
        _saqService = saqService;
    }

    [HttpGet]
    public IActionResult Index()
    {
        var statuses = Enum.GetValues<SaqStatus>()
            .Select(s => new SaqStatusOption { Value = s.ToString(), Label = s.ToString() })
            .ToList();

        return View(new SaqTemplatesIndexViewModel
        {
            SaqStatuses = statuses
        });
    }

    [HttpGet]
    public async Task<IActionResult> Grid()
    {
        var rows = await _saqService.GetHeaderRowsAsync();
        return Json(ApiResponse.Ok(rows));
    }

    [HttpPost]
    public async Task<IActionResult> Save([FromBody] SaqHeaderSaveModel model)
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
            var saved = await _saqService.SaveHeaderAsync(model);

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
    public async Task<IActionResult> Delete([FromBody] SaqDeleteRequest request)
    {
        if (request == null)
        {
            return Json(ApiResponse.Fail<object>("Invalid request."));
        }

        try
        {
            await _saqService.DeleteHeaderAsync(request.Id);
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
    public async Task<IActionResult> QuestionsGrid(int saqHeaderId)
    {
        var questions = await _saqService.GetQuestionsAsync(saqHeaderId);

        var rows = questions.Select(q => new SaqQuestionGridRow
        {
            QuestionId = q.QuestionId,
            QuestionText = q.QuestionText,
            AllowComment = q.AllowComment,
            DisplayOrder = q.DisplayOrder,
            Options = q.SaqQuestionOptions
                .OrderBy(o => o.DisplayOrder)
                .ThenBy(o => o.OptionId)
                .Select(o => new SaqOptionGridRow
                {
                    OptionId = o.OptionId,
                    OptionText = o.OptionText
                })
                .ToList()
        }).ToList();

        return Json(ApiResponse.Ok(rows));
    }

    [HttpPost]
    public async Task<IActionResult> SaveQuestion([FromBody] SaqQuestionSaveModel model)
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
            var saved = await _saqService.SaveQuestionAsync(model);

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
    public async Task<IActionResult> DeleteQuestion([FromBody] SaqDeleteRequest request)
    {
        if (request == null)
        {
            return Json(ApiResponse.Fail<object>("Invalid request."));
        }

        try
        {
            await _saqService.DeleteQuestionAsync(request.Id);
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
