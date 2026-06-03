namespace Sreda.Backend.Application.Interfaces;

/// <summary>
/// Шлюз управления устройствами (Device Gateway).
/// Абстрагирует транспорт (MQTT/ESP32 и т.п.) от бизнес-логики:
/// слой Application отправляет команды по коду устройства, не зная про брокер.
/// </summary>
public interface IDeviceGateway
{
    /// <summary>Установлено ли соединение с брокером в данный момент.</summary>
    bool IsConnected { get; }

    /// <summary>
    /// Публикует команду конкретному устройству (по его уникальному Code).
    /// Payload сериализуется в JSON.
    /// </summary>
    Task SendCommandAsync(string deviceCode, object payload, CancellationToken cancellationToken = default);
}
