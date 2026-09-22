using Donations.Domain.Entities;

namespace Donations.Domain.Tests.Entities;

public class CampaignAmountRecordTests
{
    [Theory]
    [InlineData("Active", true)]
    [InlineData("Completed", false)]
    [InlineData("Cancelled", false)]
    public void IsActive_ReflectsStatusColumn(string status, bool expected)
    {
        var record = CampaignAmountRecord.CreateForTesting(Guid.NewGuid(), status, amountRaised: 0);

        Assert.Equal(expected, record.IsActive);
    }
}
