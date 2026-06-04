using Sreda.Backend.Domain.Entities;

namespace Sreda.Backend.Application.Services;

/// <summary>
/// Локализованные подписи устройства для ответов и TTS (таблица DeviceLocalizedLabels).
/// </summary>
public static class DeviceLocalization
{
    public static string GetName(Device device, string language)
    {
        var label = FindLabel(device, language);
        return label?.Name ?? device.Name;
    }

    public static string? GetLocation(Device device, string language)
    {
        var label = FindLabel(device, language);
        return label?.Location ?? device.Location;
    }

    public static void SyncDefaultDisplayFields(Device device)
    {
        var uk = FindLabel(device, ResponseLanguageResolver.Ukrainian);
        if (uk is null)
        {
            return;
        }

        device.Name = uk.Name;
        device.Location = uk.Location;
    }

    private static DeviceLocalizedLabel? FindLabel(Device device, string language)
    {
        var lang = ResponseLanguageResolver.Normalize(language);
        var label = device.Localizations.FirstOrDefault(l =>
            string.Equals(l.Language, lang, StringComparison.OrdinalIgnoreCase));

        if (label is not null)
        {
            return label;
        }

        return device.Localizations.FirstOrDefault(l =>
            string.Equals(l.Language, ResponseLanguageResolver.Ukrainian, StringComparison.OrdinalIgnoreCase));
    }
}
