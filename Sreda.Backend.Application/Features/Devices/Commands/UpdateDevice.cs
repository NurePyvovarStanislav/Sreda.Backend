using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Sreda.Backend.Application.Common;
using Sreda.Backend.Application.Interfaces;
using Sreda.Backend.Domain.Enums;

namespace Sreda.Backend.Application.Features.Devices.Commands;

public record UpdateDeviceCommand(
    int Id,
    string Name,
    DeviceType Type,
    string? Location,
    bool IsActive) : IRequest<Result>;

public class UpdateDeviceValidator : AbstractValidator<UpdateDeviceCommand>
{
    public UpdateDeviceValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Location).MaximumLength(256);
        RuleFor(x => x.Type).IsInEnum();
    }
}

public class UpdateDeviceHandler : IRequestHandler<UpdateDeviceCommand, Result>
{
    private readonly ISredaDbContext _dbContext;

    public UpdateDeviceHandler(ISredaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result> Handle(UpdateDeviceCommand request, CancellationToken cancellationToken)
    {
        var device = await _dbContext.Devices.FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);
        if (device is null)
        {
            return Result.Failure($"Устройство с Id {request.Id} не найдено.");
        }

        device.Name = request.Name;
        device.Type = request.Type;
        device.Location = request.Location;
        device.IsActive = request.IsActive;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
