using Campaigns.Application.Exceptions;

namespace Campaigns.Api.ErrorHandling;

public sealed class CampaignNotReceivingDonationsExceptionHandler
    : ExceptionHandlerTemplate<CampaignNotReceivingDonationsException>
{
    protected override int GetStatusCode(CampaignNotReceivingDonationsException exception) =>
        StatusCodes.Status409Conflict;
}
