using Identity.Domain.Exceptions;
using Identity.Domain.ValueObjects;

namespace Identity.Domain.Tests.ValueObjects;

public class PasswordTests
{
    [Fact]
    public void Create_WithAtLeastMinimumLength_Succeeds()
    {
        var password = Password.Create("Sup3rSecret!");

        Assert.Equal("Sup3rSecret!", password.Value);
    }

    [Theory]
    [InlineData("short1")]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ShorterThanMinimumLength_ThrowsDomainException(string value)
    {
        Assert.Throws<DomainException>(() => Password.Create(value));
    }

    [Fact]
    public void Create_WithNull_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() => Password.Create(null!));
    }
}
