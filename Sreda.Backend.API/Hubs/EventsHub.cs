using Microsoft.AspNetCore.SignalR;

namespace Sreda.Backend.API.Hubs;

/// <summary>
/// SignalR-хаб журналу подій. Клієнти (дашборд) підписуються сюди
/// і отримують події через повідомлення "ReceiveEvent".
/// </summary>
public class EventsHub : Hub
{
    public const string ReceiveEvent = "ReceiveEvent";
}
