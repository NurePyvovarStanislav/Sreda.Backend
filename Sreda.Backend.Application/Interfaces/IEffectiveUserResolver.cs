namespace Sreda.Backend.Application.Interfaces;

/// <summary>
/// Визначає Id користувача для обробки команди.
/// Якщо в запиті userId не передано — використовується системний адміністратор із БД.
/// </summary>
public interface IEffectiveUserResolver
{
    Task<int> ResolveUserIdAsync(int? requestedUserId, CancellationToken cancellationToken = default);
}
