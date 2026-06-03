using Sreda.Backend.Domain.Enums;

namespace Sreda.Backend.Contracts.DTOs.Users;

public record UserDto(int Id, string Name, bool IsActive, AccessLevel AccessLevel, DateTime CreatedAt);

public record CreateUserRequest(string Name, AccessLevel AccessLevel, bool IsActive = true);

public record UpdateUserRequest(string Name, AccessLevel AccessLevel, bool IsActive);
