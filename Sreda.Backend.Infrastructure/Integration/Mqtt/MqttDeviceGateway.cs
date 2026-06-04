using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Protocol;
using Sreda.Backend.Application.Interfaces;
using Sreda.Backend.Domain.Entities;
using Sreda.Backend.Domain.Enums;
using Sreda.Backend.Infrastructure.Integration.Options;

namespace Sreda.Backend.Infrastructure.Integration.Mqtt;

/// <summary>
/// MQTT-реализация шлюза устройств. Держит подключение к брокеру (singleton),
/// принимает телеметрию/heartbeat от устройств (ESP32) и публикует им команды.
/// Входящие сообщения обновляют DeviceState и пишутся в журнал через IEventLogger.
/// Сторож периодически помечает «молчащие» устройства как оффлайн.
/// </summary>
public sealed class MqttDeviceGateway : IDeviceGateway, IAsyncDisposable
{
    private readonly MqttOptions _options;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<MqttDeviceGateway> _logger;

    private IMqttClient? _client;
    private CancellationTokenSource? _cts;
    private Task? _connectionLoop;
    private Task? _watchdogLoop;

    public MqttDeviceGateway(
        IOptions<MqttOptions> options,
        IServiceScopeFactory scopeFactory,
        ILogger<MqttDeviceGateway> logger)
    {
        _options = options.Value;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public bool IsConnected => _client?.IsConnected ?? false;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation("MQTT-шлюз вимкнено (Mqtt:Enabled = false) — підключення пропущено.");
            return;
        }

        _cts = new CancellationTokenSource();

        var factory = new MqttClientFactory();
        _client = factory.CreateMqttClient();
        _client.ApplicationMessageReceivedAsync += OnMessageReceivedAsync;
        _client.ConnectedAsync += OnConnectedAsync;
        _client.DisconnectedAsync += OnDisconnectedAsync;

        _connectionLoop = Task.Run(() => MaintainConnectionAsync(_cts.Token), CancellationToken.None);
        _watchdogLoop = Task.Run(() => RunOfflineWatchdogAsync(_cts.Token), CancellationToken.None);

        _logger.LogInformation(
            "MQTT-шлюз запущено. Брокер {Host}:{Port}, префікс топіків '{Prefix}'.",
            _options.Host, _options.Port, _options.TopicPrefix);

