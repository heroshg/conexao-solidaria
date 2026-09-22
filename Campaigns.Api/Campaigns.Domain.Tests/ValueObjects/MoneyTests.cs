using Campaigns.Domain.Exceptions;
using Campaigns.Domain.ValueObjects;

namespace Campaigns.Domain.Tests.ValueObjects;

public class MoneyTests
{
    [Fact]
    public void Create_WithPositiveAmount_Succeeds()
    {
        var money = Money.Create(100.50m);

        Assert.Equal(100.50m, money.Amount);
    }

    [Fact]
    public void Create_WithZero_Succeeds()
    {
        var money = Money.Create(0);

        Assert.Equal(0, money.Amount);
    }

    [Fact]
    public void Create_WithNegativeAmount_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() => Money.Create(-1));
    }

    [Fact]
    public void Zero_ReturnsAmountEqualToZero()
    {
        Assert.Equal(0, Money.Zero().Amount);
    }

    [Fact]
    public void Add_SumsBothAmounts()
    {
        var result = Money.Create(100).Add(Money.Create(50));

        Assert.Equal(150, result.Amount);
    }

    [Fact]
    public void TwoInstances_WithSameAmount_AreEqual()
    {
        Assert.Equal(Money.Create(42), Money.Create(42));
    }
}
