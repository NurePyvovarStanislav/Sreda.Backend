using Sreda.Backend.Domain.Enums;

namespace Sreda.Backend.Contracts.DTOs.Devices;

public record DeviceLocalizationDto(
    string Language,
    string Name,
    string? Location);

public record DeviceDto(
    int Id,
    string Code,
    string Name,
    string? Location,
    IReadOnlyList<DeviceLocalizationDto> Localizations,
    DeviceType Type,
    bool IsActive,
    DateTime CreatedAt);

public record CreateDeviceRequest(
    string Code,
    IReadOnlyList<DeviceLocalizationDto> Localizations,
    DeviceType Type,
    bool IsActive = true);

public record UpdateDeviceRequest(
    IReadOnlyList<DeviceLocalizationDto> Localizations,
    DeviceType Type,
    bool IsActive);

public record DeviceStateDto(
    int DeviceId,
    bool IsOnline,
    DateTime? LastSeenAt,
    string? StateJson,
    DateTime UpdatedAt);

public record UpdateDeviceStateRequest(
    bool IsOnline,
    string? StateJson);

public record SendDeviceCommandRequest(
    string Command,
    Dictionary<string, object?>? Parameters);
