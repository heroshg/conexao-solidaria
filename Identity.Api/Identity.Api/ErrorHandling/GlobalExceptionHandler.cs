using Microsoft.AspNetCore.Diagnostics;

namespace Identity.Api.ErrorHandling;

/// <summary>
/// Single global entry point (ASP.NET Core's built-in exception-handling middleware calls
/// this — no try/catch written by us). Dispatches to the first registered
/// IExceptionResponseHandler that matches; each one runs its own Template Method. Falls back
/// to the framework's default ProblemDetails (500) when nothing matches.
/// </summary>
public sealed class GlobalExceptionHandler(
    IEnumerable<IExceptionResponseHandler> handlers,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        foreach (var handler in handlers)
        {
            if (await handler.TryHandleAsync(httpContext, exception, cancellationToken))
                return true;
        }

        logger.LogError(exception, "Unhandled exception processing {Path}", httpContext.Request.Path);
        return false;
    }
}
