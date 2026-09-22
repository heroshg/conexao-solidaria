using Identity.Domain.Entities;
using Identity.Domain.Enums;
using Identity.Domain.Exceptions;
using Identity.Domain.ValueObjects;

namespace Identity.Domain.Tests.Entities;

public class DonorTests
{
    private static readonly Email Email = Email.Create("donor@example.com");
    private static readonly Cpf Cpf = Cpf.Create("529.982.247-25");
    private const string PasswordHash = "irrelevant-hash";

    [Fact]
    public void Register_WithValidData_CreatesDonorWithDonorRole()
    {
        var donor = Donor.Register("  Maria Silva  ", Email, Cpf, PasswordHash);

        Assert.Equal("Maria Silva", donor.FullName);
        Assert.Equal(Email, donor.Email);
        Assert.Equal(Cpf, donor.Cpf);
        Assert.Equal(Role.Donor, donor.Role);
        Assert.Equal(PasswordHash, donor.PasswordHash);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Register_WithoutFullName_ThrowsDomainException(string fullName)
    {
        Assert.Throws<DomainException>(() => Donor.Register(fullName, Email, Cpf, PasswordHash));
    }

    [Fact]
    public void Register_WithoutPasswordHash_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() => Donor.Register("Maria Silva", Email, Cpf, ""));
    }
}
