using Microsoft.AspNetCore.Mvc;

namespace Campaigns.Api.ErrorHandling;

/// <summary>
/// Template Method: fixes the algorithm for turning ONE exception type into an HTTP problem
/// response (check type -> build ProblemDetails -> write it). Concrete handlers only override
/// the two hook steps that vary (status code and, optionally, title/detail) — no handler needs
/// its own try/catch, and none of this runs inside a service/use case.
/// </summary>
public abstract class ExceptionHandlerTemplate<TException> : IExceptionResponseHandler
    where TException : Exception
{
    public async Task<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not TException typedException)
            return false;

        var problemDetails = new ProblemDetails
        {
            Status = GetStatusCode(typedException),
            Title = GetTitle(typedException),
            Detail = GetDetail(typedException),
            Instance = context.Request.Path
        };

        context.Response.StatusCode = problemDetails.Status!.Value;
        await context.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        return true;
    }

    protected abstract int GetStatusCode(TException exception);
    protected virtual string GetTitle(TException exception) => typeof(TException).Name;
    protected virtual string GetDetail(TException exception) => exception.Message;
}
