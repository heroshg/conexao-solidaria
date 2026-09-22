using Identity.Domain.Entities;
using Identity.Domain.Enums;
using Identity.Domain.Exceptions;
using Identity.Domain.ValueObjects;

namespace Identity.Domain.Tests.Entities;

public class NgoManagerTests
{
    private static readonly Email Email = Email.Create("admin@esperancasolidaria.org");
    private const string PasswordHash = "irrelevant-hash";

    [Fact]
    public void CreateSeedAdmin_WithValidData_CreatesManagerWithNgoManagerRole()
    {
        var manager = NgoManager.CreateSeedAdmin("Admin", Email, PasswordHash);

        Assert.Equal("Admin", manager.FullName);
        Assert.Equal(Role.NgoManager, manager.Role);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateSeedAdmin_WithoutFullName_ThrowsDomainException(string fullName)
    {
        Assert.Throws<DomainException>(() => NgoManager.CreateSeedAdmin(fullName, Email, PasswordHash));
    }
}
