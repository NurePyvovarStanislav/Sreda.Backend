namespace Sreda.Backend.Contracts.DTOs.Overrides;

public record UserCommandOverrideDto(
    int Id,
    int UserId,
    int AvailableCommandId,
    bool IsAllowed,
    DateTime CreatedAt);

public record CreateUserCommandOverrideRequest(
    int UserId,
    int AvailableCommandId,
    bool IsAllowed);

public record UpdateUserCommandOverrideRequest(bool IsAllowed);
