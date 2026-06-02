using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sreda.Backend.Domain.Entities;

namespace Sreda.Backend.Infrastructure.Persistence.Configurations;

public class DeviceStateConfiguration : IEntityTypeConfiguration<DeviceState>
{
    public void Configure(EntityTypeBuilder<DeviceState> builder)
    {
        builder.ToTable("DeviceStates");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.StateJson)
            .HasColumnType("jsonb");

        builder.Property(s => s.UpdatedAt)
            .HasDefaultValueSql("now()");

        builder.HasIndex(s => s.DeviceId)
            .IsUnique();
    }
}
