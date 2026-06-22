using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.Filters;
using SunAdmin.Application.Abstractions;
using SunAdmin.Domain.Entities;

namespace SunAdmin.Api.Security;

public sealed class OperationLogActionFilter(IFreeSql freeSql, ICurrentUser currentUser) : IAsyncActionFilter
{
    private static readonly HashSet<string> LoggedMethods = new(StringComparer.OrdinalIgnoreCase)
    {
        HttpMethods.Post,
        HttpMethods.Put,
        HttpMethods.Delete,
        HttpMethods.Patch
    };

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var stopwatch = Stopwatch.StartNew();
        ActionExecutedContext? executedContext = null;
        try
        {
            executedContext = await next();
        }
        finally
        {
            stopwatch.Stop();
            await TryWriteLogAsync(context, executedContext, stopwatch.ElapsedMilliseconds);
        }
    }

    private async Task TryWriteLogAsync(ActionExecutingContext context, ActionExecutedContext? executedContext, long durationMs)
    {
        var httpContext = context.HttpContext;
        if (!LoggedMethods.Contains(httpContext.Request.Method) || httpContext.Request.Path.StartsWithSegments("/health"))
        {
            return;
        }

        try
        {
            var statusCode = executedContext?.Exception is null
                ? httpContext.Response.StatusCode
                : StatusCodes.Status500InternalServerError;
            await freeSql.Insert(new OperationLog
            {
                UserId = currentUser.UserId,
                UserName = currentUser.UserName ?? "anonymous",
                Method = httpContext.Request.Method,
                Path = httpContext.Request.Path.Value ?? string.Empty,
                StatusCode = statusCode,
                Succeeded = executedContext?.Exception is null && statusCode < 400,
                DurationMs = durationMs,
                IpAddress = httpContext.Connection.RemoteIpAddress?.ToString(),
                UserAgent = httpContext.Request.Headers.UserAgent.ToString(),
                ErrorMessage = executedContext?.Exception?.Message
            }).ExecuteAffrowsAsync(httpContext.RequestAborted);
        }
        catch
        {
            // Logging failure must not block business requests.
        }
    }
}
