using Campaigns.Application.Exceptions;

namespace Campaigns.Api.ErrorHandling;

public sealed class CampaignConcurrencyConflictExceptionHandler
    : ExceptionHandlerTemplate<CampaignConcurrencyConflictException>
{
    protected override int GetStatusCode(CampaignConcurrencyConflictException exception) =>
        StatusCodes.Status409Conflict;
}
