using System.Text.RegularExpressions;

namespace Sreda.Backend.Application.Services;

/// <summary>
/// Извлекает простые аргументы из распознанного текста.
/// Пока — первое целое число (например, "на 80 процентов" -> value=80).
/// </summary>
internal static partial class ArgumentExtractor
{
    [GeneratedRegex(@"\d+")]
    private static partial Regex NumberRegex();

    public static IReadOnlyDictionary<string, object?> Extract(string normalizedText)
    {
        var arguments = new Dictionary<string, object?>();

        var match = NumberRegex().Match(normalizedText);
        if (match.Success && int.TryParse(match.Value, out var value))
        {
            arguments["value"] = value;
        }

        return arguments;
    }
}
