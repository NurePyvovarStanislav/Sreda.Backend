using Microsoft.EntityFrameworkCore;
using Sreda.Backend.Application.Common;
using Sreda.Backend.Application.Interfaces;
using Sreda.Backend.Domain.Entities;
using Sreda.Backend.Domain.Enums;

namespace Sreda.Backend.Application.Services;

/// <summary>
/// Политика доступа к командам. Приоритет: персональный override (явный
/// разрешён/запрещён) важнее базового уровня доступа. Без пользователя
/// действует уровень Guest.
/// </summary>
public class CommandAccessPolicy : ICommandAccessPolicy
{
    private readonly ISredaDbContext _dbContext;

    public CommandAccessPolicy(ISredaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AccessDecision> EvaluateAsync(
        int? userId,
        AvailableCommand command,
        CancellationToken cancellationToken = default)
    {
        if (userId is null)
        {
            // Анонимный вызов — права гостя.
            return command.RequiredAccessLevel <= AccessLevel.Guest
                ? AccessDecision.Allow()
                : AccessDecision.Deny("Команда недоступна без идентификации пользователя.");
        }

        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == userId.Value, cancellationToken);

        if (user is null)
        {
            return AccessDecision.Deny($"Пользователь с Id {userId} не найден.");
        }

        if (!user.IsActive)
        {
            return AccessDecision.Deny("Учётная запись пользователя неактивна.");
        }

        // Персональное переопределение имеет приоритет над уровнем доступа.
        var @override = await _dbContext.UserCommandOverrides
            .FirstOrDefaultAsync(
                o => o.UserId == user.Id && o.AvailableCommandId == command.Id,
                cancellationToken);

        if (@override is not null)
        {
            return @override.IsAllowed
                ? AccessDecision.Allow()
                : AccessDecision.Deny("Команда явно запрещена для пользователя.");
        }

        return user.AccessLevel >= command.RequiredAccessLevel
            ? AccessDecision.Allow()
            : AccessDecision.Deny(
                $"Недостаточный уровень доступа: требуется {command.RequiredAccessLevel}, у пользователя {user.AccessLevel}.");
    }
}
