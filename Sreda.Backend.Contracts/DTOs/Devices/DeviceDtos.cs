using Sreda.Backend.Domain.Enums;

namespace Sreda.Backend.Contracts.DTOs.Devices;

public record DeviceDto(
    int Id,
    string Code,
    string Name,
    DeviceType Type,
    string? Location,
    bool IsActive,
    DateTime CreatedAt);

public record CreateDeviceRequest(
    string Code,
    string Name,
    DeviceType Type,
    string? Location,
    bool IsActive = true);

public record UpdateDeviceRequest(
    string Name,
    DeviceType Type,
    string? Location,
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
