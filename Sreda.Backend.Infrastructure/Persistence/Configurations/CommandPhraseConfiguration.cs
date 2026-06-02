using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sreda.Backend.Domain.Entities;

namespace Sreda.Backend.Infrastructure.Persistence.Configurations;

public class CommandPhraseConfiguration : IEntityTypeConfiguration<CommandPhrase>
{
    public void Configure(EntityTypeBuilder<CommandPhrase> builder)
    {
        builder.ToTable("CommandPhrases");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Phrase)
            .IsRequired()
            .HasMaxLength(512);

        builder.Property(p => p.Language)
            .IsRequired()
            .HasMaxLength(8);

        builder.HasIndex(p => p.AvailableCommandId);
    }
}
