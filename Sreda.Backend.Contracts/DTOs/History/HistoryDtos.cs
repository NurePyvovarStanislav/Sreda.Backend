using Sreda.Backend.Domain.Enums;

namespace Sreda.Backend.Contracts.DTOs.History;

public record CommandHistoryDto(
    int Id,
    int? UserId,
    string RawText,
    int? AvailableCommandId,
    string? ArgumentsJson,
    CommandStatus Status,
    DateTime CreatedAt);

public record EventDto(
    int Id,
    int? CommandId,
    int? DeviceId,
    EventType Type,
    string? PayloadJson,
    DateTime CreatedAt);
