namespace Campaigns.Application.Dtos;

public sealed record CreateCampaignRequest(
    string Title, string Description, DateTime StartDate, DateTime EndDate, decimal FinancialGoal);

public sealed record UpdateCampaignRequest(
    string Title, string Description, DateTime StartDate, DateTime EndDate, decimal FinancialGoal, string Status);

public sealed record CampaignResponse(
    Guid Id, string Title, string Description, DateTime StartDate, DateTime EndDate,
    decimal FinancialGoal, decimal AmountRaised, string Status);

public sealed record PublicCampaignResponse(string Title, decimal FinancialGoal, decimal AmountRaised);
