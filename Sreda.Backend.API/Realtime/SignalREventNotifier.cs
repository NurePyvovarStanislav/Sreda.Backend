using Microsoft.AspNetCore.SignalR;
using Sreda.Backend.API.Hubs;
using Sreda.Backend.Application.Interfaces;
using Sreda.Backend.Contracts.DTOs.History;

namespace Sreda.Backend.API.Realtime;

/// <summary>
/// Реализация публикации событий через SignalR. Отправляет EventDto всем
/// подключённым клиентам. Ошибки доставки не должны ломать бизнес-операцию.
/// </summary>
public class SignalREventNotifier : IEventNotifier
{
    private readonly IHubContext<EventsHub> _hubContext;
    private readonly ILogger<SignalREventNotifier> _logger;

    public SignalREventNotifier(IHubContext<EventsHub> hubContext, ILogger<SignalREventNotifier> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task PublishAsync(EventDto @event, CancellationToken cancellationToken = default)
    {
        try
        {
            await _hubContext.Clients.All.SendAsync(EventsHub.ReceiveEvent, @event, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Не вдалося опублікувати подію {EventId} через SignalR.", @event.Id);
        }
    }
}
