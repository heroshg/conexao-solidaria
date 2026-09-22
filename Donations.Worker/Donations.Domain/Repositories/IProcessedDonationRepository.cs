using Donations.Domain.Entities;

namespace Donations.Domain.Repositories;

public interface IProcessedDonationRepository
{
    Task<bool> ExistsAsync(Guid donationId, CancellationToken cancellationToken = default);
    Task AddAsync(ProcessedDonation processedDonation, CancellationToken cancellationToken = default);
}
