using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sreda.Backend.Domain.Entities;

namespace Sreda.Backend.Infrastructure.Persistence.Configurations;

public class DeviceLocalizedLabelConfiguration : IEntityTypeConfiguration<DeviceLocalizedLabel>
{
    public void Configure(EntityTypeBuilder<DeviceLocalizedLabel> builder)
    {
        builder.ToTable("DeviceLocalizedLabels");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Language)
            .IsRequired()
            .HasMaxLength(8);

        builder.Property(l => l.Name)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(l => l.Location)
            .HasMaxLength(256);

        builder.HasIndex(l => new { l.DeviceId, l.Language })
            .IsUnique();
    }
}
