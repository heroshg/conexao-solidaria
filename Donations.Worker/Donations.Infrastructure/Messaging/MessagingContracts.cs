namespace Donations.Infrastructure.Messaging;

/// <summary>
/// Exchange name agreed by hand with Campaigns.Infrastructure (must match exactly there).
/// See the twin file in Campaigns.Infrastructure.Messaging for the full rationale.
/// </summary>
internal static class MessagingContracts
{
    public const string DonationReceivedExchange = "donation-received-event";
}
