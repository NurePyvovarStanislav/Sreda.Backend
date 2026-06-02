using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sreda.Backend.Domain.Entities;

namespace Sreda.Backend.Infrastructure.Persistence.Configurations;

public class CommandConfiguration : IEntityTypeConfiguration<Command>
{
    public void Configure(EntityTypeBuilder<Command> builder)
    {
        builder.ToTable("Commands");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.RawText)
            .IsRequired()
            .HasMaxLength(1024);

        builder.Property(c => c.ArgumentsJson)
            .HasColumnType("jsonb");

        builder.Property(c => c.Status)
            .HasConversion<int>();

        builder.Property(c => c.CreatedAt)
            .HasDefaultValueSql("now()");

        builder.HasMany(c => c.Events)
            .WithOne(e => e.Command)
            .HasForeignKey(e => e.CommandId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
