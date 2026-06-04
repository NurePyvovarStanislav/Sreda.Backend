using System.Text.RegularExpressions;

namespace Sreda.Backend.Infrastructure.Integration;

/// <summary>
/// Парсинг служебного вывода whisper-cli (stderr/stdout).
/// </summary>
internal static partial class WhisperOutputParser
{
    /// <summary>
    /// Пример: whisper_full: auto-detected language: ru (p = 0.98)
    /// </summary>
    [GeneratedRegex(
        @"auto-detected\s+language:\s*([a-z]{2,3})\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex AutoDetectedLanguageRegex();

    public static string? TryParseDetectedLanguage(string? standardOutput, string? standardError)
    {
        var combined = string.Concat(standardOutput, "\n", standardError);
        if (string.IsNullOrWhiteSpace(combined))
        {
            return null;
        }

        var match = AutoDetectedLanguageRegex().Match(combined);
        return match.Success ? match.Groups[1].Value : null;
    }
}
