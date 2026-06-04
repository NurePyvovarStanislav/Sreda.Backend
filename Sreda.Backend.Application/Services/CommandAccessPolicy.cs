using Microsoft.EntityFrameworkCore;
using Sreda.Backend.Application.Common;
using Sreda.Backend.Application.Interfaces;
using Sreda.Backend.Domain.Entities;
using Sreda.Backend.Domain.Enums;

namespace Sreda.Backend.Application.Services;

/// <summary>
/// Політика доступу до команд. Пріоритет: персональний override (явно
/// дозволено/заборонено) важливіший за базовий рівень доступу.
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
            return AccessDecision.Deny("Користувача не визначено.");
        }

        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == userId.Value, cancellationToken);

        if (user is null)
        {
            return AccessDecision.Deny($"Користувача з Id {userId} не знайдено.");
        }

        if (!user.IsActive)
        {
            return AccessDecision.Deny("Обліковий запис користувача неактивний.");
        }

        var @override = await _dbContext.UserCommandOverrides
            .FirstOrDefaultAsync(
                o => o.UserId == user.Id && o.AvailableCommandId == command.Id,
                cancellationToken);

        if (@override is not null)
        {
            return @override.IsAllowed
                ? AccessDecision.Allow()
                : AccessDecision.Deny("Команду явно заборонено для користувача.");
        }

        return user.AccessLevel >= command.RequiredAccessLevel
            ? AccessDecision.Allow()
            : AccessDecision.Deny(
                $"Недостатній рівень доступу: потрібно {command.RequiredAccessLevel}, у користувача {user.AccessLevel}.");
    }
}
