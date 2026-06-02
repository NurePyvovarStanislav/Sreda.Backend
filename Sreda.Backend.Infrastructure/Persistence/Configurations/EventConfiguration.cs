using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sreda.Backend.Domain.Entities;

namespace Sreda.Backend.Infrastructure.Persistence.Configurations;

public class EventConfiguration : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        builder.ToTable("Events");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Type)
            .HasConversion<int>();

        builder.Property(e => e.PayloadJson)
            .HasColumnType("jsonb");

        builder.Property(e => e.CreatedAt)
            .HasDefaultValueSql("now()");

        builder.HasIndex(e => e.Type);
        builder.HasIndex(e => e.CreatedAt);
    }
}
