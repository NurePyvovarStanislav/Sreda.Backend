using Sreda.Backend.Application.Services;
using Sreda.Backend.Contracts.DTOs.Devices;
using Sreda.Backend.Domain.Entities;

namespace Sreda.Backend.Application.Features.Devices;

internal static class DeviceMapping
{
    public static DeviceLocalizationDto ToLocalizationDto(DeviceLocalizedLabel label) =>
        new(label.Language, label.Name, label.Location);

    public static DeviceDto ToDto(Device device) =>
        new(
            device.Id,
            device.Code,
            device.Name,
            device.Location,
            device.Localizations
                .OrderBy(l => l.Language)
                .Select(ToLocalizationDto)
                .ToList(),
            device.Type,
            device.IsActive,
            device.CreatedAt);

    public static void ApplyLocalizations(Device device, IReadOnlyList<DeviceLocalizationDto> localizations)
    {
        foreach (var dto in localizations)
        {
            var lang = ResponseLanguageResolver.Normalize(dto.Language);
            var existing = device.Localizations.FirstOrDefault(l =>
                string.Equals(l.Language, lang, StringComparison.OrdinalIgnoreCase));

            if (existing is null)
            {
                device.Localizations.Add(new DeviceLocalizedLabel
                {
                    Language = lang,
                    Name = dto.Name,
                    Location = dto.Location
                });
            }
            else
            {
                existing.Language = lang;
                existing.Name = dto.Name;
                existing.Location = dto.Location;
            }
        }

        DeviceLocalization.SyncDefaultDisplayFields(device);
    }
}
