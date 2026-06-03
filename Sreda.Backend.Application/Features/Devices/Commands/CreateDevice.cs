using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Sreda.Backend.Application.Common;
using Sreda.Backend.Application.Interfaces;
using Sreda.Backend.Contracts.DTOs.Devices;
using Sreda.Backend.Domain.Entities;
using Sreda.Backend.Domain.Enums;

namespace Sreda.Backend.Application.Features.Devices.Commands;

public record CreateDeviceCommand(
    string Code,
    string Name,
    DeviceType Type,
    string? Location,
    bool IsActive) : IRequest<Result<DeviceDto>>;

public class CreateDeviceValidator : AbstractValidator<CreateDeviceCommand>
{
    public CreateDeviceValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(128);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Location).MaximumLength(256);
        RuleFor(x => x.Type).IsInEnum();
    }
}

public class CreateDeviceHandler : IRequestHandler<CreateDeviceCommand, Result<DeviceDto>>
{
    private readonly ISredaDbContext _dbContext;

    public CreateDeviceHandler(ISredaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<DeviceDto>> Handle(CreateDeviceCommand request, CancellationToken cancellationToken)
    {
        var codeExists = await _dbContext.Devices.AnyAsync(d => d.Code == request.Code, cancellationToken);
        if (codeExists)
        {
            return Result.Failure<DeviceDto>($"Устройство с кодом '{request.Code}' уже существует.");
        }

        var device = new Device
        {
            Code = request.Code,
            Name = request.Name,
            Type = request.Type,
            Location = request.Location,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Devices.Add(device);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(new DeviceDto(
            device.Id, device.Code, device.Name, device.Type, device.Location, device.IsActive, device.CreatedAt));
    }
}
