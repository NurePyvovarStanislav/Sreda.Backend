using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sreda.Backend.Domain.Entities;

namespace Sreda.Backend.Infrastructure.Persistence.Configurations;

public class DeviceConfiguration : IEntityTypeConfiguration<Device>
{
    public void Configure(EntityTypeBuilder<Device> builder)
    {
        builder.ToTable("Devices");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.Code)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(d => d.Name)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(d => d.Type)
            .HasConversion<int>();

        builder.Property(d => d.Location)
            .HasMaxLength(256);

        builder.Property(d => d.CreatedAt)
            .HasDefaultValueSql("now()");

        builder.HasIndex(d => d.Code)
            .IsUnique();

        builder.HasOne(d => d.State)
            .WithOne(s => s.Device)
            .HasForeignKey<DeviceState>(s => s.DeviceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(d => d.Events)
            .WithOne(e => e.Device)
            .HasForeignKey(e => e.DeviceId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
