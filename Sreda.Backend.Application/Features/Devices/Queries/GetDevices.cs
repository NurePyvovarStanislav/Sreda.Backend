using MediatR;
using Microsoft.EntityFrameworkCore;
using Sreda.Backend.Application.Common;
using Sreda.Backend.Application.Interfaces;
using Sreda.Backend.Contracts.DTOs.Devices;

namespace Sreda.Backend.Application.Features.Devices.Queries;

public record GetDevicesQuery : IRequest<Result<IReadOnlyList<DeviceDto>>>;

public class GetDevicesQueryHandler : IRequestHandler<GetDevicesQuery, Result<IReadOnlyList<DeviceDto>>>
{
    private readonly ISredaDbContext _dbContext;

    public GetDevicesQueryHandler(ISredaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<IReadOnlyList<DeviceDto>>> Handle(GetDevicesQuery request, CancellationToken cancellationToken)
    {
        var devices = await _dbContext.Devices
            .AsNoTracking()
            .OrderBy(d => d.Id)
            .Select(d => new DeviceDto(d.Id, d.Code, d.Name, d.Type, d.Location, d.IsActive, d.CreatedAt))
            .ToListAsync(cancellationToken);

        return Result.Success<IReadOnlyList<DeviceDto>>(devices);
    }
}
