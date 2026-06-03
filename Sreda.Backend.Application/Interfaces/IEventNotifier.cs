using Sreda.Backend.Contracts.DTOs.History;

namespace Sreda.Backend.Application.Interfaces;

/// <summary>
/// Публикует события журнала подписчикам в реальном времени (например, через SignalR).
/// Абстракция в Application, чтобы бизнес-логика не зависела от транспорта.
/// </summary>
public interface IEventNotifier
{
    Task PublishAsync(EventDto @event, CancellationToken cancellationToken = default);
}
