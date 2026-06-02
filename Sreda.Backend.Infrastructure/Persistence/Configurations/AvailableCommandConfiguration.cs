using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sreda.Backend.Domain.Entities;

namespace Sreda.Backend.Infrastructure.Persistence.Configurations;

public class AvailableCommandConfiguration : IEntityTypeConfiguration<AvailableCommand>
{
    public void Configure(EntityTypeBuilder<AvailableCommand> builder)
    {
        builder.ToTable("AvailableCommands");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Code)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(c => c.Description)
            .HasMaxLength(1024);

        builder.Property(c => c.Category)
            .HasConversion<int>();

        builder.Property(c => c.RequiredAccessLevel)
            .HasConversion<int>();

        builder.Property(c => c.CreatedAt)
            .HasDefaultValueSql("now()");

        builder.HasIndex(c => c.Code)
            .IsUnique();

        builder.HasMany(c => c.Phrases)
            .WithOne(p => p.AvailableCommand)
            .HasForeignKey(p => p.AvailableCommandId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.UserOverrides)
            .WithOne(o => o.AvailableCommand)
            .HasForeignKey(o => o.AvailableCommandId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Commands)
            .WithOne(cmd => cmd.AvailableCommand)
            .HasForeignKey(cmd => cmd.AvailableCommandId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
