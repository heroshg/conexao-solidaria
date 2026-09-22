using Campaigns.Application.Exceptions;

namespace Campaigns.Api.ErrorHandling;

public sealed class CampaignNotFoundExceptionHandler : ExceptionHandlerTemplate<CampaignNotFoundException>
{
    protected override int GetStatusCode(CampaignNotFoundException exception) => StatusCodes.Status404NotFound;
}
