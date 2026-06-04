namespace Sreda.Backend.Application.Services;

/// <summary>
/// Нормализация языка ответа (en / ru / uk) и выбор голоса Piper.
/// </summary>
public static class ResponseLanguageResolver
{
    public const string English = "en";
    public const string Russian = "ru";
    public const string Ukrainian = "uk";

    private static readonly HashSet<string> Supported = new(StringComparer.OrdinalIgnoreCase)
    {
        English, Russian, Ukrainian
    };

    /// <summary>Нормализует код Whisper/фразы; неизвестный → en.</summary>
    public static string Normalize(string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return English;
        }

        var c = code.Trim().ToLowerInvariant();
        return c switch
        {
            "en" => English,
            "ru" => Russian,
            "uk" or "ua" => Ukrainian,
            _ when Supported.Contains(c) => c,
            _ => English
        };
    }

    /// <summary>
    /// Язык для конвеєра: явный (Whisper) → язык совпавшей фразы → эвристика по тексту → en.
    /// </summary>
    public static string ResolveForPipeline(
        string? explicitLanguage,
        string? matchedPhraseLanguage,
        string rawText)
    {
        if (!string.IsNullOrWhiteSpace(explicitLanguage))
        {
            return Normalize(explicitLanguage);
        }

        if (!string.IsNullOrWhiteSpace(matchedPhraseLanguage))
        {
            return Normalize(matchedPhraseLanguage);
        }

        return InferFromText(rawText);
    }

    /// <summary>Кириллица без явного кода → ru; иначе en.</summary>
    public static string InferFromText(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return English;
        }

        return ContainsCyrillic(text) ? Russian : English;
    }

    /// <summary>
    /// Язык голоса Piper: uk → ru (нет отдельной модели), неподдерживаемый → en.
    /// </summary>
    public static string ResolveForTts(string responseLanguage, string? queryOverride = null)
    {
        if (!string.IsNullOrWhiteSpace(queryOverride))
        {
            return MapUkToRuForTts(Normalize(queryOverride));
        }

        return MapUkToRuForTts(Normalize(responseLanguage));
    }

    private static string MapUkToRuForTts(string lang) =>
        lang == Ukrainian ? Russian : lang;

    private static bool ContainsCyrillic(string text)
    {
        foreach (var ch in text)
        {
            if (ch is >= '\u0400' and <= '\u04FF')
            {
                return true;
            }
        }

        return false;
    }
}
