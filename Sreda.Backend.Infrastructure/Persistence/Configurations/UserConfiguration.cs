using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sreda.Backend.Domain.Entities;

namespace Sreda.Backend.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Name)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(u => u.AccessLevel)
            .HasConversion<int>();

        builder.Property(u => u.CreatedAt)
            .HasDefaultValueSql("now()");

        builder.HasOne(u => u.VoiceProfile)
            .WithOne(vp => vp.User)
            .HasForeignKey<VoiceProfile>(vp => vp.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.CommandOverrides)
            .WithOne(o => o.User)
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.Commands)
            .WithOne(c => c.User)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
