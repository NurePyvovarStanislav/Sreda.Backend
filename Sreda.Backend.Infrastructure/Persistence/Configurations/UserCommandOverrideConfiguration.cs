using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sreda.Backend.Domain.Entities;

namespace Sreda.Backend.Infrastructure.Persistence.Configurations;

public class UserCommandOverrideConfiguration : IEntityTypeConfiguration<UserCommandOverride>
{
    public void Configure(EntityTypeBuilder<UserCommandOverride> builder)
    {
        builder.ToTable("UserCommandOverrides");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.CreatedAt)
            .HasDefaultValueSql("now()");

        builder.HasIndex(o => new { o.UserId, o.AvailableCommandId })
            .IsUnique();
    }
}
