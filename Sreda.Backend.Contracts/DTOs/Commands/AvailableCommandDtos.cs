using Sreda.Backend.Domain.Enums;

namespace Sreda.Backend.Contracts.DTOs.Commands;

public record AvailableCommandDto(
    int Id,
    string Code,
    string Name,
    string? Description,
    CommandCategory Category,
    bool RequiresDevice,
    AccessLevel RequiredAccessLevel,
    bool IsEnabled,
    DateTime CreatedAt);

public record CreateAvailableCommandRequest(
    string Code,
    string Name,
    string? Description,
    CommandCategory Category,
    bool RequiresDevice,
    AccessLevel RequiredAccessLevel,
    bool IsEnabled = true);

public record UpdateAvailableCommandRequest(
    string Name,
    string? Description,
    CommandCategory Category,
    bool RequiresDevice,
    AccessLevel RequiredAccessLevel,
    bool IsEnabled);
