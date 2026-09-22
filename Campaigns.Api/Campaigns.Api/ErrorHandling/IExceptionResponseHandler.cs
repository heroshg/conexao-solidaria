namespace Campaigns.Api.ErrorHandling;

/// <summary>One strategy per exception type, tried in order by GlobalExceptionHandler.</summary>
public interface IExceptionResponseHandler
{
    Task<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken);
}