        await Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_cts is null)
        {
            return;
        }

        await _cts.CancelAsync();

        try
        {
            if (_connectionLoop is not null)
            {
                await _connectionLoop;
            }

            if (_watchdogLoop is not null)
            {
                await _watchdogLoop;
            }
        }
        catch (OperationCanceledException)
        {
            // Ожидаемо при остановке.
        }

        if (_client is { IsConnected: true })
        {
            try
            {
                await _client.DisconnectAsync(cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Помилка під час відключення MQTT-клієнта.");
            }
        }
    }

    public async Task SendCommandAsync(string deviceCode, object payload, CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
        {
            throw new InvalidOperationException("MQTT-шлюз вимкнено (Mqtt:Enabled = false).");
        }

        if (_client is null || !_client.IsConnected)
        {
            throw new InvalidOperationException("Немає з'єднання з MQTT-брокером — команду не надіслано.");
        }

        var json = JsonSerializer.Serialize(payload);
        var topic = MqttTopics.Command(_options.TopicPrefix, deviceCode);

        var message = new MqttApplicationMessageBuilder()
            .WithTopic(topic)
            .WithPayload(json)
            .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
            .Build();

        await _client.PublishAsync(message, cancellationToken);

        _logger.LogInformation("Команду надіслано пристрою {DeviceCode} у топік {Topic}.", deviceCode, topic);
    }

    private async Task MaintainConnectionAsync(CancellationToken cancellationToken)
    {
        var delay = TimeSpan.FromSeconds(Math.Max(1, _options.ReconnectDelaySeconds));

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                if (_client is not null && !_client.IsConnected)
                {
                    var clientOptions = new MqttClientOptionsBuilder()
                        .WithTcpServer(_options.Host, _options.Port)
                        .WithClientId(_options.ClientId)
                        .WithCleanSession()
                        .WithCredentials(_options.Username, _options.Password)
                        .Build();

                    await _client.ConnectAsync(clientOptions, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex, "Не вдалося підключитися до MQTT-брокера {Host}:{Port}. Повтор через {Delay} с.",
                    _options.Host, _options.Port, delay.TotalSeconds);
            }

            try
            {
                await Task.Delay(delay, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private async Task OnConnectedAsync(MqttClientConnectedEventArgs args)
    {
        try
        {
            var factory = new MqttClientFactory();
            var subscribeOptions = factory.CreateSubscribeOptionsBuilder()
                .WithTopicFilter(f => f.WithTopic(MqttTopics.StateFilter(_options.TopicPrefix)))
                .WithTopicFilter(f => f.WithTopic(MqttTopics.HeartbeatFilter(_options.TopicPrefix)))
                .Build();

            await _client!.SubscribeAsync(subscribeOptions);

            _logger.LogInformation("Підключено до MQTT-брокера, підписку на топіки пристроїв оформлено.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка підписки на MQTT-топіки після підключення.");
        }
    }

    private Task OnDisconnectedAsync(MqttClientDisconnectedEventArgs args)
    {
        _logger.LogWarning("З'єднання з MQTT-брокером втрачено ({Reason}). Буде перепідключення.", args.Reason);
        return Task.CompletedTask;
    }

    private async Task OnMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs args)
    {
        var topic = args.ApplicationMessage.Topic;
        var parsed = MqttTopics.Parse(_options.TopicPrefix, topic);
        if (parsed is null)
        {
            return;
        }

        var (deviceCode, channel) = parsed.Value;
        var payload = args.ApplicationMessage.ConvertPayloadToString() ?? string.Empty;
        var cancellationToken = _cts?.Token ?? CancellationToken.None;

        try
        {
            switch (channel)
            {
                case MqttTopics.StateChannel:
                    await HandleInboundAsync(deviceCode, payload, isStateUpdate: true, cancellationToken);
                    break;
                case MqttTopics.HeartbeatChannel:
                    await HandleInboundAsync(deviceCode, payload, isStateUpdate: false, cancellationToken);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка обробки MQTT-повідомлення з топіка {Topic}.", topic);
        }
    }

    /// <summary>
    /// Обновляет состояние устройства по входящему сообщению и пишет событие в журнал.
    /// state — обновляет StateJson и онлайн; heartbeat — только онлайн/LastSeenAt.
    /// </summary>
    private async Task HandleInboundAsync(string deviceCode, string payload, bool isStateUpdate, CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ISredaDbContext>();
        var eventLogger = scope.ServiceProvider.GetRequiredService<IEventLogger>();

        var device = await dbContext.Devices
            .FirstOrDefaultAsync(d => d.Code == deviceCode, cancellationToken);

        if (device is null)
        {
            _logger.LogWarning("Отримано повідомлення від невідомого пристрою '{DeviceCode}'.", deviceCode);
            return;
        }

        var now = DateTime.UtcNow;
        var state = await dbContext.DeviceStates
            .FirstOrDefaultAsync(s => s.DeviceId == device.Id, cancellationToken);

        if (state is null)
        {
            state = new DeviceState
            {
                DeviceId = device.Id,
                IsOnline = true,
                LastSeenAt = now,
                StateJson = isStateUpdate ? NormalizePayload(payload) : null,
                UpdatedAt = now
            };
            dbContext.DeviceStates.Add(state);
        }
        else
        {
            state.IsOnline = true;
            state.LastSeenAt = now;
            state.UpdatedAt = now;
            if (isStateUpdate)
            {
                state.StateJson = NormalizePayload(payload);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        var eventType = isStateUpdate ? EventType.DeviceUpdated : EventType.DeviceHeartbeat;
        await eventLogger.LogAsync(
            eventType,
            new { deviceCode, payload },
            deviceId: device.Id,
            cancellationToken: cancellationToken);
    }

    private async Task RunOfflineWatchdogAsync(CancellationToken cancellationToken)
    {
        var interval = TimeSpan.FromSeconds(Math.Max(5, _options.OfflineCheckIntervalSeconds));

        try
        {
            using var timer = new PeriodicTimer(interval);
            while (await timer.WaitForNextTickAsync(cancellationToken))
            {
                try
                {
                    await MarkStaleDevicesOfflineAsync(cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Помилка в сторожі офлайн-пристроїв.");
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Ожидаемо при остановке.
        }
    }

    private async Task MarkStaleDevicesOfflineAsync(CancellationToken cancellationToken)
    {
        var threshold = DateTime.UtcNow - TimeSpan.FromSeconds(Math.Max(1, _options.HeartbeatTimeoutSeconds));

        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ISredaDbContext>();
        var eventLogger = scope.ServiceProvider.GetRequiredService<IEventLogger>();

        var stale = await dbContext.DeviceStates
            .Where(s => s.IsOnline && (s.LastSeenAt == null || s.LastSeenAt < threshold))
            .ToListAsync(cancellationToken);

        if (stale.Count == 0)
        {
            return;
        }

        var now = DateTime.UtcNow;
        foreach (var state in stale)
        {
            state.IsOnline = false;
            state.UpdatedAt = now;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        foreach (var state in stale)
        {
            await eventLogger.LogAsync(
                EventType.DeviceOffline,
                new { reason = "heartbeat_timeout", lastSeenAt = state.LastSeenAt },
                deviceId: state.DeviceId,
                cancellationToken: cancellationToken);
        }
    }

    /// <summary>Если payload — валидный JSON, сохраняем как есть; иначе оборачиваем в объект.</summary>
    private static string NormalizePayload(string payload)
    {
        if (string.IsNullOrWhiteSpace(payload))
        {
            return "{}";
        }

        try
        {
            using var _ = JsonDocument.Parse(payload);
            return payload;
        }
        catch (JsonException)
        {
            return JsonSerializer.Serialize(new { raw = payload });
        }
    }

    public async ValueTask DisposeAsync()
    {
        _cts?.Dispose();
        if (_client is not null)
        {
            _client.Dispose();
        }

        await ValueTask.CompletedTask;
    }
}
