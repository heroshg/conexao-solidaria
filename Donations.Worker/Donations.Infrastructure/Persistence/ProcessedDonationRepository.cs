using Donations.Domain.Entities;
using Donations.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Donations.Infrastructure.Persistence;

public class ProcessedDonationRepository(DonationsDbContext context) : IProcessedDonationRepository
{
    public Task<bool> ExistsAsync(Guid donationId, CancellationToken cancellationToken = default) =>
        context.ProcessedDonations.AnyAsync(x => x.Id == donationId, cancellationToken);

    public async Task AddAsync(ProcessedDonation processedDonation, CancellationToken cancellationToken = default) =>
        await context.ProcessedDonations.AddAsync(processedDonation, cancellationToken);
}
