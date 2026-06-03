using MediatR;
using Microsoft.EntityFrameworkCore;
using Sreda.Backend.Application.Common;
using Sreda.Backend.Application.Interfaces;
using Sreda.Backend.Contracts.DTOs.Devices;
using Sreda.Backend.Domain.Entities;

namespace Sreda.Backend.Application.Features.Devices.Commands;

/// <summary>
/// Upsert текущего состояния устройства (онлайн/оффлайн + произвольный StateJson).
/// На Этапе 9 будет вызываться при поступлении телеметрии по MQTT.
/// </summary>
public record UpdateDeviceStateCommand(int DeviceId, bool IsOnline, string? StateJson)
    : IRequest<Result<DeviceStateDto>>;

public class UpdateDeviceStateHandler : IRequestHandler<UpdateDeviceStateCommand, Result<DeviceStateDto>>
{
    private readonly ISredaDbContext _dbContext;

    public UpdateDeviceStateHandler(ISredaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<DeviceStateDto>> Handle(UpdateDeviceStateCommand request, CancellationToken cancellationToken)
    {
        var deviceExists = await _dbContext.Devices.AnyAsync(d => d.Id == request.DeviceId, cancellationToken);
        if (!deviceExists)
        {
            return Result.Failure<DeviceStateDto>($"Устройство с Id {request.DeviceId} не найдено.");
        }

        var now = DateTime.UtcNow;
        var state = await _dbContext.DeviceStates
            .FirstOrDefaultAsync(s => s.DeviceId == request.DeviceId, cancellationToken);

        if (state is null)
        {
            state = new DeviceState
            {
                DeviceId = request.DeviceId,
                IsOnline = request.IsOnline,
                StateJson = request.StateJson,
                LastSeenAt = request.IsOnline ? now : null,
                UpdatedAt = now
            };
            _dbContext.DeviceStates.Add(state);
        }
        else
        {
            state.IsOnline = request.IsOnline;
            state.StateJson = request.StateJson;
            state.UpdatedAt = now;
            if (request.IsOnline)
            {
                state.LastSeenAt = now;
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(new DeviceStateDto(
            state.DeviceId, state.IsOnline, state.LastSeenAt, state.StateJson, state.UpdatedAt));
    }
}
