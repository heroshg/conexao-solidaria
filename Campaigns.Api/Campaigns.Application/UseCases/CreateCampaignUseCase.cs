using Campaigns.Application.Dtos;
using Campaigns.Application.Mapping;
using Campaigns.Domain.Entities;
using Campaigns.Domain.Repositories;
using Campaigns.Domain.ValueObjects;

namespace Campaigns.Application.UseCases;

public sealed class CreateCampaignUseCase(ICampaignRepository repository, IUnitOfWork unitOfWork)
{
    public async Task<CampaignResponse> ExecuteAsync(
        CreateCampaignRequest request, CancellationToken cancellationToken = default)
    {
        var campaign = Campaign.Create(
            request.Title, request.Description, request.StartDate, request.EndDate,
            Money.Create(request.FinancialGoal));

        await repository.AddAsync(campaign, cancellationToken);

        // No known unique-constraint conflict for Campaign today, so CampaignsUnitOfWork never
        // returns a Failure here — a genuine DB error rethrows instead of reaching this line.
        await unitOfWork.CommitAsync(cancellationToken);

        return CampaignMapper.ToResponse(campaign);
    }
}
