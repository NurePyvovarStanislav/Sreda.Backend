using Sreda.Backend.Application.Common;

namespace Sreda.Backend.Application.Services;

/// <summary>
/// Лексическая оценка похожести распознанного текста и фразы-образца.
/// Без внешних AI-моделей: точное совпадение, вхождение целой фразы и
/// нечёткое сравнение (коэффициент Дайса по токенам + нормированный Левенштейн).
/// Возвращает уверенность [0..1] и способ совпадения.
/// </summary>
internal static class PhraseSimilarity
{
    /// <summary>
    /// Оценивает похожесть. Оба аргумента должны быть уже нормализованы
    /// (нижний регистр, без пунктуации, схлопнутые пробелы).
    /// </summary>
    public static (double Score, MatchKind Kind) Evaluate(string normalizedText, string normalizedPhrase)
    {
        if (normalizedText.Length == 0 || normalizedPhrase.Length == 0)
        {
            return (0, MatchKind.None);
        }

        if (string.Equals(normalizedText, normalizedPhrase, StringComparison.Ordinal))
        {
            return (1.0, MatchKind.Exact);
        }

        var textTokens = normalizedText.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var phraseTokens = normalizedPhrase.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        // Фраза целиком встречается как непрерывная последовательность слов.
        if (ContainsContiguous(textTokens, phraseTokens))
        {
            var coverage = phraseTokens.Length / (double)textTokens.Length;
            return (0.8 + 0.2 * coverage, MatchKind.Contains);
        }

        var dice = DiceCoefficient(textTokens, phraseTokens);
        var levenshtein = LevenshteinRatio(normalizedText, normalizedPhrase);

        // Нечёткое совпадение держим ниже «вхождения», чтобы точные матчи всегда выигрывали.
        var fuzzy = Math.Max(dice, levenshtein) * 0.8;
        return (fuzzy, MatchKind.Fuzzy);
    }

    private static bool ContainsContiguous(string[] text, string[] phrase)
    {
        if (phrase.Length == 0 || phrase.Length > text.Length)
        {
            return false;
        }

        for (var start = 0; start <= text.Length - phrase.Length; start++)
        {
            var matched = true;
            for (var i = 0; i < phrase.Length; i++)
            {
                if (!string.Equals(text[start + i], phrase[i], StringComparison.Ordinal))
                {
                    matched = false;
                    break;
                }
            }

            if (matched)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Коэффициент Дайса по множествам токенов: 2|A∩B| / (|A|+|B|).</summary>
    private static double DiceCoefficient(string[] a, string[] b)
    {
        if (a.Length == 0 || b.Length == 0)
        {
            return 0;
        }

        var setA = new HashSet<string>(a, StringComparer.Ordinal);
        var setB = new HashSet<string>(b, StringComparer.Ordinal);

        var intersection = 0;
        foreach (var token in setA)
        {
            if (setB.Contains(token))
            {
                intersection++;
            }
        }

        return 2.0 * intersection / (setA.Count + setB.Count);
    }

    /// <summary>Нормированная похожесть по расстоянию Левенштейна: 1 - dist/maxLen.</summary>
    private static double LevenshteinRatio(string a, string b)
    {
        var distance = Levenshtein(a, b);
        var maxLen = Math.Max(a.Length, b.Length);
        return maxLen == 0 ? 1.0 : 1.0 - distance / (double)maxLen;
    }

    private static int Levenshtein(string a, string b)
    {
        var previous = new int[b.Length + 1];
        var current = new int[b.Length + 1];

        for (var j = 0; j <= b.Length; j++)
        {
            previous[j] = j;
        }

        for (var i = 1; i <= a.Length; i++)
        {
            current[0] = i;
            for (var j = 1; j <= b.Length; j++)
            {
                var cost = a[i - 1] == b[j - 1] ? 0 : 1;
                current[j] = Math.Min(
                    Math.Min(current[j - 1] + 1, previous[j] + 1),
                    previous[j - 1] + cost);
            }

            (previous, current) = (current, previous);
        }

        return previous[b.Length];
    }
}
