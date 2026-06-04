namespace Sreda.Backend.Application.Common;

/// <summary>
/// Знімок пристрою після виконання команди (для відповіді API та подій).
/// </summary>
public record AffectedDeviceSnapshot(
    int DeviceId,
    string Code,
    string Name,
    string? Location,
    bool IsOnline,
    string? StateJson);
