using Microsoft.AspNetCore.SignalR;

namespace Sreda.Backend.API.Hubs;

/// <summary>
/// SignalR-хаб журнала событий. Клиенты (дашборд/панель) подписываются сюда
/// и получают события через сообщение "ReceiveEvent".
/// </summary>
public class EventsHub : Hub
{
    public const string ReceiveEvent = "ReceiveEvent";
}
