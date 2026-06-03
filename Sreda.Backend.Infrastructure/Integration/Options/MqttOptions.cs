namespace Sreda.Backend.Infrastructure.Integration.Options;

/// <summary>
/// Настройки подключения к MQTT-брокеру (Device Gateway / IoT).
/// Если Enabled = false, шлюз не подключается — приложение работает без брокера.
/// </summary>
public class MqttOptions
{
    public const string SectionName = "Mqtt";

    /// <summary>Включён ли MQTT-шлюз. Без брокера держать false.</summary>
    public bool Enabled { get; set; } = false;

    public string Host { get; set; } = "localhost";

    public int Port { get; set; } = 1883;

    public string? Username { get; set; }

    public string? Password { get; set; }

    /// <summary>Идентификатор клиента для брокера.</summary>
    public string ClientId { get; set; } = "sreda-backend";

    /// <summary>
    /// Префикс топиков устройств. Итоговые топики:
    /// {Prefix}/{deviceCode}/state, /heartbeat (входящие), /command (исходящие).
    /// </summary>
    public string TopicPrefix { get; set; } = "sreda/devices";

    /// <summary>Пауза перед переподключением при разрыве, секунды.</summary>
    public int ReconnectDelaySeconds { get; set; } = 5;

    /// <summary>
    /// Сколько секунд устройство считается онлайн без heartbeat/state.
    /// По истечении сторож помечает устройство оффлайн.
    /// </summary>
    public int HeartbeatTimeoutSeconds { get; set; } = 60;

    /// <summary>Период проверки оффлайн-устройств сторожем, секунды.</summary>
    public int OfflineCheckIntervalSeconds { get; set; } = 30;
}
