using MediatR;
using Microsoft.EntityFrameworkCore;
using Sreda.Backend.Application.Common;
using Sreda.Backend.Application.Features.Devices;
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
            .Include(d => d.Localizations)
            .OrderBy(d => d.Id)
            .ToListAsync(cancellationToken);

        var dtos = devices.Select(DeviceMapping.ToDto).ToList();
        return Result.Success<IReadOnlyList<DeviceDto>>(dtos);
    }
}
