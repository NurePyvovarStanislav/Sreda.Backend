using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Sreda.Backend.Application.Common;
using Sreda.Backend.Application.Interfaces;
using Sreda.Backend.Domain.Entities;

namespace Sreda.Backend.Application.Services;

/// <summary>
/// Резолвер команд: распознанный текст -> нормализация -> поиск лучшей фразы
/// (точное / вхождение / нечёткое) -> при низкой уверенности семантический фоллбэк ->
/// извлечение аргументов -> оценка уверенности и решение о приёме.
/// </summary>
public class CommandResolverService : ICommandResolver
{
    private readonly ISredaDbContext _dbContext;
    private readonly ISemanticCommandMatcher _semanticMatcher;
    private readonly CommandResolverOptions _options;

    public CommandResolverService(
        ISredaDbContext dbContext,
        ISemanticCommandMatcher semanticMatcher,
        IOptions<CommandResolverOptions> options)
    {
        _dbContext = dbContext;
        _semanticMatcher = semanticMatcher;
        _options = options.Value;
    }

    public async Task<CommandResolution> ResolveAsync(string rawText, CancellationToken cancellationToken = default)
    {
        var text = TextNormalizer.Normalize(rawText);
        if (text.Length == 0)
        {
            return CommandResolution.NotRecognized();
        }

        var commands = await _dbContext.AvailableCommands
            .Where(c => c.IsEnabled)
            .Include(c => c.Phrases)
            .ToListAsync(cancellationToken);

        AvailableCommand? bestCommand = null;
        CommandPhrase? bestPhrase = null;
        var bestScore = 0.0;
        var bestKind = MatchKind.None;

        foreach (var command in commands)
        {
            foreach (var phrase in command.Phrases.Where(p => p.IsActive))
            {
                var normalizedPhrase = TextNormalizer.Normalize(phrase.Phrase);
                if (normalizedPhrase.Length == 0)
                {
                    continue;
                }

                var (score, kind) = PhraseSimilarity.Evaluate(text, normalizedPhrase);

                // При равенстве уверенности предпочитаем более длинную (специфичную) фразу.
                var isBetter = score > bestScore
                    || (Math.Abs(score - bestScore) < 0.0001
                        && normalizedPhrase.Length > (bestPhrase is null ? -1 : TextNormalizer.Normalize(bestPhrase.Phrase).Length));

                if (score > 0 && isBetter)
                {
                    bestScore = score;
                    bestKind = kind;
                    bestCommand = command;
                    bestPhrase = phrase;
                }
            }
        }

        // Семантический фоллбэк, если лексика не дотянула до порога приёма.
        if (bestScore < _options.MinConfidence)
        {
            var semantic = await _semanticMatcher.MatchAsync(text, commands, cancellationToken);
            if (semantic is not null && semantic.Confidence > bestScore)
            {
                var semanticCommand = commands.FirstOrDefault(c =>
                    string.Equals(c.Code, semantic.CommandCode, StringComparison.OrdinalIgnoreCase));

                if (semanticCommand is not null)
                {
                    bestCommand = semanticCommand;
                    bestPhrase = null;
                    bestScore = semantic.Confidence;
                    bestKind = MatchKind.Semantic;
                }
            }
        }

        if (bestCommand is null)
        {
            return CommandResolution.NotRecognized();
        }

        var arguments = ArgumentExtractor.Extract(text);
        var accepted = bestScore >= _options.MinConfidence;

        return new CommandResolution(
            Accepted: accepted,
            Command: bestCommand,
            Arguments: arguments,
            Confidence: Math.Round(bestScore, 4),
            MatchKind: bestKind,
            MatchedPhrase: bestPhrase?.Phrase,
            MatchedLanguage: bestPhrase?.Language);
    }
}
