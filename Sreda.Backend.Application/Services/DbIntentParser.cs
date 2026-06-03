using Microsoft.EntityFrameworkCore;
using Sreda.Backend.Application.Common;
using Sreda.Backend.Application.Interfaces;

namespace Sreda.Backend.Application.Services;

/// <summary>
/// Сопоставляет распознанный текст с командами из БД по их голосовым фразам.
/// Выбирает наиболее специфичное совпадение (по самой длинной фразе).
/// </summary>
public class DbIntentParser : IIntentParser
{
    private readonly ISredaDbContext _dbContext;

    public DbIntentParser(ISredaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IntentMatch?> ParseAsync(string text, CancellationToken cancellationToken = default)
    {
        var normalizedText = TextNormalizer.Normalize(text);
        if (normalizedText.Length == 0)
        {
            return null;
        }

        var commands = await _dbContext.AvailableCommands
            .Where(c => c.IsEnabled)
            .Include(c => c.Phrases)
            .ToListAsync(cancellationToken);

        var paddedText = $" {normalizedText} ";
        Domain.Entities.AvailableCommand? bestCommand = null;
        var bestPhraseLength = -1;

        foreach (var command in commands)
        {
            foreach (var phrase in command.Phrases.Where(p => p.IsActive))
            {
                var normalizedPhrase = TextNormalizer.Normalize(phrase.Phrase);
                if (normalizedPhrase.Length == 0)
                {
                    continue;
                }

                var isMatch = normalizedText == normalizedPhrase
                    || paddedText.Contains($" {normalizedPhrase} ", StringComparison.Ordinal);

                if (isMatch && normalizedPhrase.Length > bestPhraseLength)
                {
                    bestCommand = command;
                    bestPhraseLength = normalizedPhrase.Length;
                }
            }
        }

        if (bestCommand is null)
        {
            return null;
        }

        var arguments = ArgumentExtractor.Extract(normalizedText);
        return new IntentMatch(bestCommand, arguments);
    }
}
