namespace Campaigns.Infrastructure.Messaging;

/// <summary>
/// Exchange name agreed by hand with Donations.Infrastructure (must match exactly there).
/// Campaigns and Donations each define their OWN copy of DonationReceivedEvent, in different
/// namespaces (no shared contracts project — see CLAUDE.md), so MassTransit's default
/// type-name-based topology would NOT route between them; binding both sides to this literal
/// exchange name is what makes publish/consume actually connect.
/// </summary>
internal static class MessagingContracts
{
    public const string DonationReceivedExchange = "donation-received-event";
}
