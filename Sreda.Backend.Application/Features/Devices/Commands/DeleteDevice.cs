using MediatR;
using Microsoft.EntityFrameworkCore;
using Sreda.Backend.Application.Common;
using Sreda.Backend.Application.Interfaces;

namespace Sreda.Backend.Application.Features.Devices.Commands;

public record DeleteDeviceCommand(int Id) : IRequest<Result>;

public class DeleteDeviceHandler : IRequestHandler<DeleteDeviceCommand, Result>
{
    private readonly ISredaDbContext _dbContext;

    public DeleteDeviceHandler(ISredaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result> Handle(DeleteDeviceCommand request, CancellationToken cancellationToken)
    {
        var device = await _dbContext.Devices.FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);
        if (device is null)
        {
            return Result.Failure($"Устройство с Id {request.Id} не найдено.");
        }

        _dbContext.Devices.Remove(device);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
