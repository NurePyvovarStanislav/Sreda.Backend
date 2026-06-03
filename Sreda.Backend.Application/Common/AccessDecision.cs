namespace Sreda.Backend.Application.Common;

/// <summary>
/// Решение о доступе пользователя к выполнению команды.
/// </summary>
public record AccessDecision(bool IsAllowed, string? Reason)
{
    public static AccessDecision Allow() => new(true, null);

    public static AccessDecision Deny(string reason) => new(false, reason);
}
