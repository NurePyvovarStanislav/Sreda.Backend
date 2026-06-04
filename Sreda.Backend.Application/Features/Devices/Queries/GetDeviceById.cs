using MediatR;
using Microsoft.EntityFrameworkCore;
using Sreda.Backend.Application.Common;
using Sreda.Backend.Application.Features.Devices;
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
            .Include(d => d.Localizations)
            .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);

        return device is null
            ? Result.Failure<DeviceDto>($"Пристрій з Id {request.Id} не знайдено.")
            : Result.Success(DeviceMapping.ToDto(device));
    }
}
