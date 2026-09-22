using Identity.Domain.Exceptions;
using Identity.Domain.ValueObjects;

namespace Identity.Domain.Tests.ValueObjects;

public class CpfTests
{
    // Checksum-valid CPF, commonly used in examples/tests.
    private const string ValidCpf = "529.982.247-25";

    [Fact]
    public void Create_WithValidCpf_StripsFormattingAndKeepsOnlyDigits()
    {
        var cpf = Cpf.Create(ValidCpf);

        Assert.Equal("52998224725", cpf.Value);
    }

    [Fact]
    public void Create_WithAllDigitsTheSame_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() => Cpf.Create("111.111.111-11"));
    }

    [Fact]
    public void Create_WithWrongCheckDigits_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() => Cpf.Create("529.982.247-00"));
    }

    [Theory]
    [InlineData("123")]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidLengthOrEmpty_ThrowsDomainException(string value)
    {
        Assert.Throws<DomainException>(() => Cpf.Create(value));
    }
}
