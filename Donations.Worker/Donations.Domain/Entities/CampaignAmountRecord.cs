namespace Donations.Domain.Entities;

/// <summary>
/// Slim view of Campaigns.Api's "Campaigns" row — the only two columns this service is
/// allowed to touch (Status, to defend-in-depth against a stale event; AmountRaised, the one
/// this service owns writing to). Maps to the SAME physical table as Campaigns.Api, but this
/// context never migrates it — see CLAUDE.md "database per service" exception.
/// </summary>
public sealed class CampaignAmountRecord
{
    public Guid Id { get; private set; }
    public string Status { get; private set; } = null!;
    public decimal AmountRaised { get; private set; }

    private CampaignAmountRecord() { }

    public bool IsActive => Status == "Active";

    // No Increase(decimal) here on purpose: AmountRaised is updated via
    // ICampaignAmountRepository.IncreaseAmountRaisedAsync, an atomic "SET x = x + amount" SQL
    // statement — a read-modify-write on this in-memory property would race under concurrent
    // donations to the same campaign (two different donations could read the same starting
    // value and one's increment would silently overwrite the other's).

    // EF Core materializes real instances via the private ctor + reflection at runtime; this
    // lets Donations.Domain.Tests set up a specific state without reflection hacks.
    internal static CampaignAmountRecord CreateForTesting(Guid id, string status, decimal amountRaised) =>
        new() { Id = id, Status = status, AmountRaised = amountRaised };
}
