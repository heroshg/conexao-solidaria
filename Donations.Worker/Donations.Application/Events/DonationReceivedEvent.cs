// Deliberately in the SAME namespace as Campaigns.Application.Events.DonationReceivedEvent,
// even though this is a different assembly with no project reference to Campaigns — no
// shared contracts project, per CLAUDE.md "Estrutura de pastas sugerida". MassTransit derives
// a message's routing identity (the "urn:message:<namespace>:<type>" it stamps on every
// publish and matches consumers against) from the CLR namespace + type name, NOT the
// assembly. Two independently-defined types with different namespaces are treated as
// different message types and never reach each other's consumers, even when bound to the
// same exchange — confirmed by testing this live: everything landed in "_skipped" until the
// namespaces were made to match. Keep this namespace and the record shape in sync by hand
// with the copy in Campaigns.Application.
namespace Campaigns.Application.Events;

public sealed record DonationReceivedEvent(Guid DonationId, Guid CampaignId, Guid DonorId, decimal DonationAmount);
