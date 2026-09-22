using Identity.Application.Abstractions;
using Identity.Application.Dtos;
using Identity.Application.Exceptions;
using Identity.Domain.Repositories;
using Identity.Domain.ValueObjects;

namespace Identity.Application.UseCases;

public sealed class LoginUseCase(
    IUserAccountRepository repository,
    IPasswordHasher passwordHasher,
    ITokenGenerator tokenGenerator)
{
    public async Task<LoginResponse> ExecuteAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var email = Email.Create(request.Email);

        var account = await repository.GetByEmailAsync(email, cancellationToken);
        if (account is null)
        {
            // Argon2id is deliberately slow (see Argon2PasswordHasher) — returning immediately
            // here instead of running one anyway would make "email not registered" respond
            // near-instantly versus "registered, wrong password" taking the full hash time,
            // letting an attacker enumerate registered emails purely from response latency.
            // Hash()'s one Argon2id computation costs the same as Verify()'s below.
            passwordHasher.Hash(request.Password);
            throw new InvalidCredentialsException();
        }

        if (!passwordHasher.Verify(request.Password, account.PasswordHash))
            throw new InvalidCredentialsException();

        var token = tokenGenerator.Generate(account);

        return new LoginResponse(token.AccessToken, token.ExpiresAtUtc, account.Role.ToString());
    }
}
