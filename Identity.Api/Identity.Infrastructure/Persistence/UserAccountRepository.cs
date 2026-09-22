using Identity.Domain.Entities;
using Identity.Domain.Repositories;
using Identity.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Persistence;

public class UserAccountRepository(IdentityDbContext context) : IUserAccountRepository
{
    public Task<UserAccount?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default) =>
        context.Accounts.SingleOrDefaultAsync(x => x.Email == email, cancellationToken);

    public Task<bool> ExistsByEmailAsync(Email email, CancellationToken cancellationToken = default) =>
        context.Accounts.AnyAsync(x => x.Email == email, cancellationToken);

    // Cpf only exists on the Donor subclass (TPH) — NgoManager accounts never collide on it.
    public Task<bool> ExistsByCpfAsync(Cpf cpf, CancellationToken cancellationToken = default) =>
        context.Accounts.OfType<Donor>().AnyAsync(x => x.Cpf == cpf, cancellationToken);

    public async Task AddAsync(UserAccount account, CancellationToken cancellationToken = default) =>
        await context.Accounts.AddAsync(account, cancellationToken);
}
