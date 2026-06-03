using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Sreda.Backend.Application.Common;
using Sreda.Backend.Application.Interfaces;
using Sreda.Backend.Domain.Enums;

namespace Sreda.Backend.Application.Features.Devices.Commands;

/// <summary>
/// Отправляет команду конкретному устройству через шлюз (MQTT).
/// Бизнес-логика не зависит от транспорта — работает только с IDeviceGateway.
/// </summary>
public record SendDeviceCommandCommand(
    int DeviceId,
    string Command,
    Dictionary<string, object?>? Parameters) : IRequest<Result>;

public class SendDeviceCommandValidator : AbstractValidator<SendDeviceCommandCommand>
{
    public SendDeviceCommandValidator()
    {
        RuleFor(x => x.Command).NotEmpty().MaximumLength(128);
    }
}

public class SendDeviceCommandHandler : IRequestHandler<SendDeviceCommandCommand, Result>
{
    private readonly ISredaDbContext _dbContext;
    private readonly IDeviceGateway _deviceGateway;
    private readonly IEventLogger _eventLogger;

    public SendDeviceCommandHandler(
        ISredaDbContext dbContext,
        IDeviceGateway deviceGateway,
        IEventLogger eventLogger)
    {
        _dbContext = dbContext;
        _deviceGateway = deviceGateway;
        _eventLogger = eventLogger;
    }

    public async Task<Result> Handle(SendDeviceCommandCommand request, CancellationToken cancellationToken)
    {
        var device = await _dbContext.Devices
            .FirstOrDefaultAsync(d => d.Id == request.DeviceId, cancellationToken);

        if (device is null)
        {
            return Result.Failure($"Устройство с Id {request.DeviceId} не найдено.");
        }

        if (!device.IsActive)
        {
            return Result.Failure($"Устройство '{device.Code}' неактивно — команда не отправлена.");
        }

        var payload = new
        {
            command = request.Command,
            parameters = request.Parameters,
            sentAt = DateTime.UtcNow
        };

        try
        {
            await _deviceGateway.SendCommandAsync(device.Code, payload, cancellationToken);
        }
        catch (InvalidOperationException ex)
        {
            await _eventLogger.LogAsync(
                EventType.Error,
                new { deviceCode = device.Code, command = request.Command, error = ex.Message },
                deviceId: device.Id,
                cancellationToken: cancellationToken);

            return Result.Failure(ex.Message);
        }

        await _eventLogger.LogAsync(
            EventType.CommandExecuted,
            new { deviceCode = device.Code, command = request.Command, direction = "outbound", request.Parameters },
            deviceId: device.Id,
            cancellationToken: cancellationToken);

        return Result.Success();
    }
}
