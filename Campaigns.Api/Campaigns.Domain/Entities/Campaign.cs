using Campaigns.Domain.Common;
using Campaigns.Domain.Enums;
using Campaigns.Domain.Exceptions;
using Campaigns.Domain.ValueObjects;

namespace Campaigns.Domain.Entities;

public sealed class Campaign : Entity
{
    public string Title { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public Money FinancialGoal { get; private set; } = null!;
    public CampaignStatus Status { get; private set; }

    /// <summary>
    /// Written only by Donations.Worker (same physical DB, different service/DbContext) —
    /// Campaigns.Api never sets this after creation. See CLAUDE.md "Regras de negócio".
    /// </summary>
    public Money AmountRaised { get; private set; } = null!;

    private Campaign() { }

    private Campaign(string title, string description, DateTime startDate, DateTime endDate, Money financialGoal)
    {
        Title = title;
        Description = description;
        StartDate = startDate;
        EndDate = endDate;
        FinancialGoal = financialGoal;
        Status = CampaignStatus.Active;
        AmountRaised = Money.Zero();
    }

    public static Campaign Create(
        string title, string description, DateTime startDate, DateTime endDate, Money financialGoal)
    {
        ValidateFields(title, description, startDate, endDate, financialGoal);

        if (endDate <= DateTime.UtcNow)
            throw new DomainException("EndDate cannot be in the past.");

        return new Campaign(title.Trim(), description.Trim(), startDate, endDate, financialGoal);
    }

    public void Update(
        string title, string description, DateTime startDate, DateTime endDate,
        Money financialGoal, CampaignStatus status)
    {
        // No past-date check here (unlike Create): an NgoManager must be able to manually close
        // out a campaign whose EndDate already elapsed (no auto-expiry job exists — see
        // CLAUDE.md) without being forced to also lie about EndDate just to pass validation.
        ValidateFields(title, description, startDate, endDate, financialGoal);
        ValidateStatusTransition(status);

        Title = title.Trim();
        Description = description.Trim();
        StartDate = startDate;
        EndDate = endDate;
        FinancialGoal = financialGoal;
        Status = status;
    }

    public bool CanReceiveDonations() => Status == CampaignStatus.Active;

    /// <summary>
    /// Completed and Cancelled are terminal — once a campaign leaves Active, it cannot be
    /// reopened. Prevents e.g. PUT silently moving Completed -&gt; Active or Cancelled -&gt; Active.
    /// </summary>
    private void ValidateStatusTransition(CampaignStatus status)
    {
        if (!Enum.IsDefined(status))
            throw new DomainException($"Invalid campaign status.");

        if (Status == status)
            return;

        if (Status is CampaignStatus.Completed or CampaignStatus.Cancelled)
            throw new DomainException($"Campaign status '{Status}' is terminal and cannot change to '{status}'.");
    }

    private static void ValidateFields(
        string title, string description, DateTime startDate, DateTime endDate, Money financialGoal)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Title is required.");

        if (string.IsNullOrWhiteSpace(description))
            throw new DomainException("Description is required.");

        if (endDate <= startDate)
            throw new DomainException("EndDate must be after StartDate.");

        if (financialGoal.Amount <= 0)
            throw new DomainException("FinancialGoal must be greater than zero.");
    }
}
