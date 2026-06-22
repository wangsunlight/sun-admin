namespace SunAdmin.Contracts.Common;

public sealed record ApiResponse<T>(string Code, string Message, T? Data)
{
    public static ApiResponse<T> Ok(T? data, string message = "success") => new("OK", message, data);
    public static ApiResponse<T> Fail(string code, string message) => new(code, message, default);
}

public sealed record ApiResponse(string Code, string Message, object? Data)
{
    public static ApiResponse Ok(string message = "success") => new("OK", message, null);
    public static ApiResponse Fail(string code, string message) => new(code, message, null);
}
