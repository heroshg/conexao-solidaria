using Identity.Domain.Entities;
using Identity.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity.Infrastructure.Persistence.Configurations;

public class UserAccountConfiguration : IEntityTypeConfiguration<UserAccount>
{
    public void Configure(EntityTypeBuilder<UserAccount> builder)
    {
        builder.ToTable("UserAccounts");
        builder.HasKey(x => x.Id);

        builder.HasDiscriminator<string>("AccountType")
            .HasValue<Donor>("Donor")
            .HasValue<NgoManager>("NgoManager");

        builder.Property(x => x.Email)
            .HasConversion(email => email.Value, value => Email.Create(value))
            .HasMaxLength(256)
            .IsRequired();

        builder.HasIndex(x => x.Email).IsUnique();

        builder.Property(x => x.PasswordHash).IsRequired();
        builder.Property(x => x.Role).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(x => x.CreatedAtUtc).IsRequired();
    }
}
