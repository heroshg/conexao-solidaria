using Campaigns.Application.Dtos;
using Campaigns.Application.Exceptions;
using Campaigns.Application.Mapping;
using Campaigns.Domain.Enums;
using Campaigns.Domain.Exceptions;
using Campaigns.Domain.Repositories;
using Campaigns.Domain.ValueObjects;

namespace Campaigns.Application.UseCases;

public sealed class UpdateCampaignUseCase(ICampaignRepository repository, IUnitOfWork unitOfWork)
{
    public async Task<CampaignResponse> ExecuteAsync(
        Guid campaignId, UpdateCampaignRequest request, CancellationToken cancellationToken = default)
    {
        var campaign = await repository.GetByIdAsync(campaignId, cancellationToken)
            ?? throw new CampaignNotFoundException(campaignId);

        // Enum.Parse throws ArgumentException/ArgumentNullException on a garbage or
        // out-of-range value (e.g. "Foo", "99") — neither is caught by any registered
        // IExceptionResponseHandler, so it used to surface as an unhandled 500. TryParse +
        // IsDefined turns that into a clean 400 via DomainException instead.
        if (!Enum.TryParse<CampaignStatus>(request.Status, ignoreCase: true, out var status)
            || !Enum.IsDefined(status))
            throw new DomainException($"Invalid campaign status '{request.Status}'.");

        campaign.Update(
            request.Title, request.Description, request.StartDate, request.EndDate,
            Money.Create(request.FinancialGoal), status);

        var result = await unitOfWork.CommitAsync(cancellationToken);
        if (!result.IsSuccess)
            throw new CampaignConcurrencyConflictException(campaignId);

        return CampaignMapper.ToResponse(campaign);
    }
}
