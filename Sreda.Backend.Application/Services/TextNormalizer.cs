using System.Text;

namespace Sreda.Backend.Application.Services;

/// <summary>
/// Приводит текст к виду для сравнения: нижний регистр, без пунктуации,
/// со схлопнутыми пробелами.
/// </summary>
internal static class TextNormalizer
{
    public static string Normalize(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return string.Empty;
        }

        var lower = input.Trim().ToLowerInvariant();
        var builder = new StringBuilder(lower.Length);

        foreach (var ch in lower)
        {
            builder.Append(char.IsLetterOrDigit(ch) ? ch : ' ');
        }

        return string.Join(' ', builder.ToString().Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }
}
