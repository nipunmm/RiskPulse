namespace RiskPulse.Models.AppModel;

public class ApiResponse<T>
{
    public bool Success { get; set; }

    public string? Message { get; set; }

    public T? Data { get; set; }

    public object? Errors { get; set; }
}

public static class ApiResponse
{
    public static ApiResponse<T> Ok<T>(T data, string? message = null)
        => new() { Success = true, Data = data, Message = message };

    public static ApiResponse<T> Fail<T>(string message, object? errors = null)
        => new() { Success = false, Message = message, Errors = errors };
}