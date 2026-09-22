using Identity.Domain.Entities;
using Identity.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity.Infrastructure.Persistence.Configurations;

public class DonorConfiguration : IEntityTypeConfiguration<Donor>
{
    public void Configure(EntityTypeBuilder<Donor> builder)
    {
        builder.Property(x => x.FullName).HasMaxLength(200).IsRequired();

        builder.Property(x => x.Cpf)
            .HasConversion(cpf => cpf.Value, value => Cpf.Create(value))
            .HasColumnName("Cpf")
            .HasMaxLength(11)
            .IsRequired();

        builder.HasIndex(x => x.Cpf).IsUnique();
    }
}
