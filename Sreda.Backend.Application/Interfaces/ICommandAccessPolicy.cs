using Sreda.Backend.Application.Common;
using Sreda.Backend.Domain.Entities;

namespace Sreda.Backend.Application.Interfaces;

/// <summary>
/// Проверяет, имеет ли пользователь право выполнить команду.
/// Учитывает базовый RequiredAccessLevel команды и персональные
/// UserCommandOverrides (явное разрешение/запрет).
/// </summary>
public interface ICommandAccessPolicy
{
    Task<AccessDecision> EvaluateAsync(
        int? userId,
        AvailableCommand command,
        CancellationToken cancellationToken = default);
}
