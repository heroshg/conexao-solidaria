using Donations.Domain.Entities;

namespace Donations.Domain.Repositories;

public interface ICampaignAmountRepository
{
    Task<CampaignAmountRecord?> GetByIdAsync(Guid campaignId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Atomic "SET AmountRaised = AmountRaised + amount" at the database level — no
    /// read-modify-write in application code, so two different concurrent donations to the
    /// same campaign can never lose one's increment to the other overwriting it. Must be
    /// invoked as IUnitOfWork.CommitAsync's beforeSaveChanges so it lands in the same
    /// transaction as the ProcessedDonations insert. Returns false if the row no longer exists.
    /// </summary>
    Task<bool> IncreaseAmountRaisedAsync(
        Guid campaignId, decimal amount, CancellationToken cancellationToken = default);
}
