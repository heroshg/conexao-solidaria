using Identity.Domain.Entities;

namespace Identity.Application.Abstractions;

public interface ITokenGenerator
{
    TokenResult Generate(UserAccount account);
}

public sealed record TokenResult(string AccessToken, DateTime ExpiresAtUtc);
