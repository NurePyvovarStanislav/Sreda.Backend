using Sreda.Backend.Contracts.DTOs.History;
using Sreda.Backend.Domain.Enums;

namespace Sreda.Backend.Application.Interfaces;

/// <summary>
/// Единая точка записи событий в журнал (Event Log).
/// Сохраняет событие в БД и публикует его подписчикам в реальном времени.
/// Все части системы должны логировать события через этот сервис.
/// </summary>
public interface IEventLogger
{
    Task<EventDto> LogAsync(
        EventType type,
        object? payload = null,
        int? commandId = null,
        int? deviceId = null,
        CancellationToken cancellationToken = default);
}
