using MediatR;
using Microsoft.EntityFrameworkCore;
using Sreda.Backend.Application.Common;
using Sreda.Backend.Application.Interfaces;
using Sreda.Backend.Contracts.DTOs.Devices;

namespace Sreda.Backend.Application.Features.Devices.Queries;

public record GetDeviceStateQuery(int DeviceId) : IRequest<Result<DeviceStateDto>>;

public class GetDeviceStateQueryHandler : IRequestHandler<GetDeviceStateQuery, Result<DeviceStateDto>>
{
    private readonly ISredaDbContext _dbContext;

    public GetDeviceStateQueryHandler(ISredaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<DeviceStateDto>> Handle(GetDeviceStateQuery request, CancellationToken cancellationToken)
    {
        var state = await _dbContext.DeviceStates
            .AsNoTracking()
            .Where(s => s.DeviceId == request.DeviceId)
            .Select(s => new DeviceStateDto(s.DeviceId, s.IsOnline, s.LastSeenAt, s.StateJson, s.UpdatedAt))
            .FirstOrDefaultAsync(cancellationToken);

        return state is null
            ? Result.Failure<DeviceStateDto>($"Состояние для устройства с Id {request.DeviceId} не найдено.")
            : Result.Success(state);
    }
}
