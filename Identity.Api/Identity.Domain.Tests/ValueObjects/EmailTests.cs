using Identity.Domain.Exceptions;
using Identity.Domain.ValueObjects;

namespace Identity.Domain.Tests.ValueObjects;

public class EmailTests
{
    [Theory]
    [InlineData("donor@example.com")]
    [InlineData("Donor.Name+tag@sub.example.com")]
    public void Create_WithValidValue_NormalizesToLowercase(string value)
    {
        var email = Email.Create(value);

        Assert.Equal(value.Trim().ToLowerInvariant(), email.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not-an-email")]
    [InlineData("missing-domain@")]
    [InlineData("@missing-local.com")]
    public void Create_WithInvalidValue_ThrowsDomainException(string value)
    {
        Assert.Throws<DomainException>(() => Email.Create(value));
    }

    [Fact]
    public void Create_WithNull_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() => Email.Create(null!));
    }

    [Fact]
    public void TwoEmails_WithSameValueDifferentCasing_AreEqual()
    {
        var first = Email.Create("Donor@Example.com");
        var second = Email.Create("donor@example.com");

        Assert.Equal(first, second);
    }
}
