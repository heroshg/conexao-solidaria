using Campaigns.Domain.Entities;
using Campaigns.Domain.Enums;
using Campaigns.Domain.Exceptions;
using Campaigns.Domain.ValueObjects;

namespace Campaigns.Domain.Tests.Entities;

public class CampaignTests
{
    private static readonly DateTime StartDate = DateTime.UtcNow.AddDays(1);
    private static readonly DateTime EndDate = DateTime.UtcNow.AddDays(30);
    private static readonly Money FinancialGoal = Money.Create(10_000m);

    private static Campaign CreateValidCampaign() =>
        Campaign.Create("Campanha de Inverno", "Agasalhos para crianças", StartDate, EndDate, FinancialGoal);

    [Fact]
    public void Create_WithValidData_StartsActiveWithZeroAmountRaised()
    {
        var campaign = CreateValidCampaign();

        Assert.Equal(CampaignStatus.Active, campaign.Status);
        Assert.Equal(0, campaign.AmountRaised.Amount);
        Assert.Equal("Campanha de Inverno", campaign.Title);
    }

    [Fact]
    public void Create_WithEndDateInThePast_ThrowsDomainException()
    {
        var pastEndDate = DateTime.UtcNow.AddDays(-1);

        Assert.Throws<DomainException>(() =>
            Campaign.Create("Title", "Description", StartDate, pastEndDate, FinancialGoal));
    }

    [Fact]
    public void Create_WithEndDateBeforeStartDate_ThrowsDomainException()
    {
        var start = DateTime.UtcNow.AddDays(10);
        var end = DateTime.UtcNow.AddDays(5);

        Assert.Throws<DomainException>(() =>
            Campaign.Create("Title", "Description", start, end, FinancialGoal));
    }

    [Fact]
    public void Create_WithFinancialGoalZero_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() =>
            Campaign.Create("Title", "Description", StartDate, EndDate, Money.Zero()));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithoutTitle_ThrowsDomainException(string title)
    {
        Assert.Throws<DomainException>(() =>
            Campaign.Create(title, "Description", StartDate, EndDate, FinancialGoal));
    }

    [Fact]
    public void Update_WithValidData_AppliesNewValuesAndStatus()
    {
        var campaign = CreateValidCampaign();
        var newEndDate = EndDate.AddDays(10);

        campaign.Update("New Title", "New Description", StartDate, newEndDate, Money.Create(20_000m), CampaignStatus.Completed);

        Assert.Equal("New Title", campaign.Title);
        Assert.Equal(CampaignStatus.Completed, campaign.Status);
        Assert.Equal(20_000m, campaign.FinancialGoal.Amount);
    }

    [Fact]
    public void Update_WithEndDateInThePast_ThrowsDomainException()
    {
        var campaign = CreateValidCampaign();
        var pastEndDate = DateTime.UtcNow.AddDays(-1);

        Assert.Throws<DomainException>(() =>
            campaign.Update("Title", "Description", StartDate, pastEndDate, FinancialGoal, CampaignStatus.Active));
    }

    [Theory]
    [InlineData(CampaignStatus.Active, true)]
    [InlineData(CampaignStatus.Completed, false)]
    [InlineData(CampaignStatus.Cancelled, false)]
    public void CanReceiveDonations_ReflectsStatus(CampaignStatus status, bool expected)
    {
        var campaign = CreateValidCampaign();
        campaign.Update(campaign.Title, campaign.Description, campaign.StartDate, campaign.EndDate, campaign.FinancialGoal, status);

        Assert.Equal(expected, campaign.CanReceiveDonations());
    }
}
