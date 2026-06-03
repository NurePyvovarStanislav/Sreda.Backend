using Sreda.Backend.Domain.Entities;

namespace Sreda.Backend.Application.Common;

/// <summary>
/// Способ, которым распознанный текст был сопоставлен с командой.
/// </summary>
public enum MatchKind
{
    None = 0,
    Exact = 1,
    Contains = 2,
    Fuzzy = 3,
    Semantic = 4
}

/// <summary>
/// Результат работы резолвера команд: лучший кандидат, уверенность и как он найден.
/// Accepted = уверенность прошла порог приёма (иначе команда отклоняется).
/// </summary>
public record CommandResolution(
    bool Accepted,
    AvailableCommand? Command,
    IReadOnlyDictionary<string, object?> Arguments,
    double Confidence,
    MatchKind MatchKind,
    string? MatchedPhrase,
    string? MatchedLanguage)
{
    public static CommandResolution NotRecognized() => new(
        Accepted: false,
        Command: null,
        Arguments: EmptyArguments,
        Confidence: 0,
        MatchKind: MatchKind.None,
        MatchedPhrase: null,
        MatchedLanguage: null);

    private static readonly IReadOnlyDictionary<string, object?> EmptyArguments =
        new Dictionary<string, object?>();
}
