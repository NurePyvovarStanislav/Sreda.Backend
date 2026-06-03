using MediatR;
using Microsoft.EntityFrameworkCore;
using Sreda.Backend.Application.Common;
using Sreda.Backend.Application.Interfaces;
using Sreda.Backend.Contracts.DTOs.Devices;

namespace Sreda.Backend.Application.Features.Devices.Queries;

public record GetDeviceByIdQuery(int Id) : IRequest<Result<DeviceDto>>;

public class GetDeviceByIdQueryHandler : IRequestHandler<GetDeviceByIdQuery, Result<DeviceDto>>
{
    private readonly ISredaDbContext _dbContext;

    public GetDeviceByIdQueryHandler(ISredaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<DeviceDto>> Handle(GetDeviceByIdQuery request, CancellationToken cancellationToken)
    {
        var device = await _dbContext.Devices
            .AsNoTracking()
            .Where(d => d.Id == request.Id)
            .Select(d => new DeviceDto(d.Id, d.Code, d.Name, d.Type, d.Location, d.IsActive, d.CreatedAt))
            .FirstOrDefaultAsync(cancellationToken);

        return device is null
            ? Result.Failure<DeviceDto>($"Устройство с Id {request.Id} не найдено.")
            : Result.Success(device);
    }
}
