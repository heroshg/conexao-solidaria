using Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity.Infrastructure.Persistence.Configurations;

public class NgoManagerConfiguration : IEntityTypeConfiguration<NgoManager>
{
    public void Configure(EntityTypeBuilder<NgoManager> builder)
    {
        builder.Property(x => x.FullName).HasMaxLength(200).IsRequired();
    }
}
