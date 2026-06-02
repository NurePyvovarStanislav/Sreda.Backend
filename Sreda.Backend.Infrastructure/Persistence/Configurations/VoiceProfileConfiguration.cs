using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sreda.Backend.Domain.Entities;

namespace Sreda.Backend.Infrastructure.Persistence.Configurations;

public class VoiceProfileConfiguration : IEntityTypeConfiguration<VoiceProfile>
{
    public void Configure(EntityTypeBuilder<VoiceProfile> builder)
    {
        builder.ToTable("VoiceProfiles");

        builder.HasKey(vp => vp.Id);

        builder.Property(vp => vp.EmbeddingPath)
            .IsRequired()
            .HasMaxLength(512);

        builder.Property(vp => vp.CreatedAt)
            .HasDefaultValueSql("now()");

        builder.HasIndex(vp => vp.UserId)
            .IsUnique();
    }
}
