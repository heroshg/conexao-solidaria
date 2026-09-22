using Identity.Application.Abstractions;
using Identity.Domain.Entities;
using Identity.Domain.ValueObjects;
using Identity.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Identity.Infrastructure.Seed;

/// <summary>
/// Applies migrations and seeds one NgoManager account for testing/demo — see CLAUDE.md
/// "Regras de negócio": there is no public endpoint to create NgoManager accounts.
/// Credentials must be documented in README.md for graders.
/// </summary>
public static class NgoManagerSeeder
{
    public const string SeedEmail = "admin@esperancasolidaria.org";
    public const string SeedPassword = "Admin@12345";

    public static async Task SeedAsync(IdentityDbContext context, IPasswordHasher passwordHasher, ILogger logger)
    {
        await context.Database.MigrateAsync();

        if (await context.NgoManagers.AnyAsync())
            return;

        var manager = NgoManager.CreateSeedAdmin(
            "Administrador Esperança Solidária",
            Email.Create(SeedEmail),
            passwordHasher.Hash(SeedPassword));

        context.NgoManagers.Add(manager);
        await context.SaveChangesAsync();

        logger.LogInformation("Seeded initial NgoManager account: {Email}", SeedEmail);
    }
}
