using Microsoft.Extensions.Hosting;

namespace Sreda.Backend.Infrastructure.Integration.Mqtt;

/// <summary>
/// Управляет жизненным циклом MQTT-шлюза вместе с приложением:
/// поднимает соединение при старте и корректно гасит его при остановке.
/// </summary>
public sealed class MqttGatewayHostedService : IHostedService
{
    private readonly MqttDeviceGateway _gateway;

    public MqttGatewayHostedService(MqttDeviceGateway gateway)
    {
        _gateway = gateway;
    }

    public Task StartAsync(CancellationToken cancellationToken) => _gateway.StartAsync(cancellationToken);

    public Task StopAsync(CancellationToken cancellationToken) => _gateway.StopAsync(cancellationToken);
}
