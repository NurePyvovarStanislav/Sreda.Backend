namespace Sreda.Backend.Infrastructure.Integration.Mqtt;

/// <summary>
/// Соглашения по именованию MQTT-топиков для устройств Sreda.
/// Структура: {prefix}/{deviceCode}/{channel}.
/// </summary>
internal static class MqttTopics
{
    public const string StateChannel = "state";
    public const string HeartbeatChannel = "heartbeat";
    public const string CommandChannel = "command";

    public static string State(string prefix, string deviceCode) => $"{prefix}/{deviceCode}/{StateChannel}";

    public static string Heartbeat(string prefix, string deviceCode) => $"{prefix}/{deviceCode}/{HeartbeatChannel}";

    public static string Command(string prefix, string deviceCode) => $"{prefix}/{deviceCode}/{CommandChannel}";

    /// <summary>Фильтр подписки на все входящие state-топики: {prefix}/+/state.</summary>
    public static string StateFilter(string prefix) => $"{prefix}/+/{StateChannel}";

    /// <summary>Фильтр подписки на все входящие heartbeat-топики: {prefix}/+/heartbeat.</summary>
    public static string HeartbeatFilter(string prefix) => $"{prefix}/+/{HeartbeatChannel}";

    /// <summary>
    /// Извлекает (deviceCode, channel) из топика вида {prefix}/{deviceCode}/{channel}.
    /// Возвращает null, если топик не соответствует префиксу.
    /// </summary>
    public static (string DeviceCode, string Channel)? Parse(string prefix, string topic)
    {
        if (!topic.StartsWith(prefix + "/", StringComparison.Ordinal))
        {
            return null;
        }

        var rest = topic[(prefix.Length + 1)..];
        var parts = rest.Split('/');
        if (parts.Length != 2 || string.IsNullOrWhiteSpace(parts[0]) || string.IsNullOrWhiteSpace(parts[1]))
        {
            return null;
        }

        return (parts[0], parts[1]);
    }
}
