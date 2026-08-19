using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using RiskPulse.Models.Dto;

namespace RiskPulse.Controllers;

/// <summary>
/// Static helpers that eliminate repeated validation and try/catch boilerplate in controllers.
/// </summary>
public static class ControllerHelpers
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Validates the bound model. Returns a <see cref="JsonResult"/> error response if invalid,
    /// or <c>null</c> when the model passes validation.
    /// </summary>
    public static JsonResult? ValidateModel<T>(T? model, ModelStateDictionary modelState) where T : class
    {
        if (model == null || !modelState.IsValid)
        {
            var message = modelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .FirstOrDefault() ?? "Please correct the form errors and try again.";

            return new JsonResult(ApiResponse.Fail<object>(message), JsonOptions);
        }

        return null;
    }

    /// <summary>
    /// Wraps an async action in a standard try/catch that surfaces <see cref="InvalidOperationException"/>
    /// messages to the client and falls back to a generic error for unexpected exceptions.
    /// </summary>
    public static async Task<IActionResult> TryExecute(Func<Task<IActionResult>> action, string genericError)
    {
        try
        {
            return await action();
        }
        catch (InvalidOperationException ex)
        {
            return new JsonResult(ApiResponse.Fail<object>(ex.Message), JsonOptions);
        }
        catch (Exception)
        {
            return new JsonResult(ApiResponse.Fail<object>(genericError), JsonOptions);
        }
    }

    public static async Task<IActionResult> TrySave<TDto>(
        TDto? model,
        ModelStateDictionary modelState,
        Func<TDto, int> getId,
        Func<TDto, Task<SaveResultDto>> save,
        string entityName,
        string? genericError = null) where TDto : class
    {
        var error = ValidateModel(model, modelState);
        if (error != null) return error;

        return await TryExecute(async () =>
        {
            var isNew = getId(model!) == 0;
            var result = await save(model!);
            return new JsonResult(ApiResponse.Ok(new { id = result.Id }, isNew ? $"{entityName} created successfully." : $"{entityName} updated successfully."), JsonOptions);
        }, genericError ?? "An error occurred while saving. Please try again.");
    }

    public static async Task<IActionResult> TryDelete(
        DeleteRequestDto? request,
        ModelStateDictionary modelState,
        Func<int, Task> delete,
        string entityName,
        string? genericError = null)
    {
        var error = ValidateModel(request, modelState);
        if (error != null) return error;

        return await TryExecute(async () =>
        {
            await delete(request!.Id);
            return new JsonResult(ApiResponse.Ok<object>(new { }, $"{entityName} deleted successfully."), JsonOptions);
        }, genericError ?? "An error occurred while deleting. Please try again.");
    }
}
